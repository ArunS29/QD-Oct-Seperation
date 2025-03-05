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
    public class ReportsBySupplierController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<ReportsBySupplierController> _logger;

        public ReportsBySupplierController(ILogger<ReportsBySupplierController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAccountHead(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var AccountHeadData = dbContext.Qry20110SundryCreditors.Select(i => new
                    {
                        i.AccountId,
                        i.AccountHead
                    });
                    return Json(await DataSourceLoader.LoadAsync(AccountHeadData, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetAccountHead: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetSalesPerson(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var SalesPersonData = dbContext.Tbl20101SalesPersonMasters.Select(i => new
                    {
                        i.SalesPersonCode,
                        i.SalesPersonName,
                    });

                    return Json(await DataSourceLoader.LoadAsync(SalesPersonData, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetSalesPerson: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetCompanyBranch(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var SalesPersonData = dbContext.Tbl20115CompanyBranches.Select(i => new
                    {
                        i.BranchCode,
                        i.BranchName
                    });

                    return Json(await DataSourceLoader.LoadAsync(SalesPersonData, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetCompanyBranch: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public IActionResult GenerateReport([FromBody] string[] selectedIds)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    if (selectedIds == null || selectedIds.Length == 0)
                    {
                        return BadRequest("No records selected.");
                    }

                    var accountIdsLength = dbContext.Qry20110SundryCreditors
                        .Select(d => d.AccountId)
                        .Count();

                    string ids = (selectedIds.Length == accountIdsLength) ? "0" : string.Join(",", selectedIds);

                    return RedirectToPage("/Designer", new { reportName = "XtraReport1", selectedIds = ids });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GenerateReport: {ex.Message}");
                    return StatusCode(500, "Internal Server Error: " + ex.Message);
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
    }
}














