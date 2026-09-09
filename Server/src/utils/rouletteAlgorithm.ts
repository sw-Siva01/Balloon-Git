import * as crypto from "crypto";
import { generateServerSeed, hashSeed } from "./crashAlgorithm";

export { generateServerSeed, hashSeed };

/**
 * Provably-fair winning pocket (1-12). Unlike Aviator, there's no live
 * window between commit and reveal to exploit — bets and the spin trigger
 * arrive in the same client message, so this isn't closing a live-exploit
 * window. It's kept for audit parity: an operator or player can recompute
 * this from the revealed seed/nonce and confirm no tampering, same as
 * Aviator's crash point.
 */
export function computeWinningNumber(
  serverSeed: string,
  clientSeed: string,
  nonce: number
): number {
  // Rejection-sample to eliminate the 2^32 % 12 modulo bias.
  const bound = Math.floor(0x100000000 / 12) * 12;
  let counter = 0;
  while (true) {
    const hash = crypto
      .createHash("sha512")
      .update(`${serverSeed}:${clientSeed}:${nonce}:${counter}`)
      .digest("hex");
    const val = parseInt(hash.substring(0, 8), 16);
    if (val < bound) return (val % 12) + 1;
    counter++;
  }
}
