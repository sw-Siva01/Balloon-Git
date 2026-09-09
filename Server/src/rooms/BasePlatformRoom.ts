import { Room, Client, CloseCode } from "colyseus";
import { Schema } from "@colyseus/schema";
import { BasePlayerSchema } from "../schema/BaseSchema";
import { verifyPlayerToken } from "../api/services/playerToken";

const MAX_MESSAGES_PER_SECOND = 5;
const SESSION_TIMEOUT_MS      = 3 * 60 * 1000;

export interface BasePlayerDbInfo {
  playerId:    string;
  operatorId:  string;
  externalId:  string;
  displayName: string;
  balance:     number;
  gameId:      string;
  currency:    string;
}

// Structural contract, not a real Schema subclass — see BasePlatformRoom
// extraction notes. AviatorRoomState already has this field; keeping it a
// plain TS interface (rather than a shared Schema base class) avoids
// reshuffling @colyseus/schema wire indices, which the shipped Unity client
// mirrors with hardcoded numeric [Type(N,...)] attributes.
export interface HasPlayerCount {
  playerCount: number;
}

/**
 * Generic platform plumbing shared by every game room: JWT auth, wallet
 * call helpers, reconnection window, rate limiting, inactivity timeout.
 * Game-specific logic (phase machine, bet handling, round persistence)
 * lives in the subclass via the abstract hooks below.
 */
export abstract class BasePlatformRoom<
  TState extends Schema & HasPlayerCount,
  TPlayer extends BasePlayerSchema = BasePlayerSchema
