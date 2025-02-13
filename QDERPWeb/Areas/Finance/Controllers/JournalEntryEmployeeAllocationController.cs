using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.DAL.Entities;


namespace QD.ERP.Web.Areas.Finance.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class JournalEntryEmployeeAllocationController : Controller
	{
		private ERPMasterWtDataContext _context;

		public JournalEntryEmployeeAllocationController(ERPMasterWtDataContext context)
		{
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
			var data = _context.Tbl101Employees
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
			try
			{
				foreach (var id in rowKeys)
				{
					var item = _context.Tbl20129JournalRegisterEmployeeAllocations.Find(id);
					if (item != null)
					{
						_context.Tbl20129JournalRegisterEmployeeAllocations.Remove(item);
					}
				}
				_context.SaveChanges();
				return Json(new { success = true });

			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
	}
}
