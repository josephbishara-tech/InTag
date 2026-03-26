namespace InTagMobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("session", typeof(Views.TrackingSessionPage));
    }
}
