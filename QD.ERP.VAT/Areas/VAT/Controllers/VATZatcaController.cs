using Azure.Storage.Blobs;
using Chilkat;
using DevExpress.Entity.Model;
using DevExpress.Pdf.Native.BouncyCastle.Utilities;
using DevExpress.XtraEditors;
using DevExpress.XtraPrinting.Native;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Bcpg.OpenPgp;
using QD.ERP.Shared.Models;
using QD.ERP.Shared.DAL.Entities;
using QD.ERP.Shared.Service;
using SkiaSharp;
using System.ComponentModel.Design;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
//using Task = Chilkat.Task;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;

namespace QD.ERP.VAT.Areas.VAT.Controllers
{
    [Area("VAT")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class VATZatcaController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<VATZatcaController> _logger;
        private readonly X509Certificate2 _certificate;
        private readonly HttpClient _httpClient;
        public string gLastSuccessfulSubmittedHashfile { get; set; }
        public VATZatcaController(ILogger<VATZatcaController> logger, TenantDbContextHelper tenantDbContextHelper, HttpClient httpClient)
        {
            _tenantDbContextHelper = tenantDbContextHelper;

            _logger = logger;
            _httpClient = httpClient;
        }

        public object ObjForm;
        public string strLogonUser;
        public int intLogOnUserID;
        public int intLogOnUserLevel;
        public int intLogOnAccessLevel;
        public int intLogOnDivision;

        public bool IsExecutedFrom_frm00102eInvSalesInvoiceEdit;
        public bool IsExecutedFrom_frm00103eInvCreditNoteEdit;
        public bool IsExecutedFrom_frm00109eInvoiceLog;

        // Public Property App As Object
        public string appPath = @"C:\ZatcaCertificates";

        public string InvoiceSubTypeCode = "01";
        private string _signedXmlGlobal;

        // ---------------------------------------------------------------------
        // Check the Connection to Zatca Portal 
        // ---------------------------------------------------------------------
        Chilkat.Rest rest = new Chilkat.Rest();
        Chilkat.Global chilkatGlob = new Chilkat.Global();
        bool success;
        InvoiceData aInvoiceData = new InvoiceData();

        [HttpPost]
        public IActionResult ConnectToPortal([FromBody] ZatcaRequestDto aZatcaRequestDto)
        {
            bool bTls = true;
            int port = 443;
            bool bAutoReconnect = true;
            string invoiceNo = aZatcaRequestDto.InvNo;
            string connectionStatus = aZatcaRequestDto.ConnectionStatus;

            // Create REST client
            //var rest = new Rest();
            Chilkat.Global glob = new Chilkat.Global();
            bool unlocked = glob.UnlockBundle("WwMfDn.CBX1127_2ThZtySnD3DV");
            if (!unlocked)
            {
                Console.WriteLine(glob.LastErrorText);

            }

            // Connect to ZATCA Production (or Developer if testing)
            bool success = rest.Connect("gw-fatoora.zatca.gov.sa", port, bTls, bAutoReconnect);

            if (!success)
            {
                var errorResponse = new
                {
                    Success = false,
                    Message = "Failed to connect to ZATCA Portal",
                    ConnectFailReason = rest.ConnectFailReason,
                    LastErrorText = rest.LastErrorText
                };

                _logger.LogError("ZATCA connection failed: {Error}", rest.LastErrorText);
                return BadRequest(errorResponse);
            }

            // If connection successful
            InvoiceData aInvoiceData = FillData(invoiceNo);

            if (connectionStatus == "Connected")
            {
                SubmitInvoiceToZatca(invoiceNo, connectionStatus, aInvoiceData);
            }

            // Example: if you want subtype-specific behavior
            if (!string.IsNullOrEmpty(InvoiceSubTypeCode))
            {
                if (InvoiceSubTypeCode == "01")
                {
                    string txtFinalQRCode = "";
                    string txtQRCodeValue = "";
                    // TODO: add logic for subtype 01
                }
                else if (InvoiceSubTypeCode == "02")
                {
                    PrepareSimplifiedTaxInvoice(invoiceNo, aInvoiceData);
                }
            }

            var response = new
            {
                Success = true,
                Message = "Connected to ZATCA Fatoora Portal successfully",
                XMLResponse = _signedXmlGlobal
            };

            return Ok(response);
        }



        private InvoiceData FillData(string invoiceNo)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return null;

            try
            {
                var invoiceData = new InvoiceData();

                invoiceData.Master = dbContext.Qry201630EInvoiceToXmlmaster01s
          .Where(m => m.InvoiceNo == invoiceNo)
          .Select(m => new Qry201630EInvoiceToXmlmaster01
          {
              InvoiceNo = m.InvoiceNo,
              InvoiceUuid = m.InvoiceUuid,
              InvoiceDateWtTime = m.InvoiceDateWtTime,
              InvoiceTypeCode = (short)m.InvoiceTypeCode,
              InvoiceTransactionCode = m.InvoiceTransactionCode,
              RemarksInEn = m.RemarksInEn,
              InvoiceCurrencyCode = m.InvoiceCurrencyCode,
              TaxCurrencyCode = m.TaxCurrencyCode,
              PurchaseOrderId = m.PurchaseOrderId,
              ContractId = m.ContractId,
              InvoiceCounterValue = m.InvoiceCounterValue,
              SellerOtherIdtype = m.SellerOtherIdtype,
              SellerOtherSellerId = m.SellerOtherSellerId,
              SellerAddressStreet = m.SellerAddressStreet,
              SellerAdditionalStreet = m.SellerAdditionalStreet,
              SellerBuildingNumber = m.SellerBuildingNumber,
              SellerAdditionalNumber = m.SellerAdditionalNumber,
              SellerCity = m.SellerCity,
              SellerPostalCode = m.SellerPostalCode,
              SellerProvince = m.SellerProvince,
              SellerNeighborhood = m.SellerNeighborhood,
              SellerCountryCode = m.SellerCountryCode,
              SellerVatnumber = m.SellerVatnumber,
              SellerName = m.SellerName,
              BuyerOtherIdtype = m.BuyerOtherIdtype,
              BuyerOtherId = m.BuyerOtherId,
              BuyerAddressStreet = m.BuyerAddressStreet,
              BuyerAdditionalStreet = m.BuyerAdditionalStreet,
              BuyerBuildingNumber = m.BuyerBuildingNumber,
              BuyerAdditionalNumber = m.BuyerAdditionalNumber,
              BuyerCity = m.BuyerCity,
              BuyerPostalCode = m.BuyerPostalCode,
              BuyerProvince = m.BuyerProvince,
              BuyerNeighborhood = m.BuyerNeighborhood,
              BuyerCountryCode = m.BuyerCountryCode,
              BuyerVatnumber = m.BuyerVatnumber,
              BuyerName = m.BuyerName,
              SupplyDate = m.SupplyDate,
              SupplyEndDate = m.SupplyEndDate,
              PaymentMeansTypeCode = m.PaymentMeansTypeCode,
              PaymentTerms = m.PaymentTerms,
              AdvanceAmount = m.AdvanceAmount,
              RetentionAmount = m.RetentionAmount,
              OtherDeductionAmount = m.OtherDeductionAmount,
              CompanyId = m.CompanyId,
              CreditNoteInvoiceReferenceNo = m.CreditNoteInvoiceReferenceNo,
              CreditNoteReason = m.CreditNoteReason,
              TotalAllDeductionsAmount = m.TotalAllDeductionsAmount,
              SellerNameAr = m.SellerNameAr,
              SellerNeighborhoodAr = m.SellerNeighborhoodAr,
              SellerCityAr = m.SellerCityAr,
              SellerAddressStreetAr = m.SellerAddressStreetAr,
              BuyerNameAr = m.BuyerNameAr,
              BuyerNeighborhoodAr = m.BuyerNeighborhoodAr,
              BuyerCityAr = m.BuyerCityAr,
              BuyerAddressStreetAr = m.BuyerAddressStreetAr
          })
          .FirstOrDefault();

                invoiceData.Lines = dbContext.Qry201636EInvoiceToXmlInvoiceLines
                        .Where(x => x.InvoiceNo == invoiceNo)
                        .Select(x => new Qry201636EInvoiceToXmlInvoiceLine
                        {
                            InvoiceNo = x.InvoiceNo,
                            ItemCode = x.ItemCode,
                            Gsdescrpition = x.Gsdescrpition,
                            UnitDesc = x.UnitDesc,
                            QuantityInvoiced = x.QuantityInvoiced,
                            UnitRate = x.UnitRate,
                            CalcLineAmountBeforeDiscount = x.CalcLineAmountBeforeDiscount,
                            Discount = x.Discount,
                            LineTaxExclusiveAmount = x.LineTaxExclusiveAmount,
                            LineExtensionAmount = x.LineExtensionAmount,
                            LineAllowanceCharges = x.LineAllowanceCharges,
                            LineTaxAmount = x.LineTaxAmount,
                            TaxRateIn100 = x.TaxRateIn100,
                            TaxCodeInZatca = x.TaxCodeInZatca,
                            InvoiceChildSlNo = x.InvoiceChildSlNo,
                            InvoiceLineRoundingAmount = x.InvoiceLineRoundingAmount,
                            LineTaxExclusiveAmountOc = x.LineTaxExclusiveAmountOc,
                            DiscountOc = x.DiscountOc,
                            LineTaxAmountOc = x.LineTaxAmountOc,
                            InvoiceLineRoundingAmountOc = x.InvoiceLineRoundingAmountOc,
                            UnitRateOc = x.UnitRateOc,
                            ItemDescriptionAr = x.ItemDescriptionAr,
                            ItemDescriptionInBoth = x.ItemDescriptionInBoth
                        }).ToList();

                invoiceData.VatBreakdowns = dbContext.Qry201634EInvoiceToXmlchildLineItemsVatbreakDown02s
                    .Where(x => x.InvoiceNo == invoiceNo)
                    .Select(x => new Qry201634EInvoiceToXmlchildLineItemsVatbreakDown02
                    {
                        InvoiceNo = x.InvoiceNo,
                        TaxCodeInZatca = x.TaxCodeInZatca,
                        TaxRateIn100 = x.TaxRateIn100,
                        TotalExclusiveAmount = x.TotalExclusiveAmount,
                        TaxAmountByTaxRate = x.TaxAmountByTaxRate,
                        TotalExtentionAmount = x.TotalExtentionAmount,
                        TotalAllowanceCharges = x.TotalAllowanceCharges,
                        TotalExclusiveAmountOc = x.TotalExclusiveAmountOc,
                        TaxAmountByTaxRateOc = x.TaxAmountByTaxRateOc,
                        TotalExtentionAmountOc = x.TotalExtentionAmountOc,
                        TotalAllowanceChargesOc = x.TotalAllowanceChargesOc,
                        TaxExemptionReasonCode = x.TaxExemptionReasonCode,
                        TaxExemptionReason = x.TaxExemptionReason
                    }).ToList();

                invoiceData.Totals = dbContext.Qry201635EInvoiceToXmlchildLineItemsTotals
                    .Where(x => x.InvoiceNo == invoiceNo)
                    .Select(x => new Qry201635EInvoiceToXmlchildLineItemsTotal
                    {
                        InvoiceNo = x.InvoiceNo,
                        TotalExclusiveAmount = x.TotalExclusiveAmount,
                        TaxAmountByTaxRate = x.TaxAmountByTaxRate,
                        TotalExtentionAmount = x.TotalExtentionAmount,
                        TotalAllowanceCharges = x.TotalAllowanceCharges,
                        TaxInclusiveAmount = x.TaxInclusiveAmount,
                        TotalExclusiveAmountOc = x.TotalExclusiveAmountOc,
                        TaxAmountByTaxRateOc = x.TaxAmountByTaxRateOc,
                        TotalExtentionAmountOc = x.TotalExtentionAmountOc,
                        TotalAllowanceChargesOc = x.TotalAllowanceChargesOc,
                        TaxInclusiveAmountOc = x.TaxInclusiveAmountOc
                    }).ToList();

                invoiceData.AllowanceCharges = dbContext.Qry201636EInvoiceToXmlAllowanceCharges
                        .Where(x => x.InvoiceNo == invoiceNo)
                        .Select(x => new Qry201636EInvoiceToXmlAllowanceCharge
                        {
                            InvoiceNo = x.InvoiceNo,
                            InvoiceChildSlNo = x.InvoiceChildSlNo,
                            Gsdescrpition = x.Gsdescrpition,
                            CalcLineAmountBeforeDiscount = x.CalcLineAmountBeforeDiscount,
                            Discount = x.Discount,
                            LineTaxExclusiveAmount = x.LineTaxExclusiveAmount,
                            LineExtensionAmount = x.LineExtensionAmount,
                            LineAllowanceCharges = x.LineAllowanceCharges,
                            TaxRateIn100 = x.TaxRateIn100,
                            TaxCodeInZatca = x.TaxCodeInZatca,
                            LineAllowanceChargesOc = x.LineAllowanceChargesOc,
                            LineTaxExclusiveAmountOc = x.LineTaxExclusiveAmountOc,
                            LineExtensionAmountOc = x.LineExtensionAmountOc,
                            DiscountInOc = x.DiscountInOc
                        }).ToList();

                invoiceData.SubmissionStatus = dbContext.Qry90132InvoiceSubmissionStatuses
                        .Where(x => x.InvoiceNo == invoiceNo)
                        .Select(x => new Qry90132InvoiceSubmissionStatus
                        {
                            InvoiceNo = x.InvoiceNo,
                            InvoiceDate = x.InvoiceDate,
                            InvoiceStatus = x.InvoiceStatus,
                            EInvoiceTypeCode = x.EInvoiceTypeCode,
                            EInvoiceSubTypeCode = x.EInvoiceSubTypeCode,
                            PreviousHashFile = x.PreviousHashFile,
                            SubmissionStatus = x.SubmissionStatus,
                            CurrentHashFile = x.CurrentHashFile,
                            Qrcode = x.Qrcode,
                            SubmittedBy = x.SubmittedBy,
                            SubmittedOn = x.SubmittedOn,
                            SubmissionStatusText = x.SubmissionStatusText,
                            Icv = x.Icv
                        }).FirstOrDefault();

                invoiceData.PrepaidAdjustments = dbContext.Qry201642prepaidAdjustmentListforXml02s
                        .Where(x => x.EInvoiceNo == invoiceNo)
                        .Select(x => new Qry201642prepaidAdjustmentListforXml02
                        {
                            EInvoiceNo = x.EInvoiceNo,
                            PrepaidInvoiceNo = x.PrepaidInvoiceNo,
                            PrepaidInvoiceDateWtTime = x.PrepaidInvoiceDateWtTime,
                            InvoiceTypeCode = x.InvoiceTypeCode,
                            SubmittedInvoiceUuid = x.SubmittedInvoiceUuid,
                            CurrencyMasterCode = x.CurrencyMasterCode,
                            ExchangeRate = x.ExchangeRate,
                            TaxCodeInZatca = x.TaxCodeInZatca,
                            TaxRateIn100 = x.TaxRateIn100,
                            AdjustedAmountInFc = x.AdjustedAmountInFc,
                            AdjustedTaxAmountInFc = x.AdjustedTaxAmountInFc,
                            PrepaidInvoiceTime = x.PrepaidInvoiceTime,
                            PrepaidInvoiceDate = x.PrepaidInvoiceDate,
                            Sequence = x.Sequence
                        }).ToList();


                return invoiceData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while filling invoice data for {InvoiceNo}", invoiceNo);
                return null;
            }
        }

