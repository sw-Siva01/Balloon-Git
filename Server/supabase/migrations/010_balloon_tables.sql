-- Balloon's persistence: like roulette_rounds/aviator_bets, but collapsed to
-- ONE table since a balloon round only ever has a single stake (no per-round
-- multi-bet fan-out like Roulette, no shared multi-player round like Aviator
-- — each player's balloon inflates on its own independent timeline).
-- Deliberately NOT using the generic game_rounds/game_bets tier (009): like
-- Aviator, this has a live-ticking multiplier with an uncertain-timing
-- cash-out that doesn't resolve in one atomic step.

begin;

create table balloon_rounds (
  id                 uuid primary key default gen_random_uuid(),
  player_id          uuid not null references players(id) on delete restrict,
  operator_id        uuid not null references operators(id) on delete restrict,
  game_id            uuid references games(id),
  client_round_id    text not null,        -- idempotency key from Unity (BalloonColyseusManager's per-round GUID)
  bet_amount         numeric(12,2) not null check (bet_amount > 0),
  auto_target        numeric(10,2) not null default 0,
  client_seed        text not null default '',
  server_seed        text,                 -- populated on resolution (burst or cash-out)
  server_seed_hash   text,
  result_multiplier  numeric(10,2),        -- burst point (loss) or cash-out multiplier (win)
  payout             numeric(12,2),
  status             text not null default 'pending'
                      check (status in ('pending','placed','active','settling','cashed_out','burst','cancelled')),
  created_at         timestamptz not null default now(),
  resolved_at        timestamptz,
  unique (player_id, client_round_id)
);

create index idx_balloon_rounds_player_status on balloon_rounds(player_id, status);

create table balloon_bet_history (
  id          uuid primary key default gen_random_uuid(),
  player_id   uuid not null,
  bet_amount  numeric not null,
  win_amount  numeric not null default 0,
  created_at  timestamptz not null default now()
);

create index idx_balloon_bet_history_player_id on balloon_bet_history(player_id, created_at desc);

alter table balloon_rounds      enable row level security;
alter table balloon_bet_history enable row level security;

create policy "balloon_rounds_operator_read" on balloon_rounds
  for select using (operator_id = (current_setting('app.operator_id', true))::uuid);

insert into games (slug, name, game_url, is_active)
values ('balloon', 'Balloon', '<placeholder — set once the WebGL build is hosted>', true)
on conflict (slug) do nothing;

commit;
