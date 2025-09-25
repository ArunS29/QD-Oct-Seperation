using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.Areas.Finance.Views;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using QD.ERP.Web.Services.Logging;
using QDERPWeb.Models;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ExpenseClaimController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<ExpenseClaimController> _logger;
        private readonly IUserActionLogger _userActionLogger;
        private readonly FcmService _fcmService;

        public ExpenseClaimController(ILogger<ExpenseClaimController> logger, TenantDbContextHelper tenantDbContextHelper, IUserActionLogger userActionLogger, FcmService fcmService)
        {
            _userActionLogger = userActionLogger;
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
            _fcmService = fcmService;
        }

        [HttpGet]
        public async Task<ActionResult> GetExpenseClaim(byte ClaimerID, DateTime StartDate, DateTime EndDate, bool IfShowAll)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var rawResult = await dbContext.ExpenseClaimViews
                        .FromSqlRaw("EXEC sp20105ExpenseClaimView @p0, @p1, @p2, @p3",
                            ClaimerID, StartDate, EndDate, IfShowAll)
                        .ToListAsync(); // Fetch data first before LINQ joins

                    // Perform LINQ joins in-memory
                    var result = rawResult.AsEnumerable().Select(claim => new sp20105ExpenseClaimViewResult
                    {
                        ClaimerID = claim.ClaimerID,
                        ClaimRefNo = claim.ClaimRefNo,
                        PaymentVoucherNo = claim.PaymentVoucherNo,
                        ApprovedBy = claim.ApprovedBy,
                        ApprovedOn = claim.ApprovedOn,
                        ClaimCreatedBy = claim.ClaimCreatedBy,
                        ClaimCreatedOn = claim.ClaimCreatedOn,
                        ClaimerName = claim.ClaimerName,
                        ClaimModifiedBy = claim.ClaimModifiedBy,
                        ClaimModifiedOn = claim.ClaimModifiedOn,
                        ClaimRemarks = claim.ClaimRemarks,
                        PaidBy = claim.PaidBy,
                        PaidOn = claim.PaidOn,
                        PaymentAccount = claim.PaymentAccount,
                        PaymentType = claim.PaymentType,
                        ProjectClaimedFor = dbContext.Tbl201CostAllocationUnits
                            .FirstOrDefault(a => a.CostAllocationUnitId == claim.ProjectClaimedFor)?.CostAllocationUnit,// Lookup CostAllocationUnit
                        SubmittedBy = claim.SubmittedBy,
                        SubmittedOn = claim.SubmittedOn,
                        VerifiedBy = claim.VerifiedBy,
                        VerifiedOn = claim.VerifiedOn,
                        ClaimDate = claim.ClaimDate,
                        ClaimedAmountTotal = claim.ClaimedAmountTotal,
                        ApprovedAmountTotal = claim.ApprovedAmountTotal,
                        IsSubmittedToFinance = claim.IsSubmittedToFinance,
                        IsVerified = claim.IsVerified,
                        IsApproved = claim.IsApproved,
                        IsPaid = claim.IsPaid
                    });

                    return Json(result); // No need for ToListAsync() as it's already in-memory
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetExpenseClaim: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }



        [HttpGet]
        public async Task<ActionResult> GetUser()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var users = await dbContext.TblUserMasters.Select(u => new
                    {
                        u.UserId,
                        u.UserName
                    })
                .ToListAsync(); ;
                    return Json(users);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetUser: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        
        [HttpGet]
        public JsonResult GetExpenseClaims()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var expenseClaims = from claim in dbContext.Tbl20103ExpenseClaimChildren
                                    join account in dbContext.Tbl201ChartOfAccounts
                                    on claim.AccountId equals account.AccountId
                                    select new
                                    {
                                        claim.ExpenseDescription,
                                        claim.BillRefNo,
                                        claim.BillDate,
                                        claim.ClaimedAmount,
                                        claim.ApprovedAmount,
                                        claim.AccountId,
                                        AccountHead = account.AccountHead, // Include AccountHead
                                        claim.CostCenterCode

                                    };

                return Json(expenseClaims.ToList());
            }

            return Json(new { success = false, message = "Failed to retrieve tenant database context." });
        }

        [HttpGet]
        public IActionResult GetEditCostCenter()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var data = dbContext.Tbl201CostAllocationUnits
                    .Select(c => new
                    {
                        c.CostAllocationUnitId,
                        c.CostAllocationUnit,
                        c.CostAllocationGroup,
                        c.IsDisabled
                    }).ToList();

                return Ok(data);
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet]
        public async Task<ActionResult> GetNewClaimNo()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                string userId = HttpContext.Session.GetString("UserId") ?? "000";
                string voucherString = "EXP-" + userId + "-";
                string strNewReceiptNo;

                // SQL query with interpolated string
                string likePattern = voucherString + "%";

                try
                {
                    // Use raw SQL query to fetch the maximum voucher number
                    var result = await dbContext.VoucherResults
                        .FromSqlInterpolated($@"
     SELECT MAX(CAST(RIGHT(ClaimRefNo, 5) AS INT)) AS MaxVoucherNo
     FROM tbl20102ExpenseClaimMaster
     WHERE ClaimRefNo LIKE {likePattern}")
                        .ToListAsync();

                    int maxVoucherNo = result.FirstOrDefault()?.MaxVoucherNo ?? 0;

                    int newVoucherNo = maxVoucherNo + 1;

                    // Format the new voucher number with leading zeros
                    strNewReceiptNo = "00000" + newVoucherNo.ToString();
                    strNewReceiptNo = strNewReceiptNo.Substring(strNewReceiptNo.Length - 5);

                    // Concatenate with the voucher string
                    strNewReceiptNo = voucherString + strNewReceiptNo;
                }
                catch (Exception)
                {
                    // Handle cases where there's no existing voucher number
                    strNewReceiptNo = voucherString + "00001";
                }

                return Json(strNewReceiptNo);
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet]
        public async Task<ActionResult> GetNewPettyNo()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                string userId = HttpContext.Session.GetString("UserId") ?? "000";
                string voucherString = "PCQ-" + userId + "-";
                string strNewReceiptNo;

                // SQL query with interpolated string
                string likePattern = voucherString + "%";

                try
                {
                    // Use raw SQL query to fetch the maximum voucher number
                    var result = await dbContext.VoucherResults
                        .FromSqlInterpolated($@"
     SELECT MAX(CAST(RIGHT(ClaimRefNo, 5) AS INT)) AS MaxVoucherNo
     FROM tbl20102ExpenseClaimMaster
     WHERE ClaimRefNo LIKE {likePattern}")
                        .ToListAsync();

                    int maxVoucherNo = result.FirstOrDefault()?.MaxVoucherNo ?? 0;

                    int newVoucherNo = maxVoucherNo + 1;

                    // Format the new voucher number with leading zeros
                    strNewReceiptNo = "00000" + newVoucherNo.ToString();
                    strNewReceiptNo = strNewReceiptNo.Substring(strNewReceiptNo.Length - 5);

                    // Concatenate with the voucher string
                    strNewReceiptNo = voucherString + strNewReceiptNo;
                }
                catch (Exception)
                {
                    // Handle cases where there's no existing voucher number
                    strNewReceiptNo = voucherString + "00001";
                }

                return Json(strNewReceiptNo);
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<ActionResult> GetNewPaymentNo()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                string userId = HttpContext.Session.GetString("UserId") ?? "000";
                string voucherString = "SRQ-" + userId + "-";
                string strNewReceiptNo;

                // SQL query with interpolated string
                string likePattern = voucherString + "%";

                try
                {
                    // Use raw SQL query to fetch the maximum voucher number
                    var result = await dbContext.VoucherResults
                        .FromSqlInterpolated($@"
     SELECT MAX(CAST(RIGHT(ClaimRefNo, 5) AS INT)) AS MaxVoucherNo
     FROM tbl20102ExpenseClaimMaster
     WHERE ClaimRefNo LIKE {likePattern}")
                        .ToListAsync();

                    int maxVoucherNo = result.FirstOrDefault()?.MaxVoucherNo ?? 0;

                    int newVoucherNo = maxVoucherNo + 1;

                    // Format the new voucher number with leading zeros
                    strNewReceiptNo = "00000" + newVoucherNo.ToString();
                    strNewReceiptNo = strNewReceiptNo.Substring(strNewReceiptNo.Length - 5);

                    // Concatenate with the voucher string
                    strNewReceiptNo = voucherString + strNewReceiptNo;
                }
                catch (Exception)
                {
                    // Handle cases where there's no existing voucher number
                    strNewReceiptNo = voucherString + "00001";
                }

                return Json(strNewReceiptNo);
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public IActionResult GetSupplierpayment()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext)
                && tenant != null && dbContext != null)
            {
                var viewModel = new _SupplierPaymentRequestModel(); // or fetch actual data
                return PartialView("~/Areas/Finance/Views/_SupplierPaymentRequest.cshtml", viewModel);
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public IActionResult GetSupplierAccount()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var data = dbContext.Qry201ListOfAccounts.Where(p => p.AccountGroupId == "A012" || p.AccountGroupId == "A003")
                    .Select(c => new
                    {
                        c.AccountId,
                        c.AccountHead
                    }).ToList();

                return Ok(data);
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet]
        public IActionResult GetSubLedgerData(string accountId, string accountHead)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var data = dbContext.Qry201SubLedgerPayablesMasters
                .Where(x => x.AccountHeadNo == accountId && x.AccountHead == accountHead)
                .ToList();

                return Json(data);
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet]
        public IActionResult SupplierPaymentfooter()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext)
                && tenant != null && dbContext != null)
            {
                 // or fetch actual data
                return PartialView("~/Pages/Shared/SupplierPaymentfooter.cshtml");
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpPost]
        public async Task<IActionResult> UpdateVoucher([FromBody] ExpenseClaimViewModel model)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            if (model == null || model.ExpenseDetails == null || !model.ExpenseDetails.Any())
            {
                return BadRequest(new { success = false, message = "No data received" });
            }

            try
            {
                var TenantName = HttpContext.Session.GetString("TenantName");
                // Get user session data
                string userIdStr = HttpContext.Session.GetString("UserId");
                byte claimerId = Convert.ToByte(userIdStr); // ✅ Convert string to byte

                string userName = HttpContext.Session.GetString("UserName");
                DateTime now = DateTime.Now;
                // Check if master record exists
                var existingMaster = await dbContext.Tbl20102ExpenseClaimMasters
                    .FirstOrDefaultAsync(m => m.ClaimRefNo == model.ClaimRefNo);

                if (existingMaster != null)
                {
                    // ✅ Update master record
                    existingMaster.ClaimDate = model.ClaimDate;
                    existingMaster.ClaimEffectiveDate = model.ClaimEffectiveDate;
                    existingMaster.ProjectClaimedFor = model.ProjectClaimedFor;
                    existingMaster.ClaimRemarks = model.ClaimRemarks;
                    existingMaster.ClaimModifiedBy = userName;
                    existingMaster.ClaimModifiedOn = now;
                    existingMaster.PaymentType = model.PaymentType;
                    existingMaster.PaymentAccount = model.PaymentAccount;
                    existingMaster.Priority = model.Priority;
                    dbContext.Tbl20102ExpenseClaimMasters.Update(existingMaster);
                }
                else
                {
                    // ✅ Insert new master
                    var newMaster = new Tbl20102ExpenseClaimMaster
                    {
                        ClaimRefNo = model.ClaimRefNo,
                        ClaimDate = model.ClaimDate,
                        ClaimEffectiveDate = model.ClaimEffectiveDate,
                        ProjectClaimedFor = model.ProjectClaimedFor,
                        ClaimRemarks = model.ClaimRemarks,
                        ClaimerId = claimerId,
                        PaymentType = model.PaymentType,
                        PaymentAccount = model.PaymentAccount,
                        Priority = model.Priority,
                        ClaimCreatedBy = userName,
                        ClaimCreatedOn = now,
                        FundRequestTypeId = model.FundRequestTypeId
                    };

                    dbContext.Tbl20102ExpenseClaimMasters.Add(newMaster);
                }

                //// ✅ Remove existing child rows for this ClaimRefNo
                //var existingChildren = dbContext.Tbl20103ExpenseClaimChildren
                //    .Where(c => c.ClaimRefNo == model.ClaimRefNo);

                //dbContext.Tbl20103ExpenseClaimChildren.RemoveRange(existingChildren);

                // ✅ Add new child rows
                foreach (var item in model.ExpenseDetails)
                {
                    var existingChild = await dbContext.Tbl20103ExpenseClaimChildren
                           .FirstOrDefaultAsync(c =>
                           c.ClaimRefNo == model.ClaimRefNo &&
                           c.ClaimChildNo == item.ClaimChildNo);

                    if (existingChild != null)
                    {
                        // ✅ Only update the desired 4 columns
                        existingChild.ClaimChildNo = item.ClaimChildNo;
                        existingChild.ClaimRefNo = model.ClaimRefNo;
                        existingChild.AccountId = item.AccountId;
                        existingChild.CostCenterCode = item.CostCenterCode;
                        existingChild.ExpenseDescription = item.ExpenseDescription;
                        existingChild.BillRefNo = item.BillRefNo;
                        existingChild.BillDate = item.BillDate;
                        existingChild.ClaimedAmount = item.ClaimedAmount;
                        existingChild.ApprovedAmount = item.ApprovedAmount;


                        dbContext.Tbl20103ExpenseClaimChildren.Update(existingChild);
                    }
                }

                await dbContext.SaveChangesAsync();

                await _userActionLogger.LogAsync(
      module: "Finance > Expense Claim",
      actionDetail: $"Saved Voucher: {model.ClaimRefNo}",
      documentNo: model.ClaimRefNo
  );

                var notifyRequest = new NotificationRequest
             {
                 UserId = userIdStr, // or fetch from session/DB
                 VoucherName = model.ClaimRefNo,
                 ActionType = "You have one claim to submit",
                TenantName = TenantName 
        };

        await _fcmService.SendNotificationAsync(notifyRequest);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> CheckClaimRefNo([FromBody] ExpenseClaimViewModel model)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var exists = await dbContext.Tbl20102ExpenseClaimMasters
                    .AnyAsync(c => c.ClaimRefNo == model.ClaimRefNo);

                return Json(new { exists });
            }

            return Json(new { exists = false });
        }


        [HttpPost]
        public async Task<IActionResult> SubmitClaim([FromBody] ExpenseClaimViewModel model)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var userName = HttpContext.Session.GetString("UserName");
                var UserId = HttpContext.Session.GetString("UserId");
                var TenantName = HttpContext.Session.GetString("TenantName");

                var submittedOn = DateTime.Now;

                var claim = await dbContext.Tbl20102ExpenseClaimMasters
                    .FirstOrDefaultAsync(c => c.ClaimRefNo == model.ClaimRefNo);

                if (claim != null)
                {
                    claim.IsSubmittedToFinance = true;
                    claim.SubmittedBy = userName;
                    claim.SubmittedOn = submittedOn;

                    await dbContext.SaveChangesAsync();
                    await _userActionLogger.LogAsync(
                    module: "Finance > Expense Claim",
                    actionDetail: $"Submitted: {claim.ClaimRefNo}",
                    documentNo: claim.ClaimRefNo
                    );
                    var notifyRequest = new NotificationRequest
             {
                 UserId = UserId, // or fetch from session/DB
                 VoucherName = model.ClaimRefNo,
                 ActionType = "You have one claim to verify",
                TenantName = TenantName 
        };

        await _fcmService.SendNotificationAsync(notifyRequest);

                    return Json(new
                    {
                        success = true,
                        submittedBy = userName,
                        submittedOn = submittedOn.ToString("dd-MMM-yyyy")
                    });
                }

                return Json(new { success = false, message = "Claim not found." });
            }

            return Json(new { success = false, message = "Tenant not found." });
        }


        [HttpPost]
        public async Task<IActionResult> VerifyClaim([FromBody] ExpenseClaimViewModel model)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var userName = HttpContext.Session.GetString("UserName") ?? "System";
                var UserId = HttpContext.Session.GetString("UserId");
                var TenantName = HttpContext.Session.GetString("TenantName");

                var verifiedOn = DateTime.Now;

                var claim = await dbContext.Tbl20102ExpenseClaimMasters
                    .FirstOrDefaultAsync(c => c.ClaimRefNo == model.ClaimRefNo);

                if (claim != null)
                {
                    claim.IsVerified = true;
                    claim.VerifiedBy = userName;
                    claim.VerifiedOn = verifiedOn;

                    await dbContext.SaveChangesAsync();

                    await _userActionLogger.LogAsync(
          module: "Finance > Expense Claim",
          actionDetail: $"Verified Claim: {model.ClaimRefNo}",
          documentNo: model.ClaimRefNo
      );

                    var notifyRequest = new NotificationRequest
             {
                 UserId = UserId, // or fetch from session/DB
                 VoucherName = model.ClaimRefNo,
                 ActionType = "You have one claim to approve",
                TenantName = TenantName 
        };

        await _fcmService.SendNotificationAsync(notifyRequest);

                    return Json(new
                    {
                        success = true,
                        verifiedBy = userName,
                        verifiedOn = verifiedOn.ToString("dd-MMM-yyyy")
                    });
                }

                return Json(new { success = false, message = "Claim not found." });
            }

            return Json(new { success = false, message = "Tenant not found." });
        }
        [HttpPost]
        public async Task<IActionResult> ApproveClaim([FromBody] ExpenseClaimViewModel model)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var userName = HttpContext.Session.GetString("UserName");
                var UserId = HttpContext.Session.GetString("UserId");
                var TenantName = HttpContext.Session.GetString("TenantName");

                var approveOn = DateTime.Now;

                var claim = await dbContext.Tbl20102ExpenseClaimMasters
                    .FirstOrDefaultAsync(c => c.ClaimRefNo == model.ClaimRefNo);

                if (claim != null)
                {
                    claim.IsApproved = true;
                    claim.ApprovedBy = userName;
                    claim.ApprovedOn = approveOn;

                    await dbContext.SaveChangesAsync();
                    await _userActionLogger.LogAsync(
          module: "Finance > Expense Claim",
          actionDetail: $"Approved Claim: {model.ClaimRefNo}",
          documentNo: model.ClaimRefNo
      );
                    var notifyRequest = new NotificationRequest
             {
                 UserId = UserId, // or fetch from session/DB
                 VoucherName = model.ClaimRefNo,
                 ActionType = "You have one claim to pay",
                TenantName = TenantName 
        };

        await _fcmService.SendNotificationAsync(notifyRequest);

                    return Json(new
                    {
                        success = true,
                        approvedBy = userName,
                        approvedOn = approveOn.ToString("dd-MMM-yyyy")
                    });
                }

                return Json(new { success = false, message = "Claim not found." });
            }

            return Json(new { success = false, message = "Tenant not found." });
        }
        [HttpGet]
        public async Task<ActionResult> GetNewCPVoucherNo(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {


                string defaultCompanyString = HttpContext.Session.GetString("DefaultcompanyID") ?? "";
                byte defaultCompanyByte = 0; // or any default value you want

                if (!string.IsNullOrEmpty(defaultCompanyString))
                {
                    // Safest way (avoids exceptions):
                    byte.TryParse(defaultCompanyString, out defaultCompanyByte);
                    // Now defaultCompanyByte holds the parsed value, or 0 if parsing failed.
                }

                // Now use defaultCompanyByte as needed



                byte companyId = defaultCompanyByte;

                // Step 2: Get NoOfDigitsInVouchers
                var companyConfig = await dbContext.Tbl901CompanyDetails02s
                    .Where(c => c.CompanyId == companyId)
                    .Select(c => new { c.NoOfDigitsInVouchers })
                    .FirstOrDefaultAsync();

                byte configuredDigitCount = companyConfig?.NoOfDigitsInVouchers ?? 3; // Default to 3 if not found

                // Step 3: Prepare voucher prefix
                DateTime currentDate = DateTime.Now;
                string yearPart = currentDate.Year.ToString().Substring(2); // "25"
                string monthPart = currentDate.Month.ToString("00"); // "06"
                string voucherPrefix = $"CP-{yearPart}-{monthPart}-";
                string likePattern = voucherPrefix + "%";

                int digitCountToUse = configuredDigitCount; // this might change if series already exists
                string strNewReceiptNo;
                try
                {
                    // Step 4: Check if any vouchers already exist for current month
                    var existingVoucher = await dbContext.Tbl201VoucherEntries
                        .Where(v => v.VoucherNo.StartsWith(voucherPrefix))
                        .OrderByDescending(v => v.VoucherNo)
                        .Select(v => v.VoucherNo)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(existingVoucher))
                    {
                        // Step 5: Existing series found → infer digit count from length of number part
                        string numberPart = existingVoucher.Substring(voucherPrefix.Length);
                        digitCountToUse = numberPart.Length;
                    }

                    // Step 6: Fetch max number using resolved digit count
                    var result = await dbContext.VoucherResults
                        .FromSqlInterpolated($@"
                    SELECT MAX(CAST(RIGHT(VoucherNo, {digitCountToUse}) AS INT)) AS MaxVoucherNo
                    FROM Tbl201VoucherMaster
                    WHERE VoucherNo LIKE {likePattern}")
                        .ToListAsync();

                    int maxVoucherNo = result.FirstOrDefault()?.MaxVoucherNo ?? 0;
                    int newVoucherNo = maxVoucherNo + 1;

                    string paddedNo = newVoucherNo.ToString().PadLeft(digitCountToUse, '0');
                    strNewReceiptNo = voucherPrefix + paddedNo;
                }
                catch (Exception)
                {
                    // fallback if any failure
                    string fallback = "1".PadLeft(configuredDigitCount, '0');
                    strNewReceiptNo = voucherPrefix + fallback;
                }

                return Json(strNewReceiptNo);
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }


        [HttpGet]
        public async Task<ActionResult> GetNewBPVoucherNo(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {

                string defaultCompanyString = HttpContext.Session.GetString("DefaultcompanyID") ?? "";
                byte defaultCompanyByte = 0; // or any default value you want

                if (!string.IsNullOrEmpty(defaultCompanyString))
                {
                    // Safest way (avoids exceptions):
                    byte.TryParse(defaultCompanyString, out defaultCompanyByte);
                    // Now defaultCompanyByte holds the parsed value, or 0 if parsing failed.
                }

                // Now use defaultCompanyByte as needed



                byte companyId = defaultCompanyByte;

                // Step 2: Get NoOfDigitsInVouchers
                var companyConfig = await dbContext.Tbl901CompanyDetails02s
                    .Where(c => c.CompanyId == companyId)
                    .Select(c => new { c.NoOfDigitsInVouchers })
                    .FirstOrDefaultAsync();

                byte configuredDigitCount = companyConfig?.NoOfDigitsInVouchers ?? 3; // Default to 3 if not found

                // Step 3: Prepare voucher prefix
                DateTime currentDate = DateTime.Now;
                string yearPart = currentDate.Year.ToString().Substring(2); // "25"
                string monthPart = currentDate.Month.ToString("00"); // "06"
                string voucherPrefix = $"BP-{yearPart}-{monthPart}-";
                string likePattern = voucherPrefix + "%";

                int digitCountToUse = configuredDigitCount; // this might change if series already exists
                string strNewReceiptNo;
                try
                {
                    // Step 4: Check if any vouchers already exist for current month
                    var existingVoucher = await dbContext.Tbl201VoucherEntries
                        .Where(v => v.VoucherNo.StartsWith(voucherPrefix))
                        .OrderByDescending(v => v.VoucherNo)
                        .Select(v => v.VoucherNo)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(existingVoucher))
                    {
                        // Step 5: Existing series found → infer digit count from length of number part
                        string numberPart = existingVoucher.Substring(voucherPrefix.Length);
                        digitCountToUse = numberPart.Length;
                    }

                    // Step 6: Fetch max number using resolved digit count
                    var result = await dbContext.VoucherResults
                        .FromSqlInterpolated($@"
                    SELECT MAX(CAST(RIGHT(VoucherNo, {digitCountToUse}) AS INT)) AS MaxVoucherNo
                    FROM Tbl201VoucherMaster
                    WHERE VoucherNo LIKE {likePattern}")
                        .ToListAsync();

                    int maxVoucherNo = result.FirstOrDefault()?.MaxVoucherNo ?? 0;
                    int newVoucherNo = maxVoucherNo + 1;

                    string paddedNo = newVoucherNo.ToString().PadLeft(digitCountToUse, '0');
                    strNewReceiptNo = voucherPrefix + paddedNo;
                }
                catch (Exception)
                {
                    // fallback if any failure
                    string fallback = "1".PadLeft(configuredDigitCount, '0');
                    strNewReceiptNo = voucherPrefix + fallback;
                }

                return Json(strNewReceiptNo);
            }


            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet]
        public IActionResult IsClaimApproved(string claimRefNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var isApproved = dbContext.Tbl20102ExpenseClaimMasters
        .Any(x => x.ClaimRefNo == claimRefNo && x.IsApproved == true);

                return Ok(isApproved);
            }

            return Json(new { exists = false });
        }
        [HttpGet]
        public async Task<ActionResult> GetClaimmaster(string claimRefNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var result = await dbContext.Tbl20102ExpenseClaimMasters
                                    .Where(x => x.ClaimRefNo == claimRefNo)
                                    .Select(x => new {
                                        x.ClaimRefNo,
                                        x.ClaimDate,
                                        x.ProjectClaimedFor,
                                        x.Priority,
                                        x.ClaimEffectiveDate,
                                        x.ClaimRemarks,
                                        x.IsSubmittedToFinance,
                                        x.SubmittedBy,
                                        x.SubmittedOn,
                                        x.IsVerified,
                                        x.VerifiedBy,
                                        x.VerifiedOn,
                                        x.IsApproved,
                                        x.ApprovedBy,
                                        x.ApprovedOn,
                                        x.IsPaid,
                                        x.PaidBy,
                                        x.PaidOn,
                                        x.PaymentVoucherNo,
                                        x.PaymentType,
                                        x.PaymentAccount,
                                        Claimedproject = dbContext.Tbl201CostAllocationUnits
                       .Where(a => a.CostAllocationUnitId == x.ProjectClaimedFor)
                       .Select(a => a.CostAllocationUnit)
                       .FirstOrDefault(),
                                        PaymentAccountname = dbContext.Tbl201ChartOfAccounts
                       .Where(c => c.AccountId == x.PaymentAccount)
                       .Select(c => c.AccountHead)
                       .FirstOrDefault(),
                                       
                                    })

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
        public async Task<ActionResult> GetClaimChild(string claimRefNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var result = await dbContext.Tbl20103ExpenseClaimChildren
    .Where(x => x.ClaimRefNo == claimRefNo)
    .Select(x => new {
        x.ClaimRefNo,
        x.ClaimChildNo,
        x.ExpenseDescription,
        x.BillRefNo,
        x.BillDate,
        x.ClaimedAmount,
        x.ApprovedAmount,
        x.AccountId,
        x.CostCenterCode,
        AccountHead = dbContext.Tbl201ChartOfAccounts
                        .Where(a => a.AccountId == x.AccountId)
                        .Select(a => a.AccountHead)
                        .FirstOrDefault(),
        CostAllocationUnit = dbContext.Tbl201CostAllocationUnits
                        .Where(c => c.CostAllocationUnitId == x.CostCenterCode)
                        .Select(c => c.CostAllocationUnit)
                        .FirstOrDefault()
    })
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
        public JsonResult GetPurchaseTaxSlabs()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var result = dbContext.Tbl20168VatpurchaseTaxSlabs
            .Select(x => new
            {
                x.PurchaseTaxSlabCode,
                x.PurchaseTaxSlab
            }).ToList();


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

            return Json(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet]
        public IActionResult GetTaxpercentage()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var data = dbContext.Tbl20168VatpurchaseTaxSlabs
                    .Select(c => new
                    {
                        c.PurchaseTaxSlabCode,
                        c.PurchaseTaxSlab
                    }).ToList();

                return Ok(data);
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet]
        public IActionResult ExpenseClaimVAT(long claimChildNo, string description, string approvedAmount)
        
        {
            ViewBag.ClaimChildNo = claimChildNo;
            ViewBag.ExpenseDescription = description;
            ViewBag.ApprovedAmount = approvedAmount;

            return PartialView("~/Areas/Finance/Views/_ExpenseClaimVAT.cshtml");
        }

        [HttpGet]
        public async Task<IActionResult> GetSupplierName(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var supplierDataQuery = dbContext.Tbl20103ExpenseClaimChildren
                        // Remove null/empty/whitespace supplier names first
                        .Where(i => i.SupplierName != null && i.SupplierName.Trim() != "")
                        // Group by trimmed name so " ABC " and "ABC" are treated the same
                        .GroupBy(i => i.SupplierName.Trim())
                        .Select(g => new
                        {
                            SupplierName = g.Key,
                            SupplierVATNo = g.Select(x => x.SupplierVatno).FirstOrDefault()
                        });

                    return Json(await DataSourceLoader.LoadAsync(supplierDataQuery, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetSupplierName: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet]
        public async Task<IActionResult> GetPurchaserNames(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var supplierDataQuery = dbContext.Tbl20103ExpenseClaimChildren
                        // Remove null/empty/whitespace supplier names first
                        .Where(i => i.PurchaserName != null && i.PurchaserName.Trim() != "")
                        // Group by trimmed name so " ABC " and "ABC" are treated the same
                        .GroupBy(i => i.PurchaserName.Trim())
                        .Select(g => new
                        {
                            PurchaserName = g.Key,
                            SupplierVATNo = g.Select(x => x.SupplierVatno).FirstOrDefault()
                        });

                    return Json(await DataSourceLoader.LoadAsync(supplierDataQuery, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetSupplierName: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }


        public IActionResult GetCostEmployees()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var data = dbContext.Tbl101Employees
                    .Select(c => new
                    {
                        c.EmployeeId,
                        c.EmployeeName,
                        c.EmployeeReferenceId,
                        c.NationalId
                    }).ToList();

                return Ok(data);
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet]
        public IActionResult GetCostProperty()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var data = dbContext.Qry40102PropertyMasterView2s
                    .Select(c => new
                    {
                        c.PropertyNo,
                        c.PropertyDescription,
                        c.PlateNo,
                        c.ChassisNo,
                        c.PropertyGroup,
                        c.MobilizedTo,
                        c.Brand,
                        c.Capacity,
                        c.ClientSite,
                        c.CurrentStatus,
                        c.DiscontinuedOn,
                        c.DiscontinuedRemarks,
                        c.DoorNo,
                        c.Model,
                        c.Operator,
                        c.PropertyCategoryName,
                        c.PropertySuppliedBy,
                        c.PropertyType,
                        c.Specifications,
                        c.Year

                    }).ToList();

                return Ok(data);
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpPost]
        public IActionResult SaveExpenseClaimEntry([FromBody] ExpenseClaimChildDto model)
        {
            if (model == null || model.ClaimChildNo <= 0)
                return BadRequest("Invalid data");

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var existingEntry = dbContext.Tbl20103ExpenseClaimChildren
                    .FirstOrDefault(x => x.ClaimChildNo == model.ClaimChildNo);

                if (existingEntry != null)
                {
                    // Update existing
                    existingEntry.TaxableAmount = model.TaxableAmount;
                    existingEntry.TaxAmount = model.TaxAmount;
                    existingEntry.RoundOff = model.RoundOff;
                    existingEntry.SupplierName = model.SupplierName;
                    existingEntry.SupplierVatno = model.SupplierVATNo;
                    existingEntry.PurchaserName = model.PurchaserName;
                    existingEntry.LineNarration = model.LineNarration;
                    existingEntry.EmployeeNo = model.EmployeeNo;
                    existingEntry.PropertyNo = model.PropertyNo;
                    existingEntry.IsTaxIncluded = model.IsTaxIncluded;
                    existingEntry.VatapplicableRate = model.VATApplicableRate;
                    existingEntry.SupplierNameAr = model.SupplierNameAr;
                }
                //else
                //{
                //    // Insert new
                //    var claimEntry = new Tbl20103ExpenseClaimChild
                //    {
                //        ClaimChildNo = model.ClaimChildNo,
                //        TaxableAmount = model.TaxableAmount,
                //        TaxAmount = model.TaxAmount,
                //        RoundOff = model.RoundOff,
                //        SupplierName = model.SupplierName,
                //        SupplierVatno = model.SupplierVATNo,
                //        PurchaserName = model.PurchaserName,
                //        LineNarration = model.LineNarration,
                //        EmployeeNo = model.EmployeeNo,
                //        PropertyNo = model.PropertyNo,
                //        IsTaxIncluded = model.IsTaxIncluded
                //    };

                //    dbContext.Tbl20103ExpenseClaimChildren.Add(claimEntry);
                //}

                dbContext.SaveChanges();

                return Ok(new { message = "Saved successfully." });
            }

            return BadRequest("Unable to resolve tenant database context.");
        }

        [HttpGet]
        public async Task<ActionResult> GetChildClaimsByVoucherNo(string voucherNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var result = await dbContext.Tbl20103ExpenseClaimChildren
   .Where(x => x.ClaimRefNo == voucherNo)
   .Select(x => new {
       x.ClaimRefNo,
       x.ClaimChildNo,
       x.ExpenseDescription,
       x.BillRefNo,
       x.BillDate,
       x.ClaimedAmount,
       x.ApprovedAmount,
       x.AccountId,
       x.CostCenterCode,
       AccountHead = dbContext.Tbl201ChartOfAccounts
                       .Where(a => a.AccountId == x.AccountId)
                       .Select(a => a.AccountHead)
                       .FirstOrDefault(),
       CostAllocationUnit = dbContext.Tbl201CostAllocationUnits
                       .Where(c => c.CostAllocationUnitId == x.CostCenterCode)
                       .Select(c => c.CostAllocationUnit)
                       .FirstOrDefault()
   })
   .ToListAsync();


                  return Json(result);
                

              
            }

            return Unauthorized(); // or BadRequest("Invalid tenant context");
        }

        [HttpPost]
        public IActionResult AddOrUpdateChildClaim([FromBody] Tbl20103ExpenseClaimChild child)
        {
            if (child == null)
                return BadRequest("Invalid data");

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                child.VoucherNo = null;
                if (child.ClaimChildNo > 0)
                {
                    // Update
                    var existing = dbContext.Tbl20103ExpenseClaimChildren
                        .FirstOrDefault(x => x.ClaimChildNo == child.ClaimChildNo);

                    if (existing != null)
                    {
                        dbContext.Entry(existing).CurrentValues.SetValues(child);
                    }
                }
                else
                {
                    // Insert
                    dbContext.Tbl20103ExpenseClaimChildren.Add(child);
                }

                dbContext.SaveChanges();
                return Ok(child);
            }

            return Unauthorized(); // or BadRequest("Tenant context could not be resolved");
        }
        [HttpGet]
        public async Task<ActionResult> GetClaimVAT(long claimchildno)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var result = await dbContext.Tbl20103ExpenseClaimChildren
   .Where(x => x.ClaimChildNo == claimchildno)
   .Select(x => new {
       x.ClaimRefNo,
       x.ClaimChildNo,
       x.ApprovedAmount,
       x.TaxableAmount,
       x.TaxAmount,
       x.RoundOff,
       x.SupplierName,
       x.SupplierNameAr,
       x.SupplierVatno,
       x.PurchaserName,
       x.LineNarration,
       x.EmployeeNo,
       x.VatapplicableRate,
       x.PropertyNo,
       EmployeeName = dbContext.Tbl101Employees
                       .Where(a => a.EmployeeId == x.EmployeeNo)
                       .Select(a => a.EmployeeName)
                       .FirstOrDefault(),
       propertyname = dbContext.Qry40102PropertyMasterView2s
                       .Where(c => c.PropertyNo == x.PropertyNo)
                       .Select(c => c.PropertyDescription)
                       .FirstOrDefault(),
       TaxRate = dbContext.Tbl20168VatpurchaseTaxSlabs
                       .Where(c => c.PurchaseTaxSlabCode == x.VatapplicableRate)
                       .Select(c => c.PurchaseTaxSlab)
                       .FirstOrDefault()
   })
   .ToListAsync();


                return Json(result);



            }

            return Unauthorized(); // or BadRequest("Invalid tenant context");
        }
        [HttpPost]
        public IActionResult DeleteExpenseChild([FromBody] long claimChildNo)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            try
            {
                var record = dbContext.Tbl20103ExpenseClaimChildren.FirstOrDefault(x => x.ClaimChildNo == claimChildNo);
                if (record != null)
                {
                    
                    dbContext.Tbl20103ExpenseClaimChildren.Remove(record);
                    
                    dbContext.SaveChanges();
                    return Ok(new { success = true });
                }

                return NotFound(new { success = false, message = "Record not found." });
            }
            catch (Exception ex)
            {
                // Optionally log exception
                return StatusCode(500, new { success = false, message = ex.Message });
            }

        }
        [HttpGet]
        public async Task<IActionResult> GetCPPaymentAccounts(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var qryListOfAccountlists = dbContext.Qry201ListOfAccounts
                        .Where(p => p.AccountGroupId == "A012")
                        .Select(i => new
                        {
                            i.AccountHead,
                            i.AccountId,
                            i.AccountHeadArabic,
                            i.IsLedgerObselete
                        });

                    var result = await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions);
                    return Json(result);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, "Internal server error. Please try again later.");
                }
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }


        [HttpGet]
        public async Task<ActionResult> GetBPPaymentAccounts(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var qryListOfAccountlists = dbContext.Qry201ListOfAccounts.Where(p => p.AccountGroupId == "A013").Select(i => new
                    {
                        i.AccountHead,
                        i.AccountId,
                        i.AccountHeadArabic,
                        i.IsLedgerObselete


                    });

                    return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpPost]
        public async Task<IActionResult> FinalizeClaimPayment([FromBody] ClaimPaymentDto dto, CancellationToken ct)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return BadRequest("Tenant context could not be determined.");

                var strategy = dbContext.Database.CreateExecutionStrategy();

                await strategy.ExecuteAsync(async () =>
                {
                    await using var tx = await dbContext.Database.BeginTransactionAsync(ct);

                    var master = await dbContext.Tbl20102ExpenseClaimMasters
                        .FirstOrDefaultAsync(x => x.ClaimRefNo == dto.ClaimRefNo, ct);

                    if (master is null)
                        throw new Exception($"ClaimRefNo '{dto.ClaimRefNo}' not found.");

                    master.IsPaid = true;
                    master.PaidBy = HttpContext.Session.GetString("UserName") ?? "System";
                    master.PaidOn = DateTime.Now;
                    master.PaymentAccount = dto.SelectedAccountHead;
                    master.PaymentVoucherNo = dto.PaymentVoucherNo;
                    master.PaymentType = dto.SelectedPaymentType;

                    await dbContext.SaveChangesAsync(ct);

                    await dbContext.Database.ExecuteSqlRawAsync(
                        "EXEC sp20105InsertClaimToVoucher @ClaimRefNo = {0}, @PaymentVoucherNo = {1}, @AddedBy = {2}, @AddedOn = {3}, @TotalAmount = {4}, @JustAddedVoucherEntryNo = {5}, @TypeOfClaim = {6}, @EffectiveDate = {7}",
                        dto.ClaimRefNo,
                        dto.PaymentVoucherNo,
                        master.PaidBy,
                        master.PaidOn,
                        dto.TotalAmount,
                        0,
                        dto.TypeOfClaim,
                        dto.EffectiveDate
                    );

                    await tx.CommitAsync(ct);
                });

                return Ok(new
                {
                    IsPaid = true,
                    PaidBy = HttpContext.Session.GetString("UserName") ?? "System",
                    PaidOn = DateTime.Now.ToString("dd-MMM-yyyy")
                });
            }
            catch (Exception ex)
            {
                // You can log ex here (ILogger, or any logging mechanism)
                return StatusCode(500, new
                {
                    Error = "An error occurred while finalizing claim payment.",
                    Details = ex.Message // Optionally include stacktrace or more details for debugging
                });
            }
        }
        [HttpPost]
        public async Task<IActionResult> InsertClaimMaster([FromBody] ClaimMasterDto dto)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            string userName = HttpContext.Session.GetString("UserName");
            string userIdStr = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userIdStr))
                return Unauthorized(new { success = false, message = "User session expired." });

            byte claimerId = Convert.ToByte(userIdStr);

            using var connection = dbContext.Database.GetDbConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "sp201_27InsertClaimMaster";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@ClaimRefNo", dto.ClaimRefNo));
            command.Parameters.Add(new SqlParameter("@ClaimRemarks", dto.ClaimRemarks));
            command.Parameters.Add(new SqlParameter("@ClaimCreatedBy", userName));
            command.Parameters.Add(new SqlParameter("@ClaimerID", claimerId));
            command.Parameters.Add(new SqlParameter("@FundRequestTypeID", 3));
            command.Parameters.Add(new SqlParameter("@SupplierPaymentLedgerNo", dto.SupplierPaymentLedgerNo));

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            return Ok(new { success = true });
        }
        [HttpPost]
       
        public async Task<IActionResult> AddExpenseClaimChildRecords([FromBody] List<Tbl20103ExpenseClaimChild> records)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            if (records == null || records.Count == 0)
            {
                return BadRequest(new { success = false, message = "No data provided." });
            }

            dbContext.Tbl20103ExpenseClaimChildren.AddRange(records);
            await dbContext.SaveChangesAsync();

            return Ok(new { success = true });
        }
        [HttpPost]
        public async Task<IActionResult> UpdateVoucheronload([FromBody] ExpenseClaimViewModel model)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

           

            try
            {
                // Get user session data
                string userIdStr = HttpContext.Session.GetString("UserId");
                byte claimerId = Convert.ToByte(userIdStr); // ✅ Convert string to byte

                string userName = HttpContext.Session.GetString("UserName");
                DateTime now = DateTime.Now;
                // Check if master record exists
                var existingMaster = await dbContext.Tbl20102ExpenseClaimMasters
                    .FirstOrDefaultAsync(m => m.ClaimRefNo == model.ClaimRefNo);

                if (existingMaster != null)
                {
                    // ✅ Update master record
                    existingMaster.ClaimDate = model.ClaimDate;
                    existingMaster.ClaimEffectiveDate = model.ClaimEffectiveDate;
                    existingMaster.ProjectClaimedFor = model.ProjectClaimedFor;
                    existingMaster.ClaimRemarks = model.ClaimRemarks;
                    existingMaster.ClaimModifiedBy = userName;
                    existingMaster.ClaimModifiedOn = now;
                    existingMaster.PaymentType = model.PaymentType;
                    existingMaster.PaymentAccount = model.PaymentAccount;
                    dbContext.Tbl20102ExpenseClaimMasters.Update(existingMaster);
                }
                else
                {
                    // ✅ Insert new master
                    var newMaster = new Tbl20102ExpenseClaimMaster
                    {
                        ClaimRefNo = model.ClaimRefNo,
                        ClaimDate = model.ClaimDate,
                        ClaimEffectiveDate = model.ClaimEffectiveDate,
                        ProjectClaimedFor = model.ProjectClaimedFor,
                        ClaimRemarks = model.ClaimRemarks,
                        ClaimerId = claimerId,
                        PaymentType = model.PaymentType,
                        PaymentAccount = model.PaymentAccount,
                        ClaimCreatedBy = userName,
                        ClaimCreatedOn = now,
                        FundRequestTypeId = model.FundRequestTypeId
                    };

                    dbContext.Tbl20102ExpenseClaimMasters.Add(newMaster);
                }

                await dbContext.SaveChangesAsync();
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        public IActionResult CheckClaimStatus(string voucherNo)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            if (string.IsNullOrWhiteSpace(voucherNo))
                return BadRequest(new { success = false, message = "Voucher number is required." });

            var claim = dbContext.Tbl20102ExpenseClaimMasters
                .Where(c => c.ClaimRefNo == voucherNo)
                .Select(c => new
                {
                    c.IsSubmittedToFinance,
                    c.IsApproved,
                    c.IsPaid
                })
                .FirstOrDefault();

            if (claim == null)
                return NotFound(new { success = false, message = "Claim not found." });

            return Json(claim);
        }
        [HttpPost]
        public IActionResult DeleteClaim([FromForm] string voucherNo)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            if (string.IsNullOrWhiteSpace(voucherNo))
                return Json(new { success = false, message = "Voucher number is required." });

            var strategy = dbContext.Database.CreateExecutionStrategy();

            try
            {
                strategy.Execute(async () =>
                {
                    using var transaction = dbContext.Database.BeginTransaction();

                    var master = dbContext.Tbl20102ExpenseClaimMasters
                        .FirstOrDefault(c => c.ClaimRefNo == voucherNo);

                    if (master == null)
                        throw new Exception("Claim not found.");

                    var children = dbContext.Tbl20103ExpenseClaimChildren
                        .Where(c => c.ClaimRefNo == voucherNo)
                        .ToList();

                    dbContext.Tbl20103ExpenseClaimChildren.RemoveRange(children);
                    dbContext.Tbl20102ExpenseClaimMasters.Remove(master);

                    dbContext.SaveChanges();

                    await _userActionLogger.LogAsync(
          module: "Finance > Expense Claim",
          actionDetail: $"Deleted Claim: {master.ClaimRefNo}",
          documentNo: master.ClaimRefNo
      );
                    transaction.Commit();
                });

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        public IActionResult GetJournalStatus(string claimRefNo)
        {
            if (string.IsNullOrEmpty(claimRefNo))
                return BadRequest("Invalid JournalRefNo.");

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var journal = dbContext.Tbl20102ExpenseClaimMasters
                    .Where(j => j.ClaimRefNo == claimRefNo)
                    .Select(j => new
                    {
                        isSubmitted = j.IsSubmittedToFinance,
                        isApproved = j.IsApproved,
                        isPaid = j.IsPaid
                    })
                    .FirstOrDefault();

                if (journal == null)
                    return NotFound();

                return Json(journal);
            }

            return StatusCode(500, "Tenant context could not be loaded.");
        }

        // POST: /Finance/DeleteJournalEntry
        [HttpPost]
        public IActionResult DeleteJournalEntry(string claimRefNo)
        {
            if (string.IsNullOrEmpty(claimRefNo))
                return BadRequest("Invalid JournalRefNo.");

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var master = dbContext.Tbl20102ExpenseClaimMasters
                    .FirstOrDefault(j => j.ClaimRefNo == claimRefNo);

                if (master == null)
                    return NotFound();

                var childList = dbContext.Tbl20103ExpenseClaimChildren
                    .Where(c => c.ClaimRefNo == claimRefNo)
                    .ToList();
                


                dbContext.Tbl20103ExpenseClaimChildren.RemoveRange(childList);
                dbContext.Tbl20102ExpenseClaimMasters.Remove(master);
                
                dbContext.SaveChanges();

                return Json(new { success = true });
            }

            return StatusCode(500, "Tenant context could not be loaded.");
        }
        [HttpPost]
        public IActionResult UnlockJournalEntry(string claimRefNo)
        {
            if (string.IsNullOrEmpty(claimRefNo))
                return BadRequest(new { success = false, message = "JournalRefNo is required." });

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            var entry = dbContext.Tbl20102ExpenseClaimMasters
                .FirstOrDefault(j => j.ClaimRefNo == claimRefNo);

            if (entry == null)
                return NotFound(new { success = false, message = "Journal entry not found." });



            // Reset fields
            entry.IsSubmittedToFinance = false;
            entry.SubmittedBy = null;
            entry.SubmittedOn = null;
            entry.IsVerified = false;
            entry.VerifiedBy = null;
            entry.VerifiedOn = null;
            entry.IsApproved = false;
            entry.ApprovedBy = null;
            entry.ApprovedOn = null;

            dbContext.SaveChanges();

            return Ok(new { success = true });
        }
    }
}


