using System.Globalization;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.DAL.Entities;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class SalaryPayableMappingController : Controller
	{
		private ERPMasterWtDataContext _context;

		public SalaryPayableMappingController(ERPMasterWtDataContext context)
		{
			_context = context;
		}
		[HttpGet]
		public IActionResult GetSalaryMapping(string acchedid)
		{
			try
			{
				var query = _context.Qry20191SalaryPayableMasterMapped02s.AsQueryable();

				// Apply acchedid filter if it's not null or empty
				if (!string.IsNullOrEmpty(acchedid))
				{
					query = query.Where(e => e.LedgerNo == acchedid);
				}

				var data = query.Select(e => new
				{
					e.VoucherNo,
					VoucherDate = e.VoucherDate.HasValue
						? e.VoucherDate.Value.ToString("dd-MMM-yyyy")
						: string.Empty,
					e.EmployeeNo,
					e.EmployeeName,
					e.NationalId,
					e.VoucherAmount,
					e.TotalMappedAmount,
					e.Mapping
				}).ToList();

				return Json(data);
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = "An error occurred while fetching data.", error = ex.Message });
			}
		}

		[HttpGet]
		public IActionResult GetSalaryMappings(DateTime? startDate, DateTime? endDate)
		{
			var claims = _context.Qry20191SalaryPayableMasterMapped02s.AsQueryable();

			if (startDate.HasValue && endDate.HasValue)
			{
				claims = claims.Where(c => c.VoucherDate >= startDate && c.VoucherDate <= endDate);
			}

			// Project to an object with all required fields
			var result = claims.Select(e => new
			{
				e.VoucherNo,
				VoucherDate = e.VoucherDate.HasValue
					? e.VoucherDate.Value.ToString("dd-MMM-yyyy")
					: string.Empty,
				e.EmployeeNo,
				e.EmployeeName,
				e.NationalId,
				e.VoucherAmount,
				e.TotalMappedAmount,
				e.Mapping


			}).ToList();

			return Ok(result);
		}


		public IActionResult Depreciation()
		{
			// Logic to retrieve data for the Depreciation page, if necessary
			return PartialView("Depreciation");
		}

		// [HttpGet]
		// public async Task<ActionResult> GetLedgerMapping(string accid, string FromDate, string ToDate)
		// {
		//     try
		//     {
		//         if (!DateTime.TryParseExact(FromDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime from))
		//         {
		//             return BadRequest("Invalid from date format. Use MM/dd/yyyy.");
		//         }

		//         if (!DateTime.TryParseExact(ToDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime to))
		//         {
		//             return BadRequest("Invalid to date format. Use MM/dd/yyyy.");
		//         }

		//         // ✅ No need to re-parse with DateTime.Parse()

		//         //ERPMasterWtDataContextProcedures _procedures = new ERPMasterWtDataContextProcedures(_context);
		//         //var ledgerData = await _procedures.StProAccountLedgerAsync(accountId, from, to);
		//         var ledgerData = await _context.AccountLedgerDtos
		//.FromSqlRaw("EXEC StProAccountLedger @p0, @p1, @p2", accid, from, to)
		//.ToListAsync();

		//         return Json(ledgerData);
		//     }
		//     catch (Exception ex)
		//     {
		//         return StatusCode(500, $"Internal server error: {ex.Message}");
		//     }
		// }

		[HttpGet]
		public async Task<IActionResult> GetLedgerMapping(DataSourceLoadOptions loadOptions, string accid)
		{
			var qryListOfAccountlists = _context.Qry201114accountLedgersWtAdvances
				.Where(i => i.AccountNoInVoucher == accid) // Apply filter
				.Select(i => new
				{
					i.AccountNoInVoucher,
					i.AccountHead,
					i.VoucherNoInVoucher,
					i.BalanceInVoucher,
					i.AmountInVoucherFormatted,
					i.AmountInSubLedgerFormatted,
					i.Mapping
				});

			return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
		}
		[HttpGet]
		public async Task<IActionResult> GetBankReconciliation(DataSourceLoadOptions loadOptions, string accid)
		{
			try
			{
				if (string.IsNullOrEmpty(accid))
				{
					return BadRequest("Account ID is required.");
				}

				var query = from t1 in _context.Tbl201VoucherEntries
							join t2 in _context.Tbl201VoucherMasters
							on t1.VoucherNo equals t2.VoucherNo
							where t1.AccountHead == accid
							select new
							{
								t1.VoucherNo,
								t2.VoucherRefNo,
								t2.VoucherDate,
								t1.SysRemarks,
								t1.DrCr,
								t1.BankClearedOn,
								t1.PaymentStatus,
								t1.VoucherAmount
							};

				// Use AsQueryable() for better query optimization
				var result = await DataSourceLoader.LoadAsync(query.AsQueryable(), loadOptions);

				return Json(result);
			}
			catch (Exception ex)
			{
				// Log the error (optional but recommended)
				return StatusCode(500, $"An error occurred: {ex.Message}");
			}
		}



	}

}
