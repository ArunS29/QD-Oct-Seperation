using QD.ERP.Web.DAL.Entities;
using QDERPWeb.Models;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
//using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using DevExpress.CodeParser;
using DevExpress.XtraReports.UI;
using QD.ERP.Web.Reports;
using QD.ERP.Web.Areas.Finance.Models;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    //[Area("Finance")]
    //[ApiController]
    public class AccountingLedgersController : Controller
    {
        private ERPMasterWtDataContext _context;
        //private ERPMasterWtDataContextProcedures _contextProcedure;
        public AccountingLedgersController(ERPMasterWtDataContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult> Get(DataSourceLoadOptions loadOptions)
        {
            var qryListOfAccountlists = _context.Qry201ListOfAccounts.Select(i => new
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
            //var stProAccountLedgerList = await _contextProcedure.StProAccountLedgerAsync("L00567", from,to);
            //return Json(stProAccountLedgerList);
        }
        [HttpGet]
        public async Task<ActionResult> GetLedgerAccounts(DataSourceLoadOptions loadOptions)
        {
            try {
                var qryListOfAccountlists = _context.Qry201ListOfAccounts.Where(p => p.AccountId != null).Select(i => new
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
            catch (Exception ex)
            {
                return StatusCode(500,$"{ ex.Message}");
            }
            }
        [HttpGet]
        public async Task<ActionResult> GetVouchers(string accountId, string frmDate, string toDate)
        {
            try
            {
                if (!DateTime.TryParseExact(frmDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime from))
                {
                    return BadRequest("Invalid from date format. Use MM/dd/yyyy.");
                }

                if (!DateTime.TryParseExact(toDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime to))
                {
                    return BadRequest("Invalid to date format. Use MM/dd/yyyy.");
                }

                // ✅ No need to re-parse with DateTime.Parse()

                //ERPMasterWtDataContextProcedures _procedures = new ERPMasterWtDataContextProcedures(_context);
                //var ledgerData = await _procedures.StProAccountLedgerAsync(accountId, from, to);
                var ledgerData = await _context.AccountLedgers
       .FromSqlRaw("EXEC StProAccountLedger @p0, @p1, @p2", accountId, from, to)
       .ToListAsync();

                return Json(ledgerData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        //[HttpPost]
        //public IActionResult GenerateReport([FromBody] ReportRequest request)
        //{
        //    try
        //    {
        //        // Initialize the report
        //        AccountReport report = new AccountReport();

        //        // Ensure parameters exist and set values
        //        if (report.Parameters["AccountId"] != null)
        //            report.Parameters["AccountId"].Value = request.accountId;
        //        if (report.Parameters["FromDate"] != null)
        //            report.Parameters["FromDate"].Value = request.frmDate;
        //        if (report.Parameters["ToDate"] != null)
        //            report.Parameters["ToDate"].Value = request.toDate;

        //        // Disable request parameter validation
        //        report.RequestParameters = false;

        //        // Assign data source dynamically (if applicable)
        //        report.DataSource = GetVouchers(request.accountId, request.frmDate, request.toDate);

        //        // Export the report to a PDF
        //        using (MemoryStream reportStream = new MemoryStream())
        //        {
        //            report.ExportToPdf(reportStream);
        //            reportStream.Seek(0, SeekOrigin.Begin);

        //            // Generate a unique file name
        //            string fileName = $"AccountReport_{Guid.NewGuid()}.pdf";
        //            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "reports", fileName);

        //            // Save the report file to the server
        //            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        //            {
        //                reportStream.CopyTo(fileStream);
        //            }

        //            // Return the file URL to the client
        //            string fileUrl = $"/reports/{fileName}";
        //            return Ok(new { fileUrl });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "An error occurred while generating the report", error = ex.Message });
        //    }
        //}

		[HttpGet]
		public IActionResult GetAccountId(string id)
		{
			try
			{
				if (string.IsNullOrEmpty(id))
				{
					return Json(new { success = false, message = "Invalid Cost Center ID." });
				}

				var costCenter = _context.Tbl201ChartOfAccounts.FirstOrDefault(c => c.AccountId == id);
				if (costCenter == null)
				{
					return Json(new { success = false, message = "Cost Center not found." });
				}

				return Json(new { success = true, data = costCenter });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message });
			}
		}
	}
}