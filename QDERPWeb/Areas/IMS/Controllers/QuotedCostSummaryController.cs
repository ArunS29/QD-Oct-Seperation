using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using QD.ERP.Web.Services.Logging;
using System.Text.Json;



namespace QD.ERP.Web.Areas.IMS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class QuotedCostSummaryController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<QuotedCostSummaryController> _logger;
        private readonly IUserActionLogger _userActionLogger;


        public QuotedCostSummaryController(ILogger<QuotedCostSummaryController> logger, TenantDbContextHelper tenantDbContextHelper , IUserActionLogger userActionLogger)
        {
            _userActionLogger = userActionLogger;
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









        [HttpPost]
        public async Task<IActionResult> InsertCostDistribution([FromBody] List<InsertCostDistributionDto> models)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                foreach (var model in models)
                {
                    var existing = await dbContext.Tbl60106quoteCostDistributions
                        .FirstOrDefaultAsync(x => x.QuoteChildId == model.QuoteChildId);

                    if (existing != null)
                    {
                        // Update fields
                        existing.QuotationNo = model.QuotationNo;
                        existing.Gscode = model.Gscode;
                        existing.QuotedQuantity = model.QuotedQuantity;
                        existing.QuotedCostPrice = model.QuotedCostPrice;
                        existing.PostingAmount = model.PostingAmount;
                        existing.PostingPercentage = model.PostingPercentage;
                        existing.PostingCostItemCode = model.PostingCostItemCode;
                        existing.TotalCostOfItemInclAll = model.TotalCostOfItemInclAll;

                        dbContext.Update(existing);
                    }
                    else
                    {
                        var entry = new Tbl60106quoteCostDistribution
                        {
                            QuotationNo = model.QuotationNo,
                            QuoteChildId = model.QuoteChildId,
                            Gscode = model.Gscode,
                            QuotedQuantity = model.QuotedQuantity,
                            QuotedCostPrice = model.QuotedCostPrice,
                            PostingAmount = model.PostingAmount,
                            PostingPercentage = model.PostingPercentage,
                            PostingCostItemCode = model.PostingCostItemCode,
                            TotalCostOfItemInclAll = model.TotalCostOfItemInclAll
                        };

                        await dbContext.AddAsync(entry);
                    }
                }

                await dbContext.SaveChangesAsync();
                await _userActionLogger.LogAsync(
                  module: "IMS > Insert Cost Distribution",
                   actionDetail: $":Inserted Cost Distribution {models[0].QuotationNo}",
                   documentNo: $"{models[0].QuotationNo}"
                );
                return Ok();
            }

            return Unauthorized();
        }

        [HttpPost]
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

        [HttpPost]
        public async Task<IActionResult> DistributeByProportion([FromBody] string quotationNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var userName = HttpContext.Session.GetString("UserName") ?? "System";

                    var query = $@"
INSERT INTO Tbl601_04quotationItemCosts 
(QuoteChildID, QuoteNo, GSCode, CostItemCode, CostPercentage, CostItemPrice, CostItemQty, AddedBy, AddedOn)
SELECT QuoteChildID, QuotationNo, GSCode, PostingCostItemCode, PorpoDistPercentage, PropoDistributed, 1, '{userName}', GETDATE()
FROM qry601_21QuoteDistributionMaster02 
WHERE QuotationNo = '{quotationNo}'";

                    await dbContext.Database.ExecuteSqlRawAsync(query);
                    return Ok();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"DistributeByProportion Error: {ex.Message}");
                    return StatusCode(500, new { error = ex.Message });
                }
            }

            return Unauthorized();
        }

        [HttpPost]
        public async Task<IActionResult> DistributeByPercentage([FromBody] string quotationNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var userName = HttpContext.Session.GetString("UserName") ?? "System";

                    var query = $@"
INSERT INTO Tbl601_04quotationItemCosts 
(QuoteChildID, QuoteNo, GSCode, CostItemCode, CostPercentage, CostItemPrice, CostItemQty, AddedBy, AddedOn)
SELECT QuoteChildID, QuotationNo, GSCode, PostingCostItemCode, PostingPercentage, DistributedByPercentage, 1, '{userName}', GETDATE()
FROM qry601_21QuoteDistributionMaster02 
WHERE QuotationNo = '{quotationNo}'";

                    await dbContext.Database.ExecuteSqlRawAsync(query);
                    return Ok();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"DistributeByPercentage Error: {ex.Message}");
                    return StatusCode(500, new { error = ex.Message });
                }
            }

            return Unauthorized();
        }





        // ✅ ADD THIS METHOD BELOW
        private async Task<IActionResult> ExecuteUpdateQuery(string quotationNo, string rawSqlTemplate)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var sql = string.Format(rawSqlTemplate, quotationNo.Replace("'", "''")); // prevent SQL injection
                    await dbContext.Database.ExecuteSqlRawAsync(sql);
                    return Ok();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Distribution Update Error for Quotation {quotationNo}: {ex.Message}");
                    return StatusCode(500, new { error = ex.Message });
                }
            }

            return Unauthorized();
        }

        // 🟢 Then your methods like this will work:
        [HttpPost]
        public async Task<IActionResult> ProfitMarginByEqual([FromBody] string quotationNo)
        {
            return await ExecuteUpdateQuery(quotationNo, @"
            UPDATE T
            SET T.QuotedUnitPrice = Q.ProfitMarginPerItemByEqual
            FROM tbl601_02QuotationChild T
            INNER JOIN qry601_23QuoteDistributionOfProfitMargin Q
            ON T.QuoteChildID = Q.QuoteChildID
            WHERE T.QuoteNo = '{0}'
        ");
        }

    

        [HttpPost]
        public async Task<IActionResult> ProfitMarginByProportion([FromBody] string quotationNo)
        {
            return await ExecuteUpdateQuery(quotationNo, @"
        UPDATE T
        SET T.QuotedUnitPrice = Q.ProfitMarginPerItemByProportion
        FROM tbl601_02QuotationChild T
        INNER JOIN qry601_23QuoteDistributionOfProfitMargin Q
        ON T.QuoteChildID = Q.QuoteChildID
        WHERE T.QuoteNo = '{0}'
    ");
        }

        [HttpPost]
        public async Task<IActionResult> ProfitMarginByPercentage([FromBody] string quotationNo)
        {
            return await ExecuteUpdateQuery(quotationNo, @"
        UPDATE T
        SET T.QuotedUnitPrice = Q.ProfitMarginPerItemByPercentage
        FROM tbl601_02QuotationChild T
        INNER JOIN qry601_23QuoteDistributionOfProfitMargin Q
        ON T.QuoteChildID = Q.QuoteChildID
        WHERE T.QuoteNo = '{0}'
    ");
        }

        [HttpPost]
        public async Task<IActionResult> DiscountByEqual([FromBody] string quotationNo)
        {
            return await ExecuteUpdateQuery(quotationNo, @"
        UPDATE T
        SET T.QuotedDiscount = Q.DiscountByEqual
        FROM tbl601_02QuotationChild T
        INNER JOIN qry601_21QuoteDistributionMaster02 Q
        ON T.QuoteChildID = Q.QuoteChildID
        WHERE T.QuoteNo = '{0}'
    ");
        }

        [HttpPost]
        public async Task<IActionResult> DiscountByProportion([FromBody] string quotationNo)
        {
            return await ExecuteUpdateQuery(quotationNo, @"
        UPDATE T
        SET T.QuotedDiscount = Q.DiscountByProportion
        FROM tbl601_02QuotationChild T
        INNER JOIN qry601_21QuoteDistributionMaster02 Q
        ON T.QuoteChildID = Q.QuoteChildID
        WHERE T.QuoteNo = '{0}'
    ");
        }

        [HttpPost]
        public async Task<IActionResult> DiscountByPercentage([FromBody] string quotationNo)
        {
            return await ExecuteUpdateQuery(quotationNo, @"
        UPDATE T
        SET T.QuotedDiscount = Q.DiscountByPercentage
        FROM tbl601_02QuotationChild T
        INNER JOIN qry601_21QuoteDistributionMaster02 Q
        ON T.QuoteChildID = Q.QuoteChildID
        WHERE T.QuoteNo = '{0}'
    ");
        }

    }

}


