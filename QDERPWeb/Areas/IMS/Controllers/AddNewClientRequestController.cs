using DevExpress.DataProcessing.InMemoryDataProcessor;
using DevExpress.XtraRichEdit.Import.Html;
using DevExtreme.AspNet.Data;

using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.Areas.VAT.Controllers;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using QD.ERP.Web.Services.Logging;
using SkiaSharp;
using System.Dynamic;
using System.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;


namespace QD.ERP.Web.Areas.IMS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AddNewClientRequestController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<AddNewClientRequestController> _logger;
        private readonly IUserActionLogger _userActionLogger;


        public AddNewClientRequestController(ILogger<AddNewClientRequestController> logger, TenantDbContextHelper tenantDbContextHelper, IUserActionLogger userActionLogger)
        {
            _userActionLogger = userActionLogger;
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }



        [HttpGet]
        public ActionResult<string> GetNewRequestNoApi()
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
                        return NotFound("Company not found.");
                    }

                    string invoiceAbbrv = company.RequestAbbrv;
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
                _logger.LogError($"Error in GetNewRequestNoApi: {ex.Message}");
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }


        private string GetNewDebitNoteNo(string invoiceAbbrv, int yearInDigit, DateTime invoiceDate, bool isResetByYear, ERPMasterWtDataContext dbContext)
        {
            try
            {
                // Retrieve MPR numbers into memory
                var mprNumbers = dbContext.Tbl60601purchaseRequestMasters
                    .Where(d => d.Mprno != null && d.Mprno.Length >= 5 &&
                                (!isResetByYear || (d.Mprdate.HasValue && d.Mprdate.Value.Year == invoiceDate.Year)))
                    .Select(d => d.Mprno)
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

                return $"{invoiceAbbrv}{strYear}-{strNewDebitNoteNo}";
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

                return $"{invoiceAbbrv}{strYear}-00001";
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
        public async Task<IActionResult> GetTax(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var ClientCategory = dbContext.Tbl20168VatpurchaseTaxSlabs.Select(i => new
                    {
                        i.PurchaseTaxSlabCode,
                        i.PurchaseTaxSlab

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
        public async Task<IActionResult> GetDefaultCompany()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    // 1. Read session
                    string defaultCompanyString = HttpContext.Session.GetString("DefaultcompanyID") ?? "";
                    byte defaultCompanyByte = 0;

                    if (!string.IsNullOrEmpty(defaultCompanyString))
                    {
                        byte.TryParse(defaultCompanyString, out defaultCompanyByte);
                    }

                    byte companyId = defaultCompanyByte;

                    // 2. Get company record
                    var company = await dbContext.Tbl901CompanyDetails
                        .Where(c => c.CompanyId == companyId)
                        .Select(c => new
                        {
                            c.CompanyId,
                            c.CompanyName
                        })
                        .FirstOrDefaultAsync();

                    if (company == null)
                    {
                        return NotFound(new { success = false, message = "Default company not found." });
                    }

                    // 3. Return in PascalCase (CompanyId, CompanyName)
                    return Ok(new
                    {
                        success = true,
                        CompanyId = company.CompanyId,
                        CompanyName = company.CompanyName
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetDefaultCompany: {ex.Message}");
                    return StatusCode(500, new { success = false, message = "Error fetching default company.", error = ex.Message });
                }
            }

            return Unauthorized(new { success = false, message = "Invalid tenant." });
        }


        [HttpGet]
        public async Task<IActionResult> GetEnquiry(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var ClientCategory = dbContext.Tbl60603purchaseRequestStatusMasters.Select(i => new
                    {
                        i.PurchaseRequestStatusId,
                        i.PurchaseRequestStatus

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
        public async Task<IActionResult> GetRequesstingUnit(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {

                    var ClientCategory = dbContext.Tbl60604purchaseRequestProjectSubUnits.Select(i => new
                    {
                        i.ProjectSubUnitCode,
                        i.ProjectSubUnitName

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
        public class SignatoryDropdownDto
        {
            public int SignatoryId { get; set; }   // use int, safer than byte for -1
            public string SignatoryName { get; set; }
        }

        [HttpGet]
        public async Task<IActionResult> GetSignatory(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    // Map DB to DTO
                    var clientCategory = await dbContext.Tbl90104DocumentSignatories
                        .Select(i => new SignatoryDropdownDto
                        {
                            SignatoryId = i.SignatoryId,
                            SignatoryName = i.SignatoryName
                        })
                        .ToListAsync();

                    clientCategory.Insert(0, new SignatoryDropdownDto
                    {
                        SignatoryId = 99,
                        SignatoryName = "<Not Assigned>"
                    });

                    return Json(DataSourceLoader.Load(clientCategory, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetSignatory: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet]
        public async Task<IActionResult> GetMPRRequesterBy(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {

                    var RequestedBy = dbContext.Tbl60002requestedBies.Select(i => new
                    {
                        i.RequestedByCode,
                        i.RequestedBy

                    });

                    return Json(await DataSourceLoader.LoadAsync(RequestedBy, loadOptions));
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
        [HttpPost]
        public async Task<ActionResult> UpdateInvoiceMasterDetails(Tbl60601purchaseRequestMaster InvoiceMaster)
        {
            if (InvoiceMaster == null)
            {
                return BadRequest(new { success = false, message = "Invalid invoice data received." });
            }

            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var existingInvoice = await dbContext.Tbl60601purchaseRequestMasters
                                                                 .FirstOrDefaultAsync(v => v.Mprno == InvoiceMaster.Mprno);

                    if (existingInvoice != null)
                    {
                        // Update existing master record
                        dbContext.Entry(existingInvoice).CurrentValues.SetValues(InvoiceMaster);
                    }
                    else
                    {
                        // Insert new invoice master record
                        await dbContext.Tbl60601purchaseRequestMasters.AddAsync(InvoiceMaster);
                    }


					await dbContext.SaveChangesAsync();
                    // await transaction.CommitAsync();

                    await _userActionLogger.LogAsync(
                      module: "IMS > Update Invoice Master Details",
                      actionDetail: $"Saved Invoice MasterDetails: {InvoiceMaster.Mprno}",
                        documentNo: InvoiceMaster.Mprno
                    );

                    return Ok(new { success = true, message = existingInvoice != null ? "Invoice and child records updated successfully!" : "New invoice and child records added successfully!" });
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error in GetProject: {ex.Message}");
				return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
			}


            return BadRequest("Failed to retrieve tenant and database context.");
        }


        [HttpPost]
        public async Task<IActionResult> SaveOrUpdatePurchaseRequest([FromBody] PurchaseRequestViewModel VM)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant context." });
            }

            if (VM == null || string.IsNullOrEmpty(VM.Mprno))
            {
                return BadRequest(new { success = false, message = "MPR No. is required." });
            }

            try
            {
                // Check if the master record exists
                var existingMaster = await dbContext.Tbl60601purchaseRequestMasters
                    .FirstOrDefaultAsync(x => x.Mprno == VM.Mprno);

                if (existingMaster != null)
                {
                    //Update existing master with manual property mapping

                    existingMaster.Mprdate = VM.Mprdate;
                    existingMaster.ClientCode = VM.ClientCode;
                    existingMaster.RequestedBy = VM.RequestedBy;
                    existingMaster.RequesterContactEmail = VM.RequesterContactEmail;
                    existingMaster.RequesterContact = VM.RequesterContact;
                    existingMaster.SalesPersonCode = VM.SalesPersonCode;
                    existingMaster.ClientRefNo = VM.ClientRefNo;
                    existingMaster.PurposeOfRequest = VM.PurposeOfRequest;
                    existingMaster.Priority = VM.Priority;
                    existingMaster.CostCenterText = VM.CostCenterText;
                    existingMaster.ExpectedDate = VM.ExpectedDate;
                    existingMaster.Remarks = VM.Remarks;
                    existingMaster.ProjectMasterCode = VM.ProjectMasterCode;
                    existingMaster.BidClosingDate = VM.BidClosingDate;
                    existingMaster.BidReminderOn = VM.BidReminderOn;
                    existingMaster.ClientProject = VM.ClientProject;
                    existingMaster.ProjectSubUnitCode = VM.ProjectSubUnitCode.HasValue ? (byte?)VM.ProjectSubUnitCode.Value : null;
                    existingMaster.StoreCode = VM.StoreCode;
                    existingMaster.TypeOfMpr = VM.TypeOfMpr.HasValue ? (byte?)VM.TypeOfMpr.Value : null;
                    existingMaster.ModeOfRequest = VM.ModeOfRequest.HasValue ? (byte?)VM.ModeOfRequest.Value : null;
                    existingMaster.TypeOfRequest = VM.TypeOfRequest.HasValue ? (byte?)VM.TypeOfRequest.Value : null;
                    existingMaster.ExpectedVatrate = VM.ExpectedVatrate.HasValue ? (byte?)VM.ExpectedVatrate.Value : null;
                    existingMaster.CompanyBranch = VM.CompanyBranch.HasValue ? (byte?)VM.CompanyBranch.Value : null;
                    existingMaster.PurchaseRequestStatusId = VM.PurchaseRequestStatusId.HasValue ? (byte?)VM.PurchaseRequestStatusId.Value : null;
                    existingMaster.InventoryMasterGroupId = VM.InventoryMasterGroupId.HasValue ? (byte?)VM.InventoryMasterGroupId.Value : null;
                    existingMaster.RequestSignatory = VM.RequestSignatory.HasValue ? (byte?)VM.RequestSignatory.Value : null;
                    existingMaster.MprverifiedSign = VM.MprverifiedSign.HasValue ? (byte?)VM.MprverifiedSign.Value : null;
                    existingMaster.MprapprovedSign = VM.MprapprovedSign.HasValue ? (byte?)VM.MprapprovedSign.Value : null;
                    existingMaster.CurrencyId = VM.CurrencyId ?? 1;
                    existingMaster.CurrencyRate = VM.CurrencyRate ?? 1;
                    existingMaster.BaseCurrencyId = VM.BaseCurrencyId ?? 1;


                }
                else
                {
                    // Insert new master
                    var newMaster = new Tbl60601purchaseRequestMaster
                    {
                        Mprno = VM.Mprno,
                        Mprdate = VM.Mprdate,
                        ClientCode = VM.ClientCode,
                        RequestedBy = VM.RequestedBy,
                        RequesterContactEmail = VM.RequesterContactEmail,
                        RequesterContact = VM.RequesterContact,
                       // ModeOfRequest = Convert.ToByte(VM.ModeOfRequest),
                        //TypeOfRequest = Convert.ToByte(VM.TypeOfRequest),
                        ModeOfRequest = VM.ModeOfRequest.HasValue ? (byte?)VM.ModeOfRequest.Value : null,
                        TypeOfRequest = VM.TypeOfRequest.HasValue ? (byte?)VM.TypeOfRequest.Value : null,

                        SalesPersonCode = VM.SalesPersonCode,
                        ClientRefNo = VM.ClientRefNo,
                        PurposeOfRequest = VM.PurposeOfRequest,
                        Priority = VM.Priority,
                        CostCenterText = VM.CostCenterText,
                        ExpectedDate = VM.ExpectedDate,
                        //ExpectedVatrate = Convert.ToByte(VM.ExpectedVatrate),
                        ExpectedVatrate = VM.ExpectedVatrate.HasValue ? (byte?)VM.ExpectedVatrate.Value : null,
                        Remarks = VM.Remarks,
                        CompanyBranch = Convert.ToByte(VM.CompanyBranch),
                        //PurchaseRequestStatusId = Convert.ToByte(VM.PurchaseRequestStatusId),
                        PurchaseRequestStatusId = VM.PurchaseRequestStatusId.HasValue ? (byte?)VM.PurchaseRequestStatusId.Value : null,
                        InventoryMasterGroupId = Convert.ToByte(VM.InventoryMasterGroupId),
                        ProjectMasterCode = VM.ProjectMasterCode,
                        BidClosingDate = VM.BidClosingDate,
                        BidReminderOn = VM.BidReminderOn,
                        ClientProject = VM.ClientProject,
                        RequestSignatory = VM.RequestSignatory,
                        MprverifiedSign = VM.MprverifiedSign,
                        MprapprovedSign = VM.MprapprovedSign,
                        // ProjectSubUnitCode = Convert.ToByte(VM.ProjectSubUnitCode),
                        ProjectSubUnitCode = VM.ProjectSubUnitCode.HasValue ? (byte?)VM.ProjectSubUnitCode.Value : null,
                        StoreCode = VM.StoreCode,
                        TypeOfMpr = Convert.ToByte(VM.TypeOfMpr),
                        CurrencyId = VM.CurrencyId ?? 1,
                        CurrencyRate = VM.CurrencyRate ?? 1,
                        BaseCurrencyId = VM.BaseCurrencyId ?? 1,

                    };



                    await dbContext.Tbl60601purchaseRequestMasters.AddAsync(newMaster);
                }

                // Handle child entries
                // Handle child entries properly
                var existingChildren = await dbContext.Tbl60602purchaseRequestChildren
                    .Where(x => x.Mprno == VM.Mprno)
                    .ToListAsync();
        
                // Track IDs received from frontend
                var incomingIds = VM.PurchaseRequestDetails
                    .Where(x => x.MprchildSlNo > 0)
                    .Select(x => x.MprchildSlNo)
                    .ToList();

                // Delete children that are in DB but not in the updated list
                var toDelete = existingChildren
                    .Where(x => !incomingIds.Contains(x.MprchildSlNo))
                    .ToList();

                if (toDelete.Any())
                {
                    dbContext.Tbl60602purchaseRequestChildren.RemoveRange(toDelete);
                }

                // Now handle Add or Update
                foreach (var child in VM.PurchaseRequestDetails)
                {
                    child.Mprno = VM.Mprno; // ensure FK set
                    child.ExpectedUnitRate = child.ExpectedUnitRate * VM.CurrencyRate;
                    if (child.MprchildSlNo == 0)
                    {
                        // New row
                        await dbContext.Tbl60602purchaseRequestChildren.AddAsync(child);
                    }
                    else
                    {
                        // Update existing
                        var existingChild = existingChildren
                            .FirstOrDefault(x => x.MprchildSlNo == child.MprchildSlNo);

                        if (existingChild != null)
                        {
                            dbContext.Entry(existingChild).CurrentValues.SetValues(child);
                        }
                    }
                }


                await dbContext.SaveChangesAsync();


				return Ok(new { success = true, message = "Request Details Updated Successfully.", mprno = VM.Mprno });

			}
			catch (Exception ex)
			{
				_logger.LogError($"Error in GetProject: {ex.Message}");
				return StatusCode(500, new { success = false, message = ex.Message });
			}
		}


        [HttpGet]
        public async Task<IActionResult> GetClientdataByCode(string Mprno)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                if (string.IsNullOrEmpty(Mprno))
                    return BadRequest("Client Code is required.");

                try
                {

                    var client = await dbContext.Tbl60601purchaseRequestMasters
                        .Where(c => c.Mprno == Mprno)
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
        public async Task<ActionResult> GetPurchaseRequestChildren(string Mprno)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var resultWithDetails = new List<ExpandoObject>();

                    // Query the Tbl60602purchaseRequestChildren table for the given Mprno
                    var result = dbContext.Qry60602purchaseRequestChildren
                        .Where(x => x.Mprno == Mprno)
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

                        // Retrieve GroupName based on QuoteGroupItemSlNo
                        var groupName = await dbContext.Tbl60107quotationChildItemGroups
                            .Where(x => x.QuoteGroupItemSlNo == gridDetails.QuoteGroupItemSlNo)
                            .Select(x => x.GroupName)
                            .FirstOrDefaultAsync();

                        // Retrieve Gsdescription based on Gscode
                        var gsDescription = await dbContext.Tbl20164GoodsAndServicesMasters
                            .Where(x => x.Gscode == gridDetails.Gscode)
                            .Select(x => x.Gsdescrpition)
                            .FirstOrDefaultAsync();
                        var currencyRate = await dbContext.Tbl60601purchaseRequestMasters
                            .Where(x => x.Mprno == Mprno)
                            .Select(x => x.CurrencyRate)
                            .FirstOrDefaultAsync();

                        // Add the retrieved values to the dynamic object
                        dict["UnitDesc"] = unitDesc;
                        dict["GroupName"] = groupName;
                        dict["GsDescription"] = gsDescription;
                        dict["GSCode"] = gridDetails.Gscode;
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
        public async Task<IActionResult> DeleteChildById(int childId)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant context." });
            }

			try
			{
				var child = await dbContext.Tbl60602purchaseRequestChildren
					.FirstOrDefaultAsync(x => x.MprchildSlNo == childId);

				if (child == null)
				{
					return NotFound(new { success = false, message = "Child record not found." });
				}

				dbContext.Tbl60602purchaseRequestChildren.Remove(child);
				await dbContext.SaveChangesAsync();

				await _userActionLogger.LogAsync(
					module: "IMS > Delete Child By Id",
					actionDetail: $"Deleted Child By Id: {childId}",
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
        [HttpDelete]
        public async Task<IActionResult> DeleteMultipleChildren([FromBody] List<int> childIds)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant context." });
            }

            if (childIds == null || !childIds.Any())
            {
                return BadRequest(new { success = false, message = "No child IDs provided." });
            }

            try
            {
                var childrenToDelete = await dbContext.Tbl60602purchaseRequestChildren
                    .Where(x => childIds.Contains((int)x.MprchildSlNo))
                    .ToListAsync();

                if (!childrenToDelete.Any())
                {
                    return NotFound(new { success = false, message = "No matching child records found." });
                }

                dbContext.Tbl60602purchaseRequestChildren.RemoveRange(childrenToDelete);
                await dbContext.SaveChangesAsync();
                await _userActionLogger.LogAsync(
                   module: "IMS > Delete Multiple Children",
                   actionDetail: $"Deleted Multiple Children: {childIds}",
                    documentNo: $"{childIds}"
                );

                return Ok(new { success = true, message = "Selected child rows deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in DeleteMultipleChildren: {ex.Message}");

                return StatusCode(500, new { success = false, message = "Internal server error." });
            }
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
                   module: "IMS > Delete Purchase Request",
                   actionDetail: $"Deleted Purchase Request: {Mprno}",
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

            // Tenant context is invalid; return null
            return null;
        }

        [HttpPost]
        public async Task<IActionResult> SubmitMPR(string mprNo)
        {

            // Validate tenant context
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant context." });
            }

            // Validate MPR number
            if (string.IsNullOrEmpty(mprNo))
            {
                return BadRequest(new { success = false, message = "MPR No. is required." });
            }

            // Retrieve MPR master record
            var master = await dbContext.Tbl60601purchaseRequestMasters.FirstOrDefaultAsync(x => x.Mprno == mprNo);
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
            var signatoryId = await GetSignatoryIDfromUserID(userId);
            if (signatoryId.HasValue)
            {
                master.RequestSignatory = (byte)signatoryId.Value;
            }
            else
            {
                master.RequestSignatory = null;
            }


            master.PurchaseRequestStatusId = 31; // Enquiry/Request Submitted

			// Save changes to the database
			await dbContext.SaveChangesAsync();
            await _userActionLogger.LogAsync(
             module: "IMS > Submit MPR",
              actionDetail: $"saved MPR record: {mprNo}",
             documentNo: $"{mprNo}"
            );

            return Ok(new { success = true, message = "MPR submitted successfully.",
                VoucherApprovedBy = signatoryId
            });
        }
        [HttpPost]
        public async Task<IActionResult> VerifyMPR(string mprNo)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    return Unauthorized(new { message = "Invalid tenant context." });
                }

                if (string.IsNullOrEmpty(mprNo))
                {
                    return BadRequest(new { message = "MPR No is required." });
                }

                var voucher = await dbContext.Tbl60601purchaseRequestMasters
                    .FirstOrDefaultAsync(v => v.Mprno == mprNo);

                if (voucher == null)
                {
                    return NotFound(new { message = "MPR not found." });
                }

                var userName = HttpContext.Session.GetString("UserName");
                var userIdString = HttpContext.Session.GetString("UserId");

                if (!int.TryParse(userIdString, out int userId))
                {
                    return Unauthorized(new { message = "Invalid or missing UserId in session." });
                }

                // ✅ Check Workflow Condition
                var companySetting = await dbContext.Tbl901CompanyDetails02s.FirstOrDefaultAsync();
                bool isWorkflowEnabled = companySetting?.IsEnableMprworkflow == true;

                if (isWorkflowEnabled)
                {
                    if (voucher.IsSubmitted != true)
                    {
                        return BadRequest(new { message = "You need to submit the request before verification." });
                    }
                }

                // ✅ Proceed with verification
                voucher.IsVerified = true;
                voucher.VerifiedOn = DateTime.Now;
                voucher.VerifiedBy = userName;
                voucher.PurchaseRequestStatusId = 32; // Enquiry/Request Verified

                var signatoryId = await GetSignatoryIDfromUserID(userId);
                if (signatoryId.HasValue)
                {
                    voucher.MprverifiedSign = (byte)signatoryId.Value;
                }

                await dbContext.SaveChangesAsync();
                await _userActionLogger.LogAsync(
                     module: "IMS > VerifyMPR",
                        actionDetail: $"Verified MPR: {mprNo}",
                         documentNo: $"{mprNo}"
                );


                return Ok(new
                {
                    message = "Material Purchase Request has been Verified and processed for Approval.",
                    VoucherApprovedBy = signatoryId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in VerifyMPR: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            
            }
        }

        [HttpPost]
        public async Task<ActionResult> ApproveMPR(string mprNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var userName = HttpContext.Session.GetString("UserName");
                    var userIdString = HttpContext.Session.GetString("UserId");
                    if (!int.TryParse(userIdString, out int userId))
                    {
                        return Unauthorized(new { Message = "Invalid or missing UserId in session." });
                    }

                    if (string.IsNullOrEmpty(mprNo))
                    {
                        return BadRequest(new { Message = "MPR No is required." });
                    }

                    var voucher = dbContext.Tbl60601purchaseRequestMasters
                                           .FirstOrDefault(v => v.Mprno == mprNo);

                    if (voucher == null)
                    {
                        return NotFound(new { Message = "MPR No not found." });
                    }

                    // 🔍 Step 1: Check MPR workflow setting
                    var companySetting = await dbContext.Tbl901CompanyDetails02s.FirstOrDefaultAsync();
                    bool isWorkflowEnabled = companySetting?.IsEnableMprworkflow == true;

                    if (isWorkflowEnabled)
                    {
                        if (voucher.IsSubmitted != true || voucher.IsVerified != true)
                        {
                            return BadRequest(new { Message = "Please verify and submit the request before approval." });
                        }
                    }

                    // ✅ Step 2: Proceed with approval
                    voucher.IsApproved = true;
                    voucher.ApprovedOn = DateTime.Now;
                    voucher.ApprovedBy = userName;
                    voucher.PurchaseRequestStatusId = 33; // Status: Enquiry/Request Approved

                    var signatoryId = await GetSignatoryIDfromUserID(userId);
                    if (signatoryId.HasValue)
                    {
                        voucher.MprapprovedSign = (byte)signatoryId.Value;
                    }

                    await dbContext.SaveChangesAsync();
                    await _userActionLogger.LogAsync(
                    module: "IMS > Approve MPR",
                       actionDetail: $"Approved MPR: {mprNo}",
                        documentNo: $"{mprNo}"
                    );

                    return Ok(new
                    {
                        Message = "Material Purchase Request has been Approved.",
                        VoucherApprovedBy = signatoryId
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in ApproveMPR: {ex.Message}");
                    return BadRequest(new { Message = ex.Message });
                }
            }

            return Unauthorized(new { Message = "Invalid tenant." });
        }


        [HttpPost]
        public async Task<ActionResult> CancelMPR(string mprNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var userName = HttpContext.Session.GetString("UserName");
                    var userIdString = HttpContext.Session.GetString("UserId");

                    if (string.IsNullOrEmpty(mprNo))
                    {
                        return BadRequest(new { Message = "Mprno is required." });
                    }

                    var voucher = dbContext.Tbl60601purchaseRequestMasters
                                           .FirstOrDefault(v => v.Mprno == mprNo);

                    if (voucher == null)
                    {
                        return NotFound(new { Message = "Mprno not found." });
                    }

                    // Update cancellation details
                    voucher.IsCancelled = true;
                    voucher.CancelledOn = DateTime.Now;
                    voucher.CancelledBy = userName;
                    voucher.PurchaseRequestStatusId = 35; // Status: Enquiry/Request Cancelled

					dbContext.SaveChanges();
                    _userActionLogger.LogAsync(module: "IMS > Cancel MPR  ",
                      actionDetail: $":Canceled MPR {mprNo}",
                      documentNo: $"{mprNo}"
                    );

                    return Ok(new
					{
						Message = "Material Purchase Request has been Cancelled.",
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
        [HttpGet]
        public IActionResult GetClientContactDetails(string clientCode)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                // Replace with your actual data retrieval logic
                var client = dbContext.Tbl30101ClientMasters
                                 .FirstOrDefault(c => c.ClientCode == clientCode);

                if (client != null)
                {
                    return Json(new
                    {
                        ContactName = client.ContactPerson,
                        ContactEmail = client.ContactEmail,
                        ContactMobile = client.ContactMobile1
                    });
                }
                else
                {
                    return NotFound();
                }
            }

            return Unauthorized(new { Message = "Invalid tenant.", Success = false });

        }


        [HttpGet]
        public async Task<ActionResult> GetRequestNoteApprovalStatus(string RequestNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var requestNo = await dbContext.Tbl60601purchaseRequestMasters

                        .Where(i => i.Mprno == RequestNo)
                        .FirstOrDefaultAsync();

                    if (requestNo == null)
                    {
                        return Ok(new { isApproved = false }); // Safe fallback
                    }

                    return Ok(new { isApproved = requestNo.IsApproved ?? false });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error fetching approval status for RequestNo {RequestNo}: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized("Unable to fetch tenant information.");
        }
        [HttpGet]
        public IActionResult GetSalesPersonByUser()
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                _logger.LogWarning("GetSalesPersonByUser failed: Invalid tenant context.");
                return Unauthorized("Invalid tenant.");
            }

            var userIdStr = HttpContext.Session.GetString("UserId");
            var userName = HttpContext.Session.GetString("UserName") ?? "Unknown";

            if (!int.TryParse(userIdStr, out int userId))
            {
                _logger.LogWarning("GetSalesPersonByUser failed: User not logged in or invalid UserId. Username: {UserName}", userName);
                return Unauthorized("User not logged in.");
            }

            _logger.LogInformation("GetSalesPersonByUser called by UserId: {UserId}, UserName: {UserName}", userId, userName);

            var salesPerson = dbContext.Tbl20101SalesPersonMasters
                .Where(s => s.UserCode == userId)
                .Select(s => new
                {
                    s.SalesPersonCode,
                    s.SalesPersonName
                })
                .FirstOrDefault();

            if (salesPerson == null)
            {
                _logger.LogWarning("Sales person not found for UserId: {UserId}", userId);
                return NotFound("Sales person not found for this user.");
            }

            _logger.LogInformation("Sales person found for UserId: {UserId} -> Code: {Code}, Name: {Name}", userId, salesPerson.SalesPersonCode, salesPerson.SalesPersonName);

            return Ok(salesPerson);
        }
        [HttpGet]
        public IActionResult GetGSCodeDescription(string gsCode)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
            {
                var description = dbContext.Tbl20164GoodsAndServicesMasters
                    .Where(x => x.Gscode == gsCode)
                    .Select(x => x.Gsdescrpition)
                    .FirstOrDefault();

                return Ok(description ?? "");
            }

            return BadRequest("Failed to resolve tenant");
        }

        [HttpGet]
        public IActionResult GetGSCodeDetailedDescription(string gsCode)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
            {
                var detailedDesc = dbContext.Tbl20164GoodsAndServicesMasters
                    .Where(x => x.Gscode == gsCode)
                    .Select(x => x.GsdetailedDesc)
                    .FirstOrDefault();

                return Ok(detailedDesc ?? "");
            }

            return BadRequest("Failed to resolve tenant");
        }

        [HttpGet]
        public async Task<IActionResult> GetdataByGSCode(string GSCode)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                if (string.IsNullOrEmpty(GSCode))
                    return BadRequest("GSCode  is required.");

                try
                {

                    var client = await dbContext.Tbl20164GoodsAndServicesMasters
                        .Where(c => c.Gscode == GSCode)
                        .FirstOrDefaultAsync();

                    if (client == null)
                        return NotFound("GS data not found.");

                    return Ok(client);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetGSData: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet]
        public async Task<IActionResult> GetDetaildescriptiondata(long MprchildSlNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                if (MprchildSlNo == 0)
                    return BadRequest("MPR Child Sl No is required.");


                try
                {

                    var client = await dbContext.Tbl60602purchaseRequestChildren
                        .Where(c => c.MprchildSlNo == MprchildSlNo)
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

    }
}
