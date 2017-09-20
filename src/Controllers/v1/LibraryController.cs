using System;
using System.Linq;
using System.Threading.Tasks;
using Fileharbor.Common;
using Fileharbor.Services.Contracts;
using Fileharbor.Services.Entities;
using Fileharbor.ViewModels.v1.Libraries;
using Microsoft.AspNetCore.Mvc;

namespace Fileharbor.Controllers.v1
{
    [Route("api/v1/libraries")]
    public class LibraryController : BaseController
    {
        private readonly ILibraryService _librayService;

        public LibraryController(ILibraryService librayService, CurrentPrincipal principal) : base(principal)
        {
            _librayService = librayService;
        }

        [HttpGet]
        [Route("{collectionId}")]
        public async Task<IActionResult> Get(Guid collectionId)
        {
            return Ok((await _librayService.GetLibraries(collectionId, null)).Select(_ =>
                new LibraryResponse
                {
                    Id = _.Id,
                    Name = _.Name,
                    Description = _.Description
                }));
        }

        [HttpPost]
        [Route("{collectionId}")]
        public async Task<IActionResult> Create([FromBody] CreateLibraryRequest model, Guid collectionId)
        {
            return Ok(await _librayService.CreateLibraryAsync(collectionId, model.Name, model.Description,
                LibraryType.Custom, false, null));
        }

        [HttpPost]
        [Route("{collectionId}/{libraryId}/{contentTypeId}")]
        public async Task<IActionResult> AssignContentType(Guid collectionId, Guid libraryId, Guid contentTypeId)
        {
            await _librayService.AssignContentTypeMappingAsync(collectionId, libraryId, contentTypeId, null);
            return NoContent();
        }
    }
}