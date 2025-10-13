namespace QD.ERP.Shared.DAL.Entities
{
    public class SupplierOutstanding
    {
        public string AccountHead { get; set; } // Maps to NVARCHAR(100)
        public decimal Balance { get; set; }   // Maps to DECIMAL(18, 2)
    }
}
