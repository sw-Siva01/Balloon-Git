-- Renamed for consistency with roulette_bets/roulette_rounds — bets/rounds
-- predate Roulette and never got an Aviator-specific prefix. Postgres tracks
-- FKs/views/policies by OID, not name, so this doesn't require touching
-- transactions.bet_id, round_summary.round_ref, or any RLS policy.
alter table if exists public.bets rename to aviator_bets;
alter table if exists public.rounds rename to aviator_rounds;

notify pgrst, 'reload schema';
