using DevExpress.XtraReports.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.Areas.Finance.Reports.ExpensesClaims;
using QD.ERP.Web.Areas.Finance.Reports.test;
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