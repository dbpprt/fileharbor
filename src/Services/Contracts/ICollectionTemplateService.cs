using System.Collections.Generic;
using System.Threading.Tasks;
using Fileharbor.Services.Schema;

namespace Fileharbor.Services.Contracts
{
    public interface ICollectionTemplateService
    {
        Task<IEnumerable<Template>> GetAvailableTemplates(int language);
    }
}