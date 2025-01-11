using QD.ERP.Web.DAL.Entities;
using QDERPWeb.Models;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using static DevExpress.Xpo.Helpers.AssociatedCollectionCriteriaHelper;
using System.Xml.Linq;
using DevExpress.Xpo;
using System.Data;
using QD.ERP.Web.Areas.Finance.Models;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
	[Route("api/[controller]/[action]")]
	//[ApiController]
	public class SalesVoucherController : Controller
	{
		private ERPMasterWtDataContext _context;
		public SalesVoucherController(ERPMasterWtDataContext context)
		{
			_context = context;
		}
		[HttpGet]
		public async Task<ActionResult> GetClientName(DataSourceLoadOptions loadOptions)
		{
			var qryListOfAccountlists = _context.Qry201ListOfAccounts.Where(p => p.AccountGroupId == "A011" || p.AccountGroupId == "A012").Select(i => new
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
					i.VoucherEntryNo,
					i.DrCr,
					i.DrAmount,
					i.CrAmount,
					i.EntryNarration,
					i.AccountHead,
					i.SysRemarks,
				});

				return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
			}
			catch (Exception ex)
			{

				return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
			}


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
		//[HttpGet]
		//public async Task<ActionResult> GetVoucherDetails(DataSourceLoadOptions loadOptions, string voucherNo)
		//{
		//	if (string.IsNullOrEmpty(voucherNo))
		//	{
		//		return BadRequest(new { success = false, message = "Invalid data received." });
		//	}

		//	try
		//	{
		//		// Check if the voucher number already exists in the database
		//		var isDuplicate = _context.Tbl201VoucherMasters
		//			.Any(p => p.VoucherNo.ToLower() == voucherNo.ToLower());

		//		if (isDuplicate)
		//		{
		//			return BadRequest(new { success = false, message = "The voucher number already exists. Please use another number." });
		//		}

		//		// Fetch the voucher details
		//		var voucherDetails = _context.Tbl201VoucherMasters
		//			.Where(p => p.VoucherNo.ToLower() == voucherNo.ToLower());

		//		// If no voucher details are found, return an appropriate response
		//		if (!voucherDetails.Any())
		//		{
		//			return NotFound(new { success = false, message = "Voucher not found." });
		//		}

		//		// Use DataSourceLoader to process the data
		//		var result = await DataSourceLoader.LoadAsync(voucherDetails, loadOptions);
		//		return Json(result);
		//	}
		//	catch (Exception ex)
		//	{
		//		return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
		//	}
		//}

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


		//[HttpPost]
		//public async Task<IActionResult> Delete(DataSourceLoadOptions loadOptions, long voucherEntryNo, string VoucherNo)
		//{
		//	if (voucherEntryNo == 0 || string.IsNullOrEmpty(VoucherNo))
		//	{
		//		// Return a bad request response if required parameters are missing
		//		return BadRequest(new { success = false, message = "VoucherEntryNo and VoucherNo are required." });
		//	}

		//	try
		//	{


		//		// Find the item in the database using VoucherEntryNo
		//		var allItems = await _context.Tbl201VoucherEntries.ToListAsync();
		//		var item = allItems.FirstOrDefault(p => p.VoucherEntryNo == voucherEntryNo);
		//	//	var item = await _context.Tbl201VoucherEntries.FirstOrDefaultAsync(p => p.VoucherEntryNo == voucherEntryNo);

		//		if (item == null)
		//		{
		//			// If the record is not found, return a not-found response
		//			return NotFound(new { success = false, message = "Record not found." });
		//		}

		//		// Remove the item from the database
		//		_context.Tbl201VoucherEntries.Remove(item);
		//		await _context.SaveChangesAsync();

		//		// Fetch the updated data to return
		//		var qryListOfAccountlists = _context.Qry201VoucherEntryScreenDisplays
		//			.Where(p => p.VoucherNo == VoucherNo)
		//			.Select(i => new
		//			{
		//				i.VoucherNo,
		//				i.VoucherEntryNo,
		//				i.DrCr,
		//				i.DrAmount,
		//				i.CrAmount,
		//				i.EntryNarration,
		//				i.AccountHead,
		//				i.SysRemarks,
		//			});

		//		// Check if the query result is null or empty
		//		if (!qryListOfAccountlists.Any())
		//		{
		//			return Json(new { success = true, message = "No records found after deletion." });
		//		}

		//		// Use DataSourceLoader to format the response
		//		var result = await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions);

		//		return Json(result);
		//	}
		//	catch (ArgumentException argEx)
		//	{
		//		// Handle query-related errors
		//		return BadRequest(new
		//		{
		//			success = false,
		//			message = "Invalid query parameter.",
		//			details = argEx.Message
		//		});
		//	}
		//	catch (Exception ex)
		//	{
		//		// Handle unexpected errors
		//		return StatusCode(500, new
		//		{
		//			success = false,
		//			message = "An error occurred while deleting the record.",
		//			details = ex.Message
		//		});
		//	}
		//}




		[HttpGet]
		public IActionResult CheckVoucherExists(string voucherNo)
		{


			// Check if the Sales Invoice No already exists in the database
			bool exists = _context.Tbl201VoucherEntries.Any(s => s.VoucherNo == voucherNo);

			if (exists)
			{
				return Ok(new { success = false, message = $"The entered Voucher No  has already been recorded in the system. Please correct the Voucher No." });
			}


			return Ok(new { success = true });
		}

	}

}
