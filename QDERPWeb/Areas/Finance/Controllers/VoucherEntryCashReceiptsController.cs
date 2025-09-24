using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Form.Areas.Finance.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using QD.ERP.Web.Services.Logging;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class VoucherEntryCashReceiptsController : Controller
    {
		private readonly TenantDbContextHelper _tenantDbContextHelper;
		private readonly ILogger<VoucherEntryCashReceiptsController> _logger;
        private readonly IUserActionLogger _userActionLogger;

        public VoucherEntryCashReceiptsController(ILogger<VoucherEntryCashReceiptsController> logger, TenantDbContextHelper tenantDbContextHelper)
		{
            IUserActionLogger userActionLogger;
            _tenantDbContextHelper = tenantDbContextHelper;
			_logger = logger;
		}

		[HttpGet]

        public async Task<IActionResult> GetReceivingAccount(DataSourceLoadOptions loadOptions)
        {
			if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				return Unauthorized(new { success = false, message = "Invalid tenant." });
			}
			var tbl20101salespersonmasters = dbContext.Qry201ListOfAccounts.Where(p => p.AccountGroupId == "A012").Select(i => new
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


        public async Task<IActionResult> GetDefaultReceivingAccount()
        {
			if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				return Unauthorized(new { success = false, message = "Invalid tenant." });
			}
			var defaultAccount = await dbContext.Tbl201ChartOfAccounts
                .Where(a => a.IsDefaultForCash == true && a.AccountGroupId == "A012")
                .OrderByDescending(a => a.RecordModifiedOn) // Get the latest default account
                .Select(a => new { a.AccountId, a.AccountHead })
                .FirstOrDefaultAsync();

            return Json(defaultAccount);
        }


        [HttpGet]
        public async Task<IActionResult> GetAccountHead(DataSourceLoadOptions loadOptions, string SelectedPaymentAccount)
        {
			if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				return Unauthorized(new { success = false, message = "Invalid tenant." });
			}
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


                // Add entries to the database
                Tbl201VoucherMaster voucherMaster = new();

                bool isVoucherExists = dbContext.Tbl201VoucherMasters
               .Any(v => v.VoucherNo == voucherEntries[0].VoucherNo);

                if (!isVoucherExists)
                {
                    voucherMaster.VoucherNo = voucherEntries[0].VoucherNo;
                    voucherMaster.VoucherDate = DateTime.Now;
					dbContext.Tbl201VoucherMasters.AddRange(voucherMaster);
                }

                // Save to DB
                dbContext.Tbl201VoucherEntries.AddRange(voucherEntries);
                await dbContext.SaveChangesAsync();

                // ✅ Log the action
                await _userActionLogger.LogAsync(
                    module: "Finance > Cash Payment",
                    actionDetail: $"Saved voucher: {voucherEntries[0].VoucherNo}, Entries: {voucherEntries.Count}, AccountHead: {AccountHead}",
                    documentNo: voucherEntries[0].VoucherNo
                );




                var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();

                var qryListOfAccountlists = dbContext.Qry201VoucherEntryScreenDisplays

                    .Where(p => voucherNos.Contains(p.VoucherNo))

                    .OrderBy(i => i.DrCr == "Dr") // Order by Dr first (DrCr != "Cr"), then Cr (DrCr == "Cr")

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

                        var accountHead = dbContext.Qry201ListOfAccounts

                                                  .Where(a => a.AccountId == entry.AccountHead)

                                                  .Select(a => a.AccountHead)

                                                  .FirstOrDefault();



                        if (Gridcount != 0)

                        {

                            // Calculate Debit Amount (DrAmount)

                            debitamt = (int)(debitamt + entry.CrAmount);

                            // If Dr/Cr is Credit ("Cr"), perform specific logic

                            if (entry.DrCr == "Dr" & PaymentAccoutHeadName == "Petty Cash - Shabbir")

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

                                    // Optionally update the existing entry in the database

                                    existingEntry.VoucherAmount = entry.CrAmount;

                                    dbContext.Tbl201VoucherEntries.Update(existingEntry);

                                    dbContext.SaveChanges();

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
            string voucherPrefix = $"CR-{yearPart}-{monthPart}-";
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
                await _userActionLogger.LogAsync(
            module: "Finance > Voucher Master",
            actionDetail: $"Saved voucher master entry: {VM.VoucherNo}, Date: {VM.VoucherDate?.ToString("yyyy-MM-dd") ?? "N/A"}",
            documentNo: VM.VoucherNo
        );

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
        //        int debitamt = 0; // Initialize debit amount
        //        // Find the record to delete
        //        var record = await _context.Tbl201VoucherEntries.FirstOrDefaultAsync(v => v.VoucherEntryNo == voucherEntryNo);
        //        if (record == null)
        //        {
        //            return NotFound(new { message = "Record not found!" });
        //        }

        //        if (record.DrCr != "Dr")
        //        {
        //            // Remove the record
        //            _context.Tbl201VoucherEntries.Remove(record);
        //            await _context.SaveChangesAsync();

        //        }

        //        // Get the list of updated vouchers
        //        var voucherEntries = _context.Tbl201VoucherEntries
        //                                      .Where(ve => ve.VoucherNo == VoucherNo) // Filter by the provided VoucherNo
        //                                      .ToList();

        //        var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();

        //        // Query the display list
        //        var qryListOfAccountLists = _context.Qry201VoucherEntryScreenDisplays
        //                                            .Where(p => voucherNos.Contains(p.VoucherNo))
        //                                            .OrderBy(i => i.DrCr == "Dr")
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
        //            debitamt = (int)(debitamt + entry.DrAmount);

        //            // If Dr/Cr is Credit ("Cr"), perform specific logic
        //            if (entry.DrCr == "Dr" & PaymentAccoutHeadName == "Petty Cash - Shabbir")
        //            {
        //                // Check if an existing entry matches
        //                var existingEntry = _context.Tbl201VoucherEntries
        //                                            .FirstOrDefault(v => v.AccountHead == entry.AccountHead
        //                                                              && v.DrCr == "Dr"
        //                                                              && v.VoucherNo == entry.VoucherNo);

        //                if (existingEntry != null)
        //                {
        //                    // Update CrAmount by adding the calculated debit amount
        //                    entry.DrAmount = debitamt;

        //                    // Optionally update the existing entry in the database
        //                    existingEntry.VoucherAmount = entry.DrAmount;
        //                    _context.Tbl201VoucherEntries.Update(existingEntry);
        //                    _context.SaveChanges();

        //                }
        //                else
        //                {
        //                    // If no existing entry, assign CrAmount as debitamt
        //                    entry.DrAmount = debitamt;
        //                }
        //            }

        //            if (!string.IsNullOrEmpty(entry.AccountHead))
        //            {
        //                // Find the account head
        //                var accountHead = _context.Qry201ListOfAccounts
        //                                          .Where(a => a.AccountId == entry.AccountHead)
        //                                          .Select(a => a.AccountHead)
        //                                          .FirstOrDefault();

        //                // Update AccountHead and SysRemarks
        //                entry.AccountHead = accountHead ?? PaymentAccoutHeadName;
        //                if (resultList.Count == 1)
        //                {
        //                    entry.DrAmount = 0;
        //                    entry.CrAmount = 0;
        //                    entry.SysRemarks = "";
        //                }

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
        //[HttpPost]
        //public async Task<ActionResult> DeleteAllVoucherEntry(DataSourceLoadOptions loadOptions, string VoucherNo)
        //{
        //    try
        //    {
        //        // Find all records matching the given VoucherNo
        //        var records = await _context.Tbl201VoucherEntries
        //                                    .Where(v => v.VoucherNo == VoucherNo)
        //                                    .ToListAsync();

        //        if (records == null || !records.Any())
        //        {
        //            return NotFound(new { message = "No records found for the provided VoucherNo!" });
        //        }

        //        // Remove all matching records
        //        _context.Tbl201VoucherEntries.RemoveRange(records);
        //        await _context.SaveChangesAsync();

        //        // Fetch updated voucher list
        //        var voucherEntries = await _context.Tbl201VoucherEntries
        //                                           .Where(ve => ve.VoucherNo == VoucherNo)
        //                                           .ToListAsync();

        //        var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct().ToList();

        //        // Query the updated display list
        //        var qryListOfAccountLists = _context.Qry201VoucherEntryScreenDisplays
        //                                            .Where(p => voucherNos.Contains(p.VoucherNo))
        //                                            .OrderBy(i => i.DrCr == "Cr")
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

        //        // Return the modified list for DataSourceLoader
        //        return Json(DataSourceLoader.Load(resultList.AsQueryable(), loadOptions));
        //    }
        //    catch (Exception ex)
        //    {
        //        // Return a detailed error response
        //        return StatusCode(500, new { message = "An error occurred while deleting the records.", error = ex.Message });
        //    }
        //}

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
                await _userActionLogger.LogAsync(
                   module: "Finance > Cash Payment",
                   actionDetail: $"Added Voucher: {voucherEntries[0].VoucherNo}, Entries: {voucherEntries.Count}, AccountHead: {AccountHead}",
                   documentNo: voucherEntries[0].VoucherNo
               );
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
                int crCount1 = resultList1.Count(i => i.DrCr == "Cr");
                int drCount1 = resultList1.Count(i => i.DrCr == "Dr");
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
                   module: "Finance > Cash Payment",
                   actionDetail: $"Delete voucher: {VoucherNo}, Entries: {voucherEntryNo}, AccountHead: {PaymentAccoutHeadName}",
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

                int crCount = resultList.Count(i => i.DrCr == "Cr");
                int drCount = resultList1.Count(i => i.DrCr == "Dr");

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
                        if (entry.DrCr == "Dr" && crCount == 0 && drCount == 1)
                        {
                            entry.DrAmount = 0;
                            entry.CrAmount = 0;
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



    }
}
