using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Shared.DAL.Entities;
using QD.ERP.Web.Service;
using QD.ERP.Web.Services.Logging;

namespace QD.ERP.Web.Areas.IMS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GoodsandServicesController : ControllerBase
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
		private readonly ILogger<GoodsandServicesController> _logger;
        private readonly IUserActionLogger _userActionLogger;


        public GoodsandServicesController(ILogger<GoodsandServicesController> logger, TenantDbContextHelper tenantDbContextHelper, IUserActionLogger userActionLogger)
		{
            _userActionLogger = userActionLogger;
            _tenantDbContextHelper = tenantDbContextHelper;
			_logger = logger;
		}
        [HttpGet]
        public IActionResult GetGoodsAndServicesGroups()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var groups = dbContext.Tbl20165GoodsAndServicesGroups
                        .Select(g => new
                        {
                            g.GsgroupId,
                            g.GsgroupName,
                            g.GsgroupCode
                        })
                        .ToList();

                    return Ok(groups);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetGoodsAndServicesGroups");
                return StatusCode(500, "Internal Server Error");
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });
            }

        [HttpGet]
        public IActionResult GetLatestClientCode(string categoryCode)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var latestClientCode = dbContext.Tbl20164GoodsAndServicesMasters
                        .Where(c => c.Gscode.StartsWith(categoryCode + "-"))
                        .OrderByDescending(c => c.Gscode)
                        .Select(c => c.Gscode)
                        .FirstOrDefault();

                    return Ok(latestClientCode); // returns e.g., "SW-4"
                }

                return Unauthorized(new { message = "Invalid tenant." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetLatestClientCode");
                return StatusCode(500, "Internal Server Error");
            }
            }







        [HttpGet]
        public async Task<IActionResult> GetUnitofMeasure()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var result = await dbContext.Tbl40111PropertyUnitCodes
                    .Select(g => new
                    {
                        g.UnitType,
                        g.UnitCode
                    })
                    .ToListAsync();

                    return Ok(result);
                }
            }
            catch (Exception ex) { throw ex; }
            return Unauthorized(new { message = "Invalid tenant.", success = false });

        }

        [HttpPost]
        public IActionResult InsertOrUpdate([FromBody] Tbl20164GoodsAndServicesMaster clientMaster)
        {
            if (clientMaster == null)
            {
                return BadRequest("Invalid client data.");
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var existingClient = dbContext.Tbl20164GoodsAndServicesMasters
                        .FirstOrDefault(c => c.Gscode == clientMaster.Gscode);

                    if (existingClient != null)
                    {
                        // Update existing record
                        existingClient.GsgroupId = clientMaster.GsgroupId;
                        existingClient.ItemPartNo = clientMaster.ItemPartNo;
                        existingClient.Gsdescrpition = clientMaster.Gsdescrpition;
                        existingClient.GsuoM = clientMaster.GsuoM;
                        existingClient.GspackingUnit = clientMaster.GspackingUnit;
                        existingClient.GssellingRate = clientMaster.GssellingRate;
                        existingClient.CostPrice = clientMaster.CostPrice;
                        existingClient.GsdetailedDesc = clientMaster.GsdetailedDesc;
                        existingClient.GsdetailedDescAr = clientMaster.GsdetailedDescAr;
                      
                        dbContext.Tbl20164GoodsAndServicesMasters.Update(existingClient);
                        dbContext.SaveChanges();
                        _userActionLogger.LogAsync(module: "IMS > Insert Or Update",
                           actionDetail: $":Inserted {clientMaster.Gscode}",
                           documentNo: $"{clientMaster.Gscode}"
                        );

                        return Ok(new { success = true, message = "Goods And Service Master Information Update Successfully." });
                    }
                    else
                    {
                        bool exists = dbContext.Tbl20164GoodsAndServicesMasters
                     .Any(x => x.Gsdescrpition.ToLower().Trim() == clientMaster.Gsdescrpition.ToLower().Trim());

                        if (exists)
                        {
                            return BadRequest(new
                            {
                                success = false,
                                message = "This Stock/Service Description is already in the database. Please check again."
                            });
                        }
                        // Insert new record
                        var newClient = new Tbl20164GoodsAndServicesMaster
                        {
                            Gscode = clientMaster.Gscode,
                            GsgroupId = clientMaster.GsgroupId,
                            ItemPartNo = clientMaster.ItemPartNo,
                            Gsdescrpition = clientMaster.Gsdescrpition,
                            GsuoM = clientMaster.GsuoM,
                            GspackingUnit = clientMaster.GspackingUnit,
                            GssellingRate = clientMaster.GssellingRate,
                            CostPrice = clientMaster.CostPrice,
                            GsdetailedDesc = clientMaster.GsdetailedDesc,
                            GsdetailedDescAr = clientMaster.GsdetailedDescAr,

                          
                        };

                        dbContext.Tbl20164GoodsAndServicesMasters.Add(newClient);
                        dbContext.SaveChanges();
                        _userActionLogger.LogAsync(module: "IMS > Insert Or Update",
                           actionDetail: $":Inserted {clientMaster.Gscode}",
                           documentNo: $"{clientMaster.Gscode}"
                        );

                        return Ok(new { success = true, message = "Goods And Service Master Information Update Successfully." });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error saving ClientMaster: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public IActionResult DeleteGoodsandService([FromBody] string GoodsCode)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var entity = dbContext.Tbl20164GoodsAndServicesMasters
                        .FirstOrDefault(x => x.Gscode == GoodsCode);

                    if (entity == null)
                    {
                        return NotFound(new { success = false, message = "Record not found." });
                    }

                    dbContext.Tbl20164GoodsAndServicesMasters.Remove(entity);
                    dbContext.SaveChanges();
                    _userActionLogger.LogAsync(module: "IMS > Delete Goods and Service",
                           actionDetail: $"Deleted Goods and Service {GoodsCode}",
                           documentNo: $"{GoodsCode}"
                        );

                    return Ok(new { success = true, message = "Deleted successfully." });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { success = false, message = $"Delete failed: {ex.Message}" });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

    [HttpGet]
public IActionResult GetByGscode(string gscode)
{
    if (string.IsNullOrWhiteSpace(gscode))
        return BadRequest(new { success = false, message = "Gscode is required." });

    if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
    {
       var entity = dbContext.Tbl20164GoodsAndServicesMasters
    .FirstOrDefault(x => x.Gscode.Trim() == gscode.Trim());

        if (entity == null)
            return NotFound(new { success = false, message = "Goods & Service not found." });

        return Ok(new
        {
            entity.Gscode,
            entity.GsgroupId,
            entity.ItemPartNo,
            entity.Gsdescrpition,
            entity.GsdescriptionAr,
            entity.GsuoM,
            entity.GspackingUnit,
            entity.GssellingRate,
            entity.CostPrice,
            entity.GsdetailedDesc,
            entity.GsdetailedDescAr
        });
    }

    return Unauthorized(new { success = false, message = "Invalid tenant." });
}

        //Description Child grid 
        [HttpGet]
        public async Task<IActionResult> GetGoodsAndServiceByCode(string code)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var item = await (from g in dbContext.Tbl20164GoodsAndServicesMasters
                                  join u in dbContext.Tbl40111PropertyUnitCodes
                                      on g.GsgroupId equals u.UnitCode into gj
                                  from unit in gj.DefaultIfEmpty()
                                  where g.Gscode == code
                                  select new
                                  {
                                      GSCode = g.Gscode,
                                      GSDescrpition = g.Gsdescrpition,
                                      GsdescriptionAr = g.GsdescriptionAr,
                                      g.ItemPartNo,
                                      g.CostPrice,
                                      GSSellingRate = g.GssellingRate,
                                      g.ReorderQty,
                                      g.GsuoM,
                                      UnitDescription = unit.UnitDesc
                                  }).FirstOrDefaultAsync();

                if (item == null)
                    return NotFound();

                return Ok(item);
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet]
        public async Task<IActionResult> GetGoodsAndServices(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var query = from g in dbContext.Tbl20164GoodsAndServicesMasters
                                join u in dbContext.Tbl40111PropertyUnitCodes
                                    on g.GsgroupId equals u.UnitCode into gj
                                from unit in gj.DefaultIfEmpty()
                                orderby g.Gscode
                                select new
                                {
                                    GSCode = g.Gscode,
                                    GSDescrpition = g.Gsdescrpition,
                                    g.GsgroupId,
                                    GsdescriptionAr = g.GsdescriptionAr,
                                    g.ItemPartNo,
                                    g.IsDiscontinued,
                                    g.CostPrice,
                                    GSSellingRate = g.GssellingRate,
                                    g.ReorderQty,
                                    g.StoreCode,
                                    g.MaxQty,
                                    g.MinQty,
                                    g.GsuoM,
                                    g.GspackingUnit,
                                    UnitDescription = unit != null ? unit.UnitDesc : null,
                                    UnitCode = unit != null ? unit.UnitCode : (int?)null
                                };

                    var sql = query.ToQueryString();
                    Console.WriteLine(sql);


                    // Get total count for pager
                    var totalCount = await query.CountAsync();

                    // Apply pagination
                    var pagedData = await query
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

                    return Ok(new
                    {
                        data = pagedData,
                        totalCount = totalCount
                    });
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", details = ex.Message });
            }
        }
    }
}
