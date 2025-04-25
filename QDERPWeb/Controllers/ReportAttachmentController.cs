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
    public class ReportAttachmentController : ControllerBase
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;

        public ReportAttachmentController(TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
        }

        [HttpPost("Prepare")]
        public IActionResult PrepareEmailAttachment([FromBody] ReportAttachmentRequest request)
        {
            if (string.IsNullOrEmpty(request.ReportName) || string.IsNullOrEmpty(request.VoucherNo))
                return BadRequest("Invalid report parameters.");

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                return StatusCode(500, "Tenant not found or DbContext could not be created.");

            var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";

            string companyName = string.Empty;
            string companyAddress = string.Empty;
            string companyAddressAr = string.Empty;
            string companyNameAr = string.Empty;
            Image logoImage = null;

            var companyDetails = dbContext.Tbl901CompanyDetails
                .FirstOrDefault(x => x.CompanyNameShort == tenantName);

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

            // Generate report
            XtraReport report = GenerateReport(request.ReportName, request.VoucherNo, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr);

            // Define path to save the report in wwwroot/attachments/
            string attachmentsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "attachments");
            if (!Directory.Exists(attachmentsDirectory))
                Directory.CreateDirectory(attachmentsDirectory);

            string fileName = $"{request.VoucherNo}_{Guid.NewGuid():N}.pdf";
            string fullPath = Path.Combine(attachmentsDirectory, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                report.ExportToPdf(stream);
            }

            HttpContext.Session.SetString("EmailAttachmentPath", fullPath);

            return Ok(new { success = true, fileName });
        }


        private XtraReport GenerateReport(string reportName, string voucherNo, string tenantName, string companyName, string companyAddress, Image logoImage, string companyNameAr, string companyAddressAr)
        {
            XtraReport report;

            switch (reportName)
            {
                case "cashPayments":
                    report = new cashPayments(voucherNo, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, _tenantDbContextHelper);
                    break;

                default:
                    throw new ArgumentException("Invalid report name.");
            }

            report.Parameters["VoucherNo"].Value = voucherNo;
            report.Parameters["VoucherNo"].Visible = false;
            report.CreateDocument();
            return report;
        }
    }

    public class ReportAttachmentRequest
    {
        public string ReportName { get; set; }
        public string VoucherNo { get; set; }
    }
}
