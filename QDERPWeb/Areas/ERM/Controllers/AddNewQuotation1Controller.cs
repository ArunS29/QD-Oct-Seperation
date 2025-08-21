using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System.Dynamic;

namespace QD.ERP.Web.Areas.ERM.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class AddNewQuotation1Controller : Controller
	{
		private readonly TenantDbContextHelper _tenantDbContextHelper;
		private readonly ILogger<AddNewQuotation1Controller> _logger;

		public AddNewQuotation1Controller(ILogger<AddNewQuotation1Controller> logger, TenantDbContextHelper tenantDbContextHelper)
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

                return Ok(new { success = true, message = "Quotation details saved/updated successfully." });
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
