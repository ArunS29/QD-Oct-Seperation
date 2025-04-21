using System.Drawing;
using System.IO;
using DevExpress.XtraReports.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using QD.ERP.Web.Areas.Finance.Reports.test;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Models.DAL;

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

            // Default company info
            string companyName = string.Empty;
            string companyAddress = string.Empty;
            string companyAddressAr = string.Empty;
            string companyNameAr = string.Empty;
            Image logoImage = null;

            // Fetch company details
            var companyDetails = dbContext.Tbl901CompanyDetails
                .FirstOrDefault(x => x.CompanyNameShort == tenantName);

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
            XtraReport report = GenerateReport(reportName, voucherNo, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper);

            // Convert to PDF
            using (MemoryStream stream = new MemoryStream())
            {
                report.ExportToPdf(stream);
                stream.Position = 0;
                return File(stream.ToArray(), "application/pdf", $"{voucherNo}.pdf");
            }
        }

        private XtraReport GenerateReport(string reportName, string voucherNo, string tenantName, string companyName, string companyAddress, Image logoImage, string companyNameAr, string companyAddressAr, TenantDbContextHelper tenantHelper)
        {
            XtraReport report;

            switch (reportName)
            {
                case "cashPayments":
                    report = new cashPayments(voucherNo, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, tenantHelper);
                    break;
                // Add other report types if needed

                default:
                    throw new ArgumentException("Invalid report name.");
            }

            report.Parameters["VoucherNo"].Value = voucherNo;
            report.Parameters["VoucherNo"].Visible = false;

            report.CreateDocument();
            return report;
        }
    }
}
