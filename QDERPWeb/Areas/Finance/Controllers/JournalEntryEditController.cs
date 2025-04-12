using DevExpress.XtraRichEdit.Import.Html;
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

        [HttpPost]
        public async Task<ActionResult> UpdateVoucher([FromBody] RegisterVoucherViewModel VM)
        {
            if (VM == null || VM.JournalVoucherMaster == null || string.IsNullOrEmpty(VM.JournalVoucherMaster.JournalRefNo) || VM.JournalVoucherEntries == null || !VM.JournalVoucherEntries.Any())
            {
                return BadRequest(new { success = false, message = "Invalid data received or missing journal reference number." });
            }

            try
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    // ✅ Find existing Journal Master
                    var journalMaster = await _context.Tbl20126JournalRegisterMasters
                        .FirstOrDefaultAsync(j => j.JournalRefNo == VM.JournalVoucherMaster.JournalRefNo);

                    if (journalMaster == null)
                    {
                        return NotFound(new { success = false, message = "Journal master not found." });
                    }

                    // ✅ Update Master Record
                    journalMaster.JournalEntryDate = VM.JournalVoucherMaster.JournalEntryDate;
                    journalMaster.JournalEffectiveDate = VM.JournalVoucherMaster.JournalEffectiveDate;
                    journalMaster.JournalVoucherNarration = VM.JournalVoucherMaster.JournalVoucherNarration;
                    // Update additional fields if needed

                    // ✅ Delete Old Entries (children)
                    var existingEntries = await _context.Tbl20127JournalRegisterChildren
                        .Where(e => e.JournalRefNo == VM.JournalVoucherMaster.JournalRefNo)
                        .ToListAsync();

                    _context.Tbl20127JournalRegisterChildren.RemoveRange(existingEntries);
                    await _context.SaveChangesAsync();

                    // ✅ Add new entries
                    var newEntries = new List<Tbl20127JournalRegisterChild>();

                    foreach (var entry in VM.JournalVoucherEntries)
                    {
                       


                        newEntries.Add(new Tbl20127JournalRegisterChild
                        {
                            JournalRefNo = VM.JournalVoucherMaster.JournalRefNo,
                            DrCr = entry.DrCr,
                            DrAmount=entry.DrAmount,
                            CrAmount=entry.CrAmount,
                            EntryNarration = entry.EntryNarration,
                            AccountId=entry.AccountId
                        });
                    }

                    await _context.Tbl20127JournalRegisterChildren.AddRangeAsync(newEntries);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(new { success = true, message = "Journal voucher updated successfully!" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }
        }


        [HttpPost]

        [HttpPost]
        public IActionResult DeleteJournalEntry([FromBody] string journalRefNo)
        {
            try
            {
                var userIdString = HttpContext.Session.GetString("UserId");
                var userName = HttpContext.Session.GetString("UserName") ?? "Unknown User";

                if (string.IsNullOrEmpty(userIdString))
                    return Unauthorized("User session expired. Please log in again.");

                int userId = int.Parse(userIdString);

                //// 🔒 Check Delete Access
                //var access = _context.TblUserAccesses
                //    .FirstOrDefault(x => x.UserId == userId && x.ItemName == "btnDelete" && x.ItemVisible == true);

                //if (access == null)
                //    return BadRequest("You have no Access rights to delete this Journal Entry.");

                // 🔍 Find journal master

                // 🔍 Find journal master
                var master = _context.Tbl20126JournalRegisterMasters
                    .FirstOrDefault(x => x.JournalRefNo == journalRefNo);

                if (master == null)
                    return NotFound("Journal Entry not found.");

                if (master.IsSubmittedToFinance == true)
                    return BadRequest("Journal Entry is already submitted. You cannot delete it.");

                if (master.IsApproved == true)
                    return BadRequest("Journal Entry is already approved. You cannot delete it.");

                if (master.IsPosted == true)
                    return BadRequest("Journal Entry is already posted. You cannot delete it.");

                // 🗑️ Delete related child entries
                _context.Tbl20127JournalRegisterChildren.RemoveRange(
                    _context.Tbl20127JournalRegisterChildren.Where(x => x.JournalRefNo == journalRefNo));

                _context.Tbl20128JournalRegisterCostAllocations.RemoveRange(
                    _context.Tbl20128JournalRegisterCostAllocations.Where(x => x.VoucherNo == journalRefNo));

                _context.Tbl20129JournalRegisterEmployeeAllocations.RemoveRange(
                    _context.Tbl20129JournalRegisterEmployeeAllocations.Where(x => x.VoucherNo == journalRefNo));

                _context.Tbl20130JournalRegisterPropertyAllocations.RemoveRange(
                    _context.Tbl20130JournalRegisterPropertyAllocations.Where(x => x.VoucherNo == journalRefNo));

                _context.Tbl20126JournalRegisterMasters.Remove(master);

                // 📝 Log the delete action
                _context.Database.ExecuteSqlRaw(
                    "EXEC sp90116InsertUserLogEntry @p0, @p1, @p2, @p3",
                    "Journal Entry Form",
                    $"Journal Entry Ref No. {journalRefNo} has been deleted.",
                    userName,
                    journalRefNo
                );

                _context.SaveChanges();

                return Ok(new { message = "Journal Entry Form has been successfully removed from the Register." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting journal entry with RefNo: {JournalRefNo}", journalRefNo);
                return StatusCode(500, "An unexpected error occurred while trying to delete the Journal Entry.");
            }
        }




    }
}



