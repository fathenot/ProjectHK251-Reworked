using Microsoft.AspNetCore.Mvc;
using ProjectHK251_Reworked.Application;
using ProjectHK251_Reworked.Domain.DataTransferObj;
using ProjectHK251_Reworked.Infrastructure;
using ProjectHK251_Reworked.Infrastructure.DbSessions;

namespace ProjectHK251_Reworked.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class ExportController : ControllerBase
    {
        private readonly IDbSessionFactory _dbSessionFactory;

        public ExportController(IDbSessionFactory dbSessionFactory)
        {
            _dbSessionFactory = dbSessionFactory;
        }

        [HttpPost]
        public async Task<IActionResult> Export([FromBody] ExportRequest request)
        {
            var requestId = string.IsNullOrWhiteSpace(request.RequestId)
                ? Guid.NewGuid().ToString()
                : request.RequestId!;

            await using var session = _dbSessionFactory.Create();
            await session.BeginTransactionAsync();

            try
            {
                var connection = session.Connection;

                var productRepo = new ProductRepository(connection);
                var batchRepo = new BatchRepository(connection);
                var movementRepo = new MovementRepository(connection);

                var service = new ExportService(productRepo, batchRepo, movementRepo);
                await service.ExportFromWarehouse(request, session, requestId);

                await session.CommitAsync();
                return Ok(new { message = "Export completed", requestId });
            }
            catch (Exception ex)
            {
                await session.RollbackAsync();
                return BadRequest(new { error = ex.Message, requestId });
            }
        }
    }
}
