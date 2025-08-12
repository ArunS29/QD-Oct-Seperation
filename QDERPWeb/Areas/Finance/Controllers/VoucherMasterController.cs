using System.Data.SqlClient;
using System.Security.Policy;
using System.Xml.Linq;
using DevExpress.DataAccess.Native.Json;
using DevExpress.DataProcessing.InMemoryDataProcessor;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.Areas.IMS.Reports.quotationstoClients;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using QD.ERP.Web.Services.Logging;
using QDERPWeb.Models;


namespace QD.ERP.Web.Areas.Finance.Controllers
{
    //[Area("Finance")]
    [Route("api/[controller]/[action]")]
    // [Route("Finapi/[controller]/[action]")]
    [ApiController]
    public class VoucherMasterController : Controller
    {


        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<VoucherMasterController> _logger;
        private readonly IUserActionLogger _userActionLogger;
        private readonly FcmService _fcmService;

        public VoucherMasterController(ILogger<VoucherMasterController> logger, TenantDbContextHelper tenantDbContextHelper, IUserActionLogger userActionLogger, FcmService fcmService)
        {
            _userActionLogger = userActionLogger;
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
            _fcmService = fcmService;
        }
        [HttpGet]
        public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var tbl201vouchermasters = dbContext.Tbl201VoucherMasters.Select(i => new
                    {
                        i.VoucherNo,
                        i.VoucherDate,
                        i.VoucherRefNo,
                        i.VoucherNarration,
                        i.VoucherEnteredBy,
                        i.VoucherEnteredOn,
                        i.VoucherVerifiedBy,
                        i.VoucherVerifiedOn,
                        i.IsVerified,
                        i.VoucherApprovedBy,
                        i.VoucherApprovedOn,
                        i.IsApproved,
                        i.VoucherType,
                        i.VoucherEffectiveDate,
                        i.InvoiceSubmittedDate,
                        i.InvoiceDueDate,
                        i.InvoiceNoOfDays,
                        i.UseSubmittedDate,
                        i.SalesPersonCode,
                        i.BillNo,
                        i.BillDate,
                        i.BillPaidTo,
                        i.BillRemarks,
                        i.VoucherModifiedBy,
                        i.VoucherModifiedOn,
                        i.AuditVerifiedBy,
                        i.AuditVerifiedOn,
                        i.IsAuditVerified,
                        i.DeliveryNoteNo,
                        i.CogsInvoiceNo,
                        i.ReferenceNote,
                        i.RentalPayslipNo
                    });

                    // If underlying data is a large SQL table, specify PrimaryKey and PaginateViaPrimaryKey.
                    // This can make SQL execution plans more efficient.
                    // For more detailed information, please refer to this discussion: https://github.com/DevExpress/DevExtreme.AspNet.Data/issues/336.
                    // loadOptions.PrimaryKey = new[] { "VoucherNo" };
                    // loadOptions.PaginateViaPrimaryKey = true;

