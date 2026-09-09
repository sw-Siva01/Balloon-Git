-- Partial unique index to prevent duplicate bet rows for the same
-- (round_id, player_id, ticket_slot) while a bet is still non-terminal.
--
-- Scoped to non-terminal statuses only (not a full unique index) because
-- cancelling a bet and re-betting the same slot within the same round is a
-- real, supported flow (see AviatorRoom.handleCancelBet/handlePlaceBet) —
-- a full index would block that legitimate rebet.
--
-- IMPORTANT: do not apply this to production until the known duplicate rows
-- for (round_id, player_id, ticket_slot) have been reviewed and reconciled
-- (see Server/docs — duplicate-bet investigation). This index will fail to
-- create if any group currently has two rows simultaneously in
-- ('placed','active'). The app-level idempotency guard in
-- POST /wallet/debit prevents *new* duplicates from forming independent of
-- whether this index has been applied yet.

begin;

create unique index if not exists bets_round_player_ticket_active_uidx
  on bets (round_id, player_id, ticket_slot)
  where status in ('placed', 'active');

commit;
