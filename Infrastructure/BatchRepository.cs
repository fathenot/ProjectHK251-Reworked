using Dapper;
using Microsoft.Data.SqlClient;
using ProjectHK251_Reworked.Domain;

namespace ProjectHK251_Reworked.Infrastructure
{
    public sealed class BatchRepository
    {
        private readonly SqlConnection _conn;

        public BatchRepository(SqlConnection conn)
        {
            _conn = conn;
        }

        public Task<Batch?> findBatchAsync(long id)
        {
            const string sql = @"SELECT * FROM batch WHERE id = @id";
            return _conn.QuerySingleOrDefaultAsync<Batch>(sql, new { id });
        }

        public async Task<long> InsertBatch(Batch batch, DbSession session)
        {
            var batchCode = batch.BatchCode;
            var productId = batch.ProductId;
            const string sql = "INSERT INTO batch(product_id, batch_code) VALUES (@batchCode, @productId)";
            return await session.Connection.ExecuteScalarAsync<long>(sql, new { batchCode, productId }, transaction: session.Transaction);
        }

        public async Task<List<Batch>> findBatchByBatchCode(string batchCode, DbSession session)
        {
            const string sql = "SELECT * FROM batch WHERE batch_code = @batchCode";
            var result = await session.Connection.QueryAsync<Batch>(sql, new { batchCode }, transaction: session.Transaction);
            return result.ToList();
        }

        public async Task<Batch?> findBatchByBatchCodeAndProduct(string batchCode, long productId, DbSession session)
        {
            const string sql = @"SELECT * FROM batch WHERE batch_code = @batchCode AND product_id = @productId";
            var result = await session.Connection.QuerySingleOrDefaultAsync<Batch?>(sql, new { batchCode, productId }, transaction: session.Transaction);
            return result;
        }
    }
}
