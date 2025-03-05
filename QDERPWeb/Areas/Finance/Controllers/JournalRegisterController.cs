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
                    var result = await procedure.sp20201JournalRegisterViewAsync(RequesterID, StartDate, EndDate, IfShowAll);

                    if (result != null && result.Any())
                    {
                        return Json(result);
                    }

                    return Json(new { success = false, message = "No data found." });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetJournalview: {ex.Message}");
                    return Json(new { success = false, message = "An error occurred while fetching the data." });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
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
    }
}





