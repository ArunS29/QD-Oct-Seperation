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
    public class ProjectGroupController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<ProjectGroupController> _logger;
        private readonly IUserActionLogger _userActionLogger;


        public ProjectGroupController(ILogger<ProjectGroupController> logger, TenantDbContextHelper tenantDbContextHelper, IUserActionLogger userActionLogger)
        {
            _userActionLogger = userActionLogger;
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllProjectGroupData()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var dbProjectGroup = await dbContext.Tbl70006projectGroups
                       .Select(i => new

                       {
                         i.ProjectGroupId,
                         i.ProjectGroupCode,
                         i.ProjectGroup,
                         i.ProjectGroupAr,
                         i.ProjectCategory,
                         
                          
                       })
                        .ToListAsync();

                    return Json(dbProjectGroup); // return raw data, paging/sorting done on client-side
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ProjectGroupController");
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> SaveOrUpdateProjectGroup([FromBody] Tbl70006projectGroup model)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var existingRecord = await dbContext.Tbl70006projectGroups
                        .FirstOrDefaultAsync(x => x.ProjectGroupId == model.ProjectGroupId);

                    if (existingRecord != null)
                    {
                        // Update existing record
                        existingRecord.ProjectGroup = model.ProjectGroup;
                        existingRecord.ProjectGroupCode = model.ProjectGroupCode;
                        existingRecord.ProjectGroupAr = model.ProjectGroupAr;
                        existingRecord.ProjectCategory = model.ProjectCategory;

                     
                        await dbContext.SaveChangesAsync();
                        await _userActionLogger.LogAsync(
                         module: "IMS > Save Or Update Project Group",
                         actionDetail: $"Saved Update Project Group  {model.ProjectGroupId}",
                         documentNo: $"{model.ProjectGroupId}"
                        );

                        return Ok(new { success = true, message = "Updated successfully", id = existingRecord.ProjectGroupId });
                    }
                    else
                    {
                        // 🔍 Check duplicate ProjectSubUnitName (case-insensitive)
                        bool isDuplicate = await dbContext.Tbl70006projectGroups
                            .AnyAsync(x => x.ProjectGroup.ToLower() == model.ProjectGroup.ToLower());

                        if (isDuplicate)
                        {
                            return BadRequest(new { success = false, message = "This Project Group is already exists in the database. Please check again." });
                        }
                        // Insert new record
                        var lastId = await dbContext.Tbl70006projectGroups
                            .OrderByDescending(x => x.ProjectGroupId)
                            .Select(x => x.ProjectGroupId)
                            .FirstOrDefaultAsync();

                        model.ProjectGroupId = lastId == 0 ? (byte)1 : (byte)(lastId + 1);


                        dbContext.Tbl70006projectGroups.Add(model);
                        await dbContext.SaveChangesAsync();
                        await _userActionLogger.LogAsync(
                         module: "IMS > Save Or Update Project Group",
                         actionDetail: $"Saved Update Project Group  {model.ProjectGroupId}",
                         documentNo: $"{model.ProjectGroupId}"
                        );

                        return Ok(new { success = true, message = "Saved successfully", id = model.ProjectGroupId });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in SaveOrUpdateProjectGroup: {ex}");
                    return StatusCode(500, new { success = false, message = "An error occurred while saving Project Group." });
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
                    var record = dbContext.Tbl70006projectGroups.FirstOrDefault(x => x.ProjectGroupId == key);
                    if (record == null)
                        return NotFound();

                    dbContext.Tbl70006projectGroups.Remove(record);
                    dbContext.SaveChanges();
                    _userActionLogger.LogAsync(module: "IMS > Delete  ",
                      actionDetail: $"Deleted  {key}",
                      documentNo: $"{key}"
                    );
                    return Ok();
                }

                return Unauthorized(new { success = false, message = "Invalid tenant" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in DeleteProjectGroup: {ex}");
                return StatusCode(500, new { message = "An error occurred while fetching the data.", ex });
            }
        }
        //Projects Form Controllers
        [HttpGet]
        public IActionResult GetLatestProjectCode(string projectCode)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var latestClientCode = dbContext.Tbl70001projectMasters
                        .Where(c => c.ProjectId.StartsWith(projectCode + "-"))
                        .OrderByDescending(c => c.ProjectId)
                        .Select(c => c.ProjectId)
                        .FirstOrDefault();

                    return Ok(latestClientCode); // returns e.g., "SW-4"
                }

                return Unauthorized(new { message = "Invalid tenant." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetLatestProjectCode: {ex}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAllCostAllocationData()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var dbProjectGroup = await dbContext.Tbl201CostAllocationUnits
                       .Select(i => new

                       {
                           i.CostAllocationUnit,
                           i.CostAllocationUnitId,
                           i.CostAllocationGroup,
                           i.CostAllocationMasterGroup,
                        
                       })
                        .ToListAsync();

                    return Json(dbProjectGroup); // return raw data, paging/sorting done on client-side
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetAllCostAllocationData: {ex}");
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetProjectAllData()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var dbProjectGroup = await dbContext.Tbl70001projectMasters
                       .Select(i => new

                       {
                           i.ProjectId,
                           i.ProjectDescription,
                           i.CostAllocationUnitId,
                      

                       })
                        .ToListAsync();

                    return Json(dbProjectGroup); // return raw data, paging/sorting done on client-side
                }


                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProjectAllData: {ex}");
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveOrUpdateProject([FromBody] Tbl70001projectMaster model)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var existingRecord = await dbContext.Tbl70001projectMasters
                        .FirstOrDefaultAsync(x => x.ProjectId == model.ProjectId);

                    if (existingRecord != null)
                    {
                        // Update existing record
                    
                        existingRecord.ProjectGroupId = model.ProjectGroupId;
                        existingRecord.ProjectDescription = model.ProjectDescription;
                        existingRecord.CostAllocationUnitId = model.CostAllocationUnitId;


                        await dbContext.SaveChangesAsync();
                        await _userActionLogger.LogAsync(
                         module: "IMS > Save Or Update Project ",
                         actionDetail: $"Saved Update Project   {model.ProjectGroupId}",
                         documentNo: $"{model.ProjectGroupId}"
                        );

                        return Ok(new { success = true, message = "Updated successfully", id = existingRecord.ProjectGroupId });
                    }
                    else
                    {
                        // Insert new record
                      


                        dbContext.Tbl70001projectMasters.Add(model);
                        await dbContext.SaveChangesAsync();
                        await _userActionLogger.LogAsync(
                         module: "IMS > Save Or Update Project ",
                         actionDetail: $"Saved Update Project   {model.ProjectGroupId}",
                         documentNo: $"{model.ProjectGroupId}"
                        );

                        return Ok(new { success = true, message = "Saved successfully", id = model.ProjectId });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in SaveOrUpdateProjectGroup: {ex}");
                    return StatusCode(500, new { success = false, message = ex.Message });
                }
            }

            return Unauthorized(new { success = false, message = "Invalid tenant" });
        }
        [HttpDelete]
        public IActionResult Delete1(string key)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var record = dbContext.Tbl70001projectMasters.FirstOrDefault(x => x.ProjectId == key);
                    if (record == null)
                        return NotFound();


                    dbContext.Tbl70001projectMasters.Remove(record);
                    dbContext.SaveChanges();
                    _userActionLogger.LogAsync(module: "IMS > Delete  ",
                      actionDetail: $"Deleted  {key}",
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
