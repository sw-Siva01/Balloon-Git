import { createClient, type SupabaseClient } from "@supabase/supabase-js";

const g = globalThis as unknown as { _supa?: SupabaseClient<any>; };

export const db: SupabaseClient<any> =
  g._supa ??
  createClient<any>( process.env.SUPABASE_URL!, process.env.SUPABASE_SERVICE_ROLE_KEY!, {
      auth: { persistSession: false },
    });

if (process.env.NODE_ENV !== "production") g._supa = db;