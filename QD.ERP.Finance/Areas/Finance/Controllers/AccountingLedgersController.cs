using DevExpress.XtraRichEdit.Import.Doc;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QD.ERP.Shared.DAL.Entities;
using QD.ERP.Shared.Service;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace QD.ERP.Finance.Areas.Finance.Controllers
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
                            i.IsLedgerObselete,
                            i.IsRestricted,
                            i.IsUseInSales,
                            i.IsUsedInPurchase,
                            i.IsProfitLossAccount,
                            i.IsBalanceSheetAccount,
                            i.IsMaintainBillByBill,
                            i.IsUseInReconciliation,
                            i.IsSalaryPayable,
                            i.Expr1,

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

                    var costCenter = dbContext.Tbl201ChartOfAccounts
                                              .FirstOrDefault(c => c.AccountId == id);

                    if (costCenter == null)
                        return Json(new { success = false, message = "Cost Center not found." });

                    // fetch account group name
                    var accountGroup = dbContext.Tbl201AccountGroups
                                                .Where(x => x.AccountGroupId == costCenter.AccountGroupId)
                                                .Select(x => x.AccountGroup)
                                                .FirstOrDefault();

                    // return both costCenter and AccountGroup
                    return Json(new
                    {
                        success = true,
                        data = new
                        {
                            costCenter.AccountId,
                            costCenter.AccountHead,
                            costCenter.AccountGroupId,
                            costCenter.ReferenceNo,
                            costCenter.AccountHeadArabic,
                            AccountGroup = accountGroup
                        }
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetAccountId(string id) at {DateTime.Now}. Error: {ex.Message}");
                    return Json(new { success = false, message = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]

        public IActionResult GetAccountHeadName(string accountId)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var account = dbContext.Tbl201ChartOfAccounts
                    .Where(a => a.AccountId == accountId)
                    .Select(a => new
                    {
                        a.AccountId,
                        AccountHead = a.AccountHead
                    })
                    .FirstOrDefault();

                if (account == null)
                {
                    return NotFound(new { message = "Account not found" });
                }

                return Ok(account);
            }

            return BadRequest(new { message = "Invalid tenant context" });
        }
        [HttpGet]
        public IActionResult GetCurrencyIcon()
        {
            string currencyType = HttpContext.Session.GetString("currencytype");

            if (string.IsNullOrEmpty(currencyType))
            {
                return BadRequest("Currency type not found in session.");
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var currency = dbContext.Tbl20169CurrencyExchanges
                    .Where(c => c.CurrencyMasterCode == currencyType)
                    .Select(c => new
                    {
                        Icon = c.CurrencyImage != null && c.CurrencyImage.Trim() != ""
                        ? (string)c.CurrencyImage
                        : c.CurrencySymbole
                    })
                    .FirstOrDefault();

                if (currency == null)
                    return NotFound("Currency not found.");

                return Json(currency);
            }

            return Unauthorized("Invalid tenant.");
        }

    }
}