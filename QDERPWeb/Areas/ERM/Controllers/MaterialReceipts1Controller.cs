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

namespace QD.ERP.Web.Areas.ERM.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MaterialReceipts1Controller : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<MaterialReceipts1Controller> _logger;

        public MaterialReceipts1Controller(ILogger<MaterialReceipts1Controller> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetMaterialReceipt(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var query = dbContext.Qry40901supplierSummInvoiceViews.AsQueryable();


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
                    query = query.Where(i => i.SuppTimesheetSummaryDate >= fromDate && i.SuppTimesheetSummaryDate <= toDate);

                    // Fetching the data
                    var data = await query.Select(i => new
                    {
                        i.InvoiceEffectiveDate,
                        i.Pono,
                        i.SupplierCode,
                        i.SupplierName,
                        i.ReferenceNo,
                        i.SuppTimesheetSummaryDate,
                        i.SuppTimesheetSummaryNo,
                        i.VatpurchaseVoucherNo,

                    }).ToListAsync();

                    return Json(data);
                }

                return Unauthorized(new { message = "Invalid tenant." });
            }
            catch (Exception ex)
            {
				_logger.LogError($"An error occurred while fetching the data : {ex.Message}");
				return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
			}
        }
        [HttpGet]
        public async Task<IActionResult> GetInvoiceSumm(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var query = dbContext.Qry40113PropertyInvoiceMasterViews.AsQueryable();


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
                    query = query.Where(i => i.InvoiceDate >= fromDate && i.InvoiceDate <= toDate);

                    // Fetching the data
                    var data = await query.Select(i => new
                    {
                        i.InvoiceNo,
                        i.Pono,
                        i.InvoiceDate,
                        i.InvoiceEffectiveDate,
                        i.ClientName,
                        i.QuotationNo,
                        i.InvoicePeriod,
                        i.VatinvoiceNo,
                        i.GrossBeforeTax,

                    }).ToListAsync();

                    return Json(data);
                }

                return Unauthorized(new { message = "Invalid tenant." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while fetching the data : {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
            }
        }
     
        [HttpGet]
		public async Task<IActionResult> GetVatCreditNoteDetails(string frmDate, string toDate)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					if (!DateTime.TryParseExact(frmDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime from))
						return BadRequest("Invalid fromDate format. Use MM/dd/yyyy.");

					if (!DateTime.TryParseExact(toDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime to))
						return BadRequest("Invalid toDate format. Use MM/dd/yyyy.");

					var data = await dbContext.Qry60506materailReceiptDetails
						.Where(x => x.ReceiptDate >= from && x.ReceiptDate <= to)
						.ToListAsync();

					return Ok(data);
				}
				catch (Exception ex)
				{
					_logger.LogError($"An error occurred while fetching the data : {ex.Message}");
					return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
				}
			}

			return Unauthorized(new { message = "Invalid tenant." });
		}
	}
}
