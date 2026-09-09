create extension if not exists "pgcrypto";

create table operators (
  id           uuid        primary key default gen_random_uuid(),
  name         text        not null,
  api_key      text        not null unique,
  hmac_secret  text        not null,
  callback_url text        not null,
  is_active    boolean     not null default true,
  created_at   timestamptz not null default now(),
  updated_at   timestamptz not null default now()
);

create table players (
  id            uuid        primary key default gen_random_uuid(),
  operator_id   uuid        not null references operators(id) on delete restrict,
  external_id   text        not null,
  session_token text,
  created_at    timestamptz not null default now(),
  updated_at    timestamptz not null default now(),
  unique (operator_id, external_id)
);

create table rounds (
  id               uuid        primary key default gen_random_uuid(),
  round_number     bigserial   not null unique,
  server_seed      text        not null,
  server_seed_hash text        not null,
  nonce            integer     not null,
  crash_point      numeric(10,2),
  phase            text        not null default 'waiting'
                               check (phase in ('waiting', 'flying', 'crashed')),
  started_at       timestamptz,
  crashed_at       timestamptz,
  created_at       timestamptz not null default now()
);

create table bets (
  id                  uuid        primary key default gen_random_uuid(),
  round_id            uuid        not null references rounds(id) on delete restrict,
  player_id           uuid        not null references players(id) on delete restrict,
  operator_id         uuid        not null references operators(id) on delete restrict,
  ticket_slot         smallint    not null check (ticket_slot in (1, 2)),
  amount              numeric(12,2) not null check (amount > 0),
  client_seed         text        not null default '',
  auto_target         numeric(10,4) not null default 0,
  cash_out_multiplier numeric(10,4),
  payout              numeric(12,2),
  status              text        not null default 'placed'
                                  check (status in ('placed', 'active', 'cashed_out', 'lost', 'cancelled')),
  placed_at           timestamptz not null default now(),
  cashed_out_at       timestamptz,
  unique (round_id, player_id, ticket_slot)
);

create table transactions (
  id            uuid        primary key default gen_random_uuid(),
  operator_id   uuid        not null references operators(id) on delete restrict,
  player_id     uuid        not null references players(id) on delete restrict,
  bet_id        uuid        references bets(id) on delete restrict,
  type          text        not null check (type in ('debit', 'credit', 'rollback')),
  amount        numeric(12,2) not null check (amount > 0),
  operator_ref  text,
  status        text        not null default 'pending'
                            check (status in ('pending', 'completed', 'failed')),
  error_message text,
  created_at    timestamptz not null default now(),
  completed_at  timestamptz
);

create table audit_log (
  id            bigserial   primary key,
  event_type    text        not null,
  actor_type    text        not null,
  actor_id      text,
  resource_type text,
  resource_id   text,
  payload       jsonb,
  ip_address    inet,
  created_at    timestamptz not null default now()
);

create index idx_players_operator       on players(operator_id);
create index idx_bets_round             on bets(round_id);
create index idx_bets_player            on bets(player_id);
create index idx_bets_operator_status   on bets(operator_id, status);
create index idx_transactions_player    on transactions(player_id);
create index idx_transactions_bet       on transactions(bet_id);
create index idx_transactions_status    on transactions(operator_id, status, created_at desc);
create index idx_audit_log_event        on audit_log(event_type, created_at desc);
create index idx_audit_log_resource     on audit_log(resource_type, resource_id);

create or replace function set_updated_at()
returns trigger language plpgsql as $$
begin
  new.updated_at = now();
  return new;
end;
$$;

create trigger operators_updated_at
  before update on operators
  for each row execute function set_updated_at();

create trigger players_updated_at
  before update on players
  for each row execute function set_updated_at();

alter table operators    enable row level security;
alter table players      enable row level security;
alter table rounds       enable row level security;
alter table bets         enable row level security;
alter table transactions enable row level security;
alter table audit_log    enable row level security;

create policy "rounds_public_read" on rounds
  for select using (true);

create policy "bets_operator_read" on bets
  for select using (
    operator_id = (current_setting('app.operator_id', true))::uuid
  );

create policy "transactions_operator_read" on transactions
  for select using (
    operator_id = (current_setting('app.operator_id', true))::uuid
  );
