
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QD.ERP.Shared.DAL.Entities;
using QD.ERP.Shared.Service;
using System.Linq;
using System.Threading.Tasks;

namespace QD.ERP.Finance.Areas.Finance.Controllers
{
    [Route("Finance/api/[controller]/[action]")]
    [ApiController]
    public class SalaryPayableDetailsController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<SalaryPayableDetailsController> _logger;

        public SalaryPayableDetailsController(ILogger<SalaryPayableDetailsController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var qry20164salarypayableledgermaster = dbContext.Qry20164SalaryPayableLedgerMasters.Select(i => new
                {
                    i.ReferenceNo,
                    i.EmployeeNo,
                    i.EmployeeName,
                    i.Amount,
                    i.DrCr,
                    i.VoucherNo,
                    i.VoucherDate,
                    i.EntryNarration
                });

                return Json(await DataSourceLoader.LoadAsync(qry20164salarypayableledgermaster, loadOptions));
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
    }
}












