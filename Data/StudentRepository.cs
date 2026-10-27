using System.Diagnostics;
using System.Text.Json;
using SQLite;
using StudentCabinetMVVM.Models;
using StudentCabinetMVVM.Services;

namespace StudentCabinetMVVM.Data;

public class StudentRepository : IStudentRepository
{
    private readonly AppDatabase _database;
    private readonly IEncryptionService _encryption;

    public StudentRepository(AppDatabase database, IEncryptionService encryption)
    {
        _database = database;
        _encryption = encryption;
    }

    public async Task<List<Student>> GetAllAsync()
    {
        var db = await _database.GetConnectionAsync();
        var records = await db.Table<StudentRecord>().ToListAsync();

        var students = new List<Student>();
        foreach (var record in records)
            students.Add(await ToStudentAsync(record));

        return students;
    }

    public async Task<Student?> GetByIdAsync(int id)
    {
        var db = await _database.GetConnectionAsync();
        var record = await db.FindAsync<StudentRecord>(id);

        return record is null ? null : await ToStudentAsync(record);
    }

    public async Task<int> SaveAsync(Student student)
    {
        var db = await _database.GetConnectionAsync();

        var record = new StudentRecord
        {
            Id = student.Id,
            FullNameEncrypted = await _encryption.EncryptAsync(student.FullName),
            Group = student.Group,
            AverageScore = student.AverageScore,
            UpdatedAt = DateTime.UtcNow
        };

        var isNew = record.Id == 0;
        var rows = isNew
            ? await db.InsertAsync(record)
            : await db.UpdateAsync(record);

        student.Id = record.Id;

        Debug.WriteLine($"[DB] {(isNew ? "INSERT" : "UPDATE")} Id={record.Id}; ПІБ у БД (зашифровано): {record.FullNameEncrypted}");

        await EnqueueAsync(db, isNew ? "Insert" : "Update", JsonSerializer.Serialize(record));

        return rows;
    }

    public async Task<int> DeleteAsync(Student student)
    {
        var db = await _database.GetConnectionAsync();
        var rows = await db.DeleteAsync<StudentRecord>(student.Id);

        Debug.WriteLine($"[DB] DELETE Id={student.Id}");

        var payload = JsonSerializer.Serialize(new { student.Id, UpdatedAt = DateTime.UtcNow });
        await EnqueueAsync(db, "Delete", payload);

        return rows;
    }

    private async Task<Student> ToStudentAsync(StudentRecord record) => new()
    {
        Id = record.Id,
        FullName = await _encryption.DecryptAsync(record.FullNameEncrypted),
        Group = record.Group,
        AverageScore = record.AverageScore
    };

    private static Task EnqueueAsync(SQLiteAsyncConnection db, string operation, string payloadJson) =>
        db.InsertAsync(new SyncQueueItem
        {
            EntityType = "Student",
            Operation = operation,
            PayloadJson = payloadJson,
            CreatedAt = DateTime.UtcNow
        });
}