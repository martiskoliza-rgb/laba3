using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace StudentCabinetMVVM.ViewModels;

public class SettingsViewModel : INotifyPropertyChanged
{
    public const string ThemeKey = "app_theme";

    private bool _isDarkTheme;

    public SettingsViewModel()
    {
        var saved = Preferences.Get(ThemeKey, string.Empty);
        _isDarkTheme = saved == "Dark"
            || (saved == string.Empty && Application.Current?.RequestedTheme == AppTheme.Dark);

        GoBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
    }

    public bool IsDarkTheme
    {
        get => _isDarkTheme;
        set
        {
            if (_isDarkTheme != value)
            {
                _isDarkTheme = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ThemeName));
                ApplyTheme(value);
            }
        }
    }

    public string ThemeName => IsDarkTheme ? "Темна" : "Світла";

    public ICommand GoBackCommand { get; }

    private static void ApplyTheme(bool dark)
    {
        if (Application.Current is not null)
            Application.Current.UserAppTheme = dark ? AppTheme.Dark : AppTheme.Light;

        Preferences.Set(ThemeKey, dark ? "Dark" : "Light");
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}