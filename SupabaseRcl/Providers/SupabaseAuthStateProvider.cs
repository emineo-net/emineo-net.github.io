using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using SupabaseRcl.Services;

namespace SupabaseRcl.Providers;

public class SupabaseAuthStateProvider : AuthenticationStateProvider
{
    private readonly SupabaseService _supabaseService;
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public SupabaseAuthStateProvider(SupabaseService supabaseService)
    {
        _supabaseService = supabaseService;
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

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Email)
            }, "SupabaseAuth");

            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            return new AuthenticationState(_anonymous);
        }
    }

    // ERWEITERT: Übergreife hier auch die ID für den NameIdentifier Claim
    public void NotifyUserAuthentication(string email, string id)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, id), // Wichtig für die eindeutige Identifizierung im Code
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, email)
        }, "SupabaseAuth");

        var user = new ClaimsPrincipal(identity);
        var state = Task.FromResult(new AuthenticationState(user));
        NotifyAuthenticationStateChanged(state);
    }

    public void NotifyUserLogout()
    {
        var state = Task.FromResult(new AuthenticationState(_anonymous));
        NotifyAuthenticationStateChanged(state);
    }
}
