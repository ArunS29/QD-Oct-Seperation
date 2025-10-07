using System;
using System.Linq;
using System.Threading.Tasks;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using QD.ERP.Web.Services.Logging;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class JournalRegisterController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<JournalRegisterController> _logger;
        private readonly IUserActionLogger _userActionLogger;

        public JournalRegisterController(ILogger<JournalRegisterController> logger, IUserActionLogger userActionLogger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
            _userActionLogger = userActionLogger;
        }

        [HttpGet]
        public async Task<ActionResult> GetJournalview(byte RequesterID, DateTime StartDate, DateTime EndDate, bool IfShowAll)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    ERPMasterWtDataContextProcedures procedure = new ERPMasterWtDataContextProcedures(dbContext);
                    var result = await dbContext.JournalRegisterViews
                        .FromSqlRaw("EXEC sp20201JournalRegisterView @p0, @p1, @p2, @p3",
                            RequesterID, StartDate, EndDate, IfShowAll)
                        .ToListAsync();


                    return Json(result); // 200 OK


                    // Return 400 Bad Request if no data found

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing sp20201JournalRegisterView for requester {RequesterID}", RequesterID);

                    // Return 500 Internal Server Error
                    return StatusCode(500, new { success = false, message = "An unexpected error occurred while loading journal data." });
                }
            }

            // Return 401 Unauthorized
            return Unauthorized(new { success = false, message = "Tenant context could not be resolved. Access denied." });
        }

        [HttpGet]
        public async Task<ActionResult> GetUser()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var users = await dbContext.TblUserMasters
                        .Select(u => new
                        {
                            u.UserId,
                            u.UserName,

                        })
                        .ToListAsync();

                    return Json(users);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetUser: {ex.Message}");
                    return Json(new { error = "Unable to fetch user data at this time." });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public ActionResult UpdateData(sp20201JournalRegisterViewResult updatedItem)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    // Perform the update logic here.
                    // Example: Update the item in the database.

                    return Json(updatedItem);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in UpdateData: {ex.Message}");
                    return StatusCode(500, new { success = false, message = "An error occurred while updating the data." });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<ActionResult> GetJournalChild(string journalRefNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var result = await dbContext.Qry202101journalRegisterChildren
                                    .Where(x => x.JournalRefNo == journalRefNo)
                                    .ToListAsync();

                    if (result != null && result.Any())
                    {
                        return Json(result);
                    }

                    return Json(new { success = false, message = "No child records found." });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetJournalChild: {ex.Message}");
                    return Json(new { success = false, message = "An error occurred while fetching child records." });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<ActionResult> GetJournalmaster(string journalRefNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var result = await dbContext.Tbl20126JournalRegisterMasters
                                    .Where(x => x.JournalRefNo == journalRefNo)
                                    .ToListAsync();

                    if (result != null && result.Any())
                    {
                        return Json(result);
                    }

                    return Json(new { success = false, message = "No child records found." });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetJournalChild: {ex.Message}");
                    return Json(new { success = false, message = "An error occurred while fetching child records." });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        // GET: /Finance/GetJournalStatus
        [HttpGet]
        public IActionResult GetJournalStatus(string journalRefNo)
        {
            if (string.IsNullOrEmpty(journalRefNo))
                return BadRequest("Invalid JournalRefNo.");

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var journal = dbContext.Tbl20126JournalRegisterMasters
                    .Where(j => j.JournalRefNo == journalRefNo)
                    .Select(j => new
                    {
                        isSubmitted = j.IsSubmittedToFinance,
                        isApproved = j.IsApproved,
                        isPosted = j.IsPosted
                    })
                    .FirstOrDefault();

                if (journal == null)
                    return NotFound();

                return Json(journal);
            }

            return StatusCode(500, "Tenant context could not be loaded.");
        }

        // POST: /Finance/DeleteJournalEntry
        [HttpPost]
        public async Task<IActionResult> DeleteJournalEntryAsync(string journalRefNo)
        {
            if (string.IsNullOrEmpty(journalRefNo))
                return BadRequest("Invalid JournalRefNo.");

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var master = dbContext.Tbl20126JournalRegisterMasters
                    .FirstOrDefault(j => j.JournalRefNo == journalRefNo);

                if (master == null)
                    return NotFound();

                var childList = dbContext.Tbl20127JournalRegisterChildren
                    .Where(c => c.JournalRefNo == journalRefNo)
                    .ToList();
                var costList = dbContext.Tbl20128JournalRegisterCostAllocations
                    .Where(c => c.VoucherNo == journalRefNo)
                    .ToList();
                var employeeList = dbContext.Tbl20129JournalRegisterEmployeeAllocations
                    .Where(c => c.VoucherNo == journalRefNo)
                    .ToList();
                var propertyList = dbContext.Tbl20130JournalRegisterPropertyAllocations
                    .Where(c => c.VoucherNo == journalRefNo)
                    .ToList();


                dbContext.Tbl20127JournalRegisterChildren.RemoveRange(childList);
                dbContext.Tbl20126JournalRegisterMasters.Remove(master);
                dbContext.Tbl20128JournalRegisterCostAllocations.RemoveRange(costList);
                dbContext.Tbl20129JournalRegisterEmployeeAllocations.RemoveRange(employeeList);
                dbContext.Tbl20130JournalRegisterPropertyAllocations.RemoveRange(propertyList);
                dbContext.SaveChanges();
                await _userActionLogger.LogAsync(
                module: "Finance > Journal Register",
                actionDetail: $"Delete Journal: {master.JournalRefNo}",
                documentNo: master.JournalRefNo
            );

                return Json(new { success = true });
            }

            return StatusCode(500, "Tenant context could not be loaded.");
        }

        [HttpPost]
        public async Task<IActionResult> UnlockJournalEntryAsync(string journalRefNo)
        {
            if (string.IsNullOrEmpty(journalRefNo))
                return BadRequest(new { success = false, message = "JournalRefNo is required." });

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            var entry = dbContext.Tbl20126JournalRegisterMasters
                .FirstOrDefault(j => j.JournalRefNo == journalRefNo);

            if (entry == null)
                return NotFound(new { success = false, message = "Journal entry not found." });

           

            // Reset fields
            entry.IsSubmittedToFinance = false;
            entry.SubmittedBy = null;
            entry.SubmittedOn = null;
            entry.IsVerified = false;
            entry.VerifiedBy = null;
            entry.VerifiedOn = null;
            entry.IsApproved = false;
            entry.ApprovedBy = null;
            entry.ApprovedOn = null;

            dbContext.SaveChanges();
            await _userActionLogger.LogAsync(
            module: "Finance > Journal Register",
            actionDetail: $"Unlocked: {entry.JournalRefNo}",
            documentNo: entry.JournalRefNo
            );
            return Ok(new { success = true });
        }
        [HttpPost]
        public IActionResult CloneJournalEntry([FromBody] CloneJournalEntryRequest model)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            if (string.IsNullOrEmpty(model.FromJournalRefNo) || string.IsNullOrEmpty(model.ToJournalRefNo))
            {
                return BadRequest(new { success = false, message = "Invalid input data." });
            }

            var addedBy = HttpContext.Session.GetString("UserName") ?? "System";
            var addedOn = DateTime.Now;
            var journalDate = DateTime.Now;

            try
            {
                dbContext.Database.ExecuteSqlRaw(
                    "EXEC sp20204CloneJournalEntry @FromJournalRefNo = {0}, @ToJournalRefNo = {1}, @JournalEntryDate = {2}, @RequesterID = {3}, @AddedBy = {4}, @AddedOn = {5}",
                    model.FromJournalRefNo,
                    model.ToJournalRefNo,
                    journalDate,
                    model.RequesterId,
                    addedBy,
                    addedOn
                );

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error cloning journal entry: {ex.Message}");
                return BadRequest(new { success = false, message = "Failed to clone journal entry." });
            }
        }
       


    }
}





