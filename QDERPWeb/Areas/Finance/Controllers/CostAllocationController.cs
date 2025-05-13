using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CostAllocationController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<CostAllocationController> _logger;

        public CostAllocationController(ILogger<CostAllocationController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult CostAllocation(string voucherNo, string accountHead, string voucherAmount, string drCr, string effectiveDate)
        {
            ViewBag.VoucherNo = voucherNo;
            ViewBag.AccountHead = accountHead;
            ViewBag.VoucherAmount = voucherAmount;
            ViewBag.DrCr = drCr;
            ViewBag.EffectiveDate = effectiveDate;

            return View();
        }

        [HttpGet]
        public IActionResult GetCostAllocationUnits()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var data = dbContext.Tbl201CostAllocationUnits
                    .Select(c => new
                    {
                        c.CostAllocationUnitId,
                        c.CostAllocationUnit,
                        c.CostAllocationGroup,
                        c.IsDisabled
                    }).ToList();

                return Ok(data);
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public IActionResult SaveCostAllocations([FromBody] List<Tbl201CostAllocationMaster> allocations)
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
            var journalChildNo = allocations.First().VoucherEntryId;

            // Fetch all existing allocations for the JournalChildNo
            var existingAllocations = dbContext.Tbl201CostAllocationMasters
                .Where(x => x.VoucherEntryId == journalChildNo)
                .ToList();

            foreach (var allocation in allocations)
            {
                var existing = dbContext.Tbl201CostAllocationMasters
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
                    dbContext.Tbl201CostAllocationMasters.Add(new Tbl201CostAllocationMaster
                    {
                        CostAllocDrCr = allocation.CostAllocDrCr,
                        CostAllocationUnitId = allocation.CostAllocationUnitId,
                        EffectiveDate = allocation.EffectiveDate,
                        AmountAllocated = allocation.AmountAllocated,
                        CostAllocRemarks = allocation.CostAllocRemarks,
                        VoucherEntryId = allocation.VoucherEntryId,
                        VoucherNo = allocation.VoucherNo,
                        EnteredBy = userName,
                        EnteredOn = now
                    });
                }
            }


            dbContext.SaveChanges();



            return Ok();
        }
        [HttpGet]
        public IActionResult GetCostAllocationsBydatagrid(long voucherEntryId)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            var result = (from alloc in dbContext.Tbl201CostAllocationMasters
                          join unit in dbContext.Tbl201CostAllocationUnits
                              on alloc.CostAllocationUnitId equals unit.CostAllocationUnitId into gj
                          from unit in gj.DefaultIfEmpty()
                          where alloc.VoucherEntryId == voucherEntryId
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
    }
}

