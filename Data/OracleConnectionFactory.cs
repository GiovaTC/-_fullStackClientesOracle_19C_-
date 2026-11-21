using Oracle.ManagedDataAccess.Client;

namespace Backend.Data
{
    public sealed class OracleConnectionFactory(IConfiguration configuration)
    {
        private readonly string _connectionString = configuration.GetConnectionString("Oracle")
            ?? throw new InvalidOperationException("FALTA el connectionString: Oracle!");

        public OracleConnection Create() => new(_connectionString);
    }   
}
