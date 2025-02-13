using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.DAL.Entities;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class JournalEntryCostAllocationController : Controller
    {

        private ERPMasterWtDataContext _context;

        public JournalEntryCostAllocationController(ERPMasterWtDataContext context)
        {
            _context = context;
        }
        //[HttpGet("GetCostAllocation")]
        //public IActionResult GetCostAllocation()
        //{
        //    try
        //    {
        //        var data = _context.Tbl201CostAllocationMasters.Select(e => new
        //        {
        //            e.CostAllocDrCr,
        //            e.EffectiveDate,
        //            e.AmountAllocated,
        //            e.CostAllocRemarks
        //        }).ToList();

        //        return Json(data);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { message = "An error occurred while fetching data.", error = ex.Message });
        //    }
        //}
        public IActionResult CostAllocation(string voucherNo, string accountHead, string voucherAmount, string drCr, string effectiveDate)
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
        public IActionResult GetCostAllocationUnits()
        {
            var data = _context.Tbl201CostAllocationUnits
                .Select(c => new
                {
                    c.CostAllocationUnitId,
                    c.CostAllocationUnit,
                    c.CostAllocationGroup,
                    c.IsDisabled
                }).ToList();

            return Ok(data);
        }

        [HttpPost]
        public async Task<ActionResult> SaveCostAllocation([FromBody] Tbl20128JournalRegisterCostAllocation CM)
        {

            try
            {
                // Fetch the latest VoucherEntryID from the database
                long maxVoucherEntryID = await _context.Tbl20128JournalRegisterCostAllocations
                    .OrderByDescending(x => x.JournalChildNo)
                    .Select(x => x.JournalChildNo)
                    .FirstOrDefaultAsync();

                // Increment the VoucherEntryID
                CM.JournalChildNo = maxVoucherEntryID + 1;

                _context.Tbl20128JournalRegisterCostAllocations.Add(CM);
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
                    var item = _context.Tbl20128JournalRegisterCostAllocations.Find(id);
                    if (item != null)
                    {
                        _context.Tbl20128JournalRegisterCostAllocations.Remove(item);
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
