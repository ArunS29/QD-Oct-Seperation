using DevExpress.XtraReports.UI;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Reports;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QD.ERP.Web.ReportService
{
    public static class ReportsFactory
    {
        // Store reports with a function to generate them
        public static Dictionary<string, Func<XtraReport>> Reports = new Dictionary<string, Func<XtraReport>>()
        {
            ["StatementOfAccountReport"] = () => new StatementOfAccountReport(),
            ["AccountWithNarration"] = () => new AccountWithNarration()
        };

        public static XtraReport GetReportFromDatabase(string reportName, ReportDbContext dbContext)
        {
            var reportData = dbContext.ReportAttributes.FirstOrDefault(x => x.ReportName == reportName);
            if (reportData != null)
            {
                // Create a new report from the stored layout XML data
                XtraReport report = new XtraReport();
                using (var stream = new MemoryStream(reportData.ReportXML))
                {
                    report.LoadLayoutFromXml(stream);  // Load the saved layout into the report
                }
                return report;
            }
            else
            {
                // If not found in the database, return null or throw exception if needed
                return null;
            }
        }
    }
}
