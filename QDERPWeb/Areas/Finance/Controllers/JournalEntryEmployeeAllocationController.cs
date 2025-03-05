using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class JournalEntryEmployeeAllocationController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<JournalEntryEmployeeAllocationController> _logger;

        public JournalEntryEmployeeAllocationController(ILogger<JournalEntryEmployeeAllocationController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        public IActionResult EmployeeAllocation(string voucherNo, string accountHead, string voucherAmount, string drCr, string effectiveDate)
        {
            ViewBag.VoucherNo = voucherNo;
            ViewBag.AccountHead = accountHead;
            ViewBag.VoucherAmount = voucherAmount;
            ViewBag.DrCr = drCr;
            ViewBag.EffectiveDate = effectiveDate;

            return View();
        }

        [HttpGet]
        public IActionResult GetEmployeeName()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var data = dbContext.Tbl101Employees
                    .Select(c => new
                    {
                        c.EmployeeId,
                        c.EmployeeName,
                        c.NationalId
                    }).ToList();

                return Ok(data);
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public async Task<ActionResult> SaveEmployeeAllocation([FromBody] Tbl20129JournalRegisterEmployeeAllocation EM)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    dbContext.Tbl20129JournalRegisterEmployeeAllocations.Add(EM);
                    await dbContext.SaveChangesAsync();
                    return Ok(new { success = true, message = "Data inserted successfully!" });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in SaveEmployeeAllocation: {ex.Message}");
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
                        var item = dbContext.Tbl20129JournalRegisterEmployeeAllocations.Find(id);
                        if (item != null)
                        {
                            dbContext.Tbl20129JournalRegisterEmployeeAllocations.Remove(item);
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




