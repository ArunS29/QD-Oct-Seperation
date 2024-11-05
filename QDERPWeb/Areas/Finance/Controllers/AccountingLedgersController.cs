using QDERPWeb.DAL.Entities;
using QDERPWeb.Models;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;

namespace QDERPWeb.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    //[Area("Finance")]
    //[ApiController]
    public class AccountingLedgersController : Controller
    {
        private ERPMasterWtDataContext _context;
        //private ERPMasterWtDataContextProcedures _contextProcedure;
        public AccountingLedgersController(ERPMasterWtDataContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult> Get(DataSourceLoadOptions loadOptions)
        {             
            var qryListOfAccountlists = _context.Qry201ListOfAccounts.Select(i => new
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
            //var stProAccountLedgerList = await _contextProcedure.StProAccountLedgerAsync("L00567", from,to);
            //return Json(stProAccountLedgerList);
        }
        [HttpGet]
        public async Task<ActionResult> GetLedgerAccounts(DataSourceLoadOptions loadOptions)
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
        [HttpGet]
        public async Task<ActionResult> GetVouchers(string accountId, string frmDate, string toDate)
        {
            var from = DateTime.Parse(frmDate);
            var to = DateTime.Parse(toDate);
            ERPMasterWtDataContextProcedures _procedures = new ERPMasterWtDataContextProcedures(_context);
            var ledgerData =  await _procedures.StProAccountLedgerAsync(accountId, from, to);
            

            // Return the data in a format suitable for DevExtreme DataGrid
           // return Json(DataSourceLoader.Load(ledgerData));


            //var stProAccountLedgerList = await _contextProcedure.StProAccountLedgerAsync("L00567", from,to);
            return Json(ledgerData);
        }
        
    }
}
