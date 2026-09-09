import crypto from "crypto";
import type { Context, Next } from "hono";
import type { HonoVars } from "../types";

export async function internalAuth(c: Context<HonoVars>, next: Next): Promise<Response | void> {
  const secret = process.env.INTERNAL_SECRET ?? "";
  const auth = c.req.header("Authorization") ?? "";
  const token = auth.startsWith("Bearer ") ? auth.slice(7) : "";

  if (!token || !secret) {
    return c.json({ error: "Unauthorized" }, 401);
  }

  const tokenBuf = Buffer.from(token);
  const secretBuf = Buffer.from(secret);

  if (
    tokenBuf.length !== secretBuf.length ||
    !crypto.timingSafeEqual(tokenBuf, secretBuf)
  ) {
    return c.json({ error: "Unauthorized" }, 401);
  }

  await next();
}
