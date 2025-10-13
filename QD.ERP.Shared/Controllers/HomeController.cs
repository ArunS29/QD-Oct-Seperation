using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace QD.ERP.Shared.Controllers
{
    [Authorize] // Ensures only authenticated users can access this controller
    public class HomeController : Controller
    {
        public IActionResult Dashboard()
        {
            return View("/Pages/Dashboard.cshtml"); // Returns the Dashboard view
        }
    }
}
