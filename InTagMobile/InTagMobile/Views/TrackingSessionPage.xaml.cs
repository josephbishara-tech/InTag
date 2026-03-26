using InTagMobile.Models;
using InTagMobile.Services;
using ZXing.Net.Maui;

namespace InTagMobile.Views;

[QueryProperty(nameof(RequestId), "requestId")]
public partial class TrackingSessionPage : ContentPage
{
    private readonly IApiService _api;
    private int _requestId;
    private List<TrackingLineDto> _pendingAssets = new();
    private int _scannedCount;
    private int _totalAssets;
    private bool _isProcessing;

    public int RequestId
    {
        get => _requestId;
        set { _requestId = value; _ = LoadAsync(); }
    }

    public TrackingSessionPage(IApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    private async Task LoadAsync()
    {
        try
        {
            var detail = await _api.GetRequestAsync(_requestId);
            if (detail == null)
            {
                await DisplayAlert("Error", "Request not found", "OK");
                return;
            }

            Title = detail.RequestNumber;
            _totalAssets = detail.TotalAssets;
            _pendingAssets = detail.Lines.Where(l => !l.IsScanned).ToList();
            _scannedCount = detail.Lines.Count(l => l.IsScanned);

            PendingList.ItemsSource = _pendingAssets;
            UpdateStats();

            if (detail.StatusDisplay == "Open")
                await _api.StartRequestAsync(_requestId);

            BarcodeReader.IsDetecting = true;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private void UpdateStats()
    {
        ScannedLabel.Text = _scannedCount.ToString();
        PendingLabel.Text = _pendingAssets.Count.ToString();
        var pct = _totalAssets > 0 ? Math.Round(_scannedCount * 100.0 / _totalAssets, 1) : 0;
        ProgressLabel.Text = $"{pct}%";
    }

    private async void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        if (_isProcessing) return;
        var first = e.Results?.FirstOrDefault();
        if (first == null) return;

        _isProcessing = true;
        BarcodeReader.IsDetecting = false;

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await ProcessScanAsync(first.Value);
            await Task.Delay(1500);
            BarcodeReader.IsDetecting = true;
            _isProcessing = false;
        });
    }

    private async Task ProcessScanAsync(string scannedCode)
    {
        var match = _pendingAssets.FirstOrDefault(a =>
            a.AssetCode.Equals(scannedCode, StringComparison.OrdinalIgnoreCase));

        if (match == null)
        {
            LastScanLabel.Text = $"No match: {scannedCode}";
            LastScanLabel.TextColor = Colors.Orange;
            return;
        }

        double? lat = null, lng = null;
        try
        {
            var location = await Geolocation.GetLastKnownLocationAsync();
            if (location != null) { lat = location.Latitude; lng = location.Longitude; }
        }
        catch { }

        var scan = new ScanSubmitDto
        {
            TrackingLineId = match.Id,
            Status = 1,
            ScannedCode = scannedCode,
            Latitude = lat,
            Longitude = lng,
            ConditionAtScan = 4
        };

        var success = await _api.SubmitScanAsync(scan);
        if (success)
        {
            _pendingAssets.Remove(match);
            _scannedCount++;
            PendingList.ItemsSource = null;
            PendingList.ItemsSource = _pendingAssets;
            UpdateStats();
            LastScanLabel.Text = $"✓ {match.AssetCode} — {match.AssetName}";
            LastScanLabel.TextColor = Colors.Green;
        }
        else
        {
            LastScanLabel.Text = $"✗ Failed: {scannedCode}";
            LastScanLabel.TextColor = Colors.Red;
        }
    }

    private void OnToggleScanner(object? sender, EventArgs e)
    {
        BarcodeReader.IsDetecting = !BarcodeReader.IsDetecting;
    }

    private async void OnCompleteClicked(object? sender, EventArgs e)
    {
        var confirm = await DisplayAlert("Complete",
            $"Mark {_pendingAssets.Count} unscanned assets as missing?", "Yes", "No");
        if (!confirm) return;

        await _api.CompleteRequestAsync(_requestId);
        await Shell.Current.GoToAsync("..");
    }
}
