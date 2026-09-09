import { Schema, ArraySchema, type } from "@colyseus/schema";
import { BasePlayerSchema } from "./BaseSchema";

export class RouletteRoomState extends Schema {
  @type("uint16") playerCount: number = 0;
  @type(["uint8"]) history: ArraySchema<number> = new ArraySchema<number>(); // last ~50 winning numbers
}

export class RoulettePlayerSchema extends BasePlayerSchema {
  // No extra fields — bet composition/results are message-driven, not synced state.
}
