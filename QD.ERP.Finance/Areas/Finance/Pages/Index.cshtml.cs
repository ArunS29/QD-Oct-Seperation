using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;


namespace QD.ERP.Finance.Areas.Finance.Pages
{
    public class IndexModel : PageModel
    {
        public string UserName { get; set; }

        public void OnGet()
        {
            // Example: Session key "UserName" should be set during login
            UserName = HttpContext.Session.GetString("UserName") ?? "Guest";
        }
    }
}
