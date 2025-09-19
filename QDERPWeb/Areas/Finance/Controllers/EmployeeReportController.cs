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
    public class EmployeeReportController : ControllerBase
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<EmployeeReportController> _logger;

        public EmployeeReportController(
            ILogger<EmployeeReportController> logger,
            TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        /// <summary>
        /// Get all employee allocation entries from Qry20182EmpAllocationWtLedger.
        /// Optional filtering by EmployeeNo, Date range, CostCentreCode, etc.
        /// </summary>
        public async Task<IActionResult> GetEmployeeAllocations([FromQuery] string accountHead, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var query = dbContext.Qry20182EmpAllocationWtLedgers.AsQueryable();

                    if (!string.IsNullOrEmpty(accountHead))
                        query = query.Where(x => x.AccountHead == accountHead);

                    if (fromDate.HasValue)
                        query = query.Where(x => x.VoucherDate >= fromDate.Value);

                    if (toDate.HasValue)
                        query = query.Where(x => x.VoucherDate <= toDate.Value);

                    // ✅ Here's the missing part:
                    var rawData = await query.ToListAsync();

                    var result = rawData.Select(item => new
                    {
                        item.VoucherNo,
                        item.VoucherEntryId,
                        item.EmployeeNo,
                        item.EmpAllocDrCr,
                        item.AmountAllocated,
                        item.VoucherRefNo,
                        item.AccountHead,
                        item.EmployeeName,
                        VoucherDate = item.VoucherDate?.ToString("dd-MMM-yyyy"),
                        EffectiveDate = item.EffectiveDate?.ToString("dd-MMM-yyyy"),
                        item.CategoryTitle,
                        item.NationalityEn,
                        item.VisaPositionTitle,
                        item.WorkSite,
                        item.SponsorName,
                        item.NationalId,
                        item.IsDiscontinued,
                        item.EmployeeGroup,
                        item.JobTitle,
                        item.CostCentreCode,
                        item.CostCentreTitle,
                        item.DepartmentCode,
                        item.Department,
                        item.EmployeeReferenceId,
                        item.BankAccountNo,
                        item.BankName,
                        item.VoucherType,
                        item.AccountHeadName,
                        item.VoucherNarration,
                        item.BillRemarks
                    }).ToList();

                    return Ok(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while fetching employee allocations.");
                    return StatusCode(500, new { message = "Error fetching data.", error = ex.Message });
                }
            }

            return BadRequest("Could not get tenant context.");
        }
        public async Task<IActionResult> GetPropertyAllocations([FromQuery] string accountId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var query = dbContext.Qry201126propertyAllocationFullWtDetails.AsQueryable();

                    if (!string.IsNullOrEmpty(accountId))
                        query = query.Where(x => x.AccountHead == accountId);

                    if (fromDate.HasValue)
                        query = query.Where(x => x.VoucherDate >= fromDate.Value);

                    if (toDate.HasValue)
                        query = query.Where(x => x.VoucherDate <= toDate.Value);

                    // ✅ Here's the missing part:
                    var rawData = await query.ToListAsync();

                    var result = rawData.Select(item => new
                    {
                        item.VoucherNo,
                        item.VoucherEntryNo,
                        item.PropertyNo,
                        item.PropertyDescription,
                        item.AmountAllocated,
                        item.VoucherRefNo,
                        item.AccountHead,
                        item.AccountHeadName,
                        VoucherDate = item.VoucherDate?.ToString("dd-MMM-yyyy"),
                       
                        item.EntryNarration,
                        item.AccountGroup,
                        item.MasterGroup,
                        item.PlateNo,
                        item.DoorNo,
                        item.ChassisNo,
                        item.PropertyCategoryName,
                        item.PropertyGroup,
                        item.PropertyType,
                        item.ExpenseAmount,
                        item.RevenueAmount,
                        item.CostAndRevenueClubbed,
                        item.CostCenterCode,
                        item.CostCenterUnit,
                       
                        item.VoucherType,
                      
                        item.VoucherNarration
                    }).ToList();

                    return Ok(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while fetching employee allocations.");
                    return StatusCode(500, new { message = "Error fetching data.", error = ex.Message });
                }
            }

            return BadRequest("Could not get tenant context.");
        }
    }
}
