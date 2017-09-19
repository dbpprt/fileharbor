using System;
using System.Threading.Tasks;
using Fileharbor.Common.Database;
using Fileharbor.Services.Entities;

namespace Fileharbor.Services.Contracts
{
    public interface IPermissionService
    {
        Task<PermissionLevel> GetPermissionForCollection(Guid collectionId, Guid userId, Transaction transaction);
        Task EnsureCollectionPermission(Guid collectionId, PermissionLevel requestedPermissionLevel, Transaction transaction);
    }
}