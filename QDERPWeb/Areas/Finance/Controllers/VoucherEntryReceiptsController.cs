using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.Areas.Finance.Views;
using QD.ERP.Web.DAL.Entities;
//using QD.ERP.Web.DAL.Entities;

namespace Form.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [Area("Finance")]
    [ApiController]
    public class VoucherEntryReceiptsController : Controller
    {
        private ERPMasterWtDataContext _context;

        public VoucherEntryReceiptsController(ERPMasterWtDataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetReceivingAccount(DataSourceLoadOptions loadOptions)
        {
            try
            {
                var tbl20101salespersonmasters = _context.Qry201ListOfAccounts.Where(p => p.AccountGroupId == "A013").Select(i => new
                {

                    i.AccountId,
                    i.AccountHead,
                    i.AccountHeadArabic,
                    i.IsLedgerObselete
                });

                return Json(await DataSourceLoader.LoadAsync(tbl20101salespersonmasters, loadOptions));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }
        }



        [HttpGet]
        public async Task<IActionResult> GetAccountHead(DataSourceLoadOptions loadOptions, string SelectedPaymentAccount)
        {
            try
            {
                var qryListOfAccountlists = _context.Qry201ListOfAccounts
               .Where(i => i.AccountId != SelectedPaymentAccount) // Exclude the Receiving Account value
               .Select(i => new
               {
                   i.AccountId,
                   i.AccountHead,
                   i.AccountGroup,
                   i.AccountHeadArabic,
                   i.ReferenceNo,
                   i.IsLedgerObselete
               });

                return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
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

        //[HttpPost]
        //public async Task<ActionResult> AddVoucherEntry(DataSourceLoadOptions loadOptions, [FromBody] List<Tbl201VoucherEntry> voucherEntries)
        //{
        //    if (voucherEntries == null)
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

        //             .Where(p => voucherNos.Contains(p.VoucherNo))

        //             .Select(i => new

        //             {

        //                 i.VoucherNo,

        //                 i.DrCr,

        //                 i.DrAmount,

        //                 i.CrAmount,

        //                 i.EntryNarration,

        //                 i.AccountHead,

        //                 i.SysRemarks,

        //                 i.VoucherEntryNo
        //             });

        //        // Return the updated data as a JSON response

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
                Tbl201VoucherMaster voucherMaster = new();
                int aEntryAmount = 0;
                int Amt = 0;
                var Remarks = "";
                bool IsMatchingEntry = false;
                //var matchingEntries;
                List<VoucherEntryDisplayDTO> matchingEntries = new();
                var voucherNos1 = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
                var qryListOfAccountlists1 = _context.Qry201VoucherEntryScreenDisplays
                    .Where(p => voucherNos1.Contains(p.VoucherNo))
                    .OrderBy(i => i.DrCr == "Dr" ? 1 : 0) // Ensures "Dr" entries come first
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

                var resultList1 = await qryListOfAccountlists1.ToListAsync();
                int crCount1 = resultList1.Count(i => i.DrCr == "Dr");
                if (crCount1 >= 2)
                {
                    matchingEntries = resultList1.Where(x => x.AccountHead == PaymentAccoutHeadName).ToList();
                    IsMatchingEntry = true;
                }

                bool isVoucherExists = _context.Tbl201VoucherMasters
               .Any(v => v.VoucherNo == voucherEntries[0].VoucherNo);

                if (!isVoucherExists)
                {
                    voucherMaster.VoucherNo = voucherEntries[0].VoucherNo;
                    voucherMaster.VoucherDate = DateTime.Now;
                    _context.Tbl201VoucherMasters.AddRange(voucherMaster);
                }

                // Add entries to the database
                _context.Tbl201VoucherEntries.AddRange(voucherEntries);
                await _context.SaveChangesAsync();

                //SaveVoucher(voucherEntries);


                var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
                var qryListOfAccountlists = _context.Qry201VoucherEntryScreenDisplays
                    .Where(p => voucherNos.Contains(p.VoucherNo))
                    .OrderBy(i => i.DrCr == "Dr" ? 1 : 0) // Ensures "Dr" entries come first
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

                int crCount = resultList.Count(i => i.DrCr == "Dr");

                //var lastCrEntry = resultList.LastOrDefault(i => i.DrCr == "Cr");

                var newDrEntry = resultList
    .Where(i => i.DrCr == "Dr")
    .OrderByDescending(i => i.VoucherEntryNo) // Assuming this is sequential
    .FirstOrDefault();



                var newlist = newDrEntry != null ? new List<VoucherEntryDisplayDTO> { newDrEntry } : new List<VoucherEntryDisplayDTO>();

                var pettycashid = "";
                foreach (var petty in newlist)
                {
                    pettycashid = petty.AccountHead;
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

                            debitamt = (int)(debitamt + entry.CrAmount);

                            // If Dr/Cr is Credit ("Cr"), perform specific logic
                            if (entry.DrCr == "Dr" && crCount == 1)
                            // if (crCount==1)
                            {
                                // Check if an existing entry matches
                                var existingEntry = _context.Tbl201VoucherEntries
                                                            .FirstOrDefault(v => v.AccountHead == entry.AccountHead
                                                                              && v.DrCr == "Dr"
                                                                              && v.VoucherNo == entry.VoucherNo);

                                if (existingEntry != null)
                                {
                                    // Update CrAmount by adding the calculated debit amount
                                    entry.DrAmount = debitamt;
                                    entry.SysRemarks = Remarks;

                                    // Optionally update the existing entry in the database
                                    existingEntry.VoucherAmount = entry.DrAmount;
                                    existingEntry.SysRemarks = entry.SysRemarks;
                                    _context.Tbl201VoucherEntries.Update(existingEntry);
                                    _context.SaveChanges();

                                }
                                else
                                {
                                    // If no existing entry, assign CrAmount as debitamt
                                    entry.DrAmount = debitamt;
                                }
                            }
                            else if (newDrEntry != null && crCount >= 2 && entry.AccountHead == pettycashid)
                            {
                                if (entry.DrCr == "Dr")
                                {

                                    if (IsMatchingEntry == true)
                                    {
                                        // Accumulate `aEntryAmount` correctly
                                        foreach (var mEntry in matchingEntries)
                                        {
                                            aEntryAmount += (int)mEntry.DrAmount; // Accumulate CrAmount correctly
                                        }

                                        // Process voucherEntries
                                        foreach (var mVoucherEntry in voucherEntries)
                                        {
                                            int eAmount = (int)mVoucherEntry.VoucherAmount;
                                            Amt = aEntryAmount + eAmount;

                                            // Do something with Amt if required
                                        }

                                        // Check if an existing entry matches
                                        var existingEntry = _context.Tbl201VoucherEntries
                                                                    .FirstOrDefault(v => v.AccountHead == entry.AccountHead
                                                                                      && v.DrCr == "Dr"
                                                                                      && v.VoucherNo == entry.VoucherNo);

                                        if (existingEntry != null)
                                        {
                                            // Update CrAmount by adding the calculated debit amount
                                            entry.DrAmount = Amt;
                                            entry.SysRemarks = Remarks;

                                            // Optionally update the existing entry in the database
                                            existingEntry.VoucherAmount = entry.DrAmount;
                                            existingEntry.SysRemarks = entry.SysRemarks;
                                            _context.Tbl201VoucherEntries.Update(existingEntry);
                                            _context.SaveChanges();

                                        }
                                        else
                                        {
                                            // If no existing entry, assign CrAmount as debitamt
                                            // entry.CrAmount = debitamt;
                                        }
                                    }

                                }

                            }

                        }
                        entry.AccountHead = accountHead;
                        if (Remarks == "")
                        {
                            Remarks = entry.AccountHead;
                        }
                        else
                        {
                            Remarks = Remarks + "," + entry.AccountHead;
                        }
                    }

                }

                return Json(DataSourceLoader.Load(resultList.AsQueryable(), loadOptions));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }
        }
        [HttpGet]

        public async Task<ActionResult> GetNewBRVoucherNo(DataSourceLoadOptions loadOptions)

        {

            DateTime currentDate = DateTime.Now;

            string currentYear = currentDate.Year.ToString();

            string currentMonth = currentDate.Month.ToString("00");

            string voucherString = "BR-" + currentYear.Substring(currentYear.Length - 2, 2) + "-" + currentMonth + "-";

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
        //public async Task<ActionResult> DeleteVoucherEntry(DataSourceLoadOptions loadOptions, long voucherEntryNo, string VoucherNo /*string PaymentAccoutHeadName*/)
        //{
        //    try
        //    {
        //        // Find the record to delete
        //        var record = await _context.Tbl201VoucherEntries.FirstOrDefaultAsync(v => v.VoucherEntryNo == voucherEntryNo);
        //        if (record == null)
        //        {
        //            return NotFound(new { message = "Record not found!" });
        //        }

        //        // Remove the record
        //        _context.Tbl201VoucherEntries.Remove(record);
        //        await _context.SaveChangesAsync();

        //        // Get the list of updated vouchers
        //        var voucherEntries = _context.Tbl201VoucherEntries
        //                                      .Where(ve => ve.VoucherNo == VoucherNo) // Filter by the provided VoucherNo
        //                                      .ToList();

        //        var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();

        //        // Query the display list
        //        var qryListOfAccountLists = _context.Qry201VoucherEntryScreenDisplays
        //                                            .Where(p => voucherNos.Contains(p.VoucherNo))

        //             .Select(i => new

        //             {

        //                 i.VoucherNo,

        //                 i.DrCr,

        //                 i.DrAmount,

        //                 i.CrAmount,

        //                 i.EntryNarration,

        //                 i.AccountHead,

        //                 i.SysRemarks,

        //             });

        //        var resultList = await qryListOfAccountLists.ToListAsync();

        //        //// Update the fields in the result list
        //        //foreach (var entry in resultList)
        //        //{
        //        //    if (!string.IsNullOrEmpty(entry.AccountHead))
        //        //    {
        //        //        // Find the account head
        //        //        var accountHead = _context.Qry201ListOfAccounts
        //        //                                  .Where(a => a.AccountId == entry.AccountHead)
        //        //                                  .Select(a => a.AccountHead)
        //        //                                  .FirstOrDefault();

        //        //        // Update AccountHead and SysRemarks
        //        //        entry.AccountHead = accountHead ?? PaymentAccoutHeadName;
        //        //        //entry.SysRemarks = PaymentAccoutHeadName;
        //        //    }
        //        //}

        //        // Return the modified list for DataSourceLoader
        //        return Json(DataSourceLoader.Load(resultList.AsQueryable(), loadOptions));
        //    }
        //    catch (Exception ex)
        //    {
        //        // Return a detailed error response
        //        return StatusCode(500, new { message = "An error occurred while deleting the record.", error = ex.Message });
        //    }
        //}

        //[HttpPost]

        //public async Task<ActionResult> DeleteVoucherEntry(DataSourceLoadOptions loadOptions, long voucherEntryNo, string VoucherNo, string PaymentAccoutHeadName)

        //{

        //    try

        //    {

        //        // Find the record to delete

        //        var record = await _context.Tbl201VoucherEntries.FirstOrDefaultAsync(v => v.VoucherEntryNo == voucherEntryNo);

        //        if (record == null)

        //        {

        //            return NotFound(new { message = "Record not found!" });

        //        }

        //        // Remove the record

        //        _context.Tbl201VoucherEntries.Remove(record);

        //        await _context.SaveChangesAsync();

        //        // Get the list of updated vouchers

        //        var voucherEntries = _context.Tbl201VoucherEntries

        //                                      .Where(ve => ve.VoucherNo == VoucherNo) // Filter by the provided VoucherNo

        //                                      .ToList();

        //        var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();

        //        // Query the display list

        //        var qryListOfAccountLists = _context.Qry201VoucherEntryScreenDisplays

        //                                            .Where(p => voucherNos.Contains(p.VoucherNo))

        //                                            .Select(i => new VoucherEntryDisplayDTO

        //                                            {

        //                                                VoucherNo = i.VoucherNo,

        //                                                VoucherEntryNo = i.VoucherEntryNo,

        //                                                DrCr = i.DrCr,

        //                                                DrAmount = i.DrAmount,

        //                                                CrAmount = i.CrAmount,

        //                                                EntryNarration = i.EntryNarration,

        //                                                AccountHead = i.AccountHead,

        //                                                SysRemarks = i.SysRemarks

        //                                            });

        //        var resultList = await qryListOfAccountLists.ToListAsync();

        //        // Update the fields in the result list

        //        foreach (var entry in resultList)

        //        {

        //            if (!string.IsNullOrEmpty(entry.AccountHead))

        //            {

        //                // Find the account head

        //                var accountHead = _context.Qry201ListOfAccounts

        //                                          .Where(a => a.AccountId == entry.AccountHead)

        //                                          .Select(a => a.AccountHead)

        //                                          .FirstOrDefault();

        //                // Update AccountHead and SysRemarks

        //                entry.AccountHead = accountHead ?? PaymentAccoutHeadName;

        //                //entry.SysRemarks = PaymentAccoutHeadName;

        //            }

        //        }

        //        // Return the modified list for DataSourceLoader

        //        return Json(DataSourceLoader.Load(resultList.AsQueryable(), loadOptions));

        //    }

        //    catch (Exception ex)

        //    {

        //        // Return a detailed error response

        //        return StatusCode(500, new { message = "An error occurred while deleting the record.", error = ex.Message });

        //    }

        //}
        [HttpGet]
        public async Task<IActionResult> CheckEmployeeAllocation(string AccountHead, string AccountID)
        {
            try
            {
                var allocation = await _context.Tbl201ChartOfAccounts
                    .Where(x => x.AccountHead == AccountHead && x.AccountId == AccountID && x.IsEmployeeAllocated == true)
                    .FirstOrDefaultAsync();

                if (allocation != null)
                {
                    return Ok(new { isAllocated = true });
                }
                else
                {
                    return Ok(new { isAllocated = false });
                }
            }
            catch (Exception ex)
            {
                // Log the error here if necessary
                return StatusCode(500, new { message = "An error occurred while checking property allocation.", error = ex.Message });
            }
        }
        public IActionResult CostAllocation(string voucherNo, string accountHead, string voucherAmount, string drCr, long voucherEntryNo)
        {
            // Log or debug the incoming parameters
            ViewBag.VoucherNo = voucherNo;
            ViewBag.AccountHead = accountHead;
            ViewBag.VoucherAmount = voucherAmount;
            ViewBag.DrCr = drCr;
            ViewBag.VoucherEntryNo = voucherEntryNo;
            return PartialView("~/Areas/Finance/Views/_CostAllocation.cshtml"); // Ensure this is inside /Views/VoucherEntryReceipts/
        }
        public IActionResult PropertyAllocation(string voucherNo, string accountHead, string voucherAmount, string drCr, long voucherEntryNo, string accountId)
        {
            // Log or debug the incoming parameters
            ViewBag.VoucherNo = voucherNo;
            ViewBag.AccountHead = accountHead;
            ViewBag.VoucherAmount = voucherAmount;
            ViewBag.DrCr = drCr;
            ViewBag.VoucherEntryNo = voucherEntryNo;
            ViewBag.AccountID = accountId;
            return PartialView("~/Areas/Finance/Views/_PropertyAllocation.cshtml"); // Ensure this is inside /Views/VoucherEntryReceipts/
        }

        public IActionResult EmployeeAllocation(string voucherNo, string accountHead, string voucherAmount, string drCr, long voucherEntryNo, string accountId)
        {
            // Log or debug the incoming parameters
            ViewBag.VoucherNo = voucherNo;
            ViewBag.AccountHead = accountHead;
            ViewBag.VoucherAmount = voucherAmount;
            ViewBag.DrCr = drCr;
            ViewBag.VoucherEntryNo = voucherEntryNo;
            ViewBag.AccountID = accountId;
            return PartialView("~/Areas/Finance/Views/_EmployeeAllocation.cshtml"); // Ensure this is inside /Views/VoucherEntryReceipts/
        }
        public IActionResult BillsReceivable(string voucherNo, string accountHead, string voucherAmount, string drCr, long voucherEntryNo)
        {
            // Log or debug the incoming parameters
            ViewBag.VoucherNo = voucherNo;
            ViewBag.AccountHead = accountHead;
            ViewBag.VoucherAmount = voucherAmount;
            ViewBag.DrCr = drCr;
            ViewBag.VoucherEntryNo = voucherEntryNo;

            return PartialView("~/Areas/Finance/Views/_BillsReceivables.cshtml"); // Ensure this is inside /Views/VoucherEntryReceipts/
        }
        [HttpGet]
        public async Task<IActionResult> CheckIsMaintainBillByBill(string AccountHead, string AccountID)
        {
            try
            {
                var allocation = await _context.Qry201ListOfAccounts
                    .Where(x => x.AccountHead == AccountHead && x.AccountId == AccountID && x.IsMaintainBillByBill == true)
                    .FirstOrDefaultAsync();

                if (allocation != null)
                {
                    return Ok(new { isAllocated = true });
                }
                else
                {
                    return Ok(new { isAllocated = false });
                }
            }
            catch (Exception ex)
            {
                // Log the error here if necessary
                return StatusCode(500, new { message = "An error occurred while checking property allocation.", error = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> CheckPropertyAllocation(string AccountHead, string AccountID)
        {
            try
            {
                var allocation = await _context.Tbl201ChartOfAccounts
                    .Where(x => x.AccountHead == AccountHead && x.AccountId == AccountID && x.IsPropertyAllocated == true)
                    .FirstOrDefaultAsync();

                if (allocation != null)
                {
                    return Ok(new { isAllocated = true });
                }
                else
                {
                    return Ok(new { isAllocated = false });
                }
            }
            catch (Exception ex)
            {
                // Log the error here if necessary
                return StatusCode(500, new { message = "An error occurred while checking property allocation.", error = ex.Message });
            }
        }


        [HttpPost]
        public async Task<ActionResult> AddCRDrVoucherEntry(DataSourceLoadOptions loadOptions, [FromBody] List<Tbl201VoucherEntry> voucherEntries, string AccountHead, string PaymentAccoutHeadName, int Gridcount)
        {
            if (voucherEntries == null || !voucherEntries.Any())
            {
                return BadRequest(new { success = false, message = "Invalid data received." });
            }

            try
            {
                Tbl201VoucherMaster voucherMaster = new();
                int aEntryAmount = 0;
                int Amt = 0, Crmt = 0, CrMinusAmt = 0, NewCrMinusAmt = 0, ExistingCrMinusAmt = 0;
                bool IsMatchingEntry = false;

                //var matchingEntries;
                List<VoucherEntryDisplayDTO> matchingEntries = new();
                var voucherNos1 = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
                var qryListOfAccountlists1 = _context.Qry201VoucherEntryScreenDisplays
                    .Where(p => voucherNos1.Contains(p.VoucherNo))
                    .OrderBy(i => i.DrCr == "Cr" ? 1 : 0) // Ensures "Dr" entries come first
                    .Select(i => new VoucherEntryDisplayDTO
                    {
                        VoucherNo = i.VoucherNo,
                        VoucherEntryNo = i.VoucherEntryNo,
                        DrCr = i.DrCr,
                        DrAmount = i.DrAmount,
                        CrAmount = i.CrAmount,
                        VoucherAmountFormatted = i.VoucherAmountFormatted,
                        EntryNarration = i.EntryNarration,
                        AccountHead = i.AccountHead,
                        SysRemarks = i.SysRemarks
                    });

                var resultList1 = await qryListOfAccountlists1.ToListAsync();
                int crCount1 = resultList1.Count(i => i.DrCr == "Dr");
                if (crCount1 >= 2)
                {
                    matchingEntries = resultList1.Where(x => x.AccountHead == PaymentAccoutHeadName).ToList();
                    if (matchingEntries.Count != 0)
                    {
                        ExistingCrMinusAmt = (int)matchingEntries[0].CrAmount;
                        IsMatchingEntry = true;
                    }

                }

                bool isVoucherExists = _context.Tbl201VoucherMasters
               .Any(v => v.VoucherNo == voucherEntries[0].VoucherNo);

                if (!isVoucherExists)
                {
                    voucherMaster.VoucherNo = voucherEntries[0].VoucherNo;
                    voucherMaster.VoucherDate = DateTime.Now;
                    _context.Tbl201VoucherMasters.AddRange(voucherMaster);
                }

                // Add entries to the database
                _context.Tbl201VoucherEntries.AddRange(voucherEntries);
                await _context.SaveChangesAsync();

                //SaveVoucher(voucherEntries);


                var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
                var qryListOfAccountlists = _context.Qry201VoucherEntryScreenDisplays
                    .Where(p => voucherNos.Contains(p.VoucherNo))
                    .OrderBy(i => i.DrCr == "Dr" ? 1 : 0) // Ensures "Dr" entries come first
                    .Select(i => new VoucherEntryDisplayDTO
                    {
                        VoucherNo = i.VoucherNo,
                        VoucherEntryNo = i.VoucherEntryNo,
                        DrCr = i.DrCr,
                        DrAmount = i.DrAmount,
                        CrAmount = i.CrAmount,

                        VoucherAmountFormatted = i.VoucherAmountFormatted,
                        EntryNarration = i.EntryNarration,
                        AccountHead = i.AccountHead,
                        SysRemarks = i.SysRemarks
                    });


                var resultList = await qryListOfAccountlists.ToListAsync();

                int debitamt = 0; // Initialize debit amount

                int crCount = resultList.Count(i => i.DrCr == "Cr");

                //var lastCrEntry = resultList.LastOrDefault(i => i.DrCr == "Cr");

                var newCrEntry = resultList
    .Where(i => i.DrCr == "Dr")
    .OrderByDescending(i => i.VoucherEntryNo) // Assuming this is sequential
    .FirstOrDefault();



                var newlist = newCrEntry != null ? new List<VoucherEntryDisplayDTO> { newCrEntry } : new List<VoucherEntryDisplayDTO>();

                var pettycashid = "";
                foreach (var petty in newlist)
                {
                    pettycashid = petty.AccountHead;
                    NewCrMinusAmt = (int)petty.DrAmount;
                }

                foreach (var entry in resultList)
                {

                    if (!entry.SysRemarks.Contains("Received by:"))
                    {
                        CrMinusAmt = (int)entry.VoucherAmountFormatted;

                        entry.DrAmount = CrMinusAmt;
                        if (IsMatchingEntry == true)
                        {
                            //CrMinusAmt = (int)entry.VoucherAmountFormatted + NewCrMinusAmt;
                            CrMinusAmt = NewCrMinusAmt + ExistingCrMinusAmt;


                            var existingEntry = _context.Tbl201VoucherEntries
                                      .FirstOrDefault(v => v.AccountHead == entry.AccountHead
                                                        && v.DrCr == "Dr"
                                                        && v.VoucherNo == entry.VoucherNo);

                            // Optionally update the existing entry in the database
                            existingEntry.VoucherAmount = CrMinusAmt;
                            entry.DrAmount = -CrMinusAmt;
                            _context.Tbl201VoucherEntries.Update(existingEntry);
                            _context.SaveChanges();

                        }
                    }


                    if (!string.IsNullOrEmpty(entry.AccountHead))
                    {

                        var accountHead = _context.Qry201ListOfAccounts
                                                  .Where(a => a.AccountId == entry.AccountHead)
                                                  .Select(a => a.AccountHead)
                                                  .FirstOrDefault();

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
        public async Task<ActionResult> DeleteVoucherEntry(DataSourceLoadOptions loadOptions, long voucherEntryNo, string VoucherNo, string PaymentAccoutHeadName, int Gridcount)
        {


            try
            {
                Tbl201VoucherMaster voucherMaster = new();
                int aEntryAmount = 0;
                int Amt = 0;
                var Remarks = "";
                bool IsMatchingEntry = false;
                //var matchingEntries;
                List<VoucherEntryDisplayDTO> matchingEntries = new();
                //  var voucherNos1 = voucherEntries.Select(ve => ve.VoucherNo).Distinct();


                // Get the list of updated vouchers
                var voucherEntries1 = _context.Tbl201VoucherEntries
                                              .Where(ve => ve.VoucherNo == VoucherNo) // Filter by the provided VoucherNo
                                              .ToList();

                var voucherNos1 = voucherEntries1.Select(ve => ve.VoucherNo).Distinct();


                var qryListOfAccountlists1 = _context.Qry201VoucherEntryScreenDisplays
                    .Where(p => voucherNos1.Contains(p.VoucherNo))
                    .OrderBy(i => i.DrCr == "Dr" ? 1 : 0) // Ensures "Dr" entries come first
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

                var resultList1 = await qryListOfAccountlists1.ToListAsync();
                int crCount1 = resultList1.Count(i => i.DrCr == "Dr");
                if (crCount1 >= 2)
                {
                    matchingEntries = resultList1.Where(x => x.AccountHead == PaymentAccoutHeadName).ToList();
                    IsMatchingEntry = true;
                }

                bool isVoucherExists = _context.Tbl201VoucherMasters
               .Any(v => v.VoucherNo == voucherEntries1[0].VoucherNo);
                var record = await _context.Tbl201VoucherEntries.FirstOrDefaultAsync(v => v.VoucherEntryNo == voucherEntryNo);
                var voucherNo = voucherEntries1[0].VoucherNo;
                var masterrecord = _context.Tbl201VoucherMasters.FirstOrDefault(v => v.VoucherNo == voucherNo);
                var SubLedgerRecord = await _context.Tbl201SubLedgerMasters.FirstOrDefaultAsync(v => v.VoucherEntryNo == voucherEntryNo);
                var PropertyAllocationRecord = await _context.Tbl20122PropertyAllocationMasters.FirstOrDefaultAsync(v => v.VoucherEntryId == voucherEntryNo);
                var CostAllocationRecord = await _context.Tbl201CostAllocationMasters.FirstOrDefaultAsync(v => v.VoucherEntryId == voucherEntryNo);
                var SalaryPayableRecord = await _context.Tbl20114SalaryPayableMasters.FirstOrDefaultAsync(v => v.VoucherEntryNo == voucherEntryNo);
                var EmpAllocationRecord = await _context.Tbl20104EmployeeAllocationMasters.FirstOrDefaultAsync(v => v.VoucherEntryId == voucherEntryNo);

                if (!isVoucherExists)
                {
                    voucherMaster.VoucherNo = voucherEntries1[0].VoucherNo;
                    voucherMaster.VoucherDate = DateTime.Now;
                    _context.Tbl201VoucherMasters.Remove(masterrecord);


                }

                // Add entries to the database
                //_context.Tbl201VoucherEntries.AddRange(voucherEntries);
                //await _context.SaveChangesAsync();


                if (record == null)
                {
                    return NotFound(new { message = "Record not found!" });
                }

                if (record.VoucherAmount != 0)
                {
                    if (record != null)
                    {
                        _context.Tbl201VoucherEntries.Remove(record);
                    }
                    else if (SubLedgerRecord != null)
                    {
                        _context.Tbl201SubLedgerMasters.Remove(SubLedgerRecord);

                    }
                    else if (PropertyAllocationRecord != null)
                    {
                        _context.Tbl20122PropertyAllocationMasters.Remove(PropertyAllocationRecord);
                    }
                    else if (SubLedgerRecord != null)
                    {
                        _context.Tbl201CostAllocationMasters.Remove(CostAllocationRecord);
                    }
                    else if (SubLedgerRecord != null)
                    {
                        _context.Tbl20104EmployeeAllocationMasters.Remove(EmpAllocationRecord);
                    }

                    //  _context.Tbl201VoucherMasters.Remove(masterrecord);
                    await _context.SaveChangesAsync();


                }


                //SaveVoucher(voucherEntries);

                // Get the list of updated vouchers
                var voucherEntries = _context.Tbl201VoucherEntries
                                              .Where(ve => ve.VoucherNo == VoucherNo) // Filter by the provided VoucherNo
                                              .ToList();


                var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
                var qryListOfAccountlists = _context.Qry201VoucherEntryScreenDisplays
                    .Where(p => voucherNos.Contains(p.VoucherNo))
                    .OrderBy(i => i.DrCr == "Dr" ? 1 : 0) // Ensures "Dr" entries come first
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

                int crCount = resultList.Count(i => i.DrCr == "Dr");

                //var lastCrEntry = resultList.LastOrDefault(i => i.DrCr == "Cr");

                var newCrEntry = resultList
    .Where(i => i.DrCr == "Dr")
    .OrderByDescending(i => i.VoucherEntryNo) // Assuming this is sequential
    .FirstOrDefault();



                var newlist = newCrEntry != null ? new List<VoucherEntryDisplayDTO> { newCrEntry } : new List<VoucherEntryDisplayDTO>();

                var pettycashid = "";
                foreach (var petty in newlist)
                {
                    pettycashid = petty.AccountHead;
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

                            debitamt = (int)(debitamt + entry.CrAmount);

                            // If Dr/Cr is Credit ("Cr"), perform specific logic
                            if (entry.DrCr == "Dr" && crCount == 1)
                            // if (crCount==1)
                            {
                                // Check if an existing entry matches
                                var existingEntry = _context.Tbl201VoucherEntries
                                                            .FirstOrDefault(v => v.AccountHead == entry.AccountHead
                                                                              && v.DrCr == "Cr"
                                                                              && v.VoucherNo == entry.VoucherNo);

                                decimal? SubAmt = existingEntry.VoucherAmount - debitamt;

                                if (existingEntry != null)
                                {
                                    // Update CrAmount by adding the calculated debit amount
                                    entry.DrAmount = debitamt;
                                    entry.SysRemarks = Remarks;

                                    // Optionally update the existing entry in the database
                                    existingEntry.VoucherAmount = entry.DrAmount;
                                    existingEntry.SysRemarks = entry.SysRemarks;
                                    _context.Tbl201VoucherEntries.Update(existingEntry);
                                    _context.SaveChanges();

                                }
                                else
                                {
                                    // If no existing entry, assign CrAmount as debitamt
                                    entry.DrAmount = debitamt;
                                }
                            }
                            else if (newCrEntry != null && crCount >= 2 && entry.AccountHead == pettycashid)
                            {
                                if (entry.DrCr == "Dr")
                                {

                                    if (IsMatchingEntry == true)
                                    {
                                        // Accumulate `aEntryAmount` correctly
                                        foreach (var mEntry in matchingEntries)
                                        {
                                            aEntryAmount += (int)mEntry.DrAmount; // Accumulate CrAmount correctly

                                        }

                                        // Process voucherEntries
                                        foreach (var mVoucherEntry in voucherEntries)
                                        {
                                            int eAmount = (int)mVoucherEntry.VoucherAmount;
                                            if (aEntryAmount == 0)
                                            {
                                                Amt = Amt - eAmount;
                                            }
                                            else
                                            {
                                                Amt = aEntryAmount - eAmount;
                                            }

                                        }

                                        // Check if an existing entry matches
                                        var existingEntry = _context.Tbl201VoucherEntries
                                                                    .FirstOrDefault(v => v.AccountHead == entry.AccountHead
                                                                                      && v.DrCr == "Dr"
                                                                                      && v.VoucherNo == entry.VoucherNo);

                                        if (existingEntry != null)
                                        {
                                            // Update CrAmount by adding the calculated debit amount
                                            entry.DrAmount = Amt;
                                            entry.SysRemarks = Remarks;
                                            // Optionally update the existing entry in the database
                                            existingEntry.VoucherAmount = entry.DrAmount;
                                            existingEntry.SysRemarks = entry.SysRemarks;
                                            _context.Tbl201VoucherEntries.Update(existingEntry);
                                            _context.SaveChanges();

                                        }
                                        else
                                        {
                                            // If no existing entry, assign CrAmount as debitamt
                                            // entry.CrAmount = debitamt;
                                        }
                                    }

                                }

                            }

                        }
                        entry.AccountHead = accountHead;
                        if (Remarks == "")
                        {
                            Remarks = entry.AccountHead;
                        }
                        else
                        {
                            Remarks = Remarks + "," + entry.AccountHead;
                        }

                    }

                }

                return Json(DataSourceLoader.Load(resultList.AsQueryable(), loadOptions));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }
        }
    }
}
