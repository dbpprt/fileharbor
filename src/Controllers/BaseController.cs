using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Fileharbor.Common;
using Fileharbor.Exceptions;
using Fileharbor.Services.Schema;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Fileharbor.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class BaseController : Controller
    {
        private readonly CurrentPrincipal _currentPrincipal;

        public BaseController(CurrentPrincipal currentPrincipal)
        {
            _currentPrincipal = currentPrincipal;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (User != null && User.Identity.IsAuthenticated)
            {
                _currentPrincipal.IsAuthenticated = true;
                _currentPrincipal.Id = Guid.Parse(User.FindFirst(JwtRegisteredClaimNames.Sid).Value);
            }

            if (!ModelState.IsValid)
            {
                context.Result = new BadRequestObjectResult(ModelState);
            }
            else
            {
                base.OnActionExecuting(context);
            }
        }
    }
}
