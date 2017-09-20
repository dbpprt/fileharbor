using System;
using System.Linq;
using System.Threading.Tasks;
using Fileharbor.Common;
using Fileharbor.Services.Contracts;
using Fileharbor.ViewModels.v1.ContentTypes;
using Microsoft.AspNetCore.Mvc;

namespace Fileharbor.Controllers.v1
{
    [Route("api/v1/contenttypes")]
    public class ContentTypeController : BaseController
    {
        private readonly IContentTypeService _contentTypeService;

        public ContentTypeController(IContentTypeService contentTypeService, CurrentPrincipal principal) :
            base(principal)
        {
            _contentTypeService = contentTypeService;
        }

        [HttpGet]
        [Route("{collectionId}/infos")]
        public async Task<IActionResult> GetContentTypeInfos(Guid collectionId)
        {
            return Ok((await _contentTypeService.GetContentTypeInfosAsync(collectionId, null)).Select(_ =>
                new ContentTypeInfoResponse
                {
                    Id = _.Item1,
                    Name = _.Item2,
                    GroupName = _.Item3,
                    ParentId = _.Item4
                }));
        }

        [HttpGet]
        [Route("{collectionId}/{contentTypeId}/info")]
        public async Task<IActionResult> GetContentTypeInfo(Guid collectionId, Guid contentTypeId)
        {
            return Ok((await _contentTypeService.GetContentTypeInfoAsync(collectionId, contentTypeId, null)).Select(_ =>
                new ContentTypeInfoResponse
                {
                    Id = _.Item1,
                    Name = _.Item2,
                    GroupName = _.Item3,
                    ParentId = _.Item4
                }).FirstOrDefault());
        }
    }
}