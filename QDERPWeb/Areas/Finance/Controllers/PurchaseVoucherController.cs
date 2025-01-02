using DevExpress.XtraRichEdit.Import.Html;
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

			DateTime currentDate = DateTime.Now;

			string currentYear = currentDate.Year.ToString();

			string currentMonth = currentDate.Month.ToString("00");

			string voucherString = "PUR-" + currentYear.Substring(currentYear.Length - 2, 2) + "-" + currentMonth + "-";

			string strNewReceiptNo;

			// SQL query with interpolated string

			string likePattern = voucherString + "%";

			try

			{

				// Use raw SQL query to fetch the maximum voucher number

				var result = await _context.VoucherResults

					.FromSqlInterpolated($@"

        SELECT MAX(CAST(RIGHT(VoucherNo, 3) AS INT)) AS MaxVoucherNo

        FROM Tbl201VoucherEntry

        WHERE VoucherNo LIKE {likePattern}")

					.ToListAsync();

				int maxVoucherNo = result.FirstOrDefault()?.MaxVoucherNo ?? 0;

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


		[HttpGet]
		public async Task<ActionResult> GetAccountHead(DataSourceLoadOptions loadOptions)
		{
			var qryListOfAccountlists = _context.Qry201ListOfAccounts
				 .Where(p => p.IsUsedInPurchase.HasValue ? p.IsUsedInPurchase.Value : false) // Handle null as false
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
					 i.IsUseInSales,
					 i.IsUsedInPurchase
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
		[HttpGet]
		public async Task<ActionResult> GetVoucherDetails(DataSourceLoadOptions loadOptions, string voucherNo)
		{
			if (string.IsNullOrEmpty(voucherNo))
			{
				return BadRequest(new { success = false, message = "Invalid data received." });
			}
			try
			{
				// Fetch the voucher details
				var voucherDetails = _context.Tbl201VoucherMasters
					.Where(p => p.VoucherNo.ToLower() == voucherNo.ToLower());

				// If no voucher details are found, return an appropriate response
				if (!voucherDetails.Any())
				{
					return NotFound(new { success = false, message = "Voucher not found." });
				}

				// Use DataSourceLoader to process the data
				var result = await DataSourceLoader.LoadAsync(voucherDetails, loadOptions);
				return Json(result);
			}
			catch (Exception ex)
			{

				return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
			}


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
		[HttpPost]

		public async Task<ActionResult> DeleteVoucherEntry(DataSourceLoadOptions loadOptions, long voucherEntryNo, string VoucherNo, string PaymentAccoutHeadName)

		{

			try

			{

				// Find the record to delete

				var record = await _context.Tbl201VoucherEntries.FirstOrDefaultAsync(v => v.VoucherEntryNo == voucherEntryNo);

				if (record == null)

				{

					return NotFound(new { message = "Record not found!" });

				}

				// Remove the record

				_context.Tbl201VoucherEntries.Remove(record);

				await _context.SaveChangesAsync();

				// Get the list of updated vouchers

				var voucherEntries = _context.Tbl201VoucherEntries

											  .Where(ve => ve.VoucherNo == VoucherNo) // Filter by the provided VoucherNo

											  .ToList();

				var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();

				// Query the display list

				var qryListOfAccountLists = _context.Qry201VoucherEntryScreenDisplays

													.Where(p => voucherNos.Contains(p.VoucherNo))

													.Select(i => new VoucherEntryDisplayDTO

													{

														VoucherNo = i.VoucherNo,

														VoucherEntryNo = i.VoucherEntryNo,

														DrCr = i.DrCr,

														DrAmount = i.DrAmount,

														CrAmount = i.CrAmount,

														EntryNarration = i.EntryNarration,

														AccountHead = i.AccountHead,

														SysRemarks = i.SysRemarks

													});

				var resultList = await qryListOfAccountLists.ToListAsync();

				// Update the fields in the result list

				foreach (var entry in resultList)

				{

					if (!string.IsNullOrEmpty(entry.AccountHead))

					{

						// Find the account head

						var accountHead = _context.Qry201ListOfAccounts

												  .Where(a => a.AccountId == entry.AccountHead)

												  .Select(a => a.AccountHead)

												  .FirstOrDefault();

						// Update AccountHead and SysRemarks

						entry.AccountHead = accountHead ?? PaymentAccoutHeadName;

						//entry.SysRemarks = PaymentAccoutHeadName;

					}

				}

				// Return the modified list for DataSourceLoader

				return Json(DataSourceLoader.Load(resultList.AsQueryable(), loadOptions));

			}

			catch (Exception ex)

			{

				// Return a detailed error response

				return StatusCode(500, new { message = "An error occurred while deleting the record.", error = ex.Message });

			}

		}

	}

}