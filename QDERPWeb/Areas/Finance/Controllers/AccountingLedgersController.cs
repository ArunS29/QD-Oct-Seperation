using QD.ERP.Web.DAL.Entities;
using QDERPWeb.Models;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
//using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using DevExpress.CodeParser;

namespace QD.ERP.Web.Areas.Finance.Controllers
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
            try {
                var qryListOfAccountlists = _context.Qry201ListOfAccounts.Where(p => p.AccountId != null).Select(i => new
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
            catch (Exception ex)
            {
                return StatusCode(500,$"{ ex.Message}");
            }
            }
        [HttpGet]
        public async Task<ActionResult> GetVouchers(string accountId, string frmDate, string toDate)
        {
            try
            {
                if (!DateTime.TryParseExact(frmDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime from))
                {
                    return BadRequest("Invalid from date format. Use MM/dd/yyyy.");
                }

                if (!DateTime.TryParseExact(toDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime to))
                {
                    return BadRequest("Invalid to date format. Use MM/dd/yyyy.");
                }

                // ✅ No need to re-parse with DateTime.Parse()

                //ERPMasterWtDataContextProcedures _procedures = new ERPMasterWtDataContextProcedures(_context);
                //var ledgerData = await _procedures.StProAccountLedgerAsync(accountId, from, to);
                var ledgerData = await _context.AccountLedgers
       .FromSqlRaw("EXEC StProAccountLedger @p0, @p1, @p2", accountId, from, to)
       .ToListAsync();

                return Json(ledgerData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        //    [HttpGet]
        //    public async Task<ActionResult> GetVouchers(string accountId, string frmDate, string toDate)
        //    {
        //        try
        //        {
        //            var from = DateTime.Parse(frmDate);
        //            var to= DateTime.UtcNow;
        //            //var to = DateTime.Parse(toDate);
        //            ERPMasterWtDataContextProcedures _procedures = new ERPMasterWtDataContextProcedures(_context);
        //            //var ledgerData = await _procedures.StProAccountLedgerAsync(accountId, from, to);
        //            var ledgerData = await _context.StProAccountLedgerResults
        //.FromSqlRaw("EXEC StProAccountLedger @p0, @p1, @p2", accountId, from, to)
        //.ToListAsync();


        //            // Return the data in a format suitable for DevExtreme DataGrid
        //            // return Json(DataSourceLoader.Load(ledgerData));


        //            //var stProAccountLedgerList = await _contextProcedure.StProAccountLedgerAsync("L00567", from,to);
        //            return Json(ledgerData);
        //        }
        //        catch (Exception ex)
        //        {
        //            throw ex;

        //        }
        //    }

    }
}