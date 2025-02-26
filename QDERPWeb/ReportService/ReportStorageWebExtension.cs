using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks; // For async
using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.Web.Extensions;
using QD.ERP.Web.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace QD.ERP.Web.ReportService
{
    public class ReportStorageWebExtension : DevExpress.XtraReports.Web.Extensions.ReportStorageWebExtension
    {
        // DbContext for accessing the report table
        protected ReportDbContext DbContext { get; set; }

        // Constructor that takes the DbContext for database access
        public ReportStorageWebExtension(ReportDbContext dbContext)
        {
            DbContext = dbContext;
        }

        // Determines whether a report with the specified URL can be saved.
        public override bool CanSetData(string url)
        {
            // This can be customized based on business logic.
            return true;
        }

        // Determines whether the URL passed to the current report storage is valid.
        public override bool IsValidUrl(string url)
        {
            return !string.IsNullOrEmpty(url);
        }

        // Retrieves report layout data from the database or ReportsFactory based on the URL
        public override async Task<byte[]> GetDataAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new DevExpress.XtraReports.Web.ClientControls.FaultException("Report name cannot be empty.");
            }

            // Log the requested report
            Console.WriteLine($"Requested report: {url}");

            // First, check if the report exists in the database
            var reportData = await DbContext.ReportAttributes
                .Where(x => x.ReportName == url)
                .Select(x => x.ReportXML)
                .FirstOrDefaultAsync();

            if (reportData != null)
            {
                // Log that the report was found in the database
                Console.WriteLine($"Found report '{url}' in the database.");
                return reportData;
            }

            // If the report is not found in the database, check ReportsFactory
            if (ReportsFactory.Reports.ContainsKey(url))
            {
                Console.WriteLine($"Found report '{url}' in ReportsFactory.");
                using var ms = new MemoryStream();
                using XtraReport report = ReportsFactory.Reports[url]();
                report.SaveLayoutToXml(ms);  // Save the layout to a MemoryStream
                return ms.ToArray();
            }

            // Fallback: Try to load the report from the database using ReportsFactory.GetReportFromDatabase
            var reportFromDb = ReportsFactory.GetReportFromDatabase(url, DbContext);
            if (reportFromDb != null)
            {
                Console.WriteLine($"Found report '{url}' in the database via fallback.");
                using var ms = new MemoryStream();
                reportFromDb.SaveLayoutToXml(ms);  // Save the layout to a MemoryStream
                return ms.ToArray();
            }

            // Log that the report was not found anywhere
            Console.WriteLine($"Report '{url}' not found anywhere.");

            // If no report found, throw an exception
            throw new DevExpress.XtraReports.Web.ClientControls.FaultException($"Could not find report '{url}'.");
        }

        // Saves the specified report to the report storage with the specified name
        public override async Task SetDataAsync(XtraReport report, string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("The report URL cannot be null or empty.");
            }

            using var stream = new MemoryStream();
            report.SaveLayoutToXml(stream);  // Save the report layout to a MemoryStream

            var reportData = await DbContext.ReportAttributes
                .FirstOrDefaultAsync(x => x.ReportName == url);

            if (reportData == null)
            {
                // If report doesn't exist, insert a new record into the database
                reportData = new ReportAttribute
                {
                    ReportName = url,
                    ReportXML = stream.ToArray()  // Store the layout data as byte[]
                };

                if (reportData.ReportName.Length > 255)
                {
                    throw new Exception("Report name is too long.");
                }

                reportData.ReportNo = GenerateReportNo(); // Generate ReportNo if not already set
                await DbContext.ReportAttributes.AddAsync(reportData);
            }
            else
            {
                // If report exists, update the existing record
                reportData.ReportXML = stream.ToArray();
            }

            await DbContext.SaveChangesAsync();
        }

        // Set new report data, typically called when a new report is created
        public override async Task<string> SetNewDataAsync(XtraReport report, string defaultUrl)
        {
            await SetDataAsync(report, defaultUrl);  // Call the SetData method to save the report
            return defaultUrl;
        }

        // Helper method to generate a ReportNo if not already set (this can vary based on your business logic)
        private string GenerateReportNo()
        {
            var guid = Guid.NewGuid().ToString("N"); // "N" format removes the hyphens
            return guid.Substring(0, 10);  // Extract the first 10 characters
        }

        // Returns a dictionary that contains the report names (URLs) and display names
        public override async Task<Dictionary<string, string>> GetUrlsAsync()
        {
            var reportNames = await DbContext.ReportAttributes
                .Select(x => x.ReportName)
                .ToListAsync();

            return reportNames
                .Union(ReportsFactory.Reports.Select(x => x.Key))
                .ToDictionary(x => x, x => x); // Return a dictionary of report names
        }
    }
}
