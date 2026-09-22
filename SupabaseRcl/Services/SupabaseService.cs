using Microsoft.JSInterop;
using Supabase;
using SupabaseRcl.Handlers;

namespace SupabaseRcl.Services;

public class SupabaseService
{
    public Client Client { get; private set; } = null!;
    private readonly IJSRuntime _jsRuntime;

    public SupabaseService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync(string url, string anonKey)
    {
        var options = new SupabaseOptions
        {
            AutoRefreshToken = true,
            SessionHandler = new BlazorSessionHandler(_jsRuntime)
        };

        Client = new Client(url, anonKey, options);
        await Client.InitializeAsync();
    }
}
