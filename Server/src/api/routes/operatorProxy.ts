import { Hono } from "hono";

const router = new Hono();

const MOCK_OP_URL = process.env.MOCK_OP_URL ?? "http://127.0.0.1:4000";

router.post("/:slug/login", async (c) => {
  const { slug } = c.req.param();
  const body = await c.req.text();
  const res = await fetch(`${MOCK_OP_URL}/${slug}/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body,
  });
  const data = await res.text();
  return c.text(data, res.status as any, { "Content-Type": "application/json" });
});

router.post("/:slug/topup", async (c) => {
  const { slug } = c.req.param();
  const body     = await c.req.text();
  const adminKey = c.req.header("X-Admin-Key") ?? "";
  const res = await fetch(`${MOCK_OP_URL}/${slug}/topup`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      ...(adminKey ? { "X-Admin-Key": adminKey } : {}),
    },
    body,
  });
  const data = await res.text();
  return c.text(data, res.status as any, { "Content-Type": "application/json" });
});

router.get("/:slug/balance", async (c) => {
  const { slug }    = c.req.param();
  const playerId    = c.req.query("playerId") ?? "";
  const auth        = c.req.header("Authorization") ?? "";
  const res = await fetch(
    `${MOCK_OP_URL}/${slug}/balance?playerId=${encodeURIComponent(playerId)}`,
    { headers: auth ? { Authorization: auth } : {} },
  );
  const data = await res.text();
  return c.text(data, res.status as any, { "Content-Type": "application/json" });
});

router.post("/:slug/launch", async (c) => {
  const { slug } = c.req.param();
  const body = await c.req.text();
  const res = await fetch(`${MOCK_OP_URL}/${slug}/launch`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body,
  });
  const data = await res.text();
  return c.text(data, res.status as any, { "Content-Type": "application/json" });
});

export { router as operatorProxyRouter };
