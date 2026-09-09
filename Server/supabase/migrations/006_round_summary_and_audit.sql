-- Two independent, additive audit/reporting improvements, bundled since
-- neither touches money-decision logic (see Server/docs plan §A2):
--
-- 1. round_summary: one row per finished round/spin, any game family, so
--    "how much has this player won across every game" is a single-table
--    query again (the old Lootrix schema had this via a generic
--    gameuserbetdetails table; our per-game-family tables lost it).
--
-- 2. transactions.request_payload/response_payload: full request/response
--    JSON per operator wallet call, so a payout dispute can be replayed
--    exactly (the old schema logged this; ours only logs status/operator_ref).

begin;

create table round_summary (
  id           uuid primary key default gen_random_uuid(),
  player_id    uuid not null references players(id) on delete restrict,
  operator_id  uuid not null references operators(id) on delete restrict,
  game_id      uuid references games(id),
  game_slug    text not null,
  round_ref    uuid not null,        -- the game-specific round/bet id (rounds.id for Aviator, roulette_rounds.id for Roulette)
  stake        numeric(12,2) not null,
  payout       numeric(12,2) not null default 0,
  outcome      text not null check (outcome in ('won','lost','cancelled')),
  created_at   timestamptz not null default now()
);

create index idx_round_summary_player on round_summary(player_id, created_at desc);
create index idx_round_summary_game   on round_summary(game_slug, created_at desc);

alter table round_summary enable row level security;
create policy "round_summary_operator_read" on round_summary
  for select using (operator_id = (current_setting('app.operator_id', true))::uuid);

alter table transactions add column if not exists request_payload  jsonb;
alter table transactions add column if not exists response_payload jsonb;

commit;
