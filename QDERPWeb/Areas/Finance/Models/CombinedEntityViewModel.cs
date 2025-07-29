
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


    public class ClientStatusDisplayDTO
    {
        public byte StatusCode { get; set; }

        public string Status { get; set; }
    }
    
    public class ClientCategoryDisplayDTO
    {
        public string CategoryCode { get; set; }
        public string ClientCategory { get; set; }
        public short ClientCategoryCode { get; set; }
    }

    public class VoucherEntryDisplayDTO
    {
        public string VoucherNo { get; set; }
        public string DrCr { get; set; }
        public decimal? DrAmount { get; set; }
        public decimal? CrAmount { get; set; }
        public string EntryNarration { get; set; }
        public string AccountHead { get; set; }
        public string AccountId { get; set; }
        public string SysRemarks { get; set; }
        public long VoucherEntryNo { get; set; }
        public decimal? VoucherAmountFormatted { get; set; }

        public decimal? VoucherAmount { get; set; }
       //VoucherJounal fields
        public string AddedBy { get; set; }

        public DateTime? AddedOn { get; set; }
        public string Type { get; set; }

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
        public decimal? ConvertedDrAmount { get; set; }
        public decimal? ConvertedCrAmount { get; set; }
        public decimal? ConvertedAmount { get; set; }
        public string CurrencyImage { get; set; }
        public string CurrencyName { get; set; }
        public string EntryNarration { get; set; }
        public string TransactionCurrencySymbol { get; set; }
        public string TransactionCurrencyImage { get; set; }
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
        public decimal? ConvertedDrAmount { get; set; }
        public decimal? ConvertedCrAmount { get; set; }
        public decimal? ConvertedAmount { get; set; }
        public string CurrencyImage { get; set; }
        public string TransactionCurrencyName { get; set; }
        public string TransactionCurrencySymbol { get; set; }
        public string TransactionCurrencyImage { get; set; }
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
        //public dynamic Discount { get; set; }
        public decimal? Discount { get; set; }
        public dynamic DiscountDetails { get; set; }
        public dynamic ExemptionCode { get; set; }
        public string ItemCode { get; set; }
        public decimal? Qty { get; set; }
        public dynamic SNo { get; set; }
        public dynamic Total { get; set; }
        public dynamic TotalBeforeDiscount { get; set; }
        public dynamic UnitPrice { get; set; }
        public dynamic VAT { get; set; }
        public decimal? TaxAmount { get; set; }
        public dynamic TaxSlabCode { get; set; }
        public int InvoiceChildSlNo { get; set; }
        public int CreditNoteChildSlNo { get; set; }
        public string DetailedDescription { get; set; }
        public int UnitsToBill { get; set; }
        public decimal? UnitRate { get; set; }
        public decimal? QuantityInvoiced { get; set; }
        public decimal? QuantityCredited { get; set; }
        public string ProformaInvoiceNo { get; set; }
        public decimal? ExchangeRate { get; set; }


    }
    public class Signatory
    {
        public string SignatoryID { get; set; }
        public string SignatoryName { get; set; }
       

    }
    public class InvoiceItem1
    {
		public int PurchaseChildSlNo { get; set; }

		public string PurchaseVoucherNo { get; set; }

		public string UoM { get; set; }

		public decimal? QuantityInvoiced { get; set; }

		public decimal? UnitsToBill { get; set; }

		public byte? UnitRateMethod { get; set; }

		public decimal? UnitRate { get; set; }

		public decimal? LineAmount { get; set; }

		public string DetailedDescription { get; set; }

		public string ItemRemarks { get; set; }

		public string DatesBilledFor { get; set; }

		public string DeliveryNoteNo { get; set; }

		public DateTime? DeliveryDate { get; set; }

		public byte? TaxSlabCode { get; set; }

		public decimal? TaxAmount { get; set; }

		public string ItemCode { get; set; }

		public bool? IsExpenses { get; set; }

		public string ExpensesLedgerNo { get; set; }

		public decimal? Discount { get; set; }

		public string ItemPurchaseType { get; set; }

		public bool? IsGoodsInTransitAccount { get; set; }

		public string GoodsInTransitAccountNo { get; set; }

		public string GoodsInTransitPurchaseVoucherNo { get; set; }

		public decimal? LineOrderNo { get; set; }

		public string EmployeeNo { get; set; }

		public string PropertyNo { get; set; }
	}
		public class ExpenseClaimViewModel
    {
        public string ClaimRefNo { get; set; }
        public DateTime? ClaimDate { get; set; }
        public DateTime? ClaimEffectiveDate { get; set; }
        public string ProjectClaimedFor { get; set; }
        public string ClaimRemarks { get; set; }

        public string PaymentType { get; set; }
        public string PaymentAccount { get; set; }

        public string Priority { get; set; }

        public byte FundRequestTypeId { get; set; }

        public List<Tbl20103ExpenseClaimChild> ExpenseDetails { get; set; }
    }

    public class JournalRegisterViewModel
    {
        public string JournalRefNo { get; set; }
        public DateTime? JournalEntryDate { get; set; }
        public DateTime? JournalEffectiveDate { get; set; }
        public string JournalVoucherNarration { get; set; }

        public List<Tbl20127JournalRegisterChild> JournalDetails { get; set; }
    }
	public class PurchaseRequestViewModel
	{
		public string Mprno { get; set; }
		public DateTime? Mprdate { get; set; }
		public string ClientCode { get; set; }
		public string RequestedBy { get; set; }
		public string RequesterContactEmail { get; set; }
		public string RequesterContact { get; set; }
		public byte? ModeOfRequest { get; set; }
		public byte? TypeOfRequest { get; set; }
		public string SalesPersonCode { get; set; }
		public string ClientRefNo { get; set; }
		public string PurposeOfRequest { get; set; }
		public string Priority { get; set; }
		public string CostCenterText { get; set; }
		public DateTime? ExpectedDate { get; set; }
		public byte? ExpectedVatrate { get; set; }
		public string Remarks { get; set; }
		public byte? CompanyBranch { get; set; }
		public short? PurchaseRequestStatusId { get; set; }
		public short? InventoryMasterGroupId { get; set; }
		public string ProjectMasterCode { get; set; }
		public DateTime? BidClosingDate { get; set; }
		public DateTime? BidReminderOn { get; set; }
		public string ClientProject { get; set; }
		public byte? RequestSignatory { get; set; }
		public byte? MprverifiedSign { get; set; }
		public byte? MprapprovedSign { get; set; }
		

		public byte? TypeOfMpr { get; set; }

		

		public string StoreCode { get; set; }

		
		public string Mprremarks { get; set; }

		public bool? IsApproved { get; set; }

		public string PreparedBy { get; set; }

		public DateTime? PreparedOn { get; set; }

		public string ApprovedBy { get; set; }

		public DateTime? ApprovedOn { get; set; }

		public string AddedBy { get; set; }

		public DateTime? AddedOn { get; set; }

		public string ModifiedBy { get; set; }

		public DateTime? ModifiedOn { get; set; }

	

		public string Project { get; set; }

	
		public short? DepartmentId { get; set; }

		

		public bool? IsSubmitted { get; set; }

		public string SubmittedBy { get; set; }

		public DateTime? SubmittedOn { get; set; }

		public bool? IsVerified { get; set; }

		public string VerifiedBy { get; set; }

		public DateTime? VerifiedOn { get; set; }

		public string RequesterName { get; set; }

	
		public short? ProjectSubUnitCode { get; set; }

		

		public string MaterialQuoteNo { get; set; }

		public byte? MprrevisionId { get; set; }

		public bool? IsObseleteVersion { get; set; }

		public string MprrevisedBy { get; set; }

		public DateTime? MprrevisedOn { get; set; }

		public string MprrevisionNo { get; set; }

		public bool? IsCancelled { get; set; }

		public string CancelledBy { get; set; }

		public DateTime? CancelledOn { get; set; }

		public string RemarksByApprover { get; set; }

		public string RemarksByVerifier { get; set; }

		public List<Tbl60602purchaseRequestChild> PurchaseRequestDetails { get; set; }
		
	}
    public class SalesorderViewModel
    {
        public string SalesOrderNo { get; set; }

        public DateTime? SalesOrderDate { get; set; }

        public string ClientPono { get; set; }

        public DateTime? ClientPodate { get; set; }

        public string QuoteNo { get; set; }

        public DateTime? QuoteDate { get; set; }

        public string ClientRefNo { get; set; }

        public string Attention { get; set; }

        public string SubjectTitle { get; set; }

        public byte? TypeOfQuote { get; set; }

        public string QuoteType { get; set; }

        public decimal? QuoteTransport { get; set; }

        public decimal? QuoteDiscount { get; set; }

        public byte? PaymentTerms { get; set; }

        public byte? DeliveryPeriod { get; set; }

        public byte? DeliveryTerms { get; set; }

        public string QuoteValidity { get; set; }

        public string PreparedBy { get; set; }

        public DateTime? PreparedOn { get; set; }

        public string ApprovedBy { get; set; }

        public DateTime? ApprovedOn { get; set; }

        public string AddedBy { get; set; }

        public DateTime? AddedOn { get; set; }

        public string ModifiedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public string Rfqcode { get; set; }

        public string ClientContactNo { get; set; }

        public string ClientContactEmail { get; set; }

        public string ClientCode { get; set; }

        public string QuotationSummary { get; set; }

        public byte? QuoteSignatory { get; set; }

        public string QuoteIntro { get; set; }

        public byte? TypeOfRequest { get; set; }

        public byte? ModeOfRequest { get; set; }

        public string AdditionsText { get; set; }

        public string DiscountsText { get; set; }

        public DateTime? QuoteDueDate { get; set; }

        public string Project { get; set; }

        public string SalesPersonCode { get; set; }

        public bool? IsVerified { get; set; }

        public bool? IsApproved { get; set; }

        public byte? RevisionNo { get; set; }

        public byte? CompanyBranch { get; set; }

        public string ProjectMasterCode { get; set; }

        public DateTime? OrderExpiryDate { get; set; }

        public byte? InventoryMasterGroupId { get; set; }

        public string SalesOrderRemarks { get; set; }

        public DateTime? ExpectedDeliveryDate { get; set; }

        public bool? IsSubmitted { get; set; }

        public string SubmittedBy { get; set; }

        public DateTime? SubmittedOn { get; set; }

        public string VerifiedBy { get; set; }

        public DateTime? VerifiedOn { get; set; }

        public string CostAllocationMasterGroup { get; set; }
        public string ? ValveType { get; set; }  // e.g. "Manual Valves", "Control Valves", "Safety Valves"

        public List<Tbl60202salesOrderChild> SalesOrderChildren { get; set; }

    }
    public class RFQViewModel
	{
		public string Rfqno { get; set; }

		public DateTime? Rfqdate { get; set; }

		public string Mprno { get; set; }

		public string SupplierCode { get; set; }

		public bool? IsQuoted { get; set; }

		public bool? IsWon { get; set; }

		public string ReasonWon { get; set; }

		public string DeliveryPeriod { get; set; }

		public string PaymentTerms { get; set; }

		public string QuoteValidTo { get; set; }

		public DateTime? QuoteValidDate { get; set; }

		public string SupplierQuotationNo { get; set; }

		public DateTime? SupplierQuotationDt { get; set; }

		public string PreparedBy { get; set; }

		public DateTime? PreparedOn { get; set; }

		public string AddedBy { get; set; }

		public DateTime? AddedOn { get; set; }

		public string ModifiedBy { get; set; }

		public DateTime? ModifiedOn { get; set; }

		public byte? CompanyBranch { get; set; }

		public string Attention { get; set; }

		public string SupplierContactNo { get; set; }

		public string SupplierContactEmail { get; set; }

		public bool? IsApproved { get; set; }

		public string ApprovedBy { get; set; }

		public DateTime? ApprovedOn { get; set; }

		public string Rfqsubject { get; set; }

		public string Rfqintro { get; set; }

		public string Rfqsummary { get; set; }

		public byte? Rfqsignatory { get; set; }

		public string Project { get; set; }

		public byte? InventoryMasterGroupId { get; set; }

		public string ProjectMasterCode { get; set; }

		public bool? IsSubmitted { get; set; }

		public string SubmittedBy { get; set; }

		public DateTime? SubmittedOn { get; set; }

		public bool? IsVerified { get; set; }

		public string VerifiedBy { get; set; }

		public DateTime? VerifiedOn { get; set; }

		public string SalesPersonCode { get; set; }

		public List<Tbl60702rfqchild> RFQDetailses { get; set; }
		

	}
	public class QuotationViewModel
	{
		public string QuoteNo { get; set; }

		public DateTime? QuoteDate { get; set; }

		public string ClientRefNo { get; set; }

		public string Attention { get; set; }

		public string SubjectTitle { get; set; }

		public byte? TypeOfQuote { get; set; }

		public string QuoteType { get; set; }

		public decimal? QuoteTransport { get; set; }

		public decimal? QuoteDiscount { get; set; }

		public byte? PaymentTerms { get; set; }

		public byte? DeliveryPeriod { get; set; }

		public byte? DeliveryTerms { get; set; }

		public string QuoteValidity { get; set; }

		public string PreparedBy { get; set; }

		public DateTime? PreparedOn { get; set; }

		public string ApprovedBy { get; set; }

		public DateTime? ApprovedOn { get; set; }

		public string AddedBy { get; set; }

		public DateTime? AddedOn { get; set; }

		public string ModifiedBy { get; set; }

		public DateTime? ModifiedOn { get; set; }

		public string Rfqcode { get; set; }

		public string ClientContactNo { get; set; }

		public string ClientContactEmail { get; set; }

		public string ClientCode { get; set; }

		public string QuotationSummary { get; set; }

		public byte? QuoteSignatory { get; set; }

		public string QuoteIntro { get; set; }

		public byte? TypeOfRequest { get; set; }

		public byte? ModeOfRequest { get; set; }

		public string AdditionsText { get; set; }

		public string DiscountsText { get; set; }

		public DateTime? QuoteDueDate { get; set; }

		public string Project { get; set; }

		public string SalesPersonCode { get; set; }

		public bool? IsVerified { get; set; }

		public bool? IsApproved { get; set; }

		public byte? RevisionNo { get; set; }

		public byte? CompanyBranch { get; set; }

		public string Mprno { get; set; }

		public string QuoteThanksNote { get; set; }

		public string QuoteColumn1 { get; set; }

		public string QuoteColumn2 { get; set; }

		public string QuoteColumn3 { get; set; }

		public string QuoteLabel1 { get; set; }

		public string QuoteLabel2 { get; set; }

		public string QuoteLabel3 { get; set; }

		public DateTime? QuoteSubmittedOn { get; set; }

		public string QuoteSubmittedBy { get; set; }

		public byte? QuoteStatus { get; set; }

		public byte? InventoryMasterGroupId { get; set; }

		public byte? VerifiedSignatory { get; set; }

		public byte? ApprovedSignatory { get; set; }

		public bool? IsSubmitted { get; set; }

		public string SubmittedBy { get; set; }

		public DateTime? SubmittedOn { get; set; }

		public string VerifiedBy { get; set; }

		public DateTime? VerifiedOn { get; set; }

		public string ProjectMasterCode { get; set; }

		public DateTime? BidClosingDate { get; set; }

		public string TransportationScope { get; set; }

		public List<Tbl60102quotationChild> QuotationDetailses { get; set; }


	}
	public class MaterialReceiptViewModel
	{
		public string ReceiptNo { get; set; }

		public DateTime? ReceiptDate { get; set; }

		public byte? ModeOfReceiptId { get; set; }

		public string SupplierCode { get; set; }

		public string ClientCode { get; set; }

		public string Mprno { get; set; }

		public string Rfqno { get; set; }

		public string SupplierQuotationNo { get; set; }

		public string OurPurchaseOrderNo { get; set; }

		public string JobCode { get; set; }

		public string IssueRemarks { get; set; }

		public string PreparedBy { get; set; }

		public DateTime? PreparedOn { get; set; }

		public string ApprovedBy { get; set; }

		public DateTime? ApprovedOn { get; set; }

		public string AddedBy { get; set; }

		public DateTime? AddedOn { get; set; }

		public string ModifiedBy { get; set; }

		public DateTime? ModifiedOn { get; set; }

		public byte? CompanyBranch { get; set; }

		public bool? IsApproved { get; set; }

		public byte? ReceiptSignatory { get; set; }

		public string VatpurchaseBillNo { get; set; }

		public string StoreCode { get; set; }

		public string DeliveryNoteNo { get; set; }

		public bool? IsPosted { get; set; }

		public DateTime? PostedOn { get; set; }

		public string PostedBy { get; set; }

		public string VoucherNo { get; set; }

		public string SupplierDeliveryNoteNo { get; set; }

		public string ProjectMasterCode { get; set; }

		public byte? InventoryMasterGroupId { get; set; }

		public string SalesPersonCode { get; set; }

		public DateTime? InventoryEffectiveDate { get; set; }

		public bool? IsSubmitted { get; set; }

		public string SubmittedBy { get; set; }

		public DateTime? SubmittedOn { get; set; }

		public string VerifiedBy { get; set; }

		public DateTime? VerifiedOn { get; set; }

		public bool? IsVerified { get; set; }

		public string StoreReceivedIn { get; set; }

		public List<Tbl60502materialReceiptChild> MaterialReceiptDetailses { get; set; }


	}
	public class CloneJournalEntryRequest
    {
        public string FromJournalRefNo { get; set; }
        public string ToJournalRefNo { get; set; }
        public int RequesterId { get; set; }
    }
    public class InsertVoucherFromJournalRequest
    {
        public string JournalRefNo { get; set; }
        public string PostingVoucherNo { get; set; }
        public string AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
    }
    public class JournalPostRequest
    {
        public string VoucherNo { get; set; }
        public string NewVoucherNo { get; set; }
    }
    public class ExpenseClaimChildDto
    {
        public long ClaimChildNo { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal? RoundOff { get; set; }
        public string SupplierName { get; set; }
        public string SupplierVATNo { get; set; }
        public string PurchaserName { get; set; }
        public string LineNarration { get; set; }
        public string EmployeeNo { get; set; }
        public string PropertyNo { get; set; }
        public string SupplierNameAr { get; set; }
        public byte VATApplicableRate { get; set; }
        public bool IsTaxIncluded { get; set; }
    }
    public  class ClaimPaymentDto
    {
        public string ClaimRefNo { get; set; } = default!;
        public string PaymentVoucherNo { get; set; } = default!;
        public decimal TotalAmount { get; set; }
        public DateTime EffectiveDate { get; set; }
        public byte TypeOfClaim { get; set; }

        public string SelectedAccountHead { get; set; }
        public string SelectedPaymentType { get; set; }
    }
    public class TrialBalanceRequest
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string AccountGroup { get; set; }

        public int Skip { get; set; }
        public int Take { get; set; }
    }
    public class DepreciationRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string DocumentNo { get; set; }
    }
    public class InsertCostDistributionDto
    {
        public string QuotationNo { get; set; }

        public long QuoteChildId { get; set; }

        public decimal? QuotedQuantity { get; set; }

        public decimal? QuotedCostPrice { get; set; }

        public decimal? PostingAmount { get; set; }

        public decimal? PostingPercentage { get; set; }

        public string Gscode { get; set; }

        public string PostingCostItemCode { get; set; }

        public decimal? TotalCostOfItemInclAll { get; set; }
    }
    public class CostAllocationDto
    {
        public byte CostAllocationId { get; set; }
        public string CostAllocDrCr { get; set; }
        public string CostAllocationUnitId { get; set; }
        public DateTime EffectiveDate { get; set; }
        public decimal AmountAllocated { get; set; }
        public string CostAllocRemarks { get; set; }
        public long VoucherEntryId { get; set; }
        public string VoucherNo { get; set; }
    }
    public class PropertyAllocationDto
    {
        public byte PropertyAllocationId { get; set; }
        public string PropertyAllocDrCr { get; set; }
        public string PropertyNo { get; set; }
        public DateTime EffectiveDate { get; set; }
        public decimal AmountAllocated { get; set; }
        public string PropertyAllocRemarks { get; set; }
        public long VoucherEntryId { get; set; }
        public string VoucherNo { get; set; }

        public string LedgerAccountNo { get; set; }
    }
    public class EmployeeAllocationDto
    {
        public byte EmployeeAllocationId { get; set; }
        public string EmpAllocDrCr { get; set; }
        public string EmployeeNo { get; set; }
        public DateTime EffectiveDate { get; set; }
        public decimal AmountAllocated { get; set; }
        public string CostAllocRemarks { get; set; }
        public long VoucherEntryId { get; set; }
        public string VoucherNo { get; set; }

        public string LedgerAccountNo { get; set; }
    }
    public class UpdateCostAllocationFieldsDto
    {
        public int CostAllocationId { get; set; }
        public decimal VoucherAmount { get; set; }
        public string CostAllocRemarks { get; set; }
        public string CostAllocationUnitId { get; set; }
        public DateTime EffectiveDate { get; set; }
    }
    public class UpdatePropertyAllocationFieldsDto
    {
        public int PropertyAllocationId { get; set; }
        public decimal VoucherAmount { get; set; }
        public string PropertyAllocRemarks { get; set; }
        public string PropertyNo { get; set; }
        public DateTime EffectiveDate { get; set; }
    }
    public class UpdateEmployeeAllocationFieldsDto
    {
        public int EmployeeAllocationId { get; set; }
        public decimal VoucherAmount { get; set; }
        public string CostAllocRemarks { get; set; }
        public string EmployeeNo { get; set; }
        public DateTime EffectiveDate { get; set; }
    }
    public class VoucherDeleteRequest
    {
        public string VoucherNo { get; set; }
    }
    public class SubLedgerDto
    {
        public long VoucherEntryNo { get; set; }
        public string DrCr { get; set; }
        public string ReferenceType { get; set; }
        public string ReferenceNo { get; set; }
        public decimal Amount { get; set; }
        public string accountId { get; set; }
        public string VoucherNo { get; set; }
    }
    public class DeleteSubLedgerDto
    {
        public long VoucherEntryNo { get; set; }
        public decimal Amount { get; set; }
    }

    public class ClaimMasterDto
    {
        public string ClaimRefNo { get; set; }
        public string ClaimRemarks { get; set; }
        public string SupplierPaymentLedgerNo { get; set; }
    }

}

