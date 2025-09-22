using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;

namespace QD.ERP.Web.Areas.ERM.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PropertyGroupController : Controller
    {

        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<PropertyGroupController> _logger;

        public PropertyGroupController(ILogger<PropertyGroupController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetPropertyGroup()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {

                    var dbSignatories = await dbContext.Tbl40108PropertyGroups
                           .Select(s => new
                           {
                               s.PropertyGroupId,
                               s.PropertyGroup,
                               s.PropertyGroupCode

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
public async Task<IActionResult> SaveOrUpdatePropertyGroup([FromBody] Tbl40108PropertyGroup model)
{
    if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
        return Unauthorized(new { success = false, message = "Invalid tenant" });

    try
    {
        var now = DateTime.Now;

                // 🔎 Required fields validation
                if (string.IsNullOrWhiteSpace(model.PropertyGroup))
                {
                    return BadRequest(new { success = false, message = "Property Group is required!" });
                }

                if (string.IsNullOrWhiteSpace(model.PropertyGroupCode))
                {
                    return BadRequest(new { success = false, message = "Property Group Code is required!" });
                }

                // 🔎 Duplicate check before insert/update
                var duplicate = await dbContext.Tbl40108PropertyGroups
            .Where(x => x.PropertyGroupCode == model.PropertyGroupCode 
                        && x.PropertyGroupId != model.PropertyGroupId) // exclude self on update
            .FirstOrDefaultAsync();


        if (duplicate != null)
        {
            return BadRequest(new { success = false, message = "Duplicate Property Group Code is not allowed!" });
        }

        if (model.PropertyGroupId > 0)
        {
            var existing = await dbContext.Tbl40108PropertyGroups
                .FirstOrDefaultAsync(x => x.PropertyGroupId == model.PropertyGroupId);

            if (existing != null)
            {
                existing.PropertyGroup = model.PropertyGroup;
                existing.PropertyGroupCode = model.PropertyGroupCode;
            }
            else
            {
                return NotFound(new { success = false, message = "Record not found for update" });
            }
        }
        else
        {
            // Insert new
            var newEntity = new Tbl40108PropertyGroup
            {
                PropertyGroup = model.PropertyGroup,
                PropertyGroupCode = model.PropertyGroupCode,
                // CreatedDate = now;
            };

            dbContext.Tbl40108PropertyGroups.Add(newEntity);
            await dbContext.SaveChangesAsync(); // save once to get ID

            model.PropertyGroupId = newEntity.PropertyGroupId;
        }

        await dbContext.SaveChangesAsync();

        return Ok(new { success = true, message = "Saved successfully", id = model.PropertyGroupId });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error in SaveOrUpdatePropertyGroup");
        return StatusCode(500, new { success = false, message = "An error occurred", error = ex.Message });
    }
}

        [HttpDelete]

        public IActionResult Delete(int key)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var record = dbContext.Tbl40108PropertyGroups.FirstOrDefault(x => x.PropertyGroupId == key);

                    if (record == null)
                        return NotFound(new { success = false, message = "Record not found." });

                    dbContext.Tbl40108PropertyGroups.Remove(record);
                    dbContext.SaveChanges();

                    return Ok(new { success = true, message = "Record deleted successfully." });
                }

                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Delete method for PropertyGroupId: {Key}", key);
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

