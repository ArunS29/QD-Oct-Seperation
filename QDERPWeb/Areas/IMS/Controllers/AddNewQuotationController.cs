using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using QD.ERP.Web.Services.Logging;
using System.Dynamic;

namespace QD.ERP.Web.Areas.IMS.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class AddNewQuotationController : Controller
	{
		private readonly TenantDbContextHelper _tenantDbContextHelper;
		private readonly ILogger<AddNewQuotationController> _logger;
        private readonly IUserActionLogger _userActionLogger;


        public AddNewQuotationController(ILogger<AddNewQuotationController> logger, TenantDbContextHelper tenantDbContextHelper, IUserActionLogger userActionLogger)
        {
            _userActionLogger = userActionLogger;
            _tenantDbContextHelper = tenantDbContextHelper;
			_logger = logger;
		}

		//[HttpGet]
		//public ActionResult<string> GetNewDebitNoteNoApi()
		//{
		//	try
		//	{
		//		if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
		//		{


		//			var company = dbContext.Tbl901CompanyDetails
		//								   .FirstOrDefault(c => c.CompanyNameShort == "Pulse Infotech");


		//			if (company == null)
		//			{
		//				return NotFound("Company not found.");
		//			}

		//			string invoiceAbbrv = company.InvoiceAbbrv;
		//			int invoiceYearDigits = company.InvoiceYearDigits ?? 0;

		//			bool isResetInvoiceInYear = company.IsResetInvoiceInYear ?? false;

		//			DateTime invoiceDate = DateTime.Now;



		//			// Step 4: Generate New Debit Note No
		//			string newDebitNoteNo = GetNewDebitNoteNo(invoiceAbbrv, invoiceYearDigits, invoiceDate, isResetInvoiceInYear, dbContext);

		//			return Ok(newDebitNoteNo);
		//		}
		//		else
		//		{
		//			return BadRequest("Tenant or DB Context not found.");
		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		return StatusCode(500, "Internal server error: " + ex.Message);
		//	}
		//}

		[HttpGet]
		public ActionResult<string> GetNewDebitNoteNoApi()
		{
			try
			{
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    string defaultCompanyString = HttpContext.Session.GetString("DefaultcompanyID") ?? "";
                    byte defaultCompanyByte = 0; // or any default value you want

                    if (!string.IsNullOrEmpty(defaultCompanyString))
                    {
                        // Safest way (avoids exceptions):
                        byte.TryParse(defaultCompanyString, out defaultCompanyByte);
                        // Now defaultCompanyByte holds the parsed value, or 0 if parsing failed.
                    }

                    // Now use defaultCompanyByte as needed


                    byte companyId = defaultCompanyByte;

                    var company = dbContext.Tbl901CompanyDetails
                   .FirstOrDefault(c => c.CompanyId == companyId);

                    if (company == null)
					{
						return NotFound("Company not found in Tbl901CompanyDetails.");
					}

					// Step 4: Get NoOfDigitsToInventoryQuotation using CompanyId from Tbl901CompanyDetails02
					int noOfDigits = dbContext.Tbl901CompanyDetails02s
											  .Where(c => c.CompanyId == company.CompanyId)
											  .Select(c => c.NoOfDigitsToInventoryQuotation ?? 4)
											  .FirstOrDefault(); // Default to 4 if not found

					// Step 5: Extract values for quotation number
					string QuotationAbbrv = company.QuotationAbbrv;
					int invoiceYearDigits = company.InvoiceYearDigits ?? 0;
					bool isResetInvoiceInYear = company.IsResetInvoiceInYear ?? false;
					DateTime invoiceDate = DateTime.Now;
					// Generate new debit note QuotationAbbrv
					string newDebitNoteNo = GetNewDebitNoteNo(QuotationAbbrv, invoiceYearDigits, invoiceDate, isResetInvoiceInYear, noOfDigits, dbContext);
				
					

					return Ok(newDebitNoteNo);
				}
				else
				{
					return BadRequest("Tenant or DB Context not found.");
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error in GetNewDebitNoteNoApi: {ex.Message}");
				return StatusCode(500, "Internal server error: " + ex.Message);
			}
		}



		private string GetNewDebitNoteNo(string QuotationAbbrv, int yearInDigit, DateTime invoiceDate, bool isResetByYear, int noOfDigits, ERPMasterWtDataContext dbContext)
		{
			try
			{
				var mprNumbers = dbContext.Tbl60101quotationMasters
					.Where(d => d.QuoteNo != null &&
								d.QuoteNo.Length >= noOfDigits &&
								(!isResetByYear || (d.QuoteDate.HasValue && d.QuoteDate.Value.Year == invoiceDate.Year)))
					.Select(d => d.QuoteNo)
					.ToList();

				int maxRunningNumber = mprNumbers
					.Select(no => int.TryParse(no.Substring(no.Length - noOfDigits), out int num) ? num : 0)
					.DefaultIfEmpty(0)
					.Max();

				maxRunningNumber += 1;

				string strNewDebitNoteNo = maxRunningNumber.ToString().PadLeft(noOfDigits, '0');

				string strYear = invoiceDate.Year.ToString();
				if (yearInDigit > 0)
					strYear = strYear.Substring(strYear.Length - yearInDigit, yearInDigit);
				else
					strYear = "";

				return $"{QuotationAbbrv}{strYear}-{strNewDebitNoteNo}";
			}
			catch (Exception)
			{
				string strYear = invoiceDate.Year.ToString();
				if (yearInDigit > 0)
					strYear = strYear.Substring(strYear.Length - yearInDigit, yearInDigit);
				else
					strYear = "";

				return $"{QuotationAbbrv}{strYear}-{"1".PadLeft(noOfDigits, '0')}";
			}
		}

		[HttpGet]
		public async Task<IActionResult> GetQuotationdataByCode(string QuoteNo)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				if (string.IsNullOrEmpty(QuoteNo))
					return BadRequest("Quote No is required.");

				try
				{

					var client = await dbContext.Tbl60101quotationMasters
						.Where(c => c.QuoteNo == QuoteNo)
						.FirstOrDefaultAsync();

					if (client == null)
						return NotFound("Quotation not found.");

					return Ok(client);
				}
				catch (Exception ex)
				{
					return StatusCode(500, $"Internal server error: {ex.Message}");
				}
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}
		[HttpGet]
		public async Task<ActionResult> GetQuotationChildren(string QuoteNo)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					var resultWithDetails = new List<ExpandoObject>();

					// Query the Tbl60602purchaseRequestChildren table for the given Mprno
					var result = dbContext.Qry60102quotationChildren
						.Where(x => x.QuoteNo == QuoteNo)
						.ToList();

					foreach (var gridDetails in result)
					{
						dynamic item = new ExpandoObject();
						var dict = (IDictionary<string, object>)item;

						// Copy all existing fields from gridDetails into dynamic object
						var properties = gridDetails.GetType().GetProperties();
						foreach (var prop in properties)
						{
							dict[prop.Name] = prop.GetValue(gridDetails);
						}

						// Retrieve UnitDesc based on UnitCode
						var unitDesc = await dbContext.Tbl40111PropertyUnitCodes
							.Where(x => x.UnitCode == gridDetails.UnitRateMethod)
							.Select(x => x.UnitDesc)
							.FirstOrDefaultAsync();



						// Retrieve Gsdescription based on Gscode
						var gsDescription = await dbContext.Tbl20164GoodsAndServicesMasters
							.Where(x => x.Gscode == gridDetails.Gscode)
							.Select(x => x.Gsdescrpition)
							.FirstOrDefaultAsync();

						// Add the retrieved values to the dynamic object
						dict["UnitDesc"] = unitDesc;

						dict["GsDescription"] = gsDescription;
                       
                        dict["GSCode"] = gridDetails.Gscode;

                        resultWithDetails.Add(item);
					}

					return Json(resultWithDetails);
				}
				catch (Exception ex)
				{
					return StatusCode(500, $"Internal server error: {ex.Message}");
				}
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}
		[HttpPost]
		public async Task<IActionResult> SaveOrUpdateQuotation([FromBody] QuotationViewModel VM)
		{
			if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				return Unauthorized(new { success = false, message = "Invalid tenant context." });
			}

			if (VM == null || string.IsNullOrEmpty(VM.QuoteNo))
			{
				return BadRequest(new { success = false, message = "Quote No. is required." });
			}

			try
			{
				// Check if the master record exists
				var existingMaster = await dbContext.Tbl60101quotationMasters
					.FirstOrDefaultAsync(x => x.QuoteNo == VM.QuoteNo);

				if (existingMaster != null)
				{
					//Update existing master with manual property mapping


					existingMaster.QuoteDate = VM.QuoteDate;
					existingMaster.ClientCode = VM.ClientCode;
					existingMaster.SalesPersonCode = VM.SalesPersonCode;
					existingMaster.Mprno = VM.Mprno;
					existingMaster.Attention = VM.Attention;
					existingMaster.ClientContactEmail = VM.ClientContactEmail;
					existingMaster.ClientContactNo = VM.ClientContactNo;
					existingMaster.ModeOfRequest = VM.ModeOfRequest.HasValue ? (byte?)VM.ModeOfRequest.Value : null;
					existingMaster.TypeOfRequest = VM.TypeOfRequest.HasValue ? (byte?)VM.TypeOfRequest.Value : null;
					existingMaster.ProjectMasterCode = VM.ProjectMasterCode;
					existingMaster.Project = VM.Project;
					existingMaster.SubjectTitle = VM.SubjectTitle;
					existingMaster.QuotationSummary = VM.QuotationSummary;
					existingMaster.QuoteIntro = VM.QuoteIntro;
					existingMaster.QuoteThanksNote = VM.QuoteThanksNote;
					existingMaster.CompanyBranch = VM.CompanyBranch.HasValue ? (byte?)VM.CompanyBranch.Value : null;
					existingMaster.InventoryMasterGroupId = VM.InventoryMasterGroupId.HasValue ? (byte?)VM.InventoryMasterGroupId.Value : null;
					existingMaster.ClientRefNo = VM.ClientRefNo;
					existingMaster.QuoteSubmittedBy = VM.QuoteSubmittedBy;
					existingMaster.QuoteSubmittedOn = VM.QuoteSubmittedOn;
					existingMaster.BidClosingDate = VM.BidClosingDate;
					existingMaster.QuoteStatus = VM.QuoteStatus.HasValue ? (byte?)VM.QuoteStatus.Value : null;
					existingMaster.TransportationScope = VM.TransportationScope;
					existingMaster.AdditionsText = VM.AdditionsText;
					existingMaster.QuoteTransport = VM.QuoteTransport;
					existingMaster.DiscountsText = VM.DiscountsText;
					existingMaster.QuoteDiscount = VM.QuoteDiscount;
					existingMaster.QuoteSignatory = VM.QuoteSignatory.HasValue ? (byte?)VM.QuoteSignatory.Value : null;
					existingMaster.VerifiedSignatory = VM.VerifiedSignatory.HasValue ? (byte?)VM.VerifiedSignatory.Value : null;
					existingMaster.ApprovedSignatory = VM.ApprovedSignatory.HasValue ? (byte?)VM.ApprovedSignatory.Value : null;
					existingMaster.RevisionNo = VM.RevisionNo;
					existingMaster.QuoteValidity = VM.QuoteValidity;
				}
				else
				{
					// Insert new master
					var newMaster = new Tbl60101quotationMaster
					{

						 QuoteNo= VM.QuoteNo,
    QuoteDate= VM.QuoteDate,
    ClientCode= VM.ClientCode,
    SalesPersonCode= VM.SalesPersonCode,
	Mprno = VM.Mprno,
    Attention= VM.Attention,
    ClientContactEmail= VM.ClientContactEmail,
    ClientContactNo=VM.ClientContactNo,
    ModeOfRequest=Convert.ToByte(VM.ModeOfRequest),
    TypeOfRequest=Convert.ToByte(VM.TypeOfRequest),
    ProjectMasterCode= VM.ProjectMasterCode,
    Project=VM.Project,
    SubjectTitle= VM.SubjectTitle,
    QuotationSummary= VM.QuotationSummary,
    QuoteIntro= VM.QuoteIntro,
    QuoteThanksNote= VM.QuoteThanksNote,
    CompanyBranch=Convert.ToByte(VM.CompanyBranch),
    InventoryMasterGroupId=Convert.ToByte(VM.InventoryMasterGroupId),
    ClientRefNo= VM.ClientRefNo,
    QuoteSubmittedBy= VM.QuoteSubmittedBy,
    QuoteSubmittedOn= VM.QuoteSubmittedOn,
    BidClosingDate= VM.BidClosingDate,
    QuoteStatus=Convert.ToByte(VM.QuoteStatus),
    TransportationScope=VM.TransportationScope,
    AdditionsText= VM.AdditionsText,
    QuoteTransport= VM.QuoteTransport,
    DiscountsText= VM.DiscountsText,
    QuoteDiscount= VM.QuoteDiscount,
    QuoteSignatory=Convert.ToByte(VM.QuoteSignatory),
    VerifiedSignatory=Convert.ToByte(VM.VerifiedSignatory),
    ApprovedSignatory= Convert.ToByte(VM.ApprovedSignatory),
	RevisionNo=VM.RevisionNo,
	QuoteValidity=VM.QuoteValidity,

					};

					await dbContext.Tbl60101quotationMasters.AddAsync(newMaster);
				}

				// Handle child entries
				var existingChildren = await dbContext.Tbl60102quotationChildren
					.Where(x => x.QuoteNo == VM.QuoteNo)
					.ToListAsync();

                // Track QuoteChildId from client
                var incomingIds = VM.QuotationDetailses
                    .Where(x => x.QuoteChildId > 0)
                    .Select(x => x.QuoteChildId)
                    .ToList();

                // Delete missing children
                var toDelete = existingChildren
                    .Where(x => !incomingIds.Contains(x.QuoteChildId))
                    .ToList();

                if (toDelete.Any())
                {
                    dbContext.Tbl60102quotationChildren.RemoveRange(toDelete);
                }


                foreach (var child in VM.QuotationDetailses)
				{
					if (child.QuoteChildId == 0)
					{
						// New child entry
						child.QuoteNo = VM.QuoteNo; // Ensure foreign key is set
						await dbContext.Tbl60102quotationChildren.AddAsync(child);
					}
					else
					{
						// Existing child entry
						var existingChild = existingChildren
							.FirstOrDefault(x => x.QuoteChildId == child.QuoteChildId);

						if (existingChild != null)
						{
							dbContext.Entry(existingChild).CurrentValues.SetValues(child);
						}
					}
				}

				await dbContext.SaveChangesAsync();
                await _userActionLogger.LogAsync(
                    module: "IMS > Save Quotation",
                   actionDetail: $"Saved Quotation: {VM.QuoteNo}",
                    documentNo: $"{VM.QuoteNo}"
                );

                return Ok(new { success = true, message = "Quotation Details saved/updated successfully." });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { success = false, message = ex.Message });
			}
		}
		[HttpDelete]
		public async Task<IActionResult> DeleteQuotation([FromQuery] string QuoteNo)
		{
			if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				return Unauthorized(new { success = false, message = "Invalid tenant context." });
			}

			if (string.IsNullOrEmpty(QuoteNo))
			{
				return BadRequest(new { success = false, message = "QuoteNo. is required." });
			}

			try
			{
				// Retrieve the master record
				var masterRecord = await dbContext.Tbl60101quotationMasters
					.FirstOrDefaultAsync(x => x.QuoteNo == QuoteNo);

				if (masterRecord == null)
				{
					return NotFound(new { success = false, message = "Quotation not found." });
				}

				// Retrieve and remove child records
				var childRecords = dbContext.Tbl60102quotationChildren
					.Where(x => x.QuoteNo == QuoteNo);

				dbContext.Tbl60102quotationChildren.RemoveRange(childRecords);

				// Remove the master record
				dbContext.Tbl60101quotationMasters.Remove(masterRecord);

				await dbContext.SaveChangesAsync();
                await _userActionLogger.LogAsync(
                   module: "IMS > Delete Quotation",
                   actionDetail: $"Quotation Deleted: {QuoteNo}",
                   documentNo: $"{QuoteNo}"
                );

                return Ok(new { success = true, message = "Quotation details deleted successfully." });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { success = false, message = ex.Message });
			}
		}
        private async Task<int?> GetSignatoryIDfromUserID(int? userId)
        {
            if (userId == null)
                return null;

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return await dbContext.Tbl90104DocumentSignatories
                    .Where(x => x.UserId == userId)
                    .Select(x => x.SignatoryId)
                    .FirstOrDefaultAsync();
            }

            // Tenant context is invalid; return null
            return null;
        }



        [HttpPost]
        public async Task<IActionResult> SubmitQuotation(string QuoteNo)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { success = false, message = "Invalid tenant context." });

            if (string.IsNullOrEmpty(QuoteNo))
                return BadRequest(new { success = false, message = "Quote No. is required." });

            var master = await dbContext.Tbl60101quotationMasters
                            .FirstOrDefaultAsync(x => x.QuoteNo == QuoteNo);
            if (master == null)
                return NotFound(new { success = false, message = "Quotation not found." });

            var userName = HttpContext.Session.GetString("UserName");
            var userIdString = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(userIdString, out int userId))
                return Unauthorized(new { success = false, message = "Invalid or missing UserId in session." });

            master.IsSubmitted = true;
            master.SubmittedBy = userName;
            master.SubmittedOn = DateTime.Now;
            master.ModifiedBy = userName;
            master.ModifiedOn = DateTime.Now;
            // Retrieve signatory ID
            var signatoryId = await GetSignatoryIDfromUserID(userId);
            if (signatoryId.HasValue)
            {
                master.QuoteSignatory = (byte)signatoryId.Value;
            }
            else
            {
                master.QuoteSignatory = null;
            }


           // master.PurchaseRequestStatusId = 31; // Enquiry/Request Submitted

            await dbContext.SaveChangesAsync();
            await _userActionLogger.LogAsync(
                   module: "IMS > Submit Quotation",
                   actionDetail: $"Quotation Submitted: {QuoteNo}",
                   documentNo: $"{QuoteNo}"
                );

            return Ok(new
            {
                success = true,
                message = "Quotation submitted successfully.",
                VoucherApprovedBy = signatoryId
            });
        }

        [HttpPost]
        public async Task<IActionResult> VerifyQuotation(string quoteNo)
        {
            try
            {
                // 1. Tenant context validation
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized(new { message = "Invalid tenant context." });

                // 2. Input validation
                if (string.IsNullOrEmpty(quoteNo))
                    return BadRequest(new { message = "Quote number is required." });

                // 3. Fetch the quotation
                var quotation = await dbContext.Tbl60101quotationMasters
                                               .FirstOrDefaultAsync(q => q.QuoteNo == quoteNo);
                if (quotation == null)
                    return NotFound(new { message = "Quotation not found." });

                // 4. User session retrieval
                var userName = HttpContext.Session.GetString("UserName");
                var userIdString = HttpContext.Session.GetString("UserId");
                if (!int.TryParse(userIdString, out int userId))
                    return Unauthorized(new { message = "Invalid or missing UserId in session." });

                // ✅ 5. Workflow enforcement based on company setting
                var companySetting = await dbContext.Tbl901CompanyDetails02s.FirstOrDefaultAsync();
                bool isWorkflowEnabled = companySetting?.IsEnableQuotationWorkflow == true;

                if (isWorkflowEnabled && quotation.IsSubmitted != true)
                {
                    return BadRequest(new { message = "You need to submit the quotation before verification." });
                }

                // 6. Verification update
                quotation.IsVerified = true;
                quotation.VerifiedOn = DateTime.Now;
                quotation.VerifiedBy = userName;
              //  quotation.PurchaseRequestStatusId = 32; // Enquiry/Request Verified

                var signatoryId = await GetSignatoryIDfromUserID(userId);
                if (signatoryId.HasValue)
                {
                    quotation.VerifiedSignatory = (byte)signatoryId.Value;
                }

                await dbContext.SaveChangesAsync();
                await _userActionLogger.LogAsync(
                  module: "IMS > Verify Quotation",
                  actionDetail: $"Quotation Verified: {quoteNo}",
                  documentNo: $"{quoteNo}"
                );

                return Ok(new
                {
                    message = "Quotation has been verified and processed for Approval.",
                    VerifiedBy = signatoryId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in VerifyQuotation: {ex.Message}");
                return StatusCode(500, new { message = "Internal Server Error", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult> ApproveQuotation(string QuoteNo)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { Message = "Invalid tenant." });

            try
            {
                // ⚙️ Get session user
                var userName = HttpContext.Session.GetString("UserName");
                var userIdString = HttpContext.Session.GetString("UserId");
                if (!int.TryParse(userIdString, out int userId))
                    return Unauthorized(new { Message = "Invalid or missing UserId in session." });

                // ✅ Validate input
                if (string.IsNullOrEmpty(QuoteNo))
                    return BadRequest(new { Message = "Quote number is required." });

                var voucher = await dbContext.Tbl60101quotationMasters
                                             .FirstOrDefaultAsync(v => v.QuoteNo == QuoteNo);
                if (voucher == null)
                    return NotFound(new { Message = "Quotation not found." });

                // 🔍 Step 1: Workflow enabled?
                var companySetting = await dbContext.Tbl901CompanyDetails02s.FirstOrDefaultAsync();
                bool isWorkflowEnabled = companySetting?.IsEnableQuotationWorkflow == true;

                // If workflow is enabled, ensure prior steps: submitted + verified
                if (isWorkflowEnabled)
                {
                    if (voucher.IsSubmitted != true || voucher.IsVerified != true)
                    {
                        return BadRequest(new
                        {
                            Message = "Please submit and verify the quotation before approval."
                        });
                    }
                }

                // ✅ Step 2: Apply approval
                voucher.IsApproved = true;
                voucher.ApprovedOn = DateTime.Now;
                voucher.ApprovedBy = userName;
              //  voucher.PurchaseRequestStatusId = 33; // Status: Enquiry/Request Approved

                var signatoryId = await GetSignatoryIDfromUserID(userId);
                if (signatoryId.HasValue)
                {
                    voucher.ApprovedSignatory = (byte)signatoryId.Value;
                }
                await dbContext.SaveChangesAsync();
                await _userActionLogger.LogAsync(
                  module: "IMS > Approved Quotation",
                  actionDetail: $"Quotation Approved: {QuoteNo}",
                  documentNo: $"{QuoteNo}"
                );

                return Ok(new
                {
                    Message = "Quotation has been Approved.",
                    VoucherApprovedBy = signatoryId  // or userName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in ApproveQuotation: {ex}");
                return StatusCode(500, new { Message = "Internal Server Error", Details = ex.Message });
            }
        }

        [HttpGet]
		public async Task<IActionResult> GetQuotationStatus(DataSourceLoadOptions loadOptions)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					var clients = dbContext.Tbl60107quotationStatuses.Select(i => new
					{
					i.QuoteStatusId,
					i.QuoteStatus
					});

					return Json(await DataSourceLoader.LoadAsync(clients, loadOptions));
				}
				catch (Exception ex)
				{
					_logger.LogError($"Error in GetClientDetails: {ex.Message}");
					return StatusCode(500, new { message = "Error fetching client details", error = ex.Message });
				}
			}

			return Unauthorized(new { message = "Invalid tenant", success = false });
		}
        //IMS DetailDescription Form 
        [HttpGet]
        public async Task<IActionResult> GetDetailDescriptiondata(long QuoteChildId)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {

                    var client = await dbContext.Qry60102quotationChildren
                        .Where(c => c.QuoteChildId == QuoteChildId)
                        .FirstOrDefaultAsync();

                    if (client == null)
                        return NotFound("Quotation not found.");

                    return Ok(client);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteChildById(int childId)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant context." });
            }

            try
            {
                var child = await dbContext.Tbl60102quotationChildren
                    .FirstOrDefaultAsync(x => x.QuoteChildId == childId);

                if (child == null)
                {
                    return NotFound(new { success = false, message = "Child record not found." });
                }

                dbContext.Tbl60102quotationChildren.Remove(child);
                await dbContext.SaveChangesAsync();
                await _userActionLogger.LogAsync(
                  module: "IMS > Delete Child By Id",
                  actionDetail: $" Deleted Child: {childId}",
                  documentNo: $"{childId}"
                );

                return Ok(new { success = true, message = "Child row deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in DeleteChildById: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Internal server error." });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetItemSubGroup()
        {
            try


            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var QuoteGroup = await dbContext.Tbl60107quotationChildItemGroups
                       .Select(s => new
                       {
                           s.QuoteGroupItemSlNo,
                           s.GroupName,
                           s.GroupRemarks

                       })
                        .ToListAsync();

                    return Json(QuoteGroup); // return raw data, paging/sorting done on client-side
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetDetaildescriptiondata1(long QuoteChildId)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                if (QuoteChildId == 0)
                    return BadRequest("QuoteChildId is required.");


                try
                {

                    var client = await dbContext.Qry60102quotationChildren
                        .Where(c => c.QuoteChildId == QuoteChildId)
                        .FirstOrDefaultAsync();

                    if (client == null)
                        return NotFound("Client not found.");

                    return Ok(client);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetProject: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<ActionResult> GetUserddl(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var users = dbContext.TblUserMasters.Select(u => new { u.UserId, u.UserName });
                    return Json(await DataSourceLoader.LoadAsync(users, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetUserddl: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpPost]
        public async Task<IActionResult> UpdateSalesPerson([FromBody] Tbl20101SalesPersonMaster salesPerson)
        {
            if (salesPerson == null || string.IsNullOrEmpty(salesPerson.SalesPersonCode))
            {
                return BadRequest("Invalid salesperson data.");
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var existingSalesPerson = await dbContext.Tbl20101SalesPersonMasters
                        .FirstOrDefaultAsync(s => s.SalesPersonCode == salesPerson.SalesPersonCode);

                    if (existingSalesPerson == null)
                    {
                        return NotFound(new { success = false, message = $"Salesperson with code {salesPerson.SalesPersonCode} not found." });
                    }

                    if (salesPerson.UserCode.HasValue)
                    {
                        var userCodeExists = await dbContext.Tbl20101SalesPersonMasters
                            .AnyAsync(s => s.UserCode == salesPerson.UserCode && s.SalesPersonCode != salesPerson.SalesPersonCode);

                        if (!userCodeExists)
                        {
                            return BadRequest(new { success = false, message = $"The provided UserCode {salesPerson.UserCode} does not exist for any other salesperson." });
                        }

                        if (salesPerson.UserCode < 0 || salesPerson.UserCode > 255)
                        {
                            return BadRequest(new { success = false, message = "UserCode must be between 0 and 255." });
                        }
                    }

                    if (!string.IsNullOrEmpty(salesPerson.SalesPersonName))
                        existingSalesPerson.SalesPersonName = salesPerson.SalesPersonName;
                    if (salesPerson.UserCode.HasValue)
                        existingSalesPerson.UserCode = salesPerson.UserCode.Value;
                    if (!string.IsNullOrEmpty(salesPerson.EmailAddress))
                        existingSalesPerson.EmailAddress = salesPerson.EmailAddress;
                    if (!string.IsNullOrEmpty(salesPerson.SalesPersonContactNo))
                        existingSalesPerson.SalesPersonContactNo = salesPerson.SalesPersonContactNo;
                    if (salesPerson.TargetPerMonth.HasValue)
                    {
                        existingSalesPerson.TargetPerMonth = salesPerson.TargetPerMonth;
                    }


                    dbContext.Entry(existingSalesPerson).State = EntityState.Modified;
                    await dbContext.SaveChangesAsync();
                    await _userActionLogger.LogAsync(
                           module: "IMS > Update Sales Person",
                           actionDetail: $" Updated Sales Person: {salesPerson.SalesPersonCode}",
                            documentNo: $"{salesPerson.SalesPersonCode}"
                    );

                    return Ok(new { success = true, message = "Salesperson updated successfully." });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in UpdateSalesPerson: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
    }
}
