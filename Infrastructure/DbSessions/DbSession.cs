using System.Data;
using Microsoft.Data.SqlClient;

public sealed class DbSession : IAsyncDisposable
{
    private readonly SqlConnection _connection;
    private SqlTransaction? _transaction;

    public SqlConnection Connection => _connection;
    public SqlTransaction? Transaction => _transaction;

    public DbSession(string connectionString)
    {
        _connection = new SqlConnection(connectionString);
    }

    public async Task OpenAsync()
    {
        if (_connection.State != ConnectionState.Open)
            await _connection.OpenAsync();
    }

    public async Task BeginTransactionAsync()
    {
        await OpenAsync();
        _transaction ??= (SqlTransaction)await _connection.BeginTransactionAsync();
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
        _connection.Close();
    }
}
