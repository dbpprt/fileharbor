using System;
using System.Collections.Concurrent;
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
    public class PermissionService : ServiceBase, IPermissionService
    {
        private readonly ILogger<PermissionService> _logger;
        private readonly CurrentPrincipal _currentPrincipal;
        private readonly ConcurrentDictionary<(Guid, Guid), PermissionLevel> _permissionCache;

        public PermissionService(ILogger<PermissionService> logger, IDbConnection database, CurrentPrincipal currentPrincipal)
            : base(database)
        {
            _logger = logger;
            _currentPrincipal = currentPrincipal;
            _permissionCache = new ConcurrentDictionary<(Guid, Guid), PermissionLevel>();
        }

        public async Task<PermissionLevel> GetPermissionForCollection(Guid collectionId, Guid userId, Transaction transaction)
        {
            if (_permissionCache.TryGetValue((collectionId, userId), out var permissionLevel))
            {
                return permissionLevel;
            }

            var database = await GetDatabaseConnectionAsync();
            transaction = transaction.Spawn(database);

            return await transaction.ExecuteAsync(async () =>
            {
                var entity = await database.QueryFirstOrDefaultAsync<UserCollectionMappingEntity>(
                    "select from user_collection_mappings where user_id = @UserId and collection_id = @CollectionId",
                    new { UserId = userId, CollectionId = collectionId },
                    (DbTransaction)transaction);

                var result = entity != null ? PermissionLevel.Owner : PermissionLevel.None;

                // we want to cache the results for the current request scope
                _permissionCache.TryAdd((collectionId, userId), result);

                return result;
            });
        }

        public async Task EnsureCollectionPermission(Guid collectionId, PermissionLevel requestedPermissionLevel, Transaction transaction)
        {
            var currentPermissionLevel = await GetPermissionForCollection(collectionId, _currentPrincipal.Id, transaction);

            if (currentPermissionLevel < requestedPermissionLevel)
            {
                throw new AccessDeniedException(collectionId);
            }
        }
    }
}
