using DevExpress.XtraReports.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QD.ERP.Web.Areas.Finance.Reports;
using QD.ERP.Web.Reports; // Ensure this includes your reports

namespace QD.ERP.Web.Pages
{
    public class DocumentViewerModel : PageModel
    {
        public XtraReport Report { get; private set; }

        public IActionResult OnGet(string reportName)
        {
            if (string.IsNullOrEmpty(reportName))
            {
                return BadRequest("Invalid report name.");
            }

            // Ensure reportName matches exactly what the controller sends
            switch (reportName)
            {
                case "XtraReportBillsReceivableAgeingReport":
                    Report = new XtraReportBillsReceivableAgeingReport();
                    break;
                case "XtraReportAgeingreportsummary":
                    Report = new XtraReportAgeingreportsummary();
                    break;
                case "AccountReport":
                    Report = new AccountReport();
                    break;
                case "AccountWithNarration":
                    Report = new AccountWithNarration();
                    break;
                default:
                    return NotFound("Report not found."); // Handle invalid report names
            }

            return Page(); // Continue loading the page with the selected report
        }
    }
}