using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.Areas.Finance.Models;
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
        [HttpPost]
        public async Task<IActionResult> UnlockMaterialRequest([FromBody] PurchaseRequestViewModel request)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized(new { success = false, message = "Invalid tenant." });

                if (string.IsNullOrWhiteSpace(request?.Mprno))
                    return BadRequest(new { success = false, message = "MPR No is required." });

                var existingEntity = await dbContext.Tbl60601purchaseRequestMasters
                    .FirstOrDefaultAsync(x => x.Mprno == request.Mprno);

                if (existingEntity == null)
                    return NotFound(new { success = false, message = "Request/Enquiry not found." });

                if (existingEntity.IsApproved != true && existingEntity.IsSubmitted != true && existingEntity.IsVerified != true)
                    return Ok(new { success = false, message = "Request/Enquiry is already unlocked." });

                existingEntity.IsApproved = false;
				existingEntity.IsSubmitted = false;
				existingEntity.IsVerified = false;

                dbContext.Tbl60601purchaseRequestMasters.Update(existingEntity);
                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Request/Enquiry has been unlocked successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while unlocking Request/Enquiry.");
                return StatusCode(500, new { success = false, message = "Internal server error", details = ex.Message });
            }
        }
        [HttpGet]
        public ActionResult<string> GetNewRequestNoApi()
        {
            try
            {
                var tenantName = HttpContext.Session.GetString("TenantName");
                if (string.IsNullOrWhiteSpace(tenantName))
                    return Unauthorized(new { message = "Tenant name not found in session.", success = false });

                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var company = dbContext.Tbl901CompanyDetails
                        .FirstOrDefault(c => c.CompanyNameShort == tenantName);

                    if (company == null)
                        return NotFound("Company not found.");

                    string abbrv = company.RequestAbbrv;
                    int yearDigits = company.InvoiceYearDigits ?? 0;
                    bool isReset = company.IsResetInvoiceInYear ?? false;
                    DateTime today = DateTime.Now;

                    string newMprNo = GetNewMprNo(abbrv, yearDigits, today, isReset, dbContext);
                    return Ok(newMprNo);
                }

                return BadRequest("Invalid tenant context.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in GetNewRequestNoApi: " + ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        private string GetNewMprNo(string abbr, int yearDigits, DateTime date, bool resetByYear, ERPMasterWtDataContext db)
        {
            try
            {
                var mprList = db.Tbl60601purchaseRequestMasters
                    .Where(m => m.Mprno != null &&
                                (!resetByYear || (m.Mprdate.HasValue && m.Mprdate.Value.Year == date.Year)))
                    .Select(m => m.Mprno)
                    .ToList();

                int maxRunNo = mprList
                    .Select(m => int.TryParse(m.Substring(m.Length - 5), out int num) ? num : 0)
                    .DefaultIfEmpty(0)
                    .Max() + 1;

                string yearPart = (yearDigits > 0) ? date.Year.ToString().Substring(4 - yearDigits) : "";
                return $"{abbr}{yearPart}-{maxRunNo.ToString("D5")}";
            }
            catch
            {
                string yearPart = (yearDigits > 0) ? date.Year.ToString().Substring(4 - yearDigits) : "";
                return $"{abbr}{yearPart}-00001";
            }
        }

        [HttpPost]
        public async Task<IActionResult> ClonePurchaseRequest([FromBody] string originalMprNo)
        {
            if (string.IsNullOrWhiteSpace(originalMprNo))
                return BadRequest(new { success = false, message = "Invalid original request no." });

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { success = false, message = "Tenant context not found." });

            try
            {
                // ✅ Retrieve tenantName from session
                var tenantName = HttpContext.Session.GetString("TenantName");
                if (string.IsNullOrWhiteSpace(tenantName))
                    return Unauthorized(new { success = false, message = "Tenant name not found in session." });

                var company = dbContext.Tbl901CompanyDetails.FirstOrDefault(c => c.CompanyNameShort == tenantName);
                if (company == null)
                    return NotFound("Company not found.");

                // ✅ Generate new MPR No
                string newMprNo = GetNewMprNo(
                    company.RequestAbbrv,
                    company.InvoiceYearDigits ?? 0,
                    DateTime.Now,
                    company.IsResetInvoiceInYear ?? false,
                    dbContext
                );

                string userName = HttpContext.Session.GetString("UserName") ?? "System";
                DateTime now = DateTime.Now;

                // ✅ Call the stored procedure
                var result = dbContext.Database.ExecuteSqlRaw(
                    "EXEC sp600_20InsertDuplicatePurchaseRequest @p0, @p1, @p2, @p3, @p4",
                    originalMprNo, newMprNo, now, userName, now
                );

                dbContext.SaveChanges();

                return Ok(new
                {
                    success = true,
                    message = "Purchase Request cloned successfully.",
                    newMprNo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error cloning purchase request: " + ex.Message);
                return StatusCode(500, new { success = false, message = "Clone failed.", detail = ex.Message });
            }
        }
        [HttpPost]
        public IActionResult ReviseRequest([FromBody] string originalMprNo)
        {
            if (string.IsNullOrWhiteSpace(originalMprNo))
                return BadRequest(new { success = false, message = "Original MPR No is required." });

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { success = false, message = "Tenant context not found." });

            try
            {
                var currentRevision = dbContext.Tbl60601purchaseRequestMasters
                    .Where(p => p.Mprno.StartsWith(originalMprNo))
                    .Max(p => p.MprrevisionId ?? 0);

                int nextRevision = currentRevision + 1;
                string newMprNo = $"{originalMprNo}-(R{currentRevision})";
                string user = HttpContext.Session.GetString("UserName") ?? "System";

                dbContext.Database.ExecuteSqlRaw(
                    "EXEC sp600_32CreateNewRevisedMPR @p0, @p1, @p2, @p3",
                    originalMprNo, newMprNo, nextRevision, user
                );

                dbContext.SaveChanges();

                return Ok(new { success = true, message = "Purchase Request revised successfully.", newMprNo });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error revising purchase request: " + ex.Message);
                return StatusCode(500, new { success = false, message = "Revision failed", detail = ex.Message });
            }
        }


    }
}
