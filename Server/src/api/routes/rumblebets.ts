import crypto from "crypto";
import jwt from "jsonwebtoken";
import { Hono } from "hono";
import { createClient, type SupabaseClient } from "@supabase/supabase-js";
import { db } from "../../db";
import { issuePlayerToken } from "../services/playerToken";
import { createSessionCode } from "./game";

const router = new Hono();

const HARDCODED_OTP = "1234";
const TABLE        = "rumblebets_users";

const LOGIN_TOKEN_SECRET = process.env.JWT_SECRET ?? "dev-insecure-secret-do-not-use-in-prod";
const LOGIN_TOKEN_PURPOSE = "rumblebets_login";

function issueLoginToken(mobile: string): string {
  return jwt.sign({ mobile, purpose: LOGIN_TOKEN_PURPOSE }, LOGIN_TOKEN_SECRET, { expiresIn: "24h" });
}

function verifyLoginToken(token: string): string {
  const payload = jwt.verify(token, LOGIN_TOKEN_SECRET) as { mobile: string; purpose: string };
  if (payload.purpose !== LOGIN_TOKEN_PURPOSE) throw new Error("Wrong token type");
  return payload.mobile;
}

function getRbDb(): SupabaseClient {
  return createClient(
    process.env.RUMBLEBETS_DB_URL!,
    process.env.RUMBLEBETS_DB_KEY!,
    { auth: { persistSession: false } },
  );
}

function buildWallet(balance: number) {
  return {
    TotalDeposit:        balance,
    CashDepositVal:      balance,
    TotalDepositByPlayer: balance,
  };
}

async function verifyCallbackHmac(c: any): Promise<{ ok: boolean; body: any }> {
  const timestamp = c.req.header("X-Timestamp") ?? "";
  const signature = c.req.header("X-Signature") ?? "";
  const rawBody   = await c.req.text();
  if (!timestamp || !signature) return { ok: false, body: null };

  const tsNum = Number(timestamp);
  const tsMs  = tsNum < 1e10 ? tsNum * 1000 : tsNum;
  if (Math.abs(Date.now() - tsMs) > 30_000) return { ok: false, body: null };

  const bodyHash = crypto.createHash("sha256").update(rawBody).digest("hex");
  const method   = c.req.method;
  const path     = new URL(c.req.url).pathname;
  const secret   = process.env.RUMBLEBETS_HMAC_SECRET ?? "";
  const expected = crypto
    .createHmac("sha256", secret)
    .update(`${method}:${path}:${timestamp}:${bodyHash}`)
    .digest("hex");

  try {
    const ok = crypto.timingSafeEqual(Buffer.from(signature, "hex"), Buffer.from(expected, "hex"));
    return { ok, body: ok ? JSON.parse(rawBody) : null };
  } catch {
    return { ok: false, body: null };
  }
}

router.post("/register", async (c) => {
  const { mobile, name } = await c.req.json<{ mobile: string; name: string }>();
  if (!mobile || !name) return c.json({ Code: 400, message: "mobile and name required" }, 400);

  const rbDb = getRbDb();
  const { error } = await rbDb.from(TABLE).upsert(
    { username: name, mobile, balance: 1000 },
    { onConflict: "mobile", ignoreDuplicates: true },
  );
  if (error) {
    console.error("[rumblebets/register] upsert error:", JSON.stringify(error));
    return c.json({ Code: 500, message: "Failed to create user" }, 500);
  }

  await rbDb.from("otp_details").upsert(
    { identifier: mobile, username: name, otp: 1234, last_attempt: new Date().toISOString(), reattempt_count: 0 },
    { onConflict: "identifier" },
  );

  return c.json({ Code: 200, message: "OTP sent" });
});

router.post("/register/verify", async (c) => {
  const { mobile, otp } = await c.req.json<{ mobile: string; otp: string }>();
  if (!mobile || !otp) return c.json({ code: 400, message: "mobile and otp required" }, 400);
  if (String(otp) !== HARDCODED_OTP) return c.json({ code: 401, message: "Invalid OTP" }, 401);

  const rbDb = getRbDb();
  const { data: user } = await rbDb
    .from(TABLE)
    .select("id, username, mobile, balance")
    .eq("mobile", mobile)
    .single();
  if (!user) return c.json({ code: 404, message: "User not found" }, 404);

  return c.json({
    code:      200,
    playerID:  mobile,
    username:  (user as any).username || mobile,
    wallet:    buildWallet(Number((user as any).balance)),
    loginToken: issueLoginToken(mobile),
  });
});

router.post("/login", async (c) => {
  const { mobile } = await c.req.json<{ mobile: string }>();
  if (!mobile) return c.json({ Code: 400, message: "mobile required" }, 400);

  const rbDb = getRbDb();
  const { data: user } = await rbDb.from(TABLE).select("id").eq("mobile", mobile).single();
  if (!user) return c.json({ Code: 404, message: "User not found" }, 404);

  await rbDb.from("otp_details").upsert(
    { identifier: mobile, otp: 1234, last_attempt: new Date().toISOString() },
    { onConflict: "identifier" },
  );

  return c.json({ Code: 200, message: "OTP sent" });
});

