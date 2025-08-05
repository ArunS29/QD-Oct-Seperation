using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using QD.ERP.Web.Areas.Finance.Controllers;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.Areas.Finance.Views;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using QD.ERP.Web.Services.Logging;
using SkiaSharp;
//using QD.ERP.Web.DAL.Entities;
using QDERPWeb.Models;

namespace Form.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [Area("Finance")]
    [ApiController]
    public class VoucherEntryReceiptsController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<VoucherEntryReceiptsController> _logger;
        private readonly IUserActionLogger _userActionLogger;
        private readonly FcmService _fcmService;
        public VoucherEntryReceiptsController(ILogger<VoucherEntryReceiptsController> logger, TenantDbContextHelper tenantDbContextHelper, IUserActionLogger userActionLogger, FcmService fcmService)
        {
            _userActionLogger = userActionLogger;
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
            _fcmService = fcmService;
        }
        [HttpGet]
        public async Task<IActionResult> GetReceivingAccount(DataSourceLoadOptions loadOptions)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }
            try
            {
                var tbl20101salespersonmasters = dbContext.Qry201ListOfAccounts.Where(p => p.AccountGroupId == "A013").Select(i => new
                {

                    i.AccountId,
                    i.AccountHead,
                    i.AccountGroup,
                    i.AccountHeadArabic,
                    i.ReferenceNo,
                    i.AccountGroupId,
                    i.IsLedgerObselete,
                    i.MasterGroupId,
                    i.MasterGroup,
                    i.IsRestricted,
                    i.IsUseInSales,
                    i.IsUsedInPurchase,
                    i.IsProfitLossAccount,
                    i.IsBalanceSheetAccount,
                    i.IsMaintainBillByBill,
                    i.IsUseInReconciliation,
                    i.IsSalaryPayable
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
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }
            try
            {
                var qryListOfAccountlists = dbContext.Qry201ListOfAccounts
               .Where(i => i.AccountId != SelectedPaymentAccount) // Exclude the Receiving Account value
               .Select(i => new
               {
                   i.AccountId,
                   i.AccountHead,
                   i.AccountGroup,
                   i.AccountHeadArabic,
                   i.ReferenceNo,
                   i.AccountGroupId,
                   i.IsLedgerObselete,
                   i.MasterGroupId,
                   i.MasterGroup,
                   i.IsRestricted,
                   i.IsUseInSales,
                   i.IsUsedInPurchase,
                   i.IsProfitLossAccount,
                   i.IsBalanceSheetAccount,
                   i.IsMaintainBillByBill,
                   i.IsUseInReconciliation,
                   i.IsSalaryPayable
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
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }
            try
            {
                var qryListOfAccountlists = dbContext.Qry201ListOfAccounts

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
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }
            var qryListOfAccountlists = dbContext.Qry201VoucherEntryScreenDisplays.Where(p => p.VoucherNo == voucherNo).Select(i => new
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
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }
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
                var qryListOfAccountlists1 = dbContext.Qry201VoucherEntryScreenDisplays
                    .Where(p => voucherNos1.Contains(p.VoucherNo))
                    .OrderBy(i => i.DrCr == "Cr" ? 0 : 1) // Ensures "Dr" entries come first
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

                bool isVoucherExists = dbContext.Tbl201VoucherMasters
               .Any(v => v.VoucherNo == voucherEntries[0].VoucherNo);

                if (!isVoucherExists)
                {
                    voucherMaster.VoucherNo = voucherEntries[0].VoucherNo;
                    voucherMaster.VoucherDate = DateTime.Now;
                    dbContext.Tbl201VoucherMasters.AddRange(voucherMaster);
                }

                // Add entries to the database
                dbContext.Tbl201VoucherEntries.AddRange(voucherEntries);
                await dbContext.SaveChangesAsync();
                await _userActionLogger.LogAsync(
              module: "Finance > Cash Receipts",
              actionDetail: $"Saved voucher: {voucherEntries[0].VoucherNo}, Entries: {voucherEntries.Count}, AccountHead: {AccountHead}",
              documentNo: voucherEntries[0].VoucherNo
          );

                //SaveVoucher(voucherEntries);


                var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
                var qryListOfAccountlists = dbContext.Qry201VoucherEntryScreenDisplays
                    .Where(p => voucherNos.Contains(p.VoucherNo))
                    .OrderBy(i => i.DrCr == "Cr" ? 0 : 1) // Ensures "Dr" entries come first
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

                        var accountHead = dbContext.Qry201ListOfAccounts
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
                                var existingEntry = dbContext.Tbl201VoucherEntries
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
                                    dbContext.Tbl201VoucherEntries.Update(existingEntry);
                                    dbContext.SaveChanges();
                                    await _userActionLogger.LogAsync(
                                          module: "Finance > Cash Receipts",
                                          actionDetail: $"Updated voucher: {voucherEntries[0].VoucherNo}, Entries: {voucherEntries.Count}, AccountHead: {AccountHead}",
                                          documentNo: voucherEntries[0].VoucherNo
                                     );

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
                                        var existingEntry = dbContext.Tbl201VoucherEntries
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
                                            dbContext.Tbl201VoucherEntries.Update(existingEntry);
                                            dbContext.SaveChanges();
                                            await _userActionLogger.LogAsync(
                                                  module: "Finance > Cash Receipts",
                                                  actionDetail: $"Updated voucher: {voucherEntries[0].VoucherNo}, Entries: {voucherEntries.Count}, AccountHead: {AccountHead}",
                                                  documentNo: voucherEntries[0].VoucherNo
                                              );

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
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }


            string defaultCompanyString = HttpContext.Session.GetString("DefaultcompanyID") ?? "";
            byte defaultCompanyByte = 0; // or any default value you want

            if (!string.IsNullOrEmpty(defaultCompanyString))
            {
                // Safest way (avoids exceptions):
                byte.TryParse(defaultCompanyString, out defaultCompanyByte);
                // Now defaultCompanyByte holds the parsed value, or 0 if parsing failed.
            }

            // Now use defaultCompanyByte as needed



            byte companyId = defaultCompanyByte;

            // Step 2: Get NoOfDigitsInVouchers
            var companyConfig = await dbContext.Tbl901CompanyDetails02s
                .Where(c => c.CompanyId == companyId)
                .Select(c => new { c.NoOfDigitsInVouchers })
                .FirstOrDefaultAsync();

            byte configuredDigitCount = companyConfig?.NoOfDigitsInVouchers ?? 3; // Default to 3 if not found

            // Step 3: Prepare voucher prefix
            DateTime currentDate = DateTime.Now;
            string yearPart = currentDate.Year.ToString().Substring(2); // "25"
            string monthPart = currentDate.Month.ToString("00"); // "06"
            string voucherPrefix = $"BR-{yearPart}-{monthPart}-";
            string likePattern = voucherPrefix + "%";

            int digitCountToUse = configuredDigitCount; // this might change if series already exists
            string strNewReceiptNo;
            try
            {
                // Step 4: Check if any vouchers already exist for current month
                var existingVoucher = await dbContext.Tbl201VoucherEntries
                    .Where(v => v.VoucherNo.StartsWith(voucherPrefix))
                    .OrderByDescending(v => v.VoucherNo)
                    .Select(v => v.VoucherNo)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(existingVoucher))
                {
                    // Step 5: Existing series found → infer digit count from length of number part
                    string numberPart = existingVoucher.Substring(voucherPrefix.Length);
                    digitCountToUse = numberPart.Length;
                }

                // Step 6: Fetch max number using resolved digit count
                var result = await dbContext.VoucherResults
                    .FromSqlInterpolated($@"
    SELECT MAX(CAST(RIGHT(VoucherNo, {digitCountToUse}) AS INT)) AS MaxVoucherNo
    FROM Tbl201VoucherMaster
    WHERE VoucherNo LIKE {likePattern}")
                    .ToListAsync();

                int maxVoucherNo = result.FirstOrDefault()?.MaxVoucherNo ?? 0;
                int newVoucherNo = maxVoucherNo + 1;

                string paddedNo = newVoucherNo.ToString().PadLeft(digitCountToUse, '0');
                strNewReceiptNo = voucherPrefix + paddedNo;
            }
            catch (Exception)
            {
                // fallback if any failure
                string fallback = "1".PadLeft(configuredDigitCount, '0');
                strNewReceiptNo = voucherPrefix + fallback;
            }

            return Json(strNewReceiptNo);

        }
        [HttpPost]
        public async Task<ActionResult> SaveVoucher([FromBody] Tbl201VoucherMaster VM)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }
            if (VM == null)
            {
                return BadRequest(new { success = false, message = "Invalid data received." });
            }

            try
            {
                dbContext.Tbl201VoucherMasters.Add(VM);
                await dbContext.SaveChangesAsync();

                var UserId = HttpContext.Session.GetString("UserId");
                var TenantName = HttpContext.Session.GetString("TenantName");

                var notifyRequest = new NotificationRequest
                {
                    UserId = UserId, // or fetch from session/DB
                    VoucherName = VM.VoucherNo,
                    ActionType = "You have one Receipt Voucher to verify",
                    TenantName = TenantName
                };

                await _fcmService.SendNotificationAsync(notifyRequest);

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
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }
            try
            {
                var allocation = await dbContext.Tbl201ChartOfAccounts
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
        public IActionResult BillsReceivable(string voucherNo, string accountHead, string voucherAmount, string drCr, long voucherEntryNo, string accountId)
        {
            // Log or debug the incoming parameters
            ViewBag.VoucherNo = voucherNo;
            ViewBag.AccountHead = accountHead;
            ViewBag.VoucherAmount = voucherAmount;
            ViewBag.DrCr = drCr;
            ViewBag.VoucherEntryNo = voucherEntryNo;
            ViewBag.AccountID = accountId;

            return PartialView("~/Areas/Finance/Views/_BillsReceivables.cshtml"); // Ensure this is inside /Views/VoucherEntryReceipts/
        }
        [HttpGet]
        public async Task<IActionResult> CheckIsMaintainBillByBill(string AccountHead, string AccountID)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }
            try
            {
                var allocation = await dbContext.Qry201ListOfAccounts
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
        public async Task<IActionResult> CheckCostAllocation(string AccountHead, string AccountID)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }
            try
            {
                var allocation = await dbContext.Qry20111ListOfPandLitems
                    .Where(x => x.AccountHead == AccountHead && x.AccountId == AccountID && x.IsProfitLossAccount == true)
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
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }
            try
            {
                var allocation = await dbContext.Tbl201ChartOfAccounts
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
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }
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
                var qryListOfAccountlists1 = dbContext.Qry201VoucherEntryScreenDisplays
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

                bool isVoucherExists = dbContext.Tbl201VoucherMasters
               .Any(v => v.VoucherNo == voucherEntries[0].VoucherNo);

                if (!isVoucherExists)
                {
                    voucherMaster.VoucherNo = voucherEntries[0].VoucherNo;
                    voucherMaster.VoucherDate = DateTime.Now;
                    dbContext.Tbl201VoucherMasters.AddRange(voucherMaster);
                }

                // Add entries to the database
                dbContext.Tbl201VoucherEntries.AddRange(voucherEntries);
                await dbContext.SaveChangesAsync();

                //SaveVoucher(voucherEntries);


                var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
                var qryListOfAccountlists = dbContext.Qry201VoucherEntryScreenDisplays
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


                            var existingEntry = dbContext.Tbl201VoucherEntries
                                      .FirstOrDefault(v => v.AccountHead == entry.AccountHead
                                                        && v.DrCr == "Dr"
                                                        && v.VoucherNo == entry.VoucherNo);

                            // Optionally update the existing entry in the database
                            existingEntry.VoucherAmount = CrMinusAmt;
                            entry.DrAmount = -CrMinusAmt;
                            dbContext.Tbl201VoucherEntries.Update(existingEntry);
                            dbContext.SaveChanges();

                        }
                    }


                    if (!string.IsNullOrEmpty(entry.AccountHead))
                    {

                        var accountHead = dbContext.Qry201ListOfAccounts
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
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
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
                //  var voucherNos1 = voucherEntries.Select(ve => ve.VoucherNo).Distinct();


                // Get the list of updated vouchers
                var voucherEntries1 = dbContext.Tbl201VoucherEntries
                                              .Where(ve => ve.VoucherNo == VoucherNo) // Filter by the provided VoucherNo
                                              .ToList();

                var voucherNos1 = voucherEntries1.Select(ve => ve.VoucherNo).Distinct();


                var qryListOfAccountlists1 = dbContext.Qry201VoucherEntryScreenDisplays
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

                bool isVoucherExists = dbContext.Tbl201VoucherMasters
               .Any(v => v.VoucherNo == voucherEntries1[0].VoucherNo);
                var record = await dbContext.Tbl201VoucherEntries.FirstOrDefaultAsync(v => v.VoucherEntryNo == voucherEntryNo);
                var voucherNo = voucherEntries1[0].VoucherNo;
                var masterrecord = dbContext.Tbl201VoucherMasters.FirstOrDefault(v => v.VoucherNo == voucherNo);
                var SubLedgerRecord = await dbContext.Tbl201SubLedgerMasters.FirstOrDefaultAsync(v => v.VoucherEntryNo == voucherEntryNo);
                var PropertyAllocationRecord = await dbContext.Tbl20122PropertyAllocationMasters.FirstOrDefaultAsync(v => v.VoucherEntryId == voucherEntryNo);
                var CostAllocationRecord = await dbContext.Tbl201CostAllocationMasters.FirstOrDefaultAsync(v => v.VoucherEntryId == voucherEntryNo);
                var SalaryPayableRecord = await dbContext.Tbl20114SalaryPayableMasters.FirstOrDefaultAsync(v => v.VoucherEntryNo == voucherEntryNo);
                var EmpAllocationRecord = await dbContext.Tbl20104EmployeeAllocationMasters.FirstOrDefaultAsync(v => v.VoucherEntryId == voucherEntryNo);

                if (!isVoucherExists)
                {
                    voucherMaster.VoucherNo = voucherEntries1[0].VoucherNo;
                    voucherMaster.VoucherDate = DateTime.Now;
                    dbContext.Tbl201VoucherMasters.Remove(masterrecord);
                    // Log voucher master deletion
                    await _userActionLogger.LogAsync(
                        module: "Finance > Voucher Deletion",
                        actionDetail: $"Deleted voucher master record with VoucherNo: {VoucherNo}",
                        documentNo: VoucherNo
                    );

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
                        dbContext.Tbl201VoucherEntries.Remove(record);

                        await _userActionLogger.LogAsync(
                         module: "Finance > Voucher Deletion",
                         actionDetail: $"Deleted voucher entry with VoucherEntryNo: {voucherEntryNo}, VoucherNo: {VoucherNo}",
                         documentNo: VoucherNo
                        );
                    }
                    else if (SubLedgerRecord != null)
                    {
                        dbContext.Tbl201SubLedgerMasters.Remove(SubLedgerRecord);

                    }
                    else if (PropertyAllocationRecord != null)
                    {
                        dbContext.Tbl20122PropertyAllocationMasters.Remove(PropertyAllocationRecord);
                    }
                    else if (SubLedgerRecord != null)
                    {
                        dbContext.Tbl201CostAllocationMasters.Remove(CostAllocationRecord);
                    }
                    else if (SubLedgerRecord != null)
                    {
                        dbContext.Tbl20104EmployeeAllocationMasters.Remove(EmpAllocationRecord);
                    }

                    //  _context.Tbl201VoucherMasters.Remove(masterrecord);
                    await dbContext.SaveChangesAsync();
                    await _userActionLogger.LogAsync(
                       module: "Finance > Voucher Deletion",
                       actionDetail: $"Deleted voucher master record with VoucherNo: {VoucherNo}",
                       documentNo: VoucherNo
                   );


                }


                //SaveVoucher(voucherEntries);

                // Get the list of updated vouchers
                var voucherEntries = dbContext.Tbl201VoucherEntries
                                              .Where(ve => ve.VoucherNo == VoucherNo) // Filter by the provided VoucherNo
                                              .ToList();


                var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
                var qryListOfAccountlists = dbContext.Qry201VoucherEntryScreenDisplays
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

                        var accountHead = dbContext.Qry201ListOfAccounts
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
                                var existingEntry = dbContext.Tbl201VoucherEntries
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
                                    dbContext.Tbl201VoucherEntries.Update(existingEntry);
                                    dbContext.SaveChanges();

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
                                        var existingEntry = dbContext.Tbl201VoucherEntries
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
                                            dbContext.Tbl201VoucherEntries.Update(existingEntry);
                                            dbContext.SaveChanges();
                                            await _userActionLogger.LogAsync(
                                                   module: "Finance > Voucher Deletion",
                                                   actionDetail: $"Deleted voucher master record with VoucherNo: {VoucherNo}",
                                                   documentNo: VoucherNo
                                            );

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

        //[HttpPost]
        //public async Task<ActionResult> AddCPCrVoucherEntry(DataSourceLoadOptions loadOptions, [FromBody] List<Tbl201VoucherEntry> voucherEntries, string AccountHead, string PaymentAccoutHeadName, int Gridcount)
        //{
        //    if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
        //    {
        //        if (voucherEntries == null || !voucherEntries.Any())
        //        {
        //            return BadRequest(new { success = false, message = "Invalid data received." });
        //        }

        //        try
        //        {


        //            Tbl201VoucherMaster voucherMaster = new();
        //            int aEntryAmount = 0;
        //            int Amt = 0, Crmt = 0, CrMinusAmt = 0, NewCrMinusAmt = 0, ExistingCrMinusAmt = 0;
        //            bool IsMatchingEntry = false;

        //            //var matchingEntries;
        //            List<VoucherEntryDisplayDTO> matchingEntries = new();
        //            var voucherNos1 = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
        //            var qryListOfAccountlists1 = dbContext.Qry201VoucherEntryScreenDisplays
        //                .Where(p => voucherNos1.Contains(p.VoucherNo))
        //                .OrderBy(i => i.DrCr == "Dr" ? 1 : 0) // Ensures "Dr" entries come first
        //                .Select(i => new VoucherEntryDisplayDTO
        //                {
        //                    VoucherNo = i.VoucherNo,
        //                    VoucherEntryNo = i.VoucherEntryNo,
        //                    DrCr = i.DrCr,
        //                    DrAmount = i.DrAmount,
        //                    CrAmount = i.CrAmount,
        //                    VoucherAmountFormatted = i.VoucherAmountFormatted,
        //                    EntryNarration = i.EntryNarration,
        //                    AccountHead = i.AccountHead,
        //                    SysRemarks = i.SysRemarks
        //                });

        //            var resultList1 = await qryListOfAccountlists1.ToListAsync();
        //           // int crCount1 = resultList1.Count(i => i.DrCr == "Cr");
        //           ///// bool hasDrEntries = resultList.Any(i => i.DrCr == "Dr");
        //           // bool hasCrEntries = voucherEntries.Any(i => i.DrCr == "Cr");
        //           // int EntriesCrCount1 = voucherEntries.Count(i => i.DrCr == "Cr");
        //           // if (crCount1 >= 2)
        //           // {
        //           //     matchingEntries = resultList1.Where(x => x.AccountHead == PaymentAccoutHeadName).ToList();
        //           //     if (matchingEntries.Count != 0)
        //           //     {
        //           //         ExistingCrMinusAmt = (int)matchingEntries[0].CrAmount;
        //           //         IsMatchingEntry = true;
        //           //     }
        //           // }

        //            bool isVoucherExists = dbContext.Tbl201VoucherMasters
        //           .Any(v => v.VoucherNo == voucherEntries[0].VoucherNo);

        //            if (!isVoucherExists)
        //            {
        //                voucherMaster.VoucherNo = voucherEntries[0].VoucherNo;
        //                voucherMaster.VoucherDate = DateTime.Now;
        //                dbContext.Tbl201VoucherMasters.AddRange(voucherMaster);
        //            }

        //            // Add entries to the database
        //            dbContext.Tbl201VoucherEntries.AddRange(voucherEntries);
        //            await dbContext.SaveChangesAsync();

        //            //SaveVoucher(voucherEntries);


        //            var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
        //            var qryListOfAccountlists = dbContext.Qry201VoucherEntryScreenDisplays
        //                .Where(p => voucherNos.Contains(p.VoucherNo))
        //                .OrderBy(i => i.DrCr == "Dr" ? 1 : 0) // Ensures "Dr" entries come first
        //                .Select(i => new VoucherEntryDisplayDTO
        //                {
        //                    VoucherNo = i.VoucherNo,
        //                    VoucherEntryNo = i.VoucherEntryNo,
        //                    DrCr = i.DrCr,
        //                    DrAmount = i.DrAmount,
        //                    CrAmount = i.CrAmount,
        //                    VoucherAmountFormatted = i.VoucherAmountFormatted,
        //                    EntryNarration = i.EntryNarration,
        //                    AccountHead = i.AccountHead,
        //                    SysRemarks = i.SysRemarks,
        //                    Type = i.Type

        //                });


        //            var resultList = await qryListOfAccountlists.ToListAsync();
        //            int crCount1 = resultList.Count(i => i.DrCr == "Cr");
        //            bool hasDrEntries = resultList.Any(i => i.DrCr == "Dr");
        //            bool hasCrEntries = voucherEntries.Any(i => i.DrCr == "Cr");
        //            int EntriesCrCount1 = voucherEntries.Count(i => i.DrCr == "Cr");
        //            if (crCount1 >= 2)
        //            {
        //                matchingEntries = resultList.Where(x => x.AccountHead == PaymentAccoutHeadName).ToList();
        //                if (matchingEntries.Count != 0)
        //                {
        //                    ExistingCrMinusAmt = (int)matchingEntries[0].CrAmount;
        //                    IsMatchingEntry = true;
        //                }
        //            }

        //            int debitamt = 0; // Initialize debit amount

        //            int crCount = resultList.Count(i => i.DrCr == "Dr");

        //            //var lastCrEntry = resultList.LastOrDefault(i => i.DrCr == "Cr");

        //            var newCrEntry = resultList
        //.Where(i => i.DrCr == "Cr")
        //.OrderByDescending(i => i.VoucherEntryNo) // Assuming this is sequential
        //.FirstOrDefault();



        //            var newlist = newCrEntry != null ? new List<VoucherEntryDisplayDTO> { newCrEntry } : new List<VoucherEntryDisplayDTO>();

        //            var NewId = "";
        //            foreach (var petty in newlist)
        //            {
        //                NewId = petty.AccountHead;
        //                NewCrMinusAmt = (int)petty.CrAmount;
        //            }

        //            foreach (var entry in resultList)
        //            {
        //                if (!entry.SysRemarks.Contains("Paid thru:"))
        //                {
        //                    // if(IsMatchingEntry==false && crCount1==2)//Cr
        //                    if (hasCrEntries == true && IsMatchingEntry == true && NewId == entry.AccountHead)
        //                    {
        //                        var existingEntry = dbContext.Tbl201VoucherEntries
        //                                  .FirstOrDefault(v => v.AccountHead == entry.AccountHead
        //                                                    && v.DrCr == "Cr"
        //                                                    && v.VoucherNo == entry.VoucherNo);
        //                        if (EntriesCrCount1 == 2)
        //                        {
        //                            CrMinusAmt = (int)entry.VoucherAmountFormatted;
        //                            entry.CrAmount = CrMinusAmt;
        //                            existingEntry.VoucherAmount = CrMinusAmt;
        //                        }
        //                        else
        //                        {
        //                            //entry.CrAmount = -CrMinusAmt;
        //                            CrMinusAmt = NewCrMinusAmt - ExistingCrMinusAmt;
        //                            entry.CrAmount = -CrMinusAmt;
        //                            existingEntry.VoucherAmount = -CrMinusAmt;
        //                        }


        //                        // Optionally update the existing entry in the database


        //                        // entry.CrAmount = CrMinusAmt;

        //                        dbContext.Tbl201VoucherEntries.Update(existingEntry);
        //                        dbContext.SaveChanges();

        //                    }
        //                    else if (IsMatchingEntry == true && hasCrEntries != true)//Dr - Cr
        //                    {
        //                        //CrMinusAmt = (int)entry.VoucherAmountFormatted + NewCrMinusAmt;
        //                        CrMinusAmt = NewCrMinusAmt - ExistingCrMinusAmt;
        //                        entry.CrAmount = -CrMinusAmt;


        //                        var existingEntry = dbContext.Tbl201VoucherEntries
        //                                 .FirstOrDefault(v => v.AccountHead == entry.AccountHead
        //                                                   && v.DrCr == "Cr"
        //                                                   && v.VoucherNo == entry.VoucherNo);

        //                        // Optionally update the existing entry in the database
        //                        existingEntry.VoucherAmount = -CrMinusAmt;


        //                        // entry.CrAmount = CrMinusAmt;

        //                        dbContext.Tbl201VoucherEntries.Update(existingEntry);
        //                        dbContext.SaveChanges();
        //                    }

        //                    // CrMinusAmt = (int)entry.VoucherAmountFormatted;


        //                }


        //                if (!string.IsNullOrEmpty(entry.AccountHead))
        //                {

        //                    var accountHead = dbContext.Qry201ListOfAccounts
        //                                              .Where(a => a.AccountId == entry.AccountHead)
        //                                              .Select(a => a.AccountHead)
        //                                              .FirstOrDefault();

        //                    entry.AccountHead = accountHead;
        //                }

        //            }

        //            return Json(DataSourceLoader.Load(resultList.AsQueryable(), loadOptions));
        //        }
        //        catch (Exception ex)
        //        {
        //            return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
        //        }
        //    }
        //    return Unauthorized(new { message = "Invalid tenant.", success = false });
        //}

        [HttpPost]
        public async Task<ActionResult> AddCPCrVoucherEntry(DataSourceLoadOptions loadOptions, [FromBody] List<Tbl201VoucherEntry> voucherEntries, string AccountHead, string PaymentAccoutHeadName, int Gridcount)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
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
                    var ExistingAccHeadID = "";

                    //var matchingEntries;
                    List<VoucherEntryDisplayDTO> matchingEntries = new();
                    var voucherNos1 = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
                    var qryListOfAccountlists1 = dbContext.Qry201VoucherEntryScreenDisplays
                        .Where(p => voucherNos1.Contains(p.VoucherNo))
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

                    var resultList1 = await qryListOfAccountlists1.ToListAsync();


                    bool isVoucherExists = dbContext.Tbl201VoucherMasters
                   .Any(v => v.VoucherNo == voucherEntries[0].VoucherNo);

                    if (!isVoucherExists)
                    {
                        voucherMaster.VoucherNo = voucherEntries[0].VoucherNo;
                        voucherMaster.VoucherDate = DateTime.Now;
                        dbContext.Tbl201VoucherMasters.AddRange(voucherMaster);
                    }

                    // Add entries to the database
                    dbContext.Tbl201VoucherEntries.AddRange(voucherEntries);
                    await dbContext.SaveChangesAsync();

                    //SaveVoucher(voucherEntries);


                    var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
                    var qryListOfAccountlists = dbContext.Qry201VoucherEntryScreenDisplays
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
                            SysRemarks = i.SysRemarks,
                            Type = i.Type

                        });


                    var resultList = await qryListOfAccountlists.ToListAsync();
                    int crCount1 = resultList.Count(i => i.DrCr == "Cr");
                    bool hasDrEntries = resultList.Any(i => i.DrCr == "Dr");
                    bool hasCrEntries = voucherEntries.Any(i => i.DrCr == "Cr");
                    int EntriesCrCount1 = voucherEntries.Count(i => i.DrCr == "Cr");
                    if (crCount1 >= 2)
                    {
                        matchingEntries = resultList.Where(x => x.AccountHead == PaymentAccoutHeadName).ToList();
                        if (matchingEntries.Count != 0)
                        {
                            ExistingCrMinusAmt = (int)matchingEntries[0].CrAmount;
                            ExistingAccHeadID = matchingEntries[0].AccountHead;
                            IsMatchingEntry = true;
                        }
                    }

                    int debitamt = 0; // Initialize debit amount

                    int crCount = resultList.Count(i => i.DrCr == "Dr");

                    //var lastCrEntry = resultList.LastOrDefault(i => i.DrCr == "Cr");

                    var newCrEntry = resultList
        .Where(i => i.DrCr == "Cr")
        .OrderByDescending(i => i.VoucherEntryNo) // Assuming this is sequential
        .FirstOrDefault();



                    var newlist = newCrEntry != null ? new List<VoucherEntryDisplayDTO> { newCrEntry } : new List<VoucherEntryDisplayDTO>();

                    var NewId = "";
                    foreach (var petty in newlist)
                    {
                        NewId = petty.AccountHead;
                        NewCrMinusAmt = (int)petty.CrAmount;
                    }

                    foreach (var entry in resultList)
                    {
                        var accheadid = entry.AccountHead;
                        // if (!entry.SysRemarks.Contains("Paid thru:"))
                        if (entry.Type == "PaymentAccount" && ExistingAccHeadID == accheadid)
                        {

                            if (hasCrEntries == true && IsMatchingEntry == true) //&& NewId == accheadid)
                            {
                                var existingEntry = dbContext.Tbl201VoucherEntries
                                          .FirstOrDefault(v => v.AccountHead == entry.AccountHead
                                                            && v.DrCr == "Cr"
                                                            && v.VoucherNo == entry.VoucherNo);
                                if (EntriesCrCount1 == 2)
                                {
                                    CrMinusAmt = (int)entry.VoucherAmountFormatted;
                                    entry.CrAmount = CrMinusAmt;
                                    existingEntry.VoucherAmount = CrMinusAmt;
                                }
                                else
                                {
                                    //entry.CrAmount = -CrMinusAmt;
                                    CrMinusAmt = NewCrMinusAmt - ExistingCrMinusAmt;
                                    entry.CrAmount = -CrMinusAmt;
                                    existingEntry.VoucherAmount = -CrMinusAmt;
                                }


                                // Optionally update the existing entry in the database


                                // entry.CrAmount = CrMinusAmt;

                                dbContext.Tbl201VoucherEntries.Update(existingEntry);
                                dbContext.SaveChanges();

                            }
                            else if (IsMatchingEntry == true && hasCrEntries != true)//Dr - Cr
                            {
                                //CrMinusAmt = (int)entry.VoucherAmountFormatted + NewCrMinusAmt;
                                CrMinusAmt = NewCrMinusAmt - ExistingCrMinusAmt;
                                entry.CrAmount = -CrMinusAmt;


                                var existingEntry = dbContext.Tbl201VoucherEntries
                                         .FirstOrDefault(v => v.AccountHead == entry.AccountHead
                                                           && v.DrCr == "Cr"
                                                           && v.VoucherNo == entry.VoucherNo);

                                // Optionally update the existing entry in the database
                                existingEntry.VoucherAmount = -CrMinusAmt;


                                // entry.CrAmount = CrMinusAmt;

                                dbContext.Tbl201VoucherEntries.Update(existingEntry);
                                dbContext.SaveChanges();
                            }

                            // CrMinusAmt = (int)entry.VoucherAmountFormatted;


                        }


                        if (!string.IsNullOrEmpty(entry.AccountHead))
                        {

                            var accountHead = dbContext.Qry201ListOfAccounts
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
            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }


        [HttpPost]
        public async Task<ActionResult> AddBRVoucherEntry(DataSourceLoadOptions loadOptions, [FromBody] List<Tbl201VoucherEntry> voucherEntries, string AccountHead, string PaymentAccoutHeadName, int Gridcount)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
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
                    var ExistingAccHeadID = "";

                    //var matchingEntries;
                    List<VoucherEntryDisplayDTO> matchingEntries = new();
                    var voucherNos1 = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
                    var qryListOfAccountlists1 = dbContext.Qry201VoucherEntryScreenDisplays
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


                    bool isVoucherExists = dbContext.Tbl201VoucherMasters
                   .Any(v => v.VoucherNo == voucherEntries[0].VoucherNo);

                    if (!isVoucherExists)
                    {
                        voucherMaster.VoucherNo = voucherEntries[0].VoucherNo;
                        voucherMaster.VoucherDate = DateTime.Now;
                        dbContext.Tbl201VoucherMasters.AddRange(voucherMaster);
                    }

                    // Add entries to the database
                    dbContext.Tbl201VoucherEntries.AddRange(voucherEntries);
                    await dbContext.SaveChangesAsync();

                    //SaveVoucher(voucherEntries);


                    var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
                    var qryListOfAccountlists = dbContext.Qry201VoucherEntryScreenDisplays
                        .Where(p => voucherNos.Contains(p.VoucherNo))
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
                            SysRemarks = i.SysRemarks,
                            Type = i.Type

                        });


                    var resultList = await qryListOfAccountlists.ToListAsync();
                    int DrCount1 = resultList.Count(i => i.DrCr == "Dr");
                    bool hasCrEntries = resultList.Any(i => i.DrCr == "Cr");
                    bool hasDrEntries = voucherEntries.Any(i => i.DrCr == "Dr");
                    int EntriesDrCount1 = voucherEntries.Count(i => i.DrCr == "Dr");
                    if (DrCount1 >= 2)
                    {
                        matchingEntries = resultList.Where(x => x.AccountHead == PaymentAccoutHeadName).ToList();
                        if (matchingEntries.Count != 0)
                        {
                            ExistingCrMinusAmt = (int)matchingEntries[0].DrAmount;
                            ExistingAccHeadID = matchingEntries[0].AccountHead;
                            IsMatchingEntry = true;
                        }
                    }

                    int debitamt = 0; // Initialize debit amount

                    int DrCount = resultList.Count(i => i.DrCr == "Cr");

                    //var lastCrEntry = resultList.LastOrDefault(i => i.DrCr == "Cr");

                    var newCrEntry = resultList
        .Where(i => i.DrCr == "Dr")
        .OrderByDescending(i => i.VoucherEntryNo) // Assuming this is sequential
        .FirstOrDefault();



                    var newlist = newCrEntry != null ? new List<VoucherEntryDisplayDTO> { newCrEntry } : new List<VoucherEntryDisplayDTO>();

                    var NewId = "";
                    foreach (var petty in newlist)
                    {
                        NewId = petty.AccountHead;
                        NewCrMinusAmt = (int)petty.DrAmount;
                    }

                    foreach (var entry in resultList)
                    {
                        var accheadid = entry.AccountHead;
                        // if (!entry.SysRemarks.Contains("Paid thru:"))
                        if (entry.Type == "PaymentAccount" && ExistingAccHeadID == accheadid)
                        {

                            if (hasCrEntries == true && IsMatchingEntry == true) //&& NewId == accheadid)
                            {
                                var existingEntry = dbContext.Tbl201VoucherEntries
                                          .FirstOrDefault(v => v.AccountHead == entry.AccountHead
                                                            && v.DrCr == "Dr"
                                                            && v.VoucherNo == entry.VoucherNo);
                                if (EntriesDrCount1 == 2)
                                {
                                    CrMinusAmt = (int)entry.VoucherAmountFormatted;
                                    entry.DrAmount = CrMinusAmt;
                                    existingEntry.VoucherAmount = CrMinusAmt;
                                }
                                else
                                {
                                    //entry.CrAmount = -CrMinusAmt;
                                    CrMinusAmt = NewCrMinusAmt - ExistingCrMinusAmt;
                                    entry.DrAmount = -CrMinusAmt;
                                    existingEntry.VoucherAmount = -CrMinusAmt;
                                }


                                // Optionally update the existing entry in the database


                                // entry.CrAmount = CrMinusAmt;

                                dbContext.Tbl201VoucherEntries.Update(existingEntry);
                                dbContext.SaveChanges();

                            }
                            else if (IsMatchingEntry == true && hasCrEntries != true)//Dr - Cr
                            {
                                //CrMinusAmt = (int)entry.VoucherAmountFormatted + NewCrMinusAmt;
                                CrMinusAmt = NewCrMinusAmt - ExistingCrMinusAmt;
                                entry.DrAmount = -CrMinusAmt;


                                var existingEntry = dbContext.Tbl201VoucherEntries
                                         .FirstOrDefault(v => v.AccountHead == entry.AccountHead
                                                           && v.DrCr == "Dr"
                                                           && v.VoucherNo == entry.VoucherNo);

                                // Optionally update the existing entry in the database
                                existingEntry.VoucherAmount = -CrMinusAmt;


                                // entry.CrAmount = CrMinusAmt;

                                dbContext.Tbl201VoucherEntries.Update(existingEntry);
                                dbContext.SaveChanges();
                            }

                            // CrMinusAmt = (int)entry.VoucherAmountFormatted;


                        }


                        if (!string.IsNullOrEmpty(entry.AccountHead))
                        {

                            var accountHead = dbContext.Qry201ListOfAccounts
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
            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public async Task<ActionResult> DeleteVoucherEntries(DataSourceLoadOptions loadOptions, long voucherEntryNo, string VoucherNo, string PaymentAccoutHeadName, int Gridcount)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {

                try
                {
                    Tbl201VoucherMaster voucherMaster = new();
                    int aEntryAmount = 0;
                    int Amt = 0; decimal? mCrAmt = 0;
                    var Remarks = "";
                    bool IsMatchingEntry = false;
                    var ExistingAccHeadID = "";
                    //var matchingEntries;
                    List<VoucherEntryDisplayDTO> matchingEntries = new();
                    //  var voucherNos1 = voucherEntries.Select(ve => ve.VoucherNo).Distinct();


                    // Get the list of updated vouchers
                    var voucherEntries1 = dbContext.Tbl201VoucherEntries
                                                  .Where(ve => ve.VoucherNo == VoucherNo) // Filter by the provided VoucherNo
                                                  .ToList();

                    var voucherNos1 = voucherEntries1.Select(ve => ve.VoucherNo).Distinct();
                    int ExistingCrAmt = 0;

                    var qryListOfAccountlists1 = dbContext.Qry201VoucherEntryScreenDisplays
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
                    int drCount1 = resultList1.Count(i => i.DrCr == "Cr");
                    if (crCount1 >= 2)
                    {
                        matchingEntries = resultList1.Where(x => x.AccountHead == PaymentAccoutHeadName).ToList();
                        if (matchingEntries.Count != 0)
                        {
                            ExistingAccHeadID = matchingEntries[0].AccountHead;
                            IsMatchingEntry = true;
                        }
                    }

                    bool isVoucherExists = dbContext.Tbl201VoucherMasters
                   .Any(v => v.VoucherNo == voucherEntries1[0].VoucherNo);
                    var record = await dbContext.Tbl201VoucherEntries.FirstOrDefaultAsync(v => v.VoucherEntryNo == voucherEntryNo);
                    var voucherNo = voucherEntries1[0].VoucherNo;
                    var masterrecord = dbContext.Tbl201VoucherMasters.FirstOrDefault(v => v.VoucherNo == voucherNo);
                    var SubLedgerRecord = await dbContext.Tbl201SubLedgerMasters.FirstOrDefaultAsync(v => v.VoucherEntryNo == voucherEntryNo);
                    var PropertyAllocationRecord = await dbContext.Tbl20122PropertyAllocationMasters.FirstOrDefaultAsync(v => v.VoucherEntryId == voucherEntryNo);
                    var CostAllocationRecord = await dbContext.Tbl201CostAllocationMasters.FirstOrDefaultAsync(v => v.VoucherEntryId == voucherEntryNo);
                    var SalaryPayableRecord = await dbContext.Tbl20114SalaryPayableMasters.FirstOrDefaultAsync(v => v.VoucherEntryNo == voucherEntryNo);
                    var EmpAllocationRecord = await dbContext.Tbl20104EmployeeAllocationMasters.FirstOrDefaultAsync(v => v.VoucherEntryId == voucherEntryNo);



                    if (!isVoucherExists)
                    {
                        voucherMaster.VoucherNo = voucherEntries1[0].VoucherNo;
                        voucherMaster.VoucherDate = DateTime.Now;
                        dbContext.Tbl201VoucherMasters.Remove(masterrecord);


                    }

                    // Add entries to the database
                    //dbContext.Tbl201VoucherEntries.AddRange(voucherEntries);
                    //await dbContext.SaveChangesAsync();

                    if (record == null)
                    {
                        return NotFound(new { message = "Record not found!" });
                    }
                    decimal? rAmt = 0;
                    bool IsRemoved = false;
                    if (record.VoucherAmount != 0)
                    {
                        if (record != null)
                        {
                            rAmt = record.VoucherAmount;
                            if (record.AccountHead != ExistingAccHeadID)
                            {
                                dbContext.Tbl201VoucherEntries.Remove(record);
                                IsRemoved = true;
                            }
                        }
                        else if (SubLedgerRecord != null)
                        {
                            dbContext.Tbl201SubLedgerMasters.Remove(SubLedgerRecord);

                        }
                        else if (PropertyAllocationRecord != null)
                        {
                            dbContext.Tbl20122PropertyAllocationMasters.Remove(PropertyAllocationRecord);
                        }
                        else if (SubLedgerRecord != null)
                        {
                            dbContext.Tbl201CostAllocationMasters.Remove(CostAllocationRecord);
                        }
                        else if (SubLedgerRecord != null)
                        {
                            dbContext.Tbl20104EmployeeAllocationMasters.Remove(EmpAllocationRecord);
                        }

                        //  dbContext.Tbl201VoucherMasters.Remove(masterrecord);
                        await dbContext.SaveChangesAsync();

                    }

                    //SaveVoucher(voucherEntries);

                    // Get the list of updated vouchers
                    var voucherEntries = dbContext.Tbl201VoucherEntries
                                                  .Where(ve => ve.VoucherNo == VoucherNo) // Filter by the provided VoucherNo
                                                  .ToList();


                    var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
                    var qryListOfAccountlists = dbContext.Qry201VoucherEntryScreenDisplays
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
                    int drCount = resultList.Count(i => i.DrCr == "Cr");

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

                            var accountHead = dbContext.Qry201ListOfAccounts
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
                                    var existingEntry = dbContext.Tbl201VoucherEntries
                                                                .FirstOrDefault(v => v.AccountHead == entry.AccountHead
                                                                                  && v.DrCr == "Dr"
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
                                        dbContext.Tbl201VoucherEntries.Update(existingEntry);
                                        dbContext.SaveChanges();

                                    }
                                    else
                                    {
                                        // If no existing entry, assign CrAmount as debitamt
                                        entry.DrAmount = debitamt;
                                    }
                                }
                                else if (newCrEntry != null && crCount >= 2 && entry.AccountHead == ExistingAccHeadID && IsRemoved == true)
                                {
                                    if (entry.DrCr == "Dr")
                                    {

                                        if (IsMatchingEntry == true)
                                        {
                                            // Accumulate `aEntryAmount` correctly
                                            foreach (var mEntry in matchingEntries)
                                            {
                                                aEntryAmount += (int)mEntry.DrAmount; // Accumulate CrAmount correctly //Matching entries  - Delete entry
                                                mCrAmt = aEntryAmount - rAmt;
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
                                            var existingEntry = dbContext.Tbl201VoucherEntries
                                                                        .FirstOrDefault(v => v.AccountHead == entry.AccountHead
                                                                                          && v.DrCr == "Dr"
                                                                                          && v.VoucherNo == entry.VoucherNo);

                                            if (existingEntry != null)
                                            {
                                                // Update CrAmount by adding the calculated debit amount
                                                //entry.CrAmount = Amt;
                                                entry.DrAmount = mCrAmt;
                                                entry.SysRemarks = Remarks;
                                                // Optionally update the existing entry in the database
                                                existingEntry.VoucherAmount = entry.DrAmount;
                                                existingEntry.SysRemarks = entry.SysRemarks;
                                                dbContext.Tbl201VoucherEntries.Update(existingEntry);
                                                dbContext.SaveChanges();

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
                            if (entry.DrCr == "Dr" && crCount == 1 && drCount == 0)
                            {
                                entry.CrAmount = 0;
                                entry.DrAmount = 0;
                                entry.SysRemarks = "";
                            }

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
            return Unauthorized(new { message = "Invalid tenant.", success = false });

        }

        [HttpPost]
        public async Task<ActionResult> SalesAddVoucherEntry(DataSourceLoadOptions loadOptions, [FromBody] List<Tbl201VoucherEntry> voucherEntries, string AccountHead, string PaymentAccoutHeadName, int Gridcount)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }
            if (voucherEntries == null || !voucherEntries.Any())
            {
                return BadRequest(new { success = false, message = "Invalid data received." });
            }

            try
            {
                Tbl201VoucherMaster voucherMaster = new();
                Tbl201SubLedgerMaster subLedgerMaster = new();
                int aEntryAmount = 0;
                int Amt = 0;
                var Remarks = "";
                bool IsMatchingEntry = false;
                var UserName = HttpContext.Session.GetString("UserName");
                //var matchingEntries;
                List<VoucherEntryDisplayDTO> matchingEntries = new();
                var voucherNos1 = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
                var qryListOfAccountlists1 = dbContext.Qry201VoucherEntryScreenDisplays
                    .Where(p => voucherNos1.Contains(p.VoucherNo))
                    .OrderBy(i => i.DrCr == "Cr" ? 0 : 1) // Ensures "Dr" entries come first
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

                bool isVoucherExists = dbContext.Tbl201VoucherMasters
               .Any(v => v.VoucherNo == voucherEntries[0].VoucherNo);

                if (!isVoucherExists)
                {
                    voucherMaster.VoucherNo = voucherEntries[0].VoucherNo;
                    voucherMaster.VoucherDate = DateTime.Now;
                    dbContext.Tbl201VoucherMasters.AddRange(voucherMaster);
                }

                // Add entries to the database
                dbContext.Tbl201VoucherEntries.AddRange(voucherEntries);
                await dbContext.SaveChangesAsync();
                await _userActionLogger.LogAsync(
              module: "Finance > Cash Receipts",
              actionDetail: $"Saved voucher: {voucherEntries[0].VoucherNo}, Entries: {voucherEntries.Count}, AccountHead: {AccountHead}",
              documentNo: voucherEntries[0].VoucherNo
          );

                //SaveVoucher(voucherEntries);


                var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
                var qryListOfAccountlists = dbContext.Qry201VoucherEntryScreenDisplays
                    .Where(p => voucherNos.Contains(p.VoucherNo))
                    .OrderBy(i => i.DrCr == "Cr" ? 0 : 1) // Ensures "Dr" entries come first
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
                    var existingSubLedger = dbContext.Tbl201SubLedgerMasters
      .FirstOrDefault(x => x.VoucherEntryNo == petty.VoucherEntryNo && x.DrCr == petty.DrCr);
                    if (existingSubLedger == null)
                    {
                        subLedgerMaster.VoucherNo = petty.VoucherNo;
                        subLedgerMaster.VoucherEntryNo = petty.VoucherEntryNo;
                        subLedgerMaster.ReferenceType = "New Reference";
                        subLedgerMaster.ReferenceNo = petty.VoucherNo;
                        subLedgerMaster.AccountNo = petty.AccountHead;
                        subLedgerMaster.Amount = petty.DrAmount;
                        subLedgerMaster.DrCr = petty.DrCr;
                        subLedgerMaster.RetentionAmount = 0;
                        subLedgerMaster.RetentionDueDate = DateTime.Now;
                        subLedgerMaster.AddedBy = UserName;
                        subLedgerMaster.AddedOn = DateTime.Now;

                        dbContext.Tbl201SubLedgerMasters.AddRange(subLedgerMaster);
                    }
                    else
                    {
                        dbContext.Tbl201SubLedgerMasters.UpdateRange(existingSubLedger);
                    }
                    await dbContext.SaveChangesAsync();
                }

                foreach (var entry in resultList)
                {


                    if (!string.IsNullOrEmpty(entry.AccountHead))
                    {

                        var accountHead = dbContext.Qry201ListOfAccounts
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
                                var existingEntry = dbContext.Tbl201VoucherEntries
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
                                    dbContext.Tbl201VoucherEntries.Update(existingEntry);
                                    dbContext.SaveChanges();
                                    await _userActionLogger.LogAsync(
                                          module: "Finance > Cash Receipts",
                                          actionDetail: $"Updated voucher: {voucherEntries[0].VoucherNo}, Entries: {voucherEntries.Count}, AccountHead: {AccountHead}",
                                          documentNo: voucherEntries[0].VoucherNo
                                     );

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
                                        var existingEntry = dbContext.Tbl201VoucherEntries
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
                                            dbContext.Tbl201VoucherEntries.Update(existingEntry);
                                            dbContext.SaveChanges();
                                            await _userActionLogger.LogAsync(
                                                  module: "Finance > Cash Receipts",
                                                  actionDetail: $"Updated voucher: {voucherEntries[0].VoucherNo}, Entries: {voucherEntries.Count}, AccountHead: {AccountHead}",
                                                  documentNo: voucherEntries[0].VoucherNo
                                              );

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

        [HttpPost]
        public async Task<ActionResult> SalesAddBRVoucherEntry(DataSourceLoadOptions loadOptions, [FromBody] List<Tbl201VoucherEntry> voucherEntries, string AccountHead, string PaymentAccoutHeadName, int Gridcount)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                if (voucherEntries == null || !voucherEntries.Any())
                {
                    return BadRequest(new { success = false, message = "Invalid data received." });
                }

                try
                {


                    Tbl201VoucherMaster voucherMaster = new();
                    Tbl201SubLedgerMaster subLedgerMaster = new();
                    var UserName = HttpContext.Session.GetString("UserName");
                    int aEntryAmount = 0;
                    int Amt = 0, Crmt = 0, CrMinusAmt = 0, NewCrMinusAmt = 0, ExistingCrMinusAmt = 0;
                    bool IsMatchingEntry = false;
                    var ExistingAccHeadID = "";

                    //var matchingEntries;
                    List<VoucherEntryDisplayDTO> matchingEntries = new();
                    var voucherNos1 = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
                    var qryListOfAccountlists1 = dbContext.Qry201VoucherEntryScreenDisplays
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


                    bool isVoucherExists = dbContext.Tbl201VoucherMasters
                   .Any(v => v.VoucherNo == voucherEntries[0].VoucherNo);

                    if (!isVoucherExists)
                    {
                        voucherMaster.VoucherNo = voucherEntries[0].VoucherNo;
                        voucherMaster.VoucherDate = DateTime.Now;
                        dbContext.Tbl201VoucherMasters.AddRange(voucherMaster);
                    }

                    // Add entries to the database
                    dbContext.Tbl201VoucherEntries.AddRange(voucherEntries);
                    await dbContext.SaveChangesAsync();

                    //SaveVoucher(voucherEntries);


                    var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
                    var qryListOfAccountlists = dbContext.Qry201VoucherEntryScreenDisplays
                        .Where(p => voucherNos.Contains(p.VoucherNo))
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
                            SysRemarks = i.SysRemarks,
                            Type = i.Type

                        });


                    var resultList = await qryListOfAccountlists.ToListAsync();
                    int DrCount1 = resultList.Count(i => i.DrCr == "Dr");
                    bool hasCrEntries = resultList.Any(i => i.DrCr == "Cr");
                    bool hasDrEntries = voucherEntries.Any(i => i.DrCr == "Dr");
                    int EntriesDrCount1 = voucherEntries.Count(i => i.DrCr == "Dr");
                    if (DrCount1 >= 2)
                    {
                        matchingEntries = resultList.Where(x => x.AccountHead == PaymentAccoutHeadName).ToList();
                        if (matchingEntries.Count != 0)
                        {
                            ExistingCrMinusAmt = (int)matchingEntries[0].DrAmount;
                            ExistingAccHeadID = matchingEntries[0].AccountHead;
                            IsMatchingEntry = true;
                        }
                    }

                    int debitamt = 0; // Initialize debit amount

                    int DrCount = resultList.Count(i => i.DrCr == "Cr");

                    //var lastCrEntry = resultList.LastOrDefault(i => i.DrCr == "Cr");

                    var newCrEntry = resultList
        .Where(i => i.DrCr == "Dr")
        .OrderByDescending(i => i.VoucherEntryNo) // Assuming this is sequential
        .FirstOrDefault();



                    var newlist = newCrEntry != null ? new List<VoucherEntryDisplayDTO> { newCrEntry } : new List<VoucherEntryDisplayDTO>();

                    var NewId = "";
                    foreach (var petty in newlist)
                    {
                        NewId = petty.AccountHead;
                        NewCrMinusAmt = (int)petty.DrAmount;
                        var existingSubLedger = dbContext.Tbl201SubLedgerMasters
.FirstOrDefault(x => x.VoucherEntryNo == petty.VoucherEntryNo && x.DrCr == petty.DrCr);
                        if (existingSubLedger == null)
                        {
                            subLedgerMaster.VoucherNo = petty.VoucherNo;
                            subLedgerMaster.VoucherEntryNo = petty.VoucherEntryNo;
                            subLedgerMaster.ReferenceType = "New Reference";
                            subLedgerMaster.ReferenceNo = petty.VoucherNo;
                            subLedgerMaster.AccountNo = petty.AccountHead;
                            subLedgerMaster.Amount = petty.DrAmount;
                            subLedgerMaster.DrCr = petty.DrCr;
                            subLedgerMaster.RetentionAmount = 0;
                            subLedgerMaster.RetentionDueDate = null;
                            subLedgerMaster.AddedBy = UserName;
                            subLedgerMaster.AddedOn = DateTime.Now;

                            dbContext.Tbl201SubLedgerMasters.AddRange(subLedgerMaster);
                        }
                        else
                        {

                            dbContext.Tbl201SubLedgerMasters.UpdateRange(existingSubLedger);
                        }
                        await dbContext.SaveChangesAsync();
                    }

                    foreach (var entry in resultList)
                    {
                        var accheadid = entry.AccountHead;
                        // if (!entry.SysRemarks.Contains("Paid thru:"))
                        if (entry.Type == "PaymentAccount" && ExistingAccHeadID == accheadid)
                        {

                            if (hasCrEntries == true && IsMatchingEntry == true) //&& NewId == accheadid)
                            {
                                var existingEntry = dbContext.Tbl201VoucherEntries
                                          .FirstOrDefault(v => v.AccountHead == entry.AccountHead
                                                            && v.DrCr == "Dr"
                                                            && v.VoucherNo == entry.VoucherNo);
                                if (EntriesDrCount1 == 2)
                                {
                                    CrMinusAmt = (int)entry.VoucherAmountFormatted;
                                    entry.DrAmount = CrMinusAmt;
                                    existingEntry.VoucherAmount = CrMinusAmt;
                                }
                                else
                                {
                                    //entry.CrAmount = -CrMinusAmt;
                                    CrMinusAmt = NewCrMinusAmt - ExistingCrMinusAmt;
                                    entry.DrAmount = -CrMinusAmt;
                                    existingEntry.VoucherAmount = -CrMinusAmt;
                                }


                                // Optionally update the existing entry in the database


                                // entry.CrAmount = CrMinusAmt;

                                dbContext.Tbl201VoucherEntries.Update(existingEntry);
                                dbContext.SaveChanges();

                            }
                            else if (IsMatchingEntry == true && hasCrEntries != true)//Dr - Cr
                            {
                                //CrMinusAmt = (int)entry.VoucherAmountFormatted + NewCrMinusAmt;
                                CrMinusAmt = NewCrMinusAmt - ExistingCrMinusAmt;
                                entry.DrAmount = -CrMinusAmt;


                                var existingEntry = dbContext.Tbl201VoucherEntries
                                         .FirstOrDefault(v => v.AccountHead == entry.AccountHead
                                                           && v.DrCr == "Dr"
                                                           && v.VoucherNo == entry.VoucherNo);

                                // Optionally update the existing entry in the database
                                existingEntry.VoucherAmount = -CrMinusAmt;


                                // entry.CrAmount = CrMinusAmt;

                                dbContext.Tbl201VoucherEntries.Update(existingEntry);
                                dbContext.SaveChanges();
                            }

                            // CrMinusAmt = (int)entry.VoucherAmountFormatted;


                        }


                        if (!string.IsNullOrEmpty(entry.AccountHead))
                        {

                            var accountHead = dbContext.Qry201ListOfAccounts
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
            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public async Task<ActionResult> PurchaseAddVoucherEntry(DataSourceLoadOptions loadOptions, [FromBody] List<Tbl201VoucherEntry> voucherEntries, string AccountHead, string PaymentAccoutHeadName, int Gridcount)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                if (voucherEntries == null || !voucherEntries.Any())
                {
                    return BadRequest(new { success = false, message = "Invalid data received." });
                }

                try
                {
                    Tbl201VoucherMaster voucherMaster = new();
                    Tbl201SubLedgerMaster subLedgerMaster = new();
                    var UserName = HttpContext.Session.GetString("UserName");
                    int aEntryAmount = 0;
                    int Amt = 0;
                    var Remarks = "";
                    bool IsMatchingEntry = false;
                    var ExistingAccHeadID = "";
                    //var matchingEntries;
                    List<VoucherEntryDisplayDTO> matchingEntries = new();
                    var voucherNos1 = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
                    var qryListOfAccountlists1 = dbContext.Qry201VoucherEntryScreenDisplays
                        .Where(p => voucherNos1.Contains(p.VoucherNo))
                        .OrderBy(i => i.DrCr == "Cr" ? 1 : 0) // Ensures "Dr" entries come first
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
                    int crCount1 = resultList1.Count(i => i.DrCr == "Cr");
                    if (crCount1 >= 2)
                    {
                        matchingEntries = resultList1.Where(x => x.AccountHead == PaymentAccoutHeadName).ToList();
                        if (matchingEntries.Count != 0)
                        {
                            ExistingAccHeadID = matchingEntries[0].AccountHead;
                            IsMatchingEntry = true;
                        }

                    }

                    bool isVoucherExists = dbContext.Tbl201VoucherMasters
                   .Any(v => v.VoucherNo == voucherEntries[0].VoucherNo);

                    if (!isVoucherExists)
                    {
                        voucherMaster.VoucherNo = voucherEntries[0].VoucherNo;
                        voucherMaster.VoucherDate = DateTime.Now;
                        dbContext.Tbl201VoucherMasters.AddRange(voucherMaster);
                    }

                    // Add entries to the database
                    dbContext.Tbl201VoucherEntries.AddRange(voucherEntries);
                    await dbContext.SaveChangesAsync();

                    //SaveVoucher(voucherEntries);


                    var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
                    var qryListOfAccountlists = dbContext.Qry201VoucherEntryScreenDisplays
                        .Where(p => voucherNos.Contains(p.VoucherNo))
                        .OrderBy(i => i.DrCr == "Cr" ? 1 : 0) // Ensures "Dr" entries come first
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

                    int crCount = resultList.Count(i => i.DrCr == "Cr");

                    //var lastCrEntry = resultList.LastOrDefault(i => i.DrCr == "Cr");

                    var newCrEntry = resultList
        .Where(i => i.DrCr == "Cr")
        .OrderByDescending(i => i.VoucherEntryNo) // Assuming this is sequential
        .FirstOrDefault();



                    var newlist = newCrEntry != null ? new List<VoucherEntryDisplayDTO> { newCrEntry } : new List<VoucherEntryDisplayDTO>();

                    var pettycashid = "";
                    foreach (var petty in newlist)
                    {
                        pettycashid = petty.AccountHead;

                        // Check if petty.AccountHead is in Tbl201ChartsofAccounts with AccountGroupId 'A003'
                        bool isValidAccountHead = dbContext.Tbl201ChartOfAccounts
                            .Any(c => c.AccountId == pettycashid && c.AccountGroupId == "A003");

                        if (!isValidAccountHead)
                        {
                            Console.WriteLine($"AccountHead {pettycashid} is not under AccountGroupId A003");
                            continue; // Skip this record
                        }

                        var existingSubLedger = dbContext.Tbl201SubLedgerMasters
                       .FirstOrDefault(x => x.VoucherEntryNo == petty.VoucherEntryNo && x.DrCr == petty.DrCr);
                        if (existingSubLedger == null)
                        {
                            subLedgerMaster.VoucherNo = petty.VoucherNo;
                            subLedgerMaster.VoucherEntryNo = petty.VoucherEntryNo;
                            subLedgerMaster.ReferenceType = "New Reference";
                            subLedgerMaster.ReferenceNo = petty.VoucherNo;
                            subLedgerMaster.AccountNo = petty.AccountHead;
                            subLedgerMaster.Amount = petty.CrAmount;
                            subLedgerMaster.DrCr = petty.DrCr;
                            subLedgerMaster.RetentionAmount = 0;
                            subLedgerMaster.RetentionDueDate = null;
                            subLedgerMaster.AddedBy = UserName;
                            subLedgerMaster.AddedOn = DateTime.Now;

                            dbContext.Tbl201SubLedgerMasters.AddRange(subLedgerMaster);
                        }
                        else
                        {

                            dbContext.Tbl201SubLedgerMasters.UpdateRange(existingSubLedger);
                        }
                        await dbContext.SaveChangesAsync();
                    }

                    foreach (var entry in resultList)
                    {


                        if (!string.IsNullOrEmpty(entry.AccountHead))
                        {

                            var accountHead = dbContext.Qry201ListOfAccounts
                                                      .Where(a => a.AccountId == entry.AccountHead)
                                                      .Select(a => a.AccountHead)
                                                      .FirstOrDefault();



                            if (Gridcount != 0)
                            {
                                // Calculate Debit Amount (DrAmount)

                                debitamt = (int)(debitamt + entry.DrAmount);

                                // If Dr/Cr is Credit ("Cr"), perform specific logic
                                if (entry.DrCr == "Cr" && crCount == 1)
                                // if (crCount==1)
                                {
                                    // Check if an existing entry matches
                                    var existingEntry = dbContext.Tbl201VoucherEntries
                                                                .FirstOrDefault(v => v.AccountHead == entry.AccountHead
                                                                                  && v.DrCr == "Cr"
                                                                                  && v.VoucherNo == entry.VoucherNo);

                                    if (existingEntry != null)
                                    {
                                        // Update CrAmount by adding the calculated debit amount
                                        entry.CrAmount = debitamt;
                                        entry.SysRemarks = Remarks;

                                        // Optionally update the existing entry in the database
                                        existingEntry.VoucherAmount = entry.CrAmount;
                                        existingEntry.SysRemarks = entry.SysRemarks;
                                        dbContext.Tbl201VoucherEntries.Update(existingEntry);
                                        dbContext.SaveChanges();

                                    }
                                    else
                                    {
                                        // If no existing entry, assign CrAmount as debitamt
                                        entry.CrAmount = debitamt;
                                    }
                                }
                                else if (newCrEntry != null && crCount >= 2 && entry.AccountHead == ExistingAccHeadID) //&& entry.AccountHead == pettycashid)
                                {
                                    if (entry.DrCr == "Cr")
                                    {

                                        if (IsMatchingEntry == true)
                                        {
                                            // Accumulate `aEntryAmount` correctly
                                            foreach (var mEntry in matchingEntries)
                                            {
                                                aEntryAmount += (int)mEntry.CrAmount; // Accumulate CrAmount correctly
                                            }

                                            // Process voucherEntries
                                            foreach (var mVoucherEntry in voucherEntries)
                                            {
                                                int eAmount = (int)mVoucherEntry.VoucherAmount;
                                                Amt = aEntryAmount + eAmount;

                                            }

                                            // Check if an existing entry matches
                                            var existingEntry = dbContext.Tbl201VoucherEntries
                                                                        .FirstOrDefault(v => v.AccountHead == entry.AccountHead
                                                                                          && v.DrCr == "Cr"
                                                                                          && v.VoucherNo == entry.VoucherNo);

                                            if (existingEntry != null)
                                            {
                                                // Update CrAmount by adding the calculated debit amount
                                                entry.CrAmount = Amt;
                                                entry.SysRemarks = Remarks;
                                                // Optionally update the existing entry in the database
                                                existingEntry.VoucherAmount = entry.CrAmount;
                                                existingEntry.SysRemarks = entry.SysRemarks;
                                                dbContext.Tbl201VoucherEntries.Update(existingEntry);
                                                dbContext.SaveChanges();

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
            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }


        [HttpPost]
        public async Task<ActionResult> PurchaseAddCrVoucherEntry(DataSourceLoadOptions loadOptions, [FromBody] List<Tbl201VoucherEntry> voucherEntries, string AccountHead, string PaymentAccoutHeadName, int Gridcount)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                if (voucherEntries == null || !voucherEntries.Any())
                {
                    return BadRequest(new { success = false, message = "Invalid data received." });
                }

                try
                {
                    Tbl201VoucherMaster voucherMaster = new();
                    Tbl201SubLedgerMaster subLedgerMaster = new();
                    var UserName = HttpContext.Session.GetString("UserName");
                    int aEntryAmount = 0;
                    int Amt = 0, Crmt = 0, CrMinusAmt = 0, NewCrMinusAmt = 0, ExistingCrMinusAmt = 0;
                    bool IsMatchingEntry = false;
                    var ExistingAccHeadID = "";

                    //var matchingEntries;
                    List<VoucherEntryDisplayDTO> matchingEntries = new();
                    var voucherNos1 = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
                    var qryListOfAccountlists1 = dbContext.Qry201VoucherEntryScreenDisplays
                        .Where(p => voucherNos1.Contains(p.VoucherNo))
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

                    var resultList1 = await qryListOfAccountlists1.ToListAsync();


                    bool isVoucherExists = dbContext.Tbl201VoucherMasters
                   .Any(v => v.VoucherNo == voucherEntries[0].VoucherNo);

                    if (!isVoucherExists)
                    {
                        voucherMaster.VoucherNo = voucherEntries[0].VoucherNo;
                        voucherMaster.VoucherDate = DateTime.Now;
                        dbContext.Tbl201VoucherMasters.AddRange(voucherMaster);
                    }

                    // Add entries to the database
                    dbContext.Tbl201VoucherEntries.AddRange(voucherEntries);
                    await dbContext.SaveChangesAsync();

                    //SaveVoucher(voucherEntries);


                    var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
                    var qryListOfAccountlists = dbContext.Qry201VoucherEntryScreenDisplays
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
                            SysRemarks = i.SysRemarks,
                            Type = i.Type

                        });


                    var resultList = await qryListOfAccountlists.ToListAsync();
                    int crCount1 = resultList.Count(i => i.DrCr == "Cr");
                    bool hasDrEntries = resultList.Any(i => i.DrCr == "Dr");
                    bool hasCrEntries = voucherEntries.Any(i => i.DrCr == "Cr");
                    int EntriesCrCount1 = voucherEntries.Count(i => i.DrCr == "Cr");
                    if (crCount1 >= 2)
                    {
                        matchingEntries = resultList.Where(x => x.AccountHead == PaymentAccoutHeadName).ToList();
                        if (matchingEntries.Count != 0)
                        {
                            ExistingCrMinusAmt = (int)matchingEntries[0].CrAmount;
                            ExistingAccHeadID = matchingEntries[0].AccountHead;
                            IsMatchingEntry = true;
                        }
                    }

                    int debitamt = 0; // Initialize debit amount

                    int crCount = resultList.Count(i => i.DrCr == "Dr");

                    //var lastCrEntry = resultList.LastOrDefault(i => i.DrCr == "Cr");

                    var newCrEntry = resultList
        .Where(i => i.DrCr == "Cr")
        .OrderByDescending(i => i.VoucherEntryNo) // Assuming this is sequential
        .FirstOrDefault();



                    var newlist = newCrEntry != null ? new List<VoucherEntryDisplayDTO> { newCrEntry } : new List<VoucherEntryDisplayDTO>();

                    var NewId = "";
                    foreach (var petty in newlist)
                    {
                        NewId = petty.AccountHead;
                        NewCrMinusAmt = (int)petty.CrAmount;

                        // Check if petty.AccountHead is in Tbl201ChartsofAccounts with AccountGroupId 'A003'
                        bool isValidAccountHead = dbContext.Tbl201ChartOfAccounts
                            .Any(c => c.AccountId == NewId && c.AccountGroupId == "A003");

                        if (!isValidAccountHead)
                        {
                            Console.WriteLine($"AccountHead {NewId} is not under AccountGroupId A003");
                            continue; // Skip this record
                        }

                        var existingSubLedger = dbContext.Tbl201SubLedgerMasters
.FirstOrDefault(x => x.VoucherEntryNo == petty.VoucherEntryNo && x.DrCr == petty.DrCr);
                        if (existingSubLedger == null)
                        {
                            subLedgerMaster.VoucherNo = petty.VoucherNo;
                            subLedgerMaster.VoucherEntryNo = petty.VoucherEntryNo;
                            subLedgerMaster.ReferenceType = "New Reference";
                            subLedgerMaster.ReferenceNo = petty.VoucherNo;
                            subLedgerMaster.AccountNo = petty.AccountHead;
                            subLedgerMaster.Amount = petty.CrAmount;
                            subLedgerMaster.DrCr = petty.DrCr;
                            subLedgerMaster.RetentionAmount = 0;
                            subLedgerMaster.RetentionDueDate = DateTime.Now;
                            subLedgerMaster.AddedBy = UserName;
                            subLedgerMaster.AddedOn = DateTime.Now;

                            dbContext.Tbl201SubLedgerMasters.AddRange(subLedgerMaster);
                        }
                        else
                        {

                            dbContext.Tbl201SubLedgerMasters.UpdateRange(existingSubLedger);
                        }
                        await dbContext.SaveChangesAsync();
                    }

                    foreach (var entry in resultList)
                    {
                        var accheadid = entry.AccountHead;
                        // if (!entry.SysRemarks.Contains("Paid thru:"))
                        if (entry.Type == "PaymentAccount" && ExistingAccHeadID == accheadid)
                        {

                            if (hasCrEntries == true && IsMatchingEntry == true) //&& NewId == accheadid)
                            {
                                var existingEntry = dbContext.Tbl201VoucherEntries
                                          .FirstOrDefault(v => v.AccountHead == entry.AccountHead
                                                            && v.DrCr == "Cr"
                                                            && v.VoucherNo == entry.VoucherNo);
                                if (EntriesCrCount1 == 2)
                                {
                                    CrMinusAmt = (int)entry.VoucherAmountFormatted;
                                    entry.CrAmount = CrMinusAmt;
                                    existingEntry.VoucherAmount = CrMinusAmt;
                                }
                                else
                                {
                                    //entry.CrAmount = -CrMinusAmt;
                                    CrMinusAmt = NewCrMinusAmt - ExistingCrMinusAmt;
                                    entry.CrAmount = -CrMinusAmt;
                                    existingEntry.VoucherAmount = -CrMinusAmt;
                                }


                                // Optionally update the existing entry in the database


                                // entry.CrAmount = CrMinusAmt;

                                dbContext.Tbl201VoucherEntries.Update(existingEntry);
                                dbContext.SaveChanges();

                            }
                            else if (IsMatchingEntry == true && hasCrEntries != true)//Dr - Cr
                            {
                                //CrMinusAmt = (int)entry.VoucherAmountFormatted + NewCrMinusAmt;
                                CrMinusAmt = NewCrMinusAmt - ExistingCrMinusAmt;
                                entry.CrAmount = -CrMinusAmt;


                                var existingEntry = dbContext.Tbl201VoucherEntries
                                         .FirstOrDefault(v => v.AccountHead == entry.AccountHead
                                                           && v.DrCr == "Cr"
                                                           && v.VoucherNo == entry.VoucherNo);

                                // Optionally update the existing entry in the database
                                existingEntry.VoucherAmount = -CrMinusAmt;


                                // entry.CrAmount = CrMinusAmt;

                                dbContext.Tbl201VoucherEntries.Update(existingEntry);
                                dbContext.SaveChanges();
                            }

                            // CrMinusAmt = (int)entry.VoucherAmountFormatted;


                        }


                        if (!string.IsNullOrEmpty(entry.AccountHead))
                        {

                            var accountHead = dbContext.Qry201ListOfAccounts
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
            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

    }
}
