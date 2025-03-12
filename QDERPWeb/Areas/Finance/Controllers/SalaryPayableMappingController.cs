using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SalaryPayableMappingController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<SalaryPayableMappingController> _logger;

        public SalaryPayableMappingController(ILogger<SalaryPayableMappingController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
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
        public IActionResult GetSalaryMappings(DateTime? startDate, DateTime? endDate)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var claims = dbContext.Qry20191SalaryPayableMasterMapped02s.AsQueryable();

                if (startDate.HasValue && endDate.HasValue)
                {
                    claims = claims.Where(c => c.VoucherDate >= startDate && c.VoucherDate <= endDate);
                }

                var result = claims.Select(e => new
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

                return Ok(result);
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        public IActionResult Depreciation()
        {
            return PartialView("Depreciation");
        }

        [HttpGet]
        public async Task<IActionResult> GetLedgerMapping(DataSourceLoadOptions loadOptions, string accid)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var qryListOfAccountlists = dbContext.Qry201114accountLedgersWtAdvances
                    .Where(i => i.AccountNoInVoucher == accid)
                    .Select(i => new
                    {
                        i.AccountNoInVoucher,
                        i.AccountHead,
                        i.VoucherNoInVoucher,
                        i.BalanceInVoucher,
                        i.AmountInVoucherFormatted,
                        i.AmountInSubLedgerFormatted,
                        i.Mapping
                    });

                return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public IActionResult GetBankReconciliation(string accid)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    if (string.IsNullOrEmpty(accid))
                    {
                        return BadRequest("Account ID is required.");
                    }

                    var query = from t1 in dbContext.Tbl201VoucherEntries
                                join t2 in dbContext.Tbl201VoucherMasters
                                on t1.VoucherNo equals t2.VoucherNo
                                where t1.AccountHead == accid
                                && (t1.BankClearedOn == null)
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

                   // var result = await DataSourceLoader.LoadAsync(query.AsQueryable(), loadOptions);

                    return Json(query);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetSalaryMapping: {ex.Message}");
                    return BadRequest(new { message = "An error occurred while fetching data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }


      

        [HttpPost]
        public IActionResult UpdateBankClearedOn(string VoucherEntryNo, DateTime BankClearedOn)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                if (string.IsNullOrEmpty(VoucherEntryNo) || !int.TryParse(VoucherEntryNo, out int voucherEntryNoParsed) || voucherEntryNoParsed <= 0)
                {
                    return BadRequest("Invalid Voucher Entry No.");
                }

                if (BankClearedOn == default(DateTime))
                {
                    return BadRequest("Invalid BankClearedOn date.");
                }

                try
                {
                    var voucher = dbContext.Tbl201VoucherEntries.FirstOrDefault(v => v.VoucherEntryNo == voucherEntryNoParsed);
                    if (voucher == null)
                    {
                        return NotFound("Voucher not found.");
                    }

                    voucher.BankClearedOn = BankClearedOn;
                    dbContext.SaveChanges();

                    return Ok("BankClearedOn updated successfully.");
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
    }
}











