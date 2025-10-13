using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QD.ERP.Shared.DAL.Entities;
using QD.ERP.Shared.Service;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QD.ERP.Finance.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ExpensesClaimController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<ExpensesClaimController> _logger;

        public ExpensesClaimController(ILogger<ExpensesClaimController> logger, TenantDbContextHelper tenantDbContextHelper)
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
                    var data = dbContext.Qry20121ExpenseClaimForms.Select(e => new
                    {
                        e.ClaimRefNo,
                        ClaimDate = e.ClaimDate.HasValue
                            ? e.ClaimDate.Value.ToString("dd-MMM-yyyy")
                            : string.Empty,
                        e.ClaimerName,
                        e.PaymentVoucherNo,
                        BillDate = e.BillDate.HasValue
                            ? e.BillDate.Value.ToString("dd-MMM-yyyy")
                            : string.Empty,
                        e.BillRefNo,
                        e.ExpenseDescription,
                        e.ClaimedAmount,
                        e.ApprovedAmount,
                        e.CostCenterCode,
                        e.AccountHead,
                        e.IsTaxIncluded,
                        e.Discount,
                        e.TaxAmount,
                        e.RoundOff,
                        e.SupplierName,
                        e.SupplierVatno,
                        e.EmployeeNo,
                        e.EmployeeName,
                        e.PropertyNo,
                        e.PropertyDescription,
                        e.CostAllocationUnit,
                        e.PurchaserName
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
                var vouchers = dbContext.Qry20121ExpenseClaimForms.AsQueryable();

                if (startDate.HasValue && endDate.HasValue)
                {
                    vouchers = vouchers.Where(v => v.ClaimDate >= startDate && v.ClaimDate <= endDate);
                }

                return Ok(vouchers.ToList());
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
    }
}


