namespace ProjectHK251_Reworked.Infrastructure.DbSessions
{
    public interface IDbSessionFactory
    {
        DbSession Create();
    }

    public class DbSessionFactory : IDbSessionFactory
    {
        private readonly string _connectionString;

        public DbSessionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Default");
        }

        public DbSession Create()
        {
            return new DbSession(_connectionString);
        }
    }
}
