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

            return Json(_context.Qry20107ChartOfAccounts.ToList());
        }



        [HttpPost]
        public IActionResult Delete(string accountId)
        {
            if (string.IsNullOrEmpty(accountId))
            {
                return BadRequest("Account ID is required.");
            }

            var item = _context.Tbl201ChartOfAccounts.FirstOrDefault(p => p.AccountId == accountId);
            if (item != null)
            {
                _context.Tbl201ChartOfAccounts.Remove(item);
                _context.SaveChanges();
                return Ok(new { success = true });
            }

            return NotFound(new { success = false, message = "Account not found." });
        }

    }
}