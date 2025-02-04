//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using QD.ERP.Web.DAL.Entities;

//namespace QD.ERP.Web.Areas.Finance.Controllers
//{
//    [Route("api/[controller]/[action]")]
//    [ApiController]
//    public class ucCostAnalysisController : Controller
//    {
//        private readonly ERPMasterWtDataContext _context;

//        public ucCostAnalysisController(ERPMasterWtDataContext context)
//        {
//            _context = context;
//        }
//        // Action to get account groups for the SelectBox
//        public async Task<ActionResult> GetUser()
//        {
//            var users = await _context.Qry20108ChartOfCostCenters.ToListAsync();
//            return new JsonResult(users); // This will return the account groups list as JSON.
//        }

//        public async Task<IActionResult> GetTrialBalance(DateTime? startDate, DateTime? endDate, string CostAllocationGroup)
//        {
//            try
//            {
//                // Fetch data directly from the Qry20106CostAnalysis table
//                var result = await _context.Qry20106CostAnalyses
//                    .Where(x => (!startDate.HasValue || x.VoucherDate >= startDate.Value) &&
//                                (!endDate.HasValue || x.VoucherDate <= endDate.Value))
//                    .ToListAsync();

//                // Filter by AccountGroup if provided
//                if (!string.IsNullOrEmpty(CostAllocationGroup))
//                {
//                    result = result.Where(x => x.CostAllocationGroup == CostAllocationGroup).ToList();
//                }

//                // Map the results to the desired format for the PivotGrid
//                var pivotGridData = result.Select(i => new
//                {
//                    i.CostAllocationMasterGroup,
//                    i.CostAllocationGroup,
//                    i.Income,
//                    i.Expenses,
//                    i.CostAmount,
//                    i.VoucherDate
                  
//                }).ToList();

//                return Ok(pivotGridData);  // Return the data as JSON
//            }
//            catch (Exception ex)
//            {
//                // Log the exception (you can use any logging framework here like Serilog, NLog, etc.)
//                // Example: _logger.LogError(ex, "An error occurred while fetching the trial balance.");

//                // Return a meaningful error message
//                return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
//            }
//        }

//    }
//}
