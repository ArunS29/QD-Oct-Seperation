using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QD.ERP.Web.Pages
{
    public class RichTextEditor2Model : PageModel
    {
        public void OnGet()
        {
        }

        public IActionResult OnPostExport(string base64, string fileName, DevExpress.AspNetCore.RichEdit.DocumentFormat format, string reason)
        {
            byte[] fileContents = Convert.FromBase64String(base64);

            // Just acknowledge success (no payload)
            return new OkResult();

            // Or if you want to send something back:
            // return new OkObjectResult(new { fileName, length = fileContents.Length, reason });
        }
    }
}
