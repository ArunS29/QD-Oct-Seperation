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

        [HttpPost]
        public IActionResult DeleteRecord(string id)
        {
            try
            {
                // Validate the ID
                if (string.IsNullOrEmpty(id))
                {
                    return Json(new { success = false, message = "Invalid Reference No provided." });
                }

                // Find the record in the database
                var record = _context.Tbl201SubLedgerMasters.FirstOrDefault(r => r.ReferenceNo == id);
                if (record == null)
                {
                    return Json(new { success = false, message = "Record not found." });
                }

                // Remove the record from the database
                _context.Tbl201SubLedgerMasters.Remove(record);
                _context.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                //  _logger.LogError(ex, "Error occurred while deleting record with Reference No: {id}", id);
                return Json(new { success = false, message = "An error occurred while deleting the record." });
            }
        }


    }
}


