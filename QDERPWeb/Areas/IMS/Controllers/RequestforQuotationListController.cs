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

namespace QD.ERP.Web.Areas.IMS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class RequestforQuotationListController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<RequestforQuotationListController> _logger;

        public RequestforQuotationListController(ILogger<RequestforQuotationListController> logger, TenantDbContextHelper tenantDbContextHelper)
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
					var result = dbContext.Qry60702rfqchildren  
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

            try
            {
                var existingMaster = await dbContext.Tbl60701rfqmasters
                    .FirstOrDefaultAsync(x => x.Rfqno == VM.Rfqno);

                if (existingMaster != null)
                {
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
                    existingMaster.SalesPersonCode = VM.SalesPersonCode;
                    existingMaster.Rfqsignatory = VM.Rfqsignatory.HasValue ? (byte?)VM.Rfqsignatory.Value : null;
                    existingMaster.CompanyBranch = VM.CompanyBranch.HasValue ? (byte?)VM.CompanyBranch.Value : null;
                    existingMaster.InventoryMasterGroupId = VM.InventoryMasterGroupId.HasValue ? (byte?)VM.InventoryMasterGroupId.Value : null;
                }
                else
                {
                    var newMaster = new Tbl60701rfqmaster
                    {
                        Rfqno = VM.Rfqno,
                        Rfqdate = VM.Rfqdate,
                        SupplierCode = VM.SupplierCode,
                        Mprno = VM.Mprno,
                        Project = VM.Project,
                        Attention = VM.Attention,
                        SupplierContactEmail = VM.SupplierContactEmail,
                        SupplierContactNo = VM.SupplierContactNo,
                        SupplierQuotationNo = VM.SupplierQuotationNo,
                        SupplierQuotationDt = VM.SupplierQuotationDt,
                        ProjectMasterCode = VM.ProjectMasterCode,
                        Rfqsubject = VM.Rfqsubject,
                        Rfqintro = VM.Rfqintro,
                        Rfqsummary = VM.Rfqsummary,
                        SalesPersonCode = VM.SalesPersonCode,
                        Rfqsignatory = Convert.ToByte(VM.Rfqsignatory),
                        CompanyBranch = Convert.ToByte(VM.CompanyBranch),
                        InventoryMasterGroupId = Convert.ToByte(VM.InventoryMasterGroupId)
                    };

                    await dbContext.Tbl60701rfqmasters.AddAsync(newMaster);
                }

                // ✅ Handle child records
                var existingChildren = await dbContext.Tbl60702rfqchildren
                    .Where(x => x.Rfqno == VM.Rfqno)
                    .ToListAsync();

                // ✅ Track RFQChild IDs received from client
                var incomingIds = VM.RFQDetailses
                    .Where(x => x.RfqchildSlNo > 0)
                    .Select(x => x.RfqchildSlNo)
                    .ToList();

                // ✅ Find and delete missing children
                var toDelete = existingChildren
                    .Where(x => !incomingIds.Contains(x.RfqchildSlNo))
                    .ToList();

                if (toDelete.Any())
                {
                    dbContext.Tbl60702rfqchildren.RemoveRange(toDelete);
                }

                // ✅ Insert or update child records
                foreach (var child in VM.RFQDetailses)
                {
                    child.Rfqno = VM.Rfqno;

                    if (child.RfqchildSlNo == 0)
                    {
                        await dbContext.Tbl60702rfqchildren.AddAsync(child);
                    }
                    else
                    {
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
        [HttpPost]
        public IActionResult DeleteRFQView(string Rfqno)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Json(new { success = false, message = "Invalid tenant context." });

            try
            {
                var rfqMaster = dbContext.Tbl60701rfqmasters.FirstOrDefault(x => x.Rfqno == Rfqno);

                if (rfqMaster == null)
                    return Json(new { success = false, message = "RFQ not found." });

                // Check if RFQ is approved
                if (rfqMaster.IsApproved == true)
                    return Json(new { success = false, message = "RFQ is already approved. You cannot delete the approved Request/Enquiry." });

                // Check if RFQ has PO issued
                var rfqWithPO = dbContext.Qry60704rfqviewMasters.FirstOrDefault(x => x.Rfqno == Rfqno && !string.IsNullOrEmpty(x.Pono));
                if (rfqWithPO != null)
                    return Json(new { success = false, message = "RFQ has related Purchase Order issued. You cannot delete the RFQ." });

                // Delete RFQ Child Records
                var rfqChildren = dbContext.Tbl60702rfqchildren.Where(x => x.Rfqno == Rfqno);
                dbContext.Tbl60702rfqchildren.RemoveRange(rfqChildren);

                // Delete RFQ Master Record
                dbContext.Tbl60701rfqmasters.Remove(rfqMaster);

                // Optional: Delete scanned documents if needed
                // DeleteDocumentPDF(Rfqno, "VoucherScanned\\IMS_RFQ");

                // Save Changes
                dbContext.SaveChanges();

                // Log Deletion
                //string userId = HttpContext.Session.GetString("UserID") ?? "Unknown";
                //string userName = HttpContext.Session.GetString("UserName") ?? "Unknown";

                //InsertUserEntryLogSheet(
                //    "IMS RFQ",
                //    $"IMS RFQ Ref No. {Rfqno} has been deleted by User ID: {userId} User Name: {userName}.",
                //    userName,
                //    Rfqno
                //);

                return Json(new { success = true, message = "RFQ has been successfully removed from the database." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting RFQ.");
                return Json(new { success = false, message = "An error occurred while deleting the RFQ." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UnlockRFQ([FromBody] RFQViewModel request)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized(new { success = false, message = "Invalid tenant." });

                if (string.IsNullOrWhiteSpace(request?.Rfqno))
                    return BadRequest(new { success = false, message = "RFQ No is required." });

                var existingEntity = await dbContext.Tbl60701rfqmasters
                    .FirstOrDefaultAsync(x => x.Rfqno == request.Rfqno);

                if (existingEntity == null)
                    return NotFound(new { success = false, message = "RFQ not found." });

                if (existingEntity.IsApproved != true && existingEntity.IsSubmitted != true && existingEntity.IsVerified != true)
                    return Ok(new { success = false, message = "RFQ is already unlocked." });

                existingEntity.IsApproved = false;
                existingEntity.IsSubmitted = false;
                existingEntity.IsVerified = false;

                dbContext.Tbl60701rfqmasters.Update(existingEntity);
                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Quotation has been unlocked successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while unlocking Quotation.");
                return StatusCode(500, new { success = false, message = "Internal server error", details = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> AutoInsertRFQFromMPR([FromForm] string rfqNo, [FromForm] string mprNo)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                return Unauthorized();

            try
            {
                string user = User.Identity?.Name ?? "System";
                await dbContext.Database.ExecuteSqlRawAsync(
                    "EXEC sp600_01InsertToRfqFromEnquiry @RFQNo = {0}, @MPRNo = {1}, @AddedBy = {2}",
                    rfqNo, mprNo, user
                );

                var pr = await dbContext.Tbl60601purchaseRequestMasters.FirstOrDefaultAsync(x => x.Mprno == mprNo);
                if (pr != null)
                {
                    pr.PurchaseRequestStatusId = 2;
                    await dbContext.SaveChangesAsync();
                }

                return Ok(new { message = "RFQ inserted from MPR successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> CreatePOFromRFQ([FromBody] string rfqNo)
        {
            if (string.IsNullOrWhiteSpace(rfqNo))
                return BadRequest(new { message = "RFQ No is required", success = false });

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { message = "Invalid tenant", success = false });

            try
            {
                string addedBy = User.Identity?.Name ?? "System";

                // Step 1: Get Company Info
                var company = await dbContext.Tbl901CompanyDetails
                    .FirstOrDefaultAsync(c => c.CompanyNameShort == tenant.Name);

                if (company == null)
                    return NotFound(new { message = "Company not found in Tbl901CompanyDetails.", success = false });

                // Step 2: Setup PO number prefix
                string prefix = company.PurchaseOrderAbbrv ?? "";
                int yearDigits = company.InvoiceYearDigits ?? 0;
                bool resetByYear = company.IsResetInvoiceInYear ?? false;

                int noOfDigits = await dbContext.Tbl901CompanyDetails02s
                    .Where(c => c.CompanyId == company.CompanyId)
                    .Select(c => c.NoOfDigitsToInventoryQuotation ?? 5)
                    .FirstOrDefaultAsync();

                string yearPart = DateTime.Now.Year.ToString();
                if (yearDigits > 0)
                    yearPart = yearPart.Substring(yearPart.Length - yearDigits);

                string basePrefix = $"{prefix}{yearPart}-";

                // Step 3: Get existing POs with same prefix
                var existingPos = await dbContext.Tbl60401purchaseOrderMasters
                    .Where(x => x.Pono.StartsWith(basePrefix))
                    .Select(x => x.Pono)
                    .ToListAsync();

                int maxNumber = existingPos
                    .Select(no => int.TryParse(no.Substring(no.Length - noOfDigits), out int num) ? num : 0)
                    .DefaultIfEmpty(0)
                    .Max();

                int nextNumber = maxNumber + 1;
                string newPoNo = $"{basePrefix}{nextNumber.ToString().PadLeft(noOfDigits, '0')}";

                // Step 4: Execute stored procedure to insert PO from RFQ
                await dbContext.Database.ExecuteSqlRawAsync(
                    "EXEC sp600_02InsertToPOfromRFQ @PONo = {0}, @RFQNo = {1}, @AddedBy = {2}",
                    newPoNo, rfqNo, addedBy
                );

                return Ok(new
                {
                    success = true,
                    poNo = newPoNo,
                    message = "Purchase Order created successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in CreatePOFromRFQ: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while creating the Purchase Order.",
                    error = ex.Message
                });
            }
        }
        [HttpGet]
        public async Task<IActionResult> CheckIfApproved(string Rfqno)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var isApproved = await dbContext.Tbl60701rfqmasters
                    .Where(x => x.Rfqno == Rfqno)
                    .Select(x => x.IsApproved ?? false)
                    .FirstOrDefaultAsync();

                return Ok(isApproved);
            }

            return BadRequest("Invalid tenant or DB context.");
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
                var child = await dbContext.Tbl60702rfqchildren
                    .FirstOrDefaultAsync(x => x.RfqchildSlNo == childId);

                if (child == null)
                {
                    return NotFound(new { success = false, message = "Child record not found." });
                }

                dbContext.Tbl60702rfqchildren.Remove(child);
                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Child row deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in DeleteChildById: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Internal server error." });
            }
        }
        [HttpGet]
        public IActionResult GetSupplierContactDetails(string supplierCode)

        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                // Replace with your actual data retrieval logic
                var supplier = dbContext.Tbl30199SupplierMasters
                                 .FirstOrDefault(c => c.SupplierCode == supplierCode);

                if (supplier != null)
                {
                    return Json(new
                    {
                        ContactName =supplier.ContactPerson,
                        ContactEmail = supplier.ContactEmail,
                        ContactMobile = supplier.ContactMobile1
                    });
                }
                else
                {
                    return NotFound();
                }
            }

            return Unauthorized(new { Message = "Invalid tenant.", Success = false });

        }
    }
}
