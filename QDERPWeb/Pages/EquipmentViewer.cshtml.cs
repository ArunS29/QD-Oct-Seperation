using DevExpress.XtraReports.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QD.ERP.Web.Areas.ERM.Reports.Enquiry;
using QD.ERP.Web.Areas.ERM.Reports.PurchaseOrder;
using QD.ERP.Web.Areas.ERM.Reports.Quotation;
using QD.ERP.Web.Areas.Finance.Reports;
using QD.ERP.Web.Areas.Finance.Reports.cashPayments;
using QD.ERP.Web.Areas.Finance.Reports.ExpensesClaims;
// using QD.ERP.Web.Areas.Finance.Reports.journalEntry;
using QD.ERP.Web.Areas.Finance.Reports.Journal_Register;
using QD.ERP.Web.Areas.Finance.Reports.test;
using QD.ERP.Web.Areas.General.Pages.Report;
using QD.ERP.Web.Areas.IMS.Inventory_Reports;
using QD.ERP.Web.Areas.IMS.InventoryReports.MaterialPurchaseRequistion;
using QD.ERP.Web.Areas.IMS.Report.Inventory_Report;
using QD.ERP.Web.Areas.IMS.Reports.DeliveryNote;
using QD.ERP.Web.Areas.IMS.Reports.InventoryReports;
using QD.ERP.Web.Areas.IMS.Reports.InventroryReports.Delivery_Note;
using QD.ERP.Web.Areas.IMS.Reports.InventroryReports.PurchaseOrder;
using QD.ERP.Web.Areas.IMS.Reports.InventroryReports.RFQ;
using QD.ERP.Web.Areas.IMS.Reports.quotationstoClients;
using QD.ERP.Web.Areas.IMS.Reports.SalesOrder;
using QD.ERP.Web.Areas.VAT.Reports.B2B_INVOICE;
using QD.ERP.Web.Areas.VAT.Reports.Inventory_Reports;
using QD.ERP.Web.Areas.VAT.Reports.VAT_Sales_Invoice_Register;
using QD.ERP.Web.Areas.VAT.Reports.VATCreditNote;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Models.DAL;
using QD.ERP.Web.Pages;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Drawing;
using System.IO;
using System.Linq;
using ERPMasterWtDataContext = QD.ERP.Web.DAL.Entities.ERPMasterWtDataContext;

namespace QD.ERP.Web.Pages
{
    public class EquipmentViewerModel : PageModel
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private ERPMasterWtDataContext _eRPMasterWtDataContext;
        private string username;
        private Image sealImage;

        public EquipmentViewerModel(TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
        }

        public XtraReport Report { get; private set; }
        public string VoucherNo { get; private set; }
        public string ReportName { get; private set; }
       
        

        public IActionResult OnGet(string reportName, string voucherNo, string CompanyId, string invoiceNo, bool isApproved, string debitNoteNo, string CreditNoteNo, string RequestNo, string quotationNo, string salesOrderNo, string deliveryNoteNo, string rfqNo, string purchaseOrderNo, bool pageBreakBefore = false, bool pageBreakAfter = false,
            bool clientAcknowledgement = false, bool printItemCodeDesc = false, bool printItemPartNoDesc = false, bool printItemPartArabicDesc = false, bool showSign1 = false, bool showSeal = false, bool showSignature = false, bool printLetterhead = false, bool ShowItemLineNo = false, bool PrintFooterAtBottom = false, bool ShowitemPartNumberinsteadStockCode = false, bool ShowHSCodeinsteadStockCode = false, bool ShowPaymentTermsShippingDetails = false,
            bool ShowFullSupplierAcceptance = false, bool ShowSimpleSuppilerAcceptance = false, bool ShowSignatoryPositionOnly = false)
        {
            // Set the properties for Razor
            ReportName = reportName;
        
       

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

           

            if (reportName == "CreditForeignCurrency" || reportName == "creditnote" || reportName == "PreviewCreditNoteForeignCurrency" || reportName == "PreviewCreditNoteEnglishOnly")
            {
                if (string.IsNullOrEmpty(CreditNoteNo))
                {
                    return BadRequest("Invoice No is required for invoice reports.");
                }

                CreditNoteNo = CreditNoteNo;

                if (reportName == "CreditForeignCurrency")

                {
                    Report = new QD.ERP.Web.Areas.VAT.Reports.VATCreditNote.CreditForeignCurrency(
                        CreditNoteNo, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, isApproved, userName, _tenantDbContextHelper);



                }
                else if (reportName == "creditnote")

                {
                    Report = new QD.ERP.Web.Areas.VAT.Reports.VATCreditNote.creditnote(
                        CreditNoteNo, tenantName, companyName, companyAddress, companyNameAr, companyAddressAr, isApproved, userName, _tenantDbContextHelper);
                }
                else if (reportName == "PreviewCreditNoteEnglishOnly")

                {
                    Report = new QD.ERP.Web.Areas.VAT.Reports.VATCreditNote.PreviewCreditNoteEnglishOnly(
                        CreditNoteNo, tenantName, companyName, companyAddress, companyNameAr, companyAddressAr, isApproved, userName, _tenantDbContextHelper);
                }
                else if (reportName == "PreviewCreditNoteForeignCurrency")

                {
                    Report = new QD.ERP.Web.Areas.VAT.Reports.VATCreditNote.PreviewCreditNoteForeignCurrency(
                        CreditNoteNo, tenantName, companyName, companyAddress, companyNameAr, companyAddressAr, isApproved, userName, _tenantDbContextHelper);
                }
                //else if (reportName == "creditnote")
                //{
                //    Report = new QD.ERP.Web.Areas.VAT.Reports.VATCreditNote.creditnote(
                //        CreditNoteNo, tenantName, companyName, companyAddress, companyNameAr, companyAddressAr, isApproved, _tenantDbContextHelper,);
                //}





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
                    Report = new MaterialRequestInventory(logoImage, sealImage, showSeal, showSignature, printLetterhead, RequestNo, showSign1, tenantName, companyName, companyAddress, companyNameAr, companyAddressAr, isApproved, _tenantDbContextHelper);
                }

                else if (reportName == "MaterialPurcchaseRequestion")

                {
                    Report = new MaterialPurcchaseRequestion(logoImage, sealImage, showSeal, showSignature, printLetterhead, RequestNo, tenantName, companyName, companyAddress, companyNameAr, companyAddressAr, isApproved, _tenantDbContextHelper);
                }


                return Page();
            }


