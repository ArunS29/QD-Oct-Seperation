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
                .Where(v => v.AccountGroup == "BANK ACCOUNTS" || v.AccountGroup == "CASH-IN-HAND") // Add this filter
                .Select(v => (v.DrAmount ?? 0) - (v.CrAmount ?? 0)) // Match Part 2 logic
                .SumAsync();

                    return Ok(new { success = true, cashBalance = (int)cashBalance }); // Convert to int if needed
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
                    totalOutstanding = (int)totalOutstanding;
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

public async Task<IActionResult> GetAccountSummary(DataSourceLoadOptions loadOptions)

{

     if (_tenantDbContextHelper.TryGetTenantAndDbContext(out _, out ERPMasterWtDataContext dbContext))

     {

         try

         {

             var data = dbContext.Qry201MainVoucherEntriesWithMasters

                 .Where(v => v.AccountGroup == "BANK ACCOUNTS" || v.AccountGroup == "CASH-IN-HAND") // Filter by AccountGroup

                 .GroupBy(v => new { v.AccountHeadName, v.AccountGroup }) // Group by AccountHeadName and AccountGroup

                 .Select(g => new

                 {

                     AccountHeadName = g.Key.AccountHeadName,

                     AccountGroup = g.Key.AccountGroup,

                     TotalBalance = g.Sum(v => (v.DrAmount ?? 0) - (v.CrAmount ?? 0)) // Calculate total balance

                 })

                 .OrderByDescending(g => g.TotalBalance) // Order by TotalBalance in descending order

                 .Take(5) // Take the top 5 results

                 .AsQueryable();
 
             return Json(await DataSourceLoader.LoadAsync(data, loadOptions));

         }

         catch (Exception ex)

         {

             _logger.LogError($"Error in GetAccountSummary: {ex.Message}");

             return StatusCode(500, "Internal server error");

         }

     }
 
     return Unauthorized(new { message = "Invalid tenant.", success = false });

}


        [HttpGet]

        public async Task<IActionResult> GetAccountSummaryBank(DataSourceLoadOptions loadOptions)

        {

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out _, out ERPMasterWtDataContext dbContext))

            {

                try

                {

                    var data = dbContext.Qry201MainVoucherEntriesWithMasters

                        .Where(v => v.AccountGroup == "BANK ACCOUNTS" || v.AccountGroup == "CASH-IN-HAND") // Filter by AccountGroup

                        .GroupBy(v => new { v.AccountHeadName, v.AccountGroup }) // Group by AccountHeadName and AccountGroup

                        .Select(g => new

                        {

                            AccountHeadName = g.Key.AccountHeadName,

                            AccountGroup = g.Key.AccountGroup,

                            TotalBalance = g.Sum(v => (v.DrAmount ?? 0) - (v.CrAmount ?? 0)) // Calculate total balance

                        })

                        .OrderByDescending(g => g.TotalBalance) // Order by TotalBalance in descending order

                 

                        .AsQueryable();

                    return Json(await DataSourceLoader.LoadAsync(data, loadOptions));

                }

                catch (Exception ex)

                {

                    _logger.LogError($"Error in GetAccountSummary: {ex.Message}");

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
                    totalBillsOutstanding = (int)totalBillsOutstanding;
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
                        .Where(b => b.Balance > 0) // Only bills with outstanding balance
                        .GroupBy(b => new { b.Balance, b.OverdueDays, b.AccountHeadNo, b.AccountHead }) // Group by Balance, OverdueDays, AccountHeadNo, and AccountHead
                        .Select(g => new
                        {
                            AccountHeadNo = g.Key.AccountHeadNo,
                            AccountHead = g.Key.AccountHead,
                            Balance = g.Key.Balance,         // Use the grouped Balance
                            OverdueDays = g.Key.OverdueDays // Use the grouped OverdueDays
                        })
                        .OrderByDescending(g => g.Balance) // Highest balance first
                        .ThenByDescending(g => g.OverdueDays) // Highest overdue days next
                        .ThenBy(g => string.IsNullOrEmpty(g.AccountHead)) // Sort null/empty last
                        .ThenBy(g => g.AccountHead) // Alphabetical order if same Balance and OverdueDays
                        .Take(5) // Top 5 only
                        .AsQueryable();

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
        public async Task<IActionResult> GetBillsPayableOutstandingSupplier(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out _, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var data = dbContext.Qry20115BillsPayableOutStandings
                        .Where(b => b.Balance > 0) // Only bills with outstanding balance
                        .GroupBy(b => new {  b.AccountHeadNo, b.AccountHead }) // Group by fields
                        .Select(g => new
                        {
                            AccountHeadNo = g.Key.AccountHeadNo,
                            AccountHead = g.Key.AccountHead,
                            Balance = g.Sum(x => x.Balance  ),
                            OverdueDays = g.Max(x => x.OverdueDays),
                        })
                        .OrderByDescending(g => g.Balance)
                        .ThenByDescending(g => g.OverdueDays)
                        .ThenBy(g => string.IsNullOrEmpty(g.AccountHead))
                        .ThenBy(g => g.AccountHead)
                        .AsQueryable(); // Removed .Take(5)

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
                        .Select(b => new
                        {
                            b.AccountHeadNo,
                            b.AccountHead,
                            b.Balance,
                            b.OverdueDays,
                            Type = "Client"
                        });

                    var supplierData = dbContext.Qry20115BillsPayableOutStandings
                        .Where(b => b.Balance > 0)
                        .Select(b => new
                        {
                            b.AccountHeadNo,
                            b.AccountHead,
                            b.Balance,
                            b.OverdueDays,
                            Type = "Supplier"
                        });

                    var combinedData = await clientData
                        .Concat(supplierData)
                        .OrderByDescending(x => x.Balance)
                        .ThenByDescending(x => x.OverdueDays)
                        .ThenBy(x => string.IsNullOrEmpty(x.AccountHead))
                        .ThenBy(x => x.AccountHead)
                        .Take(5)
                        .ToListAsync();

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
        public async Task<IActionResult> GetBillsOutstandingAgingForChart()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out _, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    // Define overdue day ranges with labels
                    var overdueRanges = new[]
                    {
                        new { Min = 0, Max = 30, Label = "0-30 Days" },
                        new { Min = 31, Max = 60, Label = "31-60 Days" },
                        new { Min = 61, Max = 90, Label = "61-90 Days" },
                        new { Min = 91, Max = int.MaxValue, Label = "91+ Days" }
                    };

                    // Group and project data into labeled overdue buckets
                    var overdueData = overdueRanges.SelectMany(range =>
                        dbContext.Qry20115BillsPayableOutStandings
                            .Where(b => b.Balance > 0 && b.OverdueDays >= range.Min && b.OverdueDays <= range.Max)
                            .Select(b => new
                            {
                                OverdueRange = range.Label,
                                b.Balance,
                                b.OverdueDays,
                                b.AccountHead
                            }))
                        .OrderByDescending(b => b.Balance)
                        .ThenByDescending(b => b.OverdueDays)
                        .Take(5)
                        .ToList();

                    return Ok(new { success = true, data = overdueData });
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
        public async Task<IActionResult> GetBillsOutstandingAgingForChartSupplier()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out _, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    // Define overdue day ranges with labels
                    var overdueRanges = new[]
                    {
                new { Min = 0, Max = 30, Label = "0-30 Days" },
                new { Min = 31, Max = 60, Label = "31-60 Days" },
                new { Min = 61, Max = 90, Label = "61-90 Days" },
                new { Min = 91, Max = int.MaxValue, Label = "91+ Days" }
            };

                    // Group and project data into labeled overdue buckets
                    var overdueData = overdueRanges.SelectMany(range =>
                        dbContext.Qry20115BillsPayableOutStandings
                            .Where(b => b.Balance > 0 && b.OverdueDays >= range.Min && b.OverdueDays <= range.Max)
                            .Select(b => new
                            {
                                OverdueRange = range.Label,
                                b.Balance,
                                b.OverdueDays,
                                b.AccountHead
                            }))
                        .OrderByDescending(b => b.Balance)
                        .ThenByDescending(b => b.OverdueDays)
                        .ToList(); // Removed .Take(5) to return all records

                    return Ok(new { success = true, data = overdueData });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetBillsOutstandingAgingForChartSupplier: {ex.Message}");
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
                    // Define overdue day ranges
                    var overdueRanges = new[]
                    {
                        new { Min = 0, Max = 30, Label = "0-30 Days" },
                        new { Min = 31, Max = 60, Label = "31-60 Days" },
                        new { Min = 61, Max = 90, Label = "61-90 Days" },
                        new { Min = 91, Max = int.MaxValue, Label = "91+ Days" }
                    };

                    // Group and aggregate data by overdue day ranges asynchronously
                    var agingData = new List<object>();
                    foreach (var range in overdueRanges)
                    {
                        var totalBalance = await dbContext.Qry20117BillsOutstandingAgingForCharts
                            .Where(b => b.Balance > 0 && b.OverdueDays >= range.Min && b.OverdueDays <= range.Max)
                            .SumAsync(b => b.Balance ?? 0);

                        agingData.Add(new
                        {
                            OverdueRange = range.Label,
                            TotalBalance = totalBalance
                        });
                    }

                    // Calculate total outstanding balance
                    decimal totalOutstanding = agingData.Sum(a => (decimal)((dynamic)a).TotalBalance);

                    // Prepare final dataset
                    var result = new
                    {
                        success = true,
                        totalOutstanding,
                        agingData
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
                        .Where(b => b.Balance > 0) // Only bills with outstanding balance
                        .GroupBy(b => new { b.Balance, b.OverdueDays, b.AccountHeadNo, b.AccountHead }) // Match grouping with Payables
                        .Select(g => new
                        {
                            AccountHeadNo = g.Key.AccountHeadNo,
                            AccountHead = g.Key.AccountHead,
                            Balance = g.Key.Balance,
                            OverdueDays = g.Key.OverdueDays
                        })
                        .OrderByDescending(g => g.Balance)
                        .ThenByDescending(g => g.OverdueDays)
                        .ThenBy(g => string.IsNullOrEmpty(g.AccountHead)) // Optional: push nulls last
                        .ThenBy(g => g.AccountHead)
                        .Take(5)
                        .AsQueryable();

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


        [HttpGet]
        public async Task<IActionResult> GetClientOutstandingGrid(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out _, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var data = dbContext.Qry20115BillsOutStandings
      .Where(b => b.Balance > 0)
      .GroupBy(b => new { b.AccountHeadNo, b.AccountHead }) // Removed b.Balance and b.OverdueDays
      .Select(g => new
      {
          AccountHeadNo = g.Key.AccountHeadNo,
          AccountHead = g.Key.AccountHead,
          Balance = g.Sum(x => x.Balance),
          OverdueDays = g.Max(x => x.OverdueDays) // Optional: use Max/Avg if needed
      })
      .OrderByDescending(g => g.Balance)
      .ThenByDescending(g => g.OverdueDays)
      .ThenBy(g => string.IsNullOrEmpty(g.AccountHead))
      .ThenBy(g => g.AccountHead)
      .AsQueryable();
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



        [HttpGet]
        public async Task<IActionResult> GetBankAccountsFrequency()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out _, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var data = await Task.Run(() => dbContext.Qry201MainVoucherEntriesWithMasters
                        .GroupBy(v => new { v.AccountHead, v.AccountHeadName, v.MasterGroupId, v.AccountGroup })
                        .Select(g => new sp20103GetBankAccountsResult
                        {
                            AccountHead = g.Key.AccountHead,
                            AccountHeadName = g.Key.AccountHeadName,
                            MasterGroupID = g.Key.MasterGroupId,
                            Amount = g.Sum(v => v.CrAmount - v.DrAmount),
                            AccountGroup = g.Key.AccountGroup
                        })
                        .OrderByDescending(g => g.Amount)
                        .ThenBy(g => string.IsNullOrEmpty(g.AccountHeadName))
                        .ThenBy(g => g.AccountHeadName)
                        .ToList());

                    // Filter out negative balances
                    var filteredData = data.Where(item => item.Amount > 0).ToList();

                    // Group data by balance ranges (e.g., 0-1000, 1000-2000, etc.)
                    var balanceRanges = new[] { 0, 1000, 2000, 3000, 4000, 5000, 10000, 20000, 50000, 100000 };
                    var frequencyData = balanceRanges.Select((range, index) =>
                    {
                        var nextRange = (index + 1 < balanceRanges.Length) ? balanceRanges[index + 1] : int.MaxValue;
                        return new
                        {
                            Range = $"{range}-{nextRange}",
                            BankBalance = filteredData.Where(item => item.AccountGroup == "Bank" && item.Amount >= range && item.Amount < nextRange).Sum(item => item.Amount),
                            CashBalance = filteredData.Where(item => item.AccountGroup == "Cash" && item.Amount >= range && item.Amount < nextRange).Sum(item => item.Amount)
                        };
                    }).ToList();

                    return Json(frequencyData);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetBankAccountsFrequency: {ex.Message}");
                    return StatusCode(500, "Internal server error");
                }
            }
            return StatusCode(500, "Internal server error");
        }

        [HttpGet]
        public Task<IActionResult> GetClientOutstandingByOverdueDays()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out _, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    // Define overdue day ranges
                    var overdueRanges = new[]
                    {
                        new { Min = 0, Max = 30, Label = "0-30 Days" },
                        new { Min = 31, Max = 60, Label = "31-60 Days" },
                        new { Min = 61, Max = 90, Label = "61-90 Days" },
                        new { Min = 91, Max = int.MaxValue, Label = "91+ Days" }
                    };

                    // Fetch and group data by overdue day ranges, then sort and take top 5
                    var overdueData = overdueRanges.SelectMany(range => dbContext.Qry20115BillsOutStandings
                        .Where(b => b.Balance > 0 && b.OverdueDays >= range.Min && b.OverdueDays <= range.Max)
                        .Select(b => new
                        {
                            OverdueRange = range.Label,
                            Balance = b.Balance,
                            AccountHead = b.AccountHead,
                            OverdueDays = b.OverdueDays
                        }))
                        .OrderByDescending(b => b.Balance) // Sort by balance in descending order
                        .ThenByDescending(b => b.OverdueDays) // Then by overdue days in descending order
                        .Take(5) // Take the top 5 records
                        .ToList();

                    return Task.FromResult<IActionResult>(Json(new { success = true, data = overdueData }));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetClientOutstandingByOverdueDays: {ex.Message}");
                    return Task.FromResult<IActionResult>(StatusCode(500, "Internal server error"));
                }
            }

            return Task.FromResult<IActionResult>(Unauthorized(new { message = "Invalid tenant.", success = false }));
        }
        [HttpGet]
        public Task<IActionResult> GetClientOutstandingByAccountHead()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out _, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var groupedData = dbContext.Qry20115BillsOutStandings
                        .Where(b => b.Balance > 0)
                        .GroupBy(b => b.AccountHead)
                        .Select(g => new
                        {
                            AccountHead = g.Key,
                            TotalBalance = g.Sum(b => b.Balance ?? 0)
                        })
                        .OrderByDescending(g => g.TotalBalance)
                        .ToList();

                    return Task.FromResult<IActionResult>(Json(new { success = true, data = groupedData }));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetClientOutstandingByAccountHead: {ex.Message}");
                    return Task.FromResult<IActionResult>(StatusCode(500, "Internal server error"));
                }
            }

            return Task.FromResult<IActionResult>(Unauthorized(new { message = "Invalid tenant.", success = false }));
        }


    }
}
