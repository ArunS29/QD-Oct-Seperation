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
        public string AccountId { get; private set; }
        public DateTime FrmDate { get; private set; }
        public DateTime ToDate { get; private set; }

        public IActionResult OnGet(string reportName, string accountId, DateTime? frmDate, DateTime? toDate)
        {
            if (string.IsNullOrEmpty(reportName))
            {
                return BadRequest("Invalid report name.");
            }

            ReportName = reportName;

            if (accountId != null && frmDate != null && toDate != null)
            {
                AccountId = accountId;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;
            }

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
                    Report = new StatementOfAccountReport(AccountId, FrmDate, ToDate);
                    break;
                case "AccountWithNarration":
                    Report = new AccountWithNarration(AccountId, FrmDate, ToDate);
                    break;
                case "AccountExportFromatReport":
                    Report = new AccountExportFromatReport(AccountId, FrmDate, ToDate);
                    break;
                case "AccountExportLandscapeReport":
                    Report = new AccountExportLandscapeReport(AccountId, FrmDate, ToDate);
                    break;
                case "AccountStatementFormat2Report":
                    Report = new AccountStatementFormat2Report(AccountId, FrmDate, ToDate);
                    break;
                case "AccountOrderbyVchNoWONarrationReport":
                    Report = new AccountOrderbyVchNoWONarrationReport(AccountId, FrmDate, ToDate);
                    break;
                case "BillsReceivablelandscapeformat":
                    Report = new BillsReceivablelandscapeformat(AccountId, FrmDate, ToDate);
                    break;
                case "AccountOrderByVoucherNo":
                    Report = new AccountOrderByVoucherNo(AccountId, FrmDate, ToDate);
                    break;
                default:
                    return NotFound("Report not found.");
            }

            return Page(); // Return page with report design loaded
        }
    }
}
