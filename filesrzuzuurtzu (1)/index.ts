// supabase/functions/paddle-webhook/index.ts
//
// Empfängt Paddle-Webhooks (Billing API v2) und schreibt/aktualisiert
// den Abo-Zustand in public.subscriptions.
//
// Benötigte Secrets (supabase secrets set ...):
//   SUPABASE_URL                    (automatisch vorhanden)
//   SUPABASE_SERVICE_ROLE_KEY       (automatisch vorhanden)
//   PADDLE_WEBHOOK_SECRET_SANDBOX   -> Secret der Sandbox-Webhook-Destination
//   PADDLE_WEBHOOK_SECRET_LIVE      -> Secret der Live-Webhook-Destination
//
// In Paddle: zwei getrennte Webhook-Destinations anlegen
// (eine im Sandbox-Dashboard, eine im Live-Dashboard), beide auf diese URL zeigen:
//   https://<project-ref>.supabase.co/functions/v1/paddle-webhook

import { createClient } from "https://esm.sh/@supabase/supabase-js@2";

const supabase = createClient(
  Deno.env.get("SUPABASE_URL")!,
  Deno.env.get("SUPABASE_SERVICE_ROLE_KEY")!,
);

const SECRET_CANDIDATES: { secret: string; isSandbox: boolean }[] = [
  { secret: Deno.env.get("PADDLE_WEBHOOK_SECRET_SANDBOX") ?? "", isSandbox: true },
  { secret: Deno.env.get("PADDLE_WEBHOOK_SECRET_LIVE") ?? "", isSandbox: false },
].filter((c) => c.secret.length > 0);

async function hmacSha256Hex(secret: string, message: string): Promise<string> {
  const key = await crypto.subtle.importKey(
    "raw",
    new TextEncoder().encode(secret),
    { name: "HMAC", hash: "SHA-256" },
    false,
    ["sign"],
  );
  const sigBuf = await crypto.subtle.sign("HMAC", key, new TextEncoder().encode(message));
  return Array.from(new Uint8Array(sigBuf))
    .map((b) => b.toString(16).padStart(2, "0"))
    .join("");
}

// Prüft die Paddle-Signature (Header: "ts=...;h1=...") gegen alle bekannten Secrets.
// Gibt zurück, mit welchem Secret sie passt -> daraus leiten wir is_sandbox ab.
async function verifySignature(rawBody: string, header: string): Promise<boolean | null> {
  const parts = Object.fromEntries(
    header.split(";").map((kv) => kv.split("=") as [string, string]),
  );
  const ts = parts["ts"];
  const h1 = parts["h1"];
  if (!ts || !h1) return null;

  const signedPayload = `${ts}:${rawBody}`;

  for (const { secret, isSandbox } of SECRET_CANDIDATES) {
    const expected = await hmacSha256Hex(secret, signedPayload);
    if (expected === h1) return isSandbox;
  }
  return null;
}

function extractPriceAndProduct(data: any) {
  const item = data.items?.[0];
  return {
    priceId: item?.price?.id ?? null,
    productId: item?.price?.product_id ?? item?.product?.id ?? null,
  };
}

async function upsertSubscription(data: any, isSandbox: boolean, rawEvent: unknown) {
  // Wir erwarten, dass beim Checkout custom_data.user_id = Supabase auth.uid() mitgegeben wurde.
  const userId = data.custom_data?.user_id;
  if (!userId) {
    console.error("paddle-webhook: kein custom_data.user_id in subscription", data.id);
    return;
  }

  const { priceId, productId } = extractPriceAndProduct(data);

  const payload = {
    user_id: userId,
    paddle_subscription_id: data.id,
    paddle_customer_id: data.customer_id,
    status: data.status,
    price_id: priceId,
    product_id: productId,
    current_period_start: data.current_billing_period?.starts_at ?? null,
    current_period_end: data.current_billing_period?.ends_at ?? null,
    cancel_at_period_end: data.scheduled_change?.action === "cancel",
    canceled_at: data.status === "canceled"
      ? (data.canceled_at ?? new Date().toISOString())
      : null,
    trial_ends_at: data.status === "trialing" ? data.current_billing_period?.ends_at ?? null : null,
    is_sandbox: isSandbox,
    raw_event: rawEvent,
  };

  const { error } = await supabase
    .from("subscriptions")
    .upsert(payload, { onConflict: "paddle_subscription_id" });

  if (error) {
    console.error("paddle-webhook: upsert fehlgeschlagen", error);
  }
}

Deno.serve(async (req) => {
  if (req.method !== "POST") {
    return new Response("Method not allowed", { status: 405 });
  }

  const rawBody = await req.text();
  const sigHeader = req.headers.get("Paddle-Signature");
  if (!sigHeader) {
    return new Response("Missing Paddle-Signature header", { status: 400 });
  }

  const isSandbox = await verifySignature(rawBody, sigHeader);
  if (isSandbox === null) {
    return new Response("Invalid signature", { status: 401 });
  }

  let event: any;
  try {
    event = JSON.parse(rawBody);
  } catch {
    return new Response("Invalid JSON", { status: 400 });
  }

  const eventType: string = event.event_type;
  const data = event.data;

  switch (eventType) {
    case "subscription.created":
    case "subscription.activated":
    case "subscription.updated":
    case "subscription.canceled":
    case "subscription.paused":
    case "subscription.resumed":
      await upsertSubscription(data, isSandbox, event);
      break;
    default:
      // Andere Events (z.B. transaction.completed) hier bei Bedarf ergänzen.
      break;
  }

  return new Response("OK", { status: 200 });
});
