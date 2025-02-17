using QD.ERP.Web.DAL.Entities;
using QDERPWeb.Models;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using System.Globalization;
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
            try
            {
                DateTime from = new DateTime(2000, 1, 1); // Default from date
                DateTime to = DateTime.Now;

                if (!string.IsNullOrEmpty(frmDate) && DateTime.TryParseExact(frmDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedFrom))
                {
                    from = parsedFrom;
                }

                if (!string.IsNullOrEmpty(toDate) && DateTime.TryParseExact(toDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedTo))
                {
                    to = parsedTo;
                }

                ERPMasterWtDataContextProcedures _procedures = new ERPMasterWtDataContextProcedures(_context);

                List<QD.ERP.Web.DAL.Entities.StProAccountLedgerByVoucherTypeResult> ledgerData;

                // If voucherType is null or empty, fetch all records
                if (string.IsNullOrEmpty(voucherType))
                {
                    ledgerData = await _procedures.StProAccountLedgerByVoucherTypeAsync(null, from, to);
                }
                else
                {
                    ledgerData = await _procedures.StProAccountLedgerByVoucherTypeAsync(voucherType, from, to);
                }

                return Json(ledgerData);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

    }
}