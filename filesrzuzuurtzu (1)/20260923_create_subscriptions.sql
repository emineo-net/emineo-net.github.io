-- =========================================================
-- Tabelle: public.subscriptions
-- Speichert Paddle-Abos pro Supabase-User
-- =========================================================

create table if not exists public.subscriptions (
  id                      uuid primary key default gen_random_uuid(),

  -- Verknüpfung zum Supabase-Auth-User
  user_id                 uuid not null references auth.users(id) on delete cascade,

  -- Paddle-IDs
  paddle_subscription_id  text unique,          -- eindeutige Sub-ID von Paddle (fehlt evtl. vor erstem Checkout)
  paddle_customer_id      text not null,        -- wird für das Customer Portal gebraucht

  -- Abo-Status: trialing | active | past_due | paused | canceled
  status                  text not null,

  -- Was wurde gekauft
  price_id                text,
  product_id              text,
  plan_name               text,                 -- optional, z.B. "Pro Monatlich" für Anzeige im UI

  -- Laufzeit
  current_period_start    timestamptz,
  current_period_end      timestamptz,
  cancel_at_period_end    boolean not null default false,
  canceled_at             timestamptz,
  trial_ends_at           timestamptz,

  -- Sandbox-Kennzeichen (deine Anforderung)
  is_sandbox              boolean not null default false,

  -- Rohes Event zur Fehlersuche / Nachvollziehbarkeit
  raw_event                jsonb,

  created_at              timestamptz not null default now(),
  updated_at              timestamptz not null default now()
);

create index if not exists idx_subscriptions_user_id on public.subscriptions (user_id);
create index if not exists idx_subscriptions_customer_id on public.subscriptions (paddle_customer_id);

-- updated_at automatisch pflegen
create or replace function public.set_updated_at()
returns trigger as $$
begin
  new.updated_at = now();
  return new;
end;
$$ language plpgsql;

drop trigger if exists trg_subscriptions_updated_at on public.subscriptions;
create trigger trg_subscriptions_updated_at
  before update on public.subscriptions
  for each row execute function public.set_updated_at();

-- Row Level Security: User darf nur sein eigenes Abo sehen.
-- Schreiben passiert ausschließlich über die Edge Function (service_role, bypasst RLS).
alter table public.subscriptions enable row level security;

create policy "Users can view own subscription"
  on public.subscriptions for select
  using (auth.uid() = user_id);
