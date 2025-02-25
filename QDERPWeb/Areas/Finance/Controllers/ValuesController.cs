using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QD.ERP.Web.Areas.Finance.Models;  // Assuming you have models in this area

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Area("Finance")]  // Define the area where the controller belongs
    public class ValuesController : Controller
    {
        private readonly ILogger<ValuesController> _logger;

        // Injecting logger into the controller for logging purposes
        public ValuesController(ILogger<ValuesController> logger)
        {
            _logger = logger;
        }

        // GET: Finance/Values/Index
        public IActionResult Index()
        {
            _logger.LogInformation("ValuesController Index action called");

            // Returning a simple view with a model (e.g., a list of values)
            var valuesModel = new ValuesView
            {
                Values = new[] { "value1", "value2", "value3" }
            };

            // Log information about the model being passed
            _logger.LogInformation("Passing model to the view: {@ValuesModel}", valuesModel);

            return View(valuesModel); // Return the view with the model
        }
    }
}
