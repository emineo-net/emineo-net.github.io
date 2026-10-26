# ============================================================================
#  PADDLE-WEBHOOK — DEPLOYMENT & TEST
#  Ausgangslage: Supabase-Projekt existiert (subscriptions-Tabelle + Functions)
#  Ziel:         Webhook-Function, DB-Migration, Secrets, Paddle-Endpoint
# ============================================================================


# ============================================================================
# SCHRITT 0 — VORBEREITUNG
# Ausführen in: PowerShell (beliebiges Verzeichnis)
# ============================================================================

# 0.1 Supabase CLI prüfen
supabase --version
# Erwartet: 2.x.x. Falls Fehler: https://supabase.com/docs/guides/cli

# 0.2 Projekt-Root der Solution finden (dort, wo "supabase/" liegt)
Get-ChildItem -Path "C:\Users\steph\source\repos\NetAi.Vacancy" `
  -Recurse -Directory -Filter "supabase" -ErrorAction SilentlyContinue |
  Select-Object -ExpandProperty FullName

# 0.3 In das Projekt-Root wechseln (Beispiel — an dein Ergebnis aus 0.2 anpassen!)
cd C:\Users\steph\source\repos\NetAi.Vacancy

# 0.4 Sanity-Check: liegt supabase/ hier?
Get-ChildItem .\supabase | Select-Object Name
# Erwartet: config.toml, functions, migrations, seed.sql ...

# 0.5 Prüfen, ob das Projekt bereits mit Supabase verlinkt ist
Test-Path ".\supabase\.temp\project-ref"
# Erwartet: True
# Falls False:
#   supabase login
#   supabase link --project-ref <dein-project-ref>

# 0.6 project-ref merken (für URLs später)
$projectRef = (Select-String -Path ".\supabase\config.toml" -Pattern '^project_id\s*=\s*"([^"]+)"').Matches.Groups[1].Value
Write-Host "Project ref: $projectRef"


# ============================================================================
# SCHRITT 1 — SECRETS SETZEN
# Ausführen in: PowerShell, im Projekt-Root (cd aus 0.3)
# ============================================================================

# 1.1 Prüfen, dass der private Key vom Keygen noch da ist
Test-Path ".\LetterLoomKeygen\privateKey.pem"
# Erwartet: True
# Der Pfad muss ggf. angepasst werden, falls der Keygen anders heißt / liegt

# 1.2 Sicherheits-Check: ist es wirklich der PRIVATE Key?
Get-Content ".\LetterLoomKeygen\privateKey.pem" -TotalCount 1
# Erwartet: -----BEGIN PRIVATE KEY-----
# WENN hier "PUBLIC" steht: ABBRECHEN, du hast den falschen Key!

# 1.3 Temporäre .env-Datei erstellen (wird gleich wieder gelöscht)
#     Platzhalter pdl_ntfset_REPLACE_ME + pro_REPLACE_ME werden in Schritt 4 ersetzt
@"
LICENSE_PRIVATE_KEY="$(Get-Content -Raw -Path '.\LetterLoomKeygen\privateKey.pem')"
PADDLE_WEBHOOK_SECRET=pdl_ntfset_REPLACE_ME
PADDLE_PRODUCT_IDS=pro_REPLACE_ME
PADDLE_ENVIRONMENT=sandbox
"@ | Set-Content -Path ".\.env.paddle" -Encoding ascii

# 1.4 Prüfen (nur die Schlüsselnamen, NIEMALS den Inhalt loggen!)
Get-Content ".\.env.paddle" | ForEach-Object { ($_ -split '=')[0] }
# Erwartet:
#   LICENSE_PRIVATE_KEY
#   PADDLE_WEBHOOK_SECRET
#   PADDLE_PRODUCT_IDS
#   PADDLE_ENVIRONMENT

# 1.5 Secrets zu Supabase pushen
supabase secrets set --env-file .\.env.paddle

# 1.6 .env.paddle SOFORT löschen (enthält den Private Key im Klartext!)
Remove-Item ".\.env.paddle" -Force
Test-Path ".\.env.paddle"
# Erwartet: False

# 1.7 Verify: Secrets sind gesetzt (nur Namen sichtbar, Werte sind verschlüsselt)
supabase secrets list
# Erwartet: LICENSE_PRIVATE_KEY, PADDLE_WEBHOOK_SECRET, PADDLE_PRODUCT_IDS, PADDLE_ENVIRONMENT


# ============================================================================
# SCHRITT 2 — DB-MIGRATION
# Ausführen in: PowerShell, im Projekt-Root
# ============================================================================

# 2.1 Migrations-Ordner sicherstellen
New-Item -ItemType Directory -Force -Path ".\supabase\migrations" | Out-Null

# 2.2 Migrations-Datei anlegen (Timestamp-Konvention)
$stamp = Get-Date -Format "yyyyMMddHHmmss"
$migrationPath = ".\supabase\migrations\${stamp}_create_licenses_table.sql"
Write-Host "Migration: $migrationPath"

# 2.3 SQL schreiben (single-quote here-string = keine Interpolation)
@'
-- Lizenzen für einmalige Paddle-Käufe (LetterLoom pro)
create table if not exists public.licenses (
  id                    uuid        primary key default gen_random_uuid(),
  license_id            text        not null unique,
  paddle_transaction_id text        not null unique,
  paddle_customer_id    text        not null,
  paddle_event_id       text        not null,
  license_key           text        not null,
  tier                  text        not null default 'pro',
  issued_at             timestamptz not null default now(),
  expires_at            timestamptz,
  is_sandbox            boolean     not null default false,
  created_at            timestamptz not null default now()
);

create index if not exists idx_licenses_paddle_customer
  on public.licenses (paddle_customer_id);

-- RLS aktiv: kein direkter Client-Zugriff.
-- Lesezugriff läuft ausschließlich über Edge Functions mit Service-Role.
alter table public.licenses enable row level security;
'@ | Set-Content -Path $migrationPath -Encoding ascii

# 2.4 Content kurz prüfen
Get-Content $migrationPath | Select-Object -First 5
# Erwartet: die ersten Zeilen des CREATE TABLE

# 2.5 Migration anwenden
supabase db push
# Erwartet: "Applying migration ..." und "Finished supabase db push."

# 2.6 Verify: Migration ist in der Remote-Liste als "applied" markiert
supabase migration list
# Erwartet: neueste Zeile hat den Timestamp aus 2.2 in BEIDEN Spalten (Local + Remote)

# 2.7 Verify: Tabelle existiert und ist leer
#     → Supabase Dashboard → SQL Editor → ausführen:
#       select count(*) from public.licenses;
#     Erwartet: 0


# ============================================================================
# SCHRITT 3 — EDGE FUNCTION DEPLOYEN
# Ausführen in: PowerShell, im Projekt-Root
# ============================================================================

# 3.1 Function-Datei anlegen (MANUELL in VSCode/Cursor)
#     Pfad:  .\supabase\functions\paddle-webhook\index.ts
#     Inhalt: der TypeScript-Code aus der vorherigen Antwort
#     (Struktur analog zu deiner paddle-customer-portal/index.ts)

# 3.2 Prüfen, dass die Datei da ist
Test-Path ".\supabase\functions\paddle-webhook\index.ts"
# Erwartet: True

# 3.3 Function deployen — WICHTIG: --no-verify-jwt
#     Paddle sendet keinen Supabase-JWT, sondern Paddle-Signature.
#     Die Auth-Prüfung passiert IM Code via HMAC.
supabase functions deploy paddle-webhook --no-verify-jwt
# Erwartet: "Deployed Functions on project ...: paddle-webhook"

# 3.4 Verify: URL ist erreichbar (OPTIONS-Preflight → 200 mit CORS-Headern)
$webhookUrl = "https://$projectRef.supabase.co/functions/v1/paddle-webhook"
Write-Host "Webhook URL: $webhookUrl"
Invoke-WebRequest -Uri $webhookUrl -Method OPTIONS -UseBasicParsing |
  Select-Object StatusCode
# Erwartet: StatusCode 200

# 3.5 Verify: POST ohne Signatur → 401
try {
  Invoke-WebRequest -Uri $webhookUrl -Method POST `
    -Body '{"test":true}' -ContentType "application/json" -UseBasicParsing
  Write-Host "FEHLER: hätte 401 geben müssen"
} catch {
  Write-Host "Erwarteter Fehler: $($_.Exception.Response.StatusCode)"
}
# Erwartet: Unauthorized (401)


