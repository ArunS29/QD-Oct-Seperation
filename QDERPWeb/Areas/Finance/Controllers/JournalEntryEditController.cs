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
    public class JournalEntryEditController : Controller
    {
        private ERPMasterWtDataContext _context;

        public JournalEntryEditController(ERPMasterWtDataContext context)
        {
            _context = context;
        }
        //[HttpGet]
        //public async Task<ActionResult> GetNewVoucherNo()
        //{
        //    string voucherPrefix = "JV-REQ-";
        //    string yearPart = DateTime.Now.Year.ToString();
        //    string strNewVoucherNo;

        //    try
        //    {
        //        using (var transaction = await _context.Database.BeginTransactionAsync())
        //        {
        //            string sql = @"
        //        SELECT MAX(CAST(RIGHT(TempVoucherNo, 5) AS INT)) AS MaxVoucherNo
        //        FROM tbl20126JournalRegisterMaster WITH (TABLOCKX)
        //        WHERE TempVoucherNo LIKE {0}";

        //            var result = await _context.SqlQueryAsync<VoucherResult>(sql, new object[] { voucherPrefix + yearPart + "-%" });

        //            int maxVoucherNo = result.FirstOrDefault()?.MaxVoucherNo ?? 0;

        //            int newVoucherNo = maxVoucherNo + 1;

        //            strNewVoucherNo = $"{voucherPrefix}{yearPart}-{newVoucherNo:D5}";

        //            var newVoucherEntry = new Tbl20126JournalRegisterMaster
        //            {
        //                JournalRefNo = strNewVoucherNo
        //            };

        //            _context.Tbl20126JournalRegisterMasters.Add(newVoucherEntry);
        //            await _context.SaveChangesAsync();

        //            await transaction.CommitAsync();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, error = ex.Message });
        //    }

        //    return Json(strNewVoucherNo);
        //}
        [HttpGet]
        public async Task<ActionResult> GetNewVoucherNo(DataSourceLoadOptions loadOptions)
        {
            DateTime currentDate = DateTime.Now;
            string currentYear = currentDate.Year.ToString(); // e.g., 2025
            string voucherPrefix = $"JV-REQ-{currentYear}-";
            string strNewVoucherNo;

            // SQL query pattern for LIKE condition
            string likePattern = voucherPrefix + "%";

            try
            {
                // Query to get the max numeric part from the last voucher number
                var result = await _context.VoucherResults
                    .FromSqlInterpolated($@"
                SELECT MAX(CAST(RIGHT(VoucherNo, 5) AS INT)) AS MaxVoucherNo
                FROM tbl20126JournalRegisterMaster
                WHERE JournalRefNo LIKE {likePattern}")
                    .ToListAsync();

                int maxVoucherNo = result.FirstOrDefault()?.MaxVoucherNo ?? 0;

                int newVoucherNo = maxVoucherNo + 1;

                // Format the new voucher number with leading zeros (5 digits)
                strNewVoucherNo = newVoucherNo.ToString("D5");

                // Concatenate with the voucher prefix
                strNewVoucherNo = voucherPrefix + strNewVoucherNo;
            }
            catch (Exception)
            {
                // Handle cases where there's no existing voucher number
                strNewVoucherNo = voucherPrefix + "00001";
            }

            return Json(strNewVoucherNo);
        }

        [HttpPost]
        public async Task<ActionResult> Save([FromBody] Tbl20126JournalRegisterMaster VM)
        {
            if (VM == null)
            {
                return BadRequest(new { success = false, message = "Invalid data received." });
            }

            try
            {


                _context.Tbl20126JournalRegisterMasters.Add(VM);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Data inserted successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }
        }
        [HttpPost]
        public IActionResult GetJournalRegisterChild(DataSourceLoadOptions loadOptions)
        {
            var data = _context.Qry202101journalRegisterChildren
                .Select(x => new
                {
                    x.JournalChildNo,
                    x.AccountId,                    // Value field
                    x.AccountHead,                  // Display field
                    x.DrCr,
                    x.EntryNarration,
                    x.DrAmount,
                    x.CrAmount,
                    x.CostAllocationDescription,
                    x.EmployeeCostDescription,
                    x.PropertyCostDescription,
                    x.FormattedAmount,
                    x.TotalCostAllocated,
                    x.TotalEmpAllocated,
                    x.TotalEqpAllocted
                });

            return Json(DataSourceLoader.Load(data, loadOptions));
        }
        [HttpGet]
        public IActionResult LoadLedgerData(DataSourceLoadOptions loadOptions)
        {
            var data = _context.Qry20172LedgersForClaims
                .Select(x => new
                {
                    AccountID = x.AccountId,
                    AccountHead = x.AccountHead
                }).ToList();

            return Json(DataSourceLoader.Load(data, loadOptions));
        }
        [HttpGet]
        public IActionResult GetCostAllocationUnits()
        {
            var data = _context.Qry20172LedgersForClaims
                .Select(c => new
                {
                    c.AccountId,
                    c.AccountHead,
                    c.ReferenceNo,
                    c.AccountGroup,
                    c.AccountHeadArabic,
                    c.IsLedgerObselete

                }).ToList();

            return Ok(data);
        }


    }
}
