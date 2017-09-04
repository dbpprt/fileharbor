using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.PlatformAbstractions;

namespace Fileharbor.Controllers.v1
{
    [Route("api/v1/sys")]
    public class SystemController : BaseController
    {
        [HttpGet, Route("version")]
        public string Get()
        {
            return PlatformServices.Default.Application.ApplicationVersion;
        }
    }
}
