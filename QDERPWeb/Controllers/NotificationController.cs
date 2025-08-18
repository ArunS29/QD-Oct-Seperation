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
                    var query = dbContext.Tbl901AlertUsers
                        .Where(x => x.AlertUserId == userId)
                        .OrderByDescending(x => x.AlertUserOn)
                        .Select(x => new
                        {
                            x.AlertCode,
                            x.AlertUserMessage,
                            x.AlertStatusRemarks,
                            x.AlertUserOn,
                            x.AlertBySystem,
                            x.AlertNotifiedByUser,
                            x.IsSeen
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
                    var unseenCount = await dbContext.Tbl901AlertUsers
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
        [HttpPost]
        public async Task<IActionResult> MarkAsSeen([FromBody] MarkAsSeenRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.AlertCode))
                return BadRequest(new { success = false, message = "Invalid request data" });

            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !byte.TryParse(userIdStr, out byte userId))
                return Unauthorized(new { message = "Invalid session or user ID", success = false });

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out ERPMasterWtDataContext dbContext))
            {
                using (dbContext)
                {
                    var alert = await dbContext.Tbl901AlertUsers
                        .FirstOrDefaultAsync(x => x.AlertUserId == userId && x.AlertCode == request.AlertCode);

                    if (alert == null)
                        return NotFound(new { success = false, message = "Notification not found" });

                    alert.IsSeen = true;
                    await dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Notification marked as seen" });
                }
            }

            return Unauthorized(new { message = "Invalid tenant", success = false });
        }
        [HttpPost]
        public async Task<IActionResult> MarkAllAsSeen()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !byte.TryParse(userIdStr, out byte userId))
                return Unauthorized(new { success = false, message = "Invalid session or user ID" });

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out ERPMasterWtDataContext dbContext))
            {
                using (dbContext)
                {
                    var unseenAlerts = await dbContext.Tbl901AlertUsers
                        .Where(x => x.AlertUserId == userId && !x.IsSeen)
                        .ToListAsync();

                    if (!unseenAlerts.Any())
                    {
                        return Ok(new { success = true, message = "No unseen alerts found." });
                    }

                    unseenAlerts.ForEach(x => x.IsSeen = true);

                    await dbContext.SaveChangesAsync();

                    return Ok(new
                    {
                        success = true,
                        message = $"{unseenAlerts.Count} alert(s) marked as seen."
                    });
                }
            }

            return Unauthorized(new { success = false, message = "Invalid tenant or database context." });
        }
        [HttpGet]
        public IActionResult GetFinanceNotificationSummary()
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { message = "Invalid tenant.", success = false });

            try
            {
                var vouchers = dbContext.Qry20136VoucherMasterLists.AsQueryable();

                // --- Summary counts ---
                Func<List<string>, object> getCounts = (types) =>
                {
                    var filtered = vouchers.Where(v => types.Contains(v.VoucherType));
                    return new
                    {
                        ToBeVerified = filtered.Count(v => !v.IsVerified),
                        ToBeApproved = filtered.Count(v => v.IsVerified && !v.IsApproved),
                        ToBeAudited = filtered.Count(v => v.IsApproved && !v.IsAuditVerified)
                    };
                };

                var summary = new
                {
                    Payments = getCounts(new List<string> { "Bank Payment", "Cash Payment" }),
                    Receipts = getCounts(new List<string> { "Bank Receipts", "Cash Receipts" }),
                    SalesPurchase = getCounts(new List<string> { "Sales", "Purchase" }),
                    Journals = getCounts(new List<string> { "Journal" }),
                    ExpenseClaims = getCounts(new List<string> { "ExpenseClaim" })
                };

                // --- Detailed notifications ---
                var notifications = vouchers.Select(v => new
                {
                    VoucherType = v.VoucherType,
                    VoucherNumber = v.VoucherNo,
                    Amount = v.DebitAmount ?? v.CreditAmount,
                    User = !v.IsVerified ? v.VoucherEnteredBy
                           : v.IsVerified && !v.IsApproved ? v.VoucherVerifiedBy
                           : v.IsApproved && !v.IsAuditVerified ? v.VoucherApprovedBy
                           : v.VoucherEnteredBy,
                    Status = !v.IsVerified ? "waiting for verification"
                           : v.IsVerified && !v.IsApproved ? "awaiting your approval"
                           : v.IsApproved && !v.IsAuditVerified ? "awaiting audit"
                           : "completed",
                    CreatedOn = v.VoucherEnteredOn ?? DateTime.Now
                })
                .OrderByDescending(v => v.CreatedOn)
                .Take(20) // last 20 notifications
                .ToList();

                return Ok(new
                {
                    Summary = summary,
                    Notifications = notifications
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error while fetching finance notifications", error = ex.Message });
            }
        }



    }


    public class MarkAsSeenRequest
    {
        public string AlertCode { get; set; }
    }
   
}


