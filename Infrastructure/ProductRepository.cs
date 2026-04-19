using Dapper;
using Microsoft.Data.SqlClient;
using ProjectHK251_Reworked.Domain;

namespace ProjectHK251_Reworked.Infrastructure
{
    public class ProductRepository
    {
        private readonly SqlConnection _connection;
        public ProductRepository(SqlConnection connection) { _connection = connection; }

        public async Task<Product?> SearchById(long id, DbSession session)
        {
            const string sql = @"SELECT * FROM product WHERE id = @id";
            return await _connection.QuerySingleOrDefaultAsync<Product>(sql, new { id }, transaction: session.Transaction);
        }

        public async Task<List<Product>> SearchByCreatedDate(DateTime time, DbSession session)
        {
            var start = time.Date;
            var end = start.AddDays(1);

            const string sql = @"
                    SELECT *
                    FROM product
                    WHERE created_at >= @Start
                    AND created_at < @End";
            var result = await _connection.QueryAsync<Product>(sql, new { start, end }, transaction: session.Transaction);
            return result.ToList();
        }

        public async Task<long> InsertProduct(Product product, DbSession session)
        {
            var name = product.Name;
            var sku = product.Sku;
            const string sql = @"INSERT INTO product (name, sku) VALUES (@name, @sku)";
            long result;
            result = await session.Connection.ExecuteScalarAsync<long>(sql, new { name, sku }, transaction: session.Transaction);
            return result;
        }

        public async Task<Product?> getProductBySKU(string SKU, DbSession session)
        {
            const string sql = @"SELECT * FROM product WHERE sku = @SKU";
            return await session.Connection.QuerySingleOrDefaultAsync<Product?>(sql, new { SKU }, transaction: session.Transaction);
        }
    }
}
