using System.Data;
using System.Threading.Tasks;
using Npgsql;

namespace Fileharbor.Services
{
    public abstract class ServiceBase
    {
        private readonly IDbConnection _connection;

        protected ServiceBase(IDbConnection connection)
        {
            _connection = connection;
        }

        protected async Task<IDbConnection> GetDatabaseConnectionAsync()
        {
            if (_connection.State == ConnectionState.Open)
                return _connection;

            if (_connection is NpgsqlConnection postgresConnection)
                await postgresConnection.OpenAsync();
            else
                _connection.Open();

            return _connection;
        }
    }
}