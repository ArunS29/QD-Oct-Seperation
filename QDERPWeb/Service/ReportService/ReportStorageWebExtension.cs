using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks; // For async
using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.Web.Extensions;
using QD.ERP.Web.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace QD.ERP.Web.Service.ReportService
{
    public class ReportStorageWebExtension : DevExpress.XtraReports.Web.Extensions.ReportStorageWebExtension
    {
        protected ReportDbContext DbContext { get; set; }

        public ReportStorageWebExtension(ReportDbContext dbContext)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext), "DbContext cannot be null.");
        }

        public override bool CanSetData(string url)
        {
            try
            {
                return !string.IsNullOrEmpty(url);
            }
            catch (Exception ex)
            {
                // Log error
                Console.Error.WriteLine($"Error in CanSetData: {ex.Message}");
                throw new InvalidOperationException("An error occurred while determining if the data can be set.", ex);
            }
        }

        public override bool IsValidUrl(string url)
        {
            try
            {
                return !string.IsNullOrEmpty(url);
            }
            catch (Exception ex)
            {
                // Log error
                Console.Error.WriteLine($"Error in IsValidUrl: {ex.Message}");
                throw new InvalidOperationException("An error occurred while validating the URL.", ex);
            }
        }

        public override async Task<byte[]> GetDataAsync(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                {
                    throw new DevExpress.XtraReports.Web.ClientControls.FaultException("Report name cannot be empty.");
                }

                Console.WriteLine($"Requested report: {url}");

                var reportData = await DbContext.ReportAttributes
                    .Where(x => x.ReportName == url)
                    .Select(x => x.ReportXML)
                    .FirstOrDefaultAsync();

                if (reportData != null)
                {
                    Console.WriteLine($"Found report '{url}' in the database.");
                    return reportData;
                }

                if (ReportsFactory.Reports.ContainsKey(url))
                {
                    Console.WriteLine($"Found report '{url}' in ReportsFactory.");
                    using var ms = new MemoryStream();
                    using XtraReport report = ReportsFactory.Reports[url]();
                    report.SaveLayoutToXml(ms);
                    return ms.ToArray();
                }

                var reportFromDb = ReportsFactory.GetReportFromDatabase(url, DbContext);
                if (reportFromDb != null)
                {
                    Console.WriteLine($"Found report '{url}' in the database via fallback.");
                    using var ms = new MemoryStream();
                    reportFromDb.SaveLayoutToXml(ms);
                    return ms.ToArray();
                }

                Console.WriteLine($"Report '{url}' not found anywhere.");

                throw new DevExpress.XtraReports.Web.ClientControls.FaultException($"Could not find report '{url}'.");
            }
            catch (Exception ex)
            {
                // Log error
                Console.Error.WriteLine($"Error in GetDataAsync: {ex.Message}");
                throw new InvalidOperationException("An error occurred while retrieving report data.", ex);
            }
        }


        public override async Task SetDataAsync(XtraReport report, string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                {
                    throw new ArgumentException("The report URL cannot be null or empty.");
                }

                using var stream = new MemoryStream();
                report.SaveLayoutToXml(stream);

                var reportData = await DbContext.ReportAttributes
                    .FirstOrDefaultAsync(x => x.ReportName == url);

                if (reportData == null)
                {
                    reportData = new ReportAttribute
                    {
                        ReportName = url,
                        ReportXML = stream.ToArray()
                    };

                    if (reportData.ReportName.Length > 255)
                    {
                        throw new Exception("Report name is too long.");
                    }

                    reportData.ReportNo = GenerateReportNo();
                    await DbContext.ReportAttributes.AddAsync(reportData);
                }
                else
                {
                    reportData.ReportXML = stream.ToArray();
                }

                await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log error
                Console.Error.WriteLine($"Error in SetDataAsync: {ex.Message}");
                throw new InvalidOperationException("An error occurred while saving report data.", ex);
            }
        }

        public override async Task<string> SetNewDataAsync(XtraReport report, string defaultUrl)
        {
            try
            {
                await SetDataAsync(report, defaultUrl);
                return defaultUrl;
            }
            catch (Exception ex)
            {
                // Log error
                Console.Error.WriteLine($"Error in SetNewDataAsync: {ex.Message}");
                throw new InvalidOperationException("An error occurred while setting new report data.", ex);
            }
        }

        private string GenerateReportNo()
        {
            try
            {
                var guid = Guid.NewGuid().ToString("N");
                return guid.Substring(0, 10);
            }
            catch (Exception ex)
            {
                // Log error
                Console.Error.WriteLine($"Error in GenerateReportNo: {ex.Message}");
                throw new InvalidOperationException("An error occurred while generating a report number.", ex);
            }
        }

        public override async Task<Dictionary<string, string>> GetUrlsAsync()
        {
            try
            {
                var reportNames = await DbContext.ReportAttributes
                    .Select(x => x.ReportName)
                    .ToListAsync();

                return reportNames
                    .Union(ReportsFactory.Reports.Select(x => x.Key))
                    .ToDictionary(x => x, x => x);
            }
            catch (Exception ex)
            {
                // Log error
                Console.Error.WriteLine($"Error in GetUrlsAsync: {ex.Message}");
                throw new InvalidOperationException("An error occurred while retrieving report URLs.", ex);
            }
        }
    }
}
