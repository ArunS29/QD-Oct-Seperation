using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Shared.DAL.Entities;
using QD.ERP.Shared.Models.DAL;
using System;
using System.Linq;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class EmailTemplateController : ControllerBase
{
    private readonly TenantDbContextHelper _tenantDbContextHelper;

    public EmailTemplateController(TenantDbContextHelper tenantDbContextHelper)
    {
        _tenantDbContextHelper = tenantDbContextHelper ?? throw new ArgumentNullException(nameof(tenantDbContextHelper));
    }

    // ✅ GET: api/email/categories - Fetch Unique Categories
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
            {
                return StatusCode(500, new { success = false, message = "Tenant not found or DbContext could not be created." });
            }

            if (dbContext.EmailTemplates == null)
            {
                return NotFound(new { success = false, message = "Email templates table not found in the database." });
            }

            var categories = await dbContext.EmailTemplates
                .Where(t => !string.IsNullOrEmpty(t.Category))
                .Select(t => t.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return categories.Any()
                ? Ok(categories)
                : NotFound(new { success = false, message = "No categories found." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Internal Server Error.", error = ex.Message });
        }
    }

    // ✅ GET: api/email/templateByCategory?category=Finance - Fetch a Single Template
    [HttpGet("templateByCategory")]
    public async Task<IActionResult> GetTemplateByCategory([FromQuery] string category)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return BadRequest(new { success = false, message = "Category is required." });
            }

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
            {
                return StatusCode(500, new { success = false, message = "Tenant not found or DbContext could not be created." });
            }

            var template = await dbContext.EmailTemplates
                .Where(t => t.Category == category)
                .Select(t => new
                {
                    t.Subject,
                    t.Body
                })
                .FirstOrDefaultAsync();

            if (template == null)
            {
                return NotFound(new { success = false, message = "No template found for this category." });
            }

            return Ok(template);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Internal Server Error.", error = ex.Message });
        }
    }
}