        private void GenerateUBLFile(InvoiceData aInvoiceData)
        {
            bool success = true;
            Chilkat.Xml xml = new Chilkat.Xml();

            // --------------------------------------------------------------------------------------------------------------
            // Invoice Master Details
            // --------------------------------------------------------------------------------------------------------------
            DateTime invoiceDateTime = aInvoiceData.Master.InvoiceDateWtTime ?? DateTime.MinValue;


            string InvoiceNo = aInvoiceData.Master.InvoiceNo;
            string UniqueInvoiceID = aInvoiceData.Master.InvoiceUuid;
            string InvoiceIssueDate = invoiceDateTime.ToString("yyyy-MM-dd"); // 2025-09-04
            string InvoiceIssueTime = invoiceDateTime.ToString("HH:mm:ss");   // 11:18:02
                                                                              // short InvoiceTypeCode = aInvoiceData.Master.InvoiceTypeCode;
            string InvoiceTransactionCode = aInvoiceData.Master.InvoiceTransactionCode;
            string InvoiceNote = aInvoiceData.Master.RemarksInEn;
            short InvoiceTypeCode = aInvoiceData.Master.InvoiceTypeCode ?? 0;
            //string InvoiceCurrencyCode = aInvoiceData.Master.InvoiceCurrencyCode;
            string InvoiceCurrencyCode = "SAR";
            string TaxCurrencyCode = aInvoiceData.Master.TaxCurrencyCode;
            string InvoiceCounterValue = aInvoiceData.Master.InvoiceCounterValue.ToString();
            //string PreviousInvoiceHash = aInvoiceData.SubmissionStatus.PreviousHashFile;
            string PreviousInvoiceHash = gLastSuccessfulSubmittedHashfile;


            xml.Tag = "Invoice";
            xml.AddAttribute("xmlns", "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2");
            xml.AddAttribute("xmlns:cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
            xml.AddAttribute("xmlns:cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            xml.AddAttribute("xmlns:ext", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
            xml.UpdateChildContent("ext:UBLExtensions|ext:UBLExtension|ext:ExtensionURI", "urn:oasis:names:specification:ubl:dsig:enveloped:xades");
            xml.UpdateAttrAt("ext:UBLExtensions|ext:UBLExtension|ext:ExtensionContent|sig:UBLDocumentSignatures", true, "xmlns:sac", "urn:oasis:names:specification:ubl:schema:xsd:SignatureAggregateComponents-2");
            xml.UpdateAttrAt("ext:UBLExtensions|ext:UBLExtension|ext:ExtensionContent|sig:UBLDocumentSignatures", true, "xmlns:sbc", "urn:oasis:names:specification:ubl:schema:xsd:SignatureBasicComponents-2");
            xml.UpdateAttrAt("ext:UBLExtensions|ext:UBLExtension|ext:ExtensionContent|sig:UBLDocumentSignatures", true, "xmlns:sig", "urn:oasis:names:specification:ubl:schema:xsd:CommonSignatureComponents-2");
            xml.UpdateChildContent("ext:UBLExtensions|ext:UBLExtension|ext:ExtensionContent|sig:UBLDocumentSignatures|sac:SignatureInformation|cbc:ID", "urn:oasis:names:specification:ubl:signature:1");
            xml.UpdateChildContent("ext:UBLExtensions|ext:UBLExtension|ext:ExtensionContent|sig:UBLDocumentSignatures|sac:SignatureInformation|sbc:ReferencedSignatureID", "urn:oasis:names:specification:ubl:signature:Invoice");
            xml.UpdateChildContent("cbc:ProfileID", "reporting:1.0");
            xml.UpdateChildContent("cbc:ID", InvoiceNo);
            xml.UpdateChildContent("cbc:UUID", UniqueInvoiceID);
            xml.UpdateChildContent("cbc:IssueDate", InvoiceIssueDate);
            xml.UpdateChildContent("cbc:IssueTime", InvoiceIssueTime);
            ///Check InvoiceTypeCode
            xml.UpdateAttrAt("cbc:InvoiceTypeCode", true, "name", InvoiceTransactionCode);
            xml.UpdateChildContent("cbc:InvoiceTypeCode", InvoiceTypeCode.ToString());

            xml.UpdateChildContent("cbc:DocumentCurrencyCode", InvoiceCurrencyCode);
            xml.UpdateChildContent("cbc:TaxCurrencyCode", TaxCurrencyCode);

            // if the invoice type is Credit Note then add this...
            if (InvoiceTypeCode == 381)
            {
                string InvDtl = ((char)148).ToString() + "" + ((char)148).ToString();
                xml.UpdateChildContent("cac:BillingReference|cac:InvoiceDocumentReference|cbc:ID", InvDtl);
            }
            else if (InvoiceTypeCode == 383)
            {
                string InvDtl = ((char)148).ToString() + "" + ((char)148).ToString();
                xml.UpdateChildContent("cac:BillingReference|cac:InvoiceDocumentReference|cbc:ID", InvDtl);
            }
            else
            {
                //string InvDtl = char.ConvertFromUtf32(8221) + char.ConvertFromUtf32(8221);
                //xml.UpdateChildContent("cac:BillingReference|cac:InvoiceDocumentReference|cbc:ID", InvDtl);

                // string InvDtl = ((char)148).ToString() + "" + ((char)148).ToString();
                // xml.UpdateChildContent("cac:BillingReference|cac:InvoiceDocumentReference|cbc:ID", InvDtl);
            }


            // Need to add optional fields - PO Number & Contract No.
            // xml.UpdateChildContent("cac:OrderReference|cbc:ID", PurchaseOrderNo)
            // xml.UpdateChildContent("cac:ContractDocumentReference|cbc:ID", ContractNo)

            xml.UpdateChildContent("cac:AdditionalDocumentReference|cbc:ID", "ICV");
            xml.UpdateChildContent("cac:AdditionalDocumentReference|cbc:UUID", InvoiceCounterValue);

            // Previous Invoice Hash
            xml.UpdateChildContent("cac:AdditionalDocumentReference[1]|cbc:ID", "PIH");
            xml.UpdateAttrAt("cac:AdditionalDocumentReference[1]|cac:Attachment|cbc:EmbeddedDocumentBinaryObject", true, "mimeCode", "text/plain");
            xml.UpdateChildContent("cac:AdditionalDocumentReference[1]|cac:Attachment|cbc:EmbeddedDocumentBinaryObject", PreviousInvoiceHash);
            xml.UpdateChildContent("cac:Signature|cbc:ID", "urn:oasis:names:specification:ubl:signature:Invoice");
            xml.UpdateChildContent("cac:Signature|cbc:SignatureMethod", "urn:oasis:names:specification:ubl:dsig:enveloped:xades");


            // --------------------------------------------------------------------------------------------------------------
            // Seller Address Details
            // --------------------------------------------------------------------------------------------------------------


            string SellerOtherIDType = aInvoiceData.Master.SellerOtherIdtype;
            string SellerOtherSellerID = aInvoiceData.Master.SellerOtherSellerId;
            string SellerAddressStreet = aInvoiceData.Master.SellerAddressStreet;
            string SellerAddlStreet = aInvoiceData.Master.SellerAdditionalStreet;
            string SellerBuildingNumber = aInvoiceData.Master.SellerBuildingNumber;
            string SellerAddlNumber = aInvoiceData.Master.SellerAdditionalNumber;
            string SellerCity = aInvoiceData.Master.SellerCity;
            string SellerPostalCode = aInvoiceData.Master.SellerPostalCode;
            string SellerProvince = aInvoiceData.Master.SellerProvince;
            string SellerDistrict = aInvoiceData.Master.SellerNeighborhood;
            string SellerCountryCode = aInvoiceData.Master.SellerCountryCode;
            string SellerVATNumber = aInvoiceData.Master.SellerVatnumber;
            string SellerName = aInvoiceData.Master.SellerName;
            string SellerNameInArabic = aInvoiceData.Master.SellerNameAr;
            string SellerAddressStreetInArabic = aInvoiceData.Master.SellerAddressStreetAr;
            string SellerCityInArabic = aInvoiceData.Master.SellerCityAr;
            string SellerDistrictInArabic = aInvoiceData.Master.SellerNeighborhoodAr;

            string SellerNameInBoth = "";
            SellerNameInBoth = SellerName;


            string SellerAddressStreetInBoth = "";
            SellerAddressStreetInBoth = SellerAddressStreet + " | " + SellerAddressStreetInArabic;

            string SellerCityInBoth = "";
            SellerCityInBoth = SellerCity + " | " + SellerCityInArabic;

            string SellerDistrictInBoth = "";
            SellerDistrictInBoth = SellerDistrict + " | " + SellerDistrictInArabic;

            xml.UpdateAttrAt("cac:AccountingSupplierParty|cac:Party|cac:PartyIdentification|cbc:ID", true, "schemeID", SellerOtherIDType);
            xml.UpdateChildContent("cac:AccountingSupplierParty|cac:Party|cac:PartyIdentification|cbc:ID", SellerOtherSellerID);
            xml.UpdateChildContent("cac:AccountingSupplierParty|cac:Party|cac:PostalAddress|cbc:StreetName", SellerAddressStreetInBoth);
            // xml.UpdateChildContent("cac:AccountingSupplierParty|cac:Party|cac:PostalAddress|cbc:AdditionalStreetName", SellerAddlStreet)
            xml.UpdateChildContent("cac:AccountingSupplierParty|cac:Party|cac:PostalAddress|cbc:BuildingNumber", SellerBuildingNumber);
            xml.UpdateChildContent("cac:AccountingSupplierParty|cac:Party|cac:PostalAddress|cbc:PlotIdentification", SellerAddlNumber);
            xml.UpdateChildContent("cac:AccountingSupplierParty|cac:Party|cac:PostalAddress|cbc:CitySubdivisionName", SellerDistrictInBoth);
            xml.UpdateChildContent("cac:AccountingSupplierParty|cac:Party|cac:PostalAddress|cbc:CityName", SellerCityInBoth);
            xml.UpdateChildContent("cac:AccountingSupplierParty|cac:Party|cac:PostalAddress|cbc:PostalZone", SellerPostalCode);
            xml.UpdateChildContent("cac:AccountingSupplierParty|cac:Party|cac:PostalAddress|cbc:CountrySubentity", SellerProvince);
            xml.UpdateChildContent("cac:AccountingSupplierParty|cac:Party|cac:PostalAddress|cac:Country|cbc:IdentificationCode", SellerCountryCode);
            xml.UpdateChildContent("cac:AccountingSupplierParty|cac:Party|cac:PartyTaxScheme|cbc:CompanyID", SellerVATNumber);
            xml.UpdateChildContent("cac:AccountingSupplierParty|cac:Party|cac:PartyTaxScheme|cac:TaxScheme|cbc:ID", "VAT");
            xml.UpdateChildContent("cac:AccountingSupplierParty|cac:Party|cac:PartyLegalEntity|cbc:RegistrationName", SellerNameInBoth);


            // --------------------------------------------------------------------------------------------------------------
            // Buyer Address Details
            // --------------------------------------------------------------------------------------------------------------


            string BuyerOtherIDType = aInvoiceData.Master.BuyerOtherIdtype;
            string BuyerOtherBuyerID = aInvoiceData.Master.BuyerOtherId;
            string BuyerAddressStreet = aInvoiceData.Master.BuyerAddressStreet;
            string BuyerAddlStreet = aInvoiceData.Master.BuyerAdditionalStreet;
            string BuyerBuildingNumber = aInvoiceData.Master.BuyerBuildingNumber;
            string BuyerAddlNumber = aInvoiceData.Master.BuyerAdditionalNumber;
            string BuyerCity = aInvoiceData.Master.BuyerCity;
            string BuyerPostalCode = aInvoiceData.Master.BuyerPostalCode;
            string BuyerProvince = aInvoiceData.Master.BuyerProvince;
            string BuyerDistrict = aInvoiceData.Master.BuyerNeighborhood;
            string BuyerCountryCode = aInvoiceData.Master.BuyerCountryCode;
            string BuyerVATNumber = aInvoiceData.Master.BuyerVatnumber;
            string BuyerName = aInvoiceData.Master.BuyerName;

            string BuyerNameInArabic = aInvoiceData.Master.BuyerNameAr;
            string BuyerAddressStreetInArabic = aInvoiceData.Master.BuyerAddressStreetAr;
            string BuyerCityInArabic = aInvoiceData.Master.BuyerCityAr;
            string BuyerDistrictInArabic = aInvoiceData.Master.BuyerNeighborhoodAr;

            string BuyerNameInBoth = "";
            BuyerNameInBoth = BuyerName + " | " + BuyerNameInArabic;

            string BuyerAddressStreetInBoth = "";
            BuyerAddressStreetInBoth = BuyerAddressStreet + " | " + BuyerAddressStreetInArabic;

            string BuyerCityInBoth = "";
            BuyerCityInBoth = BuyerCity + " | " + BuyerCityInArabic;

            string BuyerDistrictInBoth = "";
            BuyerDistrictInBoth = BuyerDistrict + " | " + BuyerDistrictInArabic;

            xml.UpdateAttrAt("cac:AccountingCustomerParty|cac:Party|cac:PartyIdentification|cbc:ID", true, "schemeID", BuyerOtherIDType);
            xml.UpdateChildContent("cac:AccountingCustomerParty|cac:Party|cac:PartyIdentification|cbc:ID", BuyerOtherBuyerID);
            xml.UpdateChildContent("cac:AccountingCustomerParty|cac:Party|cac:PostalAddress|cbc:StreetName", BuyerAddressStreetInBoth);
            // xml.UpdateChildContent("cac:AccountingCustomerParty|cac:Party|cac:PostalAddress|cbc:AdditionalStreetName", BuyerAddlStreet)
            xml.UpdateChildContent("cac:AccountingCustomerParty|cac:Party|cac:PostalAddress|cbc:BuildingNumber", BuyerBuildingNumber);
            xml.UpdateChildContent("cac:AccountingCustomerParty|cac:Party|cac:PostalAddress|cbc:PlotIdentification", BuyerAddlNumber);
            xml.UpdateChildContent("cac:AccountingCustomerParty|cac:Party|cac:PostalAddress|cbc:CitySubdivisionName", BuyerDistrictInBoth);
            xml.UpdateChildContent("cac:AccountingCustomerParty|cac:Party|cac:PostalAddress|cbc:CityName", BuyerCityInBoth);
            xml.UpdateChildContent("cac:AccountingCustomerParty|cac:Party|cac:PostalAddress|cbc:PostalZone", BuyerPostalCode);
            xml.UpdateChildContent("cac:AccountingCustomerParty|cac:Party|cac:PostalAddress|cbc:CountrySubentity", BuyerProvince);
            xml.UpdateChildContent("cac:AccountingCustomerParty|cac:Party|cac:PostalAddress|cac:Country|cbc:IdentificationCode", BuyerCountryCode);
            xml.UpdateChildContent("cac:AccountingCustomerParty|cac:Party|cac:PartyTaxScheme|cbc:CompanyID", BuyerVATNumber);
            xml.UpdateChildContent("cac:AccountingCustomerParty|cac:Party|cac:PartyTaxScheme|cac:TaxScheme|cbc:ID", "VAT");
            xml.UpdateChildContent("cac:AccountingCustomerParty|cac:Party|cac:PartyLegalEntity|cbc:RegistrationName", BuyerNameInBoth);


            ////// --------------------------------------------------------------------------------------------------------------
            ////// Payment Details & Supply Details
            ////// --------------------------------------------------------------------------------------------------------------

            string SupplyDate = aInvoiceData.Master.SupplyDate.HasValue
                ? aInvoiceData.Master.SupplyDate.Value.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)
                : string.Empty; // or null if you prefer

            string SupplyEndDate = aInvoiceData.Master.SupplyEndDate.ToString();
            string PaymentMeansTypeCode = aInvoiceData.Master.PaymentMeansTypeCode.ToString();
            string PaymentTerms = aInvoiceData.Master.PaymentTerms;
            string PaymentAccountID = aInvoiceData.Master.ContractId;
            string CreditNoteReason = aInvoiceData.Master.CreditNoteReason;

            xml.UpdateChildContent("cac:Delivery|cbc:ActualDeliveryDate", SupplyDate);
            // xml.UpdateChildContent("cac:Delivery|cbc:LatestDeliveryDate", SupplyEndDate)
            PaymentMeansTypeCode = "30";//Testing static data
            xml.UpdateChildContent("cac:PaymentMeans|cbc:PaymentMeansCode", PaymentMeansTypeCode);
            // xml.UpdateChildContent("cac:PaymentMeans|cac:PayeeFinancialAccount|cbc:PaymentNote", PaymentTerms)
            // xml.UpdateChildContent("cac:PaymentMeans|cac:PayeeFinancialAccount|cbc:ID", PaymentAccountID)

            if (InvoiceTypeCode == 381)
            {
                // Fixed:
                string RetItem = ((char)147).ToString() + CreditNoteReason + ((char)148).ToString();
                xml.UpdateChildContent("cac:PaymentMeans|cbc:InstructionNote", RetItem);
            }
            else if (InvoiceTypeCode == 383)
            {
                string RetItem = ((char)147).ToString() + CreditNoteReason + ((char)148).ToString();
                xml.UpdateChildContent("cac:PaymentMeans|cbc:InstructionNote", RetItem);
            }
            else
            {
                // string RetItem = char.ConvertFromUtf32(8221) + char.ConvertFromUtf32(8221);
                string RetItem = "Bank Transfer";
                //string RetItem = ((char)147).ToString() + CreditNoteReason + ((char)148).ToString();
                xml.UpdateChildContent("cac:PaymentMeans|cbc:InstructionNote", RetItem);
            }


            // --------------------------------------------------------------------------------------------------------------
            // Allowance Charges -  Allowance (discount) at document level
            // --------------------------------------------------------------------------------------------------------------


            //xml.UpdateChildContent("cac:AllowanceCharge|cbc:ChargeIndicator", "false");
            //xml.UpdateAttrAt("cac:AllowanceCharge|cbc:Amount", true, "currencyID", InvoiceCurrencyCode);
            //xml.UpdateChildContent("cac:AllowanceCharge|cbc:Amount", "0.00");
            //xml.UpdateChildContent("cac:AllowanceCharge|cac:TaxCategory|cbc:ID", "S");
            //xml.UpdateChildContent("cac:AllowanceCharge|cac:TaxCategory|cbc:Percent", "15");
            //xml.UpdateChildContent("cac:AllowanceCharge|cac:TaxCategory|cac:TaxScheme|cbc:ID", "VAT");

            for (int i = 0; i <= aInvoiceData.AllowanceCharges.Count - 1; i++)
            {
                xml.UpdateChildContent("cac:AllowanceCharge[" + i + "]|cbc:ChargeIndicator", "false");
                xml.UpdateAttrAt("cac:AllowanceCharge[" + i + "]|cbc:Amount", true, "currencyID", InvoiceCurrencyCode);
                // xml.UpdateChildContent("cac:AllowanceCharge[" + i + "]|cbc:Amount", aInvoiceData.AllowanceCharges.GetRowCellValue(i, "LineAllowanceCharges"));
                if (aInvoiceData.AllowanceCharges != null && i < aInvoiceData.AllowanceCharges.Count)
                {
                    var allowanceCharge = aInvoiceData.AllowanceCharges[i];
                    xml.UpdateChildContent("cac:AllowanceCharge[" + i + "]|cbc:Amount", allowanceCharge.LineAllowanceCharges?.ToString("0.00") ?? "0.00");
                }
                xml.UpdateAttrAt("cac:AllowanceCharge|cac:TaxCategory|cbc:ID", true, "schemeID", "UN/ECE 5305");
                xml.UpdateAttrAt("cac:AllowanceCharge|cac:TaxCategory|cbc:ID", true, "schemeAgencyID", "6");

                //xml.UpdateChildContent("cac:AllowanceCharge[" + i + "]|cac:TaxCategory|cbc:ID", gvInvoiceAllowanceCharges.GetRowCellValue(i, "TaxCodeInZatca"));
                if (aInvoiceData.AllowanceCharges != null && i < aInvoiceData.AllowanceCharges.Count)
                {
                    var allowanceCharge = aInvoiceData.AllowanceCharges[i];
                    xml.UpdateChildContent("cac:AllowanceCharge[" + i + "]|cbc:Amount", allowanceCharge.TaxCodeInZatca);

                    var allowanceCharge1 = aInvoiceData.AllowanceCharges[i];
                    xml.UpdateChildContent("cac:AllowanceCharge[" + i + "]|cbc:Amount", allowanceCharge1.TaxRateIn100.ToString());
                }
                //xml.UpdateChildContent("cac:AllowanceCharge[" + i + "]|cac:TaxCategory|cbc:Percent", gvInvoiceAllowanceCharges.GetRowCellValue(i, "TaxRateIn100"));

                xml.UpdateAttrAt("cac:AllowanceCharge|cac:TaxCategory|cac:TaxScheme|cbc:ID", true, "schemeID", "UN/ECE 5153");
                xml.UpdateAttrAt("cac:AllowanceCharge|cac:TaxCategory|cac:TaxScheme|cbc:ID", true, "schemeAgencyID", "6");

                xml.UpdateChildContent("cac:AllowanceCharge[" + i + "]|cac:TaxCategory|cac:TaxScheme|cbc:ID", "VAT");
            }


            // --------------------------------------------------------------------------------------------------------------
            // VAT Breakdown - 
            // --------------------------------------------------------------------------------------------------------------


            //xml.UpdateAttrAt("cac:TaxTotal|cbc:TaxAmount", true, "currencyID", "SAR");
            //xml.UpdateChildContent("cac:TaxTotal|cbc:TaxAmount", "3900.00");

            ////'Taxable Amount By VAT Category -  Sum of all taxable amounts subject to a specific VAT category code and VAT category rate (if the VAT category rate is applicable). The sum of Invoice line net amount minus allowances on document level which are subject to a specific VAT category code And VAT category rate (if the VAT category rate Is applicable).
            //xml.UpdateAttrAt("cac:TaxTotal|cac:TaxSubtotal|cbc:TaxableAmount", true, "currencyID", "SAR");
            //xml.UpdateChildContent("cac:TaxTotal|cac:TaxSubtotal|cbc:TaxableAmount", "26000.00");

            ////'Tax Amount By VAT Category - Calculated by multiplying the VAT category taxable amount with the VAT category rate for the relevant VAT category.
            //xml.UpdateAttrAt("cac:TaxTotal|cac:TaxSubtotal|cbc:TaxAmount", true, "currencyID", "SAR");
            //xml.UpdateChildContent("cac:TaxTotal|cac:TaxSubtotal|cbc:TaxAmount", "3900.00");

            //xml.UpdateChildContent("cac:TaxTotal|cac:TaxSubtotal|cac:TaxCategory|cbc:ID", "S");
            //xml.UpdateChildContent("cac:TaxTotal|cac:TaxSubtotal|cac:TaxCategory|cbc:Percent", "15");
            //xml.UpdateChildContent("cac:TaxTotal|cac:TaxSubtotal|cac:TaxCategory|cac:TaxScheme|cbc:ID", "VAT");

            ////'''-???
            //xml.UpdateAttrAt("cac:TaxTotal[100]|cbc:TaxAmount", true, "currencyID", "SAR");
            //xml.UpdateChildContent("cac:TaxTotal[100]|cbc:TaxAmount", "3900.00");

            for (int i = 0; i <= aInvoiceData.Totals.Count - 1; i++)
            {
                if (aInvoiceData.Totals != null && i < aInvoiceData.Totals.Count)
                {
                    xml.UpdateAttrAt("cac:TaxTotal[" + aInvoiceData.VatBreakdowns.Count + "]|cbc:TaxAmount", true, "currencyID", "SAR");

                    var total = aInvoiceData.Totals[i];
                    xml.UpdateChildContent("cac:TaxTotal[" + aInvoiceData.VatBreakdowns.Count + "]|cbc:TaxAmount", total.TaxAmountByTaxRate?.ToString("0.00") ?? "0.00");
                }
            }



            for (int i = 0; i < aInvoiceData.VatBreakdowns.Count; i++)
            {
                var vatBreakdown = aInvoiceData.VatBreakdowns[i];
                //Testing Static Data
                // Example testing values (replace with your real values)
                vatBreakdown.TotalExclusiveAmount = 1.00m; // taxable base
                vatBreakdown.TaxRateIn100 = 15.00m;        // 15% VAT
                decimal taxable = vatBreakdown.TotalExclusiveAmount ?? 0m;
                decimal vatRate = (vatBreakdown.TaxRateIn100 ?? 0m) / 100m;
                decimal taxAmount = Math.Round(taxable * vatRate, 2);

                // --- Taxable Amount ---
                xml.UpdateAttrAt($"cac:TaxTotal|cac:TaxSubtotal[{i}]|cbc:TaxableAmount", true, "currencyID", "SAR");
                xml.UpdateChildContent(
                    $"cac:TaxTotal|cac:TaxSubtotal[{i}]|cbc:TaxableAmount",
                    taxable.ToString("0.00", CultureInfo.InvariantCulture)
                );

                // --- Tax Amount ---
                xml.UpdateAttrAt($"cac:TaxTotal|cac:TaxSubtotal[{i}]|cbc:TaxAmount", true, "currencyID", "SAR");
                xml.UpdateChildContent(
                    $"cac:TaxTotal|cac:TaxSubtotal[{i}]|cbc:TaxAmount",
                    taxAmount.ToString("0.00", CultureInfo.InvariantCulture)
                );

                // --- Tax Category ID (UNCL5305: S=Standard, Z=Zero, E=Exempt, O=Out of scope) ---
                xml.UpdateAttrAt($"cac:TaxTotal|cac:TaxSubtotal[{i}]|cac:TaxCategory|cbc:ID", true, "schemeID", "UN/ECE 5305");
                xml.UpdateAttrAt($"cac:TaxTotal|cac:TaxSubtotal[{i}]|cac:TaxCategory|cbc:ID", true, "schemeAgencyID", "6");
                xml.UpdateChildContent(
                    $"cac:TaxTotal|cac:TaxSubtotal[{i}]|cac:TaxCategory|cbc:ID",
                    vatBreakdown.TaxCodeInZatca
                );

                // --- Tax Rate Percent ---
                xml.UpdateChildContent(
                    $"cac:TaxTotal|cac:TaxSubtotal[{i}]|cac:TaxCategory|cbc:Percent",
                    vatBreakdown.TaxRateIn100?.ToString("0.00", CultureInfo.InvariantCulture) ?? "0.00"
                );

                // --- Tax Scheme ---
                xml.UpdateAttrAt($"cac:TaxTotal|cac:TaxSubtotal[{i}]|cac:TaxCategory|cac:TaxScheme|cbc:ID", true, "schemeID", "UN/ECE 5153");
                xml.UpdateAttrAt($"cac:TaxTotal|cac:TaxSubtotal[{i}]|cac:TaxCategory|cac:TaxScheme|cbc:ID", true, "schemeAgencyID", "6");
                xml.UpdateChildContent($"cac:TaxTotal|cac:TaxSubtotal[{i}]|cac:TaxCategory|cac:TaxScheme|cbc:ID", "VAT");

                // --- Tax Exemption Handling ---
                if (vatBreakdown.TaxCodeInZatca == "Z" || vatBreakdown.TaxCodeInZatca == "E")
                {
                    xml.UpdateChildContent(
                        $"cac:TaxTotal|cac:TaxSubtotal[{i}]|cac:TaxCategory|cbc:TaxExemptionReasonCode",
                        vatBreakdown.TaxExemptionReasonCode
                    );
                    xml.UpdateChildContent(
                        $"cac:TaxTotal|cac:TaxSubtotal[{i}]|cac:TaxCategory|cbc:TaxExemptionReason",
                        vatBreakdown.TaxExemptionReason
                    );
                }
                else if (vatBreakdown.TaxCodeInZatca == "O")
                {
                    xml.UpdateChildContent(
                        $"cac:TaxTotal|cac:TaxSubtotal[{i}]|cac:TaxCategory|cbc:TaxExemptionReasonCode",
                        "VATEX-SA-OOS"
                    );
                    xml.UpdateChildContent(
                        $"cac:TaxTotal|cac:TaxSubtotal[{i}]|cac:TaxCategory|cbc:TaxExemptionReason",
                        "VATEX-SA"
                    );
                }
            }


            // ' The Last Line of the Tax Category + Final attribute with total tax amount
            for (int i = 0; i <= aInvoiceData.Totals.Count - 1; i++)
            {
                xml.UpdateAttrAt("cac:TaxTotal[1]|cbc:TaxAmount", true, "currencyID", "SAR");
                var total = aInvoiceData.Totals[i];
                xml.UpdateChildContent("cac:TaxTotal[1]|cbc:TaxAmount", total.TaxAmountByTaxRate?.ToString("0.00") ?? "0.00");
            }


            // --------------------------------------------------------------------------------------------------------------
            // Invoice Level Totals - Invoice total amounts 
            // --------------------------------------------------------------------------------------------------------------

            for (int i = 0; i <= aInvoiceData.Totals.Count - 1; i++)
            {
                xml.UpdateAttrAt("cac:LegalMonetaryTotal|cbc:LineExtensionAmount", true, "currencyID", "SAR");
                var total = aInvoiceData.Totals[i];
                xml.UpdateChildContent("cac:LegalMonetaryTotal|cbc:LineExtensionAmount", total.TotalExtentionAmount?.ToString("0.00") ?? "0.00");

                // FIXED LINE:
                xml.UpdateAttrAt("cac:LegalMonetaryTotal|cbc:TaxExclusiveAmount", true, "currencyID", "SAR");
                xml.UpdateChildContent("cac:LegalMonetaryTotal|cbc:TaxExclusiveAmount", total.TotalExclusiveAmount?.ToString("0.00") ?? "0.00");

                //  xml.UpdateChildContent("cac:LegalMonetaryTotal|cbc:TaxExclusiveAmount", gvInvoiceTotals.GetRowCellValue(i, "TotalExclusiveAmount"));

                xml.UpdateAttrAt("cac:LegalMonetaryTotal|cbc:TaxInclusiveAmount", true, "currencyID", "SAR");
                xml.UpdateChildContent("cac:LegalMonetaryTotal|cbc:TaxInclusiveAmount", total.TaxInclusiveAmount?.ToString("0.00") ?? "0.00");

                //xml.UpdateChildContent("cac:LegalMonetaryTotal|cbc:TaxInclusiveAmount", gvInvoiceTotals.GetRowCellValue(i, "TaxInclusiveAmount"));

                xml.UpdateAttrAt("cac:LegalMonetaryTotal|cbc:AllowanceTotalAmount", true, "currencyID", "SAR");
                xml.UpdateChildContent("cac:LegalMonetaryTotal|cbc:AllowanceTotalAmount", total.TotalAllowanceCharges?.ToString("0.00") ?? "0.00");

                // xml.UpdateChildContent("cac:LegalMonetaryTotal|cbc:AllowanceTotalAmount", gvInvoiceTotals.GetRowCellValue(i, "TotalAllowanceCharges"));

                decimal PrepaidAmount = GetInvoicePrepaidAdjustmentAmount(aInvoiceData.Master.InvoiceNo);
                decimal TaxInclusiveAmount = total.TaxInclusiveAmount ?? 0;
                decimal PayableAmount;
                PayableAmount = TaxInclusiveAmount - PrepaidAmount;

                xml.UpdateAttrAt("cac:LegalMonetaryTotal|cbc:PrepaidAmount", true, "currencyID", "SAR");

                xml.UpdateChildContent("cac:LegalMonetaryTotal|cbc:PrepaidAmount", PrepaidAmount.ToString("0.00"));

                xml.UpdateAttrAt("cac:LegalMonetaryTotal|cbc:PayableAmount", true, "currencyID", "SAR");

                xml.UpdateChildContent("cac:LegalMonetaryTotal|cbc:PayableAmount", PayableAmount.ToString("0.00"));

                xml.UpdateAttrAt("cac:TaxTotal|cbc:TaxAmount", true, "currencyID", "SAR");
                xml.UpdateChildContent("cac:TaxTotal|cbc:TaxAmount", total.TaxAmountByTaxRate?.ToString("0.00") ?? "0.00");
            }




            // '--------------------------------------------------------------------------------------------------------------
            // 'Invoice Line Items  
            // '--------------------------------------------------------------------------------------------------------------


            // xml.UpdateChildContent("cac:InvoiceLine|cbc:ID", "1865")
            // xml.UpdateChildContent("cac:InvoiceLine|cbc:InvoicedQuantity", "1")

            // 'Invoice Line Extension Amount - The total amount of the Invoice line, including allowances (discounts). It is the item net price multiplied with the quantity. The amount Is “net” without VAT.
            // xml.UpdateAttrAt("cac:InvoiceLine|cbc:LineExtensionAmount", True, "currencyID", "SAR")
            // xml.UpdateChildContent("cac:InvoiceLine|cbc:LineExtensionAmount", "2000.00")

            // 'Invoice Line VAT amount  as per Article 53
            // xml.UpdateAttrAt("cac:InvoiceLine|cac:TaxTotal|cbc:TaxAmount", True, "currencyID", "SAR")
            // xml.UpdateChildContent("cac:InvoiceLine|cac:TaxTotal|cbc:TaxAmount", "300.00")

            // 'Invoice Line amount inclusive VAT
            // xml.UpdateAttrAt("cac:InvoiceLine|cac:TaxTotal|cbc:RoundingAmount", True, "currencyID", "SAR")
            // xml.UpdateChildContent("cac:InvoiceLine|cac:TaxTotal|cbc:RoundingAmount", "2300.00")

            // 'Invoice Line Item Details
            // xml.UpdateChildContent("cac:InvoiceLine|cac:Item|cbc:Name", "Computer Set")

            // 'Invoice Line Item - VAT category code for the invoiced item.
            // xml.UpdateChildContent("cac:InvoiceLine|cac:Item|cac:ClassifiedTaxCategory|cbc:ID", "S")
            // xml.UpdateChildContent("cac:InvoiceLine|cac:Item|cac:ClassifiedTaxCategory|cbc:Percent", "15")
            // xml.UpdateChildContent("cac:InvoiceLine|cac:Item|cac:ClassifiedTaxCategory|cac:TaxScheme|cbc:ID", "VAT")

            // 'Invoice Line Item Net Price - The price of an item, exclusive of VAT, after subtracting item price discount. The Item net price has to be equal with the Item gross price minus the Item price discount.
            // xml.UpdateAttrAt("cac:InvoiceLine|cac:Price|cbc:PriceAmount", True, "currencyID", "SAR")
            // xml.UpdateChildContent("cac:InvoiceLine|cac:Price|cbc:PriceAmount", "2000.00")

            for (int i = 0; i <= aInvoiceData.Lines.Count - 1; i++)
            {
                var line = aInvoiceData.Lines[i];

                xml.UpdateChildContent("cac:InvoiceLine[" + i + "]|cbc:ID", line.InvoiceChildSlNo?.ToString());
                xml.UpdateAttrAt("cac:InvoiceLine[" + i + "]|cbc:InvoicedQuantity", true, "unitCode", line.UnitDesc);
                xml.UpdateChildContent("cac:InvoiceLine[" + i + "]|cbc:InvoicedQuantity", line.QuantityInvoiced?.ToString());
                xml.UpdateAttrAt("cac:InvoiceLine[" + i + "]|cbc:LineExtensionAmount", true, "currencyID", "SAR");
                xml.UpdateChildContent("cac:InvoiceLine[" + i + "]|cbc:LineExtensionAmount", line.LineTaxExclusiveAmount?.ToString("0.00"));

                xml.UpdateChildContent("cac:InvoiceLine[" + i + "]|cac:AllowanceCharge|cbc:ChargeIndicator", "false");
                xml.UpdateChildContent("cac:InvoiceLine[" + i + "]|cac:AllowanceCharge|cbc:AllowanceChargeReasonCode", "95");
                xml.UpdateChildContent("cac:InvoiceLine[" + i + "]|cac:AllowanceCharge|cbc:AllowanceChargeReason", "Discount");
                xml.UpdateAttrAt("cac:InvoiceLine[" + i + "]|cac:AllowanceCharge|cbc:Amount", true, "currencyID", "SAR");
                xml.UpdateChildContent("cac:InvoiceLine[" + i + "]|cac:AllowanceCharge|cbc:Amount", line.Discount.ToString("0.0000"));

                xml.UpdateAttrAt("cac:InvoiceLine[" + i + "]|cac:TaxTotal|cbc:TaxAmount", true, "currencyID", "SAR");
                xml.UpdateChildContent("cac:InvoiceLine[" + i + "]|cac:TaxTotal|cbc:TaxAmount", line.LineTaxAmount?.ToString("0.00"));

                xml.UpdateAttrAt("cac:InvoiceLine[" + i + "]|cac:TaxTotal|cbc:RoundingAmount", true, "currencyID", "SAR");
                xml.UpdateChildContent("cac:InvoiceLine[" + i + "]|cac:TaxTotal|cbc:RoundingAmount", line.InvoiceLineRoundingAmount?.ToString("0.00"));

                xml.UpdateChildContent("cac:InvoiceLine[" + i + "]|cac:Item|cbc:Name", line.ItemDescriptionInBoth);

                xml.UpdateChildContent("cac:InvoiceLine[" + i + "]|cac:Item|cac:ClassifiedTaxCategory|cbc:ID", line.TaxCodeInZatca);
                xml.UpdateChildContent("cac:InvoiceLine[" + i + "]|cac:Item|cac:ClassifiedTaxCategory|cbc:Percent", line.TaxRateIn100?.ToString("0.00"));
                xml.UpdateChildContent("cac:InvoiceLine[" + i + "]|cac:Item|cac:ClassifiedTaxCategory|cac:TaxScheme|cbc:ID", "VAT");

                xml.UpdateAttrAt("cac:InvoiceLine[" + i + "]|cac:Price|cbc:PriceAmount", true, "currencyID", "SAR");
                xml.UpdateChildContent("cac:InvoiceLine[" + i + "]|cac:Price|cbc:PriceAmount", line.UnitRate?.ToString("0.00"));
                xml.UpdateChildContent("cac:InvoiceLine[" + i + "]|cac:Price|cbc:BaseQuantity", "1");
            }

            // -------------------------------------------------------------------------------------------------
            // 'Dim lastNoToAdd As Integer = gvInvoiceChildDetails.DataRowCount

            // 'For i As Integer = 0 To gvPrepaidAdjustments.DataRowCount - 1

            // '    Dim j As Integer = i + lastNoToAdd + 1

            // '    xml.UpdateChildContentInt("cac:InvoiceLine[" & j & "]|cbc:ID", gvPrepaidAdjustments.GetRowCellValue(i, "Sequence"))
            // '    xml.UpdateAttrAt("cac:InvoiceLine[" & j & "]|cbc:InvoicedQuantity", True, "unitCode", "PCE")
            // '    xml.UpdateChildContent("cac:InvoiceLine[" & j & "]|cbc:InvoicedQuantity", "0.000000")
            // '    xml.UpdateAttrAt("cac:InvoiceLine[" & j & "]|cbc:LineExtensionAmount", True, "currencyID", gvPrepaidAdjustments.GetRowCellValue(i, "CurrencyMasterCode"))
            // '    xml.UpdateChildContent("cac:InvoiceLine[" & j & "]|cbc:LineExtensionAmount", "0.00")

            // '    xml.UpdateChildContent("cac:InvoiceLine[" & j & "]|cac:DocumentReference|cbc:ID", gvPrepaidAdjustments.GetRowCellValue(i, "PrepaidInvoiceNo"))
            // '    xml.UpdateChildContent("cac:InvoiceLine[" & j & "]|cac:DocumentReference|cbc:UUID", gvPrepaidAdjustments.GetRowCellValue(i, "SubmittedInvoiceUUID"))
            // '    xml.UpdateChildContent("cac:InvoiceLine[" & j & "]|cac:DocumentReference|cbc:IssueDate", gvPrepaidAdjustments.GetRowCellValue(i, "PrepaidInvoiceDate"))
            // '    xml.UpdateChildContent("cac:InvoiceLine[" & j & "]|cac:DocumentReference|cbc:IssueTime", gvPrepaidAdjustments.GetRowCellValue(i, "PrepaidInvoiceTime"))
            // '    xml.UpdateChildContent("cac:InvoiceLine[" & j & "]|cac:DocumentReference|cbc:DocumentTypeCode", 386)

            // '    xml.UpdateAttrAt("cac:InvoiceLine[" & j & "]|cac:TaxTotal|cbc:TaxAmount", True, "currencyID", gvPrepaidAdjustments.GetRowCellValue(i, "CurrencyMasterCode"))
            // '    xml.UpdateChildContent("cac:InvoiceLine[" & j & "]|cac:TaxTotal|cbc:TaxAmount", 0)
            // '    xml.UpdateAttrAt("cac:InvoiceLine[" & j & "]|cac:TaxTotal|cbc:RoundingAmount", True, "currencyID", gvPrepaidAdjustments.GetRowCellValue(i, "CurrencyMasterCode"))
            // '    xml.UpdateChildContent("cac:InvoiceLine[" & j & "]|cac:TaxTotal|cbc:RoundingAmount", 0)

            // '    xml.UpdateAttrAt("cac:InvoiceLine[" & j & "]|cac:TaxTotal|cac:TaxSubtotal|cbc:TaxableAmount", True, "currencyID", gvPrepaidAdjustments.GetRowCellValue(i, "CurrencyMasterCode"))
            // '    xml.UpdateChildContent("cac:InvoiceLine[" & j & "]|cac:TaxTotal|cac:TaxSubtotal|cbc:TaxableAmount", gvPrepaidAdjustments.GetRowCellValue(i, "AdjustedAmountInFC"))
            // '    xml.UpdateAttrAt("cac:InvoiceLine[" & j & "]|cac:TaxTotal|cac:TaxSubtotal|cbc:TaxAmount", True, "currencyID", gvPrepaidAdjustments.GetRowCellValue(i, "CurrencyMasterCode"))
            // '    xml.UpdateChildContent("cac:InvoiceLine[" & j & "]|cac:TaxTotal|cac:TaxSubtotal|cbc:TaxAmount", gvPrepaidAdjustments.GetRowCellValue(i, "AdjustedTaxAmountInFC"))
            // '    xml.UpdateChildContent("cac:InvoiceLine[" & j & "]|cac:TaxTotal|cac:TaxSubtotal|cac:TaxCategory|cbc:ID", gvPrepaidAdjustments.GetRowCellValue(i, "TaxCodeInZatca"))
            // '    xml.UpdateChildContent("cac:InvoiceLine[" & j & "]|cac:TaxTotal|cac:TaxSubtotal|cac:TaxCategory|cbc:Percent", gvPrepaidAdjustments.GetRowCellValue(i, "TaxRateIn100"))
            // '    xml.UpdateChildContent("cac:InvoiceLine[" & j & "]|cac:TaxTotal|cac:TaxSubtotal|cac:TaxCategory|cac:TaxScheme|cbc:ID", "VAT")

            // '    'Invoice Line Item Details
            // '    xml.UpdateChildContent("cac:InvoiceLine[" & j & "]|cac:Item|cbc:Name", "Prepaid Advance Amount")
            // '    'Invoice Line Item - VAT category code for the invoiced item.
            // '    xml.UpdateChildContent("cac:InvoiceLine[" & j & "]|cac:Item|cac:ClassifiedTaxCategory|cbc:ID", gvPrepaidAdjustments.GetRowCellValue(i, "TaxCodeInZatca"))
            // '    xml.UpdateChildContent("cac:InvoiceLine[" & j & "]|cac:Item|cac:ClassifiedTaxCategory|cbc:Percent", gvPrepaidAdjustments.GetRowCellValue(i, "TaxRateIn100"))
            // '    xml.UpdateChildContent("cac:InvoiceLine[" & j & "]|cac:Item|cac:ClassifiedTaxCategory|cac:TaxScheme|cbc:ID", "VAT")

            // '    xml.UpdateAttrAt("cac:InvoiceLine[" & j & "]|cac:Price|cbc:PriceAmount", True, "currencyID", gvPrepaidAdjustments.GetRowCellValue(i, "CurrencyMasterCode"))
            // '    xml.UpdateChildContent("cac:InvoiceLine[" & j & "]|cac:Price|cbc:PriceAmount", "0.00")

            // 'Next

            int lastNoToAdd = aInvoiceData.Lines.Count - 1;

            for (int i = 0; i <= aInvoiceData.PrepaidAdjustments.Count - 1; i++)
            {
                int j = i + lastNoToAdd + 1;

                // Retrieve values directly from PrepaidAdjustments
                var prepaidAdjustment = aInvoiceData.PrepaidAdjustments[i];
                var sequence = prepaidAdjustment.Sequence?.ToString();
                var currencyCode = prepaidAdjustment.CurrencyMasterCode;
                var prepaidInvoiceNo = prepaidAdjustment.PrepaidInvoiceNo;
                var submittedInvoiceUUID = prepaidAdjustment.SubmittedInvoiceUuid;
                var prepaidInvoiceDate = prepaidAdjustment.PrepaidInvoiceDate;
                var prepaidInvoiceTime = prepaidAdjustment.PrepaidInvoiceTime;
                var adjustedAmount = prepaidAdjustment.AdjustedAmountInFc?.ToString("0.00");
                var adjustedTaxAmount = prepaidAdjustment.AdjustedTaxAmountInFc?.ToString("0.00");
                var taxCode = prepaidAdjustment.TaxCodeInZatca;
                var taxRate = prepaidAdjustment.TaxRateIn100?.ToString("0.00");

                // Check if necessary fields are empty
                if (string.IsNullOrEmpty(sequence) || string.IsNullOrEmpty(currencyCode) || string.IsNullOrEmpty(prepaidInvoiceNo) || string.IsNullOrEmpty(prepaidInvoiceDate) || string.IsNullOrEmpty(prepaidInvoiceTime))
                    continue;

                // Start the InvoiceLine element
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]", "");

                // ID
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]|cbc:ID", sequence);

                // Invoiced Quantity
                xml.UpdateAttrAt("cac:InvoiceLine[" + j + "]|cbc:InvoicedQuantity", true, "unitCode", "PCE");
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]|cbc:InvoicedQuantity", "0.000000");

