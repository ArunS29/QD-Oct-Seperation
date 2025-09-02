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
            public bool IsExist { get; set; } = false;

        }
        public class UploadItemDto
        {
            public string Code { get; set; }

            [JsonPropertyName("Description")]
            public string Description { get; set; }

            public string ArabicDescription { get; set; }

            [Range(1, int.MaxValue, ErrorMessage = "RequestQty must be greater than 0.")]
            public decimal QtyRequested { get; set; }

            [Range(0, double.MaxValue, ErrorMessage = "UnitPrice cannot be negative.")]
            public decimal UnitPrice { get; set; }

            public string UnitCode { get; set; }
            public string Unit { get; set; }
            public string PlanNo { get; set; }
            public string Manufacturer { get; set; }
            public string DeliveryPeriod { get; set; }
            public string Remarks { get; set; }
            public string Size { get; set; }
            public string PartNo { get; set; }
            public string Brand { get; set; }
            public string Color { get; set; }
            public string Dimension { get; set; }
            public object Thickness { get; set; }
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

            var code = await dbContext.Tbl20165GoodsAndServicesGroups
                .Where(x => x.GsgroupId == request.GSGroupID)
                .Select(x => x.GsgroupCode)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(code))
            {
                return BadRequest(new { success = false, message = "Invalid GSGroupID." });
            }

            var lastUnitCode = await dbContext.Tbl40111PropertyUnitCodes
                .MaxAsync(u => (int?)u.UnitCode) ?? 0;

            var codes = await dbContext.Tbl20164GoodsAndServicesMasters
             .Where(g => g.Gscode.StartsWith(code + "-"))
             .Select(g => g.Gscode)
             .ToListAsync();

            var lastStockGroupNo = codes.Select(g =>
                {
                    var parts = g.Split('-');
                    if (parts.Length > 1 && int.TryParse(parts[^1], out var num))
                        return (int?)num;
                    return null;
                })
                .Max() ?? 0;

            foreach (var item in request.Items)
            {
                if (string.IsNullOrEmpty(item.Description))
                    return BadRequest(new { success = false, message = "Each item must have a GSDescription." });

                if (item.QtyRequested <= 0)
                    return BadRequest(new { success = false, message = "Each item must have a positive RequestQty." });

                if (item.UnitPrice < 0)
                    return BadRequest(new { success = false, message = "UnitPrice cannot be negative." });
            }
               int slNoCounter = await dbContext.Tbl60602purchaseRequestChildren.Where(x => x.Mprno == request.RequestNo).CountAsync();
            if (request.IsExist != true)
            {
                var existingChildren = await dbContext.Tbl60602purchaseRequestChildren
                        .Where(x => x.Mprno == request.RequestNo)
                        .ToListAsync();
                slNoCounter = await dbContext.Tbl60005inventoryUploads.MaxAsync(x => (int?)x.SlNo) ?? 0;


                var toDelete = existingChildren.ToList();

                if (toDelete.Any())
                {
                    dbContext.Tbl60602purchaseRequestChildren.RemoveRange(toDelete);
                }
            }
            var executionStrategy = dbContext.Database.CreateExecutionStrategy();

            await executionStrategy.ExecuteAsync(async () =>
            {
                using var transaction = await dbContext.Database.BeginTransactionAsync();
                try
                {
                    

                    foreach (var item in request.Items)
                    {
                        bool exists = await dbContext.Tbl60005inventoryUploads
                            .AnyAsync(x => x.Gscode == item.Code);

                        if (!exists)
                        {
                            slNoCounter++;
                            dbContext.Tbl60005inventoryUploads.Add(new Tbl60005inventoryUpload
                            {
                                SlNo = slNoCounter,
                                Gscode = item.Code ?? "",
                                Gsdescription = item.Description,
                                RequestQty = item.QtyRequested,
                                UnitPrice = item.UnitPrice,
                                PlanNo = item.PlanNo,
                                Manufacturer = item.Manufacturer,
                                DeliveryPeriod = item.DeliveryPeriod,
                                Remarks = item.Remarks,
                                GsuomDesc = item.Unit,
                                ItemSize = item.Size,
                                ItemPartNo = item.PartNo,
                                ItemBrand = item.Brand,
                                ItemColor = item.Color,
                                ItemDimension = item.Dimension,
                                ItemThickness = item.Thickness?.ToString(),
                                GsdescriptionAr = item.ArabicDescription
                            });
                        }
                        else
                        {
                            _logger.LogInformation("Skipped duplicate item with GSCode: {GSCode}, PlanNo: {PlanNo}", item.Code, item.PlanNo);
                        }
                    }

                    await dbContext.SaveChangesAsync();
                    var userName = HttpContext.Session.GetString("UserName") ?? "System";
                    await dbContext.Database.ExecuteSqlRawAsync(
                        "EXEC sp600_21InventoryUploading @p0, @p1, @p2, @p3, @p4, @p5, @p6",
                        new object[]
                        {
                    code,
                    lastStockGroupNo,
                    lastUnitCode,
                    userName,
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
                _logger.LogWarning("UploadInvoice request contains an empty Items array.");
                return BadRequest(new { success = false, message = "At least one item is required." });
            }

            var code = await dbContext.Tbl20165GoodsAndServicesGroups
                .Where(x => x.GsgroupId == request.GSGroupID)
                .Select(x => x.GsgroupCode)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(code))
            {
                return BadRequest(new { success = false, message = "Invalid GSGroupID." });
            }

            var lastUnitCode = await dbContext.Tbl40111PropertyUnitCodes
                .MaxAsync(u => (int?)u.UnitCode) ?? 0;

            var codes = await dbContext.Tbl20164GoodsAndServicesMasters
             .Where(g => g.Gscode.StartsWith(code + "-"))
             .Select(g => g.Gscode)
             .ToListAsync();

            var lastStockGroupNo = codes.Select(g =>
                {
                    var parts = g.Split('-');
                    if (parts.Length > 1 && int.TryParse(parts[^1], out var num))
                        return (int?)num;
                    return null;
                })
                .Max() ?? 0;

            foreach (var item in request.Items)
            {
                if (string.IsNullOrEmpty(item.Description))
                    return BadRequest(new { success = false, message = "Each item must have a Description." });

                if (item.QtyRequested <= 0)
                    return BadRequest(new { success = false, message = "Each item must have a positive Quantity Requested." });

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

                    foreach (var item in request.Items)
                    {
                        bool exists = await dbContext.Tbl60005inventoryUploads
                            .AnyAsync(x => x.Gscode == item.Code);

                        if (!exists)
                        {
                            slNoCounter++;
                            dbContext.Tbl60005inventoryUploads.Add(new Tbl60005inventoryUpload
                            {
                                SlNo = slNoCounter,
                                Gscode = item.Code ?? "",
                                Gsdescription = item.Description,
                                RequestQty = item.QtyRequested,
                                UnitPrice = item.UnitPrice,
                                PlanNo = item.PlanNo,
                                Manufacturer = item.Manufacturer,
                                DeliveryPeriod = item.DeliveryPeriod,
                                Remarks = item.Remarks,
                                GsuomDesc = item.Unit,
                                ItemSize = item.Size,
                                ItemPartNo = item.PartNo,
                                ItemBrand = item.Brand,
                                ItemColor = item.Color,
                                ItemDimension = item.Dimension,
                                ItemThickness = item.Thickness?.ToString(),
                                GsdescriptionAr = item.ArabicDescription
                            });
                        }
                        else
                        {
                            _logger.LogInformation("Skipped duplicate item with GSCode: {GSCode}, PlanNo: {PlanNo}", item.Code, item.PlanNo);
                        }
                    }

                    await dbContext.SaveChangesAsync();
                    var userName = HttpContext.Session.GetString("UserName") ?? "System";
                    await dbContext.Database.ExecuteSqlRawAsync(
                        "EXEC sp600_21InventoryUploadingToInvoice @p0, @p1, @p2, @p3, @p4, @p5, @p6",
                        new object[]
                        {
                    code,
                    lastStockGroupNo,
                    lastUnitCode,
                    userName,
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
                    _logger.LogError(ex, "UploadInvoice transaction failed.");
                    throw;
                }
            });

            return Ok(new { success = true });
        }
        [HttpPost]
        public async Task<IActionResult> UploadPerforma([FromBody] UploadStockRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogError("Model validation failed: {@Errors}", errors);
                return BadRequest(new { message = "Invalid payload", errors });
            }

            if (request == null)
            {
                _logger.LogError("UploadPerforma request is null.");
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
                _logger.LogWarning("UploadPerforma request contains an empty Items array.");
                return BadRequest(new { success = false, message = "At least one item is required." });
            }

            var code = await dbContext.Tbl20165GoodsAndServicesGroups
                .Where(x => x.GsgroupId == request.GSGroupID)
                .Select(x => x.GsgroupCode)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(code))
            {
                return BadRequest(new { success = false, message = "Invalid GSGroupID." });
            }

            var lastUnitCode = await dbContext.Tbl40111PropertyUnitCodes
                .MaxAsync(u => (int?)u.UnitCode) ?? 0;

            var codes = await dbContext.Tbl20164GoodsAndServicesMasters
             .Where(g => g.Gscode.StartsWith(code + "-"))
             .Select(g => g.Gscode)
             .ToListAsync();

            var lastStockGroupNo = codes.Select(g =>
                {
                    var parts = g.Split('-');
                    if (parts.Length > 1 && int.TryParse(parts[^1], out var num))
                        return (int?)num;
                    return null;
                })
                .Max() ?? 0;

            foreach (var item in request.Items)
            {
                if (string.IsNullOrEmpty(item.Description))
                    return BadRequest(new { success = false, message = "Each item must have a GSDescription." });

                if (item.QtyRequested <= 0)
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

                    foreach (var item in request.Items)
                    {
                        bool exists = await dbContext.Tbl60005inventoryUploads
                            .AnyAsync(x => x.Gscode == item.Code);

                        if (!exists)
                        {
                            slNoCounter++;
                            dbContext.Tbl60005inventoryUploads.Add(new Tbl60005inventoryUpload
                            {
                                SlNo = slNoCounter,
                                Gscode = item.Code ?? "",
                                Gsdescription = item.Description,
                                RequestQty = item.QtyRequested,
                                UnitPrice = item.UnitPrice,
                                PlanNo = item.PlanNo,
                                Manufacturer = item.Manufacturer,
                                DeliveryPeriod = item.DeliveryPeriod,
                                Remarks = item.Remarks,
                                GsuomDesc = item.Unit,
                                ItemSize = item.Size,
                                ItemPartNo = item.PartNo,
                                ItemBrand = item.Brand,
                                ItemColor = item.Color,
                                ItemDimension = item.Dimension,
                                ItemThickness = item.Thickness?.ToString(),
                                GsdescriptionAr = item.ArabicDescription
                            });
                        }
                        else
                        {
                            _logger.LogInformation("Skipped duplicate item with GSCode: {GSCode}, PlanNo: {PlanNo}", item.Code, item.PlanNo);
                        }
                    }

                    await dbContext.SaveChangesAsync();
                    var userName = HttpContext.Session.GetString("UserName") ?? "System";
                    await dbContext.Database.ExecuteSqlRawAsync(
                        "EXEC sp600_21InventoryUploadingToProformaInvoice @p0, @p1, @p2, @p3, @p4, @p5, @p6",
                        new object[]
                        {
                    code,
                    lastStockGroupNo,
                    lastUnitCode,
                    userName,
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
                    _logger.LogError(ex, "UploadPerforma transaction failed.");
                    throw;
                }
            });

            return Ok(new { success = true });
        }
        [HttpPost]
        public async Task<IActionResult> UploadPurchase([FromBody] UploadStockRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogError("Model validation failed: {@Errors}", errors);
                return BadRequest(new { message = "Invalid payload", errors });
            }

            if (request == null)
            {
                _logger.LogError("UploadPurchase request is null.");
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
                _logger.LogWarning("UploadPurchase request contains an empty Items array.");
                return BadRequest(new { success = false, message = "At least one item is required." });
            }

            var code = await dbContext.Tbl20165GoodsAndServicesGroups
                .Where(x => x.GsgroupId == request.GSGroupID)
                .Select(x => x.GsgroupCode)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(code))
            {
                return BadRequest(new { success = false, message = "Invalid GSGroupID." });
            }

            var lastUnitCode = await dbContext.Tbl40111PropertyUnitCodes
                .MaxAsync(u => (int?)u.UnitCode) ?? 0;

            var codes = await dbContext.Tbl20164GoodsAndServicesMasters
             .Where(g => g.Gscode.StartsWith(code + "-"))
             .Select(g => g.Gscode)
             .ToListAsync();

            var lastStockGroupNo = codes.Select(g =>
                {
                    var parts = g.Split('-');
                    if (parts.Length > 1 && int.TryParse(parts[^1], out var num))
                        return (int?)num;
                    return null;
                })
                .Max() ?? 0;

            foreach (var item in request.Items)
            {
                if (string.IsNullOrEmpty(item.Description))
                    return BadRequest(new { success = false, message = "Each item must have a GSDescription." });

                if (item.QtyRequested <= 0)
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

                    foreach (var item in request.Items)
                    {
                        bool exists = await dbContext.Tbl60005inventoryUploads
                            .AnyAsync(x => x.Gscode == item.Code);

                        if (!exists)
                        {
                            slNoCounter++;
                            dbContext.Tbl60005inventoryUploads.Add(new Tbl60005inventoryUpload
                            {
                                SlNo = slNoCounter,
                                Gscode = item.Code ?? "",
                                Gsdescription = item.Description,
                                RequestQty = item.QtyRequested,
                                UnitPrice = item.UnitPrice,
                                PlanNo = item.PlanNo,
                                Manufacturer = item.Manufacturer,
                                DeliveryPeriod = item.DeliveryPeriod,
                                Remarks = item.Remarks,
                                GsuomDesc = item.Unit,
                                ItemSize = item.Size,
                                ItemPartNo = item.PartNo,
                                ItemBrand = item.Brand,
                                ItemColor = item.Color,
                                ItemDimension = item.Dimension,
                                ItemThickness = item.Thickness?.ToString(),
                                GsdescriptionAr = item.ArabicDescription
                            });
                        }
                        else
                        {
                            _logger.LogInformation("Skipped duplicate item with GSCode: {GSCode}, PlanNo: {PlanNo}", item.Code, item.PlanNo);
                        }
                    }

                    await dbContext.SaveChangesAsync();
                    var userName = HttpContext.Session.GetString("UserName") ?? "System";
                    await dbContext.Database.ExecuteSqlRawAsync(
                        "EXEC sp600_21InventoryUploadingToVATPurchaseVoucherNo @p0, @p1, @p2, @p3, @p4, @p5, @p6",
                        new object[]
                        {
                    code,
                    lastStockGroupNo,
                    lastUnitCode,
                    userName,
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
                    _logger.LogError(ex, "UploadPurchase transaction failed.");
                    throw;
                }
            });

            return Ok(new { success = true });
        }
    }
}