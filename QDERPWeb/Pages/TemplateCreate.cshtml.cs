using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

public class TemplateCreateModel : PageModel
{
    private readonly TenantDbContextHelper _tenantDbContextHelper;

    public TemplateCreateModel(TenantDbContextHelper tenantDbContextHelper)
    {
        _tenantDbContextHelper = tenantDbContextHelper ?? throw new ArgumentNullException(nameof(tenantDbContextHelper));
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
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
            {
                ModelState.AddModelError(string.Empty, "Unable to resolve tenant database context.");
                return Page();
            }

            // Trim and sanitize
            Template.TemplateName = Template.TemplateName?.Trim();
            Template.Subject = Template.Subject?.Trim();
            Template.Body = Template.Body?.Trim();
            Template.Status = Template.Status?.Trim() ?? "Active";

            dbContext.EmailTemplates.Add(Template);
            await dbContext.SaveChangesAsync();

            IsSuccess = true;
            return Page(); // JS will pick this and close or reload
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "An error occurred while saving the template.");
            Console.WriteLine($"[TemplateCreateModel] Error: {ex}");
            return Page();
        }
    }
}
