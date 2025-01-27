using DevExpress.AspNetCore.Reporting.WebDocumentViewer;
using DevExpress.XtraReports.UI;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System;
using QD.ERP.Web.Areas.Finance.Reports;
using DevExpress.AspNetCore.Reporting.WebDocumentViewer.Native.Services;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Area("Finance")]
    public class ReportsController : Controller
    {
        public class CustomWebDocumentViewerController : WebDocumentViewerController
        {
            public CustomWebDocumentViewerController(IWebDocumentViewerMvcControllerService controllerService)
                : base(controllerService)
            {
            }
        }
        // This method returns the model for the WebDocumentViewer
        [HttpGet]
        public IActionResult Viewer()
        {
            // Initialize the WebDocumentViewerModel with the report parameters
            var viewerModel = new DocumentViewerModel
            {
                ReportName = "Report1",  // Define the report name
                ReportUrl = Url.Action("GetReport")  // Link to the GetReport action for fetching report data
            };

            // Return the model to the view
            return View(viewerModel);
        }

        // This method returns the report data in PDF format
        [HttpGet]
        public IActionResult GetReport()
        {
            var report = new Report1(); // Create the report instance
            var reportStream = new MemoryStream();

            // Export the report to a memory stream as PDF
            report.ExportToPdf(reportStream);
            reportStream.Seek(0, SeekOrigin.Begin);

            // Return the report stream as a PDF file
            return File(reportStream.ToArray(), "application/pdf");
        }
    }
}
