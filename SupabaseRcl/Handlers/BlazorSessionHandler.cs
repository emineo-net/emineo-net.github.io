using Microsoft.JSInterop;
using Supabase;
using Supabase.Gotrue;
using Supabase.Interfaces; // Hier liegt das korrekte Interface
using System.Text.Json;

namespace SupabaseRcl.Handlers;

public class BlazorSessionHandler : DefaultSupabaseSessionHandler
{
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _module;

    public BlazorSessionHandler(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    private async ValueTask<IJSObjectReference> GetModuleAsync()
    {
        return _module ??= await _jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/SupabaseRcl/supabaseAuth.js");
    }

    // Speichern der Session im LocalStorage
    public void SaveSession(Session session)
    {
        Task.Run(async () =>
        {
            try
            {
                var module = await GetModuleAsync();
                var json = JsonSerializer.Serialize(session);
                await module.InvokeVoidAsync("saveSession", json);
            }
            catch
            {
                // Fehler abfangen
            }
        }).GetAwaiter().GetResult();
    }

    // Laden der Session aus dem LocalStorage
    public Session? LoadSession()
    {
        return Task.Run(async () =>
        {
            try
            {
                var module = await GetModuleAsync();
                var json = await module.InvokeAsync<string?>("getSession");

                if (string.IsNullOrEmpty(json)) return null;

                return JsonSerializer.Deserialize<Session>(json);
            }
            catch
            {
                return null;
            }
        }).GetAwaiter().GetResult();
    }

    // Löschen der Session beim Logout
    public void DestroySession()
    {
        Task.Run(async () =>
        {
            try
            {
                var module = await GetModuleAsync();
                await module.InvokeVoidAsync("clearSession");
            }
            catch
            {
                // Fehler abfangen
            }
        }).GetAwaiter().GetResult();
    }
}
