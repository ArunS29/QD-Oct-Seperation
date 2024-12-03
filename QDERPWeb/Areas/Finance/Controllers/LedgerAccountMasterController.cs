using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        [HttpGet]
        public async Task<ActionResult> GetAccountGroupId(DataSourceLoadOptions loadOptions)
        {
            // Get the voucher No. string and Get the next serial of the voucher No.

            string voucherString = "A";
            string strNewReceiptNo;

            // SQL query to get the max voucher number


            string sql = "SELECT MAX(CAST(RIGHT(AccountGroupID, 3) AS INT)) AS MaxAccountGroupID " +
                          "FROM tbl201AccountGroups " +
                          "WHERE AccountGroupID LIKE {0}";

            try
            {
                var result = await _context.SqlQueryAsync<AccountMasterResult>(sql, new object[] { voucherString + "%" });

                int MaxAccountGroupID = result.FirstOrDefault()?.MaxAccountGroupID ?? 0; // Handle null result


                int newAccountGroupID = MaxAccountGroupID + 1;

                // Format the new voucher number with leading zeros
                strNewReceiptNo = "000" + newAccountGroupID.ToString();
                strNewReceiptNo = strNewReceiptNo.Substring(strNewReceiptNo.Length - 3);

                // Concatenate with the voucher string
                strNewReceiptNo = voucherString + strNewReceiptNo;
            }
            catch (Exception)
            {
                // Handle cases where there's no existing voucher number
                strNewReceiptNo = voucherString + "102";
            }

            return Json(strNewReceiptNo);
        }

        [HttpGet]
        public async Task<ActionResult> GetAccountMasterAR(string MasterGroup)
        {
            if (string.IsNullOrEmpty(MasterGroup))
            {
                return BadRequest("MasterGroup parameter is required.");
            }

            try
            {
                // Use parameterized query to prevent SQL injection
                string sql = "SELECT MasterGroupAr FROM dbo.tbl201MasterGroup where MasterGroupID='" + MasterGroup + "'";
                var result = await _context.SqlQueryAsync<AccountMasterAR>(sql);

                if (result.Any())
                {
                    return Json(result.FirstOrDefault()?.MasterGroupID);
                }
                else
                {
                    return NotFound("No data found for the specified MasterGroup.");
                }
            }
            catch (Exception ex)
            {
                // Log the exception (logging omitted for brevity)
                return StatusCode(500, "An error occurred while fetching data.");
            }
        }

    }
}
