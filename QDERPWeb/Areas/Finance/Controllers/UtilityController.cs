using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System.Linq;
using System.Threading.Tasks;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UtilityController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<UtilityController> _logger;

        public UtilityController(ILogger<UtilityController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult> SaveLayout(string layout, string form)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    ERPMasterWtDataContextProcedures _procedures = new ERPMasterWtDataContextProcedures(dbContext);
                    var ledgerData = await _procedures.sp901_01UpdateLayoutAsync(layout, form, "101", true);
                    return Json(ledgerData);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in SaveLayout: {ex.Message}");
                    return StatusCode(500, new { success = false, message = "An error occurred while saving the layout.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<ActionResult> LoadLayout(string form)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var layout = dbContext.Tbl90111LayoutMasters
                        .Where(p => p.UserId == 101 && p.FormId == form)
                        .Select(i => new
                        {
                            i.LayoutJson
                        }).FirstOrDefault();

                    if (layout != null && layout.LayoutJson != null)
                    {
                        return Json(layout.LayoutJson);
                    }
                    else
                    {
                        return Json(null);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in LoadLayout: {ex.Message}");
                    return StatusCode(500, new { success = false, message = "An error occurred while loading the layout.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
    }
}






