using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Fileharbor.Common;
using Fileharbor.Common.Database;
using Fileharbor.Services.Contracts;
using Fileharbor.Services.Entities;
using Fileharbor.Services.Schema;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Fileharbor.Services
{
    [UsedImplicitly]
    public class ContentTypeService : ServiceBase, IContentTypeService
    {
        private readonly ILogger<ContentTypeService> _logger;
        private readonly IPermissionService _permissionService;

        public ContentTypeService(ILogger<ContentTypeService> logger, IPermissionService permissionService,
            IDbConnection database)
            : base(database)
        {
            _logger = logger;
            _permissionService = permissionService;
        }

        public async Task<bool> HasColumnMappingAsync(Guid collectionId, Guid contentTypeId, Guid columnId,
            Transaction transaction)
        {
            var database = await GetDatabaseConnectionAsync();
            transaction = transaction.Spawn(database);

            await _permissionService.EnsureCollectionPermission(collectionId, PermissionLevel.Member, transaction);

            return await transaction.ExecuteAsync(async () =>
            {
                var entity = await database.QueryFirstOrDefaultAsync<ContentTypeColumnMappingEntity>(
                    "select column_id from contenttype_column_mappings where contenttype_id = @contenttype_id and column_id = @column_id and collection_id = @collection_id",
                    new {contenttype_id = contentTypeId, column_id = columnId, collection_id = collectionId},
                    (DbTransaction) transaction);

                return entity != null;
            });
        }

        public async Task CreateContentTypeAsync(Guid collectionId, ContentType contentType, Transaction transaction)
        {
            var database = await GetDatabaseConnectionAsync();
            transaction = transaction.Spawn(database);

            await _permissionService.EnsureCollectionPermission(collectionId, PermissionLevel.Owner, transaction);

            try
            {
                _logger.LogDebug(LoggingEvents.InsertItem,
                    "Starting creation of new contentType in collection {0} with id {1}", collectionId, contentType.Id);

                if (await HasContentTypeAsync(collectionId, contentType.Id, transaction))
                {
                    _logger.LogWarning(LoggingEvents.InsertItem, "Column is already existing! Abort creation.");
                    return; // TODO: Throw exception? :/
                }

                await database.ExecuteAsync(
                    @"insert into contenttypes (id, collection_id, parent_id, name, description, group_name, sealed) values(@id, @collection_id, @parent_id, @name, @description, @group_name, @sealed)",
                    new
                    {
                        id = contentType.Id,
                        collection_id = collectionId,
                        parent_id = contentType.ParentId,
                        name = contentType.Name,
                        description = contentType.Description,
                        group_name = contentType.GroupName,
                        @sealed = contentType.Sealed
                    },
                    (DbTransaction) transaction);

                if (contentType.Columns != null && contentType.Columns.Any())
                    foreach (var column in contentType.Columns)
                        await AddColumnMappingAsync(collectionId, contentType.Id, column, transaction);

                await transaction.CommitAsync();

                _logger.LogDebug(LoggingEvents.InsertItem,
                    "Finished creation of new contentType in collection {0} with id {1}", collectionId, contentType.Id);
            }
            catch (Exception e)
            {
                _logger.LogWarning(LoggingEvents.InsertItem, e, "Unable to create contentType - unexpected exception");
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<(Guid, string, string, Guid?)>> GetContentTypeInfoAsync(Guid collectionId,
            Guid contentTypeId, Transaction transaction)
        {
            var database = await GetDatabaseConnectionAsync();
            transaction = transaction.Spawn(database);

            await _permissionService.EnsureCollectionPermission(collectionId, PermissionLevel.Member, transaction);

            return await transaction.ExecuteAsync(async () =>
            {
                var entities = await database.QueryAsync<ContentTypeEntity>(
                    "select id, name, parent_id from contenttypes where id = @id and collection_id = @collection_id",
                    new {Id = contentTypeId, collection_id = collectionId},
                    (DbTransaction) transaction);

                return entities.Select(_ => (_.Id, _.Name, _.GroupName, _.ParentId));
            });
        }

        public async Task<IEnumerable<(Guid, string, string, Guid?)>> GetContentTypeInfosAsync(Guid collectionId,
            Transaction transaction)
        {
            var database = await GetDatabaseConnectionAsync();
            transaction = transaction.Spawn(database);

            await _permissionService.EnsureCollectionPermission(collectionId, PermissionLevel.Member, transaction);

            return await transaction.ExecuteAsync(async () =>
            {
                var entities = await database.QueryAsync<ContentType>(
                    "select id, name, parent_id from contenttypes where collection_id = @collection_id",
                    new {collection_id = collectionId},
                    (DbTransaction) transaction);

                return entities.Select(_ => (_.Id, _.Name, _.GroupName, _.ParentId));
            });
        }

        public async Task<bool> HasContentTypeAsync(Guid collectionId, Guid contentTypeId, Transaction transaction)
        {
            var database = await GetDatabaseConnectionAsync();
            transaction = transaction.Spawn(database);

            await _permissionService.EnsureCollectionPermission(collectionId, PermissionLevel.Member, transaction);

            return await transaction.ExecuteAsync(async () =>
            {
                var entity = await database.QueryFirstOrDefaultAsync<ContentType>(
                    "select id from contenttypes where id = @id and collection_id = @collection_id",
                    new {Id = contentTypeId, collection_id = collectionId},
                    (DbTransaction) transaction);

                return entity != null;
            });
        }

        public async Task AddColumnMappingAsync(Guid collectionId, Guid contentTypeId, ColumnMapping mapping,
            Transaction transaction)
        {
            var database = await GetDatabaseConnectionAsync();
            transaction = transaction.Spawn(database);

            await _permissionService.EnsureCollectionPermission(collectionId, PermissionLevel.Owner, transaction);

            try
            {
                _logger.LogDebug(LoggingEvents.InsertItem,
                    "Starting creation of new column mapping for content type {0} in collection {1}", contentTypeId,
                    collectionId);

                if (await HasColumnMappingAsync(collectionId, contentTypeId, mapping.Id, transaction))
                {
                    _logger.LogWarning(LoggingEvents.InsertItem, "Column mapping is already existing! Abort creation.");
                    return; // TODO: Throw exception? :/
                }

                await database.ExecuteAsync(
                    @"insert into contenttype_column_mappings (contenttype_id, column_id, collection_id, required, visible, default_value) values(@contenttype_id, @column_id, @collection_id, @required, @visible, @default_value)",
                    new
                    {
                        contenttype_id = contentTypeId,
                        column_id = mapping.Id,
                        collection_id = collectionId,
                        required = mapping.Required,
                        visible = mapping.Visible,
                        default_value = mapping.Default
                    },
                    (DbTransaction) transaction);

                await transaction.CommitAsync();

                _logger.LogDebug(LoggingEvents.InsertItem,
                    "Finished creation of new column mapping in collection {0} with id {1}", collectionId, mapping.Id);
            }
            catch (Exception e)
            {
                _logger.LogWarning(LoggingEvents.InsertItem, e,
                    "Unable to create column mapping - unexpected exception");
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}