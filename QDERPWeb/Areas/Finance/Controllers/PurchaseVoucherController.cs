using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.DAL.Entities;
using System.Xml.Linq;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
	[Route("api/[controller]/[action]")]
	//[ApiController]
	public class PurchaseVoucherController : Controller
	{
		private ERPMasterWtDataContext _context;
		public PurchaseVoucherController(ERPMasterWtDataContext context)
		{
			_context = context;
		}
		[HttpGet]
		public async Task<ActionResult> GetNewVoucherNo(DataSourceLoadOptions loadOptions)
		{
			// Get the voucher No. string and Get the next serial of the voucher No.
			DateTime currentDate = DateTime.Now;
			string currentYear = currentDate.Year.ToString();
			string currentMonth = currentDate.Month.ToString("00");
			string voucherString = "PUR-" + currentYear.Substring(currentYear.Length - 2, 2) + "-" + currentMonth.Substring(currentMonth.Length - 2, 2) + "-";
			string strNewReceiptNo;

			// SQL query to get the max voucher number


			string sql = "SELECT MAX(CAST(RIGHT(VoucherNo, 3) AS INT)) AS MaxVoucherNo " +
						  "FROM tbl201VoucherMaster " +
						  "WHERE VoucherNo LIKE {0}";

			try
			{
				var result = await _context.SqlQueryAsync<VoucherResult>(sql, new object[] { voucherString + "%" });

				int maxVoucherNo = result.FirstOrDefault()?.MaxVoucherNo ?? 0; // Handle null result

				
				int newVoucherNo = maxVoucherNo + 1;

				// Format the new voucher number with leading zeros
				strNewReceiptNo = "000" + newVoucherNo.ToString();
				strNewReceiptNo = strNewReceiptNo.Substring(strNewReceiptNo.Length - 3);

				// Concatenate with the voucher string
				strNewReceiptNo = voucherString + strNewReceiptNo;
			}
			catch (Exception)
			{
				// Handle cases where there's no existing voucher number
				strNewReceiptNo = voucherString + "001";
			}

			return Json(strNewReceiptNo);
		}
		[HttpGet]
		public async Task<ActionResult> GetSupplierName(DataSourceLoadOptions loadOptions)
		{
			var qryListOfAccountlists = _context.Qry201ListOfAccounts.Where(p => p.AccountGroupId == "A012" || p.AccountGroupId == "A003").Select(i => new
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

			return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
		}
		public async Task<ActionResult> GetVoucherEntries(DataSourceLoadOptions loadOptions, string voucherNo)
		{
			var qryListOfAccountlists = _context.Qry201VoucherEntryScreenDisplays.Where(p => p.VoucherNo == voucherNo).Select(i => new
			{
				i.VoucherNo,
				i.DrCr,
				i.DrAmount,
				i.CrAmount,
				i.EntryNarration,
				i.AccountHead,
				i.SysRemarks,
			});

			return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
		}
		[HttpPost]
		public async Task<ActionResult> AddVoucherEntry(DataSourceLoadOptions loadOptions, [FromBody] Tbl201VoucherEntry VE)
		{
			if (VE == null)
			{
				return BadRequest(new { success = false, message = "Invalid data received." });
			}

			try
			{
				_context.Tbl201VoucherEntries.Add(VE);
				await _context.SaveChangesAsync();
				var qryListOfAccountlists = _context.Qry201VoucherEntryScreenDisplays.Where(p => p.VoucherNo == VE.VoucherNo).Select(i => new
				{
					i.VoucherNo,
					i.DrCr,
					i.DrAmount,
					i.CrAmount,
					i.EntryNarration,
					i.AccountHead,
					i.SysRemarks,
				});

				return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
				//return Json(new { VoucherEntryNo = VE.VoucherNo });
				//return Ok(new { success = true, message = "Data inserted successfully!" });
			}
			catch (Exception ex)
			{

				return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
			}


		}
		[HttpPost]
		public async Task<ActionResult> SaveVoucher([FromBody] Tbl201VoucherMaster VM)
		{
			if (VM == null)
			{
				return BadRequest(new { success = false, message = "Invalid data received." });
			}

			try
			{
				_context.Tbl201VoucherMasters.Add(VM);
				await _context.SaveChangesAsync();
				//return Json(new { VoucherEntryNo = VE.VoucherNo });
				return Ok(new { success = true, message = "Data inserted successfully!" });
			}
			catch (Exception ex)
			{

				return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
			}


		}
	}
}
