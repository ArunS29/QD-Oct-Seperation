using DevExpress.XtraReports.UI;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Reports;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace QD.ERP.Web.Service.ReportService
{
    public static class ReportsFactory
    {
        public static Dictionary<string, Func<XtraReport>> Reports = new Dictionary<string, Func<XtraReport>>()
        {
            ["StatementOfAccountReport"] = () => new StatementOfAccountReport(),
            ["AccountWithNarration"] = () => new AccountWithNarration()
        };

        public static XtraReport GetReportFromDatabase(string reportName, ReportDbContext dbContext)
        {
            try
            {
                var reportData = dbContext.ReportAttributes.FirstOrDefault(x => x.ReportName == reportName);

                if (reportData != null)
                {
                    XtraReport report = new XtraReport();

                    using (var stream = new MemoryStream(reportData.ReportXML))
                    {
                        report.LoadLayoutFromXml(stream);
                    }

                    return report;
                }
                else
                {
                    Console.WriteLine($"Report '{reportName}' not found in the database.");

                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error in GetReportFromDatabase for report '{reportName}': {ex.Message}");
                throw new InvalidOperationException($"An error occurred while retrieving the report '{reportName}' from the database.", ex);
            }
        }
    }
}
