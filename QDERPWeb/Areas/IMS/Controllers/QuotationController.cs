using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Humanizer;
using DevExtreme.AspNet.Data.ResponseModel;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.Areas.Finance.Reports.Payable_Statements;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace QD.ERP.Web.Areas.IMS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class QuotationController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<QuotationController> _logger;
        public QuotationController(ILogger<QuotationController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }
        [HttpGet]
        public IActionResult GetReportAttributes(string reportNo)
        {
            if (string.IsNullOrEmpty(reportNo))
                return BadRequest(new { message = "Report number is required." });

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var result = dbContext.Tbl90112ReportAttributes
                    .Where(r => r.ReportNo == reportNo)
                    .Select(r => new
                    {
                       r.ReportSubject,
                       r.ReportSummary,
                       r.ReportIntroduction,
                       r.ReportThanksNote
                    })
                    .FirstOrDefault();

                if (result == null)
                    return NotFound(new { message = "No report found" });

                return Json(result);
            }

            return Unauthorized();
        }
        public class ReportAttributeUpdateModel
        {
            public string ReportNo { get; set; }       // Always "IMS-QTN-01"
            public string Field { get; set; }          // e.g., "ReportSubject"
            public string Value { get; set; }          // The new value
        }
        [HttpPost]
        public IActionResult SaveReportAttribute([FromBody] ReportAttributeUpdateModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.ReportNo) || string.IsNullOrEmpty(model.Field))
            {
                return BadRequest(new { message = "Invalid input data" });
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var record = dbContext.Tbl90112ReportAttributes
                    .FirstOrDefault(r => r.ReportNo == model.ReportNo);

                if (record == null)
                    return NotFound(new { message = "Report not found." });

                // Update only the requested field
                switch (model.Field)
                {
                    case "ReportSubject":
                        record.ReportSubject = model.Value;
                        break;
                    case "ReportSummary":
                        record.ReportSummary = model.Value;
                        break;
                    case "ReportIntroduction":
                        record.ReportIntroduction = model.Value;
                        break;
                    case "ReportThanksNote":
                        record.ReportThanksNote = model.Value;
                        break;
                    default:
                        return BadRequest(new { message = "Invalid field name." });
                }

                dbContext.SaveChanges();
                return Ok(new { message = "Field updated successfully." });
            }

            return Unauthorized();
        }

        [HttpGet]
        public async Task<IActionResult> GetQuotation(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var query = dbContext.Qry60104quotationViewMasters.AsQueryable();


                    // Default dates if not provided
                    if (!fromDate.HasValue)
                    {
                        fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1); // Start of the current month
                    }

                    if (!toDate.HasValue)
                    {
                        toDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)); // End of the current month
                    }

                    // Filtering by date range
                    query = query.Where(i => i.QuoteDate >= fromDate && i.QuoteDate <= toDate);

                    // Fetching the data
                    var data = await query.Select(i => new
                    {
                        i.QuoteNo,
                        i.QuoteDate,
                        i.ClientName,
                        i.ClientRefNo,
                        i.Mprno,
                        i.SalesOrderNo,
                        i.SalesPersonName,
                        i.IsSubmitted,
                        i.IsApproved,
                        i.IsVerified,
                        i.NoOfItems,
                        i.TotalBeforeDiscount,
                        i.TotalWithTax,
                        i.TotalTaxAmount,
                        i.TotalAfterDiscount,
                    }).ToListAsync();

                    return Json(data);
                }

                return Unauthorized(new { message = "Invalid tenant." });

            }
            catch (Exception ex)
            {
				_logger.LogError($"An error occurred while fetching the data : {ex.Message}");
				return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
			}
        }
        	[HttpGet]
		public async Task<IActionResult> GetQuotationDetails(DateTime? fromDate, DateTime? toDate)
		{
			try
			{
				if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				{
					var query = dbContext.Qry60124quotationItemDetails.AsQueryable();


					// Default dates if not provided
					if (!fromDate.HasValue)
					{
						fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1); // Start of the current month
					}

					if (!toDate.HasValue)
					{
						toDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)); // End of the current month
					}

					// Filtering by date range
					query = query.Where(i => i.QuoteDate >= fromDate && i.QuoteDate <= toDate);

					// Fetching the data
					var data = await query.Select(i => new
					{
						i.QuoteNo,
						i.QuoteDate,
						i.ClientName,
						i.ClientRefNo,
						i.Mprno,
						i.SalesOrderNo,
						i.SalesPersonName,
					    i.ItemBrand,
                        i.ItemPartNo,
                        i.ItemSize,
                        i.Gscode,
                        i.Gsdescrpition,
                        i.QuotedQuantity,
						i.TotalBeforeDiscount,
						i.TotalWithTax,
						i.TotalTaxAmount,
						i.TotalAfterDiscount,
					}).ToListAsync();

					return Json(data);
				}

				return Unauthorized(new { message = "Invalid tenant." });

			}
			catch (Exception ex)
			{
				_logger.LogError($"An error occurred while fetching the data : {ex.Message}");
				return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
			}
		}

        //Quoted Cost Item Form
        [HttpGet]
        public async Task<IActionResult> GetCostItem(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var clients = dbContext.Tbl60105quotationCostMasters.Select(i => new
                    {
                        i.CostItemCode,
                        i.CostItem,

                    });

                    return Json(await DataSourceLoader.LoadAsync(clients, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"An error occurred while fetching the data : {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant", success = false });
        }
        [HttpGet]
        public async Task<IActionResult> GetCostItemGrid(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var clients = dbContext.Qry60114quotationItemCostEditViews.Select(i => new
                    {
                        i.CostItemCode,
                        i.CostItemDescription,
                        i.CostPercentage,
                        i.CostItemQty,
                        i.CostItemPrice,
                        i.CostItemSubTotal,
                        i.QuoteCostSlNo

                    });

                    return Json(await DataSourceLoader.LoadAsync(clients, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"An error occurred while fetching the data : {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant", success = false });
        }
        [HttpPost]
        public async Task<IActionResult> SaveOrUpdateQuotedCostitem([FromBody] Tbl60104quotationItemCost model)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var existingRecord = await dbContext.Tbl60104quotationItemCosts
                        .FirstOrDefaultAsync(x => x.QuoteCostSlNo == model.QuoteCostSlNo);

                    if (existingRecord != null)
                    {
                        // Update existing record

                        existingRecord.CostItemCode = model.CostItemCode;
                        existingRecord.CostPercentage = model.CostPercentage;
                        existingRecord.CostItemQty = model.CostItemQty;
                        existingRecord.CostItemPrice = model.CostItemPrice;


                        await dbContext.SaveChangesAsync();

                        return Ok(new { success = true, message = "Updated successfully", id = existingRecord.QuoteCostSlNo });
                    }
                    else
                    {
                        // Insert new record

                        var lastId = await dbContext.Tbl60104quotationItemCosts
                           .OrderByDescending(x => x.QuoteChildId)
                           .Select(x => x.QuoteChildId)
                           .FirstOrDefaultAsync();


                        model.QuoteChildId = lastId == 0 ? 1 : lastId + 1;


                        dbContext.Tbl60104quotationItemCosts.Add(model);
                        await dbContext.SaveChangesAsync();

                        return Ok(new { success = true, message = "Saved successfully", id = model.QuoteCostSlNo });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"An error occurred while fetching the data : {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });

                }
            }

            return Unauthorized(new { success = false, message = "Invalid tenant" });
        }
        [HttpDelete]
        public IActionResult Deletes(string key)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    if (!long.TryParse(key, out long id))
                        return BadRequest("Invalid key format.");
                    var record = dbContext.Tbl60104quotationItemCosts.FirstOrDefault(x => x.QuoteCostSlNo == id);
                    if (record == null)
                        return NotFound();


                    dbContext.Tbl60104quotationItemCosts.Remove(record);
                    dbContext.SaveChanges();
                    return Ok();
                }

                return Unauthorized(new { success = false, message = "Invalid tenant" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Delete: {ex}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
       
        [HttpPost]
        public async Task<IActionResult> SaveOrUpdateStatus([FromBody] Tbl60105quotationCostMaster model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.CostItem))
            {
                return BadRequest(new { success = false, message = "Invalid data received." });
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var existing = await dbContext.Tbl60105quotationCostMasters
                        .FirstOrDefaultAsync(x => x.CostItemCode == model.CostItemCode);

                    if (existing != null)
                    {
                        // Update existing record
                        existing.CostItem = model.CostItem;
                    }
                    else
                    {
                        string newCode = "A-001"; // Default fallback

                        try
                        {
                            // Fetch codes like A-001, A-002...
                            var codeList = await dbContext.Tbl60105quotationCostMasters
                                .Where(x => x.CostItemCode.StartsWith("A-") && x.CostItemCode.Length >= 5)
                                .Select(x => x.CostItemCode)
                                .ToListAsync();

                            // Extract last 3 digits and parse as int
                            var maxNumber = codeList
                                .Select(code =>
                                {
                                    string numericPart = code.Substring(code.Length - 3); // last 3 chars
                                    return int.TryParse(numericPart, out int result) ? result : 0;
                                })
                                .DefaultIfEmpty(0)
                                .Max();

                            int nextNumber = maxNumber + 1;
                            newCode = $"A-{nextNumber.ToString("D3")}"; // Format as A-001, A-002, etc.
                        }
                        catch
                        {
                            newCode = "A-001";
                        }

                        model.CostItemCode = newCode;
                        dbContext.Tbl60105quotationCostMasters.Add(model);
                    }

                    await dbContext.SaveChangesAsync();

                    return Ok(new
                    {
                        success = true,
                        message = "Saved successfully",
                        id = model.CostItemCode
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in SaveOrUpdateStatus");
                    return StatusCode(500, new { success = false, message = ex.Message });
                }
            }

            return Unauthorized(new { success = false, message = "Invalid tenant" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCostItem(String id)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var existing = await dbContext.Tbl60105quotationCostMasters
                        .FirstOrDefaultAsync(s => s.CostItemCode == id);

                    if (existing == null)
                    {
                        return NotFound(new { success = false, message = "CostItem not found" });
                    }

                    dbContext.Tbl60105quotationCostMasters.Remove(existing);
                    await dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Deleted successfully" });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in DeleteStatus: {ex}");
                    return StatusCode(500, new { success = false, message = ex.Message });
                }
            }

            return Unauthorized(new { success = false, message = "Invalid tenant" });
        }

        [HttpPost]
        public IActionResult DeleteQuotationView(string QuoteNo)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Json(new { success = false, message = "Invalid tenant context." });

            try
            {
                var quotation = dbContext.Tbl60101quotationMasters.FirstOrDefault(q => q.QuoteNo == QuoteNo);
                if (quotation == null)
                    return Json(new { success = false, message = "Quotation not found." });

                // ✅ Check if approved
                if (quotation.IsApproved == true)
                    return Json(new { success = false, message = "Quotation is already approved. You cannot delete the approved Quotation." });

                // ✅ Check if Sales Order exists
                var linkedSalesOrder = dbContext.Qry60104quotationViewMasters
                    .FirstOrDefault(q => q.QuoteNo == QuoteNo && q.SalesOrderNo != null);
                if (linkedSalesOrder != null)
                    return Json(new
                    {
                        success = false,
                        message = "You cannot delete this Quotation. A Sales Order has been generated. Please remove the Sales Order to unlock this Quotation and try again."
                    });

                // ✅ Delete child records
                var childRows = dbContext.Tbl60102quotationChildren.Where(x => x.QuoteNo == QuoteNo);
                dbContext.Tbl60102quotationChildren.RemoveRange(childRows);

                var itemCosts = dbContext.Tbl60104quotationItemCosts.Where(x => x.QuoteNo == QuoteNo);
                dbContext.Tbl60104quotationItemCosts.RemoveRange(itemCosts);

                var terms = dbContext.Tbl60103quotationTerms.Where(x => x.QuoteNo == QuoteNo);
                dbContext.Tbl60103quotationTerms.RemoveRange(terms);

                // ✅ Clear MaterialQuoteNo from PRs
                var prWithQuote = dbContext.Tbl60601purchaseRequestMasters
                    .Where(x => x.MaterialQuoteNo == QuoteNo)
                    .ToList();
                foreach (var pr in prWithQuote)
                {
                    pr.MaterialQuoteNo = null;
                    dbContext.Tbl60601purchaseRequestMasters.Update(pr);
                }

                // ✅ Remove Quotation master
                dbContext.Tbl60101quotationMasters.Remove(quotation);

                // ✅ Delete attached files (if needed)
                // DeleteDocumentPDF(QuoteNo, "VoucherScanned\\IMSQuote");

                dbContext.SaveChanges();

                // ✅ Log Deletion
                //string userId = HttpContext.Session.GetString("UserID") ?? "Unknown";
                //string userName = HttpContext.Session.GetString("UserName") ?? "Unknown";

                //InsertUserEntryLogSheet(
                //    "IMS Quotation",
                //    $"IMS Quotation No. {QuoteNo} has been deleted by User ID: {userId} User Name: {userName}.",
                //    userName,
                //    QuoteNo
                //);

                return Json(new { success = true, message = "Quotation has been successfully removed from the database." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting quotation.");
                return Json(new { success = false, message = "An error occurred while deleting the Quotation." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UnlockQuotation([FromBody] QuotationViewModel request)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized(new { success = false, message = "Invalid tenant." });

                if (string.IsNullOrWhiteSpace(request?.QuoteNo))
                    return BadRequest(new { success = false, message = "Quote No is required." });

                var existingEntity = await dbContext.Tbl60101quotationMasters
                    .FirstOrDefaultAsync(x => x.QuoteNo == request.QuoteNo);

                if (existingEntity == null)
                    return NotFound(new { success = false, message = "Quotation not found." });

                if (existingEntity.IsApproved != true && existingEntity.IsSubmitted != true && existingEntity.IsVerified != true)
                    return Ok(new { success = false, message = "Quotation is already unlocked." });

                existingEntity.IsApproved = false;
                existingEntity.IsSubmitted = false;
                existingEntity.IsVerified = false;

                dbContext.Tbl60101quotationMasters.Update(existingEntity);
                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Quotation has been unlocked successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while unlocking Quotation.");
                return StatusCode(500, new { success = false, message = "Internal server error", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DuplicateQuotation([FromBody] string originalQuoteNo)
        {
            if (string.IsNullOrWhiteSpace(originalQuoteNo))
                return BadRequest(new { success = false, message = "Quote No is required." });

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { success = false, message = "Tenant context not found." });

            try
            {
                // Get tenant name
                var tenantName = HttpContext.Session.GetString("TenantName");
                if (string.IsNullOrWhiteSpace(tenantName))
                    return Unauthorized(new { success = false, message = "Tenant name missing from session." });

                // Get company details
                var company = dbContext.Tbl901CompanyDetails.FirstOrDefault(c => c.CompanyNameShort == tenantName);
                if (company == null)
                    return NotFound(new { success = false, message = "Company not found." });

                // Get digit config
                int digits = dbContext.Tbl901CompanyDetails02s
                    .Where(c => c.CompanyId == company.CompanyId)
                    .Select(c => c.NoOfDigitsToInventoryQuotation ?? 4)
                    .FirstOrDefault();

                // Generate new QuoteNo
                string newQuoteNo = GetNewQuoteNo(
                    company.QuotationAbbrv,
                    company.InvoiceYearDigits ?? 0,
                    DateTime.Now,
                    company.IsResetInvoiceInYear ?? false,
                    digits,
                    dbContext
                );

                string user = HttpContext.Session.GetString("UserName") ?? "System";
                DateTime quoteDate = dbContext.Tbl60101quotationMasters
        .Where(q => q.QuoteNo == originalQuoteNo)
        .Select(q => q.QuoteDate ?? DateTime.Now)
        .FirstOrDefault();

                // Call SP
                dbContext.Database.ExecuteSqlRaw(
                    "EXEC sp600_20InsertDuplicateQuotation @p0, @p1, @p2, @p3, @p4",
                    originalQuoteNo, newQuoteNo, quoteDate, user, quoteDate
                );

                dbContext.SaveChanges();

                return Ok(new { success = true, message = "Quotation duplicated successfully.", newQuoteNo });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error duplicating quotation: " + ex.Message);
                return StatusCode(500, new { success = false, message = "Internal error", detail = ex.Message });
            }
        }

        // Helper Method
        private string GetNewQuoteNo(string abbr, int yearDigits, DateTime date, bool resetByYear, int digits, ERPMasterWtDataContext db)
        {
            try
            {
                var existing = db.Tbl60101quotationMasters
                    .Where(q => q.QuoteNo != null &&
                                q.QuoteNo.Length >= digits &&
                                (!resetByYear || (q.QuoteDate.HasValue && q.QuoteDate.Value.Year == date.Year)))
                    .Select(q => q.QuoteNo)
                    .ToList();

                int max = existing
                    .Select(no => int.TryParse(no.Substring(no.Length - digits), out int num) ? num : 0)
                    .DefaultIfEmpty(0)
                    .Max() + 1;

                string year = (yearDigits > 0) ? date.Year.ToString().Substring(4 - yearDigits) : "";
                return $"{abbr}{year}-{max.ToString().PadLeft(digits, '0')}";
            }
            catch
            {
                string year = (yearDigits > 0) ? date.Year.ToString().Substring(4 - yearDigits) : "";
                return $"{abbr}{year}-{"1".PadLeft(digits, '0')}";
            }
        }
        [HttpPost]
        public IActionResult ReviseQuotation([FromBody] string originalQuoteNo)
        {
            if (string.IsNullOrWhiteSpace(originalQuoteNo))
                return BadRequest(new { success = false, message = "Quote No is required." });

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { success = false, message = "Tenant context not found." });

            try

            {
                // Step 1: Trim quote base (remove -(R1) etc.)
                string quoteBase = originalQuoteNo;
                int bracketIndex = quoteBase.IndexOf("-(R");
                if (bracketIndex > -1)
                    quoteBase = quoteBase.Substring(0, bracketIndex);

                // Step 2: Get latest revision number
                int currentRevision = dbContext.Tbl60101quotationMasters
                    .Where(q => q.QuoteNo.StartsWith(quoteBase))
                    .Max(q => q.RevisionNo ?? 0);

                int nextRevision = currentRevision + 1;
                string newQuoteNo = $"{quoteBase}-(R{nextRevision})";
                string user = HttpContext.Session.GetString("UserName") ?? "System";

                // Step 3: Call stored procedure to duplicate with revision
                dbContext.Database.ExecuteSqlRaw(
                    "EXEC sp600_05CreateNewRevisedQuotation @p0, @p1, @p2, @p3",
                    originalQuoteNo, newQuoteNo, nextRevision, user
             );

                // Step 4: Update status of old quotation (to 'Revised' = 5)
                var oldQuote = dbContext.Tbl60101quotationMasters
                    .FirstOrDefault(q => q.QuoteNo == originalQuoteNo);
                if (oldQuote != null)
                {
                    oldQuote.QuoteStatus = 5;
                    dbContext.SaveChanges();
                }

                return Ok(new { success = true, message = "Quotation revised successfully.", newQuoteNo });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error revising quotation: " + ex.Message);
                return StatusCode(500, new { success = false, message = "Revision failed", detail = ex.Message });
            }
        }

    }
}
