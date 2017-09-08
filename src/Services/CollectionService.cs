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
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Fileharbor.Services
{
    [UsedImplicitly]
    public class CollectionService : ServiceBase, ICollectionService
    {
        private readonly ILogger<CollectionService> _logger;
        private readonly CurrentPrincipal _currentPrincipal;

        public CollectionService(ILogger<CollectionService> logger, CurrentPrincipal currentPrincipal, IDbConnection database)
            : base(database)
        {
            _logger = logger;
            _currentPrincipal = currentPrincipal;
        }

        public async Task AssignCollectionMappingAsync(Guid userId, Guid collectionId, bool isDefault, Transaction transaction)
        {
            var database = await GetDatabaseConnectionAsync();
            transaction = transaction.Spawn(database);

            if (!await HasCollectionMappingAsync(userId, collectionId, transaction))
            {
                try
                {
                    await database.ExecuteAsync(
                        "insert into user_collection_mappings (user_id, collection_id, is_default) values(@UserId, @CollectionId, @IsDefault)",
                        new {UserId = userId, CollectionId = collectionId, IsDefault = isDefault },
                        (DbTransaction) transaction);
                    await transaction.CommitAsync();
                }
                catch (Exception e)
                {
                    _logger.LogWarning(LoggingEvents.InsertItem, e, "Unable to add collection mapping - unexpected exception");
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<bool> HasCollectionMappingAsync(Guid userId, Guid collectionId, Transaction transaction)
        {
            var database = await GetDatabaseConnectionAsync();
            transaction = transaction.Spawn(database);

            var entity = await database.QueryFirstOrDefaultAsync<UserCollectionMappingEntity>(
                "select from user_collection_mappings where user_id = @UserId and collection_id = @CollectionId",
                new {UserId = userId, CollectionId = collectionId},
                (DbTransaction) transaction);

            return entity != null;
        }

        public async Task<Guid> CreateCollectionAsync(string collectionName, string description, bool isDefault, Transaction transaction)
        {
            var database = await GetDatabaseConnectionAsync();
            transaction = transaction.Spawn(database);
            
            try
            {
                var id = Guid.NewGuid();
                _logger.LogDebug(LoggingEvents.InsertItem, "Starting creation of new collection {0} with id {1}", collectionName, id);
                const double quota = 1e+9; // TODO: how to handle collection quotas :/

                await database.ExecuteAsync(
                    "insert into collections (id, name, quota, description) values(@Id, @Name, @Quota, @Description)",
                    new {Id = id, Name = collectionName, Quota = quota, Description = description},
                    (DbTransaction) transaction);
                await AssignCollectionMappingAsync(_currentPrincipal.Id, id, isDefault, transaction);
                await transaction.CommitAsync();

                _logger.LogDebug(LoggingEvents.InsertItem, "Finished creation of new collection {0} with id {1}", collectionName, id);

                return id;
            }
            catch (Exception e)
            {
                _logger.LogWarning(LoggingEvents.InsertItem, e, "Unable to create collection - unexpected exception");
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task InitializeCollectionAsync(Guid collectionId, Guid templateId)
        {
            throw new NotImplementedException();
        }
    }
}
