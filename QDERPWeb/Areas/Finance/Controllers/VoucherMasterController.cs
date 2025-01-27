using System.Data.SqlClient;
using System.Security.Policy;
using System.Xml.Linq;
using DevExpress.DataAccess.Native.Json;
using DevExpress.DataProcessing.InMemoryDataProcessor;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.DAL.Entities;


namespace QD.ERP.Web.Areas.Finance.Controllers
{
	//[Area("Finance")]
	[Route("api/[controller]/[action]")]
	// [Route("Finapi/[controller]/[action]")]
	[ApiController]
	public class VoucherMasterController : Controller
	{
		private ERPMasterWtDataContext _context;

		public VoucherMasterController(ERPMasterWtDataContext context)
		{
			_context = context;
		}
		[HttpGet]
		public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions)
		{
			var tbl201vouchermasters = _context.Tbl201VoucherMasters.Select(i => new {
				i.VoucherNo,
				i.VoucherDate,
				i.VoucherRefNo,
				i.VoucherNarration,
				i.VoucherEnteredBy,
				i.VoucherEnteredOn,
				i.VoucherVerifiedBy,
				i.VoucherVerifiedOn,
				i.IsVerified,
				i.VoucherApprovedBy,
				i.VoucherApprovedOn,
				i.IsApproved,
				i.VoucherType,
				i.VoucherEffectiveDate,
				i.InvoiceSubmittedDate,
				i.InvoiceDueDate,
				i.InvoiceNoOfDays,
				i.UseSubmittedDate,
				i.SalesPersonCode,
				i.BillNo,
				i.BillDate,
				i.BillPaidTo,
				i.BillRemarks,
				i.VoucherModifiedBy,
				i.VoucherModifiedOn,
				i.AuditVerifiedBy,
				i.AuditVerifiedOn,
				i.IsAuditVerified,
				i.DeliveryNoteNo,
				i.CogsInvoiceNo,
				i.ReferenceNote,
				i.RentalPayslipNo
			});

			// If underlying data is a large SQL table, specify PrimaryKey and PaginateViaPrimaryKey.
			// This can make SQL execution plans more efficient.
			// For more detailed information, please refer to this discussion: https://github.com/DevExpress/DevExtreme.AspNet.Data/issues/336.
			// loadOptions.PrimaryKey = new[] { "VoucherNo" };
			// loadOptions.PaginateViaPrimaryKey = true;

			return Json(await DataSourceLoader.LoadAsync(tbl201vouchermasters, loadOptions));
		}

		[HttpGet]
		public async Task<ActionResult> GetCPPaymentAccounts(DataSourceLoadOptions loadOptions)
		{

			//var qryListOfAccountlists = _context.QryCashAndBankAccounts.Where(p => p.AccountGroupId != null).Select(i => new
			var qryListOfAccountlists = _context.Qry201ListOfAccounts.Where(p => p.AccountGroupId == "A012").Select(i => new
			{
				i.AccountHead,
				i.AccountId,
				i.AccountHeadArabic,
				i.IsLedgerObselete

				//i.AccountGroup,
				//i.MasterGroup,
				//i.MasterGroupId

			});

			return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
		}

		[HttpGet]
		public async Task<ActionResult> GetBPPaymentAccounts(DataSourceLoadOptions loadOptions)
		{

			//var qryListOfAccountlists = _context.QryCashAndBankAccounts.Where(p => p.AccountGroupId != null).Select(i => new
			var qryListOfAccountlists = _context.Qry201ListOfAccounts.Where(p => p.AccountGroupId == "A013").Select(i => new
			{
				i.AccountHead,
				i.AccountId,
				i.AccountHeadArabic,
				i.IsLedgerObselete


			});

			return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
		}

