using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Dapper;
using Fileharbor.Common;
using Fileharbor.Common.Database;
using Fileharbor.Services.Contracts;
using Fileharbor.Services.Entities;
using Fileharbor.Services.Schema;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Fileharbor.Services
{
    [UsedImplicitly]
    public class ColumnService : ServiceBase, IColumnService
    {
        private readonly ILogger<ColumnService> _logger;
        private readonly IPermissionService _permissionService;

        public ColumnService(ILogger<ColumnService> logger, IPermissionService permissionService,
            IDbConnection database)
            : base(database)
        {
            _logger = logger;
            _permissionService = permissionService;
        }

        public async Task<bool> HasColumnAsync(Guid collectionId, Guid columnId, Transaction transaction)
        {
            var database = await GetDatabaseConnectionAsync();
            transaction = transaction.Spawn(database);

            await _permissionService.EnsureCollectionPermission(collectionId, PermissionLevel.Member, transaction);

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

            await _permissionService.EnsureCollectionPermission(collectionId, PermissionLevel.Owner, transaction);

            try
            {
                _logger.LogDebug(LoggingEvents.InsertItem,
                    "Starting creation of new column in collection {0} with id {1}", collectionId, column.Id);

                if (await HasColumnAsync(collectionId, column.Id, transaction))
                {
                    _logger.LogWarning(LoggingEvents.InsertItem, "Column is already existing! Abort creation.");
                    return; // TODO: Throw exception? :/
                }

                await database.ExecuteAsync(
                    @"insert into columns (id, collection_id, name, description,  group_name, type, sealed, settings) values(@id, @collection_id, @name, @description, @group_name, @type, @sealed, @settings::json)",
                    new
                    {
                        id = column.Id,
                        collection_id = collectionId,
                        name = column.Name,
                        description = column.Description,
                        group_name = column.GroupName,
                        type = column.Type,
                        @sealed = column.Sealed,
                        settings = column.Settings.ToString(Formatting.None)
                    },
                    (DbTransaction) transaction);

                await transaction.CommitAsync();

                _logger.LogDebug(LoggingEvents.InsertItem,
                    "Finished creation of new column in collection {0} with id {1}", collectionId, column.Id);
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