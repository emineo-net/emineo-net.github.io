using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SupabaseRcl.Providers;
using SupabaseRcl.Services;

namespace SupabaseRcl.Components;

public partial class AuthDialog : ComponentBase, IDisposable
{
    [Inject] private AuthDialogService DialogService { get; set; } = null!;
    [Inject] private SupabaseService SupabaseService { get; set; } = null!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = null!;

    private LoginModel loginModel = new();
    private RegisterModel registerModel = new();

    private string? message;
    private bool isSuccess;
    private bool isSubmitting;

    protected override void OnInitialized() => DialogService.Changed += OnDialogChanged;

    private void OnDialogChanged()
    {
        message = null;
        isSuccess = false;
        isSubmitting = false;
        InvokeAsync(StateHasChanged);
    }

    private async Task LoginAsync()
    {
        isSubmitting = true;
        message = null;

        try
        {
            await SupabaseService.WaitForInitializationAsync();
            var session = await SupabaseService.Client.Auth.SignIn(loginModel.Email, loginModel.Password);

            if (session?.User != null)
            {
                var email = session.User.Email ?? string.Empty;
                var userId = session.User.Id ?? string.Empty;
                ((SupabaseAuthStateProvider)AuthStateProvider).NotifyUserAuthentication(email, userId);

                loginModel = new();
                DialogService.Close();
            }
            else
            {
                message = "Login fehlgeschlagen. Bitte überprüfe deine Daten.";
            }
        }
        catch (Exception ex)
        {
            message = $"Fehler: {ex.Message}";
        }
        finally
        {
            isSubmitting = false;
        }
    }

    private async Task RegisterAsync()
    {
        isSubmitting = true;
        message = null;

        try
        {
            await SupabaseService.WaitForInitializationAsync();
            var result = await SupabaseService.Client.Auth.SignUp(registerModel.Email, registerModel.Password);

            if (result?.User != null)
            {
                registerModel = new();
                DialogService.SwitchView(AuthDialogView.Login); // setzt message zurück ...
                message = "Registrierung erfolgreich! Bitte E-Mails prüfen."; // ... daher danach setzen
                isSuccess = true;
            }
            else
            {
                message = "Registrierung fehlgeschlagen.";
                isSuccess = false;
            }
        }
        catch (Exception ex)
        {
            message = $"Fehler: {ex.Message}";
            isSuccess = false;
        }
        finally
        {
            isSubmitting = false;
        }
    }

    public void Dispose() => DialogService.Changed -= OnDialogChanged;

    public sealed class LoginModel
    {
        [Required(ErrorMessage = "Bitte E-Mail eingeben.")]
        [EmailAddress(ErrorMessage = "Ungültige E-Mail-Adresse.")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Bitte Passwort eingeben.")]
        public string Password { get; set; } = "";
    }

    public sealed class RegisterModel
    {
        [Required(ErrorMessage = "Bitte E-Mail eingeben.")]
        [EmailAddress(ErrorMessage = "Ungültige E-Mail-Adresse.")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Bitte Passwort eingeben.")]
        [MinLength(6, ErrorMessage = "Das Passwort muss mindestens 6 Zeichen haben.")]
        public string Password { get; set; } = "";

        [Compare(nameof(Password), ErrorMessage = "Passwörter stimmen nicht überein.")]
        public string ConfirmPassword { get; set; } = "";
    }
}