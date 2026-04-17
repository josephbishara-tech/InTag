using System;
using System.Collections.Generic;
using System.Text;

namespace InTagMobile.Services
{
    public interface IZebraScannerService
    {
        event EventHandler<string>? BarcodeScanned;
        void StartListening();
        void StopListening();
        void ConfigureDataWedge(string profileName);
    }
}
