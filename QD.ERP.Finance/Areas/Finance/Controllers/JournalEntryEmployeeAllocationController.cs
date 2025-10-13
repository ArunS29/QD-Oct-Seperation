using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
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

        [HttpGet]
        public IActionResult GetEmployeeAllocationsBydatagrid(long voucherEntryId)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            var result = (from alloc in dbContext.Tbl20104EmployeeAllocationMasters
                          join unit in dbContext.Tbl101Employees
                              on alloc.EmployeeNo equals unit.EmployeeId into gj
                          from unit in gj.DefaultIfEmpty()
                          where alloc.VoucherEntryId == voucherEntryId
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
    }
}




