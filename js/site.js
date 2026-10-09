// ============================================================================
// Emineo – site.js
// Paddle setup + Bootstrap modal helpers
// ============================================================================

// ---------- Paddle ----------
const PADDLE_CLIENT_TOKEN = "test_XXXXXXXXXXXXXXXXXXXXXXXXXXX"; // TODO: fill in
const PADDLE_ENV = "sandbox"; // "sandbox" | "production"

const PRICES = {
  enterprise_single: "pri_XXXXXXXXXXXXXXXXXXXXXXXX", // TODO
  bundle_enterprise: "pri_YYYYYYYYYYYYYYYYYYYYYYYY", // TODO
};

let paddleReady = false;

function initPaddle() {
  if (!window.Paddle) {
    console.warn("Paddle.js not loaded.");
    return;
  }
  Paddle.Environment.set(PADDLE_ENV);
  Paddle.Setup({ token: PADDLE_CLIENT_TOKEN });
  paddleReady = true;
}

// Called from Blazor via JS interop
window.paddleCheckout = function (planKey) {
  if (!paddleReady) initPaddle();

  const priceId = PRICES[planKey];
  if (!priceId || priceId.includes("X") || priceId.includes("Y")) {
    alert("Paddle price ID for '" + planKey + "' is missing. Fill it in site.js.");
    return;
  }

  Paddle.Checkout.open({
    items: [{ priceId: priceId, quantity: 1 }],
    settings: {
      displayMode: "overlay",
      theme: "dark",
      locale: "en",
    },
    customData: {
      userId: window.CURRENT_USER_ID || null,
      plan: planKey,
    },
  });
};

// ---------- Bootstrap modal ----------
window.openSalesModal = function () {
  const el = document.getElementById("salesModal");
  if (!el) return;
  const instance = bootstrap.Modal.getOrCreateInstance(el);
  instance.show();
};

window.closeSalesModal = function () {
  const el = document.getElementById("salesModal");
  if (!el) return;
  const instance = bootstrap.Modal.getInstance(el);
  if (instance) instance.hide();
};

// ---------- Boot ----------
document.addEventListener("DOMContentLoaded", initPaddle);