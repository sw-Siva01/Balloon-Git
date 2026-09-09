-- Mirrors aviator_bet_history exactly (same columns, same index, RLS enabled
-- with zero policies — service_role only, matching the existing convention).
-- One row per resolved spin (aggregate stake/payout across all bets in that
-- spin), not one row per individual bet — same "one round = one row"
-- decision already made for round_summary.
create table roulette_bet_history (
  id          uuid primary key default gen_random_uuid(),
  player_id   uuid not null,
  round_hash  text not null default '',
  bet_amount  numeric not null,
  win_amount  numeric not null default 0,
  created_at  timestamptz not null default now()
);

create index idx_roulette_bet_history_player_id on roulette_bet_history(player_id, created_at desc);

alter table roulette_bet_history enable row level security;
