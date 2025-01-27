using DevExpress.AspNetCore.Reporting.WebDocumentViewer;
using DevExpress.XtraReports.Web.WebDocumentViewer;
using DevExpress.XtraReports.UI;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DevExpress.XtraReports.Web;
using QD.ERP.Web.Areas.Finance.Reports;
using QD.ERP.Web.Reports;

namespace QD.ERP.Web.Areas.Finance.Pages
{
    public class ViewerPageModel : PageModel
    {
        private readonly IWebDocumentViewerClientSideModelGenerator _viewerModelGenerator;

        public ViewerPageModel(IWebDocumentViewerClientSideModelGenerator viewerModelGenerator)
        {
            _viewerModelGenerator = viewerModelGenerator;
        }

        public object DocumentViewerModel { get; private set; }

        public void OnGet()
        {
            try
            {
                var report = new Report2();
                var cachedReportSource = new CachedReportSourceWeb(report);
                DocumentViewerModel = _viewerModelGenerator.GetModel(cachedReportSource, HttpContext.Request.Path);

                if (DocumentViewerModel == null)
                {
                    throw new Exception("DocumentViewerModel is null. Check report generation logic.");
                }
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error: {ex.Message}");
            }
        }


    }
}
