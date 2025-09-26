using System;
using Microsoft.EntityFrameworkCore;

namespace QD.ERP.Web.DAL.Entities
{
    [Keyless]
    public class tbl001CostAnalysis
    {
        public long CostAllocationID { get; set; }
        public long VoucherEntryID { get; set; }
        public string? CostAllocationUnitID { get; set; }
        public string? CostAllocDrCr { get; set; }
        public decimal? AmountAllocated { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public string? CostAllocRemarks { get; set; }
        public string? CostAllocationUnit { get; set; }
        public string? CostAllocationGroup { get; set; }
        public string? CostAllocationMasterGroup { get; set; }
        public bool? IsDisabled { get; set; }
        public string? AccountHead { get; set; }
        public string? AccountGroup { get; set; }
        public string? MasterGroup { get; set; }
        public decimal? Expenses { get; set; }
        public decimal? Income { get; set; }
        public string PL { get; set; } = string.Empty;
        public decimal? CostAmount { get; set; }
        public string? AccountID { get; set; }
        public string? VoucherNo { get; set; }
        public string? VoucherType { get; set; }
        public string? VoucherTypeAndNo { get; set; }
        public string? CostCenterIncharge { get; set; }
        public string? EntryNarration { get; set; }
        public string? SysRemarks { get; set; }
        public DateTime? VoucherDate { get; set; }
        public DateTime? VoucherMonth { get; set; }
        public DateTime? VoucherYear { get; set; }
        public DateTime? EffectiveMonth { get; set; }
        public DateTime? EffectiveYear { get; set; }
        public DateTime? AllocationEffectiveDate { get; set; }
        public DateTime? AllocationEffectiveMonth { get; set; }
        public DateTime? AllocationEffectiveYear { get; set; }
        public string? ProjectMasterCode { get; set; }
        public string? BranchCode { get; set; }
        public string? BranchName { get; set; }
        public string? VoucherNarration { get; set; }
        public string? VoucherRefNo { get; set; }
        public decimal? ConvertedIncome { get; set; }
        public decimal? ConvertedExpenses { get; set; }
        public decimal? ConvertedCostAmount { get; set; }
    }
}
