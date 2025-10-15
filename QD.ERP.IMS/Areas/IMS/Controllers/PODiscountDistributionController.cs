using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using QD.ERP.Shared.Models;
using QD.ERP.Shared.DAL.Entities;
using QD.ERP.Shared.Service;
using QD.ERP.Shared.Services.Logging;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace QD.ERP.IMS.Areas.IMS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PODiscountDistributionController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<PODiscountDistributionController> _logger;
        private readonly IUserActionLogger _userActionLogger;


        public PODiscountDistributionController(ILogger<PODiscountDistributionController> logger, TenantDbContextHelper tenantDbContextHelper, IUserActionLogger userActionLogger)
        {
            _userActionLogger = userActionLogger;
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetPODiscountDistribution(string PONo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var result = await dbContext.Qry60402purchaseOrderChildren
                        .Where(i => i.Pono == PONo)
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
        [HttpPost] 
        public async Task<IActionResult> InsertDiscountDistribution([FromBody] List<PODiscountDistributionDto> models)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(
                out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized();

            if (models == null || models.Count == 0)
                return BadRequest(new { message = "No data provided." });

            try
            {
                string poNo = models.First().PONo;
                int distributionMethod = models.First().MethodType; // pass from frontend

                foreach (var dto in models)
                {
                    var existing = await dbContext.Tbl60403purchaseOrderCostDistributions
                        .FirstOrDefaultAsync(x =>
                            x.Pono == dto.PONo &&
                            x.PochildNo == dto.POChildNo &&
                            x.PostingCostItemCode == dto.PostingCostItemCode);

                    if (existing != null)
                    {
                        // Update existing
                        existing.PostingAmount = dto.PostingAmount;
                        existing.PostingPercentage = dto.PostingPercentage;
                        existing.TotalCostOfItemInclAll = dto.TotalCostOfItemInclAll;
                        // update other fields if needed
                    }
                    else
                    {
                        // Insert new
                        var entity = new Tbl60403purchaseOrderCostDistribution
                        {
                            Pono = dto.PONo,
                            PochildNo = dto.POChildNo,
                            QuotedQuantity = dto.QuotedQuantity,
                            UnitPrice = dto.UnitPrice,
                            PostingAmount = dto.PostingAmount,
                            PostingPercentage = dto.PostingPercentage,
                            Gscode = dto.GSCode,
                            PostingCostItemCode = dto.PostingCostItemCode,
                            TotalCostOfItemInclAll = dto.TotalCostOfItemInclAll
                        };
                        dbContext.Tbl60403purchaseOrderCostDistributions.Add(entity);
                    }
                }

           

                await dbContext.SaveChangesAsync();

                // Call distribution logic based on method
                switch (distributionMethod)
                {
                    case 1:
                        await DiscountByEqual(dbContext, poNo);
                        break;
                    case 2:
                        await DiscountByProportion(dbContext, poNo);
                        break;
                    case 3:
                        await DiscountByPercentage(dbContext, poNo);
                        break;
                    default:
                        return BadRequest(new { message = "Invalid distribution method." });
                }

                return Ok(new { message = "Discount has been successfully distributed for the selected items.", poNo });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in InsertDiscountDistribution");
                return StatusCode(500, new { message = "Error inserting discount distribution." });
            }
        }

        private async Task DiscountByEqual(ERPMasterWtDataContext dbContext, string poNo)
        {
            string sql = @"
            UPDATE tbl604_02PurchaseOrderChild
            SET ItemDiscount = DiscountByEqual
            FROM dbo.tbl604_02PurchaseOrderChild
            INNER JOIN dbo.qry604_12PODiscountDistribution02
                ON dbo.tbl604_02PurchaseOrderChild.POChildNo = dbo.qry604_12PODiscountDistribution02.POChildNo
            WHERE dbo.tbl604_02PurchaseOrderChild.PONo = {0}";

            await dbContext.Database.ExecuteSqlRawAsync(sql, poNo);
        }

        private async Task DiscountByProportion(ERPMasterWtDataContext dbContext, string poNo)
        {
            string sql = @"
            UPDATE tbl604_02PurchaseOrderChild
            SET ItemDiscount = DiscountByProportion
            FROM dbo.tbl604_02PurchaseOrderChild
            INNER JOIN dbo.qry604_12PODiscountDistribution02
                ON dbo.tbl604_02PurchaseOrderChild.POChildNo = dbo.qry604_12PODiscountDistribution02.POChildNo
            WHERE dbo.tbl604_02PurchaseOrderChild.PONo = {0}";

            await dbContext.Database.ExecuteSqlRawAsync(sql, poNo);
        }

        private async Task DiscountByPercentage(ERPMasterWtDataContext dbContext, string poNo)
        {
            string sql = @"
            UPDATE tbl604_02PurchaseOrderChild
            SET ItemDiscount = DiscountByPercentage,
                DiscountInOC = DiscountByPercentageInOC
            FROM dbo.tbl604_02PurchaseOrderChild
            INNER JOIN dbo.qry604_12PODiscountDistribution02
                ON dbo.tbl604_02PurchaseOrderChild.POChildNo = dbo.qry604_12PODiscountDistribution02.POChildNo
            WHERE dbo.tbl604_02PurchaseOrderChild.PONo = {0}";

            await dbContext.Database.ExecuteSqlRawAsync(sql, poNo);
        }
    
}
}
