using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StudentCabinetMVVM.Models;

public class Student : INotifyPropertyChanged
{
    public string FullName { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;

    private double _averageScore;

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

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
