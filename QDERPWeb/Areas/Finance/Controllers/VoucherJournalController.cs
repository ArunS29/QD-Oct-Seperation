using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.DAL.Entities;

using Microsoft.EntityFrameworkCore;
using DevExpress.Emf;
using QD.ERP.Web.Areas.Finance.Controllers;
using QD.ERP.Web.Service;
//using SkiaSharp;


namespace QDWEB.Areas.Finance.Controllers
{//[Area("Finance")]
    [Route("api/[controller]/[action]")]
    // [Route("Finapi/[controller]/[action]")]
    [ApiController]
    public class VoucherJournalController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<VoucherJournalController> _logger;

        public VoucherJournalController(ILogger<VoucherJournalController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult> GetNewVoucherNo()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {

                string voucherPrefix = "JV-NEW-";
            string strNewVoucherNo;

            try
            {
                using (var transaction = await dbContext.Database.BeginTransactionAsync())
                {
                    string sql = @"
                SELECT MAX(CAST(RIGHT(TempVoucherNo, 6) AS INT)) AS MaxVoucherNo
                FROM tbl201VoucherMasterTemp WITH (TABLOCKX)
                WHERE TempVoucherNo LIKE {0}";

                    // Fix: Use FromSqlRaw instead of SqlQueryAsync
                    var result = await dbContext.VoucherResults
                        .FromSqlRaw(sql, voucherPrefix + "%")
                        .ToListAsync();

                    int maxVoucherNo = result.FirstOrDefault()?.MaxVoucherNo ?? 0;

                    int newVoucherNo = maxVoucherNo + 1;
                    strNewVoucherNo = voucherPrefix + newVoucherNo.ToString("D6");

                    var newVoucherEntry = new Tbl201VoucherMasterTemp
                    {
                        TempVoucherNo = strNewVoucherNo
                    };

                        dbContext.Tbl201VoucherMasterTemps.Add(newVoucherEntry);
                    await dbContext.SaveChangesAsync();

                    await transaction.CommitAsync();
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }


            return Json(strNewVoucherNo);
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }



        [HttpGet]
        public IActionResult Delete(long VoucherEntryNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var item = dbContext.Tbl201VoucherEntryTemps.Where(p => p.VoucherEntryNo == VoucherEntryNo).FirstOrDefault();
            if (item != null)
            {
                    dbContext.Tbl201VoucherEntryTemps.Remove(item);
                    dbContext.SaveChanges();
                return Ok(new { success = true });
            }
            return NotFound();
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }


        [HttpGet]
        public async Task<ActionResult> GetSupplierName(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var qryListOfAccountlists = dbContext.Qry201ListOfAccounts.Select(i => new
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

                i.IsRestricted,
                i.IsUseInSales,
                i.IsUsedInPurchase,
                i.IsProfitLossAccount,
                i.IsBalanceSheetAccount,
                i.IsMaintainBillByBill,
                i.IsUseInReconciliation,
                i.IsSalaryPayable,
                i.Expr1,

            });

            return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet]
        public async Task<ActionResult> GetVoucherEntries(DataSourceLoadOptions loadOptions, string voucherNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var qryListOfAccountlists = dbContext.Tbl201VoucherEntryTemps.Where(p => p.VoucherNo == voucherNo).Select(i => new
            {
                i.VoucherNo,
                i.DrCr,
                i.VoucherAmount,
                i.EntryNarration,
                i.AccountHead,
                i.SysRemarks,
                i.VoucherEntryNo,
                i.AddedBy,
                i.AddedOn
            });


            return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpPost]
        public async Task<ActionResult> AddVoucherEntry(DataSourceLoadOptions loadOptions, [FromBody] Tbl201VoucherEntryTemp VE)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                if (VE == null)
            {
                return BadRequest(new { success = false, message = "Invalid data received." });
            }

            try
            {
                    // Add the new voucher entry to the table
                    dbContext.Tbl201VoucherEntryTemps.Add(VE);
                await dbContext.SaveChangesAsync();

                // Fetch voucher entries and join with account names
                var qryListOfAccountlists = await dbContext.Tbl201VoucherEntryTemps
                    .Where(p => p.VoucherNo == VE.VoucherNo) // Fix comparison operator
                    .OrderBy(i => i.DrCr == "Dr" ? 0 : 1) // "Dr" entries first
                    .Select(i => new VoucherEntryDisplayDTO
                    {
                        VoucherNo = i.VoucherNo,
                        VoucherEntryNo = i.VoucherEntryNo,
                        DrCr = i.DrCr,
                        VoucherAmount = i.VoucherAmount,
                        EntryNarration = i.EntryNarration,
                        AccountHead = dbContext.Qry201ListOfAccounts
                                              .Where(a => a.AccountId == i.AccountHead)
                                              .Select(a => a.AccountHead)
                                              .FirstOrDefault(), // Get AccountHead from ChartOfAccounts
                        SysRemarks = i.SysRemarks,
                        AddedBy = i.AddedBy,
                        AddedOn = i.AddedOn,
                    })
                    .ToListAsync(); // Async execution

                return Json(DataSourceLoader.Load(qryListOfAccountlists.AsQueryable(), loadOptions));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public async Task<ActionResult> SaveVoucher([FromBody] VoucherViewModel VM)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                if (VM == null || VM.VoucherEntries == null || !VM.VoucherEntries.Any())
            {
                return BadRequest(new { success = false, message = "Invalid data received." });
            }

            try
            {
                using (var transaction = await dbContext.Database.BeginTransactionAsync())
                {
                        DateTime currentDate = DateTime.Now;
                        string currentYear = currentDate.Year.ToString();
                        string currentMonth = currentDate.Month.ToString("00");
                        string voucherString = "JV-" + currentYear.Substring(currentYear.Length - 2, 2) + "-" + currentMonth + "-";
                        string strNewReceiptNo;

                        // SQL query with interpolated string
                        string likePattern = voucherString + "%";

                        
                            // Use raw SQL query to fetch the maximum voucher number
                            var result = await dbContext.VoucherResults
                                .FromSqlInterpolated($@"
                SELECT MAX(CAST(RIGHT(VoucherNo, 3) AS INT)) AS MaxVoucherNo
                FROM Tbl201VoucherEntry
                WHERE VoucherNo LIKE {likePattern}")
                                .ToListAsync();

                            int maxVoucherNo = result.FirstOrDefault()?.MaxVoucherNo ?? 0;

                            int journalNo = maxVoucherNo + 1;

                            // Format the new voucher number with leading zeros
                            strNewReceiptNo = "000" + journalNo.ToString();
                            strNewReceiptNo = strNewReceiptNo.Substring(strNewReceiptNo.Length - 3);

                            // Concatenate with the voucher string
                            strNewReceiptNo = voucherString + strNewReceiptNo;

                        var newVoucherNo = strNewReceiptNo;
                    // Assign `VoucherNo` and fetch `AccountId`
                    foreach (var entry in VM.VoucherEntries)
                    {
                        entry.VoucherNo = newVoucherNo;
                        entry.VoucherEntryNo = 0;

                        // Fetch AccountId based on AccountHead
                        entry.AccountHead = await dbContext.Tbl201ChartOfAccounts
                            .Where(a => a.AccountHead == entry.AccountHead)
                            .Select(a => a.AccountId)
                            .FirstOrDefaultAsync();
                    }

                    // **Save to VoucherMaster Table**
                    var voucherMaster = new Tbl201VoucherMaster
                    {
                        VoucherNo = newVoucherNo,
                        VoucherDate = VM.VoucherMaster.VoucherDate,
                        VoucherEffectiveDate = VM.VoucherMaster.VoucherEffectiveDate,
                        VoucherNarration = VM.VoucherMaster.VoucherNarration,
                        BillRemarks = VM.VoucherMaster.BillRemarks,
                        VoucherType = VM.VoucherMaster.VoucherType,
                        IsVerified = VM.VoucherMaster.IsVerified,
                        IsApproved = VM.VoucherMaster.IsApproved,
                        VoucherVerifiedBy = VM.VoucherMaster.VoucherVerifiedBy,
                        VoucherApprovedBy = VM.VoucherMaster.VoucherApprovedBy,
                        VoucherVerifiedOn = DateTime.Now,
                        VoucherApprovedOn = DateTime.Now,
                        basecurrencyid = VM.VoucherMaster.basecurrencyid,
                        currencyid = VM.VoucherMaster.currencyid,
                        currencyrate = VM.VoucherMaster.currencyrate

                    };

                        dbContext.Tbl201VoucherMasters.Add(voucherMaster);
                    await dbContext.Tbl201VoucherEntries.AddRangeAsync(VM.VoucherEntries);
                    await dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(new { success = true, message = "Voucher saved successfully!", voucherNo = newVoucherNo });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }





        [HttpPost]
        public async Task<ActionResult> UpdateVoucher([FromBody] VoucherViewModel VM)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                // **Validate Incoming Data**
                if (VM == null || VM.VoucherMaster == null || string.IsNullOrEmpty(VM.VoucherMaster.VoucherNo) || VM.VoucherEntries == null || !VM.VoucherEntries.Any())
            {
                return BadRequest(new { success = false, message = "Invalid data received or missing voucher number." });
            }

            try
            {
                using (var transaction = await dbContext.Database.BeginTransactionAsync())
                {
                    // **Find Existing Voucher Master**
                    var voucherMaster = await dbContext.Tbl201VoucherMasters
                        .FirstOrDefaultAsync(v => v.VoucherNo == VM.VoucherMaster.VoucherNo);

                    if (voucherMaster == null)
                    {
                        return NotFound(new { success = false, message = "Voucher not found." });
                    }

                    // **Update Master Record**
                    voucherMaster.VoucherDate = VM.VoucherMaster.VoucherDate;
                    voucherMaster.VoucherEffectiveDate = VM.VoucherMaster.VoucherEffectiveDate;
                    voucherMaster.VoucherNarration = VM.VoucherMaster.VoucherNarration;
                    voucherMaster.BillRemarks = VM.VoucherMaster.BillRemarks;

                    // **Delete Old Entries First**
                    var existingEntries = await dbContext.Tbl201VoucherEntries
                        .Where(e => e.VoucherNo == VM.VoucherMaster.VoucherNo)
                        .ToListAsync();

                        dbContext.Tbl201VoucherEntries.RemoveRange(existingEntries);
                    await dbContext.SaveChangesAsync(); // Ensure old records are removed first

                    // **Fetch Account IDs and Add New Entries**
                    var newEntries = new List<Tbl201VoucherEntry>();

                    foreach (var entry in VM.VoucherEntries)
                    {
                        // **Ensure AccountHead is not null**
                        if (string.IsNullOrEmpty(entry.AccountHead))
                        {
                            return BadRequest(new { success = false, message = "AccountHead cannot be null or empty." });
                        }

                        // **Fetch Account ID from ChartOfAccounts**
                        //var accountID = await _context.Tbl201ChartOfAccounts
                        //    .Where(a => a.AccountHead == entry.AccountHead)
                        //    .Select(a => a.AccountId) // Convert to nullable int to avoid null exceptions
                        //    .FirstOrDefaultAsync();

                        //if (accountID == null)
                        //         {
                        //        return BadRequest(new { success = false, message = $"AccountHead '{entry.AccountHead}' not found in ChartOfAccounts." });
                        //        }

                        // **Create New Entry** add objects to list
                        newEntries.Add(new Tbl201VoucherEntry
                        {
                            VoucherNo = VM.VoucherMaster.VoucherNo,
                            // Assign AccountID as a string
                            DrCr = entry.DrCr,
                            VoucherAmount = entry.VoucherAmount,
                            EntryNarration = entry.EntryNarration,
                            AccountHead = await dbContext.Tbl201ChartOfAccounts
                                             .Where(a => a.AccountHead == entry.AccountHead)
                                             .Select(a => a.AccountId) // Convert to nullable int to avoid null exceptions
                                             .FirstOrDefaultAsync(),
                        });
                    }

                    // **Insert New Entries**
                    await dbContext.Tbl201VoucherEntries.AddRangeAsync(newEntries);
                    await dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(new { success = true, message = "Voucher updated successfully!" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }


        //[HttpGet]
        //public async Task<ActionResult> LoadVoucherEntries(DataSourceLoadOptions loadOptions, string voucherNo)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(voucherNo))
        //        {
        //            return BadRequest(new { success = false, message = "Invalid Voucher Number." });
        //        }

        //        var qryListOfAccountlists = _context.Tbl201VoucherEntries
        //            .Where(p => p.VoucherNo == voucherNo)
        //            .OrderBy(i => i.DrCr == "Cr" ? 1 : 0) // Ensures "Dr" entries come first
        //            .Select(i => new
        //            {
        //                i.VoucherNo,
        //                i.VoucherEntryNo,
        //                i.DrCr,
        //                i.VoucherAmount,
        //                i.EntryNarration,
        //                i.AccountHead,
        //                i.SysRemarks
        //            })
        //            .ToList();

        //        // Fetch AccountHead names for mapping
        //        var accountIds = qryListOfAccountlists.Select(i => i.AccountHead).Distinct().ToList();
        //        var accountHeadMap = _context.Qry201ListOfAccounts
        //            .Where(a => accountIds.Contains(a.AccountId))
        //            .ToDictionary(a => a.AccountId, a => a.AccountHead);

        //        // Map AccountId to AccountHead
        //        var resultList = qryListOfAccountlists.Select(i => new VoucherEntryDisplayDTO
        //        {
        //            VoucherNo = i.VoucherNo,
        //            VoucherEntryNo = i.VoucherEntryNo,
        //            DrCr = i.DrCr,
        //            DrAmount = i.VoucherAmount,
        //            CrAmount = i.VoucherAmount,
        //            EntryNarration = i.EntryNarration,
        //            AccountHead = accountHeadMap.ContainsKey(i.AccountHead) ? accountHeadMap[i.AccountHead] : i.AccountHead,
        //            SysRemarks = i.SysRemarks
        //        }).ToList();

        //        return Json(DataSourceLoader.Load(resultList.AsQueryable(), loadOptions));
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
        //    }
        //}
        [HttpGet]
        public async Task<ActionResult> LoadVoucherEntries(DataSourceLoadOptions loadOptions, string voucherNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
            {
                if (string.IsNullOrEmpty(voucherNo))
                {
                    return BadRequest(new { success = false, message = "Invalid Voucher Number." });
                }

                var qryListOfAccountlists = dbContext.Tbl201VoucherEntries
              .Where(p => p.VoucherNo == voucherNo)
              .OrderBy(i => i.DrCr == "Dr" ? 0 : 1) // Ensures "Dr" entries come first
              .Select(i => new VoucherEntryDisplayDTO
              {
                  VoucherNo = i.VoucherNo,
                  VoucherEntryNo = i.VoucherEntryNo,
                  DrCr = i.DrCr,
                  VoucherAmount = i.VoucherAmount, // No need for special handling for "Cr"
                  EntryNarration = i.EntryNarration,
                  AccountHead = i.AccountHead,
                  SysRemarks = i.SysRemarks
              })
              .ToList(); // Execute the query

                foreach (var entry in qryListOfAccountlists)
                {
                    string accountHead = dbContext.Qry201ListOfAccounts
                                                 .Where(a => a.AccountId == entry.AccountHead)
                                                 .Select(a => a.AccountHead)
                                                 .FirstOrDefault();
                    entry.AccountHead = accountHead;

                }



                return Json(DataSourceLoader.Load(qryListOfAccountlists.AsQueryable(), loadOptions));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<ActionResult> GetVoucherMasterEntries(DataSourceLoadOptions loadOptions, string voucherNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var qryListOfAccountlists = dbContext.Tbl201VoucherMasters.Where(p => p.VoucherNo == voucherNo).Select(i => new
            {
                i.VoucherNo,

                i.VoucherDate,
                i.VoucherEffectiveDate,
                i.VoucherNarration,
                i.BillRemarks,
                i.VoucherType,
                i.IsVerified,
                i.IsApproved,
                i.VoucherVerifiedBy,
                i.VoucherApprovedBy,
                i.VoucherVerifiedOn,
                i.VoucherApprovedOn
            });

            return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));

            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }




        [HttpPost]

        public IActionResult DeleteAllVoucherEntry(string VoucherNo, string TemporaryNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                if (string.IsNullOrEmpty(VoucherNo) || string.IsNullOrEmpty(TemporaryNo))
            {
                return BadRequest("Invalid parameters. Both VoucherNo and TemporaryNo are required.");
            }

            try
            {
                using (var transaction = dbContext.Database.BeginTransaction()) // Start transaction
                {
                    // 1. Delete from tbl201VoucherEntryTemp where VoucherNo = TemporaryNo
                    var tempEntries = dbContext.Tbl201VoucherEntryTemps
                                                .Where(e => e.VoucherNo == TemporaryNo)
                                                .ToList();
                    if (tempEntries.Any())
                    {
                            dbContext.Tbl201VoucherEntryTemps.RemoveRange(tempEntries);
                            dbContext.SaveChanges();
                    }

                    // 2. Delete from tbl201VoucherEntry where VoucherNo = VoucherNo
                    var entryRecords = dbContext.Tbl201VoucherEntries
                                                 .Where(e => e.VoucherNo == VoucherNo)
                                                 .ToList();
                    if (entryRecords.Any())
                    {
                            dbContext.Tbl201VoucherEntries.RemoveRange(entryRecords);
                            dbContext.SaveChanges();
                    }

                    // 3. Delete from tbl201VoucherMaster where VoucherNo = VoucherNo
                    var masterEntries = dbContext.Tbl201VoucherMasters
                                                  .Where(m => m.VoucherNo == VoucherNo)
                                                  .ToList();
                    if (masterEntries.Any())
                    {
                            dbContext.Tbl201VoucherMasters.RemoveRange(masterEntries);
                            dbContext.SaveChanges();
                    }

                    transaction.Commit(); // Commit only if all deletions succeed

                    // Fetch updated data after deletion
                    var updatedData = dbContext.Tbl201VoucherMasters.ToList();

                    return Ok(new { data = updatedData, message = "Voucher deleted successfully" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting voucher: {ex.Message}");
            }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpPost]
        public async Task<ActionResult> DeleteAllEntries(DataSourceLoadOptions loadOptions, string VoucherNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {

                try
            {
                // Find all records matching the given VoucherNo
                var records = await dbContext.Tbl201VoucherEntries
                                            .Where(v => v.VoucherNo == VoucherNo)
                                            .ToListAsync();

                if (records == null || !records.Any())
                {
                    return NotFound(new { message = "No records found for the provided VoucherNo!" });
                }

                    // Remove all matching records
                    dbContext.Tbl201VoucherEntries.RemoveRange(records);
                await dbContext.SaveChangesAsync();

                // Fetch updated voucher list
                var voucherEntries = await dbContext.Tbl201VoucherEntries
                                                   .Where(ve => ve.VoucherNo == VoucherNo)
                                                   .ToListAsync();

                var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct().ToList();

                // Query the updated display list
                var qryListOfAccountLists = dbContext.Qry201VoucherEntryScreenDisplays
                                                    .Where(p => voucherNos.Contains(p.VoucherNo))
                                                    .OrderBy(i => i.DrCr == "Dr")
                                                    .Select(i => new VoucherEntryDisplayDTO
                                                    {
                                                        VoucherNo = i.VoucherNo,
                                                        VoucherEntryNo = i.VoucherEntryNo,
                                                        DrCr = i.DrCr,
                                                        VoucherAmount = i.VoucherAmount, // No need for special handling for "Cr"
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

            return Unauthorized(new { message = "Invalid tenant.", success = false });

        }

    }
}
