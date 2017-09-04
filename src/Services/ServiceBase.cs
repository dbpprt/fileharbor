using System.Data;
using System.Threading.Tasks;
using Fileharbor.Common.Database;
using Npgsql;

namespace Fileharbor.Services
{
    public abstract class ServiceBase
    {
        private readonly IDbConnection _connection;
        private bool _opened;

        protected ServiceBase(IDbConnection connection)
        {
            _connection = connection;
        }

        protected async Task<IDbConnection> GetDatabaseConnectionAsync()
        {
            if (_opened)
            {
                return _connection;
            }

            if (_connection is NpgsqlConnection postgresConnection)
            {
                await postgresConnection.OpenAsync();
            }
            else
            {
                _connection.Open();
            }

            _opened = true;
            return _connection;
        }
    }
}
