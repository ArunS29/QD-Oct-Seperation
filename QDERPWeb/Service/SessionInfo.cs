namespace QD.ERP.Web.Service
{
    // Stores details about an active user session to support single-login enforcement and user-friendly warnings
    public class SessionInfo
    {
        public string SessionId { get; set; } // JTI
        public string DeviceName { get; set; }
        public string MacAddress { get; set; }
        public string UserAgent { get; set; }
        public string IpAddress { get; set; }
        public DateTime LoginTimeUtc { get; set; } = DateTime.UtcNow;
    }
}
