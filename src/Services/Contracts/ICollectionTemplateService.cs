using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fileharbor.Services.Schema;

namespace Fileharbor.Services.Contracts
{
    public interface ICollectionTemplateService
    {
        Task<IEnumerable<Template>> GetAvailableTemplatesAsync();
        Task<Template> GetTemplateByIdAsync(Guid templateId);
        Task<IEnumerable<Template>> GetTemplatesByLanguageAsync(int language);
    }
}