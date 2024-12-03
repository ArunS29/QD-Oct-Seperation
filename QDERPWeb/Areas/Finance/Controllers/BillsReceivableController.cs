
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using QD.ERP.Web.DAL.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Form.Areas.Finance.Controllers
{
    
    [Route("/Finance/api/[controller]/[action]")]
    [ApiController]
    public class BillsReceivableController : Controller
    {
        private ERPMasterWtDataContext _context;
        public BillsReceivableController(ERPMasterWtDataContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions, string filterType = null)
        {
            var query = _context.Qry20105BillsReceivableAgeingViews.Select(i => new {
                i.AccountHead,
                i.ReferenceNo,
                i.VoucherRefNo,
                i.VoucherDate,
                i.InvoiceDueDate,
                // i.invoiceAmount,
                i.ReceivableAmount,
                i.Received,
                i.Balance,
                i.OverdueDays

            });

            // Apply filter based on filterType
            if (filterType == "WithBalance")
            {
                query = query.Where(i => i.Balance > 0); // Only show rows where Balance > 0
            }
            else if (filterType == "FullyReceived")
            {
                query = query.Where(i => i.Balance <= 0); // Only show rows where Balance = 0
            }

            return Json(await DataSourceLoader.LoadAsync(query, loadOptions));
        }
    }
}