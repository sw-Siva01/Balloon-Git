import { Schema, ArraySchema, type } from "@colyseus/schema";
import { BasePlayerSchema } from "./BaseSchema";

// Unlike Aviator (one shared "flying" phase for the whole room) Balloon is
// per-player: each player pumps their own balloon on their own timeline, so
// the round/phase state lives on the player, not on the room.
export class BalloonPlayerSchema extends BasePlayerSchema {
  @type("string")  phase: string = "idle";     // idle | inflating | resolved
  @type("float32") betAmount: number = 0;
  @type("float32") multiplier: number = 1;     // current multiplier while inflating
  @type("boolean") holding: boolean = false;   // is the player currently holding the pump button
  @type("float32") autoTarget: number = 0;     // 0 = no auto cash-out
}

export class BalloonRoomState extends Schema {
  @type("uint16")            playerCount: number = 0;
  @type(["float32"])         history: ArraySchema<number> = new ArraySchema<number>(); // last N resolved multipliers (burst or cash-out)
}
