import { Room } from "colyseus";
import { RouletteRoom } from "../rooms/RouletteRoom";
import { BalloonRoom } from "../rooms/BalloonRoom";
import { GenericRoundRoom } from "../rooms/GenericRoundRoom";

export interface GameRegistration {
  slug: string;       // games.slug in DB
  roomName: string;   // Colyseus define() key
  RoomClass: new (...args: any[]) => Room;
  options?: Record<string, unknown>; // passed as defaultOptions to gameServer.define(), merged into onCreate(options)
}

export const gameRegistry: GameRegistration[] = [
  { slug: "roulette", roomName: "roulette", RoomClass: RouletteRoom },
  { slug: "balloon", roomName: "balloon", RoomClass: BalloonRoom },
  // New "simple" games (stake -> resolve -> payout) go through
  // GenericRoundRoom instead of a bespoke Room class — add a payout config
  // entry in games/payoutTables.ts, then register it here as one line:
  //
  // { slug: "dice", roomName: "dice", RoomClass: GenericRoundRoom, options: { gameSlug: "dice" } },
];