		[HttpGet]
		public async Task<ActionResult> GetVoucherEntryPaymentGrid(DataSourceLoadOptions loadOptions)
		{

			var qryListOfAccountlists = _context.Qry201VoucherEntryScreenDisplays
.Where(p => p.VoucherNo != null).Select(i => new

{
	i.DrCr,
	i.AccountHead,
	i.DrAmount,
	i.CrAmount,
	i.EntryNarration,
	i.SysRemarks
	//,
	//i.,
	//i.MasterGroup,
	//i.MasterGroupId

});

			return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
		}

		[HttpGet]
		public async Task<IActionResult> GetAccountHead(DataSourceLoadOptions loadOptions)
		{
			//var tbl201vouchermasters = _context.Tbl201VoucherEntries.Select(i => new {
			//    i.AccountHead,
			//    i.VoucherNo //not available table in accountid

			//});
			var qryListOfAccountlists = _context.Qry201ListOfAccounts.Select(i => new
			{
				//i.MasterGroupId,
				//i.MasterGroup,
				i.AccountId,
				i.AccountHead,
				i.AccountGroup,
				i.AccountHeadArabic,
				i.ReferenceNo,
				i.IsLedgerObselete
			});


			return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
		}

		//[HttpPost]
		//public async Task<ActionResult> AddVoucherEntry(DataSourceLoadOptions loadOptions, [FromBody] Tbl201VoucherEntry VE)
		//{
		//    if (VE == null)
		//    {
		//        return BadRequest(new { success = false, message = "Invalid data received." });
		//    }

		//    try
		//    {
		//        _context.Tbl201VoucherEntries.Add(VE);
		//        await _context.SaveChangesAsync();
		//        var qryListOfAccountlists = _context.Qry201VoucherEntryScreenDisplays.Where(p => p.VoucherNo == VE.VoucherNo).Select(i => new
		//        {
		//            i.VoucherNo,
		//            i.DrCr,
		//            i.DrAmount,
		//            i.CrAmount,
		//            i.EntryNarration,
		//            i.AccountHead,
		//            i.SysRemarks,
		//        });

		//        return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
		//        //return Json(new { VoucherEntryNo = VE.VoucherNo });
		//        //return Ok(new { success = true, message = "Data inserted successfully!" });
		//    }
		//    catch (Exception ex)
		//    {

		//        return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
		//    }


		//}

		//[HttpPost]
		//public async Task<ActionResult> AddVoucherEntry(DataSourceLoadOptions loadOptions, [FromBody] List<Tbl201VoucherEntry> voucherEntries, string AccountHead, string PaymentAccoutHeadName)
		//{
		//    if (voucherEntries == null || !voucherEntries.Any())
		//    {
		//        return BadRequest(new { success = false, message = "Invalid data received." });
		//    }

		//    try
		//    {
		//        // Add entries to the database
		//        _context.Tbl201VoucherEntries.AddRange(voucherEntries);
		//        await _context.SaveChangesAsync();

		//        // Retrieve updated data for the submitted vouchers
		//        var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
		//        var qryListOfAccountlists = _context.Qry201VoucherEntryScreenDisplays
		//            .Where(p => voucherNos.Contains(p.VoucherNo))
		//            .Select(i => new
		//            {
		//                i.VoucherNo,
		//                i.DrCr,
		//                i.DrAmount,
		//                i.CrAmount,
		//                i.EntryNarration,
		//                AccountHead,
		//                i.SysRemarks,
		//            });

		//        // Materialize the query into a list *only for the second element check*
		//        var listForProcessing = await qryListOfAccountlists.Take(2).ToListAsync(); // Fetch only the first 2 records asynchronously
		//        if (listForProcessing.Count >= 2)
		//        {
		//            var secondAccountHead = listForProcessing[1].AccountHead; // Get the 2nd AccountHead
		//            if (!string.IsNullOrEmpty(secondAccountHead))
		//            {
		//                secondAccountHead = PaymentAccoutHeadName;
		//                listForProcessing[1].AccountHead = secondAccountHead;// Update PaymentAccoutHeadName with the 2nd AccountHead value
		//            }
		//        }

