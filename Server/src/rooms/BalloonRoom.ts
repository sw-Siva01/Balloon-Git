import { Client } from "colyseus";
import { BalloonRoomState, BalloonPlayerSchema } from "../schema/BalloonState";
import { generateServerSeed, hashSeed, computeBurstMultiplier } from "../utils/balloonAlgorithm";
import { writeRoundSummary } from "../utils/roundSummary";
import { BasePlatformRoom, type BasePlayerDbInfo } from "./BasePlatformRoom";
import { db } from "../db";

const TICK_INTERVAL_MS      = 200;    // matches GameController's incrementInterval (0.2s)
const INCREMENT_RATE        = 1.01;   // matches GameController's incrementRate
const HISTORY_MAX           = 50;
const DEV_STARTING_BALANCE  = 4000;
const CLIENT_SEED_MAX_LEN   = 128;
const MIN_BET               = 0.1;
const MAX_BET               = 100;
const AUTO_TARGET_MIN       = 1.01;

interface RoundInfo {
  clientRoundId: string;
  roundId:       string | null; // DB row id, filled in once the debit resolves
  serverSeed:    string;
  nonce:         number;
  burstMultiplier: number;
  houseEdgeDivisor: number;
}

export class BalloonRoom extends BasePlatformRoom<BalloonRoomState, BalloonPlayerSchema> {
  maxClients = 500;

  protected readonly gameSlug = "balloon";

  private nonce = 0;
  private tickInterval: ReturnType<typeof setInterval> | null = null;
  private activeRounds = new Map<string, RoundInfo>(); // sessionId -> round in flight

