import * as crypto from "crypto";

export interface CrashSeed {
  serverSeed: string;
  nonce: number;
}

export function generateServerSeed(): string {
  return crypto.randomBytes(32).toString("hex");
}

export function hashSeed(serverSeed: string, nonce: number): string {
  return crypto
    .createHash("sha256")
    .update(`${serverSeed}:${nonce}`)
    .digest("hex");
}

export interface CrashResult {
  crashPoint: number;
  houseEdgeDivisor: number;
}

export function computeCrashPoint(
  serverSeed: string,
  clientSeeds: string[],
  nonce: number
): CrashResult {
  const combined = [serverSeed, ...clientSeeds.slice(0, 3), String(nonce)].join("");
  const hash = crypto.createHash("sha512").update(combined).digest("hex");

  const divisor = 30 + (parseInt(hash.substring(8, 10), 16) % 7);
  if (parseInt(hash.substring(0, 8), 16) % divisor === 0) {
    return { crashPoint: 1.0, houseEdgeDivisor: divisor };
  }

  const e = BigInt(2) ** BigInt(52);
  const h = BigInt("0x" + hash.substring(0, 13));

  const numerator = BigInt(100) * e - h;
  const denominator = e - h;
  const crashPoint = Number(numerator / denominator) / 100;

  return { crashPoint: Math.max(1.01, crashPoint), houseEdgeDivisor: divisor };
}

export function computeMultiplier(elapsedMs: number): number {
  return Math.exp(0.00006 * elapsedMs);
}