                    return Json(await DataSourceLoader.LoadAsync(tbl201vouchermasters, loadOptions));
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }


            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetCPPaymentAccounts(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var qryListOfAccountlists = dbContext.Qry201ListOfAccounts
                        .Where(p => p.AccountGroupId == "A012")
                        .Select(i => new
                        {
                            i.AccountId,
                            i.AccountHead,
                            i.AccountGroup,
                            i.AccountHeadArabic,
                            i.ReferenceNo,
                            i.AccountGroupId,
                            i.IsLedgerObselete,
                            i.MasterGroupId,
                            i.MasterGroup,
                            i.IsRestricted,
                            i.IsUseInSales,
                            i.IsUsedInPurchase,
                            i.IsProfitLossAccount,
                            i.IsBalanceSheetAccount,
                            i.IsMaintainBillByBill,
                            i.IsUseInReconciliation,
                            i.IsSalaryPayable
                        });

                    var result = await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions);
                    return Json(result);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, "Internal server error. Please try again later.");
                }
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }


        [HttpGet]
        public async Task<ActionResult> GetBPPaymentAccounts(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var qryListOfAccountlists = dbContext.Qry201ListOfAccounts.Where(p => p.AccountGroupId == "A013").Select(i => new
                    {
                        i.AccountId,
                        i.AccountHead,
                        i.AccountGroup,
                        i.AccountHeadArabic,
                        i.ReferenceNo,
                        i.AccountGroupId,
                        i.IsLedgerObselete,
                        i.MasterGroupId,
                        i.MasterGroup,
                        i.IsRestricted,
                        i.IsUseInSales,
                        i.IsUsedInPurchase,
                        i.IsProfitLossAccount,
                        i.IsBalanceSheetAccount,
                        i.IsMaintainBillByBill,
                        i.IsUseInReconciliation,
                        i.IsSalaryPayable



                    });

                    return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<ActionResult> GetVoucherEntryPaymentGrid(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var qryListOfAccountlists = dbContext.Qry201VoucherEntryScreenDisplays
    .Where(p => p.VoucherNo != null).Select(i => new

    {
        i.DrCr,
        i.AccountHead,
        i.DrAmount,
        i.CrAmount,
        i.EntryNarration,
        i.SysRemarks


    });

                    return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));

                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetAccountHead(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {

                try
                {
                    var qryListOfAccountlists = dbContext.Qry201ListOfAccounts.Select(i => new
                    {

                        i.AccountId,
                        i.AccountHead,
                        i.AccountGroup,
                        i.AccountHeadArabic,
                        i.ReferenceNo,
                        i.AccountGroupId,
                        i.IsLedgerObselete,
                        i.MasterGroupId,
                        i.MasterGroup,
                        i.IsRestricted,
                        i.IsUseInSales,
                        i.IsUsedInPurchase,
                        i.IsProfitLossAccount,
                        i.IsBalanceSheetAccount,
                        i.IsMaintainBillByBill,
                        i.IsUseInReconciliation,
                        i.IsSalaryPayable

                    });


                    return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));

                }
                catch (Exception ex) { throw ex; }
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
                    int aEntryAmount = 0;
                    int Amt = 0;
                    var Remarks = "";
                    bool IsMatchingEntry = false;
                    var ExistingAccHeadID = "";
                    //var matchingEntries;
                    List<VoucherEntryDisplayDTO> matchingEntries = new();
                    var voucherNos1 = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
                    var qryListOfAccountlists1 = dbContext.Qry201VoucherEntryScreenDisplays
                        .Where(p => voucherNos1.Contains(p.VoucherNo))
                        .OrderBy(i => i.DrCr == "Cr" ? 1 : 0) // Ensures "Dr" entries come first
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

                    var resultList1 = await qryListOfAccountlists1.ToListAsync();
                    int crCount1 = resultList1.Count(i => i.DrCr == "Cr");
                    if (crCount1 >= 2)
                    {
                        matchingEntries = resultList1.Where(x => x.AccountHead == PaymentAccoutHeadName).ToList();
                        if (matchingEntries.Count != 0)
                        {
                            ExistingAccHeadID = matchingEntries[0].AccountHead;
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
                        .Where(p => voucherNos.Contains(p.VoucherNo))
                        .OrderBy(i => i.DrCr == "Cr" ? 1 : 0) // Ensures "Dr" entries come first
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

                    int debitamt = 0; // Initialize debit amount

                    int crCount = resultList.Count(i => i.DrCr == "Cr");

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
                    }

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
                                // Calculate Debit Amount (DrAmount)

                                debitamt = (int)(debitamt + entry.DrAmount);

                                // If Dr/Cr is Credit ("Cr"), perform specific logic
                                if (entry.DrCr == "Cr" && crCount == 1)
                                // if (crCount==1)
                                {
                                    // Check if an existing entry matches
                                    var existingEntry = dbContext.Tbl201VoucherEntries
                                                                .FirstOrDefault(v => v.AccountHead == entry.AccountHead
                                                                                  && v.DrCr == "Cr"
                                                                                  && v.VoucherNo == entry.VoucherNo);

                                    if (existingEntry != null)
                                    {
                                        // Update CrAmount by adding the calculated debit amount
                                        entry.CrAmount = debitamt;
                                        entry.SysRemarks = Remarks;

                                        // Optionally update the existing entry in the database
                                        existingEntry.VoucherAmount = entry.CrAmount;
                                        existingEntry.SysRemarks = entry.SysRemarks;
                                        dbContext.Tbl201VoucherEntries.Update(existingEntry);
                                        dbContext.SaveChanges();

                                    }
                                    else
                                    {
                                        // If no existing entry, assign CrAmount as debitamt
                                        entry.CrAmount = debitamt;
                                    }
                                }
                                else if (newCrEntry != null && crCount >= 2 && entry.AccountHead == ExistingAccHeadID) //&& entry.AccountHead == pettycashid)
                                {
                                    if (entry.DrCr == "Cr")
                                    {

                                        if (IsMatchingEntry == true)
                                        {
                                            // Accumulate `aEntryAmount` correctly
                                            foreach (var mEntry in matchingEntries)
                                            {
                                                aEntryAmount += (int)mEntry.CrAmount; // Accumulate CrAmount correctly
                                            }

                                            // Process voucherEntries
                                            foreach (var mVoucherEntry in voucherEntries)
                                            {
                                                int eAmount = (int)mVoucherEntry.VoucherAmount;
                                                Amt = aEntryAmount + eAmount;

                                            }

                                            // Check if an existing entry matches
                                            var existingEntry = dbContext.Tbl201VoucherEntries
                                                                        .FirstOrDefault(v => v.AccountHead == entry.AccountHead
                                                                                          && v.DrCr == "Cr"
                                                                                          && v.VoucherNo == entry.VoucherNo);

                                            if (existingEntry != null)
                                            {
                                                // Update CrAmount by adding the calculated debit amount
                                                entry.CrAmount = Amt;
                                                entry.SysRemarks = Remarks;
                                                // Optionally update the existing entry in the database
                                                existingEntry.VoucherAmount = entry.CrAmount;
                                                existingEntry.SysRemarks = entry.SysRemarks;
                                                dbContext.Tbl201VoucherEntries.Update(existingEntry);
                                                dbContext.SaveChanges();

                                            }
                                            else
                                            {
                                                // If no existing entry, assign CrAmount as debitamt
                                                // entry.CrAmount = debitamt;
                                            }
                                        }

                                    }

                                }

                            }
                            entry.AccountHead = accountHead;
                            if (Remarks == "")
                            {
                                Remarks = entry.AccountHead;
                            }
                            else
                            {
                                Remarks = Remarks + "," + entry.AccountHead;
                            }

                        }

                    }

                    return Json(DataSourceLoader.Load(resultList.AsQueryable(), loadOptions));
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
                }
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet]
        public async Task<ActionResult> LoadVoucherEntries(DataSourceLoadOptions loadOptions, string voucherNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    if (string.IsNullOrEmpty(voucherNo))
                    {
                        return BadRequest(new { success = false, message = "Invalid Voucher Number." });
                    }

                    var qryListOfAccountlists = dbContext.Qry201VoucherEntryScreenDisplays
                        .Where(p => p.VoucherNo == voucherNo)
                        .OrderBy(i => i.DrCr == "Cr" ? 1 : 0) // Ensures "Dr" entries come first
                        .Select(i => new
                        {
                            i.VoucherNo,
                            i.VoucherEntryNo,
                            i.DrCr,
                            i.DrAmount,
                            i.CrAmount,
                            i.EntryNarration,
                            i.AccountHead,
                            i.SysRemarks,
                            AccountId = dbContext.Tbl201ChartOfAccounts
                       .Where(c => c.AccountHead == i.AccountHead)
                       .Select(c => c.AccountId)
                       .FirstOrDefault()
                        })
                        .ToList();

                    // Fetch AccountHead names for mapping
                    var accountIds = qryListOfAccountlists.Select(i => i.AccountHead).Distinct().ToList();
                    var accountHeadMap = dbContext.Qry201ListOfAccounts
                        .Where(a => accountIds.Contains(a.AccountId))
                        .ToDictionary(a => a.AccountId, a => a.AccountHead);

                    // Map AccountId to AccountHead
                    var resultList = qryListOfAccountlists.Select(i => new VoucherEntryDisplayDTO
                    {
                        VoucherNo = i.VoucherNo,
                        VoucherEntryNo = i.VoucherEntryNo,
                        DrCr = i.DrCr,
                        DrAmount = i.DrAmount,
                        CrAmount = i.CrAmount,
                        EntryNarration = i.EntryNarration,
                        AccountHead = accountHeadMap.ContainsKey(i.AccountHead) ? accountHeadMap[i.AccountHead] : i.AccountHead,
                        SysRemarks = i.SysRemarks
                    }).ToList();

                    return Json(DataSourceLoader.Load(resultList.AsQueryable(), loadOptions));
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
                }
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<ActionResult> LoadreceiptEntries(DataSourceLoadOptions loadOptions, string voucherNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    if (string.IsNullOrEmpty(voucherNo))
                    {
                        return BadRequest(new { success = false, message = "Invalid Voucher Number." });
                    }

                    var qryListOfAccountlists = dbContext.Qry201VoucherEntryScreenDisplays
                        .Where(p => p.VoucherNo == voucherNo)
                        .OrderBy(i => i.DrCr == "Dr" ? 1 : 0) // Ensures "Dr" entries come first
                        .Select(i => new
                        {
                            i.VoucherNo,
                            i.VoucherEntryNo,
                            i.DrCr,
                            i.DrAmount,
                            i.CrAmount,
                            i.EntryNarration,
                            i.AccountHead,
                            i.SysRemarks
                        })
                        .ToList();

                    // Fetch AccountHead names for mapping
                    var accountIds = qryListOfAccountlists.Select(i => i.AccountHead).Distinct().ToList();
                    var accountHeadMap = dbContext.Qry201ListOfAccounts
                        .Where(a => accountIds.Contains(a.AccountId))
                        .ToDictionary(a => a.AccountId, a => a.AccountHead);

                    // Map AccountId to AccountHead
                    var resultList = qryListOfAccountlists.Select(i => new VoucherEntryDisplayDTO
                    {
                        VoucherNo = i.VoucherNo,
                        VoucherEntryNo = i.VoucherEntryNo,
                        DrCr = i.DrCr,
                        DrAmount = i.DrAmount,
                        CrAmount = i.CrAmount,
                        EntryNarration = i.EntryNarration,
                        AccountHead = accountHeadMap.ContainsKey(i.AccountHead) ? accountHeadMap[i.AccountHead] : i.AccountHead,
                        AccountId = i.AccountHead,
                        SysRemarks = i.SysRemarks
                    }).ToList();

                    return Json(DataSourceLoader.Load(resultList.AsQueryable(), loadOptions));
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
                }
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpPost]
        public async Task<ActionResult> AddBPVoucherEntry(DataSourceLoadOptions loadOptions, [FromBody] List<Tbl201VoucherEntry> voucherEntries, string AccountHead, string PaymentAccoutHeadName, int Gridcount)
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
                    int Amt = 0;
                    bool IsMatchingEntry = false;

                    //var matchingEntries;
                    List<VoucherEntryDisplayDTO> matchingEntries = new();
                    var voucherNos1 = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
                    var qryListOfAccountlists1 = dbContext.Qry201VoucherEntryScreenDisplays
                        .Where(p => voucherNos1.Contains(p.VoucherNo))
                        .OrderBy(i => i.DrCr == "Cr" ? 1 : 0) // Ensures "Dr" entries come first
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

                    var resultList1 = await qryListOfAccountlists1.ToListAsync();
                    int crCount1 = resultList1.Count(i => i.DrCr == "Cr");
                    if (crCount1 >= 2)
                    {
                        matchingEntries = resultList1.Where(x => x.AccountHead == PaymentAccoutHeadName).ToList();
                        IsMatchingEntry = true;
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
                        .Where(p => voucherNos.Contains(p.VoucherNo))
                        .OrderBy(i => i.DrCr == "Cr" ? 1 : 0) // Ensures "Dr" entries come first
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

                    int debitamt = 0; // Initialize debit amount

                    int crCount = resultList.Count(i => i.DrCr == "Cr");

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
                    }

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
                                // Calculate Debit Amount (DrAmount)

                                debitamt = (int)(debitamt + entry.DrAmount);

                                // If Dr/Cr is Credit ("Cr"), perform specific logic
                                if (entry.DrCr == "Cr" && crCount == 1)
                                // if (crCount==1)
                                {
                                    // Check if an existing entry matches
                                    var existingEntry = dbContext.Tbl201VoucherEntries
                                                                .FirstOrDefault(v => v.AccountHead == entry.AccountHead
                                                                                  && v.DrCr == "Cr"
                                                                                  && v.VoucherNo == entry.VoucherNo);

                                    if (existingEntry != null)
                                    {
                                        // Update CrAmount by adding the calculated debit amount
                                        entry.CrAmount = debitamt;

                                        // Optionally update the existing entry in the database
                                        existingEntry.VoucherAmount = entry.CrAmount;
                                        dbContext.Tbl201VoucherEntries.Update(existingEntry);
                                        dbContext.SaveChanges();

                                    }
                                    else
                                    {
                                        // If no existing entry, assign CrAmount as debitamt
                                        entry.CrAmount = debitamt;
                                    }
                                }
                                else if (newCrEntry != null && crCount >= 2 && entry.AccountHead == pettycashid)
                                {
                                    if (entry.DrCr == "Cr")
                                    {

                                        if (IsMatchingEntry == true)
                                        {
                                            // Accumulate `aEntryAmount` correctly
                                            foreach (var mEntry in matchingEntries)
                                            {
                                                aEntryAmount += (int)mEntry.CrAmount; // Accumulate CrAmount correctly
                                            }

                                            // Process voucherEntries
                                            foreach (var mVoucherEntry in voucherEntries)
                                            {
                                                int eAmount = (int)mVoucherEntry.VoucherAmount;
                                                Amt = aEntryAmount + eAmount;

                                                // Do something with Amt if required
                                            }

                                            // Check if an existing entry matches
                                            var existingEntry = dbContext.Tbl201VoucherEntries
                                                                        .FirstOrDefault(v => v.AccountHead == entry.AccountHead
                                                                                          && v.DrCr == "Cr"
                                                                                          && v.VoucherNo == entry.VoucherNo);

                                            if (existingEntry != null)
                                            {
                                                // Update CrAmount by adding the calculated debit amount
                                                entry.CrAmount = Amt;

                                                // Optionally update the existing entry in the database
                                                existingEntry.VoucherAmount = entry.CrAmount;
                                                dbContext.Tbl201VoucherEntries.Update(existingEntry);
                                                dbContext.SaveChanges();

                                            }
                                            else
                                            {
                                                // If no existing entry, assign CrAmount as debitamt
                                                // entry.CrAmount = debitamt;
                                            }
                                        }

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
                    return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
                }
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }



        [HttpPost]
        public async Task<ActionResult> AddBPCrVoucherEntry(DataSourceLoadOptions loadOptions, [FromBody] List<Tbl201VoucherEntry> voucherEntries, string AccountHead, string PaymentAccoutHeadName, int Gridcount)
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
                        .Where(p => voucherNos1.Contains(p.VoucherNo))
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
                        .Where(p => voucherNos.Contains(p.VoucherNo))
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

                        if (!entry.SysRemarks.Contains("Paid thru:"))
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
                    return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
                }
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }



        //[HttpPost]
        //public async Task<ActionResult> AddCPCrVoucherEntry(DataSourceLoadOptions loadOptions, [FromBody] List<Tbl201VoucherEntry> voucherEntries, string AccountHead, string PaymentAccoutHeadName, int Gridcount)
        //{
        //    if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
        //    {
        //        if (voucherEntries == null || !voucherEntries.Any())
        //        {
        //            return BadRequest(new { success = false, message = "Invalid data received." });
        //        }

        //        try
        //        {


        //            Tbl201VoucherMaster voucherMaster = new();
        //            int aEntryAmount = 0;
        //            int Amt = 0, Crmt = 0, CrMinusAmt = 0, NewCrMinusAmt = 0, ExistingCrMinusAmt = 0;
        //            bool IsMatchingEntry = false;

        //            //var matchingEntries;
        //            List<VoucherEntryDisplayDTO> matchingEntries = new();
        //            var voucherNos1 = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
        //            var qryListOfAccountlists1 = dbContext.Qry201VoucherEntryScreenDisplays
        //                .Where(p => voucherNos1.Contains(p.VoucherNo))
        //                .OrderBy(i => i.DrCr == "Dr" ? 1 : 0) // Ensures "Dr" entries come first
        //                .Select(i => new VoucherEntryDisplayDTO
        //                {
        //                    VoucherNo = i.VoucherNo,
        //                    VoucherEntryNo = i.VoucherEntryNo,
        //                    DrCr = i.DrCr,
        //                    DrAmount = i.DrAmount,
        //                    CrAmount = i.CrAmount,
        //                    VoucherAmountFormatted = i.VoucherAmountFormatted,
        //                    EntryNarration = i.EntryNarration,
        //                    AccountHead = i.AccountHead,
        //                    SysRemarks = i.SysRemarks
        //                });

        //            var resultList1 = await qryListOfAccountlists1.ToListAsync();


        //            bool isVoucherExists = dbContext.Tbl201VoucherMasters
        //           .Any(v => v.VoucherNo == voucherEntries[0].VoucherNo);

        //            if (!isVoucherExists)
        //            {
        //                voucherMaster.VoucherNo = voucherEntries[0].VoucherNo;
        //                voucherMaster.VoucherDate = DateTime.Now;
        //                dbContext.Tbl201VoucherMasters.AddRange(voucherMaster);
        //            }

        //            // Add entries to the database
        //            dbContext.Tbl201VoucherEntries.AddRange(voucherEntries);
        //            await dbContext.SaveChangesAsync();

        //            //SaveVoucher(voucherEntries);


        //            var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
        //            var qryListOfAccountlists = dbContext.Qry201VoucherEntryScreenDisplays
        //                .Where(p => voucherNos.Contains(p.VoucherNo))
        //                .OrderBy(i => i.DrCr == "Dr" ? 1 : 0) // Ensures "Dr" entries come first
        //                .Select(i => new VoucherEntryDisplayDTO
        //                {
        //                    VoucherNo = i.VoucherNo,
        //                    VoucherEntryNo = i.VoucherEntryNo,
        //                    DrCr = i.DrCr,
        //                    DrAmount = i.DrAmount,
        //                    CrAmount = i.CrAmount,
        //                    VoucherAmountFormatted = i.VoucherAmountFormatted,
        //                    EntryNarration = i.EntryNarration,
        //                    AccountHead = i.AccountHead,
        //                    SysRemarks = i.SysRemarks,
        //                    Type = i.Type

        //                });


        //            var resultList = await qryListOfAccountlists.ToListAsync();
        //            int crCount1 = resultList.Count(i => i.DrCr == "Cr");
        //            bool hasDrEntries = resultList.Any(i => i.DrCr == "Dr");
        //            if (crCount1 >= 2)
        //            {
        //                matchingEntries = resultList.Where(x => x.AccountHead == PaymentAccoutHeadName).ToList();
        //                if (matchingEntries.Count != 0)
        //                {
        //                    ExistingCrMinusAmt = (int)matchingEntries[0].CrAmount;
        //                    IsMatchingEntry = true;
        //                }
        //            }

        //            int debitamt = 0; // Initialize debit amount

        //            int crCount = resultList.Count(i => i.DrCr == "Dr");

        //            //var lastCrEntry = resultList.LastOrDefault(i => i.DrCr == "Cr");

        //            var newCrEntry = resultList
        //.Where(i => i.DrCr == "Cr")
        //.OrderByDescending(i => i.VoucherEntryNo) // Assuming this is sequential
        //.FirstOrDefault();



        //            var newlist = newCrEntry != null ? new List<VoucherEntryDisplayDTO> { newCrEntry } : new List<VoucherEntryDisplayDTO>();

        //            var pettycashid = "";
        //            foreach (var petty in newlist)
        //            {
        //                pettycashid = petty.AccountHead;
        //                NewCrMinusAmt = (int)petty.CrAmount;
        //            }

        //            foreach (var entry in resultList)
        //            {
        //                if (!entry.SysRemarks.Contains("Paid thru:"))
        //                {
        //                    // if(IsMatchingEntry==false && crCount1==2)//Cr
        //                    if (hasDrEntries != true)
        //                    {
        //                        if (crCount1 == 2)
        //                        {
        //                            CrMinusAmt = (int)entry.VoucherAmountFormatted;
        //                        }
        //                        else
        //                        {
        //                            CrMinusAmt = NewCrMinusAmt - ExistingCrMinusAmt;
        //                        }

        //                        entry.CrAmount = CrMinusAmt;


        //                        var existingEntry = dbContext.Tbl201VoucherEntries
        //                                  .FirstOrDefault(v => v.AccountHead == entry.AccountHead
        //                                                    && v.DrCr == "Cr"
        //                                                    && v.VoucherNo == entry.VoucherNo);

        //                        // Optionally update the existing entry in the database
        //                        existingEntry.VoucherAmount = CrMinusAmt;

        //                        // entry.CrAmount = CrMinusAmt;

        //                        dbContext.Tbl201VoucherEntries.Update(existingEntry);
        //                        dbContext.SaveChanges();

        //                    }
        //                    else if (IsMatchingEntry == true && hasDrEntries == true)//Dr - Cr
        //                    {
        //                        //CrMinusAmt = (int)entry.VoucherAmountFormatted + NewCrMinusAmt;
        //                        CrMinusAmt = NewCrMinusAmt - ExistingCrMinusAmt;
        //                        entry.CrAmount = -CrMinusAmt;


        //                        var existingEntry = dbContext.Tbl201VoucherEntries
        //                                 .FirstOrDefault(v => v.AccountHead == entry.AccountHead
        //                                                   && v.DrCr == "Cr"
        //                                                   && v.VoucherNo == entry.VoucherNo);

        //                        // Optionally update the existing entry in the database
        //                        existingEntry.VoucherAmount = -CrMinusAmt;


        //                        // entry.CrAmount = CrMinusAmt;

        //                        dbContext.Tbl201VoucherEntries.Update(existingEntry);
        //                        dbContext.SaveChanges();
        //                    }
        //                }


        //                if (!string.IsNullOrEmpty(entry.AccountHead))
        //                {

        //                    var accountHead = dbContext.Qry201ListOfAccounts
        //                                              .Where(a => a.AccountId == entry.AccountHead)
        //                                              .Select(a => a.AccountHead)
        //                                              .FirstOrDefault();

        //                    entry.AccountHead = accountHead;
        //                }

        //            }

        //            return Json(DataSourceLoader.Load(resultList.AsQueryable(), loadOptions));
        //        }
        //        catch (Exception ex)
        //        {
        //            return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
        //        }
        //    }
        //    return Unauthorized(new { message = "Invalid tenant.", success = false });
        //}





        //[HttpPost]
        //public async Task<ActionResult> SaveVoucher([FromBody] Tbl201VoucherMaster VM)
        //{
        //    if (VM == null)
        //    {
        //        return BadRequest(new { success = false, message = "Invalid data received." });
        //    }

        //    try
        //    {
        //        bool isVoucherExists = dbContext.Tbl201VoucherMasters
        //     .Any(v => v.VoucherNo == VM.VoucherNo);

        //        var existingVoucher = await dbContext.Tbl201VoucherMasters
        //                                .FirstOrDefaultAsync(v => v.VoucherNo == VM.VoucherNo);

        //        if (!isVoucherExists)
        //        {
        //            dbContext.Tbl201VoucherMasters.Add(VM);
        //            await dbContext.SaveChangesAsync();
        //            //return Json(new { VoucherEntryNo = VE.VoucherNo });
        //            return Ok(new { success = true, message = "Data inserted successfully!" });
        //        }
        //        // Update existing record
        //        dbContext.Entry(existingVoucher).CurrentValues.SetValues(VM);
        //        return Ok(new { success = true, message = "" });
        //    }
        //    catch (Exception ex)
        //    {

        //        return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
        //    }

        //}

        [HttpGet]
        public async Task<ActionResult> GetNewCPVoucherNo(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
              

                string defaultCompanyString = HttpContext.Session.GetString("DefaultcompanyID") ?? "";
                byte defaultCompanyByte = 0; // or any default value you want

                if (!string.IsNullOrEmpty(defaultCompanyString))
                {
                    // Safest way (avoids exceptions):
                    byte.TryParse(defaultCompanyString, out defaultCompanyByte);
                    // Now defaultCompanyByte holds the parsed value, or 0 if parsing failed.
                }

                // Now use defaultCompanyByte as needed



                byte companyId = defaultCompanyByte;

                // Step 2: Get NoOfDigitsInVouchers
                var companyConfig = await dbContext.Tbl901CompanyDetails02s
                    .Where(c => c.CompanyId == companyId)
                    .Select(c => new { c.NoOfDigitsInVouchers })
                    .FirstOrDefaultAsync();

                byte configuredDigitCount = companyConfig?.NoOfDigitsInVouchers ?? 3; // Default to 3 if not found

                // Step 3: Prepare voucher prefix
                DateTime currentDate = DateTime.Now;
                string yearPart = currentDate.Year.ToString().Substring(2); // "25"
                string monthPart = currentDate.Month.ToString("00"); // "06"
                string voucherPrefix = $"CP-{yearPart}-{monthPart}-";
                string likePattern = voucherPrefix + "%";

                int digitCountToUse = configuredDigitCount; // this might change if series already exists
                string strNewReceiptNo;
                try
                {
                    // Step 4: Check if any vouchers already exist for current month
                    var existingVoucher = await dbContext.Tbl201VoucherEntries
                        .Where(v => v.VoucherNo.StartsWith(voucherPrefix))
                        .OrderByDescending(v => v.VoucherNo)
                        .Select(v => v.VoucherNo)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(existingVoucher))
                    {
                        // Step 5: Existing series found → infer digit count from length of number part
                        string numberPart = existingVoucher.Substring(voucherPrefix.Length);
                        digitCountToUse = numberPart.Length;
                    }

                    // Step 6: Fetch max number using resolved digit count
                    var result = await dbContext.VoucherResults
                        .FromSqlInterpolated($@"
                    SELECT MAX(CAST(RIGHT(VoucherNo, {digitCountToUse}) AS INT)) AS MaxVoucherNo
                    FROM Tbl201VoucherMaster
                    WHERE VoucherNo LIKE {likePattern}")
                        .ToListAsync();

                    int maxVoucherNo = result.FirstOrDefault()?.MaxVoucherNo ?? 0;
                    int newVoucherNo = maxVoucherNo + 1;

                    string paddedNo = newVoucherNo.ToString().PadLeft(digitCountToUse, '0');
                    strNewReceiptNo = voucherPrefix + paddedNo;
                }
                catch (Exception)
                {
                    // fallback if any failure
                    string fallback = "1".PadLeft(configuredDigitCount, '0');
                    strNewReceiptNo = voucherPrefix + fallback;
                }

                return Json(strNewReceiptNo);
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }




        [HttpGet]
        public async Task<ActionResult> GetNewBPVoucherNo(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {

                string defaultCompanyString = HttpContext.Session.GetString("DefaultcompanyID") ?? "";
                byte defaultCompanyByte = 0; // or any default value you want

                if (!string.IsNullOrEmpty(defaultCompanyString))
                {
                    // Safest way (avoids exceptions):
                    byte.TryParse(defaultCompanyString, out defaultCompanyByte);
                    // Now defaultCompanyByte holds the parsed value, or 0 if parsing failed.
                }

                // Now use defaultCompanyByte as needed



                byte companyId = defaultCompanyByte;

                // Step 2: Get NoOfDigitsInVouchers
                var companyConfig = await dbContext.Tbl901CompanyDetails02s
                    .Where(c => c.CompanyId == companyId)
                    .Select(c => new { c.NoOfDigitsInVouchers })
                    .FirstOrDefaultAsync();

                byte configuredDigitCount = companyConfig?.NoOfDigitsInVouchers ?? 3; // Default to 3 if not found

                // Step 3: Prepare voucher prefix
                DateTime currentDate = DateTime.Now;
                string yearPart = currentDate.Year.ToString().Substring(2); // "25"
                string monthPart = currentDate.Month.ToString("00"); // "06"
                string voucherPrefix = $"BP-{yearPart}-{monthPart}-";
                string likePattern = voucherPrefix + "%";

                int digitCountToUse = configuredDigitCount; // this might change if series already exists
                string strNewReceiptNo;
                try
                {
                    // Step 4: Check if any vouchers already exist for current month
                    var existingVoucher = await dbContext.Tbl201VoucherEntries
                        .Where(v => v.VoucherNo.StartsWith(voucherPrefix))
                        .OrderByDescending(v => v.VoucherNo)
                        .Select(v => v.VoucherNo)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(existingVoucher))
                    {
                        // Step 5: Existing series found → infer digit count from length of number part
                        string numberPart = existingVoucher.Substring(voucherPrefix.Length);
                        digitCountToUse = numberPart.Length;
                    }

                    // Step 6: Fetch max number using resolved digit count
                    var result = await dbContext.VoucherResults
                        .FromSqlInterpolated($@"
                    SELECT MAX(CAST(RIGHT(VoucherNo, {digitCountToUse}) AS INT)) AS MaxVoucherNo
                    FROM Tbl201VoucherMaster
                    WHERE VoucherNo LIKE {likePattern}")
                        .ToListAsync();

                    int maxVoucherNo = result.FirstOrDefault()?.MaxVoucherNo ?? 0;
                    int newVoucherNo = maxVoucherNo + 1;

                    string paddedNo = newVoucherNo.ToString().PadLeft(digitCountToUse, '0');
                    strNewReceiptNo = voucherPrefix + paddedNo;
                }
                catch (Exception)
                {
                    // fallback if any failure
                    string fallback = "1".PadLeft(configuredDigitCount, '0');
                    strNewReceiptNo = voucherPrefix + fallback;
                }

                return Json(strNewReceiptNo);
            }


            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }




        [HttpPost]
        public async Task<ActionResult> DeleteCheque([FromBody] string chequeNo)
        {
            if (string.IsNullOrWhiteSpace(chequeNo))
            {
                return BadRequest(new { message = "Cheque No is required.", success = false });
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    // Retrieve the cheque record by ChequeNo
                    var cheque = await dbContext.Tbl20113ChequeMasters
                        .FirstOrDefaultAsync(c => c.ChequeNo == chequeNo);

                    if (cheque == null)
                    {
                        return NotFound(new { message = "Cheque not found.", success = false });
                    }

                    // Remove the cheque record
                    dbContext.Tbl20113ChequeMasters.Remove(cheque);
                    await dbContext.SaveChangesAsync();

                    return Ok(new { message = "Cheque deleted successfully.", success = true });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting cheque with ChequeNo: {ChequeNo}", chequeNo);
                    return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while deleting the cheque.", success = false });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }


        [HttpPost]
        public async Task<ActionResult> SaveChequeDetails([FromBody] Tbl20113ChequeMaster CM)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                if (CM == null)
                {
                    return BadRequest(new { success = false, message = "Invalid data received." });
                }

                try
                {
                    var userName = HttpContext.Session.GetString("UserName");
                    var now = DateTime.Now;

                    var existingVoucher = await dbContext.Tbl20113ChequeMasters
                                                         .FirstOrDefaultAsync(v => v.ChequeNo == CM.ChequeNo);

                    if (existingVoucher != null)
                    {
                        // Update existing record
                        dbContext.Entry(existingVoucher).CurrentValues.SetValues(CM);

                        existingVoucher.ModifiedBy = userName;
                        existingVoucher.ModifiedOn = now;

                        await dbContext.SaveChangesAsync();
                        return Ok(new { success = true, message = "Cheque Information Updated successfully!" });
                    }
                    else
                    {
                        // Insert new record
                        CM.AddedBy = userName;
                        CM.AddedOn = now;

                        dbContext.Tbl20113ChequeMasters.Add(CM);
                        await dbContext.SaveChangesAsync();
                        return Ok(new { success = true, message = "Cheque Information Saved successfully!" });
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
                }
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }



        [HttpGet]
        public async Task<IActionResult> CheckChequeExists(string chequeNo, string voucherNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                bool exists = await dbContext.Tbl20113ChequeMasters
                .AnyAsync(c => c.ChequeNo == chequeNo && c.VoucherNo != voucherNo); // Exclude current voucher

                return Json(exists);
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }


        [HttpGet]
        public async Task<ActionResult> GetPreview(DataSourceLoadOptions loadOptions, [FromBody] List<Tbl201VoucherEntry> voucherEntries)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                if (voucherEntries == null || !voucherEntries.Any())
                {
                    return BadRequest(new { success = false, message = "Invalid data received." });
                }

                try
                {
                    // Retrieve updated data for the submitted vouchers
                    var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
                    var qryListOfAccountlists = dbContext.Qry201VoucherEntryScreenDisplays
                        .Where(p => voucherNos.Contains(p.VoucherNo))
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

                    // Return the updated data as a JSON response
                    return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
                }
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }


        [HttpPost]
        public async Task<ActionResult> DeleteVoucherEntry(DataSourceLoadOptions loadOptions, long voucherEntryNo, string VoucherNo, string PaymentAccoutHeadName, int Gridcount)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {

                try
                {
                    Tbl201VoucherMaster voucherMaster = new();
                    int aEntryAmount = 0;
                    int Amt = 0;
                    var Remarks = "";
                    bool IsMatchingEntry = false;
                    var ExistingAccHeadID = "";
                    //var matchingEntries;
                    List<VoucherEntryDisplayDTO> matchingEntries = new();
                    //  var voucherNos1 = voucherEntries.Select(ve => ve.VoucherNo).Distinct();


                    // Get the list of updated vouchers
                    var voucherEntries1 = dbContext.Tbl201VoucherEntries
                                                  .Where(ve => ve.VoucherNo == VoucherNo) // Filter by the provided VoucherNo
                                                  .ToList();

                    var voucherNos1 = voucherEntries1.Select(ve => ve.VoucherNo).Distinct();


                    var qryListOfAccountlists1 = dbContext.Qry201VoucherEntryScreenDisplays
                        .Where(p => voucherNos1.Contains(p.VoucherNo))
                        .OrderBy(i => i.DrCr == "Cr" ? 1 : 0) // Ensures "Dr" entries come first
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

                    var resultList1 = await qryListOfAccountlists1.ToListAsync();
                    int crCount1 = resultList1.Count(i => i.DrCr == "Cr");
                    if (crCount1 >= 2)
                    {
                        matchingEntries = resultList1.Where(x => x.AccountHead == PaymentAccoutHeadName).ToList();
                        if (matchingEntries.Count != 0)
                        {
                            ExistingAccHeadID = matchingEntries[0].AccountHead;
                            IsMatchingEntry = true;
                        }
                    }

                    bool isVoucherExists = dbContext.Tbl201VoucherMasters
                   .Any(v => v.VoucherNo == voucherEntries1[0].VoucherNo);
                    var record = await dbContext.Tbl201VoucherEntries.FirstOrDefaultAsync(v => v.VoucherEntryNo == voucherEntryNo);
                    var SubLedgerRecord = await dbContext.Tbl201SubLedgerMasters.FirstOrDefaultAsync(v => v.VoucherEntryNo == voucherEntryNo);
                    var PropertyAllocationRecord = await dbContext.Tbl20122PropertyAllocationMasters.FirstOrDefaultAsync(v => v.VoucherEntryId == voucherEntryNo);
                    var CostAllocationRecord = await dbContext.Tbl201CostAllocationMasters.FirstOrDefaultAsync(v => v.VoucherEntryId == voucherEntryNo);
                    var SalaryPayableRecord = await dbContext.Tbl20114SalaryPayableMasters.FirstOrDefaultAsync(v => v.VoucherEntryNo == voucherEntryNo);
                    var EmpAllocationRecord = await dbContext.Tbl20104EmployeeAllocationMasters.FirstOrDefaultAsync(v => v.VoucherEntryId == voucherEntryNo);



                    var voucherNo = voucherEntries1[0].VoucherNo;
                    var masterrecord = dbContext.Tbl201VoucherMasters.FirstOrDefault(v => v.VoucherNo == voucherNo);
                    if (!isVoucherExists)
                    {
                        voucherMaster.VoucherNo = voucherEntries1[0].VoucherNo;
                        voucherMaster.VoucherDate = DateTime.Now;

                    }

                    // Add entries to the database
                    //dbContext.Tbl201VoucherEntries.AddRange(voucherEntries);
                    //await dbContext.SaveChangesAsync();


                    if (record == null)
                    {
                        return NotFound(new { message = "Record not found!" });
                    }

                    if (record.VoucherAmount != 0)
                    {
                        if (record != null)
                        {
                            dbContext.Tbl201VoucherEntries.Remove(record);
                        }
                        else if (SubLedgerRecord != null)
                        {
                            dbContext.Tbl201SubLedgerMasters.Remove(SubLedgerRecord);

                        }
                        else if (PropertyAllocationRecord != null)
                        {
                            dbContext.Tbl20122PropertyAllocationMasters.Remove(PropertyAllocationRecord);
                        }
                        else if (SubLedgerRecord != null)
                        {
                            dbContext.Tbl201CostAllocationMasters.Remove(CostAllocationRecord);
                        }
                        else if (SubLedgerRecord != null)
                        {
                            dbContext.Tbl20104EmployeeAllocationMasters.Remove(EmpAllocationRecord);
                        }

                        //  dbContext.Tbl201VoucherMasters.Remove(masterrecord);
                        await dbContext.SaveChangesAsync();


                    }


                    //SaveVoucher(voucherEntries);

                    // Get the list of updated vouchers
                    var voucherEntries = dbContext.Tbl201VoucherEntries
                                                  .Where(ve => ve.VoucherNo == VoucherNo) // Filter by the provided VoucherNo
                                                  .ToList();


                    var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
                    var qryListOfAccountlists = dbContext.Qry201VoucherEntryScreenDisplays
                        .Where(p => voucherNos.Contains(p.VoucherNo))
                        .OrderBy(i => i.DrCr == "Cr" ? 1 : 0) // Ensures "Dr" entries come first
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

                    int debitamt = 0; // Initialize debit amount

                    int crCount = resultList.Count(i => i.DrCr == "Cr");

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
                    }

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
                                // Calculate Debit Amount (DrAmount)

                                debitamt = (int)(debitamt + entry.DrAmount);

                                // If Dr/Cr is Credit ("Cr"), perform specific logic
                                if (entry.DrCr == "Cr" && crCount == 1)
                                // if (crCount==1)
                                {
                                    // Check if an existing entry matches
                                    var existingEntry = dbContext.Tbl201VoucherEntries
                                                                .FirstOrDefault(v => v.AccountHead == entry.AccountHead
                                                                                  && v.DrCr == "Cr"
                                                                                  && v.VoucherNo == entry.VoucherNo);

                                    decimal? SubAmt = existingEntry.VoucherAmount - debitamt;

                                    if (existingEntry != null)
                                    {
                                        // Update CrAmount by adding the calculated debit amount
                                        entry.CrAmount = debitamt;
                                        entry.SysRemarks = Remarks;

                                        // Optionally update the existing entry in the database
                                        existingEntry.VoucherAmount = entry.CrAmount;
                                        existingEntry.SysRemarks = entry.SysRemarks;
                                        dbContext.Tbl201VoucherEntries.Update(existingEntry);
                                        dbContext.SaveChanges();

                                    }
                                    else
                                    {
                                        // If no existing entry, assign CrAmount as debitamt
                                        entry.CrAmount = debitamt;
                                    }
                                }
                                else if (newCrEntry != null && crCount >= 2 && entry.AccountHead == ExistingAccHeadID)//&& entry.AccountHead == pettycashid)
                                {
                                    if (entry.DrCr == "Cr")
                                    {

                                        if (IsMatchingEntry == true)
                                        {
                                            // Accumulate `aEntryAmount` correctly
                                            foreach (var mEntry in matchingEntries)
                                            {
                                                aEntryAmount += (int)mEntry.CrAmount; // Accumulate CrAmount correctly

                                            }

                                            // Process voucherEntries
                                            foreach (var mVoucherEntry in voucherEntries)
                                            {
                                                int eAmount = (int)mVoucherEntry.VoucherAmount;
                                                if (aEntryAmount == 0)
                                                {
                                                    Amt = Amt - eAmount;
                                                }
                                                else
                                                {
                                                    Amt = aEntryAmount - eAmount;
                                                }

                                            }

                                            // Check if an existing entry matches
                                            var existingEntry = dbContext.Tbl201VoucherEntries
                                                                        .FirstOrDefault(v => v.AccountHead == entry.AccountHead
                                                                                          && v.DrCr == "Cr"
                                                                                          && v.VoucherNo == entry.VoucherNo);

                                            if (existingEntry != null)
                                            {
                                                // Update CrAmount by adding the calculated debit amount
                                                entry.CrAmount = Amt;
                                                entry.SysRemarks = Remarks;
                                                // Optionally update the existing entry in the database
                                                existingEntry.VoucherAmount = entry.CrAmount;
                                                existingEntry.SysRemarks = entry.SysRemarks;
                                                dbContext.Tbl201VoucherEntries.Update(existingEntry);
                                                dbContext.SaveChanges();

                                            }
                                            else
                                            {
                                                // If no existing entry, assign CrAmount as debitamt
                                                // entry.CrAmount = debitamt;
                                            }
                                        }

                                    }

                                }

                            }
                            entry.AccountHead = accountHead;
                            if (entry.DrCr == "Cr" && crCount == 1)
                            {
                                entry.DrAmount = 0;
                                entry.CrAmount = 0;
                                entry.SysRemarks = "";
                            }
                            if (Remarks == "")
                            {
                                Remarks = entry.AccountHead;
                            }
                            else
                            {
                                Remarks = Remarks + "," + entry.AccountHead;
                            }

                        }

                    }

                    return Json(DataSourceLoader.Load(resultList.AsQueryable(), loadOptions));
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });

        }


        [HttpPost]
        public async Task<ActionResult> DeleteBPVoucherEntry(DataSourceLoadOptions loadOptions, long voucherEntryNo, string VoucherNo, string PaymentAccoutHeadName, int Gridcount)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {

                try
                {
                    Tbl201VoucherMaster voucherMaster = new();
                    int aEntryAmount = 0;
                    int Amt = 0; decimal? mCrAmt = 0;
                    var Remarks = "";
                    bool IsMatchingEntry = false;
                    var ExistingAccHeadID = "";
                    //var matchingEntries;
                    List<VoucherEntryDisplayDTO> matchingEntries = new();
                    //  var voucherNos1 = voucherEntries.Select(ve => ve.VoucherNo).Distinct();


                    // Get the list of updated vouchers
                    var voucherEntries1 = dbContext.Tbl201VoucherEntries
                                                  .Where(ve => ve.VoucherNo == VoucherNo) // Filter by the provided VoucherNo
                                                  .ToList();

                    var voucherNos1 = voucherEntries1.Select(ve => ve.VoucherNo).Distinct();
                    int ExistingCrAmt = 0;

                    var qryListOfAccountlists1 = dbContext.Qry201VoucherEntryScreenDisplays
                        .Where(p => voucherNos1.Contains(p.VoucherNo))
                        .OrderBy(i => i.DrCr == "Cr" ? 1 : 0) // Ensures "Dr" entries come first
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

                    var resultList1 = await qryListOfAccountlists1.ToListAsync();
                    int crCount1 = resultList1.Count(i => i.DrCr == "Cr");
                    int drCount1 = resultList1.Count(i => i.DrCr == "Dr");
                    if (crCount1 >= 2)
                    {
                        matchingEntries = resultList1.Where(x => x.AccountHead == PaymentAccoutHeadName).ToList();
                        if (matchingEntries.Count != 0)
                        {
                            ExistingAccHeadID = matchingEntries[0].AccountHead;
                            IsMatchingEntry = true;
                        }
                    }

                    bool isVoucherExists = dbContext.Tbl201VoucherMasters
                   .Any(v => v.VoucherNo == voucherEntries1[0].VoucherNo);
                    var record = await dbContext.Tbl201VoucherEntries.FirstOrDefaultAsync(v => v.VoucherEntryNo == voucherEntryNo);
                    var voucherNo = voucherEntries1[0].VoucherNo;
                    var masterrecord = dbContext.Tbl201VoucherMasters.FirstOrDefault(v => v.VoucherNo == voucherNo);
                    var SubLedgerRecord = await dbContext.Tbl201SubLedgerMasters.FirstOrDefaultAsync(v => v.VoucherEntryNo == voucherEntryNo);
                    var PropertyAllocationRecord = await dbContext.Tbl20122PropertyAllocationMasters.FirstOrDefaultAsync(v => v.VoucherEntryId == voucherEntryNo);
                    var CostAllocationRecord = await dbContext.Tbl201CostAllocationMasters.FirstOrDefaultAsync(v => v.VoucherEntryId == voucherEntryNo);
                    var SalaryPayableRecord = await dbContext.Tbl20114SalaryPayableMasters.FirstOrDefaultAsync(v => v.VoucherEntryNo == voucherEntryNo);
                    var EmpAllocationRecord = await dbContext.Tbl20104EmployeeAllocationMasters.FirstOrDefaultAsync(v => v.VoucherEntryId == voucherEntryNo);



                    if (!isVoucherExists)
                    {
                        voucherMaster.VoucherNo = voucherEntries1[0].VoucherNo;
                        voucherMaster.VoucherDate = DateTime.Now;
                        dbContext.Tbl201VoucherMasters.Remove(masterrecord);


                    }

                    // Add entries to the database
                    //dbContext.Tbl201VoucherEntries.AddRange(voucherEntries);
                    //await dbContext.SaveChangesAsync();

                    if (record == null)
                    {
                        return NotFound(new { message = "Record not found!" });
                    }
                    decimal? rAmt = 0;
                    bool IsRemoved = false;
                    if (record.VoucherAmount != 0)
                    {
                        if (record != null)
                        {
                             rAmt = record.VoucherAmount;
                            if(record.AccountHead != ExistingAccHeadID)
                            {
                                dbContext.Tbl201VoucherEntries.Remove(record);
                                IsRemoved = true;
                            }
                        }
                        else if (SubLedgerRecord != null)
                        {
                            dbContext.Tbl201SubLedgerMasters.Remove(SubLedgerRecord);

                        }
                        else if (PropertyAllocationRecord != null)
                        {
                            dbContext.Tbl20122PropertyAllocationMasters.Remove(PropertyAllocationRecord);
                        }
                        else if (SubLedgerRecord != null)
                        {
                            dbContext.Tbl201CostAllocationMasters.Remove(CostAllocationRecord);
                        }
                        else if (SubLedgerRecord != null)
                        {
                            dbContext.Tbl20104EmployeeAllocationMasters.Remove(EmpAllocationRecord);
                        }

                        //  dbContext.Tbl201VoucherMasters.Remove(masterrecord);
                        await dbContext.SaveChangesAsync();


                    }

                    //SaveVoucher(voucherEntries);

                    // Get the list of updated vouchers
                    var voucherEntries = dbContext.Tbl201VoucherEntries
                                                  .Where(ve => ve.VoucherNo == VoucherNo) // Filter by the provided VoucherNo
                                                  .ToList();


                    var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
                    var qryListOfAccountlists = dbContext.Qry201VoucherEntryScreenDisplays
                        .Where(p => voucherNos.Contains(p.VoucherNo))
                        .OrderBy(i => i.DrCr == "Cr" ? 1 : 0) // Ensures "Dr" entries come first
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

                    int debitamt = 0; // Initialize debit amount

                    int crCount = resultList.Count(i => i.DrCr == "Cr");
                    int drCount = resultList.Count(i => i.DrCr == "Dr");

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
                    }

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
                                // Calculate Debit Amount (DrAmount)

                                debitamt = (int)(debitamt + entry.DrAmount);

                                // If Dr/Cr is Credit ("Cr"), perform specific logic
                                if (entry.DrCr == "Cr" && crCount == 1)
                                // if (crCount==1)
                                {
                                    // Check if an existing entry matches
                                    var existingEntry = dbContext.Tbl201VoucherEntries
                                                                .FirstOrDefault(v => v.AccountHead == entry.AccountHead
                                                                                  && v.DrCr == "Cr"
                                                                                  && v.VoucherNo == entry.VoucherNo);

                                    decimal? SubAmt = existingEntry.VoucherAmount - debitamt;

                                    if (existingEntry != null)
                                    {
                                        // Update CrAmount by adding the calculated debit amount
                                        entry.CrAmount = debitamt;
                                        entry.SysRemarks = Remarks;

                                        // Optionally update the existing entry in the database
                                        existingEntry.VoucherAmount = entry.CrAmount;
                                        existingEntry.SysRemarks = entry.SysRemarks;
                                        dbContext.Tbl201VoucherEntries.Update(existingEntry);
                                        dbContext.SaveChanges();

                                    }
                                    else
                                    {
                                        // If no existing entry, assign CrAmount as debitamt
                                        entry.CrAmount = debitamt;
                                    }
                                }
                                else if (newCrEntry != null && crCount >= 2 && entry.AccountHead == ExistingAccHeadID && IsRemoved==true)
                                {
                                    if (entry.DrCr == "Cr")
                                    {

                                        if (IsMatchingEntry == true)
                                        {
                                            // Accumulate `aEntryAmount` correctly
                                            foreach (var mEntry in matchingEntries)
                                            {
                                                aEntryAmount += (int)mEntry.CrAmount; // Accumulate CrAmount correctly //Matching entries  - Delete entry
                                                mCrAmt = aEntryAmount - rAmt;
                                            }

                                            // Process voucherEntries
                                            foreach (var mVoucherEntry in voucherEntries)
                                            {
                                                int eAmount = (int)mVoucherEntry.VoucherAmount;
                                                if (aEntryAmount == 0)
                                                {
                                                    Amt = Amt - eAmount;
                                                }
                                                else
                                                {
                                                    Amt = aEntryAmount - eAmount;
                                                }

                                            }

                                            // Check if an existing entry matches
                                            var existingEntry = dbContext.Tbl201VoucherEntries
                                                                        .FirstOrDefault(v => v.AccountHead == entry.AccountHead
                                                                                          && v.DrCr == "Cr"
                                                                                          && v.VoucherNo == entry.VoucherNo);

                                            if (existingEntry != null)
                                            {
                                                // Update CrAmount by adding the calculated debit amount
                                                //entry.CrAmount = Amt;
                                                entry.CrAmount = mCrAmt;
                                                entry.SysRemarks = Remarks;
                                                // Optionally update the existing entry in the database
                                                existingEntry.VoucherAmount = entry.CrAmount;
                                                existingEntry.SysRemarks = entry.SysRemarks;
                                                dbContext.Tbl201VoucherEntries.Update(existingEntry);
                                                dbContext.SaveChanges();

                                            }
                                            else
                                            {
                                                // If no existing entry, assign CrAmount as debitamt
                                                // entry.CrAmount = debitamt;
                                            }
                                        }

                                    }

                                }

                            }
                            entry.AccountHead = accountHead;
                            if (entry.DrCr == "Cr" && crCount == 1 && drCount == 0)
                            {
                                entry.DrAmount = 0;
                                entry.CrAmount = 0;
                                entry.SysRemarks = "";
                            }

                            if (Remarks == "")
                            {
                                Remarks = entry.AccountHead;
                            }
                            else
                            {
                                Remarks = Remarks + "," + entry.AccountHead;
                            }

                        }

                    }

                    

                    return Json(DataSourceLoader.Load(resultList.AsQueryable(), loadOptions));
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
                }
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });

        }


        [HttpPost]
        public async Task<ActionResult> DeleteAllVoucherEntry(DataSourceLoadOptions loadOptions, string VoucherNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    // Find all records matching the given VoucherNo
                    var records = await dbContext.Tbl201VoucherEntries
                                                .Where(v => v.VoucherNo == VoucherNo)
                                                .ToListAsync();

                    if (records == null || !records.Any())
                    {
                        return NotFound(new { message = "No records found for the provided VoucherNo!" });
                    }

                    // Remove all matching records
                    dbContext.Tbl201VoucherEntries.RemoveRange(records);
                    await dbContext.SaveChangesAsync();
                    // ✅ Log the deletion action
                    await _userActionLogger.LogAsync(
                        module: "Finance > Delete Receipts",
                        actionDetail: $"Deleted all voucher entries for VoucherNo: {VoucherNo}. Total deleted: {records.Count}",
                        documentNo: VoucherNo
                    );
                    // Fetch updated voucher list
                    var voucherEntries = await dbContext.Tbl201VoucherEntries
                                                   .Where(ve => ve.VoucherNo == VoucherNo)
                                                   .ToListAsync();

                    var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct().ToList();

                    // Query the updated display list
                    var qryListOfAccountLists = dbContext.Qry201VoucherEntryScreenDisplays
                                                        .Where(p => voucherNos.Contains(p.VoucherNo))
                                                        .OrderBy(i => i.DrCr == "Dr")
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

                    // Return the modified list for DataSourceLoader
                    return Json(DataSourceLoader.Load(resultList.AsQueryable(), loadOptions));
                }
                catch (Exception ex)
                {
                    // Return a detailed error response
                    return StatusCode(500, new { message = "An error occurred while deleting the records.", error = ex.Message });
                }
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });

        }

        [HttpPost]
        public async Task<ActionResult> VerifyVoucher(Tbl201VoucherMaster voucherMaster)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var UserName = HttpContext.Session.GetString("UserName");

                    if (string.IsNullOrEmpty(voucherMaster.VoucherNo))
                    {
                        return BadRequest(new { Message = "Voucher number is required." });
                    }

                    var voucher = dbContext.Tbl201VoucherMasters.FirstOrDefault(v => v.VoucherNo == voucherMaster.VoucherNo);

                    if (voucher == null)
                    {
                        return NotFound(new { Message = "Voucher not found." });
                    }

                    // Update the fields
                    voucher.IsVerified = true;
                    voucher.VoucherVerifiedOn = DateTime.Now;
                    voucher.VoucherVerifiedBy = UserName;

                    dbContext.SaveChanges();

                    return Ok(new
                    {
                        Message = "Voucher verified successfully.",
                        VoucherVerifiedBy = UserName,  // Example, replace with actual data if needed
                                                       //VoucherVerifiedOn = voucher.VoucherApprovedOn.ToString("dd-MMM-yyyy")
                    });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { Message = ex.Message });
                }
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });

        }


        [HttpPost]
        public async Task<ActionResult> ApproveVoucher(string VoucherNo, bool IsDirect)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var UserName = HttpContext.Session.GetString("UserName");

                    if (string.IsNullOrEmpty(VoucherNo))
                    {
                        return BadRequest(new { Message = "Voucher number is required." });
                    }

                    var voucher = dbContext.Tbl201VoucherMasters.FirstOrDefault(v => v.VoucherNo == VoucherNo);

                    if (voucher == null)
                    {
                        return NotFound(new { Message = "Voucher not found." });
                    }

                    // Update the fields
                    voucher.IsApproved = true;
                    voucher.VoucherApprovedOn = DateTime.Now;
                    voucher.VoucherApprovedBy = UserName;

                    if (IsDirect == true)
                    {
                        voucher.IsVerified = true;
                        voucher.VoucherVerifiedOn = DateTime.Now;
                        voucher.VoucherVerifiedBy = UserName;

                    }

                    dbContext.SaveChanges();

                    return Ok(new
                    {
                        Message = "Voucher verified successfully.",
                        VoucherApprovedBy = UserName,  // Example, replace with actual data if needed
                                                       //VoucherVerifiedOn = voucher.VoucherApprovedOn.ToString("dd-MMM-yyyy")
                    });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { Message = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });

        }


        [HttpPost]
        public async Task<IActionResult> Delete(DataSourceLoadOptions loadOptions, long voucherEntryNo, string VoucherNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                if (voucherEntryNo == 0 || string.IsNullOrEmpty(VoucherNo))
                {

                    return BadRequest(new { success = false, message = "VoucherEntryNo and VoucherNo are required." });
                }

                try
                {
                    var allItems = await dbContext.Tbl201VoucherEntries.ToListAsync();
                    var item = allItems.FirstOrDefault(p => p.VoucherEntryNo == voucherEntryNo);
                    if (item == null)
                    {

                        return NotFound(new { success = false, message = "Record not found." });
                    }


                    dbContext.Tbl201VoucherEntries.Remove(item);
                    await dbContext.SaveChangesAsync();


                    var qryListOfAccountlists = dbContext.Qry201VoucherEntryScreenDisplays
                        .Where(p => p.VoucherNo == VoucherNo)
                        .Select(i => new
                        {
                            i.VoucherNo,
                            i.VoucherEntryNo,
                            i.DrCr,
                            i.DrAmount,
                            i.CrAmount,
                            i.EntryNarration,
                            i.AccountHead,
                            i.SysRemarks,
                        });


                    if (!qryListOfAccountlists.Any())
                    {
                        return Json(new { success = true, message = "No records found after deletion." });
                    }


                    var result = await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions);

                    return Json(result);
                }

                catch (Exception ex)
                {
                    // Handle unexpected errors
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "An error occurred while deleting the record.",
                        details = ex.Message
                    });
                }
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });

        }

        [HttpGet]
        public IActionResult GetEmployeeName()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var data = dbContext.Tbl101Employees
                .Select(c => new
                {
                    c.EmployeeId,
                    c.EmployeeName,
                    c.NationalId

                }).ToList();

                return Ok(data);
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });

        }
        [HttpPost]
        public async Task<ActionResult> SaveCostAllocation([FromBody] Tbl20104EmployeeAllocationMaster EM)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    dbContext.Tbl20104EmployeeAllocationMasters.Add(EM);
                    await dbContext.SaveChangesAsync();
                    //return Json(new { VoucherEntryNo = VE.VoucherNo });
                    return Ok(new { success = true, message = "Data inserted successfully!" });
                }
                catch (Exception ex)
                {

                    return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
                }


            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });

        }


        [HttpGet]
        public async Task<IActionResult> GetEditAccountHead(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {

                try
                {
                    var qryListOfAccountlists = dbContext.Qry201ListOfAccounts
                  .Select(i => new
                  {
                      i.AccountId,
                      i.AccountHead,
                      i.AccountHeadArabic

                  });

                    return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));

                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });

        }


        public IActionResult CostAllocation(string voucherNo, string accountHead, string voucherAmount, string drCr, long voucherEntryNo)

        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {

                try
                {
                    // Log or debug the incoming parameters

                    ViewBag.VoucherNo = voucherNo;

                    ViewBag.AccountHead = accountHead;

                    ViewBag.VoucherAmount = voucherAmount;

                    ViewBag.DrCr = drCr;

                    ViewBag.VoucherEntryNo = voucherEntryNo;

                    return PartialView("~/Areas/Finance/Views/_CostAllocation.cshtml"); // Ensure this is inside /Views/VoucherEntryReceipts/
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });

        }
        public IActionResult TargetAction(int CurrencyExchangeId)
        {
            ViewData["CurrencyExchangeId"] = CurrencyExchangeId;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> UpdateVoucher([FromBody] Tbl201VoucherMaster VM)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {

                if (VM == null || string.IsNullOrWhiteSpace(VM.VoucherNo))
                {
                    return BadRequest(new { success = false, message = "Invalid data received." });
                }

                try
                {
                    var existingVoucher = await dbContext.Tbl201VoucherMasters
                                                        .FirstOrDefaultAsync(v => v.VoucherNo == VM.VoucherNo);

                    var UserId = HttpContext.Session.GetString("UserId");
                    var TenantName = HttpContext.Session.GetString("TenantName");

                    //               var existingVoucher = await dbContext.Tbl201VoucherMasters
                    //.Where(v => v.VoucherNo == VM.VoucherNo)
                    //.Select(v => new { v.VoucherNo,
                    //    v.VoucherDate,v.VoucherRefNo,v.VoucherNarration,v.VoucherEnteredBy,v.VoucherEnteredOn,
                    //v.VoucherVerifiedBy,v.VoucherVerifiedOn,v.IsApproved,v.IsAuditVerified,v.IsVerified,v.DeliveryNoteNo,
                    //v.CogsInvoiceNo,
                    //    v.ReferenceNote,v.RentalPayslipNo,
                    //    v.currencyrate,
                    //    v.VoucherModifiedBy,
                    //    v.VoucherModifiedOn,

                    //v.SalesPersonCode,v.BillNo,v.BillDate,v.BillPaidTo,v.BillRemarks,v.InvoiceNoOfDays})
                    //.FirstOrDefaultAsync();




                    if (existingVoucher != null)
                    {
                        dbContext.Entry(existingVoucher).State = EntityState.Detached;
                        dbContext.Entry(VM).State = EntityState.Modified;
                        // Update existing record
                        dbContext.Entry(existingVoucher).CurrentValues.SetValues(VM);
                    }
                    else
                    {
                        // Insert new record
                        dbContext.Tbl201VoucherMasters.Add(VM);
                    }

                    await dbContext.SaveChangesAsync();

                    var notifyRequest = new NotificationRequest
                        {
                             UserId = UserId, // or fetch from session/DB
                             VoucherName = VM.VoucherNo,
                             ActionType = "You have one Payment Voucher to verify",
                            TenantName = TenantName 
                    };

                await _fcmService.SendNotificationAsync(notifyRequest);

                    return Ok(new { success = true, message = existingVoucher != null ? "Voucher updated successfully!" : "Voucher inserted successfully!" });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
                }
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });

        }

        //[HttpPost]
        //public async Task<ActionResult> UpdateVoucher([FromBody] UpdateSalesVoucher VM)
        //{
        //    if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
        //        return Unauthorized(new { message = "Invalid tenant.", success = false });

        //    if (VM == null || string.IsNullOrWhiteSpace(VM.VoucherNo))
        //        return BadRequest(new { success = false, message = "Invalid data received." });

        //    try
        //    {
        //        var existingVoucher = await dbContext.UpdateSalesVouchers
        //            .FirstOrDefaultAsync(v => v.VoucherNo == VM.VoucherNo);

        //        //if (existingVoucher != null)
        //        //{
        //        //    // Map values from VM to existingVoucher manually or via AutoMapper
        //        //    existingVoucher.VoucherDate = VM.VoucherDate;
        //        //    existingVoucher.VoucherNarration = VM.VoucherNarration;
        //        //    existingVoucher.InvoiceSubmittedDate = VM.InvoiceSubmittedDate;
        //        //    existingVoucher.UseSubmittedDate = VM.UseSubmittedDate;
        //        //    // TODO: map all other relevant fields here

        //        //    dbContext.Tbl201VoucherMasters.Update(existingVoucher);
        //        //}
        //        //else
        //        //{
        //        //    // Map VM to entity
        //        //    var newVoucher = new Tbl201VoucherMaster
        //        //    {
        //        //        VoucherNo = VM.VoucherNo,
        //        //        VoucherDate = VM.VoucherDate,
        //        //        VoucherNarration = VM.VoucherNarration,
        //        //        InvoiceSubmittedDate = VM.InvoiceSubmittedDate,
        //        //        UseSubmittedDate = VM.UseSubmittedDate,
        //        //        // TODO: map all other relevant fields here
        //        //    };

        //        //    dbContext.Tbl201VoucherMasters.Add(newVoucher);
        //        //}

        //        await dbContext.SaveChangesAsync();

        //        return Ok(new
        //        {
        //            success = true,
        //            message = existingVoucher != null ? "Voucher updated successfully!" : "Voucher inserted successfully!"
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
        //    }
        //}




        [HttpPost]
        public IActionResult UpdateVoucherEntry([FromBody] Tbl201VoucherEntry model)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {

                if (model == null || string.IsNullOrEmpty(model.AccountHead))
                {
                    return BadRequest("Invalid data: AccountHead is missing or null.");
                }

                var existingEntry = dbContext.Tbl201VoucherEntries
                    .FirstOrDefault(v => v.VoucherEntryNo == model.VoucherEntryNo);

                var accountID = dbContext.Tbl201ChartOfAccounts
                    .Where(a => a.AccountHead == model.AccountHead)
                    .Select(a => a.AccountId)
                    .FirstOrDefault();

                if (existingEntry != null)
                {
                    existingEntry.DrCr = model.DrCr;
                    existingEntry.AccountHead = accountID; // Assign single account ID
                    existingEntry.EntryNarration = model.EntryNarration;
                    existingEntry.SysRemarks = model.SysRemarks; // Ensure SysRemarks is updated



                    dbContext.SaveChanges();
                    return Ok(new { message = "" });
                }

                return NotFound("Voucher Entry not found.");
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });

        }

        public IActionResult LoadChequePopup()
        {
            return PartialView("~/Areas/Finance/Views/_ChequePopup.cshtml");
        }

        [HttpGet]
        public async Task<IActionResult> CheckIsMaintainSalary(string AccountHead, string AccountID)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {

                try
                {
                    var allocation = await dbContext.Tbl201ChartOfAccounts
                        .Where(x => x.AccountHead == AccountHead && x.AccountId == AccountID && x.IsEmployeePaymentAc == true)
                        .FirstOrDefaultAsync();

                    if (allocation != null)
                    {
                        return Ok(new { isAllocated = true });
                    }
                    else
                    {
                        return Ok(new { isAllocated = false });
                    }
                }
                catch (Exception ex)
                {
                    // Log the error here if necessary
                    return StatusCode(500, new { message = "An error occurred while checking property allocation.", error = ex.Message });
                }
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });


        }
        public IActionResult SalaryEntry(string voucherNo, string accountHead, string voucherAmount, string drCr, long voucherEntryNo)
        {
            // Log or debug the incoming parameters
            ViewBag.VoucherNo = voucherNo;
            ViewBag.AccountHead = accountHead;
            ViewBag.VoucherAmount = voucherAmount;
            ViewBag.DrCr = drCr;
            ViewBag.VoucherEntryNo = voucherEntryNo;

            return PartialView("~/Areas/Finance/Views/_SalaryPayable.cshtml"); // Ensure this is inside /Views/VoucherEntryReceipts/
        }
        [HttpGet]
        public IActionResult GetVoucherEntry(long VoucherEntryNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    // Fetch updated data and join with Qry201ListOfAccounts to get AccountHead
                    var voucherEntry = (from v in dbContext.Qry201VoucherEntryScreenDisplays
                                        join a in dbContext.Qry201ListOfAccounts
                                        on v.AccountHead equals a.AccountId // Assuming AccountHead stores AccountId
                                        where v.VoucherEntryNo == VoucherEntryNo
                                        select new
                                        {
                                            v.VoucherEntryNo,
                                            v.DrCr,
                                            AccountHead = a.AccountHead, // Get Account Name instead of AccountId
                                            v.DrAmount,
                                            v.CrAmount,
                                            v.EntryNarration,
                                            v.SysRemarks
                                        }).FirstOrDefault();

                    if (voucherEntry == null)
                    {
                        return NotFound(new { message = "Voucher Entry not found" });
                    }

                    return Ok(voucherEntry);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "Internal server error", error = ex.Message });
                }
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });


        }

        public async Task<IActionResult> GetDefaultReceivingAccount()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var defaultAccount = await dbContext.Tbl201ChartOfAccounts
                .Where(a => a.IsDefaultForCash == true && a.AccountGroupId == "A012")
                .OrderByDescending(a => a.RecordModifiedOn) // Get the latest default account
                .Select(a => new { a.AccountId, a.AccountHead })
                .FirstOrDefaultAsync();

                return Json(defaultAccount);
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });


        }

        public async Task<IActionResult> GetDefaultPaymentAccount()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var defaultAccount = await dbContext.Tbl201ChartOfAccounts
                .Where(a => a.IsDefaultForCash == true && a.AccountGroupId == "A013")
                .OrderByDescending(a => a.RecordModifiedOn) // Get the latest default account
                .Select(a => new { a.AccountId, a.AccountHead })
                .FirstOrDefaultAsync();

                return Json(defaultAccount);
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });

        }

        [HttpPost]
        public async Task<ActionResult> UpdatePurchaseMasterDetails(Tbl20166VatpurchaseMaster InvoiceMaster)
        {
            if (InvoiceMaster == null)
            {
                return BadRequest(new { success = false, message = "Invalid invoice data received." });
            }

            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var existingInvoice = await dbContext.Tbl20166VatpurchaseMasters
                                                                 .FirstOrDefaultAsync(v => v.PurchaseVoucherNo == InvoiceMaster.PurchaseVoucherNo);
                    
                    var UserId = HttpContext.Session.GetString("UserId");
                    var TenantName = HttpContext.Session.GetString("TenantName");

                    if (existingInvoice != null)
                    {
                        // Update existing master record
                        dbContext.Entry(existingInvoice).CurrentValues.SetValues(InvoiceMaster);
                    }
                    else
                    {
                        // Insert new invoice master record
                        await dbContext.Tbl20166VatpurchaseMasters.AddAsync(InvoiceMaster);
                    }


                    await dbContext.SaveChangesAsync();
                    // await transaction.CommitAsync();

                    var notifyRequest = new NotificationRequest
                        {
                             UserId = UserId, // or fetch from session/DB
                             VoucherName = InvoiceMaster.PurchaseVoucherNo,
                             ActionType = "You have one Purchase Invoice to verify",
                            TenantName = TenantName 
                    };

                await _fcmService.SendNotificationAsync(notifyRequest);



                    return Ok(new { success = true, message = existingInvoice != null ? "Invoice and child records updated successfully!" : "New invoice and child records added successfully!" });
                }
            }
            catch (Exception ex)
            {
                // await transaction.RollbackAsync();
                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }


            return BadRequest("Failed to retrieve tenant and database context.");
        }




        [HttpPost]
        public async Task<ActionResult> UpdatePurchaseChildDetails(List<Tbl20167VatpurchaseChild> InvoiceChildren)
        {
            if (InvoiceChildren == null)
            {
                return BadRequest(new { success = false, message = "Invalid or empty invoice data received." });
            }

            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    foreach (var child in InvoiceChildren)
                    {
                        // if (child == null) continue;

                        if (child.PurchaseChildSlNo == null || child.PurchaseChildSlNo == 0)
                        {
                            if(child.TaxSlabCode==null)
                            {
                                child.TaxSlabCode = 2;
                            }
                            // Create a new instance for each child
                            var aTbl20167VatpurchaseChild = new Tbl20167VatpurchaseChild
                            {
                                PurchaseVoucherNo = child.PurchaseVoucherNo,
                                UnitRate = child.UnitRate, // Ensure null safety
                                DetailedDescription = child.DetailedDescription, // Null safety
                                QuantityInvoiced = child.QuantityInvoiced, // Null safety
                                TaxSlabCode = child.TaxSlabCode,
                                UnitsToBill = 1,
                                Discount = child.Discount,
                                UnitRateMethod = child.UnitRateMethod,
                                ItemCode = child.ItemCode ?? string.Empty, // Null safety
                                UoM = "Each"
                                // Do NOT set the ID or primary key if it is auto-incremented
                            };

                            await dbContext.Tbl20167VatpurchaseChildren.AddAsync(aTbl20167VatpurchaseChild);
                        }
                        else
                        {
                            // Find and update existing child (Update)
                            var existingChild = await dbContext.Tbl20167VatpurchaseChildren
                                .FirstOrDefaultAsync(x => x.PurchaseChildSlNo == child.PurchaseChildSlNo);

                            if (existingChild != null)
                            {
                                existingChild.PurchaseVoucherNo = child.PurchaseVoucherNo;
                                existingChild.UnitRate = child.UnitRate;
                                existingChild.DetailedDescription = child.DetailedDescription;
                                existingChild.QuantityInvoiced = child.QuantityInvoiced;
                                existingChild.TaxSlabCode = child.TaxSlabCode;
                                existingChild.UnitsToBill = 1;
                                existingChild.Discount = child.Discount;
                                existingChild.UnitRateMethod = child.UnitRateMethod;
                                existingChild.ItemCode = child.ItemCode ?? string.Empty;
                                existingChild.UoM = "Each";

                                dbContext.Tbl20167VatpurchaseChildren.Update(existingChild);
                            }
                        }

                    }

                    await dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Invoice child records updated successfully!" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }

            return BadRequest("Failed to retrieve tenant and database context.");
        }


        [HttpPost]
        public async Task<ActionResult> UpdateInvoiceMasterDetails(Tbl20161VatinvoiceMaster InvoiceMaster)
        {
            if (InvoiceMaster == null)
            {
                return BadRequest(new { success = false, message = "Invalid invoice data received." });
            }

            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                        var existingInvoice = await dbContext.Tbl20161VatinvoiceMasters
                                                                 .FirstOrDefaultAsync(v => v.InvoiceNo == InvoiceMaster.InvoiceNo);

                    var UserId = HttpContext.Session.GetString("UserId");
                    var TenantName = HttpContext.Session.GetString("TenantName");

                    if (existingInvoice != null)
                    {
                        // Update existing master record
                        dbContext.Entry(existingInvoice).CurrentValues.SetValues(InvoiceMaster);
                    }
                    else
                    {
                        // Insert new invoice master record
                        await dbContext.Tbl20161VatinvoiceMasters.AddAsync(InvoiceMaster);
                    }


                    await dbContext.SaveChangesAsync();
                    // await transaction.CommitAsync();

                    var notifyRequest = new NotificationRequest
                        {
                             UserId = UserId, // or fetch from session/DB
                             VoucherName = InvoiceMaster.InvoiceNo,
                             ActionType = "You have one Sales Invoice to verify",
                            TenantName = TenantName 
                    };

                await _fcmService.SendNotificationAsync(notifyRequest);

                    return Ok(new { success = true, message = existingInvoice != null ? "Invoice and child records updated successfully!" : "New invoice and child records added successfully!" });
                }
            }
            catch (Exception ex)
            {
                // await transaction.RollbackAsync();
                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }


            return BadRequest("Failed to retrieve tenant and database context.");
        }




        [HttpPost]
        public async Task<ActionResult> UpdateInvoiceChildDetails(List<Tbl20162VatinvoiceChild> InvoiceChildren)
        {
            if (InvoiceChildren == null || InvoiceChildren.Count == 0)
            {
                return BadRequest(new { success = false, message = "Invalid or empty invoice data received." });
            }

            try
            {

                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var savedChildren = new List<Tbl20162VatinvoiceChild>();

                    foreach (var child in InvoiceChildren)
                    {
                        if (child.InvoiceChildSlNo == null || child.InvoiceChildSlNo == 0)
                        {
                            var newChild = new Tbl20162VatinvoiceChild
                            {
                                //InvoiceNo = child.InvoiceNo,
                                //UnitRate = child.UnitPrice?.GetDecimal() ?? 0m,
                                //DetailedDescription = child.Description?.GetString() ?? string.Empty,
                                //QuantityInvoiced = child.Qty,
                                //TaxSlabCode = child.TaxSlabCode?.GetByte() ?? (byte)8,
                                //Discount = child.Discount,
                                //UnitRateInOc= child.UnitPrice?.GetDecimal() ?? 0m,
                                //DiscountInOc = child.Discount,
                                //UnitsToBill = 1,
                                //UnitRateMethod = 49,
                                //ItemCode = child.ItemCode ?? string.Empty,
                                //UoM = "Each"

                                InvoiceNo = child.InvoiceNo,
                                UnitRate = child.UnitRate,
                                DetailedDescription = child.DetailedDescription,
                                QuantityInvoiced = child.QuantityInvoiced,
                                TaxSlabCode = child.TaxSlabCode ?? (byte)8,
                                Discount = child.Discount,
                                UnitRateInOc = child.UnitRate,
                                DiscountInOc = child.Discount,
                                UnitsToBill = 1,
                                UnitRateMethod = child.UnitRateMethod,
                                ItemCode = child.ItemCode ?? string.Empty,
                                UoM = "Each"
                            };

                            await dbContext.Tbl20162VatinvoiceChildren.AddAsync(newChild);
                            await dbContext.SaveChangesAsync();

                            // Set the generated ID back to the input model if needed
                            child.InvoiceChildSlNo = newChild.InvoiceChildSlNo;

                            savedChildren.Add(newChild);
                        }
                        else
                        {
                            var existingChild = await dbContext.Tbl20162VatinvoiceChildren
                                .FirstOrDefaultAsync(x => x.InvoiceChildSlNo == child.InvoiceChildSlNo);

                            if (existingChild != null)
                            {

                                existingChild.InvoiceNo = child.InvoiceNo;
                                existingChild.UnitRate = child.UnitRate;
                                existingChild.DetailedDescription = child.DetailedDescription;
                                existingChild.QuantityInvoiced = child.QuantityInvoiced;
                                existingChild.TaxSlabCode = child.TaxSlabCode;
                                existingChild.UnitsToBill = 1;
                                existingChild.UnitRateMethod = 49;
                                existingChild.ItemCode = child.ItemCode ?? string.Empty;
                                existingChild.UoM = "Each";
                                existingChild.Discount = child.Discount;
                                existingChild.UnitRateInOc = child.UnitRate;
                                existingChild.DiscountInOc = child.Discount;
                                dbContext.Tbl20162VatinvoiceChildren.Update(existingChild);
                                savedChildren.Add(existingChild);
                            }
                        }
                    }

                    await dbContext.SaveChangesAsync();

                    return Ok(new
                    {
                        success = true,
                        message = "Invoice child records saved successfully!",
                        data = savedChildren
                    });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Failed to retrieve tenant and database context." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }
        }


        [HttpPost]
        public async Task<ActionResult> UpdateCreditNoteMasterDetails(Tbl20170VatcreditNoteMaster InvoiceMaster)
        {
            if (InvoiceMaster == null)
            {
                return BadRequest(new { success = false, message = "Invalid invoice data received." });
            }

            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var existingInvoice = await dbContext.Tbl20170VatcreditNoteMasters
                                                                 .FirstOrDefaultAsync(v => v.CreditNoteNo == InvoiceMaster.CreditNoteNo);

                    var UserId = HttpContext.Session.GetString("UserId");
                    var TenantName = HttpContext.Session.GetString("TenantName");

                    if (existingInvoice != null)
                    {
                        // Update existing master record
                        dbContext.Entry(existingInvoice).CurrentValues.SetValues(InvoiceMaster);
                    }
                    else
                    {
                        // Insert new invoice master record
                        await dbContext.Tbl20170VatcreditNoteMasters.AddAsync(InvoiceMaster);
                    }


                    await dbContext.SaveChangesAsync();
                    // await transaction.CommitAsync();

                    var notifyRequest = new NotificationRequest
                        {
                             UserId = UserId, // or fetch from session/DB
                             VoucherName = InvoiceMaster.CreditNoteNo,
                             ActionType = "You have one Credit Note to verify",
                            TenantName = TenantName 
                    };

                    await _fcmService.SendNotificationAsync(notifyRequest);

                    return Ok(new { success = true, message = existingInvoice != null ? "Invoice and child records updated successfully!" : "New invoice and child records added successfully!" });
                }
            }
            catch (Exception ex)
            {
                // await transaction.RollbackAsync();
                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }


            return BadRequest("Failed to retrieve tenant and database context.");
        }


        [HttpPost]
        public async Task<ActionResult> UpdateCreditNoteChildDetails(List<Tbl20171VatcreditNoteChild> InvoiceChildren)
        {
            if (InvoiceChildren == null)
            {
                return BadRequest(new { success = false, message = "Invalid or empty invoice data received." });
            }

            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    foreach (var child in InvoiceChildren)
                    {
                        // if (child == null) continue;

                        if (child.CreditNoteChildSlNo == null || child.CreditNoteChildSlNo == 0)
                        {
                            // Create a new instance for each child
                            var aTbl20171VatcreditNoteChild = new Tbl20171VatcreditNoteChild
                            {

                                CreditNoteNo = child.CreditNoteNo,
                                UnitRate = child.UnitRate, // Ensure null safety
                                DetailedDescription = child.DetailedDescription, // Null safety
                                QuantityCredited = child.QuantityCredited, // Null safety
                                TaxSlabCode = child.TaxSlabCode ?? (byte)8,
                                UnitsToCredited = 1,
                                UnitRateInOc = child.UnitRate,
                                DiscountInOc = child.Discount,
                                Discount = child.Discount,
                                UnitRateMethod = child.UnitRateMethod,
                                //UnitRateMethod = 49,
                                ItemCode = child.ItemCode ?? string.Empty, // Null safety
                                UoM = "Each"
                                // Do NOT set the ID or primary key if it is auto-incremented
                            };

                            await dbContext.Tbl20171VatcreditNoteChildren.AddAsync(aTbl20171VatcreditNoteChild);
                        }
                        else
                        {
                            // Find and update existing child (Update)
                            var existingChild = await dbContext.Tbl20171VatcreditNoteChildren
                                .FirstOrDefaultAsync(x => x.CreditNoteChildSlNo == child.CreditNoteChildSlNo);

                            if (existingChild != null)
                            {
                                existingChild.CreditNoteNo = child.CreditNoteNo;
                                existingChild.UnitRate = child.UnitRate;
                                existingChild.DetailedDescription = child.DetailedDescription;
                                existingChild.QuantityCredited = child.QuantityCredited;
                                existingChild.TaxSlabCode = child.TaxSlabCode;
                                existingChild.UnitsToCredited = 1;
                                existingChild.UnitRateInOc = child.UnitRate;
                                existingChild.DiscountInOc = child.Discount;
                                existingChild.Discount = child.Discount;
                                existingChild.UnitRateMethod = child.UnitRateMethod;
                                existingChild.ItemCode = child.ItemCode ?? string.Empty;
                                existingChild.UoM = "Each";

                                dbContext.Tbl20171VatcreditNoteChildren.Update(existingChild);
                            }
                        }


                    }

                    await dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Invoice child records updated successfully!" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }

            return BadRequest("Failed to retrieve tenant and database context.");
        }

        [HttpPost]
        public async Task<ActionResult> UpdateDebitNoteMasterDetails(Tbl20172VatdebitNoteMaster InvoiceMaster)
        {
            if (InvoiceMaster == null)
            {
                return BadRequest(new { success = false, message = "Invalid invoice data received." });
            }

            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var existingInvoice = await dbContext.Tbl20172VatdebitNoteMasters
                                                                 .FirstOrDefaultAsync(v => v.DebitNoteNo == InvoiceMaster.DebitNoteNo);

                    var UserId = HttpContext.Session.GetString("UserId");
                    var TenantName = HttpContext.Session.GetString("TenantName");

                    if (existingInvoice != null)
                    {
                        // Update existing master record
                        dbContext.Entry(existingInvoice).CurrentValues.SetValues(InvoiceMaster);
                    }
                    else
                    {
                        // Insert new invoice master record
                        await dbContext.Tbl20172VatdebitNoteMasters.AddAsync(InvoiceMaster);
                    }


                    await dbContext.SaveChangesAsync();
                    // await transaction.CommitAsync();

                    var notifyRequest = new NotificationRequest
                        {
                             UserId = UserId, // or fetch from session/DB
                             VoucherName = InvoiceMaster.DebitNoteNo,
                             ActionType = "You have one Debit Note to verify",
                            TenantName = TenantName 
                    };

                    await _fcmService.SendNotificationAsync(notifyRequest);

                    return Ok(new { success = true, message = existingInvoice != null ? "Invoice and child records updated successfully!" : "New invoice and child records added successfully!" });
                }
            }
            catch (Exception ex)
            {
                // await transaction.RollbackAsync();
                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }


            return BadRequest("Failed to retrieve tenant and database context.");
        }


        [HttpPost]
        public async Task<ActionResult> UpdateDebitNoteChildDetails(List<Tbl20173VatdebitNoteChild> InvoiceChildren)
        {
            if (InvoiceChildren == null)
            {
                return BadRequest(new { success = false, message = "Invalid or empty invoice data received." });
            }

            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    foreach (var child in InvoiceChildren)
                    {
                        // if (child == null) continue;

                        if (child.DebitNoteChildSlNo == null || child.DebitNoteChildSlNo == 0)
                        {
                            // Create a new instance for each child
                            var aTbl20173VatdebitNoteChild = new Tbl20173VatdebitNoteChild
                            {
                                DebitNoteNo = child.DebitNoteNo,
                                UnitRate = child.UnitRate, // Ensure null safety
                                DetailedDescription = child.DetailedDescription, // Null safety
                                QuantityDebited = child.QuantityDebited, // Null safety
                                TaxSlabCode = child.TaxSlabCode ?? (byte)8,
                                UnitsToDebited = 1,
                                Discount = child.Discount,
                                UnitRateInOc = child.UnitRate,
                                DiscountInOc = child.Discount,
                                UnitRateMethod = child.UnitRateMethod,
                                ItemCode = child.ItemCode ?? string.Empty, // Null safety
                                UoM = "Each"
                                // Do NOT set the ID or primary key if it is auto-incremented
                            };

                            await dbContext.Tbl20173VatdebitNoteChildren.AddAsync(aTbl20173VatdebitNoteChild);
                        }
                        else
                        {
                            // Find and update existing child (Update)
                            var existingChild = await dbContext.Tbl20173VatdebitNoteChildren
                                .FirstOrDefaultAsync(x => x.DebitNoteChildSlNo == child.DebitNoteChildSlNo);

                            if (existingChild != null)
                            {
                                existingChild.DebitNoteNo = child.DebitNoteNo;
                                existingChild.UnitRate = child.UnitRate;
                                existingChild.DetailedDescription = child.DetailedDescription;
                                existingChild.QuantityDebited = child.QuantityDebited;
                                existingChild.TaxSlabCode = child.TaxSlabCode;
                                existingChild.UnitsToDebited = 1;
                                existingChild.UnitRateMethod = 49;
                                existingChild.Discount = child.Discount;
                                existingChild.UnitRateInOc = child.UnitRate;
                                existingChild.DiscountInOc = child.Discount;
                                existingChild.ItemCode = child.ItemCode ?? string.Empty;
                                existingChild.UoM = "Each";

                                dbContext.Tbl20173VatdebitNoteChildren.Update(existingChild);
                            }
                        }


                    }

                    await dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Invoice child records updated successfully!" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }

            return BadRequest("Failed to retrieve tenant and database context.");
        }

        [HttpPost]
        public async Task<ActionResult> UpdateProformaInvoiceMasterDetails(Tbl20181ProformaInvoiceMaster InvoiceMaster)
        {
            if (InvoiceMaster == null)
            {
                return BadRequest(new { success = false, message = "Invalid invoice data received." });
            }

            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var existingInvoice = await dbContext.Tbl20181ProformaInvoiceMasters
                                                                 .FirstOrDefaultAsync(v => v.ProformaInvoiceNo == InvoiceMaster.ProformaInvoiceNo);

                    if (existingInvoice != null)
                    {
                        // Update existing master record
                        dbContext.Entry(existingInvoice).CurrentValues.SetValues(InvoiceMaster);
                    }
                    else
                    {
                        // Insert new invoice master record
                        await dbContext.Tbl20181ProformaInvoiceMasters.AddAsync(InvoiceMaster);
                    }


                    await dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = existingInvoice != null ? "Invoice and child records updated successfully!" : "New invoice and child records added successfully!" });
                }
            }
            catch (Exception ex)
            {
                // await transaction.RollbackAsync();
                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }


            return BadRequest("Failed to retrieve tenant and database context.");
        }

        [HttpPost]
        public async Task<ActionResult> UpdateProformaChildDetails(List<Tbl20182ProformaInvoiceChild> InvoiceChildren)
        {
            if (InvoiceChildren == null || InvoiceChildren.Count == 0)
            {
                return BadRequest(new { success = false, message = "Invalid or empty invoice data received." });
            }

            try
            {

                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var savedChildren = new List<Tbl20182ProformaInvoiceChild>();

                    foreach (var child in InvoiceChildren)
                    {
                        if (child.ProformaInvChildSlNo == null || child.ProformaInvChildSlNo == 0)
                        {
                            var newChild = new Tbl20182ProformaInvoiceChild
                            {
                                ProformaInvoiceNo = child.ProformaInvoiceNo,
                                UnitRate = child.UnitRate,
                                DetailedDescription = child.DetailedDescription,
                                QuantityInvoiced = child.QuantityInvoiced,
                                TaxSlabCode = child.TaxSlabCode ?? (byte)8,
                                Discount = child.Discount,
                                UnitRateInOc = child.UnitRate,
                                DiscountInOc = child.Discount,
                                UnitsToBill = 1,
                                UnitRateMethod = child.UnitRateMethod,
                                ItemCode = child.ItemCode ?? string.Empty,
                                UoM = "Each"
                            };

                            await dbContext.Tbl20182ProformaInvoiceChildren.AddAsync(newChild);
                            await dbContext.SaveChangesAsync();

                            // Set the generated ID back to the input model if needed
                            //child.InvoiceChildSlNo = newChild.ProformaInvChildSlNo;

                            savedChildren.Add(newChild);
                        }
                        else
                        {
                            var existingChild = await dbContext.Tbl20182ProformaInvoiceChildren
                                .FirstOrDefaultAsync(x => x.ProformaInvChildSlNo == child.ProformaInvChildSlNo);

                            if (existingChild != null)
                            {

                                existingChild.ProformaInvoiceNo = child.ProformaInvoiceNo;
                                existingChild.UnitRate = child.UnitRate;
                                existingChild.DetailedDescription = child.DetailedDescription;
                                existingChild.QuantityInvoiced = child.QuantityInvoiced;
                                existingChild.TaxSlabCode = child.TaxSlabCode;
                                existingChild.UnitsToBill = 1;
                                existingChild.UnitRateMethod = child.UnitRateMethod;
                                existingChild.ItemCode = child.ItemCode ?? string.Empty;
                                existingChild.UoM = "Each";
                                existingChild.Discount = child.Discount;
                                existingChild.UnitRateInOc = child.UnitRate;
                                existingChild.DiscountInOc = child.Discount;
                                dbContext.Tbl20182ProformaInvoiceChildren.Update(existingChild);
                                savedChildren.Add(existingChild);
                            }
                        }
                    }

                    await dbContext.SaveChangesAsync();

                    return Ok(new
                    {
                        success = true,
                        message = "Invoice child records saved successfully!",
                        data = savedChildren
                    });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Failed to retrieve tenant and database context." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAutoGeneratedVATInvoiceNo(string tenantName)
        {
            if (string.IsNullOrWhiteSpace(tenantName))
                return BadRequest(new { message = "Tenant name is required.", success = false });

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { message = "Invalid tenant.", success = false });

            // 1. Build TenantShort
            string tenantShort;
            var words = tenantName.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 1)
                tenantShort = $"{tenantName[0]}{tenantName[^1]}".ToUpper();
            else
                tenantShort = string.Concat(words.Select(w => w[0])).ToUpper();

            // 2. Build prefix
            var year = DateTime.Now.Year;
            var prefix = $"{tenantShort}-VIFSO-{year}-";

            // 3. Find max increment for this prefix in Tbl20161VatinvoiceMasters
            var existingNos = await dbContext.Tbl20161VatinvoiceMasters
                .Where(inv => inv.InvoiceNo.StartsWith(prefix))
                .Select(inv => inv.InvoiceNo)
                .ToListAsync();

            int maxIncrement = 0;
            foreach (var no in existingNos)
            {
                var parts = no.Split('-');
                if (parts.Length == 4 && int.TryParse(parts[3], out int inc))
                    if (inc > maxIncrement) maxIncrement = inc;
            }

            var newIncrement = maxIncrement + 1;
            var invoiceNo = $"{prefix}{newIncrement:D3}";

            // 4. Ensure uniqueness
            bool exists = await dbContext.Tbl20161VatinvoiceMasters.AnyAsync(inv => inv.InvoiceNo == invoiceNo);
            if (exists)
                return Conflict(new { success = false, message = "Invoice No already exists." });

            return Ok(new { success = true, invoiceNo });
        }

        [HttpGet]
        public async Task<IActionResult> GetAutoGeneratedVATPurchaseInvoiceNo(string tenantName)
        {
            if (string.IsNullOrWhiteSpace(tenantName))
                return BadRequest(new { message = "Tenant name is required.", success = false });

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { message = "Invalid tenant.", success = false });

            string tenantShort;
            var words = tenantName.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 1)
                tenantShort = $"{tenantName[0]}{tenantName[^1]}".ToUpper();
            else
                tenantShort = string.Concat(words.Select(w => w[0])).ToUpper();

            var year = DateTime.Now.Year;
            var prefix = $"{tenantShort}-VIPUR-{year}-";

            var existingNos = await dbContext.Tbl20166VatpurchaseMasters
                .Where(inv => inv.PurchaseVoucherNo.StartsWith(prefix))
                .Select(inv => inv.PurchaseVoucherNo)
                .ToListAsync();

            int maxIncrement = 0;
            foreach (var no in existingNos)
            {
                var parts = no.Split('-');
                if (parts.Length == 4 && int.TryParse(parts[3], out int inc))
                    if (inc > maxIncrement) maxIncrement = inc;
            }

            var newIncrement = maxIncrement + 1;
            var purchaseVoucherNo = $"{prefix}{newIncrement:D3}";

            bool exists = await dbContext.Tbl20166VatpurchaseMasters.AnyAsync(inv => inv.PurchaseVoucherNo == purchaseVoucherNo);
            if (exists)
                return Conflict(new { success = false, message = "Voucher No already exists." });

            return Ok(new { success = true, invoiceNo = purchaseVoucherNo });
        }

        [HttpPost]
        public async Task<ActionResult> AddCPCrVoucherEntry(DataSourceLoadOptions loadOptions, [FromBody] List<Tbl201VoucherEntry> voucherEntries, string AccountHead, string PaymentAccoutHeadName, int Gridcount)
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

                    List<VoucherEntryDisplayDTO> matchingEntries = new();

                    // Add entries to the database
                    dbContext.Tbl201VoucherEntries.AddRange(voucherEntries);
                    await dbContext.SaveChangesAsync();

               


                    var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
                    var qryListOfAccountlists = dbContext.Qry201VoucherEntryScreenDisplays
                        .Where(p => voucherNos.Contains(p.VoucherNo))
                        .OrderBy(i => i.DrCr == "Dr" ? 1 : 0) // "Dr" entries first
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

                    // You may process or modify resultList here if needed

                    return Json(DataSourceLoader.Load(resultList.AsQueryable(), loadOptions));
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetChequeDetailsByVoucherNo(string voucherNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var cheque = await dbContext.Tbl20113ChequeMasters
                    .FirstOrDefaultAsync(c => c.VoucherNo == voucherNo);

                if (cheque != null)
                    return Ok(cheque);

                return Ok(null); // No data found
            }
            return Unauthorized(new { message = "Invalid tenant" });
        }

    }

}
