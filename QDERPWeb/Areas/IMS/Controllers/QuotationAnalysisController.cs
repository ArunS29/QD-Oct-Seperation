using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using QD.ERP.Web.Areas.Finance.Controllers;
using QD.ERP.Web.Areas.Finance.Models;
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
                            x.IsWonForPo,
                            x.Gscode,
                            x.RfqchildSlNo
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

        [HttpPost]
        public async Task<IActionResult> SetZeroToWon([FromBody] ZeroToWonDto dto)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    await dbContext.Database.ExecuteSqlRawAsync(
                        "EXEC sp607_03UpdateRFQChild_SetZeroToWon @p0, @p1",
                        dto.Mprno,
                        dto.GsCode
                    );

                    return Ok();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating IsWon for MPR No: {mprno}", dto.Mprno);
                    return StatusCode(500, "Internal server error.");
                }
            }

            return BadRequest("Invalid tenant context.");
        }
        [HttpPost]
        public async Task<IActionResult> UpdateIsWon([FromBody] UpdateIsWonDto dto)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    // ✅ Get the logged-in user from session
                    var updatedBy = HttpContext.Session.GetString("UserName");
                    if (string.IsNullOrEmpty(updatedBy))
                        return Unauthorized("User session expired or not available.");

                    await dbContext.Database.ExecuteSqlRawAsync(
                        "EXEC sp607_02UpdateRFQChild_IsWon @p0, @p1, @p2",
                        dto.RFQChildSlNo,
                        updatedBy,
                        dto.ReasonForSelection ?? ""
                    );


                    return Ok(new { message = "Line item has been set as Won for Ordering Process successfully." });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating IsWon for RFQChildSlNo: {rfqchildSlNo}", dto.RFQChildSlNo);
                    return StatusCode(500, "Internal server error occurred while updating RFQChild.");
                }
            }

            return BadRequest("Invalid tenant context.");
        }

    }
}
