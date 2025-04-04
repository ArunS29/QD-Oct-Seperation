using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QD.ERP.Web.DAL.Entities;
using System.Collections.Generic;
using System.Linq;

public class EmailComposeModel : PageModel
{
    private readonly ERPMasterWtDataContext _context;

    public EmailComposeModel(ERPMasterWtDataContext context)
    {
        _context = context;
    }

    // Get Categories
    [HttpGet]
    public JsonResult OnGetCategories()
    {
        var categories = _context.EmailTemplates
            .Select(t => t.Category)
            .Distinct()
            .OrderBy(c => c)
            .Select(c => new
            {
                Id = c, // Assuming Category is a string; adjust if it's an ID-based lookup
                Name = c
            })
            .ToList();

        return new JsonResult(categories);
    }

    // Get Templates for a Selected Category
    [HttpGet]
    public JsonResult OnGetTemplates(string category)
    {
        if (string.IsNullOrEmpty(category))
        {
            return new JsonResult(new { success = false, message = "Category is required." });
        }

        var templates = _context.EmailTemplates
            .Where(t => t.Category == category)
            .Select(t => new
            {
                Id = t.Id,
                Name = t.TemplateName,
                Subject = t.Subject,
                Body = t.Body
            })
            .ToList();

        return new JsonResult(templates);
    }
}