# ============================================================================
# SCHRITT 4 — PADDLE-DASHBOARD KONFIGURIEREN
# Ausführen in: Browser
#   Sandbox: https://sandbox-vendors.paddle.com
#   Live:    https://vendors.paddle.com
# ============================================================================

# 4.1 Product-ID ablesen
#     Dashboard → Catalog → Products → LetterLoom → "Product ID"
#     Format: pro_01h...
#     MERKEN als: <PRODUCT_ID>

# 4.2 Webhook-Endpoint anlegen
#     Dashboard → Developer Tools → Notifications → "+ New destination"
#     - Destination type: Webhook
#     - URL: https://<PROJECT_REF>.supabase.co/functions/v1/paddle-webhook
#       (PROJECT_REF = Wert aus Schritt 0.6)
#     - Events: "transaction.completed" (nur dieses eine)
#     - Type: Fulfillment Webhook  ← WICHTIG, damit der license_key
#                                     in der Kaufbestätigungs-E-Mail landet
#     - Beschreibung: "LetterLoom License Issuer (Supabase)"
#     Speichern → "Secret key" kopieren
#     Format: pdl_ntfset_...
#     MERKEN als: <WEBHOOK_SECRET>

# 4.3 Zurück in PowerShell (im Projekt-Root): die beiden Platzhalter ersetzen
#     ERSETZE die drei "..."-Werte durch deine echten Werte!
supabase secrets set PADDLE_WEBHOOK_SECRET="pdl_ntfset_..."
supabase secrets set PADDLE_PRODUCT_IDS="pro_01h..."
supabase secrets set PADDLE_ENVIRONMENT="sandbox"

