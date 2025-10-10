using DevExpress.DataProcessing.InMemoryDataProcessor;
using DevExpress.XtraRichEdit.Import.Html;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
//using QD.ERP.Web.Areas.Finance.Models;
//using QD.ERP.Web.Areas.VAT.Controllers;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using QD.ERP.Web.Services.Logging;
using SkiaSharp;
using System.Data;
using System.Dynamic;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static QD.ERP.Web.Areas.ERM.Controllers.AddNewQuotation1Controller;


namespace QD.ERP.Web.Areas.ERM.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AddNewClientRequest1Controller : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<AddNewClientRequest1Controller> _logger;
        private readonly IUserActionLogger _userActionLogger;


        public AddNewClientRequest1Controller(ILogger<AddNewClientRequest1Controller> logger, TenantDbContextHelper tenantDbContextHelper, IUserActionLogger userActionLogger)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
            _userActionLogger = userActionLogger;


        }
		[HttpGet]
		public ActionResult<string> GetNewRequestNoApi()
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

					string EquipmentRequestAbbrv = company.EquipmentRequestAbbrv;
					int invoiceYearDigits = company.InvoiceYearDigits ?? 0;
					bool isResetInvoiceInYear = company.IsResetInvoiceInYear ?? false;
					DateTime invoiceDate = DateTime.Now;

					// Generate new debit note number
					string newDebitNoteNo = GetNewDebitNoteNo(EquipmentRequestAbbrv, invoiceYearDigits, invoiceDate, isResetInvoiceInYear, dbContext);

					return Ok(newDebitNoteNo);
				}
				else
				{
					return BadRequest("Tenant or DB Context not found.");
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error in GetNewRequestNoApi: {ex.Message}");
				return StatusCode(500, "Internal server error: " + ex.Message);
			}
		}


		private string GetNewDebitNoteNo(string EquipmentRequestAbbrv, int yearInDigit, DateTime invoiceDate, bool isResetByYear, ERPMasterWtDataContext dbContext)
		{
			try
			{
				// Retrieve MPR numbers into memory
				var mprNumbers = dbContext.Tbl40136PropertyRequestMasters
                    .Where(d => d.EqiupmentRequestNo != null && d.EqiupmentRequestNo.Length >= 5 &&
								(!isResetByYear || (d.RequestDate.HasValue && d.RequestDate.Value.Year == invoiceDate.Year)))
					.Select(d => d.EqiupmentRequestNo)
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

				return $"{EquipmentRequestAbbrv}{strYear}-{strNewDebitNoteNo}";
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

				return $"{EquipmentRequestAbbrv}{strYear}-00001";
			}
		}
        [HttpGet]
        public async Task<IActionResult> GetClientDetails(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var clients = dbContext.Tbl30101ClientMasters.Select(i => new
                    {
                        i.ClientCode,
                        i.ClientName,
                        i.ContactPerson,
                        i.ContactMobile1,
                        i.ContactEmail,
                        i.ClientAddress
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
		[HttpGet]
		public async Task<IActionResult> GetModeofRequest()
		{
			try
			{
				if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				{
					var dbSignatories = await dbContext.Tbl30103ModeOfRequestMasters
					   .Select(s => new
					   {
						   s.ModeOfRequestId,
						   s.ModeOfRequest

					   })
						.ToListAsync();

					return Json(dbSignatories); // return raw data, paging/sorting done on client-side
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
		public async Task<IActionResult> GetTypeOfRequest(DataSourceLoadOptions loadOptions)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					var ClientCategory = dbContext.Tbl30104TypeOfRequestMasters.Select(i => new
					{
						i.TypeOfRequestId,
						i.TypeOfRequest

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
        [HttpGet("GetStores")]
        public IActionResult GetStores()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var stores = dbContext.Tbl60001storeMasters
                        .Select(store => new
                        {
                            store.StoreId, 
                          store.StoreName
                        })
                        .ToList();

                    return Ok(stores);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error loading stores: {ex.Message}");
                    return StatusCode(500, new { message = "Failed to load store list.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant." });
        }

        [HttpGet]
		public async Task<IActionResult> GetSalesPerson(DataSourceLoadOptions loadOptions)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					var ClientCategory = dbContext.Tbl20101SalesPersonMasters.Select(i => new
					{
						i.SalesPersonCode,
						i.SalesPersonName

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
        public async Task<IActionResult> GetEqpCostClientTimesheet(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    // Example: SP returns columns ClientId, ClientName, TotalCost, TimesheetDate
                    var result = await dbContext.Database
				   .SqlQuery<sp40109Eqp_Cost_ClientTimesheetResult>($"EXEC sp40109Eqp_Cost_ClientTimesheet")
				   .ToListAsync();


                    return Json(DataSourceLoader.Load(result, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetEqpCostClientTimesheet: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        public class EqpCostClientTimesheetDto
        {

            public string PropertyNo { get; set; }
            public string ClientCode { get; set; }
            public string ClientName { get; set; }
            public string PropertySummInvoiceNo { get; set; }
            public decimal? ClientRatePerHour { get; set; }
            public decimal? ClientOTRatePerHour { get; set; }
            public string DayDescription { get; set; }
            public int ClientId { get; set; }
            public decimal? ClientRegHoursTotal { get; set; }
            public decimal? ClientRegAmountTotal { get; set; }
            public decimal? ClientAmountTotal { get; set; }
            public int? NoOfDays { get; set; }
            public decimal? ClientOTHoursTotal { get; set; }
            public decimal? ClientOTAmountTotal { get; set; }
            public decimal TotalCost { get; set; }
            public DateTime TimesheetDate { get; set; }
        }

        [HttpDelete("{requestChildSlNo}")]
        public async Task<IActionResult> DeletePurchaseRequestChild(int requestChildSlNo)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized("Invalid tenant");

            try
            {
                // Find the record
                var child = await dbContext.Tbl40137PropertyRequestChildren
                    .FirstOrDefaultAsync(c => c.RequestChildSlNo == requestChildSlNo);

                if (child == null)
                    return NotFound(new { error = "Child record not found." });

                // Remove and save
                dbContext.Tbl40137PropertyRequestChildren.Remove(child);
                await dbContext.SaveChangesAsync();
                await _userActionLogger.LogAsync(
               module: "ERM > Delete Request",
              actionDetail: $"Delete Request{requestChildSlNo}",
               documentNo: $"{requestChildSlNo}"
           );
                return Ok(new { message = "Child record deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
		public async Task<IActionResult> GetProject(DataSourceLoadOptions loadOptions)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{

					var Project = dbContext.Qry70002projectsViewMasters.Select(i => new
					{
						i.ProjectId,
						i.ProjectDescription

					});

					return Json(await DataSourceLoader.LoadAsync(Project, loadOptions));
				}
				catch (Exception ex)
				{
					_logger.LogError($"Error in GetProject: {ex.Message}");
					return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
				}
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}
	

        public class PropertyRequestViewModel
        {
            // Tbl40136PropertyRequestMaster fields
            public string EqiupmentRequestNo { get; set; }
            public DateTime? RequestDate { get; set; }
            public string ClientCode { get; set; }
            public string RequestedBy { get; set; }
            public string RequesterContactEmail { get; set; }
            public string RequesterContact { get; set; }
            public byte? TypeOfMpr { get; set; }
            public byte? ModeOfRequest { get; set; }
            public byte? TypeOfRequest { get; set; }
            public string ClientRefNo { get; set; }
            public decimal? CurrencyRate { get; set; }
            public int? BaseCurrencyId { get; set; }
            public int? CurrencyId { get; set; }
            public string ProjectMasterCode { get; set; }

            // Extra fields
            public string PurposeOfRequest { get; set; }
            public DateTime? ExpectedDate { get; set; }
            public string Remarks { get; set; }
            public byte? CompanyBranch { get; set; }

            // ✅ Use correct child ViewModel
            public List<PropertyRequestChildViewModel> EquipmentPropertychild { get; set; }
        }

        public class PropertyRequestChildViewModel
        {
            public int PropertyTypeId { get; set; }
            public long RequestChildSlNo { get; set; }
            public byte? UnitRateMethod { get; set; }
            public decimal QtyRequested { get; set; }
            public string EquipmentRequestNo { get; set; }
            public decimal ExpectedUnitRate { get; set; }
            public decimal UnitsRequested { get; set; }
            public int LineOrderNo { get; set; }
            public string PlanNo { get; set; }
            public string ItemRemarks { get; set; }
            public string AddlDescription { get; set; }
            public string DeliveryPeriod { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> SaveOrUpdatePropertyRequest([FromBody] PropertyRequestViewModel VM)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant context." });
            }

            if (VM == null || string.IsNullOrEmpty(VM.EqiupmentRequestNo))
            {
                return BadRequest(new { success = false, message = "Equipment Request No is required." });
            }

            try
            {
                // ===== MASTER TABLE =====
                var existingMaster = await dbContext.Tbl40136PropertyRequestMasters
                 .FirstOrDefaultAsync(x => x.EqiupmentRequestNo == VM.EqiupmentRequestNo);

                if (existingMaster != null)
                {
                    existingMaster.RequestDate = VM.RequestDate;
                    existingMaster.ClientCode = VM.ClientCode;
                    existingMaster.RequestedBy = VM.RequestedBy;
                    existingMaster.RequesterContactEmail = VM.RequesterContactEmail;
                    existingMaster.RequesterContact = VM.RequesterContact;
                    existingMaster.ModeOfRequest = VM.ModeOfRequest;
                    existingMaster.TypeOfRequest = VM.TypeOfRequest;
                    existingMaster.ClientRefNo = VM.ClientRefNo;
                    existingMaster.TypeOfMpr = VM.TypeOfMpr;
                    existingMaster.PurposeOfRequest = VM.PurposeOfRequest;
                    existingMaster.ExpectedDate = VM.ExpectedDate;
                    existingMaster.Remarks = VM.Remarks;
                    existingMaster.ProjectMasterCode = VM.ProjectMasterCode;
                    existingMaster.CurrencyId = VM.CurrencyId ?? 1;
					existingMaster.CurrencyRate = VM.CurrencyRate ?? 1;
                    existingMaster.BaseCurrencyId = VM.BaseCurrencyId ?? 1;
                    existingMaster.CompanyBranch = VM.CompanyBranch;
                    existingMaster.ModifiedBy = "System";
                    existingMaster.ModifiedOn = DateTime.UtcNow;
                }
                else
                {
                    var newMaster = new Tbl40136PropertyRequestMaster
                    {
                        EqiupmentRequestNo = VM.EqiupmentRequestNo,
                        RequestDate = VM.RequestDate,
                        TypeOfMpr = VM.TypeOfMpr,
                        ClientCode = VM.ClientCode,
                        RequestedBy = VM.RequestedBy,
                        RequesterContactEmail = VM.RequesterContactEmail,
                        RequesterContact = VM.RequesterContact,
                        ModeOfRequest = VM.ModeOfRequest,
                        TypeOfRequest = VM.TypeOfRequest,
                        PurposeOfRequest = VM.PurposeOfRequest,
                        ExpectedDate = VM.ExpectedDate,
                        Remarks = VM.Remarks,
                        ProjectMasterCode = VM.ProjectMasterCode,
                        CompanyBranch = VM.CompanyBranch,
                        ClientRefNo = VM.ClientRefNo,
                        CurrencyId = VM.CurrencyId ?? 1,
                        CurrencyRate = VM.CurrencyRate ?? 1,
                        BaseCurrencyId = VM.BaseCurrencyId ?? 1,
                        ModifiedBy = "System",
                        ModifiedOn = DateTime.UtcNow
                    };


                    await dbContext.Tbl40136PropertyRequestMasters.AddAsync(newMaster);
                }
                // ===== CHILDREN TABLE =====
                if (VM.EquipmentPropertychild != null && VM.EquipmentPropertychild.Any())
                {
                    // Get existing children for this request
                    var existingChildren = await dbContext.Tbl40137PropertyRequestChildren
                        .Where(c => c.EquipmentRequestNo == VM.EqiupmentRequestNo)
                        .ToListAsync();

                    // Remove old children not in the new list
                   // dbContext.Tbl40137PropertyRequestChildren.RemoveRange(existingChildren);

                    // Ensure currency rate is available
                    var currencyRate = VM.CurrencyRate ?? 1;

                    // Add new children
                        foreach (var child in VM.EquipmentPropertychild)
                        {
                            // Check if child exists in DB by PropertyTypeId (or another key)
                            var existingChild = existingChildren
                                .FirstOrDefault(x => x.RequestChildSlNo == child.RequestChildSlNo);

                            if (existingChild == null)
                            {
                                // New child → insert
                                var newChild = new Tbl40137PropertyRequestChild
                                {
                                    PropertyTypeId = child.PropertyTypeId,
                                    UnitRateMethod = child.UnitRateMethod,
                                    QtyRequested = child.QtyRequested,
                                    ExpectedUnitRate = child.ExpectedUnitRate * currencyRate,
                                    UnitsRequested = child.UnitsRequested,
                                    LineOrderNo = child.LineOrderNo,
                                    EquipmentRequestNo = VM.EqiupmentRequestNo,
                                    PlanNo = child.PlanNo,
                                    ItemRemarks = child.ItemRemarks,
                                    AddlDescription = child.AddlDescription,
                                    DeliveryPeriod = child.DeliveryPeriod
                                };

                                await dbContext.Tbl40137PropertyRequestChildren.AddAsync(newChild);
                            }
                            else
                            {
                                // Existing child → update
                                existingChild.UnitRateMethod = child.UnitRateMethod;
                                existingChild.QtyRequested = child.QtyRequested;
                                existingChild.ExpectedUnitRate = child.ExpectedUnitRate * currencyRate;
                                existingChild.UnitsRequested = child.UnitsRequested;
                                existingChild.LineOrderNo = child.LineOrderNo;
                                existingChild.EquipmentRequestNo = VM.EqiupmentRequestNo;
                                existingChild.PlanNo = child.PlanNo;
                                existingChild.ItemRemarks = child.ItemRemarks;
                                existingChild.AddlDescription = child.AddlDescription;
                                existingChild.DeliveryPeriod = child.DeliveryPeriod;
                            }
                        }
                }

                await dbContext.SaveChangesAsync();
				await _userActionLogger.LogAsync(
			  module: "ERM > Save Enquiry",
			 actionDetail: $"Saved Enquiry Request{VM.EqiupmentRequestNo}",
			  documentNo: $"{VM.EqiupmentRequestNo}"
		  );
				return Ok(new { success = true, message = "Property saved/updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in SaveOrUpdatePropertyRequest: {ex.Message}", ex);
                return StatusCode(500, new { success = false, message = "Internal server error. Please try again later." });
            }
        }

        
        [HttpGet]
		public async Task<IActionResult> GetEnquiryProperty(string EqiupmentRequestNo)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				if (string.IsNullOrEmpty(EqiupmentRequestNo))
					return BadRequest("Equipment No is required.");

				try
				{

					var RequestNo = await dbContext.Tbl40136PropertyRequestMasters
                        .Where(c => c.EqiupmentRequestNo == EqiupmentRequestNo)
						.FirstOrDefaultAsync();

					if (RequestNo == null)
						return NotFound("Equipment No not found.");

					return Ok(RequestNo);
				}
				catch (Exception ex)
				{
					_logger.LogError($"Error in GetProject: {ex.Message}");
					return StatusCode(500, $"Internal server error: {ex.Message}");
				}
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}
        [HttpPost]
        public IActionResult DeleteGridEnquiry(string EquipmentRequestNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var prMaster = dbContext.Tbl40136PropertyRequestMasters
                        .FirstOrDefault(pr => pr.EqiupmentRequestNo == EquipmentRequestNo);

                    if (prMaster == null)
                    {
                        return Json(new { success = false, message = "Equipment not found." });
                    }

                    if (prMaster.IsSubmitted.HasValue && prMaster.IsSubmitted.Value)
                    {
                        return Json(new { success = false, message = "Property Request is already submitted. You cannot delete a submitted request." });
                    }

                    if (prMaster.IsApproved.HasValue && prMaster.IsApproved.Value)
                    {
                        return Json(new { success = false, message = "Property Request is already approved. You cannot delete an approved request." });
                    }

                    // Delete child records
                    var prChildren = dbContext.Tbl40137PropertyRequestChildren
                        .Where(child => child.EquipmentRequestNo == EquipmentRequestNo);
                    dbContext.Tbl40137PropertyRequestChildren.RemoveRange(prChildren);

                    // Delete master record
                    dbContext.Tbl40136PropertyRequestMasters.Remove(prMaster);

                    dbContext.SaveChanges();

                    return Json(new { success = true, message = "Property Request has been successfully deleted." });
                }
                catch (Exception ex)
                {
                    // log ex if needed
                    return Json(new { success = false, message = "An error occurred while deleting the Property Request." });
                }
            }

            return Json(new { success = false, message = "Invalid tenant context." });
        }
        [HttpDelete("{EquipmentRequestNo}")]
        public IActionResult DeletePropertyRequest(string EquipmentRequestNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var prMaster = dbContext.Tbl40136PropertyRequestMasters
                        .FirstOrDefault(pr => pr.EqiupmentRequestNo == EquipmentRequestNo);

                    if (prMaster == null)
                    {
                        return Json(new { success = false, message = "Equipment not found." });
                    }

                    if (prMaster.IsSubmitted.HasValue && prMaster.IsSubmitted.Value)
                    {
                        return Json(new { success = false, message = "Property Request is already submitted. You cannot delete a submitted request." });
                    }

                    if (prMaster.IsApproved.HasValue && prMaster.IsApproved.Value)
                    {
                        return Json(new { success = false, message = "Property Request is already approved. You cannot delete an approved request." });
                    }

                    // Delete child records
                    var prChildren = dbContext.Tbl40137PropertyRequestChildren
                        .Where(child => child.EquipmentRequestNo == EquipmentRequestNo);
                    dbContext.Tbl40137PropertyRequestChildren.RemoveRange(prChildren);

                    // Delete master record
                    dbContext.Tbl40136PropertyRequestMasters.Remove(prMaster);

                    dbContext.SaveChanges();

                    return Json(new { success = true, message = "Property Request has been successfully deleted." });
                }
                catch (Exception ex)
                {
                    // log ex if needed
                    return Json(new { success = false, message = "An error occurred while deleting the Property Request." });
                }
            }

            return Json(new { success = false, message = "Invalid tenant context." });
        }

        [HttpGet]
		public async Task<ActionResult> GetEnquiryChildren(string EquipmentRequestNo)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					var resultWithDetails = new List<ExpandoObject>();

                    // Query the Tbl40137PropertyRequestChildren table for the given EquipmentRequestNo
                    var result = dbContext.Qry40602propertyRequestChildren
                        .Where(x => x.EquipmentRequestNo == EquipmentRequestNo)
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

                        var PropertyType = await dbContext.Tbl40110PropertyTypes
                            .Where(x => x.PropertyTypeId == gridDetails.PropertyTypeId)
                            .Select(x => x.PropertyType)
                            .FirstOrDefaultAsync();
                        // Retrieve GroupName based on QuoteGroupItemSlNo

                        var currencyRate = await dbContext.Tbl40136PropertyRequestMasters
                         .Where(x => x.EqiupmentRequestNo == EquipmentRequestNo)
                         .Select(x => x.CurrencyRate)
                         .FirstOrDefaultAsync();

                        dict["LineTotal"] = gridDetails.LineTotal / currencyRate;
                        dict["ExpectedUnitRate"] = gridDetails.ExpectedUnitRate / currencyRate;

                        resultWithDetails.Add(item);
					}

					return Json(resultWithDetails);
				}
				catch (Exception ex)
				{
					_logger.LogError($"Error in GetProject: {ex.Message}");
					return StatusCode(500, $"Internal server error: {ex.Message}");
				}
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}
		[HttpDelete]
		public async Task<IActionResult> DeletePurchaseRequest([FromQuery] string Mprno)
		{
			if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				return Unauthorized(new { success = false, message = "Invalid tenant context." });
			}

			if (string.IsNullOrEmpty(Mprno))
			{
				return BadRequest(new { success = false, message = "MPR No. is required." });
			}

			try
			{
				// Retrieve the master record
				var masterRecord = await dbContext.Tbl60601purchaseRequestMasters
					.FirstOrDefaultAsync(x => x.Mprno == Mprno);

				if (masterRecord == null)
				{
					return NotFound(new { success = false, message = "Purchase Request not found." });
				}

				// Retrieve and remove child records
				var childRecords = dbContext.Tbl60602purchaseRequestChildren
					.Where(x => x.Mprno == Mprno);

				dbContext.Tbl60602purchaseRequestChildren.RemoveRange(childRecords);

				// Remove the master record
				dbContext.Tbl60601purchaseRequestMasters.Remove(masterRecord);

				await dbContext.SaveChangesAsync();
                await _userActionLogger.LogAsync(
					module: "ERM > Delete Purchase Request",
				   actionDetail: $"Deleted Purchase Request {Mprno}",
					documentNo: $"{Mprno}"
				);

                return Ok(new { success = true, message = "Purchase Request and its details deleted successfully." });
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error in GetProject: {ex.Message}");
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

			return null;
		}



		[HttpPost]
		public async Task<IActionResult> SubmitMPR(string EqiupmentRequestNo)
		{
			// Validate tenant context
			if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				return Unauthorized(new { success = false, message = "Invalid tenant context." });
			}

			// Validate MPR number
			if (string.IsNullOrEmpty(EqiupmentRequestNo))
			{
				return BadRequest(new { success = false, message = "MPR No. is required." });
			}

			// Retrieve MPR master record
			var master = await dbContext.Tbl40136PropertyRequestMasters.
			FirstOrDefaultAsync(x => x.EqiupmentRequestNo == EqiupmentRequestNo);
			if (master == null)
			{
				return NotFound(new { success = false, message = "Equipment No not found." });
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
			var signatoryId = await GetSignatoryIDfromUserID(userId);
			if (signatoryId.HasValue)
			{
				master.RequestSignatory = (byte)signatoryId.Value;
			}
			else
			{
				master.RequestSignatory = null;
			}


			master.PurchaseRequestStatusId = 31; 

			// Save changes to the database
			await dbContext.SaveChangesAsync();
            await _userActionLogger.LogAsync(
				module: "ERM > Submit MPR",
			   actionDetail: $"Submited MPR {EqiupmentRequestNo}",
				documentNo: $"{EqiupmentRequestNo}"
			);

            return Ok(new { success = true, message = "Equipment Enquiry submitted successfully." });
		}

        [HttpPost]
        public async Task<IActionResult> VerifyMPR(string EqiupmentRequestNo)
		{
			try
			{
				if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				{
					return Unauthorized(new { message = "Invalid tenant context." });
				}

				if (string.IsNullOrEmpty(EqiupmentRequestNo))
				{
					return BadRequest(new { message = "Eqiupment Request No is required." });
				}

				var voucher = await dbContext.Tbl40136PropertyRequestMasters
						  .FirstOrDefaultAsync(v => v.EqiupmentRequestNo == EqiupmentRequestNo);

				if (voucher == null)
				{
					return NotFound(new { message = "Equipment Enquiry not found." });
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
				voucher.PurchaseRequestStatusId = 32; // Enquiry/Request Verified

				var signatoryId = await GetSignatoryIDfromUserID(userId);
				if (signatoryId.HasValue)
				{
					voucher.ERverifiedSign = (byte)signatoryId.Value;
				}

				await dbContext.SaveChangesAsync();
				   await _userActionLogger.LogAsync(
					   module: "ERM > Verify MPR",
					  actionDetail: $"Verify MPR {EqiupmentRequestNo}",
					   documentNo: $"{EqiupmentRequestNo}"
				   );

                return Ok(new
				{
					message = "Equipment Enquiry has been Verified and processed for Approval."
                });
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error in GetProject: {ex.Message}");
				return StatusCode(500, new { message = "Internal Server Error", ex.Message });
			}
		}
        [HttpPost]
      //[RequirePermission("EquipmentClientRequest_btnApprove")]
        public async Task<ActionResult> ApproveMPR(string EqiupmentRequestNo)

        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { message = "Invalid tenant." });
            }

            try
            {
                var userName = HttpContext.Session.GetString("UserName");
                var userIdString = HttpContext.Session.GetString("UserId");

                if (!int.TryParse(userIdString, out int userId))
                {
                    return Unauthorized(new { message = "Invalid or missing UserId in session." });
                }

                if (string.IsNullOrEmpty(EqiupmentRequestNo))
                {
                    return BadRequest(new { message = "Equipment No is required." });
                }

                // ✅ Use async query
                var voucher = await dbContext.Tbl40136PropertyRequestMasters
                    .FirstOrDefaultAsync(v => v.EqiupmentRequestNo == EqiupmentRequestNo);

                if (voucher == null)
                {
                    return NotFound(new { message = "Equipment No not found." });
                }

                // 🔍 Step 1: Check workflow
                var companySetting = await dbContext.Tbl901CompanyDetails02s.FirstOrDefaultAsync();
                bool isWorkflowEnabled = companySetting?.IsEnableMprworkflow == true;

                if (isWorkflowEnabled && (voucher.IsSubmitted != true || voucher.IsVerified != true))
                {
                    return BadRequest(new { message = "Please verify and submit the request before approval." });
                }

                // ✅ Step 2: Update approval fields
                voucher.IsApproved = true;
                voucher.ApprovedOn = DateTime.Now;
                voucher.ApprovedBy = userName; 
                voucher.PurchaseRequestStatusId = 33; 

                var signatoryId = await GetSignatoryIDfromUserID(userId);
                if (signatoryId.HasValue)
                {
                    voucher.ERapprovedSign = (byte)signatoryId.Value;
                }

                // ✅ Ensure EF tracks changes
                dbContext.Tbl40136PropertyRequestMasters.Update(voucher);
                await dbContext.SaveChangesAsync();
                await _userActionLogger.LogAsync(
                     module: "ERM > Approved MPR",
                    actionDetail: $"Approved MPR {EqiupmentRequestNo}",
                     documentNo: $"{EqiupmentRequestNo}"
                 );
                return Ok(new
                {
                    success = true,
                    message = "Equipment Enquiry has been Approved.",
                    voucherApprovedBy = voucher.ApprovedBy
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in ApproveMPR: {ex.Message}");
                return StatusCode(500, new { message = "Internal Server Error", detail = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> UnlockPurchaseRequest([FromBody] PropertyRequestViewModel request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.EqiupmentRequestNo))
                    return BadRequest(new { success = false, message = "Eqiupment Request No is required." });

                var tenantName = HttpContext.Session.GetString("TenantName");
                if (string.IsNullOrWhiteSpace(tenantName) || !_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized(new { success = false, message = "Tenant not found." });

                var currentUserId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));
                var userName = HttpContext.Session.GetString("UserName");

                // ✅ Fetch user level
                var userLevel = await dbContext.TblUserMasters
                    .Where(u => u.UserId == currentUserId)
                    .Select(u => u.UserLevel)
                    .FirstOrDefaultAsync();

                if (userLevel != 99)
                    return Forbid("Only admin users can unlock the Purchase Request.");

                // ✅ Fetch the MPR record
                var mpr = await dbContext.Tbl40136PropertyRequestMasters
                    .FirstOrDefaultAsync(x => x.EqiupmentRequestNo == request.EqiupmentRequestNo);

                if (mpr == null)
                    return NotFound(new { success = false, message = "Property Request not found." });

                // ✅ Unlock logic
                mpr.IsSubmitted = false;
                mpr.SubmittedBy = null;
                mpr.SubmittedOn = null;

                mpr.IsVerified = false;
                mpr.VerifiedBy = null;
                mpr.VerifiedOn = null;

                mpr.IsApproved = false;
                mpr.ApprovedBy = null;
                mpr.ApprovedOn = null;

                mpr.RequestSignatory = null;
                mpr.ERverifiedSign = null;
                mpr.ERapprovedSign = null;

                mpr.RequestSignatory = 34; // Re-Initiated

                await dbContext.SaveChangesAsync();
				await _userActionLogger.LogAsync(
					module: "IMS > Unlock the MPR",
					actionDetail: $"Unlock the MPR By Id: {request.EqiupmentRequestNo}",
				   documentNo: $"{request.EqiupmentRequestNo}"
				);

				return Ok(new { success = true, message = "Enquiry has been unlocked." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error: " + ex.Message });
            }
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





        [HttpPost]
		public async Task<ActionResult> CancelMPR(string RequestNo)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					var userName = HttpContext.Session.GetString("UserName");
					var userIdString = HttpContext.Session.GetString("UserId");

					if (string.IsNullOrEmpty(RequestNo))
					{
						return BadRequest(new { Message = "Request No is required." });
					}

					var voucher = dbContext.Tbl40136PropertyRequestMasters
								  .FirstOrDefault(v => v.EqiupmentRequestNo == RequestNo);

					if (voucher == null)
					{
						return NotFound(new { Message = "CreditNoteNo not found." });
					}
					dbContext.SaveChanges();

                    await _userActionLogger.LogAsync(
                      module: "ERM > Cancel MPR",
                     actionDetail: $"Canceled MPR: {RequestNo}",
                      documentNo: $"{RequestNo}"
                    );

                    return Ok(new
					{
						Message = "Equipment Enquiry Request has been Cancelled.",
						VoucherCancelledBy = userName
					});
				}
				catch (Exception ex)
				{
					_logger.LogError($"Error in GetProject: {ex.Message}");
					return BadRequest(new { Message = ex.Message });
				}
			}

			return Unauthorized(new { Message = "Invalid tenant.", Success = false });
		}
    }
}
