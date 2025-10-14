using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QD.ERP.Finance.Areas.Finance.Controllers;
using QD.ERP.Shared.DAL.Entities;
using QD.ERP.Shared.Service;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace QD.ERP.ERM.Areas.ERM.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class QuotationAnalysis1Controller : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<QuotationAnalysis1Controller> _logger;

        public QuotationAnalysis1Controller(ILogger<QuotationAnalysis1Controller> logger, TenantDbContextHelper tenantDbContextHelper)
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

        [HttpGet]
        public async Task<IActionResult> GetQuotationAnalysisPvg(string mprno)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var result = await dbContext.Qry60708quotationAnalysisPvgs
                        .Where(x => x.Mprno == mprno)
                        .Select(x => new
                        {
                            x.Mprno,
                            x.SupplierName,
                            x.Gsdescrpition,
                            x.UnitDesc,
                            x.QuotedQuantity,
                            x.UnitPrice,
                            x.ItemDiscount,
                            x.LineTotalBeforeTax,
                            x.LineTotalAfterDisc,
                            x.IsWonForPo
                        })
                        .ToListAsync();

                    return Ok(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching quotation analysis data for MPR No: {mprno}", mprno);
                    return StatusCode(500, "Internal server error.");
                }
            }

            return BadRequest("Invalid tenant context.");
        }


    }
}
