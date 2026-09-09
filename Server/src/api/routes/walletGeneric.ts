import { Hono } from "hono";
import { zValidator } from "@hono/zod-validator";
import { z } from "zod";
import { internalAuth } from "../middleware/internalAuth";
import { handleDebit, handleCredit, handleRollback, type RoundTableConfig } from "./walletShared";
import type { HonoVars } from "../types";

const betSchema = z.object({
  betType:    z.string().min(1),
  selection:  z.record(z.string(), z.unknown()),
  amount:     z.number().positive(),
  multiplier: z.number().positive(),
});

const debitSchema = z.object({
  playerId:      z.string().uuid(),
  operatorId:    z.string().uuid(),
  gameId:        z.string().uuid(),
  clientRoundId: z.string().min(1),
  clientSeed:    z.string().optional(),
  bets:          z.array(betSchema).min(1),
  totalAmount:   z.number().positive(),
});

const creditSchema = z.object({
  roundId:     z.string().uuid(),
  outcome:     z.unknown(),
  serverSeed:  z.string(),
  results:     z.array(z.object({
    betId:  z.string().uuid(),
    won:    z.boolean(),
    payout: z.number().min(0),
  })),
  totalPayout: z.number().min(0),
});

const rollbackSchema = z.object({
  roundId: z.string().uuid(),
});

export function createGenericWalletRouter(gameSlug: string) {
  const config: RoundTableConfig = {
    gameSlug,
    roundsTable: "game_rounds",
    betsTable: "game_bets",
    scopeByGameSlug: true,
  };

  const router = new Hono<HonoVars>();
  router.use("/*", internalAuth);

  router.post("/debit", zValidator("json", debitSchema), async (c) => {
    const body = c.req.valid("json");
    const result = await handleDebit(config, {
      playerId:      body.playerId,
      operatorId:    body.operatorId,
      gameId:        body.gameId,
      roundIdColumn: "client_round_id",
      roundIdValue:  body.clientRoundId,
      clientSeed:    body.clientSeed,
      bets:          body.bets,
      totalAmount:   body.totalAmount,
    });
    return c.json(result.body, result.status as any);
  });

  router.post("/credit", zValidator("json", creditSchema), async (c) => {
    const body = c.req.valid("json");
    const result = await handleCredit(config, {
      roundId:        body.roundId,
      resolvedFields: { outcome: body.outcome },
      serverSeed:     body.serverSeed,
      results:        body.results,
      totalPayout:    body.totalPayout,
    });
    return c.json(result.body, result.status as any);
  });

  router.post("/rollback", zValidator("json", rollbackSchema), async (c) => {
    const { roundId } = c.req.valid("json");
    const result = await handleRollback(config, { roundId });
    return c.json(result.body, result.status as any);
  });

  return router;
}
