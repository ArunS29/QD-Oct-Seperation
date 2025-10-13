using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Shared.DAL.Entities;
using QD.ERP.Web.Service;
using QD.ERP.Web.Services.Logging;
using System.Data;


namespace QD.ERP.Web.Areas.IMS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AverageCostPostingController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<DeliveryNoteJournalPostingController> _logger;
        private readonly IUserActionLogger _userActionLogger;

        public AverageCostPostingController(ILogger<DeliveryNoteJournalPostingController> logger, TenantDbContextHelper tenantDbContextHelper, IUserActionLogger userActionLogger)
        {
            _userActionLogger = userActionLogger;
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetAvgCost(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var query = dbContext.Qry60306deliveryNoteDetails.AsQueryable(); 


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
                    query = query.Where(i => i.DeliveryDate >= fromDate && i.DeliveryDate <= toDate);

                    // Fetching the data
                    var data = await query.Select(i => new
                    {
                        i.DeliveryNoteNo,
                        i.DeliveryDate,
                        i.DeliveryType,
                        i.ClientPono,
                        i.QuotationNo,
                        i.SalesOrderNo,
                        i.MaterialRequestNo,
                        i.StoreId,
                        i.ProjectId,
                        i.ClientProject,
                        i.IsVerified,
                        i.IsApproved,
                        i.ClientName,
                        i.ClientAddress,
                        i.ContactPerson,
                        i.ContactPersonTitle,
                        i.ContactMobile1,
                        i.ContactMobile2,
                        i.ContactPhone1,
                        i.ContactPhone2,
                        i.ContactEmail,
                        i.SalesPersonName,
                        i.EmailAddress,
                        i.Gscode,
                        i.IssuedQty,
                        i.IssuedUom,
                        i.UnitRateMethod,
                        i.DeliveryRemarks,
                        i.LineOrderNo,
                        i.AddlDescription,
                        i.Gsdescrpition,
                        i.UnitType,
                        i.UnitDesc,
                        i.SignatoryName,
                        i.SignatoryPosition,
                        i.SignatoryContact,
                        i.SignatoryEmail,
                        i.Salesman,
                        i.TransportedBy,
                        i.DriversName,
                        i.DriversId,
                        i.VehicleNo,
                        i.DeliveryNoteRemarks,
                        i.VendorNo,
                        i.StoreName,
                        i.StoreCode,
                        i.ClientPodate,
                        i.Mprno,
                        i.RevisionNo,
                        i.InvoiceNo,
                        i.IssuedUnitPrice,
                        i.UnitTotalPrice,
                        i.AddedOn,
                        i.ExpiryDate,
                        i.BatchNo,
                        i.DeliveryIssuedTo,
                        i.Attention,
                        i.ClientContactNo,
                        i.ClientContactEmail,
                        i.IssuedFromStoreCode,
                        i.IssuedFromStoreName,
                        i.DeliveryTypeName,
                        i.VoucherNo,
                        i.IsPosted,
                        i.PostedOn,
                        i.PostedBy,
                        i.ProjectMasterCode,
                        i.GsgroupName,
                        i.ItemSize,
                        i.ItemPartNo,
                        i.ItemBrand,
                        i.ItemMake,
                        i.StockClassification,
                        i.Hscode,
                        i.ActualSize,
                        i.Identification,
                        i.ProjectDescription,
                        i.GsdescriptionAr,
                        i.InventoryMasterGroupId,
                        i.InventoryMasterGroup,
                        i.DeliveryNoteSlNo,
                        i.ProjectLocation,
                        i.ProjectInchargeName,
                        i.ProjectInchargeEmail,
                        i.ProjectInchargeContactNo,
                        i.SalesPersonContactNo,
                        i.EmployeeNo,
                        i.EmployeeName,
                        i.PropertyNo,
                        i.PropertyDescription,
                        i.SaesPersonUserId,
                        i.TotalIssuedQty,
                        i.ClientInvoiceStatus,
                        i.IsServicesGroup

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

        [HttpPost]
        public IActionResult PostAvgCost([FromBody] List<PostAvgCostDto> items)
        {
            if (items == null || !items.Any())
                return Json(new { success = false, message = "No rows selected." });

            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    using var connection = dbContext.Database.GetDbConnection();
                connection.Open();

                foreach (var item in items)
                {
                    // 1️⃣ Call SP for Avg Cost
                    using var cmd1 = connection.CreateCommand();
                    cmd1.CommandType = CommandType.StoredProcedure;
                    cmd1.CommandText = "sp600_26InsertStockAvgCostPrice";

                    var p1 = cmd1.CreateParameter();
                    p1.ParameterName = "@EndDate";
                    p1.Value = item.EndDate;
                    cmd1.Parameters.Add(p1);

                    var p2 = cmd1.CreateParameter();
                    p2.ParameterName = "@GSCode";
                    p2.Value = item.GSCode;
                    cmd1.Parameters.Add(p2);

                    cmd1.ExecuteNonQuery();

                    // 2️⃣ Call SP for Avg Cost with GIT
                    using var cmd2 = connection.CreateCommand();
                    cmd2.CommandType = CommandType.StoredProcedure;
                    cmd2.CommandText = "sp600_26InsertStockAvgCostPrice_WtGITCost";

                    var p3 = cmd2.CreateParameter();
                    p3.ParameterName = "@EndDate";
                    p3.Value = item.EndDate;
                    cmd2.Parameters.Add(p3);

                    var p4 = cmd2.CreateParameter();
                    p4.ParameterName = "@GSCode";
                    p4.Value = item.GSCode;
                    cmd2.Parameters.Add(p4);

                    cmd2.ExecuteNonQuery();

                    // 3️⃣ Update IssuedUnitPrice in DeliveryNoteChild
                    using var cmd3 = connection.CreateCommand();
                    cmd3.CommandType = CommandType.Text;
                    cmd3.CommandText = @"UPDATE tbl603_02DeliveryNoteChild
                                     SET IssuedUnitPrice = @IssuedUnitPrice
                                     WHERE DeliveryNoteSlNo = @DeliveryNoteSlNo";

                    var p5 = cmd3.CreateParameter();
                    p5.ParameterName = "@IssuedUnitPrice";
                    p5.Value = item.IssuedUnitPrice;
                    cmd3.Parameters.Add(p5);

                    var p6 = cmd3.CreateParameter();
                    p6.ParameterName = "@DeliveryNoteSlNo";
                    p6.Value = item.DeliveryNoteSlNo;
                    cmd3.Parameters.Add(p6);

                    cmd3.ExecuteNonQuery();
                }

                return Json(new { success = true });
                }

                return Unauthorized(new { message = "Invalid tenant." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while fetching the data : {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
            }
        }
    }

}

