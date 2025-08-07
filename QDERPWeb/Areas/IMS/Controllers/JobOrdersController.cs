using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.AspNet.Mvc;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.Areas.Finance.Reports.Payable_Statements;
using QD.ERP.Web.DAL.Entities;
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
    public class JobOrdersController : Controller
    {

        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<JobOrdersController> _logger;
        private readonly IUserActionLogger _userActionLogger;


        public JobOrdersController(ILogger<JobOrdersController> logger, TenantDbContextHelper tenantDbContextHelper, IUserActionLogger userActionLogger)
        {
            _userActionLogger = userActionLogger;
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetJobOrders(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var query = dbContext.Tbl60801jobOrderMasters.AsQueryable();


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
                    query = query.Where(i => i.JobOrderDate >= fromDate && i.JobOrderDate <= toDate);

                    // Fetching the data
                    var data = await query.Select(i => new
                    {
                        i.ValveType,
                        i.JobOrderNo,
                        i.JobOrderDate,
                        i.JobOrderDescription,
                        i.JobOrderStatus,
                        i.JobOrderType,
                        i.TagNo,
                        i.Size,
                        i.Class,
                        i.Operator,
                        i.Materials,
                        i.Qty,
                        i.Make,
                        i.ModelNo,
                        i.ItemSlNo,
                        i.WorkOrderNo,
                        //i.workorderDate
                    }).ToListAsync();

                    return Json(data);
                }

                return Unauthorized(new { message = "Invalid tenant." });
            }
            catch (Exception
            
             ex)
            {
                return StatusCode(500, new { message = "Internal server error." });
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
            }
        }
        //Job Order Status
        [HttpGet]
        public async Task<IActionResult> GetJobOrderStatus()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var branches = await dbContext.Tbl60806jobOrderStatusMasters.ToListAsync();
                    return Ok(branches);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetJobOrderStatus: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public async Task<IActionResult> SaveOrUpdateJobOrderStatus([FromBody] Tbl60806jobOrderStatusMaster model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.JobOrderStatus))
            {
                return BadRequest(new { success = false, message = "Invalid data received." });
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var existing = await dbContext.Tbl60806jobOrderStatusMasters
                        .FirstOrDefaultAsync(x => x.JobOrderStatusId == model.JobOrderStatusId);

                    if (existing != null)
                    {
                        // Update existing record
                        existing.JobOrderStatus = model.JobOrderStatus;
                       // existing.IsActive = model.IsActive;
                        // Add any other fields you want to update
                    }
                    else
                    {
                        var lastId = await dbContext.Tbl60806jobOrderStatusMasters
                              .OrderByDescending(x => x.JobOrderStatusId)
                            .Select(x => x.JobOrderStatusId)
                           .FirstOrDefaultAsync();

                        // Check for overflow beyond byte (0–255)
                        if (lastId >= byte.MaxValue)
                        {
                            return BadRequest(new { success = false, message = "Maximum JobOrderStatusId limit reached." });
                        }

                        model.JobOrderStatusId = (byte)(lastId == 0 ? 1 : lastId + 1);

                        dbContext.Tbl60806jobOrderStatusMasters.Add(model);
                    }

                    await dbContext.SaveChangesAsync();
                    await _userActionLogger.LogAsync(
                      module: "IMS > Save Or Update Job Order Status",
                      actionDetail: $"Saved Job Order Status  {model.JobOrderStatusId}",
                      documentNo: $"{model.JobOrderStatusId}"
                    );


                    return Ok(new
                    {
                        success = true,
                        message = "Saved successfully",
                        id = model.JobOrderStatusId
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
        [HttpDelete("DeleteJobOrderStatus/{id}")]

        public async Task<IActionResult> DeleteJobOrderStatus(byte id)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var existing = await dbContext.Tbl60806jobOrderStatusMasters
                        .FirstOrDefaultAsync(s => s.JobOrderStatusId == id);

                    if (existing == null)
                    {
                        return NotFound(new { success = false, message = "JobOrderStatus not found" });
                    }

                    dbContext.Tbl60806jobOrderStatusMasters.Remove(existing);
                    await dbContext.SaveChangesAsync();
                    await _userActionLogger.LogAsync(
                      module: "IMS > Delete Job Order Status",
                      actionDetail: $":Deleted Job Order Status  {id}",
                      documentNo: $"{id}"
                    );

                    return Ok(new { success = true, message = "Deleted successfully" });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in DeleteJobOrderStatus");
                    return StatusCode(500, new { success = false, message = ex.Message });
                }
            }

            return Unauthorized(new { success = false, message = "Invalid tenant" });
        }

        //Job Order Test Form
        [HttpGet]
        public async Task<IActionResult> GetJobOrderTest()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var branches = await dbContext.Tbl60805jobOrderTests.ToListAsync();

                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetJobOrderStatus: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpPost]
        public IActionResult DeleteJobOrderView(string JobOrderNo)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Json(new { success = false, message = "Invalid tenant context." });

            try
            {
                var jobOrder = dbContext.Tbl60801jobOrderMasters
                    .FirstOrDefault(x => x.JobOrderNo == JobOrderNo);

                if (jobOrder == null)
                    return Json(new { success = false, message = "Job Order not found." });
                if (jobOrder.IsApproved == true)
                {
                    return Json(new { success = false, message = "Job Order is already approved. You cannot delete the approved Job Order." });
                }
                // ✅ Check if Job Order is delivered (JobOrderStatus = 1)
                if (jobOrder.JobOrderStatus == 1)
                    return Json(new { success = false, message = "Job Order is already delivered. You cannot delete the delivered Job Order." });

                // ✅ Delete related Job Order Tests
                var jobOrderTests = dbContext.Tbl60805jobOrderTests
                    .Where(x => x.JobOrderNo == JobOrderNo)
                    .ToList();
                dbContext.Tbl60805jobOrderTests.RemoveRange(jobOrderTests);

                // ✅ Delete Job Order Master
                dbContext.Tbl60801jobOrderMasters.Remove(jobOrder);

                // ✅ Optionally delete files (if applicable)
                // DeleteDocumentPDF(JobOrderNo, "VoucherScanned\\IMSJobOrder");

                dbContext.SaveChanges();
                _userActionLogger.LogAsync(module: "IMS > Delete Job Order View ",
                 actionDetail: $":Deleted Job Order View{JobOrderNo}",
                 documentNo: $"{JobOrderNo}"
                );

                // ✅ Log deletion (if logging is implemented)
                //string userId = HttpContext.Session.GetString("UserID") ?? "Unknown";
                //string userName = HttpContext.Session.GetString("UserName") ?? "Unknown";

                //InsertUserEntryLogSheet(
                //    "IMS Job Order",
                //    $"IMS Job Order Ref No. {JobOrderNo} has been deleted by User ID: {userId} User Name: {userName}.",
                //    userName,
                //    JobOrderNo
                //);

                return Json(new { success = true, message = "Job Order has been successfully removed from the database." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Job Order.");
                return Json(new { success = false, message = "An error occurred while deleting the Job Order." });
            }


        }

        [HttpPost]
        public async Task<IActionResult> UnlockJobOrder([FromBody] Tbl60801jobOrderMaster request)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized(new { success = false, message = "Invalid tenant." });

                if (string.IsNullOrWhiteSpace(request?.JobOrderNo))
                    return BadRequest(new { success = false, message = "JobOrder No is required." });

                var existingEntity = await dbContext.Tbl60801jobOrderMasters
                    .FirstOrDefaultAsync(x => x.JobOrderNo == request.JobOrderNo);

                if (existingEntity == null)
                    return NotFound(new { success = false, message = "Job Order not found." });

                if (existingEntity.IsApproved != true && existingEntity.IsVerified != true)
                    return Ok(new { success = false, message = "Job Order is already unlocked." });

                existingEntity.IsApproved = false;
                existingEntity.IsVerified = false;

                dbContext.Tbl60801jobOrderMasters.Update(existingEntity);
                await dbContext.SaveChangesAsync();
                await _userActionLogger.LogAsync(
                      module: "IMS > Unlock Job Order",
                      actionDetail: $":Unlocked Job Order  {request.JobOrderNo}",
                      documentNo: $"{request.JobOrderNo}"
                );

                return Ok(new { success = true, message = "Job Order has been unlocked successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while unlocking Job Order.");
                return StatusCode(500, new { success = false, message = "Internal server error", details = ex.Message });
            }
        }

    }
}

