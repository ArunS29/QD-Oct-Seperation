using Microsoft.AspNetCore.Mvc;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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

            if (string.IsNullOrEmpty(request.GSGroupCode))
            {
                return BadRequest(new { success = false, message = "GSGroupCode is required." });
            }

            if (request.Items == null || request.Items.Count == 0)
            {
                _logger.LogWarning("UploadStock request contains an empty Items array.");
                return BadRequest(new { success = false, message = "At least one item is required." });
            }

            foreach (var item in request.Items)
            {
                if (string.IsNullOrWhiteSpace(item.GSCode))
                {
                    //item.GSCode = $"AUTO-GS-{Guid.NewGuid().ToString().Substring(0, 8)}";
                    item.GSCode = item.GSCode ?? "";
                }

                if (string.IsNullOrEmpty(item.GSDescription))
                {
                    return BadRequest(new { success = false, message = "Each item must have a GSDescription." });
                }

                if (item.RequestQty <= 0)
                {
                    return BadRequest(new { success = false, message = "Each item must have a positive RequestQty." });
                }

                if (item.UnitPrice < 0)
                {
                    return BadRequest(new { success = false, message = "UnitPrice cannot be negative." });
                }
            }

            var executionStrategy = dbContext.Database.CreateExecutionStrategy();

            await executionStrategy.ExecuteAsync(async () =>
            {
                using var transaction = await dbContext.Database.BeginTransactionAsync();
                try
                {
                    int slNoCounter = 1;

                    foreach (var item in request.Items)
                    {
                        dbContext.Tbl60005inventoryUploads.Add(new Tbl60005inventoryUpload
                        {
                            SlNo = slNoCounter++,
                            Gscode = item.GSCode,
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

                    await dbContext.SaveChangesAsync();

                    await dbContext.Database.ExecuteSqlRawAsync(
                        "EXEC sp600_21InventoryUploading @p0, @p1, @p2, @p3, @p4, @p5, @p6",
                        new object[]
                        {
                            request.GSGroupCode,
                            0,
                            0,
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

        [HttpGet]
        public IActionResult GetGSGroupCode(int gsgroupId)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var code = dbContext.Tbl20165GoodsAndServicesGroups
                    .Where(x => x.GsgroupId == gsgroupId)
                    .Select(x => x.GsgroupCode)
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(code))
                    return NotFound("GSGroupCode not found for the given GSGroupID.");

                return Ok(code);
            }

            return Unauthorized("Tenant context not resolved.");
        }
    }
}