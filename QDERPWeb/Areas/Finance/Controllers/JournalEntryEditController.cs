using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using SkiaSharp;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class JournalEntryEditController : Controller
    {
        private ERPMasterWtDataContext _context;
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<JournalEntryEditController> _logger;

        public JournalEntryEditController(ILogger<JournalEntryEditController> logger, TenantDbContextHelper tenantDbContextHelper, ERPMasterWtDataContext context)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> GetNewVoucherNo(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                DateTime currentDate = DateTime.Now;
                string currentYear = currentDate.Year.ToString();
                string voucherPrefix = $"JV-REQ-{currentYear}-";
                string strNewVoucherNo;

                string likePattern = voucherPrefix + "%";

                try
                {
                    var result = await dbContext.VoucherResults
                        .FromSqlInterpolated($@"
                    SELECT MAX(CAST(RIGHT(JournalRefNo, 5) AS INT)) AS MaxVoucherNo
                    FROM tbl20126JournalRegisterMaster
                    WHERE JournalRefNo LIKE {likePattern}")
                        .ToListAsync();

                    int maxVoucherNo = result.FirstOrDefault()?.MaxVoucherNo ?? 0;
                    int newVoucherNo = maxVoucherNo + 1;
                    strNewVoucherNo = voucherPrefix + newVoucherNo.ToString("D5");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetNewVoucherNo: {ex.Message}");
                    strNewVoucherNo = voucherPrefix + "00001";
                }

                return Json(strNewVoucherNo);
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }


        [HttpPost]
        public async Task<ActionResult> Save([FromBody] Tbl20126JournalRegisterMaster VM)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                if (VM == null)
                {
                    return BadRequest(new { success = false, message = "Invalid data received." });
                }

                try
                {
                    dbContext.Tbl20126JournalRegisterMasters.Add(VM);
                    await dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Data inserted successfully!" });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in Save: {ex.Message}");
                    return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public IActionResult GetJournalRegisterChild(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var data = dbContext.Qry202101journalRegisterChildren
                    .Select(x => new
                    {
                        x.JournalChildNo,
                        x.AccountId,
                        x.AccountHead,
                        x.DrCr,
                        x.EntryNarration,
                        x.DrAmount,
                        x.CrAmount,
                        x.CostAllocationDescription,
                        x.EmployeeCostDescription,
                        x.PropertyCostDescription,
                        x.FormattedAmount,
                        x.TotalCostAllocated,
                        x.TotalEmpAllocated,
                        x.TotalEqpAllocted
                    });

                return Json(DataSourceLoader.Load(data, loadOptions));
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public IActionResult LoadLedgerData(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var data = dbContext.Qry20172LedgersForClaims
                    .Select(x => new
                    {
                        AccountID = x.AccountId,
                        AccountHead = x.AccountHead
                    }).ToList();

                return Json(DataSourceLoader.Load(data, loadOptions));
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public IActionResult GetCostAllocationUnits()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var data = dbContext.Qry20172LedgersForClaims
                    .Select(c => new
                    {
                        c.AccountId,
                        c.AccountHead,
                        c.ReferenceNo,
                        c.AccountGroup,
                        c.AccountHeadArabic,
                        c.IsLedgerObselete
                    }).ToList();

                return Ok(data);
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpPost]
        public async Task<ActionResult> SaveVoucher([FromBody] RegisterVoucherViewModel VM)
        {
            //if (VM == null || VM.JournalVoucherMaster == null)
            if (VM == null || VM.JournalVoucherEntries == null || VM.JournalVoucherEntries.Count == 0)
            {
                return BadRequest(new { success = false, message = "Invalid data received." });
            }

            try
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    string journalRefNo = VM.JournalVoucherMaster.JournalRefNo;

                    if (string.IsNullOrWhiteSpace(journalRefNo))
                    {
                        return BadRequest(new { success = false, message = "JournalRefNo is required." });
                    }

                   // Map AccountHead to AccountId for each entry
                    foreach (var entry in VM.JournalVoucherEntries)
                        {
                        entry.JournalRefNo = journalRefNo;
                            //entry.AccountHead = await _context.Tbl201ChartOfAccounts
                            //    .Where(a => a.AccountHead == entry.AccountHead)
                            //    .Select(a => a.AccountId)
                            //    .FirstOrDefaultAsync();
                        }

                    // Insert into Tbl20126JournalRegisterMaster
                    var journalMaster = new Tbl20126JournalRegisterMaster
                    {
                        JournalRefNo = journalRefNo,
                        JournalEntryDate = VM.JournalVoucherMaster.JournalEntryDate,
                        JournalEffectiveDate = VM.JournalVoucherMaster.JournalEffectiveDate,
                        JournalVoucherNarration = VM.JournalVoucherMaster.JournalVoucherNarration,
                        //BillRemarks = VM.JournalVoucherMaster.BillRemarks,
                        //JournalType = VM.JournalVoucherMaster.VoucherType,
                        //IsVerified = VM.JournalVoucherMaster.IsVerified,
                        //IsApproved = VM.JournalVoucherMaster.IsApproved,
                        //VoucherVerifiedBy = VM.JournalVoucherMaster.VoucherVerifiedBy,
                        //VoucherApprovedBy = VM.JournalVoucherMaster.VoucherApprovedBy,
                        //VoucherVerifiedOn = DateTime.Now,
                        //VoucherApprovedOn = DateTime.Now
                    };

                    _context.Tbl20126JournalRegisterMasters.Add(journalMaster);
                    

                    //Save voucher entries directly to qry202_101JournalRegisterChild
                    await _context.Tbl20127JournalRegisterChildren.AddRangeAsync(VM.JournalVoucherEntries);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(new { success = true, message = "Journal entry saved successfully!", journalRefNo });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

      //  [HttpPost]
        //public async Task<ActionResult> UpdateVoucher([FromBody] RegisterVoucherViewModel VM)
        //{
        //    if (VM == null || VM.JournalVoucherMaster == null || string.IsNullOrEmpty(VM.JournalVoucherMaster.JournalRefNo) || VM.JournalVoucherEntries == null || !VM.JournalVoucherEntries.Any())
        //    {
        //        return BadRequest(new { success = false, message = "Invalid data received or missing journal reference number." });
        //    }

        //    try
        //    {
        //        using (var transaction = await _context.Database.BeginTransactionAsync())
        //        {
        //            // ✅ Find existing Journal Master
        //            var journalMaster = await _context.Tbl20126JournalRegisterMasters
        //                .FirstOrDefaultAsync(j => j.JournalRefNo == VM.JournalVoucherMaster.JournalRefNo);

        //            if (journalMaster == null)
        //            {
        //                return NotFound(new { success = false, message = "Journal master not found." });
        //            }

        //            // ✅ Update Master Record
        //            journalMaster.JournalEntryDate = VM.JournalVoucherMaster.JournalEntryDate;
        //            journalMaster.JournalEffectiveDate = VM.JournalVoucherMaster.JournalEffectiveDate;
        //            journalMaster.JournalVoucherNarration = VM.JournalVoucherMaster.JournalVoucherNarration;
        //            // Update additional fields if needed

        //            // ✅ Delete Old Entries (children)
        //            var existingEntries = await _context.Qry202101journalRegisterChildren
        //                .Where(e => e.JournalRefNo == VM.JournalVoucherMaster.JournalRefNo)
        //                .ToListAsync();

        //            _context.Qry202101journalRegisterChildren.RemoveRange(existingEntries);
        //            await _context.SaveChangesAsync();

        //            // ✅ Add new entries
        //            var newEntries = new List<Qry202101journalRegisterChild>();

        //            foreach (var entry in VM.JournalVoucherEntries)
        //            {
        //                if (string.IsNullOrEmpty(entry.AccountHead))
        //                {
        //                    return BadRequest(new { success = false, message = "AccountHead is required." });
        //                }

        //                var accountId = await _context.Tbl201ChartOfAccounts
        //                    .Where(a => a.AccountHead == entry.AccountHead)
        //                    .Select(a => a.AccountId)
        //                    .FirstOrDefaultAsync();

        //                //if (accountId == 0)
        //                //{
        //                //    return BadRequest(new { success = false, message = $"AccountHead '{entry.AccountHead}' not found." });
        //                //}

        //                newEntries.Add(new Qry202101journalRegisterChild
        //                {
        //                    JournalRefNo = VM.JournalVoucherMaster.JournalRefNo,
        //                    DrCr = entry.DrCr,
        //                   // JournalAmount = entry.JournalAmount,
        //                    EntryNarration = entry.EntryNarration,
        //                    AccountHead = accountId
        //                });
        //            }

        //            await _context.Qry202101journalRegisterChildren.AddRangeAsync(newEntries);
        //            await _context.SaveChangesAsync();
        //            await transaction.CommitAsync();

        //            return Ok(new { success = true, message = "Journal voucher updated successfully!" });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
        //    }
        //}

    }
}



