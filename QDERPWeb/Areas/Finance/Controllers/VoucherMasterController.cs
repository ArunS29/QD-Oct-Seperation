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
using Microsoft.IdentityModel.Tokens;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.DAL.Entities;


namespace QD.ERP.Web.Areas.Finance.Controllers
{
    //[Area("Finance")]
    [Route("api/[controller]/[action]")]
    // [Route("Finapi/[controller]/[action]")]
    [ApiController]
    public class VoucherMasterController : Controller
    {
        private ERPMasterWtDataContext _context;

        public VoucherMasterController(ERPMasterWtDataContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions)
        {
            try
            {
                var tbl201vouchermasters = _context.Tbl201VoucherMasters.Select(i => new
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

        [HttpGet]
        public async Task<IActionResult> GetCPPaymentAccounts(DataSourceLoadOptions loadOptions)
        {
            try
            {
                var qryListOfAccountlists = _context.Qry201ListOfAccounts
                    .Where(p => p.AccountGroupId == "A012")
                    .Select(i => new
                    {
                        i.AccountHead,
                        i.AccountId,
                        i.AccountHeadArabic,
                        i.IsLedgerObselete
                    });

                var result = await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions);
                return Json(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }


        [HttpGet]
        public async Task<ActionResult> GetBPPaymentAccounts(DataSourceLoadOptions loadOptions)
        {
            try
            {
                var qryListOfAccountlists = _context.Qry201ListOfAccounts.Where(p => p.AccountGroupId == "A013").Select(i => new
                {
                    i.AccountHead,
                    i.AccountId,
                    i.AccountHeadArabic,
                    i.IsLedgerObselete


                });

                return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [HttpGet]
        public async Task<ActionResult> GetVoucherEntryPaymentGrid(DataSourceLoadOptions loadOptions)
        {
            try
            {
                var qryListOfAccountlists = _context.Qry201VoucherEntryScreenDisplays
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

        [HttpGet]
        public async Task<IActionResult> GetAccountHead(DataSourceLoadOptions loadOptions)
        {
            try
            {
                var qryListOfAccountlists = _context.Qry201ListOfAccounts.Select(i => new
                {

                    i.AccountId,
                    i.AccountHead,
                    i.AccountGroup,
                    i.AccountHeadArabic,
                    i.ReferenceNo,
                    i.AccountGroupId,
                    i.IsLedgerObselete
                });


                return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));

            }
            catch (Exception ex) { throw ex; }

        }



        [HttpPost]
        public async Task<ActionResult> AddVoucherEntry(DataSourceLoadOptions loadOptions, [FromBody] List<Tbl201VoucherEntry> voucherEntries, string AccountHead, string PaymentAccoutHeadName, int Gridcount)
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
                //var matchingEntries;
                List<VoucherEntryDisplayDTO> matchingEntries = new();
                var voucherNos1 = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
                var qryListOfAccountlists1 = _context.Qry201VoucherEntryScreenDisplays
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

                bool isVoucherExists = _context.Tbl201VoucherMasters
               .Any(v => v.VoucherNo == voucherEntries[0].VoucherNo);

                if (!isVoucherExists)
                {
                    voucherMaster.VoucherNo = voucherEntries[0].VoucherNo;
                    voucherMaster.VoucherDate = DateTime.Now;
                    _context.Tbl201VoucherMasters.AddRange(voucherMaster);
                }

                // Add entries to the database
                _context.Tbl201VoucherEntries.AddRange(voucherEntries);
                await _context.SaveChangesAsync();

                //SaveVoucher(voucherEntries);


                var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
                var qryListOfAccountlists = _context.Qry201VoucherEntryScreenDisplays
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

                        var accountHead = _context.Qry201ListOfAccounts
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
                                var existingEntry = _context.Tbl201VoucherEntries
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
                                    _context.Tbl201VoucherEntries.Update(existingEntry);
                                    _context.SaveChanges();

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
                                        var existingEntry = _context.Tbl201VoucherEntries
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
                                            _context.Tbl201VoucherEntries.Update(existingEntry);
                                            _context.SaveChanges();

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
                        if(Remarks=="")
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
        [HttpGet]
        public async Task<ActionResult> LoadVoucherEntries(DataSourceLoadOptions loadOptions, string voucherNo)
        {
            try
            {
                if (string.IsNullOrEmpty(voucherNo))
                {
                    return BadRequest(new { success = false, message = "Invalid Voucher Number." });
                }

                var qryListOfAccountlists = _context.Qry201VoucherEntryScreenDisplays
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
                        i.SysRemarks
                    })
                    .ToList();

                // Fetch AccountHead names for mapping
                var accountIds = qryListOfAccountlists.Select(i => i.AccountHead).Distinct().ToList();
                var accountHeadMap = _context.Qry201ListOfAccounts
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


        [HttpPost]
        public async Task<ActionResult> AddBPVoucherEntry(DataSourceLoadOptions loadOptions, [FromBody] List<Tbl201VoucherEntry> voucherEntries, string AccountHead, string PaymentAccoutHeadName, int Gridcount)
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
                var qryListOfAccountlists1 = _context.Qry201VoucherEntryScreenDisplays
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

                bool isVoucherExists = _context.Tbl201VoucherMasters
               .Any(v => v.VoucherNo == voucherEntries[0].VoucherNo);

                if (!isVoucherExists)
                {
                    voucherMaster.VoucherNo = voucherEntries[0].VoucherNo;
                    voucherMaster.VoucherDate = DateTime.Now;
                    _context.Tbl201VoucherMasters.AddRange(voucherMaster);
                }

                // Add entries to the database
                _context.Tbl201VoucherEntries.AddRange(voucherEntries);
                await _context.SaveChangesAsync();

                //SaveVoucher(voucherEntries);


                var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
                var qryListOfAccountlists = _context.Qry201VoucherEntryScreenDisplays
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

                        var accountHead = _context.Qry201ListOfAccounts
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
                                var existingEntry = _context.Tbl201VoucherEntries
                                                            .FirstOrDefault(v => v.AccountHead == entry.AccountHead
                                                                              && v.DrCr == "Cr"
                                                                              && v.VoucherNo == entry.VoucherNo);

                                if (existingEntry != null)
                                {
                                    // Update CrAmount by adding the calculated debit amount
                                    entry.CrAmount = debitamt;

                                    // Optionally update the existing entry in the database
                                    existingEntry.VoucherAmount = entry.CrAmount;
                                    _context.Tbl201VoucherEntries.Update(existingEntry);
                                    _context.SaveChanges();

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
                                        var existingEntry = _context.Tbl201VoucherEntries
                                                                    .FirstOrDefault(v => v.AccountHead == entry.AccountHead
                                                                                      && v.DrCr == "Cr"
                                                                                      && v.VoucherNo == entry.VoucherNo);

                                        if (existingEntry != null)
                                        {
                                            // Update CrAmount by adding the calculated debit amount
                                            entry.CrAmount = Amt;

                                            // Optionally update the existing entry in the database
                                            existingEntry.VoucherAmount = entry.CrAmount;
                                            _context.Tbl201VoucherEntries.Update(existingEntry);
                                            _context.SaveChanges();

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



        [HttpPost]
        public async Task<ActionResult> AddBPCrVoucherEntry(DataSourceLoadOptions loadOptions, [FromBody] List<Tbl201VoucherEntry> voucherEntries, string AccountHead, string PaymentAccoutHeadName, int Gridcount)
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
                var qryListOfAccountlists1 = _context.Qry201VoucherEntryScreenDisplays
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

                bool isVoucherExists = _context.Tbl201VoucherMasters
               .Any(v => v.VoucherNo == voucherEntries[0].VoucherNo);

                if (!isVoucherExists)
                {
                    voucherMaster.VoucherNo = voucherEntries[0].VoucherNo;
                    voucherMaster.VoucherDate = DateTime.Now;
                    _context.Tbl201VoucherMasters.AddRange(voucherMaster);
                }

                // Add entries to the database
                _context.Tbl201VoucherEntries.AddRange(voucherEntries);
                await _context.SaveChangesAsync();

                //SaveVoucher(voucherEntries);


                var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
                var qryListOfAccountlists = _context.Qry201VoucherEntryScreenDisplays
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


                            var existingEntry = _context.Tbl201VoucherEntries
                                      .FirstOrDefault(v => v.AccountHead == entry.AccountHead
                                                        && v.DrCr == "Cr"
                                                        && v.VoucherNo == entry.VoucherNo);

                            // Optionally update the existing entry in the database
                            existingEntry.VoucherAmount = CrMinusAmt;
                            entry.CrAmount = -CrMinusAmt;
                            _context.Tbl201VoucherEntries.Update(existingEntry);
                            _context.SaveChanges();

                        }
                    }


                    if (!string.IsNullOrEmpty(entry.AccountHead))
                    {

                        var accountHead = _context.Qry201ListOfAccounts
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



        [HttpPost]
        public async Task<ActionResult> AddCPCrVoucherEntry(DataSourceLoadOptions loadOptions, [FromBody] List<Tbl201VoucherEntry> voucherEntries, string AccountHead, string PaymentAccoutHeadName, int Gridcount)
        {
            if (voucherEntries == null || !voucherEntries.Any())
            {
                return BadRequest(new { success = false, message = "Invalid data received." });
            }

            try
            {
                Tbl201VoucherMaster voucherMaster = new();
                int aEntryAmount = 0;
                int Amt = 0, Crmt=0, CrMinusAmt=0, NewCrMinusAmt=0, ExistingCrMinusAmt=0;
                bool IsMatchingEntry = false;

                //var matchingEntries;
                List<VoucherEntryDisplayDTO> matchingEntries = new();
                var voucherNos1 = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
                var qryListOfAccountlists1 = _context.Qry201VoucherEntryScreenDisplays
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
                    if(matchingEntries.Count!=0)
                    {
                        ExistingCrMinusAmt = (int)matchingEntries[0].CrAmount;
                        IsMatchingEntry = true;
                    }
          
                }

                bool isVoucherExists = _context.Tbl201VoucherMasters
               .Any(v => v.VoucherNo == voucherEntries[0].VoucherNo);

                if (!isVoucherExists)
                {
                    voucherMaster.VoucherNo = voucherEntries[0].VoucherNo;
                    voucherMaster.VoucherDate = DateTime.Now;
                    _context.Tbl201VoucherMasters.AddRange(voucherMaster);
                }

                // Add entries to the database
                _context.Tbl201VoucherEntries.AddRange(voucherEntries);
                await _context.SaveChangesAsync();

                //SaveVoucher(voucherEntries);


                var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
                var qryListOfAccountlists = _context.Qry201VoucherEntryScreenDisplays
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
                        if(IsMatchingEntry == true)
                        {
                            //CrMinusAmt = (int)entry.VoucherAmountFormatted + NewCrMinusAmt;
                            CrMinusAmt =  NewCrMinusAmt + ExistingCrMinusAmt;
                           

                            var existingEntry = _context.Tbl201VoucherEntries
                                      .FirstOrDefault(v => v.AccountHead == entry.AccountHead
                                                        && v.DrCr == "Cr"
                                                        && v.VoucherNo == entry.VoucherNo);

                            // Optionally update the existing entry in the database
                            existingEntry.VoucherAmount = CrMinusAmt;
                            entry.CrAmount = -CrMinusAmt;
                            _context.Tbl201VoucherEntries.Update(existingEntry);
                            _context.SaveChanges();
                           
                        }
                    }
                

                    if (!string.IsNullOrEmpty(entry.AccountHead))
                    {

                        var accountHead = _context.Qry201ListOfAccounts
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





        //[HttpPost]
        //public async Task<ActionResult> SaveVoucher([FromBody] Tbl201VoucherMaster VM)
        //{
        //    if (VM == null)
        //    {
        //        return BadRequest(new { success = false, message = "Invalid data received." });
        //    }

        //    try
        //    {
        //        bool isVoucherExists = _context.Tbl201VoucherMasters
        //     .Any(v => v.VoucherNo == VM.VoucherNo);

        //        var existingVoucher = await _context.Tbl201VoucherMasters
        //                                .FirstOrDefaultAsync(v => v.VoucherNo == VM.VoucherNo);

        //        if (!isVoucherExists)
        //        {
        //            _context.Tbl201VoucherMasters.Add(VM);
        //            await _context.SaveChangesAsync();
        //            //return Json(new { VoucherEntryNo = VE.VoucherNo });
        //            return Ok(new { success = true, message = "Data inserted successfully!" });
        //        }
        //        // Update existing record
        //        _context.Entry(existingVoucher).CurrentValues.SetValues(VM);
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
            DateTime currentDate = DateTime.Now;
            string currentYear = currentDate.Year.ToString();
            string currentMonth = currentDate.Month.ToString("00");
            string voucherString = "CP-" + currentYear.Substring(currentYear.Length - 2, 2) + "-" + currentMonth + "-";
            string strNewReceiptNo;

            // SQL query with interpolated string
            string likePattern = voucherString + "%";

            try
            {
                // Use raw SQL query to fetch the maximum voucher number
                var result = await _context.VoucherResults
                    .FromSqlInterpolated($@"
                SELECT MAX(CAST(RIGHT(VoucherNo, 3) AS INT)) AS MaxVoucherNo
                FROM Tbl201VoucherEntry
                WHERE VoucherNo LIKE {likePattern}")
                    .ToListAsync();

                int maxVoucherNo = result.FirstOrDefault()?.MaxVoucherNo ?? 0;

                int newVoucherNo = maxVoucherNo + 1;

                // Format the new voucher number with leading zeros
                strNewReceiptNo = "000" + newVoucherNo.ToString();
                strNewReceiptNo = strNewReceiptNo.Substring(strNewReceiptNo.Length - 3);

                // Concatenate with the voucher string
                strNewReceiptNo = voucherString + strNewReceiptNo;
            }
            catch (Exception)
            {
                // Handle cases where there's no existing voucher number
                strNewReceiptNo = voucherString + "001";
            }

            return Json(strNewReceiptNo);
        }




        [HttpGet]
        public async Task<ActionResult> GetNewBPVoucherNo(DataSourceLoadOptions loadOptions)
        {
            DateTime currentDate = DateTime.Now;
            string currentYear = currentDate.Year.ToString();
            string currentMonth = currentDate.Month.ToString("00");
            string voucherString = "BP-" + currentYear.Substring(currentYear.Length - 2, 2) + "-" + currentMonth + "-";
            string strNewReceiptNo;

            // SQL query with interpolated string
            string likePattern = voucherString + "%";

            try
            {
                // Use raw SQL query to fetch the maximum voucher number
                var result = await _context.VoucherResults
                    .FromSqlInterpolated($@"
                SELECT MAX(CAST(RIGHT(VoucherNo, 3) AS INT)) AS MaxVoucherNo
                FROM Tbl201VoucherEntry
                WHERE VoucherNo LIKE {likePattern}")
                    .ToListAsync();

                int maxVoucherNo = result.FirstOrDefault()?.MaxVoucherNo ?? 0;

                int newVoucherNo = maxVoucherNo + 1;

                // Format the new voucher number with leading zeros
                strNewReceiptNo = "000" + newVoucherNo.ToString();
                strNewReceiptNo = strNewReceiptNo.Substring(strNewReceiptNo.Length - 3);

                // Concatenate with the voucher string
                strNewReceiptNo = voucherString + strNewReceiptNo;
            }
            catch (Exception)
            {
                // Handle cases where there's no existing voucher number
                strNewReceiptNo = voucherString + "001";
            }

            return Json(strNewReceiptNo);
        }

        [HttpPost]
        public async Task<ActionResult> SaveChequeDetails([FromBody] Tbl20113ChequeMaster CM)
        {
            if (CM == null)
            {
                return BadRequest(new { success = false, message = "Invalid data received." });
            }

            try
            {
                var existingVoucher = await _context.Tbl20113ChequeMasters
                                                    .FirstOrDefaultAsync(v => v.VoucherNo == CM.VoucherNo);

                if (existingVoucher != null)
                {
                    // Update existing record
                    _context.Entry(existingVoucher).CurrentValues.SetValues(CM);
                    await _context.SaveChangesAsync();
                    return Ok(new { success = true, message = "Cheque Information Updated sucessfully!" });
                }
                else
                {
                    // Insert new record
                    _context.Tbl20113ChequeMasters.Add(CM);
                    await _context.SaveChangesAsync();
                    return Ok(new { success = true, message = "Cheque Information Saved sucessfully!" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckChequeExists(string chequeNo, string voucherNo)
        {
            bool exists = await _context.Tbl20113ChequeMasters
                .AnyAsync(c => c.ChequeNo == chequeNo && c.VoucherNo != voucherNo); // Exclude current voucher

            return Json(exists);
        }



        [HttpGet]
        public async Task<ActionResult> GetPreview(DataSourceLoadOptions loadOptions, [FromBody] List<Tbl201VoucherEntry> voucherEntries)
        {
            if (voucherEntries == null || !voucherEntries.Any())
            {
                return BadRequest(new { success = false, message = "Invalid data received." });
            }

            try
            {
                // Retrieve updated data for the submitted vouchers
                var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
                var qryListOfAccountlists = _context.Qry201VoucherEntryScreenDisplays
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



        [HttpPost]
        public async Task<ActionResult> DeleteVoucherEntry(DataSourceLoadOptions loadOptions, long voucherEntryNo, string VoucherNo, string PaymentAccoutHeadName, int Gridcount)
        {
           

            try
            {
                Tbl201VoucherMaster voucherMaster = new();
                int aEntryAmount = 0;
                int Amt = 0;
                var Remarks = "";
                bool IsMatchingEntry = false;
                //var matchingEntries;
                List<VoucherEntryDisplayDTO> matchingEntries = new();
                //  var voucherNos1 = voucherEntries.Select(ve => ve.VoucherNo).Distinct();


                // Get the list of updated vouchers
                var voucherEntries1 = _context.Tbl201VoucherEntries
                                              .Where(ve => ve.VoucherNo == VoucherNo) // Filter by the provided VoucherNo
                                              .ToList();

                var voucherNos1 = voucherEntries1.Select(ve => ve.VoucherNo).Distinct();

              
                var qryListOfAccountlists1 = _context.Qry201VoucherEntryScreenDisplays
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

                bool isVoucherExists = _context.Tbl201VoucherMasters
               .Any(v => v.VoucherNo == voucherEntries1[0].VoucherNo);
                var record = await _context.Tbl201VoucherEntries.FirstOrDefaultAsync(v => v.VoucherEntryNo == voucherEntryNo);
                var SubLedgerRecord = await _context.Tbl201SubLedgerMasters.FirstOrDefaultAsync(v => v.VoucherEntryNo == voucherEntryNo);
                var PropertyAllocationRecord = await _context.Tbl20122PropertyAllocationMasters.FirstOrDefaultAsync(v => v.VoucherEntryId == voucherEntryNo);
                var CostAllocationRecord = await _context.Tbl201CostAllocationMasters.FirstOrDefaultAsync(v => v.VoucherEntryId == voucherEntryNo);
                var SalaryPayableRecord = await _context.Tbl20114SalaryPayableMasters.FirstOrDefaultAsync(v => v.VoucherEntryNo == voucherEntryNo);
                var EmpAllocationRecord = await _context.Tbl20104EmployeeAllocationMasters.FirstOrDefaultAsync(v => v.VoucherEntryId == voucherEntryNo);



                var voucherNo = voucherEntries1[0].VoucherNo;
                var masterrecord = _context.Tbl201VoucherMasters.FirstOrDefault(v => v.VoucherNo == voucherNo);
                if (!isVoucherExists)
                {
                    voucherMaster.VoucherNo = voucherEntries1[0].VoucherNo;
                    voucherMaster.VoucherDate = DateTime.Now;
                   
                }

                // Add entries to the database
                //_context.Tbl201VoucherEntries.AddRange(voucherEntries);
                //await _context.SaveChangesAsync();

             
                if (record == null)
                {
                    return NotFound(new { message = "Record not found!" });
                }

                if (record.VoucherAmount!=0)
                {
                    if(record!=null)
                    {
                        _context.Tbl201VoucherEntries.Remove(record);
                    }
                    else if(SubLedgerRecord!=null)
                    {
                        _context.Tbl201SubLedgerMasters.Remove(SubLedgerRecord);

                    }
                    else if (PropertyAllocationRecord != null)
                    {
                        _context.Tbl20122PropertyAllocationMasters.Remove(PropertyAllocationRecord);
                    }
                    else if (SubLedgerRecord != null)
                    {
                        _context.Tbl201CostAllocationMasters.Remove(CostAllocationRecord);
                    }
                    else if (SubLedgerRecord != null)
                    {
                        _context.Tbl20104EmployeeAllocationMasters.Remove(EmpAllocationRecord);
                    }
             
                    //  _context.Tbl201VoucherMasters.Remove(masterrecord);
                    await _context.SaveChangesAsync();


                }
                
                
                //SaveVoucher(voucherEntries);

                // Get the list of updated vouchers
                var voucherEntries = _context.Tbl201VoucherEntries
                                              .Where(ve => ve.VoucherNo == VoucherNo) // Filter by the provided VoucherNo
                                              .ToList();


                var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
                var qryListOfAccountlists = _context.Qry201VoucherEntryScreenDisplays
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

                        var accountHead = _context.Qry201ListOfAccounts
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
                                var existingEntry = _context.Tbl201VoucherEntries
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
                                    _context.Tbl201VoucherEntries.Update(existingEntry);
                                    _context.SaveChanges();

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
                                            if(aEntryAmount==0)
                                            {
                                                Amt = Amt - eAmount;
                                            }
                                            else
                                            {
                                                Amt = aEntryAmount - eAmount;
                                            }
                                        
                                        }

                                        // Check if an existing entry matches
                                        var existingEntry = _context.Tbl201VoucherEntries
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
                                            _context.Tbl201VoucherEntries.Update(existingEntry);
                                            _context.SaveChanges();

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



        [HttpPost]
        public async Task<ActionResult> DeleteBPVoucherEntry(DataSourceLoadOptions loadOptions, long voucherEntryNo, string VoucherNo, string PaymentAccoutHeadName, int Gridcount)
        {


            try
            {
                Tbl201VoucherMaster voucherMaster = new();
                int aEntryAmount = 0;
                int Amt = 0;
                var Remarks = "";
                bool IsMatchingEntry = false;
                //var matchingEntries;
                List<VoucherEntryDisplayDTO> matchingEntries = new();
                //  var voucherNos1 = voucherEntries.Select(ve => ve.VoucherNo).Distinct();


                // Get the list of updated vouchers
                var voucherEntries1 = _context.Tbl201VoucherEntries
                                              .Where(ve => ve.VoucherNo == VoucherNo) // Filter by the provided VoucherNo
                                              .ToList();

                var voucherNos1 = voucherEntries1.Select(ve => ve.VoucherNo).Distinct();


                var qryListOfAccountlists1 = _context.Qry201VoucherEntryScreenDisplays
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

                bool isVoucherExists = _context.Tbl201VoucherMasters
               .Any(v => v.VoucherNo == voucherEntries1[0].VoucherNo);
                var record = await _context.Tbl201VoucherEntries.FirstOrDefaultAsync(v => v.VoucherEntryNo == voucherEntryNo);
                var voucherNo = voucherEntries1[0].VoucherNo;
                var masterrecord = _context.Tbl201VoucherMasters.FirstOrDefault(v => v.VoucherNo == voucherNo);
                var SubLedgerRecord = await _context.Tbl201SubLedgerMasters.FirstOrDefaultAsync(v => v.VoucherEntryNo == voucherEntryNo);
                var PropertyAllocationRecord = await _context.Tbl20122PropertyAllocationMasters.FirstOrDefaultAsync(v => v.VoucherEntryId == voucherEntryNo);
                var CostAllocationRecord = await _context.Tbl201CostAllocationMasters.FirstOrDefaultAsync(v => v.VoucherEntryId == voucherEntryNo);
                var SalaryPayableRecord = await _context.Tbl20114SalaryPayableMasters.FirstOrDefaultAsync(v => v.VoucherEntryNo == voucherEntryNo);
                var EmpAllocationRecord = await _context.Tbl20104EmployeeAllocationMasters.FirstOrDefaultAsync(v => v.VoucherEntryId == voucherEntryNo);



                if (!isVoucherExists)
                {
                    voucherMaster.VoucherNo = voucherEntries1[0].VoucherNo;
                    voucherMaster.VoucherDate = DateTime.Now;
                    _context.Tbl201VoucherMasters.Remove(masterrecord);


                }

                // Add entries to the database
                //_context.Tbl201VoucherEntries.AddRange(voucherEntries);
                //await _context.SaveChangesAsync();


                if (record == null)
                {
                    return NotFound(new { message = "Record not found!" });
                }

                if (record.VoucherAmount != 0)
                {
                    if (record != null)
                    {
                        _context.Tbl201VoucherEntries.Remove(record);
                    }
                    else if (SubLedgerRecord != null)
                    {
                        _context.Tbl201SubLedgerMasters.Remove(SubLedgerRecord);

                    }
                    else if (PropertyAllocationRecord != null)
                    {
                        _context.Tbl20122PropertyAllocationMasters.Remove(PropertyAllocationRecord);
                    }
                    else if (SubLedgerRecord != null)
                    {
                        _context.Tbl201CostAllocationMasters.Remove(CostAllocationRecord);
                    }
                    else if (SubLedgerRecord != null)
                    {
                        _context.Tbl20104EmployeeAllocationMasters.Remove(EmpAllocationRecord);
                    }

                    //  _context.Tbl201VoucherMasters.Remove(masterrecord);
                    await _context.SaveChangesAsync();


                }


                //SaveVoucher(voucherEntries);

                // Get the list of updated vouchers
                var voucherEntries = _context.Tbl201VoucherEntries
                                              .Where(ve => ve.VoucherNo == VoucherNo) // Filter by the provided VoucherNo
                                              .ToList();


                var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
                var qryListOfAccountlists = _context.Qry201VoucherEntryScreenDisplays
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

                        var accountHead = _context.Qry201ListOfAccounts
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
                                var existingEntry = _context.Tbl201VoucherEntries
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
                                    _context.Tbl201VoucherEntries.Update(existingEntry);
                                    _context.SaveChanges();

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
                                        var existingEntry = _context.Tbl201VoucherEntries
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
                                            _context.Tbl201VoucherEntries.Update(existingEntry);
                                            _context.SaveChanges();

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

      

        [HttpPost]
        public async Task<ActionResult> DeleteAllVoucherEntry(DataSourceLoadOptions loadOptions, string VoucherNo)
        {
            try
            {
                // Find all records matching the given VoucherNo
                var records = await _context.Tbl201VoucherEntries
                                            .Where(v => v.VoucherNo == VoucherNo)
                                            .ToListAsync();

                if (records == null || !records.Any())
                {
                    return NotFound(new { message = "No records found for the provided VoucherNo!" });
                }

                // Remove all matching records
                _context.Tbl201VoucherEntries.RemoveRange(records);
                await _context.SaveChangesAsync();

                // Fetch updated voucher list
                var voucherEntries = await _context.Tbl201VoucherEntries
                                                   .Where(ve => ve.VoucherNo == VoucherNo)
                                                   .ToListAsync();

                var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct().ToList();

                // Query the updated display list
                var qryListOfAccountLists = _context.Qry201VoucherEntryScreenDisplays
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


        [HttpPost]
        public async Task<ActionResult> VerifyVoucher(Tbl201VoucherMaster voucherMaster)
        {
            try
            {
                var UserName = HttpContext.Session.GetString("UserName");

                if (string.IsNullOrEmpty(voucherMaster.VoucherNo))
                {
                    return BadRequest(new { Message = "Voucher number is required." });
                }

                var voucher = _context.Tbl201VoucherMasters.FirstOrDefault(v => v.VoucherNo == voucherMaster.VoucherNo);

                if (voucher == null)
                {
                    return NotFound(new { Message = "Voucher not found." });
                }

                // Update the fields
                voucher.IsVerified = true;
                voucher.VoucherVerifiedOn = DateTime.Now;
                voucher.VoucherVerifiedBy = UserName;

                _context.SaveChanges();

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



        [HttpPost]
        public async Task<ActionResult> ApproveVoucher(string VoucherNo,bool IsDirect)
        {
            try
            {
                var UserName = HttpContext.Session.GetString("UserName");

                if (string.IsNullOrEmpty(VoucherNo))
                {
                    return BadRequest(new { Message = "Voucher number is required." });
                }

                var voucher = _context.Tbl201VoucherMasters.FirstOrDefault(v => v.VoucherNo == VoucherNo);

                if (voucher == null)
                {
                    return NotFound(new { Message = "Voucher not found." });
                }

                // Update the fields
                voucher.IsApproved = true;
                voucher.VoucherApprovedOn = DateTime.Now;
                voucher.VoucherApprovedBy = UserName;

                if (IsDirect==false)
                {
                    voucher.IsVerified = true;
                    voucher.VoucherVerifiedOn = DateTime.Now;
                    voucher.VoucherVerifiedBy = UserName;

                }

                _context.SaveChanges();

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



        [HttpPost]
        public async Task<IActionResult> Delete(DataSourceLoadOptions loadOptions, long voucherEntryNo, string VoucherNo)
        {
            if (voucherEntryNo == 0 || string.IsNullOrEmpty(VoucherNo))
            {

                return BadRequest(new { success = false, message = "VoucherEntryNo and VoucherNo are required." });
            }

            try
            {
                var allItems = await _context.Tbl201VoucherEntries.ToListAsync();
                var item = allItems.FirstOrDefault(p => p.VoucherEntryNo == voucherEntryNo);
                if (item == null)
                {

                    return NotFound(new { success = false, message = "Record not found." });
                }


                _context.Tbl201VoucherEntries.Remove(item);
                await _context.SaveChangesAsync();


                var qryListOfAccountlists = _context.Qry201VoucherEntryScreenDisplays
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


        [HttpGet]
        public IActionResult GetEmployeeName()
        {
            var data = _context.Tbl101Employees
                .Select(c => new
                {
                    c.EmployeeId,
                    c.EmployeeName,
                    c.NationalId

                }).ToList();

            return Ok(data);
        }
        [HttpPost]
        public async Task<ActionResult> SaveCostAllocation([FromBody] Tbl20104EmployeeAllocationMaster EM)
        {

            try
            {
                _context.Tbl20104EmployeeAllocationMasters.Add(EM);
                await _context.SaveChangesAsync();
                //return Json(new { VoucherEntryNo = VE.VoucherNo });
                return Ok(new { success = true, message = "Data inserted successfully!" });
            }
            catch (Exception ex)
            {

                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }


        }


        [HttpGet]
        public async Task<IActionResult> GetEditAccountHead(DataSourceLoadOptions loadOptions)
        {
            try
            {
                var qryListOfAccountlists = _context.Qry201ListOfAccounts
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



        public IActionResult CostAllocation(string voucherNo, string accountHead, string voucherAmount, string drCr, long voucherEntryNo)

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

        [HttpPost]
        public async Task<ActionResult> UpdateVoucher([FromBody] Tbl201VoucherMaster VM)
        {
            if (VM == null || string.IsNullOrWhiteSpace(VM.VoucherNo))
            {
                return BadRequest(new { success = false, message = "Invalid data received." });
            }

            try
            {
                var existingVoucher = await _context.Tbl201VoucherMasters
                                                    .FirstOrDefaultAsync(v => v.VoucherNo == VM.VoucherNo);

                if (existingVoucher != null)
                {
                    // Update existing record
                    _context.Entry(existingVoucher).CurrentValues.SetValues(VM);
                }
                else
                {
                    // Insert new record
                    _context.Tbl201VoucherMasters.Add(VM);
                }

                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = existingVoucher != null ? "Voucher updated successfully!" : "Voucher inserted successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }
        }
        [HttpPost]
        public IActionResult UpdateVoucherEntry([FromBody] Tbl201VoucherEntry model)
        {
            if (model == null || string.IsNullOrEmpty(model.AccountHead))
            {
                return BadRequest("Invalid data: AccountHead is missing or null.");
            }

            var existingEntry = _context.Tbl201VoucherEntries
                .FirstOrDefault(v => v.VoucherEntryNo == model.VoucherEntryNo);

            var accountID = _context.Tbl201ChartOfAccounts
                .Where(a => a.AccountHead == model.AccountHead)
                .Select(a => a.AccountId)
                .FirstOrDefault();

            if (existingEntry != null)
            {
                existingEntry.DrCr = model.DrCr;
                existingEntry.AccountHead = accountID; // Assign single account ID
                existingEntry.EntryNarration = model.EntryNarration;
                existingEntry.SysRemarks = model.SysRemarks; // Ensure SysRemarks is updated



                _context.SaveChanges();
                return Ok(new { message = "" });
            }

            return NotFound("Voucher Entry not found.");
        }

        public IActionResult LoadChequePopup()
        {
            return PartialView("~/Areas/Finance/Views/_ChequePopup.cshtml");
        }

        [HttpGet]
        public async Task<IActionResult> CheckIsMaintainSalary(string AccountHead, string AccountID)
        {
            try
            {
                var allocation = await _context.Tbl201ChartOfAccounts
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
            try
            {
                // Fetch updated data and join with Qry201ListOfAccounts to get AccountHead
                var voucherEntry = (from v in _context.Qry201VoucherEntryScreenDisplays
                                    join a in _context.Qry201ListOfAccounts
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

        public async Task<IActionResult> GetDefaultReceivingAccount()
        {
            var defaultAccount = await _context.Tbl201ChartOfAccounts
                .Where(a => a.IsDefaultForCash == true && a.AccountGroupId == "A012")
                .OrderByDescending(a => a.RecordModifiedOn) // Get the latest default account
                .Select(a => new { a.AccountId, a.AccountHead })
                .FirstOrDefaultAsync();

            return Json(defaultAccount);
        }

        public async Task<IActionResult> GetDefaultPaymentAccount()
        {
            var defaultAccount = await _context.Tbl201ChartOfAccounts
                .Where(a => a.IsDefaultForCash == true && a.AccountGroupId == "A013")
                .OrderByDescending(a => a.RecordModifiedOn) // Get the latest default account
                .Select(a => new { a.AccountId, a.AccountHead })
                .FirstOrDefaultAsync();

            return Json(defaultAccount);
        }
    }

}
