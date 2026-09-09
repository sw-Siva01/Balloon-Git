// Per-game config for the generic round tier (GenericRoundRoom / walletGeneric.ts).
// Adding a new "simple" game (stake -> resolve -> payout, no live betting
// window) means adding ONE entry here, not new room/wallet files.
//
// Aviator does not belong here — its live-ticking multiplier with an
// uncertain-timing cashout doesn't resolve in one atomic step, which is
// what this tier assumes. It keeps AviatorRoom.ts/walletAviator.ts.
//
// See RouletteRoom.ts's PAYOUTS/validateBet/evaluateBets for a worked
// reference implementation of this same shape (Roulette isn't migrated onto
// this tier — it stays on its own roulette_rounds/roulette_bets tables —
// but its logic is the closest existing example to copy from).

export interface ValidatedGenericBet {
  betType: string;
  selection: Record<string, unknown>;
  amount: number;
  multiplier: number;
}

export interface GenericBetResult extends ValidatedGenericBet {
  won: boolean;
  payout: number;
}

export interface GamePayoutConfig {
  slug: string;
  maxBetsPerRound: number;
  minBet: number;
  maxBet: number;
  minTotalStake: number;
  maxTotalStake: number;

  // Validates shape/range only and looks up the server-side multiplier for
  // betType — never trust a client-echoed multiplier.
  validateBet(raw: unknown): ValidatedGenericBet | null;

  // Provably-fair outcome generation (mirror crashAlgorithm.ts/
  // rouletteAlgorithm.ts's seed/hash/reveal pattern). Return value is
  // whatever shape this game's outcome naturally is — stored as-is in
  // game_rounds.outcome (jsonb).
  resolveOutcome(serverSeed: string, clientSeed: string, nonce: number): unknown;

  evaluateBets(
    bets: ValidatedGenericBet[],
    outcome: unknown
  ): { results: GenericBetResult[]; totalPayout: number };
}

export const gamePayoutRegistry: Record<string, GamePayoutConfig> = {
  // Add the next game's config here.
};

export function getGamePayoutConfig(slug: string): GamePayoutConfig | null {
  return gamePayoutRegistry[slug] ?? null;
}
