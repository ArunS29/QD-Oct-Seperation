using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DepreciationMasterController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<DepreciationMasterController> _logger;

        public DepreciationMasterController(ILogger<DepreciationMasterController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetExpenseClaims()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var data = dbContext.Qry201205depreciationMasterViews.Select(e => new
                    {
                        e.JournalVoucherNo,
                        e.DepreciationDocNo,
                        e.DeprStartDate,
                        e.DeprEndDate,
                        e.NoOfAssets,
                        e.AssetOpeningBalance,
                        e.AssetTotalDebitTrans,
                        Postedon = e.PostedOn.HasValue
                            ? e.PostedOn.Value.ToString("dd-MMM-yyyy")
                            : string.Empty,
                        e.AssetTotalCreditTrans,
                        e.AssetClosingBalance,
                        e.AccumulatedOpeningBalance,
                        e.TotalDepreciationAmount,
                        e.AccumulatedTotalBalance,
                        e.TotalBookValue
                    }).ToList();

                    return Json(data);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetExpenseClaims: {ex.Message}");
                    return BadRequest(new { message = "An error occurred while fetching data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public IActionResult GetExpensesClaims(DateTime? startDate, DateTime? endDate)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var vouchers = dbContext.Qry201205depreciationMasterViews.AsQueryable();

                if (startDate.HasValue && endDate.HasValue)
                {
                    vouchers = vouchers.Where(v => v.DeprStartDate >= startDate && v.DeprEndDate <= endDate);
                }

                return Ok(vouchers.ToList());
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
    }
}

