namespace InTagMobile.Services;

/// <summary>
/// Fallback for non-Zebra / non-Android devices — does nothing
/// </summary>
public class FallbackScannerService : IZebraScannerService
{
    public event EventHandler<string>? BarcodeScanned;
    public void StartListening() { }
    public void StopListening() { }
    public void ConfigureDataWedge(string profileName) { }
}
