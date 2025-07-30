using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Humanizer;
using DevExtreme.AspNet.Data.ResponseModel;
using Microsoft.Data.SqlClient;

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
using System.Text.RegularExpressions;

namespace QD.ERP.Web.Areas.IMS.Controllers
{
    [Area("IMS")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SalesOrdersController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<SalesOrdersController> _logger;
        public SalesOrdersController(ILogger<SalesOrdersController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetSalesOrders(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var query = dbContext.Qry60204salesOrderViewMasters.AsQueryable();


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
                    query = query.Where(i => i.SalesOrderDate >= fromDate && i.SalesOrderDate <= toDate);

                    // Fetching the data
                    var data = await query.Select(i => new
                    {
                        i.SalesOrderNo,
                        i.SalesOrderDate,
                        i.ClientName,
                        i.ClientPono,
                        i.Mprno,
                        i.QuoteNo,
                        i.InvoiceStatus,
                        i.OrderStatus,
                        i.NoOfItems,
                        i.TotalBeforeDiscount,
                        i.Discount,
                        i.TotalAfterDiscount,
                        i.TotalTaxAmount,
                        i.TotalWithTax,
                    }).ToListAsync();

                    return Json(data);
                }

                return Unauthorized(new { message = "Invalid tenant." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetSalesOrdersDetails(DateTime? fromDate, DateTime? toDate)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var query = dbContext.Qry60212salesOrderItemsWithDetails.AsQueryable();
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
                query = query.Where(i => i.SalesOrderDate >= fromDate && i.SalesOrderDate <= toDate);

                // Fetching the data
                var data = await query.Select(i => new
                {
                    i.SalesOrderNo,
                    i.SalesOrderDate,
                    i.ClientName,
                    i.ClientPono,
                    i.SalesPersonName,
                    i.Gscode,
                    i.Gsdescrpition,
                    i.UnitType,
                    i.UnitDesc,
                    i.QuotedQuantity,
                    i.DeliveryTerms,
                    i.QuoteRemarks,
                    i.QuotedUnitPrice,
                    i.LineTotalBeforeDiscount,
                    i.QuotedDiscount,
                    i.LineTotalAfterDiscount,
                    i.LineTaxAmount,
                    i.LineTotalWithTax,
                    i.BalanceToDeliver,
                    i.TotalOrderedQty,
                    i.TotalIssdQty,
                    i.DeliveryPeriod,
                    i.CostPrice,
                }).ToListAsync();

                return Json(data);
            }

            return Unauthorized(new { message = "Invalid tenant." });
        }


        [HttpGet]
        public IActionResult SalesOrderNoIncrease1()
        {
            try
            {
                // Get tenant name from session
                string tenantName = HttpContext.Session.GetString("TenantName");
                if (string.IsNullOrWhiteSpace(tenantName))
                {
                    _logger.LogWarning("Tenant name not found in session when generating SalesOrderNo.");
                    return Unauthorized(new { message = "Tenant name not found in session." });
                }

                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    _logger.LogWarning("Invalid tenant context when generating SalesOrderNo.");
                    return Unauthorized(new { message = "Invalid tenant." });
                }

                // Build prefix from tenant name
                string[] words = tenantName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string prefix;
                if (words.Length == 1)
                {
                    var word = words[0];
                    prefix = (word.Length >= 2)
                        ? $"{char.ToUpper(word[0])}{char.ToUpper(word[^1])}"
                        : word.ToUpper();
                }
                else
                {
                    prefix = string.Concat(words.Select(w => char.ToUpper(w[0])));
                }

                string year = DateTime.Now.Year.ToString();

                // 🔄 Updated format: {Prefix}-SO-{Year}-
                string basePrefix = $"{prefix}-SO-{year}-";

                // Fetch existing Sales Order Nos matching this format
                var orderNos = dbContext.Tbl60201salesOrderMasters
                    .Where(x => x.SalesOrderNo.StartsWith(basePrefix))
                    .Select(x => x.SalesOrderNo)
                    .ToList();

                // Extract number part using Regex
                int maxNumber = 0;
                var regex = new Regex($@"^{Regex.Escape(basePrefix)}(\d+)$");
                foreach (var orderNo in orderNos)
                {
                    var match = regex.Match(orderNo ?? "");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int num))
                    {
                        if (num > maxNumber)
                            maxNumber = num;
                    }
                }

                // Generate next number
                int nextNumber = maxNumber + 1;
                string nextOrderNo = $"{basePrefix}{nextNumber:D5}";

                return Ok(new { salesOrderNo = nextOrderNo });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in SalesOrderNoIncrease: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
            }
        }
        [HttpGet]
        public IActionResult SalesOrderNoIncrease()
        {
            try
            {
                // Step 1: Get tenant name from session
                string tenantName = HttpContext.Session.GetString("TenantName");
                if (string.IsNullOrWhiteSpace(tenantName))
                {
                    _logger.LogWarning("Tenant name not found in session when generating SalesOrderNo.");
                    return Unauthorized(new { message = "Tenant name not found in session." });
                }

                // Step 2: Get DB context
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    _logger.LogWarning("Invalid tenant context when generating SalesOrderNo.");
                    return Unauthorized(new { message = "Invalid tenant." });
                }

                // Step 3: Get company details
                var company = dbContext.Tbl901CompanyDetails
                    .FirstOrDefault(c => c.CompanyNameShort == tenantName);

                if (company == null)
                    return NotFound("Company not found in Tbl901CompanyDetails.");

                // Step 4: Get SalesOrderAbbrv, year digit, reset flag, and number of digits
                string SalesOrderAbbrv = company.SalesOrderAbbrv ?? "";
                int yearInDigit = company.InvoiceYearDigits ?? 0;
                bool isResetByYear = company.IsResetInvoiceInYear ?? false;

                int noOfDigits = dbContext.Tbl901CompanyDetails02s
                    .Where(c => c.CompanyId == company.CompanyId)
                    .Select(c => c.NoOfDigitsToInventoryQuotation ?? 5)
                    .FirstOrDefault();

                DateTime currentDate = DateTime.Now;
                string yearPart = currentDate.Year.ToString();

                if (yearInDigit > 0)
                    yearPart = yearPart.Substring(yearPart.Length - yearInDigit, yearInDigit);
                else
                    yearPart = "";

                string basePrefix = $"{SalesOrderAbbrv}{yearPart}-";

                // Step 5: Get existing matching SalesOrderNos
                var orderNos = dbContext.Tbl60201salesOrderMasters
                    .Where(x => x.SalesOrderNo.StartsWith(basePrefix))
                    .Select(x => x.SalesOrderNo)
                    .ToList();

                // Step 6: Extract and compute next number
                int maxNumber = orderNos
                    .Select(no => int.TryParse(no?.Substring(no.Length - noOfDigits), out int num) ? num : 0)
                    .DefaultIfEmpty(0)
                    .Max();

                int nextNumber = maxNumber + 1;
                string nextOrderNo = $"{basePrefix}{nextNumber.ToString().PadLeft(noOfDigits, '0')}";

                return Ok(new { salesOrderNo = nextOrderNo });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in SalesOrderNoIncrease: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while generating Sales Order No.", error = ex.Message });
            }
        }




        private string GenerateNextOrderNo(string lastOrderNo)
        {
            if (string.IsNullOrWhiteSpace(lastOrderNo))
                return "SO-18-00001"; // Fallback default if nothing found

            var match = Regex.Match(lastOrderNo, @"^(.*-)(\d+)$");
            if (!match.Success)
                return lastOrderNo + "-00001"; // Unexpected format fallback

            string prefix = match.Groups[1].Value;
            int number = int.Parse(match.Groups[2].Value);

            return $"{prefix}{(number + 1):D5}";
        }


        [HttpGet]
        public async Task<IActionResult> GetClientNames()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var data = await dbContext.Tbl30101ClientMasters
                        .Select(i => new
                        {
                            i.ClientCode,
                            i.ClientName,
                            i.ContactMobile1,
                            i.ContactEmail,
                            i.ContactPerson


                        })
                        .ToListAsync();

                    return Json(data); // return raw data, paging/sorting done on client-side
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetProject()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var data = await dbContext.Qry70002projectsViewMasters
                        .Select(i => new
                        {
                            i.ProjectId,
                            i.ProjectDescription

                        })
                        .ToListAsync();

                    return Json(data); // return raw data, paging/sorting done on client-side
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTypeOfSalesOrder()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var data = await dbContext.Tbl30104TypeOfRequestMasters
                        .Select(i => new
                        {
                            i.TypeOfRequestId,
                            i.TypeOfRequest

                        })
                        .ToListAsync();

                    return Json(data); // return raw data, paging/sorting done on client-side
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }



        [HttpGet]
        public async Task<IActionResult> GetSignatory()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var data = await dbContext.Tbl90104DocumentSignatories
                        .Select(i => new
                        {
                            i.SignatoryId,
                            i.SignatoryName

                        })
                        .ToListAsync();

                    return Json(data); // return raw data, paging/sorting done on client-side
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCompanyBranch()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var data = await dbContext.Tbl901CompanyDetails
                        .Select(i => new
                        {
                            i.CompanyId,
                            i.CompanyName

                        })
                        .ToListAsync();

                    return Json(data); // return raw data, paging/sorting done on client-side
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetSalesPersonName()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var data = await dbContext.Tbl20101SalesPersonMasters
                        .Select(i => new
                        {
                            i.SalesPersonName,
                            i.SalesPersonCode

                        })
                        .ToListAsync();

                    return Json(data); // return raw data, paging/sorting done on client-side
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetGroupMaster()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var data = await dbContext.Tbl60008inventoryMasterGroups
                        .Select(i => new
                        {
                            i.InventoryMasterGroupId,
                            i.InventoryMasterGroup

                        })
                        .ToListAsync();

                    return Json(data); // return raw data, paging/sorting done on client-side
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCostCenterMaster()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var data = await dbContext.Tbl201CostAllocationUnits
                        .Select(i => new
                        {
                            i.CostAllocationMasterGroup,
                            i.CostAllocationUnitId


                        })
                        .ToListAsync();

                    return Json(data); // return raw data, paging/sorting done on client-side
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> SaveSalesOrder([FromBody] SalesorderViewModel model)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized(new { success = false, message = "Invalid tenant." });

                if (model == null)
                    return BadRequest(new { success = false, message = "Invalid data." });
            

                bool isUpdate = false;
                var salesOrderNo = model.SalesOrderNo?.Trim();

                var existingEntity = await dbContext.Tbl60201salesOrderMasters
                    .FirstOrDefaultAsync(x => x.SalesOrderNo == salesOrderNo);

                if (existingEntity != null)
                {
                    isUpdate = true;
                    existingEntity.SalesOrderDate = model.SalesOrderDate;
                    existingEntity.ClientPono = model.ClientPono;
                    existingEntity.ClientPodate = model.ClientPodate;
                    existingEntity.QuoteNo = model.QuoteNo;
                    existingEntity.QuoteDate = model.QuoteDate;
                    existingEntity.QuoteTransport = model.QuoteTransport;
                    existingEntity.QuoteDiscount = model.QuoteDiscount;
                    existingEntity.ClientRefNo = model.ClientRefNo;
                    existingEntity.Attention = model.Attention;
                    existingEntity.ClientCode = model.ClientCode;
                    existingEntity.Project = model.Project;
                    existingEntity.ProjectMasterCode = model.ProjectMasterCode;
                    existingEntity.SalesPersonCode = model.SalesPersonCode;
                    existingEntity.ClientContactEmail = model.ClientContactEmail;
                    existingEntity.ClientContactNo = model.ClientContactNo;
                    existingEntity.TypeOfRequest = model.TypeOfRequest;
                    existingEntity.QuoteSignatory = model.QuoteSignatory;
                    existingEntity.CompanyBranch = model.CompanyBranch;
                    existingEntity.InventoryMasterGroupId = model.InventoryMasterGroupId;
                    existingEntity.AdditionsText = model.AdditionsText;
                    existingEntity.OrderExpiryDate = model.OrderExpiryDate;
                    existingEntity.SalesOrderRemarks = model.SalesOrderRemarks;
                    existingEntity.ExpectedDeliveryDate = model.ExpectedDeliveryDate;
                    existingEntity.DeliveryPeriod = model.DeliveryPeriod;
                    existingEntity.DeliveryTerms = model.DeliveryTerms;
                    existingEntity.DiscountsText = model.DiscountsText;
                    existingEntity.CostAllocationMasterGroup = model.CostAllocationMasterGroup;
                    existingEntity.AddedBy = model.AddedBy ?? User.Identity?.Name;
                    existingEntity.AddedOn = DateTime.Now;
                    existingEntity.IsVerified = true;
                    existingEntity.CurrencyId = model.CurrencyId ?? 1;
                    existingEntity.BaseCurrencyId = model.BaseCurrencyId ?? 1;
                    existingEntity.CurrencyRate = model.CurrencyRate ?? 0;

                    dbContext.Tbl60201salesOrderMasters.Update(existingEntity);
                }
                else
                {
                    var entity = new Tbl60201salesOrderMaster
                    {
                        SalesOrderNo = salesOrderNo,
                        SalesOrderDate = model.SalesOrderDate,
                        ClientPono = model.ClientPono,
                        ClientPodate = model.ClientPodate,
                        QuoteNo = model.QuoteNo,
                        QuoteDate = model.QuoteDate,
                        ClientRefNo = model.ClientRefNo,
                        Attention = model.Attention,
                        ClientCode = model.ClientCode,
                        Project = model.Project,
                        SalesPersonCode = model.SalesPersonCode,
                        QuoteTransport = model.QuoteTransport,
                        QuoteDiscount = model.QuoteDiscount,
                        ClientContactEmail = model.ClientContactEmail,
                        ClientContactNo = model.ClientContactNo,
                        TypeOfRequest = model.TypeOfRequest,
                        QuoteSignatory = model.QuoteSignatory,
                        CompanyBranch = model.CompanyBranch,
                        ProjectMasterCode = model.ProjectMasterCode,
                        InventoryMasterGroupId = model.InventoryMasterGroupId,
                        AdditionsText = model.AdditionsText,
                        OrderExpiryDate = model.OrderExpiryDate,
                        SalesOrderRemarks = model.SalesOrderRemarks,
                        ExpectedDeliveryDate = model.ExpectedDeliveryDate,
                        DeliveryPeriod = model.DeliveryPeriod,
                        DeliveryTerms = model.DeliveryTerms,
                        DiscountsText = model.DiscountsText,
                        CostAllocationMasterGroup = model.CostAllocationMasterGroup,
                        AddedBy = model.AddedBy ?? User.Identity?.Name,
                        AddedOn = DateTime.Now,
                         IsApproved = false,
                        IsVerified = false,
                        IsSubmitted = false,
                        CurrencyId = model.CurrencyId??1,
                        BaseCurrencyId = model.BaseCurrencyId??1,
                        CurrencyRate = model.CurrencyRate ?? 0
                    };

                    await dbContext.Tbl60201salesOrderMasters.AddAsync(entity);
                }

                await dbContext.SaveChangesAsync();

                if (!isUpdate)
                {
                    var parameters = new[]
                    {
                new SqlParameter("@SalesOrderNo", salesOrderNo ?? (object)DBNull.Value),
                new SqlParameter("@ClientName", model.ClientCode ?? (object)DBNull.Value),
                new SqlParameter("@AddedBy", model.AddedBy ?? User.Identity?.Name ?? (object)DBNull.Value),
                new SqlParameter("@IsCreateCostCenterFromSalesOrder", true),
                new SqlParameter("@DefaultCostCenterMasterFromSalesOrder", model.CostAllocationMasterGroup ?? (object)DBNull.Value),
                new SqlParameter("@SalesPersonName", model.SalesPersonCode ?? (object)DBNull.Value)
            };

                    await dbContext.Database.ExecuteSqlRawAsync(
                        "EXEC [dbo].[sp600_04InsertToCostCenterFromSalesOrder] " +
                        "@SalesOrderNo, @ClientName, @AddedBy, @IsCreateCostCenterFromSalesOrder, " +
                        "@DefaultCostCenterMasterFromSalesOrder, @SalesPersonName", parameters);
                }

				var existingChildren = await dbContext.Tbl60202salesOrderChildren
			   .Where(x => x.SalesOrderNo == model.SalesOrderNo)
			   .ToListAsync();

				var incomingChildren = model.SalesOrderChildren ?? new List<Tbl60202salesOrderChild>();
				var incomingIds = incomingChildren.Where(c => c.SalesOrderChildId > 0).Select(c => c.SalesOrderChildId).ToList();

				// ✅ Update existing children
				foreach (var child in incomingChildren.Where(c => c.SalesOrderChildId > 0))
				{
					var entity = existingChildren.FirstOrDefault(x => x.SalesOrderChildId == child.SalesOrderChildId);
					if (entity != null)
					{
						_logger.LogInformation($"Updating child ID: {child.SalesOrderChildId}, QuotedDiscount: {child.QuotedDiscount}");
						entity.QuotedUnitPrice = child.QuotedUnitPrice;
						entity.QuotedDiscount = child.QuotedDiscount;
						entity.CostPrice = child.CostPrice;
						entity.UnitRateMethod = child.UnitRateMethod;
						entity.Gscode = child.Gscode;
						entity.QuotedTaxSlab = child.QuotedTaxSlab;

						dbContext.Entry(entity).State = EntityState.Modified; // Force update tracking
					}
				}

				// ✅ Insert new children
				var newChildren = incomingChildren.Where(c => c.SalesOrderChildId == 0).ToList();
				foreach (var child in newChildren)
				{
					child.SalesOrderNo = model.SalesOrderNo;
					await dbContext.Tbl60202salesOrderChildren.AddAsync(child);
				}

				// ✅ Delete removed children
				var toDelete = existingChildren.Where(x => !incomingIds.Contains(x.SalesOrderChildId)).ToList();
				if (toDelete.Any())
					dbContext.Tbl60202salesOrderChildren.RemoveRange(toDelete);

				var result = await dbContext.SaveChangesAsync();
				_logger.LogInformation($"Child records updated/added/deleted. Save result = {result}");


				

				return Ok(new
                {
                    success = true,
                    message = isUpdate ? "Sales order updated successfully." : "Sales order created successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SaveSalesOrder");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while saving the sales order.",
                    details = ex.Message
                });
            }
        }
      [HttpPost]
