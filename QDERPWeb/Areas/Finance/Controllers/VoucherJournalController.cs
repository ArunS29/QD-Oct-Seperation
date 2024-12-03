using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.DAL.Entities;

using Microsoft.EntityFrameworkCore;


namespace QDWEB.Areas.Finance.Controllers
{//[Area("Finance")]
    [Route("api/[controller]/[action]")]
// [Route("Finapi/[controller]/[action]")]
[ApiController]
    public class VoucherJournalController : Controller
    {
        private ERPMasterWtDataContext _context;

        public VoucherJournalController(ERPMasterWtDataContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult> GetNewVoucherNo()
        {
            string voucherPrefix = "JV-NEW-";
            string strNewVoucherNo;

            try
            {
              
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                   
                    string sql = @"
                SELECT MAX(CAST(RIGHT(TempVoucherNo, 6) AS INT)) AS MaxVoucherNo
                FROM tbl201VoucherMasterTemp WITH (TABLOCKX)
                WHERE TempVoucherNo LIKE {0}";

                    
                    var result = await _context.SqlQueryAsync<VoucherResult>(sql, new object[] { voucherPrefix + "%" });

                    int maxVoucherNo = result.FirstOrDefault()?.MaxVoucherNo ?? 0;

                   
                    int newVoucherNo = maxVoucherNo + 1;

                  
                    strNewVoucherNo = voucherPrefix + newVoucherNo.ToString("D6");

                   
                    var newVoucherEntry = new Tbl201VoucherMasterTemp
                    {
                        TempVoucherNo = strNewVoucherNo
                    };

                    _context.Tbl201VoucherMasterTemps.Add(newVoucherEntry);
                    await _context.SaveChangesAsync();

                    
                    await transaction.CommitAsync();
                }
            }
            catch (Exception ex)
            {
               
                return Json(new { success = false, error = ex.Message });
            }

           
            return Json(strNewVoucherNo);
        }


        [HttpGet]
        public IActionResult Delete(long VoucherEntryNo)
        {
            var item = _context.Tbl201VoucherEntries.Where(p=>p.VoucherEntryNo==VoucherEntryNo).FirstOrDefault();
            if (item != null)
            {
                _context.Tbl201VoucherEntries.Remove(item);
                _context.SaveChanges();
                return Ok(new {success=true});
            }
            return NotFound();
        }


        [HttpGet]
		public async Task<ActionResult> GetSupplierName(DataSourceLoadOptions loadOptions)
		{
			var qryListOfAccountlists = _context.Qry201ListOfAccounts.Where(p => p.AccountGroupId == "A012" || p.AccountGroupId == "A003").Select(i => new
			{
				i.MasterGroupId,
				i.MasterGroup,
				i.AccountGroup,
				i.AccountGroupId,
				i.AccountId,
				i.AccountHead,
				i.AccountHeadArabic,
				i.ReferenceNo,
				i.IsLedgerObselete
			});

			return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
		}
		[HttpGet]
		public async Task<ActionResult> GetVoucherEntries(DataSourceLoadOptions loadOptions, string voucherNo)
		{
			var qryListOfAccountlists = _context.Qry201VoucherEntryScreenDisplays.Where(p => p.VoucherNo == voucherNo).Select(i => new
			{
				i.VoucherNo,
				i.DrCr,
				i.DrAmount,
				i.CrAmount,
				i.EntryNarration,
				i.AccountHead,
				i.SysRemarks,
			});

			return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
		}
		[HttpPost]
		public async Task<ActionResult> AddVoucherEntry(DataSourceLoadOptions loadOptions, [FromBody] Tbl201VoucherEntry VE)
		{
			if (VE == null)
			{
				return BadRequest(new { success = false, message = "Invalid data received." });
			}

			try
			{
				_context.Tbl201VoucherEntries.Add(VE);
				await _context.SaveChangesAsync();
				var qryListOfAccountlists = _context.Qry201VoucherEntryScreenDisplays.Where(p => p.VoucherNo == VE.VoucherNo).Select(i => new
				{
					i.VoucherNo,
					i.VoucherEntryNo,
					i.DrCr,
					i.DrAmount,
					i.CrAmount,
					i.EntryNarration,
					i.AccountHead,
				
				});

				return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
				
			}
			catch (Exception ex)
			{

				return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
			}


		}
        [HttpPost]
        public async Task<ActionResult> SaveVoucher([FromBody] Tbl201VoucherMaster VM)
        {
            if (VM == null)
            {
                return BadRequest(new { success = false, message = "Invalid data received." });
            }

            try
            {
                _context.Tbl201VoucherMasters.Add(VM);
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
