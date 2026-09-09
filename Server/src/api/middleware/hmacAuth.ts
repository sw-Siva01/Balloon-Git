import crypto from "crypto";
import type { Context, Next } from "hono";
import { db } from "../../db";
import type { HonoVars } from "../types";

const TIMESTAMP_TOLERANCE_MS = 300_000;
const NONCE_WINDOW_MS        = 600_000;

const usedNonces = new Map<string, number>();

function evictExpiredNonces() {
  const now = Date.now();
  for (const [nonce, expiresAt] of usedNonces) {
    if (now > expiresAt) usedNonces.delete(nonce);
  }
}

export async function hmacAuth(c: Context<HonoVars>, next: Next): Promise<Response | void> {
  const apiKey    = c.req.header("X-Api-Key");
  const timestamp = c.req.header("X-Timestamp");
  const signature = c.req.header("X-Signature");
  const nonce     = c.req.header("X-Nonce");

  if (!apiKey || !timestamp || !signature || !nonce) {
    return c.json({ error: "Missing auth headers" }, 401);
  }

  const ts = parseInt(timestamp, 10);
  if (isNaN(ts) || Math.abs(Date.now() - ts) > TIMESTAMP_TOLERANCE_MS) {
    return c.json({ error: "Timestamp out of window" }, 401);
  }

  evictExpiredNonces();
  const nonceKey = `${apiKey}:${nonce}`;
  if (usedNonces.has(nonceKey)) {
    return c.json({ error: "Replayed request" }, 401);
  }

  const { data: operator } = await db
    .from("operators")
    .select("id, name, api_key, hmac_secret, currency, is_active")
    .eq("api_key", apiKey)
    .single();

  if (!operator || !(operator as any).is_active) {
    return c.json({ error: "Unauthorized" }, 401);
  }

  const rawBody  = await c.req.text();
  const bodyHash = crypto.createHash("sha256").update(rawBody).digest("hex");
  const method   = c.req.method;
  const path     = new URL(c.req.url).pathname;
  const expected = crypto
    .createHmac("sha256", (operator as any).hmac_secret as string)
    .update(`${method}:${path}:${timestamp}:${nonce}:${bodyHash}`)
    .digest("hex");

  const sigBuf = Buffer.from(signature, "hex");
  const expBuf = Buffer.from(expected, "hex");
  if (sigBuf.length !== expBuf.length || !crypto.timingSafeEqual(sigBuf, expBuf)) {
    return c.json({ error: "Invalid signature" }, 401);
  }

  usedNonces.set(nonceKey, Date.now() + NONCE_WINDOW_MS);

  c.set("operator", operator as any);
  c.set("rawBody", rawBody);
  await next();
}
