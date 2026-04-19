using Dapper;
using Microsoft.AspNetCore.Mvc;
using ProjectHK251_Reworked.Infrastructure.DbSessions;

namespace ProjectHK251_Reworked.Api
{
    [ApiController]
    [Route("api/health")]
    public sealed class HealthController : ControllerBase
    {
        private readonly IDbSessionFactory _dbSessionFactory;

        public HealthController(IDbSessionFactory dbSessionFactory)
        {
            _dbSessionFactory = dbSessionFactory;
        }

        [HttpGet("db")]
        public async Task<IActionResult> CheckDb()
        {
            await using var session = _dbSessionFactory.Create();
            try
            {
                await session.OpenAsync();
                var result = await session.Connection.ExecuteScalarAsync<int>("SELECT 1");
                return Ok(new { ok = true, result });
            }
            catch (Exception ex)
            {
                return StatusCode(503, new { ok = false, error = ex.Message });
            }
        }
    }
}
