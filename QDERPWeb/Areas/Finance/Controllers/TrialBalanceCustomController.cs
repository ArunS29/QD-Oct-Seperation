using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QDWEB.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TrialBalanceCustomController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<TrialBalanceCustomController> _logger;

        public TrialBalanceCustomController(ILogger<TrialBalanceCustomController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult> GetAssetView()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    ERPMasterWtDataContextProcedures _procedures = new ERPMasterWtDataContextProcedures(dbContext);

                    DateTime? startDate = new DateTime(2024, 1, 1);
                    DateTime? endDate = DateTime.Today;
                    bool? includeInactive = true;
                    var outputParam = new OutputParameter<int>();
                    CancellationToken cancellationToken = CancellationToken.None;

                    var ledgerData = await _procedures.sp20101TrialBalanceReportAsync(
                        startDate,
                        endDate,
                        includeInactive,
                        outputParam,
                        cancellationToken
                    );

                    if (ledgerData == null || !ledgerData.Any())
                    {
                        return NotFound(new { message = "No data found." });
                    }

                    var result = ledgerData.Select(x => new
                    {
                        x.AccountHead,
                        x.AccountName,
                        x.MasterGroup,
                        x.Op_Bal,
                        x.TransDebit,
                        x.AccountGroup,
                        x.TransCredit,
                        x.AccountHeadArabic,
                        x.AccountGroupAr,
                        x.MasterGroupAr,
                        x.ChartOfAccountsOrder,
                        x.AccountGroupOrderNo,
                        x.AccountSubGroup,
                        x.SubGroupName,
                        x.SubGroupNameAr
                    }).ToList();

                    return Json(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetAssetView: {ex.Message}");
                    return BadRequest(new { message = "An error occurred while fetching data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
    }
}

















