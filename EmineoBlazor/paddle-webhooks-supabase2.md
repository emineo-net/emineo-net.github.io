
//******** benötigte Secrets ************************************
# Aus deinem Keygen: privateKey.pem (NUR der Private Key!)
supabase secrets set LICENSE_PRIVATE_KEY="$(Get-Content -Raw privateKey.pem)"

# Aus dem Paddle-Dashboard → Developer Tools → Notifications → Endpoint anlegen
supabase secrets set PADDLE_WEBHOOK_SECRET="pdl_ntfset_..."

# Komma-getrennte Liste der LetterLoom-Product-IDs (aus dem Paddle-Dashboard)
# Damit weisen wir Käufe anderer Produkte im selben Account ab
supabase secrets set PADDLE_PRODUCT_IDS="pro_01h..."

# Sandbox oder Live? (pro Deployment unterschiedlich)
supabase secrets set PADDLE_ENVIRONMENT="sandbox"   # oder "production"


//*** migration **************************************
-- Lizenz-Tabelle für einmalige Käufe via Paddle
create table if not exists public.licenses (
  id uuid primary key default gen_random_uuid(),
  license_id text not null unique,
  paddle_transaction_id text not null unique,
  paddle_customer_id text not null,
  paddle_event_id text not null,
  license_key text not null,
  tier text not null default 'pro',
  issued_at timestamptz not null default now(),
  expires_at timestamptz,
  is_sandbox boolean not null default false,
  created_at timestamptz not null default now()
);

create index if not exists idx_licenses_customer
  on public.licenses (paddle_customer_id);

create index if not exists idx_licenses_user_id -- optional, falls du user_id verknüpfst
  on public.licenses (paddle_customer_id);

-- RLS aktivieren (Zugriff nur über Service-Role aus Edge Functions)
alter table public.licenses enable row level security;

-- Kein direkter Client-Zugriff. Lesezugriff erfolgt ausschließlich über
-- eine separate Edge Function mit Service-Role (z.B. "paddle-license"),
-- die die paddle_customer_id des eingeloggten Users kennt.

//******** deployment ************************************
# Function deployen — WICHTIG: --no-verify-jwt, weil Paddle keinen
# Supabase-JWT im Authorization-Header mitsendet.
supabase functions deploy paddle-webhook --no-verify-jwt

# Migration anwenden
supabase db push


//******* Paddle-Dashboard konfigurieren *************************************

Developer Tools → Notifications → New Endpoint:

Feld	Wert
URL	https://<project-ref>.supabase.co/functions/v1/paddle-webhook
Type	Fulfillment Webhook (nicht Notification)
Events	transaction.completed
API version	die aktuellste (z. B. 1)
Nach dem Speichern: Endpoint-Secret kopieren → PADDLE_WEBHOOK_SECRET setzen.

⚠️ Wichtig: Nur als Fulfillment Webhook registrieren, wenn du möchtest, 
dass der license_key aus dem Response-Body automatisch in der Paddle-Kaufbestätigungs-E-Mail erscheint. 

Wenn du später eine eigene E-Mail versenden willst (z. B. mit deinem Branding via Resend),
registriere den Endpoint als Notification Webhook – dann musst du den Versand selbst übernehmen.


//******* edge function *************************************

-- Lizenz-Tabelle für einmalige Käufe via Paddle
create table if not exists public.licenses (
  id uuid primary key default gen_random_uuid(),
  license_id text not null unique,
  paddle_transaction_id text not null unique,
  paddle_customer_id text not null,
  paddle_event_id text not null,
  license_key text not null,
  tier text not null default 'pro',
  issued_at timestamptz not null default now(),
  expires_at timestamptz,
  is_sandbox boolean not null default false,
  created_at timestamptz not null default now()
);

create index if not exists idx_licenses_customer
  on public.licenses (paddle_customer_id);

create index if not exists idx_licenses_user_id -- optional, falls du user_id verknüpfst
  on public.licenses (paddle_customer_id);

-- RLS aktivieren (Zugriff nur über Service-Role aus Edge Functions)
alter table public.licenses enable row level security;

-- Kein direkter Client-Zugriff. Lesezugriff erfolgt ausschließlich über
-- eine separate Edge Function mit Service-Role (z.B. "paddle-license"),
-- die die paddle_customer_id des eingeloggten Users kennt.