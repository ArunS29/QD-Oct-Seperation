using DevExpress.XtraReports.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using QD.ERP.Finance.Areas.Finance.Reports;
using QD.ERP.Finance.Areas.Finance.Reports.AccountRegister;
using QD.ERP.Shared.Models.DAL;
using QD.ERP.Shared.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using ERPMasterWtDataContext = QD.ERP.Shared.DAL.Entities.ERPMasterWtDataContext;
using QD.ERP.Finance.Areas.Finance.Reports.Cost_Analysis;
using QD.ERP.Finance.Areas.Finance.Reports.Cost_Analysis.summary_Report;
using QD.ERP.Finance.Areas.Finance.Reports.Cost_Analysis.Detailed_Report;
//using QD.ERP.VAT.Areas.VAT.Reports.VAT_Sales_Invoice_Register;
//using QD.ERP.VAT.Areas.VAT.Reports.PurchaseRegister;
//using QD.ERP.VAT.Areas.VAT.Reports.VATCreditNote;
//using QD.ERP.VAT.Areas.VAT.Reports.VATDebitNote;
using QD.ERP.Finance.Areas.Finance.Reports.ImportReports;
//using QD.ERP.VAT.Areas.VAT.Reports.VATReturns;

namespace QD.ERP.Web.Pages
{
    public class RegisterDesignerModel : PageModel
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private ERPMasterWtDataContext _eRPMasterWtDataContext;

        public XtraReport Report { get; private set; }
        public string ReportName { get; private set; }

        public RegisterDesignerModel(TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
        }

