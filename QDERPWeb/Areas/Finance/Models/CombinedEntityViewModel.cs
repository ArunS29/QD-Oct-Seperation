namespace QD.ERP.Web.Areas.Finance.Models
{
    public class CombinedEntityViewModel
    {
        public int SalaryPayableId { get; set; }
        public string ReferenceType { get; set; }
        public string ReferenceNo { get; set; }
        public string EmployeeNo { get; set; }
        public string EmployeeName { get; set; }
        public decimal Amount { get; set; }
        public string DrCr { get; set; }
        public string AdditionalFieldFromSecondEntity { get; set; } // Example
        public string SalaryPayableLedgerNo { get; set; }
    }

    public class VoucherEntryDisplayDTO
    {
        public string VoucherNo { get; set; }
        public string DrCr { get; set; }
        public decimal? DrAmount { get; set; }
        public decimal? CrAmount { get; set; }
        public string EntryNarration { get; set; }
        public string AccountHead { get; set; }
        public string SysRemarks { get; set; }
        public long VoucherEntryNo { get; set; }
    }

}

