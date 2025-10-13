using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QD.ERP.Finance.Areas.Finance.Pages
{
    public class LedgerDocumentsModel : PageModel
    {
        public void OnGet(string refNo)
        {
            ViewData["RefNo"] = refNo;
        }
    }
}
