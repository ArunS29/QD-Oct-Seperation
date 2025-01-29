using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.DAL.Entities;


namespace QD.ERP.Web.Areas.Finance.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class PropertyAllocationController : Controller
	{
		private ERPMasterWtDataContext _context;

		public PropertyAllocationController(ERPMasterWtDataContext context)
		{
			_context = context;
		}

		public IActionResult PropertyAllocation(string voucherNo, string accountHead, string voucherAmount, string drCr, string accountId, string effectiveDate)
		{
			// Log or debug the incoming parameters
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
            try
            {
                var allocation = await _context.Tbl201ChartOfAccounts
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
                // Log the error here if necessary
                return StatusCode(500, new { message = "An error occurred while checking property allocation.", error = ex.Message });
            }
        }
		[HttpGet]
		public IActionResult GetPropertyAllocationUnits()
		{
			var data = _context.Qry40102PropertyMasterView2s
				.Select(c => new
				{
					c.PropertyNo,
					c.PropertyDescription,
					c.PropertyType,
					c.PlateNo
				}).ToList();

			return Ok(data);
		}

		[HttpPost]
		public async Task<ActionResult> SavePropertyAllocation([FromBody] Tbl20122PropertyAllocationMaster CM)
		{

			try
			{
				// Fetch the latest VoucherEntryID from the database
				long maxVoucherEntryID = await _context.Tbl20122PropertyAllocationMasters
					.OrderByDescending(x => x.VoucherEntryId)
					.Select(x => x.VoucherEntryId)
					.FirstOrDefaultAsync();

				// Increment the VoucherEntryID
				CM.VoucherEntryId = maxVoucherEntryID + 1;

				_context.Tbl20122PropertyAllocationMasters.Add(CM);
				await _context.SaveChangesAsync();
				//return Json(new { VoucherEntryNo = VE.VoucherNo });
				return Ok(new { success = true, message = "Data inserted successfully!" });
			}
			catch (Exception ex)
			{

				return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
			}


		}

	}
}