  protected createPlayer(client: Client, auth: BasePlayerDbInfo | Record<string, never>): BalloonPlayerSchema {
    const player = new BalloonPlayerSchema();
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

  protected getGameSpecificState(player: BalloonPlayerSchema): Record<string, unknown> {
    return {
      phase:      player.phase,
      betAmount:  player.betAmount,
      multiplier: player.multiplier,
      holding:    player.holding,
      autoTarget: player.autoTarget,
    };
  }

  protected cleanupPlayerGameState(_sessionId: string): void {
    // activeRounds entries are cleaned up in rollbackPendingStakes before
    // removePlayer runs (see BasePlatformRoom.onAuth / onLeave call order).
  }

  protected onGameDispose(): void {
    if (this.tickInterval) { clearInterval(this.tickInterval); this.tickInterval = null; }
  }

  protected async rollbackPendingStakes(sessionId: string): Promise<void> {
    const round = this.activeRounds.get(sessionId);
    if (!round) return;
    console.log(`[onAuth] rolling back in-flight balloon round=${round.clientRoundId} for expelled player session=${sessionId}`);
    if (round.roundId) {
      void this.callWalletApi("/wallet/balloon/rollback", { roundId: round.roundId });
    }
    this.activeRounds.delete(sessionId);
  }

  async onCreate() {
    this.setState(new BalloonRoomState());
    this.setSimulationInterval(() => {}, 1000 / 20);

    this.onMessage("start_round", (client, data) => {
      if (this.checkRateLimit(client)) {
        this.resetSessionTimer(client);
        void this.handleStartRound(client, data);
      }
    });

    this.onMessage("set_holding", (client, data) => {
      if (this.checkRateLimit(client)) {
        this.resetSessionTimer(client);
        this.handleSetHolding(client, data);
      }
    });

    this.onMessage("cash_out", (client) => {
      if (this.checkRateLimit(client)) {
        this.resetSessionTimer(client);
        this.handleCashOut(client);
      }
    });

    this.onMessage("refresh_balance", async (client) => {
      const dbInfo = this.playerDb.get(client.sessionId);
      if (!dbInfo) return;
      const player = this.players.get(client.sessionId);
      if (!player || player.phase === "inflating") return; // don't stomp balance mid-round
      const result = await this.callWalletApi<{ balance: number }>("/wallet/get-balance", {
        playerId: dbInfo.playerId,
        gameId:   dbInfo.gameId,
      });
      if (!result) return;
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
      .from("balloon_rounds")
      .select("result_multiplier")
      .in("status", ["burst", "cashed_out"])
      .not("result_multiplier", "is", null)
      .order("created_at", { ascending: false })
      .limit(HISTORY_MAX);
    if (historyRows && historyRows.length > 0) {
      const sorted = [...historyRows].reverse();
      for (const row of sorted) {
        const m = parseFloat((row as any).result_multiplier);
        if (isFinite(m) && m > 0) this.state.history.push(m);
      }
      console.log(`[onCreate] Restored ${this.state.history.length} history entries from DB`);
    }

    await this.recoverStuckRounds();

    this.tickInterval = setInterval(() => this.onTick(), TICK_INTERVAL_MS);
  }

  protected async recoverStuckRounds() {
    const { data: stuckSettling, error: settlingErr } = await db
      .from("balloon_rounds")
      .update({ status: "active" })
      .eq("status", "settling")
      .select("id");
    if (settlingErr) {
      console.error("[recovery] Failed to revert stuck 'settling' balloon rounds:", settlingErr.message);
    } else if (stuckSettling && stuckSettling.length > 0) {
      console.warn(`[recovery] reverted ${stuckSettling.length} balloon round(s) stuck in 'settling' back to 'active'`);
    }

    const { data: stuckActive, error } = await db
      .from("balloon_rounds")
      .select("id")
      .eq("status", "active");

    if (error) {
      console.error("[recovery] Failed to query stuck balloon rounds:", error.message);
      return;
    }
    if (!stuckActive || stuckActive.length === 0) {
      console.log("[recovery] No stuck balloon rounds.");
      return;
    }

    console.log(`[recovery] ${stuckActive.length} stuck balloon round(s) — rolling back...`);
    for (const round of stuckActive as { id: string }[]) {
      const ok = await this.callWalletApi("/wallet/balloon/rollback", { roundId: round.id });
      if (ok) {
        console.log(`[recovery] rolled back balloon roundId=${round.id}`);
      } else {
        console.error(`[recovery] rollback failed for balloon roundId=${round.id} — marking cancelled`);
        db.from("balloon_rounds").update({ status: "cancelled" }).eq("id", round.id).eq("status", "active").select("id")
          .then(({ data: d, error: e }) => {
            if (e) console.error("[recovery] mark-cancelled failed:", e.message);
            else if (!d || d.length === 0) console.warn(`[recovery] mark-cancelled CAS no-op roundId=${round.id} — already settled`);
          });
      }
    }
    console.log("[recovery] done.");
  }

  // --- round lifecycle ---

  private async handleStartRound(
    client: Client,
    data: { clientRoundId?: unknown; betAmount?: unknown; clientSeed?: unknown; autoTarget?: unknown }
  ) {
    const player = this.players.get(client.sessionId);
    if (!player) return;

    if (player.phase !== "idle") {
      this.sendError(client, "ROUND_IN_PROGRESS", "A balloon round is already in progress.", false);
      return;
    }

    const clientRoundId = typeof data.clientRoundId === "string" && data.clientRoundId.length > 0 ? data.clientRoundId : null;
    if (!clientRoundId) {
      this.sendError(client, "INVALID_ROUND", "Missing clientRoundId.", false);
      return;
    }

    if (typeof data.betAmount !== "number" || !isFinite(data.betAmount)) {
      this.sendError(client, "INVALID_AMOUNT", "Invalid bet amount.", false);
      return;
    }
    const betAmount = Math.round(data.betAmount * 100) / 100;
    if (betAmount < MIN_BET || betAmount > MAX_BET) {
      this.sendError(client, "INVALID_AMOUNT", "Bet out of range.", false);
      return;
    }
    if (player.balance < betAmount) {
      this.sendError(client, "INSUFFICIENT_BALANCE", "Insufficient balance.", false);
      return;
    }

    const autoTarget = (typeof data.autoTarget === "number" && isFinite(data.autoTarget) && data.autoTarget >= AUTO_TARGET_MIN)
      ? data.autoTarget
      : 0;

    const clientSeed = (typeof data.clientSeed === "string" && data.clientSeed.length <= CLIENT_SEED_MAX_LEN)
      ? data.clientSeed
      : "";

    // Optimistic local deduction, same pattern as RouletteRoom.handleSpin.
    player.balance -= betAmount;
    player.phase       = "inflating";
    player.betAmount   = betAmount;
    player.multiplier  = 1.0;
    player.holding     = false;
    player.autoTarget  = autoTarget;
    this.sendPlayerState(client);

    const dbInfo = this.playerDb.get(client.sessionId);

    this.nonce += 1;
    const serverSeed = generateServerSeed();
    const { burstMultiplier, houseEdgeDivisor } = computeBurstMultiplier(serverSeed, clientSeed, this.nonce);

    const round: RoundInfo = {
      clientRoundId, roundId: null, serverSeed, nonce: this.nonce, burstMultiplier, houseEdgeDivisor,
    };
    this.activeRounds.set(client.sessionId, round);

    if (!dbInfo) {
      // Demo mode: no wallet integration, no persistence.
      return;
    }

    const debitResult = await this.callWalletApi<{ roundId: string }>("/wallet/balloon/debit", {
      playerId:      dbInfo.playerId,
      operatorId:    dbInfo.operatorId,
      gameId:        dbInfo.gameId,
      clientRoundId,
      amount:        betAmount,
      clientSeed,
      autoTarget,
    });

    if (!debitResult) {
      player.balance += betAmount; // roll back optimistic deduction
      player.phase = "idle";
      this.activeRounds.delete(client.sessionId);
      this.sendError(client, "WALLET_ERROR", "Wallet debit failed. Please try again.", true);
      this.sendPlayerState(client);
      return;
    }

    round.roundId = debitResult.roundId;
  }

  private handleSetHolding(client: Client, data: { holding?: unknown }) {
    const player = this.players.get(client.sessionId);
    if (!player || player.phase !== "inflating") return;
    player.holding = data.holding === true;
  }

  private handleCashOut(client: Client) {
    const player = this.players.get(client.sessionId);
    if (!player || player.phase !== "inflating") {
      this.sendError(client, "NOT_INFLATING", "No active balloon round.", false);
      return;
    }
    void this.executeCashOut(client, player);
  }

  // Global tick over every player currently holding — Balloon has no shared
  // room-wide phase (unlike Aviator's single "flying" clock), each player's
  // balloon inflates on its own independent timeline.
  private onTick() {
    this.players.forEach((player, sessionId) => {
      if (player.phase !== "inflating" || !player.holding) return;
      const client = this.clients.find((c) => c.sessionId === sessionId);
      if (!client) return;

      player.multiplier = Math.floor(player.multiplier * INCREMENT_RATE * 100) / 100;
      client.send("tick", { multiplier: player.multiplier });

      const round = this.activeRounds.get(sessionId);
      if (!round) return;

      if (player.autoTarget > 0 && player.multiplier >= player.autoTarget) {
        void this.executeCashOut(client, player);
        return;
      }

      if (player.multiplier >= round.burstMultiplier) {
        void this.resolveBurst(client, player, sessionId, round);
      }
    });
  }

  private async executeCashOut(client: Client, player: BalloonPlayerSchema) {
    const sessionId = client.sessionId;
    const round = this.activeRounds.get(sessionId);
    if (!round) return;

    const finalMultiplier = player.multiplier;
    const payout = parseFloat((player.betAmount * finalMultiplier).toFixed(2));

    player.balance += payout;
    player.phase     = "resolved";
    player.holding   = false;

    this.pushHistory(finalMultiplier);
    this.activeRounds.delete(sessionId);

    client.send("round_result", {
      clientRoundId: round.clientRoundId,
      outcome: "cashed_out",
      multiplier: finalMultiplier,
      payout,
      balance: player.balance,
    });
    this.sendPlayerState(client);

    const dbInfo = this.playerDb.get(sessionId);
    if (dbInfo && round.roundId) {
      const ok = await this.callWalletApi("/wallet/balloon/credit", {
        roundId: round.roundId,
        resultMultiplier: finalMultiplier,
        payout,
      });
      if (!ok) console.error(`[executeCashOut] wallet credit failed roundId=${round.roundId}`);
      void this.saveBetHistory(sessionId, player.betAmount, payout);
    }

    // Back to idle so the player can start another round.
    player.phase = "idle";
    player.betAmount = 0;
    player.multiplier = 1.0;
    this.sendPlayerState(client);
  }

  private async resolveBurst(client: Client, player: BalloonPlayerSchema, sessionId: string, round: RoundInfo) {
    const finalMultiplier = round.burstMultiplier;

    player.phase   = "resolved";
    player.holding = false;

    this.pushHistory(finalMultiplier);
    this.activeRounds.delete(sessionId);

    client.send("round_result", {
      clientRoundId: round.clientRoundId,
      outcome: "burst",
      multiplier: finalMultiplier,
      payout: 0,
      revealedSeed: round.serverSeed,
      roundHash: hashSeed(round.serverSeed, round.nonce),
      balance: player.balance,
    });

    const dbInfo = this.playerDb.get(sessionId);
    if (dbInfo && round.roundId) {
      db.from("balloon_rounds").update({
        status: "burst",
        result_multiplier: finalMultiplier,
        server_seed: round.serverSeed,
        server_seed_hash: hashSeed(round.serverSeed, round.nonce),
        resolved_at: new Date().toISOString(),
      }).eq("id", round.roundId).eq("status", "active").select("id")
        .then(({ data, error }) => {
          if (error) { console.error(`[bet] failed to mark burst roundId=${round.roundId}:`, error.message); return; }
          if (!data || data.length === 0) { console.warn(`[bet] mark-burst CAS no-op roundId=${round.roundId} — already settled`); return; }
          void writeRoundSummary({
            playerId: dbInfo.playerId, operatorId: dbInfo.operatorId, gameId: dbInfo.gameId,
            gameSlug: "balloon", roundRef: round.roundId!, stake: player.betAmount, payout: 0, outcome: "lost",
          });
        });
      void this.saveBetHistory(sessionId, player.betAmount, 0);
    }

    player.phase = "idle";
    player.betAmount = 0;
    player.multiplier = 1.0;
    this.sendPlayerState(client);
  }

  private pushHistory(multiplier: number) {
    this.state.history.push(parseFloat(multiplier.toFixed(2)));
    if (this.state.history.length > HISTORY_MAX) this.state.history.splice(0, 1);
  }

  private async saveBetHistory(sessionId: string, betAmount: number, winAmount: number): Promise<void> {
    const dbInfo = this.playerDb.get(sessionId);
    if (!dbInfo) return;
    const { error } = await db.from("balloon_bet_history").insert({
      player_id:  dbInfo.playerId,
      bet_amount: betAmount,
      win_amount: winAmount,
    });
    if (error) console.error(`[bet_history] save failed: ${error.message}`);
  }

  private async sendBetHistory(client: Client): Promise<void> {
    const dbInfo = this.playerDb.get(client.sessionId);
    if (!dbInfo) return;
    const { data, error } = await db.from("balloon_bet_history")
      .select("id, bet_amount, win_amount, created_at")
      .eq("player_id", dbInfo.playerId)
      .order("created_at", { ascending: false })
      .limit(HISTORY_MAX);
    if (error) { console.error(`[bet_history] fetch failed: ${error.message}`); return; }
    if (!data || data.length === 0) return;
    client.send("bet_history", {
      bets: (data as any[]).map(b => ({
        id:         b.id as string,
        bet_amount: Number(b.bet_amount),
        win_amount: Number(b.win_amount),
        dateTime:   b.created_at as string,
      })),
    });
  }
}
