using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization; // Hinzugefügt für AddAuthorizationCore
using SupabaseRcl.Services;                         // Hinzugefügt für den SupabaseService
using SupabaseRcl.Providers;                        // Hinzugefügt für den SupabaseAuthStateProvider

namespace EmineoBlazor;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        // Standard HttpClient-Registrierung
        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

        // 1. Core-Authentifizierung für Blazor WASM aktivieren
        builder.Services.AddAuthorizationCore();

        // 2. Den zentralen Supabase-Service als Singleton registrieren
        builder.Services.AddSingleton<SupabaseService>();

        // 3. Den Custom AuthenticationStateProvider für Blazor registrieren
        builder.Services.AddScoped<AuthenticationStateProvider, SupabaseAuthStateProvider>();

        // App bauen
        var host = builder.Build();

        // 4. Supabase-Client vor dem App-Start mit deinen echten Anmeldedaten initialisieren
        // (Damit wird auch eine bestehende Browser-Sitzung sofort wiederhergestellt)
        var supabaseService = host.Services.GetRequiredService<SupabaseService>();
        await supabaseService.InitializeAsync(
            "https://supabase.co", // Ersetze dies durch deine Supabase URL
            "dein-anon-key"                     // Ersetze dies durch deinen Supabase Anon Key
        );

        // App ausführen
        await host.RunAsync();
    }
}