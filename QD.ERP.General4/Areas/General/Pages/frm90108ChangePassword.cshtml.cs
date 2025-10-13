using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using QD.ERP.Shared.DAL.Entities;
using QD.ERP.Web.Service;
using System;
using System.Threading.Tasks;

namespace QD.ERP.Web.Areas.General.Pages
{
    public class frm90108ChangePasswordModel : PageModel
    {
        private readonly ILogger<frm90108ChangePasswordModel> _logger;
        private readonly TenantDbContextHelper _tenantDbContextHelper;

        public frm90108ChangePasswordModel(
            ILogger<frm90108ChangePasswordModel> logger,
            TenantDbContextHelper tenantDbContextHelper)
        {
            _logger = logger;
            _tenantDbContextHelper = tenantDbContextHelper;
        }

        // Remove or comment this for prod and send token in AJAX
        // [IgnoreAntiforgeryToken]
        public string CurrentUserName { get; set; }
        public byte CurrentUserId { get; set; }
        public string UserLevel { get; set; } = "N/A"; // Adjust as needed

        public void OnGet()
        {
            string userIdStr = HttpContext.Session.GetString("UserId");
            string userName = HttpContext.Session.GetString("UserName");
            

            if (byte.TryParse(userIdStr, out byte userId))
            {
                CurrentUserId = userId;
            }

            CurrentUserName = userName ?? "Unknown";
        
        }
        public async Task<IActionResult> OnPostResetPasswordAsync(string currentPassword, string newPassword)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized();

                string userIdStr = HttpContext.Session.GetString("UserId");
                if (!byte.TryParse(userIdStr, out byte currentUserId))
                    return new UnauthorizedObjectResult("Invalid or missing UserId in session.");

                var user = await dbContext.TblUserMasters.FirstOrDefaultAsync(u => u.UserId == currentUserId);
                if (user == null)
                    return NotFound("User not found.");

                // Validate current password (plain text, no hashing as you requested)
                if (!string.Equals(user.Password, currentPassword))
                {
                    return BadRequest("Current password is incorrect.");
                }

                // Update password and audit info
                user.Password = newPassword;
                user.ModifiedOn = DateTime.UtcNow;
                user.ModifiedBy = user.UserName;

                await dbContext.SaveChangesAsync();

                return new JsonResult("Password reset successful.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password.");
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
