using SQLite;

namespace StudentCabinetMVVM.Data;

[Table("SyncQueue")]
public class SyncQueueItem
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string EntityType { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}