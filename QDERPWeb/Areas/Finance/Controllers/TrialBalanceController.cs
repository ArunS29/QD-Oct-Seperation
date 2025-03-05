using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    public class TrialBalanceController : ControllerBase
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<TrialBalanceController> _logger;

        public TrialBalanceController(ILogger<TrialBalanceController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult> GetUser()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var users = await dbContext.Tbl201AccountGroups.ToListAsync();
                return new JsonResult(users);
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetTrialBalance(DateTime? startDate, DateTime? endDate, string accountGroup)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    bool? isUseEffectiveDate = false;
                    var returnValue = new OutputParameter<int>();
                    ERPMasterWtDataContextProcedures _procedures = new ERPMasterWtDataContextProcedures(dbContext);

                    var result = await _procedures.StProTrialBalanceAsync(startDate, endDate, isUseEffectiveDate, returnValue);

                    if (!string.IsNullOrEmpty(accountGroup))
                    {
                        result = result.Where(x => x.AccountGroup == accountGroup).ToList();
                    }
                    else
                    {
                        if (startDate.HasValue)
                        {
                            result = result.Where(x => x.VoucherDate >= startDate.Value).ToList();
                        }
                        if (endDate.HasValue)
                        {
                            result = result.Where(x => x.VoucherDate <= endDate.Value).ToList();
                        }
                    }

                    var pivotGridData = result.Select(item => new
                    {
                        item.VoucherNo,
                        item.VoucherDate,
                        item.AccountHead,
                        item.AccountHeadName,
                        item.DrAmount,
                        item.CrAmount,
                        item.VoucherAmountFormatted,
                        item.AccountGroup,
                        item.MonthYear
                    }).ToList();

                    return Ok(pivotGridData);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetTrialBalance: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
    }
}








