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
        public IActionResult SaveJournalRegisterChild([FromBody] Tbl20127JournalRegisterChild newEntry)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                if (ModelState.IsValid)
            {
                    // Save to DB
                    dbContext.Tbl20127JournalRegisterChildren.Add(newEntry);
                    dbContext.SaveChanges();
                return Ok();
            }
            return BadRequest(ModelState);
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }


        [HttpGet]
        public IActionResult GetJournalRegisterChildren(string voucherNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var records = (from jr in dbContext.Tbl20127JournalRegisterChildren
                               join acc in dbContext.Tbl201ChartOfAccounts
                               on jr.AccountId equals acc.AccountId into accJoin
                               from acc in accJoin.DefaultIfEmpty()
                               where jr.JournalRefNo == voucherNo
                               select new
                               {
                                   jr.LineOrderNo,
                                   jr.AccountId,
                                   AccountHead = acc != null ? acc.AccountHead : "",
                                   jr.EntryNarration,
                                   jr.DrCr,
                                   jr.DrAmount,
                                   jr.CrAmount
                               }).ToList();

                return Json(records);
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }


        [HttpGet]
        public IActionResult GetNextLineOrderNo(string voucherNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var count = dbContext.Tbl20127JournalRegisterChildren
                .Count(j => j.JournalRefNo == voucherNo);

            return Ok(count + 1); // Next LineOrderNo
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpPost]
        public async Task<IActionResult> UpdateVoucher([FromBody] JournalRegisterViewModel model)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            if (model == null || model.JournalDetails == null || !model.JournalDetails.Any())
            {
                return BadRequest(new { success = false, message = "No data received" });
            }

            try
            {
                // Get user session data
                string userIdStr = HttpContext.Session.GetString("UserId");
                byte claimerId = Convert.ToByte(userIdStr); // ✅ Convert string to byte

                string userName = HttpContext.Session.GetString("UserName");
                DateTime now = DateTime.Now;
                // Check if master record exists
                var existingMaster = await dbContext.Tbl20126JournalRegisterMasters
                    .FirstOrDefaultAsync(m => m.JournalRefNo == model.JournalRefNo);

                if (existingMaster != null)
                {
                    // ✅ Update master record
                    existingMaster.JournalEntryDate = model.JournalEntryDate;
                    existingMaster.JournalEffectiveDate = model.JournalEffectiveDate;
                    existingMaster.JournalVoucherNarration = model.JournalVoucherNarration;
                    existingMaster.JournalModifiedBy = userName;
                    existingMaster.JournalModifiedOn = now;
                    dbContext.Tbl20126JournalRegisterMasters.Update(existingMaster);
                }
                else
                {
                    // ✅ Insert new master
                    var newMaster = new Tbl20126JournalRegisterMaster
                    {
                        JournalRefNo = model.JournalRefNo,
                        JournalEntryDate = model.JournalEntryDate,
                        JournalEffectiveDate = model.JournalEffectiveDate,
                        JournalVoucherNarration = model.JournalVoucherNarration,
                        RequesterId = claimerId,
                        JournalCreatedBy = userName,
                        JournalCreatedOn = now
                    };

                    dbContext.Tbl20126JournalRegisterMasters.Add(newMaster);
                }

                // ✅ Remove existing child rows for this ClaimRefNo
                var existingChildren = dbContext.Tbl20127JournalRegisterChildren
                    .Where(c => c.JournalRefNo == model.JournalRefNo);

                dbContext.Tbl20127JournalRegisterChildren.RemoveRange(existingChildren);

                // ✅ Add new child rows
                foreach (var item in model.JournalDetails)
                {
                    var child = new Tbl20127JournalRegisterChild
                    {
                        JournalRefNo = model.JournalRefNo,
                        LineOrderNo = item.LineOrderNo,
                        DrCr = item.DrCr,
                        DrAmount = item.DrAmount,
                        CrAmount = item.CrAmount,
                        EntryNarration = item.EntryNarration,
                        AccountId = item.AccountId,
                        
                    };

                    dbContext.Tbl20127JournalRegisterChildren.Add(child);
                }

                await dbContext.SaveChangesAsync();

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        public IActionResult CostAllocation(string voucherNo, string accountHead, string voucherAmount, string drCr, long voucherEntryNo)
        {
            // Log or debug the incoming parameters
            ViewBag.VoucherNo = voucherNo;
            ViewBag.AccountHead = accountHead;
            ViewBag.VoucherAmount = voucherAmount;
            ViewBag.DrCr = drCr;
            ViewBag.VoucherEntryNo = voucherEntryNo;
            return PartialView("~/Areas/Finance/Views/_JournalEntryCostAllocation.cshtml"); // Ensure this is inside /Views/VoucherEntryReceipts/
        }
        [HttpGet]
        public IActionResult PropertyAllocation(string voucherNo, string accountHead, string voucherAmount, string drCr, long voucherEntryNo, string accountId)
        {
            // Log or debug the incoming parameters
            ViewBag.VoucherNo = voucherNo;
            ViewBag.AccountHead = accountHead;
            ViewBag.VoucherAmount = voucherAmount;
            ViewBag.DrCr = drCr;
            ViewBag.VoucherEntryNo = voucherEntryNo;
            ViewBag.AccountID = accountId;
            return PartialView("~/Areas/Finance/Views/_JournalEntryPropetyAllocation.cshtml"); // Ensure this is inside /Views/VoucherEntryReceipts/
        }
        [HttpGet]
        public IActionResult EmployeeAllocation(string voucherNo, string accountHead, string voucherAmount, string drCr, long voucherEntryNo, string accountId)
        {
            // Log or debug the incoming parameters
            ViewBag.VoucherNo = voucherNo;
            ViewBag.AccountHead = accountHead;
            ViewBag.VoucherAmount = voucherAmount;
            ViewBag.DrCr = drCr;
            ViewBag.VoucherEntryNo = voucherEntryNo;
            ViewBag.AccountID = accountId;
            return PartialView("~/Areas/Finance/Views/_JournalEntryEmployeeAllocation.cshtml"); // Ensure this is inside /Views/VoucherEntryReceipts/
        }
    }
}



