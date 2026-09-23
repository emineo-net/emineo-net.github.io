using Microsoft.JSInterop;
using Supabase;
using SupabaseRcl.Handlers;

namespace SupabaseRcl.Services;

public class SupabaseService
{
    public Client Client { get; private set; } = null!;
    private readonly IJSRuntime _jsRuntime;
    private Task? _initTask;

    public SupabaseService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public Task InitializeAsync(string url, string anonKey)
    {
        // Falls schon gestartet (oder fertig), denselben Task zurückgeben,
        // statt eine zweite Initialisierung anzustoßen.
        return _initTask ??= InitializeInternalAsync(url, anonKey);
    }

    private async Task InitializeInternalAsync(string url, string anonKey)
    {
        var options = new SupabaseOptions
        {
            AutoRefreshToken = true,
            SessionHandler = new BlazorSessionHandler(_jsRuntime)
        };

        Client = new Client(url, anonKey, options); // <- crasht hier vermutlich schon
        await Client.InitializeAsync();
    }

    // Von Komponenten aufrufen, bevor sie auf Client zugreifen.
    public Task WaitForInitializationAsync() => _initTask ?? Task.CompletedTask;
}