using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System.Linq;

namespace QDWEB.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SalaryPayableController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<SalaryPayableController> _logger;

        public SalaryPayableController(ILogger<SalaryPayableController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetSalarybydate()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var company = dbContext.Tbl901CompanyDetails
                    .FirstOrDefault();
                    var vouchers = dbContext.Qry20196SalaryPayableByDateReports.Select(v => new
                    {
                        v.EmployeeNo,
                        v.EmployeeName,
                        v.NationalId,
                        v.ReferenceNo,
                        v.MonthOf,
                        v.PayableAmount,
                        v.Balance,
                        v.ConvertedPaid,
                        v.ConvertedPayableAmount,
                        v.ConvertedBalance,
                        company.CurrencyImage
                    }).ToList();

                    return Json(vouchers);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetSalarybydate: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
    }
}













