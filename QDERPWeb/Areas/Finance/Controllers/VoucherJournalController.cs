using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.DAL.Entities;

using Microsoft.EntityFrameworkCore;
using DevExpress.Emf;
//using SkiaSharp;


namespace QDWEB.Areas.Finance.Controllers
{//[Area("Finance")]
    [Route("api/[controller]/[action]")]
    // [Route("Finapi/[controller]/[action]")]
    [ApiController]
    public class VoucherJournalController : Controller
    {
        private ERPMasterWtDataContext _context;

        public VoucherJournalController(ERPMasterWtDataContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult> GetNewVoucherNo()
        {
            string voucherPrefix = "JV-NEW-";
            string strNewVoucherNo;

            try
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    string sql = @"
                SELECT MAX(CAST(RIGHT(TempVoucherNo, 6) AS INT)) AS MaxVoucherNo
                FROM tbl201VoucherMasterTemp WITH (TABLOCKX)
                WHERE TempVoucherNo LIKE {0}";

                    // Fix: Use FromSqlRaw instead of SqlQueryAsync
                    var result = await _context.VoucherResults
                        .FromSqlRaw(sql, voucherPrefix + "%")
                        .ToListAsync();

                    int maxVoucherNo = result.FirstOrDefault()?.MaxVoucherNo ?? 0;

                    int newVoucherNo = maxVoucherNo + 1;
                    strNewVoucherNo = voucherPrefix + newVoucherNo.ToString("D6");

                    var newVoucherEntry = new Tbl201VoucherMasterTemp
                    {
                        TempVoucherNo = strNewVoucherNo
                    };

                    _context.Tbl201VoucherMasterTemps.Add(newVoucherEntry);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }

            return Json(strNewVoucherNo);
        }



        [HttpGet]
        public IActionResult Delete(long VoucherEntryNo)
        {
            var item = _context.Tbl201VoucherEntryTemps.Where(p => p.VoucherEntryNo == VoucherEntryNo).FirstOrDefault();
            if (item != null)
            {
                _context.Tbl201VoucherEntryTemps.Remove(item);
                _context.SaveChanges();
                return Ok(new { success = true });
            }
            return NotFound();
        }


