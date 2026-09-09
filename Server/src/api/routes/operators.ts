import crypto from "crypto";
import { Hono } from "hono";
import { zValidator } from "@hono/zod-validator";
import { z } from "zod";
import { db } from "../../db";
import { internalAuth } from "../middleware/internalAuth";
import type { HonoVars } from "../types";

export const operatorsRouter = new Hono<HonoVars>();

operatorsRouter.use("/*", internalAuth);

operatorsRouter.get("/", async (c) => {
  const { data, error } = await db
    .from("operators")
    .select("id, name, api_key, is_active, created_at")
    .order("created_at", { ascending: false });

  if (error) return c.json({ error: "Failed to fetch operators" }, 500);
  return c.json({ operators: data });
});

operatorsRouter.post(
  "/",
  zValidator("json", z.object({
    name: z.string().min(1),
  })),
  async (c) => {
    const { name } = c.req.valid("json");

    const api_key    = crypto.randomBytes(24).toString("hex");
    const hmac_secret = crypto.randomBytes(32).toString("hex");

    const { data, error } = await db
      .from("operators")
      .insert({ name, api_key, hmac_secret, is_active: true })
      .select("id, name, api_key, hmac_secret, is_active")
      .single();

    if (error) return c.json({ error: "Failed to create operator" }, 500);

    return c.json({
      operator: data,
      note: "Store hmac_secret securely — it will not be returned again.",
    }, 201);
  }
);

operatorsRouter.get("/:id/games", async (c) => {
  const { id } = c.req.param();
  const { data, error } = await db
    .from("operator_games")
    .select("id, game_id, callback_url, is_active, created_at, games(slug, name)")
    .eq("operator_id", id);

  if (error) return c.json({ error: "Failed to fetch operator games" }, 500);
  return c.json({ games: data });
});

operatorsRouter.post(
  "/:id/games",
  zValidator("json", z.object({
    gameSlug:     z.string().min(1),
    callback_url: z.string().url(),
  })),
  async (c) => {
    const { id } = c.req.param();
    const { gameSlug, callback_url } = c.req.valid("json");

    const { data: operator, error: operatorErr } = await db
      .from("operators")
      .select("id")
      .eq("id", id)
      .single();
    if (operatorErr || !operator) return c.json({ error: "Operator not found" }, 404);

    const { data: game, error: gameErr } = await db
      .from("games")
      .select("id")
      .eq("slug", gameSlug)
      .single();
    if (gameErr || !game) return c.json({ error: "Game not found" }, 404);

    const { data, error } = await db
      .from("operator_games")
      .insert({ operator_id: id, game_id: (game as any).id, callback_url })
      .select("id, game_id, callback_url, is_active")
      .single();

    if (error) {
      if ((error as any).code === "23505") {
        return c.json({ error: "Game already attached to this operator", code: "ALREADY_ATTACHED" }, 409);
      }
      return c.json({ error: "Failed to attach game", detail: error.message }, 500);
    }

    return c.json({ operatorGame: data }, 201);
  }
);

operatorsRouter.patch("/:id/games/:gameId/activate", async (c) => {
  const { id, gameId } = c.req.param();
  const { error } = await db
    .from("operator_games")
    .update({ is_active: true })
    .eq("operator_id", id)
    .eq("game_id", gameId);

  if (error) return c.json({ error: "Failed to activate" }, 500);
  return c.json({ success: true });
});

operatorsRouter.patch("/:id/games/:gameId/deactivate", async (c) => {
  const { id, gameId } = c.req.param();
  const { error } = await db
    .from("operator_games")
    .update({ is_active: false })
    .eq("operator_id", id)
    .eq("game_id", gameId);

  if (error) return c.json({ error: "Failed to deactivate" }, 500);
  return c.json({ success: true });
});

operatorsRouter.patch(
  "/:id/deactivate",
  async (c) => {
    const { id } = c.req.param();
    const { error } = await db
      .from("operators")
      .update({ is_active: false })
      .eq("id", id);

    if (error) return c.json({ error: "Failed to deactivate operator" }, 500);
    return c.json({ success: true });
  }
);

operatorsRouter.patch(
  "/:id/activate",
  async (c) => {
    const { id } = c.req.param();
    const { error } = await db
      .from("operators")
      .update({ is_active: true })
      .eq("id", id);

    if (error) return c.json({ error: "Failed to activate operator" }, 500);
    return c.json({ success: true });
  }
);

operatorsRouter.delete(
  "/:id",
  async (c) => {
    const { id } = c.req.param();
    const { error } = await db
      .from("operators")
      .delete()
      .eq("id", id);

    if (error) return c.json({ error: "Failed to delete operator" }, 500);
    return c.json({ success: true });
  }
);
