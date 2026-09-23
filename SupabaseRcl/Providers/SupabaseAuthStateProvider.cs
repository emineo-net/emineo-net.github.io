using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using SupabaseRcl.Services;

namespace SupabaseRcl.Providers;

public class SupabaseAuthStateProvider : AuthenticationStateProvider
{
    private readonly SupabaseService _supabaseService;
    private readonly ILogger<SupabaseAuthStateProvider> _logger;
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public SupabaseAuthStateProvider(
        SupabaseService supabaseService,
        ILogger<SupabaseAuthStateProvider> logger)
    {
        _supabaseService = supabaseService;
        _logger = logger;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var user = _supabaseService.Client.Auth.CurrentUser;

            if (user == null || string.IsNullOrEmpty(user.Email))
            {
                return new AuthenticationState(_anonymous);
            }

            var identity = BuildIdentity(user.Email, user.Id ?? string.Empty);
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAuthenticationStateAsync fehlgeschlagen, falle zurück auf anonymous");
            return new AuthenticationState(_anonymous);
        }
    }

    public void NotifyUserAuthentication(string email, string id)
    {
        var identity = BuildIdentity(email, id);
        var user = new ClaimsPrincipal(identity);
        var state = Task.FromResult(new AuthenticationState(user));
        NotifyAuthenticationStateChanged(state);
    }

    public void NotifyUserLogout()
    {
        var state = Task.FromResult(new AuthenticationState(_anonymous));
        NotifyAuthenticationStateChanged(state);
    }

    // Zentrale Stelle für die Claims, damit Session-Restore (GetAuthenticationStateAsync)
    // und Login-Event (NotifyUserAuthentication) garantiert dieselben Claims liefern.
    private static ClaimsIdentity BuildIdentity(string email, string id) =>
        new(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, id),
            new Claim("sub", id),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, email)
        }, "SupabaseAuth");
}