public async Task<IActionResult> GenerateJobOrders1([FromBody] SalesorderViewModel model)
{
    try
    {
        if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            return Unauthorized(new { success = false, message = "Invalid tenant." });

        if (string.IsNullOrWhiteSpace(model.SalesOrderNo))
            return BadRequest(new { success = false, message = "Missing Sales Order No." });

        if (model.SalesOrderChildren == null || !model.SalesOrderChildren.Any())
            return BadRequest(new { success = false, message = "No line items selected." });

        foreach (var child in model.SalesOrderChildren)
        {
            if (child.SalesOrderChildId == 0)
                return BadRequest(new { success = false, message = "Invalid SalesOrderChildId in line items." });

            var parameters = new[]
            {
                new SqlParameter("@JobOrderNo", model.SalesOrderNo ?? (object)DBNull.Value),
                new SqlParameter("@AddedBy", model.AddedBy ?? User.Identity?.Name ?? (object)DBNull.Value),
                new SqlParameter("@SalesOrderChildID", child.SalesOrderChildId),
                new SqlParameter("@ValveType", model.ValveType ?? (object)DBNull.Value)
            };

            await dbContext.Database.ExecuteSqlRawAsync(
                "EXEC [dbo].[sp608_01InsertToJobOrderFromSalesOrderChild] " +
                "@JobOrderNo, @AddedBy, @SalesOrderChildID, @ValveType", parameters);
        }

        return Ok(new { success = true, message = "Job orders generated successfully." });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error generating job orders");
        return StatusCode(500, new { success = false, message = ex.Message });
    }
}

		[HttpPost]
		public async Task<IActionResult> GenerateJobOrders([FromBody] SalesorderViewModel model)
		{
			try
			{
				if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
					return Unauthorized(new { success = false, message = "Invalid tenant." });

				if (model.SalesOrderChildren == null || !model.SalesOrderChildren.Any())
					return BadRequest(new { success = false, message = "No line items selected." });

				// 🔹 Step 1: Get tenant name
				string tenantName = HttpContext.Session.GetString("TenantName");
				if (string.IsNullOrWhiteSpace(tenantName))
					return Unauthorized(new { success = false, message = "Tenant name not found in session." });

				// 🔹 Step 2: Get company details
				var company = dbContext.Tbl901CompanyDetails.FirstOrDefault(c => c.CompanyNameShort == tenantName);
				if (company == null)
					return NotFound(new { success = false, message = "Company not found in Tbl901CompanyDetails." });

				// 🔹 Step 3: Get digit settings
				int noOfDigits = dbContext.Tbl901CompanyDetails02s
										  .Where(c => c.CompanyId == company.CompanyId)
										  .Select(c => c.NoOfDigitsToInventoryQuotation ?? 4)
										  .FirstOrDefault();

				string prefix = company.JobOrderAbbrv ?? "XXX";
				int yearDigits = company.InvoiceYearDigits ?? 4;
				DateTime invoiceDate = DateTime.Now;

				// 🔹 Step 4: Generate JobOrderNo and assign to model
				string newJobOrderNo = GetNewJobOrderNo(prefix, yearDigits, invoiceDate, noOfDigits, dbContext);
				model.SalesOrderNo = newJobOrderNo;

				// 🔹 Step 5: Loop and execute SP
				foreach (var child in model.SalesOrderChildren)
				{
					if (child.SalesOrderChildId == 0)
						return BadRequest(new { success = false, message = "Invalid SalesOrderChildId in line items." });

					var parameters = new[]
					{
				new SqlParameter("@JobOrderNo", newJobOrderNo),
				new SqlParameter("@AddedBy", model.AddedBy ?? User.Identity?.Name ?? (object)DBNull.Value),
				new SqlParameter("@SalesOrderChildID", child.SalesOrderChildId),
				new SqlParameter("@ValveType", model.ValveType ?? (object)DBNull.Value)
			};

					await dbContext.Database.ExecuteSqlRawAsync(
						"EXEC [dbo].[sp608_01InsertToJobOrderFromSalesOrderChild] " +
						"@JobOrderNo, @AddedBy, @SalesOrderChildID, @ValveType", parameters);
				}

				return Ok(new { success = true, message = "Job orders generated successfully.", jobOrderNo = newJobOrderNo });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error generating job orders");
				return StatusCode(500, new { success = false, message = ex.Message });
			}
		}


		private string GetNewJobOrderNo(string prefix, int yearDigits, DateTime date, int padLength, ERPMasterWtDataContext dbContext)
		{
			string yearPart = yearDigits == 4 ? date.Year.ToString("0000") : date.Year.ToString().Substring(2);
			string basePrefix = $"{prefix}{yearPart}-";

			var lastJobOrderNo = dbContext.Tbl60801jobOrderMasters
				.Where(x => x.JobOrderNo.StartsWith(basePrefix))
				.OrderByDescending(x => x.JobOrderNo)
				.Select(x => x.JobOrderNo)
				.FirstOrDefault();

			int nextNumber = 1;
			if (!string.IsNullOrWhiteSpace(lastJobOrderNo))
			{
				var numberPart = lastJobOrderNo.Substring(basePrefix.Length);
				if (int.TryParse(numberPart, out int lastNumber))
					nextNumber = lastNumber + 1;
			}

			return basePrefix + nextNumber.ToString().PadLeft(padLength, '0');
		}



        [HttpGet]
        public async Task<IActionResult> GetSalesOrderByNo(string salesOrderNo)
        {
            if (string.IsNullOrWhiteSpace(salesOrderNo))
                return BadRequest();

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized();

            var order = await dbContext.Tbl60201salesOrderMasters
                .FirstOrDefaultAsync(x => x.SalesOrderNo == salesOrderNo);

            if (order == null)
                return NotFound();

            var children = await (
                from c in dbContext.Qry60202salesOrderChildren
                join g in dbContext.Tbl20164GoodsAndServicesMasters
                    on c.Gscode equals g.Gscode into gj
                from g in gj.DefaultIfEmpty()
                where c.SalesOrderNo == salesOrderNo
                select new
                {
                    c.SalesOrderChildId,
                    c.SalesOrderNo,
                    c.QuoteNo,
                    c.Gscode,
                    Gsdescrpition = g.Gsdescrpition,
                    c.UnitRateMethod,
                    c.QuotedQuantity,
                    c.CostPrice,
                    c.QuotedUnitPrice,
                    c.QuotedDiscount,
                    c.QuotedTaxSlab,
                    c.LineTotalAfterDiscount,
                    c.LineTotalWithTax,
                    c.LineTaxAmount
                }
            ).ToListAsync();

            // Return both master and children
            return Ok(new
            {
                order.SalesOrderNo,
                order.SalesOrderDate,
                order.ClientPono,
                order.ClientPodate,
                order.QuoteNo,
                order.QuoteDate,
                order.ClientRefNo,
                order.ClientCode,
                order.Project,
                order.ProjectMasterCode,
                order.SalesPersonCode,
                order.ClientContactEmail,
                order.ClientContactNo,
                order.TypeOfRequest,
                order.QuoteSignatory,
                order.CompanyBranch,
                order.InventoryMasterGroupId,
                order.AdditionsText,
                order.OrderExpiryDate,
                order.SalesOrderRemarks,
                order.ExpectedDeliveryDate,
                order.DeliveryPeriod,
                order.DeliveryTerms,
                order.DiscountsText,
                order.Attention,
                order.QuoteTransport,
                order.QuoteDiscount,
                order.IsSubmitted,
                order.IsVerified,
                order.IsApproved,
                order.CostAllocationMasterGroup,
                SalesOrderChildren = children
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetSalesOrderNoByChildId(int salesOrderChildId)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                return Unauthorized(new { success = false });

            var salesOrderNo = await dbContext.Qry60202salesOrderChildren
                .Where(c => c.SalesOrderChildId == salesOrderChildId)
                .Select(c => c.SalesOrderNo)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(salesOrderNo))
                return NotFound(new { success = false, message = "Not found." });

            return Ok(new { success = true, salesOrderNo });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteSalesOrder(string salesOrderNo)
        {
            if (string.IsNullOrWhiteSpace(salesOrderNo))
                return BadRequest(new { message = "SalesOrderNo is required." });

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { message = "Invalid tenant." });

            // Fetch the master record
            var entity = await dbContext.Tbl60201salesOrderMasters
                .FirstOrDefaultAsync(x => x.SalesOrderNo == salesOrderNo);

            if (entity == null)
                return NotFound(new { message = "Sales order not found." });

            // Fetch and delete all child records
            var children = await dbContext.Tbl60202salesOrderChildren
                .Where(x => x.SalesOrderNo == salesOrderNo)
                .ToListAsync();

            if (children.Any())
                dbContext.Tbl60202salesOrderChildren.RemoveRange(children);

            // Delete master record
            dbContext.Tbl60201salesOrderMasters.Remove(entity);

            await dbContext.SaveChangesAsync();

            return Ok(new { success = true, message = "Sales order and its child items deleted successfully." });
        }

[HttpGet]
public async Task<IActionResult> CanDeleteSalesOrder(string salesOrderNo)
{
    if (string.IsNullOrWhiteSpace(salesOrderNo))
        return BadRequest(new { success = false, message = "SalesOrderNo is required." });

    if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
        return Unauthorized(new { success = false, message = "Invalid tenant." });

    var entity = await dbContext.Tbl60201salesOrderMasters
        .FirstOrDefaultAsync(x => x.SalesOrderNo == salesOrderNo);

    if (entity == null)
        return NotFound(new { success = false, message = "Sales order not found." });

    bool isVerified = entity.IsVerified ?? false;
    bool isApproved = entity.IsApproved ?? false;

    // Add this log line
    _logger.LogInformation($"CanDeleteSalesOrder: SalesOrderNo={salesOrderNo}, IsVerified={entity.IsVerified}, IsApproved={entity.IsApproved}");

    if (isVerified || isApproved)
    {
        return Ok(new
        {
            success = false,
            message = "The selected sales order was verified/approved. You cannot delete it."
        });
    }

    return Ok(new { success = true });
}   //[HttpGet]
        //public IActionResult GetGoodsAndServices()
        //{
        //    try
        //    {
        //        if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
        //        {
        //            var items = dbContext.Tbl20164GoodsAndServicesMasters
        //                .Where(x => x.IsDiscontinued == false) // Optional filter
        //                .Select(x => new
        //                {
        //                    x.Gscode,
        //                    x.Gsdescrpition,
        //                    x.GsdescriptionAr,
        //                    x.ItemPartNo,
        //                    x.GspackingUnit
        //                })
        //                .ToList();

        //            return Json(items);
        //        }
        //        else
        //        {
        //            return BadRequest("Unable to resolve tenant context.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Optional: Log the exception (example: using ILogger)
        //        Console.WriteLine("Error loading Goods and Services: " + ex.Message);
        //        return StatusCode(500, "An error occurred while retrieving the data.");
        //    }
        //}
        [HttpGet]
        public async Task<IActionResult> GetGoodsAndServices()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var data = await dbContext.Tbl20164GoodsAndServicesMasters
                        .Select(i => new
                        {
                            i.Gscode,
                            i.Gsdescrpition,
                            i.GsdescriptionAr,
                            i.ItemPartNo,
                            i.GspackingUnit,
                            i.IsDiscontinued,
                            i.CostPrice,
                            i.GsuoM

                        })
                        .ToListAsync();

                    return Json(data); // return raw data, paging/sorting done on client-side
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUnitCodes()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var data = await dbContext.Tbl40111PropertyUnitCodes
                        .Select(i => new
                        {
                            i.UnitDescAr,
                            i.UnitDesc,
                            i.UnitType,
                            i.UnitCode,

                        })
                        .ToListAsync();

                    return Json(data); // return raw data, paging/sorting done on client-side
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetStoreToIssueItem(string salesOrderNo)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    if (string.IsNullOrWhiteSpace(salesOrderNo))
                        return BadRequest(new { message = "SalesOrderNo is required.", success = false });

                    // Get StoreIds related to the selected SalesOrderNo from availability table
                    var storeIds = await dbContext.Qry65320storeStockAvaliabilityForSalesOrders
                        .Where(x => x.SalesOrderNo == salesOrderNo.Trim())
                        .Select(x => x.StoreCode.Trim().ToUpper())
                        .Distinct()
                        .ToListAsync();

                    // Now fetch matching store details from Tbl60001storeMasters
                    var filteredStores = await dbContext.Tbl60001storeMasters
                        .Where(s => storeIds.Contains(s.StoreId.Trim().ToUpper()))
                        .Select(i => new
                        {
                            i.StoreId,
                            i.StoreName,
                            i.LedgerNo,
                            i.CostAllocationUnitId,
                        })
                        .ToListAsync();

                    return Json(filteredStores); // filtered list for dropdown
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetStoreToIssueItem: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while loading store data.", details = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetTaxSlabs()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var vatTaxSlabs = dbContext.Tbl20163VatTaxSlabs
         .Select(x => new
         {
             x.TaxSlabCode,
             x.TaxSlab,
             x.TaxRate,
             x.TaxCodeToDisplay,
             x.TaxRateInWord,
            x.TaxRateIn100
         })
         .ToList();

                    return Ok(vatTaxSlabs); 
                }
            }
            catch (Exception ex)
            {
                // Ideally log the exception, don't just throw
                return StatusCode(500, new { message = ex.Message, success = false });
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        //[HttpGet]
        //public async Task<IActionResult> GetStoreStockAvailabilityGrid()
        //{
        //	try
        //	{
        //		if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
        //		{
        //			var data = await dbContext.Qry65320storeStockAvaliabilityForSalesOrders
        //				.Select(i => new
        //				{
        //					i.StoreId,
        //					i.StoreName,
        //					i.LedgerNo,
        //					i.CostAllocationUnitId,

        //				})
        //				.ToListAsync();

        //			return Json(data); // return raw data, paging/sorting done on client-side
        //		}

        //		return Unauthorized(new { message = "Invalid tenant.", success = false });
        //	}
        //	catch (Exception ex)
        //	{
        //		_logger.LogError($"Error in GetProject: {ex.Message}");
        //		return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
        //	}
        //}

        [HttpPost]
        public async Task<IActionResult> SubmitSalesOrder1([FromBody] string salesOrderNo)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized(new { success = false, message = "Invalid tenant." });

                if (string.IsNullOrWhiteSpace(salesOrderNo))
                    return BadRequest(new { success = false, message = "Invalid Sales Order No." });

                var existingEntity = await dbContext.Tbl60201salesOrderMasters
                    .FirstOrDefaultAsync(x => x.SalesOrderNo == salesOrderNo);

                if (existingEntity == null)
                    return NotFound(new { success = false, message = "Sales Order not found. Please save it first." });

                existingEntity.IsSubmitted = true;
                dbContext.Tbl60201salesOrderMasters.Update(existingEntity);
                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Sales Order submitted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while submitting Sales Order.");
                return StatusCode(500, new { success = false, message = "Internal server error", details = ex.Message });
            }
        }

           [HttpPost]

   public async Task<IActionResult> SubmitSalesOrder([FromBody] string salesOrderNo)

   {

       try

       {

           if (string.IsNullOrWhiteSpace(salesOrderNo))

               return BadRequest(new { success = false, message = "Sales Order number is required." });
 
           if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))

               return Unauthorized(new { success = false, message = "Invalid tenant." });
 
           var salesOrder = await dbContext.Tbl60201salesOrderMasters

               .FirstOrDefaultAsync(x => x.SalesOrderNo == salesOrderNo);
 
           if (salesOrder == null)

               return Ok(new { success = false, message = "Please save the sales order before submitting." });
 
           if (salesOrder.IsSubmitted == true)

               return Ok(new { success = false, message = "This sales order is already submitted." });
 
           salesOrder.IsSubmitted = true;

           salesOrder.SubmittedBy = HttpContext.Session.GetString("UserName") ?? "System";

           salesOrder.SubmittedOn = DateTime.Now;
 
           await dbContext.SaveChangesAsync();
 
           return Ok(new { success = true, message = "Sales order submitted successfully." });

       }

       catch (Exception ex)

       {

           _logger.LogError(ex, "Error while submitting sales order.");

           return StatusCode(500, new { success = false, message = "Server error: " + ex.Message });

       }

   }

 
        [HttpPost]
        public async Task<IActionResult> VerifySalesOrder1([FromBody] string salesOrderNo)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized(new { success = false, message = "Invalid tenant." });

                var order = await dbContext.Tbl60201salesOrderMasters
                    .FirstOrDefaultAsync(x => x.SalesOrderNo == salesOrderNo);

                if (order == null)
                    return NotFound(new { success = false, message = "Sales order not found." });

                if (order.IsSubmitted != true)
                    return BadRequest(new { success = false, message = "Please submit before verifying." });

                order.IsVerified = true;

                // ✅ Add this line to mark entity as modified
                dbContext.Tbl60201salesOrderMasters.Update(order);

                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Sales order verified successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "VerifySalesOrder error");
                return StatusCode(500, new { success = false, message = "Internal error." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> VerifySalesOrder([FromBody] string salesOrderNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(salesOrderNo))
                    return BadRequest(new { success = false, message = "Sales Order number is required." });

                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                    return Unauthorized(new { success = false, message = "Invalid tenant." });

                var order = await dbContext.Tbl60201salesOrderMasters
                    .FirstOrDefaultAsync(x => x.SalesOrderNo == salesOrderNo);

                if (order == null)
                    return Ok(new { success = false, message = "Please save and submit the sales order before verifying." });

                if (order.IsSubmitted != true)
                    return Ok(new { success = false, message = "Please submit before verifying." });

                if (order.IsVerified == true)
                    return Ok(new { success = false, message = "This sales order is already verified." });

                order.IsVerified = true;
                order.VerifiedBy = HttpContext.Session.GetString("UserName") ?? "System";
                order.VerifiedOn = DateTime.Now;

                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Sales order verified successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while verifying sales order.");
                return StatusCode(500, new { success = false, message = "Server error: " + ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> ApproveSalesOrder1([FromBody] string salesOrderNo)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized(new { success = false, message = "Invalid tenant." });

                if (string.IsNullOrWhiteSpace(salesOrderNo))
                    return BadRequest(new { success = false, message = "Sales Order No is required." });

                var order = await dbContext.Tbl60201salesOrderMasters
                    .FirstOrDefaultAsync(x => x.SalesOrderNo == salesOrderNo);

                if (order == null)
                    return NotFound(new { success = false, message = "Sales Order not found." });

                if (order.IsVerified != true)
                    return BadRequest(new { success = false, message = "Sales Order must be verified before approval." });

                order.IsApproved = true;
                dbContext.Tbl60201salesOrderMasters.Update(order);
                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Sales Order approved successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving Sales Order");
                return StatusCode(500, new { success = false, message = "Internal server error." });
            }
        }
        [HttpPost]
        public async Task<IActionResult> ApproveSalesOrder([FromBody] string salesOrderNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(salesOrderNo))
                    return BadRequest(new { success = false, message = "Sales Order No is required." });

                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                    return Unauthorized(new { success = false, message = "Invalid tenant." });

                var order = await dbContext.Tbl60201salesOrderMasters
                    .FirstOrDefaultAsync(x => x.SalesOrderNo == salesOrderNo);

                if (order == null)
                    return Ok(new { success = false, message = "Please save and submit the sales order before approving." });

                if (order.IsVerified != true)
                    return Ok(new { success = false, message = "Please verify before approving." });

                if (order.IsApproved == true)
                    return Ok(new { success = false, message = "This sales order is already approved." });

                order.IsApproved = true;
                order.ApprovedBy = HttpContext.Session.GetString("UserName") ?? "System";
                order.ApprovedOn = DateTime.Now;

                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Sales order approved successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving Sales Order");
                return StatusCode(500, new { success = false, message = "Server error: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSalesOrderStatus(string salesOrderNo)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized(new { success = false, message = "Invalid tenant." });

                var entity = await dbContext.Tbl60201salesOrderMasters
                    .Where(x => x.SalesOrderNo == salesOrderNo)
                    .Select(x => new
                    {
                        x.IsSubmitted,
                        x.IsVerified
                    })
                    .FirstOrDefaultAsync();

                if (entity == null)
                    return NotFound(new { success = false, message = "Sales order not found." });

                return Ok(new { success = true, data = entity });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sales order status.");
                return StatusCode(500, new { success = false, message = "Internal server error." });
            }
        }

	[HttpPost]
public async Task<IActionResult> UnlockSalesOrder([FromBody] SalesorderViewModel request)
{
    try
    {
        if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            return Unauthorized(new { success = false, message = "Invalid tenant." });

        if (string.IsNullOrWhiteSpace(request?.SalesOrderNo))
            return BadRequest(new { success = false, message = "Sales Order No is required." });

        var existingEntity = await dbContext.Tbl60201salesOrderMasters
            .FirstOrDefaultAsync(x => x.SalesOrderNo == request.SalesOrderNo);

        if (existingEntity == null)
            return NotFound(new { success = false, message = "Sales Order not found." });

        bool wasChanged = false;

        if (existingEntity.IsApproved == true)
        {
            existingEntity.IsApproved = false;
            wasChanged = true;
        }
        if (existingEntity.IsVerified == true)
        {
            existingEntity.IsVerified = false;
            wasChanged = true;
        }
        if (existingEntity.IsSubmitted == true)
        {
            existingEntity.IsSubmitted = false;
            wasChanged = true;
        }

        if (wasChanged)
        {
            dbContext.Tbl60201salesOrderMasters.Update(existingEntity);
            await dbContext.SaveChangesAsync();
            return Ok(new { success = true, message = "Sales Order has been unlocked successfully." });
        }
        else
        {
            return Ok(new { success = false, message = "Sales Order is already unlocked." });
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error while unlocking Sales Order.");
        return StatusCode(500, new { success = false, message = "Internal server error", details = ex.Message });
    }
}


		[HttpGet]
		public async Task<IActionResult> CheckIfApproved(string salesOrderNo)
		{
			if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				return Unauthorized(new { success = false, message = "Invalid tenant." });

			if (string.IsNullOrWhiteSpace(salesOrderNo))
				return BadRequest(new { success = false, message = "Sales Order No is required." });

			var existingEntity = await dbContext.Tbl60201salesOrderMasters
				.Where(x => x.SalesOrderNo == salesOrderNo)
				.Select(x => new { x.IsApproved })
				.FirstOrDefaultAsync();

			if (existingEntity == null)
				return NotFound(new { success = false, message = "Sales Order not found." });

			return Ok(new { success = true, isApproved = existingEntity.IsApproved == true });
		}

        [HttpGet]
        public IActionResult CheckDeliveryExists(string salesOrderNo)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { message = "Invalid tenant." });

            // ✅ Check the order status from the view
            var order = dbContext.Qry60204salesOrderViewMasters
                .FirstOrDefault(o => o.SalesOrderNo == salesOrderNo);

            if (order == null)
                return NotFound(new { message = "Sales order not found." });

            if (order.OrderStatus != null && order.OrderStatus.ToLower().Contains("fully delivered"))
            {
                return Ok(new { exists = true, message = "Sales order is fully delivered. You cannot create anymore." });
            }

            // Default: allow creation
            return Ok(new { exists = false });
        }


[HttpGet]
public async Task<IActionResult> GetInvoiceStatus(string salesOrderNo)
{
    if (string.IsNullOrWhiteSpace(salesOrderNo))
        return BadRequest(new { success = false, message = "SalesOrderNo is required." });

    if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
        return Unauthorized(new { success = false, message = "Invalid tenant." });

    var status = await dbContext.Qry60204salesOrderViewMasters
        .Where(x => x.SalesOrderNo == salesOrderNo)
        .Select(x => x.OrderStatus)
        .FirstOrDefaultAsync();

    if (status == null)
        return NotFound(new { success = false, message = "Sales order not found." });

    return Ok(new { success = true, OrderStatus = status });
}
        [HttpGet]
        public async Task<IActionResult> GetStoreStockGrid(string salesOrderNo, string storeId = null)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    return Unauthorized(new { message = "Invalid tenant.", success = false });
                }

                if (string.IsNullOrWhiteSpace(salesOrderNo))
                {
                    return BadRequest(new { message = "SalesOrderNo is required.", success = false });
                }

                // Step 1: Base query
                var query = dbContext.Qry65320storeStockAvaliabilityForSalesOrders
                    .Where(i => i.SalesOrderNo.Trim() == salesOrderNo.Trim());

                // Step 2: Add store filter only if storeId is provided
                if (!string.IsNullOrWhiteSpace(storeId))
                {
                    query = query.Where(i => i.StoreCode != null && i.StoreCode.Trim().ToUpper() == storeId.Trim().ToUpper());
                }

                // Step 3: Select only needed fields (with null checks)
                var data = await query.Select(i => new
                {
                    i.Gscode,
                    Gsdescrpition = i.Gsdescrpition ?? "",
                    i.IsServicesGroup,
                    i.SalesOrderChildId,
                    UnitDesc = i.UnitDesc ?? "",
                    i.TotalOrderedQty,
                    i.TotalIssdQty,
                    i.BalanceToDeliver,
                    i.CurrentyQty,
                    AvailabilityStatus = i.AvailabilityStatus ?? "",
                    i.DeliveringQuantity,
                    StoreCode = i.StoreCode ?? ""
                    // If your view has StoreName, add it like:
                    // StoreName = i.StoreName ?? ""
                }).ToListAsync();

                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in GetStoreStockGrid: {ex.Message}");

                // Send full error to client (for debugging only — remove in production)
                return StatusCode(500, new
                {
                    message = "An error occurred while loading data.",
                    details = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }




        [HttpGet]
        public IActionResult GetStoreAvailabilitySetting()
        {
            try
            {
                var tenantName = HttpContext.Session.GetString("TenantName");
                if (string.IsNullOrWhiteSpace(tenantName))
                    return Unauthorized(new { success = false, message = "Tenant name not found." });

                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized(new { success = false, message = "Invalid tenant." });

                var company = dbContext.Tbl901CompanyDetails.FirstOrDefault(c => c.CompanyNameShort == tenantName);
                if (company == null)
                    return NotFound(new { success = false, message = "Company not found." });

                var companyDetails02 = dbContext.Tbl901CompanyDetails02s
                    .FirstOrDefault(x => x.CompanyId == company.CompanyId);

                bool isUseStoreAvailability = companyDetails02?.IsUseStoreAvailabilityInDeliveryNote ?? false;

                return Ok(new { success = true, isUseStoreAvailability });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetStoreAvailabilitySetting: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Internal server error." });
            }
        }

    }



}




