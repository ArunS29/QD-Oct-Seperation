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
    }
}
