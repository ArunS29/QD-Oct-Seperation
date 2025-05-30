using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Humanizer;
using DevExtreme.AspNet.Data.ResponseModel;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.Areas.Finance.Reports.Payable_Statements;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace QD.ERP.Web.Areas.IMS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class QuotationController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<QuotationController> _logger;
        public QuotationController(ILogger<QuotationController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetQuotation(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var query = dbContext.Qry60104quotationViewMasters.AsQueryable();


                    // Default dates if not provided
                    if (!fromDate.HasValue)
                    {
                        fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1); // Start of the current month
                    }

                    if (!toDate.HasValue)
                    {
                        toDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)); // End of the current month
                    }

                    // Filtering by date range
                    query = query.Where(i => i.QuoteDate >= fromDate && i.QuoteDate <= toDate);

                    // Fetching the data
                    var data = await query.Select(i => new
                    {
                        i.QuoteNo,
                        i.QuoteDate,
                        i.ClientName,
                        i.ClientRefNo,
                        i.Mprno,
                        i.SalesOrderNo,
                        i.SalesPersonName,
                        i.IsSubmitted,
                        i.IsApproved,
                        i.IsVerified,
                        i.NoOfItems,
                        i.TotalBeforeDiscount,
                        i.TotalWithTax,
                        i.TotalTaxAmount,
                        i.TotalAfterDiscount,
                    }).ToListAsync();

                    return Json(data);
                }

                return Unauthorized(new { message = "Invalid tenant." });

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error.", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCostItem()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var costItems = await dbContext.Tbl60105quotationCostMasters
                        .Select(s => new
                        {
                            s.CostItemCode,
                            s.CostItem
                        })
                        .ToListAsync();

                    return Json(costItems);
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCostItem");
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> SaveOrUpdateStatus([FromBody] Tbl60105quotationCostMaster model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.CostItem))
            {
                return BadRequest(new { success = false, message = "Invalid data received." });
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var existing = await dbContext.Tbl60105quotationCostMasters
                        .FirstOrDefaultAsync(x => x.CostItemCode == model.CostItemCode);

                    if (existing != null)
                    {
                        // Update existing record
                        existing.CostItem = model.CostItem;
                    }
                    else
                    {
                        string newCode = "A-001"; // Default fallback

                        try
                        {
                            // Fetch codes like A-001, A-002...
                            var codeList = await dbContext.Tbl60105quotationCostMasters
                                .Where(x => x.CostItemCode.StartsWith("A-") && x.CostItemCode.Length >= 5)
                                .Select(x => x.CostItemCode)
                                .ToListAsync();

                            // Extract last 3 digits and parse as int
                            var maxNumber = codeList
                                .Select(code =>
                                {
                                    string numericPart = code.Substring(code.Length - 3); // last 3 chars
                                    return int.TryParse(numericPart, out int result) ? result : 0;
                                })
                                .DefaultIfEmpty(0)
                                .Max();

                            int nextNumber = maxNumber + 1;
                            newCode = $"A-{nextNumber.ToString("D3")}"; // Format as A-001, A-002, etc.
                        }
                        catch
                        {
                            newCode = "A-001";
                        }

                        model.CostItemCode = newCode;
                        dbContext.Tbl60105quotationCostMasters.Add(model);
                    }

                    await dbContext.SaveChangesAsync();

                    return Ok(new
                    {
                        success = true,
                        message = "Saved successfully",
                        id = model.CostItemCode
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in SaveOrUpdateStatus");
                    return StatusCode(500, new { success = false, message = ex.Message });
                }
            }

            return Unauthorized(new { success = false, message = "Invalid tenant" });
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCostItem(String id)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var existing = await dbContext.Tbl60105quotationCostMasters
                        .FirstOrDefaultAsync(s => s.CostItemCode == id);

                    if (existing == null)
                    {
                        return NotFound(new { success = false, message = "CostItem not found" });
                    }

                    dbContext.Tbl60105quotationCostMasters.Remove(existing);
                    await dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Deleted successfully" });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in DeleteStatus: {ex}");
                    return StatusCode(500, new { success = false, message = ex.Message });
                }
            }

            return Unauthorized(new { success = false, message = "Invalid tenant" });
        }


    }
}
