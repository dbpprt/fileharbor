using System;
using System.Data;
using System.Data.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Dapper;
using Fileharbor.Common;
using Fileharbor.Common.Configuration;
using Fileharbor.Common.Database;
using Fileharbor.Common.Utilities;
using Fileharbor.Services.Contracts;
using Fileharbor.Services.Entities;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Fileharbor.Services
{
    [UsedImplicitly]
    public class ContentTypeService : ServiceBase, IContentTypeService
    {
        private readonly ILogger<ContentTypeService> _logger;

        public ContentTypeService(ILogger<ContentTypeService> logger, IDbConnection database)
            : base(database)
        {
            _logger = logger;
        }
        
    }
}
