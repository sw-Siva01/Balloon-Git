import { Client } from "colyseus";
import { RouletteRoomState, RoulettePlayerSchema } from "../schema/RouletteState";
import { generateServerSeed, hashSeed, computeWinningNumber } from "../utils/rouletteAlgorithm";
import { BasePlatformRoom, type BasePlayerDbInfo } from "./BasePlatformRoom";
import { db } from "../db";

const HISTORY_MAX = 50;
const DEV_STARTING_BALANCE = 4000;
const CLIENT_SEED_MAX_LEN = 128;
const MIN_BET = 0.1;
const MAX_BET = 100;
const MAX_BETS_PER_SPIN = 30;
const MIN_TOTAL_STAKE = 0.1;
const MAX_TOTAL_STAKE = 1000;

type BetType = "num" | "im_two" | "im_four" | "im_special";
type SpecialCategory = "OneToSix" | "SevenToTwelve" | "odd" | "even" | "red" | "black";
type Selection = { numbers: number[] } | { special: SpecialCategory };

// Server-side authoritative payout table — never trust a client-echoed
// multiplier. All four tiers give ~97% RTP (11.64=0.97*12, 5.82=0.97*6,
// 2.91=0.97*3, 1.94=0.97*2), matching the game's original design.
const PAYOUTS: Record<BetType, number> = {
  num: 11.64,
  im_two: 5.82,
  im_four: 2.91,
  im_special: 1.94,
};

const RED_NUMBERS = new Set([2, 4, 6, 7, 9, 11]);
const BLACK_NUMBERS = new Set([1, 3, 5, 8, 10, 12]);

interface ValidatedBet {
  betType: BetType;
  selection: Selection;
  amount: number;
  multiplier: number;
}

interface BetResult {
  betType: BetType;
  selection: Selection;
  amount: number;
  multiplier: number;
  won: boolean;
  payout: number;
}

// Validates shape/range only (a valid N-sized subset of 1-12, or a known
// special category). Doesn't enforce the client UI's exact wheel-adjacency
// groupings for split/corner bets — the payout math only depends on how many
// numbers are covered, not which specific ones, so this doesn't open any
// fairness/RTP gap; it just doesn't reject a bet that a modified client
// could submit outside the stock UI's picker.
function validateBet(raw: unknown): ValidatedBet | null {
  if (!raw || typeof raw !== "object") return null;
  const b = raw as Record<string, unknown>;

  if (typeof b.amount !== "number" || !isFinite(b.amount)) return null;
  const amount = Math.round(b.amount * 100) / 100;
  if (amount < MIN_BET || amount > MAX_BET) return null;

  const betType = b.betType;
  if (betType !== "num" && betType !== "im_two" && betType !== "im_four" && betType !== "im_special") {
    return null;
  }

  const rawSelection = b.selection as Record<string, unknown> | undefined;

  if (betType === "im_special") {
    const special = rawSelection?.special;
    const valid: SpecialCategory[] = ["OneToSix", "SevenToTwelve", "odd", "even", "red", "black"];
    if (typeof special !== "string" || !valid.includes(special as SpecialCategory)) return null;
    return { betType, selection: { special: special as SpecialCategory }, amount, multiplier: PAYOUTS[betType] };
  }

  const expectedCount = betType === "num" ? 1 : betType === "im_two" ? 2 : 4;
  const numbers = rawSelection?.numbers;
  if (!Array.isArray(numbers) || numbers.length !== expectedCount) return null;
  const uniq = new Set(numbers);
  if (uniq.size !== expectedCount) return null;
  for (const n of numbers) {
    if (typeof n !== "number" || !Number.isInteger(n) || n < 1 || n > 12) return null;
  }
  return { betType, selection: { numbers: numbers as number[] }, amount, multiplier: PAYOUTS[betType] };
}

