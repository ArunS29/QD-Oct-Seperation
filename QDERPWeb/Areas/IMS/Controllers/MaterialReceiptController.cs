using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using QD.ERP.Web.Services.Logging;
using System.Dynamic;

namespace QD.ERP.Web.Areas.IMS.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class MaterialReceiptController : Controller
	{

		private readonly TenantDbContextHelper _tenantDbContextHelper;
		private readonly ILogger<MaterialReceiptController> _logger;
        private readonly IUserActionLogger _userActionLogger;


        public MaterialReceiptController(ILogger<MaterialReceiptController> logger, TenantDbContextHelper tenantDbContextHelper, IUserActionLogger userActionLogger)
		{
            _userActionLogger = userActionLogger;
            _tenantDbContextHelper = tenantDbContextHelper;
			_logger = logger;
		}

		[HttpGet]
		public async Task<IActionResult> GetSupplierName(DataSourceLoadOptions loadOptions)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					var SupplierName = dbContext.Tbl30199SupplierMasters.Select(i => new
					{
						i.SupplierCode,
						i.SupplierName

					});

					return Json(await DataSourceLoader.LoadAsync(SupplierName, loadOptions));
				}
				catch (Exception ex)
				{
					_logger.LogError($"An error occurred while fetching the data : {ex.Message}");
					return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
				}
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}
		[HttpGet]
		public async Task<IActionResult> GetSalesPerson(DataSourceLoadOptions loadOptions)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					var SalesPersonName = dbContext.Tbl20101SalesPersonMasters.Select(i => new
					{
						i.SalesPersonCode,
						i.SalesPersonName

					});

					return Json(await DataSourceLoader.LoadAsync(SalesPersonName, loadOptions));
				}
				catch (Exception ex)
				{
					_logger.LogError($"An error occurred while fetching the data : {ex.Message}");
					return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
				}
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}
		[HttpGet]
		public async Task<IActionResult> GetStore(DataSourceLoadOptions loadOptions)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					var StoreName = dbContext.Tbl60001storeMasters.Select(i => new
					{
						i.StoreId,
						i.StoreName

					});

					return Json(await DataSourceLoader.LoadAsync(StoreName, loadOptions));
				}
				catch (Exception ex)
				{
					_logger.LogError($"An error occurred while fetching the data : {ex.Message}");
					return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
				}
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}
		[HttpGet]
		public async Task<IActionResult> GetProject(DataSourceLoadOptions loadOptions)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					var ProjectName = dbContext.Qry70002projectsViewMasters.Select(i => new
					{
						i.ProjectId,
						i.ProjectDescription

					});

					return Json(await DataSourceLoader.LoadAsync(ProjectName, loadOptions));
				}
				catch (Exception ex)
				{
					_logger.LogError($"An error occurred while fetching the data : {ex.Message}");
					return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
				}
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}
		[HttpGet]
		public async Task<IActionResult> GetSignatory(DataSourceLoadOptions loadOptions)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					var StoreName = dbContext.Tbl90104DocumentSignatories.Select(i => new
					{
						i.SignatoryId,
						i.SignatoryName

					});

					return Json(await DataSourceLoader.LoadAsync(StoreName, loadOptions));
				}
				catch (Exception ex)
				{
					_logger.LogError($"An error occurred while fetching the data : {ex.Message}");
					return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
				}
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}
		[HttpGet]
		public async Task<IActionResult> GetCompany(DataSourceLoadOptions loadOptions)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					var Companydata = dbContext.Tbl901CompanyDetails.Select(i => new
					{
						i.CompanyId,
						i.CompanyName

					});

					return Json(await DataSourceLoader.LoadAsync(Companydata, loadOptions));
				}
				catch (Exception ex)
				{
					_logger.LogError($"An error occurred while fetching the data : {ex.Message}");
					return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
				}
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}
		[HttpGet]
		public async Task<IActionResult> GetInventoryGroup(DataSourceLoadOptions loadOptions)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					var Companydata = dbContext.Tbl60008inventoryMasterGroups.Select(i => new
					{
						i.InventoryMasterGroupId,
						i.InventoryMasterGroup

					});

					return Json(await DataSourceLoader.LoadAsync(Companydata, loadOptions));
				}
				catch (Exception ex)
				{
					_logger.LogError($"An error occurred while fetching the data : {ex.Message}");
					return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
				}
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}
		[HttpGet]
		public async Task<IActionResult> GetMaterialReceivedType(DataSourceLoadOptions loadOptions)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					var Companydata = dbContext.Tbl60503receiptTypes.Select(i => new
					{
						i.ReceiptTypeCode,
						i.ReceiptType

					});

					return Json(await DataSourceLoader.LoadAsync(Companydata, loadOptions));
				}
				catch (Exception ex)
				{
					_logger.LogError($"An error occurred while fetching the data : {ex.Message}");
					return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
				}
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}
		[HttpGet]
		public async Task<IActionResult> GetClientName(DataSourceLoadOptions loadOptions)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					var Companydata = dbContext.Tbl30101ClientMasters.Select(i => new
					{
						i.ClientCode,
						i.ClientName

					});

					return Json(await DataSourceLoader.LoadAsync(Companydata, loadOptions));
				}
				catch (Exception ex)
				{
					_logger.LogError($"An error occurred while fetching the data : {ex.Message}");
					return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
				}
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}
		

		[HttpGet]
		public ActionResult<string> GetNewDebitNoteNoApi()
		{
			try
			{
				if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    string defaultCompanyString = HttpContext.Session.GetString("DefaultcompanyID") ?? "";
                    byte defaultCompanyByte = 0; // or any default value you want

                    if (!string.IsNullOrEmpty(defaultCompanyString))
                    {
                        // Safest way (avoids exceptions):
                        byte.TryParse(defaultCompanyString, out defaultCompanyByte);
                        // Now defaultCompanyByte holds the parsed value, or 0 if parsing failed.
                    }

                    // Now use defaultCompanyByte as needed


                    byte companyId = defaultCompanyByte;

                    var company = dbContext.Tbl901CompanyDetails
                   .FirstOrDefault(c => c.CompanyId == companyId);

                    if (company == null)
					{
						return NotFound("Company not found.");
					}

					string RequestAbbrv = company.MaterialReceiptAbbrv;
					int invoiceYearDigits = company.InvoiceYearDigits ?? 0;
					bool isResetInvoiceInYear = company.IsResetInvoiceInYear ?? false;
					DateTime invoiceDate = DateTime.Now;

					// Generate new debit note number
					string newDebitNoteNo = GetNewDebitNoteNo(RequestAbbrv, invoiceYearDigits, invoiceDate, isResetInvoiceInYear, dbContext);

					return Ok(newDebitNoteNo);
				}
				else
				{
					return BadRequest("Tenant or DB Context not found.");
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error in GetNewDebitNoteNoApi: {ex.Message}");
				return StatusCode(500, "Internal server error: " + ex.Message);
			}
		}


		private string GetNewDebitNoteNo(string RequestAbbrv, int yearInDigit, DateTime invoiceDate, bool isResetByYear, ERPMasterWtDataContext dbContext)
		{
			try
			{
				// Retrieve MPR numbers into memory Tbl601_01QuotationMaster
				var mprNumbers = dbContext.Tbl60501materialReceiptMasters
					.Where(d => d.ReceiptNo != null && d.ReceiptNo.Length >= 5 &&
								(!isResetByYear || (d.ReceiptDate.HasValue && d.ReceiptDate.Value.Year == invoiceDate.Year)))
					.Select(d => d.ReceiptNo)
					.ToList();

				// Extract numeric parts and determine the maximum
				int maxRunningNumber = mprNumbers
					.Select(no => int.TryParse(no.Substring(no.Length - 5), out int num) ? num : 0)
					.DefaultIfEmpty(0)
					.Max();

				maxRunningNumber += 1;

				// Format the new debit note number
				string strNewDebitNoteNo = maxRunningNumber.ToString().PadLeft(5, '0');

				string strYear = invoiceDate.Year.ToString();
				if (yearInDigit > 0)
				{
					strYear = strYear.Substring(strYear.Length - yearInDigit, yearInDigit);
				}
				else
				{
					strYear = "";
				}

				return $"{RequestAbbrv}{strYear}-{strNewDebitNoteNo}";
			}
			catch (Exception)
			{
				string strYear = invoiceDate.Year.ToString();
				if (yearInDigit > 0)
				{
					strYear = strYear.Substring(strYear.Length - yearInDigit, yearInDigit);
				}
				else
				{
					strYear = "";
				}

				return $"{RequestAbbrv}{strYear}-00001";
			}
		}

		[HttpGet]
		public async Task<IActionResult> GetMaterialReceiptdata(string ReceiptNo)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				if (string.IsNullOrEmpty(ReceiptNo))
					return BadRequest("Receipt No is required.");

				try
				{

					var client = await dbContext.Tbl60501materialReceiptMasters
						.Where(c => c.ReceiptNo == ReceiptNo)
						.FirstOrDefaultAsync();

					if (client == null)
						return NotFound("Material Receipt not found.");

					return Ok(client);
				}
				catch (Exception ex)
				{
					_logger.LogError($"An error occurred while fetching the data : {ex.Message}");
					return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
				}
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}
		[HttpGet]
		public async Task<ActionResult> GetMaterialReceiptChildren(string ReceiptNo)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					var resultWithDetails = new List<ExpandoObject>();

					// Query the Tbl60602purchaseRequestChildren table for the given Mprno
					var result = dbContext.Qry60502materialReceiptChildren 
                        .Where(x => x.ReceiptNo == ReceiptNo)
						.ToList();

                    var currencyRate = await dbContext.Tbl60501materialReceiptMasters
                            .Where(x => x.ReceiptNo == ReceiptNo)
                            .Select(x => x.CurrencyRate)
                            .FirstOrDefaultAsync();
                    foreach (var gridDetails in result)
					{
						dynamic item = new ExpandoObject();
						var dict = (IDictionary<string, object>)item;

						// Copy all existing fields from gridDetails into dynamic object
						var properties = gridDetails.GetType().GetProperties();
						foreach (var prop in properties)
						{
							dict[prop.Name] = prop.GetValue(gridDetails);
						}

						// Retrieve UnitDesc based on UnitCode
						var unitDesc = await dbContext.Tbl40111PropertyUnitCodes
							.Where(x => x.UnitCode == gridDetails.UnitRateMethod)
							.Select(x => x.UnitDesc)
							.FirstOrDefaultAsync();



						// Retrieve Gsdescription based on Gscode
						var gsDescription = await dbContext.Tbl20164GoodsAndServicesMasters
							.Where(x => x.Gscode == gridDetails.Gscode)
							.Select(x => x.Gsdescrpition)
							.FirstOrDefaultAsync();

						// Add the retrieved values to the dynamic object
						dict["UnitDesc"] = unitDesc;

						dict["GsDescription"] = gsDescription;
                        dict["GSCode"] = gridDetails.Gscode;
                        dict["UnitPrice"] = gridDetails.UnitPrice / currencyRate;
                        dict["ItemDiscount"] = gridDetails.ItemDiscount / currencyRate;
                        dict["PurchaseTaxRate"] = gridDetails.PurchaseTaxRate / currencyRate;
                        dict["LineTotalBeforeTax"] = gridDetails.LineTotalBeforeTax / currencyRate;
                        dict["LineTotalAfterDisc"] = gridDetails.LineTotalAfterDisc / currencyRate;
                        dict["LineTaxAmount"] = gridDetails.LineTaxAmount / currencyRate;
                        dict["LineTotalWithTax"] = gridDetails.LineTotalWithTax / currencyRate;

                        resultWithDetails.Add(item);
					}

					return Json(resultWithDetails);
				}
				catch (Exception ex)
				{
					_logger.LogError($"An error occurred while fetching the data : {ex.Message}");
					return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
				}
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}
        [HttpPost]
        public async Task<IActionResult> SaveOrUpdateMaterialReceipt([FromBody] MaterialReceiptViewModel VM)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant context." });
            }

            if (VM == null || string.IsNullOrEmpty(VM.ReceiptNo))
            {
                return BadRequest(new { success = false, message = "Receipt No is required." });
            }

            try
            {
                // Check if the master record exists
                var existingMaster = await dbContext.Tbl60501materialReceiptMasters
                    .FirstOrDefaultAsync(x => x.ReceiptNo == VM.ReceiptNo);

                if (existingMaster != null)
                {
                    // Update existing master
                    existingMaster.ReceiptDate = VM.ReceiptDate;
                    existingMaster.SupplierDeliveryNoteNo = VM.SupplierDeliveryNoteNo;
                    existingMaster.SupplierCode = VM.SupplierCode;
                    existingMaster.Mprno = VM.Mprno;
                    existingMaster.SupplierQuotationNo = VM.SupplierQuotationNo;
                    existingMaster.JobCode = VM.JobCode;
                    existingMaster.ClientCode = VM.ClientCode;
                    existingMaster.Rfqno = VM.Rfqno;
                    existingMaster.CurrencyId = VM.CurrencyId ?? 1;
                    existingMaster.CurrencyRate = VM.CurrencyRate ?? 1;
                    existingMaster.BaseCurrencyId = VM.BaseCurrencyId ?? 1;
                    existingMaster.OurPurchaseOrderNo = VM.OurPurchaseOrderNo;
                    existingMaster.SalesPersonCode = VM.SalesPersonCode;
                    existingMaster.StoreReceivedIn = VM.StoreReceivedIn;
                    existingMaster.ProjectMasterCode = VM.ProjectMasterCode;
                    existingMaster.ReceiptSignatory = VM.ReceiptSignatory.HasValue ? (byte?)VM.ReceiptSignatory.Value : null;
                    existingMaster.IssueRemarks = VM.IssueRemarks;
                    existingMaster.CompanyBranch = VM.CompanyBranch.HasValue ? (byte?)VM.CompanyBranch.Value : null;
                    existingMaster.InventoryMasterGroupId = VM.InventoryMasterGroupId.HasValue ? (byte?)VM.InventoryMasterGroupId.Value : null;
                    existingMaster.ModeOfReceiptId = VM.ModeOfReceiptId.HasValue ? (byte?)VM.ModeOfReceiptId.Value : null;
                }
                else
                {
                    // Insert new master
                    var newMaster = new Tbl60501materialReceiptMaster
                    {
                        ReceiptNo = VM.ReceiptNo,
                        ReceiptDate = VM.ReceiptDate,
                        SupplierDeliveryNoteNo = VM.SupplierDeliveryNoteNo,
                        SupplierCode = VM.SupplierCode,
                        Mprno = VM.Mprno,
                        SupplierQuotationNo = VM.SupplierQuotationNo,
                        JobCode = VM.JobCode,
                        ClientCode = VM.ClientCode,
                        Rfqno = VM.Rfqno,
                        OurPurchaseOrderNo = VM.OurPurchaseOrderNo,
                        SalesPersonCode = VM.SalesPersonCode,
                        StoreReceivedIn = VM.StoreReceivedIn,
                        ProjectMasterCode = VM.ProjectMasterCode,
                        ReceiptSignatory = Convert.ToByte(VM.ReceiptSignatory),
                        IssueRemarks = VM.IssueRemarks,
                        CompanyBranch = Convert.ToByte(VM.CompanyBranch),
                        InventoryMasterGroupId = Convert.ToByte(VM.InventoryMasterGroupId),
                        ModeOfReceiptId = Convert.ToByte(VM.ModeOfReceiptId),
                        CurrencyId = VM.CurrencyId ?? 1,
                        CurrencyRate = VM.CurrencyRate ?? 1,
                        BaseCurrencyId = VM.BaseCurrencyId ?? 1

                    };

                    await dbContext.Tbl60501materialReceiptMasters.AddAsync(newMaster);
                }

                // ✅ Handle child entries
                var existingChildren = await dbContext.Tbl60502materialReceiptChildren
                    .Where(x => x.ReceiptNo == VM.ReceiptNo)
                    .ToListAsync();

                // ✅ Track incoming child IDs
                var incomingIds = VM.MaterialReceiptDetailses
                    .Where(x => x.ReceiptChildSlNo > 0)
                    .Select(x => x.ReceiptChildSlNo)
                    .ToList();

                // ✅ Delete removed child rows
                var toDelete = existingChildren
                    .Where(x => !incomingIds.Contains(x.ReceiptChildSlNo))
                    .ToList();

                if (toDelete.Any())
                {
                    dbContext.Tbl60502materialReceiptChildren.RemoveRange(toDelete);
                }

                // ✅ Add or Update current children
                foreach (var child in VM.MaterialReceiptDetailses)
                {
                    child.ReceiptNo = VM.ReceiptNo;
                    child.UnitPrice = child.UnitPrice * VM.CurrencyRate;
                    child.ItemDiscount = child.ItemDiscount * VM.CurrencyRate;

                    if (child.ReceiptChildSlNo == 0)
                    {
                        await dbContext.Tbl60502materialReceiptChildren.AddAsync(child);
                    }
                    else
                    {
                        var existingChild = existingChildren
                            .FirstOrDefault(x => x.ReceiptChildSlNo == child.ReceiptChildSlNo);

                        if (existingChild != null)
                        {
                            dbContext.Entry(existingChild).CurrentValues.SetValues(child);
                        }
                    }
                }

                await dbContext.SaveChangesAsync();
                await _userActionLogger.LogAsync(
                   module: "IMS > Save Or Update Material Receipt",
                   actionDetail: $"Saved MaterialReceipt  {VM.ReceiptNo}",
                   documentNo: $"{VM.ReceiptNo}"
                );

                return Ok(new { success = true, message = "Material Receipt Details saved/updated successfully.", receiptno = VM.ReceiptNo });
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while saving material receipt: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while saving data.", error = ex.Message });
            }
        }

        [HttpDelete]
		public async Task<IActionResult> DeleteMaterialReceipt([FromQuery] string ReceiptNo)
		{
			if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				return Unauthorized(new { success = false, message = "Invalid tenant context." });
			}

			if (string.IsNullOrEmpty(ReceiptNo))
			{
				return BadRequest(new { success = false, message = "ReceiptNo. is required." });
			}

			try
			{
				// Retrieve the master record
				var masterRecord = await dbContext.Tbl60501materialReceiptMasters
					.FirstOrDefaultAsync(x => x.ReceiptNo == ReceiptNo);

				if (masterRecord == null)
				{
					return NotFound(new { success = false, message = "Quotation not found." });
				}

				// Retrieve and remove child records
				var childRecords = dbContext.Tbl60502materialReceiptChildren
					.Where(x => x.ReceiptNo == ReceiptNo);

				dbContext.Tbl60502materialReceiptChildren.RemoveRange(childRecords);

				// Remove the master record
				dbContext.Tbl60501materialReceiptMasters.Remove(masterRecord);

				await dbContext.SaveChangesAsync();
                await _userActionLogger.LogAsync(
                   module: "IMS > Delete Material Receipt",
                   actionDetail: $"Deleted Material Receipt  {ReceiptNo}",
                   documentNo: $"{ReceiptNo}"
                );

                return Ok(new { success = true, message = "Material Receipt details deleted successfully." });
			}
			catch (Exception ex)
			{
				_logger.LogError($"An error occurred while fetching the data : {ex.Message}");
				return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
			}
		}

		[HttpPost]
		public async Task<IActionResult> SubmitMaterialReceipt(string ReceiptNo)
		{
			try
			{
				// Validate tenant context
				if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				{
					return Unauthorized(new { success = false, message = "Invalid tenant context." });
				}

				// Validate Receipt number
				if (string.IsNullOrEmpty(ReceiptNo))
				{
					return BadRequest(new { success = false, message = "Receipt No. is required." });
				}

				// Retrieve MPR master record
				var master = await dbContext.Tbl60501materialReceiptMasters.FirstOrDefaultAsync(x => x.ReceiptNo == ReceiptNo);
				if (master == null)
				{
					return NotFound(new { success = false, message = "Material Receipt not found." });
				}

				// Retrieve session values
				var userName = HttpContext.Session.GetString("UserName");
				var userIdString = HttpContext.Session.GetString("UserId");

				if (!int.TryParse(userIdString, out int userId))
				{
					return Unauthorized(new { success = false, message = "Invalid or missing UserId in session." });
				}

				// Update MPR master record
				master.IsSubmitted = true;
				master.SubmittedBy = userName;
				master.SubmittedOn = DateTime.Now;
				master.ModifiedBy = userName;
				master.ModifiedOn = DateTime.Now;

					// Save changes
				await dbContext.SaveChangesAsync();
                await _userActionLogger.LogAsync(
                   module: "IMS > Submit Material Receipt",
                   actionDetail: $"Submited Material Receipt  {ReceiptNo}",
                   documentNo: $"{ReceiptNo}"
                );

                return Ok(new { success = true, message = "Material Receipt submitted successfully." });
			}
			catch (Exception ex)
			{
				// Log exception here (e.g., using a logging framework like Serilog or NLog)
				_logger.LogError($"An error occurred while fetching the data : {ex.Message}");
				return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
			}
		}
        private async Task<int?> GetSignatoryIDfromUserID(int? userId)
        {
            if (userId == null)
                return null;

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return await dbContext.Tbl90104DocumentSignatories
                    .Where(x => x.UserId == userId)
                    .Select(x => x.SignatoryId)
                    .FirstOrDefaultAsync();
            }

            // Tenant context is invalid; return null
            return null;
        }

        [HttpPost]
        public async Task<IActionResult> VerifyMaterialReceipt(string ReceiptNo)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { message = "Invalid tenant context." });

            if (string.IsNullOrEmpty(ReceiptNo))
                return BadRequest(new { message = "Material Receipt No is required." });

            var voucher = await dbContext.Tbl60501materialReceiptMasters
                .FirstOrDefaultAsync(v => v.ReceiptNo == ReceiptNo);
            if (voucher == null)
                return NotFound(new { message = "Material Receipt not found." });

            var userName = HttpContext.Session.GetString("UserName");
            var userIdString = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(userIdString, out int userId))
                return Unauthorized(new { message = "Invalid or missing UserId in session." });

            // Workflow enforcement
            var companySetting = await dbContext.Tbl901CompanyDetails02s.FirstOrDefaultAsync();
            bool isWorkflowEnabled = companySetting?.IsEnableReceiptWorkflow == true;
            if (isWorkflowEnabled && voucher.IsSubmitted != true)
                return BadRequest(new { message = "Please submit the receipt before verification." });

            // Verification logic
            voucher.IsVerified = true;
            voucher.VerifiedOn = DateTime.Now;
            voucher.VerifiedBy = userName;

             await dbContext.SaveChangesAsync();
            await _userActionLogger.LogAsync(
              module: "IMS > Verify Material Receipt",
              actionDetail: $"Verified Material Receipt  {ReceiptNo}",
              documentNo: $"{ReceiptNo}"
            );

            return Ok(new
            {
                message = "Material Receipt has been Verified and processed for approval."
            });
        }

        [HttpPost]
        public async Task<IActionResult> ApproveMaterialReceipt(string ReceiptNo)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { Message = "Invalid tenant context." });

            try
            {
                var userName = HttpContext.Session.GetString("UserName");
                if (!int.TryParse(HttpContext.Session.GetString("UserId"), out int userId))
                    return Unauthorized(new { message = "Invalid or missing UserId in session." });

                if (string.IsNullOrEmpty(ReceiptNo))
                    return BadRequest(new { Message = "Receipt number is required." });

                var voucher = await dbContext.Tbl60501materialReceiptMasters
                    .FirstOrDefaultAsync(v => v.ReceiptNo == ReceiptNo);
                if (voucher == null)
                    return NotFound(new { Message = "Material Receipt not found." });

                // 🔍 Optional: enforce workflow (e.g., verify & submit)
                var companySetting = await dbContext.Tbl901CompanyDetails02s.FirstOrDefaultAsync();
                bool isWorkflowEnabled = companySetting?.IsEnableReceiptWorkflow == true;
                if (isWorkflowEnabled)
                {
                    if (voucher.IsSubmitted != true || voucher.IsVerified != true)
                        return BadRequest(new
                        { Message = "Please submit and verify the receipt before approval." });
                }

                // ✅ Apply approval
                voucher.IsApproved = true;
                voucher.ApprovedOn = DateTime.Now;
                voucher.ApprovedBy = userName;

                var signatoryId = await GetSignatoryIDfromUserID(userId);
                if (signatoryId.HasValue)
                    voucher.ReceiptSignatory = (byte)signatoryId.Value;

                await dbContext.SaveChangesAsync();
                await _userActionLogger.LogAsync(
                 module: "IMS > Approve Material Receipt",
                 actionDetail: $"Approved Material Receipt {ReceiptNo}",
                  documentNo: $"{ReceiptNo}"
                );

                return Ok(new
                {
                    Message = "Material Receipt has been Approved.",
                    VoucherApprovedBy = signatoryId 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in ApproveMaterialReceipt: {ex.Message}");
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteChildById(int childId)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant context." });
            }

            try
            {
                var child = await dbContext.Tbl60502materialReceiptChildren
                    .FirstOrDefaultAsync(x => x.ReceiptChildSlNo == childId);

                if (child == null)
                {
                    return NotFound(new { success = false, message = "Child record not found." });
                }

                dbContext.Tbl60502materialReceiptChildren.Remove(child);
                await dbContext.SaveChangesAsync();
                await _userActionLogger.LogAsync(
                    module: "IMS > Delete Child By Id",
                  actionDetail: $"Deleted Child By Id  {childId}",
                   documentNo: $"{childId}"
                );

                return Ok(new { success = true, message = "Child row deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in DeleteChildById: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Internal server error." });
            }
        }
        [HttpGet]
        public IActionResult GetGSCodeDescription(string gsCode)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
            {
                var description = dbContext.Tbl20164GoodsAndServicesMasters
                    .Where(x => x.Gscode == gsCode)
                    .Select(x => x.Gsdescrpition)
                    .FirstOrDefault();

                return Ok(description ?? "");
            }

            return BadRequest("Failed to resolve tenant");
        }

        [HttpGet]
        public IActionResult GetGSCodeDetailedDescription(string gsCode)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
            {
                var detailedDesc = dbContext.Tbl20164GoodsAndServicesMasters
                    .Where(x => x.Gscode == gsCode)
                    .Select(x => x.GsdetailedDesc)
                    .FirstOrDefault();

                return Ok(detailedDesc ?? "");
            }

            return BadRequest("Failed to resolve tenant");
        }

        [HttpGet]
        public async Task<IActionResult> GetdataByGSCode(string GSCode)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                if (string.IsNullOrEmpty(GSCode))
                    return BadRequest("GSCode  is required.");

                try
                {

                    var client = await dbContext.Tbl20164GoodsAndServicesMasters
                        .Where(c => c.Gscode == GSCode)
                        .FirstOrDefaultAsync();

                    if (client == null)
                        return NotFound("GS data not found.");

                    return Ok(client);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetGSData: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet]
        public async Task<IActionResult> GetDetaildescriptiondata(long ReceiptChildSlNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                if (ReceiptChildSlNo == 0)
                    return BadRequest("ReceiptChildSlNo is required.");


                try
                {

                    var client = await dbContext.Tbl60502materialReceiptChildren
                        .Where(c => c.ReceiptChildSlNo == ReceiptChildSlNo)
                        .FirstOrDefaultAsync();

                    if (client == null)
                        return NotFound("Client not found.");

                    return Ok(client);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetProject: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
    }
}
