namespace QD.ERP.Web.Service
{
    public class Tenant
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ConnectionString { get; set; }

        public string LogoUrl { get; set; }
        public string schemaname { get; set; }
        public string CompanyNameShort { get;  set; }
        public string DefaultcompanyID { get; set; }
        public string DefaultcompanyName { get; set; }
        public string? CompanyTextColor { get; set; }
        public string currencytype { get; set; }
        public string currencyID { get; set; }

        public string baseCurrencyname { get; set; }
        public string TenantName { get; set; }
    }
}
