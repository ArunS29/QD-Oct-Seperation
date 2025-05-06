using DevExpress.XtraReports.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QD.ERP.Web.Areas.Finance.Reports.cashPayments;
using QD.ERP.Web.Areas.Finance.Reports.test;
using QD.ERP.Web.Areas.Finance.Reports;
using QD.ERP.Web.Areas.VAT.Reports.VAT_Sales_Invoice_Register;
using QD.ERP.Web.Areas.VAT.Reports.B2B_INVOICE;
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

        public IActionResult OnGet(string reportName, string voucherNo, string invoiceNo, bool isApproved)
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

            var userName = HttpContext.Session.GetString("UserName") ?? "Default User";

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

            // 👉 New CASE 2: If it is Invoice-related
            if (reportName == "TAXINVOICEWTDOCUMENTALLEVELDISCOUNTSS" || reportName == "RegulartaxinvoicewithoutSignatoriesFormat05" || reportName == "SimplifiedTaxInvoice" || reportName == "PrintRegularInvoiceFormat02"|| reportName == "ForeignCurrency")
            {
                if (string.IsNullOrEmpty(invoiceNo))
                {
                    return BadRequest("Invoice No is required for invoice reports.");
                }

                invoiceNo = invoiceNo; 

                if (reportName == "TAXINVOICEWTDOCUMENTALLEVELDISCOUNTSS")
                {
                    // Instantiate the report dynamically
                    Report = new QD.ERP.Web.Areas.VAT.Reports.B2B_INVOICE.TAXINVOICEWTDOCUMENTALLEVELDISCOUNTSS(
                        invoiceNo, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, isApproved, _tenantDbContextHelper);
                }
                else if (reportName == "PrintRegularInvoiceFormat02")
                {
                    Report = new QD.ERP.Web.Areas.VAT.Reports.B2B_INVOICE.PrintRegularInvoiceFormat02(invoiceNo, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, isApproved,_tenantDbContextHelper);
                }
                else if (reportName == "ForeignCurrency")
                {
                    Report = new QD.ERP.Web.Areas.VAT.Reports.B2B_INVOICE.ForeignCurrency(invoiceNo, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, isApproved, _tenantDbContextHelper);
                }
                else if (reportName == "RegulartaxinvoicewithoutSignatoriesFormat05")
                {
                    Report = new QD.ERP.Web.Areas.VAT.Reports.B2B_INVOICE.RegulartaxinvoicewithoutSignatoriesFormat05(invoiceNo, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, isApproved, _tenantDbContextHelper);
                }

                return Page();
            }

            // 👉 Existing CASE 1: Old voucher reports
            if (string.IsNullOrEmpty(voucherNo))
            {
                return BadRequest("Voucher number is required.");
            }

            VoucherNo = voucherNo;

            switch (reportName)
            {
                case "cashPaymentformat2":
                    Report = new cashPaymentformat2(VoucherNo, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, userName,_tenantDbContextHelper);
                    break;
				case "SalesVoucherReport":
					Report = new SalesVoucherReport(VoucherNo, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper);
					break;
				case "cashPayments":
                    Report = new cashPayments(VoucherNo, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper);
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
