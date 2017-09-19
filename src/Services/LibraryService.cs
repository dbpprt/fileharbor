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
        
        public async Task CreateLibraryAsync(Guid collectionId, string name, string description, Transaction transaction)
        {
            var database = await GetDatabaseConnectionAsync();
            transaction = transaction.Spawn(database);

            await _permissionService.EnsureCollectionPermission(collectionId, PermissionLevel.Owner, transaction);

            try
            {
                _logger.LogDebug(LoggingEvents.InsertItem,
                    "Starting creation of new library in collection {0}", collectionId);

                
                await transaction.CommitAsync();

                _logger.LogDebug(LoggingEvents.InsertItem,
                    "Finished creation of new library in collection {0}", collectionId);
            }
            catch (Exception e)
            {
                _logger.LogWarning(LoggingEvents.InsertItem, e, "Unable to create library - unexpected exception");
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}