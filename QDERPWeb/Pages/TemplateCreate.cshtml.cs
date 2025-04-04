using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QD.ERP.Web.DAL.Entities;
using System.ComponentModel.DataAnnotations;

public class TemplateCreateModel : PageModel
{
    private readonly ERPMasterWtDataContext _context;

    public TemplateCreateModel(ERPMasterWtDataContext context)
    {
        _context = context;
    }

    [BindProperty]
    public EmailTemplate Template { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        Template.Status = Template.Status ?? "Active";
        _context.EmailTemplates.Add(Template);
        await _context.SaveChangesAsync();
        return RedirectToPage("/EmailCompose");
    }
}
