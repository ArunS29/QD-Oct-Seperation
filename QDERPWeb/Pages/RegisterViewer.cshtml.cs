using DevExpress.XtraReports.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QD.ERP.Web.Areas.Finance.Reports;
using QD.ERP.Web.Areas.Finance.Reports.AccountRegister;
using QD.ERP.Web.Areas.Finance.Reports.BillsReceivable;
using QD.ERP.Web.Areas.Finance.Reports.Cost_Analysis;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Models.DAL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace QD.ERP.Web.Pages
{
    public class RegisterViewerModel : PageModel
    {
        private readonly DAL.Entities.ERPMasterWtDataContext _eRPMasterWtDataContext;


        public Tbl901CompanyDetail ERPCompany_details;
        public RegisterViewerModel(DAL.Entities.ERPMasterWtDataContext eRPMasterWtDataContext)
        {
            _eRPMasterWtDataContext = eRPMasterWtDataContext;

        }

        public XtraReport Report { get; private set; }
        public string ReportName { get; private set; }
        public string VoucherType { get; private set; }
        public DateTime FrmDate { get; private set; }
        public DateTime ToDate { get; private set; }
        public List<string> SelectedValues { get; private set; } = new List<string>();

        public IActionResult OnGet(string reportName, string voucherType, DateTime? frmDate, DateTime? toDate, string[] selectedValues)
        {
            if (string.IsNullOrEmpty(reportName))
            {
                return BadRequest("Invalid report name.");
            }

            ReportName = reportName;

            // **Fetch Tenant & Company Details**
            var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
            var ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                .FirstOrDefault(x => x.CompanyNameShort == tenantName);

            // **Set default values if company details are not found**
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

            // **CASE 1: Reports using voucherType, frmDate, and toDate**
            if (!string.IsNullOrEmpty(voucherType) && frmDate.HasValue && toDate.HasValue)
            {
                VoucherType = voucherType;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;

                switch (reportName)
                {
                    case "PreviewRegister":
                        Report = new PreviewRegister(VoucherType, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr);
                        break;
                    case "OrderByVchNoRegister":
                        Report = new OrderByVchNoRegister(VoucherType, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr);
                        break;
                    case "OrderbyVchNoWIthVchNarration":
                        Report = new OrderbyVchNoWIthVchNarration(VoucherType, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr);
                        break;
                    case "Register4line":
                        Report = new Register4line(VoucherType, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr);
                        break;
                    case "RegisterLineEntryNarration":
                        Report = new RegisterLineEntryNarration(VoucherType, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr);
                        break;
                    case "RegisterWithVchNarration":
                        Report = new RegisterWithVchNarration(VoucherType, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr);
                        break;
                    default:
                        return NotFound("Report not found.");
                }
            }
            // **CASE 2: Reports using selectedValues**
            else if (selectedValues != null && selectedValues.Length > 0)
            {
                SelectedValues = selectedValues.ToList();

                switch (reportName)
                {
                    case "XtraRecivableReport":
                        Report = new XtraRecivableReport(SelectedValues.ToArray(), tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr);
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
            // **CASE 3: Reports that require only Tenant & Company details (no parameters)**
            else if (!string.IsNullOrEmpty(reportName))
            {
                switch (reportName)
                {
                    case "XtraReportAgeingreportsummary":
                        Report = new XtraReportAgeingreportsummary(tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr);
                        break;
                    case "XtraReportBillsReceivableAgeingReport":
                        Report = new XtraReportBillsReceivableAgeingReport(tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr);
                        break;
                    case "BIllsPayable":
                        Report = new BIllsPayable(tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr);
                        break;
                    case "AgeingReport":
                        Report = new AgeingReport(tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr);
                        break;
					case "ReceivableReport(EffectiveDate)":
						Report = new ReceivableReport_EffectiveDate_(tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr);
						break;
                    case "SummaryReport":
                        Report = new CostCenterSummaryReport();
                        break;
                    case "SummaryReportByDate":
                        Report = new SummaryReport_ByDate_();
                        break;
                    case "CostCenterReport":
                        Report = new CostcenterRepoer();
                        break;
              

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
