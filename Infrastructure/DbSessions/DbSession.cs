using System.Data;
using MySqlConnector;

public sealed class DbSession : IAsyncDisposable
{
    private readonly MySqlConnection _connection;
    private MySqlTransaction? _transaction;

    public MySqlConnection Connection => _connection;
    public MySqlTransaction? Transaction => _transaction;

    public DbSession(string connectionString)
    {
        _connection = new MySqlConnection(connectionString);
    }

    public async Task OpenAsync()
    {
        if (_connection.State != ConnectionState.Open)
            await _connection.OpenAsync();
    }

    public async Task BeginTransactionAsync()
    {
        await OpenAsync();
        _transaction ??= (MySqlTransaction)await _connection.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        if (_transaction == null) return;
        await _transaction.CommitAsync();
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackAsync()
    {
        if (_transaction == null) return;
        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
        }
        await _connection.DisposeAsync();
    }

    public async ValueTask CloseConnection()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
        }
        await _connection.CloseAsync();
    }
}
