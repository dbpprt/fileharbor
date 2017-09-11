using System;
using System.Data;
using System.Data.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Dapper;
using Fileharbor.Common;
using Fileharbor.Common.Configuration;
using Fileharbor.Common.Database;
using Fileharbor.Common.Utilities;
using Fileharbor.Services.Contracts;
using Fileharbor.Services.Entities;
using Fileharbor.Services.Schema;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Fileharbor.Services
{
    [UsedImplicitly]
    public class ColumnService : ServiceBase, IColumnService
    {
        private readonly ILogger<ColumnService> _logger;

        public ColumnService(ILogger<ColumnService> logger, IDbConnection database)
            : base(database)
        {
            _logger = logger;
        }

        public async Task<bool> HasColumnAsync(Guid collectionId, Guid columnId, Transaction transaction)
        {
            var database = await GetDatabaseConnectionAsync();
            transaction = transaction.Spawn(database);

            return await transaction.ExecuteAsync(async () =>
            {
                var entity = await database.QueryFirstOrDefaultAsync<Column>(
                    "select id from columns where id = @Id and collection_id = @CollectionId",
                    new {Id = columnId, CollectionId = collectionId},
                    (DbTransaction) transaction);

                return entity != null;
            });
        }

        public async Task CreateColumnAsync(Guid collectionId, Column column, Transaction transaction)
        {
            var database = await GetDatabaseConnectionAsync();
            transaction = transaction.Spawn(database);

            try
            {
                _logger.LogDebug(LoggingEvents.InsertItem, "Starting creation of new column in collection {0} with id {1}", collectionId, column.Id);

                if (await HasColumnAsync(collectionId, column.Id, transaction))
                {
                    _logger.LogWarning(LoggingEvents.InsertItem, "Column is already existing! Abort creation.");
                    return; // TODO: Throw exception? :/
                }

                await database.ExecuteAsync(
                    "insert into columns (id, collection_id, name, description, [group], type, sealed, settings) values ($Id, $CollectionId, $Name, $Description, $Group, $Type, $Sealed, $Settings)",
                    new
                    {
                        column.Id,
                        CollectionId = collectionId,
                        column.Name,
                        column.Description,
                        column.Goup,
                        column.Type,
                        column.Sealed,
                        Settings = column.Settings.ToString(Formatting.None)
                    },
                    (DbTransaction) transaction);

                await transaction.CommitAsync();

                _logger.LogDebug(LoggingEvents.InsertItem, "Finished creation of new column in collection {0} with id {1}", collectionId, column.Id);
            }
            catch (Exception e)
            {
                _logger.LogWarning(LoggingEvents.InsertItem, e, "Unable to create column - unexpected exception");
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
