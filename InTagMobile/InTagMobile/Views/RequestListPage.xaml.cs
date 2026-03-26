using InTagMobile.Models;
using InTagMobile.Services;

namespace InTagMobile.Views;

public partial class RequestListPage : ContentPage
{
    private readonly IApiService _api;

    public RequestListPage(IApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadRequestsAsync();
    }

    private async Task LoadRequestsAsync()
    {
        try
        {
            var items = await _api.GetRequestsAsync();
            RequestList.ItemsSource = items;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnRequestSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is TrackingRequestDto request)
        {
            RequestList.SelectedItem = null;
            await Shell.Current.GoToAsync($"session?requestId={request.Id}");
        }
    }
}
