namespace QD.ERP.Shared.Models
{
    public class NotificationRequest
{
    public string UserId { get; set; }
     public string TenantName { get; set; }
    public string VoucherName { get; set; }
    public string ActionType { get; set; } // Optional: "verify", "approve", etc.
}
}
