using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using StudentCabinetMVVM.Models;

namespace StudentCabinetMVVM.ViewModels;

public class StudentDetailViewModel : IQueryAttributable, INotifyPropertyChanged
{
    private Student? _student;
    private double _averageScore;

    public string FullName { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;

    public double AverageScore
    {
        get => _averageScore;
        set
        {
            if (_averageScore != value)
            {
                _averageScore = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand GoBackCommand { get; }

    public ICommand SaveCommand { get; }

    public StudentDetailViewModel()
    {
        GoBackCommand = new Command(async () =>
            await Shell.Current.GoToAsync(".."));

        SaveCommand = new Command(async () => await SaveAsync());
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("SelectedStudent", out var value)
            && value is Student student)
        {
            _student = student;
            FullName = student.FullName;
            Group = student.Group;
            AverageScore = student.AverageScore;
            OnPropertyChanged(nameof(FullName));
            OnPropertyChanged(nameof(Group));
        }
    }

    private async Task SaveAsync()
    {
        if (_student is null)
            return;

        _student.AverageScore = AverageScore;

        var parameters = new ShellNavigationQueryParameters
        {
            { "UpdatedStudent", _student }
        };

        await Shell.Current.GoToAsync("..", parameters);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}