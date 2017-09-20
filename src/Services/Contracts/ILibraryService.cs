using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fileharbor.Common.Database;
using Fileharbor.Services.Entities;

namespace Fileharbor.Services.Contracts
{
    public interface ILibraryService
    {
        Task<Guid> CreateLibraryAsync(Guid collectionId, string name, string description, LibraryType type,
            bool isSealed, Transaction transaction);

        Task<IEnumerable<LibraryEntity>> GetLibraries(Guid collectionId, Transaction transaction);

        Task AssignContentTypeMappingAsync(Guid collectionId, Guid libraryId, Guid contentTypeId,
            Transaction transaction);
    }
}