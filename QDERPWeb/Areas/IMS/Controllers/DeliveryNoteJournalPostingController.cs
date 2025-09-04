using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using QD.ERP.Web.Services.Logging;
using Microsoft.Data.SqlClient;
using System.Data;


namespace QD.ERP.Web.Areas.IMS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DeliveryNoteJournalPostingController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<DeliveryNoteJournalPostingController> _logger;
        private readonly IUserActionLogger _userActionLogger;

        public DeliveryNoteJournalPostingController(ILogger<DeliveryNoteJournalPostingController> logger, TenantDbContextHelper tenantDbContextHelper, IUserActionLogger userActionLogger)
        {
            _userActionLogger = userActionLogger;
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetDNJournalPosting(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var query = dbContext.Qry60304deliveryNoteViewMasters.AsQueryable();


                    
                    if (!fromDate.HasValue)
                    {
                        fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1); // Start of the current month
                    }

                    if (!toDate.HasValue)
                    {
                        toDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)); // End of the current month
                    }

                    
                    query = query.Where(i => i.DeliveryDate >= fromDate && i.DeliveryDate <= toDate);

                    
                    var data = await query.Select(i => new
                    {
                        i.DeliveryNoteNo,
                        i.DeliveryDate,
                        i.DeliveryType,
                        i.DeliveryIssuedTo,
                        i.DeliveredTo,

                        i.ClientName,
                        i.ClientCode,
                        i.ClientPono,
                        i.QuotationNo,
                        i.SalesOrderNo,
                        i.MaterialRequestNo,
                        i.ClientProject,
                        i.StoreName,
                        i.StoreCode,
                        i.StoreId,
                        i.NoteOfItems,
                        i.ClientPodate,
                        i.InvoiceNo,
                        i.IsVerified,
                        i.IsApproved,
                        i.PreparedBy,
                        i.PreparedOn,
                        i.ApprovedBy,
                        i.ApprovedOn,
                        i.AddedBy,
                        i.AddedOn,
                        i.ModifiedBy,
                        i.ModifiedOn,
                        i.SalesPersonCode,
                        i.Attention,
                        i.ClientContactNo,
                        i.ClientContactEmail,
                        i.CompanyBranch,
                        i.DeliveryNoteRemarks,
                        i.Salesman,
                        i.TransportedBy,
                        i.DriversName,
                        i.DriversId,
                        i.VehicleNo,
                        i.CompanyName,
                        i.Mprno,
                        i.IssuedFromStoreCode,
                        i.IssuedFromStoreName,
                        i.IsPosted,
                        i.VoucherNo,
                        i.PostedOn,
                        i.PostedBy,
                        i.ProjectMasterCode,
                        i.ProjectDescription,
                        i.ProjectDuration,
                        i.ProjectLocation,
                        i.InventoryMasterGroupId,
                        i.InventoryMasterGroup,
                        i.TotalIssuedQty,
                        i.SalesPersonName,
                        i.SaesPersonUserId,
                        i.QuoteTotalBeforeDiscount,


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
        public async Task<IActionResult> ApproveDeliveryNotes([FromBody] List<ApproveDeliveryNoteDto> deliveryNotes)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { success = false, message = "Invalid tenant" });

            if (deliveryNotes == null || deliveryNotes.Count == 0)
                return BadRequest(new { success = false, message = "No delivery notes provided" });

            try
            {
                var userName = HttpContext.Session.GetString("UserName");
                var userIdString = HttpContext.Session.GetString("UserId");

                if (string.IsNullOrEmpty(userName) || !int.TryParse(userIdString, out int userId))
                {
                    return Unauthorized(new { message = "Invalid or missing user session." });
                }

                foreach (var note in deliveryNotes)
                {
                    var entity = await dbContext.Tbl60301deliveryNoteMasters
                        .FirstOrDefaultAsync(d => d.DeliveryNoteNo == note.DeliveryNoteNo);

                    if (entity != null && entity.IsApproved != true) // only approve if not already approved
                    {
                        entity.IsApproved = true;
                        entity.ApprovedBy = userName; 
                        entity.ApprovedOn = DateTime.Now; 
                    }
                }

                await dbContext.SaveChangesAsync();

                return Json(new { success = true, message = "Selected delivery notes approved successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> PostDeliveryNotesToJournal([FromBody] List<PostJournalDto> deliveryNotes)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { success = false, message = "Invalid tenant" });

            if (deliveryNotes == null || deliveryNotes.Count == 0)
                return BadRequest(new { success = false, message = "No delivery notes provided" });

            try
            {
                var userName = HttpContext.Session.GetString("UserName");
                var userIdString = HttpContext.Session.GetString("UserId");

                if (string.IsNullOrEmpty(userName) || !int.TryParse(userIdString, out int userId))
                {
                    return Unauthorized(new { message = "Invalid or missing user session." });
                }

                using (var connection = dbContext.Database.GetDbConnection())
                {
                    await connection.OpenAsync();

                    foreach (var note in deliveryNotes)
                    {
                        // Check if already posted
                        var alreadyPosted = await dbContext.Tbl60301deliveryNoteMasters
                            .AnyAsync(d => d.DeliveryNoteNo == note.DeliveryNoteNo && d.IsPosted == true);

                        if (alreadyPosted)
                            continue;

                        // Check if has amount   
                        var hasAmount = await dbContext.Qry60306deliveryNoteDetails
                            .AnyAsync(d => d.DeliveryNoteNo == note.DeliveryNoteNo && d.IssuedUnitPrice > 0);

                        if (!hasAmount)
                            continue;

                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;

                            if (note.DeliveryType == 2)
                                cmd.CommandText = "sp600_30InsertJVfromDNForStoreConsumption";
                            else if (note.DeliveryType == 3)
                                cmd.CommandText = "sp600_29InsertJVfromDeliveryNote";
                            else
                                continue;

                            cmd.Parameters.Add(new SqlParameter("@DeliveryNoteNo", note.DeliveryNoteNo));
                            cmd.Parameters.Add(new SqlParameter("@DeliveryNoteDate", note.DeliveryDate));
                            cmd.Parameters.Add(new SqlParameter("@JustAddedVoucherEntryNoSubLedger", SqlDbType.Int) { Value = 0 });
                            cmd.Parameters.Add(new SqlParameter("@JustAddedVoucherEntryNoCostAlloc", SqlDbType.Int) { Value = 0 });

                            cmd.Parameters.Add(new SqlParameter("@AddedBy", userName));
                            cmd.Parameters.Add(new SqlParameter("@AddedOn", DateTime.Now));

                            await cmd.ExecuteNonQueryAsync();
                        }

                        // Update flags
                        var entity = await dbContext.Tbl60301deliveryNoteMasters
                            .FirstOrDefaultAsync(d => d.DeliveryNoteNo == note.DeliveryNoteNo);

                        if (entity != null)
                        {
                            entity.IsPosted = true;
                            entity.PostedBy = userName;
                            entity.PostedOn = DateTime.Now;
                            entity.VoucherNo = note.DeliveryNoteNo;

                            if (entity.IsApproved != true)
                            {
                                entity.IsApproved = true;
                                entity.ApprovedBy = userName;
                                entity.ApprovedOn = DateTime.Now;
                            }
                        }
                    }

                    await dbContext.SaveChangesAsync();
                }

                _logger.LogInformation("Delivery Notes Posted: {@DeliveryNotes} by {User} at {Time}",
                    deliveryNotes, userName, DateTime.Now);

                return Json(new { success = true, message = "Delivery notes posted to journal successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error posting delivery notes");
                return Json(new { success = false, message = ex.Message });
            }
        }
    
}
}
