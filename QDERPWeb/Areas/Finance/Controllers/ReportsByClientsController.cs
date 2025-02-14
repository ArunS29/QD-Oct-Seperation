using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.DAL.Entities;
using DevExpress.XtraReports.UI;
using QD.ERP.Web.Areas.Finance.Reports;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports;
using DevExpress.DataAccess.Native.Json;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("/api/[controller]/[action]")]
    [ApiController]
    public class ReportsByClientsController : Controller
    {
        private ERPMasterWtDataContext _context;
        public ReportsByClientsController(ERPMasterWtDataContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<IActionResult> GetAccountHead(DataSourceLoadOptions loadOptions)
        {
            try
            {
                var AccountHeadData = _context.Qry20110SundryDebtors.Select(i => new
                {
                    i.AccountId,
                    i.AccountHead
                });
                return Json(await DataSourceLoader.LoadAsync(AccountHeadData, loadOptions));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
            }

        }
        [HttpGet]
        public async Task<IActionResult> GetSalesPerson(DataSourceLoadOptions loadOptions)
        {
            try
            {
                var SalesPersonData = _context.Tbl20101SalesPersonMasters.Select(i => new
                {
                    i.SalesPersonCode,
                    i.SalesPersonName,
                });

                return Json(await DataSourceLoader.LoadAsync(SalesPersonData, loadOptions));
            }
            catch (Exception ex)
            {

                return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetCompanyBranch(DataSourceLoadOptions loadOptions)
        {
            try
            {
                var SalesPersonData = _context.Tbl20115CompanyBranches.Select(i => new
                {
                    i.BranchCode,
                    i.BranchName

                });

                return Json(await DataSourceLoader.LoadAsync(SalesPersonData, loadOptions));
            }
            catch (Exception ex)
            {

                return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
            }
        }
        //public IActionResult GenerateAgeingreportsummaryReport()
        //{
        //    // Create the report instance

        //    var report = new XtraReport3();

        //    // Export the report to PDF using a MemoryStream
        //    using (var stream = new MemoryStream())
        //    {
        //        report.ExportToPdf(stream);
        //        stream.Seek(0, SeekOrigin.Begin);

        //        // Return the PDF as a file result
        //        return File(stream.ToArray(), "application/pdf", "XtraReport3.pdf");
        //    }
        //}
        [HttpPost]
        public IActionResult GenerateReportAccountHead([FromBody] string[] selectedIds)
        {
            try
            {
                if (selectedIds == null || selectedIds.Length == 0)
                {
                    return BadRequest("No records selected.");
                }

                var accountIdsLength = _context.Qry20110SundryDebtors
                                               .Select(d => d.AccountId)
                                               .Count();

                string ids = (selectedIds.Length == accountIdsLength) ? "0" : string.Join(",", selectedIds);

                // Redirect to the Designer page and pass selectedIds as a query parameter
                return RedirectToPage("/DocumentViewer", new { reportName = "AccountsPayableReport", selectedIds = ids });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error: " + ex.Message);
            }
        }
        [HttpPost]
        public IActionResult GenerateReportSalesPerson([FromBody] string[] selectedIds)
        {
            try
            {
                if (selectedIds == null || selectedIds.Length == 0)
                {
                    return BadRequest("No records selected.");
                }

                var accountIdsLength = _context.Tbl20101SalesPersonMasters
                                               .Select(d => d.SalesPersonCode)
                                              .Count();

                string ids = (selectedIds.Length == accountIdsLength) ? "0" : string.Join(",", selectedIds);

                // Redirect to the Designer page and pass selectedIds as a query parameter
                return RedirectToPage("/DocumentViewer", new { reportName = "AccountsPayableReport", selectedIds = ids });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error: " + ex.Message);
            }
        }
        [HttpPost]
        public IActionResult GenerateReportBranch([FromBody] string[] selectedIds)
        {
            try
            {
                if (selectedIds == null || selectedIds.Length == 0)
                {
                    return BadRequest("No records selected.");
                }

                var accountIdsLength = _context.Tbl20115CompanyBranches
                                               .Select(d => d.BranchCode)
                                              .Count();

                string ids = (selectedIds.Length == accountIdsLength) ? "0" : string.Join(",", selectedIds);

                // Redirect to the Designer page and pass selectedIds as a query parameter
                return RedirectToPage("/DocumentViewer", new { reportName = "AccountsPayableReport", selectedIds = ids });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error: " + ex.Message);
            }
        }















        //   [HttpPost]
        // public IActionResult GenerateReport1([FromBody] string[] selectedIds)
        //{
        //    try
        //    {
        //         if (selectedIds == null || selectedIds.Length == 0)
        //         {
        //             return BadRequest("No records selected.");
        //         }


        //         var accountIdsLength = _context.Qry20110SundryDebtors
        //                                        .Select(d => d.AccountId)
        //                                        .Count();

        //       string ids = (selectedIds.Length == accountIdsLength) ? "0" : string.Join(",", selectedIds);


        //        var report = new AccountsPayableReport();
        //       // Set the parameter for the report (assuming the parameter name is "selectedIds")
        //         report.Parameters["selectedIds"].Value = ids;  // Pass the ids (either '0' or selected IDs)
        //         report.Parameters["selectedIds"].Visible = false;  // Optional: Hide the parameter in the report UI
        //         TempData["Report"] = report;

        //             // Redirect to the Designer view
        //                 return RedirectToAction("Index", "DocumentViewer");

        //       //return View("Designer.cshtml", report);
        //     }
        //     catch (Exception ex)
        //     {

        //         throw;
        //     }
        // }


        //  [HttpPost]
        //public IActionResult GenerateReportSalesPerson([FromBody] string[] selectedIds)
        //{
        //    try
        //    {
        //        if (selectedIds == null || selectedIds.Length == 0)
        //        {
        //            return BadRequest("No records selected.");
        //        }


        //        var accountIdsLength = _context.Tbl20101SalesPersonMasters
        //                                       .Select(d => d.SalesPersonCode)
        //                                       .Count();

        //        // Check if selectedIds.Length is equal to AccountIds length
        //        string ids = (selectedIds.Length == accountIdsLength) ? "0" : string.Join(",", selectedIds);


        //        var report = new XtraReport1();
        //        // Set the parameter for the report (assuming the parameter name is "selectedIds")
        //        report.Parameters["selectedIds"].Value = ids;  // Pass the ids (either '0' or selected IDs)
        //        report.Parameters["selectedIds"].Visible = false;  // Optional: Hide the parameter in the report UI
        //        TempData["Report"] = report;

        //        // Redirect to the Designer view
        //        return RedirectToAction("Index", "Designer");

        //        //return View("Designer.cshtml", report);
        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }
        //}
        //  [HttpPost]
        //public IActionResult GenerateReportBranch([FromBody] string[] selectedIds)
        //{
        //    try
        //    {
        //        if (selectedIds == null || selectedIds.Length == 0)
        //        {
        //            return BadRequest("No records selected.");
        //        }


        //        var accountIdsLength = _context.Tbl20115CompanyBranches
        //                                       .Select(d => d.BranchCode)
        //                                       .Count();

        //        // Check if selectedIds.Length is equal to AccountIds length
        //        string ids = (selectedIds.Length == accountIdsLength) ? "0" : string.Join(",", selectedIds);


        //        var report = new XtraReport1();
        //        // Set the parameter for the report (assuming the parameter name is "selectedIds")
        //        report.Parameters["selectedIds"].Value = ids;  // Pass the ids (either '0' or selected IDs)
        //        report.Parameters["selectedIds"].Visible = false;  // Optional: Hide the parameter in the report UI
        //        TempData["Report"] = report;

        //        // Redirect to the Designer view
        //        return RedirectToAction("Index", "Designer");

        //        //return View("Designer.cshtml", report);
        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }
        //}

    }
}
