using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QD.ERP.Shared.Models;
using QD.ERP.Shared.DAL.Entities;
using QD.ERP.Shared.Service;
using QD.ERP.Shared.Services.Logging;
using Microsoft.AspNetCore.Http;

namespace QD.ERP.Finance.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PropertyAllocationController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<PropertyAllocationController> _logger;
        private readonly IUserActionLogger _userActionLogger;
        public PropertyAllocationController(ILogger<PropertyAllocationController> logger, IUserActionLogger userActionLogger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
            _userActionLogger = userActionLogger;
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
		public async Task<IActionResult> SavePropertyAllocationsAsync([FromBody] List<Tbl20122PropertyAllocationMaster> allocations)
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
                    await _userActionLogger.LogAsync(
                    module: "Finance > Property Allocation",
                    actionDetail: $"Updated Property Allocation: {existing.VoucherNo}",
                    documentNo: existing.VoucherNo
                    );
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
                    await _userActionLogger.LogAsync(
                    module: "Finance > Property Allocation",
                    actionDetail: $"Saved Property Aloocation: {allocation.VoucherNo}",
                    documentNo: allocation.VoucherNo
                    );
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
							  PropertyDescription = unit != null ? unit.PropertyDescription : "",
							  PropertyNo = alloc.PropertyNo,
							  EffectiveDate = alloc.EffectiveDate,
							  VoucherAmount = alloc.AmountAllocated,
							  Remarks = alloc.PropertyAllocRemarks
						  }).ToList();

			return Json(result);
        }
        [HttpPost]
        public IActionResult InsertDefaultPropertyAllocation([FromBody] PropertyAllocationDto dto)
        {
            // Validate tenant and get context
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            if (dto == null)
            {
                return BadRequest(new { success = false, message = "Invalid data." });
            }

            try
            {
                // Map DTO to Entity
                var entity = new Tbl20122PropertyAllocationMaster
                {
                    PropertyAllocationId = 0,
                    PropertyAllocDrCr = dto.PropertyAllocDrCr,
                    PropertyNo = dto.PropertyNo,
                    EffectiveDate = dto.EffectiveDate,
                    AmountAllocated = dto.AmountAllocated,
                    PropertyAllocRemarks = dto.PropertyAllocRemarks,
                    VoucherEntryId = dto.VoucherEntryId,
                    VoucherNo = dto.VoucherNo,
                    LedgerAccountNo = dto.LedgerAccountNo,
                    CurrencyRate = dto.CurrencyRate
                };

                dbContext.Tbl20122PropertyAllocationMasters.Add(entity);
                dbContext.SaveChanges();

                return Ok(new { success = true, id = entity.PropertyAllocationId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error inserting cost allocation.", error = ex.Message });
            }
        }
        public IActionResult UpdatePropertyAllocationWithFields([FromBody] UpdatePropertyAllocationFieldsDto dto)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { success = false, message = "Invalid tenant." });

            var record = dbContext.Tbl20122PropertyAllocationMasters.FirstOrDefault(x => x.PropertyAllocationId == dto.PropertyAllocationId);
            if (record == null)
                return NotFound(new { success = false, message = "Record not found." });

            record.AmountAllocated = dto.VoucherAmount;
            record.PropertyAllocRemarks = dto.PropertyAllocRemarks;
            record.PropertyNo = dto.PropertyNo;
            record.EffectiveDate = dto.EffectiveDate;

            dbContext.SaveChanges();

            return Ok(new { success = true });
        }
        [HttpPost]

        public IActionResult DeletePropertyAllocation(long propertyAllocationId)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }



            var allocations = dbContext.Tbl20122PropertyAllocationMasters
                .Where(x => x.PropertyAllocationId == propertyAllocationId)
                .ToList();

            if (!allocations.Any())
            {
                return NotFound(new { success = false, message = "No records found to delete." });
            }

            dbContext.Tbl20122PropertyAllocationMasters.RemoveRange(allocations);
            dbContext.SaveChanges();

            return Ok(new { success = true, message = "All cost allocation records deleted for voucher." });
        }
        [HttpPost]

        public IActionResult DeleteByVoucherNo([FromBody] VoucherDeleteRequest request)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            if (string.IsNullOrEmpty(request.VoucherNo))
            {
                return BadRequest(new { success = false, message = "Voucher number is required." });
            }

            var allocations = dbContext.Tbl20122PropertyAllocationMasters
                .Where(x => x.VoucherNo == request.VoucherNo && x.VoucherEntryId == request.VoucherEntryId)
                .ToList();

            if (!allocations.Any())
            {
                return NotFound(new { success = false, message = "No records found to delete." });
            }

            dbContext.Tbl20122PropertyAllocationMasters.RemoveRange(allocations);
            dbContext.SaveChanges();

            return Ok(new { success = true, message = "All cost allocation records deleted for voucher." });
        }
    }
}





