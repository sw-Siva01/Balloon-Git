import crypto from "crypto";
import { EncryptJWT, jwtDecrypt } from "jose";

if (!process.env.JWT_SECRET) {
  if (process.env.NODE_ENV === "production") {
    throw new Error("JWT_SECRET environment variable is required in production");
  }
  console.warn("[playerToken] JWT_SECRET not set — using insecure default (dev only)");
}

const JWT_SECRET = process.env.JWT_SECRET ?? "dev-insecure-secret-do-not-use-in-prod";
const ENCRYPTION_KEY = crypto.createHash("sha256").update(JWT_SECRET).digest();
const JWT_EXPIRY_SECONDS = 30 * 60;

const revokedTokens = new Map<string, number>();

function evictExpiredRevocations() {
  const now = Date.now();
  for (const [jti, expiresAt] of revokedTokens) {
    if (now > expiresAt) revokedTokens.delete(jti);
  }
}

export interface PlayerTokenPayload {
  jti: string;
  sub: string;
  operatorId: string;
  externalId: string;
  displayName?: string;
  currency: string;
  gameId: string;
  gameSlug: string;
  ipHash?: string;
  uaHash?: string;
}

export async function issuePlayerToken(
  payload: Omit<PlayerTokenPayload, "jti">,
  clientIp?: string,
  userAgent?: string
): Promise<string> {
  const jti    = crypto.randomBytes(16).toString("hex");
  const ipHash = clientIp  ? crypto.createHash("sha256").update(clientIp).digest("hex")  : undefined;
  const uaHash = userAgent ? crypto.createHash("sha256").update(userAgent).digest("hex") : undefined;

  return new EncryptJWT({ ...payload, jti, ipHash, uaHash } as Record<string, unknown>)
    .setProtectedHeader({ alg: "dir", enc: "A256GCM" })
    .setIssuedAt()
    .setExpirationTime(`${JWT_EXPIRY_SECONDS}s`)
    .encrypt(ENCRYPTION_KEY);
}

export async function verifyPlayerToken(
  token: string,
  clientIp?: string,
  userAgent?: string
): Promise<PlayerTokenPayload> {
  const { payload } = await jwtDecrypt(token, ENCRYPTION_KEY);
  const typedPayload = payload as unknown as PlayerTokenPayload;

  evictExpiredRevocations();
  if (revokedTokens.has(typedPayload.jti)) throw new Error("Token has been revoked");

  if (typedPayload.ipHash && clientIp) {
    const ipHash = crypto.createHash("sha256").update(clientIp).digest("hex");
    if (ipHash !== typedPayload.ipHash) throw new Error("Token IP binding mismatch");
  }
  if (typedPayload.uaHash && userAgent) {
    const uaHash = crypto.createHash("sha256").update(userAgent).digest("hex");
    if (uaHash !== typedPayload.uaHash) throw new Error("Token User-Agent binding mismatch");
  }

  return typedPayload;
}

export function revokeToken(jti: string): void {
  revokedTokens.set(jti, Date.now() + 30 * 60 * 1000);
}
