using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PurchaseVoucherController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<PurchaseVoucherController> _logger;

        public PurchaseVoucherController(ILogger<PurchaseVoucherController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult> GetNewVoucherNo(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                DateTime currentDate = DateTime.Now;
                string currentYear = currentDate.Year.ToString();
                string currentMonth = currentDate.Month.ToString("00");
                string voucherString = "PUR-" + currentYear.Substring(currentYear.Length - 2, 2) + "-" + currentMonth + "-";

                string strNewReceiptNo;
                string likePattern = voucherString + "%";

                try
                {
                    using (var transaction = await dbContext.Database.BeginTransactionAsync())
                    {
                        var result = await dbContext.VoucherResults
                            .FromSqlInterpolated($@"
                            SELECT MAX(CAST(RIGHT(VoucherNo, 3) AS INT)) AS MaxVoucherNo
                            FROM Tbl201VoucherEntry
                            WHERE VoucherNo LIKE {likePattern}")
                            .ToListAsync();

                        int maxVoucherNo = result.FirstOrDefault()?.MaxVoucherNo ?? 0;
                        int newVoucherNo = maxVoucherNo + 1;

                        strNewReceiptNo = "000" + newVoucherNo.ToString();
                        strNewReceiptNo = strNewReceiptNo.Substring(strNewReceiptNo.Length - 3);
                        strNewReceiptNo = voucherString + strNewReceiptNo;

                        var newVoucher = new Tbl201VoucherEntry
                        {
                            VoucherNo = strNewReceiptNo,
                        };

                        dbContext.Tbl201VoucherEntries.Add(newVoucher);
                        await dbContext.SaveChangesAsync();

                        await transaction.CommitAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetNewVoucherNo: {ex.Message}");
                    strNewReceiptNo = voucherString + "001";
                }

                return Json(strNewReceiptNo);
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
        public async Task<ActionResult> GetAccountHead(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var qryListOfAccountlists = dbContext.Qry201ListOfAccounts
                    .Where(p => p.IsUsedInPurchase.HasValue ? p.IsUsedInPurchase.Value : false)
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
                        i.IsLedgerObselete,
                        i.IsUseInSales,
                        i.IsUsedInPurchase
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
                var qryListOfAccountlists = dbContext.Qry201VoucherEntryScreenDisplays
                    .Where(p => p.VoucherNo == voucherNo && !string.IsNullOrEmpty(p.DrCr))
                    .Select(i => new
                    {
                        i.VoucherNo,
                        i.DrCr,
                        i.DrAmount,
                        i.CrAmount,
                        i.EntryNarration,
                        i.AccountHead,
                        i.SysRemarks,
                    });

                return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<ActionResult> GetVoucherDetails(DataSourceLoadOptions loadOptions, string voucherNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                if (string.IsNullOrEmpty(voucherNo))
                {
                    return BadRequest(new { success = false, message = "Invalid data received." });
                }
                try
                {
                    var voucherDetails = dbContext.Tbl201VoucherMasters
                        .Where(p => p.VoucherNo.ToLower() == voucherNo.ToLower());

                    if (!voucherDetails.Any())
                    {
                        return NotFound(new { success = false, message = "Voucher not found." });
                    }

                    var result = await DataSourceLoader.LoadAsync(voucherDetails, loadOptions);
                    return Json(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetVoucherDetails: {ex.Message}");
                    return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public async Task<ActionResult> AddVoucherEntry(DataSourceLoadOptions loadOptions, [FromBody] List<Tbl201VoucherEntry> voucherEntries, string AccountHead, string PaymentAccoutHeadName, int Gridcount)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                if (voucherEntries == null || !voucherEntries.Any())
                {
                    return BadRequest(new { success = false, message = "Invalid data received." });
                }

                try
                {
                    Tbl201VoucherMaster voucherMaster = new();

                    bool isVoucherExists = dbContext.Tbl201VoucherMasters
                   .Any(v => v.VoucherNo == voucherEntries[0].VoucherNo);

                    if (!isVoucherExists)
                    {
                        voucherMaster.VoucherNo = voucherEntries[0].VoucherNo;
                        voucherMaster.VoucherDate = DateTime.Now;
                        dbContext.Tbl201VoucherMasters.AddRange(voucherMaster);
                    }

                    dbContext.Tbl201VoucherEntries.AddRange(voucherEntries);
                    await dbContext.SaveChangesAsync();

                    var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
                    var qryListOfAccountlists = dbContext.Qry201VoucherEntryScreenDisplays
                        .Where(p => voucherNos.Contains(p.VoucherNo) && !string.IsNullOrEmpty(p.DrCr))
                        .OrderBy(i => i.DrCr == "Cr")
                        .Select(i => new VoucherEntryDisplayDTO
                        {
                            VoucherNo = i.VoucherNo,
                            VoucherEntryNo = i.VoucherEntryNo,
                            DrCr = i.DrCr,
                            DrAmount = i.DrAmount,
                            CrAmount = i.CrAmount,
                            EntryNarration = i.EntryNarration,
                            AccountHead = i.AccountHead,
                            SysRemarks = i.SysRemarks
                        });

                    var resultList = await qryListOfAccountlists.ToListAsync();

                    int debitamt = 0;

                    foreach (var entry in resultList)
                    {
                        if (!string.IsNullOrEmpty(entry.AccountHead))
                        {
                            var accountHead = dbContext.Qry201ListOfAccounts
                                .Where(a => a.AccountId == entry.AccountHead)
                                .Select(a => a.AccountHead)
                                .FirstOrDefault();

                            if (Gridcount != 0)
                            {
                                debitamt = (int)(debitamt + entry.DrAmount);

                                if (entry.DrCr == "Cr")
                                {
                                    var existingEntry = dbContext.Tbl201VoucherEntries
                                        .FirstOrDefault(v => v.AccountHead == entry.AccountHead
                                            && v.DrCr == "Cr"
                                            && v.VoucherNo == entry.VoucherNo);

                                    if (existingEntry != null)
                                    {
                                        entry.CrAmount = debitamt;
                                        existingEntry.VoucherAmount = entry.CrAmount;
                                        dbContext.Tbl201VoucherEntries.Update(existingEntry);
                                        dbContext.SaveChanges();
                                    }
                                    else
                                    {
                                        entry.CrAmount = debitamt;
                                    }
                                }
                            }
                            entry.AccountHead = accountHead;
                        }
                    }

                    return Json(DataSourceLoader.Load(resultList.AsQueryable(), loadOptions));
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
        public async Task<ActionResult> AddPurchaseCrVoucherEntry(DataSourceLoadOptions loadOptions, [FromBody] List<Tbl201VoucherEntry> voucherEntries, string AccountHead, string PaymentAccoutHeadName, int Gridcount)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                if (voucherEntries == null || !voucherEntries.Any())
                {
                    return BadRequest(new { success = false, message = "Invalid data received." });
                }

                try
                {
                    Tbl201VoucherMaster voucherMaster = new();
                    int aEntryAmount = 0;
                    int Amt = 0, Crmt = 0, CrMinusAmt = 0, NewCrMinusAmt = 0, ExistingCrMinusAmt = 0;
                    bool IsMatchingEntry = false;

                    //var matchingEntries;
                    List<VoucherEntryDisplayDTO> matchingEntries = new();
                    var voucherNos1 = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
                    var qryListOfAccountlists1 = dbContext.Qry201VoucherEntryScreenDisplays
                        .Where(p => voucherNos1.Contains(p.VoucherNo) && !string.IsNullOrEmpty(p.DrCr))
                        .OrderBy(i => i.DrCr == "Dr" ? 1 : 0) // Ensures "Dr" entries come first
                        .Select(i => new VoucherEntryDisplayDTO
                        {
                            VoucherNo = i.VoucherNo,
                            VoucherEntryNo = i.VoucherEntryNo,
                            DrCr = i.DrCr,
                            DrAmount = i.DrAmount,
                            CrAmount = i.CrAmount,
                            VoucherAmountFormatted = i.VoucherAmountFormatted,
                            EntryNarration = i.EntryNarration,
                            AccountHead = i.AccountHead,
                            SysRemarks = i.SysRemarks
                        });

                    var resultList1 = await qryListOfAccountlists1.ToListAsync();
                    int crCount1 = resultList1.Count(i => i.DrCr == "Cr");
                    if (crCount1 >= 2)
                    {
                        matchingEntries = resultList1.Where(x => x.AccountHead == PaymentAccoutHeadName).ToList();
                        if (matchingEntries.Count != 0)
                        {
                            ExistingCrMinusAmt = (int)matchingEntries[0].CrAmount;
                            IsMatchingEntry = true;
                        }

                    }

                    bool isVoucherExists = dbContext.Tbl201VoucherMasters
                   .Any(v => v.VoucherNo == voucherEntries[0].VoucherNo);

                    if (!isVoucherExists)
                    {
                        voucherMaster.VoucherNo = voucherEntries[0].VoucherNo;
                        voucherMaster.VoucherDate = DateTime.Now;
                        dbContext.Tbl201VoucherMasters.AddRange(voucherMaster);
                    }

                    // Add entries to the database
                    dbContext.Tbl201VoucherEntries.AddRange(voucherEntries);
                    await dbContext.SaveChangesAsync();

                    //SaveVoucher(voucherEntries);


                    var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
                    var qryListOfAccountlists = dbContext.Qry201VoucherEntryScreenDisplays
                        .Where(p => voucherNos.Contains(p.VoucherNo)&& !string.IsNullOrEmpty(p.DrCr))
                        .OrderBy(i => i.DrCr == "Dr" ? 1 : 0) // Ensures "Dr" entries come first
                        .Select(i => new VoucherEntryDisplayDTO
                        {
                            VoucherNo = i.VoucherNo,
                            VoucherEntryNo = i.VoucherEntryNo,
                            DrCr = i.DrCr,
                            DrAmount = i.DrAmount,
                            CrAmount = i.CrAmount,

                            VoucherAmountFormatted = i.VoucherAmountFormatted,
                            EntryNarration = i.EntryNarration,
                            AccountHead = i.AccountHead,
                            SysRemarks = i.SysRemarks
                        });


                    var resultList = await qryListOfAccountlists.ToListAsync();

                    int debitamt = 0; // Initialize debit amount

                    int crCount = resultList.Count(i => i.DrCr == "Dr");

                    //var lastCrEntry = resultList.LastOrDefault(i => i.DrCr == "Cr");

                    var newCrEntry = resultList
        .Where(i => i.DrCr == "Cr")
        .OrderByDescending(i => i.VoucherEntryNo) // Assuming this is sequential
        .FirstOrDefault();



                    var newlist = newCrEntry != null ? new List<VoucherEntryDisplayDTO> { newCrEntry } : new List<VoucherEntryDisplayDTO>();

                    var pettycashid = "";
                    foreach (var petty in newlist)
                    {
                        pettycashid = petty.AccountHead;
                        NewCrMinusAmt = (int)petty.CrAmount;
                    }

                    foreach (var entry in resultList)
                    {

                        if (!entry.SysRemarks.Contains("Bill of"))
                        {
                            CrMinusAmt = (int)entry.VoucherAmountFormatted;

                            entry.CrAmount = CrMinusAmt;
                            if (IsMatchingEntry == true)
                            {
                                //CrMinusAmt = (int)entry.VoucherAmountFormatted + NewCrMinusAmt;
                                CrMinusAmt = NewCrMinusAmt + ExistingCrMinusAmt;


                                var existingEntry = dbContext.Tbl201VoucherEntries
                                          .FirstOrDefault(v => v.AccountHead == entry.AccountHead
                                                            && v.DrCr == "Cr"
                                                            && v.VoucherNo == entry.VoucherNo);

                                // Optionally update the existing entry in the database
                                existingEntry.VoucherAmount = CrMinusAmt;
                                entry.CrAmount = -CrMinusAmt;
                                dbContext.Tbl201VoucherEntries.Update(existingEntry);
                                dbContext.SaveChanges();

                            }
                        }


                        if (!string.IsNullOrEmpty(entry.AccountHead))
                        {

                            var accountHead = dbContext.Qry201ListOfAccounts
                                                      .Where(a => a.AccountId == entry.AccountHead)
                                                      .Select(a => a.AccountHead)
                                                      .FirstOrDefault();

                            entry.AccountHead = accountHead;
                        }

                    }

                    return Json(DataSourceLoader.Load(resultList.AsQueryable(), loadOptions));
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
        public async Task<ActionResult> SaveVoucher([FromBody] Tbl201VoucherMaster VM)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                if (VM == null)
                {
                    return BadRequest(new { success = false, message = "Invalid data received." });
                }

                try
                {
                    dbContext.Tbl201VoucherMasters.Add(VM);
                    await dbContext.SaveChangesAsync();
                    return Ok(new { success = true, message = "Data inserted successfully!" });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in SaveVoucher: {ex.Message}");
                    return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public async Task<ActionResult> DeleteVoucherEntry(DataSourceLoadOptions loadOptions, long voucherEntryNo, string VoucherNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    int debitamt = 0;

                    var record = await dbContext.Tbl201VoucherEntries.FirstOrDefaultAsync(v => v.VoucherEntryNo == voucherEntryNo);
                    if (record == null)
                    {
                        return NotFound(new { message = "Record not found!" });
                    }

                    dbContext.Tbl201VoucherEntries.Remove(record);
                    await dbContext.SaveChangesAsync();

                    var voucherEntries = dbContext.Tbl201VoucherEntries
                        .Where(ve => ve.VoucherNo == VoucherNo)
                        .ToList();

                    var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();

                    var qryListOfAccountLists = dbContext.Qry201VoucherEntryScreenDisplays
                        .Where(p => voucherNos.Contains(p.VoucherNo) && !string.IsNullOrEmpty(p.DrCr))
                        .OrderBy(i => i.DrCr == "Cr")
                        .Select(i => new VoucherEntryDisplayDTO
                        {
                            VoucherNo = i.VoucherNo,
                            VoucherEntryNo = i.VoucherEntryNo,
                            DrCr = i.DrCr,
                            DrAmount = i.DrAmount,
                            CrAmount = i.CrAmount,
                            EntryNarration = i.EntryNarration,
                            AccountHead = i.AccountHead,
                            SysRemarks = i.SysRemarks
                        });

                    var resultList = await qryListOfAccountLists.ToListAsync();

                    foreach (var entry in resultList)
                    {
                        debitamt = (int)(debitamt + entry.DrAmount);

                        if (entry.DrCr == "Cr")
                        {
                            var existingEntry = dbContext.Tbl201VoucherEntries
                                .FirstOrDefault(v => v.AccountHead == entry.AccountHead
                                    && v.DrCr == "Cr"
                                    && v.VoucherNo == entry.VoucherNo);

                            if (existingEntry != null)
                            {
                                entry.CrAmount = debitamt;
                                existingEntry.VoucherAmount = entry.CrAmount;
                                dbContext.Tbl201VoucherEntries.Update(existingEntry);
                                dbContext.SaveChanges();
                            }
                            else
                            {
                                entry.CrAmount = debitamt;
                            }
                        }

                        if (!string.IsNullOrEmpty(entry.AccountHead))
                        {
                            var accountHead = dbContext.Qry201ListOfAccounts
                                .Where(a => a.AccountId == entry.AccountHead)
                                .Select(a => a.AccountHead)
                                .FirstOrDefault();

                            entry.AccountHead = accountHead;
                        }

                        entry.SysRemarks = null;

                        if (resultList.Count == 1)
                        {
                            entry.DrAmount = 0;
                            entry.CrAmount = 0;
                            if (entry.DrCr == "Dr")
                            {
                                entry.DrAmount = 0;
                                entry.CrAmount = 0;
                            }
                        }
                    }

                    return Json(DataSourceLoader.Load(resultList.AsQueryable(), loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in DeleteVoucherEntry: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while deleting the record.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
    }
}






