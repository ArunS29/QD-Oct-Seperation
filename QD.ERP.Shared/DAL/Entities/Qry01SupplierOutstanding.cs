namespace QD.ERP.Shared.DAL.Entities
{
    public class Qry01SupplierOutstanding
    {
        public string AccountHeadNo { get; set; }
        public string AccountHead { get; set; }
        public decimal Balance { get; set; }
        public int OverdueDays { get; set; }
    }

}
