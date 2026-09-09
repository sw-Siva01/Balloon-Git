import * as crypto from "crypto";
import { generateServerSeed, hashSeed } from "./crashAlgorithm";

export { generateServerSeed, hashSeed };

/**
 * Provably-fair burst multiplier. Unlike Roulette (bet + spin trigger arrive
 * in the same client message, so the outcome can be computed on demand at
 * spin time), Balloon has a genuine live window between commit and reveal —
 * same as Aviator: the player watches the multiplier climb in real time and
 * decides when to cash out, so the burst point MUST be committed the instant
 * the round starts, before the player has seen a single tick, and only
 * revealed once the balloon actually bursts. Computing it any later than
 * round-start would let the result be back-fitted to the player's cash-out
 * timing, which defeats the point of "provably fair."
 *
 * Reuses the same crash-point distribution shape as Aviator's
 * computeCrashPoint (a 1/houseEdgeDivisor chance of an instant 1.00x burst,
 * otherwise a Pareto-tailed multiplier derived from the hash) but is its own
 * function rather than an import, so Balloon's house edge can be tuned
 * independently of Aviator's without touching that file.
 */
export function computeBurstMultiplier(
  serverSeed: string,
  clientSeed: string,
  nonce: number
): { burstMultiplier: number; houseEdgeDivisor: number } {
  const hash = crypto
    .createHash("sha512")
    .update(`${serverSeed}:${clientSeed}:${nonce}`)
    .digest("hex");

  const houseEdgeDivisor = 30 + (parseInt(hash.substring(8, 10), 16) % 7);
  if (parseInt(hash.substring(0, 8), 16) % houseEdgeDivisor === 0) {
    return { burstMultiplier: 1.0, houseEdgeDivisor };
  }

  // 52-bit precision window, same approach as the standard provably-fair
  // crash formula: maps the hash into (1, 100] and then rescales below.
  const e = BigInt(2) ** BigInt(52);
  const h = BigInt("0x" + hash.substring(0, 13));
  const burstMultiplier = Number((BigInt(100) * e - h) / (e - h)) / 100;

  return { burstMultiplier: Math.max(1.01, burstMultiplier), houseEdgeDivisor };
}
