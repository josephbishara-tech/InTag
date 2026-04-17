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
            System.Diagnostics.Debug.WriteLine($"[RequestList] Loaded {items.Count} items");
            RequestList.ItemsSource = items;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RequestList] Error: {ex.Message}");
            await DisplayAlert("Error", ex.ToString(), "OK");
        }
    }

    private async void OnRequestTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is TrackingRequestDto request)
        {
            System.Diagnostics.Debug.WriteLine($"[RequestList] Tapped: {request.RequestNumber} ID={request.Id}");
            await Shell.Current.GoToAsync($"session?requestId={request.Id}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("[RequestList] Tapped but parameter is null or wrong type");
        }
    }

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        await LoadRequestsAsync();
        RefreshContainer.IsRefreshing = false;
    }
}
