using System.Data.SqlClient;
using System.Security.Policy;
using DevExpress.DataProcessing.InMemoryDataProcessor;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.DAL.Entities;

namespace PaymentForm.Areas.Finance.Controllers
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
            var tbl201vouchermasters = _context.Tbl201VoucherMasters.Select(i => new {
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

        [HttpGet]
        public async Task<ActionResult> GetCPPaymentAccounts(DataSourceLoadOptions loadOptions)
        {

            //var qryListOfAccountlists = _context.QryCashAndBankAccounts.Where(p => p.AccountGroupId != null).Select(i => new
            var qryListOfAccountlists = _context.Qry201ListOfAccounts.Where(p => p.AccountGroupId == "A012").Select(i => new
            {
                i.AccountHead,
                i.AccountId,
                i.AccountHeadArabic,
                i.IsLedgerObselete

                //i.AccountGroup,
                //i.MasterGroup,
                //i.MasterGroupId

            });

            return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
        }

        [HttpGet]
        public async Task<ActionResult> GetBPPaymentAccounts(DataSourceLoadOptions loadOptions)
        {

            //var qryListOfAccountlists = _context.QryCashAndBankAccounts.Where(p => p.AccountGroupId != null).Select(i => new
            var qryListOfAccountlists = _context.Qry201ListOfAccounts.Where(p => p.AccountGroupId == "A013").Select(i => new
            {
                i.AccountHead,
                i.AccountId,
                i.AccountHeadArabic,
                i.IsLedgerObselete


            });

            return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
        }

        [HttpGet]
        public async Task<ActionResult> GetVoucherEntryPaymentGrid(DataSourceLoadOptions loadOptions)
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
                //,
                //i.,
                //i.MasterGroup,
                //i.MasterGroupId

            });

            return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
        }

        [HttpGet]
        public async Task<IActionResult> GetAccountHead(DataSourceLoadOptions loadOptions)
        {
            //var tbl201vouchermasters = _context.Tbl201VoucherEntries.Select(i => new {
            //    i.AccountHead,
            //    i.VoucherNo //not available table in accountid

            //});
            var qryListOfAccountlists = _context.Qry201ListOfAccounts.Where(p => p.AccountGroupId == "A012" || p.AccountGroupId == "A003").Select(i => new
            {
                //i.MasterGroupId,
                //i.MasterGroup,
                i.AccountId,
                i.AccountHead,
                i.AccountGroup,
                i.AccountHeadArabic,
                i.ReferenceNo,
                i.IsLedgerObselete
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
                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }
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

        //[HttpPost]
        //public async Task<ActionResult> GetNewTemporaryVoucherNo(string VoucherString,string AccountType)
        //{
           

        //    try
        //    {
        //        string prefix = "";
        //        DateTime CurrentDate = DateTime.Now;
        //        string sMonth = DateTime.Now.ToString("MM");
        //        if (AccountType=="Cash Payment")
        //        {
        //            prefix = "CP";
        //        }
        //        else if(AccountType == "Bank Payment")
        //        {
        //            prefix = "BP";
        //        }
        //        SqlConnection con = new SqlConnection();
        //        con.Open(); 
        //        SqlCommand cmd = new SqlCommand(prefix, con);   
             
        //    }
        //    catch (Exception ex)
        //    {

        //        return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
        //    }


        //}

        [HttpGet]
        public async Task<ActionResult> GetNewCPVoucherNo(DataSourceLoadOptions loadOptions)
        {
            // Get the voucher No. string and Get the next serial of the voucher No.
            DateTime currentDate = DateTime.Now;
            string currentYear = currentDate.Year.ToString();
            string currentMonth = currentDate.Month.ToString("00");
            string voucherString = "CP-" + currentYear.Substring(currentYear.Length - 2, 2) + "-" + currentMonth.Substring(currentMonth.Length - 2, 2) + "-";
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
            // Get the voucher No. string and Get the next serial of the voucher No.
            DateTime currentDate = DateTime.Now;
            string currentYear = currentDate.Year.ToString();
            string currentMonth = currentDate.Month.ToString("00");
            string voucherString = "BP-" + currentYear.Substring(currentYear.Length - 2, 2) + "-" + currentMonth.Substring(currentMonth.Length - 2, 2) + "-";
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
                _context.Tbl20113ChequeMasters.Add(CM);
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


        //[HttpGet]
        //public IActionResult GetPreview()
        //{
        //    var previewData = new
        //    {
        //        Title = "Preview Example",
        //        Description = "This is some example data for the preview functionality."
        //    };
        //    return Ok(previewData); // Returns JSON automatically
        //}







    }

}
