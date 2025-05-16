using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

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
                        x.GsgroupCode
                    });

                return Json(DataSourceLoader.Load(query, loadOptions));
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }


        [HttpGet]
        public async Task<IActionResult> GetNextStockNumber(string groupName)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var group = await dbContext.Tbl20165GoodsAndServicesGroups
                    .FirstOrDefaultAsync(x => x.GsgroupName == groupName);

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






    }
}


