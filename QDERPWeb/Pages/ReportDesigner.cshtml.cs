using DevExpress.XtraReports.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QD.ERP.Web.Areas.Finance.Reports;
using QD.ERP.Web.Areas.Finance.Reports.BillsReceivable;
using QD.ERP.Web.Areas.Finance.Reports.Payable_Statements;
using QD.ERP.Web.Areas.Finance.Reports.Receivable_Statements;
using QD.ERP.Web.Areas.Finance.Reports.TrialBalance;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Reports;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Client;
using QD.ERP.Web.Areas.Finance.Reports.Register;

namespace QD.ERP.Web.Pages
{
    public class ReportDesignerModel : PageModel
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private ERPMasterWtDataContext _eRPMasterWtDataContext;
        private Tbl901CompanyDetail ERPCompany_details;

        public XtraReport Report { get; private set; }
        public string ReportName { get; private set; }

        public ReportDesignerModel(TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
        }

        public IActionResult OnGet(string reportName, string accountId, DateTime? frmDate, DateTime? toDate, String accountGroup)
        {
            if (string.IsNullOrEmpty(reportName))
                return BadRequest("Invalid report name.");

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
            {
                return StatusCode(500, "Tenant not found or DbContext could not be created.");
            }

            if (!frmDate.HasValue || !toDate.HasValue)
                return BadRequest("From Date and To Date are required.");

            _eRPMasterWtDataContext = dbContext;
            ReportName = reportName;
            string username = HttpContext.Session.GetString("UserName") ?? "Default User";
            string tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
            var defaultCompanyIdString = HttpContext.Session.GetString("DefaultcompanyID");

            // Parse it to int (you may want to use long or Guid if that's your actual ID type)
            if (!int.TryParse(defaultCompanyIdString, out int defaultCompanyId))
            {
                // Handle invalid or missing ID (fallback or error handling)
                defaultCompanyId = 0; // or return early / throw error
            }
            // Company Info


            ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                .FirstOrDefault(x => x.CompanyId == defaultCompanyId);

            string companyName = ERPCompany_details?.CompanyName ?? string.Empty;
            string companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
            string companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
            string companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

            // Load logoImage
            Image logoImage = null;
            if (ERPCompany_details?.CompanyLogo is byte[] logoBytes && logoBytes.Length > 0)
            {
                try
                {
                    using var ms = new MemoryStream(logoBytes);
                    logoImage = Image.FromStream(ms);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error loading company logo: " + ex.Message);
                }
            }

            switch (reportName)
            {
                case "StatementOfAccountReport":
                    Report = new StatementOfAccountReport(accountId, frmDate.Value, toDate.Value,
                        tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, "", _tenantDbContextHelper);
                    Report.DisplayName = "StatementOfAccountReport";
                    break;

                case "AccountWithNarration":
                    Report = new AccountWithNarration(accountId, frmDate.Value, toDate.Value,
                        tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, "", _tenantDbContextHelper);
                    Report.DisplayName = "AccountWithNarration";
                    break;
                case "AccountDetails":
                    Report = new AccountDetails(accountId, frmDate.Value, toDate.Value,
                        tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, "", _tenantDbContextHelper);
                    Report.DisplayName = "AccountDetails";
                    break;
                case "AccountOrderByVoucherNo":
                    Report = new AccountOrderByVoucherNo(accountId, frmDate.Value, toDate.Value,
                        tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, "", _tenantDbContextHelper);
                    Report.DisplayName = "AccountOrderByVoucherNo";
                    break;
                case "AccountExportFromatReport":
                    Report = new AccountExportFromatReport(accountId, frmDate.Value, toDate.Value,
                        tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, "", _tenantDbContextHelper);
                    Report.DisplayName = "AccountExportFromatReport";
                    break;
                case "AccountExportLandscapeReport":
                    Report = new AccountExportLandscapeReport(accountId, frmDate.Value, toDate.Value,
                        tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, "", _tenantDbContextHelper);
                    Report.DisplayName = "AccountExportLandscapeReport";
                    break;
                case "AccountStatementFormat2Report":
                    Report = new AccountStatementFormat2Report(accountId, frmDate.Value, toDate.Value,
                        tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, "", _tenantDbContextHelper);
                    Report.DisplayName = "AccountStatementFormat2Report";
                    break;
                case "AccountOrderbyVchNoWONarrationReport":
                    Report = new AccountOrderbyVchNoWONarrationReport(accountId, frmDate.Value, toDate.Value,
                        tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, "", _tenantDbContextHelper);
                    Report.DisplayName = "AccountOrderbyVchNoWONarrationReport";
                    break;
                case "BillsReceivablelandscapeformat":
                    Report = new BillsReceivablelandscapeformat(accountId, frmDate.Value, toDate.Value,
                        tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, "", _tenantDbContextHelper);
                    Report.DisplayName = "BillsReceivablelandscapeformat";
                    break;
                case "BillsReceivableLedgerBalance":
                    Report = new BillsReceivableLedgerBalance(accountId, frmDate.Value, toDate.Value,
                        tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, "", _tenantDbContextHelper);
                    Report.DisplayName = "BillsReceivableLedgerBalance";
                    break;
                case "BillsReceivableRentation":
                    Report = new BillsReceivableRentation(accountId, frmDate.Value, toDate.Value,
                        tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, "", _tenantDbContextHelper);
                    Report.DisplayName = "BillsReceivableRentation";
                    break;
                case "BillsReceivableAgeingToday":
                    Report = new BillsReceivableAgeingToday(accountId, frmDate.Value, toDate.Value,
                        tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, username, _tenantDbContextHelper);
                    Report.DisplayName = "BillsReceivableAgeingToday";
                    break;
                case "BillsReceivableByAccount":
                    Report = new BillsReceivableByAccount(accountId, frmDate.Value, toDate.Value,
                        tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, "", _tenantDbContextHelper);
                    Report.DisplayName = "BillsReceivableByAccount";
                    break;

                case "BillsReceivableAll":
                    Report = new BillsReceivableAll(accountId, frmDate.Value, toDate.Value,
                        tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, "", _tenantDbContextHelper);
                    break;
                case "BillsReceivableFormat":
                    Report = new BillsReceivableFormat(accountId, frmDate.Value, toDate.Value,
                        tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, "", _tenantDbContextHelper);
                    break;
                case "Report4":
                    Report = new Report4(accountId, frmDate.Value, toDate.Value,
                        tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, "", _tenantDbContextHelper);
                    break;
                case "rpt201BillsPayable":
                    Report = new rpt201BillsPayable(accountId, frmDate.Value, toDate.Value,
                        tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, "", _tenantDbContextHelper);
                    break;
                case "rpt201BillsPayableWithVchNo":
                    Report = new rpt201BillsPayableWithVchNo(accountId, frmDate.Value, toDate.Value,
                        tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, "", _tenantDbContextHelper);
                    break;
                case "EndDate":
                    Report = new EndDate(accountId, frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr, username, _tenantDbContextHelper);
                    break;
                case "Payablelandscape":
                    Report = new Payablelandscape(accountId, frmDate.Value, toDate.Value,
                         tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr, _tenantDbContextHelper);
                    break;
                case "payableRetention":
                    Report = new payableRetention(accountId, frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr, username, _tenantDbContextHelper);
                    break;
                case "Balance":
                    Report = new Balance(accountId, frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr, username, _tenantDbContextHelper);
                    break;
                case "BillsPayablePaid":
                    Report = new BillsPayablePaid(accountId, frmDate.Value, toDate.Value,
                        tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, "", _tenantDbContextHelper);
                    break;
                case "AgeingToday":
                    Report = new AgeingToday(accountId, frmDate.Value, toDate.Value,
                        tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, "", _tenantDbContextHelper);
                    break;
                case "XtraReportBillsReceivableAgeingReport":
                    Report = new XtraReportBillsReceivableAgeingReport();
                    break;
                case "XtraReportAgeingreportsummary":
                    Report = new XtraReportAgeingreportsummary();
                    break;
                case "Group":
                    if (string.IsNullOrEmpty(accountGroup))
                        return BadRequest("Account group is required for Group report.");

                    Report = new Group(accountGroup, frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                    break;
                case "subGroup":
                    if (string.IsNullOrEmpty(accountGroup))
                        return BadRequest("Account group is required for subGroup report.");

                    Report = new subGroup(accountGroup, frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                    break;
                case "TrialBalanceReport":
                    if (string.IsNullOrEmpty(accountGroup))
                        return BadRequest("Account group is required for subGroup report.");
                    Report = new TrialBalanceReport(frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                    break;
                case "TrialBalanceExportFormat":
                    if (string.IsNullOrEmpty(accountGroup))
                        return BadRequest("Account group is required for subGroup report.");
                    Report = new TrialBalance_ExportFormat_(frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                    break;
                case "TrialBalanceDrCr":
                    if (string.IsNullOrEmpty(accountGroup))
                        return BadRequest("Account group is required for subGroup report.");
                    Report = new TrialBalanceDrCr(frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                    break;
                case "IncomeStatements":
                    if (string.IsNullOrEmpty(accountGroup))
                        return BadRequest("Account group is required for subGroup report.");
                    Report = new IncomeStatements(accountGroup, frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, username);
                    break;
                case "incomeStatementsBymonth":
                    if (string.IsNullOrEmpty(accountGroup))
                        return BadRequest("Account group is required for subGroup report.");
                    Report = new incomeStatements_Bymonth_(frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, username);
                    break;
                case "balnceSheet":
                    if (string.IsNullOrEmpty(accountGroup))
                        return BadRequest("Account group is required for subGroup report.");
                    //Report = new balnceSheet(accountGroup,  toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                    break;
                case "BalanceSheetHorizondalFormat":
                    if (string.IsNullOrEmpty(accountGroup))
                        return BadRequest("Account group is required for subGroup report.");
                    Report = new BalanceSheetHorizondalFormat(accountGroup, frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                    break;


                default:
                    return NotFound("Report not found.");


            }

            return Page();


        }


        //if (!string.IsNullOrEmpty(accountGroup) && frmDate.HasValue && toDate.HasValue)
        // {
        //     switch (reportName)
        //     {
        //         case "Group":
        //             Report = new Group(
        //                 accountGroup, // Use accountGroup, not accountId
        //                 frmDate.Value,
        //                 toDate.Value,
        //                 tenantName,
        //                 companyName,
        //                 companyAddress,
        //                 logoImage,
        //                 companyNameAr,
        //                 companyAddressAr,
        //                 _tenantDbContextHelper
        //             );
        //             break;
        //         case "subGroup":
        //             Report = new subGroup(
        //                 accountGroup, // Use accountGroup, not accountId
        //                 frmDate.Value,
        //                 toDate.Value,
        //                 tenantName,
        //                 companyName,
        //                 companyAddress,
        //                 logoImage,
        //                 companyNameAr,
        //                 companyAddressAr,
        //                 _tenantDbContextHelper
        //             );
        //             break;
        //         default:
        //             return NotFound("Trial Balance Report not found.");
        // }
        //}
    }
}
