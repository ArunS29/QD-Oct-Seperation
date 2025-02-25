
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using QD.ERP.Web.DAL.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("/development/Finance/api/[controller]/[action]")]
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
            try
            {
                // Base query
                var query = _context.Qry201SubLedgerPayablesMasters.Select(i => new
                {
                    i.AccountHeadNo,
                    i.AccountHead,
                    i.ReferenceNo,
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
                    query = query.Where(i => i.Balance != 0); // Filter for records where balance is not equal to 0
                }
                else if (filterType == "FullyPaid")
                {
                    query = query.Where(i => i.Balance == 0); // Filter for fully paid bills (Balance == 0)
                }

                // Apply the DevExtreme DataSourceLoader with sorting, filtering, and grouping from the request
                var result = await DataSourceLoader.LoadAsync(query, loadOptions);

                return Json(result);
            }
            catch (Exception ex)
            {
                // Log the exception (logging mechanism depends on your setup, e.g., Serilog, NLog, etc.)
                // _logger.LogError(ex, "An error occurred while processing the Get method."); 

                // Return a generic error response
                return StatusCode(500, new { message = "An error occurred while processing your request.", details = ex.Message });
            }
        }



    }
}