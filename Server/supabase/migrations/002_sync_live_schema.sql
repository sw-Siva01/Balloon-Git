begin;

create extension if not exists "uuid-ossp";

alter table operators drop column if exists callback_url;
alter table operators alter column api_key drop not null;
alter table operators add column if not exists currency text not null default 'USD';
alter table operators add column if not exists allowed_domains text[] not null default '{}';
alter table operators add column if not exists password text;
alter table operators add column if not exists email text;

alter table players drop column if exists session_token;
alter table players add column if not exists last_active timestamptz;
alter table players add column if not exists is_bot boolean not null default false;
alter table players add column if not exists display_name text;

create table if not exists games (
  id         uuid primary key default gen_random_uuid(),
  slug       text not null unique,
  name       text not null,
  game_url   text not null,
  is_active  boolean not null default true,
  created_at timestamptz not null default now()
);
alter table games enable row level security;

create table if not exists operator_games (
  id           uuid primary key default gen_random_uuid(),
  operator_id  uuid not null references operators(id) on delete cascade,
  game_id      uuid not null references games(id) on delete cascade,
  callback_url text not null,
  is_active    boolean not null default true,
  created_at   timestamptz not null default now(),
  unique (operator_id, game_id)
);
alter table operator_games enable row level security;

create or replace function operator_games_force_active_on_insert()
returns trigger language plpgsql as $$
begin
  new.is_active := true;
  return new;
end;
$$;

drop trigger if exists trg_operator_games_active on operator_games;
create trigger trg_operator_games_active
  before insert on operator_games
  for each row execute function operator_games_force_active_on_insert();

alter table rounds drop column if exists round_number;
alter table rounds add column if not exists game_id uuid references games(id);
create index if not exists rounds_nonce_idx  on rounds(nonce);
create index if not exists idx_rounds_game_id on rounds(game_id);

alter table bets drop constraint if exists bets_round_id_player_id_ticket_slot_key;
alter table bets add column if not exists game_id uuid references games(id);
create index if not exists idx_bets_game_id on bets(game_id);

create or replace function update_player_last_active()
returns trigger language plpgsql as $$
begin
  update players set last_active = now() where id = NEW.player_id;
  return NEW;
end;
$$;

drop trigger if exists trg_bet_last_active on bets;
create trigger trg_bet_last_active
  after insert or update on bets
  for each row execute function update_player_last_active();

create or replace view player_stats as
  select
    p.id,
    p.external_id,
    p.operator_id,
    p.last_active,
    p.is_bot,
    count(b.id)  filter (where b.status in ('cashed_out', 'lost')) as games_played,
    count(b.id)  filter (where b.status = 'cashed_out')            as games_won,
    coalesce(sum(b.amount), 0)                                     as total_bets,
    coalesce(sum(b.payout), 0) - coalesce(sum(b.amount), 0)        as net_winnings,
    coalesce(sum(b.amount), 0) - coalesce(sum(b.payout), 0)        as company_revenue
  from players p
  left join bets b on b.player_id = p.id
  group by p.id;

create or replace view round_stats as
  select
    r.id as round_id,
    r.game_id,
    r.crash_point,
    r.phase,
    r.started_at,
    r.crashed_at,
    r.nonce,
    count(b.id)                                                    as player_count,
    coalesce(sum(b.amount), 0)                                     as total_bet,
    coalesce(sum(b.payout), 0)                                     as user_winnings,
    coalesce(sum(b.amount), 0) - coalesce(sum(b.payout), 0)        as revenue
  from rounds r
  left join bets b on b.round_id = r.id
  group by r.id;

create table if not exists profiles (
  id          uuid primary key references auth.users(id) on delete cascade,
  role        text not null check (role in ('admin', 'operator')),
  operator_id uuid references operators(id) on delete set null,
  created_at  timestamptz not null default now()
);
alter table profiles enable row level security;

do $$
begin
  if not exists (
    select 1 from pg_policies where schemaname = 'public' and tablename = 'profiles' and policyname = 'profiles_self_read'
  ) then
    create policy "profiles_self_read" on profiles
      for select using (id = auth.uid());
  end if;
end $$;

create or replace function get_my_operator_id()
returns uuid
language sql stable security definer as $$
  select operator_id from profiles
  where id = auth.uid() and role = 'operator';
$$;

create or replace function is_admin()
returns boolean
language sql stable security definer as $$
  select exists (
    select 1 from profiles
    where id = auth.uid() and role = 'admin'
  );
$$;

commit;
