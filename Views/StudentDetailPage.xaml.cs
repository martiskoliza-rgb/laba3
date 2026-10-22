using StudentCabinetMVVM.ViewModels;

namespace StudentCabinetMVVM.Views;

public partial class StudentDetailPage : ContentPage
{
    public StudentDetailPage()
    {
        InitializeComponent();
        BindingContext = new StudentDetailViewModel();
    }
}