using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using Microsoft.EntityFrameworkCore;

namespace QDWEB.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class VoucherJournalController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<VoucherJournalController> _logger;

        public VoucherJournalController(ILogger<VoucherJournalController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult> GetNewVoucherNo()
        {
            string voucherPrefix = "JV-NEW-";
            string strNewVoucherNo;

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    using (var transaction = await dbContext.Database.BeginTransactionAsync())
                    {
                        string sql = @"
                            SELECT MAX(CAST(RIGHT(TempVoucherNo, 6) AS INT)) AS MaxVoucherNo
                            FROM tbl201VoucherMasterTemp WITH (TABLOCKX)
                            WHERE TempVoucherNo LIKE {0}";

                        var result = await dbContext.VoucherResults
                            .FromSqlRaw(sql, voucherPrefix + "%")
                            .ToListAsync();

                        int maxVoucherNo = result.FirstOrDefault()?.MaxVoucherNo ?? 0;
                        int newVoucherNo = maxVoucherNo + 1;
                        strNewVoucherNo = voucherPrefix + newVoucherNo.ToString("D6");

                        var newVoucherEntry = new Tbl201VoucherMasterTemp
                        {
                            TempVoucherNo = strNewVoucherNo
                        };

                        dbContext.Tbl201VoucherMasterTemps.Add(newVoucherEntry);
                        await dbContext.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetNewVoucherNo: {ex.Message}");
                    return Json(new { success = false, error = ex.Message });
                }

                return Json(strNewVoucherNo);
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public IActionResult Delete(long VoucherEntryNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var item = dbContext.Tbl201VoucherEntryTemps.FirstOrDefault(p => p.VoucherEntryNo == VoucherEntryNo);
                if (item != null)
                {
                    dbContext.Tbl201VoucherEntryTemps.Remove(item);
                    dbContext.SaveChanges();
                    return Ok(new { success = true });
                }
                return NotFound();
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<ActionResult> GetSupplierName(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var qryListOfAccountlists = dbContext.Qry201ListOfAccounts
                    .Where(p => p.AccountGroupId == "A012" || p.AccountGroupId == "A003")
                    .Select(i => new
                    {
                        i.MasterGroupId,
                        i.MasterGroup,
                        i.AccountGroup,
                        i.AccountGroupId,
                        i.AccountId,
                        i.AccountHead,
                        i.AccountHeadArabic,
                        i.ReferenceNo,
                        i.IsLedgerObselete
                    });

                return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<ActionResult> GetVoucherEntries(DataSourceLoadOptions loadOptions, string voucherNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var qryListOfAccountlists = dbContext.Tbl201VoucherEntryTemps
                    .Where(p => p.VoucherNo == voucherNo)
                    .Select(i => new
                    {
                        i.VoucherNo,
                        i.DrCr,
                        i.VoucherAmount,
                        i.EntryNarration,
                        i.AccountHead,
                        i.SysRemarks,
                    });

                return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public async Task<ActionResult> AddVoucherEntry(DataSourceLoadOptions loadOptions, [FromBody] Tbl201VoucherEntryTemp VE)
        {
            if (VE == null)
            {
                return BadRequest(new { success = false, message = "Invalid data received." });
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    dbContext.Tbl201VoucherEntryTemps.Add(VE);
                    await dbContext.SaveChangesAsync();

                    var qryListOfAccountlists = dbContext.Tbl201VoucherEntryTemps
                        .Where(p => p.VoucherNo == VE.VoucherNo)
                        .Select(i => new
                        {
                            i.VoucherNo,
                            i.VoucherEntryNo,
                            i.DrCr,
                            i.VoucherAmount,
                            i.EntryNarration,
                            i.AccountHead
                        });

                    return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in AddVoucherEntry: {ex.Message}");
                    return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public async Task<ActionResult> SaveVoucher([FromBody] Tbl201VoucherEntry VM)
        {
            if (VM == null)
            {
                return BadRequest(new { success = false, message = "Invalid data received." });
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    using (var transaction = await dbContext.Database.BeginTransactionAsync())
                    {
                        string currentDate = DateTime.Now.ToString("dd");
                        string currentMonth = DateTime.Now.ToString("MM");

                        var lastVoucher = await dbContext.Tbl201VoucherEntries
                            .Where(v => v.VoucherNo.StartsWith($"JV-{currentDate}-{currentMonth}-"))
                            .OrderByDescending(v => v.VoucherNo)
                            .FirstOrDefaultAsync();

                        int nextSequence = 1;

                        if (lastVoucher != null)
                        {
                            string lastNumberPart = lastVoucher.VoucherNo.Substring(9);
                            if (int.TryParse(lastNumberPart, out int lastNumber))
                            {
                                nextSequence = lastNumber + 1;
                            }
                        }

                        string newVoucherNo = $"JV-{currentDate}-{currentMonth}-{nextSequence:D3}";

                        while (await dbContext.Tbl201VoucherEntries.AnyAsync(v => v.VoucherNo == newVoucherNo))
                        {
                            nextSequence++;
                            newVoucherNo = $"JV-{currentDate}-{currentMonth}-{nextSequence:D3}";
                        }

                        VM.VoucherNo = newVoucherNo;

                        dbContext.Tbl201VoucherEntries.Add(VM);
                        await dbContext.SaveChangesAsync();
                        await transaction.CommitAsync();

                        return Ok(new { success = true, message = "Voucher saved successfully!", voucherNo = VM.VoucherNo });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in SaveVoucher: {ex.Message}");
                    return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
    }
}
