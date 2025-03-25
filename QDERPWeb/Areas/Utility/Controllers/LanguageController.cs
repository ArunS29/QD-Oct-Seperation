using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.Service;
using System;

namespace QD.ERP.Web.Areas.Utility.Controllers
{
    [ApiController]
    [Route("api/language")]
    public class LanguageController : ControllerBase
    {
        private readonly LanguageService _languageService;

        public LanguageController(LanguageService languageService)
        {
            _languageService = languageService ?? throw new ArgumentNullException(nameof(languageService));
        }

        [HttpGet("GetLanguages")]
        public IActionResult GetLanguages()
        {
            var languages = _languageService.GetLanguages();
            return Ok(languages);
        }
    }
}
