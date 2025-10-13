using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Shared.DAL.Entities;
using QD.ERP.Shared.Models;
using QD.ERP.Shared.Service;
using System.Diagnostics;


public class FcmService
{
    private readonly TenantDbContextHelper _tenantDbContextHelper;

    public FcmService(TenantDbContextHelper tenantDbContextHelper)
    {
        _tenantDbContextHelper = tenantDbContextHelper;
    }

    public async Task<bool> SendNotificationAsync(NotificationRequest model)
    {
        try
        {
            // Debug: print payload
            Debug.WriteLine("🔥 [SendNotificationAsync] Received model:");
            Debug.WriteLine($"UserId: {model?.UserId}, TenantName: {model?.TenantName}, Voucher: {model?.VoucherName}, Action: {model?.ActionType}");

            if (model == null)
                throw new ArgumentNullException(nameof(model), "NotificationRequest is null.");

            if (string.IsNullOrWhiteSpace(model.UserId) || string.IsNullOrWhiteSpace(model.TenantName))
                throw new ArgumentException("UserId or TenantName is missing.");

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                throw new InvalidOperationException("Tenant DB context could not be resolved.");

            // Debug: confirm tenant and DB
            Debug.WriteLine("✅ DB context resolved for tenant: " + model.TenantName);

            var tokens = await dbContext.TblFcmDeviceTokens
                .Where(t => t.UserId == model.UserId && t.TenantName == model.TenantName)
                .Select(t => t.Token)
                .ToListAsync();

            if (tokens == null || !tokens.Any())
            {
                Debug.WriteLine("⚠️ No FCM tokens found for the user and tenant.");
                return false;
            }

            Debug.WriteLine($"📲 Found {tokens.Count} device token(s)");

            var message = new MulticastMessage
            {
                Tokens = tokens,
                Notification = new Notification
                {
                    Title = $"Ref No {model.VoucherName}",
                    Body = $"{model.ActionType}"
                }
            };

            // Send notification
            var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);

            // Debug: log results
            Debug.WriteLine($"📬 Sent to {response.SuccessCount} of {tokens.Count} tokens.");
            if (response.FailureCount > 0)
            {
                for (int i = 0; i < response.Responses.Count; i++)
                {
                    if (!response.Responses[i].IsSuccess)
                    {
                        Debug.WriteLine($"❌ Token[{i}]: {tokens[i]} - {response.Responses[i].Exception.Message}");
                    }
                }
            }

            return response.FailureCount == 0;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"🔥 Exception in SendNotificationAsync: {ex.Message}");
            return false; // or rethrow if needed
        }
    }
}
