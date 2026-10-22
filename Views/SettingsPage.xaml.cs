using StudentCabinetMVVM.ViewModels;

namespace StudentCabinetMVVM.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
        BindingContext = new SettingsViewModel();
    }
}