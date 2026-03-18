using Microsoft.AspNetCore.Mvc;
using ProjectHK251_Reworked.Application;
using ProjectHK251_Reworked.Domain.DataTransferObj;
using ProjectHK251_Reworked.Infrastructure;
using ProjectHK251_Reworked.Infrastructure.DbSessions;

namespace ProjectHK251_Reworked.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class ImportController : ControllerBase
    {
        private readonly IDbSessionFactory _dbSessionFactory;

        public ImportController(IDbSessionFactory dbSessionFactory)
        {
            _dbSessionFactory = dbSessionFactory;
        }

        [HttpPost]
        public async Task<IActionResult> Import([FromBody] ImportRequest request)
        {
            await using var session = _dbSessionFactory.Create();
            await session.BeginTransactionAsync();

            try
            {
                var connection = session.Connection;

                var productRepo = new ProductRepository(connection);
                var batchRepo = new BatchRepository(connection);
                var movementRepo = new MovementRepository(connection);

                var service = new ImportService(productRepo, batchRepo, movementRepo);
                await service.ImportToWarehouse(request, session);

                await session.CommitAsync();
                return Ok(new { message = "Import completed" });
            }
            catch (Exception ex)
            {
                await session.RollbackAsync();
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
