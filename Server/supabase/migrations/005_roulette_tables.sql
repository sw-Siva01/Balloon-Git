-- Roulette's persistence: its own round/bet tables rather than forcing it
-- through Aviator-shaped `bets`/`rounds` (ticket_slot check(1,2), round_id
-- FK'd to a crash-game rounds table). See Server/docs plan for the full
-- rationale.
--
-- Also fixes a hard blocker found while designing this: transactions.bet_id
-- references bets(id) on delete restrict — passing a roulette_rounds.id into
-- callOperatorWallet's transactions insert would throw an FK violation
-- immediately. Dropping that FK is zero behavior change for Aviator (it never
-- violates it); bet_id becomes a plain settlement-subject id whose meaning
-- depends on game_slug.

begin;

alter table transactions drop constraint if exists transactions_bet_id_fkey;
alter table transactions add column if not exists game_slug text;

create table roulette_rounds (
  id                uuid primary key default gen_random_uuid(),
  player_id         uuid not null references players(id) on delete restrict,
  operator_id       uuid not null references operators(id) on delete restrict,
  game_id           uuid references games(id),
  client_spin_id    text not null,        -- idempotency key from Unity (a retried spin must not double-debit)
  -- nonce/server_seed/server_seed_hash are only known once the winning number
  -- is computed, which happens after the debit succeeds (no live betting
  -- window to commit a hash into ahead of time, unlike Aviator) — nullable,
  -- populated when the round resolves.
  nonce             integer,
  server_seed       text,
  server_seed_hash  text,
  client_seed       text not null default '',
  winning_number    smallint check (winning_number between 1 and 12),
  total_stake       numeric(12,2) not null check (total_stake > 0),
  total_payout      numeric(12,2),
  status            text not null default 'pending'
                     check (status in ('pending','active','settling','resolved','cancelled')),
  created_at        timestamptz not null default now(),
  resolved_at       timestamptz,
  unique (player_id, client_spin_id)
);

create table roulette_bets (
  id           uuid primary key default gen_random_uuid(),
  round_id     uuid not null references roulette_rounds(id) on delete restrict,
  player_id    uuid not null references players(id) on delete restrict,
  operator_id  uuid not null references operators(id) on delete restrict,
  game_id      uuid references games(id),
  bet_type     text not null check (bet_type in ('num','im_two','im_four','im_special')),
  selection    jsonb not null,   -- {"numbers":[3]} | {"numbers":[2,4]} | {"numbers":[1,2,4,5]} | {"special":"red"}
  amount       numeric(12,2) not null check (amount > 0),
  multiplier   numeric(10,4) not null,  -- server-side payout for this bet_type, snapshotted for audit
  won          boolean,
  payout       numeric(12,2) not null default 0,
  created_at   timestamptz not null default now()
);

create index idx_roulette_bets_round on roulette_bets(round_id);
create index idx_roulette_rounds_player_status on roulette_rounds(player_id, status);

alter table roulette_rounds enable row level security;
alter table roulette_bets   enable row level security;

create policy "roulette_rounds_operator_read" on roulette_rounds
  for select using (operator_id = (current_setting('app.operator_id', true))::uuid);
create policy "roulette_bets_operator_read" on roulette_bets
  for select using (operator_id = (current_setting('app.operator_id', true))::uuid);

insert into games (slug, name, game_url, is_active)
values ('roulette', 'MiniRoulette', '<placeholder — set once the WebGL build is hosted>', true)
on conflict (slug) do nothing;

commit;
