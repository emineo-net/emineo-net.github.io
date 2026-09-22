using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using SupabaseRcl.Services;
using SupabaseRcl.Providers;

namespace EmineoBlazor;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

        builder.Services.AddAuthorizationCore();
        builder.Services.AddSingleton<SupabaseService>();
        builder.Services.AddScoped<AuthenticationStateProvider, SupabaseAuthStateProvider>();

        var host = builder.Build();

        // LÖSUNG: Initialisierung in den Hintergrund verlagern (entkoppeln),
        // damit der WASM-EntryPoint ohne JS-Interop-Blockade starten kann.
        _ = Task.Run(async () =>
        {
            try
            {
                var supabaseService = host.Services.GetRequiredService<SupabaseService>();
                await supabaseService.InitializeAsync(
                    "https://supabase.co",
                    "dein-anon-key"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Supabase Init Fehler: {ex.Message}");
            }
        });

        await host.RunAsync();
    }
}