                // Line Extension Amount
                xml.UpdateAttrAt("cac:InvoiceLine[" + j + "]|cbc:LineExtensionAmount", true, "currencyID", currencyCode);
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]|cbc:LineExtensionAmount", "0.00");

                // Document Reference
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]|cac:DocumentReference|cbc:ID", prepaidInvoiceNo);
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]|cac:DocumentReference|cbc:UUID", submittedInvoiceUUID);
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]|cac:DocumentReference|cbc:IssueDate", prepaidInvoiceDate);
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]|cac:DocumentReference|cbc:IssueTime", prepaidInvoiceTime);
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]|cac:DocumentReference|cbc:DocumentTypeCode", "386");

                // Tax Total
                xml.UpdateAttrAt("cac:InvoiceLine[" + j + "]|cac:TaxTotal|cbc:TaxAmount", true, "currencyID", currencyCode);
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]|cac:TaxTotal|cbc:TaxAmount", "0");
                xml.UpdateAttrAt("cac:InvoiceLine[" + j + "]|cac:TaxTotal|cbc:RoundingAmount", true, "currencyID", currencyCode);
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]|cac:TaxTotal|cbc:RoundingAmount", "0");
                xml.UpdateAttrAt("cac:InvoiceLine[" + j + "]|cac:TaxTotal|cac:TaxSubtotal|cbc:TaxableAmount", true, "currencyID", currencyCode);
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]|cac:TaxTotal|cac:TaxSubtotal|cbc:TaxableAmount", adjustedAmount);
                xml.UpdateAttrAt("cac:InvoiceLine[" + j + "]|cac:TaxTotal|cac:TaxSubtotal|cbc:TaxAmount", true, "currencyID", currencyCode);
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]|cac:TaxTotal|cac:TaxSubtotal|cbc:TaxAmount", adjustedTaxAmount);
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]|cac:TaxTotal|cac:TaxSubtotal|cac:TaxCategory|cbc:ID", taxCode);
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]|cac:TaxTotal|cac:TaxSubtotal|cac:TaxCategory|cbc:Percent", taxRate);
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]|cac:TaxTotal|cac:TaxSubtotal|cac:TaxCategory|cac:TaxScheme|cbc:ID", "VAT");

                // Item Details
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]|cac:Item|cbc:Name", "Prepaid Advance Amount | المبلغ المدفوع مقدمًا");
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]|cac:Item|cac:ClassifiedTaxCategory|cbc:ID", taxCode);
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]|cac:Item|cac:ClassifiedTaxCategory|cbc:Percent", taxRate);
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]|cac:Item|cac:ClassifiedTaxCategory|cac:TaxScheme|cbc:ID", "VAT");

                // Price Amount
                xml.UpdateAttrAt("cac:InvoiceLine[" + j + "]|cac:Price|cbc:PriceAmount", true, "currencyID", currencyCode);
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]|cac:Price|cbc:PriceAmount", "0.0000");
            }
            // '--------------------------------------------------------------------------------------------------------------
            // 'UBL Extension Codes 
            // '--------------------------------------------------------------------------------------------------------------

            Chilkat.XmlDSigGen gen = new Chilkat.XmlDSigGen();

            gen.SigLocation = "Invoice|ext:UBLExtensions|ext:UBLExtension|ext:ExtensionContent|sig:UBLDocumentSignatures|sac:SignatureInformation";
            gen.SigLocationMod = 0;
            gen.SigId = "signature";
            gen.SigNamespacePrefix = "ds";
            gen.SigNamespaceUri = "http://www.w3.org/2000/09/xmldsig#";
            gen.SignedInfoCanonAlg = "C14N_11";
            gen.SignedInfoDigestMethod = "sha256";

            // Create an Object to be added to the Signature.

            // Commented by Saju
            // -<xadesQualifyingProperties Target = "signature" xmlns: xades = "http://uri.etsi.org/01903/v1.3.2#" >
            // -<xadesSignedProperties Id = "xadesSignedProperties" >
            // -<xadesSignedSignatureProperties>
            // <xades:SigningTime> 2022 - 11 - 16T05:20:37Z</xades:SigningTime>
            // -<xadesSigningCertificate>
            // -<xadescert>
            // -<xadesCertDigest>
            // <ds:DigestMethod Algorithm = "http://www.w3.org/2001/04/xmlenc#sha256" />
            // (Added by Chilkat)               <ds:DigestValue> lZUwCVs9AAVwENypLvUYTO1MVlyfOe1Ba2C0fglq2So =</ds:DigestValue>
            // </xadesCertDigest>
            // -<xadesIssuerSerial>
            // <ds:X509IssuerName> CN = TSZEINVOICE - SubCA - 1, DC = extgazt, DC = gov, DC = local</ds: X509IssuerName>
            // <ds:X509SerialNumber> 2475382876776561391517206651645660279462721580</ds:X509SerialNumber>
            // </xadesIssuerSerial>
            // </xadescert>
            // </xadesSigningCertificate>
            // </xadesSignedSignatureProperties>
            // </xadesSignedProperties>
            // </xadesQualifyingProperties>

            Chilkat.Xml object1 = new Chilkat.Xml();
            object1.Tag = "xades:QualifyingProperties";
            object1.AddAttribute("xmlns:xades", "http://uri.etsi.org/01903/v1.3.2#");
            object1.AddAttribute("Target", "signature");
            object1.UpdateAttrAt("xades:SignedProperties", true, "Id", "xadesSignedProperties");
            object1.UpdateChildContent("xades:SignedProperties|xades:SignedSignatureProperties|xades:SigningTime", "TO BE GENERATED BY CHILKAT");
            object1.UpdateAttrAt("xades:SignedProperties|xades:SignedSignatureProperties|xades:SigningCertificate|xades:Cert|xades:CertDigest|ds:DigestMethod", true, "Algorithm", "http://www.w3.org/2001/04/xmlenc#sha256");
            object1.UpdateChildContent("xades:SignedProperties|xades:SignedSignatureProperties|xades:SigningCertificate|xades:Cert|xades:CertDigest|ds:DigestValue", "TO BE GENERATED BY CHILKAT");

            object1.UpdateChildContent("xades:SignedProperties|xades:SignedSignatureProperties|xades:SigningCertificate|xades:Cert|xades:IssuerSerial|ds:X509IssuerName", "TO BE GENERATED BY CHILKAT"); // "CN=TSZEINVOICE-SubCA-1, DC=extgazt, DC=gov, DC=local")
            object1.UpdateChildContent("xades:SignedProperties|xades:SignedSignatureProperties|xades:SigningCertificate|xades:Cert|xades:IssuerSerial|ds:X509SerialNumber", "TO BE GENERATED BY CHILKAT"); // "2475382886904809774818644480820936050208702411")

            // object1.UpdateChildContent("xades:SignedProperties|xades:SignedSignatureProperties|xades:SigningCertificate|xades:Cert|xades:IssuerSerial|ds:X509IssuerName", "CN=TSZEINVOICE-SubCA-1, DC=extgazt, DC=gov, DC=local")
            // object1.UpdateChildContent("xades:SignedProperties|xades:SignedSignatureProperties|xades:SigningCertificate|xades:Cert|xades:IssuerSerial|ds:X509SerialNumber", "2475382886904809774818644480820936050208702411")

            // object1.UpdateChildContent("xades:SignedProperties|xades:SignedSignatureProperties|xades:SigningCertificate|xades:Cert|xades:IssuerSerial|ds:X509IssuerName", "TO BE GENERATED BY CHILKAT")
            // object1.UpdateChildContent("xades:SignedProperties|xades:SignedSignatureProperties|xades:SigningCertificate|xades:Cert|xades:IssuerSerial|ds:X509SerialNumber", "TO BE GENERATED BY CHILKAT")

            gen.AddObject("", object1.GetXml(), "", "");


            // -------- Reference 1 --------
            Chilkat.Xml xml1 = new Chilkat.Xml();
            xml1.Tag = "ds:Transforms";
            xml1.UpdateAttrAt("ds:Transform", true, "Algorithm", "http://www.w3.org/TR/1999/REC-xpath-19991116");
            xml1.UpdateChildContent("ds:Transform|ds:XPath", "not(//ancestor-or-self::ext:UBLExtensions)");
            xml1.UpdateAttrAt("ds:Transform[1]", true, "Algorithm", "http://www.w3.org/TR/1999/REC-xpath-19991116");
            xml1.UpdateChildContent("ds:Transform[1]|ds:XPath", "not(//ancestor-or-self::cac:Signature)");
            xml1.UpdateAttrAt("ds:Transform[2]", true, "Algorithm", "http://www.w3.org/TR/1999/REC-xpath-19991116");
            xml1.UpdateChildContent("ds:Transform[2]|ds:XPath", "not(//ancestor-or-self::cac:AdditionalDocumentReference[cbc:ID='QR'])");
            xml1.UpdateAttrAt("ds:Transform[3]", true, "Algorithm", "http://www.w3.org/2006/12/xml-c14n11");

            gen.AddSameDocRef2("", "sha256", xml1, "");
            gen.SetRefIdAttr("", "invoiceSignedData");

            // -------- Reference 2 --------
            gen.AddObjectRef("xadesSignedProperties", "sha256", "", "", "http://www.w3.org/2000/09/xmldsig#SignatureProperties");

            // Old Code
            // Provide a certificate + private key. (PFX password is test123)
            // Dim cert As New Chilkat.Cert
            // success = cert.LoadPfxFile(appPath & "\Certificates\gaztCertificate.p12", "123456789")
            // If (success <> True) Then
            // MessageBox.Show("Certificate Failed to Open..", "Error Certifiate", MessageBoxButtons.OK, MessageBoxIcon.Error)
            // Exit Sub
            // End If

            // ============================================================================================
            // New Update
            // ============================================================================================
            // Alternatively, if your certificate and private key are in separate PEM files, do this:
            Chilkat.Cert cert = new Chilkat.Cert();

            GetEGSUnitDetails(1);
            success = cert.LoadFromBase64(gProductionPEM);

            if ((success != true))
            {
                // MessageBox.Show(cert.LastErrorText);
                // return;
            }

            // Debug.WriteLine(cert.SubjectCN)

            // Load the private key.
            Chilkat.PrivateKey privKey = new Chilkat.PrivateKey();
            success = privKey.LoadPem(gPrivateKey);

            if ((success != true))
            {
                // MessageBox.Show(privKey.LastErrorText);
                // return;
            }

            // Associate the private key with the certificate.
            success = cert.SetPrivateKey(privKey);
            if ((success != true))
            {
                // MessageBox.Show(cert.LastErrorText);
                // return;
            }
            // =====================================================================================
            gen.SetX509Cert(cert, true);

            if ((success != true))
            {
                Debug.WriteLine(gen.LastErrorText);
                // return;
            }


            gen.KeyInfoType = "X509Data";
            gen.X509Type = "Certificate";

            // Load XML to be signed...
            Chilkat.StringBuilder sbXml = new Chilkat.StringBuilder();
            xml.GetXmlSb(sbXml);

            gen.Behaviors = "IndentedSignature,TransformSignatureXPath,ZATCA";

            string XMLVersion;
            XMLVersion = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n";
            int Succ;
            Succ = sbXml.Replace(XMLVersion, "\r\n");

            // Sign the XML...
            success = gen.CreateXmlDSigSb(sbXml);
            if ((success != true))
            {
                //MessageBox.Show("Failed to Sign XML UBL 2.1", "Error XML", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //MessageBox.Show(gen.LastErrorText);
                // return;
            }

            // Save the signed XML to a file.
            // success = sbXml.WriteFile(appPath + @"\SignedXML\signedXmlResult1.xml", "utf-8", false);

            string folderPath = Path.Combine(appPath, "SignedXML");

            // Ensure the folder exists
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Now write the file safely
            string filePath = Path.Combine(folderPath, "signedXmlResult1.xml");
            //  success = sbXml.WriteFile(filePath, "utf-8", false);
            string defaultCompanyString = HttpContext.Session.GetString("DefaultcompanyID") ?? "";
            byte defaultCompanyByte = 0; // or any default value you want

            if (!string.IsNullOrEmpty(defaultCompanyString))
            {
                // Safest way (avoids exceptions):
                byte.TryParse(defaultCompanyString, out defaultCompanyByte);
                // Now defaultCompanyByte holds the parsed value, or 0 if parsing failed.
            }

            // Now use defaultCompanyByte as needed


            byte companyId = defaultCompanyByte;

            SaveSignedXmlBasedOnAzureStatus(sbXml, filePath, companyId);
            _signedXmlGlobal = sbXml.GetAsString();
            // ----------------------------------------
            // Verify the signatures we just produced...
            Chilkat.XmlDSig verifier = new Chilkat.XmlDSig();
            success = verifier.LoadSignatureSb(sbXml);
            if ((success != true))
            {
                Debug.WriteLine(verifier.LastErrorText);
                //return;
            }


            // ---------------- This is important -----------------------------------------
            // Starting in Chilkat v9.5.0.92, specify "ZATCA" in uncommon options 
            // to validate signed XML according to ZATCA needs.
            // ----------------------------------------------------------------------------
            verifier.UncommonOptions = "ZATCA";

            int numSigs = verifier.NumSignatures;
            int verifyIdx = 0;
            while (verifyIdx < numSigs)
            {
                verifier.Selector = verifyIdx;
                bool verified = verifier.VerifySignature(true);
                if ((verified != true))
                {
                    Debug.WriteLine(verifier.LastErrorText);
                    // return;
                }

                verifyIdx = verifyIdx + 1;
            }
            // Debug.WriteLine("All signatures were successfully verified.")



            //  MessageBox.Show("Failed to Create XML UBL 2.1", "Error XML", MessageBoxButtons.OK, MessageBoxIcon.Error);

            Debug.WriteLine("Signed XML file generated at: " + appPath + @"\SignedXML\signedXmlResult1.xml");

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = appPath + @"\SignedXML\signedXmlResult1.xml",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to open XML file: " + ex.Message);
            }
            NavigateSignedXml(appPath, companyId);

            //this.WebBrowser1.Navigate(appPath + @"\SignedXML\signedXmlResult1.xml");
        }



        private void GenerateUBLFile_ForeignCurrency(InvoiceData aInvoiceData)
        {
            bool success = true;
            Chilkat.Xml xml = new Chilkat.Xml();

            // --------------------------------------------------------------------------------------------------------------
            // Invoice Master Details
            // --------------------------------------------------------------------------------------------------------------

            DateTime invoiceDateTime = aInvoiceData.Master.InvoiceDateWtTime ?? DateTime.MinValue;

            string InvoiceNo = aInvoiceData.Master.InvoiceNo;
            short InvoiceTypeCode = aInvoiceData.Master.InvoiceTypeCode ?? 0;
            // string InvoiceCurrencyCode = aInvoiceData.Master.InvoiceCurrencyCode;
            string TaxCurrencyCode = aInvoiceData.Master.TaxCurrencyCode;
            string SupplierName = aInvoiceData.Master.SellerName;
            string SupplierNameAr = aInvoiceData.Master.SellerNameAr;
            string BuyerName = aInvoiceData.Master.BuyerName;
            string BuyerNameAr = aInvoiceData.Master.BuyerNameAr;
            string UniqueInvoiceID = aInvoiceData.Master.InvoiceUuid;
            string InvoiceIssueDate = invoiceDateTime.ToString("yyyy-MM-dd"); // 2025-09-04
            string InvoiceIssueTime = invoiceDateTime.ToString("HH:mm:ss");   // 11:18:02
                                                                              // short InvoiceTypeCode = aInvoiceData.Master.InvoiceTypeCode;
            string InvoiceTransactionCode = aInvoiceData.Master.InvoiceTransactionCode;
            string InvoiceNote = aInvoiceData.Master.RemarksInEn;
            //string InvoiceCurrencyCode = aInvoiceData.Master.InvoiceCurrencyCode;
            string InvoiceCurrencyCode = "USD";
            string InvoiceCounterValue = aInvoiceData.Master.InvoiceCounterValue.ToString();
            //string PreviousInvoiceHash = aInvoiceData.SubmissionStatus.PreviousHashFile;
            string PreviousInvoiceHash = gLastSuccessfulSubmittedHashfile;

            xml.Tag = "Invoice";
            xml.AddAttribute("xmlns", "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2");
            xml.AddAttribute("xmlns:cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
            xml.AddAttribute("xmlns:cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            xml.AddAttribute("xmlns:ext", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
            xml.UpdateChildContent("ext:UBLExtensions|ext:UBLExtension|ext:ExtensionURI", "urn:oasis:names:specification:ubl:dsig:enveloped:xades");
            xml.UpdateAttrAt("ext:UBLExtensions|ext:UBLExtension|ext:ExtensionContent|sig:UBLDocumentSignatures", true, "xmlns:sac", "urn:oasis:names:specification:ubl:schema:xsd:SignatureAggregateComponents-2");
            xml.UpdateAttrAt("ext:UBLExtensions|ext:UBLExtension|ext:ExtensionContent|sig:UBLDocumentSignatures", true, "xmlns:sbc", "urn:oasis:names:specification:ubl:schema:xsd:SignatureBasicComponents-2");
            xml.UpdateAttrAt("ext:UBLExtensions|ext:UBLExtension|ext:ExtensionContent|sig:UBLDocumentSignatures", true, "xmlns:sig", "urn:oasis:names:specification:ubl:schema:xsd:CommonSignatureComponents-2");
            xml.UpdateChildContent("ext:UBLExtensions|ext:UBLExtension|ext:ExtensionContent|sig:UBLDocumentSignatures|sac:SignatureInformation|cbc:ID", "urn:oasis:names:specification:ubl:signature:1");
            xml.UpdateChildContent("ext:UBLExtensions|ext:UBLExtension|ext:ExtensionContent|sig:UBLDocumentSignatures|sac:SignatureInformation|sbc:ReferencedSignatureID", "urn:oasis:names:specification:ubl:signature:Invoice");
            xml.UpdateChildContent("cbc:ProfileID", "reporting:1.0");
            xml.UpdateChildContent("cbc:ID", InvoiceNo);
            xml.UpdateChildContent("cbc:UUID", UniqueInvoiceID);
            xml.UpdateChildContent("cbc:IssueDate", InvoiceIssueDate);
            xml.UpdateChildContent("cbc:IssueTime", InvoiceIssueTime);
            xml.UpdateAttrAt("cbc:InvoiceTypeCode", true, "name", InvoiceTransactionCode);
            xml.UpdateChildContent("cbc:InvoiceTypeCode", InvoiceTypeCode.ToString());
            xml.UpdateChildContent("cbc:DocumentCurrencyCode", InvoiceCurrencyCode);
            xml.UpdateChildContent("cbc:TaxCurrencyCode", TaxCurrencyCode);
            // xml.UpdateChildContent("cbc:LineCountNumeric", varcnt_items)

            // if the invoice type is Credit Note then add this...
            if (InvoiceTypeCode == 381)
            {

                string InvDtl = ((char)148).ToString() + "" + ((char)148).ToString();
                xml.UpdateChildContent("cac:BillingReference|cac:InvoiceDocumentReference|cbc:ID", InvDtl);
            }
            else
            {

                string InvDtl = ((char)148).ToString() + "" + ((char)148).ToString();
                xml.UpdateChildContent("cac:BillingReference|cac:InvoiceDocumentReference|cbc:ID", InvDtl);

            }


            // Need to add optional fields - PO Number & Contract No.
            // xml.UpdateChildContent("cac:OrderReference|cbc:ID", PurchaseOrderNo)
            // xml.UpdateChildContent("cac:ContractDocumentReference|cbc:ID", ContractNo)

            xml.UpdateChildContent("cac:AdditionalDocumentReference|cbc:ID", "ICV");
            xml.UpdateChildContent("cac:AdditionalDocumentReference|cbc:UUID", InvoiceCounterValue);

            // Previous Invoice Hash
            xml.UpdateChildContent("cac:AdditionalDocumentReference[1]|cbc:ID", "PIH");
            xml.UpdateAttrAt("cac:AdditionalDocumentReference[1]|cac:Attachment|cbc:EmbeddedDocumentBinaryObject", true, "mimeCode", "text/plain");
            xml.UpdateChildContent("cac:AdditionalDocumentReference[1]|cac:Attachment|cbc:EmbeddedDocumentBinaryObject", PreviousInvoiceHash);
            xml.UpdateChildContent("cac:Signature|cbc:ID", "urn:oasis:names:specification:ubl:signature:Invoice");
            xml.UpdateChildContent("cac:Signature|cbc:SignatureMethod", "urn:oasis:names:specification:ubl:dsig:enveloped:xades");


            // --------------------------------------------------------------------------------------------------------------
            // Seller Address Details
            // --------------------------------------------------------------------------------------------------------------



            string SellerOtherIDType = aInvoiceData.Master.SellerOtherIdtype;
            string SellerOtherSellerID = aInvoiceData.Master.SellerOtherSellerId;
            string SellerAddressStreet = aInvoiceData.Master.SellerAddressStreet;
            string SellerAddlStreet = aInvoiceData.Master.SellerAdditionalStreet;
            string SellerBuildingNumber = aInvoiceData.Master.SellerBuildingNumber;
            string SellerAddlNumber = aInvoiceData.Master.SellerAdditionalNumber;
            string SellerCity = aInvoiceData.Master.SellerCity;
            string SellerPostalCode = aInvoiceData.Master.SellerPostalCode;
            string SellerProvince = aInvoiceData.Master.SellerProvince;
            string SellerDistrict = aInvoiceData.Master.SellerNeighborhood;
            string SellerCountryCode = aInvoiceData.Master.SellerCountryCode;
            string SellerVATNumber = aInvoiceData.Master.SellerVatnumber;
            string SellerName = aInvoiceData.Master.SellerName;
            string SellerNameInArabic = aInvoiceData.Master.SellerNameAr;
            string SellerAddressStreetInArabic = aInvoiceData.Master.SellerAddressStreetAr;
            string SellerCityInArabic = aInvoiceData.Master.SellerCityAr;
            string SellerDistrictInArabic = aInvoiceData.Master.SellerNeighborhoodAr;

            string SellerNameInBoth = "";
            SellerNameInBoth = SellerName;


            string SellerAddressStreetInBoth = "";
            SellerAddressStreetInBoth = SellerAddressStreet + " | " + SellerAddressStreetInArabic;

            string SellerCityInBoth = "";
            SellerCityInBoth = SellerCity + " | " + SellerCityInArabic;

            string SellerDistrictInBoth = "";
            SellerDistrictInBoth = SellerDistrict + " | " + SellerDistrictInArabic;


            xml.UpdateAttrAt("cac:AccountingSupplierParty|cac:Party|cac:PartyIdentification|cbc:ID", true, "schemeID", SellerOtherIDType);
            xml.UpdateChildContent("cac:AccountingSupplierParty|cac:Party|cac:PartyIdentification|cbc:ID", SellerOtherSellerID);
            xml.UpdateChildContent("cac:AccountingSupplierParty|cac:Party|cac:PostalAddress|cbc:StreetName", SellerAddressStreetInBoth);
            // xml.UpdateChildContent("cac:AccountingSupplierParty|cac:Party|cac:PostalAddress|cbc:AdditionalStreetName", SellerAddlStreet)
            xml.UpdateChildContent("cac:AccountingSupplierParty|cac:Party|cac:PostalAddress|cbc:BuildingNumber", SellerBuildingNumber);
            xml.UpdateChildContent("cac:AccountingSupplierParty|cac:Party|cac:PostalAddress|cbc:PlotIdentification", SellerAddlNumber);
            xml.UpdateChildContent("cac:AccountingSupplierParty|cac:Party|cac:PostalAddress|cbc:CitySubdivisionName", SellerDistrictInBoth);
            xml.UpdateChildContent("cac:AccountingSupplierParty|cac:Party|cac:PostalAddress|cbc:CityName", SellerCityInBoth);
            xml.UpdateChildContent("cac:AccountingSupplierParty|cac:Party|cac:PostalAddress|cbc:PostalZone", SellerPostalCode);
            xml.UpdateChildContent("cac:AccountingSupplierParty|cac:Party|cac:PostalAddress|cbc:CountrySubentity", SellerProvince);
            xml.UpdateChildContent("cac:AccountingSupplierParty|cac:Party|cac:PostalAddress|cac:Country|cbc:IdentificationCode", SellerCountryCode);
            xml.UpdateChildContent("cac:AccountingSupplierParty|cac:Party|cac:PartyTaxScheme|cbc:CompanyID", SellerVATNumber);
            xml.UpdateChildContent("cac:AccountingSupplierParty|cac:Party|cac:PartyTaxScheme|cac:TaxScheme|cbc:ID", "VAT");
            xml.UpdateChildContent("cac:AccountingSupplierParty|cac:Party|cac:PartyLegalEntity|cbc:RegistrationName", SellerNameInBoth);


            // --------------------------------------------------------------------------------------------------------------
            // Buyer Address Details
            // --------------------------------------------------------------------------------------------------------------


            string BuyerOtherIDType = aInvoiceData.Master.BuyerOtherIdtype;
            string BuyerOtherBuyerID = aInvoiceData.Master.BuyerOtherId;
            string BuyerAddressStreet = aInvoiceData.Master.BuyerAddressStreet;
            string BuyerAddlStreet = aInvoiceData.Master.BuyerAdditionalStreet;
            string BuyerBuildingNumber = aInvoiceData.Master.BuyerBuildingNumber;
            string BuyerAddlNumber = aInvoiceData.Master.BuyerAdditionalNumber;
            string BuyerCity = aInvoiceData.Master.BuyerCity;
            string BuyerPostalCode = aInvoiceData.Master.BuyerPostalCode;
            string BuyerProvince = aInvoiceData.Master.BuyerProvince;
            string BuyerDistrict = aInvoiceData.Master.BuyerNeighborhood;
            string BuyerCountryCode = aInvoiceData.Master.BuyerCountryCode;
            string BuyerVATNumber = aInvoiceData.Master.BuyerVatnumber;


            string BuyerNameInArabic = aInvoiceData.Master.BuyerNameAr;
            string BuyerAddressStreetInArabic = aInvoiceData.Master.BuyerAddressStreetAr;
            string BuyerCityInArabic = aInvoiceData.Master.BuyerCityAr;
            string BuyerDistrictInArabic = aInvoiceData.Master.BuyerNeighborhoodAr;

            string BuyerNameInBoth = "";
            BuyerNameInBoth = BuyerName + " | " + BuyerNameInArabic;

            string BuyerAddressStreetInBoth = "";
            BuyerAddressStreetInBoth = BuyerAddressStreet + " | " + BuyerAddressStreetInArabic;

            string BuyerCityInBoth = "";
            BuyerCityInBoth = BuyerCity + " | " + BuyerCityInArabic;

            string BuyerDistrictInBoth = "";
            BuyerDistrictInBoth = BuyerDistrict + " | " + BuyerDistrictInArabic;

            xml.UpdateAttrAt("cac:AccountingCustomerParty|cac:Party|cac:PartyIdentification|cbc:ID", true, "schemeID", BuyerOtherIDType);
            xml.UpdateChildContent("cac:AccountingCustomerParty|cac:Party|cac:PartyIdentification|cbc:ID", BuyerOtherBuyerID);
            xml.UpdateChildContent("cac:AccountingCustomerParty|cac:Party|cac:PostalAddress|cbc:StreetName", BuyerAddressStreetInBoth);
            // xml.UpdateChildContent("cac:AccountingCustomerParty|cac:Party|cac:PostalAddress|cbc:AdditionalStreetName", BuyerAddlStreet)
            xml.UpdateChildContent("cac:AccountingCustomerParty|cac:Party|cac:PostalAddress|cbc:BuildingNumber", BuyerBuildingNumber);
            xml.UpdateChildContent("cac:AccountingCustomerParty|cac:Party|cac:PostalAddress|cbc:PlotIdentification", BuyerAddlNumber);
            xml.UpdateChildContent("cac:AccountingCustomerParty|cac:Party|cac:PostalAddress|cbc:CitySubdivisionName", BuyerDistrictInBoth);
            xml.UpdateChildContent("cac:AccountingCustomerParty|cac:Party|cac:PostalAddress|cbc:CityName", BuyerCityInBoth);
            xml.UpdateChildContent("cac:AccountingCustomerParty|cac:Party|cac:PostalAddress|cbc:PostalZone", BuyerPostalCode);
            xml.UpdateChildContent("cac:AccountingCustomerParty|cac:Party|cac:PostalAddress|cbc:CountrySubentity", BuyerProvince);
            xml.UpdateChildContent("cac:AccountingCustomerParty|cac:Party|cac:PostalAddress|cac:Country|cbc:IdentificationCode", BuyerCountryCode);
            xml.UpdateChildContent("cac:AccountingCustomerParty|cac:Party|cac:PartyTaxScheme|cbc:CompanyID", BuyerVATNumber);
            xml.UpdateChildContent("cac:AccountingCustomerParty|cac:Party|cac:PartyTaxScheme|cac:TaxScheme|cbc:ID", "VAT");
            xml.UpdateChildContent("cac:AccountingCustomerParty|cac:Party|cac:PartyLegalEntity|cbc:RegistrationName", BuyerNameInBoth);


            // --------------------------------------------------------------------------------------------------------------
            // Payment Details & Supply Details
            // --------------------------------------------------------------------------------------------------------------

            string SupplyDate = aInvoiceData.Master.SupplyDate.HasValue
                            ? aInvoiceData.Master.SupplyDate.Value.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)
                            : string.Empty; // or null if you prefer

            string SupplyEndDate = aInvoiceData.Master.SupplyEndDate.ToString();
            string PaymentMeansTypeCode = aInvoiceData.Master.PaymentMeansTypeCode.ToString();
            string PaymentTerms = aInvoiceData.Master.PaymentTerms;
            string PaymentAccountID = aInvoiceData.Master.ContractId;
            string CreditNoteReason = aInvoiceData.Master.CreditNoteReason;

            xml.UpdateChildContent("cac:Delivery|cbc:ActualDeliveryDate", SupplyDate);
            // xml.UpdateChildContent("cac:Delivery|cbc:LatestDeliveryDate", SupplyEndDate)
            xml.UpdateChildContent("cac:PaymentMeans|cbc:PaymentMeansCode", PaymentMeansTypeCode);
            // xml.UpdateChildContent("cac:PaymentMeans|cac:PayeeFinancialAccount|cbc:PaymentNote", PaymentTerms)
            // xml.UpdateChildContent("cac:PaymentMeans|cac:PayeeFinancialAccount|cbc:ID", PaymentAccountID)

            string RetItem = ((char)147).ToString() + CreditNoteReason + ((char)148).ToString();
            xml.UpdateChildContent("cac:PaymentMeans|cbc:InstructionNote", RetItem);


            // --------------------------------------------------------------------------------------------------------------
            // Allowance Charges -  Allowance (discount) at document level
            // --------------------------------------------------------------------------------------------------------------


            // xml.UpdateChildContent("cac:AllowanceCharge|cbc:ChargeIndicator", "false")
            // xml.UpdateAttrAt("cac:AllowanceCharge|cbc:Amount", True, "currencyID", InvoiceCurrencyCode)
            // xml.UpdateChildContent("cac:AllowanceCharge|cbc:Amount", "0.00")
            // xml.UpdateChildContent("cac:AllowanceCharge|cac:TaxCategory|cbc:ID", "S")
            // xml.UpdateChildContent("cac:AllowanceCharge|cac:TaxCategory|cbc:Percent", "15")
            // xml.UpdateChildContent("cac:AllowanceCharge|cac:TaxCategory|cac:TaxScheme|cbc:ID", "VAT")

            for (int i = 0; i <= aInvoiceData.AllowanceCharges.Count - 1; i++)
            {

                xml.UpdateChildContent("cac:AllowanceCharge[" + i + "]|cbc:ChargeIndicator", "false");
                xml.UpdateAttrAt("cac:AllowanceCharge[" + i + "]|cbc:Amount", true, "currencyID", InvoiceCurrencyCode);
                // xml.UpdateChildContent("cac:AllowanceCharge[" + i + "]|cbc:Amount", aInvoiceData.AllowanceCharges.GetRowCellValue(i, "LineAllowanceCharges"));
                if (aInvoiceData.AllowanceCharges != null && i < aInvoiceData.AllowanceCharges.Count)
                {
                    var allowanceCharge = aInvoiceData.AllowanceCharges[i];
                    xml.UpdateChildContent("cac:AllowanceCharge[" + i + "]|cbc:Amount", allowanceCharge.LineAllowanceChargesOc?.ToString("0.00") ?? "0.00");
                }
                xml.UpdateAttrAt("cac:AllowanceCharge|cac:TaxCategory|cbc:ID", true, "schemeID", "UN/ECE 5305");
                xml.UpdateAttrAt("cac:AllowanceCharge|cac:TaxCategory|cbc:ID", true, "schemeAgencyID", "6");

                //xml.UpdateChildContent("cac:AllowanceCharge[" + i + "]|cac:TaxCategory|cbc:ID", gvInvoiceAllowanceCharges.GetRowCellValue(i, "TaxCodeInZatca"));
                if (aInvoiceData.AllowanceCharges != null && i < aInvoiceData.AllowanceCharges.Count)
                {
                    var allowanceCharge = aInvoiceData.AllowanceCharges[i];
                    xml.UpdateChildContent("cac:AllowanceCharge[" + i + "]|cbc:Amount", allowanceCharge.TaxCodeInZatca);

                    var allowanceCharge1 = aInvoiceData.AllowanceCharges[i];
                    xml.UpdateChildContent("cac:AllowanceCharge[" + i + "]|cbc:Amount", allowanceCharge1.TaxRateIn100.ToString());
                }
                //xml.UpdateChildContent("cac:AllowanceCharge[" + i + "]|cac:TaxCategory|cbc:Percent", gvInvoiceAllowanceCharges.GetRowCellValue(i, "TaxRateIn100"));

                xml.UpdateAttrAt("cac:AllowanceCharge|cac:TaxCategory|cac:TaxScheme|cbc:ID", true, "schemeID", "UN/ECE 5153");
                xml.UpdateAttrAt("cac:AllowanceCharge|cac:TaxCategory|cac:TaxScheme|cbc:ID", true, "schemeAgencyID", "6");

                xml.UpdateChildContent("cac:AllowanceCharge[" + i + "]|cac:TaxCategory|cac:TaxScheme|cbc:ID", "VAT");


            }


            // --------------------------------------------------------------------------------------------------------------
            // VAT Breakdown - 
            // --------------------------------------------------------------------------------------------------------------


            for (int i = 0; i <= aInvoiceData.Totals.Count - 1; i++)
            {
                if (aInvoiceData.Totals != null && i < aInvoiceData.Totals.Count)
                {
                    xml.UpdateAttrAt("cac:TaxTotal[" + aInvoiceData.VatBreakdowns.Count + "]|cbc:TaxAmount", true, "currencyID", InvoiceCurrencyCode);

                    var total = aInvoiceData.Totals[i];
                    xml.UpdateChildContent("cac:TaxTotal[" + aInvoiceData.VatBreakdowns.Count + "]|cbc:TaxAmount", total.TaxAmountByTaxRate?.ToString("0.00") ?? "0.00");
                }
            }



            for (int i = 0; i < aInvoiceData.VatBreakdowns.Count; i++)
            {
                var vatBreakdown = aInvoiceData.VatBreakdowns[i];

                // Example testing values (replace with your real values)
                vatBreakdown.TotalExclusiveAmountOc = 1.00m; // taxable base
                vatBreakdown.TaxRateIn100 = 15.00m;        // 15% VAT
                decimal taxable = vatBreakdown.TotalExclusiveAmountOc ?? 0m;
                decimal vatRate = (vatBreakdown.TaxRateIn100 ?? 0m) / 100m;
                decimal taxAmount = Math.Round(taxable * vatRate, 2);

                // --- Taxable Amount ---
                xml.UpdateAttrAt($"cac:TaxTotal|cac:TaxSubtotal[{i}]|cbc:TaxableAmount", true, "currencyID", InvoiceCurrencyCode);
                xml.UpdateChildContent(
                    $"cac:TaxTotal|cac:TaxSubtotal[{i}]|cbc:TaxableAmount",
                    taxable.ToString("0.00", CultureInfo.InvariantCulture)
                );

                // --- Tax Amount ---
                xml.UpdateAttrAt($"cac:TaxTotal|cac:TaxSubtotal[{i}]|cbc:TaxAmount", true, "currencyID", InvoiceCurrencyCode);
                xml.UpdateChildContent(
                    $"cac:TaxTotal|cac:TaxSubtotal[{i}]|cbc:TaxAmount",
                    taxAmount.ToString("0.00", CultureInfo.InvariantCulture)
                );

                // Ensure i starts from 1 when looping over Subtotals
                xml.UpdateAttrAt(
                    $"cac:TaxTotal|cac:TaxSubtotal[{i}]|cbc:TaxAmount",
                    true,
                    "currencyID",
                    InvoiceCurrencyCode
                );
                var VatBreakdowns = aInvoiceData.VatBreakdowns[i];
                xml.UpdateChildContent(
      $"cac:TaxTotal|cac:TaxSubtotal[{i + 1}]|cbc:TaxAmount",
      VatBreakdowns.TaxAmountByTaxRateOc.GetValueOrDefault().ToString("0.00", CultureInfo.InvariantCulture)
  );


                // --- Tax Category ID (UNCL5305: S=Standard, Z=Zero, E=Exempt, O=Out of scope) ---
                xml.UpdateAttrAt($"cac:TaxTotal|cac:TaxSubtotal[{i}]|cac:TaxCategory|cbc:ID", true, "schemeID", "UN/ECE 5305");
                xml.UpdateAttrAt($"cac:TaxTotal|cac:TaxSubtotal[{i}]|cac:TaxCategory|cbc:ID", true, "schemeAgencyID", "6");
                xml.UpdateChildContent(
                    $"cac:TaxTotal|cac:TaxSubtotal[{i}]|cac:TaxCategory|cbc:ID",
                    vatBreakdown.TaxCodeInZatca
                );

                // --- Tax Rate Percent ---
                xml.UpdateChildContent(
                    $"cac:TaxTotal|cac:TaxSubtotal[{i}]|cac:TaxCategory|cbc:Percent",
                    vatBreakdown.TaxRateIn100?.ToString("0.00", CultureInfo.InvariantCulture) ?? "0.00"
                );

                // --- Tax Scheme ---
                xml.UpdateAttrAt($"cac:TaxTotal|cac:TaxSubtotal[{i}]|cac:TaxCategory|cac:TaxScheme|cbc:ID", true, "schemeID", "UN/ECE 5153");
                xml.UpdateAttrAt($"cac:TaxTotal|cac:TaxSubtotal[{i}]|cac:TaxCategory|cac:TaxScheme|cbc:ID", true, "schemeAgencyID", "6");
                xml.UpdateChildContent($"cac:TaxTotal|cac:TaxSubtotal[{i}]|cac:TaxCategory|cac:TaxScheme|cbc:ID", "VAT");

                // --- Tax Exemption Handling ---
                if (vatBreakdown.TaxCodeInZatca == "Z" || vatBreakdown.TaxCodeInZatca == "E" || vatBreakdown.TaxCodeInZatca == "O")
                {
                    xml.UpdateChildContent(
                        $"cac:TaxTotal|cac:TaxSubtotal[{i}]|cac:TaxCategory|cbc:TaxExemptionReasonCode",
                        vatBreakdown.TaxExemptionReasonCode
                    );
                    xml.UpdateChildContent(
                        $"cac:TaxTotal|cac:TaxSubtotal[{i}]|cac:TaxCategory|cbc:TaxExemptionReason",
                        vatBreakdown.TaxExemptionReason
                    );
                }

            }


            // --------------------------------------------------------------------------------------------------------------
            // Invoice Level Totals - Invoice total amounts 
            // --------------------------------------------------------------------------------------------------------------

            for (int i = 0; i <= aInvoiceData.Totals.Count - 1; i++)
            {
                xml.UpdateAttrAt("cac:LegalMonetaryTotal|cbc:LineExtensionAmount", true, "currencyID", InvoiceCurrencyCode);
                var total = aInvoiceData.Totals[i];
                xml.UpdateChildContent("cac:LegalMonetaryTotal|cbc:LineExtensionAmount", total.TotalExtentionAmountOc?.ToString("0.00") ?? "0.00");

                // FIXED LINE:
                xml.UpdateAttrAt("cac:LegalMonetaryTotal|cbc:TaxExclusiveAmount", true, "currencyID", InvoiceCurrencyCode);
                xml.UpdateChildContent("cac:LegalMonetaryTotal|cbc:TaxExclusiveAmount", total.TotalExclusiveAmountOc?.ToString("0.00") ?? "0.00");

                //  xml.UpdateChildContent("cac:LegalMonetaryTotal|cbc:TaxExclusiveAmount", gvInvoiceTotals.GetRowCellValue(i, "TotalExclusiveAmount"));

                xml.UpdateAttrAt("cac:LegalMonetaryTotal|cbc:TaxInclusiveAmount", true, "currencyID", InvoiceCurrencyCode);
                xml.UpdateChildContent("cac:LegalMonetaryTotal|cbc:TaxInclusiveAmount", total.TaxInclusiveAmountOc?.ToString("0.00") ?? "0.00");

                //xml.UpdateChildContent("cac:LegalMonetaryTotal|cbc:TaxInclusiveAmount", gvInvoiceTotals.GetRowCellValue(i, "TaxInclusiveAmount"));

                xml.UpdateAttrAt("cac:LegalMonetaryTotal|cbc:AllowanceTotalAmount", true, "currencyID", InvoiceCurrencyCode);
                xml.UpdateChildContent("cac:LegalMonetaryTotal|cbc:AllowanceTotalAmount", total.TotalAllowanceChargesOc?.ToString("0.00") ?? "0.00");

                // xml.UpdateChildContent("cac:LegalMonetaryTotal|cbc:AllowanceTotalAmount", gvInvoiceTotals.GetRowCellValue(i, "TotalAllowanceCharges"));

                decimal PrepaidAmount = GetInvoicePrepaidAdjustmentAmount(aInvoiceData.Master.InvoiceNo);
                decimal TaxInclusiveAmount = total.TaxInclusiveAmountOc ?? 0;
                decimal PayableAmount;
                PayableAmount = TaxInclusiveAmount - PrepaidAmount;

                xml.UpdateAttrAt("cac:LegalMonetaryTotal|cbc:PrepaidAmount", true, "currencyID", InvoiceCurrencyCode);

                xml.UpdateChildContent("cac:LegalMonetaryTotal|cbc:PrepaidAmount", PrepaidAmount.ToString("0.00"));

                xml.UpdateAttrAt("cac:LegalMonetaryTotal|cbc:PayableAmount", true, "currencyID", InvoiceCurrencyCode);

                xml.UpdateChildContent("cac:LegalMonetaryTotal|cbc:PayableAmount", PayableAmount.ToString("0.00"));

                xml.UpdateAttrAt("cac:TaxTotal|cbc:TaxAmount", true, "currencyID", InvoiceCurrencyCode);
                xml.UpdateChildContent("cac:TaxTotal|cbc:TaxAmount", total.TaxAmountByTaxRateOc?.ToString("0.00") ?? "0.00");
            }




            // '--------------------------------------------------------------------------------------------------------------
            // 'Invoice Line Items  
            // '--------------------------------------------------------------------------------------------------------------

            for (int i = 0; i <= aInvoiceData.Lines.Count - 1; i++)
            {
                var line = aInvoiceData.Lines[i];

                xml.UpdateChildContent("cac:InvoiceLine[" + i + "]|cbc:ID", line.InvoiceChildSlNo?.ToString());
                xml.UpdateAttrAt("cac:InvoiceLine[" + i + "]|cbc:InvoicedQuantity", true, "unitCode", line.UnitDesc);
                xml.UpdateChildContent("cac:InvoiceLine[" + i + "]|cbc:InvoicedQuantity", line.QuantityInvoiced?.ToString());
                xml.UpdateAttrAt("cac:InvoiceLine[" + i + "]|cbc:LineExtensionAmount", true, "currencyID", InvoiceCurrencyCode);
                xml.UpdateChildContent("cac:InvoiceLine[" + i + "]|cbc:LineExtensionAmount", line.LineTaxExclusiveAmountOc?.ToString("0.00"));

                xml.UpdateChildContent("cac:InvoiceLine[" + i + "]|cac:AllowanceCharge|cbc:ChargeIndicator", "false");
                xml.UpdateChildContent("cac:InvoiceLine[" + i + "]|cac:AllowanceCharge|cbc:AllowanceChargeReasonCode", "95");
                xml.UpdateChildContent("cac:InvoiceLine[" + i + "]|cac:AllowanceCharge|cbc:AllowanceChargeReason", "Discount");
                xml.UpdateAttrAt("cac:InvoiceLine[" + i + "]|cac:AllowanceCharge|cbc:Amount", true, "currencyID", InvoiceCurrencyCode);
                xml.UpdateChildContent("cac:InvoiceLine[" + i + "]|cac:AllowanceCharge|cbc:Amount", line.DiscountOc.ToString("0.0000"));

                xml.UpdateAttrAt("cac:InvoiceLine[" + i + "]|cac:TaxTotal|cbc:TaxAmount", true, "currencyID", InvoiceCurrencyCode);
                xml.UpdateChildContent("cac:InvoiceLine[" + i + "]|cac:TaxTotal|cbc:TaxAmount", line.LineTaxAmountOc?.ToString("0.00"));

                xml.UpdateAttrAt("cac:InvoiceLine[" + i + "]|cac:TaxTotal|cbc:RoundingAmount", true, "currencyID", InvoiceCurrencyCode);
                xml.UpdateChildContent("cac:InvoiceLine[" + i + "]|cac:TaxTotal|cbc:RoundingAmount", line.InvoiceLineRoundingAmountOc?.ToString("0.00"));

                xml.UpdateChildContent("cac:InvoiceLine[" + i + "]|cac:Item|cbc:Name", line.ItemDescriptionInBoth);

                xml.UpdateChildContent("cac:InvoiceLine[" + i + "]|cac:Item|cac:ClassifiedTaxCategory|cbc:ID", line.TaxCodeInZatca);
                xml.UpdateChildContent("cac:InvoiceLine[" + i + "]|cac:Item|cac:ClassifiedTaxCategory|cbc:Percent", line.TaxRateIn100?.ToString("0.00"));
                xml.UpdateChildContent("cac:InvoiceLine[" + i + "]|cac:Item|cac:ClassifiedTaxCategory|cac:TaxScheme|cbc:ID", "VAT");

                xml.UpdateAttrAt("cac:InvoiceLine[" + i + "]|cac:Price|cbc:PriceAmount", true, "currencyID", InvoiceCurrencyCode);
                xml.UpdateChildContent("cac:InvoiceLine[" + i + "]|cac:Price|cbc:PriceAmount", line.UnitRateOc.ToString("0.00"));
                xml.UpdateChildContent("cac:InvoiceLine[" + i + "]|cac:Price|cbc:BaseQuantity", "1");
            }


            int lastNoToAdd = aInvoiceData.Lines.Count - 1;

            for (int i = 0; i <= aInvoiceData.PrepaidAdjustments.Count - 1; i++)
            {
                int j = i + lastNoToAdd + 1;

                // Retrieve values directly from PrepaidAdjustments
                var prepaidAdjustment = aInvoiceData.PrepaidAdjustments[i];
                var sequence = prepaidAdjustment.Sequence?.ToString();
                var currencyCode = prepaidAdjustment.CurrencyMasterCode;
                var prepaidInvoiceNo = prepaidAdjustment.PrepaidInvoiceNo;
                var submittedInvoiceUUID = prepaidAdjustment.SubmittedInvoiceUuid;
                var prepaidInvoiceDate = prepaidAdjustment.PrepaidInvoiceDate;
                var prepaidInvoiceTime = prepaidAdjustment.PrepaidInvoiceTime;
                var adjustedAmount = prepaidAdjustment.AdjustedAmountInFc?.ToString("0.00");
                var adjustedTaxAmount = prepaidAdjustment.AdjustedTaxAmountInFc?.ToString("0.00");
                var taxCode = prepaidAdjustment.TaxCodeInZatca;
                var taxRate = prepaidAdjustment.TaxRateIn100?.ToString("0.00");

                // Check if necessary fields are empty
                if (string.IsNullOrEmpty(sequence) || string.IsNullOrEmpty(currencyCode) || string.IsNullOrEmpty(prepaidInvoiceNo) || string.IsNullOrEmpty(prepaidInvoiceDate) || string.IsNullOrEmpty(prepaidInvoiceTime))
                    continue;

                // Start the InvoiceLine element
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]", "");

                // ID
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]|cbc:ID", sequence);

                // Invoiced Quantity
                xml.UpdateAttrAt("cac:InvoiceLine[" + j + "]|cbc:InvoicedQuantity", true, "unitCode", "PCE");
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]|cbc:InvoicedQuantity", "0.000000");

                // Line Extension Amount
                xml.UpdateAttrAt("cac:InvoiceLine[" + j + "]|cbc:LineExtensionAmount", true, "currencyID", currencyCode);
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]|cbc:LineExtensionAmount", "0.00");

                // Document Reference
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]|cac:DocumentReference|cbc:ID", prepaidInvoiceNo);
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]|cac:DocumentReference|cbc:UUID", submittedInvoiceUUID);
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]|cac:DocumentReference|cbc:IssueDate", prepaidInvoiceDate);
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]|cac:DocumentReference|cbc:IssueTime", prepaidInvoiceTime);
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]|cac:DocumentReference|cbc:DocumentTypeCode", "386");

                // Tax Total
                xml.UpdateAttrAt("cac:InvoiceLine[" + j + "]|cac:TaxTotal|cbc:TaxAmount", true, "currencyID", currencyCode);
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]|cac:TaxTotal|cbc:TaxAmount", "0");
                xml.UpdateAttrAt("cac:InvoiceLine[" + j + "]|cac:TaxTotal|cbc:RoundingAmount", true, "currencyID", currencyCode);
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]|cac:TaxTotal|cbc:RoundingAmount", "0");
                xml.UpdateAttrAt("cac:InvoiceLine[" + j + "]|cac:TaxTotal|cac:TaxSubtotal|cbc:TaxableAmount", true, "currencyID", currencyCode);
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]|cac:TaxTotal|cac:TaxSubtotal|cbc:TaxableAmount", adjustedAmount);
                xml.UpdateAttrAt("cac:InvoiceLine[" + j + "]|cac:TaxTotal|cac:TaxSubtotal|cbc:TaxAmount", true, "currencyID", currencyCode);
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]|cac:TaxTotal|cac:TaxSubtotal|cbc:TaxAmount", adjustedTaxAmount);
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]|cac:TaxTotal|cac:TaxSubtotal|cac:TaxCategory|cbc:ID", taxCode);
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]|cac:TaxTotal|cac:TaxSubtotal|cac:TaxCategory|cbc:Percent", taxRate);
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]|cac:TaxTotal|cac:TaxSubtotal|cac:TaxCategory|cac:TaxScheme|cbc:ID", "VAT");

                // Item Details
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]|cac:Item|cbc:Name", "Prepaid Advance Amount | المبلغ المدفوع مقدمًا");
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]|cac:Item|cac:ClassifiedTaxCategory|cbc:ID", taxCode);
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]|cac:Item|cac:ClassifiedTaxCategory|cbc:Percent", taxRate);
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]|cac:Item|cac:ClassifiedTaxCategory|cac:TaxScheme|cbc:ID", "VAT");

                // Price Amount
                xml.UpdateAttrAt("cac:InvoiceLine[" + j + "]|cac:Price|cbc:PriceAmount", true, "currencyID", currencyCode);
                xml.UpdateChildContent("cac:InvoiceLine[" + j + "]|cac:Price|cbc:PriceAmount", "0.0000");
            }

            // '--------------------------------------------------------------------------------------------------------------
            // 'UBL Extension Codes 
            // '--------------------------------------------------------------------------------------------------------------

            Chilkat.XmlDSigGen gen = new Chilkat.XmlDSigGen();

            gen.SigLocation = "Invoice|ext:UBLExtensions|ext:UBLExtension|ext:ExtensionContent|sig:UBLDocumentSignatures|sac:SignatureInformation";
            gen.SigLocationMod = 0;
            gen.SigId = "signature";
            gen.SigNamespacePrefix = "ds";
            gen.SigNamespaceUri = "http://www.w3.org/2000/09/xmldsig#";
            gen.SignedInfoCanonAlg = "C14N_11";
            gen.SignedInfoDigestMethod = "sha256";

            // Create an Object to be added to the Signature.

            // Commented by Saju
            // -<xadesQualifyingProperties Target = "signature" xmlns: xades = "http://uri.etsi.org/01903/v1.3.2#" >
            // -<xadesSignedProperties Id = "xadesSignedProperties" >
            // -<xadesSignedSignatureProperties>
            // <xades:SigningTime> 2022 - 11 - 16T05:20:37Z</xades:SigningTime>
            // -<xadesSigningCertificate>
            // -<xadescert>
            // -<xadesCertDigest>
            // <ds:DigestMethod Algorithm = "http://www.w3.org/2001/04/xmlenc#sha256" />
            // (Added by Chilkat)               <ds:DigestValue> lZUwCVs9AAVwENypLvUYTO1MVlyfOe1Ba2C0fglq2So =</ds:DigestValue>
            // </xadesCertDigest>
            // -<xadesIssuerSerial>
            // <ds:X509IssuerName> CN = TSZEINVOICE - SubCA - 1, DC = extgazt, DC = gov, DC = local</ds: X509IssuerName>
            // <ds:X509SerialNumber> 2475382876776561391517206651645660279462721580</ds:X509SerialNumber>
            // </xadesIssuerSerial>
            // </xadescert>
            // </xadesSigningCertificate>
            // </xadesSignedSignatureProperties>
            // </xadesSignedProperties>
            // </xadesQualifyingProperties>

            Chilkat.Xml object1 = new Chilkat.Xml();
            object1.Tag = "xades:QualifyingProperties";
            object1.AddAttribute("xmlns:xades", "http://uri.etsi.org/01903/v1.3.2#");
            object1.AddAttribute("Target", "signature");
            object1.UpdateAttrAt("xades:SignedProperties", true, "Id", "xadesSignedProperties");
            object1.UpdateChildContent("xades:SignedProperties|xades:SignedSignatureProperties|xades:SigningTime", "TO BE GENERATED BY CHILKAT");
            object1.UpdateAttrAt("xades:SignedProperties|xades:SignedSignatureProperties|xades:SigningCertificate|xades:Cert|xades:CertDigest|ds:DigestMethod", true, "Algorithm", "http://www.w3.org/2001/04/xmlenc#sha256");
            object1.UpdateChildContent("xades:SignedProperties|xades:SignedSignatureProperties|xades:SigningCertificate|xades:Cert|xades:CertDigest|ds:DigestValue", "TO BE GENERATED BY CHILKAT");

            object1.UpdateChildContent("xades:SignedProperties|xades:SignedSignatureProperties|xades:SigningCertificate|xades:Cert|xades:IssuerSerial|ds:X509IssuerName", "TO BE GENERATED BY CHILKAT"); // "CN=TSZEINVOICE-SubCA-1, DC=extgazt, DC=gov, DC=local")
            object1.UpdateChildContent("xades:SignedProperties|xades:SignedSignatureProperties|xades:SigningCertificate|xades:Cert|xades:IssuerSerial|ds:X509SerialNumber", "TO BE GENERATED BY CHILKAT"); // "2475382886904809774818644480820936050208702411")

            gen.AddObject("", object1.GetXml(), "", "");


            // -------- Reference 1 --------
            Chilkat.Xml xml1 = new Chilkat.Xml();
            xml1.Tag = "ds:Transforms";
            xml1.UpdateAttrAt("ds:Transform", true, "Algorithm", "http://www.w3.org/TR/1999/REC-xpath-19991116");
            xml1.UpdateChildContent("ds:Transform|ds:XPath", "not(//ancestor-or-self::ext:UBLExtensions)");
            xml1.UpdateAttrAt("ds:Transform[1]", true, "Algorithm", "http://www.w3.org/TR/1999/REC-xpath-19991116");
            xml1.UpdateChildContent("ds:Transform[1]|ds:XPath", "not(//ancestor-or-self::cac:Signature)");
            xml1.UpdateAttrAt("ds:Transform[2]", true, "Algorithm", "http://www.w3.org/TR/1999/REC-xpath-19991116");
            xml1.UpdateChildContent("ds:Transform[2]|ds:XPath", "not(//ancestor-or-self::cac:AdditionalDocumentReference[cbc:ID='QR'])");
            xml1.UpdateAttrAt("ds:Transform[3]", true, "Algorithm", "http://www.w3.org/2006/12/xml-c14n11");

            gen.AddSameDocRef2("", "sha256", xml1, "");
            gen.SetRefIdAttr("", "invoiceSignedData");

            // -------- Reference 2 --------
            gen.AddObjectRef("xadesSignedProperties", "sha256", "", "", "http://www.w3.org/2000/09/xmldsig#SignatureProperties");

            // ============================================================================================
            // New Update
            // ============================================================================================
            // Alternatively, if your certificate and private key are in separate PEM files, do this:
            Chilkat.Cert cert = new Chilkat.Cert();

            GetEGSUnitDetails(1);
            success = cert.LoadFromBase64(gProductionPEM);

            if ((success != true))
            {
                //MessageBox.Show(cert.LastErrorText);
                //return;
            }


            // Load the private key.
            Chilkat.PrivateKey privKey = new Chilkat.PrivateKey();

            success = privKey.LoadPem(gPrivateKey);

            if ((success != true))
            {
                //MessageBox.Show(privKey.LastErrorText);
                //return;
            }

            // Associate the private key with the certificate.
            success = cert.SetPrivateKey(privKey);
            if ((success != true))
            {
                //MessageBox.Show(cert.LastErrorText);
                //return;
            }
            // =====================================================================================
            gen.SetX509Cert(cert, true);

            gen.KeyInfoType = "X509Data";
            gen.X509Type = "Certificate";

            // Load XML to be signed...
            Chilkat.StringBuilder sbXml = new Chilkat.StringBuilder();
            xml.GetXmlSb(sbXml);
            gen.Behaviors = "IndentedSignature,TransformSignatureXPath,ZATCA";

            // Sign the XML...
            success = gen.CreateXmlDSigSb(sbXml);
            if ((success != true))
            {
                //MessageBox.Show("Failed to Sign XML UBL 2.1", "Error XML", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //MessageBox.Show(gen.LastErrorText);
                //return;
            }

            // Save the signed XML to a file.
            success = sbXml.WriteFile(appPath + @"\SignedXML\signedXmlResult1.xml", "utf-8", false);

            string folderPath = Path.Combine(appPath, "SignedXML");

            // Ensure the folder exists
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Now write the file safely
            string filePath = Path.Combine(folderPath, "signedXmlResult1.xml");
            success = sbXml.WriteFile(filePath, "utf-8", false);

            //MessageBox.Show("Failed to Create XML UBL 2.1", "Error XML", MessageBoxButtons.OK, MessageBoxIcon.Error);

            Debug.WriteLine("Signed XML file generated at: " + appPath + @"\SignedXML\signedXmlResult1.xml");

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = appPath + @"\SignedXML\signedXmlResult1.xml",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        private string XMLFileNameAsPerZatca;
        [HttpPost]
        public async Task<IActionResult> SignXML(InvoiceData aInvoiceData,string invoiceNo)
        {
            try
            {
                Chilkat.BinData bdTlv = new Chilkat.BinData();

                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    bool success;
                    string defaultCompanyString = HttpContext.Session.GetString("DefaultcompanyID") ?? "";
                    byte defaultCompanyByte = 0; // or any default value you want

                    if (!string.IsNullOrEmpty(defaultCompanyString))
                    {
                        // Safest way (avoids exceptions):
                        byte.TryParse(defaultCompanyString, out defaultCompanyByte);
                        // Now defaultCompanyByte holds the parsed value, or 0 if parsing failed.
                    }

                    // Now use defaultCompanyByte as needed


                    byte companyId = defaultCompanyByte;
                    string sellerName = aInvoiceData.Master.SellerName;
                    string sellerNameInArabic = aInvoiceData.Master.SellerNameAr;
                    string sellerNameInBoth = sellerName;

                    string vatNumber = aInvoiceData.Master.SellerVatnumber;
                    string timeStamp = aInvoiceData.Master.InvoiceDateWtTime.ToString();

                    string invoiceTotal = "0.00";
                    string vatTotal = "0.00";

                    if (aInvoiceData.Totals != null && aInvoiceData.Totals.Count > 0)
                    {
                        for (int i = 0; i < aInvoiceData.Totals.Count; i++)
                        {
                            invoiceTotal = aInvoiceData.Totals[i].TaxInclusiveAmount?.ToString("0.00") ?? "0.00";
                            vatTotal = aInvoiceData.Totals[i].TaxAmountByTaxRate?.ToString("0.00") ?? "0.00";
                        }
                    }

                    // TLV encoding setup
                   
                    string charset = "utf-8";
                    int tag = 1;

                    bdTlv.AppendByte(tag++);
                    bdTlv.AppendCountedString(1, false, sellerNameInBoth, charset);
                    bdTlv.AppendByte(tag++);
                    bdTlv.AppendCountedString(1, false, vatNumber, charset);
                    bdTlv.AppendByte(tag++);
                    bdTlv.AppendCountedString(1, false, timeStamp, charset);
                    bdTlv.AppendByte(tag++);
                    bdTlv.AppendCountedString(1, false, invoiceTotal, charset);
                    bdTlv.AppendByte(tag++);
                    bdTlv.AppendCountedString(1, false, vatTotal, charset);

                    // Load signed XML
                    string signedXmlFilePath = appPath + "\\SignedXML\\signedXmlResult1.xml";
                    Chilkat.Xml xmlSigned = new Chilkat.Xml();
                    success = xmlSigned.LoadXmlFile(signedXmlFilePath);
                    if (!success)
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, new
                        {
                            success = false,
                            message = "Failed to open the previously signed XML."
                        });
                    }

                    // Extract DigestValue
                    Chilkat.StringBuilder sbDigestValue = new Chilkat.StringBuilder();
                    success = xmlSigned.GetChildContentSb("ext:UBLExtensions|ext:UBLExtension|ext:ExtensionContent|sig:UBLDocumentSignatures|sac:SignatureInformation|ds:Signature|ds:SignedInfo|ds:Reference[0]|ds:DigestValue", sbDigestValue);
                    if (!success)
                    {
                        return BadRequest(new { success = false, message = "Failed to get DigestValue from signed XML." });
                    }

                    tag = 6;
                    bdTlv.AppendByte(tag);
                    bdTlv.AppendByte(sbDigestValue.Length);
                    bdTlv.AppendSb(sbDigestValue, "utf-8");

                    int CompanyId = 1;
                    bool azureEnabled = GetAzureStatus(CompanyId);

                    string signedXmlPath = Path.Combine(appPath, "SignedXML", "signedXmlResult1.xml");
                    string signedXmlContent;

                    if (azureEnabled)
                    {
                        signedXmlContent = await DownloadFromAzureAsync("signedXmlResult1.xml", CompanyId);
                    }
                    else
                    {
                        signedXmlContent = await System.IO.File.ReadAllTextAsync(signedXmlPath);
                    }


                    success = xmlSigned.LoadXml(signedXmlContent);

                    if (!success)
                    {
                        //MessageBox.Show("XML Previous Signed Failed to Open..", "Error XML UBL 2.1", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        //return;
                    }


                    // Extract SignatureValue
                    Chilkat.StringBuilder sbSignatureValue = new Chilkat.StringBuilder();
                    success = xmlSigned.GetChildContentSb("ext:UBLExtensions|ext:UBLExtension|ext:ExtensionContent|sig:UBLDocumentSignatures|sac:SignatureInformation|ds:Signature|ds:SignatureValue", sbSignatureValue);
                    if (!success)
                    {
                        return BadRequest(new { success = false, message = "Failed to get SignatureValue from signed XML." });
                    }

                    tag = 7;
                    bdTlv.AppendByte(tag);
                    bdTlv.AppendByte(sbSignatureValue.Length);
                    bdTlv.AppendSb(sbSignatureValue, "utf-8");

                    // Extract certificate
                    string x509Certificate = xmlSigned.GetChildContent("ext:UBLExtensions|ext:UBLExtension|ext:ExtensionContent|sig:UBLDocumentSignatures|sac:SignatureInformation|ds:Signature|ds:KeyInfo|ds:X509Data|ds:X509Certificate");
                    if (!xmlSigned.LastMethodSuccess)
                    {
                        return BadRequest(new { success = false, message = "Failed to get X509Certificate from signed XML." });
                    }

                    Chilkat.Cert cert = new Chilkat.Cert();
                    success = cert.SetFromEncoded(x509Certificate);
                    if (!success)
                    {
                        return BadRequest(new { success = false, message = "Failed to load signing certificate from base64." });
                    }

                    Chilkat.BinData bdPubKey = new Chilkat.BinData();
                    success = cert.GetPubKeyDer(true, bdPubKey);
                    if (!success)
                    {
                        return BadRequest(new { success = false, message = "Failed to get certificate public key." });
                    }

                    tag = 8;
                    bdTlv.AppendByte(tag);
                    bdTlv.AppendByte(bdPubKey.NumBytes);
                    bdTlv.AppendBd(bdPubKey);

                    Chilkat.BinData bdCertSig = new Chilkat.BinData();
                    success = cert.GetSignature(bdCertSig);
                    if (!success)
                    {
                        return BadRequest(new { success = false, message = "Failed to get certificate signature." });
                    }

                    tag = 9;
                    bdTlv.AppendByte(tag);
                    bdTlv.AppendByte(bdCertSig.NumBytes);
                    bdTlv.AppendBd(bdCertSig);

                    // Insert QR into XML
                    Chilkat.Xml xmlQR = new Chilkat.Xml();
                    xmlQR.Tag = "cac:AdditionalDocumentReference";
                    xmlQR.UpdateChildContent("cbc:ID", "QR");
                    xmlQR.UpdateAttrAt("cac:Attachment|cbc:EmbeddedDocumentBinaryObject", true, "mimeCode", "text/plain");
                    xmlQR.UpdateChildContent("cac:Attachment|cbc:EmbeddedDocumentBinaryObject", bdTlv.GetEncoded("base64"));

                    Chilkat.StringBuilder sbSignedXml = new Chilkat.StringBuilder();
                    success = sbSignedXml.LoadFile(signedXmlFilePath, "utf-8");
                    if (!success)
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Failed to load signed XML file." });
                    }

                    Chilkat.StringBuilder sbReplaceStr = new Chilkat.StringBuilder();
                    xmlQR.EmitXmlDecl = false;
                    xmlQR.EmitCompact = true;
                    sbReplaceStr.Append(xmlQR.GetXml());
                    sbReplaceStr.Append("<cac:Signature>");
                    success = sbSignedXml.ReplaceFirst("<cac:Signature>", sbReplaceStr.GetAsString());
                    if (!success)
                    {
                        return BadRequest(new { success = false, message = "Did not find <cac:Signature> in signed XML." });
                    }

                    success = sbSignedXml.WriteFile(appPath + "\\SignedXML\\signedXmlResult1V.xml", "utf-8", false);
                    // 🔹 Save signedXmlResult1V
                    string finalXml = sbSignedXml.GetAsString();

                    if (azureEnabled)
                    {
                        UploadToAzure(finalXml, "signedXmlResult1V.xml", CompanyId);
                    }
                    else
                    {
                        sbSignedXml.WriteFile(Path.Combine(appPath, "SignedXML", "signedXmlResult1V.xml"), "utf-8", false);
                    }
                    var result = await dbContext.Tbl20161VatinvoiceMasters
                    .Where(x => x.InvoiceNo == invoiceNo)
                    .Select(x => new
                    {
                        x.SellerVatnumber,
                        x.InvoiceDateWtTime,
                        x.InvoiceUuid,
                        x.InvoiceTypeCode
                    })
                    .FirstOrDefaultAsync();

                    if (result != null)
                    {
                        string safeDate = result.InvoiceDateWtTime.HasValue
                            ? result.InvoiceDateWtTime.Value.ToString("yyyyMMddHHmmss")
                            : "00000000000000";

                        XMLFileNameAsPerZatca = Path.Combine("SignedXML", $"{result.SellerVatnumber}_{safeDate}_{invoiceNo}.xml");


                    }

                    else
                    {
                        _logger.LogWarning("Invoice not found for InvoiceNo {InvoiceNo}", invoiceNo);
                    }
                    if (GetAzureStatus(companyId))
                    {
                        // Upload XML to Azure
                        await UploadXmlToAzureIfEnabledAsync(appPath + XMLFileNameAsPerZatca, companyId);

                        // Delete local file after successful upload
                        string filePath = Path.Combine(appPath, XMLFileNameAsPerZatca);
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }


                    if (azureEnabled)
                    {
                        UploadToAzure(finalXml, Path.GetFileName(XMLFileNameAsPerZatca), CompanyId);
                    }
                    else
                    {
                        sbSignedXml.WriteFile(Path.Combine(appPath, XMLFileNameAsPerZatca), "utf-8", false);
                    }

                    // Verify signature
                    Chilkat.XmlDSig verifier = new Chilkat.XmlDSig();
                    success = verifier.LoadSignatureSb(sbSignedXml);
                    if (!success || verifier.NumSignatures == 0)
                    {
                        return BadRequest(new { success = false, message = "Failed to load/verify signatures." });
                    }


                    verifier.UncommonOptions = "ZATCA";

                    for (int verifyIdx = 0; verifyIdx < verifier.NumSignatures; verifyIdx++)
                    {
                        verifier.Selector = verifyIdx;
                        bool verified = verifier.VerifySignature(true);
                        if (!verified)
                        {
                            return BadRequest(new { success = false, message = "XML signature verification failed." });
                        }
                    }
                }
                    return Ok(new { success = true, message = "XML signed and verified successfully.", QRCode = bdTlv.GetEncoded("base64") });
                
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message });
            }
        }




        ////private void PrintPreview_RegularTaxInvoice()
        ////{
        ////    rpt101_eInvoiceDesign01 rpt1 = new rpt101_eInvoiceDesign01();
        ////    rpt1.Qry201_602VATInvoiceReportTableAdapter.FillByInvoiceNo(rpt1.DsRptEInvoice011.qry201_602VATInvoiceReport, this.txtInvoiceNo.EditValue);

        ////    Rpt101_eInvoicePrepaymentDetails rpt2 = new Rpt101_eInvoicePrepaymentDetails();
        ////    rpt2.Qry201_642PrepaidAdjustmentListforXML02TableAdapter.FillByInvoiceNo(rpt2.DsRpt101_eInvoicePrepaymentDetails1.qry201_642PrepaidAdjustmentListforXML02, this.txtInvoiceNo.EditValue);
        ////    rpt1.XrSubreport1.ReportSource = rpt2;

        ////    if (rpt2.DsRpt101_eInvoicePrepaymentDetails1.qry201_642PrepaidAdjustmentListforXML02.Rows.Count == 0)
        ////    {
        ////        rpt1.XrSubreport1.Visible = false;
        ////        rpt1.rowAdvanceInclTax.Visible = false;
        ////        rpt1.rowInvoiceTotalPayableAmount.Visible = false;
        ////    }

        ////    rpt1.intLogOnAccessLevel = this.intLogOnAccessLevel;
        ////    rpt1.intLogOnUserID = this.intLogOnUserID;
        ////    rpt1.intLogOnUserLevel = this.intLogOnUserLevel;
        ////    rpt1.strLogOnUser = this.strLogonUser;

        ////    for (int i = 0; i <= XtraTabControl1.TabPages.Count - 1; i++)
        ////    {
        ////        if (XtraTabControl1.TabPages(i).Name == "pagePaymentDetails")
        ////        {
        ////            XtraTabControl1.SelectedTabPage = XtraTabControl1.TabPages(i);
        ////            break;
        ////        }
        ////    }

        ////    rpt1.intCompanyID = this.txtCompanyID02.EditValue;

        ////    if (this.txtRetentionAmount.EditValue == 0)
        ////        rpt1.rowRetentionRow.Visible = false;

        ////    if (this.txtAdvanceAmount.EditValue == 0)
        ////        rpt1.rowAdvanceRow.Visible = false;

        ////    if (this.txtOtherDeductionAmount.EditValue == 0)
        ////        rpt1.rowOtherDeductionsRow.Visible = false;


        ////    if (this.txtPrintInLetterHead.EditValue == true)
        ////    {
        ////        rpt1.Watermark.Image = GetLetterHeadFull(this.txtCompanyID02.EditValue);
        ////        rpt1.Watermark.ImageAlign = ContentAlignment.TopLeft;
        ////        rpt1.Watermark.ImageTiling = false;
        ////        rpt1.Watermark.ImageViewMode = ImageViewMode.Stretch;
        ////        rpt1.Watermark.ImageTransparency = 0;
        ////        rpt1.Watermark.ShowBehind = true;
        ////    }

        ////    if (this.txtUseDateFormat.EditValue == true)
        ////        rpt1.IsShowDateFormat = true;
        ////    else
        ////        rpt1.IsShowDateFormat = false;

        ////    if (this.txtUseDateFormat02.EditValue == true)
        ////        rpt1.IsShowDateFormat02 = true;
        ////    else
        ////        rpt1.IsShowDateFormat02 = false;

        ////    if (this.txtPrintSignaturesAtBottom.EditValue == true)
        ////        rpt1.IsPrintAtBottom = true;
        ////    else
        ////        rpt1.IsPrintAtBottom = false;

        ////    if (this.txtUse2DigitsForTotal.EditValue == true)
        ////        rpt1.IsShow2DigitsInTotal = true;
        ////    else
        ////        rpt1.IsShow2DigitsInTotal = false;

        ////    if (this.txtInvoiceTypeCode.EditValue == "386")
        ////    {
        ////        rpt1.lblInvoiceTitle.Text = "PREPAYMENT TAX INVOICE";
        ////        rpt1.lblInvoiceTitleAr.Text = "فاتورة ضريبة القيمة المضافة للدفعة المقدمة";
        ////    }

        ////    // Modified for TAX Debit Note
        ////    if (this.txtInvoiceTypeCode.EditValue == "383")
        ////    {
        ////        rpt1.IsDebitNote = true;
        ////        rpt1.lblInvoiceTitle.Text = "TAX DEBIT NOTE";
        ////        rpt1.lblInvoiceTitleAr.Text = "مذكرة الخصم الضريبي";

        ////        rpt1.lblInvoiceNo.Text = "Debit Note No:";
        ////        rpt1.lblInvoiceNoAr.Text = "رقم مذكرة الخصم";

        ////        rpt1.lblInvoicePeriod.Text = "Invoice Reference No";
        ////        rpt1.lblInvoicePeriodAr.Text = "رقم مرجع الفاتورة";
        ////        // rpt1.txtInvoicePeriod.Text = Me.txtReferenceInvoiceNo.Text
        ////        // rpt1.txtInvoicePeriod.Text = "2024-02635"

        ////        rpt1.lblReferenceNo.Text = "Reason for Debit Note";
        ////        rpt1.lblReferenceNoAr.Text = "سبب إصدار سندات الخصم";
        ////    }

        ////    // -----------------------------------------------------------------------------

        ////    // If InvoiceSubTypeCode = "02" Then
        ////    // rpt1.lblVATInvoice.Text = "Simplified Tax Invoice / "
        ////    // rpt1.lblVATInvAr.Text = "فاتورة ضریبیة المبسطة"
        ////    // Else
        ////    // rpt1.lblVATInvoice.Text = "T A X   I N V O I C E / "
        ////    // rpt1.lblVATInvAr.Text = "فاتورة ضريبية"
        ////    // End If

        ////    try
        ////    {
        ////        bool IsCashEntry = false;
        ////        IsCashEntry = frm00102eInvSalesInvoiceEdit.CheckIfSalesInvoiceIsCashEntry(this.txtInvoiceNo.EditValue);
        ////        rpt1.IsCashInvoice = IsCashEntry;
        ////    }
        ////    catch (Exception ex)
        ////    {
        ////    }



        ////    rpt1.ExportOptions.PrintPreview.DefaultFileName = this.txtSellerVATNumber.EditValue + "_" + Format(this.txtInvoiceDateAndTime.EditValue, "yyyy-MM-dd") + "T" + Format(this.txtInvoiceDateAndTime.EditValue, "HHmmss") + "_" + this.txtInvoiceNo.EditValue;
        ////    rpt1.RequestParameters = false;
        ////    rpt1.PrintingSystem.ExecCommand(DevExpress.XtraPrinting.PrintingSystemCommand.Scale, new object[] { 0.7F });

        ////    rpt1.IsUse2ndPhaseQRCode = true;
        ////    rpt1.ShowPreview();

        ////    // If eInvoice status is cleared then
        ////    // Get the QR code from the Cleared eInvoice Log file

        ////    if (GeteInvoiceStatus(this.txtInvoiceNo.Text) == "CLEARED")
        ////        // GeteInvoiceClearedQRCode
        ////        rpt1.txtQRCode.Text = GeteInvoiceClearedQRCode(this.txtInvoiceNo.Text);
        ////    else
        ////    {
        ////        rpt1.txtQRCode.Text = "";

        ////        rpt1.Watermark.Text = "" + Constants.vbCrLf + Constants.vbCrLf + Constants.vbCrLf + Constants.vbCrLf + "D R A F T   C O P Y" + Constants.vbCrLf + "نسخة المسودة";
        ////        rpt1.Watermark.ForeColor = Color.FromArgb(0, 128, 255);
        ////        rpt1.Watermark.TextTransparency = 125;
        ////        rpt1.Watermark.TextDirection = DirectionMode.Horizontal;


        ////        ReportPrintTool printTool = new ReportPrintTool(rpt1);
        ////        DevExpress.XtraPrinting.PrintingSystemBase printingSystem = printTool.PrintingSystem;
        ////        if (printingSystem.GetCommandVisibility(DevExpress.XtraPrinting.PrintingSystemCommand.Watermark) != DevExpress.XtraPrinting.CommandVisibility.None)
        ////            printingSystem.SetCommandVisibility(DevExpress.XtraPrinting.PrintingSystemCommand.Watermark, DevExpress.XtraPrinting.CommandVisibility.None);
        ////    }
        ////}

        ////private void PrintPreview_RegularTaxInvoice_OC()
        ////{
        ////    rpt101_eInvoiceDesign02 rpt1 = new rpt101_eInvoiceDesign02();
        ////    rpt1.Qry201_602VATInvoiceReportTableAdapter.FillByInvoiceNo(rpt1.DsRptEInvoice011.qry201_602VATInvoiceReport, this.txtInvoiceNo.EditValue);

        ////    Rpt101_eInvoicePrepaymentDetails rpt2 = new Rpt101_eInvoicePrepaymentDetails();
        ////    rpt2.Qry201_642PrepaidAdjustmentListforXML02TableAdapter.FillByInvoiceNo(rpt2.DsRpt101_eInvoicePrepaymentDetails1.qry201_642PrepaidAdjustmentListforXML02, this.txtInvoiceNo.EditValue);
        ////    rpt1.XrSubreport1.ReportSource = rpt2;

        ////    if (rpt2.DsRpt101_eInvoicePrepaymentDetails1.qry201_642PrepaidAdjustmentListforXML02.Rows.Count == 0)
        ////    {
        ////        rpt1.XrSubreport1.Visible = false;
        ////        rpt1.rowAdvanceInclTax.Visible = false;
        ////        rpt1.rowInvoiceTotalPayableAmount.Visible = false;
        ////    }

        ////    rpt1.intLogOnAccessLevel = this.intLogOnAccessLevel;
        ////    rpt1.intLogOnUserID = this.intLogOnUserID;
        ////    rpt1.intLogOnUserLevel = this.intLogOnUserLevel;
        ////    rpt1.strLogOnUser = this.strLogonUser;

        ////    for (int i = 0; i <= XtraTabControl1.TabPages.Count - 1; i++)
        ////    {
        ////        if (XtraTabControl1.TabPages(i).Name == "pagePaymentDetails")
        ////        {
        ////            XtraTabControl1.SelectedTabPage = XtraTabControl1.TabPages(i);
        ////            break;
        ////        }
        ////    }

        ////    rpt1.intCompanyID = this.txtCompanyID02.EditValue;

        ////    if (this.txtRetentionAmount.EditValue == 0)
        ////        rpt1.rowRetentionRow.Visible = false;

        ////    if (this.txtAdvanceAmount.EditValue == 0)
        ////        rpt1.rowAdvanceRow.Visible = false;

        ////    if (this.txtOtherDeductionAmount.EditValue == 0)
        ////        rpt1.rowOtherDeductionsRow.Visible = false;


        ////    if (this.txtPrintInLetterHead.EditValue == true)
        ////    {
        ////        rpt1.Watermark.Image = GetLetterHeadFull(this.txtCompanyID02.EditValue);
        ////        rpt1.Watermark.ImageAlign = ContentAlignment.TopLeft;
        ////        rpt1.Watermark.ImageTiling = false;
        ////        rpt1.Watermark.ImageViewMode = ImageViewMode.Stretch;
        ////        rpt1.Watermark.ImageTransparency = 0;
        ////        rpt1.Watermark.ShowBehind = true;
        ////    }

        ////    if (this.txtUseDateFormat.EditValue == true)
        ////        rpt1.IsShowDateFormat = true;
        ////    else
        ////        rpt1.IsShowDateFormat = false;

        ////    if (this.txtUseDateFormat02.EditValue == true)
        ////        rpt1.IsShowDateFormat02 = true;
        ////    else
        ////        rpt1.IsShowDateFormat02 = false;

        ////    if (this.txtPrintSignaturesAtBottom.EditValue == true)
        ////        rpt1.IsPrintAtBottom = true;
        ////    else
        ////        rpt1.IsPrintAtBottom = false;

        ////    if (this.txtUse2DigitsForTotal.EditValue == true)
        ////        rpt1.IsShow2DigitsInTotal = true;
        ////    else
        ////        rpt1.IsShow2DigitsInTotal = false;

        ////    if (this.txtInvoiceTypeCode.EditValue == "386")
        ////    {
        ////        rpt1.lblInvoiceTitle.Text = "PREPAYMENT TAX INVOICE";
        ////        rpt1.lblInvoiceTitleAr.Text = "فاتورة ضريبة القيمة المضافة للدفعة المقدمة";
        ////    }

        ////    // If InvoiceSubTypeCode = "02" Then
        ////    // rpt1.lblVATInvoice.Text = "Simplified Tax Invoice / "
        ////    // rpt1.lblVATInvAr.Text = "فاتورة ضریبیة المبسطة"
        ////    // Else
        ////    // rpt1.lblVATInvoice.Text = "T A X   I N V O I C E / "
        ////    // rpt1.lblVATInvAr.Text = "فاتورة ضريبية"
        ////    // End If

        ////    try
        ////    {
        ////        bool IsCashEntry = false;
        ////        IsCashEntry = frm00102eInvSalesInvoiceEdit.CheckIfSalesInvoiceIsCashEntry(this.txtInvoiceNo.EditValue);
        ////        rpt1.IsCashInvoice = IsCashEntry;
        ////    }
        ////    catch (Exception ex)
        ////    {
        ////    }

        ////    rpt1.ExportOptions.PrintPreview.DefaultFileName = this.txtSellerVATNumber.EditValue + "_" + Format(this.txtInvoiceDateAndTime.EditValue, "yyyy-MM-dd") + "T" + Format(this.txtInvoiceDateAndTime.EditValue, "HHmmss") + "_" + this.txtInvoiceNo.EditValue;
        ////    rpt1.RequestParameters = false;
        ////    rpt1.PrintingSystem.ExecCommand(DevExpress.XtraPrinting.PrintingSystemCommand.Scale, new object[] { 0.7F });

        ////    rpt1.IsUse2ndPhaseQRCode = true;
        ////    rpt1.ShowPreview();

        ////    // If eInvoice status is cleared then
        ////    // Get the QR code from the Cleared eInvoice Log file

        ////    if (GeteInvoiceStatus(this.txtInvoiceNo.Text) == "CLEARED")
        ////        // GeteInvoiceClearedQRCode
        ////        rpt1.txtQRCode.Text = GeteInvoiceClearedQRCode(this.txtInvoiceNo.Text);
        ////    else
        ////    {
        ////        rpt1.txtQRCode.Text = "";

        ////        rpt1.Watermark.Text = "" + Constants.vbCrLf + Constants.vbCrLf + Constants.vbCrLf + Constants.vbCrLf + "D R A F T   C O P Y" + Constants.vbCrLf + "نسخة المسودة";
        ////        rpt1.Watermark.ForeColor = Color.FromArgb(0, 128, 255);
        ////        rpt1.Watermark.TextTransparency = 125;
        ////        rpt1.Watermark.TextDirection = DirectionMode.Horizontal;


        ////        ReportPrintTool printTool = new ReportPrintTool(rpt1);
        ////        DevExpress.XtraPrinting.PrintingSystemBase printingSystem = printTool.PrintingSystem;
        ////        if (printingSystem.GetCommandVisibility(DevExpress.XtraPrinting.PrintingSystemCommand.Watermark) != DevExpress.XtraPrinting.CommandVisibility.None)
        ////            printingSystem.SetCommandVisibility(DevExpress.XtraPrinting.PrintingSystemCommand.Watermark, DevExpress.XtraPrinting.CommandVisibility.None);
        ////    }
        ////}

        ////private void PrintPreview_RegularTaxCreditNote()
        ////{
        ////    rpt101_eInvoiceDesign03 rpt1 = new rpt101_eInvoiceDesign03();
        ////    rpt1.Qry201_802VATCreditNoteReportTableAdapter.FillByCreditNoteNo(rpt1.DsRpt20168CreditNote1.qry201_802VATCreditNoteReport, this.txtInvoiceNo.EditValue);
        ////    rpt1.intLogOnAccessLevel = this.intLogOnAccessLevel;
        ////    rpt1.intLogOnUserID = this.intLogOnUserID;
        ////    rpt1.intLogOnUserLevel = this.intLogOnUserLevel;
        ////    rpt1.strLogOnUser = this.strLogonUser;

        ////    for (int i = 0; i <= XtraTabControl1.TabPages.Count - 1; i++)
        ////    {
        ////        if (XtraTabControl1.TabPages(i).Name == "pagePaymentDetails")
        ////        {
        ////            XtraTabControl1.SelectedTabPage = XtraTabControl1.TabPages(i);
        ////            break;
        ////        }
        ////    }

        ////    rpt1.intCompanyID = this.txtCompanyID02.EditValue;

        ////    if (this.txtRetentionAmount.EditValue == 0)
        ////    {
        ////        rpt1.rowRetentionRow.Visible = false;
        ////        rpt1.GroupFooter2.HeightF = rpt1.GroupFooter2.HeightF - 18;
        ////    }

        ////    if (this.txtAdvanceAmount.EditValue == 0)
        ////    {
        ////        rpt1.rowAdvanceRow.Visible = false;
        ////        rpt1.GroupFooter2.HeightF = rpt1.GroupFooter2.HeightF - 18;
        ////    }

        ////    if (this.txtOtherDeductionAmount.EditValue == 0)
        ////    {
        ////        rpt1.rowOtherDeductionsRow.Visible = false;
        ////        rpt1.GroupFooter2.HeightF = rpt1.GroupFooter2.HeightF - 18;
        ////    }


        ////    if (this.txtPrintInLetterHead.EditValue == true)
        ////    {
        ////        rpt1.Watermark.Image = GetLetterHeadFull(this.txtCompanyID02.EditValue);
        ////        rpt1.Watermark.ImageAlign = ContentAlignment.TopLeft;
        ////        rpt1.Watermark.ImageTiling = false;
        ////        rpt1.Watermark.ImageViewMode = ImageViewMode.Stretch;
        ////        rpt1.Watermark.ImageTransparency = 0;
        ////        rpt1.Watermark.ShowBehind = true;
        ////    }

        ////    if (this.txtUseDateFormat.EditValue == true)
        ////        rpt1.IsShowDateFormat = true;
        ////    else
        ////        rpt1.IsShowDateFormat = false;

        ////    if (this.txtUseDateFormat02.EditValue == true)
        ////        rpt1.IsShowDateFormat02 = true;
        ////    else
        ////        rpt1.IsShowDateFormat02 = false;

        ////    if (this.txtPrintSignaturesAtBottom.EditValue == true)
        ////        rpt1.IsPrintAtBottom = true;
        ////    else
        ////        rpt1.IsPrintAtBottom = false;

        ////    // If Me.txtUse2DigitsForTotal.EditValue = True Then
        ////    // rpt1.IsShow2DigitsInTotal = True
        ////    // Else
        ////    // rpt1.IsShow2DigitsInTotal = False
        ////    // End If

        ////    rpt1.ExportOptions.PrintPreview.DefaultFileName = this.txtSellerVATNumber.EditValue + "_" + Format(this.txtInvoiceDateAndTime.EditValue, "yyyy-MM-dd") + "T" + Format(this.txtInvoiceDateAndTime.EditValue, "HHmmss") + "_" + this.txtInvoiceNo.EditValue;
        ////    rpt1.RequestParameters = false;
        ////    rpt1.PrintingSystem.ExecCommand(DevExpress.XtraPrinting.PrintingSystemCommand.Scale, new object[] { 0.7F });

        ////    rpt1.IsUse2ndPhaseQRCode = true;
        ////    rpt1.ShowPreview();

        ////    // If eInvoice status is cleared then
        ////    // Get the QR code from the Cleared eInvoice Log file

        ////    if (GeteInvoiceStatus(this.txtInvoiceNo.Text) == "CLEARED")
        ////        // GeteInvoiceClearedQRCode
        ////        rpt1.txtQRCode.Text = GeteInvoiceClearedQRCode(this.txtInvoiceNo.Text);
        ////    else
        ////    {
        ////        rpt1.txtQRCode.Text = "";

        ////        rpt1.Watermark.Text = "" + Constants.vbCrLf + Constants.vbCrLf + Constants.vbCrLf + Constants.vbCrLf + "D R A F T   C O P Y" + Constants.vbCrLf + "نسخة المسودة";
        ////        rpt1.Watermark.ForeColor = Color.FromArgb(0, 128, 255);
        ////        rpt1.Watermark.TextTransparency = 125;
        ////        rpt1.Watermark.TextDirection = DirectionMode.Horizontal;


        ////        ReportPrintTool printTool = new ReportPrintTool(rpt1);
        ////        DevExpress.XtraPrinting.PrintingSystemBase printingSystem = printTool.PrintingSystem;
        ////        if (printingSystem.GetCommandVisibility(DevExpress.XtraPrinting.PrintingSystemCommand.Watermark) != DevExpress.XtraPrinting.CommandVisibility.None)
        ////            printingSystem.SetCommandVisibility(DevExpress.XtraPrinting.PrintingSystemCommand.Watermark, DevExpress.XtraPrinting.CommandVisibility.None);
        ////    }
        ////}

        ////private void PrintPreview_SimplifiedTaxCreditNote()
        ////{
        ////    rpt101_eInvoiceDesign03 rpt1 = new rpt101_eInvoiceDesign03();
        ////    rpt1.Qry201_802VATCreditNoteReportTableAdapter.FillByCreditNoteNo(rpt1.DsRpt20168CreditNote1.qry201_802VATCreditNoteReport, this.txtInvoiceNo.EditValue);
        ////    rpt1.intLogOnAccessLevel = this.intLogOnAccessLevel;
        ////    rpt1.intLogOnUserID = this.intLogOnUserID;
        ////    rpt1.intLogOnUserLevel = this.intLogOnUserLevel;
        ////    rpt1.strLogOnUser = this.strLogonUser;

        ////    for (int i = 0; i <= XtraTabControl1.TabPages.Count - 1; i++)
        ////    {
        ////        if (XtraTabControl1.TabPages(i).Name == "pagePaymentDetails")
        ////        {
        ////            XtraTabControl1.SelectedTabPage = XtraTabControl1.TabPages(i);
        ////            break;
        ////        }
        ////    }

        ////    rpt1.intCompanyID = this.txtCompanyID02.EditValue;

        ////    if (this.txtRetentionAmount.EditValue == 0)
        ////    {
        ////        rpt1.rowRetentionRow.Visible = false;
        ////        rpt1.GroupFooter2.HeightF = rpt1.GroupFooter2.HeightF - 18;
        ////    }

        ////    if (this.txtAdvanceAmount.EditValue == 0)
        ////    {
        ////        rpt1.rowAdvanceRow.Visible = false;
        ////        rpt1.GroupFooter2.HeightF = rpt1.GroupFooter2.HeightF - 18;
        ////    }

        ////    if (this.txtOtherDeductionAmount.EditValue == 0)
        ////    {
        ////        rpt1.rowOtherDeductionsRow.Visible = false;
        ////        rpt1.GroupFooter2.HeightF = rpt1.GroupFooter2.HeightF - 18;
        ////    }


        ////    if (this.txtPrintInLetterHead.EditValue == true)
        ////    {
        ////        rpt1.Watermark.Image = GetLetterHeadFull(this.txtCompanyID02.EditValue);
        ////        rpt1.Watermark.ImageAlign = ContentAlignment.TopLeft;
        ////        rpt1.Watermark.ImageTiling = false;
        ////        rpt1.Watermark.ImageViewMode = ImageViewMode.Stretch;
        ////        rpt1.Watermark.ImageTransparency = 0;
        ////        rpt1.Watermark.ShowBehind = true;
        ////    }

        ////    // If Me.txtUseDateFormat.EditValue = True Then
        ////    // rpt1.IsShowDateFormat = True
        ////    // Else
        ////    // rpt1.IsShowDateFormat = False
        ////    // End If

        ////    // If Me.txtUseDateFormat02.EditValue = True Then
        ////    // rpt1.IsShowDateFormat02 = True
        ////    // Else
        ////    // rpt1.IsShowDateFormat02 = False
        ////    // End If

        ////    if (this.txtPrintSignaturesAtBottom.EditValue == true)
        ////        rpt1.IsPrintAtBottom = true;
        ////    else
        ////        rpt1.IsPrintAtBottom = false;

        ////    // If Me.txtUse2DigitsForTotal.EditValue = True Then
        ////    // rpt1.IsShow2DigitsInTotal = True
        ////    // Else
        ////    // rpt1.IsShow2DigitsInTotal = False
        ////    // End If

        ////    rpt1.ExportOptions.PrintPreview.DefaultFileName = this.txtSellerVATNumber.EditValue + "_" + Format(this.txtInvoiceDateAndTime.EditValue, "yyyy-MM-dd") + "T" + Format(this.txtInvoiceDateAndTime.EditValue, "HHmmss") + "_" + this.txtInvoiceNo.EditValue;
        ////    rpt1.RequestParameters = false;
        ////    rpt1.PrintingSystem.ExecCommand(DevExpress.XtraPrinting.PrintingSystemCommand.Scale, new object[] { 0.7F });

        ////    rpt1.IsUse2ndPhaseQRCode = true;
        ////    rpt1.ShowPreview();

        ////    rpt1.lblTitle.Text = "Simplified Tax Credit Note";
        ////    rpt1.lblTitleAr.Text = "اشعار دائن";

        ////    // If eInvoice status is cleared then
        ////    // Get the QR code from the Cleared eInvoice Log file

        ////    if (GeteInvoiceStatus(this.txtInvoiceNo.Text) == "REPORTED")
        ////        // GeteInvoiceClearedQRCode
        ////        rpt1.txtQRCode.Text = GeteInvoiceClearedQRCode(this.txtInvoiceNo.Text);
        ////    else
        ////    {
        ////        rpt1.txtQRCode.Text = "";

        ////        rpt1.Watermark.Text = "" + Constants.vbCrLf + Constants.vbCrLf + Constants.vbCrLf + Constants.vbCrLf + "D R A F T   C O P Y" + Constants.vbCrLf + "نسخة المسودة";
        ////        rpt1.Watermark.ForeColor = Color.FromArgb(0, 128, 255);
        ////        rpt1.Watermark.TextTransparency = 125;
        ////        rpt1.Watermark.TextDirection = DirectionMode.Horizontal;


        ////        ReportPrintTool printTool = new ReportPrintTool(rpt1);
        ////        DevExpress.XtraPrinting.PrintingSystemBase printingSystem = printTool.PrintingSystem;
        ////        if (printingSystem.GetCommandVisibility(DevExpress.XtraPrinting.PrintingSystemCommand.Watermark) != DevExpress.XtraPrinting.CommandVisibility.None)
        ////            printingSystem.SetCommandVisibility(DevExpress.XtraPrinting.PrintingSystemCommand.Watermark, DevExpress.XtraPrinting.CommandVisibility.None);
        ////    }
        ////}

        ////private void PrintPreview_RegularTaxCreditNote_OC()
        ////{
        ////    rpt101_eInvoiceDesign04 rpt1 = new rpt101_eInvoiceDesign04();
        ////    rpt1.Qry201_802VATCreditNoteReportTableAdapter.FillByCreditNoteNo(rpt1.DsRpt20168CreditNote1.qry201_802VATCreditNoteReport, this.txtInvoiceNo.EditValue);
        ////    rpt1.intLogOnAccessLevel = this.intLogOnAccessLevel;
        ////    rpt1.intLogOnUserID = this.intLogOnUserID;
        ////    rpt1.intLogOnUserLevel = this.intLogOnUserLevel;
        ////    rpt1.strLogOnUser = this.strLogonUser;

        ////    for (int i = 0; i <= XtraTabControl1.TabPages.Count - 1; i++)
        ////    {
        ////        if (XtraTabControl1.TabPages(i).Name == "pagePaymentDetails")
        ////        {
        ////            XtraTabControl1.SelectedTabPage = XtraTabControl1.TabPages(i);
        ////            break;
        ////        }
        ////    }

        ////    rpt1.intCompanyID = this.txtCompanyID02.EditValue;

        ////    if (this.txtRetentionAmount.EditValue == 0)
        ////        rpt1.rowRetentionRow.Visible = false;

        ////    if (this.txtAdvanceAmount.EditValue == 0)
        ////        rpt1.rowAdvanceRow.Visible = false;

        ////    if (this.txtOtherDeductionAmount.EditValue == 0)
        ////        rpt1.rowOtherDeductionsRow.Visible = false;


        ////    if (this.txtPrintInLetterHead.EditValue == true)
        ////    {
        ////        rpt1.Watermark.Image = GetLetterHeadFull(this.txtCompanyID02.EditValue);
        ////        rpt1.Watermark.ImageAlign = ContentAlignment.TopLeft;
        ////        rpt1.Watermark.ImageTiling = false;
        ////        rpt1.Watermark.ImageViewMode = ImageViewMode.Stretch;
        ////        rpt1.Watermark.ImageTransparency = 0;
        ////        rpt1.Watermark.ShowBehind = true;
        ////    }

        ////    if (this.txtUseDateFormat.EditValue == true)
        ////        rpt1.IsShowDateFormat = true;
        ////    else
        ////        rpt1.IsShowDateFormat = false;

        ////    if (this.txtUseDateFormat02.EditValue == true)
        ////        rpt1.IsShowDateFormat02 = true;
        ////    else
        ////        rpt1.IsShowDateFormat02 = false;

        ////    if (this.txtPrintSignaturesAtBottom.EditValue == true)
        ////        rpt1.IsPrintAtBottom = true;
        ////    else
        ////        rpt1.IsPrintAtBottom = false;

        ////    if (this.txtUse2DigitsForTotal.EditValue == true)
        ////        rpt1.IsShow2DigitsInTotal = true;
        ////    else
        ////        rpt1.IsShow2DigitsInTotal = false;

        ////    rpt1.ExportOptions.PrintPreview.DefaultFileName = this.txtSellerVATNumber.EditValue + "_" + Format(this.txtInvoiceDateAndTime.EditValue, "yyyy-MM-dd") + "T" + Format(this.txtInvoiceDateAndTime.EditValue, "HHmmss") + "_" + this.txtInvoiceNo.EditValue;
        ////    rpt1.RequestParameters = false;
        ////    rpt1.PrintingSystem.ExecCommand(DevExpress.XtraPrinting.PrintingSystemCommand.Scale, new object[] { 0.7F });

        ////    rpt1.IsUse2ndPhaseQRCode = true;
        ////    rpt1.ShowPreview();

        ////    // If eInvoice status is cleared then
        ////    // Get the QR code from the Cleared eInvoice Log file

        ////    if (GeteInvoiceStatus(this.txtInvoiceNo.Text) == "CLEARED")
        ////        // GeteInvoiceClearedQRCode
        ////        rpt1.txtQRCode.Text = GeteInvoiceClearedQRCode(this.txtInvoiceNo.Text);
        ////    else
        ////    {
        ////        rpt1.txtQRCode.Text = "";

        ////        rpt1.Watermark.Text = "" + Constants.vbCrLf + Constants.vbCrLf + Constants.vbCrLf + Constants.vbCrLf + "D R A F T   C O P Y" + Constants.vbCrLf + "نسخة المسودة";
        ////        rpt1.Watermark.ForeColor = Color.FromArgb(0, 128, 255);
        ////        rpt1.Watermark.TextTransparency = 125;
        ////        rpt1.Watermark.TextDirection = DirectionMode.Horizontal;


        ////        ReportPrintTool printTool = new ReportPrintTool(rpt1);
        ////        DevExpress.XtraPrinting.PrintingSystemBase printingSystem = printTool.PrintingSystem;
        ////        if (printingSystem.GetCommandVisibility(DevExpress.XtraPrinting.PrintingSystemCommand.Watermark) != DevExpress.XtraPrinting.CommandVisibility.None)
        ////            printingSystem.SetCommandVisibility(DevExpress.XtraPrinting.PrintingSystemCommand.Watermark, DevExpress.XtraPrinting.CommandVisibility.None);
        ////    }
        ////}


        ////private void ExportPDFA3_RegularTaxInvoice()
        ////{
        ////    rpt101_eInvoiceDesign01 rpt1 = new rpt101_eInvoiceDesign01();
        ////    rpt1.Qry201_602VATInvoiceReportTableAdapter.FillByInvoiceNo(rpt1.DsRptEInvoice011.qry201_602VATInvoiceReport, this.txtInvoiceNo.EditValue);

        ////    Rpt101_eInvoicePrepaymentDetails rpt2 = new Rpt101_eInvoicePrepaymentDetails();
        ////    rpt2.Qry201_642PrepaidAdjustmentListforXML02TableAdapter.FillByInvoiceNo(rpt2.DsRpt101_eInvoicePrepaymentDetails1.qry201_642PrepaidAdjustmentListforXML02, this.txtInvoiceNo.EditValue);
        ////    rpt1.XrSubreport1.ReportSource = rpt2;

        ////    if (rpt2.DsRpt101_eInvoicePrepaymentDetails1.qry201_642PrepaidAdjustmentListforXML02.Rows.Count == 0)
        ////    {
        ////        rpt1.XrSubreport1.Visible = false;
        ////        rpt1.rowAdvanceInclTax.Visible = false;
        ////        rpt1.rowInvoiceTotalPayableAmount.Visible = false;
        ////    }

        ////    rpt1.intLogOnAccessLevel = this.intLogOnAccessLevel;
        ////    rpt1.intLogOnUserID = this.intLogOnUserID;
        ////    rpt1.intLogOnUserLevel = this.intLogOnUserLevel;
        ////    rpt1.strLogOnUser = this.strLogonUser;

        ////    for (int i = 0; i <= XtraTabControl1.TabPages.Count - 1; i++)
        ////    {
        ////        if (XtraTabControl1.TabPages(i).Name == "pagePaymentDetails")
        ////        {
        ////            XtraTabControl1.SelectedTabPage = XtraTabControl1.TabPages(i);
        ////            break;
        ////        }
        ////    }

        ////    rpt1.intCompanyID = this.txtCompanyID02.EditValue;

        ////    if (this.txtRetentionAmount.EditValue == 0)
        ////        rpt1.rowRetentionRow.Visible = false;

        ////    if (this.txtAdvanceAmount.EditValue == 0)
        ////        rpt1.rowAdvanceRow.Visible = false;

        ////    if (this.txtOtherDeductionAmount.EditValue == 0)
        ////        rpt1.rowOtherDeductionsRow.Visible = false;

        ////    if (this.txtPrintInLetterHead.EditValue == true)
        ////    {
        ////        rpt1.Watermark.Image = GetLetterHeadFull(this.txtCompanyID02.EditValue);
        ////        rpt1.Watermark.ImageAlign = ContentAlignment.TopLeft;
        ////        rpt1.Watermark.ImageTiling = false;
        ////        rpt1.Watermark.ImageViewMode = ImageViewMode.Stretch;
        ////        rpt1.Watermark.ImageTransparency = 0;
        ////        rpt1.Watermark.ShowBehind = true;
        ////    }

        ////    if (this.txtUseDateFormat.EditValue == true)
        ////        rpt1.IsShowDateFormat = true;
        ////    else
        ////        rpt1.IsShowDateFormat = false;

        ////    if (this.txtUseDateFormat02.EditValue == true)
        ////        rpt1.IsShowDateFormat02 = true;
        ////    else
        ////        rpt1.IsShowDateFormat02 = false;

        ////    if (this.txtPrintSignaturesAtBottom.EditValue == true)
        ////        rpt1.IsPrintAtBottom = true;
        ////    else
        ////        rpt1.IsPrintAtBottom = false;

        ////    if (this.txtUse2DigitsForTotal.EditValue == true)
        ////        rpt1.IsShow2DigitsInTotal = true;
        ////    else
        ////        rpt1.IsShow2DigitsInTotal = false;

        ////    if (this.txtInvoiceTypeCode.EditValue == "386")
        ////    {
        ////        rpt1.lblInvoiceTitle.Text = "PREPAYMENT TAX INVOICE";
        ////        rpt1.lblInvoiceTitleAr.Text = "فاتورة ضريبة القيمة المضافة للدفعة المقدمة";
        ////    }

        ////    // Modified for TAX Debit Note
        ////    if (this.txtInvoiceTypeCode.EditValue == "383")
        ////    {
        ////        rpt1.IsDebitNote = true;
        ////        rpt1.lblInvoiceTitle.Text = "TAX DEBIT NOTE";
        ////        rpt1.lblInvoiceTitleAr.Text = "مذكرة الخصم الضريبي";

        ////        rpt1.lblInvoiceNo.Text = "Debit Note No:";
        ////        rpt1.lblInvoiceNoAr.Text = "رقم مذكرة الخصم";

        ////        rpt1.lblInvoicePeriod.Text = "Invoice Reference No";
        ////        rpt1.lblInvoicePeriodAr.Text = "رقم مرجع الفاتورة";
        ////        // rpt1.txtInvoicePeriod.Text = Me.txtReferenceInvoiceNo.Text
        ////        // rpt1.txtInvoicePeriod.Text = "2024-02635"

        ////        rpt1.lblReferenceNo.Text = "Reason for Debit Note";
        ////        rpt1.lblReferenceNoAr.Text = "سبب إصدار سندات الخصم";
        ////    }

        ////    // -----------------------------------------------------------------------------

        ////    try
        ////    {
        ////        bool IsCashEntry = false;
        ////        IsCashEntry = frm00102eInvSalesInvoiceEdit.CheckIfSalesInvoiceIsCashEntry(this.txtInvoiceNo.EditValue);
        ////        rpt1.IsCashInvoice = IsCashEntry;
        ////    }
        ////    catch (Exception ex)
        ////    {
        ////    }

        ////    rpt1.ExportOptions.PrintPreview.DefaultFileName = this.txtSellerVATNumber.EditValue + "_" + Format(this.txtInvoiceDateAndTime.EditValue, "yyyy-MM-dd") + "T" + Format(this.txtInvoiceDateAndTime.EditValue, "HHmmss") + "_" + this.txtInvoiceNo.EditValue;
        ////    rpt1.RequestParameters = false;
        ////    rpt1.PrintingSystem.ExecCommand(DevExpress.XtraPrinting.PrintingSystemCommand.Scale, new object[] { 0.7F });

        ////    rpt1.IsUse2ndPhaseQRCode = true;

        ////    // If eInvoice status is cleared then
        ////    // Get the QR code from the Cleared eInvoice Log file

        ////    if (GeteInvoiceStatus(this.txtInvoiceNo.Text) == "CLEARED")
        ////        // GeteInvoiceClearedQRCode
        ////        rpt1.txtQRCode.Text = GeteInvoiceClearedQRCode(this.txtInvoiceNo.Text);
        ////    else
        ////    {
        ////        rpt1.txtQRCode.Text = "";

        ////        rpt1.Watermark.Text = "" + Constants.vbCrLf + Constants.vbCrLf + Constants.vbCrLf + Constants.vbCrLf + "D R A F T   C O P Y" + Constants.vbCrLf + "نسخة المسودة";
        ////        rpt1.Watermark.ForeColor = Color.FromArgb(0, 128, 255);
        ////        rpt1.Watermark.TextTransparency = 125;
        ////        rpt1.Watermark.TextDirection = DirectionMode.Horizontal;


        ////        ReportPrintTool printTool = new ReportPrintTool(rpt1);
        ////        DevExpress.XtraPrinting.PrintingSystemBase printingSystem = printTool.PrintingSystem;
        ////        if (printingSystem.GetCommandVisibility(DevExpress.XtraPrinting.PrintingSystemCommand.Watermark) != DevExpress.XtraPrinting.CommandVisibility.None)
        ////            printingSystem.SetCommandVisibility(DevExpress.XtraPrinting.PrintingSystemCommand.Watermark, DevExpress.XtraPrinting.CommandVisibility.None);
        ////    }

        ////    string reportName = GeteInvoiceXMLFileLocation(this.txtInvoiceNo.Text);
        ////    // reportName = "\SignedXML\RegularTaxInvoice.pdf"
        ////    reportName = reportName.Substring(0, reportName.Length - 4) + ".pdf";

        ////    // \SignedXML\310906806100003_2023-06-25T122606_DA-HD-2023-00248.xml
        ////    // \SignedXML\310906806100003_2023-06-20T112803_2023-00014.xml

        ////    PdfExportOptions options = new PdfExportOptions();
        ////    options.PdfACompatibility = PdfACompatibility.PdfA3a;
        ////    options.Attachments.Add(new PdfAttachment() { FilePath = appPath + GeteInvoiceXMLFileLocation(this.txtInvoiceNo.Text), Type = "text/xml", Description = "ZATCA-XML-Format" });

        ////    rpt1.ExportToPdf(appPath + reportName, options);
        ////    Process.Start(appPath + reportName);
        ////}

        ////private void ExportPDFA3_RegularTaxInvoice_OC()
        ////{
        ////    rpt101_eInvoiceDesign02 rpt1 = new rpt101_eInvoiceDesign02();
        ////    rpt1.Qry201_602VATInvoiceReportTableAdapter.FillByInvoiceNo(rpt1.DsRptEInvoice011.qry201_602VATInvoiceReport, this.txtInvoiceNo.EditValue);

        ////    Rpt101_eInvoicePrepaymentDetails rpt2 = new Rpt101_eInvoicePrepaymentDetails();
        ////    rpt2.Qry201_642PrepaidAdjustmentListforXML02TableAdapter.FillByInvoiceNo(rpt2.DsRpt101_eInvoicePrepaymentDetails1.qry201_642PrepaidAdjustmentListforXML02, this.txtInvoiceNo.EditValue);
        ////    rpt1.XrSubreport1.ReportSource = rpt2;

        ////    if (rpt2.DsRpt101_eInvoicePrepaymentDetails1.qry201_642PrepaidAdjustmentListforXML02.Rows.Count == 0)
        ////    {
        ////        rpt1.XrSubreport1.Visible = false;
        ////        rpt1.rowAdvanceInclTax.Visible = false;
        ////        rpt1.rowInvoiceTotalPayableAmount.Visible = false;
        ////    }

        ////    rpt1.intLogOnAccessLevel = this.intLogOnAccessLevel;
        ////    rpt1.intLogOnUserID = this.intLogOnUserID;
        ////    rpt1.intLogOnUserLevel = this.intLogOnUserLevel;
        ////    rpt1.strLogOnUser = this.strLogonUser;

        ////    for (int i = 0; i <= XtraTabControl1.TabPages.Count - 1; i++)
        ////    {
        ////        if (XtraTabControl1.TabPages(i).Name == "pagePaymentDetails")
        ////        {
        ////            XtraTabControl1.SelectedTabPage = XtraTabControl1.TabPages(i);
        ////            break;
        ////        }
        ////    }

        ////    rpt1.intCompanyID = this.txtCompanyID02.EditValue;

        ////    if (this.txtRetentionAmount.EditValue == 0)
        ////        rpt1.rowRetentionRow.Visible = false;

        ////    if (this.txtAdvanceAmount.EditValue == 0)
        ////        rpt1.rowAdvanceRow.Visible = false;

        ////    if (this.txtOtherDeductionAmount.EditValue == 0)
        ////        rpt1.rowOtherDeductionsRow.Visible = false;

        ////    if (this.txtPrintInLetterHead.EditValue == true)
        ////    {
        ////        rpt1.Watermark.Image = GetLetterHeadFull(this.txtCompanyID02.EditValue);
        ////        rpt1.Watermark.ImageAlign = ContentAlignment.TopLeft;
        ////        rpt1.Watermark.ImageTiling = false;
        ////        rpt1.Watermark.ImageViewMode = ImageViewMode.Stretch;
        ////        rpt1.Watermark.ImageTransparency = 0;
        ////        rpt1.Watermark.ShowBehind = true;
        ////    }

        ////    if (this.txtUseDateFormat.EditValue == true)
        ////        rpt1.IsShowDateFormat = true;
        ////    else
        ////        rpt1.IsShowDateFormat = false;

        ////    if (this.txtUseDateFormat02.EditValue == true)
        ////        rpt1.IsShowDateFormat02 = true;
        ////    else
        ////        rpt1.IsShowDateFormat02 = false;

        ////    if (this.txtPrintSignaturesAtBottom.EditValue == true)
        ////        rpt1.IsPrintAtBottom = true;
        ////    else
        ////        rpt1.IsPrintAtBottom = false;

        ////    if (this.txtUse2DigitsForTotal.EditValue == true)
        ////        rpt1.IsShow2DigitsInTotal = true;
        ////    else
        ////        rpt1.IsShow2DigitsInTotal = false;

        ////    if (this.txtInvoiceTypeCode.EditValue == "386")
        ////    {
        ////        rpt1.lblInvoiceTitle.Text = "PREPAYMENT TAX INVOICE";
        ////        rpt1.lblInvoiceTitleAr.Text = "فاتورة ضريبة القيمة المضافة للدفعة المقدمة";
        ////    }


        ////    try
        ////    {
        ////        bool IsCashEntry = false;
        ////        IsCashEntry = frm00102eInvSalesInvoiceEdit.CheckIfSalesInvoiceIsCashEntry(this.txtInvoiceNo.EditValue);
        ////        rpt1.IsCashInvoice = IsCashEntry;
        ////    }
        ////    catch (Exception ex)
        ////    {
        ////    }

        ////    rpt1.ExportOptions.PrintPreview.DefaultFileName = this.txtSellerVATNumber.EditValue + "_" + Format(this.txtInvoiceDateAndTime.EditValue, "yyyy-MM-dd") + "T" + Format(this.txtInvoiceDateAndTime.EditValue, "HHmmss") + "_" + this.txtInvoiceNo.EditValue;
        ////    rpt1.RequestParameters = false;
        ////    rpt1.PrintingSystem.ExecCommand(DevExpress.XtraPrinting.PrintingSystemCommand.Scale, new object[] { 0.7F });

        ////    rpt1.IsUse2ndPhaseQRCode = true;

        ////    // If eInvoice status is cleared then
        ////    // Get the QR code from the Cleared eInvoice Log file

        ////    if (GeteInvoiceStatus(this.txtInvoiceNo.Text) == "CLEARED")
        ////        // GeteInvoiceClearedQRCode
        ////        rpt1.txtQRCode.Text = GeteInvoiceClearedQRCode(this.txtInvoiceNo.Text);
        ////    else
        ////    {
        ////        rpt1.txtQRCode.Text = "";

        ////        rpt1.Watermark.Text = "" + Constants.vbCrLf + Constants.vbCrLf + Constants.vbCrLf + Constants.vbCrLf + "D R A F T   C O P Y" + Constants.vbCrLf + "نسخة المسودة";
        ////        rpt1.Watermark.ForeColor = Color.FromArgb(0, 128, 255);
        ////        rpt1.Watermark.TextTransparency = 125;
        ////        rpt1.Watermark.TextDirection = DirectionMode.Horizontal;


        ////        ReportPrintTool printTool = new ReportPrintTool(rpt1);
        ////        DevExpress.XtraPrinting.PrintingSystemBase printingSystem = printTool.PrintingSystem;
        ////        if (printingSystem.GetCommandVisibility(DevExpress.XtraPrinting.PrintingSystemCommand.Watermark) != DevExpress.XtraPrinting.CommandVisibility.None)
        ////            printingSystem.SetCommandVisibility(DevExpress.XtraPrinting.PrintingSystemCommand.Watermark, DevExpress.XtraPrinting.CommandVisibility.None);
        ////    }

        ////    string reportName = GeteInvoiceXMLFileLocation(this.txtInvoiceNo.Text);
        ////    // reportName = "\SignedXML\RegularTaxInvoice.pdf"
        ////    reportName = reportName.Substring(0, reportName.Length - 4) + ".pdf";

        ////    // \SignedXML\310906806100003_2023-06-20T112803_2023-00014.xml

        ////    PdfExportOptions options = new PdfExportOptions();
        ////    options.PdfACompatibility = PdfACompatibility.PdfA3a;
        ////    options.Attachments.Add(new PdfAttachment() { FilePath = appPath + GeteInvoiceXMLFileLocation(this.txtInvoiceNo.Text), Type = "text/xml", Description = "ZATCA-XML-Format" });

        ////    rpt1.ExportToPdf(appPath + reportName, options);
        ////    Process.Start(appPath + reportName);
        ////}

        ////private void ExportPDFA3_RegularTaxCreditNote()
        ////{
        ////    rpt101_eInvoiceDesign03 rpt1 = new rpt101_eInvoiceDesign03();
        ////    rpt1.Qry201_802VATCreditNoteReportTableAdapter.FillByCreditNoteNo(rpt1.DsRpt20168CreditNote1.qry201_802VATCreditNoteReport, this.txtInvoiceNo.EditValue);
        ////    rpt1.intLogOnAccessLevel = this.intLogOnAccessLevel;
        ////    rpt1.intLogOnUserID = this.intLogOnUserID;
        ////    rpt1.intLogOnUserLevel = this.intLogOnUserLevel;
        ////    rpt1.strLogOnUser = this.strLogonUser;

        ////    for (int i = 0; i <= XtraTabControl1.TabPages.Count - 1; i++)
        ////    {
        ////        if (XtraTabControl1.TabPages(i).Name == "pagePaymentDetails")
        ////        {
        ////            XtraTabControl1.SelectedTabPage = XtraTabControl1.TabPages(i);
        ////            break;
        ////        }
        ////    }

        ////    rpt1.intCompanyID = this.txtCompanyID02.EditValue;

        ////    if (this.txtRetentionAmount.EditValue == 0)
        ////        rpt1.rowRetentionRow.Visible = false;

        ////    if (this.txtAdvanceAmount.EditValue == 0)
        ////        rpt1.rowAdvanceRow.Visible = false;

        ////    if (this.txtOtherDeductionAmount.EditValue == 0)
        ////        rpt1.rowOtherDeductionsRow.Visible = false;


        ////    if (this.txtPrintInLetterHead.EditValue == true)
        ////    {
        ////        rpt1.Watermark.Image = GetLetterHeadFull(this.txtCompanyID02.EditValue);
        ////        rpt1.Watermark.ImageAlign = ContentAlignment.TopLeft;
        ////        rpt1.Watermark.ImageTiling = false;
        ////        rpt1.Watermark.ImageViewMode = ImageViewMode.Stretch;
        ////        rpt1.Watermark.ImageTransparency = 0;
        ////        rpt1.Watermark.ShowBehind = true;
        ////    }

        ////    if (this.txtUseDateFormat.EditValue == true)
        ////        rpt1.IsShowDateFormat = true;
        ////    else
        ////        rpt1.IsShowDateFormat = false;

        ////    if (this.txtUseDateFormat02.EditValue == true)
        ////        rpt1.IsShowDateFormat02 = true;
        ////    else
        ////        rpt1.IsShowDateFormat02 = false;

        ////    if (this.txtPrintSignaturesAtBottom.EditValue == true)
        ////        rpt1.IsPrintAtBottom = true;
        ////    else
        ////        rpt1.IsPrintAtBottom = false;

        ////    if (this.txtUse2DigitsForTotal.EditValue == true)
        ////        rpt1.IsShow2DigitsInTotal = true;
        ////    else
        ////        rpt1.IsShow2DigitsInTotal = false;

        ////    rpt1.ExportOptions.PrintPreview.DefaultFileName = this.txtSellerVATNumber.EditValue + "_" + Format(this.txtInvoiceDateAndTime.EditValue, "yyyy-MM-dd") + "T" + Format(this.txtInvoiceDateAndTime.EditValue, "HHmmss") + "_" + this.txtInvoiceNo.EditValue;
        ////    rpt1.RequestParameters = false;
        ////    rpt1.PrintingSystem.ExecCommand(DevExpress.XtraPrinting.PrintingSystemCommand.Scale, new object[] { 0.7F });

        ////    rpt1.IsUse2ndPhaseQRCode = true;
        ////    rpt1.ShowPreview();

        ////    // If eInvoice status is cleared then
        ////    // Get the QR code from the Cleared eInvoice Log file

        ////    if (GeteInvoiceStatus(this.txtInvoiceNo.Text) == "CLEARED")
        ////        // GeteInvoiceClearedQRCode
        ////        rpt1.txtQRCode.Text = GeteInvoiceClearedQRCode(this.txtInvoiceNo.Text);
        ////    else
        ////    {
        ////        rpt1.txtQRCode.Text = "";

        ////        rpt1.Watermark.Text = "" + Constants.vbCrLf + Constants.vbCrLf + Constants.vbCrLf + Constants.vbCrLf + "D R A F T   C O P Y" + Constants.vbCrLf + "نسخة المسودة";
        ////        rpt1.Watermark.ForeColor = Color.FromArgb(0, 128, 255);
        ////        rpt1.Watermark.TextTransparency = 125;
        ////        rpt1.Watermark.TextDirection = DirectionMode.Horizontal;


        ////        ReportPrintTool printTool = new ReportPrintTool(rpt1);
        ////        DevExpress.XtraPrinting.PrintingSystemBase printingSystem = printTool.PrintingSystem;
        ////        if (printingSystem.GetCommandVisibility(DevExpress.XtraPrinting.PrintingSystemCommand.Watermark) != DevExpress.XtraPrinting.CommandVisibility.None)
        ////            printingSystem.SetCommandVisibility(DevExpress.XtraPrinting.PrintingSystemCommand.Watermark, DevExpress.XtraPrinting.CommandVisibility.None);
        ////    }

        ////    string reportName = GeteInvoiceXMLFileLocation(this.txtInvoiceNo.Text);
        ////    // reportName = "\SignedXML\RegularTaxInvoice.pdf"
        ////    reportName = reportName.Substring(0, reportName.Length - 4) + ".pdf";

        ////    PdfExportOptions options = new PdfExportOptions();
        ////    options.PdfACompatibility = PdfACompatibility.PdfA3a;
        ////    options.Attachments.Add(new PdfAttachment() { FilePath = appPath + GeteInvoiceXMLFileLocation(this.txtInvoiceNo.Text), Type = "text/xml", Description = "ZATCA-XML-Format" });

        ////    rpt1.ExportToPdf(appPath + reportName, options);
        ////    Process.Start(appPath + reportName);
        ////}

        ////private void ExportPDFA3_SimplifiedTaxCreditNote()
        ////{
        ////    rpt101_eInvoiceDesign03 rpt1 = new rpt101_eInvoiceDesign03();
        ////    rpt1.Qry201_802VATCreditNoteReportTableAdapter.FillByCreditNoteNo(rpt1.DsRpt20168CreditNote1.qry201_802VATCreditNoteReport, this.txtInvoiceNo.EditValue);
        ////    rpt1.intLogOnAccessLevel = this.intLogOnAccessLevel;
        ////    rpt1.intLogOnUserID = this.intLogOnUserID;
        ////    rpt1.intLogOnUserLevel = this.intLogOnUserLevel;
        ////    rpt1.strLogOnUser = this.strLogonUser;

        ////    for (int i = 0; i <= XtraTabControl1.TabPages.Count - 1; i++)
        ////    {
        ////        if (XtraTabControl1.TabPages(i).Name == "pagePaymentDetails")
        ////        {
        ////            XtraTabControl1.SelectedTabPage = XtraTabControl1.TabPages(i);
        ////            break;
        ////        }
        ////    }

        ////    rpt1.intCompanyID = this.txtCompanyID02.EditValue;

        ////    if (this.txtRetentionAmount.EditValue == 0)
        ////        rpt1.rowRetentionRow.Visible = false;

        ////    if (this.txtAdvanceAmount.EditValue == 0)
        ////        rpt1.rowAdvanceRow.Visible = false;

        ////    if (this.txtOtherDeductionAmount.EditValue == 0)
        ////        rpt1.rowOtherDeductionsRow.Visible = false;


        ////    if (this.txtPrintInLetterHead.EditValue == true)
        ////    {
        ////        rpt1.Watermark.Image = GetLetterHeadFull(this.txtCompanyID02.EditValue);
        ////        rpt1.Watermark.ImageAlign = ContentAlignment.TopLeft;
        ////        rpt1.Watermark.ImageTiling = false;
        ////        rpt1.Watermark.ImageViewMode = ImageViewMode.Stretch;
        ////        rpt1.Watermark.ImageTransparency = 0;
        ////        rpt1.Watermark.ShowBehind = true;
        ////    }

        ////    // If Me.txtUseDateFormat.EditValue = True Then
        ////    // rpt1.IsShowDateFormat = True
        ////    // Else
        ////    // rpt1.IsShowDateFormat = False
        ////    // End If

        ////    // If Me.txtUseDateFormat02.EditValue = True Then
        ////    // rpt1.IsShowDateFormat02 = True
        ////    // Else
        ////    // rpt1.IsShowDateFormat02 = False
        ////    // End If

        ////    if (this.txtPrintSignaturesAtBottom.EditValue == true)
        ////        rpt1.IsPrintAtBottom = true;
        ////    else
        ////        rpt1.IsPrintAtBottom = false;

        ////    if (this.txtUse2DigitsForTotal.EditValue == true)
        ////        rpt1.IsShow2DigitsInTotal = true;
        ////    else
        ////        rpt1.IsShow2DigitsInTotal = false;

        ////    rpt1.ExportOptions.PrintPreview.DefaultFileName = this.txtSellerVATNumber.EditValue + "_" + Format(this.txtInvoiceDateAndTime.EditValue, "yyyy-MM-dd") + "T" + Format(this.txtInvoiceDateAndTime.EditValue, "HHmmss") + "_" + this.txtInvoiceNo.EditValue;
        ////    rpt1.RequestParameters = false;
        ////    rpt1.PrintingSystem.ExecCommand(DevExpress.XtraPrinting.PrintingSystemCommand.Scale, new object[] { 0.7F });

        ////    rpt1.IsUse2ndPhaseQRCode = true;
        ////    rpt1.ShowPreview();

        ////    rpt1.lblTitle.Text = "Simplified Tax Credit Note";
        ////    rpt1.lblTitleAr.Text = "اشعار دائن";

        ////    // If eInvoice status is cleared then
        ////    // Get the QR code from the Cleared eInvoice Log file

        ////    if (GeteInvoiceStatus(this.txtInvoiceNo.Text) == "REPORTED")
        ////        // GeteInvoiceClearedQRCode
        ////        rpt1.txtQRCode.Text = GeteInvoiceClearedQRCode(this.txtInvoiceNo.Text);
        ////    else
        ////    {
        ////        rpt1.txtQRCode.Text = "";

        ////        rpt1.Watermark.Text = "" + Constants.vbCrLf + Constants.vbCrLf + Constants.vbCrLf + Constants.vbCrLf + "D R A F T   C O P Y" + Constants.vbCrLf + "نسخة المسودة";
        ////        rpt1.Watermark.ForeColor = Color.FromArgb(0, 128, 255);
        ////        rpt1.Watermark.TextTransparency = 125;
        ////        rpt1.Watermark.TextDirection = DirectionMode.Horizontal;


        ////        ReportPrintTool printTool = new ReportPrintTool(rpt1);
        ////        DevExpress.XtraPrinting.PrintingSystemBase printingSystem = printTool.PrintingSystem;
        ////        if (printingSystem.GetCommandVisibility(DevExpress.XtraPrinting.PrintingSystemCommand.Watermark) != DevExpress.XtraPrinting.CommandVisibility.None)
        ////            printingSystem.SetCommandVisibility(DevExpress.XtraPrinting.PrintingSystemCommand.Watermark, DevExpress.XtraPrinting.CommandVisibility.None);
        ////    }

        ////    string reportName = GeteInvoiceXMLFileLocation(this.txtInvoiceNo.Text);
        ////    // reportName = "\SignedXML\RegularTaxInvoice.pdf"
        ////    reportName = reportName.Substring(0, reportName.Length - 4) + ".pdf";
        ////    // \SignedXML\310906806100003_2023-06-20T112803_2023-00014.xml

        ////    PdfExportOptions options = new PdfExportOptions();
        ////    options.PdfACompatibility = PdfACompatibility.PdfA3a;
        ////    options.Attachments.Add(new PdfAttachment() { FilePath = appPath + GeteInvoiceXMLFileLocation(this.txtInvoiceNo.Text), Type = "text/xml", Description = "ZATCA-XML-Format" });

        ////    rpt1.ExportToPdf(appPath + reportName, options);
        ////    Process.Start(appPath + reportName);
        ////}

        ////private void ExportPDFA3_RegularTaxCreditNote_OC()
        ////{
        ////    rpt101_eInvoiceDesign04 rpt1 = new rpt101_eInvoiceDesign04();
        ////    rpt1.Qry201_802VATCreditNoteReportTableAdapter.FillByCreditNoteNo(rpt1.DsRpt20168CreditNote1.qry201_802VATCreditNoteReport, this.txtInvoiceNo.EditValue);
        ////    rpt1.intLogOnAccessLevel = this.intLogOnAccessLevel;
        ////    rpt1.intLogOnUserID = this.intLogOnUserID;
        ////    rpt1.intLogOnUserLevel = this.intLogOnUserLevel;
        ////    rpt1.strLogOnUser = this.strLogonUser;

        ////    for (int i = 0; i <= XtraTabControl1.TabPages.Count - 1; i++)
        ////    {
        ////        if (XtraTabControl1.TabPages(i).Name == "pagePaymentDetails")
        ////        {
        ////            XtraTabControl1.SelectedTabPage = XtraTabControl1.TabPages(i);
        ////            break;
        ////        }
        ////    }

        ////    rpt1.intCompanyID = this.txtCompanyID02.EditValue;

        ////    if (this.txtRetentionAmount.EditValue == 0)
        ////        rpt1.rowRetentionRow.Visible = false;

        ////    if (this.txtAdvanceAmount.EditValue == 0)
        ////        rpt1.rowAdvanceRow.Visible = false;

        ////    if (this.txtOtherDeductionAmount.EditValue == 0)
        ////        rpt1.rowOtherDeductionsRow.Visible = false;


        ////    if (this.txtPrintInLetterHead.EditValue == true)
        ////    {
        ////        rpt1.Watermark.Image = GetLetterHeadFull(this.txtCompanyID02.EditValue);
        ////        rpt1.Watermark.ImageAlign = ContentAlignment.TopLeft;
        ////        rpt1.Watermark.ImageTiling = false;
        ////        rpt1.Watermark.ImageViewMode = ImageViewMode.Stretch;
        ////        rpt1.Watermark.ImageTransparency = 0;
        ////        rpt1.Watermark.ShowBehind = true;
        ////    }

        ////    if (this.txtUseDateFormat.EditValue == true)
        ////        rpt1.IsShowDateFormat = true;
        ////    else
        ////        rpt1.IsShowDateFormat = false;

        ////    if (this.txtUseDateFormat02.EditValue == true)
        ////        rpt1.IsShowDateFormat02 = true;
        ////    else
        ////        rpt1.IsShowDateFormat02 = false;

        ////    if (this.txtPrintSignaturesAtBottom.EditValue == true)
        ////        rpt1.IsPrintAtBottom = true;
        ////    else
        ////        rpt1.IsPrintAtBottom = false;

        ////    if (this.txtUse2DigitsForTotal.EditValue == true)
        ////        rpt1.IsShow2DigitsInTotal = true;
        ////    else
        ////        rpt1.IsShow2DigitsInTotal = false;

        ////    rpt1.ExportOptions.PrintPreview.DefaultFileName = this.txtSellerVATNumber.EditValue + "_" + Format(this.txtInvoiceDateAndTime.EditValue, "yyyy-MM-dd") + "T" + Format(this.txtInvoiceDateAndTime.EditValue, "HHmmss") + "_" + this.txtInvoiceNo.EditValue;
        ////    rpt1.RequestParameters = false;
        ////    rpt1.PrintingSystem.ExecCommand(DevExpress.XtraPrinting.PrintingSystemCommand.Scale, new object[] { 0.7F });

        ////    rpt1.IsUse2ndPhaseQRCode = true;
        ////    rpt1.ShowPreview();

        ////    // If eInvoice status is cleared then
        ////    // Get the QR code from the Cleared eInvoice Log file

        ////    if (GeteInvoiceStatus(this.txtInvoiceNo.Text) == "CLEARED")
        ////        // GeteInvoiceClearedQRCode
        ////        rpt1.txtQRCode.Text = GeteInvoiceClearedQRCode(this.txtInvoiceNo.Text);
        ////    else
        ////    {
        ////        rpt1.txtQRCode.Text = "";

        ////        rpt1.Watermark.Text = "" + Constants.vbCrLf + Constants.vbCrLf + Constants.vbCrLf + Constants.vbCrLf + "D R A F T   C O P Y" + Constants.vbCrLf + "نسخة المسودة";
        ////        rpt1.Watermark.ForeColor = Color.FromArgb(0, 128, 255);
        ////        rpt1.Watermark.TextTransparency = 125;
        ////        rpt1.Watermark.TextDirection = DirectionMode.Horizontal;


        ////        ReportPrintTool printTool = new ReportPrintTool(rpt1);
        ////        DevExpress.XtraPrinting.PrintingSystemBase printingSystem = printTool.PrintingSystem;
        ////        if (printingSystem.GetCommandVisibility(DevExpress.XtraPrinting.PrintingSystemCommand.Watermark) != DevExpress.XtraPrinting.CommandVisibility.None)
        ////            printingSystem.SetCommandVisibility(DevExpress.XtraPrinting.PrintingSystemCommand.Watermark, DevExpress.XtraPrinting.CommandVisibility.None);
        ////    }

        ////    string reportName = GeteInvoiceXMLFileLocation(this.txtInvoiceNo.Text);
        ////    reportName = reportName.Substring(0, reportName.Length - 4) + ".pdf";

        ////    PdfExportOptions options = new PdfExportOptions();
        ////    options.PdfACompatibility = PdfACompatibility.PdfA3a;
        ////    options.Attachments.Add(new PdfAttachment() { FilePath = appPath + GeteInvoiceXMLFileLocation(this.txtInvoiceNo.Text), Type = "text/xml", Description = "ZATCA-XML-Format" });

        ////    rpt1.ExportToPdf(appPath + reportName, options);
        ////    Process.Start(appPath + reportName);
        ////}

        ////private void PrintPreview_SimplifiedTaxInvoice()
        ////{
        ////    rpt002_SimplifiedInvoiceFormat01 rpt1 = new rpt002_SimplifiedInvoiceFormat01();
        ////    rpt1.Qry201_602VATInvoiceReportTableAdapter.FillByInvoiceNo(rpt1.DsRptEInvoice011.qry201_602VATInvoiceReport, this.txtInvoiceNo.EditValue);
        ////    rpt1.intLogOnAccessLevel = this.intLogOnAccessLevel;
        ////    rpt1.intLogOnUserID = this.intLogOnUserID;
        ////    rpt1.intLogOnUserLevel = this.intLogOnUserLevel;
        ////    rpt1.strLogOnUser = this.strLogonUser;

        ////    for (int i = 0; i <= XtraTabControl1.TabPages.Count - 1; i++)
        ////    {
        ////        if (XtraTabControl1.TabPages(i).Name == "pageCompanyDetails")
        ////        {
        ////            XtraTabControl1.SelectedTabPage = XtraTabControl1.TabPages(i);
        ////            break;
        ////        }
        ////    }

        ////    rpt1.intCompanyID = this.txtCompanyID02.EditValue;

        ////    for (int i = 0; i <= XtraTabControl1.TabPages.Count - 1; i++)
        ////    {
        ////        if (XtraTabControl1.TabPages(i).Name == "pageApprovals")
        ////        {
        ////            XtraTabControl1.SelectedTabPage = XtraTabControl1.TabPages(i);
        ////            break;
        ////        }
        ////    }

        ////    if (this.txtUseDateFormat.EditValue == true)
        ////        rpt1.IsShowDateFormat = true;
        ////    else
        ////        rpt1.IsShowDateFormat = false;

        ////    if (this.txtUseDateFormat02.EditValue == true)
        ////        rpt1.IsShowDateFormat02 = true;
        ////    else
        ////        rpt1.IsShowDateFormat02 = false;

        ////    if (this.txtPrintSignaturesAtBottom.EditValue == true)
        ////        rpt1.IsPrintAtBottom = true;
        ////    else
        ////        rpt1.IsPrintAtBottom = false;

        ////    if (this.txtPrintInLetterHead.EditValue == true)
        ////    {
        ////        rpt1.Watermark.Image = GetLetterHeadFull(this.txtCompanyID02.EditValue);
        ////        rpt1.Watermark.ImageAlign = ContentAlignment.TopLeft;
        ////        rpt1.Watermark.ImageTiling = false;
        ////        rpt1.Watermark.ImageViewMode = ImageViewMode.Stretch;
        ////        rpt1.Watermark.ImageTransparency = 0;
        ////        rpt1.Watermark.ShowBehind = true;
        ////    }

        ////    // Modified for TAX Debit Note
        ////    if (this.txtInvoiceTypeCode.EditValue == "383")
        ////    {
        ////        rpt1.IsDebitNote = true;
        ////        rpt1.lblVATInvoice.Text = "SIMPLIFIED TAX DEBIT NOTE";
        ////        rpt1.lblVATInvAr.Text = "مذكرة الخصم الضريبي المبسطة";

        ////        rpt1.lblInvoiceNo.Text = "Debit Note No:";
        ////        rpt1.lblInvoiceNoAr.Text = "رقم مذكرة الخصم";

        ////        rpt1.lblInvoicePeriod.Text = "Invoice Reference No";
        ////        rpt1.lblInvoicePeriodAr.Text = "رقم مرجع الفاتورة";
        ////        // rpt1.txtInvoicePeriod.Text = "2024-02635"

        ////        rpt1.lblReferenceNo.Text = "Reason for Debit Note";
        ////        rpt1.lblReferenceNoAr.Text = "سبب إصدار سندات الخصم";
        ////    }

        ////    // -----------------------------------------------------------------------------

        ////    // If InvoiceSubTypeCode = "02" Then
        ////    // rpt1.lblVATInvoice.Text = "Simplified Tax Invoice / "
        ////    // rpt1.lblVATInvAr.Text = "فاتورة ضریبیة المبسطة"
        ////    // Else
        ////    // rpt1.lblVATInvoice.Text = "T A X   I N V O I C E / "
        ////    // rpt1.lblVATInvAr.Text = "فاتورة ضريبية"
        ////    // End If

        ////    rpt1.ExportOptions.PrintPreview.DefaultFileName = this.txtSellerVATNumber.EditValue + "_" + Format(this.txtInvoiceDateAndTime.EditValue, "yyyy-MM-dd") + "T" + Format(this.txtInvoiceDateAndTime.EditValue, "HHmmss") + "_" + this.txtInvoiceNo.EditValue;
        ////    rpt1.RequestParameters = false;
        ////    rpt1.PrintingSystem.ExecCommand(DevExpress.XtraPrinting.PrintingSystemCommand.Scale, new object[] { 0.7F });

        ////    rpt1.IsUse2ndPhaseQRCode = true;
        ////    rpt1.ShowPreview();


        ////    // If eInvoice status is cleared then
        ////    // Get the QR code from the Cleared eInvoice Log file

        ////    if (GeteInvoiceStatus(this.txtInvoiceNo.Text) == "REPORTED")
        ////        // GeteInvoiceClearedQRCode
        ////        rpt1.txtQRCode.Text = GeteInvoiceClearedQRCode(this.txtInvoiceNo.Text);
        ////    else
        ////        rpt1.txtQRCode.Text = this.txtFinalQRCode.Text;
        ////}

        ////private void ExportPDF_SimplifiedTaxInvoice()
        ////{
        ////    try
        ////    {
        ////        rpt002_SimplifiedInvoiceFormat01 rpt1 = new rpt002_SimplifiedInvoiceFormat01();
        ////        rpt1.Qry201_602VATInvoiceReportTableAdapter.FillByInvoiceNo(rpt1.DsRptEInvoice011.qry201_602VATInvoiceReport, this.txtInvoiceNo.EditValue);

        ////        // ---------------------------------
        ////        for (int i = 0; i <= XtraTabControl1.TabPages.Count - 1; i++)
        ////        {
        ////            if (XtraTabControl1.TabPages(i).Name == "pageCompanyDetails")
        ////            {
        ////                XtraTabControl1.SelectedTabPage = XtraTabControl1.TabPages(i);
        ////                break;
        ////            }
        ////        }

        ////        rpt1.intCompanyID = this.txtCompanyID02.EditValue;

        ////        for (int i = 0; i <= XtraTabControl1.TabPages.Count - 1; i++)
        ////        {
        ////            if (XtraTabControl1.TabPages(i).Name == "pageApprovals")
        ////            {
        ////                XtraTabControl1.SelectedTabPage = XtraTabControl1.TabPages(i);
        ////                break;
        ////            }
        ////        }

        ////        if (this.txtUseDateFormat.EditValue == true)
        ////            rpt1.IsShowDateFormat = true;
        ////        else
        ////            rpt1.IsShowDateFormat = false;

        ////        if (this.txtUseDateFormat02.EditValue == true)
        ////            rpt1.IsShowDateFormat02 = true;
        ////        else
        ////            rpt1.IsShowDateFormat02 = false;

        ////        if (this.txtPrintSignaturesAtBottom.EditValue == true)
        ////            rpt1.IsPrintAtBottom = true;
        ////        else
        ////            rpt1.IsPrintAtBottom = false;

        ////        // Modified for TAX Debit Note
        ////        if (this.txtInvoiceTypeCode.EditValue == "383")
        ////        {
        ////            rpt1.IsDebitNote = true;
        ////            rpt1.lblVATInvoice.Text = "SIMPLIFIED TAX DEBIT NOTE";
        ////            rpt1.lblVATInvAr.Text = "مذكرة الخصم الضريبي المبسطة";

        ////            rpt1.lblInvoiceNo.Text = "Debit Note No:";
        ////            rpt1.lblInvoiceNoAr.Text = "رقم مذكرة الخصم";

        ////            rpt1.lblInvoicePeriod.Text = "Invoice Reference No";
        ////            rpt1.lblInvoicePeriodAr.Text = "رقم مرجع الفاتورة";
        ////            // rpt1.txtInvoicePeriod.Text = "2024-02635"

        ////            rpt1.lblReferenceNo.Text = "Reason for Debit Note";
        ////            rpt1.lblReferenceNoAr.Text = "سبب إصدار سندات الخصم";
        ////        }

        ////        // -----------------------------------------------------------------------------

        ////        if (this.txtPrintInLetterHead.EditValue == true)
        ////        {
        ////            rpt1.Watermark.Image = GetLetterHeadFull(this.txtCompanyID02.EditValue);
        ////            rpt1.Watermark.ImageAlign = ContentAlignment.TopLeft;
        ////            rpt1.Watermark.ImageTiling = false;
        ////            rpt1.Watermark.ImageViewMode = ImageViewMode.Stretch;
        ////            rpt1.Watermark.ImageTransparency = 0;
        ////            rpt1.Watermark.ShowBehind = true;
        ////        }
        ////        // --------------

        ////        rpt1.IsUse2ndPhaseQRCode = true;

        ////        string reportName = GeteInvoiceXMLFileLocation(this.txtInvoiceNo.Text);
        ////        reportName = reportName.Substring(0, reportName.Length - 4) + ".pdf";

        ////        if (GeteInvoiceStatus(this.txtInvoiceNo.Text) == "REPORTED")
        ////            // GeteInvoiceClearedQRCode
        ////            rpt1.txtQRCode.Text = GeteInvoiceClearedQRCode(this.txtInvoiceNo.Text);
        ////        else
        ////            rpt1.txtQRCode.Text = this.txtFinalQRCode.Text;

        ////        PdfExportOptions options = new PdfExportOptions();
        ////        options.PdfACompatibility = PdfACompatibility.PdfA3a;
        ////        options.Attachments.Add(new PdfAttachment() { FilePath = appPath + GeteInvoiceXMLFileLocation(this.txtInvoiceNo.Text), Type = "text/xml", Description = "ZATCA-XML-Format" });

        ////        rpt1.ExportToPdf(appPath + reportName, options);
        ////        Process.Start(appPath + reportName);
        ////    }
        ////    catch (Exception ex)
        ////    {
        ////    }
        ////}




        // ---------------------------------------------------------------------
        // Report Simplified Invoices
        // ---------------------------------------------------------------------

        ////private void ReportSimplifiedTaxInvoice()
        ////{
        ////    bool success = chilkatGlob.UnlockBundle("WwMfDn.CBX1127_2ThZtySnD3DV");

        ////    if (success != true)
        ////    {
        ////        MessageBox.Show("Failed to Load License", "License", MessageBoxButtons.OK, MessageBoxIcon.Error);
        ////        return;
        ////    }

        ////    if (lblCon.Text != "Connected")
        ////    {
        ////        MessageBox.Show("Please Connect to Portal", "XML", MessageBoxButtons.OK, MessageBoxIcon.Error);
        ////        return;
        ////    }

        ////    Chilkat.Xml xml = new Chilkat.Xml();
        ////    string ds_DigestValue;
        ////    bool Retsuccess;
        ////    Retsuccess = xml.LoadXmlFile(appPath + @"\SignedXML\signedXmlResult1V.xml");

        ////    if (Retsuccess != true)
        ////    {
        ////        MessageBox.Show("Failed to Load XML " + xml.LastErrorText, "XML", MessageBoxButtons.OK, MessageBoxIcon.Error);
        ////        return;
        ////    }

        ////    Chilkat.BinData BinInvoice = new Chilkat.BinData();
        ////    bool Invsuccess = BinInvoice.LoadFile(appPath + @"\SignedXML\signedXmlResult1V.xml");

        ////    if (Invsuccess != true)
        ////    {
        ////        MessageBox.Show("Failed to Load XML Invoice" + xml.LastErrorText, "XML", MessageBoxButtons.OK, MessageBoxIcon.Error);
        ////        return;
        ////    }

        ////    string cbc_UUID = xml.GetChildContent("cbc:UUID");
        ////    ds_DigestValue = xml.GetChildContent("ext:UBLExtensions|ext:UBLExtension|ext:ExtensionContent|sig:UBLDocumentSignatures|sac:SignatureInformation|ds:Signature|ds:SignedInfo|ds:Reference[0]|ds:DigestValue");

        ////    string Base64Invoice = BinInvoice.GetEncoded("base64");

        ////    string strJasonInvoiceHash = "";
        ////    string strJasonUUID = "";
        ////    string strJasonBase64Invoice = "";

        ////    strJasonInvoiceHash = ds_DigestValue;
        ////    strJasonUUID = cbc_UUID;
        ////    strJasonBase64Invoice = Base64Invoice;

        ////    Chilkat.JsonObject json = new Chilkat.JsonObject();
        ////    json.UpdateString("invoiceHash", ds_DigestValue);
        ////    json.UpdateString("uuid", cbc_UUID);
        ////    json.UpdateString("invoice", Base64Invoice);

        ////    rest.AddHeader("accept", "application/json");
        ////    rest.AddHeader("Content-Type", "application/json");
        ////    rest.AddHeader("Accept-Language", "en");
        ////    rest.AddHeader("Accept-Version", "V2");

        ////    Chilkat.StringBuilder sbRequestBody = new Chilkat.StringBuilder();
        ////    json.EmitSb(sbRequestBody);
        ////    Chilkat.StringBuilder sbResponseBody = new Chilkat.StringBuilder();

        ////    rest.AddHeader("Authorization", "Basic " + GetAuthorizeKey());

        ////    // This submits to the Simulation Portal  'https://gw-fatoora.zatca.gov.sa/e-invoicing/simulation/invoices/reporting/single
        ////    // -------------------------------------------------------------------------------------------------------------------------
        ////    // success = rest.FullRequestSb("POST", "/e-invoicing/simulation/invoices/reporting/single", sbRequestBody, sbResponseBody)

        ////    if (success != true)
        ////    {
        ////        MessageBox.Show(rest.LastErrorText);
        ////        return;
        ////    }

        ////    int respStatusCode = rest.ResponseStatusCode;

        ////    // Get current Hash file
        ////    string CurrentInvoiceHashFile;
        ////    CurrentInvoiceHashFile = CurrentHashOfXMLInvoice;

        ////    // Get the Response Status
        ////    string ResponseText;
        ////    ResponseText = sbResponseBody.GetBetween("reportingStatus" + Strings.Chr(34) + ":" + Strings.Chr(34), Strings.Chr(34) + "}");

        ////    // Update the eInvoice Log Table
        ////    try
        ////    {
        ////        InserteInvoiceLog(GetNextICVNumber(), this.txtInvoiceNo.Text, this.txtInvoiceDateAndTime.Text, this.txtInvoiceTypeCode.Text, this.InvoiceSubTypeCode, this.txtPreviousInvoiceHASH.Text, ResponseText, CurrentInvoiceHashFile, this.txtQRCodevalue.Text, this.strLogonUser, DateTime.Now, sbResponseBody.GetAsString(), this.txtFinalQRCode.Text, this.XMLFileNameAsPerZatca, this.txtUniqueInvoiceID.Text, strJasonInvoiceHash, strJasonUUID, strJasonBase64Invoice);
        ////    }
        ////    catch (Exception ex)
        ////    {
        ////        MessageBox.Show("Error occured on Logging eInvoice Details", "Error Occured.");
        ////    }


        ////    if (ResponseText == "REPORTED")
        ////    {
        ////        // Set the Invoice status to Approved & Posted to Zatca

        ////        if (this.IsExecutedFrom_frm00102eInvSalesInvoiceEdit == true)
        ////        {
        ////            if (frm00102eInvSalesInvoiceEdit.UpdateApproved(true, this.strLogonUser, this.txtInvoiceNo.EditValue) == true)
        ////            {
        ////                // Disable all controls
        ////                this.DisableAllControls();
        ////                this.Qry90132InvoiceSubmissionStatusTableAdapter.FillByInvoiceNo(this.DseInvoiceDataset1.qry90132InvoiceSubmissionStatus, this.txtInvoiceNo.EditValue);
        ////                // Lock All controls in the Invoice Master
        ////                RefreshMainScreen();

        ////                this.Activate();
        ////            }
        ////        }

        ////        if (this.IsExecutedFrom_frm00103eInvCreditNoteEdit == true)
        ////        {
        ////            if (frm00103eInvCreditNoteEdit.UpdateApproved(true, this.strLogonUser, this.txtInvoiceNo.EditValue) == true)
        ////            {
        ////                // Disable all controls
        ////                this.DisableAllControls();
        ////                this.Qry90132InvoiceSubmissionStatusTableAdapter.FillByInvoiceNo(this.DseInvoiceDataset1.qry90132InvoiceSubmissionStatus, this.txtInvoiceNo.EditValue);
        ////                // Lock All controls in the Invoice Master
        ////                RefreshMainScreen();

        ////                this.Activate();
        ////            }
        ////        }

        ////        if (this.IsExecutedFrom_frm00109eInvoiceLog == true)
        ////        {
        ////            if (frm00102eInvSalesInvoiceEdit.UpdateApproved(true, this.strLogonUser, this.txtInvoiceNo.EditValue) == true)
        ////            {
        ////                // Disable all controls
        ////                this.DisableAllControls();
        ////                this.Qry90132InvoiceSubmissionStatusTableAdapter.FillByInvoiceNo(this.DseInvoiceDataset1.qry90132InvoiceSubmissionStatus, this.txtInvoiceNo.EditValue);
        ////                // Lock All controls in the Invoice Master
        ////                RefreshMainScreen();

        ////                this.Activate();
        ////            }

        ////            if (this.txtInvoiceTypeCode.Text == "381")
        ////            {
        ////                if (frm00103eInvCreditNoteEdit.UpdateApproved(true, this.strLogonUser, this.txtInvoiceNo.EditValue) == true)
        ////                {
        ////                    // Disable all controls
        ////                    this.DisableAllControls();
        ////                    this.Qry90132InvoiceSubmissionStatusTableAdapter.FillByInvoiceNo(this.DseInvoiceDataset1.qry90132InvoiceSubmissionStatus, this.txtInvoiceNo.EditValue);
        ////                    // Lock All controls in the Invoice Master
        ////                    RefreshMainScreen();

        ////                    this.Activate();
        ////                }
        ////            }
        ////        }


        ////        MessageBox.Show("eInvoice has been Successfully Reported to Zatca Fatoora Portal.", "E-Invoice Reporting Successfully.");
        ////    }
        ////    else
        ////        MessageBox.Show("eInvoice NOT Reported to Zatca Fatoora Portal.", "E-Invoice Reporting Failed.");
        ////}


        // ---------------------------------------------------------------------
        // Clear Tax Invoices
        // ---------------------------------------------------------------------
        public string CurrentHashOfXMLInvoice;


        private async void ClearStandardTaxInvoice([FromBody] string invoiceNo, string ConnectionStatus)
        {
            try
            {
                strLogonUser = HttpContext.Session.GetString("UserName");
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    _logger.LogError("Tenant/DB Context not available");
                    return;
                }

                // Unlock Chilkat bundle
                bool success = chilkatGlob.UnlockBundle("WwMfDn.CBX1127_2ThZtySnD3DV");
                if (!success)
                {
                    _logger.LogError("Failed to load license");
                    return;
                }

                // Check connection
                if (ConnectionStatus != "Connected")
                {
                    _logger.LogError("Please connect to portal first");
                    return;
                }


                // Load XML
                var xml = new Chilkat.Xml();
                bool retSuccess = xml.LoadXmlFile(Path.Combine(appPath, "SignedXML", "signedXmlResult1.xml"));
                string defaultCompanyString = HttpContext.Session.GetString("DefaultcompanyID") ?? "";
                byte defaultCompanyByte = 0; // or any default value you want

                if (!string.IsNullOrEmpty(defaultCompanyString))
                {
                    // Safest way (avoids exceptions):
                    byte.TryParse(defaultCompanyString, out defaultCompanyByte);
                    // Now defaultCompanyByte holds the parsed value, or 0 if parsing failed.
                }

                // Now use defaultCompanyByte as needed


                byte companyId = defaultCompanyByte;

                var company = dbContext.Tbl901CompanyDetails
               .FirstOrDefault(c => c.CompanyId == companyId);

                string signedXmlPath = GetSignedXmlFilePath(companyId, Path.Combine(appPath, "SignedXML", "signedXmlResult1.xml"));
               
                bool Retsuccess = xml.LoadXmlFile(signedXmlPath);

                if (!retSuccess)
                {
                    _logger.LogError("Failed to load XML: {Error}", xml.LastErrorText);
                    return;
                }

                // Load Invoice binary
                var binInvoice = new Chilkat.BinData();
                bool invSuccess = binInvoice.LoadFile(Path.Combine(appPath, "SignedXML", "signedXmlResult1.xml"));
                if (!invSuccess)
                {
                    _logger.LogError("Failed to load XML Invoice: {Error}", xml.LastErrorText);
                    return;
                }

                // Extract values
                string cbc_UUID = xml.GetChildContent("cbc:UUID");
                string ds_DigestValue = xml.GetChildContent("ext:UBLExtensions|ext:UBLExtension|ext:ExtensionContent|sig:UBLDocumentSignatures|sac:SignatureInformation|ds:Signature|ds:SignedInfo|ds:Reference[0]|ds:DigestValue");
                string base64Invoice = binInvoice.GetEncoded("base64");

                // Build JSON
                var json = new Chilkat.JsonObject();
                json.UpdateString("invoiceHash", ds_DigestValue);
                json.UpdateString("uuid", cbc_UUID);
                json.UpdateString("invoice", base64Invoice);

                var sbRequestBody = new Chilkat.StringBuilder();
                json.EmitSb(sbRequestBody);

                var sbResponseBody = new Chilkat.StringBuilder();

                // Set Headers
                rest.AddHeader("accept", "application/json");
                rest.AddHeader("Content-Type", "application/json");
                rest.AddHeader("Accept-Language", "en");
                rest.AddHeader("Clearance-Status", "1");
                rest.AddHeader("Accept-Version", "V2");
                rest.AddHeader("Authorization", "Basic " + GetAuthorizeKey());

                // Call ZATCA API - Simulation
                success = rest.FullRequestSb("POST", "/e-invoicing/simulation/invoices/clearance/single", sbRequestBody, sbResponseBody);

                if (!success)
                {
                    _logger.LogError("Request Failed: {Error}", rest.LastErrorText);
                    return;
                }
                string SimulationJsonResponse = sbResponseBody.GetAsString();

                // Now parse JSON
                var obj = JObject.Parse(SimulationJsonResponse);

                // Select required fields
                var simulationresult = new
                {
                    ClearanceStatus = (string)obj["clearanceStatus"],
                    ClearedInvoice = (string)obj["clearedInvoice"]
                };


                int respStatusCode = rest.ResponseStatusCode;


                // Extract status & cleared invoice
                string responseText = simulationresult.ClearanceStatus;
                string responseClearedInvoice = simulationresult.ClearedInvoice;
                //  string responseText = sbResponseBody.GetBetween("clearanceStatus" + "\"\":\"", "\",");
                //string responseClearedInvoice = sbResponseBody.GetBetween("clearedInvoice" + "\"\":\"", "\"");

                string txtQRCodevalue = "", txtFinalQRCode = "", txtUniqueInvoiceID = "", txtPreviousInvoiceHASH = "";
                short txtInvoiceTypeCode = 0;
                DateTime txtInvoiceDateAndTime = DateTime.UtcNow;
                string qrCodeBase64 = "", XMLFileNameAsPerZatca = "";
                txtPreviousInvoiceHASH = gLastSuccessfulSubmittedHashfile;

                if (!string.IsNullOrEmpty(responseClearedInvoice))
                {
                    var result = await dbContext.Tbl20161VatinvoiceMasters
                        .Where(x => x.InvoiceNo == invoiceNo)
                        .Select(x => new
                        {
                            x.SellerVatnumber,
                            x.InvoiceDateWtTime,
                            x.InvoiceUuid,
                            x.InvoiceTypeCode
                        })
                        .FirstOrDefaultAsync();

                    if (result != null)
                    {
                        string safeDate = result.InvoiceDateWtTime.HasValue
                            ? result.InvoiceDateWtTime.Value.ToString("yyyyMMddHHmmss")
                            : "00000000000000";

                        XMLFileNameAsPerZatca = Path.Combine("SignedXML", $"{result.SellerVatnumber}_{safeDate}_{invoiceNo}.xml");

                        Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(appPath, XMLFileNameAsPerZatca)));

                        string xmlFullPath = Path.Combine(appPath, XMLFileNameAsPerZatca);
                        ComposeStdInvoices(xmlFullPath, responseClearedInvoice);
                        qrCodeBase64 = GetQRCodeBase64(xmlFullPath);

                        txtFinalQRCode = qrCodeBase64;
                        txtQRCodevalue = qrCodeBase64;
                        txtInvoiceTypeCode = result.InvoiceTypeCode ?? 0;

                        txtInvoiceDateAndTime = result.InvoiceDateWtTime ?? DateTime.Now;

                        txtUniqueInvoiceID = result.InvoiceUuid;

                    }
                    else
                    {
                        _logger.LogWarning("Invoice not found for InvoiceNo {InvoiceNo}", invoiceNo);
                    }
                    if (GetAzureStatus(companyId))
                    {
                        // Upload XML to Azure
                       await UploadXmlToAzureIfEnabledAsync(appPath + XMLFileNameAsPerZatca, companyId);

                        // Delete local file after successful upload
                        string filePath = Path.Combine(appPath, XMLFileNameAsPerZatca);
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }

                }

                try
                {
                    await InserteInvoiceLog(
                        GetNextICVNumber(),
                        invoiceNo,
                        DateTime.Now,
                        Convert.ToInt16(txtInvoiceTypeCode),
                        InvoiceSubTypeCode,
                        txtPreviousInvoiceHASH,
                        responseText,
                        CurrentHashOfXMLInvoice,
                        txtFinalQRCode,
                        strLogonUser,
                        DateTime.Now,
                        SimulationJsonResponse,
                        txtFinalQRCode,
                        XMLFileNameAsPerZatca,
                        txtUniqueInvoiceID,
                        ds_DigestValue,
                        cbc_UUID,
                        base64Invoice
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while logging eInvoice details");
                }

                if (responseText == "CLEARED")
                {
                    _logger.LogInformation("Invoice {InvoiceNo} CLEARED successfully", invoiceNo);
                }
                else
                {
                    _logger.LogWarning("Invoice {InvoiceNo} NOT CLEARED. Response: {Response}", invoiceNo, SimulationJsonResponse);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while clearing tax invoice");
            }
        }



        public async Task<int> InserteInvoiceLog(
    long icv,
    string eInvoiceNo,
    DateTime eInvoiceDateTime,
    short eInvoiceTypeCode,
    string eInvoiceSubTypeCode,
    string previousHashFile,
    string invoiceStatus,
    string currentHashFile,
    string qrCode,
    string submittedBy,
    DateTime submittedOn,
    string submissionStatusText,
    string invoiceQRCode,
    string xmlFileLocation,
    string invoiceUUID,
    string jasonInvoiceHash,
    string jasonUUID,
    string jasonBase64Invoice)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var result = await dbContext.Database.ExecuteSqlInterpolatedAsync(
                        $@"EXEC sp90122InserteInvoiceLogTable 
                        {icv}, 
                        {eInvoiceNo}, 
                        {eInvoiceDateTime}, 
                        {eInvoiceTypeCode}, 
                        {eInvoiceSubTypeCode}, 
                        {previousHashFile}, 
                        {invoiceStatus}, 
                        {currentHashFile}, 
                        {qrCode}, 
                        {submittedBy}, 
                        {submittedOn}, 
                        {submissionStatusText}, 
                        {invoiceQRCode}, 
                        {xmlFileLocation}, 
                        {invoiceUUID}, 
                        {jasonInvoiceHash}, 
                        {jasonUUID}, 
                        {jasonBase64Invoice}"
                    );

                    return result; // number of rows affected
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while inserting eInvoice log");
                    throw;
                }
            }

            return 0; // no dbContext available
        }



        public string GetPreviousHashFile()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var record = dbContext.Qry90131eInvoiceLogOnlyClearedAndReporteds
                        .OrderByDescending(x => x.Icv) // MAX(ICV)
                        .Select(x => x.CurrentHashFile)
                        .FirstOrDefault();

                    if (!string.IsNullOrEmpty(record))
                    {
                        gLastSuccessfulSubmittedHashfile = record;
                        return gLastSuccessfulSubmittedHashfile;
                    }
                }
                catch (InvalidCastException)
                {
                    return "NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYzIzOWRkNGU5MWI0NjcyOWQ3M2EyN2ZiNTdlOQ==";
                }
            }

            return string.Empty;
        }

        public int GetNextICVNumber()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var lastIcv = dbContext.Tbl90121eInvoiceLogTables
                        .Max(x => (int?)x.Icv) ?? 0; // Handle nulls safely

                    return lastIcv + 1;
                }
                catch (InvalidCastException)
                {
                    return 1; // default when cast fails
                }
            }

            return 1; // fallback if no DB context
        }


        private string GetAuthorizeKey()
        {
            // Fetch details from DB (your existing function)
            GetEGSUnitDetails(1);

            string base64UserBasic = gProductionCSIDString;
            string base64UserBasicSec = gProductionCSIDSecret;

            // Combine user and secret
            string b64Authorization = $"{base64UserBasic}:{base64UserBasicSec}";

            // Chilkat encryption
            Chilkat.Crypt2 cryptAuthorization = new Chilkat.Crypt2
            {
                Charset = "utf-8",
                CryptAlgorithm = "none",
                EncodingMode = "base64"
            };

            string jb64Authorization = cryptAuthorization.EncryptStringENC(b64Authorization);

            return jb64Authorization;
        }



        public string ComposeStdInvoices(string xmlPath, string base64Xml)
        {
            if (string.IsNullOrWhiteSpace(xmlPath))
                throw new ArgumentException("XML file path cannot be null or empty.", nameof(xmlPath));

            if (string.IsNullOrWhiteSpace(base64Xml))
                throw new ArgumentException("Base64 XML string cannot be null or empty.", nameof(base64Xml));

            try
            {
                // Decode Base64 into raw XML text
                byte[] xmlBytes = Convert.FromBase64String(base64Xml);
                string xmlContent = System.Text.Encoding.UTF8.GetString(xmlBytes);

                // Save to file
                System.IO.File.WriteAllText(xmlPath, xmlContent);

                return xmlPath;
            }
            catch (FormatException ex)
            {
                throw new Exception("Invalid Base64 XML string provided.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to compose XML file at {xmlPath}", ex);
            }
        }

        /// <summary>
        /// Extracts QR code Base64 value from the cleared invoice XML.
        /// </summary>
        /// <param name="xmlFilePath">Full path to the XML file.</param>
        /// <returns>Base64 QR code string.</returns>
        public string GetQRCodeBase64(string xmlFilePath)
        {
            if (string.IsNullOrWhiteSpace(xmlFilePath))
                throw new ArgumentException("XML file path cannot be null or empty.", nameof(xmlFilePath));

            if (!System.IO.File.Exists(xmlFilePath))
                throw new FileNotFoundException("XML file not found", xmlFilePath);

            var doc = new XmlDocument { PreserveWhitespace = true };
            doc.Load(xmlFilePath);

            // Setup namespace manager
            var nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
            nsmgr.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");

            // Find <cac:AdditionalDocumentReference> (2nd occurrence)
            var nodes = doc.GetElementsByTagName("cac:AdditionalDocumentReference");
            if (nodes.Count < 2)
                throw new Exception("The required AdditionalDocumentReference[2] element was not found.");

            var additionalDocRef = nodes[1]; // zero-based index → 2nd element

            // Use SelectSingleNode with namespace manager
            var embeddedNode = additionalDocRef.SelectSingleNode("cac:Attachment/cbc:EmbeddedDocumentBinaryObject", nsmgr);

            if (embeddedNode == null)
                throw new Exception("EmbeddedDocumentBinaryObject not found in AdditionalDocumentReference[2].");

            return embeddedNode.InnerText.Trim();
        }


        // Global variables (replace with properties or DTO if preferred)
        private string gPrivateKey;
        private string gProductionCSIDString;
        private string gProductionCSIDSecret;
        private string gProductionPEM;

        public bool GetEGSUnitDetails(int egsUnitID)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var unit = dbContext.Tbl901CompanyEgsunits
                        .FirstOrDefault(x => x.EgsunitId == egsUnitID);

                    if (unit != null)
                    {
                        gPrivateKey = unit.PrivateKey;
                        gProductionCSIDString = unit.ProductionCsidstring;
                        gProductionCSIDSecret = unit.ProductionCsidsecret;
                        gProductionPEM = unit.ProductionPem;
                        // if needed: gProductionAuthKeyPair = unit.ProductionAuthKeyPair;

                        return true;
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error retrieving EGS Unit details: {ex.Message}");
                    return false;
                }
            }

            return false;
        }


        // Expose retrieved values safely
        public (string PrivateKey, string CSIDString, string CSIDSecret, string PEM) GetValues()
        {
            return (gPrivateKey, gProductionCSIDString, gProductionCSIDSecret, gProductionPEM);
        }



        ////private void btnPreviewStandard01_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        ////{
        ////    this.PopulateThruTabs();

        ////    if (this.txtInvoiceTypeCode.Text == "388" | this.txtInvoiceTypeCode.Text == "383")
        ////    {
        ////        // Sales Invoices

        ////        if (this.InvoiceSubTypeCode == "01")
        ////        {
        ////            if (this.txtInvoiceCurrencyCode.Text == "SAR")
        ////                // Standard eInvoice
        ////                PrintPreview_RegularTaxInvoice();
        ////            else
        ////                // Standard eInvoice - Foreign Currency
        ////                PrintPreview_RegularTaxInvoice_OC();
        ////        }
        ////        else if (this.InvoiceSubTypeCode == "02")
        ////            // Simplified Tax Invoice
        ////            PrintPreview_SimplifiedTaxInvoice();
        ////    }
        ////    else if (this.txtInvoiceTypeCode.Text == "381")
        ////    {
        ////        // Credit note

        ////        if (this.InvoiceSubTypeCode == "01")
        ////        {
        ////            if (this.txtInvoiceCurrencyCode.Text == "SAR")
        ////                // Standard Credit Note
        ////                PrintPreview_RegularTaxCreditNote();
        ////            else
        ////                // Standard Credit Note - Foreign Currency
        ////                PrintPreview_RegularTaxCreditNote_OC();
        ////        }
        ////        else if (this.InvoiceSubTypeCode == "02")
        ////            // Simplified Credit Note
        ////            PrintPreview_SimplifiedTaxCreditNote();
        ////    }
        ////    else if (this.txtInvoiceTypeCode.Text == "386")
        ////    {
        ////        if (this.InvoiceSubTypeCode == "01")
        ////        {
        ////            if (this.txtInvoiceCurrencyCode.Text == "SAR")
        ////                // Standard eInvoice
        ////                PrintPreview_RegularTaxInvoice();
        ////            else
        ////                // Standard eInvoice - Foreign Currency
        ////                PrintPreview_RegularTaxInvoice_OC();
        ////        }
        ////        else if (this.InvoiceSubTypeCode == "02")
        ////            // Simplified Tax Invoice
        ////            PrintPreview_SimplifiedTaxInvoice();
        ////    }
        ////}

        ////private void btnConnectToZatca_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        ////{
        ////    ConnectToPortal();
        ////}


        ////private void btnSubmitToZatca_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        ////{
        ////    FillData(this.txtInvoiceNo.Text);
        ////    SubmitInvoiceToZatca();
        ////}

        [HttpPost]
        public async Task<IActionResult> SubmitInvoiceToZatca(string invoiceNo, string connectionStatus, InvoiceData aInvoiceData)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    // 1. Check if already submitted
                    if (CheckIfInvoiceAlreadySubmittedToZatca(invoiceNo))
                    {
                        return Conflict(new
                        {
                            Message = "This Invoice is already submitted to ZATCA Fatoora Portal. Please check again."
                        });
                    }
                    string signedFile;
                    string currentHash;

                    if (InvoiceSubTypeCode == "01") // Standard Tax Invoice
                    {
                        GetPreviousHashFile();
                        if (aInvoiceData.Master.InvoiceCurrencyCode == "1")
                        {
                            GenerateUBLFile(aInvoiceData);
                        }
                        else
                        {
                            GenerateUBLFile_ForeignCurrency(aInvoiceData);
                        }

                        // Save or set signed file path
                        string outputPath = Path.Combine(appPath, "SignedXML", "signedXmlResult1.xml");
                        // System.IO.File.WriteAllText(outputPath, signedFile);  // assuming signedFile is XML string

                        currentHash = ENInvoiceHASH(outputPath);
                        CurrentHashOfXMLInvoice = currentHash;
                        ClearStandardTaxInvoice(invoiceNo, connectionStatus);

                        return Ok(new
                        {
                            Message = "Standard Invoice prepared successfully",
                            //  InvoiceNo = request.InvoiceNo,
                            CurrentHash = currentHash,
                            SignedFile = outputPath
                        });
                    }

                    else if (InvoiceSubTypeCode == "02") // Simplified
                    {

                        GenerateUBLFile(aInvoiceData);
                        signedFile = Path.Combine(appPath, "SignedXML", "signedXmlResult1.xml");
                        await SignXML(aInvoiceData, invoiceNo);
                        currentHash = ENInvoiceHASH(signedFile);

                        ReportSimplifiedTaxInvoice(invoiceNo, connectionStatus);

                        return Ok(new
                        {
                            Message = "Simplified Invoice prepared successfully",
                            // InvoiceNo = request.InvoiceNo,
                            CurrentHash = currentHash,
                            SignedFile = signedFile
                        });
                    }

                    return BadRequest(new { Message = "Unsupported Invoice SubTypeCode" });
                }

                return StatusCode(500, new { Message = "Tenant/DbContext could not be resolved." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error occurred while submitting invoice", Error = ex.Message });
            }
        }



        public bool CheckIfInvoiceAlreadySubmittedToZatca(string invoiceNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return dbContext.Qry90132InvoiceSubmissionStatuses
                    .Any(x => x.InvoiceNo == invoiceNo &&
                             (x.SubmissionStatus == "CLEARED" || x.SubmissionStatus == "REPORTED"));
            }

            return false;
        }


        // Background worker method
        private async void ReportSimplifiedTaxInvoice(string invoiceNo,string connectionStatus)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    _logger.LogError("Unable to resolve tenant context");
                    return;
                }

                // Unlock license
                bool success = chilkatGlob.UnlockBundle("WwMfDn.CBX1127_2ThZtySnD3DV");
                if (!success)
                {
                    _logger.LogError("Chilkat license unlock failed: {Error}", chilkatGlob.LastErrorText);
                    return;
                }

                if (connectionStatus != "Connected")
                {
                    _logger.LogWarning("Portal not connected for invoice {InvoiceNo}", invoiceNo);
                    return;
                }
                Xml xml = new Xml();
                bool Retsuccess;

                // 🔹 Get path (Azure or Local)
                int companyId = 1;
                string localPath = Path.Combine(appPath, @"SignedXML\signedXmlResult1V.xml");

                // Assuming GetSignedXmlFilePath1 is updated to C# and returns string
                string signedXmlPath = await GetSignedXmlFilePath1Async(companyId, localPath);

                // ✅ Load XML using resolved path
                Retsuccess = xml.LoadXmlFile(signedXmlPath);

                if (!Retsuccess)
                {
                   // MessageBox.Show("Failed to Load XML: " + xml.LastErrorText, "XML", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Load XML
                 xml = new Chilkat.Xml();
                if (!xml.LoadXmlFile(Path.Combine(appPath, "SignedXML", "signedXmlResult1V.xml")))
                {
                    _logger.LogError("Failed to load XML: {Error}", xml.LastErrorText);
                    return;
                }

                var binInvoice = new Chilkat.BinData();
                if (!binInvoice.LoadFile(Path.Combine(appPath, "SignedXML", "signedXmlResult1V.xml")))
                {
                    _logger.LogError("Failed to load signed XML invoice: {Error}", xml.LastErrorText);
                    return;
                }

                string uuid = xml.GetChildContent("cbc:UUID");
                string digestValue = xml.GetChildContent(
                    "ext:UBLExtensions|ext:UBLExtension|ext:ExtensionContent|sig:UBLDocumentSignatures|" +
                    "sac:SignatureInformation|ds:Signature|ds:SignedInfo|ds:Reference[0]|ds:DigestValue"
                );
                string base64Invoice = binInvoice.GetEncoded("base64");

                // Prepare request JSON
                var json = new Chilkat.JsonObject();
                json.UpdateString("invoiceHash", digestValue);
                json.UpdateString("uuid", uuid);
                json.UpdateString("invoice", base64Invoice);

                rest.AddHeader("accept", "application/json");
                rest.AddHeader("Content-Type", "application/json");
                rest.AddHeader("Accept-Language", "en");
                rest.AddHeader("Accept-Version", "V2");
                rest.AddHeader("Authorization", "Basic " + GetAuthorizeKey());

                var sbRequestBody = new Chilkat.StringBuilder();
                json.EmitSb(sbRequestBody);
                var sbResponseBody = new Chilkat.StringBuilder();

                // TODO: Uncomment for real submission
                // success = rest.FullRequestSb("POST", "/e-invoicing/simulation/invoices/reporting/single", sbRequestBody, sbResponseBody);

                if (!success)
                {
                    _logger.LogError("REST request failed: {Error}", rest.LastErrorText);
                    return;
                }

                string responseText = sbResponseBody.GetBetween(
                    "reportingStatus" + (char)34 + ":" + (char)34,
                    (char)34 + "}"
                );

                string currentInvoiceHashFile = CurrentHashOfXMLInvoice;

                // Fetch invoice master details
                var result = await dbContext.Tbl20161VatinvoiceMasters
                    .Where(x => x.InvoiceNo == invoiceNo)
                    .Select(x => new
                    {
                        x.SellerVatnumber,
                        x.InvoiceDateWtTime,
                        x.InvoiceUuid,
                        x.InvoiceTypeCode
                    })
                    .FirstOrDefaultAsync();

                string xmlFileNameAsPerZatca = "";
                string qrCodeBase64 = "";
                DateTime invoiceDate = DateTime.UtcNow;
                string uniqueInvoiceId = "";
                short invoiceTypeCode = 0;
                string txtPreviousInvoiceHASH = gLastSuccessfulSubmittedHashfile;

                if (result != null)
                {
                    string safeDate = result.InvoiceDateWtTime?.ToString("yyyyMMddHHmmss") ?? "00000000000000";
                    xmlFileNameAsPerZatca = Path.Combine("SignedXML", $"{result.SellerVatnumber}_{safeDate}_{invoiceNo}.xml");
                    Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(appPath, xmlFileNameAsPerZatca)) ?? appPath);

                    string xmlFullPath = Path.Combine(appPath, xmlFileNameAsPerZatca);
                    qrCodeBase64 = GetQRCodeBase64(xmlFullPath);

                    invoiceDate = result.InvoiceDateWtTime ?? DateTime.Now;
                    invoiceTypeCode = result.InvoiceTypeCode ?? 0;
                    uniqueInvoiceId = result.InvoiceUuid;
                }

                // Insert log
                try
                {
                    await InserteInvoiceLog(
                        GetNextICVNumber(),
                        invoiceNo,
                        invoiceDate,
                        invoiceTypeCode,
                        InvoiceSubTypeCode,
                        txtPreviousInvoiceHASH,
                        responseText,
                        currentInvoiceHashFile,
                        qrCodeBase64,
                        strLogonUser,
                        DateTime.Now,
                        sbResponseBody.GetAsString(),
                        qrCodeBase64,
                        xmlFileNameAsPerZatca,
                        uniqueInvoiceId,
                        digestValue,
                        uuid,
                        base64Invoice
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while logging eInvoice details");
                }

                // Update invoice status
                if (responseText == "REPORTED")
                {
                    var userName = HttpContext.Session.GetString("UserName");

                    var invoice = dbContext.Tbl20161VatinvoiceMasters.FirstOrDefault(v => v.InvoiceNo == invoiceNo);
                    if (invoice != null)
                    {
                        invoice.IsApproved = true;
                        invoice.ApprovedOn = DateTime.Now;
                        invoice.ApprovedBy = userName;

                        await dbContext.SaveChangesAsync();
                    }

                    _logger.LogInformation("Invoice {InvoiceNo} successfully reported to ZATCA", invoiceNo);
                }
                else
                {
                    _logger.LogWarning("Invoice {InvoiceNo} NOT reported to ZATCA", invoiceNo);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in ReportSimplifiedTaxInvoiceInternal");
            }
        }





        ///B2C
        [HttpPost]
        public void PrepareSimplifiedTaxInvoice(string invoiceNo, InvoiceData aInvoiceData)
        {

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return; // Exit if DB context cannot be resolved
            }

            // Assign values
            var counterValue = GetNextICVNumber();
            var uniqueInvoiceId = Guid.NewGuid().ToString();

            GetPreviousHashFile();
            var previousHash = gLastSuccessfulSubmittedHashfile;

            // Generate UBL, sign, authorize
            GenerateUBLFile(aInvoiceData);
             SignXML(aInvoiceData, invoiceNo);
            GetAuthorizeKey();

            var userName = HttpContext.Session.GetString("UserName");

            var invoice = dbContext.Tbl20161VatinvoiceMasters.FirstOrDefault(v => v.InvoiceNo == invoiceNo);
            if (invoice == null)
            {
                return; // Exit if voucher not found
            }

            // Update voucher approval info
            invoice.IsApproved = true;
            invoice.ApprovedOn = DateTime.Now;
            invoice.ApprovedBy = userName;

            dbContext.SaveChanges();

            // Get submission status using LINQ (optional, no return)
            var submissionStatus = dbContext.Qry90132InvoiceSubmissionStatuses
                .Where(x => x.InvoiceNo == invoiceNo)
                .FirstOrDefault();
                //Me.DisableAllControls()
                // RefreshMainScreen()

                //Me.Activate()
            // You can still log or process submissionStatus internally if needed
        }




        ////private void btnExportPDFA3001_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        ////{
        ////    this.PopulateThruTabs();

        ////    if (this.txtInvoiceTypeCode.Text == "388" | this.txtInvoiceTypeCode.Text == "383")
        ////    {
        ////        if (this.InvoiceSubTypeCode == "01")
        ////        {
        ////            if (this.txtInvoiceCurrencyCode.Text == "SAR")
        ////                ExportPDFA3_RegularTaxInvoice();
        ////            else
        ////                ExportPDFA3_RegularTaxInvoice_OC();
        ////        }
        ////        else if (this.InvoiceSubTypeCode == "02")
        ////            ExportPDF_SimplifiedTaxInvoice();
        ////    }
        ////    else if (this.txtInvoiceTypeCode.Text == "381")
        ////    {
        ////        // CREDIT NOTES

        ////        if (this.InvoiceSubTypeCode == "01")
        ////        {
        ////            if (this.txtInvoiceCurrencyCode.Text == "SAR")
        ////                // Standard Credit Note
        ////                ExportPDFA3_RegularTaxCreditNote();
        ////            else
        ////                // Standard Credit Note - Foreign Currency
        ////                ExportPDFA3_RegularTaxCreditNote_OC();
        ////        }
        ////        else if (this.InvoiceSubTypeCode == "02")
        ////            // Simplified Credit Note
        ////            ExportPDFA3_SimplifiedTaxCreditNote();
        ////    }
        ////    else if (this.txtInvoiceTypeCode.Text == "386")
        ////    {
        ////        if (this.InvoiceSubTypeCode == "01")
        ////        {
        ////            if (this.txtInvoiceCurrencyCode.Text == "SAR")
        ////                ExportPDFA3_RegularTaxInvoice();
        ////            else
        ////                ExportPDFA3_RegularTaxInvoice_OC();
        ////        }
        ////        else if (this.InvoiceSubTypeCode == "02")
        ////            ExportPDF_SimplifiedTaxInvoice();
        ////    }
        ////}


        // ---------------------------------------------------------------------
        // Get Authorization Key Pair from zatca using CCSID String & CCSID Secret
        // ---------------------------------------------------------------------
        // Private Sub btnGetAuthorizationKeyPair_Click(sender As Object, e As EventArgs) Handles btnGetAuthorizationKeyPair.Click

        // Dim BinAuth As Chilkat.BinData = New Chilkat.BinData()
        // Dim Authsuccess As Boolean = BinAuth.LoadFile(appPath & "\Certificates\CCSIDString.txt")

        // If Authsuccess <> True Then
        // MessageBox.Show("Failed to Load Authorization", "XML", MessageBoxButtons.OK, MessageBoxIcon.[Error])
        // Return
        // End If

        // Dim Base64UserBasic As String = BinAuth.GetString("utf-8")

        // Dim BinAuthSecret As Chilkat.BinData = New Chilkat.BinData()
        // Dim AuthSecsuccess As Boolean = BinAuthSecret.LoadFile(appPath & "\Certificates\CCSIDSecret.txt")

        // If AuthSecsuccess <> True Then
        // MessageBox.Show("Failed to Load Authorization Secret", "XML", MessageBoxButtons.OK, MessageBoxIcon.[Error])
        // Return
        // End If

        // Dim Base64UserBasicSec As String = BinAuthSecret.GetString("utf-8")

        // MessageBox.Show(Base64UserBasic & ":" & Base64UserBasicSec)

        // Dim b64Authorization As String = Base64UserBasic & ":" & Base64UserBasicSec
        // Dim CryptAuthorization As Chilkat.Crypt2 = New Chilkat.Crypt2()
        // CryptAuthorization.Charset = "utf-8"
        // CryptAuthorization.CryptAlgorithm = "none"
        // CryptAuthorization.EncodingMode = "base64"
        // Dim jb64Authorization As String = CryptAuthorization.EncryptStringENC(b64Authorization)

        // Me.txtAuthorization.Text = jb64Authorization

        // End Sub



        // ---------------------------------------------------------------------
        // Run Complaince Checks
        // ---------------------------------------------------------------------
        // Private Sub btnRunComplianceAPI_Click(sender As Object, e As EventArgs) Handles btnRunComplianceAPI.Click

        // Dim success As Boolean = chilkatGlob.UnlockBundle("WwMfDn.CBX1127_2ThZtySnD3DV")

        // If success <> True Then
        // MessageBox.Show("Failed to Load License", "License", MessageBoxButtons.OK, MessageBoxIcon.[Error])
        // Return
        // End If

        // If lblCon.Text <> "Connected" Then
        // MessageBox.Show("Please Connect to Portal", "XML", MessageBoxButtons.OK, MessageBoxIcon.[Error])
        // Return
        // End If

        // Dim xml As Chilkat.Xml = New Chilkat.Xml()
        // Dim ds_DigestValue As String
        // Dim Retsuccess As Boolean

        // 'Changed  by Saju for Standard Invoices
        // 'Retsuccess = xml.LoadXmlFile(appPath & "\SignedXML\signedXmlResult1V.xml")
        // 'Retsuccess = xml.LoadXmlFile(appPath & "\SignedXML\signedXmlResult1.xml")

        // Retsuccess = xml.LoadXmlFile(appPath & XMLFileNameAsPerZatca)

        // If Retsuccess <> True Then
        // MessageBox.Show("Failed to Load XML " & xml.LastErrorText, "XML", MessageBoxButtons.OK, MessageBoxIcon.[Error])
        // Return
        // End If

        // Dim BinInvoice As Chilkat.BinData = New Chilkat.BinData()
        // 'Changed  by Saju for Standard Invoices
        // 'Dim Invsuccess As Boolean = BinInvoice.LoadFile(appPath & "\SignedXML\signedXmlResult1.xml")
        // 'Dim Invsuccess As Boolean = BinInvoice.LoadFile(appPath & "\SignedXML\signedXmlResult1V.xml")

        // Dim Invsuccess As Boolean = BinInvoice.LoadFile(appPath & XMLFileNameAsPerZatca)

        // If Invsuccess <> True Then
        // MessageBox.Show("Failed to Load XML Invoice" & xml.LastErrorText, "XML", MessageBoxButtons.OK, MessageBoxIcon.[Error])
        // Return
        // End If

        // Dim cbc_UUID As String = xml.GetChildContent("cbc:UUID")
        // ds_DigestValue = xml.GetChildContent("ext:UBLExtensions|ext:UBLExtension|ext:ExtensionContent|sig:UBLDocumentSignatures|sac:SignatureInformation|ds:Signature|ds:SignedInfo|ds:Reference[0]|ds:DigestValue")

        // Dim Base64Invoice As String = BinInvoice.GetEncoded("base64")

        // Dim json As Chilkat.JsonObject = New Chilkat.JsonObject()
        // json.UpdateString("invoiceHash", ds_DigestValue)
        // json.UpdateString("uuid", cbc_UUID)
        // json.UpdateString("invoice", Base64Invoice)

        // rest.AddHeader("accept", "application/json")
        // rest.AddHeader("Content-Type", "application/json")
        // rest.AddHeader("Accept-Language", "en")
        // rest.AddHeader("Clearance-Status", "1")
        // rest.AddHeader("Accept-Version", "V2")

        // Dim sbRequestBody As Chilkat.StringBuilder = New Chilkat.StringBuilder()
        // json.EmitSb(sbRequestBody)
        // Dim sbResponseBody As Chilkat.StringBuilder = New Chilkat.StringBuilder()

        // rest.AddHeader("Authorization", "Basic " & Me.txtAuthorization.Text)


        // 'This submits to the Developer Portal Compliances
        // '-------------------------------------------------------------------------------------------------------------------------
        // 'success = rest.FullRequestSb("POST", "/e-invoicing/developer-portal/compliance/invoices", sbRequestBody, sbResponseBody)

        // 'This submits to the Simulation Portal  'https://gw-fatoora.zatca.gov.sa/e-invoicing/simulation/compliance/invoices
        // '-------------------------------------------------------------------------------------------------------------------------
        // success = rest.FullRequestSb("POST", "/e-invoicing/simulation/compliance/invoices", sbRequestBody, sbResponseBody)


        // If success <> True Then
        // MessageBox.Show(rest.LastErrorText)
        // Return
        // End If

        // Dim respStatusCode As Integer = rest.ResponseStatusCode
        // Me.MemoEdit1.EditValue = "Response Status Code = " & Convert.ToString(respStatusCode) & vbCrLf & "Response Header:" & rest.ResponseHeader & vbCrLf & "Response Body: " & sbResponseBody.GetAsString()
        // #Region "Old Codes"


        // 'Dim http As Chilkat.Http = New Chilkat.Http()
        // 'Dim json As Chilkat.JsonObject = New Chilkat.JsonObject()

        // 'json.UpdateString("invoiceHash", ds_DigestValue)
        // 'json.UpdateString("uuid", cbc_UUID)
        // 'json.UpdateString("invoice", Base64Invoice)

        // 'http.SetRequestHeader("accept", "application/json")
        // 'http.SetRequestHeader("Content-Type", "application/json")
        // 'http.SetRequestHeader("Authorization", "Basic " & Me.txtAuthorization.Text)
        // 'http.SetRequestHeader("Accept-Version", "V2")
        // ''http.SetRequestHeader("Clearance-Status", Me.txtClearanceStatus.Text)
        // 'http.SetRequestHeader("accept-language", "en")


        // 'Dim resp As Chilkat.HttpResponse = http.PostJson3("https://gw-apic-gov.gazt.gov.sa/e-invoicing/developer-portal/compliance/invoices", "application/json", json)

        // 'If http.LastMethodSuccess = False Then
        // '    lblStatus.Text = "Invoice Compliance Failed"
        // '    MessageBox.Show("Failed to Load XML " & http.LastErrorText, "XML", MessageBoxButtons.OK, MessageBoxIcon.[Error])
        // '    Return
        // 'End If

        // 'lblStatus.Text = "Invoice Reporting Success"


        // 'lblStatus.Text = "Invoice Reporting Success"
        // 'Me.txtStatusMessage.EditValue = Convert.ToString(resp.StatusCode)
        // 'Me.txtStatusMessage.EditValue = Convert.ToString(resp.StatusCode) & vbCrLf & vbCrLf & resp.BodyStr
        // 'MessageBox.Show("Success " & Convert.ToString(resp.StatusCode), "XML", MessageBoxButtons.OK, MessageBoxIcon.Information)
        // 'MessageBox.Show("Success " & resp.BodyStr, "XML", MessageBoxButtons.OK, MessageBoxIcon.Information)

        // 'Dim strJson As String = resp.BodyStr
        // 'Dim json02 As New Chilkat.JsonObject
        // 'json02.Load(strJson)
        // 'json02.EmitCompact = False
        // 'Dim strFormattedJson As String = json02.Emit()

        // 'Dim numMembers As Integer = json02.Size
        // 'Dim i As Integer
        // 'For i = 0 To numMembers - 1

        // '    Dim name As String = json02.NameAt(i)
        // '    Dim value As String = json02.StringAt(i)

        // '    Me.MemoEdit1.EditValue = (name & ": " & value)
        // '    Dim iValue As Integer = json02.IntAt(i)

        // '    'Me.MemoEdit1.EditValue = Me.MemoEdit1.EditValue & vbCrLf & vbCrLf & (name & " as integer: " & iValue)
        // '    'Debug.WriteLine(name & " as integer: " & iValue)

        // '    If value = "REPORTED" Then
        // '        Me.lblReportedStatus.Text = "E-INVOICE HAS BEEN SUCESSFULLY REPORTED TO ZATCA"
        // '        Me.lblReportedStatus.ForeColor = Color.Green
        // '    Else
        // '        Me.lblReportedStatus.Text = "E-INVOICE HAS BEEN FAILED TO REPORT TO ZATCA"
        // '        Me.lblReportedStatus.ForeColor = Color.Red
        // '    End If
        // 'Next
        // #End Region

        // End Sub

        // --------------------------------------------
        // Private Sub btnCreatePFX_Click(sender As Object, e As EventArgs) Handles btnCreatePFX.Click
        // Dim success As Boolean
        // Dim pkey As Chilkat.PrivateKey = New Chilkat.PrivateKey()
        // success = pkey.LoadAnyFormatFile(appPath & "\Certificates\privatekey.pem", "")

        // If success <> True Then
        // MessageBox.Show(pkey.LastErrorText)
        // Return
        // End If

        // Dim cert As Chilkat.Cert = New Chilkat.Cert()
        // success = cert.LoadFromFile(appPath & "\Certificates\PCSID.pem")

        // If success <> True Then
        // MessageBox.Show(cert.LastErrorText)
        // Return
        // End If

        // Dim certChain As Chilkat.CertChain = Nothing
        // certChain = cert.GetCertChain()

        // If Not cert.LastMethodSuccess Then
        // MessageBox.Show(cert.LastErrorText)
        // Return
        // End If

        // Dim pfx As Chilkat.Pfx = New Chilkat.Pfx()
        // success = pfx.AddPrivateKey(pkey, certChain)

        // If success <> True Then
        // MessageBox.Show(pfx.LastErrorText)
        // Return
        // End If

        // Dim password As String = "123"
        // success = pfx.ToFile(password, appPath & "\Certificates\PCSID.pfx")

        // If success <> True Then
        // MessageBox.Show(pfx.LastErrorText)
        // Return
        // End If

        // MessageBox.Show("Success.")
        // End Sub

        // Private Sub btnGetCCSIDAuthKeyPairFromDB_Click(sender As Object, e As EventArgs) Handles btnGetCCSIDAuthKeyPairFromDB.Click
        // 'Get CCSID String
        // Dim Base64UserBasic As String = "TUlJQ01UQ0NBZGVnQXdJQkFnSUdBWWdEa3BPL01Bb0dDQ3FHU000OUJBTUNNQlV4RXpBUkJnTlZCQU1NQ21WSmJuWnZhV05wYm1jd0hoY05Nak13TlRFd01ESTFNVFV5V2hjTk1qZ3dOVEE1TWpFd01EQXdXakI4TVFzd0NRWURWUVFHRXdKVFFURVdNQlFHQTFVRUN3d05Va2xaUVVSSUlFSlNRVTVEU0RFbU1DUUdBMVVFQ2d3ZFFuSmhibU5vSUc5bUlGTnBibTlvZVdSeWJ5QmhibVFnVTJWd1kyOHhMVEFyQmdOVkJBTU1KRk5GVUVOUElFRnlZV0pwWVNCUmRXbGphMFJwWTJVZ1JWSlFJRk52YkhWMGFXOXVjekJXTUJBR0J5cUdTTTQ5QWdFR0JTdUJCQUFLQTBJQUJDY1VTb280b0Q3ZG5VVzJ3Wng0WVFRY0F6cytydWZmeHIwalF0UE5TYmZCeWd3ZXo2bXQxQjB0cVQrR3ZUNitHMXlHVzlDbGM2WTBEWURKZUQvSnp1bWpnYTR3Z2Fzd0RBWURWUjBUQVFIL0JBSXdBRENCbWdZRFZSMFJCSUdTTUlHUHBJR01NSUdKTVM0d0xBWURWUVFFRENVeExWQjFiSE5sU1c1bWIzUmxZMmg4TWkxV1pYSnphVzl1TlM0d2ZETXRUREF4TWpnM01SOHdIUVlLQ1pJbWlaUHlMR1FCQVF3UE16RXdNelF3TlRZek16QXdNREF6TVEwd0N3WURWUVFNREFReE1UQXdNUTh3RFFZRFZRUWFEQVpTU1ZsQlJFZ3hGakFVQmdOVkJBOE1EVU5QVGxOVVVsVkRWRWxQVGxNd0NnWUlLb1pJemowRUF3SURTQUF3UlFJZ1lhVE96MFVaTUlkb096bmUrSkwvd25UUHFsQ1d5bEhBb3A0dm04SDBnQUVDSVFEakJmelFaN0ZDSzJFKy9aNmxLdXdlR0hzd096alNMQVVVTi9QZmVnUlp5UT09"

        // 'Get CCSID Secret
        // Dim Base64UserBasicSec As String = "DGdvZYefvdAG5xNPaYwTbcx/i3tr5fYBsY7pXF/kmcU="

        // MessageBox.Show(Base64UserBasic & ":" & Base64UserBasicSec)


        // Dim b64Authorization As String = Base64UserBasic & ":" & Base64UserBasicSec
        // Dim CryptAuthorization As Chilkat.Crypt2 = New Chilkat.Crypt2()
        // CryptAuthorization.Charset = "utf-8"
        // CryptAuthorization.CryptAlgorithm = "none"
        // CryptAuthorization.EncodingMode = "base64"
        // Dim jb64Authorization As String = CryptAuthorization.EncryptStringENC(b64Authorization)

        // Me.txtAuthBase64ComplianceChecking.Text = jb64Authorization
        // End Sub


        // Private Sub SimpleButton5_Click(sender As Object, e As EventArgs) Handles SimpleButton5.Click

        // Dim success As Boolean = chilkatGlob.UnlockBundle("WwMfDn.CBX1127_2ThZtySnD3DV")

        // If success <> True Then
        // MessageBox.Show("Failed to Load License", "License", MessageBoxButtons.OK, MessageBoxIcon.[Error])
        // Return
        // End If

        // If lblCon.Text <> "Connected" Then
        // MessageBox.Show("Please Connect to Portal", "XML", MessageBoxButtons.OK, MessageBoxIcon.[Error])
        // Return
        // End If

        // Dim xml As Chilkat.Xml = New Chilkat.Xml()
        // Dim ds_DigestValue As String
        // Dim Retsuccess As Boolean

        // Retsuccess = xml.LoadXmlFile(appPath & XMLFileNameAsPerZatca)

        // If Retsuccess <> True Then
        // MessageBox.Show("Failed to Load XML " & xml.LastErrorText, "XML", MessageBoxButtons.OK, MessageBoxIcon.[Error])
        // Return
        // End If

        // Dim BinInvoice As Chilkat.BinData = New Chilkat.BinData()

        // Dim Invsuccess As Boolean = BinInvoice.LoadFile(appPath & XMLFileNameAsPerZatca)

        // If Invsuccess <> True Then
        // MessageBox.Show("Failed to Load XML Invoice" & xml.LastErrorText, "XML", MessageBoxButtons.OK, MessageBoxIcon.[Error])
        // Return
        // End If

        // Dim cbc_UUID As String = xml.GetChildContent("cbc:UUID")
        // ds_DigestValue = xml.GetChildContent("ext:UBLExtensions|ext:UBLExtension|ext:ExtensionContent|sig:UBLDocumentSignatures|sac:SignatureInformation|ds:Signature|ds:SignedInfo|ds:Reference[0]|ds:DigestValue")

        // Dim Base64Invoice As String = BinInvoice.GetEncoded("base64")

        // Dim json As Chilkat.JsonObject = New Chilkat.JsonObject()
        // json.UpdateString("invoiceHash", ds_DigestValue)
        // json.UpdateString("uuid", cbc_UUID)
        // json.UpdateString("invoice", Base64Invoice)

        // rest.AddHeader("accept", "application/json")
        // rest.AddHeader("Content-Type", "application/json")
        // rest.AddHeader("Accept-Language", "en")
        // rest.AddHeader("Clearance-Status", "1")
        // rest.AddHeader("Accept-Version", "V2")

        // Dim sbRequestBody As Chilkat.StringBuilder = New Chilkat.StringBuilder()
        // json.EmitSb(sbRequestBody)
        // Dim sbResponseBody As Chilkat.StringBuilder = New Chilkat.StringBuilder()

        // rest.AddHeader("Authorization", "Basic " & Me.txtAuthBase64ComplianceChecking.Text)


        // 'This submits to the Developer Portal Compliances
        // '-------------------------------------------------------------------------------------------------------------------------
        // success = rest.FullRequestSb("POST", "/e-invoicing/developer-portal/compliance/invoices", sbRequestBody, sbResponseBody)

        // 'This submits to the Simulation Portal  'https://gw-fatoora.zatca.gov.sa/e-invoicing/simulation/compliance/invoices
        // '-------------------------------------------------------------------------------------------------------------------------
        // 'success = rest.FullRequestSb("POST", "/e-invoicing/simulation/compliance/invoices", sbRequestBody, sbResponseBody)


        // If success <> True Then
        // MessageBox.Show(rest.LastErrorText)
        // Return
        // End If

        // Dim respStatusCode As Integer = rest.ResponseStatusCode
        // Me.MemoEdit1.EditValue = "Response Status Code = " & Convert.ToString(respStatusCode) & vbCrLf & "Response Header:" & rest.ResponseHeader & vbCrLf & "Response Body: " & sbResponseBody.GetAsString()

        // End Sub


        // 'Load Data
        // Private Sub btnFillData_Click(sender As Object, e As EventArgs) Handles btnFillData.Click
        // FillData(Me.txtInvoiceNo.Text)
        // End Sub

        // generate ubl using PCSID
        // Private Sub btnGenerateUBL_PCSID_Click(sender As Object, e As EventArgs) Handles btnGenerateUBL_PCSID.Click
        // GenerateUBLFile(False)

        // End Sub

        // Private Sub btnSignXML_Click(sender As Object, e As EventArgs) Handles btnSignXML.Click
        // SignXML()

        // End Sub

        // 'Generate ubl using ccsid
        // Private Sub btnGeneralUBL_Click(sender As Object, e As EventArgs) Handles btnGeneralUBL.Click
        // GenerateUBLFile(True)
        // End Sub


        // Private Sub btnExportPDFA3_Click(sender As Object, e As EventArgs) Handles btnExportPDFA3.Click

        // 'Dim xmlFileName As String = appPath & "\SignedXML\signedXmlResult1V.xml"
        // 'Dim input As String = File.ReadAllText(xmlFileName)
        // 'Dim pattern As String = "(</?)(\w+:)"
        // 'Dim output As String = Regex.Replace(input, pattern, "$1")
        // 'File.WriteAllText(appPath & "\SignedXML\signedXmlResult1VForPDF.xml", output.ToString())

        // 'Rpt1.XmlDataPath = appPath & "\SignedXML\signedXmlResult1VForPDF.xml"
        // 'Rpt1.XmlDataPath = appPath & "\SignedXML\signedXmlResult1V.xml"

        // Dim InvoiceSubCode As String = Me.txtInvoiceTransactionCode.Text.ToString.Substring(0, 2)
        // If InvoiceSubCode = "01" Then
        // ExportPDF_RegularTaxInvoice()
        // ElseIf InvoiceSubCode = "02" Then
        // ExportPDF_SimplifiedTaxInvoice()

        // End If

        // End Sub


        // Private Sub btnGetNextAvailableICV_Click(sender As Object, e As EventArgs) Handles btnGetNextAvailableICV.Click
        // Me.txtInvoiceCounterValue.Text = GetNextICVNumber()

        // End Sub

        // Private Sub btnPreviousHashFile_Click(sender As Object, e As EventArgs) Handles btnPreviousHashFile.Click
        // GetPreviousHashFile()
        // Me.txtPreviousInvoiceHASH.Text = gLastSuccessfulSubmittedHashfile
        // End Sub

        // Private Sub ExportPDF_RegularTaxInvoice()
        // Dim Rpt1 As New sampleEInvoice01
        // Rpt1.Qry201_602VATInvoiceReportTableAdapter.FillByInvoiceNo(Rpt1.DsRptEInvoice011.qry201_602VATInvoiceReport, Me.txtInvoiceNo.EditValue)

        // Dim reportName As String
        // reportName = "\SignedXML\RegularTaxInvoice.pdf"
        // '\SignedXML\310906806100003_2023-06-20T112803_2023-00014.xml

        // Dim options As New PdfExportOptions
        // options.PdfACompatibility = PdfACompatibility.PdfA3a
        // options.Attachments.Add(New PdfAttachment() With {.FilePath = appPath & GeteInvoiceXMLFileLocation(Me.txtInvoiceNo.Text),
        // .Type = "text/xml",
        // .Description = "ZATCA-XML-Format"})

        // Rpt1.ExportToPdf(appPath & reportName, options)
        // Process.Start(appPath & reportName)
        // End Sub

        // Private Sub cmdReportSimplifiedInvoice_Click(sender As Object, e As EventArgs) Handles btnReportSimplifiedInvoice.Click
        // ReportSimplifiedTaxInvoice()

        // End Sub

        // Private Sub btnClearTaxInvoice_Click(sender As Object, e As EventArgs) Handles btnClearTaxInvoice.Click
        // ClearStandardTaxInvoice()

        // End Sub

        // Private Sub btnGetAuthorizationKeyPairPCSID_Click(sender As Object, e As EventArgs) Handles btnGetAuthorizationKeyPairPCSID.Click
        // GetAuthorizeKey()
        // End Sub

        ////private void BarButtonItem1_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        ////{
        ////    this.GenerateUBLFileForeignCurrency(string InvoiceCurrencyCode);
        ////}


        public string ENInvoiceHASH(string sFiles)
        {
            try
            {
                bool success;
                string strXml;

                Chilkat.Xml xml = new Chilkat.Xml();
                success = xml.LoadXmlFile(sFiles);

                if (success != true)
                {
                    return success.ToString();
                }

                // Remove unwanted XML nodes
                xml.RemoveChild("ext:UBLExtensions");
                xml.RemoveChild("cac:AdditionalDocumentReference[2]");

                strXml = xml.GetXml();

                Chilkat.XmlDSig xmldsig = new Chilkat.XmlDSig();
                string canonXml = xmldsig.CanonicalizeXml(strXml, "C14N", false);

                string strTextToHash = canonXml;

                Chilkat.Crypt2 crypt = new Chilkat.Crypt2
                {
                    Charset = "utf-8",
                    EncodingMode = "hexlower",
                    HashAlgorithm = "SHA256"
                };

                Chilkat.StringBuilder sb = new Chilkat.StringBuilder();
                success = sb.Append(strTextToHash);
                success = sb.ToCRLF();

                Chilkat.BinData bd = new Chilkat.BinData();
                success = bd.AppendString(crypt.HashStringENC(sb.GetAsString()), "utf-8");

                string strBase64 = bd.GetEncoded("base64");
                return strBase64;
            }
            catch(Exception ex)
            {
                throw ex;
            }
            
        }

        private decimal GetInvoicePrepaidAdjustmentAmount(string invoiceNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var total = dbContext.Qry201645adjustedInvoiceForXmlsummary002s
                                     .Where(x => x.EInvoiceNo == invoiceNo)
                                     .Select(x => x.TotalAdjustedInclusiveAmount)
                                     .FirstOrDefault();

                return total ?? 0;
            }

            return 0;
        }

        [HttpGet]
        public IActionResult GetInvoiceLog(string invoiceNo)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return BadRequest(new { Message = "Database connection failed" });

            try
            {
                var result = dbContext.Tbl90121eInvoiceLogTables
                    .Where(x => x.EInvoiceNo == invoiceNo)
                    .Select(x => new
                    {
                        x.EInvoiceNo,
                        x.EInvoiceDateTime,
                        x.InvoiceStatus,
                        x.SubmissionStatusText,
                        x.SubmittedBy,
                        x.SubmittedOn,
                        ZatcaResponse = _signedXmlGlobal  // static empty string same as SQL
                    })
                    .FirstOrDefault();

                if (result == null)
                    return NotFound(new { Message = "Invoice log not found" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching invoice log for {InvoiceNo}", invoiceNo);
                return StatusCode(500, new { Message = "Internal Server Error", Error = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetInvoiceMaster(string invoiceNo)
        {
            if (string.IsNullOrWhiteSpace(invoiceNo))
            {
                return BadRequest(new { Success = false, Message = "Invoice number is required." });
            }

            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var invoiceMaster = dbContext.Qry201630EInvoiceToXmlmaster01s
                        .Where(m => m.InvoiceNo == invoiceNo)
                        .Select(m => new Qry201630EInvoiceToXmlmaster01
                        {
                            InvoiceNo = m.InvoiceNo,
                            InvoiceUuid = m.InvoiceUuid,
                            InvoiceDateWtTime = m.InvoiceDateWtTime,
                            InvoiceTypeCode = (short)m.InvoiceTypeCode,
                            InvoiceTransactionCode = m.InvoiceTransactionCode,
                            RemarksInEn = m.RemarksInEn,
                            InvoiceCurrencyCode = m.InvoiceCurrencyCode,
                            TaxCurrencyCode = m.TaxCurrencyCode,
                            PurchaseOrderId = m.PurchaseOrderId,
                            ContractId = m.ContractId,
                            InvoiceCounterValue = m.InvoiceCounterValue,
                            SellerOtherIdtype = m.SellerOtherIdtype,
                            SellerOtherSellerId = m.SellerOtherSellerId,
                            SellerAddressStreet = m.SellerAddressStreet,
                            SellerAdditionalStreet = m.SellerAdditionalStreet,
                            SellerBuildingNumber = m.SellerBuildingNumber,
                            SellerAdditionalNumber = m.SellerAdditionalNumber,
                            SellerCity = m.SellerCity,
                            SellerPostalCode = m.SellerPostalCode,
                            SellerProvince = m.SellerProvince,
                            SellerNeighborhood = m.SellerNeighborhood,
                            SellerCountryCode = m.SellerCountryCode,
                            SellerVatnumber = m.SellerVatnumber,
                            SellerName = m.SellerName,
                            BuyerOtherIdtype = m.BuyerOtherIdtype,
                            BuyerOtherId = m.BuyerOtherId,
                            BuyerAddressStreet = m.BuyerAddressStreet,
                            BuyerAdditionalStreet = m.BuyerAdditionalStreet,
                            BuyerBuildingNumber = m.BuyerBuildingNumber,
                            BuyerAdditionalNumber = m.BuyerAdditionalNumber,
                            BuyerCity = m.BuyerCity,
                            BuyerPostalCode = m.BuyerPostalCode,
                            BuyerProvince = m.BuyerProvince,
                            BuyerNeighborhood = m.BuyerNeighborhood,
                            BuyerCountryCode = m.BuyerCountryCode,
                            BuyerVatnumber = m.BuyerVatnumber,
                            BuyerName = m.BuyerName,
                            SupplyDate = m.SupplyDate,
                            SupplyEndDate = m.SupplyEndDate,
                            PaymentMeansTypeCode = m.PaymentMeansTypeCode,
                            PaymentTerms = m.PaymentTerms,
                            AdvanceAmount = m.AdvanceAmount,
                            RetentionAmount = m.RetentionAmount,
                            OtherDeductionAmount = m.OtherDeductionAmount,
                            CompanyId = m.CompanyId,
                            CreditNoteInvoiceReferenceNo = m.CreditNoteInvoiceReferenceNo,
                            CreditNoteReason = m.CreditNoteReason,
                            TotalAllDeductionsAmount = m.TotalAllDeductionsAmount,
                            SellerNameAr = m.SellerNameAr,
                            SellerNeighborhoodAr = m.SellerNeighborhoodAr,
                            SellerCityAr = m.SellerCityAr,
                            SellerAddressStreetAr = m.SellerAddressStreetAr,
                            BuyerNameAr = m.BuyerNameAr,
                            BuyerNeighborhoodAr = m.BuyerNeighborhoodAr,
                            BuyerCityAr = m.BuyerCityAr,
                            BuyerAddressStreetAr = m.BuyerAddressStreetAr
                        })
                        .FirstOrDefault();

                    if (invoiceMaster == null)
                    {
                        return NotFound(new { Success = false, Message = "Invoice not found." });
                    }

                    return Ok(new { Success = true, Data = invoiceMaster });
                }
                else
                {
                    return StatusCode(500, new { Success = false, Message = "Failed to resolve tenant or database context." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching invoice master details for InvoiceNo {InvoiceNo}", invoiceNo);
                return StatusCode(500, new { Success = false, Message = "Internal server error.", Error = ex.Message });
            }
        }


        //private (string connectionString, string containerName, string azurePath, bool azureEnabled) GetAzureConnection(int companyId)
        //{
        //    bool azureEnabled = GetAzureStatus(companyId);
        //    string connectionString = "DefaultEndpointsProtocol=https;AccountName=qdprodclientfiles;AccountKey=/21EGSpU+t+LzqRSGIfAuLbzi06xAlrJ3+lhYEmfsXBnrQJRAoNE/+ED8DxbS9e+c1xeuvHxEvQT+ASteaLteA==;EndpointSuffix=core.windows.net";
        //    string containerName = "client-files";
        //    string companyName = GetCompanyName(companyId);
        //    string azurePath = $"{companyName}/Secured/VAT_ZATCA/year{DateTime.Now:yyyy}/VAT_ZATCA/";

        //    return (connectionString, containerName, azurePath, azureEnabled);
        //}

        private void SaveSignedXmlBasedOnAzureStatus(Chilkat.StringBuilder sbXml, string appPath, int companyId)
        {
            bool azureEnabled = GetAzureStatus(companyId);
            string signedXmlPath = Path.Combine(appPath, @"SignedXML\signedXmlResult1.xml");

            if (azureEnabled)
            {
                // Upload directly from memory (no local save)
                string xmlContent = sbXml.GetAsString();
                UploadXmlToAzureIfEnabled(xmlContent, "signedXmlResult1.xml", companyId);
            }
            else
            {
                // Save locally only
                sbXml.WriteFile(signedXmlPath, "utf-8", false);
            }
        }

        private void UploadXmlToAzureIfEnabled(string xmlContent, string fileName, int companyId)
        {
            try
            {
                var cfg = GetAzureConnection(companyId);
                if (cfg.azureEnabled)
                {
                    var azureService = new AzureBlobHelper(cfg.connectionString, cfg.containerName);
                    azureService.UploadString(xmlContent, cfg.azurePath + fileName);
                }
            }
            catch (Exception ex)
            {
              //  MessageBox.Show("Azure XML upload failed: " + ex.Message, "Azure Upload Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public async Task<bool> UploadXmlToAzureIfEnabledAsync(string xmlFilePath, int companyId)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    bool azureStatus = dbContext.Tbl901CompanyDetails
                                                .Where(c => c.CompanyId == companyId)
                                                .Select(c => c.AzureStatus)
                                                .FirstOrDefault();

                    if (azureStatus)
                    {
                        string connectionString = "DefaultEndpointsProtocol=https;AccountName=...;EndpointSuffix=core.windows.net";
                        string containerName = "client-files";
                        string companyName = dbContext.Tbl901CompanyDetails
                                                      .Where(c => c.CompanyId == companyId)
                                                      .Select(c => c.CompanyName)
                                                      .FirstOrDefault();

                        string azurePath = $"{companyName}/Secured/VAT_ZATCA/year{DateTime.Now:yyyy}/VAT_ZATCA/";

                        var azureService = new AzureBlobHelper(connectionString, containerName);

                        string xmlContent = await System.IO.File.ReadAllTextAsync(xmlFilePath);

                        // If AzureBlobHelper has an async method
                         azureService.UploadString(xmlContent, azurePath + Path.GetFileName(xmlFilePath));

                        return true; // Success
                    }
                }

                return false; // Tenant/dbContext not resolved or Azure not enabled
            }
            catch (Exception ex)
            {
                Console.WriteLine("Azure XML upload failed: " + ex.Message);
                return false;
            }
        }



        private void NavigateSignedXml(string appPath, int companyId)
        {
            string signedXmlPath = Path.Combine(appPath, @"SignedXML\signedXmlResult1.xml");
            var cfg = GetAzureConnection(companyId);

            if (cfg.azureEnabled)
            {
                var azureService = new AzureBlobHelper(cfg.connectionString, cfg.containerName);
                string xmlContent = azureService.DownloadStringAsync(cfg.azurePath + "signedXmlResult1.xml").GetAwaiter().GetResult();

                if (!Directory.Exists(Path.GetDirectoryName(signedXmlPath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(signedXmlPath));

               //File.WriteAllText(signedXmlPath, xmlContent);
            }

           // this.WebBrowser1.Navigate(signedXmlPath);
          
        }

        private string GetSignedXmlFilePath(int companyId, string localPath)
        {
            try
            {
                bool azureStatus = false;

                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    // Query the company details directly with EF context
                    var result = dbContext.Tbl901CompanyDetails
                                          .Where(c => c.CompanyId == companyId)
                                          .Select(c => c.AzureStatus)
                                          .FirstOrDefault();

                    azureStatus = Convert.ToBoolean(result);
                }

                if (azureStatus)
                {
                    string connectionString = "DefaultEndpointsProtocol=https;AccountName=qdprodclientfiles;AccountKey=/21EGSpU+t+LzqRSGIfAuLbzi06xAlrJ3+lhYEmfsXBnrQJRAoNE/+ED8DxbS9e+c1xeuvHxEvQT+ASteaLteA==;EndpointSuffix=core.windows.net";
                    string containerName = "client-files";
                    string companyName = GetCompanyName(companyId);
                    var azureService = new AzureBlobHelper(connectionString, containerName);
                    string azurePath = $"{companyName}/Secured/VAT_ZATCA/year{DateTime.Now:yyyy}/VAT_ZATCA/signedXmlResult1.xml";

                    string tempPath = azureService.DownloadToTempFile(azurePath);
                    if (tempPath != null) return tempPath;
                }

                return localPath;
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Error checking Azure status: " + ex.Message, "Azure Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return localPath;
            }
        }


        private void UploadPdfToAzureIfEnabled(string pdfFilePath, int companyId)
        {
            try
            {
                var cfg = GetAzureConnection(companyId);
                if (cfg.azureEnabled)
                {
                    var azureService = new AzureBlobHelper(cfg.connectionString, cfg.containerName);
                    azureService.UploadFile(pdfFilePath, cfg.azurePath + Path.GetFileName(pdfFilePath));
                }
            }
            catch (Exception ex)
            {
               // MessageBox.Show("Azure PDF upload failed: " + ex.Message, "Azure Upload Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private async Task<string> DownloadFromAzureAsync(string fileName, int companyId)
        {
            var cfg = GetAzureConnection(companyId);
            var azureService = new AzureBlobHelper(cfg.connectionString, cfg.containerName);

            string content = await azureService.DownloadStringAsync(cfg.azurePath + fileName);
            return content;
        }


        private void UploadToAzure(string content, string fileName, int companyId)
        {
            var cfg = GetAzureConnection(companyId);
            var azureService = new AzureBlobHelper(cfg.connectionString, cfg.containerName);
            azureService.UploadString(content, cfg.azurePath + fileName);
        }

        private async Task<string> GetSignedXmlFilePath1Async(int companyId, string localPath)
        {
            try
            {
                var cfg = GetAzureConnection(companyId);

                if (cfg.azureEnabled)
                {
                    var azureService = new AzureBlobHelper(cfg.connectionString, cfg.containerName);
                    string fileName = "signedXmlResult1V.xml";
                    string blobPath = cfg.azurePath + fileName;

                    // Async download
                    string xmlContent = await azureService.DownloadStringAsync(blobPath);

                    if (!string.IsNullOrEmpty(xmlContent))
                    {
                        string dir = Path.GetDirectoryName(localPath);
                        if (!Directory.Exists(dir))
                            Directory.CreateDirectory(dir);

                        // Async file write
                        await System.IO.File.WriteAllTextAsync(localPath, xmlContent);

                        return localPath;
                    }
                }

                return localPath;
            }
            catch (Exception ex)
            {
                // Log the error as needed
                Console.WriteLine("Error retrieving signed XML: " + ex.Message);
                return localPath;
            }
        }


        private bool GetAzureStatus(int companyId)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    // Fetch AzureStatus using LINQ
                    bool status = dbContext.Tbl901CompanyDetails
                                           .Where(c => c.CompanyId == companyId)
                                           .Select(c => c.AzureStatus)
                                           .FirstOrDefault();

                    return status;
                }

                // Tenant/dbContext not resolved
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error checking Azure status: " + ex.Message);
                return false;
            }
        }


        private (string connectionString, string containerName, string azurePath, bool azureEnabled) GetAzureConnection(int companyId)
        {
            bool azureEnabled = GetAzureStatus(companyId);
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=qdprodclientfiles;AccountKey=/21EGSpU+t+LzqRSGIfAuLbzi06xAlrJ3+lhYEmfsXBnrQJRAoNE/+ED8DxbS9e+c1xeuvHxEvQT+ASteaLteA==;EndpointSuffix=core.windows.net";
            string containerName = "client-files";
            string companyName = GetCompanyName(companyId);
            string azurePath = $"{companyName}/Secured/VAT_ZATCA/year{DateTime.Now:yyyy}/VAT_ZATCA/";

            return (connectionString, containerName, azurePath, azureEnabled);
        }


        private string GetCompanyName(int companyId)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return dbContext.Tbl901CompanyDetails
                                .Where(c => c.CompanyId == companyId)
                                .Select(c => c.CompanyName)
                                .FirstOrDefault();
            }

            return null;
        }





    }
}