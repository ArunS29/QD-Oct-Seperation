using DevExpress.XtraReports.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using QD.ERP.Web.Areas.Finance.Reports;
using QD.ERP.Web.Areas.Finance.Reports.AccountRegister;
using QD.ERP.Web.Models.DAL;
using QD.ERP.Web.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using ERPMasterWtDataContext = QD.ERP.Web.DAL.Entities.ERPMasterWtDataContext;
using QD.ERP.Web.Areas.Finance.Reports.Cost_Analysis;
using QD.ERP.Web.Areas.Finance.Reports.Cost_Analysis.summary_Report;
using QD.ERP.Web.Areas.Finance.Reports.Cost_Analysis.Detailed_Report;

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

        public IActionResult OnGet(string reportName, string voucherType, DateTime? frmDate, DateTime? toDate,string requestedBy)
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
            // Company Info
            var companyDetails = _eRPMasterWtDataContext.Tbl901CompanyDetails
                .FirstOrDefault(x => x.CompanyNameShort == tenantName);

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
                        Report = new CostCenterSummaryReport(requestedByValue, frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                        break;
                    case "SummaryReportByDate":
                        Report = new SummaryReport_ByDate_(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper
                        );
                        break;

                    case "CostCenterReport":
                        Report = new CostcenterRepoer(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper
                        );
                        break;

                    case "CostCenterReportByDate":
                        Report = new CostcenterBydate(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper
                        );
                        break;

                    case "CostCenterGroupReport":
                        Report = new CostCenterGroupReport(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper
                        );
                        break;

                    case "CostCenterGroupReportByDate":
                        Report = new CostcenterGroupByDate(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper
                        );
                        break;



                    case "DetailReport":
                        Report = new DetailReport(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper
                        );
                        break;
                    case "DetailedBydate":
                        Report = new DetailedBydate(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper
                        );
                        break;
                    case "DetailedGroup":
                        Report = new DetailedGroup(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper
                        );
                        break;
                    case "detailGroupBydate":
                        Report = new detailGroupBydate(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper
                        );
                        break;
                    case "CostCenterMasterGroup":
                        Report = new CostCenterMasterGroup(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper
                        );
                        break;

                    case "detailedMasterByDate":
                        Report = new detailedMasterByDate(
                            string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy,
                            frmDate.Value, toDate.Value, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper
                        );
                        break;
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
