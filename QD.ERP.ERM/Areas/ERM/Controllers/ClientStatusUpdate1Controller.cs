using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.Areas.Finance.Controllers;
using QD.ERP.Shared.DAL.Entities;
using QD.ERP.Web.Service;

namespace QD.ERP.Web.Areas.ERM.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ClientStatusUpdate1Controller : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<ClientStatusUpdate1Controller> _logger;

        public ClientStatusUpdate1Controller(ILogger<ClientStatusUpdate1Controller> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetStatus(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var ClientStatus = dbContext.Tbl30103ClientStatusCodes.Select(i => new
                    {
                        i.StatusCode,
                        i.Status,
                    });

                    return Json(await DataSourceLoader.LoadAsync(ClientStatus, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetProject: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet]
        public async Task<IActionResult> GetClient(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var ClientStatus = dbContext.Tbl30103ClientStatusCodes.Select(i => new
                    {
                        i.StatusCode,
                        i.Status,
                    });

                    return Json(await DataSourceLoader.LoadAsync(ClientStatus, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetProject: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        //get data for bind

        [HttpGet]
        public IActionResult GetClientStatusById(long clientStatusNo)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var item = dbContext.Tbl30104ClientStatuses
                        .FirstOrDefault(x => x.ClientStatusNo == clientStatusNo);

                    if (item != null)
                    {
                        return Ok(item);
                    }

                    return NotFound(new { success = false, message = "Client status not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while fetching the data.", ex });
            }
            return Unauthorized(new { success = false, message = "Invalid tenant." });
        }

        [HttpPost]
        public IActionResult SaveOrUpdateClientStatusUpdate([FromBody] Tbl30104ClientStatus item)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    if (item.ClientStatusNo > 0)
                    {
                        // Update logic
                        var existing = dbContext.Tbl30104ClientStatuses
                            .FirstOrDefault(x => x.ClientStatusNo == item.ClientStatusNo);

                        if (existing != null)
                        {
                            existing.ClientCode = item.ClientCode;
                            existing.ReportedBy = item.ReportedBy;
                            existing.ReportedOn = item.ReportedOn;
                            existing.StatusRemarks = item.StatusRemarks;
                            existing.Status = item.Status;
                            existing.FollowupOn = item.FollowupOn;

                            dbContext.SaveChanges();

                            return Ok(new
                            {
                                success = true,
                                message = "Client status updated successfully",
                                lastClientStatusNo = existing.ClientStatusNo
                            });
                        }
                        else
                        {
                            return NotFound(new { success = false, message = "Record not found for update." });
                        }
                    }
                    else
                    {
                        // Insert logic
                        dbContext.Tbl30104ClientStatuses.Add(item);
                        dbContext.SaveChanges();

                        return Ok(new
                        {
                            success = true,
                            message = "Client status saved successfully",
                            lastClientStatusNo = item.ClientStatusNo
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetProject: {ex.Message}");
                    return StatusCode(500, new
                        {
                            success = false,
                            message = "Error saving data: " + ex.Message
                        });
                }
            }

            return Unauthorized(new { success = false, message = "Invalid tenant." });
        }


        [HttpPost]
        public IActionResult DeleteClientStatusUpdate([FromBody] long ClientStatusNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var entity = dbContext.Tbl30104ClientStatuses
                        .FirstOrDefault(x => x.ClientStatusNo == ClientStatusNo);

                    if (entity == null)
                    {
                        return NotFound(new { success = false, message = "Record not found." });
                    }

                    dbContext.Tbl30104ClientStatuses.Remove(entity);
                    dbContext.SaveChanges();

                    return Ok(new { success = true, message = "Deleted successfully." });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetProject: {ex.Message}");
                    return StatusCode(500, new { success = false, message = $"Delete failed: {ex.Message}" });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

    }
}
