using Microsoft.AspNetCore.Mvc;

namespace QD.ERP.Web.Areas.VAT.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
