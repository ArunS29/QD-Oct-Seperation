using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace QD.ERP.Web.Areas.Security.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IConfiguration _config;

        public string TenantName { get; set; }
        public string LastPublishDate { get; set; }

        public LoginModel(IConfiguration config)
        {
            _config = config;
        }

        public void OnGet()
        {
            // Get tenant name from the route
            TenantName = RouteData.Values["tenantName"]?.ToString();

            // Get last publish date from Azure App Configuration (or appsettings.json)
            LastPublishDate = _config["App:LastPublishDate"] ?? "Not Set";
        }
    }
}
