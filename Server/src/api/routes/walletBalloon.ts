import { Hono } from "hono";
import { zValidator } from "@hono/zod-validator";
import { z } from "zod";
import { db } from "../../db";
import { internalAuth } from "../middleware/internalAuth";
import { callOperatorWallet, callOperatorGetBalance } from "../services/walletCallback";
import { writeRoundSummary } from "../../utils/roundSummary";
import type { HonoVars } from "../types";
import { getOpGameConfig } from "./walletAviator"; // shared operator_games lookup helper

const debitSchema = z.object({
  playerId:      z.string().uuid(),
  operatorId:    z.string().uuid(),
  gameId:        z.string().uuid(),
  clientRoundId: z.string(),
  amount:        z.number().positive(),
  clientSeed:    z.string().optional(),
  autoTarget:    z.number().optional(),
});

const creditSchema = z.object({
  roundId:           z.string().uuid(),
  resultMultiplier:  z.number().min(1),
  payout:            z.number().positive().optional(),
});

const rollbackSchema = z.object({
  roundId: z.string().uuid(),
});

const getBalanceSchema = z.object({
  playerId: z.string().uuid(),
  gameId:   z.string().uuid(),
});

export const walletBalloonRouter = new Hono<HonoVars>();

walletBalloonRouter.use("/*", internalAuth);

walletBalloonRouter.post("/debit", zValidator("json", debitSchema), async (c) => {
  const body = c.req.valid("json");

  const { data: player, error: playerErr } = await db
    .from("players")
    .select("id, external_id, operator_id")
    .eq("id", body.playerId)
    .single();
  if (playerErr || !player) return c.json({ error: "Player not found" }, 404);

  const opGame = await getOpGameConfig(body.operatorId, body.gameId);
  if (!opGame) return c.json({ error: "No wallet config for this operator/game" }, 500);

  // Idempotency guard, same reasoning as walletAviator/debit: keyed by
  // player_id (not Colyseus sessionId) so it survives a reconnect-as-new-session.
  const { data: existing, error: existingErr } = await db
    .from("balloon_rounds")
    .select("id")
    .eq("player_id", body.playerId)
    .eq("client_round_id", body.clientRoundId)
    .in("status", ["placed", "active", "settling"])
    .maybeSingle();
  if (existingErr) return c.json({ error: "Failed to check existing round" }, 500);
  if (existing) return c.json({ roundId: (existing as any).id, operatorRef: null });

  const { data: round, error: roundErr } = await db
    .from("balloon_rounds")
    .insert({
      player_id:       body.playerId,
      operator_id:     body.operatorId,
      game_id:         body.gameId,
      client_round_id: body.clientRoundId,
      bet_amount:      body.amount,
      client_seed:     body.clientSeed ?? "",
      auto_target:     body.autoTarget ?? 0,
      status:          "placed",
    })
    .select("id")
    .single();
  if (roundErr) {
    if ((roundErr as any).code === "23505") {
      // unique (player_id, client_round_id) hit — a retry landed after the
      // first insert but before our maybeSingle() check above saw it.
      const { data: retryExisting } = await db
        .from("balloon_rounds")
        .select("id")
        .eq("player_id", body.playerId)
        .eq("client_round_id", body.clientRoundId)
        .maybeSingle();
      if (retryExisting) return c.json({ roundId: (retryExisting as any).id, operatorRef: null });
    }
    return c.json({ error: "Failed to create round" }, 500);
  }
  if (!round) return c.json({ error: "Failed to create round" }, 500);

  const result = await callOperatorWallet({
    operatorId:  body.operatorId,
    callbackUrl: opGame.callback_url,
    hmacSecret:  opGame.hmac_secret,
    type:        "debit",
    betId:       (round as any).id as string,
    playerId:    (player as any).id as string,
    externalId:  (player as any).external_id as string,
    amount:      body.amount,
    roundId:     (round as any).id as string,
    gameSlug:    "balloon",
  });

  if (!result.success) {
    await db.from("balloon_rounds").update({ status: "cancelled" }).eq("id", (round as any).id);
    return c.json({ error: "Wallet debit failed", detail: result.errorMessage }, 502);
  }

  await db.from("balloon_rounds").update({ status: "active" }).eq("id", (round as any).id);
  return c.json({ roundId: (round as any).id, operatorRef: result.operatorRef });
});

