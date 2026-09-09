/* import { Schema, MapSchema, ArraySchema, type } from "@colyseus/schema";
import { BasePlayerSchema } from "./BaseSchema";

export class BetViewSchema extends Schema {
  @type("string")   username: string = "";
  @type("float32")  betAmount: number = 0;
  @type("boolean")  cashedOut: boolean = false;
  @type("float32")  cashOutMultiplier: number = 0;
  @type("uint8")    ticketSlot: number = 1;
}

export class TicketSchema extends Schema {
  @type("boolean") hasTicket: boolean = false;
  @type("number") betAmount: number = 0;
  @type("boolean") cashedOut: boolean = false;
  @type("number") cashOutMultiplier: number = 0;
  @type("number") autoTarget: number = 0;
  @type("number") stopOnLoss: number = 0;
  @type("number") stopOnWin: number = 0;
}

export class PlayerSchema extends BasePlayerSchema {
  @type(TicketSchema) ticket1: TicketSchema = new TicketSchema();
  @type(TicketSchema) ticket2: TicketSchema = new TicketSchema();
}

export class AviatorRoomState extends Schema {
  @type("string")            phase: string = "waiting";
  @type("float32")           crashMultiplier: number = 0;
  @type("string")            roundHash: string = "";
  @type("string")            revealedSeed: string = "";
  @type("uint8")             houseEdgeDivisor: number = 0;
  @type({ map: BetViewSchema }) liveBets: MapSchema<BetViewSchema> = new MapSchema<BetViewSchema>();
  @type(["float32"])         history: ArraySchema<number> = new ArraySchema<number>();
  @type("uint16")            playerCount: number = 0;
}
 */