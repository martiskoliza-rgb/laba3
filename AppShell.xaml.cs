using StudentCabinetMVVM.Views;

namespace StudentCabinetMVVM;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("studentdetail", typeof(StudentDetailPage));
        Routing.RegisterRoute("settings", typeof(SettingsPage));
    }
}