walletBalloonRouter.post("/credit", zValidator("json", creditSchema), async (c) => {
  const body = c.req.valid("json");
  const { roundId, resultMultiplier } = body;

  const { data: round, error: roundErr } = await db
    .from("balloon_rounds")
    .select("id, bet_amount, status, player_id, operator_id, game_id, players(external_id)")
    .eq("id", roundId)
    .single();
  if (roundErr || !round) return c.json({ error: "Round not found" }, 404);

  const { data: claimed, error: claimErr } = await db
    .from("balloon_rounds")
    .update({ status: "settling" })
    .eq("id", roundId)
    .eq("status", "active")
    .select("id");
  if (claimErr) return c.json({ error: "Failed to claim round" }, 500);
  if (!claimed || claimed.length === 0) {
    return c.json({ error: "Round not active", code: "ALREADY_SETTLED" }, 409);
  }

  const opGame = await getOpGameConfig((round as any).operator_id, (round as any).game_id);
  if (!opGame) {
    await db.from("balloon_rounds").update({ status: "active" }).eq("id", roundId).eq("status", "settling");
    return c.json({ error: "No wallet config for this operator/game" }, 500);
  }

  const rawPayout  = body.payout ?? (Number((round as any).bet_amount) * resultMultiplier);
  const payout     = parseFloat(rawPayout.toFixed(2));
  const externalId = ((round as any).players as { external_id: string }).external_id;

  const result = await callOperatorWallet({
    operatorId:  (round as any).operator_id as string,
    callbackUrl: opGame.callback_url,
    hmacSecret:  opGame.hmac_secret,
    type:        "credit",
    betId:       (round as any).id as string,
    playerId:    (round as any).player_id as string,
    externalId,
    amount:      payout,
    roundId:     (round as any).id as string,
    gameSlug:    "balloon",
  });

  if (!result.success) {
    await db.from("balloon_rounds").update({ status: "active" }).eq("id", roundId).eq("status", "settling");
    return c.json({ error: "Wallet credit failed", detail: result.errorMessage }, 502);
  }

  const { data: finalized } = await db.from("balloon_rounds").update({
    status:             "cashed_out",
    result_multiplier:  resultMultiplier,
    payout,
    resolved_at:        new Date().toISOString(),
  }).eq("id", roundId).eq("status", "settling").select("id");

  if (!finalized || finalized.length === 0) {
    console.error(`[wallet/balloon/credit] finalize failed roundId=${roundId} — claim was lost unexpectedly`);
  }

  await writeRoundSummary({
    playerId:   (round as any).player_id as string,
    operatorId: (round as any).operator_id as string,
    gameId:     (round as any).game_id as string,
    gameSlug:   "balloon",
    roundRef:   roundId,
    stake:      Number((round as any).bet_amount),
    payout,
    outcome:    "won",
  });

  return c.json({ payout, operatorRef: result.operatorRef });
});

walletBalloonRouter.post("/get-balance", zValidator("json", getBalanceSchema), async (c) => {
  const { playerId, gameId } = c.req.valid("json");

  const { data: player, error: playerErr } = await db
    .from("players")
    .select("id, external_id, operator_id")
    .eq("id", playerId)
    .single();
  if (playerErr || !player) return c.json({ error: "Player not found" }, 404);

  const opGame = await getOpGameConfig((player as any).operator_id, gameId);
  if (!opGame) return c.json({ error: "No wallet config for this operator/game" }, 500);

  const externalId = (player as any).external_id as string;
  const result = await callOperatorGetBalance({
    operatorId:  (player as any).operator_id as string,
    callbackUrl: opGame.callback_url,
    hmacSecret:  opGame.hmac_secret,
    externalId,
  });

  if (!result) return c.json({ error: "Operator get_balance failed" }, 502);
  return c.json({ balance: result.balance });
});

walletBalloonRouter.post("/rollback", zValidator("json", rollbackSchema), async (c) => {
  const { roundId } = c.req.valid("json");

  const { data: round, error: roundErr } = await db
    .from("balloon_rounds")
    .select("id, bet_amount, status, player_id, operator_id, game_id, players(external_id)")
    .eq("id", roundId)
    .single();
  if (roundErr || !round) return c.json({ error: "Round not found" }, 404);

  const { data: claimed, error: claimErr } = await db
    .from("balloon_rounds")
    .update({ status: "settling" })
    .eq("id", roundId)
    .eq("status", "active")
    .select("id");
  if (claimErr) return c.json({ error: "Failed to claim round" }, 500);
  if (!claimed || claimed.length === 0) {
    return c.json({ error: "Round not active", code: "ALREADY_SETTLED" }, 409);
  }

  const opGame = await getOpGameConfig((round as any).operator_id, (round as any).game_id);
  if (!opGame) {
    await db.from("balloon_rounds").update({ status: "active" }).eq("id", roundId).eq("status", "settling");
    return c.json({ error: "No wallet config for this operator/game" }, 500);
  }

  const externalId = ((round as any).players as { external_id: string }).external_id;

  const result = await callOperatorWallet({
    operatorId:  (round as any).operator_id as string,
    callbackUrl: opGame.callback_url,
    hmacSecret:  opGame.hmac_secret,
    type:        "rollback",
    betId:       (round as any).id as string,
    playerId:    (round as any).player_id as string,
    externalId,
    amount:      Number((round as any).bet_amount),
    roundId:     (round as any).id as string,
    gameSlug:    "balloon",
  });

  if (!result.success) {
    await db.from("balloon_rounds").update({ status: "active" }).eq("id", roundId).eq("status", "settling");
    return c.json({ error: "Wallet rollback failed", detail: result.errorMessage }, 502);
  }

  const { data: finalized } = await db.from("balloon_rounds")
    .update({ status: "cancelled" })
    .eq("id", roundId).eq("status", "settling").select("id");

  if (!finalized || finalized.length === 0) {
    console.error(`[wallet/balloon/rollback] finalize failed roundId=${roundId} — claim was lost unexpectedly`);
  }

  await writeRoundSummary({
    playerId:   (round as any).player_id as string,
    operatorId: (round as any).operator_id as string,
    gameId:     (round as any).game_id as string,
    gameSlug:   "balloon",
    roundRef:   roundId,
    stake:      Number((round as any).bet_amount),
    payout:     0,
    outcome:    "cancelled",
  });

  return c.json({ success: true });
});
