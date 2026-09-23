using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using PaddleRcl.Services;
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
        builder.Services.AddPaddleRcl();

        var host = builder.Build();

        // Initialisierung anstoßen (Task wird intern in SupabaseService gespeichert),
        // aber NICHT hier awaiten, damit host.RunAsync() nicht blockiert.
        var supabaseService = host.Services.GetRequiredService<SupabaseService>();
        _ = supabaseService.InitializeAsync(
            "https://otyvyonmcdigfrhngsqe.supabase.co",
            "sb_publishable_sx9-w8GWInjebZ_YNQxMuA_9XydYfKo"
        );

        await host.RunAsync();
    }
}