using DevExpress.XtraReports.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QD.ERP.Web.Areas.Finance.Reports;
using QD.ERP.Web.Areas.Finance.Reports.BillsReceivable;
using QD.ERP.Web.Areas.Finance.Reports.Payable_Statements;
using QD.ERP.Web.Areas.Finance.Reports.Receivable_Statements;
using QD.ERP.Web.Reports;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace QD.ERP.Web.Pages
{
    public class ReportDesignerModel : PageModel
    {
        public XtraReport Report { get; private set; }
        public string ReportName { get; private set; }

        private static readonly HashSet<string> reportsRequiringParameters = new()
        {
            "StatementOfAccountReport", "AccountWithNarration", "AccountExportFromatReport",
            "AccountExportLandscapeReport", "AccountStatementFormat2Report",
            "AccountOrderbyVchNoWONarrationReport", "BillsReceivablelandscapeformat",
            "BillsReceivableLedgerBalance", "BillsReceivableRentation",
            "BillsReceivableAgeingToday", "BillsReceivableByAccount",
            "BillsReceivableAll", "BillsReceivableFormat", "rpt201BillsPayable",
            "rpt201BillsPayableWithVchNo", "AgeingToday", "EndDate", "Report4",
            "AccountDetails","AccountOrderByVoucherNo","Payablelandscape","payableRetention","Balance",
            "BillsPayablePaid","AgeingReport","BIllsPayable","ReceivableReport_EffectiveDate_","XtraRecivableReport",
            "XtraReportAgeingreportsummary"," XtraReportBillsReceivableAgeingReport",
            "rpt201BillsPayble",""

        };

        private static readonly Dictionary<string, Func<string, DateTime, DateTime, XtraReport>> parameterizedReports =
            new()
            {
                { "StatementOfAccountReport", (id, from, to) => new StatementOfAccountReport(id, from, to,"","","",null,"","") },
                { "AccountWithNarration", (id, from, to) => new AccountWithNarration(id, from, to, "", "", "", null, "", "") },
                  { "AccountDetails", (id, from, to) => new AccountDetails(id, from, to) },
                {"AccountOrderByVoucherNo",( id, from, to)=> new AccountOrderByVoucherNo(id, from, to) },
                { "AccountExportFromatReport", (id, from, to) => new AccountExportFromatReport(id, from, to) },
                { "AccountExportLandscapeReport", (id, from, to) => new AccountExportLandscapeReport(id, from, to) },
                { "AccountStatementFormat2Report", (id, from, to) => new AccountStatementFormat2Report(id, from, to, "", "", "", null, "", "") },
                { "AccountOrderbyVchNoWONarrationReport", (id, from, to) => new AccountOrderbyVchNoWONarrationReport(id, from, to) },
                { "BillsReceivablelandscapeformat", (id, from, to) => new BillsReceivablelandscapeformat(id, from, to,"","","",null,"","") },
                { "BillsReceivableLedgerBalance", (id, from, to) => new BillsReceivableLedgerBalance(id, from, to,"","","",null,"","") },
                { "BillsReceivableRentation", (id, from, to) => new BillsReceivableRentation(id, from, to,"","","",null,"","") },
                { "BillsReceivableAgeingToday", (id, from, to) => new BillsReceivableAgeingToday(id, from, to,"","","",null,"","") },
                { "BillsReceivableByAccount", (id, from, to) => new BillsReceivableByAccount(id, from, to,"","","",null,"","") },
                { "BillsReceivableAll", (id, from, to) => new BillsReceivableAll(id, from, to,"","","",null,"","") },
                { "BillsReceivableFormat", (id, from, to) => new BillsReceivableFormat(id, from, to,"","","",null,"","") },
                 { "Report4", (id, from, to) => new Report4(id, from, to,"","","",null,"","") },

                { "rpt201BillsPayable", (id, from, to) => new rpt201BillsPayable(id, from, to,"","","",null,"","") },
                { "rpt201BillsPayableWithVchNo", (id, from, to) => new rpt201BillsPayableWithVchNo(id, from, to,"","","",null,"","") },
                { "AgeingToday", (id, from, to) => new AgeingToday(id, from, to,"","","",null,"","") },
                { "EndDate", (id, from, to) => new EndDate(id, from, to,"","","",null,"","") },
                {"Payablelandscape",( id, from, to)=> new Payablelandscape(id, from, to,"","","",null,"","") },
                {"payableRetention",(id,from,to )=>new payableRetention(id, from, to,"","","",null,"","") },
                {"Balance",(id,from,to )=>new Balance(id, from, to,"","","",null,"","")   },
                {"BillsPayablePaid",(id,from,to )=>new BillsPayablePaid(id, from, to,"","","",null,"","")   },
                



            };

        private static readonly Dictionary<string, Func<XtraReport>> simpleReports = new()
        {
            { "XtraReportBillsReceivableAgeingReport", () => new XtraReportBillsReceivableAgeingReport() },
            { "XtraReportAgeingreportsummary", () => new XtraReportAgeingreportsummary() }
        };

        public IActionResult OnGet(string reportName, string accountId, DateTime? frmDate, DateTime? toDate)
        {
            if (string.IsNullOrEmpty(reportName))
                return BadRequest("Invalid report name.");

            ReportName = reportName;

            if (reportsRequiringParameters.Contains(reportName))
            {
                if (string.IsNullOrEmpty(accountId) || frmDate == null || toDate == null)
                    return BadRequest($"Missing required parameters for {reportName}.");

                Report = parameterizedReports.ContainsKey(reportName)
                    ? parameterizedReports[reportName](accountId, frmDate.Value, toDate.Value)
                    : null;
            }
            else
            {
                Report = simpleReports.ContainsKey(reportName) ? simpleReports[reportName]() : null;
            }

            return Report == null ? NotFound("Report not found.") : Page();
        }
    }
}
