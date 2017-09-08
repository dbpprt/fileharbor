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
    public class PermissionService : ServiceBase, IPermissionService
    {
        private readonly ILogger<PermissionService> _logger;

        public PermissionService(ILogger<PermissionService> logger, IDbConnection database)
            : base(database)
        {
            _logger = logger;
        }
        
    }
}
