using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using QD.ERP.Web.DAL.Entities;

namespace QDWEB.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    public class ExpensesClaimController : Controller
    {
        private ERPMasterWtDataContext _context;

        public ExpensesClaimController(ERPMasterWtDataContext context) {
            _context = context;
        }

        //[HttpGet]
        //public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions) {
        //    var tbl201voucherentries = _context.Tbl201VoucherEntries.Select(i => new {
        //        i.VoucherEntryNo,
        //        i.VoucherNo,
        //        i.AccountHead,
        //        i.VoucherAmount,
        //        i.DrCr,
        //        i.AddedBy,
        //        i.AddedOn,
        //        i.EntryNarration,
        //        i.SysRemarks,
        //        i.BankClearedOn,
        //        i.ReconciliationRemarks,
        //        i.ExpenseClaimRefNo,
        //        i.CostAllocatedFromClaim,
        //        i.DuplicatedEntryNo,
        //        i.EmpCostAllocFromClaim,
        //        i.PropertyCostAllocFromClaim,
        //        i.PayrollAdditionId,
        //        i.PaymentStatus,
        //        i.FinalStatus,
        //        i.SupplierVoucherNo,
        //        i.JournalChildNo
        //    });

        //    // If underlying data is a large SQL table, specify PrimaryKey and PaginateViaPrimaryKey.
        //    // This can make SQL execution plans more efficient.
        //    // For more detailed information, please refer to this discussion: https://github.com/DevExpress/DevExtreme.AspNet.Data/issues/336.
        //    // loadOptions.PrimaryKey = new[] { "VoucherEntryNo" };
        //    // loadOptions.PaginateViaPrimaryKey = true;

        //    return Json(await DataSourceLoader.LoadAsync(tbl201voucherentries, loadOptions));
        //}

        //[HttpPost]
        //public async Task<IActionResult> Post(string values) {
        //    var model = new Tbl201VoucherEntry();
        //    var valuesDict = JsonConvert.DeserializeObject<IDictionary>(values);
        //    PopulateModel(model, valuesDict);

        //    if(!TryValidateModel(model))
        //        return BadRequest(GetFullErrorMessage(ModelState));

        //    var result = _context.Tbl201VoucherEntries.Add(model);
        //    await _context.SaveChangesAsync();

        //    return Json(new { result.Entity.VoucherEntryNo });
        //}

        //[HttpPut]
        //public async Task<IActionResult> Put(long key, string values) {
        //    var model = await _context.Tbl201VoucherEntries.FirstOrDefaultAsync(item => item.VoucherEntryNo == key);
        //    if(model == null)
        //        return StatusCode(409, "Object not found");

        //    var valuesDict = JsonConvert.DeserializeObject<IDictionary>(values);
        //    PopulateModel(model, valuesDict);

        //    if(!TryValidateModel(model))
        //        return BadRequest(GetFullErrorMessage(ModelState));

        //    await _context.SaveChangesAsync();
        //    return Ok();
        //}

        //[HttpDelete]
        //public async Task Delete(long key) {
        //    var model = await _context.Tbl201VoucherEntries.FirstOrDefaultAsync(item => item.VoucherEntryNo == key);

        //    _context.Tbl201VoucherEntries.Remove(model);
        //    await _context.SaveChangesAsync();
        //}


        //private void PopulateModel(Tbl201VoucherEntry model, IDictionary values) {
        //    string VOUCHER_ENTRY_NO = nameof(Tbl201VoucherEntry.VoucherEntryNo);
        //    string VOUCHER_NO = nameof(Tbl201VoucherEntry.VoucherNo);
        //    string ACCOUNT_HEAD = nameof(Tbl201VoucherEntry.AccountHead);
        //    string VOUCHER_AMOUNT = nameof(Tbl201VoucherEntry.VoucherAmount);
        //    string DR_CR = nameof(Tbl201VoucherEntry.DrCr);
        //    string ADDED_BY = nameof(Tbl201VoucherEntry.AddedBy);
        //    string ADDED_ON = nameof(Tbl201VoucherEntry.AddedOn);
        //    string ENTRY_NARRATION = nameof(Tbl201VoucherEntry.EntryNarration);
        //    string SYS_REMARKS = nameof(Tbl201VoucherEntry.SysRemarks);
        //    string BANK_CLEARED_ON = nameof(Tbl201VoucherEntry.BankClearedOn);
        //    string RECONCILIATION_REMARKS = nameof(Tbl201VoucherEntry.ReconciliationRemarks);
        //    string EXPENSE_CLAIM_REF_NO = nameof(Tbl201VoucherEntry.ExpenseClaimRefNo);
        //    string COST_ALLOCATED_FROM_CLAIM = nameof(Tbl201VoucherEntry.CostAllocatedFromClaim);
        //    string DUPLICATED_ENTRY_NO = nameof(Tbl201VoucherEntry.DuplicatedEntryNo);
        //    string EMP_COST_ALLOC_FROM_CLAIM = nameof(Tbl201VoucherEntry.EmpCostAllocFromClaim);
        //    string PROPERTY_COST_ALLOC_FROM_CLAIM = nameof(Tbl201VoucherEntry.PropertyCostAllocFromClaim);
        //    string PAYROLL_ADDITION_ID = nameof(Tbl201VoucherEntry.PayrollAdditionId);
        //    string PAYMENT_STATUS = nameof(Tbl201VoucherEntry.PaymentStatus);
        //    string FINAL_STATUS = nameof(Tbl201VoucherEntry.FinalStatus);
        //    string SUPPLIER_VOUCHER_NO = nameof(Tbl201VoucherEntry.SupplierVoucherNo);
        //    string JOURNAL_CHILD_NO = nameof(Tbl201VoucherEntry.JournalChildNo);

        //    if(values.Contains(VOUCHER_ENTRY_NO)) {
        //        model.VoucherEntryNo = Convert.ToInt64(values[VOUCHER_ENTRY_NO]);
        //    }

        //    if(values.Contains(VOUCHER_NO)) {
        //        model.VoucherNo = Convert.ToString(values[VOUCHER_NO]);
        //    }

        //    if(values.Contains(ACCOUNT_HEAD)) {
        //        model.AccountHead = Convert.ToString(values[ACCOUNT_HEAD]);
        //    }

        //    if(values.Contains(VOUCHER_AMOUNT)) {
        //        model.VoucherAmount = values[VOUCHER_AMOUNT] != null ? Convert.ToDecimal(values[VOUCHER_AMOUNT], CultureInfo.InvariantCulture) : (decimal?)null;
        //    }

        //    if(values.Contains(DR_CR)) {
        //        model.DrCr = Convert.ToString(values[DR_CR]);
        //    }

        //    if(values.Contains(ADDED_BY)) {
        //        model.AddedBy = Convert.ToString(values[ADDED_BY]);
        //    }

        //    if(values.Contains(ADDED_ON)) {
        //        model.AddedOn = values[ADDED_ON] != null ? Convert.ToDateTime(values[ADDED_ON]) : (DateTime?)null;
        //    }

        //    if(values.Contains(ENTRY_NARRATION)) {
        //        model.EntryNarration = Convert.ToString(values[ENTRY_NARRATION]);
        //    }

        //    if(values.Contains(SYS_REMARKS)) {
        //        model.SysRemarks = Convert.ToString(values[SYS_REMARKS]);
        //    }

        //    if(values.Contains(BANK_CLEARED_ON)) {
        //        model.BankClearedOn = values[BANK_CLEARED_ON] != null ? Convert.ToDateTime(values[BANK_CLEARED_ON]) : (DateTime?)null;
        //    }

        //    if(values.Contains(RECONCILIATION_REMARKS)) {
        //        model.ReconciliationRemarks = Convert.ToString(values[RECONCILIATION_REMARKS]);
        //    }

        //    if(values.Contains(EXPENSE_CLAIM_REF_NO)) {
        //        model.ExpenseClaimRefNo = Convert.ToString(values[EXPENSE_CLAIM_REF_NO]);
        //    }

        //    if(values.Contains(COST_ALLOCATED_FROM_CLAIM)) {
        //        model.CostAllocatedFromClaim = Convert.ToString(values[COST_ALLOCATED_FROM_CLAIM]);
        //    }

        //    if(values.Contains(DUPLICATED_ENTRY_NO)) {
        //        model.DuplicatedEntryNo = values[DUPLICATED_ENTRY_NO] != null ? Convert.ToInt64(values[DUPLICATED_ENTRY_NO]) : (long?)null;
        //    }

        //    if(values.Contains(EMP_COST_ALLOC_FROM_CLAIM)) {
        //        model.EmpCostAllocFromClaim = Convert.ToString(values[EMP_COST_ALLOC_FROM_CLAIM]);
        //    }

        //    if(values.Contains(PROPERTY_COST_ALLOC_FROM_CLAIM)) {
        //        model.PropertyCostAllocFromClaim = Convert.ToString(values[PROPERTY_COST_ALLOC_FROM_CLAIM]);
        //    }

        //    if(values.Contains(PAYROLL_ADDITION_ID)) {
        //        model.PayrollAdditionId = values[PAYROLL_ADDITION_ID] != null ? Convert.ToByte(values[PAYROLL_ADDITION_ID]) : (byte?)null;
        //    }

        //    if(values.Contains(PAYMENT_STATUS)) {
        //        model.PaymentStatus = Convert.ToString(values[PAYMENT_STATUS]);
        //    }

        //    if(values.Contains(FINAL_STATUS)) {
        //        model.FinalStatus = Convert.ToString(values[FINAL_STATUS]);
        //    }

        //    if(values.Contains(SUPPLIER_VOUCHER_NO)) {
        //        model.SupplierVoucherNo = Convert.ToString(values[SUPPLIER_VOUCHER_NO]);
        //    }

        //    if(values.Contains(JOURNAL_CHILD_NO)) {
        //        model.JournalChildNo = values[JOURNAL_CHILD_NO] != null ? Convert.ToInt64(values[JOURNAL_CHILD_NO]) : (long?)null;
        //    }
        //}

        //private string GetFullErrorMessage(ModelStateDictionary modelState) {
        //    var messages = new List<string>();

        //    foreach(var entry in modelState) {
        //        foreach(var error in entry.Value.Errors)
        //            messages.Add(error.ErrorMessage);
        //    }

        //    return String.Join(" ", messages);
        //}
        [HttpGet]
        public IActionResult GetExpenseClaims()
        {
            try
            {
                
                var data = _context.Qry20121ExpenseClaimForms.Select(e => new
                {
               
                    e.ClaimRefNo,
                    ClaimDate = e.ClaimDate.HasValue
                    ? e.ClaimDate.Value.ToString("dd-MMM-yyyy")
                    : string.Empty,
                    e.ClaimerName,
                    e.PaymentVoucherNo,
                    BillDate = e.BillDate.HasValue
                    ? e.BillDate.Value.ToString("dd-MMM-yyyy")
                    : string.Empty,
                    e.BillRefNo,
                    e.ExpenseDescription,
                    e.ClaimedAmount,
                    e.ApprovedAmount,
                    e.CostCenterCode,
                    e.AccountHead,
                    e.IsTaxIncluded,
                    e.Discount,
                    e.TaxAmount,
                    e.RoundOff,
                    e.SupplierName,
                    e.SupplierVatno,
                    e.EmployeeNo,
                    e.EmployeeName,
                    e.PropertyNo,
                    e.PropertyDescription,
                    e.CostAllocationUnit,
                    e.PurchaserName
                }).ToList();

                return Json(data);
            }
            catch (Exception ex)
            {
              
                return BadRequest(new { message = "An error occurred while fetching data.", error = ex.Message });
            }
        }
        [HttpGet]
        public IActionResult GetExpenseClaim(DateTime? startDate, DateTime? endDate)
        {
            var claims = _context.Qry20121ExpenseClaimForms.AsQueryable();

            if (startDate.HasValue && endDate.HasValue)
            {
                claims = claims.Where(c => c.ClaimDate >= startDate && c.ClaimDate <= endDate);
            }

            // Project to an object with all required fields
            var result = claims.Select(e => new
            {
                e.ClaimRefNo,
                ClaimDate = e.ClaimDate.HasValue
                    ? e.ClaimDate.Value.ToString("dd-MMM-yyyy")
                    : string.Empty,
                e.ClaimerName,
                e.PaymentVoucherNo,
                BillDate = e.BillDate.HasValue
                    ? e.BillDate.Value.ToString("dd-MMM-yyyy")
                    : string.Empty,
                e.BillRefNo,
                e.ExpenseDescription,
                e.ClaimedAmount,
                e.ApprovedAmount,
                e.CostCenterCode,
                e.AccountHead,
                e.IsTaxIncluded,
                e.Discount,
                e.TaxAmount,
                e.RoundOff,
                e.SupplierName,
                e.SupplierVatno,
                e.EmployeeNo,
                e.EmployeeName,
                e.PropertyNo,
                e.PropertyDescription,
                e.CostAllocationUnit,
                e.PurchaserName
            }).ToList();

            return Ok(result);
        }

    }
}