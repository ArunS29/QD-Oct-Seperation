using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using DevExpress.Printing.Utils.DocumentStoring;

namespace QD.ERP.Web.Areas.IMS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class StockInventoryController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<StockInventoryController> _logger;

        public StockInventoryController(ILogger<StockInventoryController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
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

        [HttpGet]
        public async Task<IActionResult> GetNextStockNumber(string GsgroupId)
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
            if (model == null)
                return BadRequest(new { message = "Invalid data", success = false });

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
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
        [HttpPut]
        public IActionResult UpdateGoodsService(int key, [FromForm] string values)
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
                   

                    dbContext.Tbl20164GoodsAndServicesMasters.Add(model);
                }
                else
                {
                    // Update existing item properties
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

                    // Optionally update ModifiedBy and ModifiedOn here
                    existingItem.ModifiedBy = User.Identity?.Name ?? "Unknown";
                    existingItem.ModifiedOn = DateTime.Now;
                }

                await dbContext.SaveChangesAsync();

                return Ok(new { message = "Record saved successfully", success = true });
            }
            catch (Exception ex)
            {
                // Log the exception somewhere, then:
                return StatusCode(500, new { message = "Error saving record: " + ex.Message, success = false });
            }
        }

    }
}




    


