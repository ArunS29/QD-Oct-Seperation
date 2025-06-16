using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DepreciationMasterController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<DepreciationMasterController> _logger;

        public DepreciationMasterController(ILogger<DepreciationMasterController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetExpenseClaims()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var data = dbContext.Qry201205depreciationMasterViews.Select(e => new
                    {
                        e.JournalVoucherNo,
                        e.DepreciationDocNo,
                        e.DeprStartDate,
                        e.DeprEndDate,
                        e.NoOfAssets,
                        e.AssetOpeningBalance,
                        e.AssetTotalDebitTrans,
                        Postedon = e.PostedOn.HasValue
                            ? e.PostedOn.Value.ToString("dd-MMM-yyyy")
                            : string.Empty,
                        e.AssetTotalCreditTrans,
                        e.AssetClosingBalance,
                        e.AccumulatedOpeningBalance,
                        e.TotalDepreciationAmount,
                        e.AccumulatedTotalBalance,
                        e.TotalBookValue
                    }).ToList();

                    return Json(data);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetExpenseClaims: {ex.Message}");
                    return BadRequest(new { message = "An error occurred while fetching data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public IActionResult GetExpensesClaims(DateTime? startDate, DateTime? endDate)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var vouchers = dbContext.Qry201205depreciationMasterViews.AsQueryable();

                if (startDate.HasValue && endDate.HasValue)
                {
                    vouchers = vouchers.Where(v => v.DeprStartDate >= startDate && v.DeprEndDate <= endDate);
                }

                return Ok(vouchers.ToList());
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet]
        public async Task<ActionResult> GetDepreciationChild(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var ledgerData = dbContext.Qry201206depreciationChildren
                    .FromSqlRaw("SELECT * FROM qry201_206DepreciationChild")
                    .AsQueryable();


                    return Json(await DataSourceLoader.LoadAsync(ledgerData, loadOptions)); // ✅ No ToListAsync() here
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetAssetView: {ex.Message}");
                    return BadRequest(new { message = "An error occurred while fetching data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet]
        public async Task<ActionResult> GetNewDocNo()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                string currentYear = DateTime.Now.Year.ToString();
                string voucherString = "DEP-" + currentYear + "-";
                string strNewReceiptNo;

                // SQL query with interpolated string
                string likePattern = voucherString + "%";

                try
                {
                    // Use raw SQL query to fetch the maximum voucher number
                    var result = await dbContext.VoucherResults
                        .FromSqlInterpolated($@"
     SELECT MAX(CAST(RIGHT(ClaimRefNo, 4) AS INT)) AS MaxVoucherNo
     FROM tbl20120DepreciationChild
     WHERE DepreciationDocNo LIKE {likePattern}")
                        .ToListAsync();

                    int maxVoucherNo = result.FirstOrDefault()?.MaxVoucherNo ?? 0;

                    int newVoucherNo = maxVoucherNo + 1;

                    // Format the new voucher number with leading zeros
                    strNewReceiptNo = "0000" + newVoucherNo.ToString();
                    strNewReceiptNo = strNewReceiptNo.Substring(strNewReceiptNo.Length - 5);

                    // Concatenate with the voucher string
                    strNewReceiptNo = voucherString + strNewReceiptNo;
                }
                catch (Exception)
                {
                    // Handle cases where there's no existing voucher number
                    strNewReceiptNo = voucherString + "0001";
                }

                return Json(strNewReceiptNo);
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public IActionResult GetDepreciationChildshow(DateTime? startDate, DateTime? endDate)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var query = dbContext.Qry201206depreciationChildren.AsQueryable();

                if (startDate.HasValue && endDate.HasValue)
                {
                    query = query.Where(x => x.DeprStartDate >= startDate && x.DeprEndDate <= endDate);
                }

                return Json(query.ToList());
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }


    }
}

