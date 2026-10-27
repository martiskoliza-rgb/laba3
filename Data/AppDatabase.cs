using SQLite;

namespace StudentCabinetMVVM.Data;

public class AppDatabase
{
    private readonly string _path;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private SQLiteAsyncConnection? _connection;

    public AppDatabase(string path)
    {
        _path = path;
    }

    public async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_connection is not null)
            return _connection;

        await _lock.WaitAsync();
        try
        {
            if (_connection is null)
            {
                var connection = new SQLiteAsyncConnection(_path);
                await connection.CreateTableAsync<StudentRecord>();
                await connection.CreateTableAsync<SyncQueueItem>();
                _connection = connection;
            }

            return _connection;
        }
        finally
        {
            _lock.Release();
        }
    }
}