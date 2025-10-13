using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using QD.ERP.Shared.DAL.Entities; 
using QDERPWeb.Models;         
using QD.ERP.Shared.Service;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Data.Utils;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class RequirePermissionAttribute : Attribute, IAsyncActionFilter
{
    private readonly string[] _permissions;

    public RequirePermissionAttribute(params string[] permissions)
    {
        _permissions = permissions;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Resolve services at runtime
        var tenantHelper = context.HttpContext.RequestServices.GetService<TenantDbContextHelper>();

        if (tenantHelper == null || 
            !tenantHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
        {
            context.Result = new StatusCodeResult(500); // Tenant DB not found
            return;
        }
        // read userId from claims
        var userIdClaim = context.HttpContext.User.FindFirst("UserId")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        try
        {
            // check permissions
            bool hasPermission = dbContext.TblUserAccessWebs.Any(p =>
            p.UserId == userId &&
            _permissions.Contains(p.ItemName) &&
            p.ItemEnabled == true
            && p.ItemVisible == true);

            if (!hasPermission)
            {
                context.Result = new JsonResult(new
                {
                    statusCode = 403,
                    Message = "You do not have permission to access this resource."
                })
                { StatusCode = 403 };
                return;
            }

            await next();
        }
        catch (Exception ex)
        {
            context.Result = new JsonResult(new
            {
                statusCode = 500,
                message = "An unexpected error occurred.",
                detailed = ex.Message
            })
            { StatusCode = 500 };
        }
    }
}
