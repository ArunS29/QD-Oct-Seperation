using Microsoft.AspNetCore.Mvc;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using DevExpress.Printing.Utils.DocumentStoring;
using DevExpress.Pdf;
using System.IO;
using PdfSharpCore.Pdf.IO;
using PdfSharpCore.Pdf;
using System.Collections.Generic;
using System.IO;

namespace QD.ERP.Web.Areas.Utility.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
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
            public List<StockItemDto> Items { get; set; }
            public string RequestNo { get; set; }
            // Add other properties as needed (e.g., User, Date)
        }

        public class StockItemDto
        {
            public string GSCode { get; set; }
            public string GSDescription { get; set; }
            public string GSUoM { get; set; }
            public decimal? RequestQty { get; set; }
            public decimal? UnitPrice { get; set; }
            public string PlanNo { get; set; }
            public string Manufacturer { get; set; }
            public string DeliveryPeriod { get; set; }
            public string Remarks { get; set; }
            public string GsuomDesc { get; set; }
            public string ItemSize { get; set; }
            public string ItemPartNo { get; set; }
            public string ItemBrand { get; set; }
            public string ItemColor { get; set; }
            public string ItemDimension { get; set; }
            public string ItemThickness { get; set; }
            public string GsdescriptionAr { get; set; }
            // Add other fields as needed
        }

        [HttpPost]
        public async Task<IActionResult> UploadStock([FromBody] UploadStockRequest request)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized("Invalid tenant.");
            }

            if (request == null || string.IsNullOrEmpty(request.GSGroupCode) || request.Items == null || request.Items.Count == 0)
            {
                return BadRequest(new { success = false, message = "Invalid request data." });
            }

            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in request.Items)
                {
                    dbContext.Tbl60005inventoryUploads.Add(new Tbl60005inventoryUpload
                    {
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
                        ItemThickness = item.ItemThickness,
                        GsdescriptionAr = item.GsdescriptionAr
                    });
                }

                await dbContext.SaveChangesAsync();

                // Call stored procedure
                await dbContext.Database.ExecuteSqlRawAsync(
                    "EXEC sp600_21InventoryUploading @p0, @p1",
                    new object[] { request.GSGroupCode, request.RequestNo }
                );

                await transaction.CommitAsync();
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
        [HttpGet("GetGSGroupCode")]
        public IActionResult GetGSGroupCode(int gsgroupId)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var code = dbContext.Tbl20165GoodsAndServicesGroups
                    .Where(x => x.GsgroupId == gsgroupId)
                    .Select(x => x.GsgroupCode)
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(code))
                {
                    return NotFound("GSGroupCode not found for the given GSGroupID.");
                }

                return Ok(code); // returns just the string
            }

            return Unauthorized("Tenant context not resolved.");
        }
    }
}

