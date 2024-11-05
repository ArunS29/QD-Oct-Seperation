using QDERPWeb.DAL.Entities;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace QDERPWeb.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    //[ApiController]
    public class QryListOfAccountController : Controller
    {
        private ERPMasterWtDataContext _context;
        public QryListOfAccountController(ERPMasterWtDataContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions) 
        {
            var qryListOfAccountlists = _context.Qry201ListOfAccounts.Where(p=>p.AccountId!=null).Select(i => new
            {
                i.MasterGroupId,
                i.MasterGroup,
                i.AccountGroup,
                i.AccountGroupId,
                i.AccountId,
                i.AccountHead,
                i.AccountHeadArabic,
                i.ReferenceNo,
                i.IsLedgerObselete
            });
            return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
        }
    }
}
