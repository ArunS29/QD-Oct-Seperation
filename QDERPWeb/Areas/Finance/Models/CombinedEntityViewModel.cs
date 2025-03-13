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
        public decimal? VoucherAmountFormatted { get; set; }
    }
    public class AccountLedger
    {
        public string VoucherNo { get; set; }
        public DateTime? VoucherDate { get; set; }
        public string VoucherRefNo { get; set; }
        public string VoucherNarration { get; set; }
        public string VoucherEnteredBy { get; set; }
        public DateTime? VoucherEnteredOn { get; set; }
        public string VoucherVerifiedBy { get; set; }
        public DateTime? VoucherVerifiedOn { get; set; }
        public string VoucherApprovedBy { get; set; }
        public DateTime? VoucherApprovedOn { get; set; }
        public long? VoucherEntryNo { get; set; }
        public string AccountHead { get; set; }
        public string AccountHeadName { get; set; }
        public string DrCr { get; set; }
        public decimal? DrAmount { get; set; }
        public decimal? CrAmount { get; set; }
        public decimal? VoucherAmountFormatted { get; set; }
        public string EntryNarration { get; set; }
        public string AccountGroup { get; set; }
        public string MasterGroup { get; set; }
        public string VoucherType { get; set; }
        public string SysRemarks { get; set; }
        public string VoucherModifiedBy { get; set; }
        public DateTime? VoucherModifiedOn { get; set; }
        public string BillNo { get; set; }
        public DateTime? BillDate { get; set; }
        public string BillPaidTo { get; set; }
        public string BillRemarks { get; set; }
        public DateTime? VoucherEffectiveDate { get; set; }
        public string AccountHeadArabic { get; set; }
        public string ReferenceNote { get; set; }
     
    }
    public class ReportRequest
    {
        public string accountId { get; set; }
        public string frmDate { get; set; }
        public string toDate { get; set; }
    }
    public class AccountRegister
    {
        public string VoucherNo { get; set; }
        public DateTime VoucherDate { get; set; }
        public string VoucherRefNo { get; set; }
        public string VoucherNarration { get; set; }
        public string VoucherEnteredBy { get; set; }
        public DateTime? VoucherEnteredOn { get; set; }
        public string VoucherVerifiedBy { get; set; }
        public DateTime? VoucherVerifiedOn { get; set; }
        public string VoucherApprovedBy { get; set; }
        public DateTime? VoucherApprovedOn { get; set; }
        public long? VoucherEntryNo { get; set; }
        public string AccountHead { get; set; }
        public string AccountHeadName { get; set; }
        public string DrCr { get; set; }
        public decimal? DrAmount { get; set; }
        public decimal? CrAmount { get; set; }
        public decimal? VoucherAmountFormatted { get; set; }
        public string EntryNarration { get; set; }
        public string AccountGroup { get; set; }
        public string MasterGroup { get; set; }
        public string VoucherType { get; set; }
        public string SysRemarks { get; set; }
        public DateTime? VoucherEffectiveDate { get; set; }
        public string SubGroupName { get; set; }
        public string VoucherModifiedBy { get; set; }
        public DateTime? VoucherModifiedOn { get; set; }

    }
}

