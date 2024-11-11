using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.DAL.Entities;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ChartOfAccountsController : Controller
    {
        private ERPMasterWtDataContext _context;
        public ChartOfAccountsController(ERPMasterWtDataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> GetChartOfAccounts()
        {

            return Json(_context.Qry20107ChartOfAccounts.ToList());
        }

    }
}
