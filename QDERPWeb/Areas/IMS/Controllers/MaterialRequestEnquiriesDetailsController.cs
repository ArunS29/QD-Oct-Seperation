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
					_logger.LogError(ex, "Error fetching VAT credit note details");
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
					_logger.LogError($"An error occurred while fetching the data : {ex.Message}");
					return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
				}
            }

            return Unauthorized(new { message = "Invalid tenant." });
        }
		[HttpGet]
		public async Task<IActionResult> GetPurchaseRequestDetails(DateTime? fromDate, DateTime? toDate)
		{
			try
			{
				if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				{
					var query = dbContext.Qry60609purchaseRequestItemDetails.AsQueryable();


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
					query = query.Where(i => i.Mprdate >= fromDate && i.Mprdate <= toDate);

					// Fetching the data
					var data = await query.Select(m=> new
					{
						m.Mprno,
						m.Mprdate,
						m.SubmittedOn,
						m.IsVerified,
						m.IsApproved,
						m.ApprovedOn,
						m.Gscode,
						m.Gsdescrpition,
						m.UnitDesc,
						m.QtyRequested,
						m.RequestedNameRef,
						m.Pono,
						m.ReceivedQty,
						m.ReceiptDate,
						m.ReceiptNo,
						m.ReceiptStatus,
						m.TypeOfRequest,
						m.ProjectDescription,
						m.BalanceToIssue,
						m.TotalIssuedQty,
						m.BalanceToReceive

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
		[HttpPost]
		public IActionResult DeletePurchaseRequest(string Mprno)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					var prMaster = dbContext.Tbl60601purchaseRequestMasters
						.FirstOrDefault(pr => pr.Mprno == Mprno);

					if (prMaster == null)
					{
						return Json(new { success = false, message = "Request/Enquiry not found." });
					}

					if (prMaster.IsSubmitted.HasValue && prMaster.IsSubmitted.Value)
					{
						return Json(new { success = false, message = "Request/Enquiry is already submitted. You cannot delete the submitted Request/Enquiry." });
					}
					if (prMaster.IsApproved.HasValue && prMaster.IsApproved.Value)
					{
						return Json(new { success = false, message = "Request/Enquiry is already approved. You cannot delete the approved Request/Enquiry." });
					}

					

					// Delete child records
					var prChildren = dbContext.Tbl60602purchaseRequestChildren
						.Where(child => child.Mprno == Mprno);
					dbContext.Tbl60602purchaseRequestChildren.RemoveRange(prChildren);

					// Delete master record
					dbContext.Tbl60601purchaseRequestMasters.Remove(prMaster);

					// Delete associated documents
					//DeleteDocumentPDF(Mprno, "VoucherScanned\\IMSEnquiry");

					dbContext.SaveChanges();
					
					// Log the deletion
					//InsertUserEntryLogSheet("IMS Purchase Request", $"IMS Purchase Request Ref No. {Mprno} has been deleted by User ID: {User.Identity.Name}.", User.Identity.Name, Mprno);

					return Json(new { success = true, message = "Request/Enquiry has been successfully removed from the database." });
				}
				catch (Exception ex)
				{
					// Log the exception as needed
					return Json(new { success = false, message = "An error occurred while deleting the Request/Enquiry." });
				}
			}

			return Json(new { success = false, message = "Invalid tenant context." });
		}

	}
}
