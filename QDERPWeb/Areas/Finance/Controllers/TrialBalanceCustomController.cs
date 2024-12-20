using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.DAL.Entities;

namespace QDWEB.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    public class TrialBalanceCustomController : Controller
    {
        private ERPMasterWtDataContext _context;

        public TrialBalanceCustomController(ERPMasterWtDataContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult> GetAssetView()
        {
            try
            {
                ERPMasterWtDataContextProcedures _procedures = new ERPMasterWtDataContextProcedures(_context);

               
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
               
                return BadRequest(new { message = "An error occurred while fetching data.", error = ex.Message });
            }
        }

    }
}
