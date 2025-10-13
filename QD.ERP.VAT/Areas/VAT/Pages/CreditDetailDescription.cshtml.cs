using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QD.ERP.Shared.DAL.Entities;
using QD.ERP.Web.Service;
using System.Linq;
using System.Threading.Tasks;

namespace QD.ERP.Web.Areas.VAT.Pages
{
    public class CreditDetailDescriptionModel : PageModel
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;

        public CreditDetailDescriptionModel(TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
        }

        [HttpGet]
        public async Task<JsonResult> OnGetDetailedDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return new JsonResult("");

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                // query DB for matching description
                var detailed = await Task.Run(() =>
                    dbContext.Tbl20164GoodsAndServicesMasters
                        .Where(x => x.Gsdescrpition == description)
                        .Select(x => x.GsdetailedDesc)
                        .FirstOrDefault()
                );

                return new JsonResult(detailed ?? "");
            }

            return new JsonResult(""); // fallback if tenant/db not resolved
        }

        public void OnGet()
        {
        }
    }
}
