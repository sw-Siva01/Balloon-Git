import { Schema, type } from "@colyseus/schema";

export class BasePlayerSchema extends Schema {
  @type("string") sessionId: string = "";
  @type("string") username: string = "";
  @type("number") balance: number = 0;
}
