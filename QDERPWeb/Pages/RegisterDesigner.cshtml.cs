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

        public IActionResult OnGet(string reportName, string voucherType, DateTime? frmDate, DateTime? toDate)
        {
            if (string.IsNullOrEmpty(reportName))
                return BadRequest("Report name is required.");

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                return StatusCode(500, "Tenant not found or DbContext could not be created.");

            _eRPMasterWtDataContext = dbContext;
            ReportName = reportName;

            // Get Tenant Name from Session
            var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";

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
                            tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                        break;
                    case "OrderByVchNoRegister":
                        Report = new OrderByVchNoRegister(voucherType, frmDate.Value, toDate.Value,
                            tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                        break;
                    case "OrderbyVchNoWIthVchNarration":
                        Report = new OrderbyVchNoWIthVchNarration(voucherType, frmDate.Value, toDate.Value,
                            tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                        break;
                    case "Register4line":
                        Report = new Register4line(voucherType, frmDate.Value, toDate.Value,
                            tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                        break;
                    case "RegisterLineEntryNarration":
                        Report = new RegisterLineEntryNarration(voucherType, frmDate.Value, toDate.Value,
                            tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                        break;
                    case "RegisterWithVchNarration":
                        Report = new RegisterWithVchNarration(voucherType, frmDate.Value, toDate.Value,
                            tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper);
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