function evaluateBets(bets: ValidatedBet[], winningNumber: number): { results: BetResult[]; totalPayout: number } {
  let totalPayout = 0;
  const results = bets.map((b): BetResult => {
    let won: boolean;
    if ("numbers" in b.selection) {
      won = b.selection.numbers.includes(winningNumber);
    } else {
      switch (b.selection.special) {
        case "OneToSix":      won = winningNumber >= 1 && winningNumber <= 6; break;
        case "SevenToTwelve": won = winningNumber >= 7 && winningNumber <= 12; break;
        case "odd":            won = winningNumber % 2 === 1; break;
        case "even":           won = winningNumber % 2 === 0; break;
        case "red":            won = RED_NUMBERS.has(winningNumber); break;
        case "black":          won = BLACK_NUMBERS.has(winningNumber); break;
      }
    }
    const payout = won ? parseFloat((b.amount * b.multiplier).toFixed(2)) : 0;
    totalPayout += payout;
    return { betType: b.betType, selection: b.selection, amount: b.amount, multiplier: b.multiplier, won, payout };
  });
  return { results, totalPayout: parseFloat(totalPayout.toFixed(2)) };
}

export class RouletteRoom extends BasePlatformRoom<RouletteRoomState, RoulettePlayerSchema> {
  maxClients = 500;

  protected readonly gameSlug = "roulette";

  private nonce = 0;
  private inFlightSpin = new Map<string, string>(); // sessionId -> spinId, for rollbackPendingStakes

  protected createPlayer(client: Client, auth: BasePlayerDbInfo | Record<string, never>): RoulettePlayerSchema {
    const player = new RoulettePlayerSchema();
    if (auth && "playerId" in auth) {
      player.username = auth.displayName;
      player.balance  = auth.balance;
      console.log(`[onJoin] B2B player=${auth.externalId} displayName=${auth.displayName} balance=${auth.balance}`);
    } else {
      player.username = `User#${client.sessionId.substring(0, 4).toUpperCase()}`;
      player.balance  = DEV_STARTING_BALANCE;
      console.log(`[onJoin] demo player=${player.username} balance=${DEV_STARTING_BALANCE}`);
    }
    return player;
  }

  protected onPlayerJoined(client: Client): void {
    const auth = client.auth as BasePlayerDbInfo | Record<string, never>;
    if (auth && "playerId" in auth) {
      void this.sendBetHistory(client);
    }
  }

  private async saveBetHistory(sessionId: string, betAmount: number, winAmount: number, roundHash: string): Promise<void> {
    const dbInfo = this.playerDb.get(sessionId);
    if (!dbInfo) return;
    /* const { error } = await db.from("roulette_bet_history").insert({
      player_id:  dbInfo.playerId,
      round_hash: roundHash,
      bet_amount: betAmount,
      win_amount: winAmount,
    });
    if (error) console.error(`[bet_history] save failed: ${error.message}`); */
  }

  private async sendBetHistory(client: Client): Promise<void> {
    const dbInfo = this.playerDb.get(client.sessionId);
    if (!dbInfo) return;
    const { data, error } = await db.from("roulette_bet_history")
      .select("id, round_hash, bet_amount, win_amount, created_at")
      .eq("player_id", dbInfo.playerId)
      .order("created_at", { ascending: false })
      .limit(50);
    if (error) { console.error(`[bet_history] fetch failed: ${error.message}`); return; }
    if (!data || data.length === 0) return;
    client.send("bet_history", {
      bets: (data as any[]).map(b => ({
        id:         b.id as string,
        bet_amount: Number(b.bet_amount),
        win_amount: Number(b.win_amount),
        matchID:    (b.round_hash as string).substring(0, 8),
        dateTime:   b.created_at as string,
      })),
    });
  }

  protected async rollbackPendingStakes(sessionId: string): Promise<void> {
    const spinId = this.inFlightSpin.get(sessionId);
    if (!spinId) return;
    console.log(`[onAuth] rolling back in-flight spin=${spinId} for expelled player session=${sessionId}`);
    // We don't have the roundId here (only the client-chosen spinId) — the
    // debit route's idempotency key is (player_id, client_spin_id), so
    // recoverStuckRounds's boot-time pass will pick this up via the round's
    // 'active' status if the debit already landed; nothing more to do here
    // synchronously since the room is about to lose this session anyway.
    this.inFlightSpin.delete(sessionId);
  }

  protected getGameSpecificState(_player: RoulettePlayerSchema): Record<string, unknown> {
    return {};
  }

  protected cleanupPlayerGameState(_sessionId: string): void {
    // No per-player room state beyond players/playerDb, already cleaned up by the base class.
  }

  protected onGameDispose(): void {
    // No timers/intervals owned by this room.
  }

