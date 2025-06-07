using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.DAL;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QD.ERP.Web.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;

        public NotificationController(TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
        }

        /// <summary>
        /// ✅ Get paginated notifications for current user (used in DevExtreme grid)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUserNotifications(DataSourceLoadOptions loadOptions)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !byte.TryParse(userIdStr, out byte userId))
                return Unauthorized(new { message = "Invalid session or user ID", success = false });

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out ERPMasterWtDataContext dbContext))
            {
                using (dbContext)
                {
                    var query = dbContext.Qry90101SystemAlertMasters
                        .Where(x => x.AlertUserId == userId)
                        .OrderByDescending(x => x.AlertCreatedOn)
                        .Select(x => new
                        {
                            x.AlertNo,
                            x.AlertUserMessage,
                            x.AlertMasterMessage,
                            x.AlertCreatedOn,
                            x.AlertType,
                            x.AlertNotifiedByUser
                        });

                    var result = await DataSourceLoader.LoadAsync(query, loadOptions);
                    return Ok(result);
                }
            }

            return Unauthorized(new { message = "Invalid tenant", success = false });
        }

        /// <summary>
        /// ✅ Get count of unseen notifications for badge
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUnseenCount()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !byte.TryParse(userIdStr, out byte userId))
                return Unauthorized(new { message = "Invalid session or user ID", success = false });

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out ERPMasterWtDataContext dbContext))
            {
                using (dbContext)
                {
                    var unseenCount = await dbContext.Qry90101SystemAlertMasters
                        .Where(x => x.AlertUserId == userId && x.AlertNotifiedByUser == false)
                        .CountAsync();

                    return Ok(new { unseenCount });
                }
            }

            return Unauthorized(new { message = "Invalid tenant", success = false });
        }

        /// <summary>
        /// ✅ Mark all unseen notifications as read for current user
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> tbl901AlertUsers()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !byte.TryParse(userIdStr, out byte userId))
                return Unauthorized(new { message = "Invalid session or user ID", success = false });

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out ERPMasterWtDataContext dbContext))
            {
                using (dbContext)
                {
                    var unseenAlerts = await dbContext.Tbl901AlertUsers
                        .Where(x => x.AlertUserId == userId && x.AlertNotifiedByUser == false)
                        .ToListAsync();

                    if (unseenAlerts.Any())
                    {
                        unseenAlerts.ForEach(alert => alert.AlertNotifiedByUser = true);
                        await dbContext.SaveChangesAsync();
                    }

                    return Ok(new { success = true, message = "Marked all as read" });
                }
            }

            return Unauthorized(new { message = "Invalid tenant", success = false });
        }

        /// <summary>
        /// ✅ Delete selected notifications by AlertNo
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] List<string> alertNos)
        {
            if (alertNos == null || alertNos.Count == 0)
                return BadRequest(new { success = false, message = "No alerts specified for deletion" });

            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !byte.TryParse(userIdStr, out byte userId))
                return Unauthorized(new { message = "Invalid session or user ID", success = false });

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out ERPMasterWtDataContext dbContext))
            {
                using (dbContext)
                {
                    var alertsToDelete = await dbContext.Tbl901AlertUsers
                        .Where(x => x.AlertUserId == userId && alertNos.Contains(x.AlertCode))
                        .ToListAsync();

                    if (alertsToDelete.Count == 0)
                        return NotFound(new { success = false, message = "No matching notifications found" });

                    dbContext.Tbl901AlertUsers.RemoveRange(alertsToDelete);
                    await dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Notifications deleted" });
                }
            }

            return Unauthorized(new { message = "Invalid tenant", success = false });
        }
    }
}
