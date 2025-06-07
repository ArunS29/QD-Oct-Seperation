using Microsoft.AspNetCore.Http;
using QD.ERP.Web.Service;
using System.Threading.Tasks;

public class LicenseValidationMiddleware
{
    private readonly RequestDelegate _next;

    public LicenseValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.ToString().ToLower();

        // Skip license check for certain paths
        if (path.StartsWith("/security/licenseactivation") ||
            path.StartsWith("/security/signin") ||
            path.StartsWith("/static") ||
            path.EndsWith(".js") || path.EndsWith(".css") ||
            path.EndsWith(".png") || path.EndsWith(".jpg") || path.EndsWith(".ico"))
        {
            await _next(context);
            return;
        }

        var licenseService = context.RequestServices.GetService<LicenseService>();

        // Extract subdomain (or use fallback for local dev)
        string host = context.Request.Host.Host;
        string companyName = host.Contains(".") ? host.Split('.')[0] : "Pulse1"; // fallback

        if (licenseService != null && !string.IsNullOrEmpty(companyName))
        {
            bool isValid = await licenseService.IsLicenseValidAsync(companyName);
            if (!isValid)
            {
                context.Response.Redirect("/pulse1/Security/LicenseActivation");
                return;
            }
        }

        await _next(context);
    }
}
