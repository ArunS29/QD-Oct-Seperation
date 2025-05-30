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
                    var lastOrderNo = dbContext.Qry60204salesOrderViewMasters
                        .OrderByDescending(x => x.SalesOrderNo)
                        .Select(x => x.SalesOrderNo)
                        .FirstOrDefault();

                    string nextOrderNo = GenerateNextOrderNo(lastOrderNo);

                    return Ok(new { salesOrderNo = nextOrderNo });
                }

                return Unauthorized(new { message = "Invalid tenant." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
            }
        }

        private string GenerateNextOrderNo(string lastOrderNo)
        {
            if (string.IsNullOrWhiteSpace(lastOrderNo))
                return "AIC-RFQ-2025-00001";

            var match = Regex.Match(lastOrderNo, @"(.*-)(\d+)$");
            if (!match.Success) return lastOrderNo + "-00001";

            var prefix = match.Groups[1].Value;
            var number = int.Parse(match.Groups[2].Value);
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
        public async Task<IActionResult> InsertSalesOrderFromQuotation([FromBody] InsertSalesOrderFromQuotationDto model)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { message = "Invalid tenant or missing context.", success = false });
            }

            try
            {
                _logger.LogInformation("InsertSalesOrderFromQuotation called with SalesOrderNo: {SalesOrderNo}, QuotationNo: {QuotationNo}, AddedBy: {AddedBy}, IsCreateCostCenter: {IsCreateCostCenter}, DefaultCostCenterMaster: {DefaultCostCenterMaster}",
                    model.SalesOrderNo, model.QuoteNo, model.AddedBy, model.IsCreateCostCenterFromSalesOrder, model.CostAllocationMasterGroup);

                var sql = "EXEC sp600_04InsertToSalesOrderFromQuotation @SalesOrderNo, @QuotationNo, @AddedBy, @IsCreateCostCenterFromSalesOrder, @DefaultCostCenterMasterFromSalesOrder";
                var parameters = new[]
                {
            new SqlParameter("@SalesOrderNo", model.SalesOrderNo ?? (object)DBNull.Value),
            new SqlParameter("@QuotationNo", model.QuoteNo ?? (object)DBNull.Value),
            new SqlParameter("@AddedBy", model.AddedBy ?? (object)DBNull.Value),
            new SqlParameter("@IsCreateCostCenterFromSalesOrder", model.IsCreateCostCenterFromSalesOrder),
            new SqlParameter("@DefaultCostCenterMasterFromSalesOrder", model.CostAllocationMasterGroup ?? (object)DBNull.Value)
        };

                await using var transaction = await dbContext.Database.BeginTransactionAsync();

                await dbContext.Database.ExecuteSqlRawAsync(sql, parameters);

                await transaction.CommitAsync();

                return Ok(new { message = "Sales order inserted successfully from quotation.", success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error executing stored procedure sp600_04InsertToSalesOrderFromQuotation");
                return StatusCode(500, new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }



    }





    public class InsertSalesOrderFromQuotationDto
    {
        public string SalesOrderNo { get; set; }
        public string QuoteNo { get; set; }
        public string AddedBy { get; set; }
        public bool IsCreateCostCenterFromSalesOrder { get; set; }
        public string CostAllocationMasterGroup { get; set; }
    }


}




