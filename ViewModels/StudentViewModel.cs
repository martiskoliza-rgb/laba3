using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using StudentCabinetMVVM.Models;
using StudentCabinetMVVM.Services;

namespace StudentCabinetMVVM.ViewModels;

public class StudentViewModel : INotifyPropertyChanged, IQueryAttributable
{
    private readonly IAppStateService _state;
    private Student _student = new();
    private bool _isLoading;

    public string FullName
    {
        get => _student.FullName;
        set
        {
            if (_student.FullName != value)
            {
                _student.FullName = value;
                _state.DraftFullName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Greeting));
                ((Command)AddStudentCommand).ChangeCanExecute();
            }
        }
    }

    public string Group
    {
        get => _student.Group;
        set
        {
            if (_student.Group != value)
            {
                _student.Group = value;
                _state.DraftGroup = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Greeting));
            }
        }
    }

    public double AverageScore
    {
        get => _student.AverageScore;
        set
        {
            if (_student.AverageScore != value)
            {
                _student.AverageScore = value;
                _state.DraftAverageScore = value;
                OnPropertyChanged();
            }
        }
    }

    public string Greeting => $"Студент: {FullName}, група {Group}";
    public ObservableCollection<Student> Students { get; } = new();

    public ICommand AddStudentCommand { get; }

    public ICommand OpenDetailsCommand { get; }

    public ICommand LoadCommand { get; }

    public ICommand OpenSettingsCommand { get; }

    public StudentViewModel(IAppStateService state)
    {
        _state = state;

        AddStudentCommand = new Command(AddStudent, CanAddStudent);
        OpenDetailsCommand = new Command<Student>(async (student) =>
            await OpenDetailsAsync(student));
        LoadCommand = new Command(async () =>
        {
            Load();
            await HandleDeepLinkAsync();
        });
        OpenSettingsCommand = new Command(async () =>
            await Shell.Current.GoToAsync("settings"));

        Students.CollectionChanged += (_, _) =>
        {
            if (!_isLoading)
                _state.Students = Students.ToList();
        };

        _state.StateRestored += (_, _) => Load();
        _state.DeepLinkReceived += async (_, _) => await HandleDeepLinkAsync();

        Load();
    }

    private void Load()
    {
        _isLoading = true;
        try
        {
            if (Students.Count == 0)
            {
                foreach (var student in _state.Students)
                    Students.Add(student);
            }

            FullName = _state.DraftFullName;
            Group = _state.DraftGroup;
            AverageScore = _state.DraftAverageScore;
        }
        finally
        {
            _isLoading = false;
        }
    }
    private void AddStudent()
    {
        Students.Add(new Student
        {
            FullName = FullName,
            Group = Group,
            AverageScore = AverageScore
        });

        FullName = string.Empty;
        Group = string.Empty;
    }

    private bool CanAddStudent() => !string.IsNullOrWhiteSpace(FullName);

    private async Task OpenDetailsAsync(Student student)
    {
        if (student is null) return;

        var parameters = new Dictionary<string, object>
        {
            { "SelectedStudent", student }
        };
        await Shell.Current.GoToAsync("studentdetail", parameters);
    }

    private async Task HandleDeepLinkAsync()
    {
        var link = _state.PendingDeepLink;
        if (link is null || Shell.Current?.CurrentPage is null) return;

        _state.PendingDeepLink = null;
        Debug.WriteLine($"[DeepLink] Отримано: {link}");

        if (!Uri.TryCreate(link, UriKind.Absolute, out var uri)
            || uri.Scheme != "studentcabinet"
            || uri.Host != "student")
        {
            Debug.WriteLine("[DeepLink] Невідомий формат посилання");
            return;
        }

        var name = Uri.UnescapeDataString(uri.AbsolutePath.Trim('/'));
        var student = Students.FirstOrDefault(s =>
            string.Equals(s.FullName, name, StringComparison.OrdinalIgnoreCase));

        if (student is null)
        {
            Debug.WriteLine($"[DeepLink] Студента не знайдено: {name}");
            return;
        }

        try
        {
            await Task.Delay(300);
            await OpenDetailsAsync(student);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[DeepLink] Помилка навігації: {ex.Message}");
        }
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("UpdatedStudent", out var value)
            && value is Student updated)
        {
            var index = Students.IndexOf(updated);
            if (index >= 0)
            {
                Students.RemoveAt(index);
                Students.Insert(index, updated);
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}