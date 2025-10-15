using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.AspNet.Mvc;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using QD.ERP.Shared.Models;
using QD.ERP.Shared.DAL.Entities;
using QD.ERP.Shared.Service;
using QD.ERP.Shared.Services.Logging;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace QD.ERP.IMS.Areas.IMS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MaterialReceiptsController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<MaterialReceiptsController> _logger;
        private readonly IUserActionLogger _userActionLogger;


        public MaterialReceiptsController(ILogger<MaterialReceiptsController> logger, TenantDbContextHelper tenantDbContextHelper, IUserActionLogger userActionLogger)
        {
            _userActionLogger = userActionLogger;
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
                        i.ModeOfReceiptId

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
                _userActionLogger.LogAsync(module: "IMS > Delete Material Receipt View",
                   actionDetail: $"Deleted Material Receipt View {ReceiptNo}",
                    documentNo: $"{ReceiptNo}"
                );

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
                await _userActionLogger.LogAsync(
                  module: "IMS > Unlock Material Receipt",
                  actionDetail: $":Unlocked Material Receipt  {request.ReceiptNo}",
                  documentNo: $"{request.ReceiptNo}"
                );

                return Ok(new { success = true, message = "Material Receipt has been unlocked successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while unlocking Material Receipt.");
                return StatusCode(500, new { success = false, message = "Internal server error", details = ex.Message });
            }
        }
        // Simple check: has receipt already a VAT purchase bill
        [HttpGet]
        public async Task<IActionResult> CheckIfPurchaseBillExists(string receiptNo)
        {
            if (string.IsNullOrWhiteSpace(receiptNo))
                return BadRequest("receiptNo is required.");

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                return BadRequest("Invalid tenant or DB context.");

            bool exists = await dbContext.Tbl60501materialReceiptMasters
                .AnyAsync(x => x.ReceiptNo == receiptNo && x.VatpurchaseBillNo != null);

            return Ok(exists);
        }

        // Return ledger/account id for a supplier (if needed by frontend)
        [HttpGet]
        public async Task<IActionResult> GetLedgerNo(string supplierCode)
        {
            if (string.IsNullOrWhiteSpace(supplierCode))
                return BadRequest("supplierCode is required.");

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                return BadRequest("Invalid tenant or DB context.");

            var ledgerNo = await dbContext.Qry65114supplierListWithLedgerNos
                .Where(x => x.SupplierCode == supplierCode)
                .Select(x => x.AccountId)
                .FirstOrDefaultAsync();

            return Ok(ledgerNo ?? "");
        }

        // Main action - accepts a model object from frontend
        [HttpPost]
        public async Task<IActionResult> CreateVatPurchaseFromReceipt([FromBody] ReceiptRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.ReceiptNo))
                    return BadRequest(new { success = false, message = "Receipt No is required." });

                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                    return Unauthorized(new { success = false, message = "Invalid tenant." });

                string receiptNo = request.ReceiptNo.Trim();

                // 0. Pre-check: if already exists, stop early
                var already = await dbContext.Tbl60501materialReceiptMasters
                    .AnyAsync(x => x.ReceiptNo == receiptNo && x.VatpurchaseBillNo != null);

                if (already)
                    return Conflict(new { success = false, message = "Purchase Bill already exists for this Receipt." });

                // 1. Get supplier code from receipt
                var supplierCode = await dbContext.Tbl60501materialReceiptMasters
                    .Where(r => r.ReceiptNo == receiptNo)
                    .Select(r => r.SupplierCode)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrWhiteSpace(supplierCode))
                    return NotFound(new { success = false, message = "Supplier not found for this receipt." });

                // 2. Get ledger/account id for supplier
                var ledgerNo = await dbContext.Qry65114supplierListWithLedgerNos
                    .Where(x => x.SupplierCode == supplierCode)
                    .Select(x => x.AccountId)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrWhiteSpace(ledgerNo))
                    return NotFound(new { success = false, message = "Ledger No not found for this supplier." });

                // 3. Generate new PurchaseVoucherNo (PUR-yy-000001)
                string purchaseVoucherNo = await GetNewPurchaseVoucherNoInternal(dbContext);

                // 4. Calculate due date from chart of accounts (NoOfDaysCreditPeriod)
                var noOfDaysDue = await dbContext.Qry20107ChartOfAccounts
                    .Where(a => a.AccountId == ledgerNo)
                    .Select(a => a.NoOfDaysCreditPeriod ?? 0)
                    .FirstOrDefaultAsync();

                var dueDate = DateTime.Today.AddDays(noOfDaysDue);

                // 5. Re-check again inside DB transactionish flow to avoid double creation (lightweight)
                // Here we will check receipt.VatpurchaseBillNo again and throw if already set
                var receipt = await dbContext.Tbl60501materialReceiptMasters
                    .FirstOrDefaultAsync(x => x.ReceiptNo == receiptNo);

                if (receipt == null)
                    return NotFound(new { success = false, message = "Receipt not found." });

                if (!string.IsNullOrWhiteSpace(receipt.VatpurchaseBillNo))
                    return Conflict(new { success = false, message = "Purchase Bill already exists for this Receipt." });

                string addedBy = HttpContext.Session.GetString("UserName") ?? "System";

                // 6. Execute stored procedure - ensure parameter order/names match your SP
                // Using ExecuteSqlRaw with positional args to avoid SQL injection
                await dbContext.Database.ExecuteSqlRawAsync(
                    "EXEC sp600_16InsertToBillFromReceiptNote @PurchaseVoucherNo = {0}, @SupplierAccountNo = {1}, @ReceiptNoteNo = {2}, @AddedBy = {3}, @DefaultPurchaseLedgerNo = {4}, @BillDueDate = {5}",
                    purchaseVoucherNo,
                    ledgerNo,
                    receiptNo,
                    addedBy,
                    request.DefaultPurchaseLedgerNo ?? "", // optional; you can fetch a default from settings
                    dueDate
                );

                // 7. After SP: ensure receipt.VatpurchaseBillNo is set (SP likely does this).
                // If SP didn't set it, set it here.
                // Reload the receipt to get current value
                var updatedReceipt = await dbContext.Tbl60501materialReceiptMasters
                    .FirstOrDefaultAsync(x => x.ReceiptNo == receiptNo);

                if (updatedReceipt != null && string.IsNullOrWhiteSpace(updatedReceipt.VatpurchaseBillNo))
                {
                    updatedReceipt.VatpurchaseBillNo = purchaseVoucherNo;
                    await dbContext.SaveChangesAsync();
                }

                // 8. Log action (optional)
                try
                {
                    await _userActionLogger.LogAsync(
                        module: "IMS > Create VAT Purchase From Receipt",
                        actionDetail: $":Created VAT Purchase {purchaseVoucherNo} from Receipt {receiptNo}",
                        documentNo: purchaseVoucherNo
                    );
                }
                catch
                {
                    // ignore logging errors
                }

                // 9. Return success
                return Ok(new
                {
                    success = true,
                    message = "New VAT Purchase Bill created successfully.",
                    purchaseVoucherNo = purchaseVoucherNo,
                    receiptNo = receiptNo
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Server error: " + ex.Message });
            }
        }

        // Helper: Generate new Purchase Voucher No in PUR-YY-000001 format (uses Tbl20166VatpurchaseMasters)
        private async Task<string> GetNewPurchaseVoucherNoInternal(ERPMasterWtDataContext dbContext)
        {
            string invoiceAbbr = "PUR";
            string yearDigits = DateTime.Now.ToString("yy");
            string prefix = $"{invoiceAbbr}-{yearDigits}-";

            var lastNo = await dbContext.Tbl20166VatpurchaseMasters
                .Where(i => i.PurchaseVoucherNo.StartsWith(prefix))
                .OrderByDescending(i => i.PurchaseVoucherNo)
                .Select(i => i.PurchaseVoucherNo)
                .FirstOrDefaultAsync();

            int nextNum = 1;
            if (!string.IsNullOrEmpty(lastNo))
            {
                var match = Regex.Match(lastNo, @"(\d{6})$");
                if (match.Success && int.TryParse(match.Groups[1].Value, out int last))
                {
                    nextNum = last + 1;
                }
            }

            return $"{prefix}{nextNum:D6}";
        }

        // Request model
        public class ReceiptRequest
        {
            public string ReceiptNo { get; set; }
            public string DefaultPurchaseLedgerNo { get; set; } // optional - if you want to pass from frontend
        }

    }


}

