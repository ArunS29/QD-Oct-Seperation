using System;
using System.Linq;
using System.Threading.Tasks;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using QD.ERP.Web.Services.Logging;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DepreciationMasterController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<DepreciationMasterController> _logger;
        private readonly IUserActionLogger _userActionLogger;

        public DepreciationMasterController(ILogger<DepreciationMasterController> logger, IUserActionLogger userActionLogger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
            _userActionLogger = userActionLogger;
        }

        [HttpGet]
        public IActionResult GetExpenseClaims()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var currentYear = DateTime.Now.Year;
                    var currentMonth = DateTime.Now.Month;

                    var startDate = new DateTime(currentYear, 1, 1); // 2025-01-01
                    var endDate = new DateTime(currentYear, currentMonth, DateTime.DaysInMonth(currentYear, currentMonth)); // e.g., 2025-07-31


                    var data = dbContext.Qry201205depreciationMasterViews.Where(v => v.DeprStartDate >= startDate && v.DeprEndDate <= endDate)
                        .Select(e => new
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
                        e.TotalBookValue,
                        e.IsPosted
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
     SELECT MAX(CAST(RIGHT(DepreciationDocNo, 4) AS INT)) AS MaxVoucherNo
     FROM tbl20119DepreciationMaster
     WHERE DepreciationDocNo LIKE {likePattern}")
                        .ToListAsync();

                    int maxVoucherNo = result.FirstOrDefault()?.MaxVoucherNo ?? 0;

                    int newVoucherNo = maxVoucherNo + 1;

                    // Format the new voucher number with leading zeros
                    strNewReceiptNo = "000" + newVoucherNo.ToString();
                    strNewReceiptNo = strNewReceiptNo.Substring(strNewReceiptNo.Length - 4);

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
        [HttpPost]
        public async Task<IActionResult> GenerateDepreciation([FromBody] DepreciationRequest request)
        {
            
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            try
            {
                var startDate = request.StartDate;
                var endDate = request.EndDate;
                var documentNo = request.DocumentNo;
                string userId = HttpContext.Session.GetString("UserId") ?? "Unknown";
                DateTime createdOn = DateTime.Now;

                await dbContext.Database.ExecuteSqlRawAsync(
                        @"EXEC sp20127GetAssetTransactionsReport 
                    @StartDate = {0}, 
                    @EndDate = {1}, 
                    @DocumentNo = {2}, 
                    @CreatedBy = {3}, 
                    @CreatedOn = {4}",
                    startDate,
                    endDate,
                    documentNo,
                    userId,
                    createdOn
                );
                await _userActionLogger.LogAsync(
                module: "Finance > Depreciation",
                actionDetail: $"Generated: {request.DocumentNo}",
                documentNo: request.DocumentNo
                );
                return Ok(new { success = true, message = "Depreciation generated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating depreciation: {ex.Message}");
                return BadRequest(new { success = false, message = "Failed to generate depreciation.", error = ex.Message });
            }
        }
        [HttpGet]
        public async Task<ActionResult> GetDepreciationbydocno( [FromQuery] DataSourceLoadOptions loadOptions, [FromQuery] string docNo,
        [FromQuery] string startDate,[FromQuery] string endDate)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }

            try
            {
                // Parse dates safely
                if (!DateTime.TryParse(startDate, out DateTime sDate) ||
                    !DateTime.TryParse(endDate, out DateTime eDate))
                {
                    return BadRequest(new { message = "Invalid date format." });
                }

                var ledgerData = dbContext.Qry201206depreciationChildren
                    .Where(x => x.DepreciationDocNo == docNo && x.DeprStartDate >= sDate && x.DeprEndDate <= eDate)
                    .AsQueryable();

                return Json(await DataSourceLoader.LoadAsync(ledgerData, loadOptions));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetDepreciationbydocno: {ex.Message}");
                return BadRequest(new { message = "An error occurred while fetching data.", error = ex.Message });
            }
        }
        [HttpGet]
        public async Task<ActionResult> GetDepreciationMaster(string depreciationdocno)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var result = await dbContext.Tbl20119DepreciationMasters
                                    .Where(x => x.DepreciationDocNo == depreciationdocno)
                                    .ToListAsync();

                    if (result != null && result.Any())
                    {
                        return Json(result);
                    }

                    return Json(new { success = false, message = "No child records found." });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetJournalChild: {ex.Message}");
                    return Json(new { success = false, message = "An error occurred while fetching child records." });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet]
        public IActionResult GetDepreciationPivotData(DataSourceLoadOptions loadOptions)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
            {
                return Unauthorized();
            }

            var data = dbContext.Qry201206depreciationChildren
                .Select(x => new
                {
                    x.DepreciationDocNo,
                    x.AccountGroup,
                    x.AssetLedgerNo,
                    x.AccountHead,
                    x.DeprStartDate,
                    x.AccDeprTotalAmount,

                    Months = x.DeprStartDate.HasValue ? x.DeprStartDate.Value.ToString("MMM-yyyy") : null,

                });

            return Json(DataSourceLoader.Load(data, loadOptions));
        }
        [HttpPost]
        
        public async Task<IActionResult> GetDepreciationTotal([FromBody] DepreciationTotalRequest request)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return BadRequest("Tenant context could not be determined.");

            if (string.IsNullOrWhiteSpace(request.DocNo))
                return BadRequest("DocNo is required.");

            try
            {
                var docNoParam = new SqlParameter("@DocNo", request.DocNo ?? string.Empty);

                var result = await dbContext
                    .TotalDepreciationResults
                    .FromSqlRaw("EXEC sp20129DepreciationTotal @DocNo", docNoParam)
                    .ToListAsync();

                // Assuming sp returns a single row with a column named TotalDepreciationAmount
                var totalAmount = result.FirstOrDefault()?.TotalDepreciationAmount ?? 0;

                return Ok(new { totalAmount });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error executing depreciation total SP.", error = ex.Message });
            }
        }

        [HttpPost]
        
        public async Task<IActionResult> InsertDepreciationToVoucher([FromBody] InsertDepreciationVoucherRequest request)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return BadRequest("Tenant context could not be determined.");

            try
            {
                request.AddedBy = HttpContext.Session.GetString("UserName") ?? "System";
                request.AddedOn = DateTime.Now;
                var parameters = new[]
                {
            new SqlParameter("@DepreciationDocNo", request.DepreciationDocNo ?? string.Empty),
            new SqlParameter("@PaymentVoucherNo", request.PaymentVoucherNo ?? string.Empty),
            new SqlParameter("@DebitAccount", request.DebitAccount ?? string.Empty),
            new SqlParameter("@CreditAccount", request.CreditAccount ?? string.Empty),
            new SqlParameter("@VoucherNarration", request.VoucherNarration ?? string.Empty),
            new SqlParameter("@AddedBy", request.AddedBy ?? string.Empty),
            new SqlParameter("@AddedOn", request.AddedOn),
           
            new SqlParameter("@TotalAmount", request.TotalAmount),
            new SqlParameter
            {
                ParameterName = "@JustAddedVoucherEntryNo",
                SqlDbType = System.Data.SqlDbType.Int,
                Direction = System.Data.ParameterDirection.InputOutput,
                Value = request.JustAddedVoucherEntryNo
            }
        };

                await dbContext.Database.ExecuteSqlRawAsync("EXEC sp20128InsertDepreciationToVoucher " +
                    "@DepreciationDocNo, @PaymentVoucherNo, @DebitAccount, @CreditAccount, " +
                    "@VoucherNarration, @AddedBy, @AddedOn, @TotalAmount, @JustAddedVoucherEntryNo", parameters);

                string returnedVoucherEntryNo = (string)parameters[1].Value;
                await _userActionLogger.LogAsync(
                module: "Finance > Depreciation",
                actionDetail: $"Depreciation Posted: {request.DepreciationDocNo}",
                documentNo: request.DepreciationDocNo
                );
                return Ok(new
                {
                    success = true,
                    JustAddedVoucherEntryNo = returnedVoucherEntryNo
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error inserting voucher.", error = ex.Message });
            }
        }
    }
}

