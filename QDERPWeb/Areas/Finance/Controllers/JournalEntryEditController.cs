using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.XtraRichEdit.Import.Html;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using QD.ERP.Web.Services.Logging;
using SkiaSharp;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class JournalEntryEditController : Controller
    {
        private ERPMasterWtDataContext _context;
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<JournalEntryEditController> _logger;
        private readonly IUserActionLogger _userActionLogger;
        public JournalEntryEditController(ILogger<JournalEntryEditController> logger, IUserActionLogger userActionLogger, TenantDbContextHelper tenantDbContextHelper, ERPMasterWtDataContext context)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
            _context = context;
            _userActionLogger = userActionLogger;
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
                var records = (from jr in dbContext.Qry202101journalRegisterChildren
                               join acc in dbContext.Tbl201ChartOfAccounts
                               on jr.AccountId equals acc.AccountId into accJoin
                               from acc in accJoin.DefaultIfEmpty()
                               where jr.JournalRefNo == voucherNo
                               select new
                               {
                                   jr.LineOrderNo,
                                   jr.AccountId,
                                   jr.AccountHead,
                                   jr.EntryNarration,
                                   jr.DrCr,
                                   jr.DrAmount,
                                   jr.CrAmount,
                                   jr.JournalChildNo,
                                   jr.CostAllocationDescription,
                                   jr.EmployeeCostDescription,
                                   jr.PropertyCostDescription,
                                   jr.TotalCostAllocated,
                                   jr.TotalEmpAllocated,
                                   jr.TotalEqpAllocted,

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

            if (model == null)
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

                foreach (var item in model.JournalDetails)
                {
                    var existingChild = await dbContext.Tbl20127JournalRegisterChildren
                        .FirstOrDefaultAsync(c =>
                            c.JournalRefNo == model.JournalRefNo &&
                            c.JournalChildNo == item.JournalChildNo);

                    if (existingChild != null)
                    {
                        // ✅ Only update the desired 4 columns
                        existingChild.JournalChildNo = item.JournalChildNo;
                        existingChild.JournalRefNo = model.JournalRefNo;
                        existingChild.AccountId = item.AccountId;
                        existingChild.DrCr = item.DrCr;
                        existingChild.DrAmount = item.DrAmount;
                        existingChild.CrAmount = item.CrAmount;
                        existingChild.EntryNarration = item.EntryNarration;


                        dbContext.Tbl20127JournalRegisterChildren.Update(existingChild);
                    }
                }


                await dbContext.SaveChangesAsync();
                await _userActionLogger.LogAsync(
        module: "Finance > Journal Register",
        actionDetail: $"Saved Journal: {model.JournalRefNo}",
        documentNo: model.JournalRefNo
    );

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

        [HttpPost]
        public IActionResult SaveCostAllocations([FromBody] List<Tbl20128JournalRegisterCostAllocation> allocations)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            if (allocations == null || !allocations.Any())
            {
                return BadRequest("No cost allocation data received.");
            }
            string userName = HttpContext.Session.GetString("UserName");
            DateTime now = DateTime.Now;
            var journalChildNo = allocations.First().JournalChildNo;

            // Fetch all existing allocations for the JournalChildNo
            var existingAllocations = dbContext.Tbl20128JournalRegisterCostAllocations
                .Where(x => x.JournalChildNo == journalChildNo)
                .ToList();

            foreach (var allocation in allocations)
            {
                var existing = dbContext.Tbl20128JournalRegisterCostAllocations
                    .FirstOrDefault(x => x.CostAllocationId == allocation.CostAllocationId);

                if (existing != null && allocation.CostAllocationId > 0)
                {
                    // Update existing record
                    existing.CostAllocDrCr = allocation.CostAllocDrCr;
                    existing.CostAllocationUnitId = allocation.CostAllocationUnitId;
                    existing.EffectiveDate = allocation.EffectiveDate;
                    existing.AmountAllocated = allocation.AmountAllocated;
                    existing.CostAllocRemarks = allocation.CostAllocRemarks;
                    existing.VoucherNo = allocation.VoucherNo;
                    existing.ModifiedBy = userName;
                    existing.ModifiedOn = now;
                }
                else
                {
                    // New insert
                    dbContext.Tbl20128JournalRegisterCostAllocations.Add(new Tbl20128JournalRegisterCostAllocation
                    {
                        CostAllocDrCr = allocation.CostAllocDrCr,
                        CostAllocationUnitId = allocation.CostAllocationUnitId,
                        EffectiveDate = allocation.EffectiveDate,
                        AmountAllocated = allocation.AmountAllocated,
                        CostAllocRemarks = allocation.CostAllocRemarks,
                        JournalChildNo = allocation.JournalChildNo,
                        VoucherNo = allocation.VoucherNo,
                        EnteredBy = userName,
                        EnteredOn = now
                    });
                }
            }


            dbContext.SaveChanges();



            return Ok();
        }



        [HttpPost]
        public IActionResult SaveEmployeeAllocations([FromBody] List<Tbl20129JournalRegisterEmployeeAllocation> allocations)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            if (allocations == null || !allocations.Any())
            {
                return BadRequest("No property allocation data received.");
            }

            string userName = HttpContext.Session.GetString("UserName");
            DateTime now = DateTime.Now;
            var voucherEntryId = allocations.First().JournalChildNo;

            // Fetch all existing allocations for the VoucherEntryId
            var existingAllocations = dbContext.Tbl20129JournalRegisterEmployeeAllocations
                .Where(x => x.JournalChildNo == voucherEntryId)
                .ToList();

            foreach (var allocation in allocations)
            {
                var existing = dbContext.Tbl20129JournalRegisterEmployeeAllocations
                    .FirstOrDefault(x => x.EmployeeAllocationId == allocation.EmployeeAllocationId);

                if (existing != null && allocation.EmployeeAllocationId > 0)
                {
                    // Update existing record
                    existing.EmployeeNo = allocation.EmployeeNo;
                    existing.EffectiveDate = allocation.EffectiveDate;
                    existing.AmountAllocated = allocation.AmountAllocated;
                    existing.CostAllocRemarks = allocation.CostAllocRemarks;
                    existing.VoucherNo = allocation.VoucherNo;
                    existing.EmpAllocDrCr = allocation.EmpAllocDrCr;
                    existing.LedgerAccountNo = allocation.LedgerAccountNo;
                    existing.ModifiedBy = userName;
                    existing.ModifiedOn = now;
                }
                else
                {
                    // Insert new record
                    dbContext.Tbl20129JournalRegisterEmployeeAllocations.Add(new Tbl20129JournalRegisterEmployeeAllocation
                    {
                        EmployeeNo = allocation.EmployeeNo,
                        EffectiveDate = allocation.EffectiveDate,
                        AmountAllocated = allocation.AmountAllocated,
                        CostAllocRemarks = allocation.CostAllocRemarks,
                        JournalChildNo = allocation.JournalChildNo,
                        VoucherNo = allocation.VoucherNo,
                        EmpAllocDrCr = allocation.EmpAllocDrCr,
                        LedgerAccountNo = allocation.LedgerAccountNo,
                        EnteredBy = userName,
                        EnteredOn = now
                    });
                }
            }

            dbContext.SaveChanges();

            return Ok(new { success = true, message = "Property allocations saved successfully." });
        }
        [HttpPost]
        public IActionResult SavePropertyAllocations([FromBody] List<Tbl20130JournalRegisterPropertyAllocation> allocations)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            if (allocations == null || !allocations.Any())
            {
                return BadRequest("No property allocation data received.");
            }

            string userName = HttpContext.Session.GetString("UserName");
            DateTime now = DateTime.Now;
            var voucherEntryId = allocations.First().JournalChildNo;

            // Fetch all existing allocations for the VoucherEntryId
            var existingAllocations = dbContext.Tbl20130JournalRegisterPropertyAllocations
                .Where(x => x.JournalChildNo == voucherEntryId)
                .ToList();

            foreach (var allocation in allocations)
            {
                var existing = dbContext.Tbl20130JournalRegisterPropertyAllocations
                    .FirstOrDefault(x => x.PropertyAllocationId == allocation.PropertyAllocationId);

                if (existing != null && allocation.PropertyAllocationId > 0)
                {
                    // Update existing record
                    existing.PropertyNo = allocation.PropertyNo;
                    existing.EffectiveDate = allocation.EffectiveDate;
                    existing.AmountAllocated = allocation.AmountAllocated;
                    existing.PropertyAllocRemarks = allocation.PropertyAllocRemarks;
                    existing.VoucherNo = allocation.VoucherNo;
                    existing.PropertyAllocDrCr = allocation.PropertyAllocDrCr;
                    existing.LedgerAccountNo = allocation.LedgerAccountNo;
                    existing.ModifiedBy = userName;
                    existing.ModifiedOn = now;
                }
                else
                {
                    // Insert new record
                    dbContext.Tbl20130JournalRegisterPropertyAllocations.Add(new Tbl20130JournalRegisterPropertyAllocation
                    {
                        PropertyNo = allocation.PropertyNo,
                        EffectiveDate = allocation.EffectiveDate,
                        AmountAllocated = allocation.AmountAllocated,
                        PropertyAllocRemarks = allocation.PropertyAllocRemarks,
                        JournalChildNo = allocation.JournalChildNo,
                        VoucherNo = allocation.VoucherNo,
                        PropertyAllocDrCr = allocation.PropertyAllocDrCr,
                        LedgerAccountNo = allocation.LedgerAccountNo,
                        EnteredBy = userName,
                        EnteredOn = now
                    });
                }
            }

            dbContext.SaveChanges();

            return Ok(new { success = true, message = "Property allocations saved successfully." });
        }

        [HttpGet]
        public IActionResult GetCostAllocationByJournalChildNo(long journalChildNo)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            var result = (from alloc in dbContext.Tbl20128JournalRegisterCostAllocations
                          join unit in dbContext.Tbl201CostAllocationUnits
                              on alloc.CostAllocationUnitId equals unit.CostAllocationUnitId
                          where alloc.JournalChildNo == journalChildNo
                          select new
                          {
                              CostCode = alloc.CostAllocationUnitId,
                              CostUnit = unit.CostAllocationUnit,
                              Amount = alloc.AmountAllocated,
                              DrCr = alloc.CostAllocDrCr
                          }).ToList();

            return Json(result);
        }

        [HttpGet]
        public IActionResult GetEmployeeAllocationByJournalChildNo(long journalChildNo)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            var result = (from alloc in dbContext.Tbl20129JournalRegisterEmployeeAllocations
                          join unit in dbContext.Tbl101Employees
                              on alloc.EmployeeNo equals unit.EmployeeId
                          where alloc.JournalChildNo == journalChildNo
                          select new
                          {
                              EmpNo = alloc.EmployeeNo,
                              EmployeeId = unit.EmployeeName,
                              VoucherAmount = alloc.AmountAllocated,
                              DrCr = alloc.EmpAllocDrCr
                          }).ToList();

            return Json(result);
        }
        [HttpGet]
        public IActionResult GetPropertyAllocationByJournalChildNo(long journalChildNo)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            var result = (from alloc in dbContext.Tbl20130JournalRegisterPropertyAllocations
                          join unit in dbContext.Qry40102PropertyMasterView2s
                              on alloc.PropertyNo equals unit.PropertyNo
                          where alloc.JournalChildNo == journalChildNo
                          select new
                          {
                              PropNo = alloc.PropertyNo,
                              PropertyNo = unit.PropertyDescription,
                              VoucherAmount = alloc.AmountAllocated,
                              DrCr = alloc.PropertyAllocDrCr
                          }).ToList();

            return Json(result);
        }
        [HttpGet]
        public IActionResult GetCostAllocationsBydatagrid(long voucherEntryId)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            var result = (from alloc in dbContext.Tbl20128JournalRegisterCostAllocations
                          join unit in dbContext.Tbl201CostAllocationUnits
                              on alloc.CostAllocationUnitId equals unit.CostAllocationUnitId into gj
                          from unit in gj.DefaultIfEmpty()
                          where alloc.JournalChildNo == voucherEntryId
                          select new
                          {
                              CostAllocationId = alloc.CostAllocationId,
                              DrCr = alloc.CostAllocDrCr,
                              CostAllocationUnit = unit != null ? unit.CostAllocationUnit : "Common Overheads",
                              CostAllocationUnitId = alloc.CostAllocationUnitId,
                              EffectiveDate = alloc.EffectiveDate,
                              VoucherAmount = alloc.AmountAllocated,
                              Remarks = alloc.CostAllocRemarks
                          }).ToList();

            return Json(result);
        }
        [HttpGet]
        public IActionResult GetEmployeeAllocationsBydatagrid(long voucherEntryId)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            var result = (from alloc in dbContext.Tbl20129JournalRegisterEmployeeAllocations
                          join unit in dbContext.Tbl101Employees
                              on alloc.EmployeeNo equals unit.EmployeeId into gj
                          from unit in gj.DefaultIfEmpty()
                          where alloc.JournalChildNo == voucherEntryId
                          select new
                          {
                              EmployeeAllocationId = alloc.EmployeeAllocationId,
                              DrCr = alloc.EmpAllocDrCr,
                              EmployeeName = unit != null ? unit.EmployeeName : "",
                              EmployeeNo = alloc.EmployeeNo,
                              EffectiveDate = alloc.EffectiveDate,
                              VoucherAmount = alloc.AmountAllocated,
                              Remarks = alloc.CostAllocRemarks
                          }).ToList();

            return Json(result);
        }
        [HttpGet]
        public IActionResult GetPropertyAllocationsBydatagrid(long voucherEntryId)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            var result = (from alloc in dbContext.Tbl20130JournalRegisterPropertyAllocations
                          join unit in dbContext.Tbl40101PropertyMasters
                              on alloc.PropertyNo equals unit.PropertyNo into gj
                          from unit in gj.DefaultIfEmpty()
                          where alloc.JournalChildNo == voucherEntryId
                          select new
                          {
                              PropertyAllocationId = alloc.PropertyAllocationId,
                              DrCr = alloc.PropertyAllocDrCr,
                              PropertyDescription = unit != null ? unit.PropertyDescription : "",
                              PropertyNo = alloc.PropertyNo,
                              EffectiveDate = alloc.EffectiveDate,
                              VoucherAmount = alloc.AmountAllocated,
                              Remarks = alloc.PropertyAllocRemarks
                          }).ToList();

            return Json(result);
        }
        [HttpPost]
        public IActionResult DeleteJournalChild([FromBody] long journalChildNo)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            try
            {
                var record = dbContext.Tbl20127JournalRegisterChildren.FirstOrDefault(x => x.JournalChildNo == journalChildNo);
                if (record != null)
                {
                    var costList = dbContext.Tbl20128JournalRegisterCostAllocations
                    .Where(c => c.JournalChildNo == journalChildNo)
                    .ToList();
                    var employeeList = dbContext.Tbl20129JournalRegisterEmployeeAllocations
                        .Where(c => c.JournalChildNo == journalChildNo)
                        .ToList();
                    var propertyList = dbContext.Tbl20130JournalRegisterPropertyAllocations
                        .Where(c => c.JournalChildNo == journalChildNo)
                        .ToList();
                    dbContext.Tbl20127JournalRegisterChildren.Remove(record);
                    dbContext.Tbl20128JournalRegisterCostAllocations.RemoveRange(costList);
                    dbContext.Tbl20129JournalRegisterEmployeeAllocations.RemoveRange(employeeList);
                    dbContext.Tbl20130JournalRegisterPropertyAllocations.RemoveRange(propertyList);
                    dbContext.SaveChanges();
                    return Ok(new { success = true });
                }

                return NotFound(new { success = false, message = "Record not found." });
            }
            catch (Exception ex)
            {
                // Optionally log exception
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult DeleteSelectedJournalChildren([FromBody] List<long> journalChildNos)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }
            if (journalChildNos == null || !journalChildNos.Any())
            {
                return Json(new { success = false, message = "No journal child numbers received." });
            }

            try
            {


                var journalChildren = dbContext.Tbl20127JournalRegisterChildren
                    .Where(j => journalChildNos.Contains(j.JournalChildNo)).ToList();

                var costAllocations = dbContext.Tbl20128JournalRegisterCostAllocations
                    .Where(c => journalChildNos.Contains(c.JournalChildNo)).ToList();

                var employeeAllocations = dbContext.Tbl20129JournalRegisterEmployeeAllocations
                    .Where(e => journalChildNos.Contains(e.JournalChildNo)).ToList();

                var propertyAllocations = dbContext.Tbl20130JournalRegisterPropertyAllocations
                    .Where(p => journalChildNos.Contains(p.JournalChildNo)).ToList();

                dbContext.Tbl20128JournalRegisterCostAllocations.RemoveRange(costAllocations);
                dbContext.Tbl20129JournalRegisterEmployeeAllocations.RemoveRange(employeeAllocations);
                dbContext.Tbl20130JournalRegisterPropertyAllocations.RemoveRange(propertyAllocations);
                dbContext.Tbl20127JournalRegisterChildren.RemoveRange(journalChildren);

                dbContext.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Log error
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> SubmitToFinanceAsync(string voucherNo)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            var username = HttpContext.Session.GetString("UserName");
            var now = DateTime.Now;

            if (string.IsNullOrEmpty(username))
            {
                return Json(new { success = false, message = "User not logged in." });
            }

            try
            {
                var entry = dbContext.Tbl20126JournalRegisterMasters
                    .FirstOrDefault(x => x.JournalRefNo == voucherNo);

                if (entry == null)
                {
                    return Json(new { success = false, message = "Entry not found." });
                }

                entry.IsSubmittedToFinance = true;
                entry.SubmittedBy = username;
                entry.SubmittedOn = now;

                dbContext.SaveChanges();
                await _userActionLogger.LogAsync(
                module: "Finance > Journal Register",
                actionDetail: $"Submitted: {entry.JournalRefNo}",
                documentNo: entry.JournalRefNo
            );

                return Json(new
                {
                    success = true,
                    submittedBy = username,
                    submittedOn = now.ToString("dd-MMM-yyyy")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while submitting to finance.");
                return Json(new { success = false, message = "Server error occurred." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> VerifyJournalAsync(string voucherNo)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            var username = HttpContext.Session.GetString("UserName");
            var now = DateTime.Now;

            if (string.IsNullOrEmpty(username))
            {
                return Json(new { success = false, message = "User not logged in." });
            }

            try
            {
                var entry = dbContext.Tbl20126JournalRegisterMasters
                    .FirstOrDefault(x => x.JournalRefNo == voucherNo);

                if (entry == null)
                {
                    return Json(new { success = false, message = "Entry not found." });
                }

                entry.IsVerified = true;
                entry.VerifiedBy = username;
                entry.VerifiedOn = now;

                dbContext.SaveChanges();
                await _userActionLogger.LogAsync(
                 module: "Finance > Journal Register",
                 actionDetail: $"Verified: {entry.JournalRefNo}",
                 documentNo: entry.JournalRefNo
                );
                return Json(new
                {
                    success = true,
                    verifiedBy = username,
                    verifedOn = now.ToString("dd-MMM-yyyy")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while submitting to finance.");
                return Json(new { success = false, message = "Server error occurred." });
            }
        }
        [HttpPost]
        public async Task<IActionResult> ApproveJournalAsync(string voucherNo)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            var username = HttpContext.Session.GetString("UserName");
            var now = DateTime.Now;

            if (string.IsNullOrEmpty(username))
            {
                return Json(new { success = false, message = "User not logged in." });
            }

            try
            {
                var entry = dbContext.Tbl20126JournalRegisterMasters
                    .FirstOrDefault(x => x.JournalRefNo == voucherNo);

                if (entry == null)
                {
                    return Json(new { success = false, message = "Entry not found." });
                }

                entry.IsApproved = true;
                entry.ApprovedBy = username;
                entry.ApprovedOn = now;

                dbContext.SaveChanges();
                await _userActionLogger.LogAsync(
                 module: "Finance > Journal Register",
                 actionDetail: $"Approved: {entry.JournalRefNo}",
                 documentNo: entry.JournalRefNo
                );
                return Json(new
                {
                    success = true,
                    approvedBy = username,
                    approvedOn = now.ToString("dd-MMM-yyyy")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while submitting to finance.");
                return Json(new { success = false, message = "Server error occurred." });
            }
        }
        [HttpPost]
        public IActionResult PostJournal([FromBody] JournalPostRequest request)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            var username = HttpContext.Session.GetString("UserName");
            var now = DateTime.Now;

            if (string.IsNullOrEmpty(username))
            {
                return Json(new { success = false, message = "User not logged in." });
            }

            try
            {
                var entry = dbContext.Tbl20126JournalRegisterMasters
                    .FirstOrDefault(x => x.JournalRefNo == request.VoucherNo);

                if (entry == null)
                {
                    return Json(new { success = false, message = "Entry not found." });
                }

                entry.IsPosted = true;
                entry.PostedBy = username;
                entry.PostedOn = now;
                entry.PostedVoucherNo = request.NewVoucherNo;

                dbContext.SaveChanges();

                return Json(new
                {
                    success = true,
                    postedBy = username,
                    postedOn = now.ToString("dd-MMM-yyyy")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while submitting to finance.");
                return Json(new { success = false, message = "Server error occurred." });
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetGeneratedVoucherNo(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                DateTime currentDate = DateTime.Now;
                string currentYear = currentDate.Year.ToString();
                string currentMonth = currentDate.Month.ToString("00");
                string voucherString = "JV-" + currentYear.Substring(currentYear.Length - 2, 2) + "-" + currentMonth + "-";
                string strNewReceiptNo;

                // SQL query with interpolated string
                string likePattern = voucherString + "%";

                try
                {
                    // Use raw SQL query to fetch the maximum voucher number
                    var result = await dbContext.VoucherResults
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
            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public IActionResult InsertVoucherFromJournal([FromBody] InsertVoucherFromJournalRequest model)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            try
            {
                

                dbContext.Database.ExecuteSqlRaw(
                    "EXEC sp20203InsertJournalEntryToVoucher @JournalRefNo = {0}, @PostingVoucherNo = {1}, @AddedBy = {2}, @AddedOn = {3}, @JustAddedVoucherEntryNo = 0",
                    model.JournalRefNo,
                    model.PostingVoucherNo,
                    model.AddedBy,
                    model.AddedOn
                   
                );

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error inserting voucher: {ex.Message}");
                return BadRequest(new { success = false, message = "Failed to insert voucher." });
            }
        }
        [HttpGet]
        public IActionResult IsPLItemLedger(string accountId)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(false);
            }

            bool isPLItem = dbContext.Qry20111ListOfPandLitems.Any(p => p.AccountId == accountId);
            return Ok(isPLItem);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateVoucheronload([FromBody] JournalRegisterViewModel model)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
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




                await dbContext.SaveChangesAsync();

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public IActionResult InsertDefaultCostAllocation([FromBody] CostAllocationDto dto)
        {
            // Validate tenant and get context
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            if (dto == null)
            {
                return BadRequest(new { success = false, message = "Invalid data." });
            }

            try
            {
                // Map DTO to Entity
                var entity = new Tbl20128JournalRegisterCostAllocation
                {
                    CostAllocationId = 0,
                    CostAllocDrCr = dto.CostAllocDrCr,
                    CostAllocationUnitId = dto.CostAllocationUnitId,
                    EffectiveDate = dto.EffectiveDate,
                    AmountAllocated = dto.AmountAllocated,
                    CostAllocRemarks = dto.CostAllocRemarks,
                    JournalChildNo = dto.VoucherEntryId,
                    VoucherNo = dto.VoucherNo
                };

                dbContext.Tbl20128JournalRegisterCostAllocations.Add(entity);
                dbContext.SaveChanges();

                return Ok(new { success = true, id = entity.CostAllocationId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error inserting cost allocation.", error = ex.Message });
            }
        }
        public IActionResult UpdateCostAllocationWithFields([FromBody] UpdateCostAllocationFieldsDto dto)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { success = false, message = "Invalid tenant." });

            var record = dbContext.Tbl20128JournalRegisterCostAllocations.FirstOrDefault(x => x.CostAllocationId == dto.CostAllocationId);
            if (record == null)
                return NotFound(new { success = false, message = "Record not found." });

            record.AmountAllocated = dto.VoucherAmount;
            record.CostAllocRemarks = dto.CostAllocRemarks;
            record.CostAllocationUnitId = dto.CostAllocationUnitId;
            record.EffectiveDate = dto.EffectiveDate;

            dbContext.SaveChanges();

            return Ok(new { success = true });
        }
        [HttpPost]

        public IActionResult DeleteByVoucherNo([FromBody] VoucherDeleteRequest request)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            if (string.IsNullOrEmpty(request.VoucherNo))
            {
                return BadRequest(new { success = false, message = "Voucher number is required." });
            }

            var allocations = dbContext.Tbl20128JournalRegisterCostAllocations
                .Where(x => x.VoucherNo == request.VoucherNo)
                .ToList();

            if (!allocations.Any())
            {
                return NotFound(new { success = false, message = "No records found to delete." });
            }

            dbContext.Tbl20128JournalRegisterCostAllocations.RemoveRange(allocations);
            dbContext.SaveChanges();

            return Ok(new { success = true, message = "All cost allocation records deleted for voucher." });
        }
        [HttpPost]

        public IActionResult DeleteCostAllocation(long costAllocationId)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }



            var allocations = dbContext.Tbl20128JournalRegisterCostAllocations
                .Where(x => x.CostAllocationId == costAllocationId)
                .ToList();

            if (!allocations.Any())
            {
                return NotFound(new { success = false, message = "No records found to delete." });
            }

            dbContext.Tbl20128JournalRegisterCostAllocations.RemoveRange(allocations);
            dbContext.SaveChanges();

            return Ok(new { success = true, message = "All cost allocation records deleted for voucher." });
        }
        public IActionResult InsertDefaultEmployeeAllocation([FromBody] EmployeeAllocationDto dto)
        {
            // Validate tenant and get context
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            if (dto == null)
            {
                return BadRequest(new { success = false, message = "Invalid data." });
            }

            try
            {
                // Map DTO to Entity
                var entity = new Tbl20129JournalRegisterEmployeeAllocation
                {
                    EmployeeAllocationId = 0,
                    EmpAllocDrCr = dto.EmpAllocDrCr,
                    EmployeeNo = dto.EmployeeNo,
                    EffectiveDate = dto.EffectiveDate,
                    AmountAllocated = dto.AmountAllocated,
                    CostAllocRemarks = dto.CostAllocRemarks,
                    JournalChildNo = dto.VoucherEntryId,
                    VoucherNo = dto.VoucherNo,
                    LedgerAccountNo = dto.LedgerAccountNo,
                };

                dbContext.Tbl20129JournalRegisterEmployeeAllocations.Add(entity);
                dbContext.SaveChanges();

                return Ok(new { success = true, id = entity.EmployeeAllocationId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error inserting cost allocation.", error = ex.Message });
            }
        }
        public IActionResult UpdateEmployeeAllocationWithFields([FromBody] UpdateEmployeeAllocationFieldsDto dto)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { success = false, message = "Invalid tenant." });

            var record = dbContext.Tbl20129JournalRegisterEmployeeAllocations.FirstOrDefault(x => x.EmployeeAllocationId == dto.EmployeeAllocationId);
            if (record == null)
                return NotFound(new { success = false, message = "Record not found." });

            record.AmountAllocated = dto.VoucherAmount;
            record.CostAllocRemarks = dto.CostAllocRemarks;
            record.EmployeeNo = dto.EmployeeNo;
            record.EffectiveDate = dto.EffectiveDate;

            dbContext.SaveChanges();

            return Ok(new { success = true });
        }
        public IActionResult DeleteEmployeeAllocation(long employeeAllocationId)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }



            var allocations = dbContext.Tbl20129JournalRegisterEmployeeAllocations
                .Where(x => x.EmployeeAllocationId == employeeAllocationId)
                .ToList();

            if (!allocations.Any())
            {
                return NotFound(new { success = false, message = "No records found to delete." });
            }

            dbContext.Tbl20129JournalRegisterEmployeeAllocations.RemoveRange(allocations);
            dbContext.SaveChanges();

            return Ok(new { success = true, message = "All cost allocation records deleted for voucher." });
        }
        [HttpPost]

        public IActionResult DeleteEmployeeByVoucherNo([FromBody] VoucherDeleteRequest request)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            if (string.IsNullOrEmpty(request.VoucherNo))
            {
                return BadRequest(new { success = false, message = "Voucher number is required." });
            }

            var allocations = dbContext.Tbl20129JournalRegisterEmployeeAllocations
                .Where(x => x.VoucherNo == request.VoucherNo)
                .ToList();

            if (!allocations.Any())
            {
                return NotFound(new { success = false, message = "No records found to delete." });
            }

            dbContext.Tbl20129JournalRegisterEmployeeAllocations.RemoveRange(allocations);
            dbContext.SaveChanges();

            return Ok(new { success = true, message = "All cost allocation records deleted for voucher." });
        }
        [HttpPost]
        public IActionResult InsertDefaultPropertyAllocation([FromBody] PropertyAllocationDto dto)
        {
            // Validate tenant and get context
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            if (dto == null)
            {
                return BadRequest(new { success = false, message = "Invalid data." });
            }

            try
            {
                // Map DTO to Entity
                var entity = new Tbl20130JournalRegisterPropertyAllocation
                {
                    PropertyAllocationId = 0,
                    PropertyAllocDrCr = dto.PropertyAllocDrCr,
                    PropertyNo = dto.PropertyNo,
                    EffectiveDate = dto.EffectiveDate,
                    AmountAllocated = dto.AmountAllocated,
                    PropertyAllocRemarks = dto.PropertyAllocRemarks,
                    JournalChildNo = dto.VoucherEntryId,
                    VoucherNo = dto.VoucherNo,
                    LedgerAccountNo = dto.LedgerAccountNo,
                };

                dbContext.Tbl20130JournalRegisterPropertyAllocations.Add(entity);
                dbContext.SaveChanges();

                return Ok(new { success = true, id = entity.PropertyAllocationId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error inserting cost allocation.", error = ex.Message });
            }
        }
        public IActionResult UpdatePropertyAllocationWithFields([FromBody] UpdatePropertyAllocationFieldsDto dto)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { success = false, message = "Invalid tenant." });

            var record = dbContext.Tbl20130JournalRegisterPropertyAllocations.FirstOrDefault(x => x.PropertyAllocationId == dto.PropertyAllocationId);
            if (record == null)
                return NotFound(new { success = false, message = "Record not found." });

            record.AmountAllocated = dto.VoucherAmount;
            record.PropertyAllocRemarks = dto.PropertyAllocRemarks;
            record.PropertyNo = dto.PropertyNo;
            record.EffectiveDate = dto.EffectiveDate;

            dbContext.SaveChanges();

            return Ok(new { success = true });
        }
        [HttpPost]

        public IActionResult DeletePropertyAllocation(long propertyAllocationId)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }



            var allocations = dbContext.Tbl20130JournalRegisterPropertyAllocations
                .Where(x => x.PropertyAllocationId == propertyAllocationId)
                .ToList();

            if (!allocations.Any())
            {
                return NotFound(new { success = false, message = "No records found to delete." });
            }

            dbContext.Tbl20130JournalRegisterPropertyAllocations.RemoveRange(allocations);
            dbContext.SaveChanges();

            return Ok(new { success = true, message = "All cost allocation records deleted for voucher." });
        }
        [HttpPost]

        public IActionResult DeletePropertyByVoucherNo([FromBody] VoucherDeleteRequest request)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            if (string.IsNullOrEmpty(request.VoucherNo))
            {
                return BadRequest(new { success = false, message = "Voucher number is required." });
            }

            var allocations = dbContext.Tbl20130JournalRegisterPropertyAllocations
                .Where(x => x.VoucherNo == request.VoucherNo)
                .ToList();

            if (!allocations.Any())
            {
                return NotFound(new { success = false, message = "No records found to delete." });
            }

            dbContext.Tbl20130JournalRegisterPropertyAllocations.RemoveRange(allocations);
            dbContext.SaveChanges();

            return Ok(new { success = true, message = "All cost allocation records deleted for voucher." });
        }
    }
}



