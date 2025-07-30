namespace QD.ERP.Web.DAL.Entities
{
    public class FinancialSummaryReport
    {
        public string AccountHead { get; set; }
        public string AccountHeadName { get; set; }
        public decimal VoucherAmountFormatted { get; set; }
        public string AccountGroup { get; set; }
        public string MasterGroup { get; set; }
        public string TransactionsFull { get; set; }
        public string AccountHeadArabic { get; set; }
        public string AccountGroupAr { get; set; }
        public string MasterGroupAr { get; set; }

        public DateTime VoucherDate { get; set; }
    }
}
