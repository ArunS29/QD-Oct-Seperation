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

namespace QD.ERP.Web.Areas.ERM.Controllers
{
    [Area("ERM")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ERSalesOrdersController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<ERSalesOrdersController> _logger;
        public ERSalesOrdersController(ILogger<ERSalesOrdersController> logger, TenantDbContextHelper tenantDbContextHelper)
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
        public IActionResult SalesOrderNoIncrease()
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
                        IsSubmitted = false
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
public async Task<IActionResult> GenerateJobOrders([FromBody] SalesorderViewModel model)
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

            // Load children with description from Goods and Services
            var children = await (
                from c in dbContext.Tbl60202salesOrderChildren
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
                    Gsdescrpition = g.Gsdescrpition, // <-- include description
                    c.UnitRateMethod,
                    c.QuotedQuantity,
                    c.CostPrice,
                    c.QuotedUnitPrice,
                    c.QuotedDiscount,
					c.QuotedTaxSlab
					// Add any other fields you need for the grid
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


        //[HttpGet]
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
		public async Task<IActionResult> GetStoreToIssueItem()
		{
			try
			{
				if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				{
					var data = await dbContext.Tbl60001storeMasters
						.Select(i => new
						{
							i.StoreId,
							i.StoreName,
							i.LedgerNo,
							i.CostAllocationUnitId,

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
        public async Task<IActionResult> SubmitSalesOrder([FromBody] string salesOrderNo)
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
        public async Task<IActionResult> VerifySalesOrder([FromBody] string salesOrderNo)
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
        public async Task<IActionResult> ApproveSalesOrder([FromBody] string salesOrderNo)
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

				if (existingEntity.IsApproved != true)
					return Ok(new { success = false, message = "Sales Order is already unlocked." });

				existingEntity.IsApproved = false;
				dbContext.Tbl60201salesOrderMasters.Update(existingEntity);
				await dbContext.SaveChangesAsync();

				return Ok(new { success = true, message = "Sales Order has been unlocked successfully." });
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

			// Use the correct DbSet name here
			bool exists = dbContext.Tbl60301deliveryNoteMasters.Any(d => d.SalesOrderNo == salesOrderNo);
			return Ok(new { exists });
		}



	}



}




