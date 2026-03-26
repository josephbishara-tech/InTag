using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using InTagMobile.Models;
using InTagMobile.Services;

namespace InTagMobile.ViewModels;

public partial class RequestListViewModel : BaseViewModel
{
    private readonly IApiService _api;

    public ObservableCollection<TrackingRequestDto> Requests { get; } = new();

    public RequestListViewModel(IApiService api)
    {
        _api = api;
        Title = "Tracking Requests";
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var items = await _api.GetRequestsAsync();
            Requests.Clear();
            foreach (var r in items)
                Requests.Add(r);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SelectAsync(TrackingRequestDto? request)
    {
        if (request == null) return;
        await Shell.Current.GoToAsync($"session?requestId={request.Id}");
    }
}
