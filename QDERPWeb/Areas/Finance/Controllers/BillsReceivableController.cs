using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BillsReceivableController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<BillsReceivableController> _logger;

        public BillsReceivableController(ILogger<BillsReceivableController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions, string filterType = null)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var query = dbContext.Qry20105BillsReceivableAgeingViews.Select(i => new
                    {
                        i.AccountHeadNo,
                        i.AccountHead,
                        i.ReferenceNo,
                        i.VoucherRefNo,
                        i.VoucherDate,
                        i.InvoiceDueDate,
                        i.ReceivableAmount,
                        i.Received,
                        i.Balance,
                        i.NotOverdue,
                        i.Less30,
                        i.Less30to60,
                        i.Less60to90,
                        i.Less90to180,
                        i.Less180to365,
                        i.More365,
                        i.OverdueDays
                    });

                    if (filterType == "WithBalance")
                    {
                        query = query.Where(i => i.Balance > 0);
                    }
                    else if (filterType == "FullyReceived")
                    {
                        query = query.Where(i => i.Balance <= 0);
                    }

                    var result = await DataSourceLoader.LoadAsync(query, loadOptions);
                    return Json(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in Get: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while processing the request.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public IActionResult GenerateReport()
        {
            return RedirectToPage("/pulse/DocumentViewer", new { reportName = "XtraReportBillsReceivableAgeingReport" });
        }

        [HttpGet]
        public IActionResult GenerateAgeingreportsummaryReport()
        {
            return RedirectToPage("/pulse/DocumentViewer", new { reportName = "XtraReportAgeingreportsummary" });
        }
    }
}

