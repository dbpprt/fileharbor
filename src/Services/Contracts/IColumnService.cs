using System;
using System.Threading.Tasks;
using Fileharbor.Common.Database;
using Fileharbor.Services.Schema;

namespace Fileharbor.Services.Contracts
{
    public interface IColumnService
    {
        Task<bool> HasColumnAsync(Guid collectionId, Guid columnId, Transaction transaction);
        Task CreateColumnAsync(Guid collectionId, Column column, Transaction transaction);
    }
}