using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using QD.ERP.Web.Services.Logging;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ChartOfAccountsController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<ChartOfAccountsController> _logger;
        private readonly IUserActionLogger _userActionLogger;
        public ChartOfAccountsController(ILogger<ChartOfAccountsController> logger, IUserActionLogger userActionLogger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
            _userActionLogger = userActionLogger;
        }
        [FinancePermission("uc20101ChartOfAccounts")]
        public IActionResult ChartOfAccounts() => View();

        [HttpGet]
        public async Task<ActionResult> GetChartOfAccounts()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var accounts = await dbContext.Qry20107ChartOfAccounts.ToListAsync();
                    return Json(accounts);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetChartOfAccounts: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string accountId)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    if (string.IsNullOrEmpty(accountId))
                    {
                        return BadRequest(new { success = false, message = "Account ID is required." });
                    }

                    bool hasVATEntries = await dbContext.Tbl20161VatinvoiceMasters.AnyAsync(v => v.ClientCode == accountId);
                    if (hasVATEntries)
                    {
                        return BadRequest(new { success = false, message = "This Ledger Account has entries in VAT Invoices. Please remove them and try again." });
                    }

                    bool hasVoucherEntries = await dbContext.Tbl201VoucherEntries.AnyAsync(v => v.AccountHead == accountId);
                    if (hasVoucherEntries)
                    {
                        return BadRequest(new { success = false, message = "Ledger Account has transactions posted (In Vouchers). Please remove them before deleting." });
                    }

                    bool hasSubLedgerEntries = await dbContext.Tbl201SubLedgerMasters.AnyAsync(s => s.AccountNo == accountId);
                    if (hasSubLedgerEntries)
                    {
                        return BadRequest(new { success = false, message = "Ledger Account has transactions posted (In Sub Ledger). Please remove them before deleting." });
                    }

                    var item = await dbContext.Tbl201ChartOfAccounts.FirstOrDefaultAsync(p => p.AccountId == accountId);
                    if (item != null)
                    {
                        dbContext.Tbl201ChartOfAccounts.Remove(item);
                        await dbContext.SaveChangesAsync();
                        await _userActionLogger.LogAsync(
                        module: "Finance > Chart of Account",
                        actionDetail: $"Updated: {accountId}",
                        documentNo: accountId
                        );
                        return Ok(new { success = true, message = "Ledger has been successfully removed from the database" });
                    }

                    return NotFound(new { success = false, message = "Account not found." });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in Delete: {ex.Message}");
                    return StatusCode(500, new { success = false, message = "An error occurred while deleting the account.", error = ex.Message });
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
                    {
                        return Json(new { success = false, message = "Invalid Cost Center ID." });
                    }

                    var costCenter = dbContext.Tbl201ChartOfAccounts.FirstOrDefault(c => c.AccountId == id);
                    if (costCenter == null)
                    {
                        return Json(new { success = false, message = "Cost Center not found." });
                    }

                    return Json(new { success = true, data = costCenter });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetAccountId: {ex.Message}");
                    return Json(new { success = false, message = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
    }
}
