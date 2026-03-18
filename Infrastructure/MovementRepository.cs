using Dapper;
using MySqlConnector;
using ProjectHK251_Reworked.Domain;

namespace ProjectHK251_Reworked.Infrastructure
{
    public class MovementRepository
    {
        private readonly MySqlConnection _conn;

        public MovementRepository(MySqlConnection conn)
        {
            _conn = conn;
        }

        public Task<int?> getAmountOfProduct(int productId)
        {
            const string query = "SELECT * FROM movement WHERE product_id = @productId";
            return _conn.QuerySingleOrDefaultAsync<int?>(query, new { productId });
        }

        public async Task<List<InventoryMovement>> GetInventoryMovementAtDate(DateTime date)
        {
            var start = date.Date;
            var end = start.AddDays(1);

            const string sql = @"
                    SELECT *
                    FROM inventory_movement
                    WHERE created_at >= @Start
                    AND created_at < @End";

            var result = await _conn.QueryAsync<InventoryMovement>(sql, new
            {
                Start = start,
                End = end
            });

            return result.ToList();
        }

        public async Task<List<InventoryMovement>> filter(MovementType type)
        {
            const string sql = @"SELECT * FROM inventory_movement WHERE movement_type = @type";
            var result = await _conn.QueryAsync<InventoryMovement>(sql, new { type });
            return result.ToList();
        }

        public async Task Import(long batchId, int quantity, string referenceDocId, string requestId)
        {
            await _conn.ExecuteAsync(
                "CALL sp_import_inventory(@batchId, @quantity, @ReferenceDocId, @requestId)",
                new { batchId, quantity, ReferenceDocId = referenceDocId, requestId }
            );
        }

        public async Task Export(long batchId, int quantity, string referenceDocId, string requestId)
        {
            await _conn.ExecuteAsync(
                "CALL sp_export_inventory(@batchId, @quantity, @ReferenceDocId, @requestId)",
                new { batchId, quantity, ReferenceDocId = referenceDocId, requestId }
            );
        }

        public async Task<List<InventoryMovement>> FilterMovementEventByReq(string requestId)
        {
            const string sql = @"SELECT * FROM inventory_movement WHERE request_id = @requestId";
            var result = await _conn.QueryAsync<InventoryMovement>(sql, new { requestId });
            return result.ToList();
        }
    }
}
