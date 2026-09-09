import { db } from "../../db";
import { callOperatorWallet } from "../services/walletCallback";
import { getOpGameConfig } from "./walletAviator";
import { writeRoundSummary } from "../../utils/roundSummary";

export interface RoundTableConfig {
  gameSlug: string;
  roundsTable: string;
  betsTable: string;
  scopeByGameSlug: boolean;
}

export interface RouteResult {
  status: number;
  body: Record<string, unknown>;
}

interface BetInput {
  betType: string;
  selection: Record<string, unknown>;
  amount: number;
  multiplier: number;
}

export interface DebitParams {
  playerId: string;
  operatorId: string;
  gameId: string;
  roundIdColumn: string;
  roundIdValue: string;
  clientSeed?: string;
  bets: BetInput[];
  totalAmount: number;
}

export async function handleDebit(config: RoundTableConfig, params: DebitParams): Promise<RouteResult> {
  const { data: player, error: playerErr } = await db
    .from("players")
    .select("id, external_id, operator_id")
    .eq("id", params.playerId)
    .single();
  if (playerErr || !player) return { status: 404, body: { error: "Player not found" } };

  const opGame = await getOpGameConfig(params.operatorId, params.gameId);
  if (!opGame) return { status: 500, body: { error: "No wallet config for this operator/game" } };

  let existingQuery = db
    .from(config.roundsTable)
    .select("id")
    .eq("player_id", params.playerId)
    .eq(params.roundIdColumn, params.roundIdValue);
  if (config.scopeByGameSlug) existingQuery = existingQuery.eq("game_slug", config.gameSlug);
  const { data: existing, error: existingErr } = await existingQuery.maybeSingle();
  if (existingErr) return { status: 500, body: { error: "Failed to check existing round" } };
  if (existing) return { status: 409, body: { error: "Round already exists", code: "ROUND_EXISTS" } };

  const insertRow: Record<string, unknown> = {
    player_id:   params.playerId,
    operator_id: params.operatorId,
    game_id:     params.gameId,
    client_seed: params.clientSeed ?? "",
    total_stake: params.totalAmount,
    status:      "pending",
    [params.roundIdColumn]: params.roundIdValue,
  };
  if (config.scopeByGameSlug) insertRow.game_slug = config.gameSlug;

  const { data: round, error: roundErr } = await db
    .from(config.roundsTable)
    .insert(insertRow)
    .select("id")
    .single();
  if (roundErr) {
    if ((roundErr as any).code === "23505") {
      return { status: 409, body: { error: "Round already exists", code: "ROUND_EXISTS" } };
    }
    return { status: 500, body: { error: "Failed to create round" } };
  }
  if (!round) return { status: 500, body: { error: "Failed to create round" } };

  const roundId = (round as any).id as string;

  const betRows = params.bets.map((b) => {
    const row: Record<string, unknown> = {
      round_id:    roundId,
      player_id:   params.playerId,
      operator_id: params.operatorId,
      game_id:     params.gameId,
      bet_type:    b.betType,
      selection:   b.selection,
      amount:      b.amount,
      multiplier:  b.multiplier,
    };
    if (config.scopeByGameSlug) row.game_slug = config.gameSlug;
    return row;
  });

  const { data: insertedBets, error: betsErr } = await db.from(config.betsTable).insert(betRows).select("id");
  if (betsErr || !insertedBets) {
    await db.from(config.roundsTable).update({ status: "cancelled" }).eq("id", roundId);
    return { status: 500, body: { error: "Failed to create bets" } };
  }
  const betIds = (insertedBets as { id: string }[]).map((r) => r.id);

  const result = await callOperatorWallet({
    operatorId:  params.operatorId,
    callbackUrl: opGame.callback_url,
    hmacSecret:  opGame.hmac_secret,
    type:        "debit",
    betId:       roundId,
    playerId:    (player as any).id as string,
    externalId:  (player as any).external_id as string,
    amount:      params.totalAmount,
    roundId,
    gameSlug:    config.gameSlug,
  });

  if (!result.success) {
    await db.from(config.roundsTable).update({ status: "cancelled" }).eq("id", roundId);
    return { status: 502, body: { error: "Wallet debit failed", detail: result.errorMessage } };
  }

  await db.from(config.roundsTable).update({ status: "active" }).eq("id", roundId);
  return { status: 200, body: { roundId, betIds, operatorRef: result.operatorRef } };
}

export interface CreditParams {
  roundId: string;
  resolvedFields: Record<string, unknown>;
  serverSeed: string;
  results: { betId: string; won: boolean; payout: number }[];
  totalPayout: number;
}

