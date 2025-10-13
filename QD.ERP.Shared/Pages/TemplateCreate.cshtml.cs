using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QD.ERP.Shared.DAL.Entities;
using QD.ERP.Shared.Service;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

[Authorize] // 👈 Prevents anonymous users
public class TemplateCreateModel : PageModel
{
    private readonly TenantDbContextHelper _tenantDbContextHelper;

    public TemplateCreateModel(TenantDbContextHelper tenantDbContextHelper)
    {
        _tenantDbContextHelper = tenantDbContextHelper ?? throw new ArgumentNullException(nameof(tenantDbContextHelper));
    }

    [BindProperty]
    public EmailTemplate Template { get; set; } = new EmailTemplate();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
        {
            ModelState.AddModelError(string.Empty, "Unable to resolve tenant database context.");
            return Page();
        }

        try
        {
            Template.TemplateName = Template.TemplateName?.Trim();
            Template.Subject = Template.Subject?.Trim();
            Template.Body = Template.Body?.Trim();
            Template.Status = Template.Status?.Trim() ?? "Active";

            dbContext.EmailTemplates.Add(Template);
            await dbContext.SaveChangesAsync();

            // ✅ Use redirect with query flag
            return RedirectToPage("TemplateCreate", new { isSuccess = true });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "An error occurred while saving the template.");
            Console.WriteLine($"[TemplateCreateModel] Error: {ex}");
            return Page();
        }
    }
}