		//        // Pass the original IQueryable to DataSourceLoader for proper async processing
		//        return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
		//    }
		//    catch (Exception ex)
		//    {
		//        return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
		//    }
		//}

		[HttpPost]
		public async Task<ActionResult> AddVoucherEntry(DataSourceLoadOptions loadOptions, [FromBody] List<Tbl201VoucherEntry> voucherEntries, string AccountHead, string PaymentAccoutHeadName, int Gridcount)
		{
			if (voucherEntries == null || !voucherEntries.Any())
			{
				return BadRequest(new { success = false, message = "Invalid data received." });
			}

			try
			{
				// Add entries to the database
				_context.Tbl201VoucherEntries.AddRange(voucherEntries);
				await _context.SaveChangesAsync();


				var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
				var qryListOfAccountlists = _context.Qry201VoucherEntryScreenDisplays
					.Where(p => voucherNos.Contains(p.VoucherNo))
					.OrderBy(i => i.DrCr == "Cr") // Order by Dr first (DrCr != "Cr"), then Cr (DrCr == "Cr")
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



				var resultList = await qryListOfAccountlists.ToListAsync();

				int debitamt = 0; // Initialize debit amount

				var pettycashshabbir = "";
				foreach (var petty in voucherEntries)
				{
					pettycashshabbir = petty.AccountHead;
				}

				foreach (var entry in resultList)
				{
					if (!string.IsNullOrEmpty(entry.AccountHead))
					{

						var accountHead = _context.Qry201ListOfAccounts
												  .Where(a => a.AccountId == entry.AccountHead)
												  .Select(a => a.AccountHead)
												  .FirstOrDefault();



						if (Gridcount != 0)
						{
							// Calculate Debit Amount (DrAmount)

							debitamt = (int)(debitamt + entry.DrAmount);

							// If Dr/Cr is Credit ("Cr"), perform specific logic
							if (entry.DrCr == "Cr" & PaymentAccoutHeadName == "Petty Cash - Shabbir")
							{
								// Check if an existing entry matches
								var existingEntry = _context.Tbl201VoucherEntries
															.FirstOrDefault(v => v.AccountHead == entry.AccountHead
																			  && v.DrCr == "Cr"
																			  && v.VoucherNo == entry.VoucherNo);

								if (existingEntry != null)
								{
									// Update CrAmount by adding the calculated debit amount
									entry.CrAmount = debitamt;

									// Optionally update the existing entry in the database
									existingEntry.VoucherAmount = entry.CrAmount;
									_context.Tbl201VoucherEntries.Update(existingEntry);
									_context.SaveChanges();

								}
								else
								{
									// If no existing entry, assign CrAmount as debitamt
									entry.CrAmount = debitamt;
								}
							}

						}
						entry.AccountHead = accountHead;
					}

				}

				return Json(DataSourceLoader.Load(resultList.AsQueryable(), loadOptions));
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

		[HttpGet]
		public async Task<ActionResult> GetNewCPVoucherNo(DataSourceLoadOptions loadOptions)
		{
			DateTime currentDate = DateTime.Now;
			string currentYear = currentDate.Year.ToString();
			string currentMonth = currentDate.Month.ToString("00");
			string voucherString = "CP-" + currentYear.Substring(currentYear.Length - 2, 2) + "-" + currentMonth + "-";
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


		//[HttpGet]
		//public async Task<ActionResult> GetNewCPVoucherNo(DataSourceLoadOptions loadOptions)
		//{
		//    // Get the voucher No. string and Get the next serial of the voucher No.
		//    DateTime currentDate = DateTime.Now;
		//    string currentYear = currentDate.Year.ToString();
		//    string currentMonth = currentDate.Month.ToString("00");
		//    string voucherString = "CP-" + currentYear.Substring(currentYear.Length - 2, 2) + "-" + currentMonth.Substring(currentMonth.Length - 2, 2) + "-";
		//    string strNewReceiptNo;

		//    // SQL query to get the max voucher number


		//    string sql = "SELECT MAX(CAST(RIGHT(VoucherNo, 3) AS INT)) AS MaxVoucherNo " +
		//                  "FROM tbl201VoucherMaster " +
		//                  "WHERE VoucherNo LIKE {0}";

		//    try
		//    {
		//        var result = await _context.SqlQueryAsync<VoucherResult>(sql, new object[] { voucherString + "%" });

		//        int maxVoucherNo = result.FirstOrDefault()?.MaxVoucherNo ?? 0; // Handle null result


		//        int newVoucherNo = maxVoucherNo + 1;

		//        // Format the new voucher number with leading zeros
		//        strNewReceiptNo = "000" + newVoucherNo.ToString();
		//        strNewReceiptNo = strNewReceiptNo.Substring(strNewReceiptNo.Length - 3);

		//        // Concatenate with the voucher string
		//        strNewReceiptNo = voucherString + strNewReceiptNo;
		//    }
		//    catch (Exception)
		//    {
		//        // Handle cases where there's no existing voucher number
		//        strNewReceiptNo = voucherString + "001";
		//    }

		//    return Json(strNewReceiptNo);
		//}

		[HttpGet]
		public async Task<ActionResult> GetNewBPVoucherNo(DataSourceLoadOptions loadOptions)
		{
			DateTime currentDate = DateTime.Now;
			string currentYear = currentDate.Year.ToString();
			string currentMonth = currentDate.Month.ToString("00");
			string voucherString = "BP-" + currentYear.Substring(currentYear.Length - 2, 2) + "-" + currentMonth + "-";
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

		[HttpPost]
		public async Task<ActionResult> SaveChequeDetails([FromBody] Tbl20113ChequeMaster CM)
		{
			if (CM == null)
			{
				return BadRequest(new { success = false, message = "Invalid data received." });
			}

			try
			{
				_context.Tbl20113ChequeMasters.Add(CM);
				await _context.SaveChangesAsync();
				//return Json(new { VoucherEntryNo = VE.VoucherNo });
				return Ok(new { success = true, message = "Data inserted successfully!" });
			}
			catch (Exception ex)
			{

				return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
			}


		}


		[HttpGet]
		public async Task<ActionResult> GetPreview(DataSourceLoadOptions loadOptions, [FromBody] List<Tbl201VoucherEntry> voucherEntries)
		{
			if (voucherEntries == null || !voucherEntries.Any())
			{
				return BadRequest(new { success = false, message = "Invalid data received." });
			}

			try
			{
				// Retrieve updated data for the submitted vouchers
				var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
				var qryListOfAccountlists = _context.Qry201VoucherEntryScreenDisplays
					.Where(p => voucherNos.Contains(p.VoucherNo))
					.Select(i => new
					{
						i.VoucherNo,
						i.DrCr,
						i.DrAmount,
						i.CrAmount,
						i.EntryNarration,
						i.AccountHead,
						i.SysRemarks,
					});

				// Return the updated data as a JSON response
				return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
			}
		}


		//[HttpGet]
		//public IActionResult GetPreview()
		//{
		//    var previewData = new
		//    {
		//        Title = "Preview Example",
		//        Description = "This is some example data for the preview functionality."
		//    };
		//    return Ok(previewData); // Returns JSON automatically
		//}

		[HttpPost]
		public async Task<ActionResult> DeleteVoucherEntry(DataSourceLoadOptions loadOptions, long voucherEntryNo, string VoucherNo)
		{
			try
			{
				// Find the record to delete
				var record = await _context.Tbl201VoucherEntries.FirstOrDefaultAsync(v => v.VoucherEntryNo == voucherEntryNo);
				if (record == null)
				{
					return NotFound(new { message = "Record not found!" });
				}

				// Remove the selected record
				_context.Tbl201VoucherEntries.Remove(record);
				await _context.SaveChangesAsync();

				// Update the remaining voucher entries
				var voucherEntries = await _context.Tbl201VoucherEntries
												   .Where(ve => ve.VoucherNo == VoucherNo) // Filter by the provided VoucherNo
												   .ToListAsync();

				// Update the grid amounts
				foreach (var entry in voucherEntries)
				{
					if (entry.DrCr == "Dr")
					{
						entry.VoucherAmount = 0; // Reset DrAmount
					}
					else if (entry.DrCr == "Cr")
					{
						entry.VoucherAmount = 0; // Reset CrAmount
					}
				}

				// Save updated entries to the database
				_context.Tbl201VoucherEntries.UpdateRange(voucherEntries);
				await _context.SaveChangesAsync();

				// Retrieve updated data for the grid
				var qryListOfAccountLists = _context.Qry201VoucherEntryScreenDisplays
													.Where(p => p.VoucherNo == VoucherNo)
													.Select(i => new
													{
														i.VoucherNo,
														i.VoucherEntryNo,
														i.DrCr,
														DrAmount = i.DrCr == "Dr" ? 0 : i.DrAmount, // Set DrAmount to 0
														CrAmount = i.DrCr == "Cr" ? 0 : i.CrAmount, // Set CrAmount to 0
														i.EntryNarration,
														i.AccountHead,
														SysRemarks = "" // Remove SysRemarks
													});

				var result = await DataSourceLoader.LoadAsync(qryListOfAccountLists, loadOptions);

				// Return updated data to the grid
				return Json(result);
			}
			catch (Exception ex)
			{
				// Return a detailed error response
				return StatusCode(500, new { message = "An error occurred while deleting the record.", error = ex.Message });
			}
		}



		[HttpPost]
		public async Task<ActionResult> VerifyVoucher([FromBody] string voucherNo)
		{
			try
			{
				// Find the voucher by VoucherNo
				var voucher = _context.Tbl201VoucherMasters.FirstOrDefault(v => v.VoucherNo == voucherNo);

				if (voucher == null)
				{
					throw new Exception("Voucher not found.");
				}

				// Update the fields
				voucher.IsVerified = true;
				voucher.VoucherApprovedBy = "Admin";
				voucher.VoucherApprovedOn = DateTime.Now;

				// Save changes to the database
				_context.SaveChanges();
				return Ok(new { Message = "Voucher verified successfully." });
			}
			catch (Exception ex)
			{
				return BadRequest(new { Message = ex.Message });
			}
		}



		[HttpPost]
		public async Task<IActionResult> Delete(DataSourceLoadOptions loadOptions, long voucherEntryNo, string VoucherNo)
		{
			if (voucherEntryNo == 0 || string.IsNullOrEmpty(VoucherNo))
			{

				return BadRequest(new { success = false, message = "VoucherEntryNo and VoucherNo are required." });
			}

			try
			{
				var allItems = await _context.Tbl201VoucherEntries.ToListAsync();
				var item = allItems.FirstOrDefault(p => p.VoucherEntryNo == voucherEntryNo);
				if (item == null)
				{

					return NotFound(new { success = false, message = "Record not found." });
				}


				_context.Tbl201VoucherEntries.Remove(item);
				await _context.SaveChangesAsync();


				var qryListOfAccountlists = _context.Qry201VoucherEntryScreenDisplays
					.Where(p => p.VoucherNo == VoucherNo)
					.Select(i => new
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


				if (!qryListOfAccountlists.Any())
				{
					return Json(new { success = true, message = "No records found after deletion." });
				}


				var result = await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions);

				return Json(result);
			}

			catch (Exception ex)
			{
				// Handle unexpected errors
				return StatusCode(500, new
				{
					success = false,
					message = "An error occurred while deleting the record.",
					details = ex.Message
				});
			}
		}






	}

}
