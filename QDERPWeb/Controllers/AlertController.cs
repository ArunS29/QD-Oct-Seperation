using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QD.ERP.Web.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AlertController : ControllerBase
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;

        public AlertController(TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
        }

        // Get all users for dropdown (Username and UserId)
        [HttpGet]
        public async Task<IActionResult> GetNextAlertCode()
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                return Unauthorized(new { message = "Invalid tenant", success = false });

            var lastCode = await dbContext.Tbl901AlertUsers
                .Where(a => a.AlertCode.StartsWith("TSK-"))
                .OrderByDescending(a => a.AlertCode)
                .Select(a => a.AlertCode)
                .FirstOrDefaultAsync();

            int lastNum = 0;
            if (!string.IsNullOrEmpty(lastCode))
            {
                var parts = lastCode.Split('-');
                if (parts.Length == 2)
                    int.TryParse(parts[1], out lastNum);
            }

            string nextCode = $"TSK-{(lastNum + 1):D5}";
            return Ok(new { AlertCode = nextCode });
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] Tbl901AlertUser newAlert)
        {
            if (newAlert == null)
                return BadRequest(new { success = false, message = "Invalid data." });

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                return Unauthorized(new { message = "Invalid tenant", success = false });

            var userName = HttpContext.Session.GetString("UserName") ?? "System";

            // Check if AlertCode already exists
            var existingAlert = await dbContext.Tbl901AlertUsers
                .FirstOrDefaultAsync(a => a.AlertCode == newAlert.AlertCode);

            if (existingAlert != null)
            {
                // Update existing
                existingAlert.AlertUserId = newAlert.AlertUserId;
                existingAlert.AlertUserOn = newAlert.AlertUserOn;
                existingAlert.AlertUserTime = newAlert.AlertUserTime;
                existingAlert.AlertByEmail = newAlert.AlertByEmail;
                existingAlert.AlertBySystem = newAlert.AlertBySystem;
                existingAlert.AlertBySms = newAlert.AlertBySms;
                existingAlert.AlertUserMessage = newAlert.AlertUserMessage;
                existingAlert.AlertUserEmail = newAlert.AlertUserEmail;
                existingAlert.AlertUserMobileNo = newAlert.AlertUserMobileNo;
                existingAlert.AlertUserModifiedOn = DateTime.UtcNow;
                existingAlert.AlertUserModifiedBy = userName;

                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Alert updated successfully." });
            }
            else
            {
                // Insert new
                newAlert.AlertUserAddedOn = DateTime.UtcNow;
                newAlert.AlertUserAddedBy = userName;

                dbContext.Tbl901AlertUsers.Add(newAlert);
                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Alert created successfully." });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                return Unauthorized(new { message = "Invalid tenant", success = false });

            var users = await dbContext.TblUserMasters
                .Select(u => new { u.UserId, u.UserName })
                .ToListAsync();

            return Ok(users);
        }
    }
}
