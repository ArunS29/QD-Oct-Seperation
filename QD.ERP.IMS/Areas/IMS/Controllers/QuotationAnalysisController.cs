using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using QD.ERP.Web.Areas.Finance.Controllers;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Shared.DAL.Entities;
using QD.ERP.Web.Service;
using QD.ERP.Web.Services.Logging;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace QD.ERP.Web.Areas.IMS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class QuotationAnalysisController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<QuotationAnalysisController> _logger;
        private readonly IUserActionLogger _userActionLogger;

        public QuotationAnalysisController(ILogger<QuotationAnalysisController> logger, TenantDbContextHelper tenantDbContextHelper, IUserActionLogger userActionLogger)
        {
            _userActionLogger = userActionLogger;
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }
        [HttpGet]
        public async Task<ActionResult> GetQuotationAnalysis(DataSourceLoadOptions loadOptions)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    using (dbContext)
                    {
                        var ledgerAccounts = dbContext.Qry60707rfqissuedByMprno02s
                            .Where(p => p.Mprno != null)
                            .Select(i => new
                            {
                                i.Mprno,
                                i.NoOfRfqissued,
                                i.ClientName,
                                i.StoreName,
                                i.Mprdate,
                                i.RequestedBy,


                            });

                        return Json(await DataSourceLoader.LoadAsync(ledgerAccounts, loadOptions));
                    }
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetQuotationAnalysis");
                return StatusCode(500, new { message = "Internal Server Error", success = false });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetQuotationAnalysisPvg(string mprno)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var result = await dbContext.Qry60708quotationAnalysisPvgs
                        .Where(x => x.Mprno == mprno)
                        .Select(x => new
                        {
                            x.Mprno,
                            x.SupplierName,
                            x.Gsdescrpition,
                            x.UnitDesc,
                            x.QuotedQuantity,
                            x.UnitPrice,
                            x.ItemDiscount,
                            x.LineTotalBeforeTax,
                            x.LineTotalAfterDisc,
                            x.IsWonForPo,
                            x.Gscode,
                            x.RfqchildSlNo
                        })
                        .ToListAsync();

                    return Ok(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching quotation analysis data for MPR No: {mprno}", mprno);
                    return StatusCode(500, "Internal server error.");
                }
            }

            return BadRequest("Invalid tenant context.");
        }

        [HttpPost]
        public async Task<IActionResult> SetZeroToWon([FromBody] ZeroToWonDto dto)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    await dbContext.Database.ExecuteSqlRawAsync(
                        "EXEC sp607_03UpdateRFQChild_SetZeroToWon @p0, @p1",
                        dto.Mprno,
                        dto.GsCode
                    );

                    return Ok();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating IsWon for MPR No: {mprno}", dto.Mprno);
                    return StatusCode(500, "Internal server error.");
                }
            }

            return BadRequest("Invalid tenant context.");
        }
        [HttpPost]
        public async Task<IActionResult> UpdateIsWon([FromBody] UpdateIsWonDto dto)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    // ✅ Get the logged-in user from session
                    var updatedBy = HttpContext.Session.GetString("UserName");
                    if (string.IsNullOrEmpty(updatedBy))
                        return Unauthorized("User session expired or not available.");

                    await dbContext.Database.ExecuteSqlRawAsync(
                        "EXEC sp607_02UpdateRFQChild_IsWon @p0, @p1, @p2",
                        dto.RFQChildSlNo,
                        updatedBy,
                        dto.ReasonForSelection ?? ""
                    );


                    return Ok(new { message = "Line item has been set as Won for Ordering Process successfully." });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating IsWon for RFQChildSlNo: {rfqchildSlNo}", dto.RFQChildSlNo);
                    return StatusCode(500, "Internal server error occurred while updating RFQChild.");
                }
            }

            return BadRequest("Invalid tenant context.");
        }
        [HttpPost]

        public async Task<IActionResult> CreateQuotationsFromMprList([FromBody] CreateQuotationRequest dto)
        {
            if (dto?.MprNos == null || dto.MprNos.Count == 0)
                return BadRequest(new { success = false, message = "At least one MPR No is required." });

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                return Unauthorized(new { success = false, message = "Invalid tenant." });

            string defaultCompanyString = HttpContext.Session.GetString("DefaultcompanyID") ?? "";
            byte defaultCompanyByte = 0;
            if (!string.IsNullOrEmpty(defaultCompanyString))
                byte.TryParse(defaultCompanyString, out defaultCompanyByte);

            byte companyId = defaultCompanyByte;

            var company = dbContext.Tbl901CompanyDetails.FirstOrDefault(c => c.CompanyId == companyId);
            if (company == null)
                return NotFound(new { success = false, message = "Company not found." });

            int noOfDigits = dbContext.Tbl901CompanyDetails02s
                                    .Where(x => x.CompanyId == company.CompanyId)
                                    .Select(x => x.NoOfDigitsToInventoryQuotation ?? 4)
                                    .FirstOrDefault();

            string abbrv = company.QuotationAbbrv ?? "QT";
            int yearDigits = company.InvoiceYearDigits ?? 0;
            bool isResetByYear = company.IsResetInvoiceInYear ?? false;
            DateTime today = DateTime.Now;
            string addedBy = HttpContext.Session.GetString("UserName") ?? "System";

            var createdQuotations = new List<(string MprNo, string QuoteNo)>();


            foreach (var mprNo in dto.MprNos.Distinct())
            {
                // Check if quotation already exists for this MPR
                bool alreadyExists = dbContext.Tbl60101quotationMasters
                    .Any(q => q.Mprno == mprNo); // adjust column name if needed

                if (alreadyExists)
                    continue;
                // Get existing quote numbers filtered by year if needed
                var existingQuoteNos = dbContext.Tbl60101quotationMasters
                    .Where(q => q.QuoteNo != null &&
                                q.QuoteNo.Length >= noOfDigits &&
                                (!isResetByYear || (q.QuoteDate.HasValue && q.QuoteDate.Value.Year == today.Year)))
                    .Select(q => q.QuoteNo)
                    .ToList();

                int maxRunningNo = existingQuoteNos
                    .Select(no => int.TryParse(no.Substring(no.Length - noOfDigits), out int num) ? num : 0)
                    .DefaultIfEmpty(0)
                    .Max() + 1;

                string paddedNumber = maxRunningNo.ToString().PadLeft(noOfDigits, '0');
                string yearPart = yearDigits > 0 ? today.Year.ToString().Substring(4 - yearDigits) : "";
                string quoteNo = $"{abbrv}{yearPart}-{paddedNumber}";

                // Call your SP
                await dbContext.Database.ExecuteSqlRawAsync(
                    "EXEC sp600_03InsertToQuotationFromEnquiry @QuotationNo = {0}, @MPRNo = {1}, @AddedBy = {2}",
                    quoteNo, mprNo, addedBy
                );

                // Update MPR status
                var mpr = await dbContext.Tbl60601purchaseRequestMasters.FirstOrDefaultAsync(x => x.Mprno == mprNo);
                if (mpr != null)
                {
                    mpr.PurchaseRequestStatusId = 4; // Quotation created
                    await dbContext.SaveChangesAsync();

                    await _userActionLogger.LogAsync(
                        module: "IMS > Created Quotation From Mpr",
                        actionDetail: $":Created Quotation From Mpr {mprNo}",
                        documentNo: $"{mprNo}"
                    );
                }

                createdQuotations.Add((mprNo, quoteNo));
            }

            var firstQuote = createdQuotations.FirstOrDefault();

            if (!string.IsNullOrEmpty(firstQuote.QuoteNo))
            {
                return Ok(new
                {
                    success = true,
                    message = "Quotations created successfully.",
                    quotationNo = firstQuote.QuoteNo
                });
            }

            else
            {
                return BadRequest(new { success = false, message = "No quotations created." });
            }
        }




 


    }
}
