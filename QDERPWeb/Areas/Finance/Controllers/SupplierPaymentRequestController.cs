using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QDWEB.Areas.Finance.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierPaymentRequestController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<SupplierPaymentRequestController> _logger;

        public SupplierPaymentRequestController(ILogger<SupplierPaymentRequestController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpGet("GetSupplierPaymentRequest")]
        public IActionResult GetSupplierPaymentRequest(string supplierId)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var query = dbContext.Qry201SubLedgerPayablesMasters
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
                        e.Balance,
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
                    _logger.LogError($"Error in GetSupplierPaymentRequest: {ex.Message}");
                    return BadRequest(new { message = "An error occurred while fetching data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet("GetSuppliers")]
        public IActionResult GetSuppliers()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var suppliers = dbContext.Qry201SubLedgerPayablesMasters
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
                    _logger.LogError($"Error in GetSuppliers: {ex.Message}");
                    return BadRequest(new { message = "An error occurred while fetching suppliers.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
    }
}










