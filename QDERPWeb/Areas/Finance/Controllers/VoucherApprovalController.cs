using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;

namespace QDWEB.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class VoucherApprovalController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<VoucherApprovalController> _logger;

        public VoucherApprovalController(ILogger<VoucherApprovalController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetVoucherApproval()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var data = dbContext.Qry20136VoucherMasterLists.Select(v => new
                    {
                        v.VoucherNo,
                        VoucherDate = v.VoucherDate.ToString("dd-MMM-yyyy"),
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
                        VoucherEffectiveDate = v.VoucherEffectiveDate.HasValue
                            ? v.VoucherEffectiveDate.Value.ToString("dd-MMM-yyyy")
                            : string.Empty,
                        v.VoucherModifiedBy,
                        v.VoucherModifiedOn,
                        v.DebitAmount,
                        v.CreditAmount,
                        v.AuditVerifiedBy,
                        v.AuditVerifiedOn,
                        v.IsAuditVerified
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
        public IActionResult GetVoucherApprovals(DateTime? startDate, DateTime? endDate)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var vouchers = dbContext.Qry20136VoucherMasterLists.AsQueryable();

                    if (startDate.HasValue && endDate.HasValue)
                    {
                        vouchers = vouchers.Where(v => v.VoucherDate >= startDate && v.VoucherDate <= endDate);
                    }

                    return Ok(vouchers.ToList());
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
    }
}
