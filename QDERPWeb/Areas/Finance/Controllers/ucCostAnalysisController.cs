//using Microsoft.AspNetCore.Mvc;
//using QD.ERP.Web.DAL.Entities;
//using System;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Identity.Client;

//namespace QD.ERP.Web.Areas.Finance.Controllers
//{
//    [Route("api/[controller]/[action]")]
//    [ApiController]
//    public class UCCostAnalysisController : ControllerBase
//    {
//        private readonly ERPMasterWtDataContext _context;

//        // Inject the ERPMasterWtDataContext in the constructor
//        public UCCostAnalysisController(ERPMasterWtDataContext context)
//        {
//            _context = context;
//        }

//        // Action to get account groups for the SelectBox
//        public async Task<ActionResult> GetUser()
//        {
//            var users = await _context.Qry20108ChartOfCostCenters.ToListAsync();
//            return new JsonResult(users); // This will return the account groups list as JSON.
//        }

//        public async Task<IActionResult> GetTrialBalance(DateTime? startDate, DateTime? endDate, string accountGroup)
//        {
//            try
//            {
//                // Fetch data directly from the Qry20106CostAnalyses table
//                var result = await _context.Qry20106CostAnalyses.ToListAsync();

//                // Apply filtering based on AccountGroup, startDate, and endDate
//                if (!string.IsNullOrEmpty(accountGroup))
//                {
//                    result = result.Where(x => x.CostAllocationUnit == accountGroup).ToList();
//                }

//                if (startDate.HasValue)
//                {
//                    result = result.Where(x => x.VoucherDate >= startDate.Value).ToList();
//                }

//                if (endDate.HasValue)
//                {
//                    result = result.Where(x => x.VoucherDate <= endDate.Value).ToList();
//                }

//                // Map the results to the format needed for the PivotGrid
//                var pivotGridData = result.Select(item => new
//                {
//                    item.CostAllocationMasterGroup,
//                    item.CostAllocationGroup,
//                    item.CostAllocationUnit,
//                    item.CostAmount,
//                    item.Income,
//                    item.Expenses,
//                    item.VoucherDate,

//                    item.CostAllocationId,
//                    item.VoucherEntryId,
//                    item.CostAllocationUnitId,
//                    item.CostAllocDrCr,
//                    item.AmountAllocated,
//                    item.EffectiveDate,
//                    item.CostAllocRemarks,
//                    item.IsDisabled,
//                    item.AccountHead,
//                    item.AccountGroup,
//                    item.MasterGroup,
//                    item.Pl,
//                    item.AccountId,
//                    item.VoucherNo,
//                    item.VoucherType,
//                    item.VoucherTypeAndNo,
//                    item.CostCenterIncharge,
//                    item.EntryNarration,
//                    item.SysRemarks,
//                    item.VoucherMonth,
//                    item.VoucherYear,
//                    item.EffectiveMonth,
//                    item.EffectiveYear,
//                    item.AllocationEffectiveDate,
//                    item.AllocationEffectiveMonth,
//                    item.AllocationEffectiveYear,
//                    item.ProjectMasterCode,
//                    item.BranchCode,
//                    item.BranchName,
//                    item.VoucherNarration,
//                    item.VoucherRefNo


//                }).ToList();

//                return Ok(pivotGridData);  // Return the data as JSON
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
//            }
//        }

    //}
//}
