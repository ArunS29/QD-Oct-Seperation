using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Humanizer;
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

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    public class DashboardController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ILogger<DashboardController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetTotalClientOutstanding()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out _, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var totalOutstanding = await dbContext.Qry20115BillsOutStandings
                        .Where(b => b.Balance > 0)
                        .SumAsync(b => b.Balance);

                    return Ok(new { success = true, totalOutstanding });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetTotalClientOutstanding: {ex.Message}");
                    return StatusCode(500, "Internal server error");
                }
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet]
        public async Task<IActionResult> GetBankAccounts(DataSourceLoadOptions loadOptions, string masterGroupID, DateTime? endDate)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out _, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    // Ensure parameters are not null
                    if (string.IsNullOrEmpty(masterGroupID) || endDate == null)
                    {
                        return BadRequest(new { message = "Invalid parameters.", success = false });
                    }

                    var masterGroupParam = new Microsoft.Data.SqlClient.SqlParameter("@p0", masterGroupID ?? (object)DBNull.Value);
                    var endDateParam = new Microsoft.Data.SqlClient.SqlParameter("@p1", System.Data.SqlDbType.DateTime2)
                    {
                        Value = endDate ?? (object)DBNull.Value
                    };

                    var allLedgerData = dbContext.Qry201MainVoucherEntriesWithMasters;
                    var data = dbContext.Qry201MainVoucherEntriesWithMasters
                   .Select(v => new
                   {
                       v.VoucherNo,
                       v.VoucherDate,
                       v.VoucherRefNo,
                       v.VoucherNarration,
                       v.VoucherEnteredBy,
                       v.VoucherEnteredOn,
                       v.VoucherVerifiedBy,
                       v.VoucherVerifiedOn,
                       v.VoucherApprovedBy,
                       v.VoucherApprovedOn,
                       v.VoucherEntryNo,
                       v.AccountHead,
                       v.AccountHeadName,
                       v.DrCr,
                       v.DrAmount,
                       v.CrAmount,
                       v.VoucherAmountFormatted,
                       v.EntryNarration,
                       v.AccountGroup,
                       v.MasterGroup,
                       v.IsApproved,
                       v.VoucherType,
                       v.SysRemarks,
                       v.IsCalculateOpeningBalance,
                       v.BankClearedOn,
                       v.AccountGroupId,
                       v.MasterGroupId,
                       v.IsProfitLossAccount,
                       v.IsBalanceSheetAccount,
                       v.MasterOrderNo,
                       v.MasterGroupCategory,
                       v.VoucherEffectiveDate,
                       v.BillNo,
                       v.BillDate,
                       v.BillPaidTo,
                       v.BillRemarks,
                       v.VoucherModifiedBy,
                       v.VoucherModifiedOn,
                       v.AccountHeadArabic,
                       v.AccountGroupAr,
                       v.MasterGroupAr,
                       v.VoucherTypeAr,
                       v.ChartOfAccountsOrder,
                       v.AccountGroupOrderNo,
                       v.MasterGroupCategoryAr,
                       v.IsAccumulatedDepAcc,
                       v.AccountSubGroup,
                       v.SubGroupName,
                       v.SubGroupNameAr,
                       v.MainGroup,
                       v.ReferenceNote
                   });

                    return Json(await DataSourceLoader.LoadAsync(data, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetBankAccounts: {ex.Message}");
                    return StatusCode(500, "Internal server error");
                }
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetTotalBillsOutstanding()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out _, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var totalBillsOutstanding = await dbContext.Qry20115BillsPayableOutStandings
                        .Where(b => b.Balance > 0)
                        .SumAsync(b => b.Balance);

                    return Ok(new { success = true, totalBillsOutstanding });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetTotalBillsOutstanding: {ex.Message}");
                    return StatusCode(500, "Internal server error");
                }
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetBillsPayableOutstanding(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out _, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var data = dbContext.Qry20115BillsPayableOutStandings
                        .Where(b => b.Balance > 0)
                        .Select(b => new
                        {
                            b.AccountHeadNo,
                            b.AccountHead,
                            b.Balance,
                            b.OverdueDays
                        });

                    return Json(await DataSourceLoader.LoadAsync(data, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetBillsPayableOutstanding: {ex.Message}");
                    return StatusCode(500, "Internal server error");
                }
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetBillsOutstandingAgingForChart(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out _, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var data = dbContext.Qry20117BillsOutstandingAgingForCharts
                        .Where(b => b.Balance > 0)
                        .Select(b => new
                        {
                            b.OverdueDays,
                            b.Balance,
                            b.OverDueGroup
                        });

                    return Json(await DataSourceLoader.LoadAsync(data, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetBillsOutstandingAgingForChart: {ex.Message}");
                    return StatusCode(500, "Internal server error");
                }
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet]
        public async Task<IActionResult> GetClientOutstandingAgingForChart(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out _, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    // Fetch aging data
                    var data = await dbContext.Qry20117BillsOutstandingAgingForCharts
                        .Where(b => b.Balance > 0)
                        .Select(b => new
                        {
                            b.OverdueDays,
                            b.Balance,
                            b.OverDueGroup
                        })
                        .ToListAsync();

                    // Calculate total outstanding
                    decimal totalOutstanding = data.Sum(b => b.Balance ?? 0);

                    // Prepare final dataset
                    var result = new
                    {
                        success = true,
                        totalOutstanding,
                        agingData = data
                    };

                    return Json(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetClientOutstandingAgingForChart: {ex.Message}");
                    return StatusCode(500, "Internal server error");
                }
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetVoucherEntries(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out _, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var data = dbContext.Qry201MainVoucherEntriesWithMasters
                        .Select(v => new
                        {
                            v.VoucherNo,
                            v.VoucherDate,
                            v.VoucherRefNo,
                            v.VoucherNarration,
                            v.VoucherEnteredBy,
                            v.VoucherEnteredOn,
                            v.VoucherVerifiedBy,
                            v.VoucherVerifiedOn,
                            v.VoucherApprovedBy,
                            v.VoucherApprovedOn,
                            v.VoucherEntryNo,
                            v.AccountHead,
                            v.AccountHeadName,
                            v.DrCr,
                            v.DrAmount,
                            v.CrAmount,
                            v.VoucherAmountFormatted,
                            v.EntryNarration,
                            v.AccountGroup,
                            v.MasterGroup,
                            v.IsApproved,
                            v.VoucherType,
                            v.SysRemarks,
                            v.IsCalculateOpeningBalance,
                            v.BankClearedOn,
                            v.AccountGroupId,
                            v.MasterGroupId,
                            v.IsProfitLossAccount,
                            v.IsBalanceSheetAccount,
                            v.MasterOrderNo,
                            v.MasterGroupCategory,
                            v.VoucherEffectiveDate,
                            v.BillNo,
                            v.BillDate,
                            v.BillPaidTo,
                            v.BillRemarks,
                            v.VoucherModifiedBy,
                            v.VoucherModifiedOn,
                            v.AccountHeadArabic,
                            v.AccountGroupAr,
                            v.MasterGroupAr,
                            v.VoucherTypeAr,
                            v.ChartOfAccountsOrder,
                            v.AccountGroupOrderNo,
                            v.MasterGroupCategoryAr,
                            v.IsAccumulatedDepAcc,
                            v.AccountSubGroup,
                            v.SubGroupName,
                            v.SubGroupNameAr,
                            v.MainGroup,
                            v.ReferenceNote
                        });

                    return Json(await DataSourceLoader.LoadAsync(data, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetVoucherEntries: {ex.Message}");
                    return StatusCode(500, "Internal server error");
                }
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }


        [HttpGet]
        public async Task<IActionResult> GetClientOutstanding(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out _, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var data = dbContext.Qry20115BillsOutStandings
                        .Where(b => b.Balance > 0)
                        .Select(b => new
                        {
                            b.AccountHeadNo,
                            b.AccountHead,
                            b.Balance,
                            b.AccountGroupId,
                            b.OverdueDays
                        });

                    return Json(await DataSourceLoader.LoadAsync(data, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetClientOutstanding: {ex.Message}");
                    return StatusCode(500, "Internal server error");
                }
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
    }
}
