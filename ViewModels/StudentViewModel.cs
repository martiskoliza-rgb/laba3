using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using StudentCabinetMVVM.Data;
using StudentCabinetMVVM.Models;
using StudentCabinetMVVM.Services;

namespace StudentCabinetMVVM.ViewModels;

public class StudentViewModel : INotifyPropertyChanged, IQueryAttributable
{
    private readonly IAppStateService _state;
    private readonly IStudentRepository _repository;
    private readonly ISyncService _sync;
    private readonly SemaphoreSlim _loadLock = new(1, 1);
    private Student _student = new();
    private bool _isLoaded;
    private int _pendingCount;
    private (int Id, double Score)? _lastSaved;

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

    public int PendingCount
    {
        get => _pendingCount;
        private set
        {
            if (_pendingCount != value)
            {
                _pendingCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PendingText));
            }
        }
    }

    public string PendingText => $"У черзі синхронізації: {PendingCount}";

    public ObservableCollection<Student> Students { get; } = new();

    public ICommand AddStudentCommand { get; }

    public ICommand OpenDetailsCommand { get; }

    public ICommand DeleteStudentCommand { get; }

    public ICommand LoadCommand { get; }

    public ICommand OpenSettingsCommand { get; }

    public ICommand SyncCommand { get; }

    public StudentViewModel(IAppStateService state, IStudentRepository repository, ISyncService sync)
    {
        _state = state;
        _repository = repository;
        _sync = sync;

        AddStudentCommand = new Command(async () => await AddStudentAsync(), CanAddStudent);
        OpenDetailsCommand = new Command<Student>(async (student) =>
            await OpenDetailsAsync(student));
        DeleteStudentCommand = new Command<Student>(async (student) =>
            await DeleteStudentAsync(student));
        LoadCommand = new Command(async () => await LoadAsync());
        OpenSettingsCommand = new Command(async () =>
            await Shell.Current.GoToAsync("settings"));
        SyncCommand = new Command(async () => await SyncNowAsync());

        _state.StateRestored += (_, _) => LoadDraft();
        _state.DeepLinkReceived += async (_, _) => await HandleDeepLinkAsync();
        _sync.QueueChanged += (_, _) =>
            MainThread.BeginInvokeOnMainThread(async () => await RefreshPendingAsync());

        LoadDraft();
    }

    private void LoadDraft()
    {
        FullName = _state.DraftFullName;
        Group = _state.DraftGroup;
        AverageScore = _state.DraftAverageScore;
    }

    private async Task LoadAsync()
    {
        await _loadLock.WaitAsync();
        try
        {
            if (!_isLoaded)
            {
                var students = await _repository.GetAllAsync();

                Students.Clear();
                foreach (var student in students)
                    Students.Add(student);

                _isLoaded = true;
            }
        }
        finally
        {
            _loadLock.Release();
        }

        await RefreshPendingAsync();
        await HandleDeepLinkAsync();
    }

    private bool CanAddStudent() => !string.IsNullOrWhiteSpace(FullName);

    private async Task AddStudentAsync()
    {
        var student = new Student
        {
            FullName = FullName,
            Group = Group,
            AverageScore = AverageScore
        };

        await _repository.SaveAsync(student);
        Students.Add(student);

        FullName = string.Empty;
        Group = string.Empty;

        await AfterChangeAsync();
    }

    private async Task DeleteStudentAsync(Student student)
    {
        if (student is null) return;

        await _repository.DeleteAsync(student);
        Students.Remove(student);

        await AfterChangeAsync();
    }

    private async Task OpenDetailsAsync(Student student)
    {
        if (student is null) return;

        var parameters = new Dictionary<string, object>
        {
            { "SelectedStudent", student }
        };
        await Shell.Current.GoToAsync("studentdetail", parameters);
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("UpdatedStudent", out var value)
            && value is Student updated)
        {
            _ = SaveUpdatedAsync(updated);
        }
    }

    private async Task SaveUpdatedAsync(Student updated)
    {
        if (_lastSaved == (updated.Id, updated.AverageScore))
            return;

        _lastSaved = (updated.Id, updated.AverageScore);
        await _repository.SaveAsync(updated);

        var index = Students.IndexOf(updated);
        if (index >= 0)
        {
            Students.RemoveAt(index);
            Students.Insert(index, updated);
        }

        await AfterChangeAsync();
    }

    private async Task AfterChangeAsync()
    {
        await RefreshPendingAsync();
        _ = _sync.SyncAsync();
    }

    private async Task RefreshPendingAsync()
    {
        PendingCount = await _sync.GetPendingCountAsync();
    }

    private async Task SyncNowAsync()
    {
        await _sync.SyncAsync();
        await RefreshPendingAsync();
    }

    private async Task HandleDeepLinkAsync()
    {
        var link = _state.PendingDeepLink;
        if (link is null || !_isLoaded || Shell.Current?.CurrentPage is null) return;

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

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
