using DevExpress.XtraReports.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Client;
using QD.ERP.Web.Areas.Finance.Reports;
using QD.ERP.Web.Areas.Finance.Reports.Payable_Statements;
using QD.ERP.Web.Areas.Finance.Reports.Receivable_Statements;
using QD.ERP.Web.Reports;

namespace QD.ERP.Web.Pages
{
    public class DocumentViewerModel : PageModel
    {
        // Property to hold the dynamically generated report
        public XtraReport Report { get; private set; }

        // Property to hold the report name
        public string ReportName { get; private set; }

        // Properties to hold parameters for StatementOfAccountReport
        public string AccountId { get; private set; }
        public DateTime FrmDate { get; private set; }
        public DateTime ToDate { get; private set; }

        public IActionResult OnGet(string reportName, string accountId, DateTime? frmDate, DateTime? toDate)
        {
            if (string.IsNullOrEmpty(reportName))
            {
                return BadRequest("Invalid report name.");
            }

            ReportName = reportName;  // Set the ReportName property


            if (reportName == "StatementOfAccountReport")
            {
                // Ensure required parameters are provided for StatementOfAccountReport
                if (accountId == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for StatementOfAccountReport.");
                }

                // Set the properties for the report
                AccountId = accountId;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;


                Report = new StatementOfAccountReport(AccountId, FrmDate, ToDate);

            }
            else if (reportName == "AccountWithNarration")
            {
                // Ensure required parameters are provided for StatementOfAccountReport
                if (accountId == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for AccountWithNarration.");
                }

                // Set the properties for the report
                AccountId = accountId;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;


                Report = new AccountWithNarration(AccountId, FrmDate, ToDate);
            }
            else if (reportName == "AccountStatementFormat2Report")
            {
                // Ensure required parameters are provided for StatementOfAccountReport
                if (accountId == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for AccountStatementFormat2Report.");
                }

                // Set the properties for the report
                AccountId = accountId;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;


                Report = new AccountStatementFormat2Report(AccountId, FrmDate, ToDate);
            }
            else if (reportName == "AccountExportFromatReport")
            {
                // Ensure required parameters are provided for StatementOfAccountReport
                if (accountId == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for AccountExportFromatReport.");
                }

                // Set the properties for the report
                AccountId = accountId;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;


                Report = new AccountExportFromatReport(AccountId, FrmDate, ToDate);
            }
            else if (reportName == "AccountOrderbyVchNoWONarrationReport")
            {
                // Ensure required parameters are provided for StatementOfAccountReport
                if (accountId == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for AccountOrderbyVchNoWONarrationReport.");
                }

                // Set the properties for the report
                AccountId = accountId;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;


                Report = new AccountOrderbyVchNoWONarrationReport(AccountId, FrmDate, ToDate);
            }
            else if (reportName == "AccountExportLandscapeReport")
            {
                // Ensure required parameters are provided for StatementOfAccountReport
                if (accountId == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for AccountExportLandscapeReport.");
                }

                // Set the properties for the report
                AccountId = accountId;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;


                Report = new AccountExportLandscapeReport(AccountId, FrmDate, ToDate);
            }
            else if (reportName == "AccountOrderbyVchNoWONarrationReport")
            {
                // Ensure required parameters are provided for StatementOfAccountReport
                if (accountId == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for AccountOrderbyVchNoWONarrationReport.");
                }

                // Set the properties for the report
                AccountId = accountId;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;


                Report = new AccountOrderbyVchNoWONarrationReport(AccountId, FrmDate, ToDate);
            }
            //else if (reportName == "BillsReceivablelandscapeformat")
            //{
            //    // Ensure required parameters are provided for StatementOfAccountReport
            //    if (accountId == null || frmDate == null || toDate == null)
            //    {
            //        return BadRequest("Missing required parameters for BillsReceivablelandscapeformat.");
            //    }

            //    // Set the properties for the report
            //    AccountId = accountId;
            //    FrmDate = frmDate.Value;
            //    ToDate = toDate.Value;


            //    Report = new BillsReceivablelandscapeformat(AccountId, FrmDate, ToDate);
            //}
            else if (reportName == "AccountOrderByVoucherNo")
            {
                // Ensure required parameters are provided for StatementOfAccountReport
                if (accountId == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for AccountOrderByVoucherNo.");
                }

                // Set the properties for the report
                AccountId = accountId;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;


                Report = new AccountOrderByVoucherNo(AccountId, FrmDate, ToDate);
            }




            else if (reportName == "BillsReceivablelandscapeformat")
            {
                // Ensure required parameters are provided for StatementOfAccountReport
                if (accountId == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for BillsReceivablelandscapeformat.");
                }

                // Set the properties for the report
                AccountId = accountId;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;


                Report = new BillsReceivablelandscapeformat(AccountId, FrmDate, ToDate);
            }
            else if (reportName == "BillsReceivableAll")
            {
                // Ensure required parameters are provided for StatementOfAccountReport
                if (accountId == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for BillsReceivableAll.");
                }

                // Set the properties for the report
                AccountId = accountId;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;


                Report = new BillsReceivableAll(AccountId, FrmDate, ToDate);
            }
            else if (reportName == "BillsReceivableByAccount")
            {
                // Ensure required parameters are provided for StatementOfAccountReport
                if (accountId == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for BillsReceivableAll.");
                }

                // Set the properties for the report
                AccountId = accountId;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;


                Report = new BillsReceivableByAccount(AccountId, FrmDate, ToDate);
            }
            else if (reportName == "BillsReceivableAgeingToday")
            {
                // Ensure required parameters are provided for StatementOfAccountReport
                if (accountId == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for BillsReceivableAll.");
                }

                // Set the properties for the report
                AccountId = accountId;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;


                Report = new BillsReceivableAgeingToday(AccountId, FrmDate, ToDate);
            }



            //else if (reportName == " Report4")
            //{
            //    // Ensure required parameters are provided for StatementOfAccountReport
            //    if (accountId == null || frmDate == null || toDate == null)
            //    {
            //        return BadRequest("Missing required parameters for BillsReceivableAll.");
            //    }

            //    // Set the properties for the report
            //    AccountId = accountId;
            //    FrmDate = frmDate.Value;
            //    ToDate = toDate.Value;


            //    Report = new Report4(AccountId, FrmDate, ToDate);
            //}

            else if (reportName == "BillsReceivableRentation")
            {
                // Ensure required parameters are provided for StatementOfAccountReport
                if (accountId == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for BillsReceivableAll.");
                }

                // Set the properties for the report
                AccountId = accountId;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;


                Report = new BillsReceivableRentation(AccountId, FrmDate, ToDate);
            }

            else if (reportName == "BillsReceivableLedgerBalance")
            {
                // Ensure required parameters are provided for StatementOfAccountReport
                if (accountId == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for BillsReceivableAll.");
                }

                // Set the properties for the report
                AccountId = accountId;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;


                Report = new BillsReceivableLedgerBalance(AccountId, FrmDate, ToDate);
            }

            else if (reportName == "BillsReceivableFormat")
            {
                // Ensure required parameters are provided for StatementOfAccountReport
                if (accountId == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for BillsReceivableAll.");
                }

                // Set the properties for the report
                AccountId = accountId;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;


                Report = new BillsReceivableFormat(AccountId, FrmDate, ToDate);
            }
            else if (reportName == "rpt201BillsPayable")

            {

                // Ensure required parameters are provided for StatementOfAccountReport

                if (accountId == null || frmDate == null || toDate == null)

                {

                    return BadRequest("Missing required parameters for rpt201BillsPayable.");

                }

                // Set the properties for the report

                AccountId = accountId;

                FrmDate = frmDate.Value;

                ToDate = toDate.Value;


                Report = new rpt201BillsPayable(AccountId, FrmDate, ToDate);

            }

            else if (reportName == "rpt201BillsPayableWithVchNo")

            {

                // Ensure required parameters are provided for StatementOfAccountReport

                if (accountId == null || frmDate == null || toDate == null)

                {

                    return BadRequest("Missing required parameters for rpt201BillsPayableWithVchNo.");

                }

                // Set the properties for the report

                AccountId = accountId;

                FrmDate = frmDate.Value;

                ToDate = toDate.Value;


                Report = new rpt201BillsPayableWithVchNo(AccountId, FrmDate, ToDate);

            }

            else
            {
                switch (reportName)
                {
                    case "XtraReportBillsReceivableAgeingReport":
                        Report = new XtraReportBillsReceivableAgeingReport();
                        break;
                    case "XtraReportAgeingreportsummary":
                        Report = new XtraReportAgeingreportsummary();
                        break;
                    //case "StatementOfAccountReport":
                    //    //Report = new StatementOfAccountReport();
                    //    break;
                    //case "AccountWithNarration":
                    //    Report = new AccountWithNarration();
                    //    break;
                    //case "AccountExportFormatReport":
                    //    Report = new AccountExportFromatReport();
                    //    break;
                    //case "AccountExportLandscapeReport":
                    //    Report = new AccountExportLandscapeReport();
                    //    break;
                    //case "AccountStatementFormat2Report":
                    //    Report = new AccountStatementFormat2Report();
                    //    break;
                    ////case "AccountOrderbyVchNoWONarrationReport":
                    ////    Report = new AccountOrderbyVchNoWONarrationReport();
                    ////    break;
                    default:
                        return NotFound("Report not found.");
                }
            }

            // Dynamically set the Report based on the provided reportName


            // Return the page and bind the Report to the Razor page
            return Page();
        }
    }
}