        public IActionResult OnGet(string reportName, string voucherType, DateTime? frmDate, DateTime? toDate, string requestedBy,bool useEffectiveDate)
        {

            if (string.IsNullOrEmpty(reportName))
                return BadRequest("Report name is required.");

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                return StatusCode(500, "Tenant not found or DbContext could not be created.");

            _eRPMasterWtDataContext = dbContext;
            ReportName = reportName;

            // Get Tenant Name from Session
            var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
            var userName = HttpContext.Session.GetString("UserName") ?? "Default User";
            var defaultCompanyIdString = HttpContext.Session.GetString("DefaultcompanyID");

            // Parse it to int (you may want to use long or Guid if that's your actual ID type)
            if (!int.TryParse(defaultCompanyIdString, out int defaultCompanyId))
            {
                // Handle invalid or missing ID (fallback or error handling)
                defaultCompanyId = 0; // or return early / throw error
            }
            // Company Info
            var companyDetails = _eRPMasterWtDataContext.Tbl901CompanyDetails
                .FirstOrDefault(x => x.CompanyId == defaultCompanyId);
            var DefaultCurrencyDecimals = companyDetails?.DefaultCurrencyDecimals ?? 0;
            string companyName = companyDetails?.CompanyName ?? string.Empty;
            string companyAddress = companyDetails?.CompanyFullAddress ?? string.Empty;
            string companyNameAr = companyDetails?.CompanyNameAr ?? string.Empty;
            string companyAddressAr = companyDetails?.CompanyFullAddressAr ?? string.Empty;

            Image logoImage = null;
            if (companyDetails?.CompanyLogo != null && companyDetails.CompanyLogo.Length > 0)
            {
                try
                {
                    using MemoryStream ms = new(companyDetails.CompanyLogo);
                    logoImage = Image.FromStream(ms);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to load logo image: " + ex.Message);
                }
            }

            // Instantiate reports
            if (!string.IsNullOrEmpty(voucherType) && frmDate.HasValue && toDate.HasValue)
            {
               
                switch (reportName)
                {

                 

                    case "PreviewRegister":
                        Report = new PreviewRegister(voucherType, frmDate.Value, toDate.Value,
                            tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName);
                        break;
                    case "OrderByVchNoRegister":
                        Report = new OrderByVchNoRegister(voucherType, frmDate.Value, toDate.Value,
                            tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, 
                            _tenantDbContextHelper, userName);
                        break;
                    case "OrderbyVchNoWIthVchNarration":
                        Report = new OrderbyVchNoWIthVchNarration(voucherType, frmDate.Value, toDate.Value,
                            tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName);
                        break;
                    case "Register4line":
                        Report = new Register4line(voucherType, frmDate.Value, toDate.Value,
                            tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName);
                        break;
                    case "RegisterLineEntryNarration":
                        Report = new RegisterLineEntryNarration(voucherType, frmDate.Value, toDate.Value,
                            tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName);
                        break;
                    case "RegisterWithVchNarration":
                        Report = new RegisterWithVchNarration(voucherType, frmDate.Value, toDate.Value,
                            tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName);
                        break;
                    default:
                        return NotFound("Report not found.");
                }
            }
            else if (frmDate.HasValue && toDate.HasValue)
            {
                frmDate = frmDate.Value;
                toDate = toDate.Value;

                switch (reportName)
                {
                    case "SummaryReport":
                        // **Handle requestedBy being empty or null**
                        string requestedByValue = string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy;
                        Report = new CostCenterSummaryReport(requestedByValue, frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper,userName);
                        break;
                    case "SummaryReportByDate":
                        Report = new SummaryReport_ByDate_(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName, useEffectiveDate
                        );
                        break;

                    case "CostCenterReport":
                        Report = new CostcenterRepoer(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName
                        );
                        break;

                    case "CostCenterReportByDate":
                       Report = new CostcenterBydate(
                          string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                          frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName, useEffectiveDate
                       );
                        break;

                    case "CostCenterGroupReport":
                        Report = new CostCenterGroupReport(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName
                        );
                        break;

                    case "CostCenterGroupReportByDate":
                        Report = new CostcenterGroupByDate(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName, useEffectiveDate
                        );
                        break;



                    case "DetailReport":
                        Report = new DetailReport(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName
                        );
                        break;
                    case "DetailedBydate":
                        Report = new DetailedBydate(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName
                        );
                        break;
                    case "DetailedGroup":
                        Report = new DetailedGroup(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName
                        );
                        break;
                    case "detailGroupBydate":
                        Report = new detailGroupBydate(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName
                        );
                        break;
                    case "CostCenterMasterGroup":
                        Report = new CostCenterMasterGroup(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName
                        );
                        break;

                    case "detailedMasterByDate":
                        Report = new detailedMasterByDate(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName
                        );
                        break;

             
                    //case "TaxSummaryReport":
                    //    Report = new TaxSummaryReport(frmDate.Value,toDate.Value,tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper, DefaultCurrencyDecimals);
                    //    break;

                    //case "TaxVATReport":
                    //    Report = new TaxVATReport(frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper);
                    //    break;
                    //case "TaxReportRevenueInArabic":
                    //    Report = new TaxReportRevenueInArabic(frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper, DefaultCurrencyDecimals);
                    //    break;
                    //case "CreditSummary":
                    //    Report = new CreditSummary(frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper, DefaultCurrencyDecimals);
                    //    break;
                    //case "DebitNoteSummary":
                    //    Report = new DebitNoteSummary(frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper, DefaultCurrencyDecimals);
                    //    break;
                    //case "VATPurchasesAndExpReport":
                    //    Report = new VATPurchasesAndExpReport(frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper, DefaultCurrencyDecimals);
                    //    break;
                    //case "TaxSummaryReportPurchaseInArabic":
                    //    Report = new TaxSummaryReportPurchaseInArabic(frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper);
                    //    break;
                    //case "VATReturnsform":
                    //    Report = new VATReturnsform(frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper, DefaultCurrencyDecimals);
                    //    break;

                    //import reports

                    case "CostcenterBydateImport":
                        Report = new CostcenterBydateImport(
                           string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                           frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName, useEffectiveDate
                        );
                        break;
                    case "CostcenterGroupByDateImport":
                        Report = new CostcenterGroupByDateImport(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                             frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName, useEffectiveDate
                        );
                        break;
                    case "CostCenterGroupReportImport":
                        Report = new CostCenterGroupReportImport(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName
                        );
                        break;
                    case "CostCenterMasterGroupImport":
                        Report = new CostCenterMasterGroupImport(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName
                        );
                        break;
                    case "CostcenterRepoerImport":
                        Report = new CostcenterRepoerImport(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName
                        );
                        break;
                    case "CostCenterSummaryReportImport":
                        // **Handle requestedBy being empty or null**
                        string summaryImportRequestedBy = string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy;
                        Report = new CostCenterSummaryReportImport(summaryImportRequestedBy, frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName);
                        break;
                    case "DetailedBydateImport":
                        Report = new DetailedBydateImport(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName
                        );
                        break;
                    case "DetailedGroupImport":
                        Report = new DetailedGroupImport(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName
                        );
                        break;
                    case "detailedMasterByDateImport":
                        Report = new detailedMasterByDateImport(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                             frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName
                        );
                        break;
                    case "detailGroupBydateImport":
                        Report = new detailGroupBydateImport(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName
                        );
                        break;
                    case "DetailReportImport":
                        Report = new DetailReportImport(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName
                        );
                        break;
                    case "SummaryReportByDateImport":
                        Report = new SummaryReportByDateImport(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName, useEffectiveDate
                        );
                        break;
                    default:
                        return NotFound("Cost Analysis Report not found.");
                }
            }

            else
            {
                return BadRequest("Missing required parameters.");
            }

            return Page();
        }
    }
}
