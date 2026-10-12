// ============================================================================
// Emineo â€“ site.js
// Paddle setup + Bootstrap modal helpers + share helpers
// ============================================================================

const PADDLE_CLIENT_TOKEN = "test_XXXXXXXXXXXXXXXXXXXXXXXXXXX";
const PADDLE_ENV = "sandbox";

const PRICES = {
  enterprise_single: "pri_XXXXXXXXXXXXXXXXXXXXXXXX",
  bundle_enterprise: "pri_YYYYYYYYYYYYYYYYYYYYYYYY",
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

window.emineoShare = {
    async share(payload) {
        if (navigator.share) {
            try {
                await navigator.share(payload);
                return true;
            } catch (e) {
                if (e.name === 'AbortError') return false;
            }
        }
        if (navigator.clipboard && window.isSecureContext) {
            await navigator.clipboard.writeText(payload.url);
            return true;
        }
        window.prompt("Copy this link:", payload.url);
        return true;
    }
};

document.addEventListener("DOMContentLoaded", initPaddle);