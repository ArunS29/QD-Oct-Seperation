using DevExpress.XtraReports.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QD.ERP.Web.Areas.Finance.Reports;
using QD.ERP.Web.Areas.Finance.Reports.Payable_Statements;
using QD.ERP.Web.Areas.Finance.Reports.Receivable_Statements;
using QD.ERP.Web.Reports;
using System;

namespace QD.ERP.Web.Pages
{
    public class ReportDesignerModel : PageModel
    {
        public XtraReport Report { get; private set; }
        public string ReportName { get; private set; }

        public IActionResult OnGet(string reportName, string accountId, DateTime? frmDate, DateTime? toDate)
        {
            if (string.IsNullOrEmpty(reportName))
            {
                return BadRequest("Invalid report name.");
            }

            ReportName = reportName;

            if (NeedsParameters(reportName))
            {
                if (accountId == null || frmDate == null || toDate == null)
                {
                    return BadRequest($"Missing required parameters for {reportName}.");
                }

                DateTime fromDate = frmDate.Value;
                DateTime endDate = toDate.Value;

                Report = reportName switch
                {
                    "StatementOfAccountReport" => new StatementOfAccountReport(accountId, fromDate, endDate),
                    "AccountWithNarration" => new AccountWithNarration(accountId, fromDate, endDate),
                    "AccountExportFromatReport" => new AccountExportFromatReport(accountId, fromDate, endDate),
                    "AccountExportLandscapeReport" => new AccountExportLandscapeReport(accountId, fromDate, endDate),
                    "AccountStatementFormat2Report" => new AccountStatementFormat2Report(accountId, fromDate, endDate),
                    "AccountOrderbyVchNoWONarrationReport" => new AccountOrderbyVchNoWONarrationReport(accountId, fromDate, endDate),
                    "BillsReceivablelandscapeformat" => new BillsReceivablelandscapeformat(accountId, fromDate, endDate),
                    "BillsReceivableLedgerBalance" => new BillsReceivableLedgerBalance(accountId, fromDate, endDate),
                    "BillsReceivableRentation" => new BillsReceivableRentation(accountId, fromDate, endDate),
                    "BillsReceivableAgeingToday" => new BillsReceivableAgeingToday(accountId, fromDate, endDate),
                    "BillsReceivableByAccount" => new BillsReceivableByAccount(accountId, fromDate, endDate),
                    "BillsReceivableAll" => new BillsReceivableAll(accountId, fromDate, endDate),
                    "BillsReceivableFormat" => new BillsReceivableFormat(accountId, fromDate, endDate),
                    "rpt201BillsPayable" => new rpt201BillsPayable(accountId, fromDate, endDate),
                    "rpt201BillsPayableWithVchNo" => new rpt201BillsPayableWithVchNo(accountId, fromDate, endDate),
                    _ => null
                };
            }
            else
            {
                Report = reportName switch
                {
                    "XtraReportBillsReceivableAgeingReport" => new XtraReportBillsReceivableAgeingReport(),
                    "XtraReportAgeingreportsummary" => new XtraReportAgeingreportsummary(),
                    _ => null
                };
            }

            if (Report == null)
            {
                return NotFound("Report not found.");
            }

            return Page();
        }

        private bool NeedsParameters(string reportName)
        {
            return reportName switch
            {
                "StatementOfAccountReport" => true,
                "AccountWithNarration" => true,
                "AccountExportFromatReport" => true,
                "AccountExportLandscapeReport" => true,
                "AccountStatementFormat2Report" => true,
                "AccountOrderbyVchNoWONarrationReport" => true,
                "BillsReceivablelandscapeformat" => true,
                "BillsReceivableLedgerBalance" => true,
                "BillsReceivableRentation" => true,
                "BillsReceivableAgeingToday" => true,
                "BillsReceivableByAccount" => true,
                "BillsReceivableAll" => true,
                "BillsReceivableFormat" => true,
                "rpt201BillsPayable" => true,
                "rpt201BillsPayableWithVchNo" => true,
                _ => false
            };
        }
    }
}