using Microsoft.JSInterop;
using SupabaseRcl.Services;

namespace EmineoBlazor.Pages;

    public partial class Home
    {

        private void HandlePaymentSuccess(object data)
        {
            // Hier Code ausführen (z.B. UI-Meldung anzeigen, User-Rolle updaten)
            Console.WriteLine("Zahlung war erfolgreich!");
        }

        private void HandleCheckoutClosed()
        {
            Console.WriteLine("Checkout wurde vom Nutzer geschlossen.");
        }


        private async Task OpenCustomerPortal()
        {
            await SupabaseService.WaitForInitializationAsync();
            var token = SupabaseService.Client.Auth.CurrentSession?.AccessToken;
            if (string.IsNullOrWhiteSpace(token))
                return;

            var url = await PortalService.GetCustomerPortalUrlAsync(token);
            if (url is not null)
                await JSRuntime.InvokeVoidAsync("open", url, "_blank");
        }

        public class SalesInquiry
        {
            public string Plan { get; set; } = "";
            public string Name { get; set; } = "";
            public string Company { get; set; } = "";
            public string Email { get; set; } = "";
            public string TeamSize { get; set; } = "6-20";
            public string Deployment { get; set; } = "cloud";
            public string Message { get; set; } = "";
        }

        private SalesInquiry salesModel = new();

        private async Task Checkout(string planKey)
        {
            await JSRuntime.InvokeVoidAsync("paddleCheckout", planKey);
        }

        private async Task ContactSales(string plan)
        {
            salesModel.Plan = plan;
            await JSRuntime.InvokeVoidAsync("openSalesModal");
        }

        private async Task SubmitSales()
        {
            // TODO: send `salesModel` to your backend / form service
            // await Http.PostAsJsonAsync("/api/contact-sales", salesModel);

            await JSRuntime.InvokeVoidAsync("closeSalesModal");
            salesModel = new SalesInquiry();
        }
}

