#if ANDROID
using Android.Content;
using Application = Android.App.Application;

namespace InTagMobile.Services;

public class ZebraScannerService : IZebraScannerService
{
    public event EventHandler<string>? BarcodeScanned;

    private const string IntentAction = "com.intag.mobile.SCAN";
    private const string IntentExtraData = "com.symbol.datawedge.data_string";
    private const string IntentExtraSource = "com.symbol.datawedge.source";
    private const string IntentExtraLabelType = "com.symbol.datawedge.label_type";

    private const string DataWedgeApiAction = "com.symbol.datawedge.api.ACTION";
    private const string DataWedgeSoftScanTrigger = "com.symbol.datawedge.api.SOFT_SCAN_TRIGGER";
    private const string DataWedgeCreateProfile = "com.symbol.datawedge.api.CREATE_PROFILE";
    private const string DataWedgeSetConfig = "com.symbol.datawedge.api.SET_CONFIG";

    private ScanBroadcastReceiver? _receiver;

    public void StartListening()
    {
        if (_receiver != null) return;

        _receiver = new ScanBroadcastReceiver();
        _receiver.OnBarcodeReceived += (_, barcode) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                BarcodeScanned?.Invoke(this, barcode);
            });
        };

        var filter = new IntentFilter();
        filter.AddAction(IntentAction);
        filter.AddCategory(Intent.CategoryDefault);

#if NET10_0_OR_GREATER
        Application.Context.RegisterReceiver(_receiver, filter, ReceiverFlags.Exported);
#else
        Application.Context.RegisterReceiver(_receiver, filter);
#endif

        System.Diagnostics.Debug.WriteLine("[Zebra] Broadcast receiver registered");
    }

    public void StopListening()
    {
        if (_receiver != null)
        {
            Application.Context.UnregisterReceiver(_receiver);
            _receiver = null;
            System.Diagnostics.Debug.WriteLine("[Zebra] Broadcast receiver unregistered");
        }
    }

    public void ConfigureDataWedge(string profileName)
    {
        try
        {
            // Step 1: Create profile
            var createIntent = new Intent();
            createIntent.SetAction(DataWedgeApiAction);
            createIntent.PutExtra(DataWedgeCreateProfile, profileName);
            Application.Context.SendBroadcast(createIntent);

            // Step 2: Configure profile
            var configBundle = new Android.OS.Bundle();
            configBundle.PutString("PROFILE_NAME", profileName);
            configBundle.PutString("PROFILE_ENABLED", "true");
            configBundle.PutString("CONFIG_MODE", "UPDATE");

            // Associate with our app
            var appBundle = new Android.OS.Bundle();
            appBundle.PutString("PACKAGE_NAME", Application.Context.PackageName!);
            appBundle.PutStringArray("ACTIVITY_LIST", new[] { "*" });
            configBundle.PutParcelableArray("APP_LIST", new Android.OS.IParcelable[] { appBundle });

            // Configure barcode input plugin
            var barcodeBundle = new Android.OS.Bundle();
            barcodeBundle.PutString("PLUGIN_NAME", "BARCODE");
            barcodeBundle.PutString("RESET_CONFIG", "true");

            var barcodeProps = new Android.OS.Bundle();
            barcodeProps.PutString("scanner_input_enabled", "true");
            barcodeProps.PutString("scanner_selection", "auto");
            barcodeBundle.PutBundle("PARAM_LIST", barcodeProps);
            configBundle.PutBundle("PLUGIN_CONFIG", barcodeBundle);

            var configIntent = new Intent();
            configIntent.SetAction(DataWedgeApiAction);
            configIntent.PutExtra(DataWedgeSetConfig, configBundle);
            Application.Context.SendBroadcast(configIntent);

            // Step 3: Configure intent output
            var intentBundle = new Android.OS.Bundle();
            intentBundle.PutString("PLUGIN_NAME", "INTENT");
            intentBundle.PutString("RESET_CONFIG", "true");

            var intentProps = new Android.OS.Bundle();
            intentProps.PutString("intent_output_enabled", "true");
            intentProps.PutString("intent_action", IntentAction);
            intentProps.PutString("intent_delivery", "2"); // 2 = Broadcast
            intentBundle.PutBundle("PARAM_LIST", intentProps);

            var intentConfigBundle = new Android.OS.Bundle();
            intentConfigBundle.PutString("PROFILE_NAME", profileName);
            intentConfigBundle.PutString("PROFILE_ENABLED", "true");
            intentConfigBundle.PutString("CONFIG_MODE", "UPDATE");
            intentConfigBundle.PutBundle("PLUGIN_CONFIG", intentBundle);

            // Disable keystroke output (we use intent instead)
            var keystrokeBundle = new Android.OS.Bundle();
            keystrokeBundle.PutString("PLUGIN_NAME", "KEYSTROKE");
            keystrokeBundle.PutString("RESET_CONFIG", "true");

            var keystrokeProps = new Android.OS.Bundle();
            keystrokeProps.PutString("keystroke_output_enabled", "false");
            keystrokeBundle.PutBundle("PARAM_LIST", keystrokeProps);

            var keystrokeConfigBundle = new Android.OS.Bundle();
            keystrokeConfigBundle.PutString("PROFILE_NAME", profileName);
            keystrokeConfigBundle.PutString("PROFILE_ENABLED", "true");
            keystrokeConfigBundle.PutString("CONFIG_MODE", "UPDATE");
            keystrokeConfigBundle.PutBundle("PLUGIN_CONFIG", keystrokeBundle);

            var intentConfigIntent = new Intent();
            intentConfigIntent.SetAction(DataWedgeApiAction);
            intentConfigIntent.PutExtra(DataWedgeSetConfig, intentConfigBundle);
            Application.Context.SendBroadcast(intentConfigIntent);

            var keystrokeConfigIntent = new Intent();
            keystrokeConfigIntent.SetAction(DataWedgeApiAction);
            keystrokeConfigIntent.PutExtra(DataWedgeSetConfig, keystrokeConfigBundle);
            Application.Context.SendBroadcast(keystrokeConfigIntent);

            System.Diagnostics.Debug.WriteLine($"[Zebra] DataWedge profile '{profileName}' configured");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Zebra] DataWedge config error: {ex.Message}");
        }
    }

    // Inner class: BroadcastReceiver
    [BroadcastReceiver(Enabled = true, Exported = true)]
    private class ScanBroadcastReceiver : BroadcastReceiver
    {
        public event EventHandler<string>? OnBarcodeReceived;

        public override void OnReceive(Context? context, Intent? intent)
        {
            if (intent == null) return;

            var barcode = intent.GetStringExtra(IntentExtraData);
            var source = intent.GetStringExtra(IntentExtraSource);
            var labelType = intent.GetStringExtra(IntentExtraLabelType);

            System.Diagnostics.Debug.WriteLine($"[Zebra] Scan received: '{barcode}' source={source} type={labelType}");

            if (!string.IsNullOrEmpty(barcode))
            {
                OnBarcodeReceived?.Invoke(this, barcode.Trim());
            }
        }
    }
}
#endif
