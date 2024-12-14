//using DevExpress.Xpo;
using System.Xml.Linq;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.DAL.Entities;
using static DevExpress.Xpo.Helpers.AssociatedCollectionCriteriaHelper;

namespace PaymentForm.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class LedgerAccountMasterController : Controller
    {
        private ERPMasterWtDataContext _context;

        public LedgerAccountMasterController(ERPMasterWtDataContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult> SaveLedgerMasterDetails([FromBody] Tbl201AccountGroup AG)
        {
            if (AG == null)
            {
                return BadRequest(new { success = false, message = "Invalid data received." });
            }

            try
            {
                _context.Tbl201AccountGroups.Add(AG);
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
        public async Task<ActionResult> GetAccountMaster(DataSourceLoadOptions loadOptions)
        {

            var qryListOfAccountlists = _context.Tbl201MasterGroups.Select(i => new

            {
                i.MasterGroupId,
                i.MasterGroup,
                i.MasterGroupAr,
                i.IsCalculateOpeningBalance,
                i.IsProfitLossAccount,
                i.IsBalanceSheetAccount



            });

            return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
        }



        //[HttpGet]
        //public async Task<ActionResult> GetAccountGroupId(DataSourceLoadOptions loadOptions)
        //{
        //    // Get the voucher No. string and Get the next serial of the voucher No.

        //    string voucherString = "A";
        //    string strNewReceiptNo;

        //    // SQL query to get the max voucher number


        //    string sql = "SELECT MAX(CAST(RIGHT(AccountGroupID, 3) AS INT)) AS MaxAccountGroupID " +
        //                  "FROM tbl201AccountGroups " +
        //                  "WHERE AccountGroupID LIKE {0}";

        //    try
        //    {
        //        var result = await _context.SqlQueryAsync<AccountMasterResult>(sql, new object[] { voucherString + "%" });

        //        int MaxAccountGroupID = result.FirstOrDefault()?.MaxAccountGroupID ?? 0; // Handle null result


        //        int newAccountGroupID = MaxAccountGroupID + 1;

        //        // Format the new voucher number with leading zeros
        //        strNewReceiptNo = "000" + newAccountGroupID.ToString();
        //        strNewReceiptNo = strNewReceiptNo.Substring(strNewReceiptNo.Length - 3);

        //        // Concatenate with the voucher string
        //        strNewReceiptNo = voucherString + strNewReceiptNo;
        //    }
        //    catch (Exception)
        //    {
        //        // Handle cases where there's no existing voucher number
        //        strNewReceiptNo = voucherString + "102";
        //    }

        //    return Json(strNewReceiptNo);
        //}

        [HttpGet]
        public async Task<ActionResult> GetAccountGroupId(DataSourceLoadOptions loadOptions)
        {
            string voucherString = "A";
            string strNewReceiptNo;

            string likePattern = voucherString + "%";

            try
            {
                // Execute the raw SQL query with interpolated parameters
                var result = await _context.Set<AccountMasterResult>()
                    .FromSqlInterpolated($@"
                SELECT MAX(CAST(RIGHT(AccountGroupID, 3) AS INT)) AS MaxAccountGroupID
                FROM tbl201AccountGroups
                WHERE AccountGroupID LIKE {likePattern}")
                    .ToListAsync();

                int maxAccountGroupID = result.FirstOrDefault()?.MaxAccountGroupID ?? 0;

                int newAccountGroupID = maxAccountGroupID + 1;

                // Format the new AccountGroupID with leading zeros
                strNewReceiptNo = "000" + newAccountGroupID.ToString();
                strNewReceiptNo = strNewReceiptNo.Substring(strNewReceiptNo.Length - 3);

                strNewReceiptNo = voucherString + strNewReceiptNo;
            }
            catch (Exception)
            {
                // Handle cases where there's no existing AccountGroupID
                strNewReceiptNo = voucherString + "101";
            }

            return Json(strNewReceiptNo);
        }


       

        [HttpGet]
        public async Task<ActionResult> GetAccountMasterAR(string MasterGroup)
        {
            string val = "";
            try
           {
                if (string.IsNullOrEmpty(MasterGroup))
                {
                    return BadRequest("MasterGroup parameter is required.");
                }

                var result = await _context.Tbl201MasterGroups
                    .Where(x => x.MasterGroupId == MasterGroup)
                    .Select(x => x.MasterGroupAr)
                    .FirstOrDefaultAsync();
                val= result.ToString(); 
            }
            catch (ArgumentException argEx)
            {
                // Log the detailed exception for debugging
                Console.WriteLine($"ArgumentException: {argEx.Message}, ParamName: {argEx.ParamName}");
                return BadRequest($"Invalid argument: {argEx.ParamName}");
            }
            catch (Exception ex)
            {
                // Log the general exception
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(500, "An error occurred while processing your request.");
            }
            return Json(val);
        }

        


        [HttpGet]
        public async Task<ActionResult> GetPaymentAccounts(DataSourceLoadOptions loadOptions)
        {

            //var qryListOfAccountlists = _context.QryCashAndBankAccounts.Where(p => p.AccountGroupId != null).Select(i => new
            var qryListOfAccountlists = _context.Qry201ListOfAccounts.Where(p => p.AccountId != null).Select(i => new
            {
                i.MasterGroupId,
                i.MasterGroup,
                i.AccountGroupId,
                i.AccountId,
                i.AccountHead,
                i.AccountHeadArabic,
                i.ReferenceNo,
                i.IsLedgerObselete

            });

            return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
        }

       

        [HttpGet]
        public async Task<ActionResult> GetDocumentNo(DataSourceLoadOptions loadOptions)
        {
            // Get the voucher No. string and Get the next serial of the voucher No.

         
            int newAccountGroupID;
            string DocumentNo = "";
            // SQL query to get the max voucher number


            string sql = "SELECT MAX(CAST(RIGHT(DocumentNo, 3) AS INT)) AS MaxDocumentNo " +
                          "FROM Tbl20116LedgerDocuments ";

            try
            {
                var results = await _context.VoucherResults
    .FromSqlInterpolated($"SELECT MAX(CAST(RIGHT(DocumentNo, 3) AS INT)) AS MaxDocumentNo FROM Tbl20116LedgerDocuments")
    .ToListAsync();

                int MaxAccountGroupID = results.FirstOrDefault()?.MaxVoucherNo ?? 0; // Handle null result


                newAccountGroupID = MaxAccountGroupID + 1;
                DocumentNo = newAccountGroupID.ToString();


            }
            catch (Exception)
            {
                // Handle cases where there's no existing voucher number
                DocumentNo = "1";
            }

            return Json(DocumentNo);
        }

       


        //[HttpGet]
        //public IActionResult GetAgainstPayable([FromQuery] string inputParameter)
        //{
        //    var result = _context.Qry20167SalaryLedgerPayableBalances
        //        .Where(s => s.EmployeeNo == inputParameter)
        //        .Select(s => new
        //        {
        //            s.EmployeeNo,
        //            s.EmployeeName,
        //            s.ReferenceNo,
        //            s.PayableAmount,
        //            s.Paid,
        //            s.Balance
        //        });

        //    if (result == null || !result.Any())
        //    {
        //        return NotFound(new { Message = "No records found for the specified EmployeeNo." });
        //    }

        //    return Json(result);
        //}


    }
}
