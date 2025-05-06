using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.Service; // Assuming EncryptionHelper is in this namespace

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Area("Finance")]
    public class NavigationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        // Action to handle navigation with encrypted view name
        public IActionResult Navigate(string tenantName, string encryptedViewName)
        {
            try
            {
                // Decrypt the view name
                string viewName = EncryptionHelper.Decrypt(encryptedViewName);

                // Render the decrypted view
                return View(viewName);
            }
            catch
            {
                // Handle decryption errors (e.g., invalid or tampered encryptedViewName)
                return RedirectToAction("Error", "Home");
            }
        }
    }
}
