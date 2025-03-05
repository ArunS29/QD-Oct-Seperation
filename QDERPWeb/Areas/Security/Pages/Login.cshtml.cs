using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QD.ERP.Web.Areas.Security.Pages
{
    public class LoginModel : PageModel
    {
        public string TenantName { get; set; }

        //public string CompanyLogo { get; set; }

        public void OnGet()
        {
            // Get tenant name from the route
            TenantName = RouteData.Values["tenantName"]?.ToString();
            //CompanyLogo = RouteData.Values["companyLogo"]?.ToString();
        }
    }
}
