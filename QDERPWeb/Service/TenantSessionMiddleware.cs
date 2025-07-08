using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
namespace QD.ERP.Web.Service
{
    public class TenantSessionMiddleware
    {

        private readonly RequestDelegate _next;

        public TenantSessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Get tenant from route (adjust key as per your route setup)
            var routeTenant = context.GetRouteValue("tenantName")?.ToString();
            var cookieTenant = context.Request.Cookies["CurrentTenant"];

            if (!string.IsNullOrEmpty(routeTenant) && !string.IsNullOrEmpty(cookieTenant) && !string.Equals(routeTenant, cookieTenant, System.StringComparison.OrdinalIgnoreCase))
            {
                // Clear session and cookies
                context.Session.Clear();
                context.Response.Cookies.Delete("AuthToken");
                context.Response.Cookies.Delete("Permissions");
                context.Response.Cookies.Delete("CurrentTenant");

                // Redirect to login page for the current tenant
                var loginUrl = $"/{routeTenant}/Security/Login";
                context.Response.Redirect(loginUrl);
                return;
            }

            await _next(context);
        }
    }
}
