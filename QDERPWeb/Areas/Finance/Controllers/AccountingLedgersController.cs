using QD.ERP.Web.DAL.Entities;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
	[Route("api/[controller]/[action]")]
	public class AccountingLedgersController : Controller
	{
		private ERPMasterWtDataContext _context;

		public AccountingLedgersController(ERPMasterWtDataContext context)
		{
			_context = context;
		}

		[HttpGet]
		public async Task<ActionResult> Get(DataSourceLoadOptions loadOptions)
		{
			try
			{
				var accountList = _context.Qry201ListOfAccounts.Select(i => new
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
				return Json(await DataSourceLoader.LoadAsync(accountList, loadOptions));
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error: {ex.Message}");
			}
		}

		[HttpGet]
		public async Task<ActionResult> GetLedgerAccounts(DataSourceLoadOptions loadOptions)
		{
			try
			{
				var ledgerAccounts = _context.Qry201ListOfAccounts
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
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error: {ex.Message}");
			}
		}

		[HttpGet]
		public async Task<ActionResult> GetVouchers(string accountId, string frmDate, string toDate)
		{
			try
			{
				if (!DateTime.TryParseExact(frmDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime from))
					return BadRequest("Invalid from date format. Use MM/dd/yyyy.");

				if (!DateTime.TryParseExact(toDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime to))
					return BadRequest("Invalid to date format. Use MM/dd/yyyy.");

				var ledgerData = await _context.AccountLedgers
					.FromSqlRaw("EXEC StProAccountLedger @p0, @p1, @p2", accountId, from, to)
					.ToListAsync();

				return Json(ledgerData);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error: {ex.Message}");
			}
		}

		[HttpGet]
		public IActionResult GetAccountId(string id)
		{
			try
			{
				if (string.IsNullOrEmpty(id))
					return Json(new { success = false, message = "Invalid Cost Center ID." });

				var costCenter = _context.Tbl201ChartOfAccounts.FirstOrDefault(c => c.AccountId == id);
				if (costCenter == null)
					return Json(new { success = false, message = "Cost Center not found." });

				return Json(new { success = true, data = costCenter });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message });
			}
		}
	}
}
