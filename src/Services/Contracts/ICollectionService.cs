using System;
using System.Threading.Tasks;
using Fileharbor.Common.Database;

namespace Fileharbor.Services.Contracts
{
    public interface ICollectionService
    {
        Task<Guid> CreateCollectionAsync(string collectionName, string description, bool isDefault, Transaction transaction);
    }
}