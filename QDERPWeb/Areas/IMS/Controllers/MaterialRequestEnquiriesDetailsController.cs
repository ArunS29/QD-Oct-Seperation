using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System.Globalization;
using System.Net;

namespace QD.ERP.Web.Areas.IMS.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class MaterialRequestEnquiriesDetailsController : Controller
	{
		private readonly TenantDbContextHelper _tenantDbContextHelper;
		private readonly ILogger<MaterialRequestEnquiriesDetailsController> _logger;

		public MaterialRequestEnquiriesDetailsController(ILogger<MaterialRequestEnquiriesDetailsController> logger, TenantDbContextHelper tenantDbContextHelper)
		{
			_tenantDbContextHelper = tenantDbContextHelper;
			_logger = logger;
		}
		[HttpGet]
		public async Task<ActionResult> GetVatCreditNoteDetail2s(string frmDate, string toDate)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					if (!DateTime.TryParseExact(frmDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime from))
						return BadRequest("Invalid from date format. Use MM/dd/yyyy.");

					if (!DateTime.TryParseExact(toDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime to))
						return BadRequest("Invalid to date format. Use MM/dd/yyyy.");

					// Fetch records based on the date range
					var vatInvoices = await dbContext.Qry201807vatcreditNoteRegisterMainViews
						.FromSqlRaw("SELECT * FROM qry201_807VATCreditNoteRegisterMainView WHERE CreditNoteDate BETWEEN @p0 AND @p1", from, to)
						.ToListAsync();

					return Json(vatInvoices);
				}
				catch (Exception ex)
				{
					return StatusCode(500, $"Internal server error: {ex.Message}");
				}
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
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

					var data = await dbContext.Qry60604purchaseRequestViewMasters
						.Where(x => x.Mprdate >= from && x.Mprdate <= to)
						.ToListAsync();

					return Ok(data);
				}
				catch (Exception ex)
				{
					return StatusCode(500, $"Internal server error: {ex.Message}");
				}
			}

			return Unauthorized(new { message = "Invalid tenant." });
		}
        [HttpGet]
        public IActionResult Get(DataSourceLoadOptions loadOptions, string Mprno)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
            {
                var data = dbContext.Qry60604purchaseRequestViewMasters.AsQueryable();

                if (!string.IsNullOrEmpty(Mprno))
                {
                    data = data.Where(item => item.Mprno == Mprno);
                }

                var result = DataSourceLoader.Load(data, loadOptions);
                return Json(result);
            }
            catch (Exception ex)
            {
                // Log the exception details here as needed

                // Return a JSON response with error details and a 500 status code
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return Json(new
                {
                    error = "An error occurred while processing your request.",
                    details = ex.Message
                });
            }
            }

            return Unauthorized(new { message = "Invalid tenant." });
        }

    }
}
