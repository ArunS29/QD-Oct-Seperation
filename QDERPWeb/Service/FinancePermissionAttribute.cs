// Attribute class to restrict access by menu key using UserAccessService
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using QD.ERP.Web.Service;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace QD.ERP.Web.Service
{
    public class FinancePermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _menuKey;

        public FinancePermissionAttribute(string menuKey)
        {
            _menuKey = menuKey;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            var userIdClaim = user.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Home", null);
                return;
            }

            var accessService = context.HttpContext.RequestServices.GetRequiredService<UserAccessService>();
            var accessList = await accessService.GetFinanceMenuAccessAsync(userId);

            var menuAccess = accessList.FirstOrDefault(a => a.ItemDescription == _menuKey);
            if (menuAccess == null || !menuAccess.ItemVisible || !menuAccess.ItemEnabled)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Home", null);
            }
        }
    }

    // Base class for secure finance Razor Pages
    public abstract class SecureFinancePageModel : PageModel
    {
        protected readonly UserAccessService _accessService;

        protected SecureFinancePageModel(UserAccessService accessService)
        {
            _accessService = accessService;
        }

        protected async Task<IActionResult> EnsureFinancePermissionAsync(string itemName)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return RedirectToPage("/AccessDenied");
            }

            var accessList = await _accessService.GetFinanceMenuAccessAsync(userId);
            var menuAccess = accessList.FirstOrDefault(x => x.ItemName == itemName);

            if (menuAccess == null || !menuAccess.ItemVisible || !menuAccess.ItemEnabled)
            {
                return RedirectToPage("/AccessDenied");
            }

            return null; // Authorized
        }
    }
}
