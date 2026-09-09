import { Hono } from "hono";
import { zValidator } from "@hono/zod-validator";
import { z } from "zod";
import { db } from "../../db";
import { internalAuth } from "../middleware/internalAuth";
import { callOperatorWallet, callOperatorGetBalance } from "../services/walletCallback";
import { writeRoundSummary } from "../../utils/roundSummary";
import type { HonoVars } from "../types";

const debitSchema = z.object({
  playerId:   z.string().uuid(),
  operatorId: z.string().uuid(),
  gameId:     z.string().uuid(),
  roundId:    z.string().uuid(),
  amount:     z.number().positive(),
  ticketSlot: z.number().int().min(1).max(2),
  clientSeed: z.string().optional(),
  autoTarget: z.number().optional(),
});

const creditSchema = z.object({
  betId:             z.string().uuid(),
  cashOutMultiplier: z.number().min(1),
  payout:            z.number().positive().optional(),
});

const rollbackSchema = z.object({
  betId: z.string().uuid(),
});

const getBalanceSchema = z.object({
  playerId: z.string().uuid(),
  gameId:   z.string().uuid(),
});

export const walletAviatorRouter = new Hono<HonoVars>();

walletAviatorRouter.use("/*", internalAuth);

export async function getOpGameConfig(operatorId: string, gameId: string) {
  const { data, error } = await db
    .from("operator_games")
    .select("callback_url, operators(hmac_secret)")
    .eq("operator_id", operatorId)
    .eq("game_id", gameId)
    .eq("is_active", true)
    .single();
  if (error || !data) return null;
  return {
    callback_url: (data as any).callback_url as string,
    hmac_secret:  ((data as any).operators as { hmac_secret: string }).hmac_secret,
  };
}

walletAviatorRouter.post("/debit", zValidator("json", debitSchema), async (c) => {
  const body = c.req.valid("json");

  const { data: player, error: playerErr } = await db
    .from("players")
    .select("id, external_id, operator_id")
    .eq("id", body.playerId)
    .single();
  if (playerErr || !player) return c.json({ error: "Player not found" }, 404);

  const opGame = await getOpGameConfig(body.operatorId, body.gameId);
  if (!opGame) return c.json({ error: "No wallet config for this operator/game" }, 500);

  // Idempotency guard: block a second bet for the same round/player/slot while
  // one is already placed/active. The in-room guard is keyed by Colyseus
  // sessionId, which resets on a reconnect-as-new-session — this DB-level
  // check is keyed by player_id instead, so it holds regardless of session churn.
  const { data: existing, error: existingErr } = await db
    .from("aviator_bets")
    .select("id")
    .eq("round_id", body.roundId)
    .eq("player_id", body.playerId)
    .eq("ticket_slot", body.ticketSlot)
    .in("status", ["placed", "active", "settling"])
    .maybeSingle();
  if (existingErr) return c.json({ error: "Failed to check existing bet" }, 500);
  if (existing) return c.json({ error: "Bet already exists for this round/slot", code: "BET_EXISTS" }, 409);

  const { data: bet, error: betErr } = await db
    .from("aviator_bets")
    .insert({
      round_id:    body.roundId,
      player_id:   body.playerId,
      operator_id: body.operatorId,
      game_id:     body.gameId,
      ticket_slot: body.ticketSlot,
      amount:      body.amount,
      client_seed: body.clientSeed ?? "",
      auto_target: body.autoTarget ?? 0,
      status:      "placed",
    })
    .select("id")
    .single();
  if (betErr) {
    if ((betErr as any).code === "23505") {
      return c.json({ error: "Bet already exists for this round/slot", code: "BET_EXISTS" }, 409);
    }
    return c.json({ error: "Failed to create bet" }, 500);
  }
  if (!bet) return c.json({ error: "Failed to create bet" }, 500);

  const result = await callOperatorWallet({
    operatorId:  body.operatorId,
    callbackUrl: opGame.callback_url,
    hmacSecret:  opGame.hmac_secret,
    type:        "debit",
    betId:       (bet as any).id as string,
    playerId:    (player as any).id as string,
    externalId:  (player as any).external_id as string,
    amount:      body.amount,
    roundId:     body.roundId,
    gameSlug:    "aviator",
  });

  if (!result.success) {
    await db.from("aviator_bets").update({ status: "cancelled" }).eq("id", (bet as any).id);
    return c.json({ error: "Wallet debit failed", detail: result.errorMessage }, 502);
  }

  await db.from("aviator_bets").update({ status: "active" }).eq("id", (bet as any).id);
  return c.json({ betId: (bet as any).id, operatorRef: result.operatorRef });
});

