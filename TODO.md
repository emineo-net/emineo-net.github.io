# 📋 To-Do: Integration Supabase Auth & Paddle Billing (Blazor WASM)

### 1. Architektur & Code-Ebene (RCLs)
- [ ] **RCL-Trennung beibehalten:** Stellen Sie sicher, dass `Supabase.Auth.RCL` und `Paddle.Billing.RCL` zwei separate Projekte bleiben.
- [ ] **Brücken-Logik im Hauptprojekt:** Die Kommunikation zwischen den beiden RCLs erfolgt ausschließlich über das Blazor WASM Hauptprojekt
- [ ] (z. B. durch Übergabe der `UserId` per Parameter).

### 2. Supabase Datenbank-Setup
- [ ] **Profile-Tabelle erstellen:** Eine Tabelle `public.profiles` im `public`-Schema anlegen, die alle B2B-Daten und Paddle-Daten hält.
  ```sql
  create table public.profiles (
    id uuid references auth.users not null primary key,
    company_name text,
    paddle_customer_id text unique,
    subscription_status text default 'inactive',
    updated_at timestamp with time zone
  );
  ```
- [ ] **Auto-Erstellungs-Trigger einrichten:** Eine SQL-Funktion und einen Trigger in Supabase anlegen, 
- [ ] damit bei jeder Registrierung automatisch ein Eintrag in `public.profiles` erzeugt wird.
- [ ] 
- [ ] **Row-Level Security (RLS) aktivieren:** RLS für `public.profiles` einschalten, sodass eingeloggte Benutzer (`auth.uid() = id`) nur ihr eigenes Profil lesen dürfen.

### 3. Paddle & Blazor Checkout-Verknüpfung
- [ ] **User ID an Paddle übergeben:** Im Checkout-Code der `Paddle.Billing.RCL` die aktuelle `supabase.auth.CurrentUser.Id` in das Feld `custom_data` (Passthrough)
- [ ] des Paddle-Checkouts einbetten.

### 4. Webhook & Synchronisation (No-Backend)
- [ ] **Supabase Edge Function erstellen:** Eine TypeScript/Deno Edge Function in Supabase anlegen (z. B. `paddle-webhook`).
- [ ] 
- [ ] **Webhook-Logik implementieren:** Die Edge Function so programmieren, dass sie das Paddle-Event empfängt,
- [ ] die `User.Id` aus `custom_data` ausliest und die `paddle_customer_id` sowie den `subscription_status` in `public.profiles` 
- [ ] mittels Supabase Service-Role (Admin) aktualisiert.
- [ ] 
- [ ] **Paddle Webhook registrieren:** Im Paddle-Dashboard die URL der Supabase Edge Function als Webhook-Ziel eintragen 
- [ ] (z. B. für `subscription.created` und `subscription.updated`).

### 5. Frontend-Abfrage (Blazor WASM)
- [ ] **Status-Check einbauen:** Im Hauptprojekt oder den geschützten Bereichen der App den Zustand aus `public.profiles` abfragen, 
- [ ] um Premium-Features basierend auf dem `subscription_status` freizuschalten.
