// supabase/functions/paddle-customer-portal/index.ts
//
// Gibt dem eingeloggten Supabase-User die URL zu seinem Paddle Customer Portal zurück.
// Wird vom Blazor-Frontend mit dem Supabase-Access-Token (Authorization: Bearer <token>) aufgerufen.
//
// Benötigte Secrets:
//   SUPABASE_URL, SUPABASE_ANON_KEY, SUPABASE_SERVICE_ROLE_KEY  (automatisch vorhanden)
//   PADDLE_API_KEY_SANDBOX   -> Sandbox API-Key aus dem Paddle-Dashboard
//   PADDLE_API_KEY_LIVE      -> Live API-Key aus dem Paddle-Dashboard

import { createClient } from "https://esm.sh/@supabase/supabase-js@2";

const CORS_HEADERS = {
  "Access-Control-Allow-Origin": "*", // in Produktion auf deine Domain einschränken
  "Access-Control-Allow-Headers": "authorization, content-type",
  "Access-Control-Allow-Methods": "POST, OPTIONS",
};

Deno.serve(async (req) => {
  if (req.method === "OPTIONS") {
    return new Response(null, { headers: CORS_HEADERS });
  }
  if (req.method !== "POST") {
    return new Response("Method not allowed", { status: 405, headers: CORS_HEADERS });
  }

  const authHeader = req.headers.get("Authorization");
  if (!authHeader) {
    return new Response("Unauthorized", { status: 401, headers: CORS_HEADERS });
  }

  // Client im Namen des eingeloggten Users, um das JWT zu prüfen
  const supabaseAuth = createClient(
    Deno.env.get("SUPABASE_URL")!,
    Deno.env.get("SUPABASE_ANON_KEY")!,
    { global: { headers: { Authorization: authHeader } } },
  );

  const { data: { user }, error: userError } = await supabaseAuth.auth.getUser();
  if (userError || !user) {
    return new Response("Unauthorized", { status: 401, headers: CORS_HEADERS });
  }

  // Service-Role-Client für den DB-Zugriff (umgeht RLS gezielt und sicher, da wir user.id serverseitig haben)
  const supabaseAdmin = createClient(
    Deno.env.get("SUPABASE_URL")!,
    Deno.env.get("SUPABASE_SERVICE_ROLE_KEY")!,
  );

  const { data: sub, error: subError } = await supabaseAdmin
    .from("subscriptions")
    .select("paddle_customer_id, is_sandbox")
    .eq("user_id", user.id)
    .order("created_at", { ascending: false })
    .limit(1)
    .maybeSingle();

  if (subError || !sub) {
    return new Response(
      JSON.stringify({ error: "Kein Abo für diesen User gefunden." }),
      { status: 404, headers: { ...CORS_HEADERS, "Content-Type": "application/json" } },
    );
  }

  const apiBase = sub.is_sandbox ? "https://sandbox-api.paddle.com" : "https://api.paddle.com";
  const apiKey = sub.is_sandbox
    ? Deno.env.get("PADDLE_API_KEY_SANDBOX")
    : Deno.env.get("PADDLE_API_KEY_LIVE");

  if (!apiKey) {
    return new Response("Paddle API key nicht konfiguriert", { status: 500, headers: CORS_HEADERS });
  }

  const paddleRes = await fetch(
    `${apiBase}/customers/${sub.paddle_customer_id}/portal-sessions`,
    {
      method: "POST",
      headers: {
        "Authorization": `Bearer ${apiKey}`,
        "Content-Type": "application/json",
      },
      body: JSON.stringify({}), // optional: subscription_ids: [...] um direkt auf ein Abo zu verlinken
    },
  );

  if (!paddleRes.ok) {
    const errText = await paddleRes.text();
    console.error("Paddle portal-session Fehler:", errText);
    return new Response(
      JSON.stringify({ error: "Paddle-Anfrage fehlgeschlagen" }),
      { status: 502, headers: { ...CORS_HEADERS, "Content-Type": "application/json" } },
    );
  }

  const json = await paddleRes.json();
  const portalUrl = json.data?.urls?.general?.overview;

  return new Response(
    JSON.stringify({ url: portalUrl }),
    { status: 200, headers: { ...CORS_HEADERS, "Content-Type": "application/json" } },
  );
});
