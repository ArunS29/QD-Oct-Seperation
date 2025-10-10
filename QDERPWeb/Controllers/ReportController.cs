using DevExpress.XtraReports.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.Areas.Finance.Reports.ExpensesClaims;
using QD.ERP.Web.Areas.Finance.Reports.test;
using QD.ERP.Web.Areas.Finance.Reports.TrialBalance;
using QD.ERP.Web.Areas.Finance.Reports.TrialBalance.AgeingReport;
using QD.ERP.Web.Areas.Finance.Reports.TrialBalance.AgeingReports;
using QD.ERP.Web.Areas.Finance.Reports.Register;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Models.DAL;
using System.Drawing;
using System.IO;
using System.Linq;

namespace QD.ERP.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;

        public ReportController(TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
        }

        [HttpGet("Download")]
        public IActionResult DownloadReport(string reportName, string voucherNo)
        {
            if (string.IsNullOrEmpty(reportName) || string.IsNullOrEmpty(voucherNo))
            {
                return BadRequest("Invalid report parameters.");
            }

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
            {
                return StatusCode(500, "Tenant not found or DbContext could not be created.");
            }

            // Get tenant name from session
            var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
            var username = HttpContext.Session.GetString("UserName") ?? "Default Tenant";
            var defaultCompanyIdString = HttpContext.Session.GetString("DefaultcompanyID");

            // Parse it to int (you may want to use long or Guid if that's your actual ID type)
            if (!int.TryParse(defaultCompanyIdString, out int defaultCompanyId))
            {
                // Handle invalid or missing ID (fallback or error handling)
                defaultCompanyId = 0; // or return early / throw error
            }
            // Default company info
            string companyName = string.Empty;
            string companyAddress = string.Empty;
            string companyAddressAr = string.Empty;
            string companyNameAr = string.Empty;
            Image logoImage = null;

            // Fetch company details
            var companyDetails = dbContext.Tbl901CompanyDetails
                .FirstOrDefault(x => x.CompanyId == defaultCompanyId);

            if (companyDetails != null)
            {
                companyName = companyDetails.CompanyName ?? string.Empty;
                companyAddress = companyDetails.CompanyFullAddress ?? string.Empty;
                companyAddressAr = companyDetails.CompanyFullAddressAr ?? string.Empty;
                companyNameAr = companyDetails.CompanyNameAr ?? string.Empty;

                // Handle logo image
                if (companyDetails.CompanyLogo != null && companyDetails.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(companyDetails.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }
            }

            // Generate report dynamically
            XtraReport report = GenerateReport(reportName, voucherNo, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, username, _tenantDbContextHelper);

            // Convert to PDF
            using (MemoryStream stream = new MemoryStream())
            {
                report.ExportToPdf(stream);
                stream.Position = 0;
                return File(stream.ToArray(), "application/pdf", $"{voucherNo}.pdf");
            }
        }
        [HttpGet("DownloadReports")]
        public IActionResult DownloadReports(string reportName, string frmDate, string toDate, string accountGroup = null, string format = "pdf")
        {
            if (string.IsNullOrEmpty(reportName) || string.IsNullOrEmpty(frmDate) || string.IsNullOrEmpty(toDate))
            {
                return BadRequest("Invalid report parameters. reportName, frmDate, and toDate are required.");
            }

            // Validate format parameter
            format = format.ToLower();
            if (format != "pdf" && format != "excel")
            {
                return BadRequest("Invalid format. Supported formats are 'pdf' and 'excel'.");
            }

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
            {
                return StatusCode(500, "Tenant not found or DbContext could not be created.");
            }

            // Get tenant/session info
            var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
            var username = HttpContext.Session.GetString("UserName") ?? "Default User";
            var defaultCompanyIdString = HttpContext.Session.GetString("DefaultcompanyID");

            if (!int.TryParse(defaultCompanyIdString, out int defaultCompanyId))
            {
                defaultCompanyId = 0;
            }

            // Parse dates
            if (!DateTime.TryParse(frmDate, out DateTime fromDate) || !DateTime.TryParse(toDate, out DateTime toEndDate))
            {
                return BadRequest("Invalid date format. Please use a valid date format.");
            }

            // Company info
            string companyName = string.Empty;
            string companyAddress = string.Empty;
            string companyAddressAr = string.Empty;
            string companyNameAr = string.Empty;
            Image logoImage = null;

            var companyDetails = dbContext.Tbl901CompanyDetails
                .FirstOrDefault(x => x.CompanyId == defaultCompanyId);

            if (companyDetails != null)
            {
                companyName = companyDetails.CompanyName ?? string.Empty;
                companyAddress = companyDetails.CompanyFullAddress ?? string.Empty;
                companyAddressAr = companyDetails.CompanyFullAddressAr ?? string.Empty;
                companyNameAr = companyDetails.CompanyNameAr ?? string.Empty;

                if (companyDetails.CompanyLogo != null && companyDetails.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(companyDetails.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }
            }

            // Generate financial report
            XtraReport report = GenerateFinancialReport(
                reportName, fromDate, toEndDate, accountGroup,
                tenantName, companyName, companyAddress,
                logoImage, companyNameAr, companyAddressAr,
                username, _tenantDbContextHelper
            );

            // Export based on selected format with streaming support
            using (MemoryStream stream = new MemoryStream())
            {
                string fileName;
                string contentType;

                if (format == "excel")
                {
                    // Export to Excel
                    report.ExportToXlsx(stream);
                    fileName = $"{reportName}_{fromDate:yyyy-MM-dd}_to_{toEndDate:yyyy-MM-dd}.xlsx";
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                }
                else
                {
                    // Export to PDF (default)
                    report.ExportToPdf(stream);
                    fileName = $"{reportName}_{fromDate:yyyy-MM-dd}_to_{toEndDate:yyyy-MM-dd}.pdf";
                    contentType = "application/pdf";
                }

                byte[] fileBytes = stream.ToArray();
                
                // Set proper headers for download progress
                Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                Response.Headers.Add("Content-Length", fileBytes.Length.ToString());
                Response.Headers.Add("Content-Type", contentType);
                Response.Headers.Add("Accept-Ranges", "bytes");
                
                return File(fileBytes, contentType, fileName);
            }
        }

        private XtraReport GenerateReport(string reportName, string voucherNo, string tenantName, string companyName, string companyAddress, Image logoImage, string companyNameAr, string companyAddressAr, string username, TenantDbContextHelper tenantHelper)
        {
            XtraReport report;

            switch (reportName)
            {
                case "cashPayments":
                    report = new cashPayments(voucherNo, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, username, tenantHelper);
                    break;
                // Add other report types if needed
                case "PreviewClaimRequestForm":
                    report = new PreviewClaimRequestForm(voucherNo, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr,tenantHelper,username);
                    break;  
                case "ClaimDetailed":
                    report = new ClaimDetailed(voucherNo, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr,tenantHelper,username);
                    break;  
                default:
                    throw new ArgumentException("Invalid report name.");
            }

            report.Parameters["VoucherNo"].Value = voucherNo;
            report.Parameters["VoucherNo"].Visible = false;

            report.CreateDocument();
            return report;
        }

        private XtraReport GenerateFinancialReport(string reportName, DateTime frmDate, DateTime toDate, string accountGroup, string tenantName, string companyName, string companyAddress, Image logoImage, string companyNameAr, string companyAddressAr, string username, TenantDbContextHelper tenantHelper)
        {
            XtraReport report;

            try
            {
                switch (reportName)
                {
                    // Trial Balance Reports
                    case "TrialBalanceReport":
                        report = new TrialBalanceReport(frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, tenantHelper,username);
                        break;

                    case "TrialBalanceExportFormat":
                        report = new TrialBalance_ExportFormat_(frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, tenantHelper, username);
                        break;

                    case "TrialBalanceDrCr":
                        report = new TrialBalanceDrCr(frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, tenantHelper, username);
                        break;

                    // Account Group Reports
                    case "Group":
                        report = new Group(accountGroup, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, tenantHelper);
                        break;

                    case "subGroup":
                        report = new subGroup(accountGroup, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, tenantHelper);
                        break;

                    // Income Statement Reports
                    case "IncomeStatements":
                        report = new IncomeStatements(accountGroup, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, tenantHelper, username);
                        break;

                    case "IncomestatementsHorizondal":
                        report = new IncomestatementsHorizondal(accountGroup, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, tenantHelper);
                        break;

                    // Balance Sheet Reports
                    case "balnceSheet":
                        report = new balnceSheet(accountGroup, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, tenantHelper, username);
                        break;

                    case "BalanceSheetHorizondalFormat":
                        report = new BalanceSheetHorizondalFormat(accountGroup, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, tenantHelper);
                        break;

                    // Bills Receivable Aging Reports
                    case "BillsReceivableEndDate":
                        report = new BillsReceivableEndDate(accountGroup, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, tenantHelper, username);
                        break;

                    case "BillsReceivableledgerBalance":
                        report = new BillsReceivableledgerBalance(accountGroup, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, tenantHelper, username);
                        break;

                    case "BillsReceivablesSummaryAgeingasperLedgerBalanceByEndDate":
                        report = new BillsReceivablesSummaryAgeingasperLedgerBalanceByEndDate(accountGroup, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, tenantHelper, username);
                        break;

                    case "BillsReceivablesReportByEndDate":
                        report = new BillsReceivablesReportByEndDate(accountGroup, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, tenantHelper, username);
                        break;

                    // Bills Payable Aging Reports
                    case "BillsPayablesAgeingByEndDate":
                        report = new BillsPayablesAgeingByEndDate(accountGroup, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, tenantHelper, username);
                        break;

                    case "BillsPayablesSummaryAgeingasperLedgerBalanceByEndDate":
                        report = new BillsPayablesSummaryAgeingasperLedgerBalanceByEndDate(accountGroup, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, tenantHelper, username);
                        break;

                    case "billsPayableSummaryLedgerBalance":
                        report = new BillsPayablesAgeingasperLedgerBalanceByEndDate(accountGroup, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, tenantHelper, username);
                        break;

                    // Reports not yet implemented as XtraReport classes
                    case "incomeStatementsBymonth":
                    case "BillsPayableReportEnddate":
                        // Create a temporary report with a message directing to DocumentViewer
                        var tempReport = new XtraReport();
                        
                        var detailBand = new DetailBand();
                        var label = new XRLabel();
                        label.Text = $"Report '{reportName}' is available through DocumentViewer.\n\n" +
                                    $"Please use the DocumentViewer page to access this report.\n\n" +
                                    $"Report Parameters:\n" +
                                    $"From Date: {frmDate:yyyy-MM-dd}\n" +
                                    $"To Date: {toDate:yyyy-MM-dd}\n" +
                                    $"Company: {companyName}";
                        label.Font = new Font("Arial", 12);
                        label.BoundsF = new System.Drawing.RectangleF(50, 50, 500, 200);
                        label.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft;
                        
                        detailBand.Controls.Add(label);
                        detailBand.HeightF = 300;
                        
                        tempReport.Bands.Add(detailBand);
                        tempReport.CreateDocument();
                        return tempReport;

                    default:
                        throw new ArgumentException($"Invalid financial report name: {reportName}");
                }

                // Create the document for actual reports
                report.CreateDocument();
                return report;
            }
            catch (Exception ex)
            {
                // If report creation fails, create an error report
                var errorReport = new XtraReport();
                var detailBand = new DetailBand();
                var label = new XRLabel();
                label.Text = $"Error generating report '{reportName}': {ex.Message}\n\n" +
                            $"Please contact system administrator or use DocumentViewer page.";
                label.Font = new Font("Arial", 12);
                label.BoundsF = new System.Drawing.RectangleF(50, 50, 500, 200);
                label.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft;
                
                detailBand.Controls.Add(label);
                detailBand.HeightF = 300;
                
                errorReport.Bands.Add(detailBand);
                errorReport.CreateDocument();
                return errorReport;
            }
        }

        [HttpDelete("DeleteSavedLayout")]
        public IActionResult DeleteSavedLayout([FromQuery] string reportName)
        {
            if (string.IsNullOrWhiteSpace(reportName))
                return BadRequest("Invalid report name");

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                return StatusCode(500, "Tenant not found or DbContext could not be created.");

            var rowsDeleted = dbContext.Database.ExecuteSqlInterpolated(
                $"DELETE FROM Tbl90112ReportAttributes WHERE ReportName = {reportName}");

            if (rowsDeleted > 0)
                return Ok(new { message = "Saved layout deleted" });

            return NotFound(new { message = "No layout found for the given report name" });
        }


    }
}