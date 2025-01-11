using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using QD.ERP.Web.DAL.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FormQD.ERP.Web.Areas.Finance.Controllers
{

    [Route("/Finance/api/[controller]/[action]")]
    [ApiController]

    public class BillsPayableReceivableController : Controller
    {
        private ERPMasterWtDataContext _context;
        public BillsPayableReceivableController(ERPMasterWtDataContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult> GetVoucherEntries(DataSourceLoadOptions loadOptions, string ReferenceNo)
        {
            var qryListOfAccountlists = _context.Tbl201SubLedgerMasters.Where(p => p.ReferenceNo == ReferenceNo).Select(i => new
            {

                i.DrCr,
                i.ReferenceType,
                i.ReferenceNo,
                i.Amount
            });

            return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
        }

        [HttpPost]
        public async Task<ActionResult> AddBillsPayableReceivable(DataSourceLoadOptions loadOptions, [FromBody] Tbl201SubLedgerMaster VE)
        {
            if (VE == null)
            {
                return BadRequest(new { success = false, message = "Invalid data received." });
            }

            try
            {
                // Add the new voucher entry
                _context.Tbl201SubLedgerMasters.Add(VE);
                await _context.SaveChangesAsync();
                var qryListOfAccountlists = _context.Tbl201SubLedgerMasters.Where(p => p.ReferenceNo == VE.ReferenceNo).Select(i => new
                {

                    i.DrCr,
                    i.ReferenceType,
                    i.ReferenceNo,
                    i.Amount
                });

                return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
                // Provide a success response
                //return Ok(new { success = true, message = "Data inserted successfully!" });
            }
            catch (Exception ex)
            {
                // Handle errors gracefully
                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }
        }




        [HttpPost]
        public async Task<ActionResult> UpdateBillsPayableReceivable([FromBody] Tbl201SubLedgerMaster VM)
        {
            if (VM == null)
            {
                return BadRequest(new { success = false, message = "Invalid data received." });
            }

            try
            {
                _context.Tbl201SubLedgerMasters.Add(VM);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Data inserted successfully!" });
            }
            catch (Exception ex)
            {

                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }


        }



    }
}


