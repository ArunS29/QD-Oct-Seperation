using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Humanizer;
using DevExtreme.AspNet.Data.ResponseModel;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.Areas.Finance.Reports.Payable_Statements;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace QD.ERP.Web.Areas.IMS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MaterialReceiptsController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<MaterialReceiptsController> _logger;

        public MaterialReceiptsController(ILogger<MaterialReceiptsController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetMaterialReceipt(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var query = dbContext.Qry60504materialReceiptViewMasters.AsQueryable();


                    // Default dates if not provided
                    if (!fromDate.HasValue)
                    {
                        fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1); // Start of the current month
                    }

                    if (!toDate.HasValue)
                    {
                        toDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)); // End of the current month
                    }

                    // Filtering by date range
                    query = query.Where(i => i.ReceiptDate >= fromDate && i.ReceiptDate <= toDate);

                    // Fetching the data
                    var data = await query.Select(i => new
                    {
                        i.ReceiptNo,
                        i.ReceiptDate,
                        i.SupplierCode,
                        i.SupplierName,
                        i.OurPurchaseOrderNo,
                        i.VatpurchaseBillNo,
                        i.IsSubmitted,
                        i.IsVerified,
                        i.IsApproved,
                        i.NoOfItems,
                        i.TotalBeforeTax,
                        i.TotalDiscount,
                        i.TotalAfterDiscount,
                        i.TotalTaxAmount,
                        i.TotalWithTax,

                    }).ToListAsync();

                    return Json(data);
                }

                return Unauthorized(new { message = "Invalid tenant." });
            }
            catch (Exception ex)
            {
				_logger.LogError($"An error occurred while fetching the data : {ex.Message}");
				return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
			}
        }
		[HttpGet]
		public async Task<IActionResult> GetVatCreditNoteDetails(string frmDate, string toDate)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					if (!DateTime.TryParseExact(frmDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime from))
						return BadRequest("Invalid fromDate format. Use MM/dd/yyyy.");

					if (!DateTime.TryParseExact(toDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime to))
						return BadRequest("Invalid toDate format. Use MM/dd/yyyy.");

					var data = await dbContext.Qry60506materailReceiptDetails
						.Where(x => x.ReceiptDate >= from && x.ReceiptDate <= to)
						.ToListAsync();

					return Ok(data);
				}
				catch (Exception ex)
				{
					_logger.LogError($"An error occurred while fetching the data : {ex.Message}");
					return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
				}
			}

			return Unauthorized(new { message = "Invalid tenant." });
		}
        [HttpPost]
        public IActionResult DeleteMaterialReceiptView(string ReceiptNo)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Json(new { success = false, message = "Invalid tenant context." });

            try
            {
                var receipt = dbContext.Tbl60501materialReceiptMasters
                    .FirstOrDefault(x => x.ReceiptNo == ReceiptNo);

                if (receipt == null)
                    return Json(new { success = false, message = "Material Receipt not found." });

                // ✅ 1. Check if posted to ledgers
                if (receipt.IsPosted.HasValue && receipt.IsPosted.Value)
                    return Json(new { success = false, message = "This Receipt Entry is already posted to your ledgers." });

                // ✅ 2. Check if approved
                if (receipt.IsApproved == true)
                    return Json(new { success = false, message = "Material Receipt is already approved. You cannot delete the Approved Material Receipt." });

                // ✅ 3. Check if VAT Purchase Bill is created
                if (!string.IsNullOrEmpty(receipt.VatpurchaseBillNo))
                    return Json(new { success = false, message = "Purchase Bill has already been created for this Material Receipt Entry. You cannot delete this Material Receipt." });

                // ✅ 4. Delete child records
                var children = dbContext.Tbl60502materialReceiptChildren
                    .Where(x => x.ReceiptNo == ReceiptNo)
                    .ToList();
                dbContext.Tbl60502materialReceiptChildren.RemoveRange(children);

                // ✅ 5. Delete master record
                dbContext.Tbl60501materialReceiptMasters.Remove(receipt);

                // ✅ 6. Optionally delete scanned files
                // DeleteDocumentPDF(ReceiptNo, "VoucherScanned\\IMSReceipt");

                // ✅ 7. Save changes
                dbContext.SaveChanges();

                // ✅ 8. Log deletion
                //string userId = HttpContext.Session.GetString("UserID") ?? "Unknown";
                //string userName = HttpContext.Session.GetString("UserName") ?? "Unknown";
                //InsertUserEntryLogSheet(
                //    "IMS Material Receipt",
                //    $"IMS Material Receipt Ref No. {ReceiptNo} has been deleted by User ID: {userId} User Name: {userName}.",
                //    userName,
                //    ReceiptNo
                //);

                // ✅ 9. Return success
                return Json(new { success = true, message = "Material Receipt has been successfully removed from the database." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Material Receipt.");
                return Json(new { success = false, message = "An error occurred while deleting the Material Receipt." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UnlockMaterialReceipt([FromBody] MaterialReceiptViewModel request)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized(new { success = false, message = "Invalid tenant." });

                if (string.IsNullOrWhiteSpace(request?.ReceiptNo))
                    return BadRequest(new { success = false, message = "Receipt No is required." });

                var existingEntity = await dbContext.Tbl60501materialReceiptMasters
                    .FirstOrDefaultAsync(x => x.ReceiptNo == request.ReceiptNo);

                if (existingEntity == null)
                    return NotFound(new { success = false, message = "Receipt No not found." });

                if (existingEntity.IsApproved != true && existingEntity.IsSubmitted != true && existingEntity.IsVerified != true)
                    return Ok(new { success = false, message = "Material Receipt is already unlocked." });

                existingEntity.IsApproved = false;
                existingEntity.IsSubmitted = false;
                existingEntity.IsVerified = false;

                dbContext.Tbl60501materialReceiptMasters.Update(existingEntity);
                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Material Receipt has been unlocked successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while unlocking Material Receipt.");
                return StatusCode(500, new { success = false, message = "Internal server error", details = ex.Message });
            }
        }

    }
}
