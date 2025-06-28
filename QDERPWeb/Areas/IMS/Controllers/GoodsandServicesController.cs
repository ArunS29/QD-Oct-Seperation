using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.Service;
using QD.ERP.Web.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace QD.ERP.Web.Areas.IMS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GoodsandServicesController : ControllerBase
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
		private readonly ILogger<GoodsandServicesController> _logger;

		public GoodsandServicesController(ILogger<GoodsandServicesController> logger, TenantDbContextHelper tenantDbContextHelper)
		{
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

                        return Ok(new { success = true, message = "Client updated successfully." });
                    }
                    else
                    {
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

                        return Ok(new { success = true, message = "Client saved successfully." });
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
    }
}
