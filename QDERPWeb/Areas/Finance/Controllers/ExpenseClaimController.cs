using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.Areas.Finance.Views;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ExpenseClaimController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<ExpenseClaimController> _logger;

        public ExpenseClaimController(ILogger<ExpenseClaimController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
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
        //[HttpGet]
        //public async Task<IActionResult> GetReceivingAccount(DataSourceLoadOptions loadOptions)
        //{
        //    if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
        //    {

        //        try
        //        {
        //            var tbl20101salespersonmasters = await dbContext.Tbl20103ExpenseClaimChildren.Select(i => new
        //            {

        //                i.AccountId,
        //                i.ClaimChildNo,
        //                i.ClaimRefNo,
        //            });

        //            return Json(await DataSourceLoader.LoadAsync(tbl20101salespersonmasters, loadOptions));
        //        }
        //        catch (Exception ex)
        //        {
        //            return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
        //        }
        //    }
        //}
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
     FROM tbl20103ExpenseClaimChild
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
     FROM tbl20103ExpenseClaimChild
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
                    existingMaster.ProjectClaimedFor = model.ProjectClaimedFor;
                    existingMaster.ClaimRemarks = model.ClaimRemarks;
                    existingMaster.ClaimModifiedBy = userName;
                    existingMaster.ClaimModifiedOn = now;
                    dbContext.Tbl20102ExpenseClaimMasters.Update(existingMaster);
                }
                else
                {
                    // ✅ Insert new master
                    var newMaster = new Tbl20102ExpenseClaimMaster
                    {
                        ClaimRefNo = model.ClaimRefNo,
                        ClaimDate = model.ClaimDate,
                        ProjectClaimedFor = model.ProjectClaimedFor,
                        ClaimRemarks = model.ClaimRemarks,
                        ClaimerId = claimerId,
                        ClaimCreatedBy = userName,
                        ClaimCreatedOn = now
                    };

                    dbContext.Tbl20102ExpenseClaimMasters.Add(newMaster);
                }

                // ✅ Remove existing child rows for this ClaimRefNo
                var existingChildren = dbContext.Tbl20103ExpenseClaimChildren
                    .Where(c => c.ClaimRefNo == model.ClaimRefNo);

                dbContext.Tbl20103ExpenseClaimChildren.RemoveRange(existingChildren);

                // ✅ Add new child rows
                foreach (var item in model.ExpenseDetails)
                {
                    var child = new Tbl20103ExpenseClaimChild
                    {
                        ClaimRefNo = model.ClaimRefNo,
                        ExpenseDescription = item.ExpenseDescription,
                        BillRefNo = item.BillRefNo,
                        BillDate = item.BillDate,
                        ClaimedAmount = item.ClaimedAmount,
                        ApprovedAmount = item.ApprovedAmount,
                        AccountId = item.AccountId,
                        CostCenterCode = item.CostCenterCode
                    };

                    dbContext.Tbl20103ExpenseClaimChildren.Add(child);
                }

                await dbContext.SaveChangesAsync();

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
                var submittedOn = DateTime.Now;

                var claim = await dbContext.Tbl20102ExpenseClaimMasters
                    .FirstOrDefaultAsync(c => c.ClaimRefNo == model.ClaimRefNo);

                if (claim != null)
                {
                    claim.IsSubmittedToFinance = true;
                    claim.SubmittedBy = userName;
                    claim.SubmittedOn = submittedOn;

                    await dbContext.SaveChangesAsync();

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



    }
}


