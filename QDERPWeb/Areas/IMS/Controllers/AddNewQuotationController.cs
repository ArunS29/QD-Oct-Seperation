using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System.Dynamic;

namespace QD.ERP.Web.Areas.IMS.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class AddNewQuotationController : Controller
	{
		private readonly TenantDbContextHelper _tenantDbContextHelper;
		private readonly ILogger<AddNewQuotationController> _logger;

		public AddNewQuotationController(ILogger<AddNewQuotationController> logger, TenantDbContextHelper tenantDbContextHelper)
		{
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
				// Retrieve tenant name from session
				var tenantName = HttpContext.Session.GetString("TenantName");
				if (string.IsNullOrWhiteSpace(tenantName))
				{
					return Unauthorized(new { message = "Tenant name not found in session.", success = false });
				}

				if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				{
					// Use tenantName to find the company
					var company = dbContext.Tbl901CompanyDetails
										   .FirstOrDefault(c => c.CompanyNameShort == tenantName);

					if (company == null)
					{
						return NotFound("Company not found.");
					}

					string invoiceAbbrv = company.InvoiceAbbrv;
					int invoiceYearDigits = company.InvoiceYearDigits ?? 0;
					bool isResetInvoiceInYear = company.IsResetInvoiceInYear ?? false;
					DateTime invoiceDate = DateTime.Now;

					// Generate new debit note number
					string newDebitNoteNo = GetNewDebitNoteNo(invoiceAbbrv, invoiceYearDigits, invoiceDate, isResetInvoiceInYear, dbContext);

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


		private string GetNewDebitNoteNo(string invoiceAbbrv, int yearInDigit, DateTime invoiceDate, bool isResetByYear, ERPMasterWtDataContext dbContext)
		{
			try
			{
				// Retrieve MPR numbers into memory Tbl601_01QuotationMaster
				var mprNumbers = dbContext.Tbl60101quotationMasters
					.Where(d => d.QuoteNo != null && d.QuoteNo.Length >= 5 &&
								(!isResetByYear || (d.QuoteDate.HasValue && d.QuoteDate.Value.Year == invoiceDate.Year)))
					.Select(d => d.QuoteNo)
					.ToList();

				// Extract numeric parts and determine the maximum
				int maxRunningNumber = mprNumbers
					.Select(no => int.TryParse(no.Substring(no.Length - 5), out int num) ? num : 0)
					.DefaultIfEmpty(0)
					.Max();

				maxRunningNumber += 1;

				// Format the new debit note number
				string strNewDebitNoteNo = maxRunningNumber.ToString().PadLeft(5, '0');

				string strYear = invoiceDate.Year.ToString();
				if (yearInDigit > 0)
				{
					strYear = strYear.Substring(strYear.Length - yearInDigit, yearInDigit);
				}
				else
				{
					strYear = "";
				}

				return $"AIC-QTN-{strYear}-{strNewDebitNoteNo}";
			}
			catch (Exception)
			{
				string strYear = invoiceDate.Year.ToString();
				if (yearInDigit > 0)
				{
					strYear = strYear.Substring(strYear.Length - yearInDigit, yearInDigit);
				}
				else
				{
					strYear = "";
				}

				return $"AIC_QTN-{strYear}-00001";
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
					var result = dbContext.Tbl60102quotationChildren
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

			try//
			{
				// Ensure child list is initialized
				//VM.RFQDetailses = VM.RFQDetailses ?? new List<Tbl60702rfqchild>();

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
	RevisionNo=VM.RevisionNo

					};

					await dbContext.Tbl60101quotationMasters.AddAsync(newMaster);
				}

				// Handle child entries
				var existingChildren = await dbContext.Tbl60102quotationChildren
					.Where(x => x.QuoteNo == VM.QuoteNo)
					.ToListAsync();

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

				return Ok(new { success = true, message = "Quotation details deleted successfully." });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { success = false, message = ex.Message });
			}
		}



		[HttpPost]
		public async Task<IActionResult> SubmitQuotation(string QuoteNo)
		{
			// Validate tenant context
			if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				return Unauthorized(new { success = false, message = "Invalid tenant context." });
			}

			// Validate MPR number
			if (string.IsNullOrEmpty(QuoteNo))
			{
				return BadRequest(new { success = false, message = "Quote No. is required." });
			}

			// Retrieve MPR master record
			var master = await dbContext.Tbl60101quotationMasters.FirstOrDefaultAsync(x => x.QuoteNo == QuoteNo);
			if (master == null)
			{
				return NotFound(new { success = false, message = "MPR not found." });
			}

			// Retrieve session values
			var userName = HttpContext.Session.GetString("UserName");
			var userIdString = HttpContext.Session.GetString("UserId");

			if (!int.TryParse(userIdString, out int userId))
			{
				return Unauthorized(new { success = false, message = "Invalid or missing UserId in session." });
			}

			// Update MPR master record
			master.IsSubmitted = true;
			master.SubmittedBy = userName;
			master.SubmittedOn = DateTime.Now;
			master.ModifiedBy = userName;
			master.ModifiedOn = DateTime.Now;

			// Retrieve signatory ID
			//var signatoryId = await GetSignatoryIDfromUserID(userId);
			//if (signatoryId.HasValue)
			//{
			//	master.RequestSignatory = (byte)signatoryId.Value;
			//}
			//else
			//{
			//	master.RequestSignatory = null;
			//}


			//master.PurchaseRequestStatusId = 31; // Enquiry/Request Submitted

			// Save changes to the database
			await dbContext.SaveChangesAsync();

			return Ok(new { success = true, message = "Quotation submitted successfully." });
		}
		[HttpPost]
		public async Task<IActionResult> VerifyQuotation(string QuoteNo)
		{
			if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				return Unauthorized(new { message = "Invalid tenant context." });
			}

			if (string.IsNullOrEmpty(QuoteNo))
			{
				return BadRequest(new { message = "Quote is required." });
			}

			var voucher = await dbContext.Tbl60101quotationMasters
				.FirstOrDefaultAsync(v => v.QuoteNo == QuoteNo);

			if (voucher == null)
			{
				return NotFound(new { message = "Credit note not found." });
			}

			var userName = HttpContext.Session.GetString("UserName");
			var userIdString = HttpContext.Session.GetString("UserId");

			if (!int.TryParse(userIdString, out int userId))
			{
				return Unauthorized(new { message = "Invalid or missing UserId in session." });
			}

			// Update voucher fields
			voucher.IsVerified = true;
			voucher.VerifiedOn = DateTime.Now;
			voucher.VerifiedBy = userName;
			//voucher.PurchaseRequestStatusId = 32; // Enquiry/Request Verified

			//var signatoryId = await GetSignatoryIDfromUserID(userId);
			//if (signatoryId.HasValue)
			//{
			//	voucher.MprverifiedSign = (byte)signatoryId.Value;
			//}

			await dbContext.SaveChangesAsync();

			return Ok(new
			{
				message = "Quotation has been Verified and processed for Approval."
			});
		}
		[HttpPost]
		public async Task<ActionResult> ApproveQuotation(string QuoteNo)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					var userName = HttpContext.Session.GetString("UserName");
					var userIdString = HttpContext.Session.GetString("UserId");
					if (!int.TryParse(userIdString, out int userId))
					{
						return Unauthorized(new { message = "Invalid or missing UserId in session." });
					}


					if (string.IsNullOrEmpty(QuoteNo))
					{
						return BadRequest(new { Message = "Quote number is required." });
					}

					var voucher = dbContext.Tbl60101quotationMasters
										   .FirstOrDefault(v => v.QuoteNo == QuoteNo);

					if (voucher == null)
					{
						return NotFound(new { Message = "CreditNoteNo not found." });
					}

					// Update approval details
					voucher.IsApproved = true;
					voucher.ApprovedOn = DateTime.Now;
					voucher.ApprovedBy = userName;
					//voucher.PurchaseRequestStatusId = 33; // Status: Enquiry/Request Approved
					//var signatoryId = await GetSignatoryIDfromUserID(userId);
					//if (signatoryId.HasValue)
					//{
					//	voucher.MprapprovedSign = (byte)signatoryId.Value;
					//}

					dbContext.SaveChanges();

					return Ok(new
					{
						Message = "Quotation has been Approved.",
						VoucherApprovedBy = userName
					});
				}
				catch (Exception ex)
				{
					return BadRequest(new { Message = ex.Message });
				}
			}

			return Unauthorized(new { Message = "Invalid tenant.", Success = false });
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
	}
}
