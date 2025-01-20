using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.DAL.Entities;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ChartOfCostCentersController : Controller
    {
        private ERPMasterWtDataContext _context;
        public ChartOfCostCentersController(ERPMasterWtDataContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetChartOfCostCenters(DataSourceLoadOptions loadOptions)
        {
            try
            {
                // Base query
                var query = _context.Qry20108ChartOfCostCenters.Select(i => new
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



                // Apply the DevExtreme DataSourceLoader with sorting, filtering, and grouping from the request
                var result = await DataSourceLoader.LoadAsync(query, loadOptions);

                return Json(result);
            }
            catch (Exception ex)
            {
                // Log the exception (logging mechanism depends on your setup, e.g., Serilog, NLog, etc.)
                // _logger.LogError(ex, "An error occurred while processing the Get method."); 

                // Return a generic error response
                return StatusCode(500, new { message = "An error occurred while processing your request.", details = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UpdateIsDisabled(string id, bool isDisabled)
        {
            try
            {
                // Fetch the record from the database using the provided id
                var record = _context.Qry20108ChartOfCostCenters.FirstOrDefault(cc => cc.CostAllocationUnitId == id);

                if (record == null)
                {
                    return Json(new { success = false, message = "Record not found." });
                }

                // Update the IsDisabled property
                record.IsDisabled = isDisabled;

                // Save changes to the database
                _context.SaveChanges();

                return Json(new { success = true, message = "Record updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult DeleteCostCenter(string id)
        {
            try
            {
                // Check if the provided id is for the Common Overheads (ADMIN-0001)
                if (id == "ADMIN-0001")
                {
                    // Return a specific error message if it's the Common Overheads cost center
                    return Json(new { success = false, message = "Cost Center for Common Overheads cannot be removed from the database. Overheads is default." });
                }

                // Check if there are any transactions associated with the CostCenterCode in tbl201CostAllocationMaster
                var transactionsExist = _context.Tbl201CostAllocationMasters
                    .Any(c => c.CostAllocationUnitId == id);  // Modify this based on actual field names

                if (transactionsExist)
                {
                    // Return an alert message if transactions are found
                    return Json(new { success = false, message = "Cost Center has transactions posted. Please remove them before deleting." });
                }

                // Find the record in the database by its CostAllocationUnitId
                var costCenter = _context.Tbl201CostAllocationUnits.FirstOrDefault(c => c.CostAllocationUnitId == id);
                if (costCenter == null)
                {
                    // Return an error if the record doesn't exist
                    return Json(new { success = false, message = "Cost Center not found." });
                }

                // Remove the record
                _context.Tbl201CostAllocationUnits.Remove(costCenter);
                _context.SaveChanges();

                // Return a success response
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Return an error response with the exception message
                return Json(new { success = false, message = ex.Message });
            }
        }



        [HttpGet]
        public IActionResult GetCostCenterById(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return Json(new { success = false, message = "Invalid Cost Center ID." });
                }

                var costCenter = _context.Tbl201CostAllocationUnits.FirstOrDefault(c => c.CostAllocationUnitId == id);
                if (costCenter == null)
                {
                    return Json(new { success = false, message = "Cost Center not found." });
                }

                return Json(new { success = true, data = costCenter });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdateCostCenter(Tbl201CostAllocationUnit model)
        {
            try
            {
                if (model == null || string.IsNullOrEmpty(model.CostAllocationUnitId))
                {
                    return Json(new { success = false, message = "Invalid data." });
                }

                var costCenter = _context.Tbl201CostAllocationUnits.FirstOrDefault(c => c.CostAllocationUnitId == model.CostAllocationUnitId);
                if (costCenter == null)
                {
                    return Json(new { success = false, message = "Cost Center not found." });
                }

                // Update the fields
                costCenter.CostAllocationUnit = model.CostAllocationUnit;
                costCenter.CostAllocationGroup = model.CostAllocationGroup;
                costCenter.IsDisabled = model.IsDisabled;
                costCenter.CostCenterIncharge = model.CostCenterIncharge;
                costCenter.CostUnitRemarks = model.CostUnitRemarks;
                costCenter.CostAllocationMasterGroup = model.CostAllocationMasterGroup;
                costCenter.ProjectMasterCode = model.ProjectMasterCode;
                costCenter.BranchCode = model.BranchCode;
                // Save changes
                _context.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

    }
}
