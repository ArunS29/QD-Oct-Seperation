using System;
using System.Linq;
using System.Threading.Tasks;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QD.ERP.Shared.DAL.Entities;
using QD.ERP.Shared.Service;
using QD.ERP.Shared.Services.Logging;

namespace QD.ERP.Finance.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ChartOfCostCentersController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<ChartOfCostCentersController> _logger;
        private readonly IUserActionLogger _userActionLogger;
        public ChartOfCostCentersController(ILogger<ChartOfCostCentersController> logger, IUserActionLogger userActionLogger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
            _userActionLogger = userActionLogger;
        }

        [HttpGet]
        public async Task<IActionResult> GetChartOfCostCenters(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var query = dbContext.Qry20108ChartOfCostCenters.Select(i => new
                    {
                        i.CostAllocationUnitId,
                        i.CostAllocationMasterGroup,
                        i.CostAllocationGroup,
                        i.CostAllocationUnit,
                        i.IsDisabled,
                        i.CostCenterIncharge,
                        i.CostUnitRemarks,
                        i.CreatedBy,
                        i.CreatedOn,
                        i.ModifiedBy,
                        i.ModifiedOn
                    });

                    var result = await DataSourceLoader.LoadAsync(query, loadOptions);
                    return Json(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetChartOfCostCenters: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while processing your request.", details = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public JsonResult UpdateIsDisabled(string id, bool isDisabled)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var record = dbContext.Qry20108ChartOfCostCenters.FirstOrDefault(cc => cc.CostAllocationUnitId == id);

                    if (record == null)
                    {
                        return Json(new { success = false, message = "Record not found." });
                    }

                    record.IsDisabled = isDisabled;
                    dbContext.SaveChanges();

                    return Json(new { success = true, message = "Record updated successfully." });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in UpdateIsDisabled: {ex.Message}");
                    return Json(new { success = false, message = "An error occurred: " + ex.Message });
                }
            }

            return Json(new { success = false, message = "Invalid tenant." });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCostCenterAsync(string id)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    if (id == "ADMIN-0001")
                    {
                        return Json(new { success = false, message = "Cost Center for Common Overheads cannot be removed from the database. Overheads is default." });
                    }

                    var transactionsExist = dbContext.Tbl201CostAllocationMasters.Any(c => c.CostAllocationUnitId == id);
                    if (transactionsExist)
                    {
                        return Json(new { success = false, message = "Cost Center has transactions posted. Please remove them before deleting." });
                    }

                    var costCenter = dbContext.Tbl201CostAllocationUnits.FirstOrDefault(c => c.CostAllocationUnitId == id);
                    if (costCenter == null)
                    {
                        return Json(new { success = false, message = "Cost Center not found." });
                    }

                    dbContext.Tbl201CostAllocationUnits.Remove(costCenter);
                    dbContext.SaveChanges();
                    await _userActionLogger.LogAsync(
                    module: "Finance > Chart of Cost Center",
                    actionDetail: $"Deleted: {costCenter.CostAllocationUnitId}",
                    documentNo: costCenter.CostAllocationUnitId
                    );

                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in DeleteCostCenter: {ex.Message}");
                    return Json(new { success = false, message = ex.Message });
                }
            }

            return Json(new { success = false, message = "Invalid tenant." });
        }

        [HttpGet]
        public IActionResult GetCostCenterById(string id)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    if (string.IsNullOrEmpty(id))
                    {
                        return Json(new { success = false, message = "Invalid Cost Center ID." });
                    }

                    var costCenter = dbContext.Tbl201CostAllocationUnits.FirstOrDefault(c => c.CostAllocationUnitId == id);
                    if (costCenter == null)
                    {
                        return Json(new { success = false, message = "Cost Center not found." });
                    }

                    return Json(new { success = true, data = costCenter });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetCostCenterById: {ex.Message}");
                    return Json(new { success = false, message = ex.Message });
                }
            }

            return Json(new { success = false, message = "Invalid tenant." });
        }

        [HttpPost]
        public IActionResult UpdateCostCenter(Tbl201CostAllocationUnit model)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    if (model == null || string.IsNullOrEmpty(model.CostAllocationUnitId))
                    {
                        return Json(new { success = false, message = "Invalid data." });
                    }

                    var costCenter = dbContext.Tbl201CostAllocationUnits.FirstOrDefault(c => c.CostAllocationUnitId == model.CostAllocationUnitId);
                    if (costCenter == null)
                    {
                        return Json(new { success = false, message = "Cost Center not found." });
                    }

                    costCenter.CostAllocationUnit = model.CostAllocationUnit;
                    costCenter.CostAllocationGroup = model.CostAllocationGroup;
                    costCenter.IsDisabled = model.IsDisabled;
                    costCenter.CostCenterIncharge = model.CostCenterIncharge;
                    costCenter.CostUnitRemarks = model.CostUnitRemarks;
                    costCenter.CostAllocationMasterGroup = model.CostAllocationMasterGroup;
                    costCenter.ProjectMasterCode = model.ProjectMasterCode;
                    costCenter.BranchCode = model.BranchCode;

                    dbContext.SaveChanges();

                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in UpdateCostCenter: {ex.Message}");
                    return Json(new { success = false, message = ex.Message });
                }
            }

            return Json(new { success = false, message = "Invalid tenant." });
        }
    }
}

