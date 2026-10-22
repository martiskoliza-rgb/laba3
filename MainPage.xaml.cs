using System.Diagnostics;
using StudentCabinetMVVM.ViewModels;

namespace StudentCabinetMVVM;

public partial class MainPage : ContentPage
{
    public MainPage(StudentViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Debug.WriteLine("[Lifecycle] MainPage.OnAppearing");

        if (BindingContext is StudentViewModel vm)
            vm.LoadCommand.Execute(null);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Debug.WriteLine("[Lifecycle] MainPage.OnDisappearing: перехід на іншу сторінку або згортання");
    }
}