
﻿using System.ComponentModel.DataAnnotations.Schema;

﻿using QD.ERP.Web.DAL.Entities;

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
    //public partial class qry20103GetBankAccountsResult
    //{
    //    public string AccountHead { get; set; }
    //    public string AccountHeadName { get; set; }
    //    public string MasterGroupID { get; set; }
    //    public decimal? Amount { get; set; }
    //    public string AccountGroup { get; set; }
    //}

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

        public decimal? VoucherAmount { get; set; }
       //VoucherJounal fields
        public string AddedBy { get; set; }

        public DateTime? AddedOn { get; set; }

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

    public class TrialBalanceResult
    {
        public string VoucherNo { get; set; }
        public DateTime? VoucherDate { get; set; }
        public long? VoucherEntryNo { get; set; }
        public string AccountHead { get; set; }
        public string AccountHeadName { get; set; }
        public string DrCr { get; set; }
        public decimal? DrAmount { get; set; }
        public decimal? CrAmount { get; set; }
        public decimal? VoucherAmountFormatted { get; set; }
        public string AccountGroup { get; set; }
        public string MasterGroup { get; set; }
        public string VoucherType { get; set; }
        public string Transactions { get; set; }
        public string MonthYear { get; set; }
        public int? MonthNumber { get; set; }
        public string Category { get; set; }
        public string VoucherRefNo { get; set; }
        public string VoucherNarration { get; set; }
        public string EntryNarration { get; set; }
        public string SysRemarks { get; set; }
    }
    public class AssetRegisterViews
    {
        public string MasterGroupID { get; set; }
        public string MasterGroup { get; set; }
        public string AccountGroup { get; set; }
        public string AccountID { get; set; }
        public string AccountHead { get; set; }
        public string AssetLedgerNo { get; set; }
        public string AssetDescription { get; set; }
        public string Specifications { get; set; }
        public string Brand { get; set; }
        public string PlateNo { get; set; }
        public string Model { get; set; }
        public string Year { get; set; }
        public byte? Ownership { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public byte? PurchasedAs { get; set; }
        public bool? IsFinanced { get; set; }
        public short? FinancedFrom { get; set; }
        public decimal? ValueOfProperty { get; set; }
        public decimal? InitialDownPayment { get; set; }
        public decimal? InitialDocCharges { get; set; }
        public decimal? MonthlyInstallment { get; set; }
        public byte? NoOfInstallments { get; set; }
        public DateTime? InstallmentStartDate { get; set; }
        public DateTime? InstallmentEndDate { get; set; }
        public decimal? FinalInstallment { get; set; }
        public string DepreciationMethod { get; set; }
        public byte? LifeSpanOfProperty { get; set; }
        public decimal? ScrapValueOfProperty { get; set; }
        public string AddedBy { get; set; }
        public DateTime? AddedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool? IsDiscontinued { get; set; }
        public DateTime? DiscontinuedOn { get; set; }
        public string DiscontinuedRemarks { get; set; }
        public string AssetCategory { get; set; }
        public string AssetLocation { get; set; }
        public decimal? ValueAfterScrap { get; set; }
        public decimal? MonthlyDepreciation { get; set; }
        public decimal? YearlyDepreciation { get; set; }
        public int? NoOfMonthsOld { get; set; }
        public decimal? DepAsOnDate { get; set; }
        public decimal? CurrentValueOfAsset { get; set; }
        public decimal? OpeningTotal { get; set; }
        public decimal? TotalDebit { get; set; }
        public decimal? TotalCredit { get; set; }
        public decimal? ClosingBalance { get; set; }
        [Column("DepreciationPercentage", TypeName = "decimal(6,2)")]
        public decimal? DepreciationPercentage { get; set; }
        public decimal? TotalDepreciatedAmount { get; set; }
        [Column("CalculatedDepreciationAmount", TypeName = "decimal(38,7)")]
        public decimal? CalculatedDepreciationAmount { get; set; }
        [Column("FinalDepreciationAmount", TypeName = "decimal(38,7)")]
        public decimal? FinalDepreciationAmount { get; set; }
        [Column("AccumulatedDepreciationTotal", TypeName = "decimal(38,7)")]
        public decimal? AccumulatedDepreciationTotal { get; set; }
        [Column("BookValue", TypeName = "decimal(38,7)")]
        public decimal? BookValue { get; set; }
        public string DepreciationLedgerNo { get; set; }
        public string AccumDepLedgerNo { get; set; }
        public string PurchasedFrom { get; set; }
        public string CurrentCondition { get; set; }
        public string PurchasedAs2 { get; set; }
        public string FinancedBy { get; set; }
        public string CurrentReading { get; set; }
        public string AssetType { get; set; }
        public decimal? FMV { get; set; }
        public decimal? BMV { get; set; }
        public string PropertyNo { get; set; }
        public decimal? NetBookValue { get; set; }
        public string EquipmentSerialNo { get; set; }
        public string EquipmentLocationDelivered { get; set; }
        public string EquipmentCurrentStatus { get; set; }
        public string EquipmentMobilizedTo { get; set; }
        public string EquipmentClientSite { get; set; }
        public decimal? EquipmentClientRatePerHour { get; set; }
        public string EquipmentCertification { get; set; }
        public string EquipmentPWAS { get; set; }
        public string EquipmentAttachments { get; set; }
        public string EquipmentCurrentOperators { get; set; }
        public string EquipmentCapacity { get; set; }

    }
    public class ExpenseClaimViews
    {
        public string ClaimRefNo { get; set; }
        public DateTime? ClaimDate { get; set; }
        public string ProjectClaimedFor { get; set; }
        public string ClaimRemarks { get; set; }
        public string ClaimCreatedBy { get; set; }
        public DateTime? ClaimCreatedOn { get; set; }
        public string ClaimModifiedBy { get; set; }
        public DateTime? ClaimModifiedOn { get; set; }
        public bool IsSubmittedToFinance { get; set; }
        public string SubmittedBy { get; set; }
        public DateTime? SubmittedOn { get; set; }
        public bool IsApproved { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public decimal? ClaimedAmountTotal { get; set; }
        public decimal? ApprovedAmountTotal { get; set; }
        public byte? ClaimerID { get; set; }
        public string ClaimerName { get; set; }
        public string PaymentType { get; set; }
        public string PaymentAccount { get; set; }
        public string PaidBy { get; set; }
        public DateTime? PaidOn { get; set; }
        public string PaymentVoucherNo { get; set; }
        public bool IsPaid { get; set; }
        public bool IsVerified { get; set; }
        public string VerifiedBy { get; set; }
        public DateTime? VerifiedOn { get; set; }
    }


    public class VoucherViewModel
    {
        public Tbl201VoucherMaster VoucherMaster { get; set; }
        public List<Tbl201VoucherEntry> VoucherEntries { get; set; }

    }
    public class RegisterVoucherViewModel
    {
        public Tbl20126JournalRegisterMaster JournalVoucherMaster { get; set; }
        public List<Tbl20127JournalRegisterChild> JournalVoucherEntries { get; set; }



    }
    public class DashBoardBankAccount
    {
        public string MasterGroupID { get; set; }
        public DateTime EndDate { get; set; }

    }
    // DTO for API request
    public class VoucherUpdateRequest
    {
        public List<string> VoucherNos { get; set; }
        public string ActionType { get; set; } // "verify", "approve", "unlock"
        public string LogOnUser { get; set; } // Populated with logged-in user
    }

    public class JournalRegisterView
    {
        public string JournalRefNo { get; set; }
        public DateTime? JournalEntryDate { get; set; }
        public string JournalVoucherNarration { get; set; }
        public string JournalCreatedBy { get; set; }
        public DateTime? JournalCreatedOn { get; set; }
        public string JournalModifiedBy { get; set; }
        public DateTime? JournalModifiedOn { get; set; }
        public bool IsSubmittedToFinance { get; set; }
        public string SubmittedBy { get; set; }
        public DateTime? SubmittedOn { get; set; }
        public bool IsVerified { get; set; }
        public string VerifiedBy { get; set; }
        public DateTime? VerifiedOn { get; set; }
        public bool IsApproved { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public bool IsPosted { get; set; }
        public string PostedVoucherNo { get; set; }
        public string PostedBy { get; set; }
        public DateTime? PostedOn { get; set; }
        public DateTime? JournalEffectiveDate { get; set; }
        public byte? RequesterID { get; set; }
        public decimal? DrAmountTotal { get; set; }
        public decimal? CrAmountTotal { get; set; }
        public decimal? Difference { get; set; }
    }
    //public class InvoiceUpdateRequest
    //{
    //    public List<Tbl20162VatinvoiceChild> InvoiceChildren { get; set; }
    //    public Tbl20161VatinvoiceMaster InvoiceMaster { get; set; }

    //}

    public class InvoiceItem
    {
        public string InvoiceNo { get; set; }
        public dynamic Amount { get; set; }
        public dynamic Description { get; set; }
        public dynamic Discount { get; set; }
        public dynamic DiscountDetails { get; set; }
        public dynamic ExemptionCode { get; set; }
        public dynamic ItemCode { get; set; }
        public dynamic Qty { get; set; }
        public dynamic SNo { get; set; }
        public dynamic Total { get; set; }
        public dynamic TotalBeforeDiscount { get; set; }
        public dynamic UnitPrice { get; set; }
        public dynamic VAT { get; set; }
        public decimal? TaxAmount { get; set; }
        // public decimal VAT% { get; set; }
    }
    public class Signatory
    {
        public string SignatoryID { get; set; }
        public string SignatoryName { get; set; }
       

    }



}

