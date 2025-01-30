using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.DAL.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TrialBalanceController : ControllerBase
    {
        private readonly ERPMasterWtDataContext _context;

        // Inject the ERPMasterWtDataContext in the constructor
        public TrialBalanceController(ERPMasterWtDataContext context)
        {
            _context = context;
        }

        // Action to get account groups for the SelectBox
        public async Task<ActionResult> GetUser()
        {
            var users = await _context.Tbl201AccountGroups.ToListAsync();
            return new JsonResult(users); // This will return the account groups list as JSON.
        }

        public async Task<IActionResult> GetTrialBalance(DateTime? startDate, DateTime? endDate, string accountGroup)
        {
            try
            {
                bool? isUseEffectiveDate = false;
                var returnValue = new OutputParameter<int>();
                ERPMasterWtDataContextProcedures _procedures = new ERPMasterWtDataContextProcedures(_context);

                // Fetch data from the database using stored procedure
                var result = await _procedures.StProTrialBalanceAsync(startDate, endDate, isUseEffectiveDate, returnValue);

                // Filter by AccountGroup if provided, or by date range if not
                if (!string.IsNullOrEmpty(accountGroup))
                {
                    result = result.Where(x => x.AccountGroup == accountGroup).ToList();
                }
                else
                {
                    // Filter by date range if AccountGroup is not provided
                    if (startDate.HasValue)
                    {
                        result = result.Where(x => x.VoucherDate >= startDate.Value).ToList();
                    }
                    if (endDate.HasValue)
                    {
                        result = result.Where(x => x.VoucherDate <= endDate.Value).ToList();
                    }
                }

                // Map the results to the desired format for the PivotGrid
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

                return Ok(pivotGridData);  // Return the data as JSON
            }
            catch (Exception ex)
            {
                // Log the exception (you can use any logging framework here like Serilog, NLog, etc.)
                // Example: _logger.LogError(ex, "An error occurred while fetching the trial balance.");

                // Return a meaningful error message
                return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
            }
        }

    }
}
