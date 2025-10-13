using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Shared.DAL.Entities;
using QD.ERP.Web.Service;
using QD.ERP.Web.Services.Logging;
using System.Xml.Linq;

namespace QD.ERP.Web.Areas.ERM.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AddNewQuotationStatus1Controller : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<AddNewQuotationStatus1Controller> _logger;
        private readonly IUserActionLogger _userActionLogger;


        public AddNewQuotationStatus1Controller(ILogger<AddNewQuotationStatus1Controller> logger, TenantDbContextHelper tenantDbContextHelper, IUserActionLogger userActionLogger)
        {
            _userActionLogger = userActionLogger;
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetQuotationStatus()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var quotationStatus = await dbContext.Tbl60107quotationStatuses
                       .Select(s => new
                       {
                           s.QuoteStatusId,
                           s.QuoteStatus

                       })
                        .ToListAsync();

                    return Json(quotationStatus); // return raw data, paging/sorting done on client-side
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetQuotationStatus");
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveOrUpdateStatus([FromBody] Tbl60107quotationStatus model)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var now = DateTime.Now;

                    var existing = await dbContext.Tbl60107quotationStatuses
                        .FirstOrDefaultAsync(x => x.QuoteStatusId == model.QuoteStatusId
                    );

                    if (existing != null)
                    {
                        existing.QuoteStatus = model.QuoteStatus;


                    }
                    else
                    {
                        // Assign new SignatoryId
                        var lastId = await dbContext.Tbl60107quotationStatuses
                            .OrderByDescending(x => x.QuoteStatusId)
                            .Select(x => x.QuoteStatusId)
                            .FirstOrDefaultAsync();

                        model.QuoteStatusId = (byte)(lastId + 1); // Assuming short type

                        dbContext.Tbl60107quotationStatuses.Add(model);
                    }

                    await dbContext.SaveChangesAsync();
                    await _userActionLogger.LogAsync(
                         module: "ERM > Save Status",
                        actionDetail: $"Saved Status {model}",
                         documentNo: $"{model}"
                    );

                    return Ok(new { success = true, message = "Saved successfully", id = model.QuoteStatusId });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in SaveSignatory: {ex}");
                    return StatusCode(500, new { success = false, message = ex.Message });
                }
            }

            return Unauthorized(new { success = false, message = "Invalid tenant" });
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuotationStatus(byte id)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var existing = await dbContext.Tbl60107quotationStatuses
                        .FirstOrDefaultAsync(x => x.QuoteStatusId == id);

                    if (existing == null)
                    {
                        return NotFound(new { success = false, message = "Status not found" });
                    }

                    dbContext.Tbl60107quotationStatuses.Remove(existing);
                    await dbContext.SaveChangesAsync();
                    await _userActionLogger.LogAsync(
                         module: "ERM > Delete Quotation Status",
                        actionDetail: $"Deleted Quotation Status {id}",
                         documentNo: $"{id}"
                    );

                    return Ok(new { success = true, message = "Deleted successfully" });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in DeleteStatus: {ex}");
                    return StatusCode(500, new { success = false, message = ex.Message });
                }
            }

            return Unauthorized(new { success = false, message = "Invalid tenant" });
        }

    }
}
