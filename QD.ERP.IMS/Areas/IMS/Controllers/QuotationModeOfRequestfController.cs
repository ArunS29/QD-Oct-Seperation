using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QD.ERP.Shared.DAL.Entities;
using QD.ERP.Shared.Service;
using QD.ERP.Shared.Services.Logging;

namespace QD.ERP.IMS.Areas.IMS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class QuotationModeOfRequestfController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<QuotationModeOfRequestfController> _logger;
        private readonly IUserActionLogger _userActionLogger;


        public QuotationModeOfRequestfController(ILogger<QuotationModeOfRequestfController> logger, TenantDbContextHelper tenantDbContextHelper, IUserActionLogger userActionLogger)
        {
            _userActionLogger = userActionLogger;
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetModeofRequest()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var dbSignatories = await dbContext.Tbl30103ModeOfRequestMasters
                       .Select(s => new
                       {
                           s.ModeOfRequestId,
                           s.ModeOfRequest
                         
                       })
                        .ToListAsync();

                    return Json(dbSignatories); // return raw data, paging/sorting done on client-side
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }

       
        [HttpPost]
        public async Task<IActionResult> SaveOrUpdateMode([FromBody] Tbl30103ModeOfRequestMaster model)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { success = false, message = "Invalid tenant" });

            try
            {
                var existing = await dbContext.Tbl30103ModeOfRequestMasters
                    .FirstOrDefaultAsync(x => x.ModeOfRequestId == model.ModeOfRequestId);

                if (existing != null)
                {
                    // 🔁 Update
                    existing.ModeOfRequest = model.ModeOfRequest;
                }
                else
                {
                    // 🔍 Check if ModeOfRequest already exists (case-insensitive)
                    bool isDuplicate = await dbContext.Tbl30103ModeOfRequestMasters
                        .AnyAsync(x => x.ModeOfRequest.ToLower() == model.ModeOfRequest.ToLower());

                    if (isDuplicate)
                    {
                        return BadRequest(new { success = false, message = "This Mode Of Request already exists in the database. Please check again." });
                    }

                    // Assign new ID
                    var lastId = await dbContext.Tbl30103ModeOfRequestMasters
                        .OrderByDescending(x => x.ModeOfRequestId)
                        .Select(x => x.ModeOfRequestId)
                        .FirstOrDefaultAsync();

                    model.ModeOfRequestId = (byte)(lastId + 1);

                    dbContext.Tbl30103ModeOfRequestMasters.Add(model);
                }

                await dbContext.SaveChangesAsync();

                await _userActionLogger.LogAsync(
                  module: "IMS > Save Or Update Mode",
                  actionDetail: $"Saved Mode {model.ModeOfRequestId}",
                  documentNo: $"{model.ModeOfRequestId}"
                );

                return Ok(new { success = true, message = "Saved successfully", id = model.ModeOfRequestId });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in SaveOrUpdateMode: {ex}");
                return StatusCode(500, new { success = false, message = "An error occurred while saving Mode Of Request." });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int key)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    return Unauthorized(new { success = false, message = "Invalid tenant" });
                }

                var record = await dbContext.Tbl30103ModeOfRequestMasters
                    .FirstOrDefaultAsync(x => x.ModeOfRequestId == key);

                if (record == null)
                {
                    return NotFound(new { success = false, message = "Record not found" });
                }

                dbContext.Tbl30103ModeOfRequestMasters.Remove(record);
                await dbContext.SaveChangesAsync();

                await _userActionLogger.LogAsync(
                    module: "IMS > Delete",
                    actionDetail: $"Deleted ModeOfRequestId: {key}",
                    documentNo: $"{key}"
                );

                return Ok(new { success = true, message = "Deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Delete: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while deleting the record.",
                    error = ex.Message
                });
            }
        }


    }
}
