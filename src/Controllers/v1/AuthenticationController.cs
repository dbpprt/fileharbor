using System;
using System.Net;
using System.Threading.Tasks;
using Fileharbor.Services.Contracts;
using Fileharbor.Services.Entities;
using Fileharbor.ViewModels.v1.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fileharbor.Controllers.v1
{
    [Route("api/v1/auth")]
    public class AuthenticationController : BaseController
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpPost, Route("token"), AllowAnonymous]
        public async Task<IActionResult> AcquireToken([FromBody]LoginRequest model)
        {
            return Json(new
            {
                token = await _authenticationService.AcquireTokenAsync(model.MailAddress, model.Password, null)
            });
        }

        [HttpPost, Route("register"), AllowAnonymous]
        public async Task<IActionResult> Register([FromBody]RegistrationRequest model)
        {
            (var id, var validated) = await _authenticationService.RegisterAsync(new UserEntity
            {
                MailAddress = model.MailAddress,
                GivenName = model.GivenName,
                SurName = model.SurName
            }, model.Password, null);

            return Json(new
            {
                id,
                success = id.HasValue,
                validated
            });
        }
    }
}
