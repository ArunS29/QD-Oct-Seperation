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
                    model.IsVerified = false;
                    model.IsApproved = false;

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
                        DeliveryRemarks = x.DeliveryRemarks
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
                dn.IsVerified = false;
                dn.IsApproved = false;
                dn.ApprovedBy = null;
                dn.ApprovedOn = null;

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



    }

}
