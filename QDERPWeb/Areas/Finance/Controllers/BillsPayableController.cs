using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
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
    public class BillsPayableController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<BillsPayableController> _logger;

        public BillsPayableController(ILogger<BillsPayableController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions, string filterType = null)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var company = dbContext.Tbl901CompanyDetails
                   .FirstOrDefault();
                    var query = dbContext.Qry201SubLedgerPayablesMasters.Select(i => new
                    {
                        i.AccountHeadNo,
                        i.AccountHead,
                        i.ReferenceNo,
                        i.VoucherDate,
                        i.VoucherRefNo,
                        i.InvoiceAmountBeforeRetention,
                        i.Paid,
                        i.Balance,
                        i.InvoiceDueDate,
                        i.NoOfDaysCreditPeriod,
                        i.OverdueDays,
                        i.PayableAmount,
                       
                        i.VoucherNarration,
                        i.AccountGroup,
                        
                        i.VoucherEffectiveDate,
                        i.InvoiceSubmittedDate,
                        i.VoucherType,
                        i.BankAccountNo,
                        i.BankIban,
                        i.BankName,
                        i.BankBranch,
                        i.LedgerRemarks,
                        i.AccountHeadArabic,
                        i.AccountGroupId,
                        i.AccountBranch,
                        i.SalesPersonCode,
                        i.BranchName,
                        i.CostCenterCode,
                        i.CostAllocationUnit,
                        i.CostAllocationGroup,
                        i.CostAllocationMasterGroup,
                        i.AccountSubGroup,
                        i.SubGroupName,
                        i.BankAccountName,
                        i.RetentionAmount,
                      
                        i.TotalPayableAmount,
                        i.BalanceDueWithOutRetention,
                        i.RetentionPayable,
                        i.BalanceDueWithRetention,
                        i.ReferenceNote,
                        i.PurchaseBillNo,
                        i.PurchaseBillDate,
                        i.PurchaseOrderNo,
                        i.MaterialReceiptNo,
                        i.Expr1,
                        i.ConvertedInvoiceAmountBeforeRetention,
                        i.ConvertedBalance,
                        i.ConvertedPaid,
                        company.CurrencyImage

                    });

                    if (filterType == "WithBalance")
                    {
                        query = query.Where(i => i.Balance != 0);
                    }
                    else if (filterType == "FullyPaid")
                    {
                        query = query.Where(i => i.Balance == 0);
                    }

                    var result = await DataSourceLoader.LoadAsync(query, loadOptions);
                    return Json(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in Get: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while processing your request.", details = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet]
        public async Task<IActionResult> Getoffline(DataSourceLoadOptions loadOptions, string filterType = null)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var company = dbContext.Tbl901CompanyDetails
                   .FirstOrDefault();
                    var query = dbContext.Tbl201SubLedgerReceivablesMaster.Select(i => new
                    {
                        i.AccountHeadNo,
                        i.AccountHead,
                        i.ReferenceNo,
                        i.VoucherDate,
                        i.VoucherRefNo,
                        i.InvoiceAmountBeforeRetention,
                        //i.Paid,
                        i.Balance,
                        i.InvoiceDueDate,
                        i.NoOfDaysCreditPeriod,
                        i.OverdueDays,
                        //i.PayableAmount,

                        i.VoucherNarration,
                        i.AccountGroup,

                        i.VoucherEffectiveDate,
                       // i.InvoiceSubmittedDate,
                        i.VoucherType,
                        //i.BankAccountNo,
                        //i.BankIban,
                        //i.BankName,
                        //i.BankBranch,
                        //i.LedgerRemarks,
                        //i.AccountHeadArabic,
                        //i.AccountGroupId,
                        i.AccountBranch,
                        i.SalesPersonCode,
                        i.BranchName,
                       // i.CostCenterCode,
                        i.CostAllocationUnit,
                        i.CostAllocationGroup,
                        i.CostAllocationMasterGroup,
                       // i.AccountSubGroup,
                        i.SubGroupName,
                       // i.BankAccountName,
                        i.RetentionAmount,

                        //i.TotalPayableAmount,
                        //i.BalanceDueWithOutRetention,
                        //i.RetentionPayable,
                        //i.BalanceDueWithRetention,
                        //i.ReferenceNote,
                        //i.PurchaseBillNo,
                        //i.PurchaseBillDate,
                        //i.PurchaseOrderNo,
                        //i.MaterialReceiptNo,
                        //i.Expr1,
                        //i.ConvertedInvoiceAmountBeforeRetention,
                        //i.ConvertedBalance,
                        //i.ConvertedPaid,
                        company.CurrencyImage

                    });

                    if (filterType == "WithBalance")
                    {
                        query = query.Where(i => i.Balance != 0);
                    }
                    else if (filterType == "FullyPaid")
                    {
                        query = query.Where(i => i.Balance == 0);
                    }

                    var result = await DataSourceLoader.LoadAsync(query, loadOptions);
                    return Json(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in Get: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while processing your request.", details = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetBillDetails(string accountHeadNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var billDetails = await dbContext.Qry201SubLedgerPayablesMasters
                        .Where(b => b.AccountHeadNo == accountHeadNo)
                        .Select(b => new
                        {
                            b.AccountHeadNo,
                            b.AccountHead,
                            b.ReferenceNo,
                            b.VoucherDate,
                            b.VoucherRefNo,
                            b.InvoiceAmountBeforeRetention,
                            b.Paid,
                            b.Balance,
                            b.InvoiceDueDate,
                            b.NoOfDaysCreditPeriod,
                            b.OverdueDays,
                        })
                        .ToListAsync();

                    if (billDetails == null || !billDetails.Any())
                    {
                        return NotFound(new { message = "Bill details not found.", success = false });
                    }

                    return Ok(new { success = true, data = billDetails });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetBillDetails: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching bill details.", details = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBill([FromBody] Qry201SubLedgerPayablesMaster updatedBill)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    if (updatedBill == null)
                    {
                        return BadRequest(new { message = "Invalid bill data." });
                    }

                    var existingBill = await dbContext.Qry201SubLedgerPayablesMasters
                        .FirstOrDefaultAsync(b => b.AccountHeadNo == updatedBill.AccountHeadNo);

                    if (existingBill == null)
                    {
                        return NotFound(new { message = "Bill not found." });
                    }

                    existingBill.AccountHead = updatedBill.AccountHead;
                    existingBill.ReferenceNo = updatedBill.ReferenceNo;
                    existingBill.VoucherDate = updatedBill.VoucherDate;
                    existingBill.VoucherRefNo = updatedBill.VoucherRefNo;
                    existingBill.InvoiceAmountBeforeRetention = updatedBill.InvoiceAmountBeforeRetention;
                    existingBill.Paid = updatedBill.Paid;
                    existingBill.Balance = updatedBill.Balance;
                    existingBill.InvoiceDueDate = updatedBill.InvoiceDueDate;
                    existingBill.NoOfDaysCreditPeriod = updatedBill.NoOfDaysCreditPeriod;
                    existingBill.OverdueDays = updatedBill.OverdueDays;

                    await dbContext.SaveChangesAsync();

                    return Ok(new { message = "Bill updated successfully." });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in UpdateBill: {ex.Message}");
                    return BadRequest(new { message = "An error occurred while updating the bill.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBill(string accountHeadNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var bill = await dbContext.Qry201SubLedgerPayablesMasters
                        .FirstOrDefaultAsync(b => b.AccountHeadNo == accountHeadNo);

                    if (bill == null)
                    {
                        return NotFound(new { message = "Bill not found." });
                    }

                    dbContext.Qry201SubLedgerPayablesMasters.Remove(bill);
                    await dbContext.SaveChangesAsync();

                    return Ok(new { message = "Bill deleted successfully." });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in DeleteBill: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while deleting the bill.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
    }
}
