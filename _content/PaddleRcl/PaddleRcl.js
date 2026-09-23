let paddleInitialized = false;

export function initializePaddle(token, environment) {
    if (paddleInitialized || window.Paddle) return Promise.resolve();

    return new Promise((resolve, reject) => {
        const script = document.createElement('script');
        script.src = "https://cdn.paddle.com/paddle/v2/paddle.js";
        script.async = true;
        script.onload = () => {
            window.Paddle.Environment.set(environment);
            window.Paddle.Initialize({ token: token });
            paddleInitialized = true;
            resolve();
        };
        script.onerror = () => reject(new Error("Paddle SDK konnte nicht geladen werden."));
        document.head.appendChild(script);
    });
}

export function openPaddleCheckout(priceId, customerEmail, customerId, dotNetHelper) {
    if (!window.Paddle) {
        console.error("Paddle ist nicht initialisiert.");
        return;
    }

    let items = [{ priceId: priceId, quantity: 1 }];

    window.Paddle.Checkout.open({
        settings: {
            displayMode: "overlay",
            theme: "light",
            locale: "de"
        },
        items: items,
        customer: {
            email: customerEmail
        },
        // Wichtig: landet 1:1 im Webhook-Payload unter data.custom_data.
        // So kann die Edge Function das Abo dem Supabase-User zuordnen.
        customData: {
            user_id: customerId
        },
        eventCallback: function (data) {
            // Wir leiten wichtige Events an Blazor weiter
            if (data.name === "checkout.completed") {
                dotNetHelper.invokeMethodAsync('OnCheckoutCompleted', data.data);
            } else if (data.name === "checkout.closed") {
                dotNetHelper.invokeMethodAsync('OnCheckoutClosed');
            } else {
                dotNetHelper.invokeMethodAsync('OnPaddleEvent', data.name, JSON.stringify(data.data));
            }
        }
    });
}