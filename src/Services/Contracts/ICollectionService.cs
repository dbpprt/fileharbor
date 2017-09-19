using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fileharbor.Common.Database;
using Fileharbor.Services.Entities;

namespace Fileharbor.Services.Contracts
{
    public interface ICollectionService
    {
        Task<Guid> CreateCollectionAsync(string collectionName, string description, bool isDefault,
            Transaction transaction);

        Task InitializeCollectionAsync(Guid collectionId, Guid templateId, Transaction transaction);

        Task<IEnumerable<(Guid, string, PermissionLevel)>> GetMyCollectionsAsync(Transaction transaction);
    }
}