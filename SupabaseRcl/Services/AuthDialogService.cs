namespace SupabaseRcl.Services;

public enum AuthDialogView { Login, Register }

public class AuthDialogService
{
    public event Action? Changed;

    public bool IsVisible { get; private set; }
    public AuthDialogView CurrentView { get; private set; } = AuthDialogView.Login;

    public void ShowLogin()    => Show(AuthDialogView.Login);
    public void ShowRegister() => Show(AuthDialogView.Register);

    public void SwitchView(AuthDialogView view)
    {
        CurrentView = view;
        Changed?.Invoke();
    }

    public void Close()
    {
        IsVisible = false;
        Changed?.Invoke();
    }

    private void Show(AuthDialogView view)
    {
        CurrentView = view;
        IsVisible = true;
        Changed?.Invoke();
    }
}
