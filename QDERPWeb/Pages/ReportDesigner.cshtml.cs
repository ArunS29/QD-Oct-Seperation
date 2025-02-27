using DevExpress.XtraReports.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QD.ERP.Web.Areas.Finance.Reports;
using QD.ERP.Web.Reports;

namespace QD.ERP.Web.Pages
{
    public class ReportDesignerModel : PageModel
    {
        public XtraReport Report { get; private set; }
        public string ReportName { get; private set; }

        public IActionResult OnGet(string reportName)
        {
            if (string.IsNullOrEmpty(reportName))
            {
                return BadRequest("Invalid report name.");
            }

            ReportName = reportName;

            // Load the appropriate report based on the report name for design mode
            switch (reportName)
            {
                case "XtraReportBillsReceivableAgeingReport":
                    Report = new XtraReportBillsReceivableAgeingReport();
                    break;
                case "XtraReportAgeingreportsummary":
                    Report = new XtraReportAgeingreportsummary();
                    break;
                case "StatementOfAccountReport":
                    Report = new StatementOfAccountReport();
                    break;
                case "AccountWithNarration":
                    Report = new AccountWithNarration();  // Specific report for Account with Narration
                    break;
                case "AccountExportFormatReport":
                    Report = new AccountExportFromatReport();
                    break;
                case "AccountExportLandscapeReport":
                    Report = new AccountExportLandscapeReport();
                    break;
                case "AccountStatementFormat2Report":
                    Report = new AccountStatementFormat2Report();
                    break;
                case "AccountOrderbyVchNoWONarrationReport":
                    Report = new AccountOrderbyVchNoWONarrationReport();
                    break;
                default:
                    return NotFound("Report not found.");
            }

            return Page(); // Return page with report design loaded
        }
    }
}
