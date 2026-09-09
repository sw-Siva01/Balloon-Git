-- Adds an interim 'settling' bet status so /wallet/credit and /wallet/rollback
-- can atomically claim a bet (active -> settling) BEFORE calling the operator's
-- wallet webhook, instead of only checking-then-writing after the call. This
-- closes a narrow race where two concurrent settlement attempts for the same
-- bet could both pass the old precondition check and both reach the operator
-- (a real double-payout risk), not just corrupt the internal bets row.
--
-- 'settling' is intentionally included in the same partial unique index used
-- by 003_bets_idempotency.sql (widened from ('placed','active') to
-- ('placed','active','settling')) so a bet mid-claim is still protected
-- against a duplicate insert for the same (round_id, player_id, ticket_slot).
--
-- Same production caveat as 003: apply here only after 003 is live and the
-- known duplicate-bet groups have been reconciled.

begin;

alter table bets drop constraint bets_status_check;
alter table bets add constraint bets_status_check
  check (status in ('placed', 'active', 'settling', 'cashed_out', 'lost', 'cancelled'));

drop index if exists bets_round_player_ticket_active_uidx;
create unique index bets_round_player_ticket_active_uidx
  on bets (round_id, player_id, ticket_slot)
  where status in ('placed', 'active', 'settling');

commit;
