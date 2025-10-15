using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QD.ERP.Shared.DAL.Entities;
using QD.ERP.Shared.Service;

namespace QD.ERP.ERM.Areas.ERM.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ERQuotedCostSummaryController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<ERQuotedCostSummaryController> _logger;

        public ERQuotedCostSummaryController(ILogger<ERQuotedCostSummaryController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }


        [HttpGet]
        public async Task<IActionResult> GetQuotedCostSummary(string QuoteNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var result = await dbContext.Qry60102quotationChildren
                        .Where(i => i.QuoteNo == QuoteNo)
                        .ToListAsync();

                    return Json(result);
                }
                catch (Exception ex)
                {

                    _logger.LogError($"Error in GetQuotedCostSummary: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
                }
            }

            return Unauthorized();
        }

        [HttpGet]
        public async Task<IActionResult> GetquotationCostList(string QuoteNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var result = await dbContext.Qry60120quotationCostList02s
                        .Where(i => i.QuoteNo == QuoteNo)
                        .ToListAsync();

                    return Json(result);
                }
                catch (Exception ex)
                {

                    _logger.LogError($"Error in GetQuotedCostSummary: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
                }
            }
 
            return Unauthorized();
        }
        [HttpGet]
        public async Task<IActionResult> GetCostItem(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var result = dbContext.Tbl60105quotationCostMasters.Select(i => new
                    {
                        i.CostItemCode,
                        i.CostItem
                    });

                    return Json(await DataSourceLoader.LoadAsync(result, loadOptions));


                }


                catch (Exception ex)
                {

                    _logger.LogError($"Error in GetCostItem: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
                }
            }

            return Unauthorized();
        }
    }
}