walletAviatorRouter.post("/credit", zValidator("json", creditSchema), async (c) => {
  const body = c.req.valid("json");
  const { betId, cashOutMultiplier } = body;

  const { data: bet, error: betErr } = await db
    .from("aviator_bets")
    .select("id, amount, status, round_id, player_id, operator_id, game_id, players(external_id)")
    .eq("id", betId)
    .single();
  if (betErr || !bet) return c.json({ error: "Bet not found" }, 404);

  // Atomically claim the bet BEFORE calling the operator's wallet, so a
  // concurrent settlement attempt (credit/rollback/loss-resolution racing
  // this one) is rejected up front — instead of both reaching the operator
  // and only the internal bets row noticing the conflict afterward.
  const { data: claimed, error: claimErr } = await db
    .from("aviator_bets")
    .update({ status: "settling" })
    .eq("id", betId)
    .eq("status", "active")
    .select("id");
  if (claimErr) return c.json({ error: "Failed to claim bet" }, 500);
  if (!claimed || claimed.length === 0) {
    return c.json({ error: "Bet not active", code: "ALREADY_SETTLED" }, 409);
  }

  const opGame = await getOpGameConfig((bet as any).operator_id, (bet as any).game_id);
  if (!opGame) {
    await db.from("aviator_bets").update({ status: "active" }).eq("id", betId).eq("status", "settling");
    return c.json({ error: "No wallet config for this operator/game" }, 500);
  }

  const rawPayout  = body.payout ?? (Number((bet as any).amount) * cashOutMultiplier);
  const payout     = parseFloat(rawPayout.toFixed(2));
  const externalId = ((bet as any).players as { external_id: string }).external_id;

  const result = await callOperatorWallet({
    operatorId:  (bet as any).operator_id as string,
    callbackUrl: opGame.callback_url,
    hmacSecret:  opGame.hmac_secret,
    type:        "credit",
    betId:       (bet as any).id as string,
    playerId:    (bet as any).player_id as string,
    externalId,
    amount:      payout,
    roundId:     (bet as any).round_id as string,
    gameSlug:    "aviator",
  });

  if (!result.success) {
    // Release the claim so the bet is retryable, matching the pre-claim behavior.
    await db.from("aviator_bets").update({ status: "active" }).eq("id", betId).eq("status", "settling");
    return c.json({ error: "Wallet credit failed", detail: result.errorMessage }, 502);
  }

  const { data: finalized } = await db.from("aviator_bets").update({
    status:              "cashed_out",
    cash_out_multiplier: cashOutMultiplier,
    payout,
    cashed_out_at:       new Date().toISOString(),
  }).eq("id", betId).eq("status", "settling").select("id");

  if (!finalized || finalized.length === 0) {
    console.error(`[wallet/credit] finalize failed betId=${betId} — claim was lost unexpectedly`);
  }

  await writeRoundSummary({
    playerId:   (bet as any).player_id as string,
    operatorId: (bet as any).operator_id as string,
    gameId:     (bet as any).game_id as string,
    gameSlug:   "aviator",
    roundRef:   betId,
    stake:      Number((bet as any).amount),
    payout,
    outcome:    "won",
  });

  return c.json({ payout, operatorRef: result.operatorRef });
});

