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
    }
}


