using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.DAL.Entities;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    // [Route("Finapi/[controller]/[action]")]
    [ApiController]
    public class ChartOfAccountsController : Controller
    {
        private ERPMasterWtDataContext _context;
        public ChartOfAccountsController(ERPMasterWtDataContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult> GetChartOfAccounts()
        {
            try  

			{
                var accounts = _context.Qry20107ChartOfAccounts.ToList();
                return Json(accounts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching data.", error = ex.Message });
            }
        }




        [HttpPost]
        public IActionResult Delete(string accountId)
        {
            try
            {
                if (string.IsNullOrEmpty(accountId))
                {
                    return BadRequest(new { success = false, message = "Account ID is required." });
                }

                // Check if the account exists in VAT Invoice Master
                bool hasVATEntries = _context.Tbl20161VatinvoiceMasters.Any(v => v.ClientCode == accountId);
                if (hasVATEntries)
                {
                    return BadRequest(new { success = false, message = "This Ledger Account has entries in VAT Invoices. Please remove them and try again." });
                }

                // Check if the account exists in Voucher Entries
                bool hasVoucherEntries = _context.Tbl201VoucherEntries.Any(v => v.AccountHead == accountId);
                if (hasVoucherEntries)
                {
                    return BadRequest(new { success = false, message = "Ledger Account has transactions posted (In Vouchers). Please remove them before deleting." });
                }

                // Check if the account exists in Sub Ledger Master
                bool hasSubLedgerEntries = _context.Tbl201SubLedgerMasters.Any(s => s.AccountNo == accountId);
                if (hasSubLedgerEntries)
                {
                    return BadRequest(new { success = false, message = "Ledger Account has transactions posted (In Sub Ledger). Please remove them before deleting." });
                }

                // Find the account in Chart of Accounts
                var item = _context.Tbl201ChartOfAccounts.FirstOrDefault(p => p.AccountId == accountId);
                if (item != null)
                {
                    _context.Tbl201ChartOfAccounts.Remove(item);
                    _context.SaveChanges();
                    return Ok(new { success = true, message = "Ledger has been successfully removed from the database" });
                }

                return NotFound(new { success = false, message = "Account not found." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while deleting the account.", error = ex.Message });
            }
        }


        [HttpGet]
        public IActionResult GetAccountId(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return Json(new { success = false, message = "Invalid Cost Center ID." });
                }

                var costCenter = _context.Tbl201ChartOfAccounts.FirstOrDefault(c => c.AccountId == id);
                if (costCenter == null)
                {
                    return Json(new { success = false, message = "Cost Center not found." });
                }

                return Json(new { success = true, data = costCenter });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

    }
}