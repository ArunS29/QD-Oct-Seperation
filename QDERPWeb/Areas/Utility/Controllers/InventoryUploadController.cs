using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


namespace QD.ERP.Web.Areas.Utility.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class InventoryUploadController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<InventoryUploadController> _logger;

        public InventoryUploadController(ILogger<InventoryUploadController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpGet("GetStock")]
        public IActionResult GetStock()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var stocks = dbContext.Tbl20165GoodsAndServicesGroups
                        .Select(stock => new
                        {
                            stock.GsgroupId,
                            stock.GsgroupName
                        })
                        .ToList();

                    return Ok(stocks);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error loading stocks: {ex.Message}");
                    return StatusCode(500, new { message = "Failed to load stock list.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant." });
        }

        public class UploadStockRequest
        {
            public string GSGroupCode { get; set; }
            public string RequestNo { get; set; }

            [Required(ErrorMessage = "The Items array is required.")]
            [MinLength(1, ErrorMessage = "At least one item is required.")]
            public List<UploadItemDto> Items { get; set; }

            public int GSGroupID { get; set; }
        }
        public class UploadItemDto
        {
            public string GSCode { get; set; }

            [JsonPropertyName("gsDescription")]
            public string GSDescription { get; set; }

            public string GsdescriptionAr { get; set; }

            [Range(1, int.MaxValue, ErrorMessage = "RequestQty must be greater than 0.")]
            public decimal RequestQty { get; set; }

            [Range(0, double.MaxValue, ErrorMessage = "UnitPrice cannot be negative.")]
            public decimal UnitPrice { get; set; }

            public string UnitCode { get; set; }
            public string GsuomDesc { get; set; }
            public string PlanNo { get; set; }
            public string Manufacturer { get; set; }
            public string DeliveryPeriod { get; set; }
            public string Remarks { get; set; }
            public string ItemSize { get; set; }
            public string ItemPartNo { get; set; }
            public string ItemBrand { get; set; }
            public string ItemColor { get; set; }
            public string ItemDimension { get; set; }
            public object ItemThickness { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> UploadStock([FromBody] UploadStockRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogError("Model validation failed: {@Errors}", errors);
                return BadRequest(new { message = "Invalid payload", errors });
            }

            if (request == null)
            {
                _logger.LogError("UploadStock request is null.");
                return BadRequest(new { success = false, message = "Request cannot be null." });
            }

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized("Invalid tenant.");
            }

            if (request.GSGroupID == 0)
            {
                return BadRequest(new { success = false, message = "GSGroupCode is required." });
            }

            if (request.Items == null || request.Items.Count == 0)
            {
                _logger.LogWarning("UploadStock request contains an empty Items array.");
                return BadRequest(new { success = false, message = "At least one item is required." });
            }
            
            var code = dbContext.Tbl20165GoodsAndServicesGroups
                    .Where(x => x.GsgroupId == request.GSGroupID)
                    .Select(x => x.GsgroupCode)
                    .FirstOrDefault();
            var value1 = await dbContext.Tbl40111PropertyUnitCodes
            .MaxAsync(u => (int?)u.UnitCode) ?? 0;

            var value2 = await dbContext
                .Tbl20164GoodsAndServicesMasters
                .Where(g => g.Gscode.StartsWith(code + "-"))
                .Select(g => (int?)Convert.ToInt32(g.Gscode.Substring(g.Gscode.Length - 5)))
                .MaxAsync() ?? 0;
            foreach (var item in request.Items)
            {
                if (string.IsNullOrEmpty(item.GSDescription))
                    return BadRequest(new { success = false, message = "Each item must have a GSDescription." });

                if (item.RequestQty <= 0)
                    return BadRequest(new { success = false, message = "Each item must have a positive RequestQty." });

                if (item.UnitPrice < 0)
                    return BadRequest(new { success = false, message = "UnitPrice cannot be negative." });
            }

            var executionStrategy = dbContext.Database.CreateExecutionStrategy();

            await executionStrategy.ExecuteAsync(async () =>
            {
                using var transaction = await dbContext.Database.BeginTransactionAsync();
                try
                {
                    int slNoCounter = await dbContext.Tbl60005inventoryUploads.MaxAsync(x => (int?)x.SlNo) ?? 0;

                    var existingUnitCodes = await dbContext.Tbl40111PropertyUnitCodes
                        .Select(u => u.UnitDesc)
                        .ToListAsync();

                    var newUnitsToInsert = new List<Tbl40111PropertyUnitCode>();

                    foreach (var item in request.Items)
                    {
                        if (!string.IsNullOrWhiteSpace(item.GsuomDesc) &&
                            !existingUnitCodes.Contains(item.GsuomDesc, StringComparer.OrdinalIgnoreCase) &&
                            !newUnitsToInsert.Any(u => string.Equals(u.UnitDesc, item.GsuomDesc, StringComparison.OrdinalIgnoreCase)))
                        {
                            newUnitsToInsert.Add(new Tbl40111PropertyUnitCode
                            {
                                UnitCode = 0, 
                                UnitDesc = item.GsuomDesc
                            });
                        }

                        // Check duplicate inventory upload
                        bool exists = await dbContext.Tbl60005inventoryUploads
                            .AnyAsync(x => x.PlanNo == item.PlanNo && x.Gscode == item.GSCode);

                        if (!exists)
                        {
                            slNoCounter++; // Ensure increment per record
                            dbContext.Tbl60005inventoryUploads.Add(new Tbl60005inventoryUpload
                            {
                                SlNo = slNoCounter,
                                Gscode = item.GSCode ?? "",
                                Gsdescription = item.GSDescription,
                                RequestQty = item.RequestQty,
                                UnitPrice = item.UnitPrice,
                                PlanNo = item.PlanNo,
                                Manufacturer = item.Manufacturer,
                                DeliveryPeriod = item.DeliveryPeriod,
                                Remarks = item.Remarks,
                                GsuomDesc = item.GsuomDesc,
                                ItemSize = item.ItemSize,
                                ItemPartNo = item.ItemPartNo,
                                ItemBrand = item.ItemBrand,
                                ItemColor = item.ItemColor,
                                ItemDimension = item.ItemDimension,
                                ItemThickness = item.ItemThickness?.ToString(),
                                GsdescriptionAr = item.GsdescriptionAr
                            });
                        }
                        else
                        {
                            _logger.LogInformation("Skipped duplicate item with GSCode: {GSCode}, PlanNo: {PlanNo}", item.GSCode, item.PlanNo);
                        }
                    }

                    if (newUnitsToInsert.Count > 0)
                    {
                        dbContext.Tbl40111PropertyUnitCodes.AddRange(newUnitsToInsert);
                    }

                    await dbContext.SaveChangesAsync();

                    await dbContext.Database.ExecuteSqlRawAsync(
                        "EXEC sp600_21InventoryUploading @p0, @p1, @p2, @p3, @p4, @p5, @p6",
                        new object[]
                        {
                            code,
                            value1,
                            (byte)value2,
                            "System",
                            DateTime.Now,
                            request.RequestNo,
                            request.GSGroupID
                        }
                    );



                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "UploadStock transaction failed.");
                    throw;
                }
            });

            return Ok(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> SaveMprno([FromBody] PurchaseRequestViewModel request)
        {
            // Validate tenant context
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant context." });
            }

            // Validate MPR number
            if (string.IsNullOrEmpty(request.Mprno))
            {
                return BadRequest(new { success = false, message = "MPR No. is required." });
            }

            // Retrieve MPR master record
            var master = await dbContext.Tbl60601purchaseRequestMasters.FirstOrDefaultAsync(x => x.Mprno == request.Mprno);
            if (master == null)
            {
                dbContext.Tbl60601purchaseRequestMasters.Add(new Tbl60601purchaseRequestMaster
                {
                    Mprno = request.Mprno,
                    Mprdate = request.Mprdate,
                    ClientCode = request.ClientCode,
                    RequestedBy = request.RequestedBy,
                    RequesterContactEmail = request.RequesterContactEmail,
                    RequesterContact = request.RequesterContact,
                    ModeOfRequest = Convert.ToByte(request.ModeOfRequest),
                    TypeOfRequest = Convert.ToByte(request.TypeOfRequest),
                    SalesPersonCode = request.SalesPersonCode,
                    ClientRefNo = request.ClientRefNo,
                    PurposeOfRequest = request.PurposeOfRequest,
                    Priority = request.Priority,
                    CostCenterText = request.CostCenterText,
                    ExpectedDate = request.ExpectedDate,
                    ExpectedVatrate = Convert.ToByte(request.ExpectedVatrate),
                    Remarks = request.Remarks,
                    CompanyBranch = Convert.ToByte(request.CompanyBranch),
                    PurchaseRequestStatusId = Convert.ToByte(request.PurchaseRequestStatusId),
                    InventoryMasterGroupId = Convert.ToByte(request.InventoryMasterGroupId),
                    ProjectMasterCode = request.ProjectMasterCode,
                    BidClosingDate = request.BidClosingDate,
                    BidReminderOn = request.BidReminderOn,
                    ClientProject = request.ClientProject,
                    RequestSignatory = request.RequestSignatory,
                    MprverifiedSign = request.MprverifiedSign,
                    MprapprovedSign = request.MprapprovedSign,
                    ProjectSubUnitCode = Convert.ToByte(request.ProjectSubUnitCode),
                    StoreCode = request.StoreCode,
                    TypeOfMpr = Convert.ToByte(request.TypeOfMpr),
                    CurrencyId = request.CurrencyId ?? 1,
                    CurrencyRate = request.CurrencyRate ?? 1,
                    BaseCurrencyId = request.BaseCurrencyId ?? 1,
                });
            }
            
            await dbContext.SaveChangesAsync();
            
            return Ok(new
            {
                success = true,
                message = "MPR saved successfully.",
            });
        }
        

        [HttpPost]
        public async Task<IActionResult> UploadInvoice([FromBody] UploadStockRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogError("Model validation failed: {@Errors}", errors);
                return BadRequest(new { message = "Invalid payload", errors });
            }

            if (request == null)
            {
                _logger.LogError("UploadStock request is null.");
                return BadRequest(new { success = false, message = "Request cannot be null." });
            }

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized("Invalid tenant.");
            }

            if (request.GSGroupID == 0)
            {
                return BadRequest(new { success = false, message = "GSGroupCode is required." });
            }

            if (request.Items == null || request.Items.Count == 0)
            {
                _logger.LogWarning("UploadStock request contains an empty Items array.");
                return BadRequest(new { success = false, message = "At least one item is required." });
            }

            var code = dbContext.Tbl20165GoodsAndServicesGroups
                    .Where(x => x.GsgroupId == request.GSGroupID)
                    .Select(x => x.GsgroupCode)
                    .FirstOrDefault();
            var value1 = await dbContext.Tbl40111PropertyUnitCodes
            .MaxAsync(u => (int?)u.UnitCode) ?? 0;

            var value2 = await dbContext
                .Tbl20164GoodsAndServicesMasters
                .Where(g => g.Gscode.StartsWith(code + "-"))
                .Select(g => (int?)Convert.ToInt32(g.Gscode.Substring(g.Gscode.Length - 5)))
                .MaxAsync() ?? 0;
            foreach (var item in request.Items)
            {
                if (string.IsNullOrEmpty(item.GSDescription))
                    return BadRequest(new { success = false, message = "Each item must have a GSDescription." });

                if (item.RequestQty <= 0)
                    return BadRequest(new { success = false, message = "Each item must have a positive RequestQty." });

                if (item.UnitPrice < 0)
                    return BadRequest(new { success = false, message = "UnitPrice cannot be negative." });
            }

            var executionStrategy = dbContext.Database.CreateExecutionStrategy();

            await executionStrategy.ExecuteAsync(async () =>
            {
                using var transaction = await dbContext.Database.BeginTransactionAsync();
                try
                {
                    int slNoCounter = await dbContext.Tbl60005inventoryUploads.MaxAsync(x => (int?)x.SlNo) ?? 0;

                    var existingUnitCodes = await dbContext.Tbl40111PropertyUnitCodes
                        .Select(u => u.UnitDesc)
                        .ToListAsync();

                    var newUnitsToInsert = new List<Tbl40111PropertyUnitCode>();

                    foreach (var item in request.Items)
                    {
                        if (!string.IsNullOrWhiteSpace(item.GsuomDesc) &&
                            !existingUnitCodes.Contains(item.GsuomDesc, StringComparer.OrdinalIgnoreCase) &&
                            !newUnitsToInsert.Any(u => string.Equals(u.UnitDesc, item.GsuomDesc, StringComparison.OrdinalIgnoreCase)))
                        {
                            newUnitsToInsert.Add(new Tbl40111PropertyUnitCode
                            {
                                UnitCode = 0,
                                UnitDesc = item.GsuomDesc
                            });
                        }

                        // Check duplicate inventory upload
                        bool exists = await dbContext.Tbl60005inventoryUploads
                            .AnyAsync(x => x.PlanNo == item.PlanNo && x.Gscode == item.GSCode);

                        if (!exists)
                        {
                            slNoCounter++; // Ensure increment per record
                            dbContext.Tbl60005inventoryUploads.Add(new Tbl60005inventoryUpload
                            {
                                SlNo = slNoCounter,
                                Gscode = item.GSCode ?? "",
                                Gsdescription = item.GSDescription,
                                RequestQty = item.RequestQty,
                                UnitPrice = item.UnitPrice,
                                PlanNo = item.PlanNo,
                                Manufacturer = item.Manufacturer,
                                DeliveryPeriod = item.DeliveryPeriod,
                                Remarks = item.Remarks,
                                GsuomDesc = item.GsuomDesc,
                                ItemSize = item.ItemSize,
                                ItemPartNo = item.ItemPartNo,
                                ItemBrand = item.ItemBrand,
                                ItemColor = item.ItemColor,
                                ItemDimension = item.ItemDimension,
                                ItemThickness = item.ItemThickness?.ToString(),
                                GsdescriptionAr = item.GsdescriptionAr
                            });
                        }
                        else
                        {
                            _logger.LogInformation("Skipped duplicate item with GSCode: {GSCode}, PlanNo: {PlanNo}", item.GSCode, item.PlanNo);
                        }
                    }

                    if (newUnitsToInsert.Count > 0)
                    {
                        dbContext.Tbl40111PropertyUnitCodes.AddRange(newUnitsToInsert);
                    }

                    await dbContext.SaveChangesAsync();

                    await dbContext.Database.ExecuteSqlRawAsync(
                        "EXEC sp600_21InventoryUploadingToInvoice @p0, @p1, @p2, @p3, @p4, @p5, @p6",
                        new object[]
                        {
                            code,
                            value1,
                            value2,
                            "System",
                            DateTime.Now,
                            request.RequestNo,
                            request.GSGroupID
                        }
                    );



                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "UploadStock transaction failed.");
                    throw;
                }
            });

            return Ok(new { success = true });
        }
    }
}