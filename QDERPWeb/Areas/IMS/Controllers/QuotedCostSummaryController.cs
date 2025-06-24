using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;


namespace QD.ERP.Web.Areas.IMS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class QuotedCostSummaryController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<QuotedCostSummaryController> _logger;

        public QuotedCostSummaryController(ILogger<QuotedCostSummaryController> logger, TenantDbContextHelper tenantDbContextHelper)
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


        [HttpPost("InsertCostDistribution")]
        public async Task<IActionResult> InsertCostDistribution([FromBody] JObject model)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var quotationNo = model["QuotationNo"]?.ToString();
                    var quoteChildID = (int)model["QuoteChildID"];
                    var gsCode = model["GSCode"]?.ToString();
                    var quotedQuantity = (decimal)model["QuotedQuantity"];
                    var quotedCostPrice = (decimal)model["QuotedCostPrice"];
                    var postingAmount = (decimal)model["PostingAmount"];
                    var postingPercentage = (decimal)model["PostingPercentage"];
                    var postingCostItemCode = model["PostingCostItemCode"]?.ToString();
                    var totalCostOfItemInclAll = (decimal)model["TotalCostOfItemInclAll"];

                    var sql = $@"
                INSERT INTO Tbl601_06quoteCostDistribution
                (QuotationNo, QuoteChildID, GSCode, QuotedQuantity, QuotedCostPrice,
                 PostingAmount, PostingPercentage, PostingCostItemCode, TotalCostOfItemInclAll)
                VALUES (
                    '{quotationNo}', {quoteChildID}, '{gsCode}', {quotedQuantity}, {quotedCostPrice},
                    {postingAmount}, {postingPercentage}, '{postingCostItemCode}', {totalCostOfItemInclAll}
                )";

                    await dbContext.Database.ExecuteSqlRawAsync(sql);
                    return Ok();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"InsertCostDistribution Error: {ex.Message}");
                    return StatusCode(500, new { error = ex.Message });
                }
            }

            return Unauthorized();
        }
        [HttpPost("DistributeEqually")]
        public async Task<IActionResult> DistributeEqually([FromBody] string quotationNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var userName = HttpContext.Session.GetString("UserName") ?? "System";

                    // Example:
                    var query = $@"
    INSERT INTO Tbl601_04quotationItemCosts 
    (QuoteChildID, QuoteNo, GSCode, CostItemCode, CostPercentage, CostItemPrice, CostItemQty, AddedBy, AddedOn)
    SELECT QuoteChildID, QuotationNo, GSCode, PostingCostItemCode, EqualPercentage, EquallyDistributed, 1, '{userName}', GETDATE()
    FROM qry601_21QuoteDistributionMaster02 
    WHERE QuotationNo = '{quotationNo}'
";


                    await dbContext.Database.ExecuteSqlRawAsync(query);
                    return Ok();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"DistributeEqually Error: {ex.Message}");
                    return StatusCode(500, new { error = ex.Message });
                }
            }

            return Unauthorized();
        }

    }
}
