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
        public async Task<IActionResult> GetCashBalance()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out _, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var cashBalance = await dbContext.Qry201MainVoucherEntriesWithMasters
                        .Select(v => v.CrAmount - v.DrAmount)
                        .SumAsync();

                    return Ok(new { success = true, cashBalance });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetCashBalance: {ex.Message}");
                    return StatusCode(500, "Internal server error");
                }
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });
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
        public async Task<IActionResult> GetBankAccounts(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out _, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var data = dbContext.Qry201MainVoucherEntriesWithMasters
                        .GroupBy(v => new { v.AccountHead, v.AccountHeadName, v.MasterGroupId, v.AccountGroup })
                        .Select(g => new sp20103GetBankAccountsResult
                        {
                            AccountHead = g.Key.AccountHead,
                            AccountHeadName = g.Key.AccountHeadName,
                            MasterGroupID = g.Key.MasterGroupId,
                            Amount = g.Sum(v => v.CrAmount - v.DrAmount),
                            AccountGroup = g.Key.AccountGroup
                        })
                        .OrderByDescending(g => g.Amount) // Order by Amount in descending order
                        .ThenBy(g => string.IsNullOrEmpty(g.AccountHeadName))
                        .ThenBy(g => g.AccountHeadName)
                        .AsQueryable(); // Convert to IQueryable before passing to DataSourceLoader

                    return Json(await DataSourceLoader.LoadAsync<sp20103GetBankAccountsResult>(data, loadOptions));
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
                        .GroupBy(b => new { b.AccountHeadNo, b.AccountHead })
                        .Select(g => new
                        {
                            AccountHeadNo = g.Key.AccountHeadNo,
                            AccountHead = g.Key.AccountHead,
                            Balance = g.Sum(b => b.Balance),
                            OverdueDays = g.Max(b => b.OverdueDays) // Assuming you want the max overdue days for the group
                        })
                        .OrderBy(g => g.OverdueDays) // Order by OverdueDays
                        .ThenBy(g => string.IsNullOrEmpty(g.AccountHead))
                        .ThenBy(g => g.AccountHead)
                        .AsQueryable(); // Convert to IQueryable before passing to DataSourceLoader

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
        public async Task<IActionResult> GetOutstandingChartData()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out _, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var clientData = dbContext.Qry20115BillsOutStandings
                        .Where(b => b.Balance > 0)
                        .GroupBy(b => new { b.AccountHeadNo, b.AccountHead })
                        .Select(g => new
                        {
                            AccountHead = g.Key.AccountHead,
                            Balance = g.Sum(b => b.Balance),
                            OverdueDays = g.Max(b => b.OverdueDays),
                            Type = "Client"
                        })
                        .OrderBy(g => g.OverdueDays);

                    var supplierData = dbContext.Qry20115BillsPayableOutStandings
                        .Where(b => b.Balance > 0)
                        .GroupBy(b => new { b.AccountHeadNo, b.AccountHead })
                        .Select(g => new
                        {
                            AccountHead = g.Key.AccountHead,
                            Balance = g.Sum(b => b.Balance),
                            OverdueDays = g.Max(b => b.OverdueDays),
                            Type = "Supplier"
                        })
                        .OrderBy(g => g.OverdueDays);

                    var combinedData = await clientData.Concat(supplierData).ToListAsync();

                    return Json(new { success = true, data = combinedData });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetOutstandingChartData: {ex.Message}");
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
                    var data = await Task.Run(() => dbContext.Qry20117BillsOutstandingAgingForCharts
                        .Where(b => b.Balance > 0)
                        .Select(b => new
                        {
                            b.OverdueDays,
                            b.Balance,
                            b.OverDueGroup
                        })
                        .ToList());

                    return Ok(new { success = true, data });
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
        public async Task<IActionResult> GetClientOutstanding(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out _, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var data = dbContext.Qry20115BillsOutStandings
                        .Where(b => b.Balance > 0)
                        .GroupBy(b => new { b.AccountHeadNo, b.AccountHead, b.AccountGroupId })
                        .Select(g => new
                        {
                            AccountHeadNo = g.Key.AccountHeadNo,
                            AccountHead = g.Key.AccountHead,
                            AccountGroupId = g.Key.AccountGroupId,
                            Balance = g.Sum(b => b.Balance),
                            OverdueDays = g.Max(b => b.OverdueDays) // Assuming you want the max overdue days for the group
                        })
                        .OrderBy(g => g.OverdueDays) // Order by OverdueDays
                        .ThenBy(g => string.IsNullOrEmpty(g.AccountHead))
                        .ThenBy(g => g.AccountHead)
                        .AsQueryable(); // Convert to IQueryable before passing to DataSourceLoader

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
