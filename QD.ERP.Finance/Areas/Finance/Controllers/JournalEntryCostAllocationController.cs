using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QD.ERP.Shared.DAL.Entities;
using QD.ERP.Shared.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QD.ERP.Finance.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class JournalEntryCostAllocationController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<JournalEntryCostAllocationController> _logger;

        public JournalEntryCostAllocationController(ILogger<JournalEntryCostAllocationController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

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
        public async Task<ActionResult> SaveCostAllocation([FromBody] Tbl20128JournalRegisterCostAllocation CM)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    long maxVoucherEntryID = await dbContext.Tbl20128JournalRegisterCostAllocations
                        .OrderByDescending(x => x.JournalChildNo)
                        .Select(x => x.JournalChildNo)
                        .FirstOrDefaultAsync();

                    CM.JournalChildNo = maxVoucherEntryID + 1;

                    dbContext.Tbl20128JournalRegisterCostAllocations.Add(CM);
                    await dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Data inserted successfully!" });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in SaveCostAllocation: {ex.Message}");
                    return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public IActionResult Delete(List<int> rowKeys)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    foreach (var id in rowKeys)
                    {
                        var item = dbContext.Tbl20128JournalRegisterCostAllocations.Find(id);
                        if (item != null)
                        {
                            dbContext.Tbl20128JournalRegisterCostAllocations.Remove(item);
                        }
                    }
                    dbContext.SaveChanges();
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in Delete: {ex.Message}");
                    return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
    }
}


