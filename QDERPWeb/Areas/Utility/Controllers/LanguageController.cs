using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.Service;


namespace QD.ERP.Web.Areas.Utility.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LanguageController : Controller
    {
        private readonly LanguageService _languageService;

        public LanguageController(LanguageService languageService)
        {
            _languageService = languageService;
        }

        [HttpGet]
        public IActionResult GetLanguages()
        {
            var languages = _languageService.GetLanguages();
            return Ok(languages);
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
