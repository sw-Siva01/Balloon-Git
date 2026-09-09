-- Generic tier for future games (see 2026-08-03 design discussion): most
-- casino-style games share one shape — stake something, resolve an outcome,
-- pay back 0 or more. Roulette already turned out to fit this shape by
-- accident (bet_type/selection/multiplier/won/payout); this generalizes it
-- with a game_slug column instead of one dedicated table family per game.
--
-- Not a replacement for roulette_rounds/roulette_bets or aviator_bets/
-- aviator_rounds — those stay as-is. This is only for NEW games going
-- forward, starting with whichever game gets onboarded next.
--
-- Aviator is deliberately excluded from ever using this tier: its live
-- ticking multiplier with an uncertain-timing cashout doesn't resolve in one
-- atomic step the way this schema assumes.

create table game_rounds (
  id                uuid primary key default gen_random_uuid(),
  game_slug         text not null,
  player_id         uuid not null references players(id) on delete restrict,
  operator_id       uuid not null references operators(id) on delete restrict,
  game_id           uuid references games(id),
  client_round_id   text not null,        -- idempotency key from the client, one per round/spin attempt
  -- nullable like roulette_rounds — not known until after debit succeeds,
  -- and not currently persisted by GenericRoundRoom/walletGeneric.ts at all
  -- (the seed/nonce used for resolveOutcome() lives room-side only, same as
  -- RouletteRoom's own nonce counter). Column kept for future provably-fair
  -- persistence if a game needs it.
  nonce             integer,
  server_seed       text,
  server_seed_hash  text,
  client_seed       text not null default '',
  outcome           jsonb,                 -- game-defined result payload (a number, a dice roll, revealed cells, ...)
  total_stake       numeric(12,2) not null check (total_stake > 0),
  total_payout      numeric(12,2),
  status            text not null default 'pending'
                     check (status in ('pending','active','settling','resolved','cancelled')),
  created_at        timestamptz not null default now(),
  resolved_at       timestamptz,
  unique (player_id, client_round_id)
);

create index idx_game_rounds_player_status on game_rounds(player_id, status);
create index idx_game_rounds_slug_created on game_rounds(game_slug, created_at desc);

create table game_bets (
  id           uuid primary key default gen_random_uuid(),
  round_id     uuid not null references game_rounds(id) on delete restrict,
  game_slug    text not null,
  player_id    uuid not null references players(id) on delete restrict,
  operator_id  uuid not null references operators(id) on delete restrict,
  game_id      uuid references games(id),
  bet_type     text not null,             -- vocabulary is game-defined, not constrained by a shared check
  selection    jsonb not null,
  amount       numeric(12,2) not null check (amount > 0),
  multiplier   numeric(10,4) not null,    -- server-side payout for this bet_type, snapshotted for audit
  won          boolean,
  payout       numeric(12,2) not null default 0,
  created_at   timestamptz not null default now()
);

create index idx_game_bets_round on game_bets(round_id);
create index idx_game_bets_slug_created on game_bets(game_slug, created_at desc);

create table game_bet_history (
  id          uuid primary key default gen_random_uuid(),
  game_slug   text not null,
  player_id   uuid not null,
  round_hash  text not null default '',
  bet_amount  numeric not null,
  win_amount  numeric not null default 0,
  created_at  timestamptz not null default now()
);

create index idx_game_bet_history_player on game_bet_history(player_id, created_at desc);

alter table game_rounds enable row level security;
alter table game_bets enable row level security;
alter table game_bet_history enable row level security;
