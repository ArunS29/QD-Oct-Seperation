using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.DAL;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QD.ERP.Web.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;

        public NotificationController(TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
        }

        /// <summary>
        /// ✅ Get paginated notifications for current user (used in DevExtreme grid)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUserNotifications(DataSourceLoadOptions loadOptions)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !byte.TryParse(userIdStr, out byte userId))
                return Unauthorized(new { message = "Invalid session or user ID", success = false });

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out ERPMasterWtDataContext dbContext))
            {
                using (dbContext)
                {
                    var query = dbContext.Tbl901AlertUsers
                        .Where(x => x.AlertUserId == userId)
                        .OrderByDescending(x => x.AlertUserOn)
                        .Select(x => new
                        {
                            x.AlertCode,
                            x.AlertUserMessage,
                            x.AlertStatusRemarks,
                            x.AlertUserOn,
                            x.AlertBySystem,
                            x.AlertNotifiedByUser
                        });

                    var result = await DataSourceLoader.LoadAsync(query, loadOptions);
                    return Ok(result);
                }
            }

            return Unauthorized(new { message = "Invalid tenant", success = false });
        }



        /// <summary>
        /// ✅ Get count of unseen notifications for badge
        /// </summary>
        /// <summary>
        /// ✅ Get count of unseen notifications for badge
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUnseenCount()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !byte.TryParse(userIdStr, out byte userId))
                return Unauthorized(new { message = "Invalid session or user ID", success = false });

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out ERPMasterWtDataContext dbContext))
            {
                using (dbContext)
                {
                    var unseenCount = await dbContext.Tbl901AlertUsers
                        .Where(x => x.AlertUserId == userId && x.AlertNotifiedByUser == false)
                        .CountAsync();

                    return Ok(new { unseenCount });
                }
            }

            return Unauthorized(new { message = "Invalid tenant", success = false });
        }


        /// <summary>
        /// ✅ Mark all unseen notifications as read for current user
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> tbl901AlertUsers()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !byte.TryParse(userIdStr, out byte userId))
                return Unauthorized(new { message = "Invalid session or user ID", success = false });

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out ERPMasterWtDataContext dbContext))
            {
                using (dbContext)
                {
                    var unseenAlerts = await dbContext.Tbl901AlertUsers
                        .Where(x => x.AlertUserId == userId && x.AlertNotifiedByUser == false)
                        .ToListAsync();

                    if (unseenAlerts.Any())
                    {
                        unseenAlerts.ForEach(alert => alert.AlertNotifiedByUser = true);
                        await dbContext.SaveChangesAsync();
                    }

                    return Ok(new { success = true, message = "Marked all as read" });
                }
            }

            return Unauthorized(new { message = "Invalid tenant", success = false });
        }

        /// <summary>
        /// ✅ Delete selected notifications by AlertNo
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] List<string> alertNos)
        {
            if (alertNos == null || alertNos.Count == 0)
                return BadRequest(new { success = false, message = "No alerts specified for deletion" });

            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !byte.TryParse(userIdStr, out byte userId))
                return Unauthorized(new { message = "Invalid session or user ID", success = false });

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out ERPMasterWtDataContext dbContext))
            {
                using (dbContext)
                {
                    var alertsToDelete = await dbContext.Tbl901AlertUsers
                        .Where(x => x.AlertUserId == userId && alertNos.Contains(x.AlertCode))
                        .ToListAsync();

                    if (alertsToDelete.Count == 0)
                        return NotFound(new { success = false, message = "No matching notifications found" });

                    dbContext.Tbl901AlertUsers.RemoveRange(alertsToDelete);
                    await dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Notifications deleted" });
                }
            }

            return Unauthorized(new { message = "Invalid tenant", success = false });
        }
        [HttpPost]
        public async Task<IActionResult> MarkAsSeen([FromBody] MarkAsSeenRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.AlertCode))
                return BadRequest(new { success = false, message = "Invalid request data" });

            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !byte.TryParse(userIdStr, out byte userId))
                return Unauthorized(new { message = "Invalid session or user ID", success = false });

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out ERPMasterWtDataContext dbContext))
            {
                using (dbContext)
                {
                    var alert = await dbContext.Tbl901AlertUsers
                        .FirstOrDefaultAsync(x => x.AlertUserId == userId && x.AlertCode == request.AlertCode);

                    if (alert == null)
                        return NotFound(new { success = false, message = "Notification not found" });

                    alert.IsSeen = true;
                    await dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Notification marked as seen" });
                }
            }

            return Unauthorized(new { message = "Invalid tenant", success = false });
        }
        [HttpPost]
        public async Task<IActionResult> MarkAllAsSeen()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !byte.TryParse(userIdStr, out byte userId))
                return Unauthorized(new { success = false, message = "Invalid session or user ID" });

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out ERPMasterWtDataContext dbContext))
            {
                using (dbContext)
                {
                    var unseenAlerts = await dbContext.Tbl901AlertUsers
                        .Where(x => x.AlertUserId == userId && !x.IsSeen)
                        .ToListAsync();

                    if (!unseenAlerts.Any())
                    {
                        return Ok(new { success = true, message = "No unseen alerts found." });
                    }

                    unseenAlerts.ForEach(x => x.IsSeen = true);

                    await dbContext.SaveChangesAsync();

                    return Ok(new
                    {
                        success = true,
                        message = $"{unseenAlerts.Count} alert(s) marked as seen."
                    });
                }
            }

            return Unauthorized(new { success = false, message = "Invalid tenant or database context." });
        }
        [HttpGet]
        public IActionResult GetFinanceNotificationSummary()
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { message = "Invalid tenant.", success = false });

            try
            {
                var vouchers = dbContext.Qry20136VoucherMasterLists.AsQueryable();

                // --- Summary counts ---
                Func<List<string>, object> getCounts = (types) =>
                {
                    var filtered = vouchers.Where(v => types.Contains(v.VoucherType));
                    return new
                    {
                        ToBeVerified = filtered.Count(v => !v.IsVerified),
                        ToBeApproved = filtered.Count(v => v.IsVerified && !v.IsApproved),
                        ToBeAudited = filtered.Count(v => v.IsApproved && !v.IsAuditVerified)
                    };
                };

                var summary = new
                {
                    Payments = getCounts(new List<string> { "Bank Payment", "Cash Payment" }),
                    Receipts = getCounts(new List<string> { "Bank Receipts", "Cash Receipts" }),
                    SalesPurchase = getCounts(new List<string> { "Sales", "Purchase" }),
                    Journals = getCounts(new List<string> { "Journal" }),
                    ExpenseClaims = getCounts(new List<string> { "ExpenseClaim" })
                };

                // --- Detailed notifications ---
                var notifications = vouchers.Select(v => new
                {
                    VoucherType = v.VoucherType,
                    VoucherNumber = v.VoucherNo,
                    Amount = v.DebitAmount ?? v.CreditAmount,
                    User = !v.IsVerified ? v.VoucherEnteredBy
                           : v.IsVerified && !v.IsApproved ? v.VoucherVerifiedBy
                           : v.IsApproved && !v.IsAuditVerified ? v.VoucherApprovedBy
                           : v.VoucherEnteredBy,
                    Status = !v.IsVerified ? "waiting for verification"
                           : v.IsVerified && !v.IsApproved ? "awaiting your approval"
                           : v.IsApproved && !v.IsAuditVerified ? "awaiting audit"
                           : "completed",
                    CreatedOn = v.VoucherEnteredOn ?? DateTime.Now
                })
                .OrderByDescending(v => v.CreatedOn)
                .Take(20) // last 20 notifications
                .ToList();

                return Ok(new
                {
                    Summary = summary,
                    Notifications = notifications
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error while fetching finance notifications", error = ex.Message });
            }
        }

        public IActionResult GetVatSalesNotificationSummary()
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { message = "Invalid tenant.", success = false });

            try
            {
                var sales = dbContext.Tbl20161VatinvoiceMasters.AsQueryable();

                // Summary counts
                var summary = new
                {
                    ToBeVerified = sales.Count(x => x.IsSubmitted == true && (x.IsVerified != true)),
                    ToBeApproved = sales.Count(x => x.IsVerified == true && (x.IsApproved != true)),
                    ToBeAudited = sales.Count(x => x.IsApproved == true /* && (x.IsAuditVerified != true) */)
                };

                // Latest notifications
                var notifications = sales
                    .Select(v => new
                    {
                        VoucherType = "VAT Sales",
                        VoucherNumber = v.InvoiceNo,
                        Amount = (decimal?)0, // Replace if you have actual amount field
                        User = (v.IsVerified != true) ? v.SubmittedBy
                               : (v.IsVerified == true && v.IsApproved != true) ? v.VerifiedBy
                               : (v.IsApproved == true /* && v.IsAuditVerified != true */) ? v.ApprovedBy
                               : v.SubmittedBy,
                        Status = (v.IsVerified != true) ? "waiting for verification"
                               : (v.IsVerified == true && v.IsApproved != true) ? "awaiting your approval"
                               : (v.IsApproved == true /* && v.IsAuditVerified != true */) ? "awaiting audit"
                               : "completed",
                        CreatedOn = v.SubmittedOn ?? DateTime.Now
                    })
                    .OrderByDescending(v => v.CreatedOn)
                    .Take(20)
                    .ToList();

                return Ok(new
                {
                    success = true,
                    Summary = summary,
                    Notifications = notifications
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

      
        public IActionResult GetVatDebitNotificationSummary()
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { message = "Invalid tenant.", success = false });

            try
            {
                var debitNotes = dbContext.Tbl20172VatdebitNoteMasters.AsQueryable();

                // Summary counts
                var summary = new
                {
                    ToBeVerified = debitNotes.Count(x => x.IsSubmitted == true && (x.IsVerified ?? false) == false),
                    ToBeApproved = debitNotes.Count(x => x.IsVerified == true && (x.IsApproved ?? false) == false),
                    ToBeAudited = debitNotes.Count(x => x.IsApproved == true /* && (x.IsAuditVerified ?? false) == false */)
                };

                // Latest notifications
                var notifications = debitNotes
                    .Select(v => new
                    {
                        VoucherType = "VAT Debit",
                        VoucherNumber = v.DebitNoteNo,
                        Amount = (decimal?)0, // replace if you have amount field
                        User = !(v.IsVerified ?? false) ? v.SubmittedBy
                               : (v.IsVerified ?? false) && !(v.IsApproved ?? false) ? v.VerifiedBy
                               : (v.IsApproved ?? false) /* && !(v.IsAuditVerified ?? false) */ ? v.ApprovedBy
                               : v.SubmittedBy,
                        Status = !(v.IsVerified ?? false) ? "waiting for verification"
                               : (v.IsVerified ?? false) && !(v.IsApproved ?? false) ? "awaiting your approval"
                               : (v.IsApproved ?? false) /* && !(v.IsAuditVerified ?? false) */ ? "awaiting audit"
                               : "completed",
                        CreatedOn = v.SubmittedOn ?? DateTime.Now
                    })
                    .OrderByDescending(v => v.CreatedOn)
                    .Take(20)
                    .ToList();

                return Ok(new
                {
                    success = true,
                    Summary = summary,
                    Notifications = notifications
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        public IActionResult GetVatCreditNotificationSummary()
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { message = "Invalid tenant.", success = false });

            try
            {
                var creditNotes = dbContext.Tbl20170VatcreditNoteMasters.AsQueryable();

                // Summary counts
                var summary = new
                {
                    ToBeVerified = creditNotes.Count(x => x.IsSubmitted == true && (x.IsVerified ?? false) == false),
                    ToBeApproved = creditNotes.Count(x => x.IsVerified == true && (x.IsApproved ?? false) == false),
                    ToBeAudited = creditNotes.Count(x => x.IsApproved == true && (x.IsPosted ?? false) == false)
                };

                // Latest notifications
                var notifications = creditNotes
                    .Select(v => new
                    {
                        VoucherType = "VAT Credit",
                        VoucherNumber = v.CreditNoteNo,
                        Amount = (decimal?)0, // replace if you have amount field
                        User = !(v.IsVerified ?? false) ? v.SubmittedBy
                               : (v.IsVerified ?? false) && !(v.IsApproved ?? false) ? v.VerifiedBy
                               : (v.IsApproved ?? false) && !(v.IsPosted ?? false) ? v.ApprovedBy
                               : v.SubmittedBy,
                        Status = !(v.IsVerified ?? false) ? "waiting for verification"
                               : (v.IsVerified ?? false) && !(v.IsApproved ?? false) ? "awaiting your approval"
                               : (v.IsApproved ?? false) && !(v.IsPosted ?? false) ? "awaiting audit"
                               : "completed",
                        CreatedOn = v.SubmittedOn ?? v.AddedOn ?? DateTime.Now
                    })
                    .OrderByDescending(v => v.CreatedOn)
                    .Take(20)
                    .ToList();

                return Ok(new
                {
                    success = true,
                    Summary = summary,
                    Notifications = notifications
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        public IActionResult GetVatPurchaseNotificationSummary()
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { message = "Invalid tenant.", success = false });

            try
            {
                var purchaseNotes = dbContext.Tbl20166VatpurchaseMasters.AsQueryable();

                var summary = new
                {
                    ToBeVerified = purchaseNotes.Count(x => x.IsVerified == false || x.IsVerified == null),
                    ToBeApproved = purchaseNotes.Count(x => x.IsVerified == true && (x.IsApproved == false || x.IsApproved == null)),
                    ToBeAudited = purchaseNotes.Count(x => x.IsApproved == true) // Adjust if you have audit logic
                };

                var notifications = purchaseNotes
                    .Select(v => new
                    {
                        VoucherType = "VAT Purchase",
                        VoucherNumber = v.PurchaseVoucherNo,
                        Amount = (decimal?)0, // replace with amount field if exists
                        User = !(v.IsVerified ?? false) ? v.AddedBy
                               : (v.IsVerified ?? false) && !(v.IsApproved ?? false) ? v.VerifiedBy
                               : (v.IsApproved ?? false) ? v.ApprovedBy
                               : v.AddedBy,
                        Status = !(v.IsVerified ?? false) ? "waiting for verification"
                               : (v.IsVerified ?? false) && !(v.IsApproved ?? false) ? "awaiting your approval"
                               : (v.IsApproved ?? false) ? "awaiting audit"
                               : "completed",
                        CreatedOn = v.AddedOn ?? DateTime.Now
                    })
                    .OrderByDescending(v => v.CreatedOn)
                    .Take(20)
                    .ToList();

                return Ok(new
                {
                    success = true,
                    Summary = summary,
                    Notifications = notifications
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetMaterialRequestSummary()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    // Query the view
                    var query = dbContext.Qry60604purchaseRequestViewMasters.AsQueryable();

                    // Build summary counts (Stages from your table)
                    var summary = new
                    {
                        ToBeVerified = await query.CountAsync(x => x.IsSubmitted == true && x.IsVerified != true && x.IsCancelled == false),
                        ToBeApproved = await query.CountAsync(x => x.IsVerified == true && x.IsApproved != true && x.IsCancelled == false),
                        ToBeCancelled = await query.CountAsync(x => x.IsCancelled == true)
                    };

                    // Get recent notifications (optional, for UI display)
                    var notifications = await query
                        .OrderByDescending(x => x.AddedOn)
                        .Take(10)
                        .Select(x => new
                        {
                            VoucherNumber = x.Mprno,
                            VoucherType = "Material Request",
                            Status = x.IsCancelled ? "Cancelled"
                                   : x.IsApproved == true ? "Approved"
                                   : x.IsVerified == true ? "Verified"
                                   : x.IsSubmitted == true ? "Submitted"
                                   : "Draft",
                            User = x.PreparedBy ?? "System",
                            Amount = x.TotalRequestCost,
                            CreatedOn = x.AddedOn
                        })
                        .ToListAsync();

                    return Ok(new { success = true, Summary = summary, Notifications = notifications });
                }
                catch (Exception ex)
                {
                    
                    return BadRequest(new { success = false, message = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant." });
        }
        [HttpGet]
        public IActionResult GetQuotationSummary()
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { message = "Invalid tenant.", success = false });

            try
            {
                var query = dbContext.Qry60104quotationViewMasters.AsQueryable();

                // Group summary
                var summary = new
                {
                    Total = query.Count(),

                    // Status-based counts
                    Submitted = query.Count(q => q.IsSubmitted == true),
                    ToBeSubmitted = query.Count(q => q.IsSubmitted != true),

                    Verified = query.Count(q => q.IsVerified == true),
                    ToBeVerified = query.Count(q => q.IsSubmitted == true && q.IsVerified != true),

                    Approved = query.Count(q => q.IsApproved == true),
                    ToBeApproved = query.Count(q => q.IsVerified == true && q.IsApproved != true),

                    // Optional: custom status (from QuoteStatus column if you’re using it)
                    Draft = query.Count(q => q.QuoteStatus == "Draft"),
                    Rejected = query.Count(q => q.QuoteStatus == "Rejected")
                };

                return Ok(new { success = true, data = summary });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        public IActionResult GetSalesOrderSummary()
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { message = "Invalid tenant.", success = false });

            try
            {
                var query = dbContext.Qry60204salesOrderViewMasters.AsQueryable();

                var summary = new
                {
                    Total = query.Count(),

                    // Verification status
                    Verified = query.Count(x => x.IsVerified == true),
                    ToBeVerified = query.Count(x => x.IsVerified != true),

                    // Approval status
                    Approved = query.Count(x => x.IsApproved == true),
                    ToBeApproved = query.Count(x => x.IsVerified == true && x.IsApproved != true),

                    // Posting status
                    Posted = query.Count(x => x.OrderStatus == "Posted"),
                    ToBePosted = query.Count(x => x.IsVerified == true && x.IsApproved == true && x.OrderStatus != "Posted")
                };

                return Ok(new { success = true, data = summary });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetRFQSummary()
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { message = "Invalid tenant.", success = false });

            try
            {
                var query = dbContext.Qry60704rfqviewMasters.AsQueryable();

                // Build summary counts (customize statuses based on your business rules)
                var summary = new
                {
                    ToBeQuoted = await query.CountAsync(x => x.IsQuoted != true), // not quoted yet
                    Quoted = await query.CountAsync(x => x.IsQuoted == true && x.IsWon != true), // quoted but not won
                    Won = await query.CountAsync(x => x.IsWon == true), // RFQ won
                    Lost = await query.CountAsync(x => x.IsQuoted == true && x.IsWon != true && x.ReasonWon != null) // lost or rejected
                };

                // Optional: get recent notifications (last 10 RFQs)
                var notifications = await query
                    .OrderByDescending(x => x.AddedOn)
                    .Take(10)
                    .Select(x => new
                    {
                        VoucherNumber = x.Rfqno,
                        VoucherType = "RFQ",
                        Status = x.IsWon == true ? "Won"
                               : x.IsQuoted == true ? "Quoted"
                               : "To Be Quoted",
                        User = x.PreparedBy ?? "System",
                        Amount = x.TotalAfterDiscount,
                        CreatedOn = x.AddedOn
                    })
                    .ToListAsync();

                return Ok(new { success = true, Summary = summary, Notifications = notifications });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }


    public class MarkAsSeenRequest
    {
        public string AlertCode { get; set; }
    }
   
}


