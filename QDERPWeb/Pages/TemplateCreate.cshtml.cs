using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

public class TemplateCreateModel : PageModel
{
    [BindProperty]
    [Required]
    public string TemplateName { get; set; }

    [BindProperty]
    [Required]
    public string Subject { get; set; }

    [BindProperty]
    [Required]
    public string Body { get; set; }

    [BindProperty]
    public string Status { get; set; } = "Active";

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Save the template to the database (or any storage)
        TempData["Success"] = "Template saved successfully!";
        return RedirectToPage("/Templates/Create");
    }
}
