using DevExpress.XtraReports.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QD.ERP.Web.Areas.Finance.Reports.cashPayments;
using QD.ERP.Web.Areas.Finance.Reports.test;
using QD.ERP.Web.Areas.Finance.Reports;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Models.DAL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using QD.ERP.Web.Areas.Finance.Reports.ExpensesClaims;
using ERPMasterWtDataContext = QD.ERP.Web.DAL.Entities.ERPMasterWtDataContext;


namespace QD.ERP.Web.Pages
{
    public class VoucherViewerModel : PageModel
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private ERPMasterWtDataContext _eRPMasterWtDataContext;

        public VoucherViewerModel(TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
        }

        public XtraReport Report { get; private set; }
        public string VoucherNo { get; private set; }
        public string ReportName { get; private set; }

        public IActionResult OnGet(string reportName, string voucherNo)
        {
            if (string.IsNullOrEmpty(reportName) || string.IsNullOrEmpty(voucherNo))
            {
                return BadRequest("Invalid report name or voucher number.");
            }

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
            {
                return StatusCode(500, "Tenant not found or DbContext could not be created.");
            }

            _eRPMasterWtDataContext = dbContext;

            var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
            var ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                .FirstOrDefault(x => x.CompanyNameShort == tenantName);

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

            VoucherNo = voucherNo;

            switch (reportName)
            {
                case "cashPaymentformat2":
                    Report = new cashPaymentformat2(VoucherNo, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                    break;
                case "cashPayments":
                    Report = new cashPayments(VoucherNo, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                    break;
                case "PreviewClaimRequestForm":
                    Report = new PreviewClaimRequestForm(VoucherNo, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                    break;
                case "ClaimDetailed":
                    Report = new ClaimDetailed(VoucherNo, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                    break;
                case "ClaimEntryCheck":
                    Report = new ClaimEntryCheck(VoucherNo, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr);
                    break;
                case "PreviewClaimRequestForm_wtVAT_":
                    Report = new PreviewClaimRequestForm_wtVAT_(VoucherNo, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                    break;
                case "PaymentsAdviceSupplierPayments":
                    Report = new PaymentsAdviceSupplierPayments(VoucherNo, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr);
                    break;
                default:
                    return NotFound("Report not found.");
            }

            return Page();
        }
    }
}
