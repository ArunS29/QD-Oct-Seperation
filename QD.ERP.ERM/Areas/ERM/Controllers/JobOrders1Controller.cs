using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Humanizer;
using DevExtreme.AspNet.Data.ResponseModel;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.Areas.Finance.Reports.Payable_Statements;
using QD.ERP.Shared.DAL.Entities;
using QD.ERP.Web.Service;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
namespace QD.ERP.Web.Areas.ERM.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class JobOrders1Controller : Controller
    {

        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<JobOrders1Controller> _logger;

        public JobOrders1Controller(ILogger<JobOrders1Controller> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetJobOrders(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var query = dbContext.Tbl60801jobOrderMasters.AsQueryable();


                    // Default dates if not provided
                    if (!fromDate.HasValue)
                    {
                        fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1); // Start of the current month
                    }

                    if (!toDate.HasValue)
                    {
                        toDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)); // End of the current month
                    }

                    // Filtering by date range
                    query = query.Where(i => i.JobOrderDate >= fromDate && i.JobOrderDate <= toDate);

                    // Fetching the data
                    var data = await query.Select(i => new
                    {
                        i.ValveType,
                        i.JobOrderNo,
                        i.JobOrderDate,
                        i.JobOrderDescription,
                        i.JobOrderStatus,
                        i.JobOrderType,
                        i.TagNo,
                        i.Size,
                        i.Class,
                        i.Operator,
                        i.Materials,
                        i.Qty,
                        i.Make,
                        i.ModelNo,
                        i.ItemSlNo,
                        i.WorkOrderNo,
                        //i.workorderDate
                    }).ToListAsync();

                    return Json(data);
                }

                return Unauthorized(new { message = "Invalid tenant." });
            }
            catch (Exception
            
             ex)
            {
                return StatusCode(500, new { message = "Internal server error." });
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
            }
        }
    }
}
