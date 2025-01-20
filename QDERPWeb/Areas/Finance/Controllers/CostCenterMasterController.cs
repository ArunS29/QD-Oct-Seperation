using DevExpress.Xpo;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QD.ERP.Web.DAL.Entities;


namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("/api/[controller]/[action]")]
    [ApiController]
    public class CostCenterMasterController : Controller
    {
        private ERPMasterWtDataContext _context;
        public CostCenterMasterController(ERPMasterWtDataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCostAllocationGroup(DataSourceLoadOptions loadOptions)
        {
            try
            {
                // Fetching the data from the context
                var CostCenterMaster = _context.Tbl201CostAllocationUnits.Select(i => new
                {
                    i.CostAllocationUnitId,
                    i.CostAllocationGroup,
                });

                // Return the data as a JSON response using DataSourceLoader to handle the load options
                return Json(await DataSourceLoader.LoadAsync(CostCenterMaster, loadOptions));
            }
            catch (Exception ex)
            {
                // Log the error (You can use your logging mechanism here, e.g., log to a file or database)
                // For now, we're just returning the error message
                return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCostAllocationMasterGroup(DataSourceLoadOptions loadOptions)
        {
            try
            {
                // Fetching the data from the context
                var CostCenterMaster = _context.Tbl201CostAllocationUnits.Select(i => new
                {
                    i.CostAllocationUnitId,
                    i.CostAllocationMasterGroup,
                });

                // Return the data as a JSON response using DataSourceLoader to handle the load options
                return Json(await DataSourceLoader.LoadAsync(CostCenterMaster, loadOptions));
            }
            catch (Exception ex)
            {
                // Log the error (You can use your logging mechanism here, e.g., log to a file or database)
                // For now, we're just returning the error message
                return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProject(DataSourceLoadOptions loadOptions)
        {
            try
            {
                // Fetching the data from the context
                var CostCenterMaster = _context.Qry70002projectsViewMasters.Select(i => new
                {
                    i.ProjectId,
                    i.ProjectDescription,
                });

                // Return the data as a JSON response using DataSourceLoader to handle the load options
                return Json(await DataSourceLoader.LoadAsync(CostCenterMaster, loadOptions));
            }
            catch (Exception ex)
            {
                // Log the error (you can use your logging mechanism here, e.g., log to a file or database)
                // For now, we're just returning the error message
                return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetBranchCode(DataSourceLoadOptions loadOptions)
        {
            try
            {
                // Fetching the data from the context
                var CostCenterMaster = _context.Tbl20115CompanyBranches.Select(i => new
                {
                    i.BranchCode,
                    i.BranchName,
                    i.BranchNameAr
                });

                // Return the data as a JSON response using DataSourceLoader to handle the load options
                return Json(await DataSourceLoader.LoadAsync(CostCenterMaster, loadOptions));
            }
            catch (Exception ex)
            {
                // Log the error (you can use your logging mechanism here, e.g., log to a file or database)
                // For now, we're just returning the error message
                return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
            }
        }



        [HttpGet]
        [Route("api/CostCenterMaster/CheckCostAllocationUnitIdExists")]
        public IActionResult CheckCostAllocationUnitIdExists([FromQuery] string costAllocationUnitId)
        {
            var existingUnit = _context.Tbl201CostAllocationUnits
                                        .FirstOrDefault(x => x.CostAllocationUnitId == costAllocationUnitId);
            if (existingUnit != null)
            {
                return Ok(new { exists = true });
            }
            return Ok(new { exists = false });
        }


        [HttpGet]
        public ActionResult CheckCostAllocationUnitId(string costAllocationUnitId)
        {
            if (string.IsNullOrEmpty(costAllocationUnitId))
            {
                return BadRequest(new { exists = false });
            }

            // Check if the CostAllocationUnitId exists in the database
            var existingUnit = _context.Tbl201CostAllocationUnits
                                       .FirstOrDefault(x => x.CostAllocationUnitId == costAllocationUnitId);

            if (existingUnit != null)
            {
                return Ok(new { exists = true }); // Return true if the ID exists
            }
            else
            {
                return Ok(new { exists = false }); // Return false if the ID does not exist
            }
        }
        [HttpPost]
        public async Task<ActionResult> SaveCostCenterMaster([FromBody] Tbl201CostAllocationUnit VM)
        {
            if (VM == null)
            {
                return BadRequest(new { success = false, message = "Invalid data received." });
            }

            try
            {
                // Check if the CostAllocationUnitId already exists in the database
                var existingUnit = _context.Tbl201CostAllocationUnits
                    .Where(x => x.CostAllocationUnitId == VM.CostAllocationUnitId)
                    .FirstOrDefault();

                if (existingUnit != null)
                {
                    return BadRequest(new { success = false, message = "Cost Allocation Unit ID already exists." });
                }

                // Add the new unit
                _context.Tbl201CostAllocationUnits.Add(VM);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Cost Center Information Saved Successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        [HttpPut]
        public async Task<ActionResult> UpdateCostCenterMaster([FromBody] Tbl201CostAllocationUnit VM)
        {
            if (VM == null)
            {
                return BadRequest(new { success = false, message = "Invalid data received." });
            }

            try
            {
                // Use the EntityFrameworkCore extension for FirstOrDefaultAsync
                var existingUnit = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                    .FirstOrDefaultAsync(_context.Tbl201CostAllocationUnits
                        .Where(x => x.CostAllocationUnitId == VM.CostAllocationUnitId));

                if (existingUnit == null)
                {
                    return BadRequest(new { success = false, message = "Cost Allocation Unit ID does not exist." });
                }

                // Update the existing unit with new values
                existingUnit.CostAllocationUnit = VM.CostAllocationUnit;
                existingUnit.CostAllocationGroup = VM.CostAllocationGroup;
                existingUnit.CostAllocationMasterGroup = VM.CostAllocationMasterGroup;
                existingUnit.CostUnitRemarks = VM.CostUnitRemarks;
                existingUnit.CostCenterIncharge = VM.CostCenterIncharge;
                existingUnit.ProjectMasterCode = VM.ProjectMasterCode;
                existingUnit.BranchCode = VM.BranchCode;
                existingUnit.IsDisabled = VM.IsDisabled;
                //existingUnit.CreatedBy= VM.CreatedBy;
                //existingUnit.CreatedOn= VM.CreatedOn;
                existingUnit.ModifiedBy = VM.ModifiedBy;
                existingUnit.ModifiedOn = VM.ModifiedOn;

                // Save changes to the database
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Cost Center Information Updated Successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }
        }


    }

}

