import { db } from "../db";

export interface RoundSummaryRow {
  playerId:   string;
  operatorId: string;
  gameId:     string;
  gameSlug:   string;
  roundRef:   string;
  stake:      number;
  payout:     number;
  outcome:    "won" | "lost" | "cancelled";
}

/**
 * One row per finished round/spin, any game — a pure audit/reporting
 * side-write. Nothing reads this table to make a wallet decision, so a bug
 * here can't cause a financial error, only an incomplete report.
 */
export async function writeRoundSummary(row: RoundSummaryRow): Promise<void> {
  const { error } = await db.from("round_summary").insert({
    player_id:   row.playerId,
    operator_id: row.operatorId,
    game_id:     row.gameId,
    game_slug:   row.gameSlug,
    round_ref:   row.roundRef,
    stake:       row.stake,
    payout:      row.payout,
    outcome:     row.outcome,
  });
  if (error) console.error(`[round_summary] insert failed (${row.gameSlug}):`, error.message);
}
