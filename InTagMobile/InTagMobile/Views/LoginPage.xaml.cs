using InTagMobile.Services;

namespace InTagMobile.Views;

public partial class LoginPage : ContentPage
{
    private readonly IApiService _api;
    private bool _isBusy = false;

    public LoginPage(IApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        if (_isBusy) return; 
        _isBusy = true;

        try
        {
            var email = EmailEntry.Text?.Trim();
            var password = PasswordEntry.Text;

            ErrorLabel.IsVisible = false;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Email and password are required.");
                await ShakeView(EmailEntry);
                await ShakeView(PasswordEntry);
                return;
            }

            // Start Loading
            SetLoading(true);

            // Animation
            await LoginBtn.ScaleTo(0.95, 100);
            await LoginBtn.ScaleTo(1, 100);

            var result = await _api.LoginAsync(email, password);

            if (result?.AccessToken != null)
            {
                await Shell.Current.GoToAsync("//requests");
            }
            else
            {
                ShowError("Invalid email or password.");
                await ShakeView(PasswordEntry);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex); 
            ShowError("Connection error. Please try again.");
        }
        finally
        {
            SetLoading(false);
            _isBusy = false;
        }
    }

    private void SetLoading(bool isLoading)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
        LoginBtn.IsEnabled = !isLoading;
    }

    private async void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;

        ErrorLabel.Opacity = 0;
        await ErrorLabel.FadeTo(1, 300);
    }

    private async Task ShakeView(View view)
    {
        for (int i = 0; i < 3; i++)
        {
            await view.TranslateTo(-10, 0, 50);
            await view.TranslateTo(10, 0, 50);
        }
        await view.TranslateTo(0, 0, 50);
    }
}
