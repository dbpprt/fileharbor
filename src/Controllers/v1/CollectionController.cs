using System;
using System.Linq;
using System.Threading.Tasks;
using Fileharbor.Common;
using Fileharbor.Services.Contracts;
using Fileharbor.ViewModels.v1.Collections;
using Microsoft.AspNetCore.Mvc;

namespace Fileharbor.Controllers.v1
{
    [Route("api/v1/collections")]
    public class CollectionController : BaseController
    {
        private readonly ICollectionService _collectionService;
        private readonly ICollectionTemplateService _collectionTemplateService;

        public CollectionController(ICollectionService collectionService, ICollectionTemplateService collectionTemplateService, CurrentPrincipal principal) : base(principal)
        {
            _collectionService = collectionService;
            _collectionTemplateService = collectionTemplateService;
        }

        [HttpPost, Route("")]
        public async Task<IActionResult> Create([FromBody]CreateCollectionRequest model)
        {
            return Ok(await _collectionService.CreateCollectionAsync(model.Name, model.Description, model.IsDefault, null));
        }

        [HttpGet, Route("templates/{language}")]
        public async Task<IActionResult> GetTemplates(int language)
        {
            return Ok((await _collectionTemplateService.GetTemplatesByLanguageAsync(language)).Select(_ => new CollectionTemplateResponse
            {
                Id = _.Id,
                Name = _.Name,
                Description = _.Description
            }));
        }

        [HttpPatch, Route("initialize/{collectionId}/{templateId}")]
        public async Task<IActionResult> GetTemplates(Guid collectionId, Guid templateId)
        {
            await _collectionService.InitializeCollectionAsync(collectionId, templateId, null);
            return NoContent();
        }
    }
}