        [HttpGet]
        public async Task<ActionResult> GetSupplierName(DataSourceLoadOptions loadOptions)
        {
            var qryListOfAccountlists = _context.Qry201ListOfAccounts.Select(i => new
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
        public async Task<ActionResult> GetVoucherEntries(DataSourceLoadOptions loadOptions, string voucherNo)
        {
            var qryListOfAccountlists = _context.Tbl201VoucherEntryTemps.Where(p => p.VoucherNo == voucherNo).Select(i => new
            {
                i.VoucherNo,
                i.DrCr,
                i.VoucherAmount,
                i.EntryNarration,
                i.AccountHead,
                i.SysRemarks,
            });


            return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
        }
        [HttpPost]
        public async Task<ActionResult> AddVoucherEntry(DataSourceLoadOptions loadOptions, [FromBody] Tbl201VoucherEntryTemp VE)
        {
            if (VE == null)
            {
                return BadRequest(new { success = false, message = "Invalid data received." });
            }

            try
            {
                // Add the new voucher entry to the table
                _context.Tbl201VoucherEntryTemps.Add(VE);
                await _context.SaveChangesAsync();

                // Fetch voucher entries and join with account names
                var qryListOfAccountlists = await _context.Tbl201VoucherEntryTemps
                    .Where(p => p.VoucherNo == VE.VoucherNo) // Fix comparison operator
                    .OrderBy(i => i.DrCr == "Dr" ? 0 : 1) // "Dr" entries first
                    .Select(i => new VoucherEntryDisplayDTO
                    {
                        VoucherNo = i.VoucherNo,
                        VoucherEntryNo = i.VoucherEntryNo,
                        DrCr = i.DrCr,
                        VoucherAmount = i.VoucherAmount,
                        EntryNarration = i.EntryNarration,
                        AccountHead = _context.Qry201ListOfAccounts
                                              .Where(a => a.AccountId == i.AccountHead)
                                              .Select(a => a.AccountHead)
                                              .FirstOrDefault(), // Get AccountHead from ChartOfAccounts
                        SysRemarks = i.SysRemarks
                    })
                    .ToListAsync(); // Async execution

                return Json(DataSourceLoader.Load(qryListOfAccountlists.AsQueryable(), loadOptions));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult> SaveVoucher([FromBody] VoucherViewModel VM)
        {
            if (VM == null || VM.VoucherEntries == null || !VM.VoucherEntries.Any())
            {
                return BadRequest(new { success = false, message = "Invalid data received." });
            }

            try
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    string currentDate = DateTime.Now.ToString("dd");
                    string currentMonth = DateTime.Now.ToString("MM");

                    // Get last used VoucherNo for the same date
                    var lastVoucher = await _context.Tbl201VoucherEntries
                        .Where(v => v.VoucherNo.StartsWith($"JV-{currentDate}-{currentMonth}-"))
                        .OrderByDescending(v => v.VoucherNo)
                        .FirstOrDefaultAsync();

                    int nextSequence = 1;
                    if (lastVoucher != null)
                    {
                        string lastNumberPart = lastVoucher.VoucherNo.Substring(9);
                        if (int.TryParse(lastNumberPart, out int lastNumber))
                        {
                            nextSequence = lastNumber + 1;
                        }
                    }

                    string newVoucherNo = $"JV-{currentDate}-{currentMonth}-{nextSequence:D3}";

                    while (await _context.Tbl201VoucherEntries.AnyAsync(v => v.VoucherNo == newVoucherNo))
                    {
                        nextSequence++;
                        newVoucherNo = $"JV-{currentDate}-{currentMonth}-{nextSequence:D3}";
                    }

                    // Assign `VoucherNo` and fetch `AccountId`
                    foreach (var entry in VM.VoucherEntries)
                    {
                        entry.VoucherNo = newVoucherNo;
                        entry.VoucherEntryNo = 0;

                        // Fetch AccountId based on AccountHead
                        entry.AccountHead = await _context.Tbl201ChartOfAccounts
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
                        VoucherApprovedOn = DateTime.Now
                    };

                    _context.Tbl201VoucherMasters.Add(voucherMaster);
                    await _context.Tbl201VoucherEntries.AddRangeAsync(VM.VoucherEntries);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(new { success = true, message = "Voucher saved successfully!", voucherNo = newVoucherNo });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }
        }





        [HttpPost]
        public async Task<ActionResult> UpdateVoucher([FromBody] VoucherViewModel VM)
        {
            // **Validate Incoming Data**
            if (VM == null || VM.VoucherMaster == null || string.IsNullOrEmpty(VM.VoucherMaster.VoucherNo) || VM.VoucherEntries == null || !VM.VoucherEntries.Any())
            {
                return BadRequest(new { success = false, message = "Invalid data received or missing voucher number." });
            }

            try
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    // **Find Existing Voucher Master**
                    var voucherMaster = await _context.Tbl201VoucherMasters
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
                    var existingEntries = await _context.Tbl201VoucherEntries
                        .Where(e => e.VoucherNo == VM.VoucherMaster.VoucherNo)
                        .ToListAsync();

                    _context.Tbl201VoucherEntries.RemoveRange(existingEntries);
                    await _context.SaveChangesAsync(); // Ensure old records are removed first

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
                        var accountID = await _context.Tbl201ChartOfAccounts
                            .Where(a => a.AccountHead == entry.AccountHead)
                            .Select(a => a.AccountId) // Convert to nullable int to avoid null exceptions
                            .FirstOrDefaultAsync();

                        if (accountID == null)
                        {
                            return BadRequest(new { success = false, message = $"AccountHead '{entry.AccountHead}' not found in ChartOfAccounts." });
                        }

                        // **Create New Entry**
                        newEntries.Add(new Tbl201VoucherEntry
                        {
                            VoucherNo = VM.VoucherMaster.VoucherNo,
                            AccountHead = accountID.ToString(), // Assign AccountID as a string
                            DrCr = entry.DrCr,
                            VoucherAmount = entry.VoucherAmount,
                            EntryNarration = entry.EntryNarration
                        });
                    }

                    // **Insert New Entries**
                    await _context.Tbl201VoucherEntries.AddRangeAsync(newEntries);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(new { success = true, message = "Voucher updated successfully!" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }
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
            try
            {
                if (string.IsNullOrEmpty(voucherNo))
                {
                    return BadRequest(new { success = false, message = "Invalid Voucher Number." });
                }

                var qryListOfAccountlists = _context.Tbl201VoucherEntries
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
                    string accountHead = _context.Qry201ListOfAccounts
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

        [HttpGet]
        public async Task<ActionResult> GetVoucherMasterEntries(DataSourceLoadOptions loadOptions, string voucherNo)
        {
            var qryListOfAccountlists = _context.Tbl201VoucherMasters.Where(p => p.VoucherNo == voucherNo).Select(i => new
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




        [HttpPost]
       
        public IActionResult DeleteAllVoucherEntry(string VoucherNo, string TemporaryNo)
        {
            if (string.IsNullOrEmpty(VoucherNo) || string.IsNullOrEmpty(TemporaryNo))
            {
                return BadRequest("Invalid parameters. Both VoucherNo and TemporaryNo are required.");
            }

            try
            {
                using (var transaction = _context.Database.BeginTransaction()) // Start transaction
                {
                    // 1. Delete from tbl201VoucherEntryTemp where VoucherNo = TemporaryNo
                    var tempEntries = _context.Tbl201VoucherEntryTemps
                                                .Where(e => e.VoucherNo == TemporaryNo)
                                                .ToList();
                    if (tempEntries.Any())
                    {
                        _context.Tbl201VoucherEntryTemps.RemoveRange(tempEntries);
                        _context.SaveChanges();
                    }

                    // 2. Delete from tbl201VoucherEntry where VoucherNo = VoucherNo
                    var entryRecords = _context.Tbl201VoucherEntries
                                                 .Where(e => e.VoucherNo == VoucherNo)
                                                 .ToList();
                    if (entryRecords.Any())
                    {
                        _context.Tbl201VoucherEntries.RemoveRange(entryRecords);
                        _context.SaveChanges();
                    }

                    // 3. Delete from tbl201VoucherMaster where VoucherNo = VoucherNo
                    var masterEntries = _context.Tbl201VoucherMasters
                                                  .Where(m => m.VoucherNo == VoucherNo)
                                                  .ToList();
                    if (masterEntries.Any())
                    {
                        _context.Tbl201VoucherMasters.RemoveRange(masterEntries);
                        _context.SaveChanges();
                    }

                    transaction.Commit(); // Commit only if all deletions succeed

                    // Fetch updated data after deletion
                    var updatedData = _context.Tbl201VoucherMasters.ToList();

                    return Ok(new { data = updatedData, message = "Voucher deleted successfully" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting voucher: {ex.Message}");
            }
        }


    }
}
