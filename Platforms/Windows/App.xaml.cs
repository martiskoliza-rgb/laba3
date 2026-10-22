using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using StudentCabinetMVVM.Services;

namespace StudentCabinetMVVM.WinUI;

public partial class App : MauiWinUIApplication
{
    public App()
    {
        this.InitializeComponent();
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        var activation = AppInstance.GetCurrent().GetActivatedEventArgs();
        if (activation.Kind == ExtendedActivationKind.Protocol
            && activation.Data is global::Windows.ApplicationModel.Activation.IProtocolActivatedEventArgs protocol)
        {
            var state = IPlatformApplication.Current?.Services.GetService<IAppStateService>();
            if (state is not null)
                state.PendingDeepLink = protocol.Uri.AbsoluteUri;
        }
    }
}