using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SupabaseRcl.Services;
using SupabaseRcl.Providers;

namespace SupabaseRcl.Components;

public partial class Checkout : ComponentBase
{
    [Inject] private SupabaseService SupabaseService { get; set; } = null!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = null!;

    protected string Email { get; set; } = string.Empty;
    protected string Password { get; set; } = string.Empty;
    protected string ErrorMessage { get; set; } = string.Empty;
    protected bool IsRegisterMode { get; set; } = false;
    protected bool IsLoading { get; set; } = false;
    protected bool IsAuthenticated { get; set; } = false;
    protected string CurrentUserEmail { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        var client = SupabaseService.Client;
        if (client?.Auth.CurrentSession?.User != null)
        {
            IsAuthenticated = true;
            CurrentUserEmail = client.Auth.CurrentSession.User.Email ?? string.Empty;

            // Holt die ID des Benutzers aus der bestehenden Session und übergibt sie an den Provider
            var userId = client.Auth.CurrentSession.User.Id ?? string.Empty;
            ((SupabaseAuthStateProvider)AuthStateProvider).NotifyUserAuthentication(CurrentUserEmail, userId);
        }
    }

    protected void ToggleMode() => IsRegisterMode = !IsRegisterMode;

    protected async Task HandleSubmit()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Bitte fülle alle Felder aus.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            if (IsRegisterMode)
            {
                var signUpResult = await SupabaseService.Client.Auth.SignUp(Email, Password);
                if (signUpResult?.User != null)
                {
                    ErrorMessage = "Registrierung erfolgreich! Bitte E-Mails prüfen.";
                    IsRegisterMode = false;
                }
            }
            else
            {
                var session = await SupabaseService.Client.Auth.SignIn(Email, Password);
                if (session?.User != null)
                {
                    IsAuthenticated = true;
                    CurrentUserEmail = session.User.Email ?? string.Empty;

                    // Übergibt die Email UND die ID an den Provider
                    var userId = session.User.Id ?? string.Empty;
                    ((SupabaseAuthStateProvider)AuthStateProvider).NotifyUserAuthentication(CurrentUserEmail, userId);
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Fehler: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected async Task HandleLogout()
    {
        await SupabaseService.Client.Auth.SignOut();
        IsAuthenticated = false;
        CurrentUserEmail = string.Empty;

        // Blazor Bescheid geben!
        ((SupabaseAuthStateProvider)AuthStateProvider).NotifyUserLogout();
    }
}
