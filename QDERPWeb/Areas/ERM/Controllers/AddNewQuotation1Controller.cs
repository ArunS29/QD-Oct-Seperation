using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using QD.ERP.Web.Services.Logging;
using System.Dynamic;

namespace QD.ERP.Web.Areas.ERM.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class AddNewQuotation1Controller : Controller
	{
		private readonly TenantDbContextHelper _tenantDbContextHelper;
		private readonly ILogger<AddNewQuotation1Controller> _logger;
        private readonly IUserActionLogger _userActionLogger;


        public AddNewQuotation1Controller(ILogger<AddNewQuotation1Controller> logger, TenantDbContextHelper tenantDbContextHelper, IUserActionLogger userActionLogger)
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
				// Step 1: Get tenant name from session
				var tenantName = HttpContext.Session.GetString("TenantName");
				if (string.IsNullOrWhiteSpace(tenantName))
				{
					return Unauthorized(new { message = "Tenant name not found in session.", success = false });
				}

				// Step 2: Try to get DbContext for tenant
				if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				{
					// Step 3: Get company from Tbl901CompanyDetails using tenantName
					var company = dbContext.Tbl901CompanyDetails
										   .FirstOrDefault(c => c.CompanyNameShort == tenantName);

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




        public class QuotationViewModel
        {
            public string QuoteNo { get; set; }

            public DateTime? QuoteDate { get; set; }

            public string ClientRefNo { get; set; }

            public string Attention { get; set; }

            public string SubjectTitle { get; set; }

            public byte? TypeOfQuote { get; set; }

            public string QuoteType { get; set; }

            public decimal? QuoteTransport { get; set; }

            public decimal? QuoteDiscount { get; set; }

            public byte? PaymentTerms { get; set; }

            public byte? DeliveryPeriod { get; set; }

            public byte? DeliveryTerms { get; set; }

            public string QuoteValidity { get; set; }

            public string PreparedBy { get; set; }

            public DateTime? PreparedOn { get; set; }

            public string ApprovedBy { get; set; }

            public DateTime? ApprovedOn { get; set; }

            public string AddedBy { get; set; }

            public DateTime? AddedOn { get; set; }

            public string ModifiedBy { get; set; }

            public DateTime? ModifiedOn { get; set; }

            public string Rfqcode { get; set; }

            public string ClientContactNo { get; set; }

            public string ClientContactEmail { get; set; }

            public string ClientCode { get; set; }

            public string QuotationSummary { get; set; }

            public byte? QuoteSignatory { get; set; }

            public string QuoteIntro { get; set; }

            public byte? TypeOfRequest { get; set; }

            public byte? ModeOfRequest { get; set; }

            public string AdditionsText { get; set; }

            public string DiscountsText { get; set; }

            public DateTime? QuoteDueDate { get; set; }

            public string Project { get; set; }

            public string SalesPersonCode { get; set; }

            public bool? IsVerified { get; set; }

            public bool? IsApproved { get; set; }

            public byte? RevisionNo { get; set; }

            public byte? CompanyBranch { get; set; }

            public string Mprno { get; set; }

            public string QuoteThanksNote { get; set; }

            public string QuoteColumn1 { get; set; }

            public string QuoteColumn2 { get; set; }

            public string QuoteColumn3 { get; set; }

            public string QuoteLabel1 { get; set; }

            public string QuoteLabel2 { get; set; }

            public string QuoteLabel3 { get; set; }

            public DateTime? QuoteSubmittedOn { get; set; }

            public string QuoteSubmittedBy { get; set; }

            public byte? QuoteStatus { get; set; }

            public byte? InventoryMasterGroupId { get; set; }

            public byte? VerifiedSignatory { get; set; }

            public byte? ApprovedSignatory { get; set; }

            public bool? IsSubmitted { get; set; }

            public string SubmittedBy { get; set; }

            public DateTime? SubmittedOn { get; set; }

            public string VerifiedBy { get; set; }

            public DateTime? VerifiedOn { get; set; }

            public string ProjectMasterCode { get; set; }

            public DateTime? BidClosingDate { get; set; }

            public string TransportationScope { get; set; }

            public decimal? CurrencyRate { get; set; }
            public int? BaseCurrencyId { get; set; }
            public int? CurrencyId { get; set; }
            public List<Tbl40104PropertyQuoteChild> QuotationDetailses { get; set; }


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
                var existingMaster = await dbContext.Tbl40103PropertyQuoteMasters
                    .FirstOrDefaultAsync(x => x.QuoteNo == VM.QuoteNo);

                if (existingMaster != null)
                {
                    //Update existing master with manual property mapping


                    existingMaster.QuoteDate = VM.QuoteDate;
                    existingMaster.ClientCode = VM.ClientCode;
                    existingMaster.SalesPersonCode = VM.SalesPersonCode;
                    //existingMaster.Mprno = VM.Mprno;
                    existingMaster.Attention = VM.Attention;
                    existingMaster.ClientContactEmail = VM.ClientContactEmail;
                    existingMaster.ClientContactNo = VM.ClientContactNo;
                    existingMaster.ModeOfRequest = VM.ModeOfRequest.HasValue ? (byte?)VM.ModeOfRequest.Value : null;
                    existingMaster.TypeOfRequest = VM.TypeOfRequest.HasValue ? (byte?)VM.TypeOfRequest.Value : null;
                    // existingMaster.ProjectMasterCode = VM.ProjectMasterCode;
                    existingMaster.Project = VM.Project;
                    existingMaster.SubjectTitle = VM.SubjectTitle;
                    existingMaster.QuotationSummary = VM.QuotationSummary;
                    existingMaster.QuoteIntro = VM.QuoteIntro;
                    existingMaster.QuoteThanksNote = VM.QuoteThanksNote;
                    existingMaster.CompanyBranch = VM.CompanyBranch.HasValue ? (byte?)VM.CompanyBranch.Value : null;
                    // existingMaster.InventoryMasterGroupId = VM.InventoryMasterGroupId.HasValue ? (byte?)VM.InventoryMasterGroupId.Value : null;
                    existingMaster.ClientRefNo = VM.ClientRefNo;
                    existingMaster.AddedBy = VM.QuoteSubmittedBy;
                    existingMaster.AddedOn = VM.QuoteSubmittedOn;
                    existingMaster.QuoteDate = VM.BidClosingDate;
                    // existingMaster.QuoteStatus = VM.QuoteStatus.HasValue ? (byte?)VM.QuoteStatus.Value : null;
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
                    //existingMaster.CurrencyId = VM.CurrencyId ?? 1;
                    //existingMaster.CurrencyRate = VM.CurrencyRate ?? 1;
                    //existingMaster.BaseCurrencyId = VM.BaseCurrencyId ?? 1;

                }
                else
                {
                    // Insert new master
                    var newMaster = new Tbl40103PropertyQuoteMaster
                    {

                        QuoteNo = VM.QuoteNo,
                        QuoteDate = VM.QuoteDate,
                        ClientCode = VM.ClientCode,
                        SalesPersonCode = VM.SalesPersonCode,
                        //  Mprno = VM.Mprno,
                        Attention = VM.Attention,
                        ClientContactEmail = VM.ClientContactEmail,
                        ClientContactNo = VM.ClientContactNo,
                        ModeOfRequest = Convert.ToByte(VM.ModeOfRequest),
                        TypeOfRequest = Convert.ToByte(VM.TypeOfRequest),
                        // ProjectMasterCode = VM.ProjectMasterCode,
                        Project = VM.Project,
                        SubjectTitle = VM.SubjectTitle,
                        QuotationSummary = VM.QuotationSummary,
                        QuoteIntro = VM.QuoteIntro,
                        QuoteThanksNote = VM.QuoteThanksNote,
                        CompanyBranch = Convert.ToByte(VM.CompanyBranch),
                        // InventoryMasterGroupId = Convert.ToByte(VM.InventoryMasterGroupId),
                        ClientRefNo = VM.ClientRefNo,
                        AddedBy = VM.QuoteSubmittedBy,
                        AddedOn = VM.QuoteSubmittedOn,
                        QuoteDueDate = VM.BidClosingDate,
                        // QuoteStatus = Convert.ToByte(VM.QuoteStatus),
                        TransportationScope = VM.TransportationScope,
                        AdditionsText = VM.AdditionsText,
                        QuoteTransport = VM.QuoteTransport,
                        DiscountsText = VM.DiscountsText,
                        QuoteDiscount = VM.QuoteDiscount,
                        QuoteSignatory = Convert.ToByte(VM.QuoteSignatory),
                        VerifiedSignatory = Convert.ToByte(VM.VerifiedSignatory),
                        ApprovedSignatory = Convert.ToByte(VM.ApprovedSignatory),
                        RevisionNo = VM.RevisionNo,
                        QuoteValidity = VM.QuoteValidity
                        //CurrencyId = VM.CurrencyId ?? 1,
                        //CurrencyRate = VM.CurrencyRate ?? 1,
                        //BaseCurrencyId = VM.BaseCurrencyId ?? 1,

                    };

                    await dbContext.Tbl40103PropertyQuoteMasters.AddAsync(newMaster);
                }

                // Handle child entries
                var existingChildren = await dbContext.Tbl40104PropertyQuoteChildren
                    .Where(x => x.QuoteNo == VM.QuoteNo)
                    .ToListAsync();



                //var currencyRate = await dbContext.Tbl60101quotationMasters
                //            .Where(x => x.QuoteNo == VM.QuoteNo)
                //            .Select(x => x.CurrencyRate)
                //            .FirstOrDefaultAsync();



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
                    dbContext.Tbl40104PropertyQuoteChildren.RemoveRange(toDelete);
                }


                foreach (var child in VM.QuotationDetailses)
                {
                    if (child.QuoteChildId == 0)
                    {
                        // New child entry
                        child.QuoteNo = VM.QuoteNo; // Ensure foreign key is set

                        //child.CostPrice = child.CostPrice * currencyRate;
                        //child.QuotedUnitPrice = child.QuotedUnitPrice * currencyRate;
                        //child.QuotedDiscount = child.QuotedDiscount * currencyRate;

                        await dbContext.Tbl40104PropertyQuoteChildren.AddAsync(child);
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

                // await dbContext.SaveChangesAsync();

                var rows = await dbContext.SaveChangesAsync();

                await _userActionLogger.LogAsync(
                    module: "IMS > Save Quotation",
                   actionDetail: $"Saved Quotation: {VM.QuoteNo}",
                    documentNo: $"{VM.QuoteNo}"
                );

                return Ok(new { success = true, message = "Quotation Details saved/updated successfully.", quoteno = VM.QuoteNo });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

















        public class QuotationDetailsRequest
        {
            public string QuoteNo { get; set; }
            public string DetailedDescription { get; set; }
           // public bool? Operator { get; set; }
            public byte? QuoteMethod2 { get; set; }
            public decimal? Rate2 { get; set; }
            public byte? QuoteMethod3 { get; set; }
            public decimal? Rate3 { get; set; }
            public string AdditionalNotes { get; set; }
            public decimal? MobilizationRate { get; set; }
            public decimal? DemobRate { get; set; }
            public string DeliveryDetails { get; set; }
        }










        [HttpPost]
        public async Task<IActionResult> SaveOrUpdateQuotationChild([FromBody] QuotationDetailsRequest request)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant context." });
            }

            if (request == null || string.IsNullOrEmpty(request.QuoteNo))
            {
                return BadRequest(new { success = false, message = "Quote No is required." });
            }

            try
            {
                // Check if quotation already exists
                var existing = await dbContext.Tbl40104PropertyQuoteChildren
                    .FirstOrDefaultAsync(x => x.QuoteNo == request.QuoteNo);

                if (existing != null)
                {
                    // Update existing record
                    existing.PropertyAddlDescription = request.DetailedDescription;
                   // existing.IsWithOperator = request.Operator;
                    existing.UnitRateMethod2 = request.QuoteMethod2;
                    existing.UnitRate2 = request.Rate2;
                    existing.UnitRateMethod3 = request.QuoteMethod3;
                    existing.UnitRate3 = request.Rate3;
                    existing.AddlNotes = request.AdditionalNotes;
                    existing.MobRate = request.MobilizationRate;
                    existing.DemobRate = request.DemobRate;
                    existing.DeliveryTerms = request.DeliveryDetails;
                }
                else
                {
                    // Insert new record
                    var newChild = new Tbl40104PropertyQuoteChild
                    {
                        QuoteNo = request.QuoteNo,
                        PropertyAddlDescription = request.DetailedDescription,
                       // IsWithOperator = request.Operator,
                        UnitRateMethod2 = request.QuoteMethod2,
                        UnitRate2 = request.Rate2,
                        UnitRateMethod3 = request.QuoteMethod3,
                        UnitRate3 = request.Rate3,
                        AddlNotes = request.AdditionalNotes,
                        MobRate = request.MobilizationRate,
                        DemobRate = request.DemobRate,
                        DeliveryTerms = request.DeliveryDetails,
                    };

                    await dbContext.Tbl40104PropertyQuoteChildren.AddAsync(newChild);
                }

                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Detailed Description saved successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }





        [HttpGet]
        public async Task<ActionResult> GetQuoteChildDetails(string QuoteNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var resultWithDetails = new List<ExpandoObject>();

                    // Query Tbl40104PropertyQuoteChild by QuoteNo
                    var result = await dbContext.Tbl40104PropertyQuoteChildren
                        .Where(x => x.QuoteNo == QuoteNo)
                        .ToListAsync();

                    foreach (var gridDetails in result)
                    {
                        dynamic item = new ExpandoObject();
                        var dict = (IDictionary<string, object>)item;

                        // Map only required fields
                        dict["DetailedDescription"] = gridDetails.PropertyAddlDescription;
                        dict["MobRate"] = gridDetails.MobRate;
                        dict["DemobRate"] = gridDetails.DemobRate;

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
