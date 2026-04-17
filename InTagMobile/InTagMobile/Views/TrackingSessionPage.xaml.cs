using InTagMobile.Models;
using InTagMobile.Services;
using ZXing.Net.Maui;

namespace InTagMobile.Views;

[QueryProperty(nameof(RequestId), "requestId")]
public partial class TrackingSessionPage : ContentPage
{
    private readonly IApiService _api;
    private readonly IZebraScannerService _zebraScanner;
    private int _requestId;
    private List<TrackingLineDto> _pendingAssets = new();
    private int _scannedCount;
    private int _totalAssets;
    private bool _isProcessing;
    private bool _isLoaded;

    private enum ScanMode { Zebra, Camera, Manual }
    private ScanMode _currentMode = ScanMode.Zebra;

    public int RequestId
    {
        get => _requestId;
        set => _requestId = value;
    }

    public TrackingSessionPage(IApiService api, IZebraScannerService zebraScanner)
    {
        InitializeComponent();
        _api = api;
        _zebraScanner = zebraScanner;

        // Configure camera barcode reader
        BarcodeReader.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormats.All,
            AutoRotate = true,
            Multiple = false,
            TryHarder = true
        };

        // Subscribe to Zebra scanner events
        _zebraScanner.BarcodeScanned += OnZebraBarcodeScanned;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_isLoaded && _requestId > 0)
        {
            _isLoaded = true;

            // Configure DataWedge for our app
            _zebraScanner.ConfigureDataWedge("InTagMobile");
            _zebraScanner.StartListening();

            await LoadAsync();
            SetScanMode(ScanMode.Zebra);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _zebraScanner.StopListening();
        BarcodeReader.IsDetecting = false;
        _isLoaded = false;
    }

    // ═══════════════════════════════════════
    //  DATA LOADING
    // ═══════════════════════════════════════

    private async Task LoadAsync()
    {
        try
        {
            var detail = await _api.GetRequestAsync(_requestId);
            if (detail == null)
            {
                await DisplayAlert("Error", "Request not found", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            Title = $"{detail.RequestNumber} — {detail.LocationName}";
            _totalAssets = detail.TotalAssets;
            _pendingAssets = detail.Lines?.Where(l => !l.IsScanned).ToList() ?? new();
            _scannedCount = detail.Lines?.Count(l => l.IsScanned) ?? 0;

            PendingList.ItemsSource = _pendingAssets;
            PendingHeader.Text = $"Pending Assets ({_pendingAssets.Count})";
            UpdateStats();

            if (detail.StatusDisplay == "Open")
                await _api.StartRequestAsync(_requestId);
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

    // ═══════════════════════════════════════
    //  SCAN MODE SWITCHING
    // ═══════════════════════════════════════

    private void SetScanMode(ScanMode mode)
    {
        _currentMode = mode;

        // Reset all button colors
        BtnZebra.BackgroundColor = Color.FromArgb("#6C757D");
        BtnCamera.BackgroundColor = Color.FromArgb("#6C757D");
        BtnManual.BackgroundColor = Color.FromArgb("#6C757D");

        // Stop camera if switching away
        BarcodeReader.IsDetecting = false;
        BarcodeReader.IsVisible = false;
        ZebraScannerPanel.IsVisible = false;

        switch (mode)
        {
            case ScanMode.Zebra:
                BtnZebra.BackgroundColor = Color.FromArgb("#28A745");
                ZebraScannerPanel.IsVisible = true;
                ScannerStatusLabel.Text = "Press hardware trigger to scan";
                _zebraScanner.StartListening();
                LastScanLabel.Text = "🔫 Zebra scanner ready — press trigger";
                LastScanLabel.TextColor = Colors.Green;
                break;

            case ScanMode.Camera:
                BtnCamera.BackgroundColor = Color.FromArgb("#17A2B8");
                BarcodeReader.IsVisible = true;
                BarcodeReader.IsDetecting = true;
                LastScanLabel.Text = "📷 Point camera at barcode";
                LastScanLabel.TextColor = Colors.Green;
                break;

            case ScanMode.Manual:
                BtnManual.BackgroundColor = Color.FromArgb("#FFC107");
                ZebraScannerPanel.IsVisible = true;
                ScannerStatusLabel.Text = "Enter code manually below";
                break;
        }
    }

    private void OnZebraMode(object? sender, EventArgs e) => SetScanMode(ScanMode.Zebra);

    private async void OnCameraMode(object? sender, EventArgs e)
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Permission", "Camera permission required", "OK");
                return;
            }
        }
        SetScanMode(ScanMode.Camera);
    }

    private async void OnManualEntry(object? sender, EventArgs e)
    {
        SetScanMode(ScanMode.Manual);

        var code = await DisplayPromptAsync("Manual Entry",
            "Enter asset code:", "Submit", "Cancel",
            placeholder: "e.g. AST-001");

        if (!string.IsNullOrWhiteSpace(code))
            await ProcessScanAsync(code.Trim());
    }

    // ═══════════════════════════════════════
    //  SCAN HANDLERS
    // ═══════════════════════════════════════

    /// <summary>
    /// Zebra hardware scanner callback
    /// </summary>
    private async void OnZebraBarcodeScanned(object? sender, string barcode)
    {
        if (_isProcessing || string.IsNullOrEmpty(barcode)) return;
        _isProcessing = true;

        System.Diagnostics.Debug.WriteLine($"[Zebra] Hardware scan: '{barcode}'");

        try
        {
            // Vibrate to confirm receipt
            try { Vibration.Vibrate(TimeSpan.FromMilliseconds(100)); } catch { }

            ScannerStatusLabel.Text = $"Processing: {barcode}";
            await ProcessScanAsync(barcode);
        }
        finally
        {
            await Task.Delay(500);
            _isProcessing = false;
            ScannerStatusLabel.Text = "Press hardware trigger to scan";
        }
    }

    /// <summary>
    /// Camera scanner callback
    /// </summary>
    private void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        if (_isProcessing) return;
        var first = e.Results?.FirstOrDefault();
        if (first == null || string.IsNullOrEmpty(first.Value)) return;

        _isProcessing = true;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            BarcodeReader.IsDetecting = false;
            try
            {
                try { Vibration.Vibrate(TimeSpan.FromMilliseconds(100)); } catch { }
                await ProcessScanAsync(first.Value);
            }
            finally
            {
                await Task.Delay(2000);
                BarcodeReader.IsDetecting = true;
                _isProcessing = false;
            }
        });
    }

    // ═══════════════════════════════════════
    //  SCAN PROCESSING (shared by all modes)
    // ═══════════════════════════════════════

    private async Task ProcessScanAsync(string scannedCode)
    {
        System.Diagnostics.Debug.WriteLine($"[Scan] Processing: '{scannedCode}'");

        // Match by: exact barcode, exact asset code, exact serial number, then partial
        var match = _pendingAssets.FirstOrDefault(a =>
            // Exact matches
            (!string.IsNullOrEmpty(a.Barcode) && a.Barcode.Equals(scannedCode, StringComparison.OrdinalIgnoreCase))
            || a.AssetCode.Equals(scannedCode, StringComparison.OrdinalIgnoreCase)
            || (!string.IsNullOrEmpty(a.SerialNumber) && a.SerialNumber.Equals(scannedCode, StringComparison.OrdinalIgnoreCase))
        );

        // Partial match fallback (barcode might have prefix/suffix)
        if (match == null)
        {
            match = _pendingAssets.FirstOrDefault(a =>
                (!string.IsNullOrEmpty(a.Barcode) && (
                    scannedCode.Contains(a.Barcode, StringComparison.OrdinalIgnoreCase)
                    || a.Barcode.Contains(scannedCode, StringComparison.OrdinalIgnoreCase)))
                || scannedCode.Contains(a.AssetCode, StringComparison.OrdinalIgnoreCase)
                || a.AssetCode.Contains(scannedCode, StringComparison.OrdinalIgnoreCase)
            );
        }

        if (match == null)
        {
            LastScanLabel.Text = $"❌ No match: {scannedCode}";
            LastScanLabel.TextColor = Colors.Orange;

            // Show what we're looking for
            var sample = _pendingAssets.Take(3).Select(a =>
                $"{a.AssetCode}" + (string.IsNullOrEmpty(a.Barcode) ? "" : $" [{a.Barcode}]"));
            System.Diagnostics.Debug.WriteLine($"[Scan] Available: {string.Join(", ", sample)}");
            return;
        }

        // Found a match — show what matched
        var matchedBy = match.Barcode?.Equals(scannedCode, StringComparison.OrdinalIgnoreCase) == true
            ? "barcode" : match.AssetCode.Equals(scannedCode, StringComparison.OrdinalIgnoreCase)
            ? "asset code" : "partial match";

        LastScanLabel.Text = $"⏳ {match.AssetCode} (matched by {matchedBy})...";
        LastScanLabel.TextColor = Colors.Blue;

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
            Status = 1, // Found
            ScannedCode = scannedCode,
            Latitude = lat,
            Longitude = lng,
            ConditionAtScan = 4 // Good
        };

        var success = await _api.SubmitScanAsync(scan);
        if (success)
        {
            _pendingAssets.Remove(match);
            _scannedCount++;
            PendingList.ItemsSource = null;
            PendingList.ItemsSource = _pendingAssets;
            PendingHeader.Text = $"Pending Assets ({_pendingAssets.Count})";
            UpdateStats();
            LastScanLabel.Text = $"✅ {match.AssetCode} — {match.AssetName}";
            LastScanLabel.TextColor = Colors.Green;

            try { Vibration.Vibrate(TimeSpan.FromMilliseconds(200)); } catch { }

            if (_pendingAssets.Count == 0)
            {
                await Task.Delay(1000);
                var complete = await DisplayAlert("All Scanned",
                    "All assets found! Complete tracking?", "Yes", "Not Yet");
                if (complete)
                {
                    await _api.CompleteRequestAsync(_requestId);
                    await Shell.Current.GoToAsync("..");
                }
            }
        }
        else
        {
            LastScanLabel.Text = $"❌ Server error: {scannedCode}";
            LastScanLabel.TextColor = Colors.Red;
        }
    }

    private async void OnCompleteClicked(object? sender, EventArgs e)
    {
        BarcodeReader.IsDetecting = false;
        _zebraScanner.StopListening();

        var confirm = await DisplayAlert("Complete Tracking",
            $"Mark {_pendingAssets.Count} unscanned assets as missing?", "Yes", "No");

        if (!confirm)
        {
            if (_currentMode == ScanMode.Camera) BarcodeReader.IsDetecting = true;
            if (_currentMode == ScanMode.Zebra) _zebraScanner.StartListening();
            return;
        }

        var success = await _api.CompleteRequestAsync(_requestId);
        if (success)
            await Shell.Current.GoToAsync("..");
        else
            await DisplayAlert("Error", "Failed to complete", "OK");
    }
}
