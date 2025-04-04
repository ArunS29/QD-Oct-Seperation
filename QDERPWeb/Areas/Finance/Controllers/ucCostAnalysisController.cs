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
    public class UCCostAnalysisController : ControllerBase
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<UCCostAnalysisController> _logger;

        public UCCostAnalysisController(ILogger<UCCostAnalysisController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult> GetUser()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var users = await dbContext.Qry20108ChartOfCostCenters.ToListAsync();
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
                    var result = await dbContext.Qry20106CostAnalyses.ToListAsync();

                    if (!string.IsNullOrEmpty(accountGroup))
                    {
                        result = result.Where(x => x.CostAllocationUnit == accountGroup).ToList();
                    }

                    if (startDate.HasValue)
                    {
                        result = result.Where(x => x.VoucherDate >= startDate.Value).ToList();
                    }

                    if (endDate.HasValue)
                    {
                        result = result.Where(x => x.VoucherDate <= endDate.Value).ToList();
                    }

                    var pivotGridData = result.Select(item => new
                    {
                        item.CostAllocationMasterGroup,
                        item.CostAllocationGroup,
                        item.CostAllocationUnit,
                        item.CostAmount,
                        item.Income,
                        item.Expenses,

                        // ✅ Format Date Fields to "MMM-yyyy"
                        VoucherDate = item.VoucherDate != null ? item.VoucherDate.Value.ToString("MMM-yyyy") : null,
                        EffectiveDate = item.EffectiveDate != null ? item.EffectiveDate.Value.ToString("MMM-yyyy") : null,
                        AllocationEffectiveDate = item.AllocationEffectiveDate != null ? item.AllocationEffectiveDate.Value.ToString("MMM-yyyy") : null,

                        item.CostAllocationId,
                        item.VoucherEntryId,
                        item.CostAllocationUnitId,
                        item.CostAllocDrCr,
                        item.AmountAllocated,
                        item.CostAllocRemarks,
                        item.IsDisabled,
                        item.AccountHead,
                        item.AccountGroup,
                        item.MasterGroup,
                        item.Pl,
                        item.AccountId,
                        item.VoucherNo,
                        item.VoucherType,
                        item.VoucherTypeAndNo,
                        item.CostCenterIncharge,
                        item.EntryNarration,
                        item.SysRemarks,
                        item.VoucherMonth,
                        item.VoucherYear,
                        item.EffectiveMonth,
                        item.EffectiveYear,
                        item.AllocationEffectiveMonth,
                        item.AllocationEffectiveYear,
                        item.ProjectMasterCode,
                        item.BranchCode,
                        item.BranchName,
                        item.VoucherNarration,
                        item.VoucherRefNo
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