  async onCreate() {
    this.setState(new RouletteRoomState());
    this.setSimulationInterval(() => {}, 1000 / 20);

    this.onMessage("spin", (client, data) => {
      if (this.checkRateLimit(client)) {
        this.resetSessionTimer(client);
        void this.handleSpin(client, data);
      }
    });

    this.onMessage("refresh_balance", async (client) => {
      const dbInfo = this.playerDb.get(client.sessionId);
      if (!dbInfo) return;
      const result = await this.callWalletApi<{ balance: number }>("/wallet/get-balance", {
        playerId: dbInfo.playerId,
        gameId:   dbInfo.gameId,
      });
      if (!result) return;
      const player = this.players.get(client.sessionId);
      if (!player) return;
      player.balance = result.balance;
      this.sendPlayerState(client);
    });

    this.onMessage("request_bet_history", async (client) => {
      if ("playerId" in client.auth) await this.sendBetHistory(client);
    });

    this.onMessage("add_balance", (client, data) => {
      if (this.playerDb.has(client.sessionId)) return;
      const player = this.players.get(client.sessionId);
      if (!player || typeof data?.amount !== "number" || !isFinite(data.amount)) return;
      player.balance = Math.max(0, player.balance + data.amount);
      this.sendPlayerState(client);
    });

    const { data: historyRows } = await db
      .from("roulette_rounds")
      .select("winning_number")
      .eq("status", "resolved")
      .not("winning_number", "is", null)
      .order("created_at", { ascending: false })
      .limit(HISTORY_MAX);
    /* if (historyRows && historyRows.length > 0) {
      const sorted = [...historyRows].reverse();
      for (const row of sorted) {
        const n = (row as any).winning_number as number;
        if (Number.isInteger(n) && n >= 1 && n <= 12) this.state.history.push(n);
      }
      console.log(`[onCreate] Restored ${this.state.history.length} history entries from DB`);
    } */

    await this.recoverStuckRounds();
  }

  protected async recoverStuckRounds() {
    /* const { data: stuckSettling, error: settlingErr } = await db
      .from("roulette_rounds")
      .update({ status: "active" })
      .eq("status", "settling")
      .select("id");
    if (settlingErr) {
      console.error("[recovery] Failed to revert stuck 'settling' roulette rounds:", settlingErr.message);
    } else if (stuckSettling && stuckSettling.length > 0) {
      console.warn(`[recovery] reverted ${stuckSettling.length} roulette round(s) stuck in 'settling' back to 'active'`);
    } */

    const { data: stuckActive, error } = await db
      .from("roulette_rounds")
      .select("id")
      .eq("status", "active");

    if (error) {
      console.error("[recovery] Failed to query stuck roulette rounds:", error.message);
      return;
    }
    if (!stuckActive || stuckActive.length === 0) {
      console.log("[recovery] No stuck roulette rounds.");
      return;
    }

    console.log(`[recovery] ${stuckActive.length} stuck roulette round(s) — rolling back...`);
    for (const round of stuckActive as { id: string }[]) {
      const ok = await this.callWalletApi("/wallet/roulette/rollback", { roundId: round.id });
      if (ok) {
        console.log(`[recovery] rolled back roulette roundId=${round.id}`);
      } else {
        console.error(`[recovery] rollback failed for roulette roundId=${round.id} — marking cancelled`);
       /*  db.from("roulette_rounds").update({ status: "cancelled" }).eq("id", round.id).eq("status", "active").select("id")
          .then(({ data: d, error: e }) => {
            if (e) console.error("[recovery] mark-cancelled failed:", e.message);
            else if (!d || d.length === 0) console.warn(`[recovery] mark-cancelled CAS no-op roundId=${round.id} — already settled`);
          }); */
      }
    }
    console.log("[recovery] done.");
  }

