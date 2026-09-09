import "dotenv/config";
import { readFileSync } from "fs";
import { join } from "path";
const serverVersion: string = JSON.parse(readFileSync(join(__dirname, "../../package.json"), "utf-8")).version;
import { serve } from "@hono/node-server";
import { serveStatic } from "@hono/node-server/serve-static";
import { Hono } from "hono";
import type { HonoVars } from "./types";
import { logger } from "hono/logger";
import { cors } from "hono/cors";
import { gameRouter } from "./routes/game";
import { walletAviatorRouter } from "./routes/walletAviator";
import { walletRouletteRouter } from "./routes/walletRoulette";
import { walletBalloonRouter } from "./routes/walletBalloon";
import { operatorsRouter } from "./routes/operators";
import { operatorProxyRouter } from "./routes/operatorProxy";
import { rumblebetsRouter } from "./routes/rumblebets";
import { createGenericWalletRouter } from "./routes/walletGeneric";
import { gamePayoutRegistry } from "../games/payoutTables";

const app = new Hono<HonoVars>();

app.use("*", logger());
app.use("*", async (c, next) => {
  await next();
  c.header("Access-Control-Allow-Private-Network", "true");
});
app.use(
  "*",
  cors({
    origin: (process.env.ALLOWED_ORIGINS ?? "").split(",").map((s) => s.trim()),
    allowMethods: ["GET", "POST", "OPTIONS"],
    allowHeaders: ["Content-Type", "X-Api-Key", "X-Timestamp", "X-Signature", "Authorization"],
  })
);

app.route("/game", gameRouter);
app.route("/wallet", walletAviatorRouter);
app.route("/wallet/roulette", walletRouletteRouter);
app.route("/wallet/balloon", walletBalloonRouter);
app.route("/admin/operators", operatorsRouter);
app.route("/op", operatorProxyRouter);
app.route("/rumblebets", rumblebetsRouter);

// Generic tier: any game with a payoutTables.ts entry automatically gets its
// /wallet/<slug> route mounted here — one source of truth shared with
// GenericRoundRoom, so the two can't drift out of sync.
for (const slug of Object.keys(gamePayoutRegistry)) {
  app.route(`/wallet/${slug}`, createGenericWalletRouter(slug));
}

app.get("/health", (c) => c.json({ status: "ok", ts: new Date().toISOString() }));

if (process.env.GAME_ENV === "dev") {
  app.post("/dev/log", async (c) => {
    const { level, msg } = await c.req.json<{ level: string; msg: string }>();
    const prefix = level === "error" ? "[browser:ERR]" : level === "warn" ? "[browser:WRN]" : "[browser:LOG]";
    console.log(`${prefix} ${msg}`);
    return c.json({ ok: true });
  });
}
app.get("/version", (c) => c.json({ version: serverVersion ?? "unknown" }));

app.use("/*", serveStatic({ root: "./public" }));

app.onError((err, c) => {
  console.error("[API error]", err);
  return c.json({ error: "Internal server error" }, 500);
});

const port = parseInt(process.env.API_PORT ?? "3000", 10);

serve({ fetch: app.fetch, port }, () => {
  console.log(`[api] Hono API listening on http://localhost:${port}`);
});
