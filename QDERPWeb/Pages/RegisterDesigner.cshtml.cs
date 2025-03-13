using DevExpress.XtraReports.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QD.ERP.Web.Areas.Finance.Reports;
using QD.ERP.Web.Areas.Finance.Reports.AccountRegister;
using QD.ERP.Web.Areas.Finance.Reports.Payable_Statements;
using QD.ERP.Web.Areas.Finance.Reports.Receivable_Statements;
using QD.ERP.Web.Reports;
using System;
using System.Collections.Generic;

namespace QD.ERP.Web.Pages
{
    public class RegisterDesignerModel : PageModel
    {
        public XtraReport Report { get; private set; }
        public string ReportName { get; private set; }

        private static readonly HashSet<string> reportsRequiringParameters = new()
        {
            "PreviewRegister"
        };

        private static readonly Dictionary<string, Func<string, DateTime, DateTime, XtraReport>> parameterizedReports =
            new()
            {
                { "PreviewRegister", (type, from, to) => new PreviewRegister(type, from, to) }
            };

        private static readonly Dictionary<string, Func<XtraReport>> simpleReports = new()
        {
            { "XtraReportBillsReceivableAgeingReport", () => new XtraReportBillsReceivableAgeingReport() },
            { "XtraReportAgeingreportsummary", () => new XtraReportAgeingreportsummary() }
        };

        public IActionResult OnGet(string reportName, string voucherType, DateTime? frmDate, DateTime? toDate)
        {
            if (string.IsNullOrWhiteSpace(reportName))
            {
                return BadRequest("Invalid report name.");
            }

            ReportName = reportName;

            if (reportsRequiringParameters.Contains(reportName))
            {
                if (string.IsNullOrWhiteSpace(voucherType) || !frmDate.HasValue || !toDate.HasValue)
                {
                    return BadRequest($"Missing required parameters for {reportName}.");
                }

                if (parameterizedReports.TryGetValue(reportName, out var reportGenerator))
                {
                    Report = reportGenerator(voucherType, frmDate.Value, toDate.Value);
                }
            }
            else if (simpleReports.TryGetValue(reportName, out var simpleReportGenerator))
            {
                Report = simpleReportGenerator();
            }

            if (Report == null)
            {
                return NotFound("Report not found.");
            }

            return Page();
        }
    }
}
