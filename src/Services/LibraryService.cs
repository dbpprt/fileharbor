using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Dapper;
using Fileharbor.Common;
using Fileharbor.Common.Database;
using Fileharbor.Services.Contracts;
using Fileharbor.Services.Entities;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Fileharbor.Services
{
    [UsedImplicitly]
    public class LibraryService : ServiceBase, ILibraryService
    {
        private readonly ILogger<LibraryService> _logger;
        private readonly IPermissionService _permissionService;

        public LibraryService(ILogger<LibraryService> logger, IPermissionService permissionService,
            IDbConnection database)
            : base(database)
        {
            _logger = logger;
            _permissionService = permissionService;
        }

        public async Task<Guid> CreateLibraryAsync(Guid collectionId, string name, string description, LibraryType type,
            bool isSealed, Transaction transaction)
        {
            var database = await GetDatabaseConnectionAsync();
            transaction = transaction.Spawn(database);

            await _permissionService.EnsureCollectionPermission(collectionId, PermissionLevel.Owner, transaction);

            try
            {
                var id = Guid.NewGuid();
                _logger.LogDebug(LoggingEvents.InsertItem,
                    "Starting creation of new library in collection {0} with id {1}", collectionId, id);

                await database.ExecuteAsync(
                    @"insert into libraries (id, collection_id, name, description, type, sealed) values(@id, @collection_id, @name, @description, @type, @sealed)",
                    new
                    {
                        id,
                        collection_id = collectionId,
                        name,
                        description,
                        type,
                        @sealed = isSealed
                    },
                    (DbTransaction) transaction);

                await transaction.CommitAsync();

                _logger.LogDebug(LoggingEvents.InsertItem,
                    "Finished creation of new library in collection {0}", collectionId);

                return id;
            }
            catch (Exception e)
            {
                _logger.LogWarning(LoggingEvents.InsertItem, e, "Unable to create library - unexpected exception");
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<LibraryEntity>> GetLibraries(Guid collectionId, Transaction transaction)
        {
            var database = await GetDatabaseConnectionAsync();
            transaction = transaction.Spawn(database);

            await _permissionService.EnsureCollectionPermission(collectionId, PermissionLevel.Member, transaction);

            return await transaction.ExecuteAsync(async () =>
            {
                var entities = await database.QueryAsync<LibraryEntity>(
                    "select * from libraries where collection_id = @collection_id",
                    new {collection_id = collectionId},
                    (DbTransaction) transaction);

                return entities;
            });
        }

        public async Task AssignContentTypeMappingAsync(Guid collectionId, Guid libraryId, Guid contentTypeId,
            Transaction transaction)
        {
            var database = await GetDatabaseConnectionAsync();
            transaction = transaction.Spawn(database);

            // TODO: Validate not Inbox!

            if (!await HasContentTypeMappingAsync(collectionId, libraryId, contentTypeId, transaction))
            {
                await _permissionService.EnsureCollectionPermission(collectionId, PermissionLevel.Owner,
                    transaction);

                try
                {
                    await database.ExecuteAsync(
                        "insert into library_contenttype_mappings (library_id, collection_id, contenttype_id) values(@library_id, @collection_id, @contenttype_id)",
                        new {library_id = libraryId, collection_id = collectionId, contenttype_id = contentTypeId},
                        (DbTransaction) transaction);
                    await transaction.CommitAsync();
                }
                catch (Exception e)
                {
                    _logger.LogWarning(LoggingEvents.InsertItem, e,
                        "Unable to add contenttype mapping - unexpected exception");
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<bool> HasContentTypeMappingAsync(Guid collectionId, Guid libraryId, Guid contentTypeId,
            Transaction transaction)
        {
            var database = await GetDatabaseConnectionAsync();
            transaction = transaction.Spawn(database);

            return await transaction.ExecuteAsync(async () =>
            {
                var entity = await database.QueryFirstOrDefaultAsync<LibraryContentTypeMappingEntity>(
                    "select * from library_contenttype_mappings where library_id = @library_id and collection_id = @collection_id and contenttype_id = @contenttype_id",
                    new {library_id = libraryId, collection_id = collectionId, contenttype_id = contentTypeId},
                    (DbTransaction) transaction);

                return entity != null;
            });
        }
    }
}