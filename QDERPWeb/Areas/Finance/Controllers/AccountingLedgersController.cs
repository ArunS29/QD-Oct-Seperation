using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    public class AccountingLedgersController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<AccountingLedgersController> _logger;

        public AccountingLedgersController(ILogger<AccountingLedgersController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult> GetLedgerAccounts(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                using (dbContext)
                {
                    var ledgerAccounts = dbContext.Qry201ListOfAccounts
                        .Where(p => p.AccountId != null)
                        .Select(i => new
                        {
                            i.MasterGroupId,
                            i.MasterGroup,
                            i.AccountGroup,
                            i.AccountGroupId,
                            i.AccountId,
                            i.AccountHead,
                            i.AccountHeadArabic,
                            i.ReferenceNo,
                            i.IsLedgerObselete
                        });

                    return Json(await DataSourceLoader.LoadAsync(ledgerAccounts, loadOptions));
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<ActionResult> GetVouchers(string accountId, string frmDate, string toDate)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    if (!DateTime.TryParseExact(frmDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime from))
                        return BadRequest("Invalid from date format. Use MM/dd/yyyy.");

                    if (!DateTime.TryParseExact(toDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime to))
                        return BadRequest("Invalid to date format. Use MM/dd/yyyy.");

					//var ledgerData = await dbContext.AccountLedgers
					//    .FromSqlRaw("EXEC StProAccountLedger @p0, @p1, @p2", accountId, from, to)
					//    .ToListAsync();
					var allLedgerData = await dbContext.AccountLedgers
		.FromSqlRaw("EXEC StProAccountLedger @p0, @p1, @p2", accountId, from, to)
		.ToListAsync(); // Fetch all records first

					var ledgerData = allLedgerData
						.Where(x => !string.IsNullOrEmpty(x.VoucherType)) // Filter results
						.ToList();


					return Json(ledgerData);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public IActionResult GetAccountId(string id)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    if (string.IsNullOrEmpty(id))
                        return Json(new { success = false, message = "Invalid Cost Center ID." });

                    var costCenter = dbContext.Tbl201ChartOfAccounts.FirstOrDefault(c => c.AccountId == id);
                    if (costCenter == null)
                        return Json(new { success = false, message = "Cost Center not found." });

                    return Json(new { success = true, data = costCenter });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"This is an error log message from the AccountingLedger controller from GetAccountId(string id) at {DateTime.Now}. Error: {ex.Message}");
                    return Json(new { success = false, message = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
    }
}
