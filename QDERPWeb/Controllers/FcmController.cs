using Microsoft.AspNetCore.Mvc;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using QDERPWeb.Models;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.Models;

namespace QDERPWeb.Controllers
{
    [ApiController]
[Route("api/[controller]")]
public class FcmController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly TenantDbContextHelper _tenantDbContextHelper;

    public FcmController(IConfiguration config, TenantDbContextHelper tenantDbContextHelper)
    {
        _config = config;
        _tenantDbContextHelper = tenantDbContextHelper;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterToken([FromBody] FcmTokenRequest model)
    {
        if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            return BadRequest("Tenant context missing.");

        if (string.IsNullOrEmpty(model.DeviceId) || string.IsNullOrEmpty(model.Token))
            return BadRequest("Missing fields.");

        var TenantName =  HttpContext.Session.GetString("TenantName");

        var userId = HttpContext.Session.GetString("UserId");

        var existing = await dbContext.TblFcmDeviceTokens
            .FirstOrDefaultAsync(x => x.UserId == userId  && x.TenantName == TenantName && x.DeviceId == model.DeviceId);

        if (existing != null)
        {
            existing.Token = model.Token;
            existing.Platform = model.Platform ?? "unknown";
            existing.LastUpdated = DateTime.UtcNow;
        }
        else
        {
            dbContext.TblFcmDeviceTokens.Add(new TblFcmDeviceToken
            {
                UserId = userId,
                DeviceId = model.DeviceId,
                Token = model.Token,
                Platform = model.Platform ?? "unknown",
                TenantName = TenantName ?? "unknown",
                LastUpdated = DateTime.UtcNow
            });
        }

        await dbContext.SaveChangesAsync();
        return Ok("FCM token registered successfully.");
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendNotification([FromBody] NotificationRequest model)
    {
        if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            return BadRequest("Tenant context missing.");

        var TenantName =  HttpContext.Session.GetString("TenantName");

        var userId = HttpContext.Session.GetString("UserId");

        var tokens = await dbContext.TblFcmDeviceTokens
            .Where(t => t.TenantName == TenantName)
            .Select(t => t.Token)
            .ToListAsync();

        foreach (var token in tokens)
        {
            var message = new Message()
            {
                Token = token,
                Notification = new Notification()
                {
                    Title = $"Voucher {model.ActionType}",
                    Body = $"{model.VoucherName} has been {model.ActionType}d."
                }
            };

            try
            {
                await FirebaseMessaging.DefaultInstance.SendAsync(message);
            }
            catch (FirebaseMessagingException ex)
            {
                Console.WriteLine($"Failed to send push to {token}: {ex.Message}");
            }
        }

        return Ok($"Push sent to {tokens.Count} devices for tenant '{TenantName}'.");
    }
}
}
