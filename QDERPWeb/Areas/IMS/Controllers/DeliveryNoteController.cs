using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.AspNet.Mvc;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.Areas.Finance.Reports.Payable_Statements;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using QD.ERP.Web.Services.Logging;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace QD.ERP.Web.Areas.IMS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DeliveryNoteController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<DeliveryNoteController> _logger;
        private readonly IUserActionLogger _userActionLogger;


        public DeliveryNoteController(ILogger<DeliveryNoteController> logger, TenantDbContextHelper tenantDbContextHelper, IUserActionLogger userActionLogger)
        {
            _userActionLogger = userActionLogger;
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetDeliveryNotes(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var query = dbContext.Qry60304deliveryNoteViewMasters.AsQueryable();


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
                    query = query.Where(i => i.DeliveryDate >= fromDate && i.DeliveryDate <= toDate);

                    // Fetching the data
                    var data = await query.Select(i => new
                    {
                        i.DeliveredTo,
                        i.DeliveryNoteNo,
                        i.DeliveryDate,
                        i.DeliveryIssuedTo,
                        i.Mprno,
                        i.InvoiceNo,
                        i.ClientName,
                        i.ClientPono,
                        i.ClientProject,
                        i.SalesOrderNo,
                        i.StoreName,
                        i.QuotationNo,
                        i.MaterialRequestNo,
                        i.NoteOfItems,
                        i.StoreCode,
                        i.ClientPodate,
                        i.IsVerified,
                        i.IsApproved,
                        i.PreparedBy,
                        i.PreparedOn,
                        i.ApprovedBy,
                        i.ApprovedOn,
                        i.AddedBy,
                        i.AddedOn,
                        i.ModifiedBy,
                        i.ModifiedOn,
                        i.SalesPersonCode,
                        i.Attention,
                        i.ClientContactNo,
                        i.ClientContactEmail,
                        i.CompanyBranch,
                        i.DeliveryNoteRemarks,
                        i.Salesman,
                        i.TransportedBy,
                        i.DriversName,
                        i.DriversId,
                        i.VehicleNo,
                        i.CompanyName,
                        i.IssuedFromStoreCode,
                        i.IssuedFromStoreName,
                        i.IsPosted,
                        i.VoucherNo,
                        i.PostedOn,
                        i.PostedBy,
                        i.ProjectMasterCode,
                        i.ProjectDescription,
                        i.ProjectDuration,
                        i.ProjectLocation,
                        i.InventoryMasterGroupId,
                        i.InventoryMasterGroup,
                        i.TotalIssuedQty,
                        i.SalesPersonName,
                        i.SaesPersonUserId,
                        i.QuoteTotalBeforeDiscount,

                    }).ToListAsync();

                    return Json(data);
                }
                return Unauthorized(new { message = "Invalid tenant." });
             }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching delivery notes");
                return StatusCode(500, "Internal Server Error");
            }
        }
        [HttpPost]
        public async Task<IActionResult> SaveDeliveryNote([FromBody] Tbl60301deliveryNoteMaster model)
        {
            if (model == null || !ModelState.IsValid)
                return BadRequest(new { message = "Invalid data", success = false });

            if (string.IsNullOrWhiteSpace(model.DeliveryNoteNo))
                return BadRequest(new { message = "Delivery Note No is required", success = false });

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { message = "Invalid tenant", success = false });

            try
            {
                string userName = HttpContext.Session.GetString("UserName") ?? "System";

                var existingNote = await dbContext.Tbl60301deliveryNoteMasters
                    .FirstOrDefaultAsync(x => x.DeliveryNoteNo == model.DeliveryNoteNo);
               

                if (existingNote == null)
                {
                    model.AddedBy = userName;
                    model.AddedOn = DateTime.Now;
                    //model.IsVerified = false;
                    //model.IsApproved = false;

                    dbContext.Tbl60301deliveryNoteMasters.Add(model);
                }
                else
                {
                    // Update all properties
                    existingNote.DeliveryDate = model.DeliveryDate;
                    existingNote.ClientCode = model.ClientCode;
                    existingNote.SalesPersonCode = model.SalesPersonCode;
                    existingNote.ClientPono = model.ClientPono;
                    existingNote.ClientPodate = model.ClientPodate;
                    existingNote.Attention = model.Attention;
                    existingNote.ClientContactNo = model.ClientContactNo;
                    existingNote.ClientContactEmail = model.ClientContactEmail;
                   existingNote.StoreId = model.StoreId;
                    existingNote.ProjectId = model.ProjectId;
                    existingNote.ClientProject = model.ClientProject;
                    existingNote.DeliveryType = model.DeliveryType;
                    existingNote.CompanyBranch = model.CompanyBranch;
                    existingNote.Dnsignatory = model.Dnsignatory;
                    existingNote.DeliveryNoteRemarks = model.DeliveryNoteRemarks;
                    existingNote.Salesman = model.Salesman;
                    existingNote.TransportedBy = model.TransportedBy;
                    existingNote.DriversName = model.DriversName;
                    existingNote.DriversId = model.DriversId;
                    existingNote.VehicleNo = model.VehicleNo;
                    existingNote.StoreIssuedFrom = model.StoreIssuedFrom;
                    existingNote.StoreCode = model.StoreCode;
                    existingNote.RevisionNo = model.RevisionNo;
                    existingNote.InvoiceNo = model.InvoiceNo;
                    existingNote.StoreIssuedFrom = model.StoreIssuedFrom;
                    existingNote.VoucherNo = model.VoucherNo;
                    existingNote.IsPosted = model.IsPosted;
                    existingNote.PostedOn = model.PostedOn;
                    existingNote.PostedBy = model.PostedBy;
                    existingNote.ProjectMasterCode = model.ProjectMasterCode;
                    existingNote.JobOrderNo = model.JobOrderNo;
                    existingNote.InventoryMasterGroupId = model.InventoryMasterGroupId;
                    existingNote.InventoryEffectiveDate = model.InventoryEffectiveDate;
                    existingNote.QuotationNo = model.QuotationNo;
                    existingNote.SalesOrderNo = model.SalesOrderNo;
                    existingNote.CurrencyId = model.CurrencyId ?? 1;
                    existingNote.CurrencyRate = model.CurrencyRate ?? 1;
                    existingNote.BaseCurrencyId = model.BaseCurrencyId ?? 1;
                    existingNote.ModifiedBy = userName;
                    existingNote.ModifiedOn = DateTime.Now;

                }

                await dbContext.SaveChangesAsync();
                await _userActionLogger.LogAsync(
                 module: "IMS > Save Delivery Note",
                 actionDetail: $"Saved Delivery Note  {model.DeliveryNoteNo}",
                   documentNo: $"{model.DeliveryNoteNo}"
                );

                return Ok(new
                {
                    message = "Delivery note saved successfully",
                    deliveryNoteNo = model.DeliveryNoteNo,
                    success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error saving delivery note: " + ex.Message, success = false });
            }
        }
        [HttpPost]
        public async Task<IActionResult> SubmitDN(string deliveryNoteNo)
        {
            // Validate tenant context
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant context." });
            }

            // Validate MPR number
            if (string.IsNullOrEmpty(deliveryNoteNo))
            {
                return BadRequest(new { success = false, message = "DeliveryNoteNo is required." });
            }

            // Retrieve MPR master record
            var master = await dbContext.Tbl60301deliveryNoteMasters.FirstOrDefaultAsync(x => x.DeliveryNoteNo == deliveryNoteNo);
            if (master == null)
            {
                return NotFound(new { success = false, message = "DeliveryNote not found." });
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
         

            // Retrieve signatory ID
            var signatoryId = await GetSignatoryIDfromUserID(userId);
           

            // Save changes to the database
            await dbContext.SaveChangesAsync();
            await _userActionLogger.LogAsync(
             module: "IMS > Submit DN",
              actionDetail: $"saved DN record: {deliveryNoteNo}",
             documentNo: $"{deliveryNoteNo}"
            );

            return Ok(new
            {
                success = true,
                message = "MPR submitted successfully.",
                VoucherApprovedBy = signatoryId
            });
        }
        [HttpPost]
        public async Task<IActionResult> VerifyDN(string deliveryNoteNo)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    return Unauthorized(new { message = "Invalid tenant context." });
                }

                if (string.IsNullOrEmpty(deliveryNoteNo))
                {
                    return BadRequest(new { message = "DeliveryNoteNo is required." });
                }

                var voucher = await dbContext.Tbl60301deliveryNoteMasters
                    .FirstOrDefaultAsync(v => v.DeliveryNoteNo == deliveryNoteNo);

                if (voucher == null)
                {
                    return NotFound(new { message = "Delivery Note not found." });
                }

                var userName = HttpContext.Session.GetString("UserName");
                var userIdString = HttpContext.Session.GetString("UserId");

                if (!int.TryParse(userIdString, out int userId))
                {
                    return Unauthorized(new { message = "Invalid or missing UserId in session." });
                }

                // ✅ Check Workflow Condition
                var companySetting = await dbContext.Tbl901CompanyDetails02s.FirstOrDefaultAsync();
                bool isWorkflowEnabled = companySetting?.IsEnableDeliveryNoteWorkflow == true;

                if (isWorkflowEnabled)
                {
                    if (voucher.IsSubmitted != true)
                    {
                        return BadRequest(new { message = "You need to submit the Delivery Note before verification." });
                    }
                }

                // ✅ Proceed with verification
                voucher.IsVerified = true;
                voucher.VerifiedOn = DateTime.Now;
                voucher.VerifiedBy = userName;
               
                var signatoryId = await GetSignatoryIDfromUserID(userId);
              

                await dbContext.SaveChangesAsync();
                await _userActionLogger.LogAsync(
                     module: "IMS > VerifyDN",
                        actionDetail: $"Verified DN: {deliveryNoteNo}",
                         documentNo: $"{deliveryNoteNo}"
                );


                return Ok(new
                {
                    message = "Delivery Note has been Verified and processed for Approval.",
                    VoucherApprovedBy = signatoryId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in VerifyDN: {ex.Message}");
                return StatusCode(500, new { message = "Internal Server Error", ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> ApproveDeliveryNote([FromBody] string deliveryNoteNo)
        {
            if (string.IsNullOrWhiteSpace(deliveryNoteNo))
                return BadRequest(new { message = "Delivery Note No is required", success = false });

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { message = "Invalid tenant", success = false });

            try
            {
                var existingNote = await dbContext.Tbl60301deliveryNoteMasters
                    .FirstOrDefaultAsync(x => x.DeliveryNoteNo == deliveryNoteNo);

                if (existingNote == null)
                    return NotFound(new { message = "Delivery note not found", success = false });


                string approvedBy = HttpContext.Session.GetString("UserName") ?? "System";
                var userIdString = HttpContext.Session.GetString("UserId");

                if (!int.TryParse(userIdString, out int userId))
                {
                    return Unauthorized(new { success = false, message = "Invalid or missing UserId in session." });
                }
                // ✅ Check Workflow Condition
                var companySetting = await dbContext.Tbl901CompanyDetails02s.FirstOrDefaultAsync();
                bool isWorkflowEnabled = companySetting?.IsEnableDeliveryNoteWorkflow == true;

                if (isWorkflowEnabled)
                {
                    if (existingNote.IsSubmitted != true || existingNote.IsVerified != true)
                    {
                        return BadRequest(new { message = "Please verify and submit the DeliveryNote before approval." });
                    }
                }
                existingNote.IsApproved = true;
                existingNote.ApprovedBy = approvedBy; // ? Correct usage
                existingNote.ApprovedOn = DateTime.Now;

                var signatoryId = await GetSignatoryIDfromUserID(userId);
                if (signatoryId.HasValue)
                {
                    existingNote.Dnsignatory = (byte)signatoryId.Value;
                }
                await dbContext.SaveChangesAsync();
                await _userActionLogger.LogAsync(
                   module: "IMS > Approve Delivery Note",
                   actionDetail: $"Approved Approve Delivery Note  {deliveryNoteNo}",
                   documentNo: $"{deliveryNoteNo}"
                );


                return Ok(new { message = "Delivery note approved successfully", success = true,
                    VoucherApprovedBy = signatoryId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error approving delivery note: " + ex.Message, success = false });
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

        [HttpGet("GetCompanyBranch")]
        public async Task<IActionResult> GetCompanyBranch()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                {
                    var result = await dbContext.Tbl901CompanyDetails
                        .Select(g => new
                        {
                            g.CompanyId,
                            g.CompanyName,
                        })
                        .ToListAsync();

                    return Ok(result);
                }
                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDeliveryTypes()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var deliveryTypes = await dbContext.Tbl60303deliveryTypes
                        .Select(x => new
                        {
                            Id = x.DeliveryTypeCode,
                            Name = x.DeliveryType
                        })
                        .ToListAsync();

                    return Json(deliveryTypes);
                }

                return BadRequest("Tenant context not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving delivery types: {ex.Message}");
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetInventoryGroups()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var result = await dbContext.Tbl60008inventoryMasterGroups
                        .Select(g => new
                        {
                            Id = g.InventoryMasterGroupId,
                            Name = g.InventoryMasterGroup
                        })
                        .ToListAsync();

                    return Ok(result);
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAutoGeneratedDeliveryNoteNumber()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    // 1. Get companyId from session
                    string defaultCompanyString = HttpContext.Session.GetString("DefaultcompanyID") ?? "";
                    byte defaultCompanyByte = 0;

                    if (!string.IsNullOrEmpty(defaultCompanyString))
                    {
                        byte.TryParse(defaultCompanyString, out defaultCompanyByte);
                    }

                    byte companyId = defaultCompanyByte;

                    // 2. Get company details
                    var company = await dbContext.Tbl901CompanyDetails
                        .FirstOrDefaultAsync(c => c.CompanyId == companyId);

                    if (company == null)
                    {
                        return NotFound("Company not found.");
                    }

                    // 3. Extract Delivery Note config
                    string deliveryNoteAbbrv = company.DeliveryNoteAbbrv;   // e.g. "DN-"
                    int yearDigits = company.InvoiceYearDigits ?? 0;       // No. of year digits
                    int noOfDigits = company.NoOfDigitsToDeliveryNote ?? 5; // e.g. 5 => 00001
                    bool resetByYear = company.IsResetDeliverInYear ?? false;
                    DateTime deliveryDate = DateTime.Now;

                    // 4. Generate new Delivery Note No
                    string deliveryNoteNo = await GetNewDeliveryNoteNo(
                        deliveryNoteAbbrv,
                        yearDigits,
                        noOfDigits,
                        deliveryDate,
                        resetByYear,
                        dbContext
                    );

                    return Ok(new { deliveryNoteNo, success = true });
                }
                else
                {
                    return BadRequest("Tenant or DB Context not found.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetNewDeliveryNoteNoApi: {ex.Message}");
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        private async Task<string> GetNewDeliveryNoteNo(
            string deliveryNoteAbbr,
            int yearInDigit,
            int noOfDigits,
            DateTime deliveryDate,
            bool isResetByYear,
            ERPMasterWtDataContext dbContext)
        {
            try
            {
                // 1. Query existing Delivery Note numbers
                var query = dbContext.Tbl60301deliveryNoteMasters
                    .Where(d => d.DeliveryNoteNo != null && d.DeliveryNoteNo.Length >= noOfDigits);

                if (isResetByYear)
                {
                    query = query.Where(d => d.DeliveryDate.Year == deliveryDate.Year);
                }

                var noteNumbers = await query
                    .Select(d => d.DeliveryNoteNo)
                    .ToListAsync();

                // 2. Get max running number
                int maxRunningNumber = noteNumbers
                    .Select(no => int.TryParse(no.Substring(no.Length - noOfDigits), out int num) ? num : 0)
                    .DefaultIfEmpty(0)
                    .Max();

                maxRunningNumber += 1;

                // 3. Format running number
                string strNewNo = maxRunningNumber.ToString().PadLeft(noOfDigits, '0');

                // 4. Year formatting
                string strYear = deliveryDate.Year.ToString();
                if (yearInDigit > 0 && strYear.Length >= yearInDigit)
                {
                    strYear = strYear.Substring(strYear.Length - yearInDigit, yearInDigit);
                }
                else
                {
                    strYear = "";
                }

                // 5. Final Delivery Note No
                return $"{deliveryNoteAbbr}{strYear}-{strNewNo}";
            }
            catch
            {
                string strYear = deliveryDate.Year.ToString();
                if (yearInDigit > 0 && strYear.Length >= yearInDigit)
                {
                    strYear = strYear.Substring(strYear.Length - yearInDigit, yearInDigit);
                }
                else
                {
                    strYear = "";
                }

                return $"{deliveryNoteAbbr}{strYear}-".PadRight(noOfDigits + strYear.Length + 1, '0');
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAutoGeneratedDeliveryNoteNumber1()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    // Step 1: Get DefaultcompanyID from session
                    string defaultCompanyString = HttpContext.Session.GetString("DefaultcompanyID") ?? "";
                    byte defaultCompanyByte = 0;

                    if (!string.IsNullOrEmpty(defaultCompanyString))
                    {
                        byte.TryParse(defaultCompanyString, out defaultCompanyByte);
                    }

                    byte companyId = defaultCompanyByte;

                    // Step 2: Get company details by CompanyId
                    var company = await dbContext.Tbl901CompanyDetails
                        .FirstOrDefaultAsync(c => c.CompanyId == companyId);

                    if (company == null)
                    {
                        return NotFound(new { message = "Company not found.", success = false });
                    }

                    // Step 3: Build prefix using company abbreviation and year
                    string shortCode = GetAbbreviatedCompanyCode(company.CompanyNameShort ?? "");
                    string prefix = $"{shortCode}-DN-{DateTime.Now.Year}";

                    // Step 4: Get last delivery note number matching the prefix
                    var lastNote = await dbContext.Tbl60301deliveryNoteMasters
                        .Where(d => d.DeliveryNoteNo.StartsWith(prefix))
                        .OrderByDescending(d => d.DeliveryNoteNo)
                        .FirstOrDefaultAsync();

                    int nextNumber = 1;
                    if (lastNote != null)
                    {
                        var lastNumberPart = lastNote.DeliveryNoteNo.Split('-').Last();
                        if (int.TryParse(lastNumberPart, out int lastNum))
                        {
                            nextNumber = lastNum + 1;
                        }
                    }

                    // Step 5: Format and return new delivery note number
                    string formattedNumber = nextNumber.ToString("D5");
                    string deliveryNoteNo = $"{prefix}-{formattedNumber}";

                    return Ok(new { deliveryNoteNo, success = true });
                }

                return Unauthorized(new { message = "Invalid tenant context.", success = false });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Server error: {ex.Message}", success = false });
            }
        }


        private string GetAbbreviatedCompanyCode(string companyNameShort)
        {
            if (string.IsNullOrWhiteSpace(companyNameShort))
                return "UNK"; // Default fallback

            var trimmed = companyNameShort.Trim();

            // Use as-is if short enough
            if (trimmed.Length <= 5)
                return trimmed.ToUpper();

            // Generate initials (e.g. "Pulse Infotech" -> "PI", "Al Injaz Trading" -> "AIT")
            var words = trimmed.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
            var initials = string.Concat(words.Select(w => char.ToUpper(w[0])));

            // Limit to 3 characters max, pad if less than 3
            return initials.Length >= 3 ? initials.Substring(0, 3) : initials.PadRight(3, 'T');
        }

        [HttpPost("SaveItem")]
        public async Task<IActionResult> SaveItem([FromBody] Qry60302deliveryNoteChild newItem)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                {
                    // Optional: set default values or dates here, e.g.,
                    newItem.AddedOn = DateTime.UtcNow;

                    await dbContext.Qry60302deliveryNoteChildren.AddAsync(newItem);
                    await dbContext.SaveChangesAsync();
                    await _userActionLogger.LogAsync(
                      module: "IMS > Save Item",
                      actionDetail: $"Saved Item {newItem.DeliveryNoteSlNo}",
                      documentNo: $"{newItem.DeliveryNoteSlNo}"
                    );

                    return Ok(new { success = true, message = "Item saved successfully." });
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }


        [HttpPost("UpdateItem")]
        public async Task<IActionResult> UpdateItem([FromBody] Qry60302deliveryNoteChild updatedItem)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                {
                    var existingItem = await dbContext.Qry60302deliveryNoteChildren
                        .FirstOrDefaultAsync(i => i.DeliveryNoteSlNo == updatedItem.DeliveryNoteSlNo);

                    if (existingItem == null)
                        return NotFound(new { success = false, message = "Item not found." });

                    existingItem.DeliveryRemarks = updatedItem.DeliveryRemarks;
                    existingItem.IssuedQty = updatedItem.IssuedQty;
                    existingItem.IssuedUom = updatedItem.IssuedUom;
                    existingItem.UnitRateMethod = updatedItem.UnitRateMethod;
                    existingItem.AddlDescription = updatedItem.AddlDescription;
                    //existingItem.EmployeeName = updatedItem.EmployeeName;
                    //existingItem.PropertyDescription = updatedItem.PropertyDescription;
                    existingItem.EmployeeNo = updatedItem.EmployeeNo;
                    existingItem.PropertyNo = updatedItem.PropertyNo;
                    existingItem.BatchNo = updatedItem.BatchNo;

                    await dbContext.SaveChangesAsync();
                    await _userActionLogger.LogAsync(
                      module: "IMS > Update Item",
                      actionDetail: $":Updated Item  {updatedItem.DeliveryNoteSlNo}",
                      documentNo: $"{updatedItem.DeliveryNoteSlNo}"
                    );

                    return Ok(new { success = true, message = "Item updated successfully." });
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployee()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var result = await dbContext.Tbl101Employees
                        .Select(e => new
                        {
                            EmployeeId = e.EmployeeId,
                            EmployeeName = e.EmployeeName
                        })
                        .ToListAsync();

                    return Ok(result);
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetPropertyOrEquipment()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var SubGroup = await dbContext.Tbl40101PropertyMasters.ToListAsync();
                    return Ok(SubGroup);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetDocumentTypes: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        public class DeliveryNoteItemsRequest
        {
            public string DeliveryNoteNo { get; set; }
            public decimal? CurrencyRate { get; set; }
            public int? BaseCurrencyId { get; set; }
            public int? CurrencyId { get; set; }
            public List<DeliveryNoteItemDto> Items { get; set; }
        }

        public class DeliveryNoteItemDto
        {
            public long? DeliveryNoteSlNo { get; set; }   // Nullable for new rows
            public string ItemCode { get; set; }
            public string StockDescription { get; set; }
            public byte? UnitRateMethod { get; set; }
            public decimal? Qty { get; set; }
            public decimal? UnitCostPrice { get; set; }
            public string EmployeeName { get; set; }
            public string PropertyOrEquipment { get; set; }
            public string AddlDescription { get; set; }
            public string EmployeeNo { get; set; }
            public string DeliveryRemarks { get; set; }
            public string PropertyNo { get; set; }
            public string Gscode { get; set; }
            public decimal? IssuedQuoteUnitPrice { get; set; }
            public decimal? IssuedUnitPrice { get; set; }
        }


        [HttpPost]
        public async Task<IActionResult> SaveOrUpdateDeliveryNoteItems([FromBody] DeliveryNoteItemsRequest request)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                    return Unauthorized(new { success = false, message = "Invalid tenant." });

                var deliveryNoteNo = request.DeliveryNoteNo;
                var items = request.Items ?? new List<DeliveryNoteItemDto>();

                // Load existing items for this delivery note
                var existingItems = await dbContext.Tbl60302deliveryNoteChildren
                    .Where(x => x.DeliveryNoteNo == deliveryNoteNo)
                    .ToListAsync();

                // Track incoming IDs
                var incomingIds = items
                    .Where(x => x.DeliveryNoteSlNo.HasValue)
                    .Select(x => x.DeliveryNoteSlNo.Value)
                    .ToList();

                // Delete missing rows
                var toDelete = existingItems
                    .Where(x => !incomingIds.Contains(x.DeliveryNoteSlNo))
                    .ToList();

                if (toDelete.Any())
                    dbContext.Tbl60302deliveryNoteChildren.RemoveRange(toDelete);

                // Insert / Update loop
                foreach (var dto in items)
                {
                    if (dto.DeliveryNoteSlNo.HasValue)
                    {
                        // Update
                        var entity = existingItems.FirstOrDefault(x => x.DeliveryNoteSlNo == dto.DeliveryNoteSlNo.Value);
                        if (entity != null)
                        {
                            entity.Gscode = dto.Gscode;
                            entity.UnitRateMethod = dto.UnitRateMethod;
                            entity.IssuedQty = dto.Qty;
                            entity.IssuedUnitPrice = dto.IssuedUnitPrice;
                            entity.IssuedQuoteUnitPrice = dto.IssuedQuoteUnitPrice;
                            entity.EmployeeNo = dto.EmployeeNo;
                            entity.PropertyNo = dto.PropertyNo;
                            entity.AddlDescription = dto.AddlDescription;
                            entity.DeliveryRemarks = dto.DeliveryRemarks;
                        }
                    }
                    else
                    {
                        // Insert
                        var entity = new Tbl60302deliveryNoteChild
                        {
                            DeliveryNoteNo = deliveryNoteNo,
                            Gscode = dto.Gscode,
                            UnitRateMethod = dto.UnitRateMethod,
                            IssuedQty = dto.Qty,
                            IssuedUnitPrice = dto.IssuedUnitPrice,
                            IssuedQuoteUnitPrice = dto.IssuedQuoteUnitPrice,
                            EmployeeNo = dto.EmployeeNo,
                            PropertyNo = dto.PropertyNo,
                            AddlDescription = dto.AddlDescription,
                            DeliveryRemarks = dto.DeliveryRemarks
                        };

                        dbContext.Tbl60302deliveryNoteChildren.Add(entity);
                    }
                }

                await dbContext.SaveChangesAsync();

                await _userActionLogger.LogAsync(
                    module: "IMS > Save Or Update Delivery Note Items",
                    actionDetail: $"Saved Delivery Note Items {deliveryNoteNo}",
                    documentNo: deliveryNoteNo
                );

                return Ok(new { success = true, message = "Items saved/updated successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Server error: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetByDeliveryNoteNo(string deliveryNoteNo)
        {
            if (string.IsNullOrWhiteSpace(deliveryNoteNo))
                return BadRequest(new { message = "Delivery Note No is required", success = false });

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { message = "Invalid tenant", success = false });

            try
            {
                var master = await dbContext.Tbl60301deliveryNoteMasters
                    .FirstOrDefaultAsync(x => x.DeliveryNoteNo == deliveryNoteNo);

                if (master == null)
                    return NotFound(new { message = "Delivery Note not found", success = false });

                var children = await dbContext.Qry60302deliveryNoteChildren
                    .Where(x => x.DeliveryNoteNo == deliveryNoteNo)
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    deliveryNoteNo = master.DeliveryNoteNo,
                    deliveryDate = master.DeliveryDate,
                    clientCode = master.ClientCode,
                    salesPersonCode = master.SalesPersonCode,
                    clientPono = master.ClientPono,
                    clientPodate = master.ClientPodate,
                    attention = master.Attention,
                    clientContactNo = master.ClientContactNo,
                    clientContactEmail = master.ClientContactEmail,
                    StoreCode = master.StoreCode,
                    StoreIssuedFrom = master.StoreIssuedFrom,
                    projectId = master.ProjectId,
                    clientProject = master.ClientProject,
                    deliveryNoteRemarks = master.DeliveryNoteRemarks,
                    salesman = master.Salesman,
                    transportedBy = master.TransportedBy,
                    driversName = master.DriversName,
                    driversId = master.DriversId,
                    vehicleNo = master.VehicleNo,
                    companyBranch = master.CompanyBranch,
                    deliveryType = master.DeliveryType,
                    inventoryMasterGroupId = master.InventoryMasterGroupId,
                    salesOrderNo = master.SalesOrderNo,
                    quotationNo = master.QuotationNo,
                    Dnsignatory = master.Dnsignatory,
                    CurrencyId = master.CurrencyId ?? 1,
                    CurrencyRate = master.CurrencyRate ?? 1,
                    BaseCurrencyId = master.BaseCurrencyId ?? 1,
                    IsApproved = master.IsApproved,
                    items = children.Select(x => new
                    {
                        DeliveryNoteSlNo = x.DeliveryNoteSlNo,
                        Gscode = x.Gscode,
                        ItemCode = x.Gscode,
                        UnitRateMethod = x.UnitRateMethod,
                        Qty = x.IssuedQty,
                        // UnitCostPrice = x.IssuedUnitPrice / master.CurrencyRate,
                        IssuedUnitPrice = x.IssuedUnitPrice,
                        IssuedQuoteUnitPrice = x.IssuedQuoteUnitPrice,
                        EmployeeNo = x.EmployeeNo,
                        PropertyNo = x.PropertyNo,
                        AddlDescription = x.AddlDescription,
                        DeliveryRemarks = x.DeliveryRemarks,
                        QuoteTotalBeforeDiscount=x.QuoteTotalBeforeDiscount,
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error loading delivery note", success = false, error = ex.Message });
            }
        }
        public async Task<IActionResult> GetDeliveryNoteDetails()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                {
                    var result = await dbContext.Qry60306deliveryNoteDetails
                        .Select(i=> new
                        {
                            i.DeliveryNoteNo,
                            i.DeliveryDate,
                            i.DeliveryIssuedTo,
                            i.Gscode,
                            i.Gsdescrpition,
                            i.UnitType,
                            i.UnitRateMethod,
                            i.IssuedQty,
                            i.AddedOn,
                            i.AddlDescription,
                            i.Attention,
                            i.BatchNo,
                            i.ClientAddress,
                            i.ClientContactNo,
                            i.ClientContactEmail,
                            i.ClientPodate,
                            i.ClientPono,
                            i.ClientProject,
                            i.ContactEmail,
                            i.ContactMobile1,
                            i.ContactMobile2,
                            i.ContactPhone1,
                            i.ContactPhone2,
                            i.ContactPerson,
                            i.ContactPersonTitle,
                            i.DeliveryNoteRemarks,
                            i.DeliveryRemarks,
                            i.DeliveryTypeName,
                            i.DeliveryType,
                            i.DriversName,
                            i.DriversId,
                            i.EmployeeName,
                            i.EmployeeNo,
                            i.ExpiryDate,
                            i.GsgroupName,
                            i.Hscode,
                            i.Identification,


                        })
                        .ToListAsync();

                    return Ok(result);
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<IActionResult> DeleteDeliveryItem([FromBody] string deliveryNoteNo)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                {
                    // Find the matching record from the delivery note master view/table
                    var deliveryItem = await dbContext.Qry60304deliveryNoteViewMasters
                        .FirstOrDefaultAsync(d => d.DeliveryNoteNo == deliveryNoteNo);

                    if (deliveryItem == null)
                    {
                        return NotFound(new { success = false, message = $"Delivery note '{deliveryNoteNo}' not found." });
                    }

                    
                    var entityToDelete = await dbContext.Tbl60301deliveryNoteMasters
                        .FirstOrDefaultAsync(d => d.DeliveryNoteNo == deliveryNoteNo);

                    if (entityToDelete == null)
                    {
                        return NotFound(new { success = false, message = $"No deletable record found for '{deliveryNoteNo}'." });
                    }

                    dbContext.Tbl60301deliveryNoteMasters.Remove(entityToDelete);
                    await dbContext.SaveChangesAsync();
                    await _userActionLogger.LogAsync(
                      module: "IMS > Delete Delivery Item",
                      actionDetail: $":Delete Delivery Item  {deliveryNoteNo}",
                      documentNo: $"{deliveryNoteNo}"
                    );

                    return Ok(new { success = true, message = $"Stock item '{deliveryNoteNo}' deleted successfully." });
                }

                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Server error: {ex.Message}" });
            }
        }

		[HttpGet]
		public async Task<IActionResult> GetAutoGeneratedDeliveryNoteNumberForTenant(string tenantName, string salesOrderNo)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(tenantName))
					return BadRequest(new { message = "Tenant name is required.", success = false });

				if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
					return Unauthorized(new { message = "Invalid tenant.", success = false });

				// 1. Build TenantCode
				string tenantCode;
				var words = tenantName.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
				if (words.Length == 1)
					tenantCode = $"{tenantName[0]}{tenantName[^1]}".ToUpper();
				else
					tenantCode = string.Concat(words.Select(w => w[0])).ToUpper();

				// 2. Build prefix
				var year = DateTime.Now.Year;
				var prefix = $"{tenantCode}-SO-DN-{year}-";

				// 3. Find max increment for this prefix in Tbl60301deliveryNoteMasters
				var existingNos = await dbContext.Tbl60301deliveryNoteMasters
					.Where(dn => dn.DeliveryNoteNo.StartsWith(prefix))
					.Select(dn => dn.DeliveryNoteNo)
					.ToListAsync();

				int maxIncrement = 0;
				foreach (var no in existingNos)
				{
					var parts = no.Split('-');
					if (parts.Length == 5 && int.TryParse(parts[4], out int inc))
						if (inc > maxIncrement) maxIncrement = inc;
				}

				var newIncrement = maxIncrement + 1;
				var deliveryNoteNo = $"{prefix}{newIncrement.ToString("D4")}";

				// 4. Ensure uniqueness
				bool exists = await dbContext.Tbl60301deliveryNoteMasters.AnyAsync(dn => dn.DeliveryNoteNo == deliveryNoteNo);
				if (exists)
					return Conflict(new { success = false, message = "Delivery Note No already exists." });

				return Ok(new { success = true, deliveryNoteNo });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = $"Server error: {ex.Message}", success = false });
			}
		}

		[HttpGet]
		public IActionResult GetDeliveryNotesWithDetail()
		{
			if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				return Unauthorized(new { message = "Invalid tenant." });

			var data = dbContext.Qry60311deliveryNotesWithDetails
				.Select(x => new
				{
					x.DeliveryNoteNo,
					x.DeliveryDate,
					x.DeliveryType,
					x.DeliveredTo,
					x.ClientName,
					x.ClientCode,
					x.ClientPono,
					x.SalesOrderNo,
					x.InvoiceNo,
					x.ClientAccountLedgerNo,
					x.OrderStatus,
					x.InvoiceStatus
				})
				.ToList();

			return Ok(data);
		}
		[HttpGet]
		public IActionResult GetDeliveryNotesWithDetailBySalesOrderNo(string salesOrderNo)
		{
			if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				return Unauthorized(new { message = "Invalid tenant." });

			var data = dbContext.Qry60311deliveryNotesWithDetails
				.Where(x => x.SalesOrderNo == salesOrderNo)
				.Select(x => new
				{
					x.DeliveryNoteNo,
					x.DeliveryDate,
					x.DeliveryType,
					x.DeliveredTo,
					x.ClientName,
					x.ClientCode,
					x.ClientPono,
					x.SalesOrderNo,
					x.InvoiceNo,
					x.ClientAccountLedgerNo,
					x.OrderStatus,
					x.InvoiceStatus
				})
				.ToList();

			return Ok(data);
		}


		[HttpGet]
		public IActionResult GetClientsBySalesOrderNo(string salesOrderNo)
		{
			if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				return Unauthorized(new { message = "Invalid tenant." });

			var clients = dbContext.Qry60311deliveryNotesWithDetails
				.Where(x => x.SalesOrderNo == salesOrderNo)
				.Select(x => new {
					x.ClientCode,
					x.ClientName
				})
				.Distinct()
				.ToList();

			return Ok(clients);
		}

		[HttpGet]
		public IActionResult GetSalesOrders()
		{
			if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				return Unauthorized(new { message = "Invalid tenant." });

			var orders = dbContext.Qry60311deliveryNotesWithDetails
				.Select(x => new {
					x.SalesOrderNo
				})
				.Distinct()
				.ToList();

			return Ok(orders);
		}

        [HttpGet]
        public async Task<IActionResult> GetEmployeeDetails(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var clients = dbContext.Tbl101Employees.Select(i => new
                    {
                    i.EmployeeId,
                    i.EmployeeName,
                    i.NationalId,
                    i.EmployeeReferenceId
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
        public async Task<IActionResult> GetPropertyDetails(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var clients = dbContext.Tbl40101PropertyMasters.Select(i => new
                    {
                       i.PropertyNo,
                       i.PropertyDescription
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
        public async Task<IActionResult> GetDetaildescriptiondata(long DeliveryNoteSlNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                if (DeliveryNoteSlNo == 0)
                    return BadRequest("DeliveryNoteSlNo is required.");


                try
                {

                    var client = await dbContext.Tbl60302deliveryNoteChildren
                        .Where(c => c.DeliveryNoteSlNo == DeliveryNoteSlNo)
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
        public async Task<IActionResult> GetDeliveryType(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {

                    var RequestedBy = dbContext.Tbl60303deliveryTypes.Select(i => new
                    {
                        i.DeliveryTypeCode,
                        i.DeliveryType

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
        [HttpDelete]
        public async Task<IActionResult> DeleteDeliveryNote([FromQuery] string deliveryNoteNo)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant context." });
            }

            if (string.IsNullOrEmpty(deliveryNoteNo))
            {
                return BadRequest(new { success = false, message = "DeliveryNote No. is required." });
            }

            try
            {
                // Retrieve the master record
                var masterRecord = await dbContext.Tbl60301deliveryNoteMasters
                    .FirstOrDefaultAsync(x => x.DeliveryNoteNo == deliveryNoteNo);

                if (masterRecord == null)
                {
                    return NotFound(new { success = false, message = "Delivery Note not found." });
                }

                // Retrieve and remove child records
                var childRecords = dbContext.Tbl60302deliveryNoteChildren
                    .Where(x => x.DeliveryNoteNo == deliveryNoteNo);

                dbContext.Tbl60302deliveryNoteChildren.RemoveRange(childRecords);

                // Remove the master record
                dbContext.Tbl60301deliveryNoteMasters.Remove(masterRecord);

                await dbContext.SaveChangesAsync();
                await _userActionLogger.LogAsync(
                   module: "IMS > Delete Delivery Note",
                   actionDetail: $"Deleted Delivery Note: {deliveryNoteNo}",
                   documentNo: $"{deliveryNoteNo}"
                );

                return Ok(new { success = true, message = "Delivery Note and its details deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UnlockDeliveryNote([FromBody] Tbl60301deliveryNoteMaster data)
        {
            try
            {
                 string deliveryNoteNo = data?.DeliveryNoteNo;
                if (string.IsNullOrWhiteSpace(deliveryNoteNo))
                    return BadRequest(new { success = false, message = "Delivery Note number is required." });

                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                    return Unauthorized(new { success = false, message = "Invalid tenant." });

                // ✅ 1. Check logged-in user level
              
                var currentUserId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));
                var userName = HttpContext.Items["UserName"]?.ToString();

                var userLevel = await dbContext.TblUserMasters
                    .Where(u => u.UserId == currentUserId)
                    .Select(u => u.UserLevel)
                    .FirstOrDefaultAsync();

                if (userLevel != 99)
                {
                    return Ok(new { success = false, message = "Your access level cannot unlock this Delivery Note." });
                }

                // ✅ 2. Check if Delivery Note exists
                var dn = await dbContext.Tbl60301deliveryNoteMasters
                    .FirstOrDefaultAsync(x => x.DeliveryNoteNo == deliveryNoteNo);

                if (dn == null)
                    return Ok(new { success = false, message = "Delivery Note not found." });

                // ✅ 3. Check if already posted
                var postedVoucher = await dbContext.Tbl201VoucherMasters
                    .AnyAsync(v => v.VoucherRefNo == deliveryNoteNo);

                if (postedVoucher)
                {
                    return Ok(new { success = false, message = "This Delivery Note is already posted to ledgers." });
                }

                // ✅ 4. Unlock → reset flags
                dn.IsSubmitted = false;
                dn.IsVerified = false;
                dn.IsApproved = false;
                dn.ApprovedBy = null;
                dn.ApprovedOn = null;
                dn.SubmittedBy = null;
                dn.SubmittedOn = null;
                dn.VerifiedBy = null;
                dn.VerifiedOn = null;
                await dbContext.SaveChangesAsync();

                // ✅ 5. Log the action
                await _userActionLogger.LogAsync(
                    module: "IMS Delivery Note",
                    actionDetail: $"Delivery Note {deliveryNoteNo} has been unlocked by {userName} (User ID: {currentUserId}).",
                    documentNo: deliveryNoteNo
                );

                return Ok(new { success = true, message = "Delivery Note has been unlocked successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Server error: " + ex.Message });
            }
        }

       
        public async Task<IActionResult> CreateInvoiceFromDeliveryNote([FromBody] string deliveryNoteNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(deliveryNoteNo))
                    return BadRequest(new { success = false, message = "Delivery Note No is required." });

                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                    return Unauthorized(new { success = false, message = "Invalid tenant context." });

                // 1️⃣ Check if invoice already exists for this Delivery Note
                bool isInvoiceAvailable = dbContext.Tbl60301deliveryNoteMasters
                    .Any(d => d.DeliveryNoteNo == deliveryNoteNo && !string.IsNullOrEmpty(d.InvoiceNo));

                if (isInvoiceAvailable)
                    return BadRequest(new { success = false, message = "Invoice is already created for this Delivery Note, please review again." });

                // 2️⃣ Get Ledger No for client
                var deliveryNote = await dbContext.Tbl60301deliveryNoteMasters
                    .FirstOrDefaultAsync(d => d.DeliveryNoteNo == deliveryNoteNo);

                if (deliveryNote == null)
                    return NotFound(new { success = false, message = "Delivery Note not found." });

                string ledgerNo = dbContext.Qry65113clientListWithLedgerNos
                    .Where(c => c.ClientCode == deliveryNote.ClientCode)
                    .Select(c => c.AccountId)
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(ledgerNo))
                    return BadRequest(new { success = false, message = "This Client details are not added to Accounting Ledger. Please link the client with accounts ledgers." });

                // 3️⃣ Generate new VAT Invoice No
                string defaultCompanyString = HttpContext.Session.GetString("DefaultcompanyID") ?? "";
                byte companyId = 0;
                byte.TryParse(defaultCompanyString, out companyId);

                var company = dbContext.Tbl901CompanyDetails.FirstOrDefault(c => c.CompanyId == companyId);
                if (company == null)
                    return NotFound(new { success = false, message = "Company not found." });

                string invoiceNo = GetVATInvoiceNoAPI(
     invoiceAbbr: company.IsUseEinvoiceAbbrv == true ? company.EinvoiceAbbrv : "",
     yearInDigit: company.InvoiceYearDigits ?? 0,
     invoiceDate: DateTime.Now,
     isResetByYear: company.IsResetInvoiceInYear ?? false,
     noOfDigits: company.NoOfDigitsInEinvoiceNo ?? 5   // <--- use "noOfDigits" to match method signature
        );


                // 4️⃣ Generate Invoice UUID & Counter
                string invoiceUUID = Guid.NewGuid().ToString();
              long invoiceCounter = await GetNewInvoiceCounterValueAsync();


                // 5️⃣ Call SP to insert invoice master & child
                await dbContext.Database.ExecuteSqlRawAsync(
                    "EXEC sp600_06InsertToInvoiceFromDeliveryNote @InvoiceNo={0}, @ClientAccountNo={1}, @DeliveryNoteNo={2}, @AddedBy={3}, @InvoiceUUID={4}, @InvoiceCounterValue={5}",
                    invoiceNo, ledgerNo, deliveryNoteNo, HttpContext.Session.GetString("UserName") ?? "System", invoiceUUID, invoiceCounter
                );

                // 6️⃣ Update Invoice Due Date
               // DateTime dueDate = GetDueDateOfInvoice(ledgerNo);
               // deliveryNote.InvoiceDueDate = dueDate;
                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Invoice created successfully.", invoiceNo });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Server error: " + ex.Message });
            }
        }
        public string GetVATInvoiceNoAPI(string invoiceAbbr, int yearInDigit, DateTime invoiceDate, bool isResetByYear, int noOfDigits)
        {
            string strYear = "";
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    int maxNumber = 0;

                    var query = dbContext.Tbl20161VatinvoiceMasters.AsQueryable();

                    if (isResetByYear)
                    {
                        query = query.Where(d => d.InvoiceDate.HasValue && d.InvoiceDate.Value.Year == invoiceDate.Year);
                    }

                    maxNumber = query
                        .Select(d => d.InvoiceNo)
                        .Where(no => !string.IsNullOrEmpty(no) && no.Length >= noOfDigits)
                        .AsEnumerable()
                        .Select(no => int.TryParse(no.Substring(no.Length - noOfDigits), out int number) ? number : 0)
                        .DefaultIfEmpty(0)
                        .Max();

                    maxNumber += 1;

                    strYear = invoiceDate.Year.ToString();
                    if (yearInDigit > 0 && yearInDigit <= 4)
                    {
                        strYear = strYear.Substring(strYear.Length - yearInDigit, yearInDigit);
                    }
                    else if (yearInDigit <= 0)
                    {
                        strYear = "";
                    }

                    return $"{(string.IsNullOrWhiteSpace(invoiceAbbr) ? "" : invoiceAbbr)}{strYear}-{maxNumber.ToString().PadLeft(noOfDigits, '0')}";
                }
                else
                {
                    throw new Exception("Tenant or DbContext not found");
                }
            }
            catch
            {
                strYear = invoiceDate.Year.ToString();
                if (yearInDigit > 0 && yearInDigit <= 4)
                    strYear = strYear.Substring(strYear.Length - yearInDigit, yearInDigit);
                else
                    strYear = "";

                return $"{(string.IsNullOrWhiteSpace(invoiceAbbr) ? "" : invoiceAbbr)}-{strYear}-000001";
            }
        }
        private async Task<long> GetNewInvoiceCounterValueAsync()
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                return 0;

            var counter = await dbContext.Qry00101invoiceCounterValues
     .Select(x => (long?)x.InvoiceCounterValue)
     .FirstOrDefaultAsync();

            long maxCounter = counter ?? 0;

            return maxCounter + 1;
        }

        //private DateTime GetDueDateOfInvoice(string ledgerNo)
        //{
        //    if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
        //        return DateTime.Now;

        //    var creditDays = dbContext.TblAccountLedgers
        //        .Where(l => l.LedgerNo == ledgerNo)
        //        .Select(l => (int?)l.CreditDays ?? 0)
        //        .FirstOrDefault();

        //    return DateTime.Now.AddDays(creditDays);
        //}

        //Receive button form
        [HttpGet]
        public async Task<IActionResult> GetDeliveryAmount(string deliveryNoteNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {

                try
                {

                var amount = await dbContext.Qry60303deliveryNoteItemsWithTotals
                .Where(x => x.DeliveryNoteNo == deliveryNoteNo)
                .Select(x => x.TotalAfterDiscount ?? 0)
                .FirstOrDefaultAsync();

            return Ok(new { DeliveryNoteNo = deliveryNoteNo, TotalAfterDiscount = amount });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetDeliveryAmount: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
            }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet]
        public async Task<IActionResult> GetDescription(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var clients = dbContext.Tbl20164GoodsAndServicesMasters.Select(i => new
                    {
                      i.Gscode,
                      i.Gsdescrpition,
                      i.GsuoM,
                      i.CostPrice

                    });

                    return Json(await DataSourceLoader.LoadAsync(clients, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetDescriptionDetails: {ex.Message}");
                    return StatusCode(500, new { message = "Error fetching client details", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant", success = false });
        }
        [HttpGet]
        public async Task<IActionResult> GetUOM(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var clients = dbContext.Tbl40111PropertyUnitCodes.Select(i => new
                    {
                       i.UnitCode,
                       i.UnitType
                    });

                    return Json(await DataSourceLoader.LoadAsync(clients, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetUOMDetails: {ex.Message}");
                    return StatusCode(500, new { message = "Error fetching client details", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant", success = false });
        }
       
      
        [HttpPost]
        public async Task<IActionResult> PostToBooks([FromBody] PostDeliveryNoteRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.DeliveryNoteNo))
                return BadRequest(new { success = false, message = "Delivery Note No is required." });

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { success = false, message = "Invalid tenant context." });

            try
            {
                // 1) Get master record
                var master = await dbContext.Tbl60301deliveryNoteMasters
                    .FirstOrDefaultAsync(x => x.DeliveryNoteNo == request.DeliveryNoteNo);

                if (master == null)
                    return Ok(new { success = false, message = "Delivery Note not found." });

                // 2) Already posted?
                if (master.IsPosted == true || dbContext.Tbl201VoucherMasters.Any(v => v.DeliveryNoteNo == request.DeliveryNoteNo))
                {
                    return Ok(new { success = false, message = "This Delivery Note is already posted to your ledgers." });
                }

                // 3) Must have amount
                var totalAmount = await dbContext.Qry60315deliveryNoteToJournals
                    .Where(x => x.DeliveryNoteNo == request.DeliveryNoteNo)
                    .Select(x => (decimal?)x.CostPriceTotal)
                    .SumAsync() ?? 0m;

                if (totalAmount <= 0)
                    return Ok(new { success = false, message = "This Delivery has no amount to post." });

                // 4) Choose SP based on DeliveryType
                var dt = master.DeliveryType ?? 0;
                string spName = null;

                if (dt == 2)      // Issued to Store
                    spName = "sp600_30InsertJVfromDNForStoreConsumption";
                else if (dt == 3) // Issued for Project
                    spName = "sp600_29InsertJVfromDeliveryNote";
                else
                    return Ok(new { success = false, message = "Posting for this Delivery Type is not implemented." });

                // 5) Execute SP
                var user = HttpContext.Session.GetString("UserName");
                var effectiveDate = request.DeliveryNoteDate ?? (DateTime?)master.DeliveryDate ?? DateTime.Now;

                await dbContext.Database.ExecuteSqlRawAsync(
                    $"EXEC {spName} @p0, @p1, @p2, @p3, @p4, @p5",
                    new object[]
                    {
                request.DeliveryNoteNo,
                effectiveDate,
                0, // JustAddedVoucherEntryNoSubLedger (stub)
                0, // JustAddedVoucherEntryNoCostAlloc (stub)
                user,
                DateTime.Now
                    });

                // 6) Update flags
                master.IsPosted = true;
                master.PostedBy = user;
                master.PostedOn = DateTime.Now;
                master.VoucherNo = request.DeliveryNoteNo;

                if (master.IsApproved != true)
                {
                    master.IsApproved = true;
                    master.ApprovedBy = user;
                    master.ApprovedOn = DateTime.Now;
                }

                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Delivery Note has been posted to Books." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error posting delivery note {DN}", request.DeliveryNoteNo);
                return StatusCode(500, new { success = false, message = "Error while posting: " + ex.Message });
            }
        }

        //Receive button click  form functionality
        [HttpGet]
        public async Task<IActionResult> GetReceiptNoByDeliveryNo(string deliveryNo)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized();

            try
            {
                // Check if master record already exists for this DeliveryNoteNo
                var master = await dbContext.Tbl60501materialReceiptMasters
                    .Where(m => m.DeliveryNoteNo == deliveryNo)
                    .OrderByDescending(m => m.ReceiptNo) // get last revision if multiple
                    .FirstOrDefaultAsync();

                if (master != null)
                {
                    return Ok(new { success = true, receiptNo = master.ReceiptNo, isNew = false });
                }

                // If no existing, return isNew = true (frontend can call GetNewReceiptNo)
                return Ok(new { success = true, isNew = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetNewReceiptNo(string prefix = "PRV-")
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
            {
                // 🔹 1. Get companyId from Session
                string defaultCompanyString = HttpContext.Session.GetString("DefaultcompanyID") ?? "";
                if (!byte.TryParse(defaultCompanyString, out byte companyId))
                {
                    return BadRequest(new { success = false, message = "Invalid company in session." });
                }

                // 🔹 2. Get company settings
                var company = dbContext.Tbl901CompanyDetails
                    .FirstOrDefault(c => c.CompanyId == companyId);

                if (company == null)
                {
                    return NotFound(new { success = false, message = "Company not found." });
                }

                int invoiceYearDigits = company.InvoiceYearDigits ?? 0;

                // 🔹 3. Get year part (last N digits)
                string fullYear = DateTime.Now.Year.ToString();
                string yearPart = fullYear.Substring(fullYear.Length - invoiceYearDigits, invoiceYearDigits);

                // 🔹 4. Find last receipt for this prefix + year
                var lastReceipt = dbContext.Tbl60501materialReceiptMasters
                    .Where(r => r.ReceiptNo.StartsWith(prefix + yearPart + "-") && r.ReceiptNo != "OPENING-BAL")
                    .OrderByDescending(r => r.ReceiptNo)
                    .Select(r => r.ReceiptNo)
                    .FirstOrDefault();

                int nextNo = 1;

                if (!string.IsNullOrEmpty(lastReceipt))
                {
                    string numericPart = lastReceipt.Substring(lastReceipt.Length - 5); // last 5 chars
                    if (int.TryParse(numericPart, out int lastNumeric))
                    {
                        nextNo = lastNumeric + 1;
                    }
                }

                string paddedNo = nextNo.ToString("D5");

                string newReceiptNo = $"{prefix}{yearPart}-{paddedNo}";

                return Ok(new { success = true, receiptNo = newReceiptNo });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
            }

            return Unauthorized(new { message = "Invalid tenant", success = false });
        }

        [HttpPost]
        public async Task<IActionResult> SaveMaterialReceipt([FromBody] MaterialReceiptDto dto)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized();

            if (dto == null || dto.Items == null || !dto.Items.Any())
                return BadRequest("No items to save.");

            var strategy = dbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await dbContext.Database.BeginTransactionAsync();

                try
                {
                    // 🔹 Check if master already exists
                    var master = await dbContext.Tbl60501materialReceiptMasters
                        .FirstOrDefaultAsync(m => m.ReceiptNo == dto.ReceiptNo);

                    if (master == null)
                    {
                        // ✅ Case 1: New Revision → insert Master
                        master = new Tbl60501materialReceiptMaster
                        {
                            ReceiptNo = dto.ReceiptNo,
                            DeliveryNoteNo = dto.DeliveryNoteNo,
                            ReceiptDate = dto.DeliveryDate,
                            BaseCurrencyId = 1,
                            CurrencyId = 1,
                            CurrencyRate = 1
                        };

                        dbContext.Tbl60501materialReceiptMasters.Add(master);
                        await dbContext.SaveChangesAsync();
                    }
                    else
                    {
                        // ✅ Case 2: Existing Revision → skip master
                        // Optionally you can update some fields if needed
                        master.ReceiptDate = dto.DeliveryDate;
                        dbContext.Tbl60501materialReceiptMasters.Update(master);
                    }

                    // 🔹 Always insert child records
                    foreach (var item in dto.Items)
                    {
                        var child = new Tbl60502materialReceiptChild
                        {
                            ReceiptNo = master.ReceiptNo,
                            Gscode = item.ItemCode,
                            UnitRateMethod = item.UnitRateMethod,
                            QtyReceived = item.QtyReceived,
                            UnitPrice = item.UnitPrice
                        };

                        dbContext.Tbl60502materialReceiptChildren.Add(child);
                    }

                    await dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(new

                    {
                        success = true,
                        message = master == null
                            ? "Material Receipt (new revision) saved successfully."
                            : "Child items added to existing Material Receipt.",
                        ReceiptNo = dto.ReceiptNo
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, new { success = false, message = ex.InnerException?.Message ?? ex.Message });
                }
            });
        }

        //Initially load grid data
        [HttpGet]
        public async Task<IActionResult> GetMaterialReceiptByDeliveryNo(string deliveryNo)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized();

            try
            {
                // Step 1: get ReceiptNo (Revision No) from master table
                var master = await dbContext.Tbl60501materialReceiptMasters
                    .Where(m => m.DeliveryNoteNo == deliveryNo)
                    .OrderByDescending(m => m.ReceiptNo) // if multiple revisions, get last
                    .FirstOrDefaultAsync();

                if (master == null)
                    return NotFound(new { success = false, message = "No receipt found for this Delivery Note." });

                string receiptNo = master.ReceiptNo;

                // Step 2: get child data using ReceiptNo
                var childRecords = await dbContext.Qry60502materialReceiptChildren
                    .Where(c => c.ReceiptNo == receiptNo)
                    .ToListAsync();

                return Ok(childRecords);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetMaterialReceiptByDeliveryNo: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMaterialReceipt(string ReceiptNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var result = await dbContext.Qry60502materialReceiptChildren
                        .Where(i => i.ReceiptNo == ReceiptNo)
                        .ToListAsync();

                    return Json(result);
                }
                catch (Exception ex)
                {

                    _logger.LogError($"Error in GetMaterialReceipt: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
                }
            }

            return Unauthorized();
        }
        [HttpPost]
        public async Task<IActionResult> UpdateMaterialReceipt([FromBody] MaterialReceiptChildDto dto)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { success = false, message = "Invalid tenant." });

            if (dto == null || dto.ReceiptChildSlNo <= 0)
                return BadRequest(new { success = false, message = "Invalid request data." });

            try
            {
                var child = await dbContext.Tbl60502materialReceiptChildren
                    .FirstOrDefaultAsync(x => x.ReceiptChildSlNo == dto.ReceiptChildSlNo);

                if (child == null)
                    return NotFound(new { success = false, message = "Record not found." });

                // ✅ Update fields
           
                child.QtyReceived = dto.QtyReceived;
                child.UnitRateMethod = dto.UnitRateMethod;
                child.UnitPrice = dto.UnitPrice;


                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Material Receipt updated successfully." ,
                    ReceiptNo = child.ReceiptNo
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteMaterialReceipt(string deliveryNoteNo)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized(new { success = false, message = "Invalid tenant" });

                if (string.IsNullOrEmpty(deliveryNoteNo))
                    return BadRequest(new { success = false, message = "Delivery Note No is required." });

                // ✅ Get all master records for this Delivery Note
                var masterRecords = await dbContext.Tbl60501materialReceiptMasters
                    .Where(m => m.DeliveryNoteNo == deliveryNoteNo)
                    .ToListAsync();

                if (masterRecords == null || masterRecords.Count == 0)
                    return NotFound(new { success = false, message = "No receipts found for this Delivery Note." });

                foreach (var master in masterRecords)
                {
                    // 🚨 Prevent deleting posted receipts
                    if (master.IsPosted == true)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = $"Receipt {master.ReceiptNo} is already posted to your ledgers. Delete aborted."
                        });
                    }

                    // ✅ Delete child records for this master
                    var childRecords = dbContext.Tbl60502materialReceiptChildren
                        .Where(c => c.ReceiptNo == master.ReceiptNo);

                    dbContext.Tbl60502materialReceiptChildren.RemoveRange(childRecords);

                    // ✅ Delete master
                    dbContext.Tbl60501materialReceiptMasters.Remove(master);

                    await _userActionLogger.LogAsync(
                        module: "IMS > Delete Material Receipt",
                        actionDetail: $"Deleted Material Receipt {master.ReceiptNo} (DeliveryNote: {deliveryNoteNo})",
                        documentNo: master.ReceiptNo
                    );
                }

                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = $"All receipts under Delivery Note {deliveryNoteNo} were deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in DeleteByDeliveryNote: {ex}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while deleting the data.",
                    error = ex.Message
                });
            }
        }

        //Post Journal
        [HttpPost]
        public async Task<IActionResult> PostJournal([FromBody] PostJournalDtos dto)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { success = false, message = "Invalid tenant context." });

            if (dto == null || string.IsNullOrEmpty(dto.DeliveryNoteNo))
                return BadRequest(new { success = false, message = "Delivery Note No is required." });

            try
            {
                // ✅ 1. Find Receipt using DeliveryNoteNo
                var master = await dbContext.Tbl60501materialReceiptMasters
                    .FirstOrDefaultAsync(x => x.DeliveryNoteNo == dto.DeliveryNoteNo);

                if (master == null)
                    return NotFound(new { success = false, message = "Material Receipt not found for this Delivery Note." });

                if (master.IsPosted == true)
                    return BadRequest(new { success = false, message = "This Receipt is already posted to ledgers." });

                string deliveryNoteNo = master.DeliveryNoteNo;
                string receiptNo = master.ReceiptNo;

                // ✅ 2. Generate VoucherNo
                var currentDate = master.ReceiptDate ?? DateTime.Now;
                string currentYear = currentDate.Year.ToString();
                string currentMonth = currentDate.Month.ToString("D2");
                string voucherPrefix = $"JV-{currentYear.Substring(2)}-{currentMonth}-";

                string voucherNo = await GetNewVoucherNo(dbContext, voucherPrefix);

                // ✅ 3. Call SP
                string addedBy = User?.Identity?.Name ?? "system";
                DateTime addedOn = DateTime.Now;

                await dbContext.Database.ExecuteSqlRawAsync(
                    "EXEC sp600_23InsertProductionReceiptToVoucher @p0, @p1, @p2, @p3, @p4",
                    deliveryNoteNo, voucherNo, addedBy, addedOn, receiptNo
                );

                // ✅ 4. Update master
                master.IsPosted = true;
                master.PostedBy = addedBy;
                master.PostedOn = addedOn;
                master.VoucherNo = voucherNo;
                await dbContext.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Production Receipt has been posted to the Accounting Books.",
                    voucherNo,
                    receiptNo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error posting journal: {ex}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Utility function to generate new voucher number
        private async Task<string> GetNewVoucherNo(ERPMasterWtDataContext dbContext, string prefix)
{
    // Get last voucher
    var lastVoucher = await dbContext.Tbl201VoucherMasters
        .Where(v => v.VoucherNo.StartsWith(prefix))
        .OrderByDescending(v => v.VoucherNo)
        .Select(v => v.VoucherNo)
        .FirstOrDefaultAsync();

    int nextNumber = 1;
    if (!string.IsNullOrEmpty(lastVoucher))
    {
        var parts = lastVoucher.Split('-');
        if (parts.Length == 4 && int.TryParse(parts[3], out int lastNo))
        {
            nextNumber = lastNo + 1;
        }
    }

    return $"{prefix}{nextNumber:D5}";
}

     [HttpPost]
        public async Task<IActionResult> PostJournal11(string deliveryNoteNo)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized(new { success = false, message = "Invalid tenant" });

                if (string.IsNullOrEmpty(deliveryNoteNo))
                    return BadRequest(new { success = false, message = "Delivery Note No is required." });

                // 🔹 Find receipt master
                var masterRecord = await dbContext.Tbl60501materialReceiptMasters
                                                  .FirstOrDefaultAsync(x => x.DeliveryNoteNo == deliveryNoteNo);

                if (masterRecord == null)
                    return NotFound(new { success = false, message = "Material Receipt not found for this Delivery Note." });

                if (masterRecord.IsPosted == true)
                    return BadRequest(new { success = false, message = "This Receipt is already posted to ledgers." });

                // 🔹 Generate Voucher No (same logic as your VB)
                var currentDate = masterRecord.ReceiptDate ?? DateTime.Now;
                var currentYear = currentDate.Year.ToString();
                var currentMonth = currentDate.Month.ToString("00");

                string voucherPrefix = $"JV-{currentYear.Substring(currentYear.Length - 2)}-{currentMonth}-";
                string voucherNo = await GenerateNewVoucherNoAsync(dbContext, "Journal", "JV", voucherPrefix, currentMonth, currentYear);

                // 🔹 Call Stored Procedure
                var addedBy = "System"; // or HttpContext.User.Identity.Name
                var addedOn = DateTime.Now;

                await dbContext.Database.ExecuteSqlRawAsync(
                    "EXEC sp600_23InsertProductionReceiptToVoucher @p0, @p1, @p2, @p3, @p4",
                    deliveryNoteNo, voucherNo, addedBy, addedOn, masterRecord.ReceiptNo
                );

                // 🔹 Update master record
                masterRecord.IsPosted = true;
                masterRecord.PostedBy = addedBy;
                masterRecord.PostedOn = addedOn;
                masterRecord.VoucherNo = voucherNo;

                await dbContext.SaveChangesAsync();

                await _userActionLogger.LogAsync(
                    module: "IMS > Post Journal",
                    actionDetail: $"Posted Delivery Note {deliveryNoteNo} to Journal {voucherNo}",
                    documentNo: voucherNo
                );

                return Ok(new { success = true, message = $"Delivery Note {deliveryNoteNo} posted to Journal {voucherNo}." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in PostJournal: {ex}");
                return StatusCode(500, new { success = false, message = "Error posting journal.", error = ex.Message });
            }
        }

        /// <summary>
        /// Generates new VoucherNo (similar to GetNewVoucherNo in VB)
        /// </summary>
        private async Task<string> GenerateNewVoucherNoAsync(ERPMasterWtDataContext dbContext, string voucherType, string prefix, string voucherString, string month, string year)
        {
            // 🔹 Get last voucher for this month/year
            var lastVoucher = await dbContext.Tbl201VoucherMasters
                                             .Where(x => x.VoucherNo.StartsWith(voucherString))
                                             .OrderByDescending(x => x.VoucherNo)
                                             .Select(x => x.VoucherNo)
                                             .FirstOrDefaultAsync();

            int nextNo = 1;
            if (!string.IsNullOrEmpty(lastVoucher))
            {
                var parts = lastVoucher.Split('-');
                if (parts.Length >= 3 && int.TryParse(parts.Last(), out int lastNo))
                {
                    nextNo = lastNo + 1;
                }
            }

            return voucherString + nextNo.ToString("0000"); // e.g. JV-25-09-0001
        }

       

    }
}


