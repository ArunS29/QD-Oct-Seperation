using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using DevExtreme.AspNet.Mvc;
using DevExtreme.AspNet.Data;
using QD.ERP.Web.Areas.VAT.Models;
using QD.ERP.Web.Areas.Finance.Models;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Dynamic;
using DevExpress.DataProcessing.InMemoryDataProcessor;
using System.Numerics;
using System.Data;




namespace QD.ERP.Web.Areas.VAT.Controllers
{

	[Route("api/[controller]/[action]")]
	public class VATModuleController : Controller
	{
		private readonly TenantDbContextHelper _tenantDbContextHelper;
		private readonly ILogger<VATModuleController> _logger;

		public VATModuleController(ILogger<VATModuleController> logger, TenantDbContextHelper tenantDbContextHelper)
		{
			_tenantDbContextHelper = tenantDbContextHelper;
			_logger = logger;
		}
		[HttpGet]
		public async Task<ActionResult> GetVatInvoices(string frmDate, string toDate)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					if (!DateTime.TryParseExact(frmDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime from))
						return BadRequest("Invalid from date format. Use MM/dd/yyyy.");

					if (!DateTime.TryParseExact(toDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime to))
						return BadRequest("Invalid to date format. Use MM/dd/yyyy.");

					// Fetch records based on the date range
					var vatInvoices = await dbContext.Qry201607vatinvoiceRegisterMainViews
						.FromSqlRaw("SELECT * FROM Qry201_607vatinvoiceRegisterMainView WHERE InvoiceDate BETWEEN @p0 AND @p1", from, to)
						.ToListAsync();

