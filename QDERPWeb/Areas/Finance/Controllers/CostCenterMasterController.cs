using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CostCenterMasterController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<CostCenterMasterController> _logger;

        public CostCenterMasterController(ILogger<CostCenterMasterController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetCostAllocationGroup(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var CostCenterMaster = dbContext.Tbl201CostAllocationUnits.Select(i => new
                    {
                        i.CostAllocationUnitId,
                        i.CostAllocationGroup,
                    });

                    return Json(await DataSourceLoader.LoadAsync(CostCenterMaster, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetCostAllocationGroup: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetCostAllocationMasterGroup(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var CostCenterMaster = dbContext.Tbl201CostAllocationUnits.Select(i => new
                    {
                        i.CostAllocationUnitId,
                        i.CostAllocationMasterGroup,
                    });

                    return Json(await DataSourceLoader.LoadAsync(CostCenterMaster, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetCostAllocationMasterGroup: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetProject(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var CostCenterMaster = dbContext.Qry70002projectsViewMasters.Select(i => new
                    {
                        i.ProjectId,
                        i.ProjectDescription,
                    });

                    return Json(await DataSourceLoader.LoadAsync(CostCenterMaster, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetProject: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetBranchCode(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var CostCenterMaster = dbContext.Tbl20115CompanyBranches.Select(i => new
                    {
                        i.BranchCode,
                        i.BranchName,
                        i.BranchNameAr
                    });

                    return Json(await DataSourceLoader.LoadAsync(CostCenterMaster, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetBranchCode: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public IActionResult CheckCostAllocationUnitIdExists([FromQuery] string costAllocationUnitId)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var existingUnit = dbContext.Tbl201CostAllocationUnits
                                            .FirstOrDefault(x => x.CostAllocationUnitId == costAllocationUnitId);
                if (existingUnit != null)
                {
                    return Ok(new { exists = true });
                }
                return Ok(new { exists = false });
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public IActionResult CheckCostAllocationUnitId(string costAllocationUnitId)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                if (string.IsNullOrEmpty(costAllocationUnitId))
                {
                    return BadRequest(new { exists = false });
                }

                var existingUnit = dbContext.Tbl201CostAllocationUnits
                                           .FirstOrDefault(x => x.CostAllocationUnitId == costAllocationUnitId);

                if (existingUnit != null)
                {
                    return Ok(new { exists = true });
                }
                else
                {
                    return Ok(new { exists = false });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public async Task<ActionResult> SaveCostCenterMaster([FromBody] Tbl201CostAllocationUnit VM)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                if (VM == null)
                {
                    return BadRequest(new { success = false, message = "Invalid data received." });
                }

                try
                {
                    var existingUnit = dbContext.Tbl201CostAllocationUnits
                        .Where(x => x.CostAllocationUnitId == VM.CostAllocationUnitId)
                        .FirstOrDefault();

                    if (existingUnit != null)
                    {
                        return BadRequest(new { success = false, message = "Cost Allocation Unit ID already exists." });
                    }

                    dbContext.Tbl201CostAllocationUnits.Add(VM);
                    await dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Cost Center Information Saved Successfully!" });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in SaveCostCenterMaster: {ex.Message}");
                    return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPut]
        public async Task<ActionResult> UpdateCostCenterMaster([FromBody] Tbl201CostAllocationUnit VM)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                if (VM == null)
                {
                    return BadRequest(new { success = false, message = "Invalid data received." });
                }

                try
                {
                    var existingUnit = await dbContext.Tbl201CostAllocationUnits
                        .FirstOrDefaultAsync(x => x.CostAllocationUnitId == VM.CostAllocationUnitId);

                    if (existingUnit == null)
                    {
                        return BadRequest(new { success = false, message = "Cost Allocation Unit ID does not exist." });
                    }

                    existingUnit.CostAllocationUnit = VM.CostAllocationUnit;
                    existingUnit.CostAllocationGroup = VM.CostAllocationGroup;
                    existingUnit.CostAllocationMasterGroup = VM.CostAllocationMasterGroup;
                    existingUnit.CostUnitRemarks = VM.CostUnitRemarks;
                    existingUnit.CostCenterIncharge = VM.CostCenterIncharge;
                    existingUnit.ProjectMasterCode = VM.ProjectMasterCode;
                    existingUnit.BranchCode = VM.BranchCode;
                    existingUnit.IsDisabled = VM.IsDisabled;
                    existingUnit.ModifiedBy = VM.ModifiedBy;
                    existingUnit.ModifiedOn = VM.ModifiedOn;

                    await dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Cost Center Information Updated Successfully!" });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in UpdateCostCenterMaster: {ex.Message}");
                    return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
    }
}

