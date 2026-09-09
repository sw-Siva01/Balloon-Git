import { config } from "dotenv";
config();

import { Server }             from "colyseus";
import { WebSocketTransport } from "@colyseus/ws-transport";
import { gameRegistry }       from "./games/registry";

const PORT   = Number(process.env.PORT ?? 2567);
const isProd = process.env.NODE_ENV === "production";

const ALLOWED_ORIGINS = (process.env.ALLOWED_ORIGINS ?? "http://localhost,http://localhost:3000")
  .split(",").map(o => o.trim());

const transport = new WebSocketTransport({
  verifyClient: (info, next) => {
    if (!isProd) return next(true);
    const allowed = ALLOWED_ORIGINS.includes(info.origin);
    if (!allowed) console.warn(`[ws] rejected origin: ${info.origin}`);
    next(allowed);
  }
});

const gameServer = new Server({ transport });

if (!isProd) {
  import("@colyseus/playground").then(({ playground }) => {
    (gameServer as any).app?.use("/playground", playground);
  }).catch(() => {});
}

for (const { roomName, RoomClass, options } of gameRegistry) {
  gameServer.define(roomName, RoomClass, options);
}

gameServer.listen(PORT).then(() => {
  transport.server?.prependListener("request", (_req: any, res: any) => {
    res.setHeader("Access-Control-Allow-Private-Network", "true");
  });

  console.log(`✅ Aviator server listening on ws://localhost:${PORT}`);
  if (!isProd) console.log("⚠️  Dev mode — origin check & playground disabled for prod");
}).catch((err) => {
  console.error("Failed to start server:", err);
  process.exit(1);
});
