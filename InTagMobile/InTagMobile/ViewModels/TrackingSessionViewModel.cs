using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InTagMobile.Models;
using InTagMobile.Services;

namespace InTagMobile.ViewModels;

[QueryProperty(nameof(RequestId), "requestId")]
public partial class TrackingSessionViewModel : BaseViewModel
{
    private readonly IApiService _api;

    [ObservableProperty] private int _requestId;
    [ObservableProperty] private string _requestNumber = string.Empty;
    [ObservableProperty] private string _locationName = string.Empty;
    [ObservableProperty] private int _totalAssets;
    [ObservableProperty] private int _scannedCount;
    [ObservableProperty] private decimal _progressPercent;
    [ObservableProperty] private string _lastScanResult = string.Empty;
    [ObservableProperty] private bool _isScanning;

    public ObservableCollection<TrackingLineDto> PendingAssets { get; } = new();
    public ObservableCollection<TrackingLineDto> ScannedAssets { get; } = new();

    public TrackingSessionViewModel(IApiService api)
    {
        _api = api;
        Title = "Scanning";
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var detail = await _api.GetRequestAsync(RequestId);
            if (detail == null)
            {
                await Shell.Current.DisplayAlert("Error", "Request not found", "OK");
                return;
            }

            RequestNumber = detail.RequestNumber;
            LocationName = detail.LocationName;
            TotalAssets = detail.TotalAssets;
            ProgressPercent = detail.ProgressPercent;

            PendingAssets.Clear();
            ScannedAssets.Clear();

            foreach (var line in detail.Lines)
            {
                if (line.IsScanned)
                    ScannedAssets.Add(line);
                else
                    PendingAssets.Add(line);
            }
            ScannedCount = ScannedAssets.Count;

            if (detail.StatusDisplay == "Open")
                await _api.StartRequestAsync(RequestId);
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
    private async Task ProcessScanAsync(string? scannedCode)
    {
        if (string.IsNullOrEmpty(scannedCode)) return;

        var match = PendingAssets.FirstOrDefault(a =>
            a.AssetCode.Equals(scannedCode, StringComparison.OrdinalIgnoreCase));

        if (match == null)
        {
            LastScanResult = $"No pending asset matches: {scannedCode}";
            return;
        }

        double? lat = null, lng = null;
        try
        {
            var location = await Geolocation.GetLastKnownLocationAsync()
                ?? await Geolocation.GetLocationAsync(
                    new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(5)));
            if (location != null)
            {
                lat = location.Latitude;
                lng = location.Longitude;
            }
        }
        catch { /* GPS not available */ }

        var scan = new ScanSubmitDto
        {
            TrackingLineId = match.Id,
            Status = 1, // Found
            ScannedCode = scannedCode,
            Latitude = lat,
            Longitude = lng,
            ConditionAtScan = 4 // Good
        };

        var success = await _api.SubmitScanAsync(scan);
        if (success)
        {
            PendingAssets.Remove(match);
            ScannedAssets.Insert(0, match);
            ScannedCount = ScannedAssets.Count;
            ProgressPercent = TotalAssets > 0
                ? Math.Round(ScannedCount * 100m / TotalAssets, 1) : 0;
            LastScanResult = $"✓ {match.AssetCode} — {match.AssetName}";
        }
        else
        {
            LastScanResult = $"✗ Failed to submit: {scannedCode}";
        }
    }

    [RelayCommand]
    private async Task CompleteAsync()
    {
        var confirm = await Shell.Current.DisplayAlert(
            "Complete Tracking",
            $"Mark {PendingAssets.Count} unscanned assets as missing?",
            "Yes", "No");

        if (!confirm) return;

        await _api.CompleteRequestAsync(RequestId);
        await Shell.Current.GoToAsync("..");
    }
}
