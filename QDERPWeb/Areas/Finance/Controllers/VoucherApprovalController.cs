using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.DAL.Entities;
namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    public class VoucherApprovalController : Controller
    {
        private ERPMasterWtDataContext _context;

        public VoucherApprovalController(ERPMasterWtDataContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult GetVoucherApproval()
        {
            try
            {
              
                var data = _context.Qry20136VoucherMasterLists.Select(v => new
                {
                    v.VoucherNo,
                    v.VoucherDate,
                    v.VoucherRefNo,
                    v.VoucherNarration,
                    v.VoucherEnteredBy,
                    v.VoucherEnteredOn,
                    v.IsVerified,
                    v.VoucherVerifiedBy,
                    v.VoucherVerifiedOn,
                    v.IsApproved,
                    v.VoucherApprovedBy,
                    v.VoucherApprovedOn,
                    v.VoucherType,
                    v.VoucherEffectiveDate,
                    v.VoucherModifiedBy,
                    v.VoucherModifiedOn,
                    v.DebitAmount,
                    v.CreditAmount,
                    v.AuditVerifiedBy,
                    v.AuditVerifiedOn,
                    v.IsAuditVerified
                }).ToList();

                return Json(data);
            }
            catch (Exception ex)
            {
                
                return BadRequest(new { message = "An error occurred while fetching data.", error = ex.Message });
            }
        }
        [HttpGet]
        public IActionResult GetVoucherApprovals(DateTime? startDate, DateTime? endDate)
        {
            var vouchers = _context.Qry20136VoucherMasterLists.AsQueryable();

            if (startDate.HasValue && endDate.HasValue)
            {
                vouchers = vouchers.Where(v => v.VoucherDate >= startDate && v.VoucherDate <= endDate);
            }

            return Ok(vouchers.ToList());
        }
        [HttpGet]
        public IActionResult GetAssetsSummary()
        {
            try
            {
                var assetSummary = _context.Qry20149AssetsRegisterViews
                    .GroupBy(a => 1) // Group by a constant to aggregate the entire table
                    .Select(g => new
                    {
                        NoOfAssets = g.Count(),
                        CurrentAssetValue = g.Sum(a => a.NetBookValue) ?? 0 // Handle null values
                    })
                    .FirstOrDefault();

                return Ok(assetSummary); // Return default if null
            }
            catch (Exception ex)
            {
                // Log the exception if necessary
                return StatusCode(500, new { Error = "Failed to fetch asset summary.", Details = ex.Message });
            }
        }


    }
}

