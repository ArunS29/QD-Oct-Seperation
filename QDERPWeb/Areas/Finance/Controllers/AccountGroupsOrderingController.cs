using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("/api/[controller]/[action]")]
    [ApiController]
    public class AccountGroupsOrderingController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<AccountGroupsOrderingController> _logger;

        public AccountGroupsOrderingController(ILogger<AccountGroupsOrderingController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAccountGroupsOrdering(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var qry20164salarypayableledgermaster = dbContext.Qry201207accountGroupOrderings
    .Select(i => new
    {
        i.ChartOfAccountsOrder,
        i.MasterGroup,
        i.AccountGroupId,
        i.AccountGroup,
        i.AccountGroupOrderNo
    })
    .OrderBy(i => i.ChartOfAccountsOrder)   // First order by ChartOfAccountsOrder
    .ThenBy(i => i.AccountGroupOrderNo);  // Then order by AccountGroupOrderNo

                var resultList1 = await qry20164salarypayableledgermaster.ToListAsync();
                return Json(DataSourceLoader.Load(resultList1.AsQueryable(), loadOptions));
                
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public IActionResult UpdateAccountGroupOrder([FromBody] List<Qry201207accountGroupOrdering> updatedData)
        {
            if (updatedData == null || updatedData.Count == 0)
            {
                return BadRequest("No data received");
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                foreach (var item in updatedData)
                {
                    var existingRecord = dbContext.Tbl201AccountGroups
                        .FirstOrDefault(x => x.AccountGroupId == item.AccountGroupId);

                    if (existingRecord != null)
                    {
                        existingRecord.AccountGroupOrderNo = item.AccountGroupOrderNo;
                        dbContext.Tbl201AccountGroups.Update(existingRecord);
                    }
                }

                dbContext.SaveChanges(); // Save changes to the database

                return Ok(new { message = "Data updated successfully" });
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpPost]
        public async Task<IActionResult> SwapWithinGroup([FromBody] SwapRequest request)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                return BadRequest(new { success = false, message = "Tenant context not found." });

            var strategy = dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await dbContext.Database.BeginTransactionAsync();

                try
                {
                    // Find current row
                    var current = await dbContext.Qry201207accountGroupOrderings
                        .FirstOrDefaultAsync(x => x.AccountGroupId == request.AccountGroupId);

                    if (current == null)
                        throw new Exception("Row not found.");

                    // Find target row in same group based on direction
                    var target = request.Direction == "up"
                        ? await dbContext.Qry201207accountGroupOrderings
                            .Where(x => x.ChartOfAccountsOrder < current.ChartOfAccountsOrder &&
                                        x.MasterGroup == current.MasterGroup)
                            .OrderByDescending(x => x.ChartOfAccountsOrder)
                            .FirstOrDefaultAsync()
                        : await dbContext.Qry201207accountGroupOrderings
                            .Where(x => x.ChartOfAccountsOrder > current.ChartOfAccountsOrder &&
                                        x.MasterGroup == current.MasterGroup)
                            .OrderBy(x => x.ChartOfAccountsOrder)
                            .FirstOrDefaultAsync();

                    if (target == null)
                        throw new Exception("No row to swap with.");

                    // Swap order numbers
                    var temp = current.ChartOfAccountsOrder;
                    current.ChartOfAccountsOrder = target.ChartOfAccountsOrder;
                    target.ChartOfAccountsOrder = temp;

                    await dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });

            return Ok(new { success = true });
        }


    }
}
