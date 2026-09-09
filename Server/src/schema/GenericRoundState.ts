import { Schema, type } from "@colyseus/schema";
import { BasePlayerSchema } from "./BaseSchema";
import type { HasPlayerCount } from "../rooms/BasePlatformRoom";

export class GenericRoundState extends Schema implements HasPlayerCount {
  @type("uint16") playerCount: number = 0;
}

export class GenericPlayerSchema extends BasePlayerSchema {}
