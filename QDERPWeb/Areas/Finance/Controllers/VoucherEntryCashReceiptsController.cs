using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        //[HttpGet]
        //public async Task<IActionResult> GetAccountHead(string inputParameter)
        //{
        //    // Debugging log
        //    //Console.WriteLine($"Received selectedPaymentAccount: {selectedPaymentAccount}");

        //    // Ensure selectedPaymentAccount is not null or empty
        //    if (string.IsNullOrEmpty(inputParameter))
        //    {
        //        return BadRequest("selectedPaymentAccount cannot be null or empty");
        //    }

        //    // Query to fetch the accounts excluding the selected receiving account
        //    var tbl20101salespersonmasters = _context.Qry201ListOfAccounts
        //        .Where(p => p.AccountId != inputParameter) // Filter out the selected receiving account
        //        .Select(i => new
        //        {
        //            i.AccountId,
        //            i.AccountHead,
        //            i.AccountGroup,
        //            i.AccountHeadArabic,
        //            i.ReferenceNo,
        //            i.IsLedgerObselete
        //        });

        //    // Load the data and return as JSON
        //    // return Json(await DataSourceLoader.LoadAsync(tbl20101salespersonmasters, loadOptions));
        //    return Json(tbl20101salespersonmasters);
        //}

        [HttpGet]
        public async Task<IActionResult> GetAccountHead([FromQuery] string inputParameter)
        {
            if (string.IsNullOrEmpty(inputParameter))
            {
                Console.WriteLine("Error: inputParameter is null or empty.");
                return BadRequest("inputParameter cannot be null or empty");
            }

            Console.WriteLine($"Received inputParameter: {inputParameter}");

            var tbl20101salespersonmasters = _context.Qry201ListOfAccounts
                .Where(p => p.AccountId != inputParameter)
                .Select(i => new
                {
                    i.AccountId,
                    i.AccountHead,
                    i.AccountGroup,
                    i.AccountHeadArabic,
                    i.ReferenceNo,
                    i.IsLedgerObselete
                });

            return Json(tbl20101salespersonmasters);
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
        //[HttpPost]
        //public async Task<ActionResult> AddVoucherEntry(DataSourceLoadOptions loadOptions, [FromBody] Tbl201VoucherEntry VE)
        //{
        //    if (VE == null)
        //    {
        //        return BadRequest(new { success = false, message = "Invalid data received." });
        //    }

        //    try
        //    {
        //        _context.Tbl201VoucherEntries.Add(VE);
        //        await _context.SaveChangesAsync();
        //        var qryListOfAccountlists = _context.Qry201VoucherEntryScreenDisplays.Where(p => p.VoucherNo == VE.VoucherNo).Select(i => new
        //        {
        //            i.VoucherNo,
        //            i.DrCr,
        //            i.DrAmount,
        //            i.CrAmount,
        //            i.EntryNarration,
        //            i.AccountHead,
        //            i.SysRemarks,
        //        });

        //        return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
        //        //return Json(new { VoucherEntryNo = VE.VoucherNo });
        //        //return Ok(new { success = true, message = "Data inserted successfully!" });
        //    }
        //    catch (Exception ex)
        //    {

        //        return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
        //    }


        //}
        [HttpPost]
        public async Task<ActionResult> AddVoucherEntry(DataSourceLoadOptions loadOptions, [FromBody] List<Tbl201VoucherEntry> voucherEntries)
        {
            if (voucherEntries == null || !voucherEntries.Any())
            {
                return BadRequest(new { success = false, message = "Invalid data received." });
            }

            try
            {


                //var PaymentAccount = "Select AccountHead From tbl201ChartOfAccounts where AccountGroupID = 'A012'and AccountHead = ''";

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
                    });

                // Return the updated data as a JSON response
                return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException?.Message);
                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }
        }
        [HttpGet]

        public async Task<ActionResult> GetNewBRVoucherNo(DataSourceLoadOptions loadOptions)

        {

            // Get the voucher No. string and Get the next serial of the voucher No.

            DateTime currentDate = DateTime.Now;

            string currentYear = currentDate.Year.ToString();

            string currentMonth = currentDate.Month.ToString("00");

            string voucherString = "CR-" + currentYear.Substring(currentYear.Length - 2, 2) + "-" + currentMonth.Substring(currentMonth.Length - 2, 2) + "-";

            string strNewReceiptNo;

            // SQL query to get the max voucher number


            string sql = "SELECT MAX(CAST(RIGHT(VoucherNo, 3) AS INT)) AS MaxVoucherNo " +

                          "FROM tbl201VoucherMaster " +

                          "WHERE VoucherNo LIKE {0}";

            try

            {

                var result = await _context.SqlQueryAsync<VoucherResult>(sql, new object[] { voucherString + "%" });

                int maxVoucherNo = result.FirstOrDefault()?.MaxVoucherNo ?? 0; // Handle null result


                int newVoucherNo = maxVoucherNo + 1;

                // Format the new voucher number with leading zeros

                strNewReceiptNo = "000" + newVoucherNo.ToString();

                strNewReceiptNo = strNewReceiptNo.Substring(strNewReceiptNo.Length - 3);

                // Concatenate with the voucher string

                strNewReceiptNo = voucherString + strNewReceiptNo;

            }

            catch (Exception ex)

            {

                // Handle cases where there's no existing voucher number

                strNewReceiptNo = voucherString + "002";

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
    }
}
