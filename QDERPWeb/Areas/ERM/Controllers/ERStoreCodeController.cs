using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;

namespace QD.ERP.Web.Areas.ERM.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ERStoreCodeController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<ERStoreCodeController> _logger;

        public ERStoreCodeController(ILogger<ERStoreCodeController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
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
                    

                    var existing = await dbContext.Tbl60001storeMasters
                        .FirstOrDefaultAsync(x => x.StoreId == model.StoreId
);

                    if (existing != null)
                    {
                        existing.StoreId = model.StoreId;
                        existing.StoreName = model.StoreName;
                        existing.CostAllocationUnitId = model.CostAllocationUnitId;

                    }
                    else
                    {
                        

                        dbContext.Tbl60001storeMasters.Add(model);
                    }

                    await dbContext.SaveChangesAsync();

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
