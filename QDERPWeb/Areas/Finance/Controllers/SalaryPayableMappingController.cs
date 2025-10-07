using DevExpress.CodeParser;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using QD.ERP.Web.Services.Logging;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SalaryPayableMappingController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<SalaryPayableMappingController> _logger;
        private readonly IUserActionLogger _userActionLogger;
        public SalaryPayableMappingController(ILogger<SalaryPayableMappingController> logger, IUserActionLogger userActionLogger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
            _userActionLogger = userActionLogger;
        }

        [HttpGet]
        public IActionResult GetSalaryMapping(string acchedid)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var query = dbContext.Qry20191SalaryPayableMasterMapped02s.AsQueryable();

                    if (!string.IsNullOrEmpty(acchedid))
                    {
                        query = query.Where(e => e.LedgerNo == acchedid);
                    }

                    var data = query.Select(e => new
                    {
                        e.VoucherNo,
                        VoucherDate = e.VoucherDate.HasValue
                            ? e.VoucherDate.Value.ToString("dd-MMM-yyyy")
                            : string.Empty,
                        e.EmployeeNo,
                        e.EmployeeName,
                        e.NationalId,
                        e.VoucherAmount,
                        e.TotalMappedAmount,
                        e.Mapping
                    }).ToList();

                    return Json(data);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetSalaryMapping: {ex.Message}");
                    return BadRequest(new { message = "An error occurred while fetching data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public IActionResult GetSalaryMappings(string accountId , DateTime? startDate, DateTime? endDate)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out _, out ERPMasterWtDataContext dbContext))
                    return Unauthorized(new { message = "Invalid tenant.", success = false });

                var claims = dbContext.Qry20191SalaryPayableMasterMapped02s.AsQueryable();

                if (startDate.HasValue && endDate.HasValue)
                    claims = claims.Where(c => c.VoucherDate >= startDate && c.VoucherDate <= endDate && c.LedgerNo == accountId);

                return Ok(claims.Select(e => new
                {
                    e.VoucherNo,
                    VoucherDate = e.VoucherDate.HasValue
      ? e.VoucherDate.Value.ToString("dd-MMM-yyyy")
      : string.Empty,
                    e.EmployeeNo,
                    e.EmployeeName,
                    e.NationalId,
                    e.VoucherAmount,
                    e.TotalMappedAmount,
                    e.Mapping
                }).ToList());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving salary mappings.", success = false, error = ex.Message });
            }
        }



        public IActionResult Depreciation()
        {
            return PartialView("Depreciation");
        }









        //[HttpGet]
        //public async Task<IActionResult> GetLedgerMapping(DataSourceLoadOptions loadOptions, string accid)
        //{
        //    if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
        //    {
        //        var qryListOfAccountlists = dbContext.Qry201114accountLedgersWtAdvances
        //            .Where(i => i.AccountNoInVoucher == accid)
        //            .Select(i => new
        //            {
        //                i.AccountNoInVoucher,
        //                i.AccountHead,
        //                i.VoucherNoInVoucher,
        //                i.BalanceInVoucher,
        //                i.AmountInVoucherFormatted,
        //                i.AmountInSubLedgerFormatted,
        //                i.Mapping
        //            });



        //        return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
        //    }

        //    return Unauthorized(new { message = "Invalid tenant.", success = false });
        //}

        [HttpGet]
        public IActionResult GetBankReconciliation(DataSourceLoadOptions loadOptions, string accid, DateTime? fromDate, DateTime? toDate)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }

            if (string.IsNullOrWhiteSpace(accid) || accid == "0")
            {
                return BadRequest("Valid Account ID is required.");
            }

            try
            {
                var query = from t1 in dbContext.Tbl201VoucherEntries
                            join t2 in dbContext.Tbl201VoucherMasters
                                on t1.VoucherNo equals t2.VoucherNo
                            where t1.AccountHead == accid
                                  && t1.BankClearedOn == null
                                  && t1.SysRemarks != "System Generated Opening Balance"
                                 
                            select new
                            {
                                t1.VoucherEntryNo,
                                t1.VoucherNo,
                                t2.VoucherRefNo,
                                t2.VoucherDate,
                                t1.SysRemarks,
                                t1.DrCr,
                                t1.BankClearedOn,
                                t1.PaymentStatus,
                                DrAmount = t1.DrCr == "Dr" ? t1.VoucherAmount : 0,
                                CrAmount = t1.DrCr == "Cr" ? t1.VoucherAmount : 0
                            };

                return Ok(DataSourceLoader.Load(query, loadOptions));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetBankReconciliation");
                return BadRequest(new { message = "An error occurred while fetching data.", error = ex.Message });
            }
        }


        [HttpGet]
        public IActionResult Getshowreconcileditems(DataSourceLoadOptions loadOptions, string accid, DateTime? fromDate, DateTime? toDate)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }

            if (string.IsNullOrWhiteSpace(accid) || accid == "0")
            {
                return BadRequest("Valid Account ID is required.");
            }

            try
            {
                var query = from t1 in dbContext.Tbl201VoucherEntries
                            join t2 in dbContext.Tbl201VoucherMasters
                                on t1.VoucherNo equals t2.VoucherNo
                            where t1.BankClearedOn != null
                                  && t1.AccountHead == accid
                                  && (!fromDate.HasValue || t2.VoucherDate >= fromDate.Value)
                                  && (!toDate.HasValue || t2.VoucherDate <= toDate.Value)
                            select new
                            {
                                t1.VoucherEntryNo,
                                t1.VoucherNo,
                                t2.VoucherRefNo,
                                t2.VoucherDate,
                                t1.SysRemarks,
                                t1.DrCr,
                                t1.BankClearedOn,
                                t1.PaymentStatus,
                                DrAmount = t1.DrCr == "Dr" ? t1.VoucherAmount : 0,
                                CrAmount = t1.DrCr == "Cr" ? t1.VoucherAmount : 0
                            };

                return Ok(DataSourceLoader.Load(query, loadOptions));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Getshowreconcileditems");
                return BadRequest(new { message = "An error occurred while fetching data.", error = ex.Message });
            }
        }



        [HttpPost]
        public async Task<IActionResult> UpdateBankReconcilation([FromBody] Tbl201VoucherEntry model)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var now = DateTime.Now;

                    // Find the existing record by VoucherEntryNo
                    var existing = await dbContext.Tbl201VoucherEntries
                        .FirstOrDefaultAsync(x => x.VoucherEntryNo == model.VoucherEntryNo);

                    if (existing != null)
                    {
                        // Update only the fields that are being modified
                        existing.PaymentStatus = model.PaymentStatus;

                        // If you want to update other fields, add them here
                        // existing.OtherField = model.OtherField;
                    }
                    else
                    {
                        // If no existing record found, return an error message
                        return NotFound(new { success = false, message = "Voucher entry not found." });
                    }

                    // Save the changes to the database
                    await dbContext.SaveChangesAsync();

                    // Return success response
                    return Ok(new { success = true, message = "Bank Reconciliation updated successfully." });
                }
                catch (Exception ex)
                {
                    // Log the error and return a server error response
                    _logger.LogError($"Error in SaveOrUpdateSignatory: {ex}");
                    return StatusCode(500, new { success = false, message = ex.Message });
                }
            }

            // Return Unauthorized response if tenant is invalid
            return Unauthorized(new { success = false, message = "Invalid tenant" });
        }


        [HttpPost]
        public async Task<IActionResult> UpdateBankClearedOnAsync(string VoucherEntryNo, DateTime? BankClearedOn, string PaymentStatus)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                if (string.IsNullOrEmpty(VoucherEntryNo) || !int.TryParse(VoucherEntryNo, out int voucherEntryNoParsed) || voucherEntryNoParsed <= 0)
                {
                    return BadRequest("Invalid Voucher Entry No.");
                }

                try
                {
                    var voucher = dbContext.Tbl201VoucherEntries.FirstOrDefault(v => v.VoucherEntryNo == voucherEntryNoParsed);
                    if (voucher == null)
                    {
                        return NotFound("Voucher not found.");
                    }

                    // If BankClearedOn is provided (not null), update it
                    if (BankClearedOn.HasValue && BankClearedOn.Value != default(DateTime))
                    {
                        voucher.BankClearedOn = BankClearedOn.Value;
                    }
                    // If BankClearedOn is not provided, update PaymentStatus instead
                    else if (!string.IsNullOrEmpty(PaymentStatus))
                    {
                        voucher.PaymentStatus = PaymentStatus;
                    }
                    else
                    {
                        return BadRequest("No valid update field provided.");
                    }

                    dbContext.SaveChanges();
                    await _userActionLogger.LogAsync(
                    module: "Finance > Bank Reconcialiation",
                    actionDetail: $"Updated: {voucher.VoucherNo}",
                    documentNo: voucher.VoucherNo
                    );

                    return Ok("Voucher entry updated successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in UpdateBankClearedOn: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateClearSelectedVoucher([FromQuery] List<long> VoucherEntryNo, [FromQuery] string BankClearedOn)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    if (VoucherEntryNo == null || !VoucherEntryNo.Any() || string.IsNullOrEmpty(BankClearedOn))
                    {
                        return BadRequest("VoucherEntryNo and BankClearedOn are required.");
                    }

                    if (!DateTime.TryParse(BankClearedOn, out DateTime bankClearedDate))
                    {
                        return BadRequest("Invalid BankClearedOn date format.");
                    }

                    var recordsToUpdate = dbContext.Tbl201VoucherEntries
                        .Where(spm => VoucherEntryNo.Contains(spm.VoucherEntryNo))
                        .ToList();

                    if (!recordsToUpdate.Any())
                    {
                        return NotFound("No records found for the provided VoucherEntryNo(s).");
                    }

                    foreach (var record in recordsToUpdate)
                    {
                        record.BankClearedOn = bankClearedDate;
                    }

                    await dbContext.SaveChangesAsync();

                    return Ok(new { message = "BankClearedOn updated successfully!" });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in UpdateClearSelectedVoucher: {ex.Message}");
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }




        /// <summary>
        /// Loads receivable/payable with advances data from Qry201114accountLedgersWtAdvances.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetLedgerMapping(DataSourceLoadOptions loadOptions, string accid, string drCrFilter = null)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var query = dbContext.Qry201114accountLedgersWtAdvances
                    .Where(x => x.AccountNoInVoucher == accid);

                // Apply DrCr filter if provided
                if (!string.IsNullOrEmpty(drCrFilter))
                {
                    query = query.Where(x => x.DrCr == drCrFilter);  // Filter by Dr or Cr
                }

                var data = query.Select(x => new
                {
                    x.AccountNoInVoucher,
                    x.AccountHead,
                    x.VoucherNoInVoucher,
                    x.AmountInVoucherFormatted,
                    x.AmountInSubLedgerFormatted,
                    x.BalanceInVoucher,
                    x.Mapping,
                    x.DrCr,
                    x.VoucherType
                });

                return Json(await DataSourceLoader.LoadAsync(data, loadOptions));
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }


        /// <summary>
        /// Loads subledger mapping data from qry20173SubledgersWtVoucherEntry.
        /// </summary>
        [HttpGet]
            public async Task<IActionResult> GetSubledgerMapping(DataSourceLoadOptions loadOptions, string accid)
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var query = dbContext.Qry20173SubledgersWtVoucherEntries
                        .Where(x => x.AccountNoInVoucher == accid)
                        .Select(x => new
                        {
                            x.VoucherNoInSubLedger,
                            x.VoucherNoInVoucher,
                            x.VoucherRefNo,
                            x.AccountNoInVoucher,
                            x.AccountNoInSubLedger,
                            x.ReferenceNoInSubLedger,
                            x.AmountInVoucherFormatted,
                            x.AmountInSubLedgerFormatted,
                            x.TotalAmount,
                            x.Mapping,
                            x.VoucherType

                        });

                    return Json(await DataSourceLoader.LoadAsync(query, loadOptions));
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
        [HttpGet]
        public async Task<IActionResult> GetCreditors(DataSourceLoadOptions loadOptions, string accid)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var query = from ledger in dbContext.Qry201114accountLedgersWtAdvances
                            join coa in dbContext.Tbl201ChartOfAccounts
                                on ledger.AccountNoInVoucher equals coa.AccountId
                            where ledger.AccountGroupId == "A003"
                            select new
                            {
                                ledger.VoucherNoInVoucher,
                                ledger.VoucherRefNo,
                                ledger.AccountNoInVoucher,
                                ledger.DrCr,
                                ledger.AmountInVoucherFormatted,
                                ledger.AmountInSubLedgerFormatted,
                                ledger.BalanceInVoucher,
                                ledger.Mapping,
                                ledger.VoucherType,
                                AccountHeadName = coa.AccountHead   // << add Account Head
                            };

                return Json(await DataSourceLoader.LoadAsync(query, loadOptions));
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetDebtors(DataSourceLoadOptions loadOptions, string accid)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var query = from ledger in dbContext.Qry201114accountLedgersWtAdvances
                            join coa in dbContext.Tbl201ChartOfAccounts
                                on ledger.AccountNoInVoucher equals coa.AccountId
                            where ledger.AccountGroupId == "A011"
                            select new
                            {
                                ledger.VoucherNoInVoucher,
                                ledger.VoucherRefNo,
                                ledger.AccountNoInVoucher,
                                ledger.DrCr,
                                ledger.AmountInVoucherFormatted,
                                ledger.AmountInSubLedgerFormatted,
                                ledger.BalanceInVoucher,
                                ledger.Mapping,
                                ledger.VoucherType,
                                AccountHeadName = coa.AccountHead   // << add Account Head
                            };

                return Json(await DataSourceLoader.LoadAsync(query, loadOptions));
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet]
        public IActionResult GetSubledgerMappings(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                // Just return all records from the table
                var query = dbContext.Qry20173SubledgersWtVoucherEntries.Select(x => new
                {
                    x.AccountNoInVoucher,
                    x.AccountHead,
                    x.VoucherNoInVoucher,
                    x.AmountInVoucher,
                    x.AmountInVoucherFormatted,
                    x.AmountInSubLedgerFormatted,
                    x.Mapping,

                });

                return Ok(DataSourceLoader.Load(query, loadOptions));
        }
        

                return Unauthorized(new { message = "Invalid tenant.", success = false });
    }



        [HttpGet]
        public async Task<IActionResult> GetBankBalances(string accid, DateTime toDate, CancellationToken ct)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return BadRequest("Tenant context could not be determined.");

            try
            {
                // 1️⃣ Get Account Balance
                var accountBalanceResult = await dbContext.AccountBalanceResults
                    .FromSqlRaw("EXEC sp20102GetAccountBalance @p0, @p1", accid, toDate)
                    .ToListAsync(ct);

                // 2️⃣ Get Bank Cleared Balance
                var bankClearedBalanceResult = await dbContext.BankClearedBalanceResults
                    .FromSqlRaw("EXEC sp20102GetBankClearedBalance @p0, @p1", accid, toDate)
                    .ToListAsync(ct);

                var accountBalance = accountBalanceResult.Sum(x => x.Amount);

                var bankClearedBalance = bankClearedBalanceResult.FirstOrDefault()?.Balance ?? 0;

                return Ok(new
                {
                    AccountBalance = accountBalance,
                    BankClearedBalance = bankClearedBalance
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching balances: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBankClearedOnBulk([FromBody] List<BankReconciliationUpdateDto> updates)
        {
            if (updates == null || updates.Count == 0)
                return BadRequest("No data received for update.");

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { message = "Invalid tenant.", success = false });

            try
            {
                var entryNos = updates.Select(u => u.VoucherEntryNo).ToList();
                var vouchers = dbContext.Tbl201VoucherEntries
                                        .Where(v => entryNos.Contains(v.VoucherEntryNo))
                                        .ToList();

                foreach (var update in updates)
                {
                    var voucher = vouchers.FirstOrDefault(v => v.VoucherEntryNo == update.VoucherEntryNo);
                    if (voucher == null)
                        continue;

                    // Directly update row values (null allowed)
                    voucher.BankClearedOn = update.BankClearedOn;
                    voucher.PaymentStatus = update.PaymentStatus;
                }

                await dbContext.SaveChangesAsync();

                await _userActionLogger.LogAsync(
                    module: "Finance > Bank Reconciliation",
                    actionDetail: $"Bulk updated {updates.Count} vouchers",
                    documentNo: string.Join(",", updates.Select(x => x.VoucherEntryNo))
                );

                return Ok(new { message = "All rows updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateBankClearedOnBulk");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }







    }
}











