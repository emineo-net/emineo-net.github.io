using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace PaddleRcl;

public partial class Checkout : ComponentBase, IAsyncDisposable
{
    [Parameter][EditorRequired] public string VendorToken { get; set; } = string.Empty;
    [Parameter][EditorRequired] public string PriceId { get; set; } = string.Empty;
    [Parameter] public string CustomerEmail { get; set; } = string.Empty;
    [Parameter] public string ButtonText { get; set; } = "Jetzt kaufen";
    [Parameter] public string Environment { get; set; } = "sandbox"; // oder "production"

    // Neue Callbacks für die Haupt-App
    [Parameter] public EventCallback<object> OnSuccess { get; set; }
    [Parameter] public EventCallback OnClosed { get; set; }
    [Parameter] public EventCallback<(string EventName, string JsonData)> OnGenericEvent { get; set; }

    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<Checkout>? _dotNetRef;
    protected bool IsLoading { get; private set; } = false;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            _jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/PaddleRcl/PaddleRcl.js");
    }

    protected async Task StartCheckoutAsync()
    {
        if (_jsModule is null) return;

        try
        {
            IsLoading = true;
            StateHasChanged();

            await _jsModule.InvokeVoidAsync("initializePaddle", VendorToken, Environment);

            // Referenz auf diese C#-Klasse für JS erstellen
            _dotNetRef = DotNetObjectReference.Create(this);

            // Wir übergeben die Referenz an die Checkout-Funktion
            await _jsModule.InvokeVoidAsync("openPaddleCheckout", PriceId, CustomerEmail, _dotNetRef);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Paddle-Checkout: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    // Diese Methoden werden vom JavaScript aufgerufen
    [JSInvokable]
    public async Task OnCheckoutCompleted(object paddleData)
    {
        IsLoading = false;
        StateHasChanged();
        await OnSuccess.InvokeAsync(paddleData);
    }

    [JSInvokable]
    public async Task OnCheckoutClosed()
    {
        IsLoading = false;
        StateHasChanged();
        await OnClosed.InvokeAsync();
    }

    [JSInvokable]
    public async Task OnPaddleEvent(string eventName, string jsonData)
    {
        await OnGenericEvent.InvokeAsync((eventName, jsonData));
    }

    public async ValueTask DisposeAsync()
    {
        _dotNetRef?.Dispose();

        if (_jsModule is not null)
        {
            try
            {
                await _jsModule.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // Ignorieren im WASM-Kontext bei Seitenwechsel
            }
        }
        GC.SuppressFinalize(this);
    }
}
