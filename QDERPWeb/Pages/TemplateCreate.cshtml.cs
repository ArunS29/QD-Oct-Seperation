using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.DAL.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using QD.ERP.Web.Service; // Ensure this namespace includes TenantDbContextHelper

public class TemplateCreateModel : PageModel
{
    private readonly ERPMasterWtDataContext _context;
    private readonly TenantDbContextHelper _tenantDbContextHelper;

    public TemplateCreateModel(ERPMasterWtDataContext context, TenantDbContextHelper tenantDbContextHelper)
    {
        _context = context;
        _tenantDbContextHelper = tenantDbContextHelper;
    }

    [BindProperty]
    public EmailTemplate Template { get; set; } = new EmailTemplate();

    public bool IsSuccess { get; set; } = false; 

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out var _, out var dbContext))
            {
                // Trim input values
                Template.TemplateName = Template.TemplateName?.Trim();
                Template.Subject = Template.Subject?.Trim();
                Template.Body = Template.Body?.Trim();
                Template.Status = Template.Status?.Trim() ?? "Active";

                // Save the template to the tenant-specific database
                dbContext.EmailTemplates.Add(Template);
                await dbContext.SaveChangesAsync();

                IsSuccess = true;
                return Page(); // JS will alert and go back
            }
            else
            {
                throw new Exception("Unable to retrieve tenant context.");
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "An error occurred while saving the template. Please try again.");
            Console.WriteLine($"Error saving template: {ex.Message}");
            return Page();
        }
    }


}
