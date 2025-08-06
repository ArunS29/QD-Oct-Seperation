using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using QD.ERP.Web.Services.Logging;

namespace QD.ERP.Web.Areas.IMS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ProjectSubUnitsController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<ProjectSubUnitsController> _logger;
        private readonly IUserActionLogger _userActionLogger;

        public ProjectSubUnitsController(ILogger<ProjectSubUnitsController> logger, TenantDbContextHelper tenantDbContextHelper, IUserActionLogger userActionLogger)
        {
            _userActionLogger = userActionLogger;
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetProjectSubUnit()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var dbSignatories = await dbContext.Tbl60604purchaseRequestProjectSubUnits
                       .Select(s => new
                       {
                           s.ProjectSubUnitCode,
                           s.ProjectSubUnitName

                       })
                        .ToListAsync();

                    return Json(dbSignatories); // return raw data, paging/sorting done on client-side
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetProjectSubUnit");
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveOrUpdatede([FromBody] Tbl60604purchaseRequestProjectSubUnit model)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var now = DateTime.Now;

                    var existing = await dbContext.Tbl60604purchaseRequestProjectSubUnits
                        .FirstOrDefaultAsync(x => x.ProjectSubUnitCode == model.ProjectSubUnitCode
);

                    if (existing != null)
                    {
                        existing.ProjectSubUnitName = model.ProjectSubUnitName;


                    }
                    else
                    {
                        // Assign new SignatoryId
                        var lastId = await dbContext.Tbl60604purchaseRequestProjectSubUnits
                            .OrderByDescending(x => x.ProjectSubUnitCode)
                            .Select(x => x.ProjectSubUnitCode)
                            .FirstOrDefaultAsync();

                        model.ProjectSubUnitCode = (byte)(lastId + 1); // Assuming short type

                        dbContext.Tbl60604purchaseRequestProjectSubUnits.Add(model);
                    }

                    await dbContext.SaveChangesAsync();
                    await _userActionLogger.LogAsync(
                       module: "IMS > Save Or Update de",
                       actionDetail: $":Saved Update Project Group  {model.ProjectSubUnitCode}",
                        documentNo: $"{model.ProjectSubUnitCode}"
                    );


                    return Ok(new { success = true, message = "Saved successfully", id = model.ProjectSubUnitCode });
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
        public IActionResult Delete1(short key)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var record = dbContext.Tbl60604purchaseRequestProjectSubUnits.FirstOrDefault(x => x.ProjectSubUnitCode == key);
                    if (record == null)
                        return NotFound();


                    dbContext.Tbl60604purchaseRequestProjectSubUnits.Remove(record);
                    dbContext.SaveChanges();
                    _userActionLogger.LogAsync(module: "IMS > Delete1",
                      actionDetail: $":Deleted {key}",
                      documentNo: $"{key}"
                    );
                    return Ok();
                }

                return Unauthorized(new { success = false, message = "Invalid tenant" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Delete: {ex}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
