
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using QD.ERP.Web.DAL.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.Areas.Finance.Reports;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    
    [Route("/Finance/api/[controller]/[action]")]
    [ApiController]
    public class BillsReceivableController : Controller
    {
        private ERPMasterWtDataContext _context;
        public BillsReceivableController(ERPMasterWtDataContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions, string filterType = null)
        {
            try
            {
                var query = _context.Qry20105BillsReceivableAgeingViews.Select(i => new
                {
                    i.AccountHeadNo,
                    i.AccountHead,
                    i.ReferenceNo,
                    i.VoucherRefNo,
                    i.VoucherDate,
                    i.InvoiceDueDate,
                    // i.invoiceAmount,
                    i.ReceivableAmount,
                    i.Received,
                    i.Balance,
                    i.OverdueDays
                });

                // Apply filter based on filterType
                if (filterType == "WithBalance")
                {
                    query = query.Where(i => i.Balance > 0); // Only show rows where Balance > 0
                }
                else if (filterType == "FullyReceived")
                {
                    query = query.Where(i => i.Balance <= 0); // Only show rows where Balance = 0
                }

                var result = await DataSourceLoader.LoadAsync(query, loadOptions);
                return Json(result);
            }
            catch (Exception ex)
            {

                return StatusCode(500, new { message = "An error occurred while processing the request.", error = ex.Message });
            }
        }
        public IActionResult GenerateReport()
  {
      // Create the report instance
      var report = new XtraReportBillsReceivableAgeingReport();
 
      // Export the report to PDF using a MemoryStream
      using (var stream = new MemoryStream())
      {
          report.ExportToPdf(stream);
          stream.Seek(0, SeekOrigin.Begin);
 
          // Return the PDF as a file result
          return File(stream.ToArray(), "application/pdf", "XtraReportBillsReceivableAgeingReport.pdf");
      }
  }
  public IActionResult GenerateAgeingreportsummaryReport()
  {
      // Create the report instance
 
      var report = new XtraReportAgeingreportsummary ();
 
      // Export the report to PDF using a MemoryStream
      using (var stream = new MemoryStream())
      {
          report.ExportToPdf(stream);
          stream.Seek(0, SeekOrigin.Begin);
 
          // Return the PDF as a file result
          return File(stream.ToArray(), "application/pdf", "XtraReportAgeingreportsummary.pdf");
      }
  }                                              
    }
}