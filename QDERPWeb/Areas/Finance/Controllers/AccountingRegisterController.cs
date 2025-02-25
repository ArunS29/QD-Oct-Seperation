using QD.ERP.Web.DAL.Entities;
using QDERPWeb.Models;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
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
            try
            {
                var voucherTypelists = _context.Tbl201VoucherTypes.Select(i => new
                {
                    i.VoucherTypeId,
                    i.VoucherType,
                    i.VoucherTypeAr
                });

                return Json(await DataSourceLoader.LoadAsync(voucherTypelists, loadOptions));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetVouchers(string voucherType, string frmDate, string toDate)
        {
            try
            {
                DateTime from = new DateTime(2000, 1, 1); 
                DateTime to = DateTime.Now;

                if (!string.IsNullOrEmpty(frmDate) && DateTime.TryParseExact(frmDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedFrom))
                {
                    from = parsedFrom;
                }

                if (!string.IsNullOrEmpty(toDate) && DateTime.TryParseExact(toDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedTo))
                {
                    to = parsedTo;
                }

                var procedures = new ERPMasterWtDataContextProcedures(_context);
                var ledgerData = await procedures.StProAccountLedgerByVoucherTypeAsync(string.IsNullOrEmpty(voucherType) ? null : voucherType, from, to);

                return Json(ledgerData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