# 4.4 Verify: alle 4 Secrets sind gesetzt
supabase secrets list | Select-String "PADDLE_|LICENSE_"
# Erwartet: LICENSE_PRIVATE_KEY, PADDLE_WEBHOOK_SECRET, PADDLE_PRODUCT_IDS, PADDLE_ENVIRONMENT

# 4.5 Function neu deployen (damit die neuen Secrets gelesen werden)
supabase functions deploy paddle-webhook --no-verify-jwt


# ============================================================================
# SCHRITT 5 — END-TO-END-TEST (Sandbox)
# Ausführen in: PowerShell (5.1: loggen), Dashboard (5.2-5.3), WPF (5.4)
# ============================================================================

# 5.1 Paddle Sandbox Simulator (Voraussetzung: Paddle CLI, `paddle auth` bereits durch)
paddle sandbox simulate transaction.completed `
  --customer-id "ctm_test_REPLACE_ME" `
  --product-id  "pro_01h..."
# Erwartet: "Simulation scheduled. Watch your webhook endpoint."
# Falls Simulator nicht verfügbar ist:
#   Dashboard → Developer Tools → Notifications → Endpoint öffnen →
#   "Send test event" → Event-Typ: transaction.completed

# 5.2 Kurz warten (5-10 s), dann in Supabase Dashboard → SQL Editor:
#     select license_id, paddle_transaction_id, tier, is_sandbox,
#            left(license_key, 20) as key_prefix, created_at
#     from public.licenses
#     order by created_at desc
#     limit 3;
#     Erwartet: eine neue Zeile mit key_prefix = "llv1...."

# 5.3 Function-Logs prüfen
#     Dashboard → Edge Functions → paddle-webhook → Logs
#     Erwartet: "[paddle-webhook] Issued license ... for txn ..."
#     Keine "Invalid signature" oder "DB insert failed"

# 5.4 Idempotenz-Test: dasselbe Event nochmal senden
paddle sandbox simulate transaction.completed `
  --customer-id "ctm_test_REPLACE_ME" `
  --product-id  "pro_01h..."
# Erwartet in 5.2: KEINE neue Zeile (paddle_transaction_id ist unique,
#                   Function gibt denselben license_key zurück)

# 5.5 Vollen license_key aus der DB kopieren
#     Supabase Dashboard → SQL Editor:
#       select license_key from public.licenses
#       order by created_at desc limit 1;
#     → kompletten String in die Zwischenablage

