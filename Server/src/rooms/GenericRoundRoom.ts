import { Client } from "colyseus";
import { GenericRoundState, GenericPlayerSchema } from "../schema/GenericRoundState";
import { BasePlatformRoom, type BasePlayerDbInfo } from "./BasePlatformRoom";
import { getGamePayoutConfig, type GamePayoutConfig, type ValidatedGenericBet } from "../games/payoutTables";
import { db } from "../db";

const HISTORY_MAX = 50;
const DEV_STARTING_BALANCE = 4000;
const CLIENT_SEED_MAX_LEN = 128;
const MAX_BETS_HARD_CAP = 20; // outer bound regardless of per-game config

// One room class serves every game registered in payoutTables.ts's
// gamePayoutRegistry — the game slug comes from options passed at
// gameServer.define(slug, GenericRoundRoom, { gameSlug: slug }), one line
// per game in games/registry.ts, not a new file. Mirrors RouletteRoom.ts's
// handleSpin shape almost exactly; see that file for a worked bespoke
// reference of the same flow before it was generalized here.
//
// Aviator is not, and should not be, served by this room — its live-ticking
// multiplier with an uncertain-timing cashout doesn't resolve in one atomic
// "play" message the way this room assumes.
export class GenericRoundRoom extends BasePlatformRoom<GenericRoundState, GenericPlayerSchema> {
  maxClients = 500;

  private _gameSlug = "";
  protected get gameSlug(): string {
    return this._gameSlug;
  }

  private payoutConfig!: GamePayoutConfig;
  private nonce = 0;
  private inFlightRound = new Map<string, string>(); // sessionId -> clientRoundId, for rollbackPendingStakes

  protected createPlayer(client: Client, auth: BasePlayerDbInfo | Record<string, never>): GenericPlayerSchema {
    const player = new GenericPlayerSchema();
    if (auth && "playerId" in auth) {
      player.username = auth.displayName;
      player.balance  = auth.balance;
      console.log(`[onJoin:${this._gameSlug}] B2B player=${auth.externalId} displayName=${auth.displayName} balance=${auth.balance}`);
    } else {
      player.username = `User#${client.sessionId.substring(0, 4).toUpperCase()}`;
      player.balance  = DEV_STARTING_BALANCE;
      console.log(`[onJoin:${this._gameSlug}] demo player=${player.username} balance=${DEV_STARTING_BALANCE}`);
    }
    return player;
  }

  protected async rollbackPendingStakes(sessionId: string): Promise<void> {
    const clientRoundId = this.inFlightRound.get(sessionId);
    if (!clientRoundId) return;
    console.log(`[onAuth:${this._gameSlug}] rolling back in-flight round=${clientRoundId} for expelled player session=${sessionId}`);
    // Same reasoning as RouletteRoom: we don't have the roundId here (only the
    // client-chosen clientRoundId) — recoverStuckRounds' boot-time pass will
    // pick this up via the round's 'active' status if the debit already landed.
    this.inFlightRound.delete(sessionId);
  }

  protected getGameSpecificState(_player: GenericPlayerSchema): Record<string, unknown> {
    return {};
  }

  protected cleanupPlayerGameState(_sessionId: string): void {
    // No per-player room state beyond players/playerDb, already cleaned up by the base class.
  }

  protected onGameDispose(): void {
    // No timers/intervals owned by this room.
  }

  protected onPlayerJoined(client: Client): void {
    const auth = client.auth as BasePlayerDbInfo | Record<string, never>;
    if (auth && "playerId" in auth) {
      void this.sendBetHistory(client);
    }
  }

