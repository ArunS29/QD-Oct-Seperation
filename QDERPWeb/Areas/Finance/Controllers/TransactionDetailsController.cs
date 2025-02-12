
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.DAL.Entities;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TransactionDetailsController : ControllerBase
    {
        private readonly ERPMasterWtDataContext _context;

        // Inject the ERPMasterWtDataContext in the constructor
        public TransactionDetailsController(ERPMasterWtDataContext context)
        {
            _context = context;
        }


        // Action to get the trial balance data
        

        public async Task<IActionResult> GetVoucherForAudits()
        {
            try
            {
                // Fetch all records from the Qry20513cashFlowOpeningAndTransCombineds table
                var result = await _context.Qry20176VouchersForAudits.ToListAsync();

                // Map the results to the desired format for the PivotGrid
                var pivotGridData = result.Select(item => new
                {
                    item.VoucherNo,
                    item.AccountHeadName,
                    item.DrCr,
                    item.CrAmount,
                    item.DrAmount,
                    item.VoucherDate,
                    item.VoucherRefNo,
                    item.VoucherNarration,
                    item.VoucherEnteredBy,
                    item.VoucherEnteredOn,
                    item.VoucherVerifiedBy,
                    item.VoucherVerifiedOn,
                    item.VoucherApprovedBy,
                    item.VoucherApprovedOn,
                    item.VoucherEntryNo,
                    item.AccountHead,
                    item.VoucherAmountFormatted,
                    item.EntryNarration,
                    item.AccountGroup,
                    item.MasterGroup,
                    item.IsApproved,
                    item.VoucherType,
                    item.SysRemarks,
                    item.IsCalculateOpeningBalance,
                    item.BankClearedOn,
                    item.AccountGroupId,
                    item.MasterGroupId,
                    item.IsProfitLossAccount,
                    item.IsBalanceSheetAccount,
                    item.MasterOrderNo,
                    item.MasterGroupCategory,
                    item.VoucherEffectiveDate,
                    item.BillNo,
                    item.BillDate,
                    item.BillPaidTo,
                    item.BillRemarks,
                    item.VoucherModifiedBy,
                    item.AccountHeadArabic,
                    item.AccountGroupAr,
                    item.MasterGroupAr,
                    item.VoucherTypeAr,
                    item.ChartOfAccountsOrder,
                    item.BankTransaction,
                    item.BankTransactionCr,
                    item.BankDrTransaction,
                    item.BankCrTransaction,
                    item.CashCrTransaction,
                    item.CashDrTransaction,
                    item.CashTransactionCr,
                    item.CashTransactionDr,
                    item.PettyCashLedgerName



                }).ToList();

                // Return the data as JSON
                return Ok(pivotGridData);
            }
            catch (Exception ex)
            {
                // Log the exception and return an error message
                return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
            }
        }
    }
}

