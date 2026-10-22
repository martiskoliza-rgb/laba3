using StudentCabinetMVVM.Models;

namespace StudentCabinetMVVM.Services;

public interface IAppStateService
{
    DateTime SessionStartedAt { get; }

    string DraftFullName { get; set; }
    string DraftGroup { get; set; }
    double DraftAverageScore { get; set; }

    List<Student> Students { get; set; }

    string? PendingDeepLink { get; set; }

    event EventHandler? StateRestored;
    event EventHandler? DeepLinkReceived;

    void Save();
    void Restore();
}