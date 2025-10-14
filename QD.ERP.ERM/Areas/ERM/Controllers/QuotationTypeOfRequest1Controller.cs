using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QD.ERP.Shared.DAL.Entities;
using QD.ERP.Shared.Service;
using QD.ERP.Shared.Services.Logging;

namespace QD.ERP.ERM.Areas.ERM.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class QuotationTypeOfRequest1Controller : Controller
    {

        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<QuotationTypeOfRequest1Controller> _logger;
        private readonly IUserActionLogger _userActionLogger;


        public QuotationTypeOfRequest1Controller(ILogger<QuotationTypeOfRequest1Controller> logger, TenantDbContextHelper tenantDbContextHelper, IUserActionLogger userActionLogger)
        {
            _userActionLogger = userActionLogger;
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetTypeofRequest()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var dbSignatories = await dbContext.Tbl30104TypeOfRequestMasters
                       .Select(s => new
                       {
                          s.TypeOfRequest,
                          s.TermsCategory,
                          s.TypeOfRequestId

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
        public async Task<IActionResult> SaveOrUpdateSignatory([FromBody] Tbl30104TypeOfRequestMaster model)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var now = DateTime.Now;

                    var existing = await dbContext.Tbl30104TypeOfRequestMasters
                        .FirstOrDefaultAsync(x => x.TypeOfRequestId == model.TypeOfRequestId
);

                    if (existing != null)
                    {
                        existing.TypeOfRequest = model.TypeOfRequest;
                        existing.TermsCategory = model.TermsCategory;
                  
                    }
                    else
                    {
                        // Assign new SignatoryId
                        var lastId = await dbContext.Tbl30104TypeOfRequestMasters
                            .OrderByDescending(x => x.TypeOfRequestId)
                            .Select(x => x.TypeOfRequestId)
                            .FirstOrDefaultAsync();

                        model.TypeOfRequestId = (byte)(lastId + 1); // Assuming short type

                        dbContext.Tbl30104TypeOfRequestMasters.Add(model);
                    }

                    await dbContext.SaveChangesAsync();
                    await _userActionLogger.LogAsync(
                          module: "ERM > Save Signatory",
                         actionDetail: $"Saved Signatory {model}",
                          documentNo: $"{model}"
                    );

                    return Ok(new { success = true, message = "Saved successfully", id = model.TypeOfRequestId });
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
                    var record = dbContext.Tbl30104TypeOfRequestMasters.FirstOrDefault(x => x.TypeOfRequestId == key);
                    if (record == null)
                        return NotFound();

                    dbContext.Tbl30104TypeOfRequestMasters.Remove(record);
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
