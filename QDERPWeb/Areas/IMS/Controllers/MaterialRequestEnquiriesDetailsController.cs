using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using QD.ERP.Web.Services.Logging;
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
        private readonly IUserActionLogger _userActionLogger;


        public MaterialRequestEnquiriesDetailsController(ILogger<MaterialRequestEnquiriesDetailsController> logger, TenantDbContextHelper tenantDbContextHelper, IUserActionLogger userActionLogger)
		{
            _userActionLogger = userActionLogger;
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
                    _userActionLogger.LogAsync(module: "IMS > Delete Purchase Request  ",
                      actionDetail: $":Deleted Purchase Request {Mprno}",
                      documentNo: $"{Mprno}"
                    );

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
                await _userActionLogger.LogAsync(
                  module: "IMS > Unlock Material Receipt",
                  actionDetail: $":Unlocked Material Receipt  {request.Mprno}",
                  documentNo: $"{request.Mprno}"
                );

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
                _userActionLogger.LogAsync(module: "IMS > Clone Purchase Request",
                     actionDetail: $":Cloned Purchase Request {originalMprNo}",
                     documentNo: $"{originalMprNo}"
                   );

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
                _userActionLogger.LogAsync(module: "IMS > Revise Request",
                 actionDetail: $":Revised Request {originalMprNo}",
                 documentNo: $"{originalMprNo}"
                );
                var currentRevisionno = dbContext.Tbl60601purchaseRequestMasters
           .Where(p => p.Mprno.StartsWith(originalMprNo))
           .Max(p => p.MprrevisionNo);

                return Ok(new { success = true, message = "Purchase Request revised successfully.", newMprNo,RevisionNo= currentRevisionno });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error revising purchase request: " + ex.Message);
                return StatusCode(500, new { success = false, message = "Revision failed", detail = ex.Message });
            }
        }
        //New RFQ

        [HttpGet]
        public async Task<IActionResult> GetIsEnableMPRWorkflow()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var companyIdStr = HttpContext.Session.GetString("DefaultcompanyID");
                if (!int.TryParse(companyIdStr, out int companyId))
                    return BadRequest("Invalid Company ID");

                var isEnabled = await dbContext.Tbl901CompanyDetails02s
                    .Where(x => x.CompanyId == companyId)
                    .Select(x => x.IsEnableMprworkflow ?? false)
                    .FirstOrDefaultAsync();

                return Ok(isEnabled);
            }

            return BadRequest("Invalid tenant or DB context.");
        }



        [HttpGet]
        public async Task<IActionResult> CheckIfApproved(string mprNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {




                var isApproved = await dbContext.Tbl60601purchaseRequestMasters
                    .Where(x => x.Mprno == mprNo)
                    .Select(x => x.IsApproved ?? false)
                    .FirstOrDefaultAsync();

                return Ok(isApproved);
            }

            return BadRequest("Invalid tenant or DB context.");
        }

        [HttpPost]
        public async Task<IActionResult> ReceiveItemFully([FromBody] Tbl60401purchaseOrderMaster data)
        {
            try
            {
                string pono = data?.Pono;
                if (string.IsNullOrWhiteSpace(pono))
                    return BadRequest(new { success = false, message = "PO number is required." });

                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                    return Unauthorized(new { success = false, message = "Invalid tenant." });

                // --- Generate receiptNoteNo using company settings and last number in Tbl60501materialReceiptMasters ---
                string tenantName = HttpContext.Session.GetString("TenantName");
                if (string.IsNullOrWhiteSpace(tenantName))
                    return Unauthorized(new { message = "Tenant name not found in session.", success = false });

                var company = dbContext.Tbl901CompanyDetails
                    .FirstOrDefault(c => c.CompanyNameShort == tenantName);

                if (company == null)
                    return NotFound("Company not found.");

                string requestAbbrv = company.RequestAbbrv ?? "MRN";
                int yearInDigit = company.InvoiceYearDigits ?? 0;
                bool isResetByYear = company.IsResetInvoiceInYear ?? false;
                DateTime receiptDate = DateTime.Now;

                // Get the last receipt number for this year/company
                var receiptNumbers = dbContext.Tbl60501materialReceiptMasters
                    .Where(d => d.ReceiptNo != null && d.ReceiptNo.Length >= 5 &&
                                (!isResetByYear || (d.ReceiptDate.HasValue && d.ReceiptDate.Value.Year == receiptDate.Year)))
                    .Select(d => d.ReceiptNo)
                    .ToList();

                int maxRunningNumber = receiptNumbers
                    .Select(no => int.TryParse(no.Substring(no.Length - 5), out int num) ? num : 0)
                    .DefaultIfEmpty(0)
                    .Max();

                maxRunningNumber += 1;

                string strNewReceiptNoteNo = maxRunningNumber.ToString().PadLeft(5, '0');

                string strYear = receiptDate.Year.ToString();
                if (yearInDigit > 0)
                {
                    strYear = strYear.Substring(strYear.Length - yearInDigit, yearInDigit);
                }
                else
                {
                    strYear = "";
                }

                string receiptNoteNo = $"{requestAbbrv}{strYear}-{strNewReceiptNoteNo}";

                byte modeOfReceiptId = 1; // Set as needed
                string addedBy = HttpContext.Session.GetString("UserName") ?? "System";

                await dbContext.Database.ExecuteSqlRawAsync(
                    "EXEC sp600_16InsertToPartialReceiptFromPurchaseOrder @ReceiptNoteNo = {0}, @ModeOfReceiptID = {1}, @PONo = {2}, @AddedBy = {3}",
                    receiptNoteNo, modeOfReceiptId, pono, addedBy
                );

                return Ok(new { success = true, message = "Material Receipt created.", receiptNoteNo = receiptNoteNo, pono = pono });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Server error: " + ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> CreateRfqFromMpr([FromBody] string mprNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(mprNo))
                    return BadRequest(new { success = false, message = "MPR No is required." });

                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                    return Unauthorized(new { success = false, message = "Invalid tenant." });

                // 1. Get tenant name from session
                string tenantName = HttpContext.Session.GetString("TenantName");
                if (string.IsNullOrWhiteSpace(tenantName))
                    return Unauthorized(new { success = false, message = "Tenant name not found in session." });

                // 2. Get company settings
                var company = dbContext.Tbl901CompanyDetails.FirstOrDefault(c => c.CompanyNameShort == tenantName);
                if (company == null)
                    return NotFound(new { success = false, message = "Company not found." });

                string rfqAbbrv = company.Rfqabbrv ?? "RFQ";
                int yearDigits = company.InvoiceYearDigits ?? 0;
                bool isResetByYear = company.IsResetInvoiceInYear ?? false;
                DateTime currentDate = DateTime.Now;

                // 3. Generate new RFQ No
                var existingRfqNos = dbContext.Tbl60701rfqmasters
                    .Where(r => r.Rfqno != null && r.Rfqno.Length >= 5 &&
                               (!isResetByYear || (r.Rfqdate.HasValue && r.Rfqdate.Value.Year == currentDate.Year)))
                    .Select(r => r.Rfqno)
                    .ToList();

                int maxRunningNumber = existingRfqNos
                    .Select(no => int.TryParse(no.Substring(no.Length - 5), out int num) ? num : 0)
                    .DefaultIfEmpty(0)
                    .Max() + 1;

                string paddedNumber = maxRunningNumber.ToString().PadLeft(5, '0');
                string yearPart = yearDigits > 0 ? currentDate.Year.ToString().Substring(4 - yearDigits) : "";

                string rfqNo = $"{rfqAbbrv}{yearPart}-{paddedNumber}";

                // 4. Execute SP
                string addedBy = HttpContext.Session.GetString("UserName") ?? "System";

                await dbContext.Database.ExecuteSqlRawAsync(
                    "EXEC sp600_01InsertToRfqFromEnquiry @RFQNo = {0}, @MPRNo = {1}, @AddedBy = {2}",
                    rfqNo, mprNo, addedBy
                );

                // 5. Update MPR status
                var mpr = await dbContext.Tbl60601purchaseRequestMasters.FirstOrDefaultAsync(x => x.Mprno == mprNo);
                if (mpr != null)
                {
                    mpr.PurchaseRequestStatusId = 2; // Assuming '2' is the 'converted to RFQ' status
                    await dbContext.SaveChangesAsync();
                    await _userActionLogger.LogAsync(
                      module: "IMS > Create Rfq From Mpr",
                      actionDetail: $":Created Rfq From Mpr  {mprNo}",
                      documentNo: $"{mprNo}"
                    );
                }

                // 6. Return success
                return Ok(new { success = true, message = "RFQ created successfully.", rfqno = rfqNo, mprno = mprNo });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Server error: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuotationFromMpr([FromBody] string mprNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(mprNo))
                    return BadRequest(new { success = false, message = "MPR No is required." });

                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                    return Unauthorized(new { success = false, message = "Invalid tenant." });

                string tenantName = HttpContext.Session.GetString("TenantName");
                if (string.IsNullOrWhiteSpace(tenantName))
                    return Unauthorized(new { success = false, message = "Tenant name not found in session." });

                // Step 1: Get company info
                var company = dbContext.Tbl901CompanyDetails.FirstOrDefault(c => c.CompanyNameShort == tenantName);
                if (company == null)
                    return NotFound(new { success = false, message = "Company not found." });

                // Step 2: Get NoOfDigits for quotation from CompanyDetails02
                int noOfDigits = dbContext.Tbl901CompanyDetails02s
                                          .Where(x => x.CompanyId == company.CompanyId)
                                          .Select(x => x.NoOfDigitsToInventoryQuotation ?? 4)
                                          .FirstOrDefault();

                // Step 3: Generate Quotation No directly here
                string abbrv = company.QuotationAbbrv ?? "QT";
                int yearDigits = company.InvoiceYearDigits ?? 0;
                bool isResetByYear = company.IsResetInvoiceInYear ?? false;
                DateTime today = DateTime.Now;

                var existingQuoteNos = dbContext.Tbl60101quotationMasters
                    .Where(q => q.QuoteNo != null &&
                                q.QuoteNo.Length >= noOfDigits &&
                                (!isResetByYear || (q.QuoteDate.HasValue && q.QuoteDate.Value.Year == today.Year)))
                    .Select(q => q.QuoteNo)
                    .ToList();

                int maxRunningNo = existingQuoteNos
                    .Select(no => int.TryParse(no.Substring(no.Length - noOfDigits), out int num) ? num : 0)
                    .DefaultIfEmpty(0)
                    .Max() + 1;

                string paddedNumber = maxRunningNo.ToString().PadLeft(noOfDigits, '0');

                string yearPart = yearDigits > 0 ? today.Year.ToString().Substring(4 - yearDigits) : "";

                string quoteNo = $"{abbrv}{yearPart}-{paddedNumber}";

                // Step 4: Execute SP
                string addedBy = HttpContext.Session.GetString("UserName") ?? "System";

                await dbContext.Database.ExecuteSqlRawAsync(
                    "EXEC sp600_03InsertToQuotationFromEnquiry @QuotationNo = {0}, @MPRNo = {1}, @AddedBy = {2}",
                    quoteNo, mprNo, addedBy
                );

                // Step 5: Update MPR status
                var mpr = await dbContext.Tbl60601purchaseRequestMasters.FirstOrDefaultAsync(x => x.Mprno == mprNo);
                if (mpr != null)
                {
                    mpr.PurchaseRequestStatusId = 4; // Assume 3 = Quotation created
                    await dbContext.SaveChangesAsync();
                    await _userActionLogger.LogAsync(
                      module: "IMS > Created Quotation From Mpr",
                      actionDetail: $":Created Quotation From Mpr  {mprNo}",
                      documentNo: $"{mprNo}"
                    );
                }

                return Ok(new { success = true, message = "Quotation created successfully.", quoteno = quoteNo });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Server error: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateMaterialReceiptFromMpr([FromBody] string mprNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(mprNo))
                    return BadRequest(new { success = false, message = "MPR No is required." });

                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                    return Unauthorized(new { success = false, message = "Invalid tenant." });

                string tenantName = HttpContext.Session.GetString("TenantName");
                if (string.IsNullOrWhiteSpace(tenantName))
                    return Unauthorized(new { success = false, message = "Tenant name not found in session." });

                // Step 1: Get company details
                var company = dbContext.Tbl901CompanyDetails.FirstOrDefault(c => c.CompanyNameShort == tenantName);
                if (company == null)
                    return NotFound(new { success = false, message = "Company not found." });

                // Step 2: Generate Receipt No
                string abbrv = company.RequestAbbrv ?? "MR";
                int yearDigits = company.InvoiceYearDigits ?? 0;
                bool isResetByYear = company.IsResetInvoiceInYear ?? false;
                DateTime today = DateTime.Now;

                var existingNos = dbContext.Tbl60501materialReceiptMasters
                    .Where(r => r.ReceiptNo != null &&
                                r.ReceiptNo.Length >= 5 &&
                                (!isResetByYear || (r.ReceiptDate.HasValue && r.ReceiptDate.Value.Year == today.Year)))
                    .Select(r => r.ReceiptNo)
                    .ToList();

                int maxRunning = existingNos
                    .Select(no => int.TryParse(no.Substring(no.Length - 5), out int num) ? num : 0)
                    .DefaultIfEmpty(0)
                    .Max() + 1;

                string padded = maxRunning.ToString().PadLeft(5, '0');
                string yearPart = yearDigits > 0 ? today.Year.ToString().Substring(4 - yearDigits) : "";
                string receiptNo = $"{abbrv}{yearPart}-{padded}";

                string addedBy = HttpContext.Session.GetString("UserName") ?? "System";

                // Step 3: Call stored procedure
                byte modeOfReceiptId = 1; // Set in backend
                await dbContext.Database.ExecuteSqlRawAsync(
                    "EXEC sp600_31InsertToMaterialReceiptfromEnquiry @ReceiptNoteNo = {0}, @ModeOfReceiptID = {1}, @MPRNo = {2}, @AddedBy = {3}",
                    receiptNo, modeOfReceiptId, mprNo, addedBy
                );

                // ✅ Step 4: Update MPR status
                var mpr = await dbContext.Tbl60601purchaseRequestMasters.FirstOrDefaultAsync(x => x.Mprno == mprNo);
                if (mpr != null)
                {
                    mpr.PurchaseRequestStatusId = 8; // ✅ Set status for "Material Receipt Created"
                    await dbContext.SaveChangesAsync();
                    await _userActionLogger.LogAsync(
                      module: "IMS > Create Material Receipt From Mpr",
                      actionDetail: $":Created Material Receipt From Mpr  {mprNo}",
                      documentNo: $"{mprNo}"
                    );
                }

                return Ok(new
                {
                    success = true,
                    message = "Material Receipt created successfully.",
                    receiptNo = receiptNo
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Server error: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePOFromMpr([FromBody] string mprNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(mprNo))
                    return BadRequest(new { success = false, message = "MPR No is required." });

                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                    return Unauthorized(new { success = false, message = "Invalid tenant context." });

                string tenantName = HttpContext.Session.GetString("TenantName");
                string addedBy = HttpContext.Session.GetString("UserName") ?? "System";

                // 1. Generate new PO No
                var company = dbContext.Tbl901CompanyDetails.FirstOrDefault(c => c.CompanyNameShort == tenantName);
                var config = dbContext.Tbl901CompanyDetails02s.FirstOrDefault(c => c.CompanyId == company.CompanyId);
                string prefix = company.PurchaseOrderAbbrv ?? "PO";
                int yearDigits = company.InvoiceYearDigits ?? 0;
                int digits = config?.NoOfDigitsToInventoryQuotation ?? 5;
                bool resetYearly = company.IsResetInvoiceInYear ?? false;

                DateTime now = DateTime.Now;
                string year = yearDigits > 0 ? now.Year.ToString().Substring(4 - yearDigits) : "";
                string basePrefix = $"{prefix}{year}-";

                var existing = dbContext.Tbl60401purchaseOrderMasters
                    .Where(p => p.Pono.StartsWith(basePrefix))
                    .Select(p => p.Pono)
                    .ToList();

                int max = existing
                    .Select(no => int.TryParse(no.Substring(no.Length - digits), out int n) ? n : 0)
                    .DefaultIfEmpty(0)
                    .Max();

                string newPoNo = $"{basePrefix}{(max + 1).ToString().PadLeft(digits, '0')}";

                // 2. Execute stored procedure
                await dbContext.Database.ExecuteSqlRawAsync(
                    "EXEC sp600_01InsertToPOfromEnquiry @PONo = {0}, @MPRNo = {1}, @AddedBy = {2}",
                    newPoNo, mprNo, addedBy);

                // 3. Update MPR status (6 = Converted to PO)
                var mpr = await dbContext.Tbl60601purchaseRequestMasters.FirstOrDefaultAsync(x => x.Mprno == mprNo);
                if (mpr != null)
                {
                    mpr.PurchaseRequestStatusId = 6;
                    await dbContext.SaveChangesAsync();
                    await _userActionLogger.LogAsync(
                      module: "IMS > Create PO From Mpr",
                      actionDetail: $":Created PO From Mpr  {mprNo}",
                      documentNo: $"{mprNo}"
                    );
                }

                return Ok(new { success = true, poNo = newPoNo, message = "Purchase Order created successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error: " + ex.Message });
            }
        }


    }
}
