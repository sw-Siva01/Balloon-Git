import crypto from "crypto";
import { db } from "../../db";

interface CallbackRequest {
  operatorId: string;
  callbackUrl: string;
  hmacSecret: string;
  type: "debit" | "credit" | "rollback";
  betId: string;
  playerId: string;
  externalId: string;
  amount: number;
  roundId: string;
  gameSlug: string;
  operatorRef?: string;
}

interface CallbackResponse {
  success: boolean;
  operatorRef?: string;
  errorMessage?: string;
}

function buildSignature(secret: string, method: string, path: string, timestamp: string, bodyHash: string): string {
  const message = `${method}:${path}:${timestamp}:${bodyHash}`;
  return crypto.createHmac("sha256", secret).update(message).digest("hex");
}

interface GetBalanceRequest {
  operatorId:  string;
  callbackUrl: string;
  hmacSecret:  string;
  externalId:  string;
}

export async function callOperatorGetBalance(req: GetBalanceRequest): Promise<{ balance: number } | null> {
  const base = req.callbackUrl.endsWith("/") ? req.callbackUrl : req.callbackUrl + "/";
  const url  = new URL("get_balance", base);
  const body = JSON.stringify({ playerId: req.externalId });

  const timestamp = Date.now().toString();
  const bodyHash  = crypto.createHash("sha256").update(body).digest("hex");
  const signature = buildSignature(req.hmacSecret, "POST", url.pathname, timestamp, bodyHash);

  try {
    const response = await fetch(url.toString(), {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "X-Api-Key":    req.operatorId,
        "X-Timestamp":  timestamp,
        "X-Signature":  signature,
      },
      body,
      signal: AbortSignal.timeout(8_000),
    });

    if (!response.ok) {
      const errorText = await response.text().catch(() => "");
      console.error(`[wallet] get_balance → ${response.status}: ${errorText}`);
      return null;
    }

    const json = (await response.json()) as { balance?: number };
    if (typeof json.balance !== "number") {
      console.error("[wallet] get_balance response missing balance field");
      return null;
    }

    return { balance: json.balance };
  } catch (err) {
    console.error("[wallet] get_balance error:", err);
    return null;
  }
}

export async function callOperatorWallet(req: CallbackRequest): Promise<CallbackResponse> {
  const base = req.callbackUrl.endsWith("/") ? req.callbackUrl : req.callbackUrl + "/";
  const url  = new URL(req.type, base);
  const body = JSON.stringify({
    betId:       req.betId,
    playerId:    req.externalId,
    amount:      req.amount,
    roundId:     req.roundId,
    operatorRef: req.operatorRef,
  });

  const timestamp = Date.now().toString();
  const bodyHash  = crypto.createHash("sha256").update(body).digest("hex");
  const signature = buildSignature(req.hmacSecret, "POST", url.pathname, timestamp, bodyHash);

  let txId: string | undefined;
  const requestPayload = {
    url: url.toString(),
    body: JSON.parse(body),
    headers: { "X-Api-Key": req.operatorId, "X-Timestamp": timestamp },
  };

  try {
    const { data: tx } = await db.from("transactions").insert({
      operator_id:     req.operatorId,
      player_id:       req.playerId,
      bet_id:          req.betId,
      game_slug:       req.gameSlug,
      type:            req.type,
      amount:          req.amount,
      status:          "pending",
      request_payload: requestPayload,
    }).select("id").single();
    txId = (tx as any)?.id as string | undefined;

    const response = await fetch(url.toString(), {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "X-Api-Key":    req.operatorId,
        "X-Timestamp":  timestamp,
        "X-Signature":  signature,
      },
      body,
      signal: AbortSignal.timeout(10_000),
    });

    const responseText = await response.text();
    if (!response.ok) {
      if (txId) {
        await db.from("transactions").update({
          status: "failed",
          error_message: `Operator wallet returned ${response.status}`,
          response_payload: { status: response.status, body: responseText },
        }).eq("id", txId);
      }
      throw new Error(`Operator wallet returned ${response.status}: ${responseText}`);
    }

    const json = JSON.parse(responseText) as { operatorRef?: string };

    if (txId) {
      await db.from("transactions").update({
        status:            "completed",
        operator_ref:      json.operatorRef ?? null,
        completed_at:      new Date().toISOString(),
        response_payload:  { status: response.status, body: json },
      }).eq("id", txId);
    }

    return { success: true, operatorRef: json.operatorRef };
  } catch (err) {
    const msg = err instanceof Error ? err.message : String(err);
    if (txId) {
      await db.from("transactions").update({ status: "failed", error_message: msg }).eq("id", txId);
    }
    return { success: false, errorMessage: msg };
  }
}
