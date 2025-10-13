using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QD.ERP.Shared.Models;
using QD.ERP.Shared.DAL.Entities;
using QD.ERP.Shared.Service;
using QD.ERP.Shared.Services.Logging;
using QDERPWeb.Models;


namespace QDWEB.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    public class VoucherApprovalController : Controller
    {

        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<VoucherApprovalController> _logger;
        private readonly IUserActionLogger _userActionLogger;
        private readonly FcmService _fcmService;

        public VoucherApprovalController(ILogger<VoucherApprovalController> logger, IUserActionLogger userActionLogger, TenantDbContextHelper tenantDbContextHelper, FcmService fcmService)
        {
            _userActionLogger = userActionLogger;
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
            _fcmService = fcmService;
        }
        [HttpGet]
        public IActionResult GetVoucherApproval(string voucherTypes)
        {
            
            
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var query = dbContext.Qry20136VoucherMasterLists.AsQueryable();

                    if (!string.IsNullOrEmpty(voucherTypes) && voucherTypes != "all")
                    {
                        var typesList = voucherTypes.Split(',').ToList();
                        query = query.Where(v => typesList.Contains(v.VoucherType));
                    }
                 //   var companyNameShort = HttpContext.Session.GetString("TenantName");

                    var company = dbContext.Tbl901CompanyDetails
                    .FirstOrDefault();

                    var data = query.Select(v => new
                    {
                        v.VoucherNo,
                        VoucherDate = v.VoucherDate.ToString("dd-MMM-yyyy"),
                        v.VoucherEffectiveDate,
                        v.VoucherRefNo,
                        v.VoucherNarration,
                        v.VoucherEnteredBy,
                        v.VoucherEnteredOn,
                        v.IsVerified,
                        v.VoucherVerifiedBy,
                        v.VoucherVerifiedOn,
                        v.IsApproved,
                        v.VoucherApprovedBy,
                        v.VoucherApprovedOn,
                        v.VoucherType,
                        v.DebitAmount,
                        v.CreditAmount,
                        company.CurrencyImage
                    }).ToList();

                    return Json(data);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetVoucherApproval: {ex.Message}");
                    return BadRequest(new { message = "An error occurred while fetching data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }


        [HttpGet]
        public IActionResult GetVoucherApprovals(DateTime? startDate, DateTime? endDate, string voucherTypes)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var vouchers = dbContext.Qry20136VoucherMasterLists.AsQueryable();

                    if (!string.IsNullOrEmpty(voucherTypes) && voucherTypes != "all" && startDate.HasValue && endDate.HasValue)
                    {
                        var typesList = voucherTypes.Split(',').ToList();
                        vouchers = vouchers.Where(v => v.VoucherDate.Date >= startDate && v.VoucherDate.Date <= endDate && typesList.Contains(v.VoucherType));
                    }
                    //   var companyNameShort = HttpContext.Session.GetString("TenantName");

                    var company = dbContext.Tbl901CompanyDetails
                    .FirstOrDefault();
                    string CurrencyImage = company.CurrencyImage;


                    var voucherslist = vouchers
    .Select(v => new
    {
        v.VoucherNo,
        VoucherDate = v.VoucherDate.ToString("dd-MMM-yyyy"),
        v.VoucherEffectiveDate,
        v.VoucherRefNo,
        v.VoucherNarration,
        v.VoucherEnteredBy,
        v.VoucherEnteredOn,
        v.IsVerified,
        v.VoucherVerifiedBy,
        v.VoucherVerifiedOn,
        v.IsApproved,
        v.VoucherApprovedBy,
        v.VoucherApprovedOn,
        v.VoucherType,
        v.DebitAmount,
        v.CreditAmount,
        v.ConvertedCrAmount,
        v.ConvertedDrAmount,
        v.IsAuditVerified,
        v.AuditVerifiedBy,
        v.AuditVerifiedOn,
        company.CurrencyImage

    });

                    return Ok(voucherslist.ToList());
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetVoucherApprovals: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public IActionResult GetAssetsSummary()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var assetSummary = dbContext.Qry20149AssetsRegisterViews
                        .GroupBy(a => 1)
                        .Select(g => new
                        {
                            NoOfAssets = g.Count(),
                            CurrentAssetValue = g.Sum(a => a.NetBookValue) ?? 0
                        })
                        .FirstOrDefault();

                    return Ok(assetSummary);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetAssetsSummary: {ex.Message}");
                    return StatusCode(500, new { Error = "Failed to fetch asset summary.", Details = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateVoucherStatus([FromBody] VoucherUpdateRequest request)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }
            var UserName = HttpContext.Session.GetString("UserName");
            var UserId = HttpContext.Session.GetString("UserId");
            var TenantName = HttpContext.Session.GetString("TenantName");

            var vouchers = dbContext.Tbl201VoucherMasters
                .Where(v => request.VoucherNos.Contains(v.VoucherNo))
                .ToList();

            if (!vouchers.Any())
            {
                return Json(new { success = false, message = "No valid vouchers found." });
            }

            foreach (var voucher in vouchers)
            {
                switch (request.ActionType.ToLower())
                {
                    case "verify":
                        if (!voucher.IsVerified.GetValueOrDefault(false))
                        {
                            voucher.VoucherVerifiedBy = UserName;
                            voucher.VoucherVerifiedOn = DateTime.Now;
                            voucher.IsVerified = true;
                            await _userActionLogger.LogAsync(
                        module: "Finance > Voucher Verification",
                        actionDetail: $"Verified: {voucher.VoucherNo}",
                        documentNo: voucher.VoucherNo
                    );
                        }
                        break;

                    case "approve":
                        if (!voucher.IsApproved.GetValueOrDefault(false))
                        {
                            voucher.VoucherApprovedBy = UserName;
                            voucher.VoucherApprovedOn = DateTime.Now;
                            voucher.IsApproved = true;
                            await _userActionLogger.LogAsync(
                       module: "Finance > Voucher Verification",
                       actionDetail: $"Approved: {voucher.VoucherNo}",
                       documentNo: voucher.VoucherNo
                   );
                        }
                        break;

                    case "unlock":
                        if (voucher.IsApproved.GetValueOrDefault(false))
                        {
                            voucher.VoucherApprovedBy = null;
                            voucher.VoucherApprovedOn = null;
                            voucher.IsApproved = false;
                            await _userActionLogger.LogAsync(
                        module: "Finance > Voucher Verification",
                        actionDetail: $"Unlocked: {voucher.VoucherNo}",
                        documentNo: voucher.VoucherNo
                    );
                        }
                        break;

                    default:
                        return Json(new { success = false, message = "Invalid action type." });
                }
            }

            dbContext.SaveChanges();

            string message = request.ActionType switch
            {
                "verify" => "Vouchers have been verified.",
                "approve" => "Vouchers have been approved.",
                "unlock" => "Vouchers have been unlocked.",
                _ => "Operation completed."
            };

             var notifyRequest = new NotificationRequest
             {
                 UserId = UserId, // or fetch from session/DB
                 VoucherName = vouchers[0].VoucherNo,
                 ActionType = request.ActionType,
                TenantName = TenantName 
            };

        await _fcmService.SendNotificationAsync(notifyRequest);

        return Json(new { success = true, message = message });
        
}

        [HttpPost]
        public async Task<IActionResult> VerifyVoucher(string voucherNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var UserName = HttpContext.Session.GetString("UserName");
                    var UserId = HttpContext.Session.GetString("UserId");
                    var TenantName = HttpContext.Session.GetString("TenantName");

                    if (string.IsNullOrEmpty(voucherNo))
                        return BadRequest("Invalid VoucherNo");

                    var voucher = dbContext.Tbl201VoucherMasters.FirstOrDefault(v => v.VoucherNo == voucherNo);
                    if (voucher == null)
                        return NotFound("Voucher not found");

                    voucher.VoucherVerifiedBy = UserName; // Replace with actual user
                    voucher.VoucherVerifiedOn = DateTime.Now;
                    voucher.IsVerified = true;

                    dbContext.SaveChanges();
                    await _userActionLogger.LogAsync(
                    module: "Finance > Voucher Verification",
                    actionDetail: $"Verified: {voucherNo}",
                    documentNo: voucherNo
                    );
                    var notifyRequest = new NotificationRequest
                                {
                                    UserId = UserId, // or fetch from session/DB
                                    VoucherName = voucher.VoucherNo,
                                    ActionType = "Verify Voucher",
                                    TenantName = TenantName 
                                };

                    await _fcmService.SendNotificationAsync(notifyRequest);

                    return Ok(new
                    {
                        Message = "Voucher verified successfully.",
                        VoucherVerifiedBy = UserName,  // Example, replace with actual data if needed
                                                       //VoucherVerifiedOn = voucher.VoucherApprovedOn.ToString("dd-MMM-yyyy")
                    });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { Message = ex.Message });
                }
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });

        }

        [HttpPost]
        public async Task<IActionResult> DeleteVouchersAsync([FromBody] VoucherUpdateRequest request)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            if (request.VoucherNos == null || !request.VoucherNos.Any())
            {
                return Json(new { success = false, message = "No voucher numbers provided." });
            }

            var voucherNos = request.VoucherNos;

            // Fetch related entries and masters
            var entries = dbContext.Tbl201VoucherEntries
                .Where(e => voucherNos.Contains(e.VoucherNo))
                .ToList();

            var masters = dbContext.Tbl201VoucherMasters
                .Where(m => voucherNos.Contains(m.VoucherNo))
                .ToList();

            if (!entries.Any() && !masters.Any())
            {
                return Json(new { success = false, message = "No matching vouchers found to delete." });
            }

            dbContext.Tbl201VoucherEntries.RemoveRange(entries);
            dbContext.Tbl201VoucherMasters.RemoveRange(masters);

            dbContext.SaveChanges();
            await _userActionLogger.LogAsync(
        module: "Finance > Voucher Verification",
        actionDetail: $"Deleted Vouchers: {request.VoucherNos}",
         documentNo: string.Join(", ", request.VoucherNos)
        );
            return Json(new { success = true, message = "Selected vouchers have been deleted successfully." });
        }
        [HttpPost]
        public async Task<IActionResult> UpdateAuditVerificationAsync(string voucherNo)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }
            var userName = HttpContext.Session.GetString("UserName");
            var now = DateTime.Now;

            var voucher = dbContext.Tbl201VoucherMasters.FirstOrDefault(v => v.VoucherNo == voucherNo);
            if (voucher == null) return NotFound();

            voucher.IsAuditVerified = true;
            voucher.AuditVerifiedBy = userName;
            voucher.AuditVerifiedOn = now;

            dbContext.SaveChanges();
            await _userActionLogger.LogAsync(
                    module: "Finance > Voucher Verification",
                    actionDetail: $"Audited: {voucherNo}",
                    documentNo: voucherNo
                    );
            return Ok();
        }

        [HttpGet]
        public IActionResult GetVoucherApprovalFiltered(string docType, string status)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { message = "Invalid tenant.", success = false });

            try
            {
                var query = dbContext.Qry20136VoucherMasterLists.AsQueryable();

                // Status column mapping
                var statusColumnMap = new Dictionary<string, Expression<Func<Qry20136VoucherMasterList, bool>>>()
        {
            { "ToBeVerified", v => v.IsVerified == false },
            { "ToBeApproved", v => v.IsApproved == false },
            { "ToBeAudited", v => v.IsAuditVerified == false }
        };

                // Apply voucher type filter
                if (!string.IsNullOrEmpty(docType))
                {
                    if (docType == "BankCashPayment")
                    {
                        query = query.Where(v => v.VoucherType == "Bank Payment" || v.VoucherType == "Cash Payment");
                    }
                    else if (docType == "BankCashReceipt")
                    {
                        query = query.Where(v => v.VoucherType == "Bank Receipts" || v.VoucherType == "Cash Receipts");
                    }
                    else if (docType == "SalesPurchase")
                    {
                        query = query.Where(v => v.VoucherType == "Sales" || v.VoucherType == "Purchases");
                    }
                    else
                    {
                        query = query.Where(v => v.VoucherType == docType);
                    }
                }

                // Apply status filter
                if (!string.IsNullOrEmpty(status) && statusColumnMap.ContainsKey(status))
                {
                    query = query.Where(statusColumnMap[status]);
                }

                // Get company details
                var company = dbContext.Tbl901CompanyDetails.FirstOrDefault();

                // Select and return data
                var data = query.Select(v => new
                {
                    v.VoucherNo,
                    VoucherDate = v.VoucherDate.ToString("dd-MMM-yyyy"),
                    v.VoucherEffectiveDate,
                    v.VoucherRefNo,
                    v.VoucherNarration,
                    v.VoucherEnteredBy,
                    v.VoucherEnteredOn,
                    v.IsVerified,
                    v.VoucherVerifiedBy,
                    v.VoucherVerifiedOn,
                    v.IsApproved,
                    v.VoucherApprovedBy,
                    v.VoucherApprovedOn,
                    v.VoucherType,
                    v.DebitAmount,
                    v.CreditAmount,
                    company.CurrencyImage
                }).ToList();

                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetVoucherApprovalFiltered: {ex.Message}");
                return BadRequest(new { message = "An error occurred while fetching filtered data.", error = ex.Message });
            }
        }

    }
}
