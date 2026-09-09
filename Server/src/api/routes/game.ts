import crypto from "crypto";
import { Hono } from "hono";
import { zValidator } from "@hono/zod-validator";
import { z } from "zod";
import { db } from "../../db";
import { hmacAuth } from "../middleware/hmacAuth";
import { issuePlayerToken, revokeToken, verifyPlayerToken } from "../services/playerToken";
import type { HonoVars } from "../types";

const SESSION_CODE_TTL_MS = 30 * 60 * 1000;

export interface SessionCodeMeta {
  game:     string;
  user:     string;
  currency: string;
  operator: string;
  lang:     string;
}

const sessionCodes = new Map<string, { token: string; meta: SessionCodeMeta; expiresAt: number }>();

export function createSessionCode(token: string, meta: SessionCodeMeta): string {
  const code = crypto.randomBytes(32).toString("hex");
  sessionCodes.set(code, { token, meta, expiresAt: Date.now() + SESSION_CODE_TTL_MS });
  return code;
}

export function redeemSessionCode(code: string): { token: string; meta: SessionCodeMeta } | null {
  const entry = sessionCodes.get(code);
  if (!entry || Date.now() > entry.expiresAt) {
    sessionCodes.delete(code);
    return null;
  }
  return { token: entry.token, meta: entry.meta };
}

const launchSchema = z.object({
  playerId:    z.string().min(1),
  balance:     z.number().nonnegative(),
  currency:    z.string().optional(),
  lang:        z.string().optional(),
  displayName: z.string().optional(),
});

export const gameRouter = new Hono<HonoVars>();

async function handleLaunch(c: Parameters<Parameters<typeof gameRouter.post>[1]>[0], gameSlug: string) {
  const operator   = c.get("operator");
  const { playerId: externalId, balance, displayName, lang } = c.req.valid("json" as any) as z.infer<typeof launchSchema>;
  console.log(`[launch] externalId=${externalId} displayName=${displayName ?? "none"}`);

  const { data: game, error: gameErr } = await db
    .from("games")
    .select("id, game_url")
    .eq("slug", gameSlug)
    .eq("is_active", true)
    .single();

  if (gameErr || !game) {
    console.error(`[launch] game not found: slug=${gameSlug}`);
    return c.json({ error: "Game not found" }, 404);
  }

  const { data: opGame, error: opGameErr } = await db
    .from("operator_games")
    .select("id")
    .eq("operator_id", (operator as any).id)
    .eq("game_id", (game as any).id)
    .eq("is_active", true)
    .single();

  if (opGameErr || !opGame) {
    console.error(`[launch] operator=${(operator as any).name} has no access to game=${gameSlug}`);
    return c.json({ error: "Game not available for this operator" }, 403);
  }

  const { data: player, error: playerErr } = await db
    .from("players")
    .upsert(
      { operator_id: (operator as any).id, external_id: externalId, ...(displayName ? { display_name: displayName } : {}) },
      { onConflict: "operator_id,external_id", ignoreDuplicates: false }
    )
    .select("id")
    .single();

  if (playerErr || !player) {
    console.error(`[launch] DB error for ${externalId}:`, playerErr?.message);
    return c.json({ error: "Failed to create/find player" }, 500);
  }

  const currency   = ((operator as any).currency as string | undefined) ?? "USD";
  const clientIp   = c.req.header("x-forwarded-for")?.split(",")[0].trim() ?? c.req.header("cf-connecting-ip");
  const userAgent  = c.req.header("user-agent");

  const token = await issuePlayerToken(
    {
      sub:        (player as any).id as string,
      operatorId: (operator as any).id,
      externalId,
      currency,
      gameId:     (game as any).id as string,
      gameSlug,
    },
    clientIp,
    userAgent
  );

  const operatorName = (operator as any).name as string;
  const sessionCode  = createSessionCode(token, {
    game:     gameSlug,
    user:     externalId,
    currency,
    operator: operatorName,
    lang:     lang ?? "en",
  });
  const gameUrl = process.env.GAME_URL || ((game as any).game_url as string);
  const query   = new URLSearchParams({
    game:     gameSlug,
    user:     externalId,
    token:    sessionCode,
    currency,
    operator: operatorName,
    lang:     lang ?? "en",
  });
  if (process.env.GAME_ENV === "dev") query.set("env", "dev");
  console.log(`[launch] player=${externalId} op=${operatorName} game=${gameSlug} currency=${currency} balance=${balance} code=${sessionCode.slice(0, 8)}…`);

  return c.json({
    sessionCode,
    gameUrl:  `${gameUrl}?${query.toString()}`,
    playerId: (player as any).id as string,
  });
}

gameRouter.post("/:gameSlug/launch", hmacAuth, zValidator("json", launchSchema), (c) => {
  const { gameSlug } = c.req.param();
  return handleLaunch(c, gameSlug);
});

gameRouter.post("/launch", hmacAuth, zValidator("json", launchSchema), (c) => {
  return handleLaunch(c, "aviator");
});

gameRouter.post("/session", zValidator("json", z.object({ code: z.string().length(64) })), (c) => {
  const { code } = c.req.valid("json");
  const result = redeemSessionCode(code);
  if (!result) {
    console.warn(`[session] 401 — code not found or expired: ${code.slice(0, 8)}… (pool size: ${sessionCodes.size})`);
    return c.json({ error: "Invalid or expired session code" }, 401);
  }
  console.log(`[session] redeemed code ${code.slice(0, 8)}… (pool size: ${sessionCodes.size})`);
  return c.json({ token: result.token, ...result.meta });
});

gameRouter.post("/revoke-token", hmacAuth, zValidator("json", z.object({ token: z.string() })), async (c) => {
  const { token } = c.req.valid("json");
  try {
    const payload = await verifyPlayerToken(token);
    revokeToken(payload.jti);
    return c.json({ success: true });
  } catch {
    return c.json({ error: "Invalid token" }, 400);
  }
});