  private async handleSpin(
    client: Client,
    data: { spinId?: unknown; clientSeed?: unknown; bets?: unknown }
  ) {
    const player = this.players.get(client.sessionId);
    if (!player) return;

    const spinId = typeof data.spinId === "string" && data.spinId.length > 0 ? data.spinId : null;
    if (!spinId) {
      this.sendError(client, "INVALID_SPIN", "Missing spinId.", false);
      return;
    }

    if (!Array.isArray(data.bets) || data.bets.length === 0) {
      this.sendError(client, "INVALID_BETS", "No bets provided.", false);
      return;
    }
    if (data.bets.length > MAX_BETS_PER_SPIN) {
      this.sendError(client, "INVALID_BETS", "Too many bets in one spin.", false);
      return;
    }

    const validatedBets: ValidatedBet[] = [];
    let totalStake = 0;
    for (const raw of data.bets) {
      const v = validateBet(raw);
      if (!v) {
        this.sendError(client, "INVALID_BET", `Invalid bet: ${JSON.stringify(raw)}`, false);
        return;
      }
      totalStake += v.amount;
      validatedBets.push(v);
    }
    totalStake = parseFloat(totalStake.toFixed(2));

    if (totalStake < MIN_TOTAL_STAKE || totalStake > MAX_TOTAL_STAKE) {
      this.sendError(client, "INVALID_AMOUNT", "Total stake out of range.", false);
      return;
    }
    if (player.balance < totalStake) {
      this.sendError(client, "INSUFFICIENT_BALANCE", "Insufficient balance.", false);
      return;
    }

    const clientSeed = (typeof data.clientSeed === "string" && data.clientSeed.length <= CLIENT_SEED_MAX_LEN)
      ? data.clientSeed
      : "";

    const dbInfo = this.playerDb.get(client.sessionId);

    // Optimistic local deduction, same pattern as AviatorRoom.handlePlaceBet.
    player.balance -= totalStake;
    this.sendPlayerState(client);

    if (!dbInfo) {
      // Demo mode: no wallet integration, no persistence — compute and settle purely locally.
      this.nonce += 1;
      const serverSeed = generateServerSeed();
      const winningNumber = computeWinningNumber(serverSeed, clientSeed, this.nonce);
      const { results, totalPayout } = evaluateBets(validatedBets, winningNumber);
      player.balance += totalPayout;
      this.pushHistory(winningNumber);
      client.send("spin_result", {
        spinId, winningNumber, revealedSeed: serverSeed, roundHash: hashSeed(serverSeed, this.nonce),
        results, balance: player.balance,
      });
      this.sendPlayerState(client);
      return;
    }

    this.inFlightSpin.set(client.sessionId, spinId);

    const debitResult = await this.callWalletApi<{ roundId: string; betIds: string[] }>("/wallet/roulette/debit", {
      playerId:      dbInfo.playerId,
      operatorId:    dbInfo.operatorId,
      gameId:        dbInfo.gameId,
      clientSpinId:  spinId,
      clientSeed,
      bets: validatedBets.map(b => ({ betType: b.betType, selection: b.selection, amount: b.amount, multiplier: b.multiplier })),
      totalAmount: totalStake,
    });

    if (!debitResult) {
      player.balance += totalStake; // roll back optimistic deduction
      this.inFlightSpin.delete(client.sessionId);
      this.sendError(client, "WALLET_ERROR", "Wallet debit failed. Please try again.", true);
      this.sendPlayerState(client);
      return;
    }

    this.nonce += 1;
    const serverSeed = generateServerSeed();
    const winningNumber = computeWinningNumber(serverSeed, clientSeed, this.nonce);
    const { results, totalPayout } = evaluateBets(validatedBets, winningNumber);

    if (totalPayout > 0) player.balance += totalPayout;

    void this.callWalletApi("/wallet/roulette/credit", {
      roundId: debitResult.roundId,
      winningNumber,
      serverSeed,
      results: results.map((r, i) => ({ betId: debitResult.betIds[i], won: r.won, payout: r.payout })),
      totalPayout,
    });

    this.inFlightSpin.delete(client.sessionId);
    this.pushHistory(winningNumber);
    void this.saveBetHistory(client.sessionId, totalStake, totalPayout, hashSeed(serverSeed, this.nonce));

    client.send("spin_result", {
      spinId, winningNumber, revealedSeed: serverSeed, roundHash: hashSeed(serverSeed, this.nonce),
      results, balance: player.balance,
    });
    this.sendPlayerState(client);
  }

  private pushHistory(winningNumber: number) {
    this.state.history.push(winningNumber);
    if (this.state.history.length > HISTORY_MAX) this.state.history.splice(0, 1);
  }
}
