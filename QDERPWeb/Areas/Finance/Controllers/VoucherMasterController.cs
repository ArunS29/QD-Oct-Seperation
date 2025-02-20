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
                i.AccountGroupId,
                i.IsLedgerObselete
            });


            return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
        }



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
        public async Task<ActionResult> AddBPVoucherEntry(DataSourceLoadOptions loadOptions, [FromBody] List<Tbl201VoucherEntry> voucherEntries, string AccountHead, string PaymentAccoutHeadName, int Gridcount)
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
                            if (entry.DrCr == "Cr" & PaymentAccoutHeadName == "Riyadh Bank")
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




        [HttpPost]
        public async Task<ActionResult> DeleteVoucherEntry(DataSourceLoadOptions loadOptions, long voucherEntryNo, string VoucherNo, string PaymentAccoutHeadName)
        {
            try
            {
                int debitamt = 0; // Initialize debit amount
                // Find the record to delete
                var record = await _context.Tbl201VoucherEntries.FirstOrDefaultAsync(v => v.VoucherEntryNo == voucherEntryNo);
                if (record == null)
                {
                    return NotFound(new { message = "Record not found!" });
                }

                if (record.DrCr != "Cr")
                {
                    // Remove the record
                    _context.Tbl201VoucherEntries.Remove(record);
                    await _context.SaveChangesAsync();

                }

                // Get the list of updated vouchers
                var voucherEntries = _context.Tbl201VoucherEntries
                                              .Where(ve => ve.VoucherNo == VoucherNo) // Filter by the provided VoucherNo
                                              .ToList();

                var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();

                // Query the display list
                var qryListOfAccountLists = _context.Qry201VoucherEntryScreenDisplays
                                                    .Where(p => voucherNos.Contains(p.VoucherNo))
                                                    .OrderBy(i => i.DrCr == "Cr")
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

                    if (!string.IsNullOrEmpty(entry.AccountHead))
                    {
                        // Find the account head
                        var accountHead = _context.Qry201ListOfAccounts
                                                  .Where(a => a.AccountId == entry.AccountHead)
                                                  .Select(a => a.AccountHead)
                                                  .FirstOrDefault();

                        // Update AccountHead and SysRemarks
                        entry.AccountHead = accountHead ?? PaymentAccoutHeadName;
                        if (resultList.Count == 1)
                        {
                            entry.DrAmount = 0;
                            entry.CrAmount = 0;
                        }

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


        [HttpPost]
        public async Task<ActionResult> DeleteAllVoucherEntry(DataSourceLoadOptions loadOptions, string VoucherNo)
        {
            try
            {
                // Find all records matching the given VoucherNo
                var records = await _context.Tbl201VoucherEntries
                                            .Where(v => v.VoucherNo == VoucherNo)
                                            .ToListAsync();

                if (records == null || !records.Any())
                {
                    return NotFound(new { message = "No records found for the provided VoucherNo!" });
                }

                // Remove all matching records
                _context.Tbl201VoucherEntries.RemoveRange(records);
                await _context.SaveChangesAsync();

                // Fetch updated voucher list
                var voucherEntries = await _context.Tbl201VoucherEntries
                                                   .Where(ve => ve.VoucherNo == VoucherNo)
                                                   .ToListAsync();

                var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct().ToList();

                // Query the updated display list
                var qryListOfAccountLists = _context.Qry201VoucherEntryScreenDisplays
                                                    .Where(p => voucherNos.Contains(p.VoucherNo))
                                                    .OrderBy(i => i.DrCr == "Dr")
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

                // Return the modified list for DataSourceLoader
                return Json(DataSourceLoader.Load(resultList.AsQueryable(), loadOptions));
            }
            catch (Exception ex)
            {
                // Return a detailed error response
                return StatusCode(500, new { message = "An error occurred while deleting the records.", error = ex.Message });
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


        public IActionResult CostAllocation(string voucherNo, string accountHead, string voucherAmount, string drCr, string effectiveDate)
        {
            // Log or debug the incoming parameters
            ViewBag.VoucherNo = voucherNo;
            ViewBag.AccountHead = accountHead;
            ViewBag.VoucherAmount = voucherAmount;
            ViewBag.DrCr = drCr;
            ViewBag.EffectiveDate = effectiveDate;

            return View();
        }


        [HttpGet]
        public IActionResult GetEmployeeName()
        {
            var data = _context.Tbl101Employees
                .Select(c => new
                {
                    c.EmployeeId,
                    c.EmployeeName,
                    c.NationalId

                }).ToList();

            return Ok(data);
        }
        [HttpPost]
        public async Task<ActionResult> SaveCostAllocation([FromBody] Tbl20104EmployeeAllocationMaster EM)
        {

            try
            {
                _context.Tbl20104EmployeeAllocationMasters.Add(EM);
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
        public async Task<ActionResult> DeleteBPVoucherEntry(DataSourceLoadOptions loadOptions, long voucherEntryNo, string VoucherNo, string PaymentAccoutHeadName)
        {
            try
            {
                int debitamt = 0; // Initialize debit amount
                // Find the record to delete
                var record = await _context.Tbl201VoucherEntries.FirstOrDefaultAsync(v => v.VoucherEntryNo == voucherEntryNo);
                if (record == null)
                {
                    return NotFound(new { message = "Record not found!" });
                }

                if (record.DrCr != "Cr")
                {
                    // Remove the record
                    _context.Tbl201VoucherEntries.Remove(record);
                    await _context.SaveChangesAsync();

                }

                // Get the list of updated vouchers
                var voucherEntries = _context.Tbl201VoucherEntries
                                              .Where(ve => ve.VoucherNo == VoucherNo) // Filter by the provided VoucherNo
                                              .ToList();

                var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();

                // Query the display list
                var qryListOfAccountLists = _context.Qry201VoucherEntryScreenDisplays
                                                    .Where(p => voucherNos.Contains(p.VoucherNo))
                                                    .OrderBy(i => i.DrCr == "Cr")
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
                    debitamt = (int)(debitamt + entry.DrAmount);

                    // If Dr/Cr is Credit ("Cr"), perform specific logic
                    if (entry.DrCr == "Cr" & PaymentAccoutHeadName == "Riyadh Bank")
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

                    if (!string.IsNullOrEmpty(entry.AccountHead))
                    {
                        // Find the account head
                        var accountHead = _context.Qry201ListOfAccounts
                                                  .Where(a => a.AccountId == entry.AccountHead)
                                                  .Select(a => a.AccountHead)
                                                  .FirstOrDefault();

                        // Update AccountHead and SysRemarks
                        entry.AccountHead = accountHead ?? PaymentAccoutHeadName;
                        if (resultList.Count == 1)
                        {
                            entry.DrAmount = 0;
                            entry.CrAmount = 0;
                        }

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

        [HttpGet]
        public async Task<IActionResult> GetEditAccountHead(DataSourceLoadOptions loadOptions)
        {
            try
            {
                var qryListOfAccountlists = _context.Qry201ListOfAccounts
              .Select(i => new
              {
                  i.AccountId,
                  i.AccountHead,
                  i.AccountHeadArabic
              });

                return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));

            }
            catch (Exception ex)
            {
                throw ex;
            }
          
        }

        [HttpGet]

        public IActionResult IsVoucherLocked([FromQuery] DateTime voucherDate)

        {

            try

            {

                Console.WriteLine($"Received Voucher Date: {voucherDate}"); // Debugging

                var isLocked = _context.Tbl90117VoucherDateLockings

                    .Any(v => v.VoucherTypeCode == "PAYMENT_VOUCHER" && v.VoucherDateLocked >= voucherDate);

                return Ok(new { Locked = isLocked });

            }

            catch (Exception ex)

            {

                return StatusCode(500, new { Message = "An error occurred.", Error = ex.Message });

            }

        }



        //[HttpPut]
        //public async Task<ActionResult> UpdateVoucherEntry(DataSourceLoadOptions loadOptions, [FromQuery] List<long> VoucherEntryNos,[FromBody] VoucherEntryUpdate updatedEntry)
        //{
        //    try
        //    {
        //        if (VoucherEntryNos == null || !VoucherEntryNos.Any())
        //        {
        //            return BadRequest(new { message = "Invalid voucher entry numbers." });
        //        }

        //        int debitamt = 0;

        //        // Find the existing records
        //        var records = await _context.Tbl201VoucherEntries
        //                                    .Where(v => VoucherEntryNos.Contains(v.VoucherEntryNo))
        //                                    .ToListAsync();

        //        if (!records.Any())
        //        {
        //            return NotFound(new { message = "Record not found!" });
        //        }

        //        // Update records
        //        foreach (var record in records)
        //        {
        //            record.VoucherAmount = updatedEntry.VoucherAmount;
        //            record.EntryNarration = updatedEntry.EntryNarration;
        //            record.AccountHead = updatedEntry.AccountHead;
        //            record.DrCr = updatedEntry.DrCr;
        //        }

        //        await _context.SaveChangesAsync(); // Save all changes once

        //        // Get the updated list of voucher entries
        //        var voucherEntries = await _context.Tbl201VoucherEntries
        //                                           .Where(ve => ve.VoucherNo == updatedEntry.VoucherNo)
        //                                           .ToListAsync();

        //        var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct().ToList();

        //        // Query to get the display list
        //        var qryListOfAccountLists = await _context.Qry201VoucherEntryScreenDisplays
        //                                                  .Where(p => voucherNos.Contains(p.VoucherNo))
        //                                                  .OrderByDescending(i => i.DrCr == "Cr") // Correct sorting for "Cr" entries
        //                                                  .Select(i => new VoucherEntryDisplayDTO
        //                                                  {
        //                                                      VoucherNo = i.VoucherNo,
        //                                                      VoucherEntryNo = i.VoucherEntryNo,
        //                                                      DrCr = i.DrCr,
        //                                                      DrAmount = i.DrAmount,
        //                                                      CrAmount = i.CrAmount,
        //                                                      EntryNarration = i.EntryNarration,
        //                                                      AccountHead = i.AccountHead,
        //                                                      SysRemarks = i.SysRemarks
        //                                                  })
        //                                                  .ToListAsync();

        //        // Process the updated list
        //        foreach (var entry in qryListOfAccountLists)
        //        {
        //            debitamt += (int?)entry.DrAmount ?? 0;

        //            // Adjust CrAmount for specific conditions
        //            if (entry.DrCr == "Cr" && updatedEntry.PaymentAccoutHeadName == "Petty Cash - Shabbir")
        //            {
        //                var existingEntry = records.FirstOrDefault(v => v.AccountHead == entry.AccountHead && v.DrCr == "Cr" && v.VoucherNo == entry.VoucherNo);

        //                if (existingEntry != null)
        //                {
        //                    entry.CrAmount = debitamt;
        //                    existingEntry.VoucherAmount = entry.CrAmount;
        //                }
        //                else
        //                {
        //                    entry.CrAmount = debitamt;
        //                }
        //            }

        //            // Update AccountHead from lookup table if necessary
        //            if (!string.IsNullOrEmpty(entry.AccountHead))
        //            {
        //                var accountHead = await _context.Qry201ListOfAccounts
        //                                                .Where(a => a.AccountId == entry.AccountHead)
        //                                                .Select(a => a.AccountHead)
        //                                                .FirstOrDefaultAsync();

        //                entry.AccountHead = accountHead ?? updatedEntry.PaymentAccoutHeadName;

        //                if (qryListOfAccountLists.Count == 1)
        //                {
        //                    entry.DrAmount = 0;
        //                    entry.CrAmount = 0;
        //                }
        //            }
        //        }

        //        await _context.SaveChangesAsync(); // Save changes made during processing

        //        // Return the updated result list
        //        return Ok(DataSourceLoader.Load(qryListOfAccountLists.AsQueryable(), loadOptions));
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "An error occurred while updating the record.", error = ex.Message });
        //    }
        //}


        //[HttpPost]
        //public async Task<IActionResult> UpdateVoucherEntry(DataSourceLoadOptions loadOptions, [FromBody] VoucherEntryUpdate updatedEntry)
        //{
        //    try
        //    {
        //        if (updatedEntry == null)
        //        {
        //            return BadRequest("Invalid document data.");
        //        }

        //        // Find the existing document by DocumentNo
        //        var document = await _context.Tbl201VoucherEntries
        //            .FirstOrDefaultAsync(d => d.VoucherEntryNo == updatedEntry.VoucherEntryNo);

        //        // Check if the document exists
        //        if (document == null)
        //        {
        //            return NotFound($"Document with DocumentNo {updatedEntry.VoucherEntryNo} not found.");
        //        }

        //        // Query the list of account lists
        //        var qryListOfAccountlists = _context.Tbl201VoucherEntries
        //            .Where(p => p.VoucherEntryNo == document.VoucherEntryNo)
        //            .Select(i => new
        //            {
        //                i.DocumentNo,
        //                i.DocumentType,
        //                i.DocumentRefNo,
        //                i.DocumentRemarks,
        //                i.DocumentExpDate,
        //                i.DocumentExpDateAr,
        //                i.DocumentNotificationDate,
        //            });

        //        // Return the data
        //        return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception (optional)
        //        Console.WriteLine($"Error: {ex.Message}");
        //        return StatusCode(500, "An error occurred while processing your request.");
        //    }
        //}







    }

}
