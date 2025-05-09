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
    public class MaterialReceiptsController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<MaterialReceiptsController> _logger;

        public MaterialReceiptsController(ILogger<MaterialReceiptsController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetMaterialReceipt(DateTime? fromDate, DateTime? toDate)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var query = dbContext.Qry60504materialReceiptViewMasters.AsQueryable();


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
                query = query.Where(i => i.ReceiptDate >= fromDate && i.ReceiptDate <= toDate);

                // Fetching the data
                var data = await query.Select(i => new
                {
                    i.ReceiptNo,
                    i.ReceiptDate,
                    i.SupplierCode,
                    i.SupplierName,
                    i.OurPurchaseOrderNo,
                    i.VatpurchaseBillNo,
                    i.IsSubmitted,
                    i.IsVerified,
                    i.IsApproved,
                    i.NoOfItems,
                    i.TotalBeforeTax,
                    i.TotalDiscount,
                    i.TotalAfterDiscount,
                    i.TotalTaxAmount,
                    i.TotalWithTax,
                  
                }).ToListAsync();

                return Json(data);
            }

            return Unauthorized(new { message = "Invalid tenant." });
        }

    }
}
