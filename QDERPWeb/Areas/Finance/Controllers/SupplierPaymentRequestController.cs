using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.DAL.Entities;

namespace QDWEB.Areas.Finance.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierPaymentRequestController : Controller
    {
        private readonly ERPMasterWtDataContext _context;

        public SupplierPaymentRequestController(ERPMasterWtDataContext context)
        {
            _context = context;
        }

        [HttpGet("GetSupplierPaymentRequest")]
        public IActionResult GetSupplierPaymentRequest(string supplierId)
        {
            try
            {
               
                var query = _context.Qry201SubLedgerPayablesMasters
                    .Where(e => e.AccountHeadNo == supplierId);

              
                var totalCount = query.Count();

                var data = query.Select(e => new
                {

                    e.ReferenceNo,
                    e.AccountHeadNo,
                   
                    VoucherDate = e.VoucherDate.HasValue
                        ? e.VoucherDate.Value.ToString("dd-MMM-yyyy")
                        : string.Empty,
                        
                    e.PayableAmount,
                    e.Paid,
                    e.Balance ,
                    e.VoucherNarration,
                    InvoiceDueDate = e.InvoiceDueDate.HasValue
                        ? e.InvoiceDueDate.Value.ToString("dd-MMM-yyyy")
                        : string.Empty,
                    e.OverdueDays,
                    VoucherEffectiveDate = e.VoucherEffectiveDate.HasValue
                        ? e.VoucherEffectiveDate.Value.ToString("dd-MMM-yyyy")
                        : string.Empty,
                    e.PurchaseOrderNo
                }).ToList();

                
                return Json(new
                {
                    data = data,
                    totalCount = totalCount
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred while fetching data.", error = ex.Message });
            }
        }

        [HttpGet("GetSuppliers")]
        public IActionResult GetSuppliers()
        {
            try
            {
                var suppliers = _context.Qry201SubLedgerPayablesMasters
                    .GroupBy(s => new { s.AccountHeadNo, s.AccountHead })
                    .Select(g => new
                    {
                        AccountHeadNo = g.Key.AccountHeadNo,
                        AccountHead = g.Key.AccountHead
                    })
                    .ToList();

                if (!suppliers.Any())
                {
                    return Json(new { message = "No suppliers found." });
                }

                return Json(suppliers);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred while fetching suppliers.", error = ex.Message });
            }
        }
    }
}
