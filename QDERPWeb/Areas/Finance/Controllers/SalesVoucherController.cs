using QD.ERP.Web.DAL.Entities;
using QDERPWeb.Models;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QD.ERP.Web.Service;
using System.Drawing;
using System.Xml.Linq;
using DevExpress.Xpo;
using System.Data;
using QD.ERP.Web.Areas.Finance.Models;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SalesVoucherController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<SalesVoucherController> _logger;

        public SalesVoucherController(ILogger<SalesVoucherController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult> GetClientName(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var qryListOfAccountlists = dbContext.Qry201ListOfAccounts
                    .Where(p => p.AccountGroupId == "A011" || p.AccountGroupId == "A012")
                    .Select(i => new
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

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public async Task<ActionResult> AddVoucherEntry(DataSourceLoadOptions loadOptions, [FromBody] Tbl201VoucherEntry VE)
        {
            if (VE == null)
            {
                return BadRequest(new { success = false, message = "Invalid data received." });
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    Tbl201VoucherMaster voucherMaster = new();
                    bool isVoucherExists = dbContext.Tbl201VoucherMasters
              .Any(v => v.VoucherNo == VE.VoucherNo);

                    if (!isVoucherExists)
                    {
                        voucherMaster.VoucherNo = VE.VoucherNo;
                        voucherMaster.VoucherDate = DateTime.Now;
                        dbContext.Tbl201VoucherMasters.Add(voucherMaster);
                    }

                    dbContext.Tbl201VoucherEntries.Add(VE);
                    await dbContext.SaveChangesAsync();

                    var qryListOfAccountlists = dbContext.Qry201VoucherEntryScreenDisplays
                        .Where(p => p.VoucherNo == VE.VoucherNo)
                        .Select(i => new
                        {
                            i.VoucherNo,
                            i.VoucherEntryNo,
                            i.DrCr,
                            i.DrAmount,
                            i.CrAmount,
                            i.EntryNarration,
                            AccountHead = dbContext.Qry201ListOfAccounts
                        .Where(a => a.AccountId == i.AccountHead)
                        .Select(a => a.AccountHead)
                        .FirstOrDefault() ?? i.AccountHead,
                            i.SysRemarks,
                        });

                    return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in AddVoucherEntry: {ex.Message}");
                    return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<ActionResult> GetVoucherEntries(DataSourceLoadOptions loadOptions, string voucherNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var qryListOfAccountlists = dbContext.Qry201VoucherEntryScreenDisplays
                    .Where(p => p.VoucherNo == voucherNo)
                    .Select(i => new
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

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public async Task<ActionResult> SaveVoucher([FromBody] Tbl201VoucherMaster VM)
        {
            if (VM == null)
            {
                return BadRequest(new { success = false, message = "Invalid data received." });
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    dbContext.Tbl201VoucherMasters.Add(VM);
                    await dbContext.SaveChangesAsync();
                    return Ok(new { success = true, message = "Data inserted successfully!" });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in SaveVoucher: {ex.Message}");
                    return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
		[HttpGet]
		public async Task<ActionResult> GetVoucherDetails(DataSourceLoadOptions loadOptions, string voucherNo)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				if (string.IsNullOrEmpty(voucherNo))
				{
					return BadRequest(new { success = false, message = "Invalid data received." });
				}
				try
				{
					var voucherDetails = dbContext.Tbl201VoucherMasters
						.Where(p => p.VoucherNo.ToLower() == voucherNo.ToLower());

					if (!voucherDetails.Any())
					{
						return NotFound(new { success = false, message = "Voucher not found." });
					}

					var result = await DataSourceLoader.LoadAsync(voucherDetails, loadOptions);
					return Json(result);
				}
				catch (Exception ex)
				{
					_logger.LogError($"Error in GetVoucherDetails: {ex.Message}");
					return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
				}
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}

		[HttpGet]
        public IActionResult CheckVoucherExists(string voucherNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                bool exists = dbContext.Tbl201VoucherEntries.Any(s => s.VoucherNo == voucherNo);

                if (exists)
                {
                    return Ok(new { success = false, message = $"The entered Voucher No has already been recorded in the system. Please correct the Voucher No." });
                }

                return Ok(new { success = true });
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAssetDocument(DataSourceLoadOptions loadOptions, [FromBody] Tbl20116LedgerDocument updatedDocument)
        {
            if (updatedDocument == null)
            {
                return BadRequest("Invalid document data.");
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var document = await dbContext.Tbl20108AssetDocuments
                        .FirstOrDefaultAsync(d => d.DocumentNo == updatedDocument.DocumentNo);

                    if (document == null)
                    {
                        return NotFound($"Document with DocumentNo {updatedDocument.DocumentNo} not found.");
                    }

                    var qryListOfAccountlists = dbContext.Tbl20108AssetDocuments
                        .Where(p => p.DocumentNo == document.DocumentNo)
                        .Select(i => new
                        {
                            i.DocumentNo,
                            i.DocumentType,
                            i.DocumentRefNo,
                            i.DocumentRemarks,
                            i.DocumentExpDate,
                            i.DocumentExpDateAr,
                            i.DocumentNotificationDate,
                        });

                    return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in UpdateAssetDocument: {ex.Message}");
                    return StatusCode(500, "An error occurred while processing your request.");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        //[HttpPost]
        //public async Task<ActionResult> UpdateSalesVoucher([FromBody] Tbl201VoucherMaster VM)
        //{
        //    if (VM == null || string.IsNullOrWhiteSpace(VM.VoucherNo))
        //    {
        //        return BadRequest(new { success = false, message = "Invalid data received." });
        //    }

        //    try
        //    {

        //        if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
        //        {
        //            var existingVoucher = await dbContext.Tbl201VoucherMasters
        //                                            .FirstOrDefaultAsync(v => v.VoucherNo == VM.VoucherNo);

        //            if (existingVoucher != null)
        //            {
        //                // Update existing record
        //                dbContext.Entry(existingVoucher).CurrentValues.SetValues(VM);
        //            }
        //            else
        //            {
        //                // Insert new record
        //                dbContext.Tbl201VoucherMasters.Add(VM);
        //            }

        //            await dbContext.SaveChangesAsync();
        //            return Ok(new { success = true, message = existingVoucher != null ? "Voucher updated successfully!" : "Voucher inserted successfully!" });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Error in UpdateAssetDocument: {ex.Message}");
        //        return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
        //    }
        //    return Unauthorized(new { message = "Invalid tenant.", success = false });

        //}


         [HttpPost]
        public async Task<ActionResult> UpdateSalesVoucher([FromBody] Tbl201VoucherMaster VM)
        {
            
            if (VM == null || string.IsNullOrWhiteSpace(VM.VoucherNo))
            {
                return BadRequest(new { success = false, message = "Invalid data received." });
            }

            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var voucherNo = VM.VoucherNo?.Trim(); // Remove whitespace and prevent null issues
                    if (string.IsNullOrEmpty(voucherNo))
                    {
                        return BadRequest(new { success = false, message = "Voucher number is required." });
                    }

                    var existingVoucher = await dbContext.Tbl201VoucherMasters
                                                         .FirstOrDefaultAsync(v => v.VoucherNo == voucherNo);

                    if (existingVoucher != null)
                    {
                        dbContext.Entry(existingVoucher).CurrentValues.SetValues(VM);


                        //dbContext.Tbl201VoucherMasters.Update(existingVoucher);
                    }
                    else
                    {
                        // Insert new record
                        dbContext.Tbl201VoucherMasters.Add(VM);
                    }

                    await dbContext.SaveChangesAsync();
                    return Ok(new { success = true, message = existingVoucher != null ? "Voucher updated successfully!" : "Voucher inserted successfully!" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UpdateSalesVoucher: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

		[HttpGet]
		public async Task<IActionResult> CheckVoucherDateLocking(DateTime voucherDate)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				bool isDateBlocked = await dbContext.Tbl90117VoucherDateLockings
					.AnyAsync(v => v.VoucherTypeCode == "SALES_VOUCHER" && v.VoucherDateLocked >= voucherDate);

				if (isDateBlocked)
				{
					return Json(new { success = false, message = "This Voucher Entry date has been blocked. Please review your entry date." });
				}

				return Json(new { success = true, todayDate = DateTime.Now.ToString("yyyy-MM-dd") });
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}


	}
}
