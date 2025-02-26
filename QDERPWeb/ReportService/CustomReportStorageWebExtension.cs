//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using DevExpress.XtraReports.Web.Extensions;
//using DevExpress.XtraReports.UI;
//using Microsoft.AspNetCore.Hosting;

//public class CustomReportStorageWebExtension : ReportStorageWebExtension
//{
//    private readonly string _reportDirectory;
//    private const string FileExtension = ".repx";

//    public CustomReportStorageWebExtension(IWebHostEnvironment env)
//    {
//        _reportDirectory = Path.Combine(env.ContentRootPath, "Reports");
//        if (!Directory.Exists(_reportDirectory))
//        {
//            Directory.CreateDirectory(_reportDirectory);
//        }
//    }

//    // Get list of stored reports
//    public override Dictionary<string, string> GetUrls()
//    {
//        return Directory.GetFiles(_reportDirectory, "*" + FileExtension)
//                        .Select(Path.GetFileNameWithoutExtension)
//                        .ToDictionary(name => name, name => name);
//    }

//    // Load report from storage
//    //public override XtraReport LoadReport(string url)
//    //{
//    //    string filePath = Path.Combine(_reportDirectory, url + FileExtension);
//    //    if (File.Exists(filePath))
//    //    {
//    //        XtraReport report = new XtraReport();
//    //        report.LoadLayout(filePath);
//    //        return report;
//    //    }
//    //    throw new FileNotFoundException($"Report '{url}' not found.");
//    //}

//    // Check if a report URL is valid
//    public override bool IsValidUrl(string url)
//    {
//        string filePath = Path.Combine(_reportDirectory, url + FileExtension);
//        return File.Exists(filePath);
//    }

//    // Save the report
//    public override void SetData(XtraReport report, string url)
//    {
//        string filePath = Path.Combine(_reportDirectory, url + FileExtension);
//        report.SaveLayout(filePath);
//    }

//    // Allow overwriting existing reports
//    public override bool CanSetData(string url)
//    {
//        return true;
//    }
//}