					return Json(vatInvoices);
				}
				catch (Exception ex)
				{
					return StatusCode(500, $"Internal server error: {ex.Message}");
				}
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}

		[HttpGet]
		public async Task<ActionResult> GetVat(string frmDate, string toDate)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					if (!DateTime.TryParseExact(frmDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime from))
						return BadRequest("Invalid from date format. Use MM/dd/yyyy.");

					if (!DateTime.TryParseExact(toDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime to))
						return BadRequest("Invalid to date format. Use MM/dd/yyyy.");

					// Fetch records based on the date range
					var vatInvoices = await dbContext.Qry201707vatpurchaseRegisterMainViews
						.FromSqlRaw("SELECT * FROM qry201_707VATPurchaseRegisterMainView WHERE PurchaseVoucherDate BETWEEN @p0 AND @p1", from, to)
						.ToListAsync();

					return Json(vatInvoices);
				}
				catch (Exception ex)
				{
					return StatusCode(500, $"Internal server error: {ex.Message}");
				}
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}

		[HttpGet]
		public async Task<ActionResult> GetVatDetails(string frmDate, string toDate)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					if (!DateTime.TryParseExact(frmDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime from))
						return BadRequest("Invalid from date format. Use MM/dd/yyyy.");

					if (!DateTime.TryParseExact(toDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime to))
						return BadRequest("Invalid to date format. Use MM/dd/yyyy.");

					// Fetch records based on the date range
					var vatInvoices = await dbContext.Qry201617vatinvoiceInDetails
						.FromSqlRaw("SELECT * FROM Qry201_617vatinvoiceInDetails WHERE InvoiceDate BETWEEN @p0 AND @p1", from, to)
						.ToListAsync();

					return Json(vatInvoices);
				}
				catch (Exception ex)
				{
					return StatusCode(500, $"Internal server error: {ex.Message}");
				}
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}

		//[HttpGet]
		//public async Task<ActionResult> GetVATInvoiceNo(DataSourceLoadOptions loadOptions)
		//{
		//    DateTime currentDate = DateTime.Now;
		//    string currentYear = currentDate.Year.ToString();
		//    string currentMonth = currentDate.Month.ToString("00");
		//    string voucherString = "SELECT TOP 1 EInvoiceAbbrv FROM tbl901CompanyDetails"; // Optimized substring
		//    string strNewReceiptNo;

		//    // SQL LIKE pattern
		//    string likePattern = $"{voucherString}%";

		//    try
		//    {
		//        if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
		//        {
		//            return Unauthorized(new { message = "Invalid tenant.", success = false });
		//        }

		//        // Use raw SQL query to fetch the maximum voucher number
		//        var result = await dbContext.VoucherResults
		//            .FromSqlInterpolated($@"
		//        SELECT MAX(CAST(RIGHT(VoucherNo, 3) AS INT)) AS MaxVoucherNo
		//        FROM Tbl201VoucherEntry
		//        WHERE VoucherNo LIKE {likePattern}")
		//            .ToListAsync();

		//        int maxVoucherNo = result.FirstOrDefault()?.MaxVoucherNo ?? 0;
		//        int newVoucherNo = maxVoucherNo + 1;

		//        // Format the new voucher number with leading zeros
		//        strNewReceiptNo = $"{voucherString}{newVoucherNo:000}"; // Cleaner formatting

		//        return Json(strNewReceiptNo);
		//    }
		//    catch (Exception ex)
		//    {
		//        Console.WriteLine($"Error: {ex.Message}"); // Log exception for debugging
		//                                                   // Handle cases where there's no existing voucher number
		//        return Json($"{voucherString}001");
		//    }
		//}


		[HttpGet]
		public async Task<IActionResult> GetPaymentMeans(DataSourceLoadOptions loadOptions)
		{
			try
			{

				if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				{
					var qryListOfAccountlists = dbContext.Tbl00106PaymentMeansTypeCodes.Select(i => new
					{

						i.PaymentMeansTypeCode,
						i.PaymentMeansType

					});


					return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
				}

			}
			catch (Exception ex) { throw ex; }

			return Unauthorized(new { message = "Invalid tenant.", success = false });

		}


		[HttpGet]
		public async Task<IActionResult> GetClientCode(DataSourceLoadOptions loadOptions)
		{
			try
			{

				if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				{
					var qryListOfAccountlists = dbContext.Qry201710vatsundryDebtorsAndCashAccs.Select(i => new
					{

						i.AccountId,
						i.AccountHead,
						i.VatregistrationNo,
						i.ReferenceNo,
						i.IsLedgerObselete,
						i.ClientGroupVatnumber,
						i.ClientVendorNo,
						i.ClientOtherId,
						i.ClientOtherIdtype,
						i.ClientAddressStreet,
						i.ClientAddressStreetAr,
						i.ClientAdditionalStreet,
						i.ClientAdditionalStreetAr,
						i.ClientBuildingNumber,
						i.ClientCity,
						i.ClientCityAr,
						i.ClientAdditionalNumber,
						i.ClientProvince,
						i.ClientProvinceAr,
						i.ClientPostalCode,
						i.ClientNeighborhoodAr,
						i.ClientCountryCode

					});


					return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
				}

			}
			catch (Exception ex) { throw ex; }
			return Unauthorized(new { message = "Invalid tenant.", success = false });

		}

		[HttpGet]
		public async Task<IActionResult> GetGoodsAndServices()
		{
			try
			{
				if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				{
					var result = await dbContext.Tbl20164GoodsAndServicesMasters
					.Select(g => new
					{
						g.Gscode,
						g.Gsdescrpition,
						g.GsgroupId,
						g.GsdescriptionAr,
						g.ItemPartNo,
						g.CostPrice,
						g.GssellingRate,
						g.ReorderQty

					})
					.ToListAsync();

					return Ok(result);
				}
			}
			catch (Exception ex) { throw ex; }
			return Unauthorized(new { message = "Invalid tenant.", success = false });

		}
		[HttpGet]
		public async Task<IActionResult> GetUnitofMeasure()
		{
			try
			{
				if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				{
					var result = await dbContext.Tbl40111PropertyUnitCodes
					.Select(g => new
					{
						g.UnitType,
						g.UnitCode
					})
					.ToListAsync();

					return Ok(result);
				}
			}
			catch (Exception ex) { throw ex; }
			return Unauthorized(new { message = "Invalid tenant.", success = false });

		}


		[HttpGet]
		public IActionResult GetGoodsAndServiceMaster()
		{
			try
			{
				if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				{
					var data = dbContext.Tbl20164GoodsAndServicesMasters
					.Select(g => new
					{
						g.Gscode,
						g.Gsdescrpition,
						g.GsdescriptionAr,
						g.ItemPartNo,
						g.CostPrice,
						g.GssellingRate,
						g.ReorderQty
					})
					.ToList();

					return Ok(data);
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return Unauthorized(new { message = "Invalid tenant.", success = false });

		}

		[HttpGet]
		public async Task<ActionResult> GetVATInvoiceNo(DataSourceLoadOptions loadOptions)

		{
			try
			{
				if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				{
					string yearSuffix = DateTime.Now.ToString("yy"); // Get last two digits of the year

					// Get invoice abbreviation
					var invoiceAbbrv = await dbContext.Tbl901CompanyDetails
						.Select(c => c.EinvoiceAbbrv)
						.FirstOrDefaultAsync();

					if (string.IsNullOrEmpty(invoiceAbbrv))
						return BadRequest("Invoice abbreviation not found.");

					// Get last invoice number
					var lastInvoiceNumber = await dbContext.Tbl20161VatinvoiceMasters
						.Where(i => i.InvoiceNo.StartsWith($"{invoiceAbbrv}{yearSuffix}-"))
						.OrderByDescending(i => i.InvoiceNo)
						.Select(i => i.InvoiceNo)
						.FirstOrDefaultAsync();

					int newNumber = 1; // Default if no previous invoices exist
					if (!string.IsNullOrEmpty(lastInvoiceNumber))
					{
						var match = Regex.Match(lastInvoiceNumber, @"-(\d+)$");
						if (match.Success)
						{
							newNumber = int.Parse(match.Groups[1].Value) + 1;
						}
					}

					// Generate new invoice number
					string newInvoiceNumber = $"{invoiceAbbrv}{yearSuffix}-{newNumber:D5}";

					//  return Ok(new { InvoiceNumber = newInvoiceNumber });
					return Json(newInvoiceNumber);
				}

			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error: {ex.Message}");
			}


			return BadRequest("Failed to retrieve tenant and database context.");
		}

		[HttpPost]
		public async Task<IActionResult> CreateNewInvoiceCounter()
		{
			try
			{
				if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				{
					var maxCounter = await dbContext.Qry00101invoiceCounterValues
					.MaxAsync(x => (long?)x.InvoiceCounterValue);


					var newCounterValue = maxCounter + 1;

					return Json(newCounterValue);
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return BadRequest("Failed to retrieve tenant and database context.");

		}

		[HttpPost]
		public IActionResult GenerateGuid()
		{
			try
			{
				if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				{
					Guid newGuid = Guid.NewGuid();
					string registryFormatGuid = newGuid.ToString("D"); // Format: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx

					// return Ok(new { guid = registryFormatGuid }); // More conventional for Web APIs
					return Json(registryFormatGuid);
				}

				return BadRequest("Failed to retrieve tenant and database context.");
			}
			catch (Exception ex)
			{

				return StatusCode(500, "An error occurred while generating GUID.");
			}
		}




		//[HttpPost]
		//public async Task<ActionResult> UpdateInvoiceMasterDetails(Tbl20161VatinvoiceMaster InvoiceMaster)
		//{
		//    if (InvoiceMaster == null)
		//    {
		//        return BadRequest(new { success = false, message = "Invalid invoice data received." });
		//    }

		//    try
		//    {
		//        if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
		//        {
		//            var existingInvoice = await dbContext.Tbl20161VatinvoiceMasters
		//                                             .FirstOrDefaultAsync(v => v.InvoiceNo == InvoiceMaster.InvoiceNo);

		//            if (existingInvoice != null)
		//            {
		//                // Update existing master record
		//                dbContext.Entry(existingInvoice).CurrentValues.SetValues(InvoiceMaster);
		//            }
		//            else
		//            {
		//                // Insert new invoice master record
		//                await dbContext.Tbl20161VatinvoiceMasters.AddAsync(InvoiceMaster);
		//            }


		//            await dbContext.SaveChangesAsync();
		//            // await transaction.CommitAsync();

		//            return Ok(new { success = true, message = existingInvoice != null ? "Invoice and child records updated successfully!" : "New invoice and child records added successfully!" });
		//        }
		//    }
		//    catch (Exception ex)
		//    {
		//        // await transaction.RollbackAsync();
		//        return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
		//    }

		//    return BadRequest("Failed to retrieve tenant and database context.");
		//}

		//[HttpPost]
		//public async Task<ActionResult> UpdateInvoiceChildDetails(List<InvoiceItem> InvoiceChildren)
		//{
		//    if (InvoiceChildren == null)
		//    {
		//        return BadRequest(new { success = false, message = "Invalid or empty invoice data received." });
		//    }

		//    try
		//    {
		//        if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
		//        {
		//            Tbl20162VatinvoiceChild aTbl20162VatinvoiceChild = new Tbl20162VatinvoiceChild();


		//            // Process child records if available
		//            foreach (var child in InvoiceChildren)
		//            {
		//                if (child == null) continue;  // Skip null child records if any

		//                var existingChild = await dbContext.Tbl20162VatinvoiceChildren
		//                                                   .FirstOrDefaultAsync(c => c.InvoiceNo == child.InvoiceNo
		//                                                                        );
		//                aTbl20162VatinvoiceChild.InvoiceNo = child.InvoiceNo;
		//                aTbl20162VatinvoiceChild.UnitRate = child.Amount.GetDecimal();
		//                aTbl20162VatinvoiceChild.DetailedDescription = child.Description.GetString();
		//                //  aTbl20162VatinvoiceChild.Discount = child.Discount.GetString();
		//                //aTbl20162VatinvoiceChild.TaxExemptionReasonCode = child.ExemptionCode.GetString();
		//                //aTbl20162VatinvoiceChild.ItemCode = child.ItemCode.GetString();
		//                aTbl20162VatinvoiceChild.QuantityInvoiced = child.Qty.GetDecimal();
		//                aTbl20162VatinvoiceChild.UnitsToBill = 1;
		//                aTbl20162VatinvoiceChild.UnitRateMethod = 49;
		//                aTbl20162VatinvoiceChild.UoM = "Each";

		//                if (existingChild != null)
		//                {
		//                    // Update the existing child record
		//                    dbContext.Entry(existingChild).CurrentValues.SetValues(aTbl20162VatinvoiceChild);
		//                }
		//                else
		//                {
		//                    // Add a new child record
		//                    await dbContext.Tbl20162VatinvoiceChildren.AddAsync(aTbl20162VatinvoiceChild);
		//                }
		//            }
		//            //aTbl20162VatinvoiceChild.Discount = child.Discount;
		//            //aTbl20162VatinvoiceChild.TaxExemptionReasonCode = child.ExemptionCode;
		//            //aTbl20162VatinvoiceChild.ItemCode = child.ItemCode;
		//            //aTbl20162VatinvoiceChild.QuantityInvoiced = child.Qty;
		//            //aTbl20162VatinvoiceChild.UnitsToBill = 1;
		//            //aTbl20162VatinvoiceChild.UnitRateMethod = 49;
		//            //aTbl20162VatinvoiceChild.UoM = "Each";



		//            // Save changes to the database
		//            await dbContext.SaveChangesAsync();
		//            // await transaction.CommitAsync();

		//            return Ok(new { success = true, message = "Invoice child records updated successfully!" });
		//        }
		//    }

		//    catch (Exception ex)
		//    {
		//        //await transaction.RollbackAsync();
		//        return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
		//    }

		//    return BadRequest("Failed to retrieve tenant and database context.");
		//}
		[HttpGet]
		public IActionResult GetGoodsAndServicesGroups()
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				var groups = dbContext.Tbl20165GoodsAndServicesGroups
					.Select(g => new
					{
						g.GsgroupId,
						g.GsgroupName,
						g.GsgroupCode
					})
					.ToList();

				return Ok(groups);
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}

		[HttpGet]
		public IActionResult GetUnitOfMeasures()
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				var units = dbContext.Tbl40111PropertyUnitCodes
					.Select(u => new
					{
						u.UnitCode,     // Primary key
						u.UnitType,     // Display text
						u.UnitDesc  // Optional display or additional info
					})
					.ToList();

				return Ok(units);
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}

		[HttpPost]
		public async Task<ActionResult> SaveCostAllocation([FromBody] Tbl20164GoodsAndServicesMaster CM)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					dbContext.Tbl20164GoodsAndServicesMasters.Add(CM);
					await dbContext.SaveChangesAsync();
					return Ok(new { success = true, message = "Data inserted successfully!" });
				}
				catch (Exception ex)
				{
					_logger.LogError($"Error in SaveCostAllocation: {ex.Message}");
					return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
				}
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}
		[HttpPost]
		public async Task<ActionResult> SaveGoodsAndServices([FromBody] Tbl20164GoodsAndServicesMaster model)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					var now = DateTime.Now;

					var existing = await dbContext.Tbl20164GoodsAndServicesMasters
						.FirstOrDefaultAsync(x => x.Gscode == model.Gscode);

					// === 🔍 Validate duplicate ItemPartNo ===
					// === 🔍 Validate duplicate ItemPartNo ===
					if (!string.IsNullOrWhiteSpace(model.ItemPartNo))
					{
						var duplicatePartNo = await dbContext.Tbl20164GoodsAndServicesMasters
							.Where(x => x.ItemPartNo == model.ItemPartNo && x.Gscode != model.Gscode)
							.Select(x => x.Gscode)
							.FirstOrDefaultAsync();

						if (!string.IsNullOrEmpty(duplicatePartNo))
						{
							return BadRequest(new
							{
								success = false,
								message = $"Stock Item with this Part Number already exists (GSCode: {duplicatePartNo})."
							});
						}
					}

					// === 🔍 Validate duplicate Gsdescrpition ===
					if (!string.IsNullOrWhiteSpace(model.Gsdescrpition))
					{
						var duplicateDescription = await dbContext.Tbl20164GoodsAndServicesMasters
							.Where(x => x.Gsdescrpition == model.Gsdescrpition && x.Gscode != model.Gscode)
							.Select(x => x.Gscode)
							.FirstOrDefaultAsync();

						if (!string.IsNullOrEmpty(duplicateDescription))
						{
							return BadRequest(new
							{
								success = false,
								message = $"Stock Item with this Stock Description has already been added to the database (GSCode: {duplicateDescription})."
							});
						}
					}


					if (existing != null)
					{
						// === ✏️ Update existing record ===
						existing.Gsdescrpition = model.Gsdescrpition;
						existing.GsdescriptionAr = model.GsdescriptionAr;
						existing.GsdetailedDesc = model.GsdetailedDesc;
						existing.GsdetailedDescAr = model.GsdetailedDescAr;
						existing.GsgroupId = model.GsgroupId;
						existing.GsuoM = model.GsuoM;
						existing.GspackingUnit = model.GspackingUnit;
						existing.GssellingRate = model.GssellingRate;
						existing.CostPrice = model.CostPrice;
						existing.ItemPartNo = model.ItemPartNo;
						existing.ModifiedOn = now;
						existing.ModifiedBy = "User"; // TODO: Replace with actual user identity
					}
					else
					{
						// === ➕ Insert new record ===
						string prefix = "TRM"; // default
						if (!string.IsNullOrWhiteSpace(model.Gscode))
						{
							var parts = model.Gscode.Split('-');
							if (parts.Length > 0)
								prefix = parts[0].ToUpper();
						}

						var fullPrefix = prefix + "-";

						var maxCode = await dbContext.Tbl20164GoodsAndServicesMasters
							.Where(x => x.Gscode.StartsWith(fullPrefix))
							.OrderByDescending(x => x.Gscode)
							.Select(x => x.Gscode)
							.FirstOrDefaultAsync();

						int nextNo = 1;
						if (!string.IsNullOrEmpty(maxCode))
						{
							var numberPart = maxCode.Substring(fullPrefix.Length);
							if (int.TryParse(numberPart, out int parsedNo))
							{
								nextNo = parsedNo + 1;
							}
						}

						model.Gscode = $"{fullPrefix}{nextNo:D5}";
						model.CreatedOn = now;
						model.CreatedBy = "User"; // TODO: Replace with actual user identity

						dbContext.Tbl20164GoodsAndServicesMasters.Add(model);
					}

					await dbContext.SaveChangesAsync();
					return Ok(new { success = true, message = "Saved successfully!", gscode = model.Gscode });
				}
				catch (Exception ex)
				{
					_logger.LogError($"Error in SaveGoodsAndServices: {ex}");
					return StatusCode(500, new { success = false, message = "Error: " + ex.Message });
				}
			}

			return Unauthorized(new { message = "Invalid tenant", success = false });
		}

		[HttpGet]
		public async Task<IActionResult> GetCostCenterAccount()
		{
			try
			{
				if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				{
					var result = await dbContext.Tbl201CostAllocationUnits
					.Select(g => new
					{
						g.CostAllocationUnitId,
						g.CostAllocationUnit


					})
					.ToListAsync();

					return Ok(result);
				}
			}
			catch (Exception ex) { throw ex; }
			return Unauthorized(new { message = "Invalid tenant.", success = false });

		}
		[HttpGet]
		public async Task<IActionResult> GetRevenueAccount()
		{
			try
			{
				if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				{
					var result = await dbContext.Qry201609vatrevenueLedgers
					.Select(g => new
					{
						g.IncomeLedger,
						g.AccountGroup,
						g.AccountGroupId,
						g.AccountId,
						g.AccountGroupUnder,
						g.IsLedgerObselete

					})
					.ToListAsync();

					return Ok(result);
				}
			}
			catch (Exception ex) { throw ex; }
			return Unauthorized(new { message = "Invalid tenant.", success = false });

		}
		[HttpGet]
		public async Task<IActionResult> GetInvoiceCurrency()
		{
			try
			{
				if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				{
					var result = await dbContext.Tbl20169CurrencyExchanges
					.Select(g => new
					{
						g.CurrencyExchangeId,
						g.CurrencyName,
						g.CurrencyNameAr,
						g.ExchangeRate,
						g.CurrencyCode,
						g.CurrencyCodeAr

					})
					.ToListAsync();

					return Ok(result);
				}
			}
			catch (Exception ex) { throw ex; }
			return Unauthorized(new { message = "Invalid tenant.", success = false });

		}
		[HttpGet]
		public async Task<IActionResult> GetTaxCurrency()
		{
			try
			{
				if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				{
					var result = await dbContext.Tbl20169CurrencyExchanges
					.Select(g => new
					{
						g.CurrencyExchangeId,
						g.CurrencyName,
						g.CurrencyNameAr,
						g.ExchangeRate,
						g.CurrencyCode,
						g.CurrencyCodeAr

					})
					.ToListAsync();

					return Ok(result);
				}
			}
			catch (Exception ex) { throw ex; }
			return Unauthorized(new { message = "Invalid tenant.", success = false });

		}
		[HttpGet]
		public async Task<IActionResult> GetDivision()
		{
			try
			{
				if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				{
					var result = await dbContext.Tbl20115CompanyBranches
					.Select(g => new
					{
						g.BranchName,
						g.BranchCode,
						g.BranchNameAr

					})
					.ToListAsync();

					return Ok(result);
				}
			}
			catch (Exception ex) { throw ex; }
			return Unauthorized(new { message = "Invalid tenant.", success = false });

		}
		[HttpGet]
		public async Task<IActionResult> GetSalesPerson()
		{
			try
			{
				if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				{
					var result = await dbContext.Tbl20101SalesPersonMasters
					.Select(g => new
					{
						g.SalesPersonCode,
						g.SalesPersonName,
						g.UserCode,
						g.EmailAddress

					})
					.ToListAsync();

					return Ok(result);
				}
			}
			catch (Exception ex) { throw ex; }
			return Unauthorized(new { message = "Invalid tenant.", success = false });

        }
        [HttpGet]
        public async Task<IActionResult> GetCompanyBranch()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var result = await dbContext.Tbl901CompanyDetails
                    .Select(g => new
                    {
                        g.CompanyId,
                        g.CompanyName,
                        g.SellerGroupVatnumber,
                        g.CompanyVatno,
                        g.CompanyNameAr,
                        g.SellerOtherIdtype,
                        g.SellerOtherSellerId,
                        g.SellerAddressStreet,
                        g.SellerAddressStreetAr,
                        g.SellerAdditionalStreet,
                        g.SellerAdditionalStreetAr,
                        g.SellerBuildingNumber,
                        g.SellerCity,
                        g.SellerCityAr,
                        g.SellerAdditionalNumber,
                        g.SellerProvince,
                        g.SellerProvinceAr,
                        g.SellerPostalCode,
                        g.SellerNeighborhood,
                        g.SellerNeighborhoodAr,
                        g.SellerCountryCode


					})
					.ToListAsync();

					return Ok(result);
				}
			}
			catch (Exception ex) { throw ex; }
			return Unauthorized(new { message = "Invalid tenant.", success = false });

		}
		[HttpGet]
		public async Task<IActionResult> GetOtherIDType()
		{
			try
			{
				if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				{
					var result = await dbContext.Tbl00104SellerIdtypes
					.Select(g => new
					{
						g.SellerOtherIdtype,
						g.SellerIdtypeName

					})
					.ToListAsync();

					return Ok(result);
				}
			}
			catch (Exception ex) { throw ex; }
			return Unauthorized(new { message = "Invalid tenant.", success = false });

		}
		[HttpGet]
		public IActionResult GetAllSignatories()
		{
			try
			{
				if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				{
					var dbSignatories = dbContext.Tbl90104DocumentSignatories
						.Select(s => new Signatory
						{
							SignatoryID = s.SignatoryId.ToString(),
							SignatoryName = s.SignatoryName
						})

						.ToList(); // Materialize the query first

					var combinedSignatories = dbSignatories
						.Union(new[]
						{
					new Signatory
					{
						SignatoryID = "99",
						SignatoryName = "<Not Assigned>"
					}
						})
						.ToList();

					return Ok(combinedSignatories);
				}
			}
			catch (Exception ex)
			{
				// Ideally log the exception, don't just throw
				return StatusCode(500, new { message = ex.Message, success = false });
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}
		[HttpGet]
		public IActionResult GetAccountGroups()
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				var groups = dbContext.Tbl40111PropertyUnitCodes
					.Select(g => new
					{
						g.UnitDescAr,   // Primary Key
						g.UnitDesc,     // Display text
						g.UnitType,
						g.UnitCode // Optional extra info
					})
					.ToList();

				return Ok(groups);
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}
		[HttpPost]
		public ActionResult AddUom(string unitType, string unitDesc, string unitDescAr)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				// Check if the UnitType and UnitDesc already exists
				bool exists = dbContext.Tbl40111PropertyUnitCodes.Any(u =>
					u.UnitType.Trim().ToLower() == unitType.Trim().ToLower() &&
					u.UnitDesc.Trim().ToLower() == unitDesc.Trim().ToLower() &&
					u.UnitDescAr.Trim().ToLower() == unitDescAr.Trim().ToLower());

				if (exists)
				{
					return Json(new { success = false, message = "This Unit Rate Method already exists." });
				}

				int nextUnitCode = 1;
				if (dbContext.Tbl40111PropertyUnitCodes.Any())
				{
					nextUnitCode = dbContext.Tbl40111PropertyUnitCodes.Max(u => u.UnitCode) + 1;
				}

				if (nextUnitCode > byte.MaxValue)
				{
					return Json(new { success = false, message = "Unit code limit exceeded (max 255)." });
				}

				var newUom = new Tbl40111PropertyUnitCode
				{
					UnitCode = (byte)nextUnitCode,
					UnitType = unitType,
					UnitDesc = unitDesc,
					UnitDescAr = unitDescAr
				};

				dbContext.Tbl40111PropertyUnitCodes.Add(newUom);
				dbContext.SaveChanges();

				return Json(new { success = true, unitCode = newUom.UnitCode });
			}

			return Json(new { success = false, message = "Unable to get tenant context" });
		}

		[HttpGet]
		public IActionResult GetVatTaxSlabs()
		{
			try
			{
				if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				{
					var vatTaxSlabs = dbContext.Tbl20163VatTaxSlabs
		 .Select(x => new
		 {
			 x.TaxSlabCode,
			 x.TaxSlab,
			 x.TaxRate,
			 x.TaxCodeToDisplay
		 })
		 .ToList();

					return Ok(vatTaxSlabs);
				}
			}
			catch (Exception ex)
			{
				// Ideally log the exception, don't just throw
				return StatusCode(500, new { message = ex.Message, success = false });
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}
		[HttpGet]
		public IActionResult GetAllSign()
		{
			try
			{
				if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				{
					var dbSignatories = dbContext.Tbl90104DocumentSignatories
						.Select(s => new
						{
							SignatoryID = s.SignatoryId.ToString(),
							s.SignatoryName,
							s.SignatoryPosition,
							s.SignatoryContact,
							s.SignatoryEmail,
							s.SignatureDescription,
							s.SignatureCode,
							s.SignatoryMobile1,
							s.SignatoryMobile2,
							s.SignatoryNameAr,
							s.SignatoryPositionAr,
							s.IsFinanceManager,
							s.UserId,
							SignatureImage = s.SignatureImage != null
			? Convert.ToBase64String(s.SignatureImage)
			: null
						})
						.ToList();

					return Ok(dbSignatories);
				}
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = ex.Message, success = false });
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}
		[HttpPost]
		public async Task<IActionResult> SaveSignatory([FromBody] Tbl90104DocumentSignatory model)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					var now = DateTime.Now;

					var existing = await dbContext.Tbl90104DocumentSignatories
						.FirstOrDefaultAsync(x => x.SignatoryId == model.SignatoryId);

					if (existing != null)
					{
						existing.SignatoryName = model.SignatoryName;
						existing.SignatoryPosition = model.SignatoryPosition;
						existing.SignatoryContact = model.SignatoryContact;
						existing.SignatoryEmail = model.SignatoryEmail;
						existing.SignatureImage = model.SignatureImage;
						existing.SignatureDescription = model.SignatureDescription;
						existing.SignatureCode = model.SignatureCode;
						existing.SignatoryMobile1 = model.SignatoryMobile1;
						existing.SignatoryMobile2 = model.SignatoryMobile2;
						existing.SignatoryNameAr = model.SignatoryNameAr;
						existing.SignatoryPositionAr = model.SignatoryPositionAr;
						existing.IsFinanceManager = model.IsFinanceManager;
						existing.UserId = model.UserId;
						// No Created/Modified dates in entity? Add if needed
					}
					else
					{
						dbContext.Tbl90104DocumentSignatories.Add(model);
					}

					await dbContext.SaveChangesAsync();

					return Ok(new { success = true, message = "Saved successfully", id = model.SignatoryId });
				}
				catch (Exception ex)
				{
					_logger.LogError($"Error in SaveSignatory: {ex}");
					return StatusCode(500, new { success = false, message = ex.Message });
				}
			}

			return Unauthorized(new { success = false, message = "Invalid tenant" });
		}



		[HttpPost]
		public async Task<ActionResult> VerifyVoucher(string InvoiceNo)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					var UserName = HttpContext.Session.GetString("UserName");

					if (string.IsNullOrEmpty(InvoiceNo))
					{
						return BadRequest(new { Message = "Voucher number is required." });
					}

					var voucher = dbContext.Tbl20161VatinvoiceMasters.FirstOrDefault(v => v.InvoiceNo == InvoiceNo);

					if (voucher == null)
					{
						return NotFound(new { Message = "Voucher not found." });
					}

					// Update the fields
					voucher.IsVerified = true;
					voucher.VerifiedOn = DateTime.Now;
					voucher.VerifiedBy = UserName;

					dbContext.SaveChanges();

					return Ok(new
					{
						Message = "Voucher verified successfully.",
						VoucherVerifiedBy = UserName,  // Example, replace with actual data if needed
													   //VoucherVerifiedOn = voucher.VoucherApprovedOn.ToString("dd-MMM-yyyy")
					});
				}
				catch (Exception ex)
				{
					return BadRequest(new { Message = ex.Message });
				}
			}
			return Unauthorized(new { message = "Invalid tenant.", success = false });

		}

		[HttpPost]
		public async Task<ActionResult> ApproveVoucher(string InvoiceNo)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					var UserName = HttpContext.Session.GetString("UserName");

					if (string.IsNullOrEmpty(InvoiceNo))
					{
						return BadRequest(new { Message = "InvoiceNo number is required." });
					}

					var voucher = dbContext.Tbl20161VatinvoiceMasters.FirstOrDefault(v => v.InvoiceNo == InvoiceNo);

					if (voucher == null)
					{
						return NotFound(new { Message = "InvoiceNo not found." });
					}

					// Update the fields
					voucher.IsApproved = true;
					voucher.ApprovedOn = DateTime.Now;
					voucher.ApprovedBy = UserName;

					//if (IsDirect == false)
					//{
					//    voucher.IsVerified = true;
					//    voucher.VoucherVerifiedOn = DateTime.Now;
					//    voucher.VoucherVerifiedBy = UserName;

					//}

					dbContext.SaveChanges();

					return Ok(new
					{
						Message = "InvoiceNo verified successfully.",
						VoucherApprovedBy = UserName,  // Example, replace with actual data if needed
													   //VoucherVerifiedOn = voucher.VoucherApprovedOn.ToString("dd-MMM-yyyy")
					});
				}
				catch (Exception ex)
				{
					return BadRequest(new { Message = ex.Message });
				}
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });

		}


		[HttpPost]
		public async Task<ActionResult> PostInvoice(string InvoiceNo, bool IsDirect)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					var UserName = HttpContext.Session.GetString("UserName");

					if (string.IsNullOrEmpty(InvoiceNo))
					{
						return BadRequest(new { Message = "Invoice number is required." });
					}

					var voucher = dbContext.Tbl20161VatinvoiceMasters.FirstOrDefault(v => v.InvoiceNo == InvoiceNo);

					if (voucher == null)
					{
						return NotFound(new { Message = "Invoice not found." });
					}

					// Update the fields
					voucher.IsPosted = true;
					voucher.PostedOn = DateTime.Now;
					voucher.PostedBy = UserName;

					int JustAddedVoucherEntryNoSubLedger = 0;
					int JustAddedVoucherEntryNoCostAlloc = 0;
					bool IsCashOrBankAccount = false;

					// 🔁 Call the stored procedure sp201_62InsertVATtoVoucher
					var result = dbContext.Database.ExecuteSqlRaw("EXEC sp201_62InsertVATtoVoucher @p0,@p1,@p2,@p3", InvoiceNo, JustAddedVoucherEntryNoSubLedger, JustAddedVoucherEntryNoCostAlloc, IsCashOrBankAccount);

					//   var result1 = dbContext.Database.ExecuteSqlRaw("EXEC sp201_62InsertVATtoVoucher_BHD @p0,@p1,@p2,@p3", InvoiceNo, JustAddedVoucherEntryNoSubLedger, JustAddedVoucherEntryNoCostAlloc, IsCashOrBankAccount);



					dbContext.SaveChanges();

					return Ok(new
					{
						Message = "Invoice posted successfully.",
						VoucherVerifiedBy = UserName,  // Example, replace with actual data if needed
													   //VoucherVerifiedOn = voucher.VoucherApprovedOn.ToString("dd-MMM-yyyy")
					});
				}
				catch (Exception ex)
				{
					return BadRequest(new { Message = ex.Message });
				}
			}
			return Unauthorized(new { message = "Invalid tenant.", success = false });

		}

		[HttpGet]
		public async Task<ActionResult> GetVatInvoice(string frmDate, string toDate)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					// Base query (IQueryable for optional filtering)
					var query = dbContext.Qry201807vatcreditNoteRegisterMainViews.AsQueryable();

					// Apply date filter only if both dates are passed
					if (!string.IsNullOrEmpty(frmDate) && !string.IsNullOrEmpty(toDate) &&
						DateTime.TryParseExact(frmDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime from) &&
						DateTime.TryParseExact(toDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime to))
					{
						query = query.Where(x => x.InvoiceDateWtTime >= from && x.InvoiceDateWtTime <= to);
					}

					var vatInvoices = await query.ToListAsync();

					return Json(vatInvoices);
				}
				catch (Exception ex)
				{
					return StatusCode(500, $"Internal server error: {ex.Message}");
				}
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}

		[HttpGet]
		public async Task<ActionResult> GetInvoiceDetails(string InvoiceNo)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{

					var result = dbContext.Tbl20161VatinvoiceMasters
					  .Where(x => x.InvoiceNo == InvoiceNo)
					  .ToList();


					return Json(result);
				}
				catch (Exception ex)
				{
					return StatusCode(500, $"Internal server error: {ex.Message}");
				}
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}

		[HttpGet]
		public async Task<ActionResult> GetGridInvoiceDetails(string InvoiceNo)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					var resultWithVAT = new List<ExpandoObject>();

					var result1 = dbContext.Qry201601vatinvoiceChildren
						.Where(x => x.InvoiceNo == InvoiceNo)
						.ToList();

					foreach (var gridDetails in result1)
					{
						dynamic item = new ExpandoObject();
						var dict = (IDictionary<string, object>)item;

						// Copy all existing fields from gridDetails into dynamic object
						var properties = gridDetails.GetType().GetProperties();
						foreach (var prop in properties)
						{
							dict[prop.Name] = prop.GetValue(gridDetails);
						}

						// Get the TaxRateInWord from the TaxSlab table
						var taxRateInWord = dbContext.Tbl20163VatTaxSlabs
							.Where(x => x.TaxSlabCode == gridDetails.TaxSlabCode)
							.Select(x => x.TaxRateInWord)
							.FirstOrDefault();

						var UnitRateMethodDesc = dbContext.Tbl40111PropertyUnitCodes
				   .Where(x => x.UnitCode == gridDetails.UnitRateMethod)
				   .Select(x => x.UnitDesc)
				   .FirstOrDefault();





						//var qty = gridDetails.UnitsToBill;
						//var unitPrice = gridDetails.UnitRate;
						//var vatRate = decimal.TryParse(taxRateInWord.Replace("%", ""), out decimal rate) ? rate / 100 : 0;

						//var amount = qty * unitPrice;
						//var vatValue = amount * vatRate;
						//var totalValue = amount + vatValue;

						// Add new dynamic column
						dict["UnitRateMethodDesc"] = UnitRateMethodDesc;
						dict["VATPercentage"] = taxRateInWord;

						//dict["VAT"] = vatValue;
						//dict["TotalVAT"] = totalValue;

						resultWithVAT.Add(item);
					}

					return Json(resultWithVAT);
				}
				catch (Exception ex)
				{
					return StatusCode(500, $"Internal server error: {ex.Message}");
				}
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}

		[HttpGet]
		public async Task<ActionResult> GetVatPurchaseDetails(string frmDate, string toDate)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					if (!DateTime.TryParseExact(frmDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime from))
						return BadRequest("Invalid from date format. Use MM/dd/yyyy.");

					if (!DateTime.TryParseExact(toDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime to))
						return BadRequest("Invalid to date format. Use MM/dd/yyyy.");

					// Fetch records based on the date range
					var vatInvoices = await dbContext.Qry201707vatpurchaseRegisterMainViews
						.FromSqlRaw("SELECT * FROM qry201_707VATPurchaseRegisterMainView WHERE InvoiceDate BETWEEN @p0 AND @p1", from, to)
						.ToListAsync();

					return Json(vatInvoices);
				}
				catch (Exception ex)
				{
					return StatusCode(500, $"Internal server error: {ex.Message}");
				}
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}
		[HttpGet]
		public IActionResult GetVATInvoiceMaster()
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				// Fetch data from the tbl00105BuyerIDTypes table
				var units = dbContext.Tbl00105BuyerIdtypes
					.Select(u => new
					{
						u.BuyerOtherIdtype,  // Primary key (or value)
						u.BuyerIdtypeName    // Display text
					})
					.ToList();

				return Ok(units);
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}
		[HttpGet]
		public async Task<IActionResult> getInvoiceSubTypeCode(DataSourceLoadOptions loadOptions)
		{

			try
			{

				if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				{
					var qryListOfAccountlists = dbContext.Tbl00108InvoiceSubTypeCodes.Select(i => new
					{

						i.InvoiceSubTypeCode,
						i.InvoiceSubType

					});


					return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
				}

			}
			catch (Exception ex) { throw ex; }

			return Unauthorized(new { message = "Invalid tenant.", success = false });

		}
		[HttpGet]
		public async Task<ActionResult> GetVATCreditNoteNo(DataSourceLoadOptions loadOptions)
		{
			try
			{
				if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				{
					string yearSuffix = DateTime.Now.ToString("yy"); // e.g., "25"
					string creditNoteAbbrv = "CRN"; // Hardcoded abbreviation

					// Get last credit note number
					var lastCreditNoteNumber = await dbContext.Tbl20170VatcreditNoteMasters
						.Where(cn => cn.CreditNoteNo.StartsWith($"{creditNoteAbbrv}-{yearSuffix}-"))
						.OrderByDescending(cn => cn.CreditNoteNo)
						.Select(cn => cn.CreditNoteNo)
						.FirstOrDefaultAsync();

					int newNumber = 1; // Default if no previous credit notes exist
					if (!string.IsNullOrEmpty(lastCreditNoteNumber))
					{
						var match = Regex.Match(lastCreditNoteNumber, @"-(\d+)$");
						if (match.Success)
						{
							newNumber = int.Parse(match.Groups[1].Value) + 1;
						}
					}

					// Generate new Credit Note number
					string newCreditNoteNumber = $"{creditNoteAbbrv}-{yearSuffix}-{newNumber:D5}";

					return Json(newCreditNoteNumber);
				}
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error: {ex.Message}");
			}

			return BadRequest("Failed to retrieve tenant and database context.");
		}

		[HttpGet]
		public async Task<ActionResult> GetVatPurchaseDetail(string frmDate, string toDate)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					if (!DateTime.TryParseExact(frmDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime from))
						return BadRequest("Invalid from date format. Use MM/dd/yyyy.");

					if (!DateTime.TryParseExact(toDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime to))
						return BadRequest("Invalid to date format. Use MM/dd/yyyy.");

					// Fetch records based on the date range
					var vatInvoices = await dbContext.Qry201717vatpurchaseInDetails
						.FromSqlRaw("SELECT * FROM qry201_717VATPurchaseInDetails WHERE PurchaseVoucherDate BETWEEN @p0 AND @p1", from, to)
						.ToListAsync();

					return Json(vatInvoices);
				}
				catch (Exception ex)
				{
					return StatusCode(500, $"Internal server error: {ex.Message}");
				}
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}
		[HttpGet]
		public async Task<ActionResult> GetVatPurchase(string frmDate, string toDate)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					if (!DateTime.TryParseExact(frmDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime from))
						return BadRequest("Invalid from date format. Use MM/dd/yyyy.");

					if (!DateTime.TryParseExact(toDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime to))
						return BadRequest("Invalid to date format. Use MM/dd/yyyy.");

					// Fetch records based on the date range
					var vatInvoices = await dbContext.Qry201723vatonPurchaseAndExpenses
						.FromSqlRaw("SELECT * FROM qry201_723VATonPurchaseAndExpenses WHERE PurchaseVoucherDate BETWEEN @p0 AND @p1", from, to)
						.ToListAsync();

					return Json(vatInvoices);
				}
				catch (Exception ex)
				{
					return StatusCode(500, $"Internal server error: {ex.Message}");
				}
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}


		[HttpGet]
		public string GetNewCreditNoteNo(string invoiceAbbr, int yearInDigit, DateTime invoiceDate, bool isResetByYear)
		{
			string strNewQuotationNo = "";
			try
			{
				int int1 = 0;
				string query;

				if (isResetByYear)
				{
					query = $"SELECT MAX(CAST(RIGHT(CreditNoteNo, 6) AS INT)) FROM tbl20170VATCreditNoteMaster WHERE YEAR(CreditNoteDate) = '{invoiceDate.Year}'";
				}
				else
				{
					query = "SELECT MAX(CAST(RIGHT(CreditNoteNo, 6) AS INT)) FROM tbl20170VATCreditNoteMaster";
				}

				using (SqlConnection conn = new SqlConnection("your_connection_string"))
				{
					SqlCommand cmd = new SqlCommand(query, conn);
					conn.Open();
					object result = cmd.ExecuteScalar();
					int1 = result != DBNull.Value && result != null ? Convert.ToInt32(result) : 0;
				}

				int1 += 1;
				strNewQuotationNo = "000000" + int1.ToString();
				strNewQuotationNo = strNewQuotationNo.Substring(strNewQuotationNo.Length - 6); // Keep only last 6 digits

				if (yearInDigit == 0)
				{
					yearInDigit = 0;
				}

				string strYear = invoiceDate.Year.ToString();
				strYear = strYear.Substring(strYear.Length - yearInDigit, yearInDigit);

				strNewQuotationNo = $"CRN-{strYear}-{strNewQuotationNo}";
				return strNewQuotationNo;
			}
			catch (Exception ex)
			{
				string strYear = invoiceDate.Year.ToString();
				strYear = strYear.Substring(strYear.Length - yearInDigit, yearInDigit);
				strNewQuotationNo = $"CRN-{strYear}-000001";
				return strNewQuotationNo;
			}
		}


		[HttpGet]
		public async Task<IActionResult> GetSupplierInvoices(DataSourceLoadOptions loadOptions)
		{
			try
			{
				if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				{
					var supplierInvoices = dbContext.Tbl20166VatpurchaseMasters
						.Where(i => i.PurchaseBillNo != null)
						.Select(i => new
						{
							i.PurchaseBillNo
						});

					return Json(await DataSourceLoader.LoadAsync(supplierInvoices, loadOptions));
				}
			}
			catch (Exception ex)
			{
				// Optional: log the exception before throwing
				throw;
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}

		[HttpGet]
		public IActionResult GetStores()
		{
			try
			{
				if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				{
					var stores = dbContext.Tbl60001storeMasters
				.Select(s => new
				{
					s.StoreId,
					s.StoreName
				})
				.ToList();

					return Ok(stores);
				}
			}
			catch (Exception ex)
			{
				// Optional: log the exception before throwing
				throw;
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}

		[HttpGet]
		public async Task<ActionResult> GetVatCreditNoteDetails(string frmDate, string toDate)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					if (!DateTime.TryParseExact(frmDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime from))
						return BadRequest("Invalid from date format. Use MM/dd/yyyy.");

					if (!DateTime.TryParseExact(toDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime to))
						return BadRequest("Invalid to date format. Use MM/dd/yyyy.");

					// Fetch records based on the date range
					var vatInvoices = await dbContext.Qry201807vatcreditNoteRegisterMainViews
						.FromSqlRaw("SELECT * FROM qry201_807VATCreditNoteRegisterMainView WHERE CreditNoteDate BETWEEN @p0 AND @p1", from, to)
						.ToListAsync();

					return Json(vatInvoices);
				}
				catch (Exception ex)
				{
					return StatusCode(500, $"Internal server error: {ex.Message}");
				}
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}

		[HttpGet]
		public async Task<ActionResult> GetCreditNoteDetails(string CreditNoteNo)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{

					var result = dbContext.Tbl20170VatcreditNoteMasters
					  .Where(x => x.CreditNoteNo == CreditNoteNo)
					  .ToList();


					return Json(result);
				}
				catch (Exception ex)
				{
					return StatusCode(500, $"Internal server error: {ex.Message}");
				}
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}


		[HttpGet]
		public async Task<ActionResult> GetGridCreditNoteDetails(string CreditNoteNo)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					var resultWithVAT = new List<ExpandoObject>();

					var result1 = dbContext.Qry201801vatcreditNoteChildren
						.Where(x => x.CreditNoteNo == CreditNoteNo)
						.ToList();

					foreach (var gridDetails in result1)
					{
						dynamic item = new ExpandoObject();
						var dict = (IDictionary<string, object>)item;

						// Copy all existing fields from gridDetails into dynamic object
						var properties = gridDetails.GetType().GetProperties();
						foreach (var prop in properties)
						{
							dict[prop.Name] = prop.GetValue(gridDetails);
						}

						// Get the TaxRateInWord from the TaxSlab table
						var taxRateInWord = dbContext.Tbl20163VatTaxSlabs
							.Where(x => x.TaxSlabCode == gridDetails.TaxSlabCode)
							.Select(x => x.TaxRateInWord)
							.FirstOrDefault();

						var UnitRateMethodDesc = dbContext.Tbl40111PropertyUnitCodes
				   .Where(x => x.UnitCode == gridDetails.UnitRateMethod)
				   .Select(x => x.UnitDesc)
				   .FirstOrDefault();

						//var qty = gridDetails.UnitsToBill;
						//var unitPrice = gridDetails.UnitRate;
						//var vatRate = decimal.TryParse(taxRateInWord.Replace("%", ""), out decimal rate) ? rate / 100 : 0;

						//var amount = qty * unitPrice;
						//var vatValue = amount * vatRate;
						//var totalValue = amount + vatValue;

						// Add new dynamic column
						dict["UnitRateMethodDesc"] = UnitRateMethodDesc;
						dict["VATPercentage"] = taxRateInWord;

						//dict["VAT"] = vatValue;
						//dict["TotalVAT"] = totalValue;

						resultWithVAT.Add(item);
					}

					return Json(resultWithVAT);
				}
				catch (Exception ex)
				{
					return StatusCode(500, $"Internal server error: {ex.Message}");
				}
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}

		[HttpPost]
		public async Task<ActionResult> CreditVerifyVoucher(string CreditNoteNo)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					var UserName = HttpContext.Session.GetString("UserName");

					if (string.IsNullOrEmpty(CreditNoteNo))
					{
						return BadRequest(new { Message = "Credit number is required." });
					}

					var voucher = dbContext.Tbl20170VatcreditNoteMasters.FirstOrDefault(v => v.CreditNoteNo == CreditNoteNo);

					if (voucher == null)
					{
						return NotFound(new { Message = "Credit not found." });
					}

					// Update the fields
					voucher.IsVerified = true;
					voucher.VerifiedOn = DateTime.Now;
					voucher.VerifiedBy = UserName;

					dbContext.SaveChanges();

					return Ok(new
					{
						Message = "CreditNoteNo verified successfully.",
						VoucherVerifiedBy = UserName,  // Example, replace with actual data if needed
													   //VoucherVerifiedOn = voucher.VoucherApprovedOn.ToString("dd-MMM-yyyy")
					});
				}
				catch (Exception ex)
				{
					return BadRequest(new { Message = ex.Message });
				}
			}
			return Unauthorized(new { message = "Invalid tenant.", success = false });

		}

		[HttpPost]
		public async Task<ActionResult> CreditApproveVoucher(string CreditNoteNo)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					var UserName = HttpContext.Session.GetString("UserName");

					if (string.IsNullOrEmpty(CreditNoteNo))
					{
						return BadRequest(new { Message = "CreditNoteNo number is required." });
					}

					var voucher = dbContext.Tbl20170VatcreditNoteMasters.FirstOrDefault(v => v.CreditNoteNo == CreditNoteNo);

					if (voucher == null)
					{
						return NotFound(new { Message = "CreditNoteNo not found." });
					}

					// Update the fields
					voucher.IsApproved = true;
					voucher.ApprovedOn = DateTime.Now;
					voucher.ApprovedBy = UserName;

					//if (IsDirect == false)
					//{
					//    voucher.IsVerified = true;
					//    voucher.VoucherVerifiedOn = DateTime.Now;
					//    voucher.VoucherVerifiedBy = UserName;

					//}

					dbContext.SaveChanges();

					return Ok(new
					{
						Message = "CreditNoteNo approved successfully.",
						VoucherApprovedBy = UserName,  // Example, replace with actual data if needed
													   //VoucherVerifiedOn = voucher.VoucherApprovedOn.ToString("dd-MMM-yyyy")
					});
				}
				catch (Exception ex)
				{
					return BadRequest(new { Message = ex.Message });
				}
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });

		}


		[HttpPost]
		public async Task<ActionResult> CreditPostInvoice(string CreditNoteNo, bool IsDirect)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				try
				{
					var UserName = HttpContext.Session.GetString("UserName");

					if (string.IsNullOrEmpty(CreditNoteNo))
					{
						return BadRequest(new { Message = "Credit number is required." });
					}

					var voucher = dbContext.Tbl20170VatcreditNoteMasters.FirstOrDefault(v => v.CreditNoteNo == CreditNoteNo);

					if (voucher == null)
					{
						return NotFound(new { Message = "CreditNoteNo not found." });
					}

					// Update the fields
					voucher.IsPosted = true;
					voucher.PostedOn = DateTime.Now;
					voucher.PostedBy = UserName;

					int JustAddedVoucherEntryNoSubLedger = 0;
					int JustAddedVoucherEntryNoCostAlloc = 0;

					// 🔁 Call the stored procedure sp201_62InsertVATtoVoucher
					// var result = dbContext.Database.ExecuteSqlRaw("EXEC sp201_82InsertVATCreditNotetoVoucher @p0,@p1,@p2", CreditNoteNo, JustAddedVoucherEntryNoSubLedger, JustAddedVoucherEntryNoCostAlloc);

					var result = dbContext.Database.ExecuteSqlRaw("EXEC sp201_82InsertVATCreditNotetoVoucher_BHD @p0,@p1,@p2", CreditNoteNo, JustAddedVoucherEntryNoSubLedger, JustAddedVoucherEntryNoCostAlloc);


					//   var result1 = dbContext.Database.ExecuteSqlRaw("EXEC  sp201_82InsertVATCreditNotetoVoucher_BHD @p0,@p1,@p2,@p3", InvoiceNo, JustAddedVoucherEntryNoSubLedger, JustAddedVoucherEntryNoCostAlloc, IsCashOrBankAccount);



					dbContext.SaveChanges();

					return Ok(new
					{
						Message = "Invoice posted successfully.",
						VoucherVerifiedBy = UserName,  // Example, replace with actual data if needed
													   //VoucherVerifiedOn = voucher.VoucherApprovedOn.ToString("dd-MMM-yyyy")
					});
				}
				catch (Exception ex)
				{
					return BadRequest(new { Message = ex.Message });
				}
			}
			return Unauthorized(new { message = "Invalid tenant.", success = false });

		}
		[HttpGet]
		public async Task<IActionResult> GetSupplierCode(DataSourceLoadOptions loadOptions)
		{
			try
			{
				if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				{

					var qrySupplierCodeList = dbContext.Qry201710vatsundryCreditorsAndCashAccs
					  .Where(x => x.RecordStatus == "Record Complete" && (x.IsLedgerObselete == null || x.IsLedgerObselete == false))
						 .Select(i => new
						 {
							 i.AccountId,
							 i.AccountHead,
							 i.AccountHeadArabic,
							 i.AccountGroup,
							 i.SupplierName,
							 i.SupplierNameAr,
							 i.SupplierAddress,
							 i.SupplierAddressAr,
							 i.SupplierVatno,
							 i.AccountGroupId,
							 i.RecordStatus,
							 i.MasterGroupId,
							 i.MasterGroup,
							 i.SupplierGroupVatnumber,
							 i.SupplierOtherIdtype,
							 i.SupplierOtherId,
							 i.SupplierAddressStreet,
							 i.SupplierAddressStreetAr,
							 i.SupplierAdditionalStreet,
							 i.SupplierAdditionalStreetAr,
							 i.SupplierBuildingNumber,
							 i.SupplierAdditionalNumber,
							 i.SupplierCity,
							 i.SupplierCityAr,
							 i.SupplierPostalCode,
							 i.SupplierProvince,
							 i.SupplierProvinceAr,
							 i.SupplierNeighborhood,
							 i.SupplierNeighborhoodAr,
							 i.SupplierCountryCode
						 });

					return Json(await DataSourceLoader.LoadAsync(qrySupplierCodeList, loadOptions));
				}
			}
			catch (Exception ex)
			{
				throw;
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}


		[HttpGet]
		public async Task<IActionResult> GetSalesOrderNo(string accheadid)
		{
			try
			{
				if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				{
					var result = dbContext.Qry60210ordersBalanceToInvoices
	 .Where(o => o.ClientAccountLedgerNo == accheadid)
	 .Select(o => new
	 {
		 o.SalesOrderNo,
		 o.TotalOrderValue,
		 o.TotalInvoicedValue,
		 o.BalanceToInvoiceValue,
		 o.ClientPono,
		 o.ClientPodate,
		 o.ClientCode,
		 o.ClientAccountLedgerNo,
		 o.AccountHead,
		 o.OrderValueWoTax,
		 o.BalanceToInvoiceWoTax
	 })
	 .ToList();


					return Ok(result);
				}
			}
			catch (Exception ex) { throw ex; }
			return Unauthorized(new { message = "Invalid tenant.", success = false });

		}
		[HttpGet]
		public async Task<IActionResult> GetCompanyBranch(DataSourceLoadOptions loadOptions)
		{
			try
			{
				if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				{
					var supplierInvoices = dbContext.Tbl20166VatpurchaseMasters
						.Where(i => i.PurchaseBillNo != null)
						.Select(i => new
						{
							i.PurchaseBillNo
						});

					return Json(await DataSourceLoader.LoadAsync(supplierInvoices, loadOptions));
				}
			}
			catch (Exception ex)
			{
				// Optional: log the exception before throwing
				throw;
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}

		[HttpGet]
		public async Task<IActionResult> GetCompanyList(DataSourceLoadOptions loadOptions)
		{
			try
			{
				if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				{
					var companyList = dbContext.Tbl901CompanyDetails
						.Select(c => new
						{
							c.CompanyId,
							c.CompanyName
						});

					return Json(await DataSourceLoader.LoadAsync(companyList, loadOptions));
				}
			}
			catch (Exception ex)
			{
				throw;
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}
		[HttpGet]
		public async Task<IActionResult> GetAllSuppliers(DataSourceLoadOptions loadOptions)
		{
			try
			{
				if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				{
					var query = dbContext.Tbl20166VatpurchaseMasters
						.Select(s => new
						{
							s.SupplierCode,
							s.SupplierNameAr,
							s.SupplierName
						});

					return Json(await DataSourceLoader.LoadAsync(query, loadOptions));
				}
			}
			catch (Exception ex)
			{
				// Optional: log the error
				return StatusCode(500, new { message = "Internal Server Error", detail = ex.Message });
			}

			return Unauthorized(new { message = "Invalid tenant", success = false });
		}
		[HttpGet]
		public async Task<IActionResult> GetRevenueAccounts()
		{
			try
			{
				if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				{
					var result = await dbContext.Qry201709vatexpensesLedgers
						.Select(g => new
						{
							g.AccountId,           // Value member for GridLookUpEdit
							g.IncomeLedger,        // Display member
							g.AccountGroup,
							g.AccountGroupId,
							g.AccountGroupUnder,
							g.IsLedgerObselete,
							g.AccountHeadArabic    // Optional: For multilingual support
						})
						.ToListAsync();

					return Ok(result);
				}
			}
			catch (Exception)
			{
				throw;
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}
		[HttpGet]
		public async Task<ActionResult> GetNewPurchaseVoucherNo()
		{
			try
			{
				if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				{
					string invoiceAbbr = "PUR"; // Fixed abbreviation
					DateTime invoiceDate = DateTime.Now;
					string yearDigits = invoiceDate.ToString("yy");

					// Format: PUR-YY-
					string invoicePrefix = $"{invoiceAbbr}-{yearDigits}-";

					// Get last voucher number matching current year
					var lastInvoiceNumber = await dbContext.Tbl20166VatpurchaseMasters
						.Where(i => i.PurchaseVoucherNo.StartsWith(invoicePrefix))
						.OrderByDescending(i => i.PurchaseVoucherNo)
						.Select(i => i.PurchaseVoucherNo)
						.FirstOrDefaultAsync();

			
					int newNumber = 1;
					if (!string.IsNullOrEmpty(lastInvoiceNumber))
					{
						// Extract numeric part after last hyphen
						var match = Regex.Match(lastInvoiceNumber, @"(\d{6})$");
						if (match.Success)
						{
							newNumber = int.Parse(match.Groups[1].Value) + 1;
						}
					}

					// Build new voucher number: PUR-YY-000001
					string newPurchaseVoucherNo = $"{invoiceAbbr}-{yearDigits}-{newNumber:D6}";

					return Json(newPurchaseVoucherNo);
				}
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error: {ex.Message}");
			}

			return BadRequest("Failed to retrieve tenant and database context.");
		}
	

        [HttpPost]
        public async Task<ActionResult> InsertCloneEInvoice(string InvoiceNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    // Fetch the invoice master details using the given InvoiceNo
                    var invoice = await dbContext.Tbl20161VatinvoiceMasters
                                                 .FirstOrDefaultAsync(i => i.InvoiceNo == InvoiceNo);

                    if (invoice == null)
                    {
                        return NotFound(new { Message = "Invoice not found." });
                    }

                    string yearSuffix = DateTime.Now.ToString("yy"); // Get last two digits of the year

                    // Get invoice abbreviation
                    var invoiceAbbrv = await dbContext.Tbl901CompanyDetails
                        .Select(c => c.EinvoiceAbbrv)
                        .FirstOrDefaultAsync();

                    if (string.IsNullOrEmpty(invoiceAbbrv))
                        return BadRequest("Invoice abbreviation not found.");

                    // Get last invoice number
                    var lastInvoiceNumber = await dbContext.Tbl20161VatinvoiceMasters
                        .Where(i => i.InvoiceNo.StartsWith($"{invoiceAbbrv}{yearSuffix}-"))
                        .OrderByDescending(i => i.InvoiceNo)
                        .Select(i => i.InvoiceNo)
                        .FirstOrDefaultAsync();

                    int newNumber = 1; // Default if no previous invoices exist
                    if (!string.IsNullOrEmpty(lastInvoiceNumber))
                    {
                        var match = Regex.Match(lastInvoiceNumber, @"-(\d+)$");
                        if (match.Success)
                        {
                            newNumber = int.Parse(match.Groups[1].Value) + 1;
                        }
                    }

                    // Generate new invoice number
                    string newInvoiceNumber = $"{invoiceAbbrv}{yearSuffix}-{newNumber:D5}";

                    // Extract values from the fetched invoice
                    string ToInvoiceNo = newInvoiceNumber; // You can generate or assign this as needed
                    DateTime InvoiceDate = invoice.InvoiceDate ?? DateTime.Now;
                    string AddedBy = invoice.AddedBy ?? "System"; // Fallback if null
                    DateTime AddedOn = invoice.AddedOn ?? DateTime.Now;
                    string InvoiceUUID = invoice.InvoiceUuid ?? Guid.NewGuid().ToString();
                    long? InvoiceCounterValue = invoice.InvoiceCounterValue;

                    // Execute the stored procedure
                    var result = dbContext.Database.ExecuteSqlRaw(
                        "EXEC sp201_67InsertClone_EInvoice @p0,@p1,@p2,@p3,@p4,@p5,@p6",
                        InvoiceNo, ToInvoiceNo, InvoiceDate, AddedBy, AddedOn, InvoiceUUID, InvoiceCounterValue);

                    dbContext.SaveChanges();

                    return Ok(new
                    {
                        Message = "Invoice cloned successfully.",
                        VoucherVerifiedBy = User.Identity?.Name ?? "System"
                    });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { Message = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }


        [HttpPost]
        public async Task<ActionResult> InsertAmendEInvoice(string InvoiceNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    string yearSuffix = DateTime.Now.ToString("yy"); // e.g., "25"
                    string creditNoteAbbrv = "CRN"; // Hardcoded abbreviation

                    // Get last credit note number
                    var lastCreditNoteNumber = await dbContext.Tbl20170VatcreditNoteMasters
                        .Where(cn => cn.CreditNoteNo.StartsWith($"{creditNoteAbbrv}-{yearSuffix}-"))
                        .OrderByDescending(cn => cn.CreditNoteNo)
                        .Select(cn => cn.CreditNoteNo)
                        .FirstOrDefaultAsync();

                    int newNumber = 1; // Default if no previous credit notes exist
                    if (!string.IsNullOrEmpty(lastCreditNoteNumber))
                    {
                        var match = Regex.Match(lastCreditNoteNumber, @"-(\d+)$");
                        if (match.Success)
                        {
                            newNumber = int.Parse(match.Groups[1].Value) + 1;
                        }
                    }

                    // Generate new Credit Note number
                    string newCreditNoteNumber = $"{creditNoteAbbrv}-{yearSuffix}-{newNumber:D5}";

                    // Fetch the invoice master details using the given InvoiceNo
                    var invoice = await dbContext.Tbl20161VatinvoiceMasters
                                                 .FirstOrDefaultAsync(i => i.InvoiceNo == InvoiceNo);

                    // Extract values from the fetched invoice
                    string CreditNoteNo = newCreditNoteNumber; // You can generate or assign this as needed
                    DateTime InvoiceDate = invoice.InvoiceDate ?? DateTime.Now;
                    string AddedBy = invoice.AddedBy ?? "System"; // Fallback if null
                    DateTime AddedOn = invoice.AddedOn ?? DateTime.Now;
                    string CreditNoteUUID = invoice.InvoiceUuid ?? Guid.NewGuid().ToString();
                    long? InvoiceCounterValue = invoice.InvoiceCounterValue;

                    // Execute the stored procedure
                    var result = dbContext.Database.ExecuteSqlRaw(
                        "EXEC sp201_66InsertCreditNoteFromInvoice @p0,@p1,@p2,@p3,@p4,@p5,@p6",
                        InvoiceNo, CreditNoteNo, AddedBy, AddedOn, CreditNoteUUID, InvoiceCounterValue);

                    dbContext.SaveChanges();

                    return Ok(new
                    {
                        Message = "Invoice Amended successfully.",
                        VoucherVerifiedBy = User.Identity?.Name ?? "System"
                    });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { Message = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }


        [HttpGet]
        public async Task<ActionResult> GetVATRetursSalesandPurchase(string frmDate, string toDate)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    if (!DateTime.TryParseExact(frmDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime from))
                        return BadRequest("Invalid from date format. Use MM/dd/yyyy.");

                    if (!DateTime.TryParseExact(toDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime to))
                        return BadRequest("Invalid to date format. Use MM/dd/yyyy.");

               
                    var allLedgerData = await dbContext.VATFinalReturnsSummarys
        .FromSqlRaw("EXEC sp201_999VATFinalReturns @p0, @p1", from, to)
        .ToListAsync(); 

                    return Json(allLedgerData);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }




    }
}