  async onCreate(options: { gameSlug?: string }) {
    const slug = options?.gameSlug;
    if (!slug) throw new Error("GenericRoundRoom requires options.gameSlug (set via gameServer.define(slug, GenericRoundRoom, { gameSlug: slug }))");
    const config = getGamePayoutConfig(slug);
    if (!config) throw new Error(`No payout config registered for game slug "${slug}" in games/payoutTables.ts`);
    this._gameSlug = slug;
    this.payoutConfig = config;

    this.setState(new GenericRoundState());
    this.setSimulationInterval(() => {}, 1000 / 20);

    this.onMessage("play", (client, data) => {
      if (this.checkRateLimit(client)) {
        this.resetSessionTimer(client);
        void this.handlePlay(client, data);
      }
    });

    this.onMessage("request_bet_history", async (client) => {
      if ("playerId" in client.auth) await this.sendBetHistory(client);
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

    this.onMessage("add_balance", (client, data) => {
      if (this.playerDb.has(client.sessionId)) return;
      const player = this.players.get(client.sessionId);
      if (!player || typeof data?.amount !== "number" || !isFinite(data.amount)) return;
      player.balance = Math.max(0, player.balance + data.amount);
      this.sendPlayerState(client);
    });

    await this.recoverStuckRounds();
  }

  protected async recoverStuckRounds() {
    const { data: stuckSettling, error: settlingErr } = await db
      .from("game_rounds")
      .update({ status: "active" })
      .eq("game_slug", this._gameSlug)
      .eq("status", "settling")
      .select("id");
    if (settlingErr) {
      console.error(`[recovery:${this._gameSlug}] Failed to revert stuck 'settling' rounds:`, settlingErr.message);
    } else if (stuckSettling && stuckSettling.length > 0) {
      console.warn(`[recovery:${this._gameSlug}] reverted ${stuckSettling.length} round(s) stuck in 'settling' back to 'active'`);
    }

    const { data: stuckActive, error } = await db
      .from("game_rounds")
      .select("id")
      .eq("game_slug", this._gameSlug)
      .eq("status", "active");

    if (error) {
      console.error(`[recovery:${this._gameSlug}] Failed to query stuck rounds:`, error.message);
      return;
    }
    if (!stuckActive || stuckActive.length === 0) {
      console.log(`[recovery:${this._gameSlug}] No stuck rounds.`);
      return;
    }

    console.log(`[recovery:${this._gameSlug}] ${stuckActive.length} stuck round(s) — rolling back...`);
    for (const round of stuckActive as { id: string }[]) {
      const ok = await this.callWalletApi(`/wallet/${this._gameSlug}/rollback`, { roundId: round.id });
      if (ok) {
        console.log(`[recovery:${this._gameSlug}] rolled back roundId=${round.id}`);
      } else {
        console.error(`[recovery:${this._gameSlug}] rollback failed for roundId=${round.id} — marking cancelled`);
        db.from("game_rounds").update({ status: "cancelled" }).eq("id", round.id).eq("status", "active").select("id")
          .then(({ data: d, error: e }) => {
            if (e) console.error(`[recovery:${this._gameSlug}] mark-cancelled failed:`, e.message);
            else if (!d || d.length === 0) console.warn(`[recovery:${this._gameSlug}] mark-cancelled CAS no-op roundId=${round.id} — already settled`);
          });
      }
    }
    console.log(`[recovery:${this._gameSlug}] done.`);
  }

  private async handlePlay(
    client: Client,
    data: { roundId?: unknown; clientSeed?: unknown; bets?: unknown }
  ) {
    const player = this.players.get(client.sessionId);
    if (!player) return;

    const clientRoundId = typeof data.roundId === "string" && data.roundId.length > 0 ? data.roundId : null;
    if (!clientRoundId) {
      this.sendError(client, "INVALID_ROUND", "Missing roundId.", false);
      return;
    }

    if (!Array.isArray(data.bets) || data.bets.length === 0) {
      this.sendError(client, "INVALID_BETS", "No bets provided.", false);
      return;
    }
    if (data.bets.length > Math.min(this.payoutConfig.maxBetsPerRound, MAX_BETS_HARD_CAP)) {
      this.sendError(client, "INVALID_BETS", "Too many bets in one round.", false);
      return;
    }

    const validatedBets: ValidatedGenericBet[] = [];
    let totalStake = 0;
    for (const raw of data.bets) {
      const v = this.payoutConfig.validateBet(raw);
      if (!v) {
        this.sendError(client, "INVALID_BET", `Invalid bet: ${JSON.stringify(raw)}`, false);
        return;
      }
      totalStake += v.amount;
      validatedBets.push(v);
    }
    totalStake = parseFloat(totalStake.toFixed(2));

    if (totalStake < this.payoutConfig.minTotalStake || totalStake > this.payoutConfig.maxTotalStake) {
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

    // Optimistic local deduction, same pattern as AviatorRoom/RouletteRoom.
    player.balance -= totalStake;
    this.sendPlayerState(client);

    if (!dbInfo) {
      // Demo mode: no wallet integration, no persistence — compute and settle purely locally.
      this.nonce += 1;
      const outcome = this.payoutConfig.resolveOutcome("", clientSeed, this.nonce);
      const { results, totalPayout } = this.payoutConfig.evaluateBets(validatedBets, outcome);
      player.balance += totalPayout;
      client.send("play_result", { roundId: clientRoundId, outcome, results, balance: player.balance });
      this.sendPlayerState(client);
      return;
    }

    this.inFlightRound.set(client.sessionId, clientRoundId);

    const debitResult = await this.callWalletApi<{ roundId: string; betIds: string[] }>(`/wallet/${this._gameSlug}/debit`, {
      playerId:      dbInfo.playerId,
      operatorId:    dbInfo.operatorId,
      gameId:        dbInfo.gameId,
      clientRoundId,
      clientSeed,
      bets: validatedBets.map(b => ({ betType: b.betType, selection: b.selection, amount: b.amount, multiplier: b.multiplier })),
      totalAmount: totalStake,
    });

    if (!debitResult) {
      player.balance += totalStake; // roll back optimistic deduction
      this.inFlightRound.delete(client.sessionId);
      this.sendError(client, "WALLET_ERROR", "Wallet debit failed. Please try again.", true);
      this.sendPlayerState(client);
      return;
    }

    this.nonce += 1;
    const outcome = this.payoutConfig.resolveOutcome("", clientSeed, this.nonce);
    const { results, totalPayout } = this.payoutConfig.evaluateBets(validatedBets, outcome);

    if (totalPayout > 0) player.balance += totalPayout;

    void this.callWalletApi(`/wallet/${this._gameSlug}/credit`, {
      roundId: debitResult.roundId,
      outcome,
      serverSeed: "",
      results: results.map((r, i) => ({ betId: debitResult.betIds[i], won: r.won, payout: r.payout })),
      totalPayout,
    });

    this.inFlightRound.delete(client.sessionId);
    void this.saveBetHistory(client.sessionId, totalStake, totalPayout);

    client.send("play_result", { roundId: clientRoundId, outcome, results, balance: player.balance });
    this.sendPlayerState(client);
  }

  private async saveBetHistory(sessionId: string, betAmount: number, winAmount: number): Promise<void> {
    const dbInfo = this.playerDb.get(sessionId);
    if (!dbInfo) return;
    const { error } = await db.from("game_bet_history").insert({
      game_slug:  this._gameSlug,
      player_id:  dbInfo.playerId,
      bet_amount: betAmount,
      win_amount: winAmount,
    });
    if (error) console.error(`[bet_history:${this._gameSlug}] save failed: ${error.message}`);
  }

  private async sendBetHistory(client: Client): Promise<void> {
    const dbInfo = this.playerDb.get(client.sessionId);
    if (!dbInfo) return;
    const { data, error } = await db.from("game_bet_history")
      .select("id, round_hash, bet_amount, win_amount, created_at")
      .eq("game_slug", this._gameSlug)
      .eq("player_id", dbInfo.playerId)
      .order("created_at", { ascending: false })
      .limit(HISTORY_MAX);
    if (error) { console.error(`[bet_history:${this._gameSlug}] fetch failed: ${error.message}`); return; }
    if (!data || data.length === 0) return;
    client.send("bet_history", {
      bets: (data as any[]).map(b => ({
        id:         b.id as string,
        bet_amount: Number(b.bet_amount),
        win_amount: Number(b.win_amount),
        matchID:    (b.round_hash as string || "").substring(0, 8),
        dateTime:   b.created_at as string,
      })),
    });
  }
}
