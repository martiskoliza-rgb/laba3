using System.Diagnostics;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using StudentCabinetMVVM.Services;
using StudentCabinetMVVM.ViewModels;

namespace StudentCabinetMVVM;

public partial class App : Application
{
    private readonly IAppStateService _state;

    public App(IAppStateService state)
    {
        InitializeComponent();

        _state = state;
        _state.Restore();

        ApplySavedTheme();
    }

    private void ApplySavedTheme()
    {
        var saved = Preferences.Get(SettingsViewModel.ThemeKey, string.Empty);

        if (saved == "Dark")
            UserAppTheme = AppTheme.Dark;
        else if (saved == "Light")
            UserAppTheme = AppTheme.Light;
    }

    protected override void OnAppLinkRequestReceived(Uri uri)
    {
        base.OnAppLinkRequestReceived(uri);
        _state.PendingDeepLink = uri.ToString();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());

        window.Stopped += (s, e) =>
        {
            Debug.WriteLine("[Lifecycle] App Stopped: збереження стану");
            _state.Save();
        };

        window.Resumed += (s, e) =>
        {
            Debug.WriteLine("[Lifecycle] App Resumed: відновлення стану");
            _state.Restore();
        };

        window.Destroying += (s, e) =>
        {
            Debug.WriteLine("[Lifecycle] App Destroying: звільнення ресурсів");
            _state.Save();
        };

        return window;
    }
}