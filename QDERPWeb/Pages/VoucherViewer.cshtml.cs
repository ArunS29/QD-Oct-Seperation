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
using QD.ERP.Web.Areas.VAT.Reports.VATCreditNote;
using QD.ERP.Web.Areas.Finance.Reports.journalEntry;
using QD.ERP.Web.Areas.Finance.Reports.Journal_Register;
using QD.ERP.Web.Areas.VAT.Reports.Inventory_Reports;

using QD.ERP.Web.Areas.IMS.Reports.InventoryReports;

using QD.ERP.Web.Areas.IMS.Inventory_Reports;
using QD.ERP.Web.Areas.IMS.Reports.quotationstoClients;
using QD.ERP.Web.Areas.IMS.InventoryReports.MaterialPurchaseRequistion;
using QD.ERP.Web.Areas.IMS.Report.Inventory_Report;


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

        public IActionResult OnGet(string reportName, string voucherNo, string invoiceNo, bool isApproved, string debitNoteNo, string CreditNoteNo,string RequestNo,string quotationNo,string salesOrderNo)
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
            var companyPhone = ERPCompany_details?.CompanyPhone ?? string.Empty;
            var website = ERPCompany_details?.Website ?? string.Empty;
            var emailAddress = ERPCompany_details?.EmailAddress ?? string.Empty;

            Image logoImage = null;
            Image companySealImage = null;

            if ((ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0) ||
                (ERPCompany_details?.CompanySeal != null && ERPCompany_details.CompanySeal.Length > 0))
            {
                try
                {
                    // Convert Logo
                    if (ERPCompany_details.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }

                    // Convert Seal
                    if (ERPCompany_details.CompanySeal != null && ERPCompany_details.CompanySeal.Length > 0)
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanySeal))
                        {
                            companySealImage = Image.FromStream(ms);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error processing company logo or seal: " + ex.Message);
                }
            }

            if (reportName == "DebitNoteView")
            {
                if (string.IsNullOrEmpty(debitNoteNo))
                {
                    return BadRequest("Debit Note No is required for debit note reports.");
                }
                debitNoteNo = debitNoteNo;

                Report = new QD.ERP.Web.Areas.VAT.Reports.VATDebitNote.DebitNoteView(
                    debitNoteNo, tenantName, companyName, companyAddress, companyNameAr, companyAddressAr, isApproved, _tenantDbContextHelper);

                return Page();
            }
            if (string.IsNullOrEmpty(reportName))
                return BadRequest("Report name is required.");

            if (
      reportName == "TAXINVOICEWTDOCUMENTALLEVELDISCOUNTSS" ||
      reportName == "PrintsimplifiedTaxInvoices" ||
      reportName == "RegulartaxinvoicewithoutSignatoriesFormat05" ||
      reportName == "Foreigncurrency1" ||
      reportName == "PrintRegularInvoiceFormat02" ||
      reportName == "ForeignCurrencyProforma" ||
      reportName == "ForeignEnglishProforma" ||
      reportName == "ProformaInvoiceEnglish" ||
      reportName == "ProformaPreviewInvoice" ||
      reportName == "ProformaNewFormat" ||
      reportName == "BillsPurchases" ||
      reportName == "PrintRegularTaxInvoiceWtDocumentLevelDiscount" ||
      reportName == "Withoutsignatories" ||
      reportName == "withsignatories" ||
      reportName == "RegularTaxInvoiceFormat06" ||
      reportName == "PrintRegularTaxInvoiceWithSignatories_Format05_" ||
      reportName == "taxinvoicewithSignatoriesWithSymbols" ||
      reportName == "WithoutDiscountInvoice" ||
      reportName == "Wtdiscountpreviewinvoice")



            {
                if (string.IsNullOrEmpty(invoiceNo))
                {
                    return BadRequest("Invoice No is required for invoice reports.");
                }

                Report = reportName switch
                {
                    "TAXINVOICEWTDOCUMENTALLEVELDISCOUNTSS" =>
                        new QD.ERP.Web.Areas.VAT.Reports.B2B_INVOICE.TAXINVOICEWTDOCUMENTALLEVELDISCOUNTSS(


                            invoiceNo, tenantName, companyName, companyAddress, logoImage, companySealImage,

                            companyNameAr, companyAddressAr, isApproved, _tenantDbContextHelper),

                    "PrintsimplifiedTaxInvoices" => new QD.ERP.Web.Areas.VAT.Reports.B2B_INVOICE.PrintsimplifiedTaxInvoices(
                              invoiceNo, tenantName, companyName, companyAddress, logoImage,
                              companyNameAr, companyAddressAr, companyPhone, website, emailAddress,
                              isApproved, _tenantDbContextHelper),


                    "PrintRegularInvoiceFormat02" =>
                        new QD.ERP.Web.Areas.VAT.Reports.B2B_INVOICE.PrintRegularInvoiceFormat02(



                            invoiceNo, tenantName, companyName, companyAddress, logoImage, companySealImage,

                            companyNameAr, companyAddressAr, companyPhone, website, emailAddress,
                            isApproved, _tenantDbContextHelper),

                    "Foreigncurrency1" =>
                        new QD.ERP.Web.Areas.VAT.Reports.B2B.Foreigncurrency1(



                            invoiceNo, tenantName, companyName, companyAddress, logoImage, companySealImage,

                            companyNameAr, companyAddressAr, companyPhone, website, emailAddress,
                            isApproved, _tenantDbContextHelper),

                    "RegulartaxinvoicewithoutSignatoriesFormat05" =>
                        new QD.ERP.Web.Areas.VAT.Reports.B2B_INVOICE.RegulartaxinvoicewithoutSignatoriesFormat05(
                            invoiceNo, tenantName, companyName, companyAddress, logoImage,
                            companyNameAr, companyAddressAr, companyPhone, website, emailAddress,
                            isApproved, _tenantDbContextHelper),

                    "ForeignCurrencyProforma" =>
                        new QD.ERP.Web.Areas.VAT.Reports.ProformaInvoices.ForeignCurrencyProforma(
                            invoiceNo, tenantName, companyName, companyAddress, logoImage, companySealImage,
                            companyNameAr, companyAddressAr, companyPhone, website, emailAddress,
                            isApproved, _tenantDbContextHelper),

                    "ForeignEnglishProforma" =>
                        new QD.ERP.Web.Areas.VAT.Reports.ProformaInvoices.ForeignEnglishProforma(



                            invoiceNo, tenantName, companyName, companyAddress, logoImage, companySealImage,

                            companyNameAr, companyAddressAr, companyPhone, website, emailAddress,
                            isApproved, _tenantDbContextHelper),

                    "ProformaInvoiceEnglish" =>
                        new QD.ERP.Web.Areas.VAT.Reports.ProformaInvoices.ProformaInvoiceEnglish(



                            invoiceNo, tenantName, companyName, companyAddress, logoImage, 

                            companyNameAr, companyAddressAr, companyPhone, website, emailAddress,
                            isApproved, _tenantDbContextHelper),



                    "ProformaPreviewInvoice" =>
                        new QD.ERP.Web.Areas.VAT.Reports.ProformaInvoices.ProformaPreviewInvoice(



                            invoiceNo, tenantName, companyName, companyAddress, logoImage,

                            companyNameAr, companyAddressAr, companyPhone, website, emailAddress,
                            isApproved, _tenantDbContextHelper),

                    "ProformaNewFormat" =>
                        new QD.ERP.Web.Areas.VAT.Reports.ProformaInvoices.ProformaNewFormat(
                       invoiceNo, tenantName, companyName, companyAddress, logoImage,  companyNameAr,
                       companyAddressAr, isApproved, _tenantDbContextHelper),
                    "WithoutDiscountInvoice" =>
                     new QD.ERP.Web.Areas.VAT.Reports.ProformaInvoices.WithoutDiscountInvoice(
                         invoiceNo, tenantName, companyName, companyAddress, logoImage, companySealImage, companyNameAr,
                         companyAddressAr, isApproved, _tenantDbContextHelper),



                    "Wtdiscountpreviewinvoice" =>
               new QD.ERP.Web.Areas.VAT.Reports.ProformaInvoices.Wtdiscountpreviewinvoice(
                   invoiceNo, tenantName, companyName, companyAddress, logoImage, companySealImage, companyNameAr,
                   companyAddressAr, isApproved, _tenantDbContextHelper),





                    "PrintRegularTaxInvoiceWtDocumentLevelDiscount" =>
                             new QD.ERP.Web.Areas.VAT.Reports.B2B.PrintRegularTaxInvoiceWtDocumentLevelDiscount(
                                 invoiceNo, tenantName, companyName, companyAddress, logoImage, companySealImage,
                                 companyNameAr, companyAddressAr,
                                 isApproved, _tenantDbContextHelper),
                    "Withoutsignatories" =>
                    new QD.ERP.Web.Areas.VAT.Reports.B2B.Withoutsignatories(
                   invoiceNo, tenantName, companyName, companyAddress, logoImage, companySealImage,
                   companyNameAr, companyAddressAr,
                   isApproved, _tenantDbContextHelper),
                    "withsignatories" =>
                 new QD.ERP.Web.Areas.VAT.Reports.B2B.withsignatories2(
                invoiceNo, tenantName, companyName, companyAddress, logoImage, companySealImage,
                companyNameAr, companyAddressAr,
                isApproved, _tenantDbContextHelper),
                    "RegularTaxInvoiceFormat06" =>
                    new QD.ERP.Web.Areas.VAT.Reports.B2B.RegularTaxInvoiceFormat06(
                   invoiceNo, tenantName, companyName, companyAddress, logoImage, companySealImage,
                   companyNameAr, companyAddressAr,
                   isApproved, _tenantDbContextHelper),
                    "PrintRegularTaxInvoiceWithSignatories_Format05_" =>
                 new QD.ERP.Web.Areas.VAT.Reports.B2B.PrintRegularTaxInvoiceWithSignatories_Format05_(
                invoiceNo, tenantName, companyName, companyAddress, logoImage, companySealImage,
                companyNameAr, companyAddressAr,
                isApproved, _tenantDbContextHelper),
                    "taxinvoicewithSignatoriesWithSymbols" =>
                    new QD.ERP.Web.Areas.VAT.Reports.B2B.taxinvoicewithSignatoriesWithSymbols(
                   invoiceNo, tenantName, companyName, companyAddress, logoImage, companySealImage,
                   companyNameAr, companyAddressAr,
                   isApproved, _tenantDbContextHelper),

                    "BillsPurchases" =>
                     new QD.ERP.Web.Areas.VAT.Reports.PurchaseRegister.BillsPurchases(
                invoiceNo, tenantName, companyName, companyAddress, logoImage,
                companyNameAr, companyAddressAr, isApproved, _tenantDbContextHelper)





                };

                return Page();
            }








            if (reportName == "CreditForeignCurrency" || reportName == "creditnote")
            {
                if (string.IsNullOrEmpty(CreditNoteNo))
                {
                    return BadRequest("Invoice No is required for invoice reports.");
                }

                CreditNoteNo = CreditNoteNo;

                if (reportName == "CreditForeignCurrency")

                {
                    Report = new QD.ERP.Web.Areas.VAT.Reports.VATCreditNote.CreditForeignCurrency(
                        CreditNoteNo, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, isApproved, _tenantDbContextHelper);

                  
          
                }
                else if (reportName == "creditnote")

                {
                    Report = new QD.ERP.Web.Areas.VAT.Reports.VATCreditNote.creditnote(
                        CreditNoteNo, tenantName, companyName, companyAddress, companyNameAr, companyAddressAr, isApproved, _tenantDbContextHelper);
                }
                else if (reportName == "creditnote")
                {
                    Report = new QD.ERP.Web.Areas.VAT.Reports.VATCreditNote.creditnote(
                        CreditNoteNo, tenantName, companyName, companyAddress, companyNameAr, companyAddressAr, isApproved, _tenantDbContextHelper);
                }





                return Page();
            }

            if (reportName == "MaterialRequestInventory" || reportName == "MaterialPurcchaseRequestion")
            {
                if (string.IsNullOrEmpty(RequestNo))
                {
                    return BadRequest("requestNo is required for IMS reports.");
                }

                RequestNo = RequestNo;

                if (reportName == "MaterialRequestInventory")

                {
                    Report = new MaterialRequestInventory(RequestNo, tenantName, companyName, companyAddress, companyNameAr, companyAddressAr, isApproved, _tenantDbContextHelper);
                }

                else if (reportName == "MaterialPurcchaseRequestion")

                {
                    Report = new MaterialPurcchaseRequestion(RequestNo, tenantName, companyName, companyAddress, companyNameAr, companyAddressAr, isApproved, _tenantDbContextHelper);
                }


                return Page();
            }


            if (reportName == "PreviewQuotations" || reportName == "PreviewQuotationwithadditionalDetails" || reportName == "PreviewQuotationwithoutPrice" || reportName == "QuotationWOvat" || reportName == "vatTotalPricewithout" || 
                reportName == "withoutvatDiscount" || reportName == "GroupCode" || reportName == "wtDiscount" || reportName == "PreviewQuotationWithImage" ||  reportName == "PreviewQuotationWithSubGroup")
            {
                if (string.IsNullOrEmpty(quotationNo))
                {
                    return BadRequest("is required for IMS reports.");
                }

                quotationNo = quotationNo;
                switch (reportName)
                {
                    case "PreviewQuotations":
                     
                        Report = new PreviewQuotations(quotationNo, tenantName, companyName, logoImage,companySealImage, companyAddress, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                        break;
                    case "PreviewQuotationwithadditionalDetails":

                        Report = new PreviewQuotationwithadditionalDetails(quotationNo, tenantName, companyName, logoImage, companySealImage, companyAddress, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                        break;
                    case "PreviewQuotationwithoutPrice":

                        Report = new PreviewQuotationwithoutPrice(quotationNo, tenantName, companyName, logoImage, companySealImage, companyAddress, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper);
                        break;
                    case "QuotationWOvat":
                     
                        Report = new QuotationWOvat(quotationNo, tenantName, companyName, logoImage, companySealImage, companyAddress, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper);
                        break;
                    case "vatTotalPricewithout":

                        Report = new vatTotalPricewithout(quotationNo, tenantName, companyName, logoImage, companySealImage, companyAddress,  companyNameAr, companyAddressAr, userName, _tenantDbContextHelper);
                        break;
                    case "withoutvatDiscount":

                        Report = new withoutvatDiscount(quotationNo, tenantName, companyName, logoImage, companySealImage, companyAddress,  companyNameAr, companyAddressAr, userName, _tenantDbContextHelper);
                        break;

                    case "GroupCode":

                        Report = new GroupCode(quotationNo, tenantName, companyName, logoImage, companySealImage, companyAddress,  companyNameAr, companyAddressAr, userName, _tenantDbContextHelper);
                        break;
                    case "wtDiscount":

                        Report = new wtDiscount(quotationNo, tenantName, companyName, logoImage, companySealImage, companyAddress,  companyNameAr, companyAddressAr,userName, _tenantDbContextHelper);
                        break;
                    case "PreviewQuotationWithImage":

                        Report = new PreviewQuotationWithImage(quotationNo, tenantName, companyName, logoImage, companySealImage, companyAddress,  companyNameAr, companyAddressAr, _tenantDbContextHelper);
                        break;
                    case "PreviewQuotationWithSubGroup":

                        Report = new PreviewQuotationWithSubGroup(quotationNo, tenantName, companyName, logoImage, companySealImage, companyAddress,  companyNameAr, companyAddressAr,  _tenantDbContextHelper);
                        break;

                    default:
                        return NotFound("Report not found.");
                }
                return Page();
            }


            if (reportName == "SalesOrderReport")
            {
                if (string.IsNullOrEmpty(salesOrderNo))
                {
                    return BadRequest("requestNo is required for IMS reports.");
                }

                salesOrderNo = salesOrderNo;

                if (reportName == "SalesOrderReport")

                {
                    Report = new SalesOrderReport(salesOrderNo, tenantName, companyName, logoImage, companySealImage, companyAddress, companyPhone, emailAddress, website, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper);
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
                case "journalEntryForm1":
                    Report = new journalEntryForm1(VoucherNo, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper);
                    break;
                case "employeecostJournalRegister":
                    Report = new employeecostJournalRegister(VoucherNo, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper);
                    break;
                case "cashPaymentformat2":
                    Report = new cashPaymentformat2(VoucherNo, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper);
                    break;
                case "SalesReport2":
                    Report = new SalesReport2(VoucherNo, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper);
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
