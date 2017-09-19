﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Fileharbor.Common;
using Fileharbor.Services.Contracts;
using Fileharbor.ViewModels.v1;
using Fileharbor.ViewModels.v1.Collections;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Fileharbor.Controllers.v1
{
    [Route("api/v1/collections")]
    public class CollectionController : BaseController
    {
        private readonly ICollectionService _collectionService;
        private readonly ICollectionTemplateService _collectionTemplateService;

        public CollectionController(ICollectionService collectionService,
            ICollectionTemplateService collectionTemplateService, CurrentPrincipal principal) : base(principal)
        {
            _collectionService = collectionService;
            _collectionTemplateService = collectionTemplateService;
        }
        
        [HttpPost, Route("")]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(Guid))]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, typeof(ExceptionResponse))]
        public async Task<IActionResult> Create([FromBody] CreateCollectionRequest model)
        {
            return Ok(await _collectionService.CreateCollectionAsync(model.Name, model.Description, model.IsDefault,
                null));
        }
        
        [HttpGet, Route("templates/{language}")]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(IEnumerable<CollectionTemplateResponse>))]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, typeof(ExceptionResponse))]
        public async Task<IActionResult> GetTemplates(int language)
        {
            return Ok((await _collectionTemplateService.GetTemplatesByLanguageAsync(language)).Select(_ =>
                new CollectionTemplateResponse
                {
                    Id = _.Id,
                    Name = _.Name,
                    Description = _.Description
                }));
        }
        
        [HttpPatch, Route("initialize/{collectionId}/{templateId}")]
        [SwaggerResponse((int) HttpStatusCode.NoContent, typeof(void))]
        [SwaggerResponse((int) HttpStatusCode.InternalServerError, typeof(ExceptionResponse))]
        public async Task<IActionResult> Initialize(Guid collectionId, Guid templateId)
        {
            await _collectionService.InitializeCollectionAsync(collectionId, templateId, null);
            return NoContent();
        }
        
        [HttpGet, Route("my")]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(IEnumerable<MyCollectionsResponse>))]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, typeof(ExceptionResponse))]
        public async Task<IActionResult> MyCollections()
        {
            return Ok((await _collectionService.GetMyCollectionsAsync(null)).Select(_ => new MyCollectionsResponse
            {
                Id = _.Item1,
                Name = _.Item2,
                PermissionLevel = _.Item3
            }));
        }
    }
}