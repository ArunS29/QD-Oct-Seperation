using QD.ERP.Web.DAL.Entities;
using QDERPWeb.Models;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    //[ApiController]
    public class AccountingRegisterController : Controller
    {
        private ERPMasterWtDataContext _context;
        public AccountingRegisterController(ERPMasterWtDataContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult> GetVoucherTypes(DataSourceLoadOptions loadOptions)
        {
            var voucherTypelists = _context.Tbl201VoucherTypes.Select(i => new
            {
                i.VoucherTypeId,
                i.VoucherType,
                i.VoucherTypeAr
            });

            return Json(await DataSourceLoader.LoadAsync(voucherTypelists, loadOptions));
        }
        [HttpGet]
        public async Task<ActionResult> GetVouchers(string voucherType, string frmDate, string toDate)
        {
            var from = DateTime.Parse(frmDate);
            var to = DateTime.Parse(toDate);
            ERPMasterWtDataContextProcedures _procedures = new ERPMasterWtDataContextProcedures(_context);
            var ledgerData = await _procedures.StProAccountLedgerByVoucherTypeAsync(voucherType, from, to);
            return Json(ledgerData);
        }
    }
}