using DevExpress.XtraReports.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Finance.Areas.Finance.Reports;
using QD.ERP.Finance.Areas.Finance.Reports.ImportReports.PayableandReceivable;
using QD.ERP.Finance.Areas.Finance.Reports.AccountRegister;
using QD.ERP.Finance.Areas.Finance.Reports.BillsReceivable;
using QD.ERP.Finance.Areas.Finance.Reports.Cost_Analysis;
using QD.ERP.Finance.Areas.Finance.Reports.Cost_Analysis.Detailed_Report;
using QD.ERP.Finance.Areas.Finance.Reports.Cost_Analysis.summary_Report;
using QD.ERP.Finance.Areas.Finance.Reports.ImportReports;
using QD.ERP.Finance.Areas.Finance.Reports.ImportReports.PayableandReceivable;
//using QD.ERP.VAT.Areas.VAT.Reports.InventoryReports;
using QD.ERP.VAT.Areas.VAT.Reports.PurchaseRegister;
using QD.ERP.VAT.Areas.VAT.Reports.VAT_Sales_Invoice_Register;
using QD.ERP.VAT.Areas.VAT.Reports.VATCreditNote;
using QD.ERP.VAT.Areas.VAT.Reports.VATDebitNote;
using QD.ERP.VAT.Areas.VAT.Reports.VATReturns;
using QD.ERP.Shared.DAL.Entities;
using QD.ERP.Shared.Models.DAL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using ERPMasterWtDataContext = QD.ERP.Shared.DAL.Entities.ERPMasterWtDataContext;

namespace QD.ERP.Web.Pages
{
	public class RegisterViewerModel : PageModel
	{
		private readonly TenantDbContextHelper _tenantDbContextHelper;
		private ERPMasterWtDataContext _eRPMasterWtDataContext;

		public RegisterViewerModel(TenantDbContextHelper tenantDbContextHelper)
		{
			_tenantDbContextHelper = tenantDbContextHelper;
		}


