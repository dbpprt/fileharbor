using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fileharbor.Common.Database;
using Fileharbor.Services.Schema;

namespace Fileharbor.Services.Contracts
{
    public interface IContentTypeService
    {
        Task<bool> HasColumnMappingAsync(Guid collectionId, Guid contentTypeId, Guid columnId, Transaction transaction);
        Task CreateContentTypeAsync(Guid collectionId, ContentType contentType, Transaction transaction);

        Task<IEnumerable<(Guid, string, string, Guid?)>> GetContentTypeInfoAsync(Guid collectionId, Guid contentTypeId,
            Transaction transaction);

        Task<IEnumerable<(Guid, string, string, Guid?)>> GetContentTypeInfosAsync(Guid collectionId,
            Transaction transaction);
    }
}