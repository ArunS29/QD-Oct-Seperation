using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QD.ERP.Web.Areas.Finance.Controllers;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;

namespace QD.ERP.Web.Areas.ERM.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ClientCategory1Controller : ControllerBase
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<MasterController> _logger;

        public ClientCategory1Controller(ILogger<MasterController> logger, TenantDbContextHelper tenantDbContextHelper)
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
                    var branches = await dbContext.Tbl30102ClientCategories.ToListAsync();
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
        public async Task<IActionResult> AddBranch([FromBody] Tbl30102ClientCategory branch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var exists = await dbContext.Tbl30102ClientCategories
                        .AnyAsync(x => x.ClientCategory == branch.ClientCategory);
                    if (exists)
                    {
                        return BadRequest(new { success = false, message = "ClientCategory already exists." });
                    }
                    short lastCode = dbContext.Tbl30102ClientCategories
                                         .OrderByDescending(c => c.ClientCategoryCode)
                                         .Select(c => c.ClientCategoryCode)
                                         .FirstOrDefault();

                    short newCode = (short)(lastCode + 1);


                    branch.ClientCategoryCode = newCode;

                    dbContext.Tbl30102ClientCategories.Add(branch);
                    await dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Client Category added successfully." });
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
        public async Task<IActionResult> UpdateBranch([FromBody] Tbl30102ClientCategory branch)
        {
            if (branch == null || branch.ClientCategoryCode == 0)
            {
                return BadRequest("Invalid ClientCategory data.");
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var existingBranch = await dbContext.Tbl30102ClientCategories
                        .FirstOrDefaultAsync(b => b.ClientCategoryCode == branch.ClientCategoryCode);

                    if (existingBranch == null)
                    {
                        return NotFound(new { success = false, message = $"Branch with code {branch.ClientCategoryCode} not found." });
                    }

                    var exists = await dbContext.Tbl30102ClientCategories
                        .AnyAsync(b => b.ClientCategory == branch.ClientCategory && b.CategoryCode != branch.CategoryCode);
                    if (exists)
                    {
                        return BadRequest(new { success = false, message = "Client Category already exists." });
                    }

                    if (!string.IsNullOrEmpty(branch.ClientCategory))
                    {
                        existingBranch.ClientCategory = branch.ClientCategory;
                    }

                    if (!string.IsNullOrEmpty(branch.CategoryCode))
                    {
                        existingBranch.CategoryCode = branch.CategoryCode;
                    }

                    dbContext.Entry(existingBranch).State = EntityState.Modified;
                    await dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Client Category updated successfully." });
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
        public async Task<IActionResult> DeleteBranchMaster([FromBody] Tbl30102ClientCategory branch)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var BranchToDelete = await dbContext.Tbl30102ClientCategories.FindAsync(branch.ClientCategoryCode);
                    if (BranchToDelete == null)
                    {
                        return NotFound();
                    }

                    dbContext.Tbl30102ClientCategories.Remove(BranchToDelete);
                    await dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Client Category deleted successfully." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in DeleteBranchMaster: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
        }
    }

