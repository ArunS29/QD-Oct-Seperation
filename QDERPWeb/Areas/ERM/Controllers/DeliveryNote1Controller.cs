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

namespace QD.ERP.Web.Areas.ERM.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DeliveryNote1Controller : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<DeliveryNote1Controller> _logger;

        public DeliveryNote1Controller(ILogger<DeliveryNote1Controller> logger, TenantDbContextHelper tenantDbContextHelper)
        {
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
                        i.StoreName
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

                    existingNote.ModifiedBy = userName;
                    existingNote.ModifiedOn = DateTime.Now;
                }

                await dbContext.SaveChangesAsync();

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

                existingNote.IsApproved = true;
                existingNote.ApprovedBy = approvedBy; // ? Correct usage
                existingNote.ApprovedOn = DateTime.Now;

                await dbContext.SaveChangesAsync();

                return Ok(new { message = "Delivery note approved successfully", success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error approving delivery note: " + ex.Message, success = false });
            }
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
                // Get tenant name from session
                var tenantName = HttpContext.Session.GetString("TenantName");
                if (string.IsNullOrWhiteSpace(tenantName))
                    return Unauthorized(new { message = "Tenant name not found in session.", success = false });

                // Resolve tenant and DB context
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    // Get company short name using tenant name
                    var companyShortName = await dbContext.Tbl901CompanyDetails
                        .Where(c => c.CompanyNameShort == tenantName)
                        .Select(c => c.CompanyNameShort)
                        .FirstOrDefaultAsync();

                    if (string.IsNullOrWhiteSpace(companyShortName))
                        return BadRequest(new { message = "Company short name not found for tenant.", success = false });

                    // Generate delivery note number
                    string shortCode = GetAbbreviatedCompanyCode(companyShortName);
                    string prefix = $"{shortCode}-DN-{DateTime.Now.Year}";

                    var lastNote = await dbContext.Tbl60301deliveryNoteMasters
                        .Where(d => d.DeliveryNoteNo.StartsWith(prefix))
                        .OrderByDescending(d => d.DeliveryNoteNo)
                        .FirstOrDefaultAsync();

                    int nextNumber = 1;
                    if (lastNote != null)
                    {
                        var lastNumberPart = lastNote.DeliveryNoteNo.Split('-').Last();
                        if (int.TryParse(lastNumberPart, out int lastNum))
                            nextNumber = lastNum + 1;
                    }

                    string formattedNumber = nextNumber.ToString("D5");
                    string deliveryNoteNo = $"{prefix}-{formattedNumber}";

                    return Ok(new { deliveryNoteNo, success = true });
                }

                return Unauthorized(new { message = "Tenant not found.", success = false });
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
                    existingItem.EmployeeName = updatedItem.EmployeeName;
                    existingItem.PropertyDescription = updatedItem.PropertyDescription;
                    existingItem.BatchNo = updatedItem.BatchNo;

                    await dbContext.SaveChangesAsync();

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
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var result = await dbContext.Tbl40111PropertyUnitCodes
                        .Select(p => new
                        {
                            UnitCode = p.UnitCode,
                            UnitDesc = p.UnitDesc
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
        public class DeliveryNoteItemsRequest
        {
            public string DeliveryNoteNo { get; set; }
            public List<DeliveryNoteItemDto> Items { get; set; }
        }

        public class DeliveryNoteItemDto
        {
            public int SNo { get; set; }
            public string ItemCode { get; set; }
            public string StockDescription { get; set; }
            public byte? UnitRateMethod { get; set; }
            public decimal? Qty { get; set; }
            public decimal? UnitCostPrice { get; set; }
            public string EmployeeName { get; set; }
            public string PropertyOrEquipment { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> SaveOrUpdateDeliveryNoteItems([FromBody] DeliveryNoteItemsRequest request)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                {
                    var deliveryNoteNo = request.DeliveryNoteNo;
                    var items = request.Items;

                    // Get existing records for DeliveryNoteNo
                    var existingItems = await dbContext.Tbl60302deliveryNoteChildren
                        .Where(x => x.DeliveryNoteNo == deliveryNoteNo)
                        .ToListAsync();

                    // Find max DeliveryNoteSlNo across all records (or filter by DeliveryNoteNo if preferred)
                    long maxSlNo = existingItems.Any() ? existingItems.Max(x => x.DeliveryNoteSlNo) : 0;

                    foreach (var item in items)
                    {
                        Tbl60302deliveryNoteChild entity = null;

                        if (item.SNo != 0)
                        {
                            // Update existing
                            entity = existingItems.FirstOrDefault(x => x.DeliveryNoteSlNo == item.SNo);
                        }

                        if (entity != null)
                        {
                            // Update existing record
                            entity.Gscode = item.ItemCode;
                            entity.UnitRateMethod = item.UnitRateMethod;
                            entity.IssuedQty = item.Qty;
                            entity.IssuedUnitPrice = item.UnitCostPrice;
                            entity.EmployeeNo = item.EmployeeName;
                            entity.PropertyNo = item.PropertyOrEquipment;
                        }
                        else
                        {


                            entity = new Tbl60302deliveryNoteChild
                            {
                                DeliveryNoteNo = deliveryNoteNo,
                                Gscode = item.ItemCode,
                                UnitRateMethod = item.UnitRateMethod,
                                IssuedQty = item.Qty,
                                IssuedUnitPrice = item.UnitCostPrice,
                                EmployeeNo = item.EmployeeName,
                                PropertyNo = item.PropertyOrEquipment,

                            };

                            dbContext.Tbl60302deliveryNoteChildren.Add(entity);
                        }
                    }

                    await dbContext.SaveChangesAsync();
                    return Ok(new { success = true, message = "Items saved/updated successfully!" });
                }

                return Unauthorized(new { success = false, message = "Invalid tenant." });
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

                var children = await dbContext.Tbl60302deliveryNoteChildren
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
                    IsApproved = master.IsApproved,

                    items = children.Select(x => new
                    {
                        SNo = x.DeliveryNoteSlNo,
                        ItemCode = x.Gscode,
                        UnitRateMethod = x.UnitRateMethod,
                        Qty = x.IssuedQty,
                        UnitCostPrice = x.IssuedUnitPrice,
                        EmployeeName = x.EmployeeNo,
                        PropertyOrEquipment = x.PropertyNo
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
                        .Select(d => new
                        {
                            d.DeliveryNoteNo,
                            d.DeliveryDate,
                            d.DeliveryIssuedTo,
                            d.Gscode,
                            d.Gsdescrpition,
                            d.UnitType,
                            d.UnitRateMethod,
                            d.IssuedQty
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


	}


}