# 5.6 WPF-Aktivierung testen
#     - WPF-App starten
#     - Sidebar → "License"
#     - Key einfügen → "Activate"
#     Erwartet: grüner "Pro license active"-Banner
#             PRO-Badge in der Sidebar
#             license.key liegt jetzt unter:
#               %LOCALAPPDATA%\NetAi.LetterLoom\license.key

# 5.7 WPF-Persistenz-Test
#     - WPF-App schließen und neu starten
#     - Sidebar → "License"
#     Erwartet: "Pro license active" (Key wurde geladen und offline verifiziert)

# 5.8 Deaktivierungs-Test
#     - Im License-Dialog → "Deactivate on this PC"
#     Erwartet: Banner verschwindet, PRO-Badge weg,
#               license.key gelöscht

# 5.9 Deaktivierung ist umkehrbar: Key erneut aus 5.5 einfügen → wieder Pro


# ============================================================================
# SCHRITT 6 — TROUBLESHOOTING
# Ausführen in: Dashboard (SQL/Logs) und PowerShell
# ============================================================================

# 6.1 Function gibt 401 "Invalid signature" zurück
#     Ursache: PADDLE_WEBHOOK_SECRET stimmt nicht mit dem Endpoint-Secret überein
#     Prüfen:  Dashboard → Notifications → Endpoint → "Secret key" neu kopieren
#              → supabase secrets set PADDLE_WEBHOOK_SECRET="..." (Schritt 4.3)
#              → supabase functions deploy paddle-webhook --no-verify-jwt

# 6.2 Function gibt 500 "server misconfigured" zurück
#     Ursache: mindestens ein Secret fehlt
#     Prüfen:  supabase secrets list
#     Fix:     Schritt 4.3 + 4.5 wiederholen

# 6.3 Function gibt 200 zurück, aber keine Zeile in DB
#     Ursache: product_id nicht in PADDLE_PRODUCT_IDS, oder Event != transaction.completed
#     Prüfen:  Logs in Edge Functions → paddle-webhook
#              "[paddle-webhook] Unknown product: ..." → PADDLE_PRODUCT_IDS korrigieren
#              "{ok:true, ignored: '...'}" → falscher Event-Typ im Endpoint konfiguriert
#              ("Fulfillment" braucht transaction.completed, nicht subscription.*)

# 6.4 Function gibt 500 "signing failed" zurück
#     Ursache: LICENSE_PRIVATE_KEY ist kein gültiger PEM / PKCS#8
#     Prüfen:  Get-Content .\LetterLoomKeygen\privateKey.pem -TotalCount 1
#              Erwartet: -----BEGIN PRIVATE KEY-----
#              Falls "-----BEGIN RSA PRIVATE KEY-----": mit OpenSSL zu PKCS#8 konvertieren
#              openssl pkcs8 -topk8 -inform PEM -in privateKey.pem -outform PEM -nocrypt -out privateKey-pkcs8.pem
#              Dann Schritt 1.3-1.6 mit der PKCS#8-Datei wiederholen

# 6.5 WPF sagt "InvalidSignature" trotz korrektem Key aus DB
#     Ursache: publicKey in license-public.pem passt nicht zum privateKey in Supabase
#     Prüfen:  Fingerprint auf beiden Seiten vergleichen (nur bei dir lokal!):
#              $pem = Get-Content -Raw .\LetterLoomKeygen\publicKey.pem
#              $rsa = [System.Security.Cryptography.RSA]::Create()
#              $rsa.ImportFromPem($pem)
#              [Convert]::ToBase64String($rsa.ExportSubjectPublicKeyInfo())
#              → vergleichen mit: Get-Content .\NetAi.LetterLoom\Licensing\license-public.pem
#                (Body ohne Header/Footer, Zeilenumbrüche entfernen)
#     Fix:     Wenn unterschiedlich → Keygen neu laufen lassen, BEIDE Seiten tauschen
#              (Public Key in WPF neu einbetten, Private Key neu in Supabase)

# 6.6 Cleanup: Sandbox-Lizenzen aus der DB löschen
#     Supabase Dashboard → SQL Editor:
#       delete from public.licenses where is_sandbox = true;