using InTagMobile.Services;

namespace InTagMobile.Views;

public partial class LoginPage : ContentPage
{
    private readonly IApiService _api;

    public LoginPage(IApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim();
        var password = PasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ErrorLabel.Text = "Email and password are required.";
            return;
        }

        ErrorLabel.Text = string.Empty;
        LoginBtn.IsEnabled = false;
        LoadingIndicator.IsRunning = true;
        LoadingIndicator.IsVisible = true;

        try
        {
            var result = await _api.LoginAsync(email, password);
            if (result != null)
                await Shell.Current.GoToAsync("//requests");
            else
                ErrorLabel.Text = "Invalid email or password.";
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = $"Connection error: {ex.Message}";
        }
        finally
        {
            LoginBtn.IsEnabled = true;
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }
}
