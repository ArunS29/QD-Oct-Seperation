using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.Areas.Finance.Controllers;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;

namespace QD.ERP.Web.Areas.ERM.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SupplierCategoryController : ControllerBase
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<MasterController> _logger;

        public SupplierCategoryController(ILogger<MasterController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }



        [HttpGet]
        public async Task<IActionResult> GetBranches()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var branches = await dbContext.Tbl3019901SupplierCategories.ToListAsync();
                    return Ok(branches);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetBranches: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public async Task<IActionResult> AddBranch([FromBody] Tbl3019901SupplierCategory branch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var exists = await dbContext.Tbl3019901SupplierCategories
                        .AnyAsync(x => x.SupplierCategory == branch.SupplierCategory);
                    if (exists)
                    {
                        return BadRequest(new { success = false, message = "SupplierCategory already exists." });
                    }
                    //short lastCode = dbContext.Tbl3019901SupplierCategories
                    //                     .OrderByDescending(c => c.SupplierCategoryCode)
                    //                     .Select(c => c.SupplierCategoryCode)
                    //                     .FirstOrDefault();

                    //short newCode = (short)(lastCode + 1);


                    //branch.SupplierCategoryCode = newCode;

                    dbContext.Tbl3019901SupplierCategories.Add(branch);
                    await dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Supplier Category added successfully." });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in AddBranch: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBranch([FromBody] Tbl3019901SupplierCategory branch)
        {
            if (branch == null || branch.SupplierCategoryCode == 0)
            {
                return BadRequest("Invalid SupplierCategory data.");
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var existingBranch = await dbContext.Tbl3019901SupplierCategories
                        .FirstOrDefaultAsync(b => b.SupplierCategoryCode == branch.SupplierCategoryCode);

                    if (existingBranch == null)
                    {
                        return NotFound(new { success = false, message = $"Branch with code {branch.SupplierCategoryCode} not found." });
                    }

                    var exists = await dbContext.Tbl3019901SupplierCategories
                        .AnyAsync(b => b.SupplierCategory == branch.SupplierCategory && b.CategoryCode != branch.CategoryCode);
                    if (exists)
                    {
                        return BadRequest(new { success = false, message = "SupplierCategory already exists." });
                    }

                    if (!string.IsNullOrEmpty(branch.SupplierCategory))
                    {
                        existingBranch.SupplierCategory = branch.SupplierCategory;
                    }

                    if (!string.IsNullOrEmpty(branch.CategoryCode))
                    {
                        existingBranch.CategoryCode = branch.CategoryCode;
                    }

                    dbContext.Entry(existingBranch).State = EntityState.Modified;
                    await dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Supplier Category updated successfully." });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in UpdateBranch: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBranchMaster([FromBody] Tbl3019901SupplierCategory branch)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var BranchToDelete = await dbContext.Tbl3019901SupplierCategories.FindAsync(branch.SupplierCategoryCode);
                    if (BranchToDelete == null)
                    {
                        return NotFound();
                    }

                    dbContext.Tbl3019901SupplierCategories.Remove(BranchToDelete);
                    await dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Supplier Category deleted successfully." });
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                  _logger.LogError($"Error in GetProject: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
            }
        }
    }
}