export async function handleCredit(config: RoundTableConfig, params: CreditParams): Promise<RouteResult> {
  let roundQuery = db
    .from(config.roundsTable)
    .select("id, total_stake, status, player_id, operator_id, game_id, players(external_id)")
    .eq("id", params.roundId);
  if (config.scopeByGameSlug) roundQuery = roundQuery.eq("game_slug", config.gameSlug);
  const { data: round, error: roundErr } = await roundQuery.single();
  if (roundErr || !round) return { status: 404, body: { error: "Round not found" } };

  const { data: claimed, error: claimErr } = await db
    .from(config.roundsTable)
    .update({ status: "settling" })
    .eq("id", params.roundId)
    .eq("status", "active")
    .select("id");
  if (claimErr) return { status: 500, body: { error: "Failed to claim round" } };
  if (!claimed || claimed.length === 0) {
    return { status: 409, body: { error: "Round not active", code: "ALREADY_SETTLED" } };
  }

  for (const r of params.results) {
    await db.from(config.betsTable).update({ won: r.won, payout: r.payout }).eq("id", r.betId);
  }

  const externalId = ((round as any).players as { external_id: string }).external_id;

  if (params.totalPayout > 0) {
    const opGame = await getOpGameConfig((round as any).operator_id, (round as any).game_id);
    if (!opGame) {
      await db.from(config.roundsTable).update({ status: "active" }).eq("id", params.roundId).eq("status", "settling");
      return { status: 500, body: { error: "No wallet config for this operator/game" } };
    }

    const result = await callOperatorWallet({
      operatorId:  (round as any).operator_id as string,
      callbackUrl: opGame.callback_url,
      hmacSecret:  opGame.hmac_secret,
      type:        "credit",
      betId:       params.roundId,
      playerId:    (round as any).player_id as string,
      externalId,
      amount:      params.totalPayout,
      roundId:     params.roundId,
      gameSlug:    config.gameSlug,
    });

    if (!result.success) {
      await db.from(config.roundsTable).update({ status: "active" }).eq("id", params.roundId).eq("status", "settling");
      return { status: 502, body: { error: "Wallet credit failed", detail: result.errorMessage } };
    }
  }

  const { data: finalized } = await db.from(config.roundsTable).update({
    status:       "resolved",
    server_seed:  params.serverSeed,
    total_payout: params.totalPayout,
    resolved_at:  new Date().toISOString(),
    ...params.resolvedFields,
  }).eq("id", params.roundId).eq("status", "settling").select("id");

  if (!finalized || finalized.length === 0) {
    console.error(`[wallet/${config.gameSlug}/credit] finalize failed roundId=${params.roundId} — claim was lost unexpectedly`);
  }

  await writeRoundSummary({
    playerId:   (round as any).player_id as string,
    operatorId: (round as any).operator_id as string,
    gameId:     (round as any).game_id as string,
    gameSlug:   config.gameSlug,
    roundRef:   params.roundId,
    stake:      Number((round as any).total_stake),
    payout:     params.totalPayout,
    outcome:    params.totalPayout > 0 ? "won" : "lost",
  });

  return { status: 200, body: { success: true } };
}

export interface RollbackParams {
  roundId: string;
}

export async function handleRollback(config: RoundTableConfig, params: RollbackParams): Promise<RouteResult> {
  let roundQuery = db
    .from(config.roundsTable)
    .select("id, total_stake, status, player_id, operator_id, game_id, players(external_id)")
    .eq("id", params.roundId);
  if (config.scopeByGameSlug) roundQuery = roundQuery.eq("game_slug", config.gameSlug);
  const { data: round, error: roundErr } = await roundQuery.single();
  if (roundErr || !round) return { status: 404, body: { error: "Round not found" } };

  const { data: claimed, error: claimErr } = await db
    .from(config.roundsTable)
    .update({ status: "settling" })
    .eq("id", params.roundId)
    .eq("status", "active")
    .select("id");
  if (claimErr) return { status: 500, body: { error: "Failed to claim round" } };
  if (!claimed || claimed.length === 0) {
    return { status: 409, body: { error: "Round not active", code: "ALREADY_SETTLED" } };
  }

  const opGame = await getOpGameConfig((round as any).operator_id, (round as any).game_id);
  if (!opGame) {
    await db.from(config.roundsTable).update({ status: "active" }).eq("id", params.roundId).eq("status", "settling");
    return { status: 500, body: { error: "No wallet config for this operator/game" } };
  }

  const externalId = ((round as any).players as { external_id: string }).external_id;

  const result = await callOperatorWallet({
    operatorId:  (round as any).operator_id as string,
    callbackUrl: opGame.callback_url,
    hmacSecret:  opGame.hmac_secret,
    type:        "rollback",
    betId:       params.roundId,
    playerId:    (round as any).player_id as string,
    externalId,
    amount:      Number((round as any).total_stake),
    roundId:     params.roundId,
    gameSlug:    config.gameSlug,
  });

  if (!result.success) {
    await db.from(config.roundsTable).update({ status: "active" }).eq("id", params.roundId).eq("status", "settling");
    return { status: 502, body: { error: "Wallet rollback failed", detail: result.errorMessage } };
  }

  const { data: finalized } = await db.from(config.roundsTable)
    .update({ status: "cancelled" })
    .eq("id", params.roundId).eq("status", "settling").select("id");

  if (!finalized || finalized.length === 0) {
    console.error(`[wallet/${config.gameSlug}/rollback] finalize failed roundId=${params.roundId} — claim was lost unexpectedly`);
  }

  await writeRoundSummary({
    playerId:   (round as any).player_id as string,
    operatorId: (round as any).operator_id as string,
    gameId:     (round as any).game_id as string,
    gameSlug:   config.gameSlug,
    roundRef:   params.roundId,
    stake:      Number((round as any).total_stake),
    payout:     0,
    outcome:    "cancelled",
  });

  return { status: 200, body: { success: true } };
}
