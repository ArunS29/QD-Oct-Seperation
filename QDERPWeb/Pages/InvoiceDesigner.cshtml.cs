using Azure.Communication.Email;
using DevExpress.XtraReports.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QD.ERP.Web.Areas.Finance.Reports;
using QD.ERP.Web.Areas.Finance.Reports.AccountRegister;
using QD.ERP.Web.Areas.Finance.Reports.Cost_Analysis;
using QD.ERP.Web.Areas.Finance.Reports.Cost_Analysis.Detailed_Report;
using QD.ERP.Web.Areas.Finance.Reports.Cost_Analysis.summary_Report;
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
using QD.ERP.Web.Areas.VAT.Reports.Inventory_Reports;
using QD.ERP.Web.Areas.VAT.Reports.PurchaseRegister;
using QD.ERP.Web.Areas.VAT.Reports.VAT_Sales_Invoice_Register;
using QD.ERP.Web.Areas.VAT.Reports.VATCreditNote;
using QD.ERP.Web.Areas.VAT.Reports.VATDebitNote;
using QD.ERP.Web.Areas.VAT.Reports.VATReturns;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Models.DAL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mail;
using ERPMasterWtDataContext = QD.ERP.Web.DAL.Entities.ERPMasterWtDataContext;

namespace QD.ERP.Web.Pages
{
    public class InvoiceDesignerModel : PageModel
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private ERPMasterWtDataContext _eRPMasterWtDataContext;

        public XtraReport Report { get; private set; }
        public string ReportName { get; private set; }

        public InvoiceDesignerModel(TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
        }

