using Azure.Communication.Email;
using DevExpress.XtraReports.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.Areas.Finance.Reports;
using QD.ERP.Web.Areas.Finance.Reports.AccountRegister;
using QD.ERP.Web.Areas.Finance.Reports.AccountStatement;
using QD.ERP.Web.Areas.Finance.Reports.BillsReceivable;
using QD.ERP.Web.Areas.Finance.Reports.Payable_Statements;
using QD.ERP.Web.Areas.Finance.Reports.Receivable_Statements;
using QD.ERP.Web.Areas.VAT.Reports.VATCreditNote;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Models.DAL;
using QD.ERP.Web.Reports;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mail;

namespace QD.ERP.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountReportController : ControllerBase
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;

        public AccountReportController(TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
        }

        [HttpGet("Download")]
        public IActionResult DownloadAccountReport(string reportName, string accountId, DateTime? frmDate, DateTime? toDate, string accountHead)
        {
            if (string.IsNullOrEmpty(reportName) || string.IsNullOrEmpty(accountId))
                return BadRequest("Invalid report parameters.");

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                return StatusCode(500, "Tenant or DbContext could not be resolved.");

            // Get session data
            var tenantName = HttpContext.Session.GetString("TenantName") ?? "DefaultTenant";
            var username = HttpContext.Session.GetString("UserName") ?? "DefaultUser";
            var defaultCompanyIdString = HttpContext.Session.GetString("DefaultcompanyID");
            // Parse it to int (you may want to use long or Guid if that's your actual ID type)
            if (!int.TryParse(defaultCompanyIdString, out int defaultCompanyId))
            {
                // Handle invalid or missing ID (fallback or error handling)
                defaultCompanyId = 0; // or return early / throw error
            }
            string companyName = "", companyAddress = "", companyNameAr = "", companyAddressAr = "";
            Image logoImage = null;

            var companyDetails = dbContext.Tbl901CompanyDetails
                .FirstOrDefault(x => x.CompanyId == defaultCompanyId);

            if (companyDetails != null)
            {
                companyName = companyDetails.CompanyName ?? "";
                companyAddress = companyDetails.CompanyFullAddress ?? "";
                companyAddressAr = companyDetails.CompanyFullAddressAr ?? "";
                companyNameAr = companyDetails.CompanyNameAr ?? "";

                if (companyDetails.CompanyLogo?.Length > 0)
                {
                    try
                    {
                        using var ms = new MemoryStream(companyDetails.CompanyLogo);
                        logoImage = Image.FromStream(ms);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error reading company logo: " + ex.Message);
                    }
                }
            }

            var report = GenerateAccountReport(
                 reportName,
                 accountId,
                 frmDate ?? DateTime.MinValue,  
                 toDate ?? DateTime.Now,       // or DateTime.MaxValue
                 tenantName,
                 companyName,
                 companyAddress,
                 logoImage,
                 companyNameAr,
                 companyAddressAr,
                 username,
                 _tenantDbContextHelper
             );

            using var stream = new MemoryStream();
            report.ExportToPdf(stream);
            stream.Position = 0;
            return File(stream.ToArray(), "application/pdf", $"{reportName}_{accountId}.pdf");
        }

        private XtraReport GenerateAccountReport(
              string reportName,
              string accountId,
              DateTime frmDate,
              DateTime toDate,
              string tenantName,
              string companyName,
              string companyAddress,
              Image logoImage,
              string companyNameAr,
              string companyAddressAr,
              string username,
              TenantDbContextHelper tenantHelper)

        {
            XtraReport report = reportName switch
            {
                "StatementOfAccountReport" => new StatementOfAccountReport(accountId, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, username, tenantHelper),
                "AccountWithNarration" => new AccountWithNarration(accountId, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, username, tenantHelper),
                "AccountStatementFormat2Report" => new AccountStatementFormat2Report(accountId, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, username, tenantHelper),
                "AccountExportFromatReport" => new AccountExportFromatReport(accountId, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, username, tenantHelper),
                "AccountOrderbyVchNoWONarrationReport" => new AccountOrderbyVchNoWONarrationReport(accountId, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, username, tenantHelper),
                "AccountExportLandscapeReport" => new AccountExportLandscapeReport(accountId, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, username, tenantHelper),
                "AccountOrderByVoucherNo" => new AccountOrderByVoucherNo(accountId, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, username, tenantHelper),
                "BillsReceivablelandscapeformat" => new BillsReceivablelandscapeformat(accountId, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, username, tenantHelper),
                "BillsReceivableAll" => new BillsReceivableAll(accountId, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, username, tenantHelper),
                "BillsReceivableByAccount" => new BillsReceivableByAccount(accountId, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, username, tenantHelper),
                "BillsReceivableAgeingToday" => new BillsReceivableAgeingToday(accountId, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, username, tenantHelper),
                "Report4" => new Report4(accountId, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, username, tenantHelper),
                "BillsReceivableRentation" => new BillsReceivableRentation(accountId, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, username, tenantHelper),
                "BillsReceivableLedgerBalance" => new BillsReceivableLedgerBalance(accountId, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, username, tenantHelper),
                "BillsReceivableFormat" => new BillsReceivableFormat(accountId, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, username, tenantHelper),
                "rpt201BillsPayable" => new rpt201BillsPayable(accountId, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, username, tenantHelper),
                "rpt201BillsPayableWithVchNo" => new rpt201BillsPayableWithVchNo(accountId, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, username, tenantHelper),
                "AccountDetails" => new AccountDetails(accountId, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, username, tenantHelper),
                "AgeingToday" => new AgeingToday(accountId, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, username, tenantHelper),
                "EndDate" => new EndDate(accountId, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, username, tenantHelper),
                "payableRetention" => new payableRetention(accountId, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, username, tenantHelper),
                "Balance" => new Balance(accountId, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, username, tenantHelper),
                "BillsPayablePaid" => new BillsPayablePaid(accountId, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, username, tenantHelper),
                _ => throw new ArgumentException("Invalid report name.")
            };



            report.CreateDocument();
            return report;
        }

        [HttpGet("DownloadVAT")]
        public IActionResult DownloadVATReport(
    string reportName,
    string invoiceNo = null,
    string creditNoteNo = null,
    string debitNoteNo = null,
    bool isApproved = false)
        {
            if (string.IsNullOrEmpty(reportName))
                return BadRequest("Invalid report parameters.");

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                return StatusCode(500, "Tenant or DbContext could not be resolved.");

            var tenantName = HttpContext.Session.GetString("TenantName") ?? "DefaultTenant";
            var username = HttpContext.Session.GetString("UserName") ?? "DefaultUser";
            var defaultCompanyIdString = HttpContext.Session.GetString("DefaultcompanyID");

            if (!int.TryParse(defaultCompanyIdString, out int defaultCompanyId))
                defaultCompanyId = 0;

            string companyName = "", companyAddress = "", companyNameAr = "", companyAddressAr = "";
            string companyPhone = "", companyWebsite = "", companyEmail = "";
            Image logoImage = null, companySealImage = null;

            var companyDetails = dbContext.Tbl901CompanyDetails
                .FirstOrDefault(x => x.CompanyId == defaultCompanyId);

            if (companyDetails != null)
            {
                companyName = companyDetails.CompanyName ?? "";
                companyAddress = companyDetails.CompanyFullAddress ?? "";
                companyAddressAr = companyDetails.CompanyFullAddressAr ?? "";
                companyNameAr = companyDetails.CompanyNameAr ?? "";
                companyPhone = companyDetails.CompanyPhone ?? string.Empty;
                companyWebsite = companyDetails.Website ?? string.Empty;
                companyEmail = companyDetails.EmailAddress ?? string.Empty;

                if (companyDetails.CompanyLogo?.Length > 0)
                {
                    try
                    {
                        using var ms = new MemoryStream(companyDetails.CompanyLogo);
                        logoImage = Image.FromStream(ms);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error reading company logo: " + ex.Message);
                    }
                }

                if (companyDetails.CompanySeal?.Length > 0)
                {
                    try
                    {
                        using var ms = new MemoryStream(companyDetails.CompanySeal);
                        companySealImage = Image.FromStream(ms);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error reading company seal: " + ex.Message);
                    }
                }
            }

            // Decide which report parameter to pass
            string documentNumber = "";
            if (!string.IsNullOrEmpty(invoiceNo))
                documentNumber = invoiceNo;
            else if (!string.IsNullOrEmpty(creditNoteNo))
                documentNumber = creditNoteNo;
            else if (!string.IsNullOrEmpty(debitNoteNo))
                documentNumber = debitNoteNo;
            else
                return BadRequest("No valid document number provided.");

            var report = GenerateVATReport(
                  reportName,
                  invoiceNo,
                  creditNoteNo,
                  debitNoteNo,
                  tenantName,
                  companyName,
                  companyAddress,
                  logoImage,
                  companySealImage,
                  companyNameAr,
                  companyAddressAr,
                  companyEmail,
                  companyWebsite,
                  companyPhone,
                  isApproved,
                  username,
                  _tenantDbContextHelper
              );


            using var stream = new MemoryStream();
            report.ExportToPdf(stream);
            stream.Position = 0;
            return File(stream.ToArray(), "application/pdf", $"{reportName}_{documentNumber}.pdf");
        }


        private XtraReport GenerateVATReport(
      string reportName,
      string invoiceNo,
      string creditNoteNo,
      string debitNoteNo,
      string tenantName,
      string companyName,
      string companyAddress,
      Image logoImage,
      Image companySealImage,
      string companyNameAr,
      string companyAddressAr,
      string companyEmail,
      string companyWebsite,
      string companyPhone,
      bool isApproved,string username,
      TenantDbContextHelper tenantHelper)
        {
            XtraReport report;

            switch (reportName)
            {
                case "TAXINVOICEWTDOCUMENTALLEVELDISCOUNTSS":
                    report = new QD.ERP.Web.Areas.VAT.Reports.B2B_INVOICE
                        .TAXINVOICEWTDOCUMENTALLEVELDISCOUNTSS(
                            invoiceNo, // Only invoiceNo for this report
                            tenantName,
                            companyName,
                            companyAddress,
                            logoImage,
                            companySealImage,
                            companyNameAr,
                            companyAddressAr,
                            isApproved,
                            tenantHelper
                        );
                    break;

                case "PrintsimplifiedTaxInvoices":
                    report = new QD.ERP.Web.Areas.VAT.Reports.B2B_INVOICE.PrintsimplifiedTaxInvoices(
                        invoiceNo, // This also uses invoiceNo
                        tenantName,
                        companyName,
                        companyAddress,
                        logoImage,
                        companyNameAr,
                        companyAddressAr,
                        companyEmail,
                        companyWebsite,
                        companyPhone,
                        isApproved,
                        tenantHelper
                    );
                    break;

                case "creditnote":
                    report = new QD.ERP.Web.Areas.VAT.Reports.VATCreditNote.creditnote(
                        creditNoteNo, tenantName, companyName, companyAddress,  companyNameAr, companyAddressAr, isApproved, username,
                        tenantHelper
                    );
                    break;


                case "DebitNoteView":
                    report = new QD.ERP.Web.Areas.VAT.Reports.VATDebitNote.DebitNoteView(
                         debitNoteNo, tenantName, companyName, companyAddress, companyNameAr, companyAddressAr, isApproved, username,
                        tenantHelper
                    );
                    break;
                case "BillsPurchases":
                    report = new QD.ERP.Web.Areas.VAT.Reports.PurchaseRegister.BillsPurchases(
                 invoiceNo, tenantName, companyName, companyAddress, logoImage,
                 companyNameAr, companyAddressAr, isApproved, tenantHelper);
                                        break;
                default:
                    throw new ArgumentException("Invalid VAT report name.");
            }

            report.CreateDocument();
            return report;
        }


    }
}
