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
    public class JournalEntryPropertyAllocationController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<JournalEntryPropertyAllocationController> _logger;

        public JournalEntryPropertyAllocationController(ILogger<JournalEntryPropertyAllocationController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        public IActionResult PropertyAllocation(string voucherNo, string accountHead, string voucherAmount, string drCr, string accountId, string effectiveDate)
        {
            ViewBag.VoucherNo = voucherNo;
            ViewBag.AccountHeadVal = accountHead;
            ViewBag.VoucherAmount = voucherAmount;
            ViewBag.DrCr = drCr;
            ViewBag.AccountID = accountId;
            ViewBag.EffectiveDate = effectiveDate;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> CheckPropertyAllocation(string AccountHead, string AccountID)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var allocation = await dbContext.Tbl201ChartOfAccounts
                        .Where(x => x.AccountHead == AccountHead && x.AccountId == AccountID && x.IsPropertyAllocated == true)
                        .FirstOrDefaultAsync();

                    if (allocation != null)
                    {
                        return Ok(new { isAllocated = true });
                    }
                    else
                    {
                        return Ok(new { isAllocated = false });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in CheckPropertyAllocation: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while checking property allocation.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public IActionResult GetPropertyAllocationUnits()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var data = dbContext.Qry40102PropertyMasterView2s
                    .Select(c => new
                    {
                        c.PropertyNo,
                        c.PropertyDescription,
                        c.PropertyType,
                        c.PlateNo
                    }).ToList();

                return Ok(data);
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public async Task<ActionResult> SaveJournalPropertyAllocation([FromBody] Tbl20130JournalRegisterPropertyAllocation CM)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    long maxVoucherEntryID = await dbContext.Tbl20130JournalRegisterPropertyAllocations
                        .OrderByDescending(x => x.JournalChildNo)
                        .Select(x => x.JournalChildNo)
                        .FirstOrDefaultAsync();

                    CM.JournalChildNo = maxVoucherEntryID + 1;

                    dbContext.Tbl20130JournalRegisterPropertyAllocations.Add(CM);
                    await dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Data inserted successfully!" });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in SaveJournalPropertyAllocation: {ex.Message}");
                    return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
    }
}




