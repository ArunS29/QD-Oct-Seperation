using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.DAL.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QD.ERP.Web.Areas.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly ERPMasterWtDataContext _context;

        public ReportController(ERPMasterWtDataContext context)
        {
            _context = context;
        }

        [HttpPost("SaveReportLayout")]
        public async Task<IActionResult> SaveReportLayout([FromBody] ReportLayoutModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.XmlData))
            {
                return BadRequest("Invalid report data.");
            }

            try
            {
                // Check if the report exists
                var existingReport = await _context.Tbl90112ReportAttributes
                    .FirstOrDefaultAsync(r => r.ReportNo == model.ReportNo);

                if (existingReport != null)
                {
                    // Update existing record
                    existingReport.ReportXml = model.XmlData;
                    existingReport.ModifiedBy = "Admin";  // Replace with the logged-in user
                    existingReport.ModifiedOn = DateTime.UtcNow;
                }
                else
                {
                    return NotFound("Report not found.");
                }

                await _context.SaveChangesAsync();
                return Ok("Report layout saved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error saving layout: " + ex.Message);
            }
        }
    }

    public class ReportLayoutModel
    {
        public string ReportNo { get; set; }
        public string XmlData { get; set; }
    }
}
