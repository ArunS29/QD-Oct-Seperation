using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using QD.ERP.Web.Areas.Finance.Controllers;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace QD.ERP.Web.Areas.IMS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class QuotationAnalysisController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<QuotationAnalysisController> _logger;

        public QuotationAnalysisController(ILogger<QuotationAnalysisController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }
        [HttpGet]
        public async Task<ActionResult> GetQuotationAnalysis(DataSourceLoadOptions loadOptions)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    using (dbContext)
                    {
                        var ledgerAccounts = dbContext.Qry60707rfqissuedByMprno02s
                            .Where(p => p.Mprno != null)
                            .Select(i => new
                            {
                                i.Mprno,
                                i.NoOfRfqissued,
                                i.ClientName,
                                i.StoreName,
                                i.Mprdate,
                                i.RequestedBy,


                            });

                        return Json(await DataSourceLoader.LoadAsync(ledgerAccounts, loadOptions));
                    }
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetQuotationAnalysis");
                return StatusCode(500, new { message = "Internal Server Error", success = false });
            }
        }

    }
}
