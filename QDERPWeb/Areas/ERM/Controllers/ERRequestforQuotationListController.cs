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
using System.Dynamic;

namespace QD.ERP.Web.Areas.ERM.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ERRequestforQuotationListController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<ERRequestforQuotationListController> _logger;

        public ERRequestforQuotationListController(ILogger<ERRequestforQuotationListController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetRFQRequest(DateTime? fromDate, DateTime? toDate)
        {
            try {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var query = dbContext.Qry60704rfqviewMasters.AsQueryable();


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
                    query = query.Where(i => i.Rfqdate >= fromDate && i.Rfqdate <= toDate);

                    // Fetching the data
                    var data = await query.Select(i => new
                    {
                        i.Rfqno,
                        i.Rfqdate,
                        i.Mprno,
                        i.SupplierName,
                        i.SupplierQuotationNo,
                        i.QuoteHasItemsToPo,
                        i.Pono,
                        i.NoOfItems,
                        i.TotalBeforeTax,
                        i.TotalDiscount,
                        i.TotalAfterDiscount,
                    }).ToListAsync();

                    return Json(data);
                }

                return Unauthorized(new { message = "Invalid tenant." });
            }
            catch (Exception ex)
                            {
                    _logger.LogError($"Error in GetProject: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
                }
        }
        //RFQ Item Details form
		[HttpGet]
		 public async Task<IActionResult> GetRFQwithItemDetails(DateTime? fromDate, DateTime? toDate)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var query = dbContext.Qry60706rfqdetails.AsQueryable();


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
                query = query.Where(i => i.Rfqdate >= fromDate && i.Rfqdate <= toDate);

                // Fetching the data
                var data = await query.Select(i => new
                {
                    i.Rfqno,
                    i.Rfqdate,
                    i.Mprno,
                    i.SupplierName,
                    i.SupplierQuotationNo,
                    i.QuoteHasItemsToPo,
                    i.Pono,
                    i.NoOfItems,
                    i.TotalBeforeTax,
                    i.TotalDiscount,
                    i.TotalAfterDiscount,
                    i.Gscode,
                    i.Gsdescrpition,
                    i.QuotedQuantity,
                    i.UnitDesc,
                    i.UnitPrice,
                    i.ItemDiscount,
                    i.IsWonForPo,
                    i.LineTotalBeforeTax,
                    i.LineTotalAfterDisc
				}).ToListAsync();

                return Json(data);
            }

            return Unauthorized(new { message = "Invalid tenant." });
        }
		//Add New RFQ Form
		
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

					string Rfqabbrv = company.Rfqabbrv;
					int invoiceYearDigits = company.InvoiceYearDigits ?? 0;
					bool isResetInvoiceInYear = company.IsResetInvoiceInYear ?? false;
					DateTime invoiceDate = DateTime.Now;

					// Generate new debit note number
					string newDebitNoteNo = GetNewDebitNoteNo(Rfqabbrv, invoiceYearDigits, invoiceDate, isResetInvoiceInYear, dbContext);

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



		private string GetNewDebitNoteNo(string Rfqabbrv, int yearInDigit, DateTime invoiceDate, bool isResetByYear, ERPMasterWtDataContext dbContext)
		{
			try
			{
				
				// Retrieve MPR numbers into memory
				var mprNumbers = dbContext.Tbl60701rfqmasters
					.Where(d => d.Rfqno != null && d.Rfqno.Length >= 5 &&
								(!isResetByYear || (d.Rfqdate.HasValue && d.Rfqdate.Value.Year == invoiceDate.Year)))
					.Select(d => d.Rfqno)
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

				return $"{Rfqabbrv}{strYear}-{strNewDebitNoteNo}";
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

				return $"{Rfqabbrv}{strYear}-00001";
			}
		}
		[HttpGet]
		public async Task<IActionResult> GetSupplierName(DataSourceLoadOptions loadOptions)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					var ClientCategory = dbContext.Tbl30199SupplierMasters.Select(i => new
					{
						i.SupplierCode,
						i.SupplierName

					});

					return Json(await DataSourceLoader.LoadAsync(ClientCategory, loadOptions));
				}
				catch (Exception ex)
				{
					_logger.LogError($"Error in GetProject: {ex.Message}");
					return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
				}
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}
		[HttpGet]
		public async Task<IActionResult> GetSignatory(DataSourceLoadOptions loadOptions)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					var ClientCategory = dbContext.Tbl90104DocumentSignatories.Select(i => new
					{
						i.SignatoryId,
						i.SignatoryName

					});

					return Json(await DataSourceLoader.LoadAsync(ClientCategory, loadOptions));
				}
				catch (Exception ex)
				{
					_logger.LogError($"Error in GetProject: {ex.Message}");
					return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
				}
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}
		[HttpGet]
		public async Task<IActionResult> GetCompany(DataSourceLoadOptions loadOptions)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					var ClientCategory = dbContext.Tbl901CompanyDetails.Select(i => new
					{
						i.CompanyId,
						i.CompanyName

					});

					return Json(await DataSourceLoader.LoadAsync(ClientCategory, loadOptions));
				}
				catch (Exception ex)
				{
					_logger.LogError($"Error in GetProject: {ex.Message}");
					return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
				}
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}
		[HttpGet]
		public async Task<IActionResult> GetInventoryGroup(DataSourceLoadOptions loadOptions)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					var ClientCategory = dbContext.Tbl60008inventoryMasterGroups.Select(i => new
					{
						i.InventoryMasterGroupId,
						i.InventoryMasterGroup

					});

					return Json(await DataSourceLoader.LoadAsync(ClientCategory, loadOptions));
				}
				catch (Exception ex)
				{
					_logger.LogError($"Error in GetProject: {ex.Message}");
					return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
				}
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}

		[HttpGet]
		public async Task<IActionResult> GetRFQdataByCode(string RFQno)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				if (string.IsNullOrEmpty(RFQno))
					return BadRequest("RFQ No is required.");

				try
				{

					var client = await dbContext.Tbl60701rfqmasters
						.Where(c => c.Rfqno == RFQno)
						.FirstOrDefaultAsync();

					if (client == null)
						return NotFound("RFQ not found.");

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
		public async Task<ActionResult> GetRFQChildren(string RFQno)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					var resultWithDetails = new List<ExpandoObject>();

					// Query the Tbl60602purchaseRequestChildren table for the given Mprno
					var result = dbContext.Tbl60702rfqchildren
						.Where(x => x.Rfqno == RFQno)
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
		public async Task<IActionResult> SaveOrUpdateRFQ([FromBody] RFQViewModel VM)
		{
			if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				return Unauthorized(new { success = false, message = "Invalid tenant context." });
			}

			if (VM == null || string.IsNullOrEmpty(VM.Rfqno))
			{
				return BadRequest(new { success = false, message = "RFQ No. is required." });
			}

			try//
			{
				// Ensure child list is initialized
				//VM.RFQDetailses = VM.RFQDetailses ?? new List<Tbl60702rfqchild>();

				// Check if the master record exists
				var existingMaster = await dbContext.Tbl60701rfqmasters
					.FirstOrDefaultAsync(x => x.Rfqno == VM.Rfqno);

				if (existingMaster != null)
				{
					//Update existing master with manual property mapping

					//existingMaster.Rfqno = VM.Rfqno;
					existingMaster.Rfqdate = VM.Rfqdate;
					existingMaster.SupplierCode = VM.SupplierCode;
					existingMaster.Mprno = VM.Mprno;
					existingMaster.Project = VM.Project;
					existingMaster.Attention = VM.Attention;
					existingMaster.SupplierContactEmail = VM.SupplierContactEmail;
					existingMaster.SupplierContactNo = VM.SupplierContactNo;
					existingMaster.SupplierQuotationNo = VM.SupplierQuotationNo;
					existingMaster.SupplierQuotationDt = VM.SupplierQuotationDt;
					existingMaster.ProjectMasterCode = VM.ProjectMasterCode;
					existingMaster.Rfqsubject = VM.Rfqsubject;
					existingMaster.Rfqintro = VM.Rfqintro;
					existingMaster.Rfqsummary = VM.Rfqsummary;


					existingMaster.Rfqsignatory = VM.Rfqsignatory.HasValue ? (byte?)VM.Rfqsignatory.Value : null;
					existingMaster.CompanyBranch = VM.CompanyBranch.HasValue ? (byte?)VM.CompanyBranch.Value : null; 
					existingMaster.InventoryMasterGroupId = VM.InventoryMasterGroupId.HasValue ? (byte?)VM.InventoryMasterGroupId.Value : null;

				}
				else
				{
					// Insert new master
					var newMaster = new Tbl60701rfqmaster
					{
						Rfqno = VM.Rfqno,
						Rfqdate=VM.Rfqdate,
						SupplierCode= VM.SupplierCode,

						Mprno = VM.Mprno,
						Project = VM.Project,
						Attention = VM.Attention,
						SupplierContactEmail = VM.SupplierContactEmail,
						SupplierContactNo = VM.SupplierContactNo,

						SupplierQuotationNo = VM.SupplierQuotationNo,
						SupplierQuotationDt = VM.SupplierQuotationDt,
						ProjectMasterCode = VM.ProjectMasterCode,
						Rfqsubject=VM.Rfqsubject,
						Rfqintro = VM.Rfqintro,
						Rfqsummary=VM.Rfqsummary,
						Rfqsignatory = Convert.ToByte(VM.Rfqsignatory),
						CompanyBranch = Convert.ToByte(VM.CompanyBranch),
						InventoryMasterGroupId=Convert.ToByte(VM.InventoryMasterGroupId)
,

					};

					await dbContext.Tbl60701rfqmasters.AddAsync(newMaster);
				}

				// Handle child entries
				var existingChildren = await dbContext.Tbl60702rfqchildren
					.Where(x => x.Rfqno == VM.Rfqno)
					.ToListAsync();

				foreach (var child in VM.RFQDetailses)
				{
					if (child.RfqchildSlNo == 0)
					{
						// New child entry
						child.Rfqno = VM.Rfqno; // Ensure foreign key is set
						await dbContext.Tbl60702rfqchildren.AddAsync(child);
					}
					else
					{
						// Existing child entry
						var existingChild = existingChildren
							.FirstOrDefault(x => x.RfqchildSlNo == child.RfqchildSlNo);

						if (existingChild != null)
						{
							dbContext.Entry(existingChild).CurrentValues.SetValues(child);
						}
					}
				}

				await dbContext.SaveChangesAsync();

				return Ok(new { success = true, message = "RFQ Details saved/updated successfully." });
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
		[HttpDelete]
		public async Task<IActionResult> DeleteRfq([FromQuery] string Rfqno)
		{
			if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				return Unauthorized(new { success = false, message = "Invalid tenant context." });
			}

			if (string.IsNullOrEmpty(Rfqno))
			{
				return BadRequest(new { success = false, message = "Rfqno. is required." });
			}

			try
			{
				// Retrieve the master record
				var masterRecord = await dbContext.Tbl60701rfqmasters
					.FirstOrDefaultAsync(x => x.Rfqno == Rfqno);

				if (masterRecord == null)
				{
					return NotFound(new { success = false, message = "RFQ not found." });
				}

				// Retrieve and remove child records
				var childRecords = dbContext.Tbl60702rfqchildren
					.Where(x => x.Rfqno == Rfqno);

				dbContext.Tbl60702rfqchildren.RemoveRange(childRecords);

				// Remove the master record
				dbContext.Tbl60701rfqmasters.Remove(masterRecord);

				await dbContext.SaveChangesAsync();

				return Ok(new { success = true, message = "RFQ details deleted successfully." });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { success = false, message = ex.Message });
			}
		}

		[HttpPost]
		public async Task<IActionResult> SubmitRFQ(string Rfqno)
		{
			// Validate tenant context
			if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				return Unauthorized(new { success = false, message = "Invalid tenant context." });
			}

			// Validate MPR number
			if (string.IsNullOrEmpty(Rfqno))
			{
				return BadRequest(new { success = false, message = "RFQ No. is required." });
			}

			// Retrieve MPR master record
			var master = await dbContext.Tbl60701rfqmasters.FirstOrDefaultAsync(x => x.Rfqno == Rfqno);
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

			return Ok(new { success = true, message = "RFQ submitted successfully." });
		}
		[HttpPost]
		public async Task<IActionResult> VerifyRFQ(string Rfqno)
		{
			if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				return Unauthorized(new { message = "Invalid tenant context." });
			}

			if (string.IsNullOrEmpty(Rfqno))
			{
				return BadRequest(new { message = "Rfqno is required." });
			}

			var voucher = await dbContext.Tbl60701rfqmasters
				.FirstOrDefaultAsync(v => v.Rfqno == Rfqno);

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
				message = "RFQ has been Verified and processed for Approval."
			});
		}
		[HttpPost]
		public async Task<ActionResult> ApproveRFQ(string Rfqno)
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


					if (string.IsNullOrEmpty(Rfqno))
					{
						return BadRequest(new { Message = "RFQ No number is required." });
					}

					var voucher = dbContext.Tbl60701rfqmasters
										   .FirstOrDefault(v => v.Rfqno == Rfqno);

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
						Message = "RFQ has been Approved.",
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
	}
}
