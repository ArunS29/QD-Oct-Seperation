using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.DAL.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

public class TemplateCreateModel : PageModel
{
    private readonly ERPMasterWtDataContext _context;

    public TemplateCreateModel(ERPMasterWtDataContext context)
    {
        _context = context;
    }

    [BindProperty]
    public EmailTemplate Template { get; set; } = new EmailTemplate();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            // Trim and ensure required fields are properly formatted
            Template.TemplateName = Template.TemplateName?.Trim();
            Template.Subject = Template.Subject?.Trim();
            Template.Body = Template.Body?.Trim();
            Template.Status = Template.Status?.Trim() ?? "Active";

            _context.EmailTemplates.Add(Template);
            await _context.SaveChangesAsync();

            return RedirectToPage("/EmailCompose");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "An error occurred while saving the template. Please try again.");
            Console.WriteLine($"Error saving template: {ex.Message}");
            return Page();
        }
    }
}