walletAviatorRouter.post("/get-balance", zValidator("json", getBalanceSchema), async (c) => {
  const { playerId, gameId } = c.req.valid("json");

  const { data: player, error: playerErr } = await db
    .from("players")
    .select("id, external_id, operator_id")
    .eq("id", playerId)
    .single();
  if (playerErr || !player) {
    console.warn(`[get-balance] player not found: ${playerId}`);
    return c.json({ error: "Player not found" }, 404);
  }

  const opGame = await getOpGameConfig((player as any).operator_id, gameId);
  if (!opGame) {
    console.error(`[get-balance] no operator_games config for player=${playerId} game=${gameId}`);
    return c.json({ error: "No wallet config for this operator/game" }, 500);
  }

  const externalId = (player as any).external_id as string;
  const result = await callOperatorGetBalance({
    operatorId:  (player as any).operator_id as string,
    callbackUrl: opGame.callback_url,
    hmacSecret:  opGame.hmac_secret,
    externalId,
  });

  if (!result) {
    console.error(`[get-balance] operator callback failed for ${externalId}`);
    return c.json({ error: "Operator get_balance failed" }, 502);
  }
  console.log(`[get-balance] player=${externalId} balance=${result.balance}`);
  return c.json({ balance: result.balance });
});

walletAviatorRouter.post("/rollback", zValidator("json", rollbackSchema), async (c) => {
  const { betId } = c.req.valid("json");

  const { data: bet, error: betErr } = await db
    .from("aviator_bets")
    .select("id, amount, status, round_id, player_id, operator_id, game_id, players(external_id)")
    .eq("id", betId)
    .single();
  if (betErr || !bet) return c.json({ error: "Bet not found" }, 404);

  // Atomically claim the bet BEFORE calling the operator's wallet — see the
  // matching comment in /credit.
  const { data: claimed, error: claimErr } = await db
    .from("aviator_bets")
    .update({ status: "settling" })
    .eq("id", betId)
    .eq("status", "active")
    .select("id");
  if (claimErr) return c.json({ error: "Failed to claim bet" }, 500);
  if (!claimed || claimed.length === 0) {
    return c.json({ error: "Bet not active", code: "ALREADY_SETTLED" }, 409);
  }

  const opGame = await getOpGameConfig((bet as any).operator_id, (bet as any).game_id);
  if (!opGame) {
    await db.from("aviator_bets").update({ status: "active" }).eq("id", betId).eq("status", "settling");
    return c.json({ error: "No wallet config for this operator/game" }, 500);
  }

  const externalId = ((bet as any).players as { external_id: string }).external_id;

  const result = await callOperatorWallet({
    operatorId:  (bet as any).operator_id as string,
    callbackUrl: opGame.callback_url,
    hmacSecret:  opGame.hmac_secret,
    type:        "rollback",
    betId:       (bet as any).id as string,
    playerId:    (bet as any).player_id as string,
    externalId,
    amount:      Number((bet as any).amount),
    roundId:     (bet as any).round_id as string,
    gameSlug:    "aviator",
  });

  if (!result.success) {
    await db.from("aviator_bets").update({ status: "active" }).eq("id", betId).eq("status", "settling");
    return c.json({ error: "Wallet rollback failed", detail: result.errorMessage }, 502);
  }

  const { data: finalized } = await db.from("aviator_bets")
    .update({ status: "cancelled" })
    .eq("id", betId).eq("status", "settling").select("id");

  if (!finalized || finalized.length === 0) {
    console.error(`[wallet/rollback] finalize failed betId=${betId} — claim was lost unexpectedly`);
  }

  await writeRoundSummary({
    playerId:   (bet as any).player_id as string,
    operatorId: (bet as any).operator_id as string,
    gameId:     (bet as any).game_id as string,
    gameSlug:   "aviator",
    roundRef:   betId,
    stake:      Number((bet as any).amount),
    payout:     0,
    outcome:    "cancelled",
  });

  return c.json({ success: true });
});
