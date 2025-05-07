using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class EmployeeAllocationController : Controller
    {
		private ERPMasterWtDataContext _context;
		private readonly TenantDbContextHelper _tenantDbContextHelper;
		private readonly ILogger<EmployeeAllocationController> _logger;

		public EmployeeAllocationController(ILogger<EmployeeAllocationController> logger, TenantDbContextHelper tenantDbContextHelper, ERPMasterWtDataContext context)
		{
			_tenantDbContextHelper = tenantDbContextHelper;
			_logger = logger;
			_context = context;
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
                //return Json(new { VoucherEntryNo = VE.VoucherNo });
                return Ok(new { success = true, message = "Data inserted successfully!" });
            }
            catch (Exception ex)
            {

                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }


        }
        [HttpPost]
        public IActionResult Delete(List<int> rowKeys)
        {
			if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				return Unauthorized(new { success = false, message = "Invalid tenant." });
			}
			try
            {
                foreach (var id in rowKeys)
                {
                    var item = dbContext.Tbl20129JournalRegisterEmployeeAllocations.Find(id);
                    if (item != null)
                    {
						dbContext.Tbl20129JournalRegisterEmployeeAllocations.Remove(item);
                    }
                }
				dbContext.SaveChanges();
                return Json(new { success = true });

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
