using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.DAL.Entities;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CostAllocationController : Controller
    {
        private ERPMasterWtDataContext _context;

        public CostAllocationController(ERPMasterWtDataContext context)
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
        public async Task<ActionResult> SaveCostAllocation([FromBody] Tbl201CostAllocationMaster CM)
        {

            try
            {
                _context.Tbl201CostAllocationMasters.Add(CM);
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
