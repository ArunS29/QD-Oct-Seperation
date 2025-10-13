using QD.ERP.Shared.DAL.Entities;

namespace QD.ERP.Shared.Models
{
    public class VATFinalReturnsSummary
    {
        public decimal? DSS_Revenue { get; set; }
        public decimal? DZR_Revenue { get; set; }
        public decimal? DEX_Revenue { get; set; }
        public decimal? GCC_Revenue { get; set; }
        public decimal? EXP_Revenue { get; set; }
        public decimal? DSS_Sales_Tax { get; set; }

        public decimal? DSS_Purchases { get; set; }
        public decimal? DZR_Purchases { get; set; }
        public decimal? DEX_Purchases { get; set; }
        public decimal? IPC_Purchases { get; set; }
        public decimal? RCM_Purchases { get; set; }

        public decimal? DSS_Pur_Tax { get; set; }
        public decimal? IPC_Pur_Tax { get; set; }
        public decimal? RCM_Pur_Tax { get; set; }

        public decimal? DSS_CreditNote { get; set; }
        public decimal? DZR_CreditNote { get; set; }
        public decimal? DEX_CreditNote { get; set; }
        public decimal? GCC_CreditNote { get; set; }
        public decimal? EXP_CreditNote { get; set; }
        public decimal? DSS_Tax_CreditNote { get; set; }

        public decimal? DSS_DebitNote { get; set; }
        public decimal? DZR_DebitNote { get; set; }
        public decimal? DEX_DebitNote { get; set; }
        public decimal? IPC_DebitNote { get; set; }
        public decimal? RCM_DebitNote { get; set; }

        public decimal? DSS_Debit_Tax { get; set; }
        public decimal? IPC_Debit_Tax { get; set; }
        public decimal? RCM_Debit_Tax { get; set; }

        public decimal? RevenueTotal { get; set; }
        public decimal? PurchaseTotal { get; set; }
        public decimal? CreditNoteTotal { get; set; }
        public decimal? DebitNoteTotal { get; set; }

        public decimal? DSS_Sales_TaxAfterAdj { get; set; }
        public decimal? DSS_Pur_TaxAfterAdj { get; set; }
        public decimal? IPC_Pur_TaxAfterAdj { get; set; }
        public decimal? RCM_Pur_TaxAfterAdj { get; set; }

        public decimal? TotalPurchaseTax { get; set; }
        public decimal? TotalPayableTax { get; set; }

        public decimal? DSS_15_Revenue { get; set; }
        public decimal? DSS_15_Sales_Tax { get; set; }
        public decimal? DSS_15_Sales_TaxAfterAdj { get; set; }
        public decimal? DSS_Total_SalesTax { get; set; }

        public decimal? DSS_15_CreditNote { get; set; }

        public decimal? DSS_Purchases15Perc { get; set; }
        public decimal? IPC_Purchases15Perc { get; set; }
        public decimal? RCM_Purchases15Perc { get; set; }

        public decimal? DSS_15Per_DebitNote { get; set; }
        public decimal? IPC_15Per_DebitNote { get; set; }
        public decimal? RCM_15Per_DebitNote { get; set; }

        public decimal? DSS_15Per_Pur_TaxAfterAdj { get; set; }
        public decimal? IPC_15Per_Pur_TaxAfterAdj { get; set; }
        public decimal? RCM_15Per_Pur_TaxAfterAdj { get; set; }

        public decimal? DSS_10_Revenue { get; set; }
        public decimal? DSS_10_Sales_Tax { get; set; }
        public decimal? DSS_10_Sales_TaxAfterAdj { get; set; }
        public decimal? DSS_10_CreditNote { get; set; }
        public decimal? DSS_Total_SalesTax_BH { get; set; }

        public decimal? DSS_Purchases10Perc { get; set; }
        public decimal? IPC_Purchases10Perc { get; set; }
        public decimal? RCM_Purchases10Perc { get; set; }

        public decimal? PurchaseTotal_BHD { get; set; }

        public decimal? DSS_10Per_DebitNote { get; set; }
        public decimal? IPC_10Per_DebitNote { get; set; }
        public decimal? RCM_10Per_DebitNote { get; set; }

        public decimal? DSS_10Per_Pur_TaxAfterAdj { get; set; }
        public decimal? IPC_10Per_Pur_TaxAfterAdj { get; set; }
        public decimal? RCM_10Per_Pur_TaxAfterAdj { get; set; }

        public decimal? DebitNoteTotal_BHD { get; set; }
        public decimal? TotalPurchaseTax_BHD { get; set; }
        public decimal? RevenueTotal_BHD { get; set; }
        public decimal? CreditNoteTotal_BHD { get; set; }
        public decimal? TotalPayableTax_BHD { get; set; }
    }
    public class SaveGoodsAndServicesRequest
    {
        public Tbl20164GoodsAndServicesMaster Model { get; set; }
        public int invoiceChildSlNo { get; set; }
    }


}
