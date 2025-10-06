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
    public class TransactionDetailsController : ControllerBase
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<TransactionDetailsController> _logger;

        public TransactionDetailsController(ILogger<TransactionDetailsController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetVoucherForAudits()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var oneMonthAgo = DateTime.UtcNow.AddMonths(-1);

                    // Add filtering here for last 1 month
                    var result = await dbContext.Qry20176VouchersForAudits
                        .Where(item => item.VoucherDate >= oneMonthAgo)
                        .ToListAsync();

                    var pivotGridData = result.Select(item => new
                    {
                        item.VoucherNo,
                        item.AccountHeadName,
                        item.DrCr,
                        item.CrAmount,
                        item.DrAmount,
                        item.VoucherDate,
                        item.VoucherRefNo,
                        item.VoucherNarration,
                        item.VoucherEnteredBy,
                        item.VoucherEnteredOn,
                        item.VoucherVerifiedBy,
                        item.VoucherVerifiedOn,
                        item.VoucherApprovedBy,
                        item.VoucherApprovedOn,
                        item.VoucherEntryNo,
                        item.AccountHead,
                        item.VoucherAmountFormatted,
                        item.EntryNarration,
                        item.AccountGroup,
                        item.MasterGroup,
                        item.IsApproved,
                        item.VoucherType,
                        item.SysRemarks,
                        item.IsCalculateOpeningBalance,
                        item.BankClearedOn,
                        item.AccountGroupId,
                        item.MasterGroupId,
                        item.IsProfitLossAccount,
                        item.IsBalanceSheetAccount,
                        item.MasterOrderNo,
                        item.MasterGroupCategory,
                        item.VoucherEffectiveDate,
                        item.BillNo,
                        item.BillDate,
                        item.BillPaidTo,
                        item.BillRemarks,
                        item.VoucherModifiedBy,
                        item.AccountHeadArabic,
                        item.AccountGroupAr,
                        item.MasterGroupAr,
                        item.VoucherTypeAr,
                        item.ChartOfAccountsOrder,
                        item.BankTransaction,
                        item.BankTransactionCr,
                        item.BankDrTransaction,
                        item.BankCrTransaction,
                        item.CashCrTransaction,
                        item.CashDrTransaction,
                        item.CashTransactionCr,
                        item.CashTransactionDr,
                        item.PettyCashLedgerName
                    }).ToList();

                    return Ok(pivotGridData);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetVoucherForAudits: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }



        [HttpGet]
        public async Task<IActionResult> GetTrialBalance(DateTime? startDate, DateTime? endDate, string accountGroup)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                using (dbContext) // Ensures DbContext is properly disposed
                {
                    try
                    {
                        var company = dbContext.Tbl901CompanyDetails
                .FirstOrDefault();
                        var result = await dbContext.Qry20106CostAnalyses
                            .AsNoTracking()
                            .Where(x => (!startDate.HasValue || x.VoucherDate >= startDate.Value)
                                     && (!endDate.HasValue || x.VoucherDate <= endDate.Value)
                                     && (string.IsNullOrEmpty(accountGroup) || x.CostAllocationUnit == accountGroup))
                            .OrderBy(x => x.VoucherDate)
                            .Select(item => new
                            {
                                item.CostAllocationMasterGroup,
                                item.CostAllocationGroup,
                                item.CostAllocationUnit,
                                item.CostAmount,
                                item.Income,
                                item.Expenses,
                                item.VoucherDate,
                                item.EffectiveDate,
                                item.AllocationEffectiveDate,
                                item.CostAllocationId,
                                item.VoucherEntryId,
                                item.CostAllocationUnitId,
                                item.CostAllocDrCr,
                                item.AmountAllocated,
                                item.CostAllocRemarks,
                                item.IsDisabled,
                                item.AccountHead,
                                item.AccountGroup,
                                item.MasterGroup,
                                item.Pl,
                                item.AccountId,
                                item.VoucherNo,
                                item.VoucherType,
                                item.VoucherTypeAndNo,
                                item.CostCenterIncharge,
                                item.EntryNarration,
                                item.SysRemarks,
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
                                item.ConvertedIncome,
                                item.ConvertedExpenses,
                                item.ConvertedCostAmount,
                                company.CurrencyImage
                            })
                            .ToListAsync();

                        return Ok(result);
                    }
                    catch (TaskCanceledException ex)
                    {
                        _logger.LogError($"Timeout in GetTrialBalance: {ex.Message}");
                        return StatusCode(504, new { message = "The request timed out. Please try again later.", error = ex.Message });
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
        public async Task<IActionResult> GetTrialBalanceOffline(DateTime? startDate, DateTime? endDate, string accountGroup)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                using (dbContext) // Ensures DbContext is properly disposed
                {
                    try
                    {
                        var company = dbContext.Tbl901CompanyDetails.FirstOrDefault();

                        var result = await dbContext.tbl001CostAnalysis
                            .AsNoTracking()
                            .Where(x => (!startDate.HasValue || x.VoucherDate >= startDate.Value)
                                     && (!endDate.HasValue || x.VoucherDate <= endDate.Value)
                                     && (string.IsNullOrEmpty(accountGroup) || x.CostAllocationUnit == accountGroup))
                            .OrderBy(x => x.VoucherDate)
                            .Select(item => new
                            {
                                item.CostAllocationMasterGroup,
                                item.CostAllocationGroup,
                                item.CostAllocationUnit,
                                item.CostAmount,
                                item.Income,
                                item.Expenses,
                                item.VoucherDate,
                                item.EffectiveDate,
                                item.AllocationEffectiveDate,
                                item.CostAllocationID,
                                item.VoucherEntryID,
                                item.CostAllocationUnitID,
                                item.CostAllocDrCr,
                                item.AmountAllocated,
                                item.CostAllocRemarks,
                                item.IsDisabled,
                                item.AccountHead,
                                item.AccountGroup,
                                item.MasterGroup,
                                item.PL,
                                item.AccountID,
                                item.VoucherNo,
                                item.VoucherType,
                                item.VoucherTypeAndNo,
                                item.CostCenterIncharge,
                                item.EntryNarration,
                                item.SysRemarks,
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
                                item.ConvertedIncome,
                                item.ConvertedExpenses,
                                item.ConvertedCostAmount,
                                company.CurrencyImage
                            })
                            .ToListAsync();

                        return Ok(result);
                    }
                    catch (TaskCanceledException ex)
                    {
                        _logger.LogError($"Timeout in GetTrialBalanceOffline: {ex.Message}");
                        return StatusCode(504, new { message = "The request timed out. Please try again later.", error = ex.Message });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error in GetTrialBalanceOffline: {ex.Message}");
                        return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
                    }
                }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }


        [HttpGet]
        public async Task<IActionResult> GetPagedTrialBalance(
    DateTime? startDate,
    DateTime? endDate,
    string accountGroup,
    int skip = 0,
    int take = 5)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                using (dbContext)
                {
                    try
                    {
                        var query = dbContext.Qry20106CostAnalyses
                            .AsNoTracking()
                            .Where(x => (!startDate.HasValue || x.VoucherDate >= startDate.Value)
                                     && (!endDate.HasValue || x.VoucherDate <= endDate.Value)
                                     && (string.IsNullOrEmpty(accountGroup) || x.CostAllocationUnit == accountGroup));

                        var totalCount = await query.CountAsync();

                        var pagedData = await query
                            .OrderBy(x => x.VoucherDate)
                            .Skip(skip)
                            .Take(take)
                            .Select(item => new
                            {
                                item.CostAllocationMasterGroup,
                                item.CostAllocationGroup,
                                item.CostAllocationUnit,
                                item.CostAmount,
                                item.Income,
                                item.Expenses,
                                item.VoucherDate,
                                item.EffectiveDate,
                                item.AllocationEffectiveDate,
                                item.CostAllocationId,
                                item.VoucherEntryId,
                                item.CostAllocationUnitId,
                                item.CostAllocDrCr,
                                item.AmountAllocated,
                                item.CostAllocRemarks,
                                item.IsDisabled,
                                item.AccountHead,
                                item.AccountGroup,
                                item.MasterGroup,
                                item.Pl,
                                item.AccountId,
                                item.VoucherNo,
                                item.VoucherType,
                                item.VoucherTypeAndNo,
                                item.CostCenterIncharge,
                                item.EntryNarration,
                                item.SysRemarks,
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
                            })
                            .ToListAsync();

                        return Ok(new
                        {
                            data = pagedData,
                            totalCount = totalCount
                        });
                    }
                    catch (TaskCanceledException ex)
                    {
                        _logger.LogError($"Timeout in GetPagedTrialBalance: {ex.Message}");
                        return StatusCode(504, new { message = "The request timed out. Please try again later.", error = ex.Message });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error in GetPagedTrialBalance: {ex.Message}");
                        return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
                    }
                }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

    }
}









