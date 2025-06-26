using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QD.ERP.Web.Service;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QDERPWeb.Areas.Finance.Pages
{
    public class ChartOfAccountsModel : PageModel
    {
        private readonly UserAccessService _accessService;

        public ChartOfAccountsModel(UserAccessService accessService)
        {
            _accessService = accessService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return RedirectToTenantAccessDenied();
            }

            var accessList = await _accessService.GetFinanceMenuAccessAsync(userId);
            var menuItem = accessList.Find(x =>
    x.ItemDescription?.Trim().Equals("Chart Of Accounts", StringComparison.OrdinalIgnoreCase) == true);


            if (menuItem == null || !menuItem.ItemVisible || !menuItem.ItemEnabled)
            {
                return RedirectToTenantAccessDenied();
            }

            return Page(); // Access granted
        }

        private RedirectResult RedirectToTenantAccessDenied()
        {
            var path = HttpContext.Request.Path.Value;
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var tenant = segments.Length > 0 ? Uri.EscapeDataString(segments[0]) : "Default";

            return new RedirectResult($"/{tenant}/Finance/AccessDenied");
        }
    }
}
