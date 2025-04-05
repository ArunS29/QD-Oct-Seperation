using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class JournalRegisterController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<JournalRegisterController> _logger;

        public JournalRegisterController(ILogger<JournalRegisterController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult> GetJournalview(byte RequesterID, DateTime StartDate, DateTime EndDate, bool IfShowAll)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    ERPMasterWtDataContextProcedures procedure = new ERPMasterWtDataContextProcedures(dbContext);
                    var result = await dbContext.JournalRegisterViews
                        .FromSqlRaw("EXEC sp20201JournalRegisterView @p0, @p1, @p2, @p3",
                            RequesterID, StartDate, EndDate, IfShowAll)
                        .ToListAsync();

                    if (result != null && result.Any())
                    {
                        return Json(result); // 200 OK
                    }

                    // Return 400 Bad Request if no data found
                    return BadRequest(new { success = false, message = "No journal entries found for the given filters." });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing sp20201JournalRegisterView for requester {RequesterID}", RequesterID);

                    // Return 500 Internal Server Error
                    return StatusCode(500, new { success = false, message = "An unexpected error occurred while loading journal data." });
                }
            }

            // Return 401 Unauthorized
            return Unauthorized(new { success = false, message = "Tenant context could not be resolved. Access denied." });
        }

        [HttpGet]
        public async Task<ActionResult> GetUser()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var users = await dbContext.TblUserMasters
                        .Select(u => new
                        {
                            u.UserId,
                            u.UserName
                        })
                        .ToListAsync();

                    return Json(users);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetUser: {ex.Message}");
                    return Json(new { error = "Unable to fetch user data at this time." });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public ActionResult UpdateData(sp20201JournalRegisterViewResult updatedItem)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    // Perform the update logic here.
                    // Example: Update the item in the database.

                    return Json(updatedItem);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in UpdateData: {ex.Message}");
                    return StatusCode(500, new { success = false, message = "An error occurred while updating the data." });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<ActionResult> GetJournalChild(string journalRefNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var result = await dbContext.Qry202101journalRegisterChildren
                                    .Where(x => x.JournalRefNo== journalRefNo)
                                    .ToListAsync();

                    if (result != null && result.Any())
                    {
                        return Json(result);
                    }

                    return Json(new { success = false, message = "No child records found." });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetJournalChild: {ex.Message}");
                    return Json(new { success = false, message = "An error occurred while fetching child records." });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

    }
}





