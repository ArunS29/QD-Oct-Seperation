using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QD.ERP.Shared.DAL.Entities;
using QD.ERP.Shared.Service;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FormQD.ERP.Finance.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BillsPayableReceivableController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<BillsPayableReceivableController> _logger;

        public BillsPayableReceivableController(ILogger<BillsPayableReceivableController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult> GetVoucherEntries(DataSourceLoadOptions loadOptions, string ReferenceNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var qryListOfAccountlists = dbContext.Tbl201SubLedgerMasters
                    .Where(p => p.ReferenceNo == ReferenceNo)
                    .Select(i => new
                    {
                        i.DrCr,
                        i.ReferenceType,
                        i.ReferenceNo,
                        i.Amount,
                        i.SubLedgerId
                    });

                return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public async Task<ActionResult> AddBillsPayableReceivable(DataSourceLoadOptions loadOptions, [FromBody] Tbl201SubLedgerMaster VE)
        {
            if (VE == null)
            {
                return BadRequest(new { success = false, message = "Invalid data received." });
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    dbContext.Tbl201SubLedgerMasters.Add(VE);
                    await dbContext.SaveChangesAsync();

                    var qryListOfAccountlists = dbContext.Tbl201SubLedgerMasters
                        .Where(p => p.ReferenceNo == VE.ReferenceNo)
                        .Select(i => new
                        {
                            i.SubLedgerId,
                            i.DrCr,
                            i.ReferenceType,
                            i.ReferenceNo,
                            i.Amount
                        });

                    return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in AddBillsPayableReceivable: {ex.Message}");
                    return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public async Task<ActionResult> UpdateBillsPayableReceivable([FromBody] Tbl201SubLedgerMaster VM)
        {
            if (VM == null)
            {
                return BadRequest(new { success = false, message = "Invalid data received." });
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    dbContext.Tbl201SubLedgerMasters.Update(VM);
                    await dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Data updated successfully!" });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in UpdateBillsPayableReceivable: {ex.Message}");
                    return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRecord(long id)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    if (id <= 0)
                    {
                        return Json(new { success = false, message = "Invalid SubLedgerId provided." });
                    }

                    var record = await dbContext.Tbl201SubLedgerMasters.FirstOrDefaultAsync(r => r.SubLedgerId == id);
                    if (record == null)
                    {
                        return Json(new { success = false, message = "Record not found." });
                    }

                    dbContext.Tbl201SubLedgerMasters.Remove(record);
                    await dbContext.SaveChangesAsync();

                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in DeleteRecord: {ex.Message}");
                    return Json(new { success = false, message = "An error occurred while deleting the record." });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
    }
}
