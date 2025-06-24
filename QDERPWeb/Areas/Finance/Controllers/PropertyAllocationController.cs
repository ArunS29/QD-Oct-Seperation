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
    public class PropertyAllocationController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<PropertyAllocationController> _logger;

        public PropertyAllocationController(ILogger<PropertyAllocationController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        public IActionResult PropertyAllocation(string voucherNo, string accountHead, string voucherAmount, string drCr, string accountId, string effectiveDate)
        {
            ViewBag.VoucherNo = voucherNo;
            ViewBag.AccountHeadVal = accountHead;
            ViewBag.VoucherAmount = voucherAmount;
            ViewBag.DrCr = drCr;
            ViewBag.AccountID = accountId;
            ViewBag.EffectiveDate = effectiveDate;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> CheckPropertyAllocation(string AccountHead, string AccountID)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var allocation = await dbContext.Tbl201ChartOfAccounts
                        .Where(x => x.AccountHead == AccountHead && x.AccountId == AccountID && x.IsPropertyAllocated == true)
                        .FirstOrDefaultAsync();

                    if (allocation != null)
                    {
                        return Ok(new { isAllocated = true });
                    }
                    else
                    {
                        return Ok(new { isAllocated = false });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in CheckPropertyAllocation: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while checking property allocation.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public IActionResult GetPropertyAllocationUnits()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var data = dbContext.Qry40102PropertyMasterView2s
                    .Select(c => new
                    {
                        c.PropertyNo,
                        c.PropertyDescription,
                        c.PropertyType,
                        c.PlateNo
                    }).ToList();

                return Ok(data);
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

		[HttpPost]
		public IActionResult SavePropertyAllocations([FromBody] List<Tbl20122PropertyAllocationMaster> allocations)
		{
			if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				return Unauthorized(new { success = false, message = "Invalid tenant." });
			}

			if (allocations == null || !allocations.Any())
			{
				return BadRequest("No property allocation data received.");
			}

			string userName = HttpContext.Session.GetString("UserName");
			DateTime now = DateTime.Now;
			var voucherEntryId = allocations.First().VoucherEntryId;

			// Fetch all existing allocations for the VoucherEntryId
			var existingAllocations = dbContext.Tbl20122PropertyAllocationMasters
				.Where(x => x.VoucherEntryId == voucherEntryId)
				.ToList();

			foreach (var allocation in allocations)
			{
				var existing = dbContext.Tbl20122PropertyAllocationMasters
					.FirstOrDefault(x => x.PropertyAllocationId == allocation.PropertyAllocationId);

				if (existing != null && allocation.PropertyAllocationId > 0)
				{
					// Update existing record
					existing.PropertyNo = allocation.PropertyNo;
					existing.EffectiveDate = allocation.EffectiveDate;
					existing.AmountAllocated = allocation.AmountAllocated;
					existing.PropertyAllocRemarks = allocation.PropertyAllocRemarks;
					existing.VoucherNo = allocation.VoucherNo;
					existing.PropertyAllocDrCr = allocation.PropertyAllocDrCr;
					existing.LedgerAccountNo = allocation.LedgerAccountNo;
					existing.ModifiedBy = userName;
					existing.ModifiedOn = now;
				}
				else
				{
					// Insert new record
					dbContext.Tbl20122PropertyAllocationMasters.Add(new Tbl20122PropertyAllocationMaster
					{
						PropertyNo = allocation.PropertyNo,
						EffectiveDate = allocation.EffectiveDate,
						AmountAllocated = allocation.AmountAllocated,
						PropertyAllocRemarks = allocation.PropertyAllocRemarks,
						VoucherEntryId = allocation.VoucherEntryId,
						VoucherNo = allocation.VoucherNo,
						PropertyAllocDrCr = allocation.PropertyAllocDrCr,
						LedgerAccountNo = allocation.LedgerAccountNo,
						EnteredBy = userName,
						EnteredOn = now
					});
				}
			}

			dbContext.SaveChanges();

			return Ok(new { success = true, message = "Property allocations saved successfully." });
		}

		[HttpGet]
		public IActionResult GetPropertyAllocationsBydatagrid(long voucherEntryId)
		{
			if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				return Unauthorized(new { success = false, message = "Invalid tenant." });
			}

			var result = (from alloc in dbContext.Tbl20122PropertyAllocationMasters
						  join unit in dbContext.Tbl40101PropertyMasters
							  on alloc.PropertyNo equals unit.PropertyNo into gj
						  from unit in gj.DefaultIfEmpty()
						  where alloc.VoucherEntryId == voucherEntryId
						  select new
						  {
							  PropertyAllocationId = alloc.PropertyAllocationId,
							  DrCr = alloc.PropertyAllocDrCr,
							  PropertyDescription = unit != null ? unit.PropertyDescription : "Common Overheads",
							  PropertyNo = alloc.PropertyNo,
							  EffectiveDate = alloc.EffectiveDate,
							  VoucherAmount = alloc.AmountAllocated,
							  Remarks = alloc.PropertyAllocRemarks
						  }).ToList();

			return Json(result);
		}
	}
}