            if (reportName == "PreviewQuotation" || reportName == "Quotation_Mob_DemobDetails_" || reportName == "Quotation_Mob_DemobwithTotal_" || reportName == "Quotation_With2Rates_" || reportName == "Quotation_with3Rates_" ||
                reportName == "QuotationWithoutVAT" || reportName == "GroupCode" || reportName == "wtDiscount" || reportName == "PreviewQuotationWithImage" || reportName == "PreviewQuotationWithSubGroup")
            {
                if (string.IsNullOrEmpty(quotationNo))
                {
                    return BadRequest("is required for IMS reports.");
                }

                quotationNo = quotationNo;
                switch (reportName)
                {
                    case "PreviewQuotation":

                        Report = new PreviewQuotation(showSeal, showSignature, printLetterhead, pageBreakAfter, pageBreakBefore, clientAcknowledgement,  quotationNo, tenantName, companyName, logoImage, companySealImage, companyAddress, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                        break;
                    case "Quotation_Mob_DemobDetails_":

                        Report = new Quotation_Mob_DemobDetails_(showSeal, showSignature, printLetterhead, pageBreakAfter, pageBreakBefore, clientAcknowledgement,  quotationNo, tenantName, companyName, logoImage, companySealImage, companyAddress, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                        break;
                    case "Quotation_Mob_DemobwithTotal_":

                        Report = new Quotation_Mob_DemobwithTotal_(showSeal, showSignature, printLetterhead, pageBreakAfter, pageBreakBefore, clientAcknowledgement,  quotationNo, tenantName, companyName, logoImage, companySealImage, companyAddress, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                        break;
                    case "Quotation_With2Rates_":

                        Report = new Quotation_With2Rates_(showSeal, showSignature, printLetterhead, pageBreakAfter, pageBreakBefore, clientAcknowledgement, quotationNo, tenantName, companyName, logoImage, companySealImage, companyAddress, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                        break;
                    case "Quotation_with3Rates_":

                        Report = new Quotation_with3Rates_(showSeal, showSignature, printLetterhead, pageBreakAfter, pageBreakBefore, clientAcknowledgement, quotationNo, tenantName, companyName, logoImage, companySealImage, companyAddress, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                        break;
                    case "QuotationWithoutVAT":

                        Report = new QuotationWithoutVAT(showSeal, showSignature, printLetterhead, pageBreakAfter, pageBreakBefore, clientAcknowledgement,quotationNo, tenantName, companyName, logoImage, companySealImage, companyAddress, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                        break;

            
                  
                    default:
                        return NotFound("Report not found.");
                }
                return Page();
            }


            if (reportName == "SalesOrderReport" || reportName == "SalesOrderReportWithoutPrice")
            {
                if (string.IsNullOrEmpty(salesOrderNo))
                {
                    return BadRequest("requestNo is required for IMS reports.");
                }

                salesOrderNo = salesOrderNo;

                if (reportName == "SalesOrderReport")

                {
                    Report = new SalesOrderReport(showSeal, showSignature, printLetterhead, salesOrderNo, tenantName, companyName, logoImage, companySealImage, companyAddress, companyPhone, emailAddress, website, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper);
                }
                else if (reportName == "SalesOrderReportWithoutPrice")

                {
                    Report = new SalesOrderReportWithoutPrice(showSeal, showSignature, printLetterhead, salesOrderNo, tenantName, companyName, logoImage, companySealImage, companyAddress, companyPhone, emailAddress, website, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper);
                }




                return Page();
            }





            if (reportName == "PreviewDeliveryNote" || reportName == "PreviewDeliveryNotewithPrice" || reportName == "ReportforMaterialIssueNote" || reportName == "DotMatrics" || reportName == "DeliveryNoteWithCostPrice")
            {
                if (string.IsNullOrEmpty(deliveryNoteNo))
                {
                    return BadRequest("requestNo is required for IMS reports.");
                }

                deliveryNoteNo = deliveryNoteNo;

                if (reportName == "PreviewDeliveryNote")

                {
                    Report = new previewDeliveryNote(PrintFooterAtBottom, ShowItemLineNo, printItemPartArabicDesc, showSeal, showSignature, printLetterhead, printItemCodeDesc, printItemPartNoDesc, deliveryNoteNo, tenantName, companyName, logoImage, companySealImage, companyAddress, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper);
                }
                if (reportName == "ReportforMaterialIssueNote")

                {
                    Report = new ReportforMaterialIssueNote(PrintFooterAtBottom, ShowItemLineNo, printItemPartArabicDesc, showSeal, showSignature, printLetterhead, printItemCodeDesc, printItemPartNoDesc, deliveryNoteNo, tenantName, companyName, logoImage, companySealImage, companyAddress, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper);
                }
                if (reportName == "PreviewDeliveryNotewithPrice")

                {
                    Report = new PreviewDeliveryNotewithPrice(PrintFooterAtBottom, ShowItemLineNo, printItemPartArabicDesc, showSeal, showSignature, printLetterhead, printItemCodeDesc, printItemPartNoDesc, deliveryNoteNo, tenantName, companyName, logoImage, companySealImage, companyAddress, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper);
                }
                if (reportName == "DotMatrics")

                {
                    Report = new DotMatrics(deliveryNoteNo, tenantName, companyName, companyAddress, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                }
                if (reportName == "DeliveryNoteWithCostPrice")

                {
                    Report = new DeliveryNoteWithCostPrice(PrintFooterAtBottom, ShowItemLineNo, printItemPartArabicDesc, showSeal, showSignature, printLetterhead, printItemCodeDesc, printItemPartNoDesc, deliveryNoteNo, tenantName, companyName, logoImage, companySealImage, companyAddress, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                }




                return Page();
            }

            if (reportName == "PreviewRequests")
            {
                if (string.IsNullOrEmpty(RequestNo))
                {
                    return BadRequest("RequestNo is required for IMS reports.");
                }

                if (reportName == "PreviewRequests")
                {
                    Report = new PreviewRequests(
                        showSeal, showSignature, printLetterhead,
                        RequestNo,
                        tenantName,
                        companyName,
                        logoImage,
                        companySealImage,
                        companyAddress,
                        companyNameAr,
                        companyAddressAr,
                        userName,
                        _tenantDbContextHelper
                    );
                }

                return Page();
            }


            if (reportName == "CompanyDetail1")
            {
                if (string.IsNullOrEmpty(CompanyId))
                {
                    return BadRequest("rfqNo is required for IMS reports.");
                }

                if (reportName == "CompanyDetail1")
                {
                    Report = new CompanyDetail1(

                        CompanyId,
                        _tenantDbContextHelper
                    );
                }

                return Page();
            }
            if (reportName == "PreviewPurchaseOrders" || reportName == "PreviewPurchaseOrderWoVAT")


            {
                if (string.IsNullOrEmpty(purchaseOrderNo))
                {
                    return BadRequest("purchaseOrderNo is required for IMS reports.");
                }
                if (reportName == "PreviewPurchaseOrders")
                {
                    Report = new PreviewPurchaseOrders(companyPhone, emailAddress, website, logoImage, ShowFullSupplierAcceptance, ShowSignatoryPositionOnly,
    
    showSeal,
    showSignature,
    printLetterhead,
    pageBreakAfter,
    pageBreakBefore,
    purchaseOrderNo,
    tenantName,
    companyName,
    companySealImage,
    companyAddress,
    companyNameAr,
    companyAddressAr,
    _tenantDbContextHelper
);
                }
                if (reportName == "PreviewPurchaseOrderWoVAT")
                {
                    Report = new PreviewPurchaseOrderWoVAT(
                       companyPhone, emailAddress, website, logoImage, ShowFullSupplierAcceptance, ShowSignatoryPositionOnly, showSeal, showSignature, printLetterhead, pageBreakAfter, pageBreakBefore, purchaseOrderNo, tenantName, companyName, sealImage, companyAddress, companyNameAr, companyAddressAr,
                        _tenantDbContextHelper
                    );
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
                    Report = new PreviewClaimRequestForm(VoucherNo, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName);
                    break;
                case "ClaimDetailed":
                    Report = new ClaimDetailed(VoucherNo, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName);
                    break;
                case "ClaimEntryCheck":
                    Report = new ClaimEntryCheck(VoucherNo, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper);
                    break;
                case "PreviewClaimRequestForm_wtVAT_":
                    Report = new PreviewClaimRequestForm_wtVAT_(VoucherNo, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper, userName);
                    break;
                case "PaymentsAdviceSupplierPayments":
                    Report = new PaymentsAdviceSupplierPayments(VoucherNo, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                    break;
                default:
                    return NotFound("Report not found.");
            }



            return Page();
        }
    }
}
