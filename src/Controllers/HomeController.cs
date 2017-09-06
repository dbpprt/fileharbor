using Microsoft.AspNetCore.Mvc;

namespace Fileharbor.Controllers
{
    public class HomeController : BaseController
    {
        [HttpGet, Route("deprecated"), ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Index()
        {
            return View();
        }
    }
}


