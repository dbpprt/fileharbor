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
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Fileharbor.Services
{
    [UsedImplicitly]
    public class CollectionService : ServiceBase, ICollectionService
    {
        private readonly ICollectionTemplateService _collectionTemplateService;
        private readonly IColumnService _columnService;
        private readonly IContentTypeService _contentTypeService;
        private readonly CurrentPrincipal _currentPrincipal;
        private readonly ILogger<CollectionService> _logger;
        private readonly IPermissionService _permissionService;

        public CollectionService(ILogger<CollectionService> logger, IPermissionService permissionService,
            ICollectionTemplateService collectionTemplateService, IColumnService columnService,
            IContentTypeService contentTypeService, CurrentPrincipal currentPrincipal, IDbConnection database)
            : base(database)
        {
            _logger = logger;
            _permissionService = permissionService;
            _collectionTemplateService = collectionTemplateService;
            _columnService = columnService;
            _contentTypeService = contentTypeService;
            _currentPrincipal = currentPrincipal;
        }

        public async Task<Guid> CreateCollectionAsync(string collectionName, string description, bool isDefault,
            Transaction transaction)
        {
            var database = await GetDatabaseConnectionAsync();
            transaction = transaction.Spawn(database);

            try
            {
                var id = Guid.NewGuid();
                _logger.LogDebug(LoggingEvents.InsertItem, "Starting creation of new collection {0} with id {1}",
                    collectionName, id);
                const double quota = 1e+9; // TODO: how to handle collection quotas :/

                await database.ExecuteAsync(
                    "insert into collections (id, name, quota, description) values(@id, @name, @quota, @description)",
                    new {id, name = collectionName, quota, description},
                    (DbTransaction) transaction);
                await AssignCollectionMappingAsync(_currentPrincipal.Id, id, isDefault, true, transaction);
                await transaction.CommitAsync();

                _logger.LogDebug(LoggingEvents.InsertItem, "Finished creation of new collection {0} with id {1}",
                    collectionName, id);

                return id;
            }
            catch (Exception e)
            {
                _logger.LogWarning(LoggingEvents.InsertItem, e, "Unable to create collection - unexpected exception");
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task InitializeCollectionAsync(Guid collectionId, Guid templateId, Transaction transaction)
        {
            var database = await GetDatabaseConnectionAsync();
            transaction = transaction.Spawn(database);

            await _permissionService.EnsureCollectionPermission(collectionId, PermissionLevel.Owner, transaction);

            try
            {
                _logger.LogDebug(LoggingEvents.InsertItem,
                    "Starting collection initialization for collection {0} with template id {1}", collectionId,
                    templateId);
                var template = await _collectionTemplateService.GetTemplateByIdAsync(templateId);

                if (await IsCollectionInitializedAsync(collectionId, transaction))
                {
                    _logger.LogWarning(LoggingEvents.InsertItem, "Collection is already initialized - aborting!");
                    throw new CollectionAlreadyInitializedException(collectionId, templateId);
                }

                foreach (var column in template.Columns)
                    await _columnService.CreateColumnAsync(collectionId, column, transaction);

                foreach (var contentType in template.ContentTypes)
                    await _contentTypeService.CreateContentTypeAsync(collectionId, contentType, transaction);

                await SetTemplateIdForCollectionAsync(collectionId, templateId, transaction);
                await transaction.CommitAsync();

                _logger.LogDebug(LoggingEvents.InsertItem,
                    "Finished collection initialization for collection {0} with template id {1}", collectionId,
                    templateId);
            }
            catch (Exception e)
            {
                _logger.LogWarning(LoggingEvents.InsertItem, e,
                    "Unable to initialize collection - unexpected exception");
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task AssignCollectionMappingAsync(Guid userId, Guid collectionId, bool isDefault,
            bool skipPermissionCheck, Transaction transaction)
        {
            var database = await GetDatabaseConnectionAsync();
            transaction = transaction.Spawn(database);

            if (!await HasCollectionMappingAsync(userId, collectionId, transaction))
            {
                if (!skipPermissionCheck)
                    await _permissionService.EnsureCollectionPermission(collectionId, PermissionLevel.Owner,
                        transaction);

                try
                {
                    await database.ExecuteAsync(
                        "insert into user_collection_mappings (user_id, collection_id, is_default) values(@user_id, @collection_id, @is_default)",
                        new {user_id = userId, collection_id = collectionId, is_default = isDefault},
                        (DbTransaction) transaction);
                    await transaction.CommitAsync();
                }
                catch (Exception e)
                {
                    _logger.LogWarning(LoggingEvents.InsertItem, e,
                        "Unable to add collection mapping - unexpected exception");
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<bool> HasCollectionMappingAsync(Guid userId, Guid collectionId, Transaction transaction)
        {
            var database = await GetDatabaseConnectionAsync();
            transaction = transaction.Spawn(database);

            return await transaction.ExecuteAsync(async () =>
            {
                var entity = await database.QueryFirstOrDefaultAsync<UserCollectionMappingEntity>(
                    "select * from user_collection_mappings where user_id = @user_id and collection_id = @collection_id",
                    new {user_id = userId, collection_id = collectionId},
                    (DbTransaction) transaction);

                return entity != null;
            });
        }

        public async Task<IEnumerable<(Guid, string, PermissionLevel)>> GetMyCollectionsAsync(Transaction transaction)
        {
            var database = await GetDatabaseConnectionAsync();
            transaction = transaction.Spawn(database);

            return await transaction.ExecuteAsync(async () =>
            {
                var entities = await database.QueryAsync<CollectionEntity>(
                    "select id, name from user_collection_mappings join collections on (user_collection_mappings.collection_id = collections.id) where user_collection_mappings.user_id = @user_id",
                    new { user_id = _currentPrincipal.Id },
                    (DbTransaction)transaction);

                return entities.Select(_ => (_.Id, _.Name, PermissionLevel.Owner));
            });
        }

        public async Task<bool> IsCollectionInitializedAsync(Guid collectionId, Transaction transaction)
        {
            var database = await GetDatabaseConnectionAsync();
            transaction = transaction.Spawn(database);

            return await transaction.ExecuteAsync(async () =>
            {
                await _permissionService.EnsureCollectionPermission(collectionId, PermissionLevel.Member, transaction);

                var entity = await database.QueryFirstOrDefaultAsync<Guid?>(
                    "select template_id from collections where id = @id",
                    new {id = collectionId},
                    (DbTransaction) transaction);

                return entity.HasValue;
            });
        }

        public async Task SetTemplateIdForCollectionAsync(Guid collectionId, Guid templateId, Transaction transaction)
        {
            var database = await GetDatabaseConnectionAsync();
            transaction = transaction.Spawn(database);

            await _permissionService.EnsureCollectionPermission(collectionId, PermissionLevel.Owner, transaction);

            await transaction.ExecuteAsync<Task>(async () =>
            {
                await database.ExecuteAsync(
                    "update collections set template_id = @template_id where id = @id",
                    new {id = collectionId, template_id = templateId},
                    (DbTransaction) transaction);
            });
        }
    }
}