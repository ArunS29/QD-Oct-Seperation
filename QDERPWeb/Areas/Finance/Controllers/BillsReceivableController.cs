using System;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Spreadsheet;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using QD.ERP.Web.Services.Logging;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BillsReceivableController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<BillsReceivableController> _logger;
        private readonly IUserActionLogger _userActionLogger;
        public BillsReceivableController(ILogger<BillsReceivableController> logger, IUserActionLogger userActionLogger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
            _userActionLogger = userActionLogger;
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
                    var query = dbContext.Qry20105BillsReceivableAgeingViews.Select(i => new
                    {
                        i.AccountHeadNo,
                        i.AccountHead,
                        i.ReferenceNo,
                        i.VoucherRefNo,
                        i.VoucherDate,
                        i.InvoiceDueDate,
                        i.ReceivableAmount,
                        i.Received,
                        i.Balance,
                        i.NotOverdue,
                        i.Less30,
                        i.Less30to60,
                        i.Less60to90,
                        i.Less90to180,
                        i.Less180to365,
                        i.More365,
                        i.OverdueDays,
                        i.ConvertedReceivableAmount,
                        i.ConvertedReceived,
                        i.ConvertedBalance,
                        company.CurrencyImage

                    });

                    if (filterType == "WithBalance")
                    {
                        query = query.Where(i => i.Balance > 0);
                    }
                    else if (filterType == "FullyReceived")
                    {
                        query = query.Where(i => i.Balance <= 0);
                    }

                    var result = await DataSourceLoader.LoadAsync(query, loadOptions);
                    return Json(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in Get: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while processing the request.", error = ex.Message });
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
                        i.VoucherRefNo,
                        i.VoucherDate,
                        i.InvoiceDueDate,
                        i.ReceivableAmount,
                        i.Received,
                        i.Balance,
                        //i.NotOverdue,
                        //i.Less30,
                        //i.Less30to60,
                        //i.Less60to90,
                        //i.Less90to180,
                        //i.Less180to365,
                        //i.More365,
                        i.OverdueDays,
                        i.ConvertedReceivableAmount,
                        i.ConvertedReceived,
                        i.ConvertedBalance,
                        company.CurrencyImage

                    });

                    if (filterType == "WithBalance")
                    {
                        query = query.Where(i => i.Balance > 0);
                    }
                    else if (filterType == "FullyReceived")
                    {
                        query = query.Where(i => i.Balance <= 0);
                    }

                    var result = await DataSourceLoader.LoadAsync(query, loadOptions);
                    return Json(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in Get: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while processing the request.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public IActionResult GenerateReport()
        {
            return RedirectToPage("/pulse/DocumentViewer", new { reportName = "XtraReportBillsReceivableAgeingReport" });
        }

        [HttpGet]
        public IActionResult GenerateAgeingreportsummaryReport()
        {
            return RedirectToPage("/pulse/DocumentViewer", new { reportName = "XtraReportAgeingreportsummary" });
        }

        [HttpGet]
        public IActionResult GetSubLedgerReceivables(string voucherType, string accountId, bool isPayable, int CurrencyId)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { success = false, message = "Invalid tenant." });

            if (isPayable)
            {
                var payables = dbContext.Qry201SubLedgerPayablesMasters
                    .Where(x => x.VoucherType == voucherType && x.AccountHeadNo == accountId && x.Currencyid == CurrencyId)
                    .Select(x => new
                    {
                        x.ReferenceNo,
                        ReceivableAmount = x.PayableAmount, // 👈 Alias to match DataField
                        Received = x.Paid,
                        x.Balance,
                        x.RetentionAmount,
                        x.BalanceDueWithOutRetention
                    }).ToList();

                return Json(payables);
            }
            else
            {
                var receivables = dbContext.Qry201SubLedgerReceivablesMasters
                    .Where(x => x.VoucherType == voucherType && x.AccountHeadNo == accountId && x.Currencyid == CurrencyId)
                    .Select(x => new
                    {
                        x.ReferenceNo,
                        x.ReceivableAmount,
                        x.Received,
                        x.Balance,
                        x.RetentionAmount,
                        x.BalanceDueWithOutRetention
                    }).ToList();

                return Json(receivables);
            }
        }

        [HttpGet]
        public IActionResult GetSubLedgerByVoucher(long voucherNo)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { success = false, message = "Invalid tenant." });

           

            var subLedgerData = dbContext.Tbl201SubLedgerMasters
                .Where(x => x.VoucherEntryNo == voucherNo)
                .ToList();

            return Json(subLedgerData);
        }
        [HttpPost]
        public IActionResult AddSubLedgerEntry([FromBody] SubLedgerDto model)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { success = false, message = "Invalid tenant." });

            if (model == null || model.VoucherEntryNo <= 0)
                return BadRequest(new { success = false, message = "Invalid data." });

            var newEntry = new Tbl201SubLedgerMaster
            {
                VoucherEntryNo = model.VoucherEntryNo,
                DrCr = model.DrCr,
                ReferenceType = model.ReferenceType,
                ReferenceNo = model.ReferenceNo,
                Amount = model.Amount,
                RetentionAmount = null,
                AccountNo = model.accountId,
                VoucherNo = model.VoucherNo,
            };

            dbContext.Tbl201SubLedgerMasters.Add(newEntry);
            dbContext.SaveChanges();

            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<IActionResult> UpdateSubLedgerEntriesAsync([FromBody] List<Tbl201SubLedgerMaster> entries)
        {
            // Resolve tenant-specific DB context
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            if (entries == null || !entries.Any())
            {
                return BadRequest(new { success = false, message = "No ledger entries were received." });
            }

            try
            {
                foreach (var updatedEntry in entries)
                {
                    var existingEntry = dbContext.Tbl201SubLedgerMasters
                        .FirstOrDefault(x => x.ReferenceNo == updatedEntry.ReferenceNo
                                          && x.SubLedgerId == updatedEntry.SubLedgerId);
                    await _userActionLogger.LogAsync(
              module: "Finance > Bills Receivable",
              actionDetail: $"Updated: {existingEntry.VoucherNo}",
              documentNo: existingEntry.VoucherNo
              );
                    if (existingEntry != null)
                    {
                        existingEntry.Amount = updatedEntry.Amount;
                        existingEntry.RetentionAmount = updatedEntry.RetentionAmount;
                        existingEntry.ReferenceType = updatedEntry.ReferenceType;
                        existingEntry.DrCr = updatedEntry.DrCr;
                        // Add more fields as necessary
                    }
                    else
                    {
                        // Optional: handle inserts
                        // dbContext.Tbl201SubLedgerMaster.Add(updatedEntry);
                    }
                }

                dbContext.SaveChanges();
              
                return Ok(new { success = true, message = "Ledger entries updated successfully." });
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while updating ledger entries.",
                    error = ex.Message
                });
            }
        }
        [HttpPost]
        public async Task<IActionResult> DeleteSubLedgerEntryAsync([FromBody] DeleteSubLedgerDto model)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { success = false, message = "Invalid tenant." });

            if (model == null || model.VoucherEntryNo <= 0)
                return BadRequest(new { success = false, message = "Invalid data." });

            var entry = dbContext.Tbl201SubLedgerMasters
                .FirstOrDefault(x => x.VoucherEntryNo == model.VoucherEntryNo && x.Amount == model.Amount);

            if (entry == null)
                return NotFound(new { success = false, message = "Entry not found." });

            dbContext.Tbl201SubLedgerMasters.Remove(entry);
            dbContext.SaveChanges();

            await _userActionLogger.LogAsync(
              module: "Finance > Bills Receivable",
              actionDetail: $"Deleted: {entry.VoucherNo}",
              documentNo: entry.VoucherNo
              );
            return Ok(new { success = true });
        }

        [HttpGet]
        public IActionResult GetAll(DataSourceLoadOptions loadOptions)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { success = false, message = "Invalid tenant." });

            try
            {
                var data = dbContext.Qry201SubLedgerReceivablesMasters.AsQueryable();
                return Json(DataSourceLoader.Load(data, loadOptions));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error occurred while loading data.", detail = ex.Message });
            }
        }

    }
}

