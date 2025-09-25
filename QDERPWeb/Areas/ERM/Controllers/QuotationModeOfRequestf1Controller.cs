using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using QD.ERP.Web.Services.Logging;

namespace QD.ERP.Web.Areas.ERM.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class QuotationModeOfRequestf1Controller : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<QuotationModeOfRequestf1Controller> _logger;
        private readonly IUserActionLogger _userActionLogger;


        public QuotationModeOfRequestf1Controller(ILogger<QuotationModeOfRequestf1Controller> logger, TenantDbContextHelper tenantDbContextHelper, IUserActionLogger userActionLogger)
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
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var now = DateTime.Now;

                    var existing = await dbContext.Tbl30103ModeOfRequestMasters
                        .FirstOrDefaultAsync(x => x.ModeOfRequestId == model.ModeOfRequestId
);

                    if (existing != null)
                    {
                        existing.ModeOfRequest = model.ModeOfRequest;
                      

                    }
                    else
                    {
                        // Assign new SignatoryId
                        var lastId = await dbContext.Tbl30103ModeOfRequestMasters
                            .OrderByDescending(x => x.ModeOfRequestId)
                            .Select(x => x.ModeOfRequestId)
                            .FirstOrDefaultAsync();

                        model.ModeOfRequestId = (byte)(lastId + 1); // Assuming short type

                        dbContext.Tbl30103ModeOfRequestMasters.Add(model);
                    }

                    await dbContext.SaveChangesAsync();
                    await _userActionLogger.LogAsync(
                          module: "ERM > Save Mode",
                         actionDetail: $"Saved Mode {model}",
                          documentNo: $"{model}"
                    );

                    return Ok(new { success = true, message = "Saved successfully", id = model.ModeOfRequestId });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in SaveSignatory: {ex}");
                    return StatusCode(500, new { success = false, message = ex.Message });
                }
            }

            return Unauthorized(new { success = false, message = "Invalid tenant" });
        }
        [HttpDelete]
        public IActionResult Delete(int key)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var record = dbContext.Tbl30103ModeOfRequestMasters.FirstOrDefault(x => x.ModeOfRequestId == key);
                    if (record == null)
                        return NotFound();

                    dbContext.Tbl30103ModeOfRequestMasters.Remove(record);
                    dbContext.SaveChanges();
                     _userActionLogger.LogAsync(
                         module: "ERM > Delete",
                        actionDetail: $"Deleted {key}",
                         documentNo: $"{key}"
                   );
                    return Ok();
                }

                return Unauthorized(new { success = false, message = "Invalid tenant" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Delete: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });

            }
        }

    }
}