router.post("/login/verify", async (c) => {
  const { mobile, otp } = await c.req.json<{ mobile: string; otp: string }>();
  if (!mobile || !otp) return c.json({ code: 400, message: "mobile and otp required" }, 400);
  if (String(otp) !== HARDCODED_OTP) return c.json({ code: 401, message: "Invalid OTP" }, 401);

  const rbDb = getRbDb();
  const { data: user } = await rbDb
    .from(TABLE)
    .select("id, username, mobile, balance")
    .eq("mobile", mobile)
    .single();
  if (!user) return c.json({ code: 404, message: "User not found" }, 404);

  return c.json({
    code:      200,
    playerID:  mobile,
    username:  (user as any).username || mobile,
    wallet:    buildWallet(Number((user as any).balance)),
    loginToken: issueLoginToken(mobile),
  });
});

router.get("/balance", async (c) => {
  const userId = c.req.query("userId") ?? "";
  if (!userId) return c.json({ error: "userId required" }, 400);

  const rbDb = getRbDb();
  const { data: user } = await rbDb.from(TABLE).select("balance").eq("mobile", userId).single();
  if (!user) return c.json({ error: "User not found" }, 404);

  const balance = Number((user as any).balance);
  return c.json({ balance, wallet: buildWallet(balance) });
});

router.post("/wallet/add-money", async (c) => {
  const { user_id, amount } = await c.req.json<{ user_id: string; amount: number }>();
  if (!user_id || !amount || amount <= 0) return c.json({ error: "user_id and positive amount required" }, 400);

  const rbDb = getRbDb();
  const { data: user } = await rbDb.from(TABLE).select("id, balance").eq("mobile", user_id).single();
  if (!user) return c.json({ error: "User not found" }, 404);

  const oldBalance = parseFloat(Number((user as any).balance).toFixed(2));
  const newBalance = parseFloat((oldBalance + amount).toFixed(2));

  await rbDb.from(TABLE).update({ balance: newBalance, updated_at: new Date().toISOString() }).eq("mobile", user_id);

  await rbDb.from("transaction_details").insert({
    player_id:   (user as any).id,
    amount,
    old_balance: oldBalance,
    new_balance: newBalance,
    mode:        "Deposit",
    status:      "completed",
  });

  await rbDb.from("wallet_ledger").insert({
    user_id:        (user as any).id,
    changeset:      { type: "deposit", amount },
    totaldeposit:   newBalance,
    cashdepositval: newBalance,
  });

  return c.json({ balance: newBalance, wallet: buildWallet(newBalance) });
});

router.get("/game/url", async (c) => {
  const game = c.req.query("game") || "aviator";
  const { data: gameRow } = await db
    .from("games")
    .select("game_url")
    .eq("slug", game)
    .eq("is_active", true)
    .single();

  const url = process.env.GAME_URL || (gameRow as any)?.game_url || "";
  return c.json({ url });
});

router.post("/game/launch", async (c) => {
  const { token: loginToken, game = "aviator", currency = "INR" } =
    await c.req.json<{ token: string; game?: string; currency?: string }>();
  if (!loginToken) return c.json({ error: "token required" }, 400);
  if (!/^[A-Z]{3}$/.test(currency)) return c.json({ error: "Invalid currency" }, 400);

  let user_id: string;
  try {
    user_id = verifyLoginToken(loginToken);
  } catch {
    return c.json({ error: "Invalid or expired login token" }, 401);
  }

  const rbDb = getRbDb();
  const { data: rbUser } = await rbDb
    .from(TABLE)
    .select("id, username, mobile")
    .eq("mobile", user_id)
    .single();
  if (!rbUser) return c.json({ error: "User not found" }, 404);

  const displayName = (rbUser as any).username || user_id;

  const { data: operator } = await db
    .from("operators")
    .select("id, name, currency, hmac_secret")
    .eq("name", "op_rumblebets")
    .eq("is_active", true)
    .single();
  if (!operator) return c.json({ error: "Operator not configured" }, 500);

  const { data: gameRow } = await db
    .from("games")
    .select("id, game_url")
    .eq("slug", game)
    .eq("is_active", true)
    .single();
  if (!gameRow) return c.json({ error: "Game not found" }, 404);

  const { data: player } = await db
    .from("players")
    .upsert(
      { operator_id: (operator as any).id, external_id: user_id, display_name: displayName },
      { onConflict: "operator_id,external_id", ignoreDuplicates: false },
    )
    .select("id")
    .single();
  if (!player) return c.json({ error: "Failed to create player session" }, 500);

  const token = await issuePlayerToken({
    sub:         (player as any).id as string,
    operatorId:  (operator as any).id as string,
    externalId:  user_id,
    displayName: displayName,
    currency:    ((operator as any).currency as string | undefined) ?? currency,
    gameId:      (gameRow as any).id as string,
    gameSlug:    game,
  });

  const resolvedCurrency = ((operator as any).currency as string | undefined) ?? currency;
  const operatorName     = (operator as any).name as string;
  const sessionCode = createSessionCode(token, {
    game,
    user:     user_id,
    currency: resolvedCurrency,
    operator: operatorName,
    lang:     "en",
  });
  const gameUrl = process.env.GAME_URL || ((gameRow as any).game_url as string);
  const query   = new URLSearchParams({
    game,
    user:     user_id,
    token:    sessionCode,
    currency: resolvedCurrency,
    operator: operatorName,
    lang:     "en",
  });
  if (process.env.GAME_ENV === "dev") query.set("env", "dev");

  await rbDb.from(TABLE).update({ session_token: sessionCode }).eq("mobile", user_id);

  return c.json({
    launch_url: `${gameUrl}?${query.toString()}`,
    user_token: sessionCode,
  });
});

