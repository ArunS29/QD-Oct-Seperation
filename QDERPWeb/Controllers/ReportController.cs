using System.Drawing;
using System.IO;
using DevExpress.XtraReports.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using QD.ERP.Web.Areas.Finance.Reports.test;
using QD.ERP.Web.DAL.Entities;

namespace QD.ERP.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly ERPMasterWtDataContext _eRPMasterWtDataContext;

        public ReportController(ERPMasterWtDataContext eRPMasterWtDataContext)
        {
            _eRPMasterWtDataContext = eRPMasterWtDataContext;
        }

        [HttpGet("Download")]
        public IActionResult DownloadReport(string reportName, string voucherNo)
        {
            if (string.IsNullOrEmpty(reportName) || string.IsNullOrEmpty(voucherNo))
            {
                return BadRequest("Invalid report parameters.");
            }

            // **Get Tenant Name from Session**
            var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";

            // **Initialize Company Information with Defaults**
            string companyName = string.Empty;
            string companyAddress = string.Empty;
            string companyAddressAr = string.Empty;
            string companyNameAr = string.Empty;
            Image logoImage = null;

            // **Fetch Company Details (If Available)**
            var companyDetails = _eRPMasterWtDataContext.Tbl901CompanyDetails
                .FirstOrDefault(x => x.CompanyNameShort == tenantName);

            if (companyDetails != null)
            {
                companyName = companyDetails.CompanyName ?? string.Empty;
                companyAddress = companyDetails.CompanyFullAddress ?? string.Empty;
                companyAddressAr = companyDetails.CompanyFullAddressAr ?? string.Empty;
                companyNameAr = companyDetails.CompanyNameAr ?? string.Empty;

                // **Handle Company Logo (If Available)**
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

            // **Generate the report dynamically**
            XtraReport report = GenerateReport(reportName, voucherNo, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr);

            // Convert the report to a PDF stream
            using (MemoryStream stream = new MemoryStream())
            {
                report.ExportToPdf(stream);
                stream.Position = 0;

                // **Set Content-Disposition for direct download with VoucherNo as filename**
                return File(stream.ToArray(), "application/pdf", $"{voucherNo}.pdf");
            }
        }

        private XtraReport GenerateReport(string reportName, string voucherNo, string tenantName, string companyName, string companyAddress, Image logoImage, string companyNameAr, string companyAddressAr)
        {
            XtraReport report;

            if (reportName == "cashPayments")
            {
                report = new cashPayments(voucherNo, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr);
            }
            else
            {
                throw new ArgumentException("Invalid report name.");
            }

            // Set parameters
            report.Parameters["VoucherNo"].Value = voucherNo;
            report.Parameters["VoucherNo"].Visible = false;

            report.CreateDocument();
            return report;
        }
    }
}
