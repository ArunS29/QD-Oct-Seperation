using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
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
    public class JournalEntryEditController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<JournalEntryEditController> _logger;

        public JournalEntryEditController(ILogger<JournalEntryEditController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
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
                        SELECT MAX(CAST(RIGHT(VoucherNo, 5) AS INT)) AS MaxVoucherNo
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
    }
}