        public IActionResult OnGet(string reportName, string invoiceNo, bool isApproved, string debitNoteNo, string CreditNoteNo, string RequestNo, string quotationNo, string salesOrderNo, string deliveryNoteNo, string rfqNo, string purchaseOrderNo, bool pageBreakBefore = false, bool pageBreakAfter = false,
            bool clientAcknowledgement = false, bool printItemCodeDesc = false, bool printItemPartNoDesc = false, bool printItemPartArabicDesc = false, bool showSign1 = false, bool showSeal = false, bool showSignature = false, bool printLetterhead = false, bool ShowItemLineNo = false, bool PrintFooterAtBottom = false,
           bool ShowitemPartNumberinsteadStockCode = false,bool ShowHSCodeinsteadStockCode = false,bool ShowPaymentTermsShippingDetails = false)
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
            var companyPhone = companyDetails?.CompanyPhone ?? string.Empty;
            var website = companyDetails?.Website ?? string.Empty;
            var emailAddress = companyDetails?.EmailAddress ?? string.Empty;
            string companyAddressAr = companyDetails?.CompanyFullAddressAr ?? string.Empty;
            Image companySealImage = null;
            Image logoImage = null;
            Image sealImage = null;
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
            if (!string.IsNullOrEmpty(quotationNo) )
            {

                switch (reportName)
                {
                    case "PreviewQuotations":

                        Report = new PreviewQuotations(showSeal, showSignature, printLetterhead, pageBreakAfter, pageBreakBefore, clientAcknowledgement, printItemCodeDesc, printItemPartNoDesc, printItemPartArabicDesc, quotationNo, tenantName, companyName, logoImage, companySealImage, companyAddress, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                        break;
                    case "PreviewQuotationwithadditionalDetails":

                        Report = new PreviewQuotationwithadditionalDetails(showSeal, showSignature, printLetterhead, pageBreakAfter, pageBreakBefore, clientAcknowledgement, printItemCodeDesc, printItemPartNoDesc, printItemPartArabicDesc, quotationNo, tenantName, companyName, logoImage, companySealImage, companyAddress, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                        break;
                    case "PreviewQuotationwithoutPrice":

                        Report = new PreviewQuotationwithoutPrice(showSeal, showSignature, printLetterhead, pageBreakAfter, pageBreakBefore, clientAcknowledgement, printItemCodeDesc, printItemPartNoDesc, printItemPartArabicDesc, quotationNo, tenantName, companyName, logoImage, companySealImage, companyAddress, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper);
                        break;
                    case "QuotationWOvat":

                        Report = new QuotationWOvat(showSeal, showSignature, printLetterhead, pageBreakAfter, pageBreakBefore, clientAcknowledgement, printItemCodeDesc, printItemPartNoDesc, printItemPartArabicDesc, quotationNo, tenantName, companyName, logoImage, companySealImage, companyAddress, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper);
                        break;
                    case "vatTotalPricewithout":

                        Report = new vatTotalPricewithout(showSeal, showSignature, printLetterhead, pageBreakAfter, pageBreakBefore, clientAcknowledgement, printItemCodeDesc, printItemPartNoDesc, printItemPartArabicDesc, quotationNo, tenantName, companyName, logoImage, companySealImage, companyAddress, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper);
                        break;
                    case "withoutvatDiscount":

                        Report = new withoutvatDiscount(showSeal, showSignature, printLetterhead, pageBreakAfter, pageBreakBefore, clientAcknowledgement, printItemCodeDesc, printItemPartNoDesc, printItemPartArabicDesc, quotationNo, tenantName, companyName, logoImage, companySealImage, companyAddress, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper);
                        break;

                    case "GroupCode":

                        Report = new GroupCode(showSeal, showSignature, printLetterhead,pageBreakAfter, pageBreakBefore, clientAcknowledgement, printItemCodeDesc, printItemPartNoDesc, printItemPartArabicDesc, quotationNo, tenantName, companyName, logoImage, companySealImage, companyAddress, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper);
                        break;
                    case "wtDiscount":

                        Report = new wtDiscount(showSeal, showSignature, printLetterhead, pageBreakAfter, pageBreakBefore, clientAcknowledgement, printItemCodeDesc, printItemPartNoDesc, printItemPartArabicDesc, quotationNo, tenantName, companyName, logoImage, companySealImage, companyAddress, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper);
                        break;
                    case "PreviewQuotationWithImage":

                        Report = new PreviewQuotationWithImage(showSeal, showSignature, printLetterhead, pageBreakAfter, pageBreakBefore, clientAcknowledgement, printItemCodeDesc, printItemPartNoDesc, printItemPartArabicDesc, quotationNo, tenantName, companyName, logoImage, companySealImage, companyAddress, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                        break;
                    case "PreviewQuotationWithSubGroup":

                        Report = new PreviewQuotationWithSubGroup(showSeal, showSignature, printLetterhead, pageBreakAfter, pageBreakBefore, clientAcknowledgement, printItemCodeDesc, printItemPartNoDesc, printItemPartArabicDesc, quotationNo, tenantName, companyName, logoImage, companySealImage, companyAddress, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                        break;

                    default:
                        return NotFound("Report not found.");
                }
            }


           else  if (!string.IsNullOrEmpty(RequestNo))
            {

                switch (reportName)
                {
                    case "MaterialRequestInventory":

                        Report = new MaterialRequestInventory(logoImage, sealImage,showSeal, showSignature, printLetterhead, RequestNo, showSign1, tenantName, companyName, companyAddress, companyNameAr, companyAddressAr, isApproved, _tenantDbContextHelper);
                        break;
                    case "MaterialPurcchaseRequestion":

                        Report = new MaterialPurcchaseRequestion(logoImage, sealImage,showSeal, showSignature, printLetterhead, RequestNo, tenantName, companyName, companyAddress, companyNameAr, companyAddressAr, isApproved, _tenantDbContextHelper);
                        break;


                    default:
                        return NotFound("Report not found.");
                }
            }

            else if (!string.IsNullOrEmpty(salesOrderNo))
            {

                switch (reportName)
                {
                    case "SalesOrderReport":

                        Report = new SalesOrderReport(showSeal, showSignature, printLetterhead, salesOrderNo, tenantName, companyName, logoImage, companySealImage, companyAddress, companyPhone, emailAddress, website, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper);
                        break;
                    case "SalesOrderReportWithoutPrice":

                        Report = new SalesOrderReportWithoutPrice(showSeal, showSignature, printLetterhead, salesOrderNo, tenantName, companyName, logoImage, companySealImage, companyAddress, companyPhone, emailAddress, website, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper);
                        break;


                    default:
                        return NotFound("Report not found.");
                }
            }
          else  if (!string.IsNullOrEmpty(deliveryNoteNo))
            {

                switch (reportName)
                {
                    case "PreviewDeliveryNote":

                        Report = new previewDeliveryNote(PrintFooterAtBottom, ShowItemLineNo, printItemPartArabicDesc, showSeal, showSignature, printLetterhead, printItemCodeDesc, printItemPartNoDesc, deliveryNoteNo, tenantName, companyName, logoImage, companySealImage, companyAddress, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper);
                        break;
                    case "ReportforMaterialIssueNote":

                        Report = new ReportforMaterialIssueNote(PrintFooterAtBottom, ShowItemLineNo, printItemPartArabicDesc, showSeal, showSignature, printLetterhead, printItemCodeDesc, printItemPartNoDesc, deliveryNoteNo, tenantName, companyName, logoImage, companySealImage, companyAddress, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper);
                        break;
                    case "PreviewDeliveryNotewithPrice":

                        Report = new PreviewDeliveryNotewithPrice(PrintFooterAtBottom, ShowItemLineNo, printItemPartArabicDesc, showSeal, showSignature, printLetterhead, printItemCodeDesc, printItemPartNoDesc, deliveryNoteNo, tenantName, companyName, logoImage, companySealImage, companyAddress, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper);
                        break;
                    case "DotMatrics":

                        Report = new DotMatrics(deliveryNoteNo, tenantName, companyName, companyAddress, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                        break;
                    case "DeliveryNoteWithCostPrice":

                        Report = new DeliveryNoteWithCostPrice(PrintFooterAtBottom, ShowItemLineNo, printItemPartArabicDesc, showSeal, showSignature, printLetterhead, printItemCodeDesc, printItemPartNoDesc, deliveryNoteNo, tenantName, companyName, logoImage, companySealImage, companyAddress, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                        break;


                    default:
                        return NotFound("Report not found.");
                }
            }
           else  if (!string.IsNullOrEmpty(rfqNo))
            {

                switch (reportName)
                {
                    case "RFQEdit":

                        Report = new RFQEdit(showSeal, showSignature, printLetterhead, rfqNo,
                                              tenantName,
                                              companyName,
                                              logoImage,
                                              companySealImage,
                                              companyAddress,
                                              companyNameAr,
                                              companyAddressAr,
                                              userName,
                                              _tenantDbContextHelper
                                          ); break;



                    default:
                        return NotFound("Report not found.");
                }
            }
            else if (!string.IsNullOrEmpty(purchaseOrderNo))
            {

                switch (reportName)
                {
                    case "PreviewPurchaseOrder":

                        Report = new PreviewPurchaseOrder(ShowPaymentTermsShippingDetails,
      ShowitemPartNumberinsteadStockCode,
      ShowHSCodeinsteadStockCode,
      showSeal,
      showSignature,
      printLetterhead,
      pageBreakAfter,
      pageBreakBefore,
      purchaseOrderNo,
      tenantName,
      companyName,
      logoImage,
      companySealImage,
      companyAddress,
      companyNameAr,
      companyAddressAr,
      _tenantDbContextHelper
  ); break;
                    case "PreviewPurchaseOrderForeignCurrency":

                        Report = new PreviewPurchaseOrderForeignCurrency(
                                               pageBreakAfter, pageBreakBefore,
                                               purchaseOrderNo,
                                               tenantName,
                                               companyName,

                                               companySealImage,
                                               companyAddress,
                                               companyNameAr,
                                               companyAddressAr,
                                               _tenantDbContextHelper
                                           ); break;
                    case "PreviewPurchaseOrderWithoutVAT":

                        Report = new PreviewPurchaseOrderWithoutVAT(
                                               pageBreakAfter, pageBreakBefore,
                                              purchaseOrderNo,
                                              tenantName,
                                              companyName,

                                              companySealImage,
                                              companyAddress,
                                              companyNameAr,
                                              companyAddressAr,
                                              _tenantDbContextHelper
                                          ); break;
                    case "WithoutVATTotalPrice":


                        Report = new PreviewPurchaseOrderWithoutVATwWithoutTotalPrice(
                                pageBreakAfter, pageBreakBefore,
                            purchaseOrderNo,
                            tenantName,
                            companyName,

                            companySealImage,
                            companyAddress,
                            companyNameAr,
                            companyAddressAr,
                            _tenantDbContextHelper
                        ); break;
                    default:
                        return NotFound("Report not found.");
                }
            }
            return Page();
        }
    }
}
