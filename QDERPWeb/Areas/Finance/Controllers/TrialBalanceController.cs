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
    public class TrialBalanceController : ControllerBase
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<TrialBalanceController> _logger;

        public TrialBalanceController(ILogger<TrialBalanceController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllowDataModeSelection()
        {
            var defaultCompanyIdStr = HttpContext.Session.GetString("DefaultcompanyID");

            if (string.IsNullOrEmpty(defaultCompanyIdStr) || !int.TryParse(defaultCompanyIdStr, out int defaultCompanyId))
            {
                return BadRequest(new { message = "Company ID is missing or invalid.", success = false });
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var allow = await dbContext.Tbl901CompanyDetails
                    .Where(c => c.CompanyId == defaultCompanyId)
                    .Select(c => c.AllowDataModeSelection)
                    .FirstOrDefaultAsync();

                return Ok(new { allowDataMode = allow });
            }

            return Unauthorized(new { message = "Invalid context.", success = false });
        }

        [HttpGet]
        public async Task<ActionResult> GetUser()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var users = await dbContext.Tbl201AccountGroups.ToListAsync();
                return new JsonResult(users);
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetTrialBalances(DateTime? startDate, DateTime? endDate, string accountGroup)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    bool? isUseEffectiveDate = false;

                    // Execute stored procedure using FromSqlRaw
                    var result = await dbContext.TrialBalanceResults
                        .FromSqlRaw("EXEC StProTrialBalance @p0, @p1, @p2", startDate, endDate, isUseEffectiveDate)
                        .ToListAsync();

                    // Apply filters dynamically
                    if (!string.IsNullOrEmpty(accountGroup))
                    {
                        result = result.Where(x => x.AccountGroup == accountGroup).ToList();
                    }
                    else
                    {
                        if (startDate.HasValue)
                            result = result.Where(x => x.VoucherDate >= startDate.Value).ToList();

                        if (endDate.HasValue)
                            result = result.Where(x => x.VoucherDate <= endDate.Value).ToList();
                    }

                    // Convert to JSON response format
                    var pivotGridData = result.Select(item => new
                    {
                        item.VoucherNo,
                        item.VoucherDate,
                        item.VoucherEntryNo,
                        item.AccountHead,
                        item.AccountHeadName,
                        item.DrCr,
                        item.DrAmount,
                        item.CrAmount,
                        item.VoucherAmountFormatted,
                        item.AccountGroup,
                        item.MasterGroup,
                        item.VoucherType,
                        item.Transactions,
                        item.MonthYear,
                        item.MonthNumber,
                        item.Category,
                        item.VoucherRefNo,
                        item.VoucherNarration,
                        item.EntryNarration,
                        item.SysRemarks
                    }).ToList();

                    return Ok(pivotGridData);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetTrialBalance: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet]
        public async Task<IActionResult> GetOfflineTrialBalances(DateTime? startDate, DateTime? endDate, string accountGroup)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var result = await dbContext.FinancialSummaryReports
                        .Where(x =>
                            (!startDate.HasValue || x.VoucherDate >= startDate.Value) &&
                            (!endDate.HasValue || x.VoucherDate <= endDate.Value))
                        .ToListAsync();

                    if (!string.IsNullOrEmpty(accountGroup))
                    {
                        result = result.Where(x => x.AccountGroup == accountGroup).ToList();
                    }

                    var simplifiedData = result.Select(item => new
                    {
                        item.AccountHead,
                        item.AccountHeadName,
                        item.VoucherAmountFormatted,
                        item.AccountGroup,
                        item.MasterGroup,
                        TransactionsFull = item.TransactionsFull ?? "N/A",
                        AccountHeadArabic = item.AccountHeadArabic ?? string.Empty,
                        AccountGroupAr = item.AccountGroupAr ?? string.Empty,
                        MasterGroupAr = item.MasterGroupAr ?? string.Empty
                    });


                    return Ok(simplifiedData);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetOfflineTrialBalances: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }



    }
}








