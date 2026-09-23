using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace PaddleRcl;

public partial class Checkout : ComponentBase, IAsyncDisposable
{
    [Parameter][EditorRequired] public string CustomerId { get; set; } = string.Empty;
    [Parameter][EditorRequired] public string VendorToken { get; set; } = string.Empty;
    [Parameter][EditorRequired] public string PriceId { get; set; } = string.Empty;
    [Parameter] public string CustomerEmail { get; set; } = string.Empty;
    [Parameter] public string ButtonText { get; set; } = "Jetzt kaufen";
    [Parameter] public string Environment { get; set; } = "sandbox"; // oder "production"

    [Parameter] public EventCallback<object> OnSuccess { get; set; }
    [Parameter] public EventCallback OnClosed { get; set; }
    [Parameter] public EventCallback<(string EventName, string JsonData)> OnGenericEvent { get; set; }

    // Neu: Fehler nach außen durchreichen, statt sie nur zu loggen.
    [Parameter] public EventCallback<string> OnError { get; set; }

    [Inject] private ILogger<Checkout> Logger { get; set; } = default!;

    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<Checkout>? _dotNetRef;
    private bool _isCheckoutInProgress;

    protected bool IsLoading { get; private set; }

    // Modul wird nicht mehr in OnAfterRenderAsync geladen, sondern erst
    // wenn der Checkout tatsächlich gestartet wird ("lazy").
    private async Task<IJSObjectReference> GetModuleAsync()
    {
        _jsModule ??= await JSRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/PaddleRcl/PaddleRcl.js");
        return _jsModule;
    }

    protected async Task StartCheckoutAsync()
    {
        // Schutz gegen Doppel-/Mehrfachklicks, solange ein Checkout bereits läuft.
        if (_isCheckoutInProgress) return;

        if (string.IsNullOrWhiteSpace(CustomerId))
        {
            const string msg = "CustomerId fehlt – Checkout kann ohne eingeloggten User nicht gestartet werden.";
            Logger.LogWarning(msg);
            await OnError.InvokeAsync(msg);
            return;
        }

        _isCheckoutInProgress = true;
        IsLoading = true;
        StateHasChanged();

        try
        {
            var module = await GetModuleAsync();

            await module.InvokeVoidAsync("initializePaddle", VendorToken, Environment);

            _dotNetRef ??= DotNetObjectReference.Create(this);

            await module.InvokeVoidAsync(
                "openPaddleCheckout", PriceId, CustomerEmail, CustomerId, _dotNetRef);
        }
        catch (JSDisconnectedException)
        {
            // Seite wurde gewechselt / Verbindung getrennt, während der Checkout startete.
            // Kein echter Fehlerfall, einfach ignorieren.
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Fehler beim Starten des Paddle-Checkouts");
            await OnError.InvokeAsync($"Checkout konnte nicht gestartet werden: {ex.Message}");
        }
        finally
        {
            // IsLoading bleibt bewusst bis checkout.completed/closed/Fehler bestehen,
            // _isCheckoutInProgress wird hier zurückgesetzt, da Paddle.Checkout.open
            // asynchron über eventCallback weiterläuft, nicht über diesen Task.
            _isCheckoutInProgress = false;
            IsLoading = false;
            StateHasChanged();
        }
    }

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
                // Ignorieren im WASM-Kontext bei Seitenwechsel.
            }
        }

        GC.SuppressFinalize(this);
    }
}