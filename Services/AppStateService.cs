using System.Text.Json;
using Microsoft.Maui.Storage;
using StudentCabinetMVVM.Models;

namespace StudentCabinetMVVM.Services;

public class AppStateService : IAppStateService
{
    private const string DraftFullNameKey = "draft_fullname";
    private const string DraftGroupKey = "draft_group";
    private const string DraftScoreKey = "draft_score";
    private const string StudentsKey = "students_json";

    private string? _pendingDeepLink;

    public DateTime SessionStartedAt { get; } = DateTime.Now;

    public string DraftFullName { get; set; } = string.Empty;
    public string DraftGroup { get; set; } = string.Empty;
    public double DraftAverageScore { get; set; }

    public List<Student> Students { get; set; } = new();

    public string? PendingDeepLink
    {
        get => _pendingDeepLink;
        set
        {
            _pendingDeepLink = value;
            if (value is not null)
                DeepLinkReceived?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? StateRestored;
    public event EventHandler? DeepLinkReceived;

    public void Save()
    {
        Preferences.Set(DraftFullNameKey, DraftFullName);
        Preferences.Set(DraftGroupKey, DraftGroup);
        Preferences.Set(DraftScoreKey, DraftAverageScore);
        Preferences.Set(StudentsKey, JsonSerializer.Serialize(Students));
    }

    public void Restore()
    {
        DraftFullName = Preferences.Get(DraftFullNameKey, string.Empty);
        DraftGroup = Preferences.Get(DraftGroupKey, string.Empty);
        DraftAverageScore = Preferences.Get(DraftScoreKey, 0.0);

        var json = Preferences.Get(StudentsKey, string.Empty);
        Students = string.IsNullOrEmpty(json)
            ? new List<Student>()
            : JsonSerializer.Deserialize<List<Student>>(json) ?? new List<Student>();

        StateRestored?.Invoke(this, EventArgs.Empty);
    }
}