		public XtraReport Report { get; private set; }
        public string ReportName { get; private set; }
        public string VoucherType { get; private set; }
        public DateTime FrmDate { get; private set; }
        public DateTime ToDate { get; private set; }
        public string requestedBy { get; private set; }
        public List<string> SelectedValues { get; private set; } = new List<string>();
        public bool UseEffectiveDate { get; private set; }
        public IActionResult OnGet(string reportName, string voucherType, DateTime? frmDate, DateTime? toDate, string requestedBy, string[] selectedValues, bool? useEffectiveDate, string selectionType)
        {
            if (string.IsNullOrEmpty(reportName))
            {
                return BadRequest("Invalid report name.");
            }
			if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
			{
				return StatusCode(500, "Tenant not found or DbContext could not be created.");
			}

			_eRPMasterWtDataContext = dbContext;

			ReportName = reportName;

            var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
            var userName = HttpContext.Session.GetString("UserName") ?? "Default User";
            // Get the DefaultcompanyID from session
            var defaultCompanyIdString = HttpContext.Session.GetString("DefaultcompanyID");

            // Parse it to int (you may want to use long or Guid if that's your actual ID type)
            if (!int.TryParse(defaultCompanyIdString, out int defaultCompanyId))
            {
                // Handle invalid or missing ID (fallback or error handling)
                defaultCompanyId = 0; // or return early / throw error
            }

            // Now fetch the company details using DefaultcompanyID
            var ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                .FirstOrDefault(x => x.CompanyId == defaultCompanyId);
            var DefaultCurrencyDecimals = ERPCompany_details?.DefaultCurrencyDecimals ?? 0;
            var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
            var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
            var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
            var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

            Image logoImage = null;
            if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
            {
                try
                {
                    using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                    {
                        logoImage = Image.FromStream(ms);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error processing company logo: " + ex.Message);
                }
            }

            //Account Register Reports
            if (!string.IsNullOrEmpty(voucherType) && frmDate.HasValue && toDate.HasValue)
            {
                VoucherType = voucherType;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;

                switch (reportName)
                {
                    case "PreviewRegister":
                        Report = new PreviewRegister(VoucherType, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName);
                        break;
                    case "OrderByVchNoRegister":
                        Report = new OrderByVchNoRegister(VoucherType, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName);
                        break;
                    case "OrderbyVchNoWIthVchNarration":
                        Report = new OrderbyVchNoWIthVchNarration(VoucherType, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName);
                        break;
                    case "Register4line":
                        Report = new Register4line(VoucherType, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName);
                        break;
                    case "RegisterLineEntryNarration":
                        Report = new RegisterLineEntryNarration(VoucherType, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName);
                        break;
                    case "RegisterWithVchNarration":
                        Report = new RegisterWithVchNarration(VoucherType, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName);
                        break;
                    default:
                        return NotFound("Report not found.");
                }
            }


            ////Cost Analysis Reports

            else if (frmDate.HasValue && toDate.HasValue)
            {
              
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;

                switch (reportName)
                {
                    
                    case "SummaryReport":
                        // **Handle requestedBy being empty or null**
                        string summaryReportRequestedBy = string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy;
                        Report = new CostCenterSummaryReport(summaryReportRequestedBy, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName);
                        break;
                    case "SummaryReportByDate":
                        Report = new SummaryReport_ByDate_(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName, useEffectiveDate
                        );
                        break;

                    case "CostCenterReport":
                        Report = new CostcenterRepoer(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName
                        );
                        break;

                    case "CostCenterReportByDate":
                        Report = new CostcenterBydate(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName, useEffectiveDate
                        );
                        break;

                    case "CostCenterGroupReport":
                        Report = new CostCenterGroupReport(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName
                        );
                        break;

                    case "CostCenterGroupReportByDate":
                        Report = new CostcenterGroupByDate(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName, useEffectiveDate
                        );
                        break;



                    case "DetailReport":
                        Report = new DetailReport(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName
                        );
                        break;
                    case "DetailedBydate":
                        Report = new DetailedBydate(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName
                        );
                        break;
                    case "DetailedGroup":
                        Report = new DetailedGroup(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName
                        );
                        break;
                    case "detailGroupBydate":
                        Report = new detailGroupBydate(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName
                        );
                        break;
                    case "CostCenterMasterGroup":
                        Report = new CostCenterMasterGroup(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName
                        );
                        break;

                    case "detailedMasterByDate":
                        Report = new detailedMasterByDate(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName
                        );
                        break;


                    //////VAT REports
                    case "TaxSummaryReport":
                        Report = new TaxSummaryReport(FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper, DefaultCurrencyDecimals);
                        break;
                    case "TaxVATReport":
                        Report = new TaxVATReport(FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper);
                        break;
                    case "TaxReportRevenueInArabic":
                        Report = new TaxReportRevenueInArabic(FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper, DefaultCurrencyDecimals);
                        break;
                    case "CreditSummary":
                        Report = new CreditSummary(FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper, DefaultCurrencyDecimals);
                        break;
                    case "DebitNoteSummary":
                        Report = new DebitNoteSummary(FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper, DefaultCurrencyDecimals);
                        break;
                    case "VATPurchasesAndExpReport":
                        Report = new VATPurchasesAndExpReport(FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper, DefaultCurrencyDecimals);
                        break;
                    case "TaxSummaryReportPurchaseInArabic":
                        Report = new TaxSummaryReportPurchaseInArabic(FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper);
                        break;
                    case "VATReturnsform":
                        Report = new VATReturnsform(FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper, DefaultCurrencyDecimals);
                        break;



                    //Import Reports 

                    case "CostcenterBydateImport":
                        Report = new CostcenterBydateImport(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName, useEffectiveDate
                        );
                        break;
                    case "CostcenterGroupByDateImport":
                        Report = new CostcenterGroupByDateImport(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName, useEffectiveDate
                        );
                        break;
                    case "CostCenterGroupReportImport":
                        Report = new CostCenterGroupReportImport(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName
                        );
                        break;
                    case "CostCenterMasterGroupImport":
                        Report = new CostCenterMasterGroupImport(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName
                        );
                        break;
                    case "CostcenterRepoerImport":
                        Report = new CostcenterRepoerImport(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName
                        );
                        break;
                    case "CostCenterSummaryReportImport":
                        // **Handle requestedBy being empty or null**
                        string summaryImportRequestedBy = string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy;
                        Report = new CostCenterSummaryReportImport(summaryImportRequestedBy, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName);
                        break;
                    case "DetailedBydateImport":
                        Report = new DetailedBydateImport(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName
                        );
                        break;
                    case "DetailedGroupImport":
                        Report = new DetailedGroupImport(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName
                        );
                        break;
                    case "detailedMasterByDateImport":
                        Report = new detailedMasterByDateImport(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName
                        );
                        break;
                    case "detailGroupBydateImport":
                        Report = new detailGroupBydateImport(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName
                        );
                        break;
                    case "DetailReportImport":
                        Report = new DetailReportImport(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName
                        );
                        break;
                    case "SummaryReportByDateImport":
                        Report = new SummaryReportByDateImport(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName, useEffectiveDate
                        );
                        break;

                    default:
                        return NotFound("cost analysis report not found.");

                }
            }

       
            ///Bills Receivable reports
            // **CASE 3: Reports using selectedValues**
            else if (selectedValues != null && selectedValues.Length > 0)
            {
                SelectedValues = selectedValues.ToList();

                switch (reportName)
                {
                    case "XtraRecivableReport":
                        Report = new XtraRecivableReport(userName, SelectedValues.ToArray(),  selectionType,tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                        break;
                    case "XtraRecivableReportImport":
                        Report = new XtraRecivableReportImport(userName, SelectedValues.ToArray(),  selectionType,tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                        break;
                    case "XtraRecivableReport2":
                        Report = new XtraRecivableReport2(userName, SelectedValues.ToArray(),  selectionType,tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                        break;
                    case "XtraRecivableReport2Import":
                    Report = new XtraRecivableReport2Import(userName, SelectedValues.ToArray(),  selectionType,tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                    break;
                    default:
                        return NotFound("Report not found.");
                }

                // **Pass SelectedValues to the Report**
                if (Report != null && SelectedValues.Count > 0)
                {
                    Report.Parameters["SelectedValues"].Value = string.Join(",", SelectedValues);
                    Report.Parameters["SelectedValues"].Visible = false;
                }
            }
            // **CASE 4: Reports that require only Tenant & Company details (no parameters)**
            else if (!string.IsNullOrEmpty(reportName))
            {
                switch (reportName)
                {
                    // Regular Reports
                    case "XtraReportAgeingreportsummary":
                        Report = new XtraReportAgeingreportsummary(tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName);
                        break;
                    case "XtraReportBillsReceivableAgeingReport":
                        Report = new XtraReportBillsReceivableAgeingReport(tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName);
                        break;
                    case "BIllsPayable":
                        Report = new BIllsPayable(tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName);
                        break;
                    case "Summary":
                        Report = new Summary(userName,tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                        break;
                    case "BillsReceivableAgeingReport":
                        Report = new BillsReceivableAgeingReport(tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName);
                        break;
                    case "ReceivableReport(EffectiveDate)":
                        Report = new ReceivableReport_EffectiveDate_(tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName);
                        break;
                    case "BillsRecivableReport1":
                        Report = new BillsRecivableReport1(tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName);
                        break;
                    //case "InventoryReportWithExpireDates":
                    //    Report = new InventoryReportWithExpireDates(tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper);
                    //    break;

                    // Import Reports (Offline versions)
                    case "XtraReportAgeingreportsummaryImport":
                        Report = new XtraReportAgeingreportsummaryImport(tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName);
                        break;
                    case "XtraReportBillsReceivableAgeingReportImport":
                        Report = new XtraReportBillsReceivableAgeingReportImport(tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName);
                        break;
                    case "BIllsPayableImport":
                        Report = new BIllsPayableImport(tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName);
                        break;
                    case "SummaryImport":
                        Report = new SummaryImport(userName,tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                        break;
                    case "BillsReceivableAgeingReportImport":
                        Report = new BillsReceivableAgeingReportImport(tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName);
                        break;
                    case "ReceivableReportEffectiveDateImport":
                        Report = new ReceivableReportEffectiveDateImport(tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName);
                        break;
                    case "BillsRecivableReport1Import":
                        Report = new BillsRecivableReport1Import(tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName);
                        break;

                    //case "InventoryReportWithExpireDatesImport":
                    //    Report = new InventoryReportWithExpireDatesImport(tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr,  userName,_tenantDbContextHelper);
                    //    break;

                    default:
                        return NotFound("Report not found.");
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
