
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using QD.ERP.Web.DAL.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Form.Areas.Finance.Controllers
{
    [Route("/Finance/api/[controller]/[action]")]
    [ApiController]
    public class BillsPayableController : Controller
    {
        private ERPMasterWtDataContext _context;

        public BillsPayableController(ERPMasterWtDataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions, string filterType = null)
        {
            var query = _context.Qry201SubLedgerPayablesMasters.Select(i => new {
                i.AccountHeadNo,
                i.AccountHead,
                i.ReferenceNo,  // This is the field you want to access in the view
                i.VoucherDate,
                i.VoucherRefNo,

                i.InvoiceAmountBeforeRetention,
                i.Paid,
                i.Balance,
                i.InvoiceDueDate,
                i.NoOfDaysCreditPeriod,
                i.OverdueDays,
            });

            // Apply filter based on filterType
            if (filterType == "WithBalance")
            {
                query = query.Where(i => i.Balance > 0); // Only show rows where Balance > 0
            }
            else if (filterType == "FullyPaid")
            {
                query = query.Where(i => i.Balance <= 0); // Only show rows where Balance = 0
            }

            return Json(await DataSourceLoader.LoadAsync(query, loadOptions));
        }



    }
}