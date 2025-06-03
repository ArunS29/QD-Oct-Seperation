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
        public IActionResult SalesOrderNoIncrease()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var lastOrder = dbContext.Qry60204salesOrderViewMasters
                        .AsEnumerable() // Required for Regex and parsing
                        .Select(x => new
                        {
                            Original = x.SalesOrderNo,
                            Match = Regex.Match(x.SalesOrderNo ?? "", @"^(.*-)(\d+)$")
                        })
                        .Where(x => x.Match.Success)
                        .Select(x => new
                        {
                            Original = x.Original,
                            Prefix = x.Match.Groups[1].Value,
                            Number = int.Parse(x.Match.Groups[2].Value)
                        })
                        .OrderByDescending(x => x.Number)
                        .FirstOrDefault();

                    string lastOrderNo = lastOrder?.Original;
                    string nextOrderNo = GenerateNextOrderNo(lastOrderNo);

                    return Ok(new { salesOrderNo = nextOrderNo });
                }

                return Unauthorized(new { message = "Invalid tenant." });
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
                    var data = await dbContext.Tbl20115CompanyBranches
                        .Select(i => new
                        {
                            i.BranchCode,
                            i.BranchName

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
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { message = "Invalid tenant." });

            if (model == null)
                return BadRequest(new { message = "Invalid data." });

            bool isUpdate = false;

            var salesOrderNo = model.SalesOrderNo?.Trim();

            // Check if Sales Order already exists
            var existingEntity = dbContext.Tbl60201salesOrderMasters
                .FirstOrDefault(x => x.SalesOrderNo == salesOrderNo);

            if (existingEntity != null)
            {
                // Update
                isUpdate = true;
                existingEntity.SalesOrderDate = model.SalesOrderDate;
                existingEntity.ClientPono = model.ClientPono;
                existingEntity.ClientPodate = model.ClientPodate;
                existingEntity.QuoteNo = model.QuoteNo;
                existingEntity.QuoteDate = model.QuoteDate;
                existingEntity.ClientRefNo = model.ClientRefNo;
                existingEntity.ClientCode = model.ClientCode;
                existingEntity.Project = model.Project;
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
                existingEntity.AddedBy = User.Identity?.Name;
                existingEntity.AddedOn = DateTime.Now;

                dbContext.Tbl60201salesOrderMasters.Update(existingEntity);
            }
            else
            {
                // Insert new
                var entity = new Tbl60201salesOrderMaster
                {
                    SalesOrderNo = salesOrderNo,
                    SalesOrderDate = model.SalesOrderDate,
                    ClientPono = model.ClientPono,
                    ClientPodate = model.ClientPodate,
                    QuoteNo = model.QuoteNo,
                    QuoteDate = model.QuoteDate,
                    ClientRefNo = model.ClientRefNo,
                    ClientCode = model.ClientCode,
                    Project = model.Project,
                    SalesPersonCode = model.SalesPersonCode,
                    ClientContactEmail = model.ClientContactEmail,
                    ClientContactNo = model.ClientContactNo,
                    TypeOfRequest = model.TypeOfRequest,
                    QuoteSignatory = model.QuoteSignatory,
                    CompanyBranch = model.CompanyBranch,
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
                    AddedOn = DateTime.Now
                };

                await dbContext.Tbl60201salesOrderMasters.AddAsync(entity);
            }

            // Save header
            await dbContext.SaveChangesAsync();

            // Call stored procedure for Cost Center
            if (!isUpdate)
            { 
                var salesOrderNoParam = new SqlParameter("@SalesOrderNo", salesOrderNo ?? (object)DBNull.Value);
            var clientNameParam = new SqlParameter("@ClientName", model.ClientCode ?? (object)DBNull.Value);
            var addedByParam = new SqlParameter("@AddedBy", model.AddedBy ?? User.Identity?.Name ?? (object)DBNull.Value);
            var isCreateCostCenterParam = new SqlParameter("@IsCreateCostCenterFromSalesOrder", true);
            var defaultCostCenterParam = new SqlParameter("@DefaultCostCenterMasterFromSalesOrder", model.CostAllocationMasterGroup ?? (object)DBNull.Value);
            var salesPersonNameParam = new SqlParameter("@SalesPersonName", model.SalesPersonCode ?? (object)DBNull.Value);

            await dbContext.Database.ExecuteSqlRawAsync(
                "EXEC [dbo].[sp600_04InsertToCostCenterFromSalesOrder] " +
                "@SalesOrderNo, @ClientName, @AddedBy, @IsCreateCostCenterFromSalesOrder, " +
                "@DefaultCostCenterMasterFromSalesOrder, @SalesPersonName",
                salesOrderNoParam, clientNameParam, addedByParam, isCreateCostCenterParam, defaultCostCenterParam, salesPersonNameParam
            );
            }
            // Handle child records
            var existingChildren = await dbContext.Tbl60202salesOrderChildren
                .Where(x => x.SalesOrderNo == salesOrderNo)
                .ToListAsync();

            var newChildren = new List<Tbl60202salesOrderChild>();

            foreach (var child in model.SalesOrderChildren)
            {
                if (child.SalesOrderChildId == 0)
                {
                    child.SalesOrderNo = salesOrderNo;
                    newChildren.Add(child);
                }
                else
                {
                    var existingChild = existingChildren
                        .FirstOrDefault(x => x.SalesOrderChildId == child.SalesOrderChildId);

                    if (existingChild != null)
                    {
                        dbContext.Entry(existingChild).CurrentValues.SetValues(child);
                    }
                }
            }

            if (newChildren.Any())
            {
                await dbContext.Tbl60202salesOrderChildren.AddRangeAsync(newChildren);
            }

            await dbContext.SaveChangesAsync();

            // Call child-level SP
            foreach (var child in model.SalesOrderChildren)
            {
                var parameters = new[]
                {
            new SqlParameter("@JobOrderNo", salesOrderNo ?? (object)DBNull.Value),
            new SqlParameter("@AddedBy", model.AddedBy ?? User.Identity?.Name ?? (object)DBNull.Value),
            new SqlParameter("@SalesOrderChildID", child.SalesOrderChildId),
            new SqlParameter("@ValveType", child.Gscode ?? (object)DBNull.Value)
        };

                await dbContext.Database.ExecuteSqlRawAsync(
                    "EXEC [dbo].[sp608_01InsertToJobOrderFromSalesOrderChild] " +
                    "@JobOrderNo, @AddedBy, @SalesOrderChildID, @ValveType", parameters);
            }

            return Ok(new
            {
                success = true,
                message = isUpdate ? "Sales order updated successfully." : "Sales order created successfully."
            });
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

            return Ok(order);
        }


        [HttpDelete]
        public async Task<IActionResult> DeleteSalesOrder(string salesOrderNo)
        {
            if (string.IsNullOrWhiteSpace(salesOrderNo))
                return BadRequest(new { message = "SalesOrderNo is required." });

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { message = "Invalid tenant." });

            var entity = await dbContext.Tbl60201salesOrderMasters
                .FirstOrDefaultAsync(x => x.SalesOrderNo == salesOrderNo);

            if (entity == null)
                return NotFound(new { message = "Sales order not found." });

            dbContext.Tbl60201salesOrderMasters.Remove(entity);
            await dbContext.SaveChangesAsync();

            return Ok(new { success = true, message = "Sales order deleted successfully." });
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
                            i.IsDiscontinued

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



    }


}




