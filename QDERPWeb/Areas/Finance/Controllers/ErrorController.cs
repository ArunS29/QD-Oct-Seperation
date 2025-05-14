using Microsoft.AspNetCore.Mvc;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [Route("Error/404")]
        public IActionResult PageNotFound()
        {
            return View();
        }
    }
}
