using DevExpress.Emf;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Finance.Areas.Finance.Controllers;
using QD.ERP.Shared.Models;
using QD.ERP.Shared.DAL.Entities;
using QD.ERP.Shared.Service;
using QD.ERP.Shared.Services.Logging;
//using SkiaSharp;
using QDERPWeb.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;


namespace QDWEB.Areas.Finance.Controllers
{//[Area("Finance")]
    [Route("api/[controller]/[action]")]
    // [Route("Finapi/[controller]/[action]")]
    [ApiController]
    public class VoucherJournalController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<VoucherJournalController> _logger;

        private readonly FcmService _fcmService;
        private readonly IUserActionLogger _userActionLogger;

        public VoucherJournalController(ILogger<VoucherJournalController> logger, TenantDbContextHelper tenantDbContextHelper, IUserActionLogger userActionLogger, FcmService fcmService)
        {
            
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
            _fcmService = fcmService;
            _userActionLogger = userActionLogger;
        }

        [HttpGet]
        public async Task<ActionResult> GetNewVoucherNo()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                string voucherPrefix = "JV-NEW-";

                try
                {
                    var strategy = dbContext.Database.CreateExecutionStrategy();

                    string strNewVoucherNo = null;

                    // Wrap everything in the retry strategy
                    strNewVoucherNo = await strategy.ExecuteAsync(async () =>
                    {
                        using (var transaction = await dbContext.Database.BeginTransactionAsync())
                        {
                            string sql = @"
                        SELECT MAX(CAST(RIGHT(TempVoucherNo, 6) AS INT)) AS MaxVoucherNo
                        FROM tbl201VoucherMasterTemp WITH (TABLOCKX)
                        WHERE TempVoucherNo LIKE {0}";

                            var result = await dbContext.VoucherResults
                                .FromSqlRaw(sql, voucherPrefix + "%")
                                .ToListAsync();

                            int maxVoucherNo = result.FirstOrDefault()?.MaxVoucherNo ?? 0;
                            int newVoucherNo = maxVoucherNo + 1;

                            string generatedVoucherNo = voucherPrefix + newVoucherNo.ToString("D6");

                            var newVoucherEntry = new Tbl201VoucherMasterTemp
                            {
                                TempVoucherNo = generatedVoucherNo
                            };

                            dbContext.Tbl201VoucherMasterTemps.Add(newVoucherEntry);
                            await dbContext.SaveChangesAsync();

                            await transaction.CommitAsync();

                            return generatedVoucherNo; // Return from inside the lambda
                        }
                    });

                    return Json(strNewVoucherNo);
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, error = ex.Message });
                }
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


        private async Task<string> GenerateNewVoucherNo(ERPMasterWtDataContext dbContext, byte companyId)
        {
            // Step 2: Get NoOfDigitsInVouchers
            var companyConfig = await dbContext.Tbl901CompanyDetails02s
                .Where(c => c.CompanyId == companyId)
                .Select(c => new { c.NoOfDigitsInVouchers })
                .FirstOrDefaultAsync();

            byte configuredDigitCount = companyConfig?.NoOfDigitsInVouchers ?? 3;

            // Step 3: Prepare voucher prefix
            DateTime currentDate = DateTime.Now;
            string yearPart = currentDate.Year.ToString().Substring(2); // "25"
            string monthPart = currentDate.Month.ToString("00");        // "07"
            string voucherPrefix = $"JV-{yearPart}-{monthPart}-";
            string likePattern = voucherPrefix + "%";

            int digitCountToUse = configuredDigitCount;
            string strNewReceiptNo;

            try
            {
                var existingVoucher = await dbContext.Tbl201VoucherEntries
                    .Where(v => v.VoucherNo.StartsWith(voucherPrefix))
                    .OrderByDescending(v => v.VoucherNo)
                    .Select(v => v.VoucherNo)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(existingVoucher))
                {
                    string numberPart = existingVoucher.Substring(voucherPrefix.Length);
                    digitCountToUse = numberPart.Length;
                }

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
            catch
            {
                string fallback = "1".PadLeft(configuredDigitCount, '0');
                strNewReceiptNo = voucherPrefix + fallback;
            }

            return strNewReceiptNo;
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
                    var strategy = dbContext.Database.CreateExecutionStrategy();
                    string newVoucherNo = null;

                    await strategy.ExecuteAsync(async () =>
                    {
                        using (var transaction = await dbContext.Database.BeginTransactionAsync())
                        {
                            // 🔁 Use shared method to generate voucher number
                            string defaultCompanyString = HttpContext.Session.GetString("DefaultcompanyID") ?? "0";
                            byte.TryParse(defaultCompanyString, out byte companyId);
                            newVoucherNo = await GenerateNewVoucherNo(dbContext, companyId);

                            // ✅ Assign voucher number and resolve AccountId
                            foreach (var entry in VM.VoucherEntries)
                            {
                                entry.VoucherNo = newVoucherNo;
                                entry.VoucherEntryNo = 0;

                                // Convert AccountHead to AccountId
                                entry.AccountHead = await dbContext.Tbl201ChartOfAccounts
                                    .Where(a => a.AccountHead == entry.AccountHead)
                                    .Select(a => a.AccountId)
                                    .FirstOrDefaultAsync();
                            }

                            var voucherMaster = new Tbl201VoucherMaster
                            {
                                VoucherNo = newVoucherNo,
                                VoucherDate = VM.VoucherMaster.VoucherDate,
                                VoucherEffectiveDate = VM.VoucherMaster.VoucherEffectiveDate,
                                VoucherNarration = VM.VoucherMaster.VoucherNarration,
                                BillRemarks = VM.VoucherMaster.BillRemarks,
                                VoucherType = VM.VoucherMaster.VoucherType,
                                basecurrencyid = VM.VoucherMaster.basecurrencyid,
                                currencyid = VM.VoucherMaster.currencyid,
                                currencyrate = VM.VoucherMaster.currencyrate
                            };

                            dbContext.Tbl201VoucherMasters.Add(voucherMaster);
                            await dbContext.Tbl201VoucherEntries.AddRangeAsync(VM.VoucherEntries);
                            await dbContext.SaveChangesAsync();
                            await _userActionLogger.LogAsync(
                  module: "Finance > Voucher Journal",
                  actionDetail: $"Saved Voucher: {VM.VoucherMaster.VoucherNo}",
                  documentNo: VM.VoucherMaster.VoucherNo
              );
                            await _userActionLogger.LogAsync(
                  module: "Finance > Voucher Journal",
                  actionDetail: $"Added Voucher: {VM.VoucherEntries[0].VoucherNo}, Entries: {VM.VoucherEntries.Count}",
                  documentNo: VM.VoucherEntries[0].VoucherNo
              );
                            await transaction.CommitAsync();
                        }
                    });

                    var UserId = HttpContext.Session.GetString("UserId");
                    var TenantName = HttpContext.Session.GetString("TenantName");

                    var notifyRequest = new NotificationRequest
                    {
                        UserId = UserId, // or fetch from session/DB
                        VoucherName = newVoucherNo,
                        ActionType = "You have one Journal Voucher to verify",
                        TenantName = TenantName
                    };

                    await _fcmService.SendNotificationAsync(notifyRequest);

                    return Ok(new { success = true, message = "Voucher saved successfully!", voucherNo = newVoucherNo });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }


        [HttpPost]
        public IActionResult UpdateVoucherEntry([FromBody] Tbl201VoucherEntry model)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {

                if (model == null || string.IsNullOrEmpty(model.AccountHead))
                {
                    return BadRequest("Invalid data: AccountHead is missing or null.");
                }

                var existingEntry = dbContext.Tbl201VoucherEntries
                    .FirstOrDefault(v => v.VoucherEntryNo == model.VoucherEntryNo);

                var accountID = dbContext.Tbl201ChartOfAccounts
                    .Where(a => a.AccountHead == model.AccountHead)
                    .Select(a => a.AccountId)
                    .FirstOrDefault();

                if (existingEntry != null)
                {
                    existingEntry.DrCr = model.DrCr;
                    existingEntry.AccountHead = accountID; // Assign single account ID
                    existingEntry.EntryNarration = model.EntryNarration;
                    existingEntry.SysRemarks = model.SysRemarks; // Ensure SysRemarks is updated



                    dbContext.SaveChanges();
                    return Ok(new { message = "" });
                }

                return NotFound("Voucher Entry not found.");
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });

        }


        [HttpPost]
        public IActionResult UpdateVoucherEntryTemp([FromBody] Tbl201VoucherEntryTemp model)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {

                if (model == null || string.IsNullOrEmpty(model.AccountHead))
                {
                    return BadRequest("Invalid data: AccountHead is missing or null.");
                }

                var existingEntry = dbContext.Tbl201VoucherEntryTemps
                    .FirstOrDefault(v => v.VoucherEntryNo == model.VoucherEntryNo);

                var accountID = dbContext.Tbl201ChartOfAccounts
                    .Where(a => a.AccountHead == model.AccountHead)
                    .Select(a => a.AccountId)
                    .FirstOrDefault();

                if (existingEntry != null)
                {
                    existingEntry.DrCr = model.DrCr;
                    existingEntry.AccountHead = accountID; // Assign single account ID
                    existingEntry.EntryNarration = model.EntryNarration;
                    existingEntry.SysRemarks = model.SysRemarks; // Ensure SysRemarks is updated



                    dbContext.SaveChanges();
                    return Ok(new { message = "" });
                }

                return NotFound("Voucher Entry not found.");
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });

        }

        [HttpPost]
        public async Task<ActionResult> UpdateVoucher([FromBody] VoucherViewModel VM)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                if (VM == null || VM.VoucherMaster == null || string.IsNullOrEmpty(VM.VoucherMaster.VoucherNo) || VM.VoucherEntries == null || !VM.VoucherEntries.Any())
                {
                    return BadRequest(new { success = false, message = "Invalid data received or missing voucher number." });
                }

                try
                {
                    var strategy = dbContext.Database.CreateExecutionStrategy();

                    await strategy.ExecuteAsync(async () =>
                    {
                        using (var transaction = await dbContext.Database.BeginTransactionAsync())
                        {
                            // Find and update voucher master
                            var voucherMaster = await dbContext.Tbl201VoucherMasters
                                .FirstOrDefaultAsync(v => v.VoucherNo == VM.VoucherMaster.VoucherNo);

                            if (voucherMaster == null)
                            {
                                throw new Exception("Voucher not found.");
                            }

                            voucherMaster.VoucherDate = VM.VoucherMaster.VoucherDate;
                            voucherMaster.VoucherEffectiveDate = VM.VoucherMaster.VoucherEffectiveDate;
                            voucherMaster.VoucherNarration = VM.VoucherMaster.VoucherNarration;
                            voucherMaster.BillRemarks = VM.VoucherMaster.BillRemarks;

                            // Delete old entries
                            var existingEntries = await dbContext.Tbl201VoucherEntries
                                .Where(e => e.VoucherNo == VM.VoucherMaster.VoucherNo)
                                .ToListAsync();

                            dbContext.Tbl201VoucherEntries.RemoveRange(existingEntries);
                            await dbContext.SaveChangesAsync();
                            await _userActionLogger.LogAsync(
module: "Finance > Voucher Journal",
actionDetail: $"Added Voucher: {VM.VoucherEntries[0].VoucherNo}, Entries: {VM.VoucherEntries.Count}",
documentNo: VM.VoucherEntries[0].VoucherNo
);
                            // Add new entries
                            var newEntries = new List<Tbl201VoucherEntry>();
                            foreach (var entry in VM.VoucherEntries)
                            {
                                if (string.IsNullOrEmpty(entry.AccountHead))
                                {
                                    throw new Exception("AccountHead cannot be null or empty.");
                                }

                                var accountId = await dbContext.Tbl201ChartOfAccounts
                                    .Where(a => a.AccountHead == entry.AccountHead)
                                    .Select(a => a.AccountId)
                                    .FirstOrDefaultAsync();

                                newEntries.Add(new Tbl201VoucherEntry
                                {
                                    VoucherNo = VM.VoucherMaster.VoucherNo,
                                    DrCr = entry.DrCr,
                                    VoucherAmount = entry.VoucherAmount,
                                    EntryNarration = entry.EntryNarration,
                                    AccountHead = accountId,
                                    SysRemarks = entry.SysRemarks
                                });
                            }

                            await dbContext.Tbl201VoucherEntries.AddRangeAsync(newEntries);
                            await dbContext.SaveChangesAsync();
                            await _userActionLogger.LogAsync(
module: "Finance > Voucher Journal",
actionDetail: $"Added Voucher: {VM.VoucherEntries[0].VoucherNo}, Entries: {VM.VoucherEntries.Count}",
documentNo: VM.VoucherEntries[0].VoucherNo
);
                            await transaction.CommitAsync();
                        }
                    });

                    var UserId = HttpContext.Session.GetString("UserId");
                    var TenantName = HttpContext.Session.GetString("TenantName");

                    var notifyRequest = new NotificationRequest
                    {
                        UserId = UserId, // or fetch from session/DB
                        VoucherName = VM.VoucherMaster.VoucherNo,
                        ActionType = "You have one Journal Voucher to verify",
                        TenantName = TenantName
                    };

                    await _fcmService.SendNotificationAsync(notifyRequest);

                    return Ok(new { success = true, message = "Voucher updated successfully!" });
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
                      AccountId = i.AccountHead,
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
                    i.VoucherApprovedOn,
                    i.currencyid,
                    i.currencyrate,
                    i.basecurrencyid
                });

                return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));

            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }




        [HttpPost]
        public IActionResult DeleteAllVoucherEntry(string VoucherNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                if (string.IsNullOrEmpty(VoucherNo))
                {
                    return BadRequest("Invalid parameters. VoucherNo is required.");
                }

                try
                {
                    // Use EF execution strategy for retryable operations
                    var strategy = dbContext.Database.CreateExecutionStrategy();

                    strategy.Execute(async () =>
                    {
                        using (var transaction = dbContext.Database.BeginTransaction())
                        {
                            // 1. Delete Temp
                            var tempEntries = dbContext.Tbl201VoucherEntryTemps
                                                       .Where(e => e.VoucherNo == VoucherNo)
                                                       .ToList();
                            if (tempEntries.Any())
                            {
                                dbContext.Tbl201VoucherEntryTemps.RemoveRange(tempEntries);
                                dbContext.SaveChanges();
                            }

                            // 2. Delete Entries
                            var entryRecords = dbContext.Tbl201VoucherEntries
                                                        .Where(e => e.VoucherNo == VoucherNo)
                                                        .ToList();
                            if (entryRecords.Any())
                            {
                                dbContext.Tbl201VoucherEntries.RemoveRange(entryRecords);
                                dbContext.SaveChanges();
                               
                            }

                            // 3. Delete Masters
                            var masterEntries = dbContext.Tbl201VoucherMasters
                                                         .Where(m => m.VoucherNo == VoucherNo)
                                                         .ToList();
                            if (masterEntries.Any())
                            {
                                dbContext.Tbl201VoucherMasters.RemoveRange(masterEntries);
                                dbContext.SaveChanges();
                                await _userActionLogger.LogAsync(
module: "Finance > Voucher Journal",
actionDetail: $"Deleted Voucher : {VoucherNo}",
documentNo: VoucherNo
);
                            }

                            // 4. Delete Cost Allocation
                            var costEntries = dbContext.Tbl201CostAllocationMasters
                                                       .Where(m => m.VoucherNo == VoucherNo)
                                                       .ToList();
                            if (costEntries.Any())
                            {
                                dbContext.Tbl201CostAllocationMasters.RemoveRange(costEntries);
                                dbContext.SaveChanges();
                            }

                            // 5. Delete Property
                            var propertyEntries = dbContext.Tbl20122PropertyAllocationMasters
                                                           .Where(m => m.VoucherNo == VoucherNo)
                                                           .ToList();
                            if (propertyEntries.Any())
                            {
                                dbContext.Tbl20122PropertyAllocationMasters.RemoveRange(propertyEntries);
                                dbContext.SaveChanges();
                            }

                            // 6. Delete Employee
                            var employeeEntries = dbContext.Tbl20104EmployeeAllocationMasters
                                                           .Where(m => m.VoucherNo == VoucherNo)
                                                           .ToList();
                            if (employeeEntries.Any())
                            {
                                dbContext.Tbl20104EmployeeAllocationMasters.RemoveRange(employeeEntries);
                                dbContext.SaveChanges();
                            }
                
                            transaction.Commit();
                        }
                    });

                    // Fetch updated data after deletion
                    var updatedData = dbContext.Tbl201VoucherMasters.ToList();
                    return Ok(new { data = updatedData, message = "Voucher deleted successfully" });
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
                    // Find all records matching the given VoucherNo
                    var record = await dbContext.Tbl201VoucherEntryTemps
                                                .Where(v => v.VoucherNo == VoucherNo)
                                                .ToListAsync();

                    if (records == null || !records.Any())
                    {
                        return NotFound(new { message = "No records found for the provided VoucherNo!" });
                    }
                    if (records != null)
                    {
                        dbContext.Tbl201VoucherEntries.RemoveRange(records);
                    }
                    if (record != null)
                    {
                        dbContext.Tbl201VoucherEntryTemps.RemoveRange(record);
                    }
                    // Remove all matching records

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
