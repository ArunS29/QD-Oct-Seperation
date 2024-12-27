using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.DAL.Entities;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class VoucherEntryCashReceiptsController : Controller
    {
        private ERPMasterWtDataContext _context;

        public VoucherEntryCashReceiptsController(ERPMasterWtDataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetReceivingAccount(DataSourceLoadOptions loadOptions)
        {
            var tbl20101salespersonmasters = _context.Qry201ListOfAccounts.Where(p => p.AccountGroupId == "A012").Select(i => new
            {

                i.AccountId,
                i.AccountHead,
                i.AccountHeadArabic,
                i.IsLedgerObselete
            });

            return Json(await DataSourceLoader.LoadAsync(tbl20101salespersonmasters, loadOptions));
        }





        [HttpGet]
        public async Task<IActionResult> GetAccountHead(DataSourceLoadOptions loadOptions, string SelectedPaymentAccount)
        {
            var qryListOfAccountlists = _context.Qry201ListOfAccounts
                .Where(i => i.AccountId != SelectedPaymentAccount) // Exclude the Receiving Account value
                .Select(i => new
                {
                    i.AccountId,
                    i.AccountHead,
                    i.AccountGroup,
                    i.AccountHeadArabic,
                    i.ReferenceNo,
                    i.IsLedgerObselete
                });

            return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
        }




        public async Task<ActionResult> GetVoucherEntries(DataSourceLoadOptions loadOptions, string voucherNo)
        {
            var qryListOfAccountlists = _context.Qry201VoucherEntryScreenDisplays.Where(p => p.VoucherNo == voucherNo).Select(i => new
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

        [HttpPost]
        public async Task<ActionResult> AddVoucherEntry(DataSourceLoadOptions loadOptions, [FromBody] List<Tbl201VoucherEntry> voucherEntries)
        {
            if (voucherEntries == null)
            {
                return BadRequest(new { success = false, message = "Invalid data received." });
            }

            try
            {
                // Add entries to the database

                _context.Tbl201VoucherEntries.AddRange(voucherEntries);

                await _context.SaveChangesAsync();

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

                         i.VoucherEntryNo

                     });

                // Return the updated data as a JSON response

                return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));

            }
            catch (Exception ex)
            {

                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }


        }
        [HttpGet]

        public async Task<ActionResult> GetNewBRVoucherNo(DataSourceLoadOptions loadOptions)

        {

            DateTime currentDate = DateTime.Now;

            string currentYear = currentDate.Year.ToString();

            string currentMonth = currentDate.Month.ToString("00");

            string voucherString = "CR-" + currentYear.Substring(currentYear.Length - 2, 2) + "-" + currentMonth + "-";

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
        public async Task<ActionResult> SaveVoucher([FromBody] Tbl201VoucherMaster VM)
        {
            if (VM == null)
            {
                return BadRequest(new { success = false, message = "Invalid data received." });
            }

            try
            {
                _context.Tbl201VoucherMasters.Add(VM);
                await _context.SaveChangesAsync();
                //return Json(new { VoucherEntryNo = VE.VoucherNo });
                return Ok(new { success = true, message = "Data inserted successfully!" });
            }
            catch (Exception ex)
            {

                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }


        }

        [HttpPost]
        public async Task<ActionResult> DeleteVoucherEntry(DataSourceLoadOptions loadOptions, long voucherEntryNo, string VoucherNo /*string PaymentAccoutHeadName*/)
        {
            try
            {
                // Find the record to delete
                var record = await _context.Tbl201VoucherEntries.FirstOrDefaultAsync(v => v.VoucherEntryNo == voucherEntryNo);
                if (record == null)
                {
                    return NotFound(new { message = "Record not found!" });
                }

                // Remove the record
                _context.Tbl201VoucherEntries.Remove(record);
                await _context.SaveChangesAsync();

                // Get the list of updated vouchers
                var voucherEntries = _context.Tbl201VoucherEntries
                                              .Where(ve => ve.VoucherNo == VoucherNo) // Filter by the provided VoucherNo
                                              .ToList();

                var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();

                // Query the display list
                var qryListOfAccountLists = _context.Qry201VoucherEntryScreenDisplays
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

                var resultList = await qryListOfAccountLists.ToListAsync();

                //// Update the fields in the result list
                //foreach (var entry in resultList)
                //{
                //    if (!string.IsNullOrEmpty(entry.AccountHead))
                //    {
                //        // Find the account head
                //        var accountHead = _context.Qry201ListOfAccounts
                //                                  .Where(a => a.AccountId == entry.AccountHead)
                //                                  .Select(a => a.AccountHead)
                //                                  .FirstOrDefault();

                //        // Update AccountHead and SysRemarks
                //        entry.AccountHead = accountHead ?? PaymentAccoutHeadName;
                //        //entry.SysRemarks = PaymentAccoutHeadName;
                //    }
                //}

                // Return the modified list for DataSourceLoader
                return Json(DataSourceLoader.Load(resultList.AsQueryable(), loadOptions));
            }
            catch (Exception ex)
            {
                // Return a detailed error response
                return StatusCode(500, new { message = "An error occurred while deleting the record.", error = ex.Message });
            }
        }
    }
}
