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
    public class AlertTaskController : ControllerBase
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;

        public AlertTaskController(TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
        }

        // Get all users for dropdown (Username and UserId)
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                return Unauthorized(new { message = "Invalid tenant", success = false });

            var users = await dbContext.TblUserMasters
                .Select(u => new { u.UserId, u.UserName })
                .OrderBy(u => u.UserName)
                .ToListAsync();

            return Ok(users);
        }

        // Get next AlertCode starting with "TSK-"
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

            string nextCode = $"TSK-{(lastNum + 1):D3}";
            return Ok(new { AlertCode = nextCode });
        }

        // Create new notification task using Tbl901AlertUsers entity directly
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] Tbl901AlertUser newAlert)
        {
            if (newAlert == null)
                return BadRequest(new { success = false, message = "Invalid data." });

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                return Unauthorized(new { message = "Invalid tenant", success = false });

            // Set audit fields
            newAlert.AlertUserAddedOn = DateTime.UtcNow;
            newAlert.AlertUserAddedBy = User.Identity?.Name ?? "System";

            dbContext.Tbl901AlertUsers.Add(newAlert);
            await dbContext.SaveChangesAsync();

            return Ok(new { success = true, message = "Notification task created successfully." });
        }
    }
}
