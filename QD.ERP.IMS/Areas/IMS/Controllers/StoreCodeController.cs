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
    public class StoreCodeController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<StoreCodeController> _logger;
        private readonly IUserActionLogger _userActionLogger;


        public StoreCodeController(ILogger<StoreCodeController> logger, TenantDbContextHelper tenantDbContextHelper, IUserActionLogger userActionLogger)
        {
            _userActionLogger = userActionLogger;
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetCostCenter()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var CostCenter = await dbContext.Tbl201CostAllocationUnits
                       .Select(i => new
                       {
                         i.CostAllocationUnitId,
                         i.CostAllocationUnit,
                         i.CostAllocationGroup,
                         i.CostAllocationMasterGroup

                       })
                        .ToListAsync();

                    return Json(CostCenter); // return raw data, paging/sorting done on client-side
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
        public async Task<IActionResult> SaveOrUpdateStory([FromBody] Tbl60001storeMaster model)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    // Check if record exists by StoreId (for update)
                    var existing = await dbContext.Tbl60001storeMasters
                        .FirstOrDefaultAsync(x => x.StoreId == model.StoreId);

                    // 🔍 If it's a new record (insert)
                    if (existing == null)
                    {
                        // Check if StoreName already exists (case-insensitive)
                        bool isDuplicateName = await dbContext.Tbl60001storeMasters
                            .AnyAsync(x => x.StoreName.ToLower() == model.StoreName.ToLower());

                        if (isDuplicateName)
                        {
                            return BadRequest(new { success = false, message = "This StoreName is already in the database. Please check again." });
                        }

                        dbContext.Tbl60001storeMasters.Add(model);
                    }
                    else
                    {
                        // 🔁 Update logic
                        existing.StoreName = model.StoreName;
                        existing.CostAllocationUnitId = model.CostAllocationUnitId;
                    }

                    await dbContext.SaveChangesAsync();
                    await _userActionLogger.LogAsync(
                      module: "IMS > Save Project Document",
                      actionDetail: $"Saved Project Document  {model.StoreId}",
                      documentNo: $"{model.StoreId}"
                    );

                    return Ok(new { success = true, message = "Saved successfully", id = model.StoreId });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in SaveSignatory: {ex}");
                    return StatusCode(500, new { success = false, message = ex.Message });
                }
            }

            return Unauthorized(new { success = false, message = "Invalid tenant" });
        }

        [HttpGet]
        public async Task<IActionResult> GetStoreData()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var StoreData = await dbContext.Tbl60001storeMasters
                       .Select(i => new
                       {
                           i.StoreId,
                           i.StoreName,
                           i.CostAllocationUnitId

                       })
                        .ToListAsync();

                    return Json(StoreData); // return raw data, paging/sorting done on client-side
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }
        [HttpDelete]
        public IActionResult Delete(string key)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var record = dbContext.Tbl60001storeMasters.FirstOrDefault(x => x.StoreId == key);
                    if (record == null)
                        return NotFound();

                    dbContext.Tbl60001storeMasters.Remove(record);
                    dbContext.SaveChanges();
                    _userActionLogger.LogAsync(module: "IMS > Delete ",
                        actionDetail: $"Deleted  {key}",
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
