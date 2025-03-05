using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TransactionDetailsController : ControllerBase
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<TransactionDetailsController> _logger;

        public TransactionDetailsController(ILogger<TransactionDetailsController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetVoucherForAudits()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var result = await dbContext.Qry20176VouchersForAudits.ToListAsync();

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

                    return Ok(pivotGridData);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetVoucherForAudits: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
    }
}









