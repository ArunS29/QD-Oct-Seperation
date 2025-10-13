using QD.ERP.Shared.Areas.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using QD.ERP.Shared.DAL.Entities;

namespace QD.ERP.Shared.Areas.Security.Pages
{
    public class ForgotPasswordModel : PageModel
    {
       


        public string TenantName { get; set; }

        public void OnGet()
        {
            TenantName = RouteData.Values["tenantName"]?.ToString();
        }

    }
}
