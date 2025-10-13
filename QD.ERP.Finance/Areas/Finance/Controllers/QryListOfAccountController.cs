using QD.ERP.Shared.DAL.Entities;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QD.ERP.Shared.Service;

namespace QDERPWeb.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class QryListOfAccountController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<QryListOfAccountController> _logger;

        public QryListOfAccountController(ILogger<QryListOfAccountController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var qryListOfAccountlists = dbContext.Qry201ListOfAccounts.Where(p => p.AccountId != null).Select(i => new
                {
                    i.MasterGroupId,
                    i.MasterGroup,
                    i.AccountGroup,
                    i.AccountGroupId,
                    i.AccountId,
                    i.AccountHead,
                    i.AccountHeadArabic,
                    i.ReferenceNo,
                    i.IsLedgerObselete
                });

                return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
    }
}
