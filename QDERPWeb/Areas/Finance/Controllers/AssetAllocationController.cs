using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AssetAllocationController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<AssetAllocationController> _logger;

        public AssetAllocationController(ILogger<AssetAllocationController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAssetCostAllocations(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                using (dbContext)
                {
                    var data = dbContext.Tbl20117AssetCostAllocationMasters
                        .Select(a => new
                        {
                            a.AssetCostAllocationId,
                            a.AssetCostAllocDrCr,
                            a.AssetNo,
                            a.EffectiveDate,
                            a.AmountAllocated,
                            a.AssetCostAllocRemarks
                        });

                    return Json(await DataSourceLoader.LoadAsync(data, loadOptions));
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetAssetCostAllocationById(int id)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var allocation = await dbContext.Tbl20117AssetCostAllocationMasters
                        .FirstOrDefaultAsync(a => a.AssetCostAllocationId == id);

                    if (allocation == null)
                        return NotFound(new { message = "Asset Cost Allocation not found.", success = false });

                    return Ok(new { success = true, data = allocation });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetAssetCostAllocationById: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while processing your request.", details = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
    }
}