router.post("/callback/get_balance", async (c) => {
  const { ok, body } = await verifyCallbackHmac(c);
  if (!ok) return c.json({ error: "Unauthorized" }, 401);

  const { playerId } = body as { playerId: string };
  const rbDb = getRbDb();
  const { data: user } = await rbDb.from(TABLE).select("balance").eq("mobile", playerId).single();
  if (!user) return c.json({ error: "Player not found" }, 404);

  return c.json({ balance: Number((user as any).balance) });
});

router.post("/callback/debit", async (c) => {
  const { ok, body } = await verifyCallbackHmac(c);
  if (!ok) return c.json({ error: "Unauthorized" }, 401);

  const { playerId, amount, betId } = body as { playerId: string; amount: number; betId: string };
  const rbDb = getRbDb();

  const { data, error } = await rbDb.rpc("atomic_wallet_debit", { p_username: playerId, p_amount: amount });
  if (error) {
    if (error.message?.includes("insufficient_balance")) return c.json({ error: "Insufficient balance" }, 402);
    if (error.message?.includes("user_not_found"))       return c.json({ error: "Player not found" }, 404);
    console.error("[rumblebets/debit] rpc error:", error.message);
    return c.json({ error: "Wallet error" }, 500);
  }

  const row        = data as { id: string; balance: number };
  const newBalance = Number(row.balance);
  await rbDb.from("transaction_details").insert({
    player_id: row.id, amount, old_balance: newBalance + amount, new_balance: newBalance,
    mode: "Withdraw", status: "completed",
  });

  return c.json({ success: true, balance: newBalance, operatorRef: betId });
});

router.post("/callback/credit", async (c) => {
  const { ok, body } = await verifyCallbackHmac(c);
  if (!ok) return c.json({ error: "Unauthorized" }, 401);

  const { playerId, amount, betId } = body as { playerId: string; amount: number; betId: string };
  const rbDb = getRbDb();

  const { data, error } = await rbDb.rpc("atomic_wallet_credit", { p_username: playerId, p_amount: amount });
  if (error) {
    if (error.message?.includes("user_not_found")) return c.json({ error: "Player not found" }, 404);
    console.error("[rumblebets/credit] rpc error:", error.message);
    return c.json({ error: "Wallet error" }, 500);
  }

  const row        = data as { id: string; balance: number };
  const newBalance = Number(row.balance);
  await rbDb.from("transaction_details").insert({
    player_id: row.id, amount, old_balance: newBalance - amount, new_balance: newBalance,
    mode: "Deposit", status: "completed",
  });

  return c.json({ success: true, balance: newBalance, operatorRef: betId });
});

router.post("/callback/rollback", async (c) => {
  const { ok, body } = await verifyCallbackHmac(c);
  if (!ok) return c.json({ error: "Unauthorized" }, 401);

  const { playerId, amount, betId } = body as { playerId: string; amount: number; betId: string };
  const rbDb = getRbDb();

  const { data, error } = await rbDb.rpc("atomic_wallet_credit", { p_username: playerId, p_amount: amount });
  if (error) {
    if (error.message?.includes("user_not_found")) return c.json({ error: "Player not found" }, 404);
    console.error("[rumblebets/rollback] rpc error:", error.message);
    return c.json({ error: "Wallet error" }, 500);
  }

  const row        = data as { id: string; balance: number };
  const newBalance = Number(row.balance);
  await rbDb.from("transaction_details").insert({
    player_id: row.id, amount, old_balance: newBalance - amount, new_balance: newBalance,
    mode: "Rollback", status: "completed",
  });

  return c.json({ success: true, balance: newBalance, operatorRef: betId });
});

export { router as rumblebetsRouter };