> extends Room {
  /*** Colyseus 0.17's Room generic parameter is not the room State type. * 
   ** Keep the actual state strongly typed here so subclasses can use: 
   ** this.state.playerCount * this.state.history * etc. 
   ***/ 
  declare state: TState;
  
  protected abstract readonly gameSlug: string;

  protected players  = new Map<string, TPlayer>();
  protected playerDb = new Map<string, BasePlayerDbInfo>();

  private msgCounts     = new Map<string, { count: number; windowStart: number }>();
  private sessionTimers = new Map<string, ReturnType<typeof setTimeout>>();

  // --- abstract hooks subclasses must implement ---
  protected abstract createPlayer(client: Client, auth: BasePlayerDbInfo | Record<string, never>): TPlayer;
  protected abstract rollbackPendingStakes(sessionId: string): Promise<void>;
  protected abstract getGameSpecificState(player: TPlayer): Record<string, unknown>;
  protected abstract cleanupPlayerGameState(sessionId: string): void;
  // Contract only — not called from this base class (onCreate stays fully in
  // the subclass, see AviatorRoom). Declared here as a compiler-enforced
  // forcing function for future game rooms.
  protected abstract recoverStuckRounds(): Promise<void>;

  // --- optional hooks, default no-op ---
  protected onPlayerJoined(_client: Client): void {}
  protected onGameDispose(): void {}

  protected checkRateLimit(client: Client): boolean {
    const now   = Date.now();
    const entry = this.msgCounts.get(client.sessionId) ?? { count: 0, windowStart: now };

    if (now - entry.windowStart >= 1000) {
      entry.count = 1; entry.windowStart = now;
    } else {
      entry.count += 1;
    }
    this.msgCounts.set(client.sessionId, entry);

    if (entry.count > MAX_MESSAGES_PER_SECOND) {
      this.sendError(client, "RATE_LIMITED", "Too many messages — slow down.", true);
      return false;
    }
    return true;
  }

  protected resetSessionTimer(client: Client) {
    const existing = this.sessionTimers.get(client.sessionId);
    if (existing) clearTimeout(existing);
    const timer = setTimeout(() => {
      this.sendError(client, "SESSION_TIMEOUT", "Session expired due to inactivity.", false);
      client.leave(CloseCode.CONSENTED);
    }, SESSION_TIMEOUT_MS);
    this.sessionTimers.set(client.sessionId, timer);
  }

  protected clearSessionTimer(sessionId: string) {
    const t = this.sessionTimers.get(sessionId);
    if (t) { clearTimeout(t); this.sessionTimers.delete(sessionId); }
  }

  protected async callWalletApi<T>(path: string, body: unknown): Promise<T | null> {
    const secret = process.env.INTERNAL_SECRET ?? "";
    const port   = process.env.API_PORT ?? "3000";
    try {
      const res = await fetch(`http://127.0.0.1:${port}${path}`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          "Authorization": `Bearer ${secret}`,
        },
        body: JSON.stringify(body),
        signal: AbortSignal.timeout(8000),
      });
      if (!res.ok) {
        const text = await res.text().catch(() => "");
        console.error(`[wallet:${this.gameSlug}] POST ${path} → ${res.status}: ${text}`);
        return null;
      }
      return res.json() as Promise<T>;
    } catch (err) {
      console.error(`[wallet:${this.gameSlug}] POST ${path} error:`, err);
      return null;
    }
  }

  protected sendError(client: Client, code: string, message: string, retryable: boolean) {
    client.send("error", { error: { code, message, retryable } });
  }

  async onAuth(client: Client, options: { token?: string; ip?: string; ua?: string }): Promise<BasePlayerDbInfo | Record<string, never>> {
    console.log(`[onAuth] called — token=${options?.token ? "PRESENT" : "NONE"}`);
    if (!options?.token) {
      return {};
    }

    let payload: Awaited<ReturnType<typeof verifyPlayerToken>>;
    try {
      payload = await verifyPlayerToken(options.token, options.ip, options.ua);
      console.log(`[onAuth] token verified — externalId=${payload.externalId} displayName=${payload.displayName} sub=${payload.sub}`);
    } catch (err) {
      console.error(`[onAuth] token verification FAILED:`, err);
      throw new Error("Invalid or expired token");
    }

    if (payload.gameSlug !== this.gameSlug) {
      console.error(`[onAuth] GAME_MISMATCH — token issued for game=${payload.gameSlug}, this room is "${this.gameSlug}" (externalId=${payload.externalId})`);
      throw new Error("GAME_MISMATCH");
    }

    for (const [sessionId, info] of this.playerDb.entries()) {
      if (info.playerId === payload.sub) {
        console.warn(`[onAuth] expiring old session for playerId=${payload.sub}`);

        await this.rollbackPendingStakes(sessionId);

        const oldClient = this.clients.find(c => c.sessionId === sessionId);
        if (oldClient) {
          this.sendError(oldClient, "SESSION_EXPIRED", "Your session was opened in another window.", false);
          oldClient.leave(CloseCode.CONSENTED);
        }
        this.removePlayer(sessionId);
        break;
      }
    }

    const result = await this.callWalletApi<{ balance: number }>("/wallet/get-balance", {
      playerId: payload.sub,
      gameId:   payload.gameId,
    });
    if (!result) {
      console.error(`[onAuth] get_balance returned null for player=${payload.externalId}`);
      throw new Error("get_balance callback failed — operator unreachable");
    }
    const balance = result.balance;
    console.log(`[onAuth] player=${payload.externalId} game=${payload.gameId} currency=${payload.currency} operatorBalance=${balance}`);

    return {
      playerId:    payload.sub,
      operatorId:  payload.operatorId,
      externalId:  payload.externalId,
      displayName: payload.displayName ?? payload.externalId,
      balance,
      gameId:      payload.gameId,
      currency:    payload.currency,
    };
  }

  onJoin(client: Client) {
    const auth   = client.auth as BasePlayerDbInfo | Record<string, never>;
    const player = this.createPlayer(client, auth);
    player.sessionId = client.sessionId;

    if (auth && "playerId" in auth) {
      this.playerDb.set(client.sessionId, auth as BasePlayerDbInfo);
    }

    this.players.set(client.sessionId, player);
    this.state.playerCount = this.clients.length;
    this.resetSessionTimer(client);
    this.sendPlayerState(client);
    this.onPlayerJoined(client);
  }

  onLeave(client: Client, code: number) {
    this.clearSessionTimer(client.sessionId);
    this.msgCounts.delete(client.sessionId);

    if (code !== CloseCode.CONSENTED) {
      this.allowReconnection(client, 30).catch(() => {
        this.removePlayer(client.sessionId);
      });
      return;
    }
    this.removePlayer(client.sessionId);
  }

  onDispose() {
    this.onGameDispose();
    this.sessionTimers.forEach((t) => clearTimeout(t));
    this.sessionTimers.clear();
  }

  async onReconnect(client: Client) {
    this.state.playerCount = this.clients.length;
    const player = this.players.get(client.sessionId);
    if (player) {
      this.resetSessionTimer(client);
      this.sendPlayerState(client);
    }
  }

  protected sendPlayerState(client: Client): void {
    const player = this.players.get(client.sessionId);
    if (!player) return;
    const dbInfo   = this.playerDb.get(client.sessionId);
    const currency = dbInfo?.currency ?? "USD";
    client.send("player_state", {
      balance:  player.balance,
      currency,
      username: player.username,
      ...this.getGameSpecificState(player),
    });
  }

  protected removePlayer(sessionId: string): void {
    this.players.delete(sessionId);
    this.playerDb.delete(sessionId);
    this.cleanupPlayerGameState(sessionId);
    this.state.playerCount = this.clients.length;
  }
}
