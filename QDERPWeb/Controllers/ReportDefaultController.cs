using DevExpress.XtraReports.UI;
using Microsoft.AspNetCore.Mvc;
using QD.ERP.Finance.Areas.Finance.Reports.TrialBalance;
using QD.ERP.Web.Reports;
using QD.ERP.Shared.Service.ReportService;
using System;
using System.IO;
using System.Threading.Tasks;

namespace QD.ERP.Web.Controllers
{
    [ApiController]
    [Route("api/report/default")]
    public class ReportDefaultController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public ReportDefaultController(IWebHostEnvironment env)
        {
            _env = env;
        }

        // Save as Default Layout to File System or Database
        [HttpPost("set")]
        public async Task<IActionResult> SetDefaultLayout([FromBody] DefaultLayoutDto dto)
        {
            try
            {
                var path = Path.Combine(_env.ContentRootPath, "ReportLayouts", dto.ReportName + ".layout");
                var layoutBytes = Convert.FromBase64String(dto.LayoutBase64);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                await System.IO.File.WriteAllBytesAsync(path, layoutBytes);
                return Ok();
            }
            catch
            {
                return StatusCode(500);
            }
        }

        // Load Original Layout from Code
        [HttpGet("reset")]
        public IActionResult ResetToOriginalLayout(string reportName)
        {
            XtraReport report = reportName switch
            {
                "StatementOfAccountReport" => new StatementOfAccountReport(), // add your actual report
                "TrialBalanceReport" => new TrialBalanceReport(),
                // Add all your reports here
                _ => null
            };

            if (report == null)
                return NotFound("Report not found");

            using var ms = new MemoryStream();
            report.SaveLayoutToXml(ms);
            ms.Position = 0;
            using var reader = new StreamReader(ms);
            return Content(reader.ReadToEnd(), "text/xml");
        }
    }

    public class DefaultLayoutDto
    {
        public string ReportName { get; set; }
        public string LayoutBase64 { get; set; }
    }

}
