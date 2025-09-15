using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using QD.ERP.Web.Services.Logging;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class EmployeeAllocationController : Controller
    {
		private ERPMasterWtDataContext _context;
		private readonly TenantDbContextHelper _tenantDbContextHelper;
		private readonly ILogger<EmployeeAllocationController> _logger;
        private readonly IUserActionLogger _userActionLogger;
        public EmployeeAllocationController(ILogger<EmployeeAllocationController> logger, IUserActionLogger userActionLogger, TenantDbContextHelper tenantDbContextHelper, ERPMasterWtDataContext context)
		{
			_tenantDbContextHelper = tenantDbContextHelper;
			_logger = logger;
			_context = context;
            _userActionLogger = userActionLogger;
        }

		public IActionResult EmployeeAllocation(string voucherNo, string accountHead, string voucherAmount, string drCr, string effectiveDate)
        {
            // Log or debug the incoming parameters
            ViewBag.VoucherNo = voucherNo;
            ViewBag.AccountHead = accountHead;
            ViewBag.VoucherAmount = voucherAmount;
            ViewBag.DrCr = drCr;
            ViewBag.EffectiveDate = effectiveDate;

            return View();
        }


        [HttpGet]
        public IActionResult GetEmployeeName()
        {
			if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				return Unauthorized(new { success = false, message = "Invalid tenant." });
			}
			var data = dbContext.Tbl101Employees
                .Select(c => new
                {
                    c.EmployeeId,
                    c.EmployeeName,
                    c.NationalId

                }).ToList();

            return Ok(data);
        }
        [HttpPost]
        public async Task<ActionResult> SaveEmployeeAllocation([FromBody] Tbl20129JournalRegisterEmployeeAllocation EM)
        {

            try
            {
                _context.Tbl20129JournalRegisterEmployeeAllocations.Add(EM);
                await _context.SaveChangesAsync();
                await _userActionLogger.LogAsync(
                module: "Finance > Employee Allocation",
                actionDetail: $"Saveded Employee Allocation: {EM.VoucherNo}",
                documentNo: EM.VoucherNo
                );
                //return Json(new { VoucherEntryNo = VE.VoucherNo });
                return Ok(new { success = true, message = "Data inserted successfully!" });
            }
            catch (Exception ex)
            {

                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }


        }
        [HttpGet]
        public IActionResult GetEmployeeAllocationsBydatagrid(long voucherEntryId)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            var result = (from alloc in dbContext.Tbl20104EmployeeAllocationMasters
                          join unit in dbContext.Tbl101Employees
                              on alloc.EmployeeNo equals unit.EmployeeId into gj
                          from unit in gj.DefaultIfEmpty()
                          where alloc.VoucherEntryId == voucherEntryId
                          select new
                          {
                              EmployeeAllocationId = alloc.EmployeeAllocationId,
                              DrCr = alloc.EmpAllocDrCr,
                              EmployeeName = unit != null ? unit.EmployeeName : "",
                              EmployeeNo = alloc.EmployeeNo,
                              EffectiveDate = alloc.EffectiveDate,
                              VoucherAmount = alloc.AmountAllocated,
                              Remarks = alloc.CostAllocRemarks
                          }).ToList();

            return Json(result);
        }
        [HttpGet]
        public async Task<IActionResult> CheckEmployeeAllocation(string AccountHead, string AccountID)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }
            try
            {
                var allocation = await dbContext.Tbl201ChartOfAccounts
                    .Where(x => x.AccountHead == AccountHead && x.AccountId == AccountID && x.IsEmployeeAllocated == true)
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
                // Log the error here if necessary
                return StatusCode(500, new { message = "An error occurred while checking property allocation.", error = ex.Message });
            }
        }
        public IActionResult InsertDefaultEmployeeAllocation([FromBody] EmployeeAllocationDto dto)
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
                var entity = new Tbl20104EmployeeAllocationMaster
                {
                    EmployeeAllocationId = 0,
                    EmpAllocDrCr = dto.EmpAllocDrCr,
                    EmployeeNo = dto.EmployeeNo,
                    EffectiveDate = dto.EffectiveDate,
                    AmountAllocated = dto.AmountAllocated,
                    CostAllocRemarks = dto.CostAllocRemarks,
                    VoucherEntryId = dto.VoucherEntryId,
                    VoucherNo = dto.VoucherNo,
                    LedgerAccountNo = dto.LedgerAccountNo,
                    CurrencyRate = dto.CurrencyRate
                };

                dbContext.Tbl20104EmployeeAllocationMasters.Add(entity);
                dbContext.SaveChanges();

                return Ok(new { success = true, id = entity.EmployeeAllocationId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error inserting cost allocation.", error = ex.Message });
            }
        }
        public IActionResult UpdateEmployeeAllocationWithFields([FromBody] UpdateEmployeeAllocationFieldsDto dto)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { success = false, message = "Invalid tenant." });

            var record = dbContext.Tbl20104EmployeeAllocationMasters.FirstOrDefault(x => x.EmployeeAllocationId == dto.EmployeeAllocationId);
            if (record == null)
                return NotFound(new { success = false, message = "Record not found." });

            record.AmountAllocated = dto.VoucherAmount;
            record.CostAllocRemarks = dto.CostAllocRemarks;
            record.EmployeeNo = dto.EmployeeNo;
            record.EffectiveDate = dto.EffectiveDate;

            dbContext.SaveChanges();

            return Ok(new { success = true });
        }
        public IActionResult DeleteEmployeeAllocation(long employeeAllocationId)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }



            var allocations = dbContext.Tbl20104EmployeeAllocationMasters
                .Where(x => x.EmployeeAllocationId == employeeAllocationId)
                .ToList();

            if (!allocations.Any())
            {
                return NotFound(new { success = false, message = "No records found to delete." });
            }

            dbContext.Tbl20104EmployeeAllocationMasters.RemoveRange(allocations);
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

            var allocations = dbContext.Tbl20104EmployeeAllocationMasters
                .Where(x => x.VoucherNo == request.VoucherNo && x.VoucherEntryId == request.VoucherEntryId)
                .ToList();

            if (!allocations.Any())
            {
                return NotFound(new { success = false, message = "No records found to delete." });
            }

            dbContext.Tbl20104EmployeeAllocationMasters.RemoveRange(allocations);
            dbContext.SaveChanges();

            return Ok(new { success = true, message = "All cost allocation records deleted for voucher." });
        }
        [HttpPost]
        public IActionResult SaveEmployeeAllocations([FromBody] List<Tbl20104EmployeeAllocationMaster> allocations)
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
            var existingAllocations = dbContext.Tbl20104EmployeeAllocationMasters
                .Where(x => x.VoucherEntryId == voucherEntryId)
                .ToList();

            foreach (var allocation in allocations)
            {
                var existing = dbContext.Tbl20104EmployeeAllocationMasters
                    .FirstOrDefault(x => x.EmployeeAllocationId == allocation.EmployeeAllocationId);

                if (existing != null && allocation.EmployeeAllocationId > 0)
                {
                    // Update existing record
                    existing.EmployeeNo = allocation.EmployeeNo;
                    existing.EffectiveDate = allocation.EffectiveDate;
                    existing.AmountAllocated = allocation.AmountAllocated;
                    existing.CostAllocRemarks = allocation.CostAllocRemarks;
                    existing.VoucherNo = allocation.VoucherNo;
                    existing.EmpAllocDrCr = allocation.EmpAllocDrCr;
                    existing.LedgerAccountNo = allocation.LedgerAccountNo;
                    existing.ModifiedBy = userName;
                    existing.ModifiedOn = now;
                }
                else
                {
                    // Insert new record
                    dbContext.Tbl20104EmployeeAllocationMasters.Add(new Tbl20104EmployeeAllocationMaster
                    {
                        EmployeeNo = allocation.EmployeeNo,
                        EffectiveDate = allocation.EffectiveDate,
                        AmountAllocated = allocation.AmountAllocated,
                        CostAllocRemarks = allocation.CostAllocRemarks,
                        VoucherEntryId = allocation.VoucherEntryId,
                        VoucherNo = allocation.VoucherNo,
                        EmpAllocDrCr = allocation.EmpAllocDrCr,
                        LedgerAccountNo = allocation.LedgerAccountNo,
                        EnteredBy = userName,
                        EnteredOn = now
                    });
                }
            }

            dbContext.SaveChanges();

            return Ok(new { success = true, message = "Property allocations saved successfully." });
        }
    }
}
