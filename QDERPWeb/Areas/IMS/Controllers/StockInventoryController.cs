using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
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

namespace QD.ERP.Web.Areas.IMS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class StockInventoryController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<StockInventoryController> _logger;
        private readonly IConfiguration _configuration; // ✅ Add this

        public StockInventoryController(ILogger<StockInventoryController> logger, TenantDbContextHelper tenantDbContextHelper, IConfiguration configuration)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetInventory(DataSourceLoadOptions loadOptions, string filterType = null)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var query = dbContext.Qry60001inventoryStockViews.Select(i => new
                    {
                        i.Gscode,
                        i.Gsdescrpition,
                        i.GsgroupName,
                        i.ItemPartNo,
                        i.ClosingBalance,
                        i.TotalReceived,
                        i.TotalIssues,
                        i.GssellingRate,
                        i.UnitType,
                        i.ReorderLevel,
                        i.ReorderQty,
                        i.CostPrice,
                        i.IsDiscontinued

                    });



                    var result = await DataSourceLoader.LoadAsync(query, loadOptions);
                    return Json(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in Get: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while processing the request.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet]
        public IActionResult GetStockGroups(DataSourceLoadOptions loadOptions)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var query = dbContext.Tbl20165GoodsAndServicesGroups
                        .Select(x => new
                        {
                            x.GsgroupName,
                            x.GsgroupCode,
                            x.GsgroupId
                        });

                    return Json(DataSourceLoader.Load(query, loadOptions));
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                                    _logger.LogError($"Error in GetProject: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });

            }
        }

        [HttpGet]
        public async Task<IActionResult> GetNextStockNumber(string GsgroupId)
        {
            try
            {
                if (!byte.TryParse(GsgroupId, out byte groupIdByte))
                {
                    return BadRequest(new { message = "Invalid Group ID format." });
                }

                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var group = await dbContext.Tbl20165GoodsAndServicesGroups
                        .FirstOrDefaultAsync(x => x.GsgroupId == groupIdByte);

                    if (group == null)
                        return NotFound(new { message = "Group not found." });

                    string groupCode = group.GsgroupCode;

                    var existingCodes = await dbContext.Tbl20164GoodsAndServicesMasters
                        .Where(x => x.Gscode.StartsWith(groupCode + "-"))
                        .Select(x => x.Gscode)
                        .ToListAsync();

                    int maxNumber = 0;
                    foreach (var code in existingCodes)
                    {
                        var parts = code.Split('-');
                        if (parts.Length == 2 && int.TryParse(parts[1], out int number))
                        {
                            if (number > maxNumber)
                                maxNumber = number;
                        }
                    }

                    string nextCode = $"{groupCode}-{(maxNumber + 1):D3}";
                    return Ok(nextCode);
                }

                return Unauthorized(new { message = "Invalid tenant." });
            }
            catch (Exception ex)
            {
                                    _logger.LogError($"Error in GetProject: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });

            }
        }

        [HttpGet]
        public async Task<IActionResult> GoodsServicesData(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var query = dbContext.Tbl20165GoodsAndServicesGroups.AsNoTracking();

                    var result = await DataSourceLoader.LoadAsync(query, loadOptions);
                    return Json(result);
                }
                catch (System.Exception ex)
                {
                    _logger.LogError($"Error in Get: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while loading data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant." });
        }
        public class UpdateIsServicesRequest
        {
            public byte GsgroupId { get; set; }
            public bool IsServicesGroup { get; set; }
        }
        // POST: Update IsServicesGroup boolean flag
        [HttpPost]
        public async Task<IActionResult> UpdateIsServices([FromBody] UpdateIsServicesRequest request)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var entity = await dbContext.Tbl20165GoodsAndServicesGroups.FindAsync(request.GsgroupId);
                    if (entity == null)
                        return NotFound(new { message = "Entity not found." });

                    entity.IsServicesGroup = request.IsServicesGroup;

                    await dbContext.SaveChangesAsync();

                    return Ok(new { success = true });
                }
                catch (System.Exception ex)
                {
                    _logger.LogError($"Error in UpdateIsServices: {ex.Message}");
                    return StatusCode(500, new { message = "Failed to update IsServices flag.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant." });
        }
        [HttpGet("GetInventoryMasterGroups")]
        public IActionResult GetInventoryMasterGroups()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var data = dbContext.Tbl60008inventoryMasterGroups
                        .Select(g => new
                        {
                            g.InventoryMasterGroupId,
                            g.InventoryMasterGroup,
                            DisplayText = g.InventoryMasterGroupId + " | " + g.InventoryMasterGroup
                        })
                        .ToList();

                    return Ok(data);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetInventoryMasterGroups: {ex.Message}");
                    return StatusCode(500, new { message = "Failed to load inventory master groups.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant." });
        }
        [HttpGet("GetInventoryLedgerAccounts")]
        public IActionResult GetInventoryLedgerAccounts()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var data = (from acc in dbContext.Tbl201ChartOfAccounts
                                join grp in dbContext.Tbl201AccountGroups
                                    on acc.AccountGroupId equals grp.AccountGroupId
                                where grp.AccountGroupId == "A014"
                                orderby grp.AccountGroupUnder
                                select new
                                {
                                    acc.AccountId,
                                    IncomeLedger = acc.AccountHead,
                                    grp.AccountGroup,
                                    grp.AccountGroupId,
                                    grp.AccountGroupUnder,
                                    DisplayText = acc.AccountHead
                                }).ToList();

                    return Ok(data);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetInventoryLedgerAccounts: {ex.Message}");
                    return StatusCode(500, new { message = "Failed to load ledger accounts.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant." });
        }
        [HttpGet("GetCostLedgerAccounts")]
        public IActionResult GetCostLedgerAccounts()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var data = (from acc in dbContext.Tbl201ChartOfAccounts
                                join grp in dbContext.Tbl201AccountGroups
                                    on acc.AccountGroupId equals grp.AccountGroupId
                                where grp.AccountGroupUnder == "M5" || grp.AccountGroupUnder == "M7"
                                orderby grp.AccountGroupUnder
                                select new
                                {
                                    acc.AccountId,
                                    IncomeLedger = acc.AccountHead,
                                    grp.AccountGroup,
                                    grp.AccountGroupId,
                                    grp.AccountGroupUnder,
                                    DisplayText = acc.AccountHead
                                }).ToList();

                    return Ok(data);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetCostLedgerAccounts: {ex.Message}");
                    return StatusCode(500, new { message = "Failed to load cost ledger accounts.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant." });
        }
        [HttpPost]
        public IActionResult AddGoodsServiceGroup([FromBody] Tbl20165GoodsAndServicesGroup model)
        {
            try
            {
                if (model == null)
                    return BadRequest(new { message = "Invalid data", success = false });

                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    // Check for duplicate GsgroupCode before adding
                    bool exists = dbContext.Tbl20165GoodsAndServicesGroups
                        .Any(x => x.GsgroupCode == model.GsgroupCode);

                    if (exists)
                    {
                        return BadRequest(new { message = $"GsgroupCode '{model.GsgroupCode}' already exists.", success = false });
                    }

                    byte maxId = dbContext.Tbl20165GoodsAndServicesGroups
                            .Select(x => x.GsgroupId)
                            .AsEnumerable()                // Bring data to memory first
                            .DefaultIfEmpty((byte)0)
                            .Max();

                    // Check to avoid exceeding byte.MaxValue (255)
                    if (maxId == byte.MaxValue)
                        return BadRequest(new { message = "Maximum group ID limit reached.", success = false });

                    model.GsgroupId = (byte)(maxId + 1);

                    dbContext.Tbl20165GoodsAndServicesGroups.Add(model);
                    dbContext.SaveChanges();

                    return Ok(new { message = "Saved successfully", success = true });
                }

                return Unauthorized(new { message = "Invalid tenant", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in AddGoodsServiceGroup: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while saving the data.", error = ex.Message });
            }
        }

        [HttpPut]
        public IActionResult UpdateGoodsService([FromForm] int key, [FromForm] string values)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var item = dbContext.Tbl20165GoodsAndServicesGroups.FirstOrDefault(g => g.GsgroupId == key);
                    if (item == null)
                        return StatusCode(409, "Item not found");

                    JsonConvert.PopulateObject(values, item);
                    dbContext.SaveChanges();

                    return Ok(item);
                }

                return Unauthorized(new { message = "Invalid tenant", success = false });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Update failed: {ex.Message}");
            }
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
        [HttpGet("GetItemClassifications")]
        public IActionResult GetItemClassifications()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var classifications = dbContext.Tbl30111StockClassificationMasters
                        .Select(x => new
                        {
                            x.StockClassId,
                            x.StockClassification
                        })
                        .ToList();

                    return Ok(classifications);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error loading item classifications: {ex.Message}");
                    return StatusCode(500, new { message = "Failed to load item classifications.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant." });
        }
        [HttpGet("GetAllStores")]
        public IActionResult GetAllStores()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var stores = dbContext.Tbl60001storeMasters
                        .Select(store => new
                        {
                            store.StoreId,
                            store.StoreName,
                            CostAllocationUnitId = store.CostAllocationUnitId // This maps to "Cost Center for Consumption"
                        })
                        .ToList();

                    return Ok(stores);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error loading stores: {ex.Message}");
                    return StatusCode(500, new { message = "Failed to load store data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant." });
        }
        [HttpGet("GetCostCenters")]
        public IActionResult GetCostCenters()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var costCenters = dbContext.Tbl201CostAllocationUnits
                        .Select(c => new
                        {
                            c.CostAllocationUnitId,
                            c.CostAllocationUnit
                        })
                        .ToList();

                    return Ok(costCenters);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error loading cost centers: {ex.Message}");
                    return StatusCode(500, new { message = "Failed to load cost center data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant." });
        }

        public class StoreMasterInputModel
        {
            public string StoreId { get; set; }
            public string StoreName { get; set; }
            public string CostAllocationUnitId { get; set; }
        }
        [HttpPost]
        public IActionResult AddStore([FromBody] StoreMasterInputModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.StoreId) || string.IsNullOrWhiteSpace(model.StoreName) || string.IsNullOrWhiteSpace(model.CostAllocationUnitId))
                {
                    return BadRequest(new { message = "All fields are required." });
                }

                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    return Unauthorized(new { message = "Invalid tenant." });
                }

                var newStore = new Tbl60001storeMaster
                {
                    StoreId = model.StoreId,
                    StoreName = model.StoreName,
                    CostAllocationUnitId = model.CostAllocationUnitId,
                    LedgerNo = null
                };

                dbContext.Tbl60001storeMasters.Add(newStore);
                dbContext.SaveChanges();

                return Ok(new { message = "Store saved successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddStore failed");
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later.", detailed = ex.Message });
            }
        }
        public class StoreUpdateDto
        {
            public string Key { get; set; }
            public string Values { get; set; } // Will be a JSON string
        }

        [HttpPut]
        public IActionResult UpdateStore([FromForm] StoreUpdateDto updateDto)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized(new { message = "Invalid tenant." });

                var key = updateDto.Key;
                var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(updateDto.Values);

                var existingStore = dbContext.Tbl60001storeMasters.FirstOrDefault(s => s.StoreId == key);
                if (existingStore == null)
                    return NotFound(new { message = "Store not found." });

                var jsonValues = JsonConvert.SerializeObject(values);
                JsonConvert.PopulateObject(jsonValues, existingStore);

                dbContext.SaveChanges();
                return Ok(new { message = "Store updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateStore failed");
                return StatusCode(500, new { message = "An unexpected error occurred.", detailed = ex.Message });
            }
        }



        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var stockClassifications = dbContext.Tbl30111StockClassificationMasters
                        .Select(s => new
                        {
                            s.StockClassId,
                            s.StockClassification
                        })
                        .ToList();

                    return Ok(stockClassifications);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error loading stock classifications: {ex.Message}");
                    return StatusCode(500, new { message = "Failed to load stock classifications.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant." });
        }
        public class StockClassificationInputModel
        {
            public string StockClassification { get; set; }
        }
        [HttpPost]
        public IActionResult AddStockClassification([FromBody] StockClassificationInputModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.StockClassification))
                    return BadRequest(new { message = "Stock Classification is required." });

                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized(new { message = "Invalid tenant." });

                // Get the max existing ID (assuming StockClassificationId is a byte, short, or int)
                short maxId = (short)dbContext.Tbl30111StockClassificationMasters
                         .Select(x => x.StockClassId)
                         .AsEnumerable()
                         .DefaultIfEmpty((short)0)
                         .Max();

                // Check if maxId has reached its limit
                if (maxId == short.MaxValue)
                    return BadRequest(new { message = "Maximum stock classification ID limit reached." });

                var newStockClass = new Tbl30111StockClassificationMaster
                {
                    StockClassId = (short)(maxId + 1),  // Assign the new ID
                    StockClassification = model.StockClassification
                };

                dbContext.Tbl30111StockClassificationMasters.Add(newStockClass);
                dbContext.SaveChanges();

                return Ok(new { message = "Stock Classification saved successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddStockClassification failed");
                return StatusCode(500, new { message = "An unexpected error occurred.", detailed = ex.Message });
            }
        }

      


        public class StockClassificationUpdateDto
        {
            public int Key { get; set; }
            public string Values { get; set; }  // JSON string of updated fields
        }

        [HttpPut("UpdateStockClassification")]
        public IActionResult UpdateStockClassification([FromForm] StockClassificationUpdateDto updateDto)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized(new { message = "Invalid tenant." });

                var existing = dbContext.Tbl30111StockClassificationMasters.FirstOrDefault(x => x.StockClassId == updateDto.Key);
                if (existing == null)
                    return NotFound(new { message = "Record not found." });

                // Deserialize the JSON string of values into dictionary
                var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(updateDto.Values);

                // Serialize back to JSON and populate existing entity only on provided fields
                var jsonValues = JsonConvert.SerializeObject(values);
                JsonConvert.PopulateObject(jsonValues, existing);

                dbContext.SaveChanges();

                return Ok(new { message = "Stock Classification updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateStockClassification failed");
                return StatusCode(500, new { message = "An error occurred while updating.", detailed = ex.Message });
            }
        }
         [HttpDelete]
        public IActionResult DeleteStockClassification(string key)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized(new { message = "Invalid tenant." });

                if (!short.TryParse(key, out short id))
                    return BadRequest(new { message = "Invalid Stock Classification ID." });

                var stockClass = dbContext.Tbl30111StockClassificationMasters.FirstOrDefault(x => x.StockClassId == id);
                if (stockClass == null)
                    return NotFound(new { message = "Stock Classification not found." });

                dbContext.Tbl30111StockClassificationMasters.Remove(stockClass);
                dbContext.SaveChanges();

                return Ok(new { message = "Deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error.", error = ex.Message });
            }
        }
        [HttpGet]
        public IActionResult GetUnitCodes()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var unitCodes = dbContext.Tbl40111PropertyUnitCodes
                        .Select(u => new
                        {
                            u.UnitType,
                            u.UnitCode
                        })
                        .ToList();

                    return Ok(unitCodes);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error loading unit codes: {ex.Message}");
                    return StatusCode(500, new { message = "Failed to load unit codes.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant." });
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] Tbl20164GoodsAndServicesMaster model)
        {
            if (model == null || !ModelState.IsValid)
                return BadRequest(new { message = "Invalid data", success = false });

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { message = "Invalid tenant", success = false });
            }

            try
            {
                var existingItem = await dbContext.Tbl20164GoodsAndServicesMasters
                    .FirstOrDefaultAsync(x => x.Gscode == model.Gscode);

                if (existingItem == null)
                {
                    // Add new item in main table
                    dbContext.Tbl20164GoodsAndServicesMasters.Add(model);

                    // Insert opening balance using SP
                    await dbContext.Database.ExecuteSqlRawAsync(
                        "EXEC sp600_18InsertStockOpeningBalance @StockNo = {0}, @QtyReceived = {1}, @UnitPrice = {2}, @UnitRateMethod = {3}",
                        model.Gscode,
                        model.OpeningBalance,
                        model.CostPrice,
                        model.CostPrice  // or some other value if you want a different method
                    );
                }
                else
                {
                    // Update fields
                    existingItem.Gsdescrpition = model.Gsdescrpition;
                    existingItem.GsdescriptionAr = model.GsdescriptionAr;
                    existingItem.GsgroupId = model.GsgroupId;
                    existingItem.StoreId = model.StoreId;
                    existingItem.ItemClassificationId = model.ItemClassificationId;
                    existingItem.GsdetailedDesc = model.GsdetailedDesc;
                    existingItem.GsdetailedDescAr = model.GsdetailedDescAr;
                    existingItem.GsuoM = model.GsuoM;
                    existingItem.GspackingUnit = model.GspackingUnit;
                    existingItem.ItemSize = model.ItemSize;
                    existingItem.ItemThickness = model.ItemThickness;
                    existingItem.ItemDimension = model.ItemDimension;
                    existingItem.ActualSize = model.ActualSize;
                    existingItem.OpeningBalance = model.OpeningBalance;
                    existingItem.CostPrice = model.CostPrice;
                    existingItem.ExFactoryCostPrice = model.ExFactoryCostPrice;
                    existingItem.GssellingRate = model.GssellingRate;
                    existingItem.MinSellingPrice = model.MinSellingPrice;
                    existingItem.MaxSellingPrice = model.MaxSellingPrice;
                    existingItem.ItemPartNo = model.ItemPartNo;
                    existingItem.Hscode = model.Hscode;
                    existingItem.StoreCode = model.StoreCode;
                    existingItem.ItemColor = model.ItemColor;
                    existingItem.ItemBrand = model.ItemBrand;
                    existingItem.ItemMake = model.ItemMake;
                    existingItem.CountryOfOrigin = model.CountryOfOrigin;
                    existingItem.MaxQty = model.MaxQty;
                    existingItem.MinQty = model.MinQty;
                    existingItem.ReorderQty = model.ReorderQty;
                    existingItem.ReorderLevel = model.ReorderLevel;
                    existingItem.ReorderLeadTime = model.ReorderLeadTime;
                    existingItem.ModifiedBy = User.Identity?.Name ?? "Unknown";
                    existingItem.ModifiedOn = DateTime.Now;
                    existingItem.ItemImage = model.ItemImage;

                    // Update opening balance using SP
                    await dbContext.Database.ExecuteSqlRawAsync(
                        "EXEC sp600_17UpdateStockOpeningBalance @StockNo = {0}, @QtyReceived = {1}, @UnitPrice = {2}",
                        model.Gscode,
                        model.OpeningBalance,
                        model.CostPrice
                    );
                }

                await dbContext.SaveChangesAsync();

                return Ok(new { message = "Record saved successfully", success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error saving record: " + ex.Message, success = false });
            }
        }


        [HttpPost]
        public async Task<IActionResult> DeleteStockItem([FromBody] string code)
        {
            if (string.IsNullOrEmpty(code))
                return BadRequest(new { message = "Invalid stock code", success = false });

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { message = "Unauthorized access or invalid tenant", success = false });
            }

            try
            {
                var stockItem = await dbContext.Tbl20164GoodsAndServicesMasters
                    .FirstOrDefaultAsync(x => x.Gscode == code);

                if (stockItem == null)
                {
                    return NotFound(new { message = "Stock item not found", success = false });
                }

                // Optional: load from Tbl20164GoodsAndServicesMaster if deletion should occur from that table
                var itemToDelete = await dbContext.Tbl20164GoodsAndServicesMasters
                    .FirstOrDefaultAsync(x => x.Gscode == code);

                if (itemToDelete == null)
                {
                    return NotFound(new { message = "Stock item master record not found", success = false });
                }

                dbContext.Tbl20164GoodsAndServicesMasters.Remove(itemToDelete);
                await dbContext.SaveChangesAsync();

                return Ok(new { message = "Stock item deleted successfully", success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the stock item: " + ex.Message, success = false });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetByCode(string code)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                    return BadRequest(new { message = "Code is required", success = false });

                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    return Unauthorized(new { message = "Invalid tenant", success = false });
                }

                var item = await dbContext.Tbl20164GoodsAndServicesMasters
                    .FirstOrDefaultAsync(x => x.Gscode == code);

                if (item == null)
                    return NotFound(new { message = "Item not found", success = false });

                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });

            }
        }
        [HttpGet]
        public async Task<IActionResult> GetStockCardData(string gscode)
        {
            if (string.IsNullOrEmpty(gscode))
                return BadRequest("GsCode is required.");

            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    return Unauthorized("Invalid tenant.");
                }


                var stockCardData = await dbContext.Qry65111stockCardForStockMasterEdits
                    .Where(x => x.Gscode == gscode)
                    .ToListAsync();


                return Ok(stockCardData);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, "An error occurred: " + ex.Message);
            }
        }
        public async Task<IActionResult> GetInvoiceGridData(string gscode)
        {
            if (string.IsNullOrEmpty(gscode))
                return BadRequest("GsCode is required.");

            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    return Unauthorized("Invalid tenant.");
                }

                var invoiceData = await dbContext.Qry60003stockInvoiceDetails
                    .Where(x => x.ItemCode == gscode)   // Assuming you want to filter by ItemCode or GsCode? Adjust accordingly
                    .ToListAsync();

                return Ok(invoiceData);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, "An error occurred: " + ex.Message);
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetQuotationGridData(string gscode)
        {
            if (string.IsNullOrEmpty(gscode))
                return BadRequest("GsCode is required.");

            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    return Unauthorized("Invalid tenant.");
                }

                var data = await dbContext.Qry60106quotationDetails
                    .Where(q => q.Gscode == gscode)
                    .ToListAsync();

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, "An error occurred: " + ex.Message);
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetPurchaseOrderGridData(string gscode)
        {
            if (string.IsNullOrEmpty(gscode))
                return BadRequest("GsCode is required.");

            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    return Unauthorized("Invalid tenant.");
                }

                var data = await dbContext.Qry60406purchaseOrderDetails
                    .Where(p => p.Gscode == gscode) // Change this field name if needed
                    .ToListAsync();

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, "An error occurred: " + ex.Message);
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetItemDeliversGridData(string gscode)
        {
            if (string.IsNullOrEmpty(gscode))
                return BadRequest("GsCode is required.");

            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    return Unauthorized("Invalid tenant.");
                }

                var data = await dbContext.Qry60306deliveryNoteDetails
                    .Where(d => d.Gscode == gscode) // Adjust 'Gscode' property name if necessary
                    .ToListAsync();

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, "An error occurred: " + ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetItemReceivedGridData(string gscode)
        {
            if (string.IsNullOrEmpty(gscode))
                return BadRequest("GsCode is required.");

            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized("Invalid tenant.");

                var data = await dbContext.Qry60506materailReceiptDetails
                    .Where(x => x.Gscode == gscode)
                    .ToListAsync();

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUnitConversionGridData(string gscode)
        {
            if (string.IsNullOrWhiteSpace(gscode))
                return BadRequest("GsCode is required.");

            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized("Invalid tenant context.");

                var data = await dbContext.Tbl60003unitConversionMasters
                    .Where(x => x.Gscode == gscode)
                    .ToListAsync();

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetUnitCodeOptions()
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized("Invalid tenant context.");

                var unitCodes = await dbContext.Tbl40111PropertyUnitCodes
                    .Select(u => new
                    {
                        u.UnitCode,
                        u.UnitType,
                        u.UnitDesc
                    }).ToListAsync();

                return Ok(unitCodes);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetPurchaseGridData(string gscode)
        {
            if (string.IsNullOrWhiteSpace(gscode))
                return BadRequest("GsCode is required.");

            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized("Invalid tenant context.");

                var data = await dbContext.Qry60406purchaseOrderDetails
                    .Where(x => x.Gscode == gscode)
                    .ToListAsync();

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetDocumentGridData(string gscode)
        {
            if (string.IsNullOrWhiteSpace(gscode))
                return BadRequest("GsCode is required.");

            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized("Invalid tenant context.");

                var data = await dbContext.Tbl60007inventoryStockDocuments
                    .Where(x => x.Gscode == gscode)
                    .ToListAsync();

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [HttpPost]
        public IActionResult MergeSelectedDocuments([FromBody] List<string> filePaths)
        {
            try
            {
                var outputDocument = new PdfSharpCore.Pdf.PdfDocument();

                foreach (var filePath in filePaths)
                {
                    if (!System.IO.File.Exists(filePath)) continue;

                    using var inputDocument = PdfSharpCore.Pdf.IO.PdfReader.Open(filePath, PdfSharpCore.Pdf.IO.PdfDocumentOpenMode.Import);
                    for (int idx = 0; idx < inputDocument.PageCount; idx++)
                    {
                        var page = inputDocument.Pages[idx];
                        outputDocument.AddPage(page);
                    }
                }

                using var stream = new MemoryStream();
                outputDocument.Save(stream, false);
                stream.Position = 0;

                return File(stream.ToArray(), "application/pdf", "MergedDocument.pdf");
            }
            catch (Exception ex)
            {
                  _logger.LogError($"Error in GetProject: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
            }
        }



        [HttpPost]
        public async Task<IActionResult> DownloadMultipleDocuments([FromBody] List<string> filePaths)
        {
            try
            {
                using var zipStream = new MemoryStream();
                using var archive = new System.IO.Compression.ZipArchive(zipStream, System.IO.Compression.ZipArchiveMode.Create, true);

                foreach (var fullPath in filePaths)
                {
                    if (!System.IO.File.Exists(fullPath)) continue;

                    var fileName = Path.GetFileName(fullPath);
                    var entry = archive.CreateEntry(fileName, System.IO.Compression.CompressionLevel.Fastest);

                    using var entryStream = entry.Open();
                    using var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
                    await fileStream.CopyToAsync(entryStream);
                }

                zipStream.Position = 0;
                return File(zipStream.ToArray(), "application/zip", "Documents.zip");
            }
            catch (Exception ex)
            {
                  _logger.LogError($"Error in GetProject: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return BadRequest(new { success = false, message = "Invalid stock code." });

            
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Unauthorized access or invalid tenant." });
            }

            try
            {
                var item = await dbContext.Tbl20164GoodsAndServicesMasters
                    .FirstOrDefaultAsync(x => x.Gscode == code);

                if (item == null)
                    return NotFound(new { success = false, message = "Stock item not found." });

                dbContext.Tbl20164GoodsAndServicesMasters.Remove(item);
                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Stock item deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while deleting the stock item.",
                    details = ex.Message
                });
            }

        }

        [HttpGet]
        public async Task<IActionResult> GetDocumentTypes()
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized("Invalid tenant context.");

                var docTypes = await dbContext.Tbl101DocumentTypes
                    .Select(d => new { id = d.DocumentTypeId, text = d.DocumentType })
                    .ToListAsync();

                return Ok(docTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<IActionResult> SaveDocument([FromBody] Tbl70003projectDocument model)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized("Invalid tenant context.");

                if (string.IsNullOrEmpty(model.DocumentNo))
                {
                    
                    dbContext.Tbl70003projectDocuments.Add(model);
                }
                else
                {
                    var existing = await dbContext.Tbl70003projectDocuments
                        .FirstOrDefaultAsync(d => d.DocumentNo == model.DocumentNo);

                    if (existing == null)
                    {
                        dbContext.Tbl70003projectDocuments.Add(model);
                    }
                    else
                    {
                        existing.DocumentType = model.DocumentType;
                        existing.DocumentRefNo = model.DocumentRefNo;
                        existing.DocumentRemarks = model.DocumentRemarks;
                        existing.DocumentExpDate = model.DocumentExpDate;
                        existing.DocumentExpDateAr = model.DocumentExpDateAr;
                        existing.DocumentNotificationDate = model.DocumentNotificationDate;
                        existing.DocumentStatus = model.DocumentStatus;
                        existing.DocumentStatusRemarks = model.DocumentStatusRemarks;
                    }
                }

                await dbContext.SaveChangesAsync();
                return Json(new { success = true, documentNo = model.DocumentNo });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [HttpGet]
        public async Task<IActionResult> GenerateNewDocumentNo()
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized("Invalid tenant context.");

                var lastDoc = await dbContext.Tbl70003projectDocuments
                    .OrderByDescending(d => d.DocumentNo)
                    .FirstOrDefaultAsync();

                if (lastDoc == null || string.IsNullOrEmpty(lastDoc.DocumentNo))
                    return Ok("1"); 

                
                if (int.TryParse(lastDoc.DocumentNo, out int lastNumber))
                    return Ok((lastNumber + 1).ToString());

                return Ok(""); 
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, $"Error generating document number: {ex.Message}");
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetDocuments()
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized("Invalid tenant context.");

                var documents = await dbContext.Tbl70003projectDocuments
                    .Select(d => new
                    {
                        DocumentNo = d.DocumentNo,
                        DocumentType = d.DocumentType,
                        DocumentRefNo = d.DocumentRefNo,
                        DocumentRemarks = d.DocumentRemarks,
                        DocumentExpDate = d.DocumentExpDate,
                        DocumentExpDateAr = d.DocumentExpDateAr,
                        DocumentNotificationDate = d.DocumentNotificationDate,
                        ProjectId = d.ProjectId
                    })
                    .ToListAsync();

                return Json(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<IActionResult> SaveProjectDocument()
        {
            try
            {
                // Step 1: Get tenant info
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                    return Unauthorized("Invalid tenant context.");

                var tenantName = HttpContext.Session.GetString("TenantName")?.Trim();
                if (string.IsNullOrWhiteSpace(tenantName))
                    return Unauthorized("Tenant name not found in session.");

                // Step 2: Get form data and file
                var form = await Request.ReadFormAsync();
                var file = form.Files.FirstOrDefault();
                if (file == null || file.Length == 0)
                    return BadRequest("No file uploaded.");

                // Step 3: Get module and form name (fallback to URL path if route data is null)
                var routeValues = HttpContext.Request.RouteValues;
                var module = routeValues["area"]?.ToString();
                var formName = routeValues["page"]?.ToString();

                if (string.IsNullOrWhiteSpace(module) || string.IsNullOrWhiteSpace(formName))
                {
                    var segments = HttpContext.Request.Path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries);
                    module ??= segments.Length > 1 ? segments[1] : "UnknownModule";
                    formName ??= segments.LastOrDefault() ?? "UnknownForm";
                }

                // Normalize path elements
                module = module.Replace(" ", "_");
                formName = formName.Replace(" ", "_");
                tenantName = tenantName.Replace(" ", "_");

                // Step 4: Build blob file path
                var fileName = $"{form["DocumentNo"]}_{Path.GetFileName(file.FileName)}";
                var filePathInBlob = $"Inventory/{module}/{fileName}";

                // Step 5: Upload to Azure Blob
                var connectionString = _configuration.GetConnectionString("AzureBlobStorage");
                var containerName = "client-files";
                var blobHelper = new AzureBlobHelper(connectionString, containerName);
                var blobPath = await blobHelper.UploadFileAsync(file, filePathInBlob, tenantName);

                // Step 6: Save metadata
                var model = new Tbl70003projectDocument
                {
                    DocumentNo = form["DocumentNo"],
                    ProjectId = form["ProjectId"],
                    DocumentType = short.TryParse(form["DocumentType"], out var docType) ? docType : null,
                    DocumentRefNo = form["DocumentRefNo"],
                    DocumentRemarks = form["DocumentRemarks"],
                    DocumentExpDate = DateTime.TryParse(form["DocumentExpDate"], out var expDate) ? expDate : null,
                    DocumentNotificationDate = DateTime.TryParse(form["DocumentNotificationDate"], out var notiDate) ? notiDate : null,
                    DocumentExpDateAr = form["DocumentExpDateAr"],
                    DocumentStatus = 1,
                    DocumentStatusRemarks = "Active",
                    AzurePath = blobPath,
                    DocumentFile = null
                };

                dbContext.Tbl70003projectDocuments.Add(model);
                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Document saved and uploaded successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in SaveProjectDocument: {ex.Message}");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }





        [HttpGet]
        public async Task<IActionResult> GetOpeningBalanceByGscode(string gscode)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized("Invalid tenant context.");

                if (string.IsNullOrEmpty(gscode))
                    return BadRequest("Gscode is required.");

                var data = await dbContext.Tbl60502materialReceiptChildren
                    .Where(x => x.Gscode == gscode)
                    .Select(x => new
                    {
                        StockCode = x.Gscode,
                        Unit = x.UnitRateMethod, 
                        UnitPrice = x.UnitPrice,
                        Quantity = x.QtyReceived,
                        ExpiryDate = x.ExpiryDate,
                        BatchNo = x.BatchNo
                    })
                    .ToListAsync();

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetOpeningBalanceByGscode: {ex.Message}");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet]
        public IActionResult GetAllDocuments()
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                return StatusCode(500, "Tenant not found.");

            var connectionString = _configuration.GetConnectionString("AzureBlobStorage");
            var containerName = "client-files"; // or from config if preferred

            if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(containerName))
                return StatusCode(500, "Azure Blob configuration is missing.");

            var blobHelper = new AzureBlobHelper(connectionString, containerName);

            var documents = dbContext.Tbl70003projectDocuments
                .Select(d => new
                {
                    d.DocumentNo,
                    d.DocumentType,
                    d.DocumentRefNo,
                    d.DocumentRemarks,
                    d.AzurePath // ✅ Already includes tenant folder
                })
                .ToList();

            var result = documents
                .Where(doc => !string.IsNullOrWhiteSpace(doc.AzurePath))
                .Select(doc => new
                {
                    doc.DocumentNo,
                    doc.DocumentType,
                    doc.DocumentRefNo,
                    doc.DocumentRemarks,
                    FileUrl = blobHelper.GetBlobSasUrl(doc.AzurePath) // ✅ No extraction needed
                });

            return Ok(result);
        }




    }
}
