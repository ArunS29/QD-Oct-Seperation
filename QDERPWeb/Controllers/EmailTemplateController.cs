using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.DAL.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class EmailTemplateController : ControllerBase
{
    private readonly ERPMasterWtDataContext _context;

    public EmailTemplateController(ERPMasterWtDataContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    // ✅ GET: api/email/categories - Fetch Unique Categories
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            if (_context.EmailTemplates == null)
            {
                return NotFound(new { success = false, message = "Email templates table not found in the database." });
            }

            var categories = await _context.EmailTemplates
                .Where(t => !string.IsNullOrEmpty(t.Category))
                .Select(t => t.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync(); // ✅ Direct async DB call

            return categories.Any()
                ? Ok(categories)
                : NotFound(new { success = false, message = "No categories found." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Internal Server Error.", error = ex.Message });
        }
    }


    // ✅ NEW: GET api/email/templateByCategory?category=Finance - Fetch a Single Template
    [HttpGet("templateByCategory")]
    public async Task<IActionResult> GetTemplateByCategory([FromQuery] string category)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return BadRequest(new { success = false, message = "Category is required." });
            }

            var template = await _context.EmailTemplates
                .Where(t => t.Category == category)
                .Select(t => new
                {
                    t.Subject,
                    t.Body
                })
                .FirstOrDefaultAsync(); // ✅ Fetch only 1 record

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
