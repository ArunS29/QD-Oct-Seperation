
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.AspNet.Mvc;
using QD.ERP.Web.DAL.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    //[Area("Finance")]
    [Route("Finance/api/[controller]/[action]")]
    [ApiController]
    public class SalaryPayableDetailsController : Controller
    {
        private ERPMasterWtDataContext _context;

        public SalaryPayableDetailsController(ERPMasterWtDataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions)
        {
            var qry20164salarypayableledgermaster = _context.Qry20164SalaryPayableLedgerMasters.Select(i => new {
                i.ReferenceNo,
                i.EmployeeNo,
                i.EmployeeName,
                i.Amount,
                i.DrCr,
                i.VoucherNo,
                i.VoucherDate,
                i.EntryNarration
            });



            return Json(await DataSourceLoader.LoadAsync(qry20164salarypayableledgermaster, loadOptions));
        }


    }
}
