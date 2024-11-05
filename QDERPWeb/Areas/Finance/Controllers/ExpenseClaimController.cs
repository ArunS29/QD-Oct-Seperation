using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QDERPWeb.DAL.Entities;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ExpenseClaimController : Controller
    {
        private ERPMasterWtDataContext _context;
        public ExpenseClaimController(ERPMasterWtDataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> GetExpenseClaim(byte ClaimerID, DateTime StartDate, DateTime EndDate, bool IfShowAll)
        {
            ERPMasterWtDataContextProcedures procedure = new ERPMasterWtDataContextProcedures(_context);
            var result = await procedure.sp20105ExpenseClaimViewAsync(ClaimerID, StartDate, EndDate, IfShowAll);
            return Json(result);
        }
        [HttpGet]
        public async Task<ActionResult> GetUser()
        {

            return Json(_context.TblUserMasters.ToList());
        }
    }
}
