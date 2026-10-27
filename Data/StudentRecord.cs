using SQLite;

namespace StudentCabinetMVVM.Data;

[Table("Students")]
public class StudentRecord
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string FullNameEncrypted { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public double AverageScore { get; set; }
    public DateTime UpdatedAt { get; set; }
}