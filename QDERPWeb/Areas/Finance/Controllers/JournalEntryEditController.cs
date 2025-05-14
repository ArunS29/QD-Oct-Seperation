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
		public IActionResult SaveemployeeAllocations([FromBody] List<Tbl20129JournalRegisterEmployeeAllocation> allocations)
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
			var existingAllocations = dbContext.Tbl20129JournalRegisterEmployeeAllocations
				.Where(x => x.JournalChildNo == journalChildNo)
				.ToList();

			foreach (var allocation in allocations)
			{
				var existing = dbContext.Tbl20129JournalRegisterEmployeeAllocations
					.FirstOrDefault(x => x.EmployeeAllocationId == allocation.EmployeeAllocationId);

				if (existing != null && allocation.EmployeeAllocationId > 0)
				{
					// Update existing record
					existing.EmpAllocDrCr = allocation.EmpAllocDrCr;
					existing.EmployeeNo = allocation.EmployeeNo;
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
					dbContext.Tbl20129JournalRegisterEmployeeAllocations.Add(new Tbl20129JournalRegisterEmployeeAllocation
					{
						EmpAllocDrCr = allocation.EmpAllocDrCr,
						EmployeeNo = allocation.EmployeeNo,
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
		public IActionResult SavepropertyAllocations([FromBody] List<Tbl20130JournalRegisterPropertyAllocation> allocations)
		{
			if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				return Unauthorized(new { success = false, message = "Invalid tenant." });
			}

			if (allocations == null || !allocations.Any())
			{
				return BadRequest("No cost allocation data received.");
			}

			foreach (var allocation in allocations)
			{
				var entity = new Tbl20130JournalRegisterPropertyAllocation
				{
					PropertyAllocDrCr = allocation.PropertyAllocDrCr,
					PropertyNo = allocation.PropertyNo,
					EffectiveDate = allocation.EffectiveDate,
					AmountAllocated = decimal.Parse(allocation.AmountAllocated.ToString()),
					PropertyAllocRemarks = allocation.PropertyAllocRemarks,
					JournalChildNo = allocation.JournalChildNo,
					VoucherNo = allocation.VoucherNo,
					LedgerAccountNo = allocation.LedgerAccountNo
				};

				dbContext.Tbl20130JournalRegisterPropertyAllocations.Add(entity);
			}

			dbContext.SaveChanges();

			//// Group allocations by JournalChildNo to create descriptions
			//var journalChildNos = allocations.Select(a => a.JournalChildNo).Distinct();

			//foreach (var journalChildNo in journalChildNos)
			//{
			//	var relatedAllocations = dbContext.Tbl20129JournalRegisterEmployeeAllocations
			//		.Where(x => x.JournalChildNo == journalChildNo)
			//		.Join(dbContext.Tbl101Employees,
			//			alloc => alloc.EmployeeNo,
			//			unit => unit.EmployeeId,
			//			(alloc, unit) => new
			//			{
			//				alloc.EmployeeNo,
			//				unit.EmployeeName,
			//				alloc.AmountAllocated,
			//				alloc.EmpAllocDrCr
			//			})
			//		.ToList();

			//	var descriptionParts = relatedAllocations.Select(x =>
			//		$"{x.EmployeeNo} | {x.EmployeeName.Substring(0, Math.Min(25, x.EmployeeName.Length))} | ({x.AmountAllocated} {x.EmpAllocDrCr}) ::"
			//	);

			//	string costAllocationDescription = string.Join(" ", descriptionParts);

			//	var journalChildRecord = dbContext.Qry202101journalRegisterChildren
			//		.FirstOrDefault(x => x.JournalChildNo == journalChildNo);

			//	if (journalChildRecord != null)
			//	{
			//		journalChildRecord.EmployeeCostDescription = costAllocationDescription;
			//	}
			//}

			//dbContext.SaveChanges();

			return Ok();
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
        public IActionResult GetCostAllocationsBydatagrid(long journalChildNo)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            var result = (from alloc in dbContext.Tbl20128JournalRegisterCostAllocations
                          join unit in dbContext.Tbl201CostAllocationUnits
                              on alloc.CostAllocationUnitId equals unit.CostAllocationUnitId into gj
                          from unit in gj.DefaultIfEmpty()
                          where alloc.JournalChildNo == journalChildNo
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
		public IActionResult GetEmployeeAllocationsBydatagrid(long journalChildNo)
		{
			if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				return Unauthorized(new { success = false, message = "Invalid tenant." });
			}

			var result = (from alloc in dbContext.Tbl20129JournalRegisterEmployeeAllocations
						  join unit in dbContext.Tbl101Employees
							  on alloc.EmployeeNo equals unit.EmployeeId into gj
						  from unit in gj.DefaultIfEmpty()
						  where alloc.JournalChildNo == journalChildNo
						  select new
						  {
							  EmployeeNo = unit.EmployeeId,
                              EmployeeID = alloc.EmployeeNo,
							  DrCr = alloc.EmpAllocDrCr,
							  EmployeeName = unit.EmployeeName,
							  EmployeeAllocationId = alloc.EmployeeAllocationId,
							  EffectiveDate = alloc.EffectiveDate,
							  VoucherAmount = alloc.AmountAllocated,
							  Remarks = alloc.CostAllocRemarks
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
        public IActionResult SubmitToFinance(string voucherNo)
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
        public IActionResult VerifyJournal(string voucherNo)
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
    }
}



