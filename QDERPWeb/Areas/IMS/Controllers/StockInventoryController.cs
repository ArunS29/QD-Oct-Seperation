using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;

namespace QD.ERP.Web.Areas.IMS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class StockInventoryController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<StockInventoryController> _logger;

        public StockInventoryController(ILogger<StockInventoryController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions, string filterType = null)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var query = dbContext.Qry60001inventoryStockViews.Select(i => new
                    {
                        i.Gscode,
                        i.Gsdescrpition,
                        i.GsgroupName,
                        i.ItemPartNo,
                        i.ClosingBalance,
                        i.TotalReceived,
                        i.TotalIssues,
                        i.GssellingRate,
                        i.UnitType,
                        i.ReorderLevel,
                        i.ReorderQty,
                        i.CostPrice,
                        i.IsDiscontinued
                    
                    });



                    var result = await DataSourceLoader.LoadAsync(query, loadOptions);
                    return Json(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in Get: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while processing the request.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
    }
}
