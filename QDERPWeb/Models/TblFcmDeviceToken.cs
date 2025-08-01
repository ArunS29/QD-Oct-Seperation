namespace QD.ERP.Web.Models
{
    public class TblFcmDeviceToken
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string TenantName { get; set; }
        public string DeviceId { get; set; }
        public string Token { get; set; }
        public string Platform { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
