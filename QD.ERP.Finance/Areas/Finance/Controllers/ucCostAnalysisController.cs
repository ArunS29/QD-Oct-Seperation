using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QD.ERP.Shared.DAL.Entities;
using QD.ERP.Shared.Service;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QD.ERP.Finance.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UCCostAnalysisController : ControllerBase
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<UCCostAnalysisController> _logger;

        public UCCostAnalysisController(ILogger<UCCostAnalysisController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult> GetUser()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var users = await dbContext.Qry20108ChartOfCostCenters.ToListAsync();
                return new JsonResult(users);
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        //[HttpGet]
        //public async Task<IActionResult> GetTrialBalance(DateTime? startDate, DateTime? endDate, string accountGroup)
        //{
        //    if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
        //    {
        //        try
        //        {
        //            var result = await dbContext.Qry20106CostAnalyses.ToListAsync();

        //            if (!string.IsNullOrEmpty(accountGroup))
        //            {
        //                result = result.Where(x => x.CostAllocationUnit == accountGroup).ToList();
        //            }

        //            if (startDate.HasValue)
        //            {
        //                result = result.Where(x => x.VoucherDate >= startDate.Value).ToList();
        //            }

        //            if (endDate.HasValue)
        //            {
        //                result = result.Where(x => x.VoucherDate <= endDate.Value).ToList();
        //            }

        //            var pivotGridData = result.Select(item => new
        //            {
        //                item.CostAllocationMasterGroup,
        //                item.CostAllocationGroup,
        //                item.CostAllocationUnit,
        //                item.CostAmount,
        //                item.Income,
        //                item.Expenses,

        //                // ✅ Format Date Fields to "MMM-yyyy"
        //                VoucherDate = item.VoucherDate != null ? item.VoucherDate.Value.ToString("MMM-yyyy") : null,
        //                EffectiveDate = item.EffectiveDate != null ? item.EffectiveDate.Value.ToString("MMM-yyyy") : null,
        //                AllocationEffectiveDate = item.AllocationEffectiveDate != null ? item.AllocationEffectiveDate.Value.ToString("MMM-yyyy") : null,

        //                item.CostAllocationId,
        //                item.VoucherEntryId,
        //                item.CostAllocationUnitId,
        //                item.CostAllocDrCr,
        //                item.AmountAllocated,
        //                item.CostAllocRemarks,
        //                item.IsDisabled,
        //                item.AccountHead,
        //                item.AccountGroup,
        //                item.MasterGroup,
        //                item.Pl,
        //                item.AccountId,
        //                item.VoucherNo,
        //                item.VoucherType,
        //                item.VoucherTypeAndNo,
        //                item.CostCenterIncharge,
        //                item.EntryNarration,
        //                item.SysRemarks,
        //                item.VoucherMonth,
        //                item.VoucherYear,
        //                item.EffectiveMonth,
        //                item.EffectiveYear,
        //                item.AllocationEffectiveMonth,
        //                item.AllocationEffectiveYear,
        //                item.ProjectMasterCode,
        //                item.BranchCode,
        //                item.BranchName,
        //                item.VoucherNarration,
        //                item.VoucherRefNo,

        //            }).ToList();

        //            return Ok(pivotGridData);
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError($"Error in GetTrialBalance: {ex.Message}");
        //            return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
        //        }
        //    }

        //    return Unauthorized(new { message = "Invalid tenant.", success = false });
        //}

        //[HttpGet]
        //public async Task<IActionResult> GetTrialBalance(DateTime? startDate, DateTime? endDate, string accountGroup)
        //{
        //    if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
        //    {
        //        try
        //        {
        //            var result = await dbContext.Qry20106CostAnalyses
        //                .AsNoTracking()
        //                .Where(x => (!startDate.HasValue || x.VoucherDate >= startDate.Value)
        //                         && (!endDate.HasValue || x.VoucherDate <= endDate.Value))
        //                .OrderBy(x => x.VoucherDate)
        //                .Skip(0)
        //                .Take(1000) // Optional: add pagination
        //                .Select(item => new
        //                {
        //                    item.CostAllocationMasterGroup,
        //                    item.CostAllocationGroup,
        //                    item.CostAllocationUnit,
        //                    item.CostAmount,
        //                    item.Income,
        //                    item.Expenses,
        //                    VoucherDate = item.VoucherDate,
        //                    EffectiveDate = item.EffectiveDate,
        //                    AllocationEffectiveDate = item.AllocationEffectiveDate,
        //                    item.CostAllocationId,
        //                    item.VoucherEntryId,
        //                    item.CostAllocationUnitId,
        //                    item.CostAllocDrCr,
        //                    item.AmountAllocated,
        //                    item.CostAllocRemarks,
        //                    item.IsDisabled,
        //                    item.AccountHead,
        //                    item.AccountGroup,
        //                    item.MasterGroup,
        //                    item.Pl,
        //                    item.AccountId,
        //                    item.VoucherNo,
        //                    item.VoucherType,
        //                    item.VoucherTypeAndNo,
        //                    item.CostCenterIncharge,
        //                    item.EntryNarration,
        //                    item.SysRemarks,
        //                    item.VoucherMonth,
        //                    item.VoucherYear,
        //                    item.EffectiveMonth,
        //                    item.EffectiveYear,
        //                    item.AllocationEffectiveMonth,
        //                    item.AllocationEffectiveYear,
        //                    item.ProjectMasterCode,
        //                    item.BranchCode,
        //                    item.BranchName,
        //                    item.VoucherNarration,
        //                    item.VoucherRefNo,
        //                })
        //                .ToListAsync();

        //            return Ok(result);
        //        }
        //        catch (TaskCanceledException ex)
        //        {
        //            _logger.LogError($"Timeout in GetTrialBalance: {ex.Message}");
        //            return StatusCode(504, new { message = "The request timed out. Please try again later.", error = ex.Message });
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError($"Error in GetTrialBalance: {ex.Message}");
        //            return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
        //        }
        //    }

        //    return Unauthorized(new { message = "Invalid tenant.", success = false });
        //}


        [HttpGet]
        public async Task<ActionResult> GetUsers()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var users = await dbContext.Qry70011projectCostAnalyses
                    .Where(u => u.BranchName != null && u.BranchCode != null)
                    .ToListAsync();

                return new JsonResult(users);
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetCostAllocationReport([FromQuery] string accountId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var query = dbContext.Qry20181CostAllocationWtLedgers.AsQueryable();

                    if (!string.IsNullOrEmpty(accountId))
                    {
                        query = query.Where(x => x.AccountId == accountId);
                    }

                    if (fromDate.HasValue)
                    {
                        query = query.Where(x => x.VoucherDate >= fromDate.Value);
                    }

                    if (toDate.HasValue)
                    {
                        query = query.Where(x => x.VoucherDate <= toDate.Value);
                    }

                    var result = await query.Select(item => new
                    {
                        item.CostAllocationMasterGroup,
                        item.CostAllocationGroup,
                        item.CostAllocationUnit,
                        item.CostAmount,
                        item.Income,
                        item.Expenses,
                        VoucherDate = item.VoucherDate != null ? item.VoucherDate.Value.ToString("MMM-yyyy") : null,
                        EffectiveDate = item.EffectiveDate != null ? item.EffectiveDate.Value.ToString("MMM-yyyy") : null,
                        AllocationEffectiveDate = item.AllocationEffectiveDate != null ? item.AllocationEffectiveDate.Value.ToString("MMM-yyyy") : null,
                        item.CostAllocationId,
                        item.VoucherEntryId,
                        item.CostAllocationUnitId,
                        item.CostAllocDrCr,
                        item.AmountAllocated,
                        item.CostAllocRemarks,
                       
                        item.AccountHead,
                        item.AccountGroup,
                        item.MasterGroup,
                        item.Pl,
                        item.AccountId,
                        item.VoucherNo,
                        item.VoucherType,
                       
                        item.VoucherMonth,
                        item.VoucherYear,
                        item.EffectiveMonth,
                        item.EffectiveYear,
                        item.AllocationEffectiveMonth,
                        item.AllocationEffectiveYear,
                        item.ProjectMasterCode,
                        item.BranchCode,
                        item.BranchName,
                        item.VoucherNarration,
                        item.VoucherRefNo,
                    }).ToListAsync();

                    return Ok(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetCostAllocationReport: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetLedgerName(string accountId)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return BadRequest("Tenant context could not be determined.");

            if (string.IsNullOrWhiteSpace(accountId))
                return BadRequest("Invalid AccountId");

            var ledger = await dbContext.Tbl201ChartOfAccounts
                .Where(a => a.AccountId == accountId)
                .Select(a => a.AccountHead)
                .FirstOrDefaultAsync();

            if (ledger == null)
                return NotFound("Ledger not found");

            return Ok(new { ledgerName = ledger });
        }

        [HttpPost]
        public async Task<ActionResult> DeleteWronglyAllocatedEntries()
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }

            try
            {
                await dbContext.Database.ExecuteSqlRawAsync("EXEC sp201_93DeleteWronglyAllocatedEntries");
                return Ok(new { message = "Entries deleted successfully.", success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCashFlowReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    if (!startDate.HasValue || !endDate.HasValue)
                        return BadRequest(new { message = "Start date and end date are required." });

                    var startParam = new SqlParameter("@StartDate", startDate.Value);
                    var endParam = new SqlParameter("@EndDate", endDate.Value);

                    // Execute the SP that fills tbl20154CashFlowMaster
                    await dbContext.Database.ExecuteSqlRawAsync("EXEC sp20156CashFlowMasterReport @StartDate, @EndDate", startParam, endParam);

                    // Fetch the data from the filled table
                    var result = await dbContext.Tbl20154CashFlowMasters
                        .Select(item => new
                        {
                            item.AccountHead,
                            item.AccountName,
                            item.VoucherFormattedAmount,
                            item.AccountGroup,
                            item.MasterGroup,
                            item.TransactionsFull,
                            item.VoucherNo
                        })
                        .ToListAsync();

                    return Ok(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetCashFlowReport: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }


    }
}







