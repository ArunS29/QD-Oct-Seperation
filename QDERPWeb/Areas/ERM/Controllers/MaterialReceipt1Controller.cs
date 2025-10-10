using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System.Dynamic;

namespace QD.ERP.Web.Areas.ERM.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class MaterialReceipt1Controller : Controller
	{

		private readonly TenantDbContextHelper _tenantDbContextHelper;
		private readonly ILogger<MaterialReceipt1Controller> _logger;

		public MaterialReceipt1Controller(ILogger<MaterialReceipt1Controller> logger, TenantDbContextHelper tenantDbContextHelper)
		{
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
					var StoreName = dbContext.Qry70002projectsViewMasters.Select(i => new
					{
						i.ProjectId,
						i.ProjectDescription

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
        //[HttpGet]
        //public ActionResult<string> GetNewDebitNoteNoApi()
        //{
        //	try
        //	{
        //		if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
        //		{


        //			var company = dbContext.Tbl901CompanyDetails
        //								   .FirstOrDefault(c => c.CompanyNameShort == "Pulse Infotech");


        //			if (company == null)
        //			{
        //				return NotFound("Company not found.");
        //			}

        //			string invoiceAbbrv = company.InvoiceAbbrv;
        //			int invoiceYearDigits = company.InvoiceYearDigits ?? 0;

        //			bool isResetInvoiceInYear = company.IsResetInvoiceInYear ?? false;

        //			DateTime invoiceDate = DateTime.Now;



        //			// Step 4: Generate New Debit Note No
        //			string newDebitNoteNo = GetNewDebitNoteNo(invoiceAbbrv, invoiceYearDigits, invoiceDate, isResetInvoiceInYear, dbContext);

        //			return Ok(newDebitNoteNo);
        //		}
        //		else
        //		{
        //			return BadRequest("Tenant or DB Context not found.");
        //		}
        //	}
        //	catch (Exception ex)
        //	{
        //		_logger.LogError($"An error occurred while fetching the data : {ex.Message}");
        //		return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
        //	}
        //}

        [HttpGet]
        public ActionResult<string> GetEquipmentInvoiceNo()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    string defaultCompanyString = HttpContext.Session.GetString("DefaultcompanyID") ?? "";
                    byte defaultCompanyByte = 0;

                    if (!string.IsNullOrEmpty(defaultCompanyString))
                    {
                        byte.TryParse(defaultCompanyString, out defaultCompanyByte);
                    }

                    byte companyId = defaultCompanyByte;

                    var company = dbContext.Tbl901CompanyDetails
                        .FirstOrDefault(c => c.CompanyId == companyId);
                    var company02 = dbContext.Tbl901CompanyDetails02s
                        .FirstOrDefault(c => c.CompanyId == companyId);

                    if (company == null)
                    {
                        return NotFound("Company not found.");
                    }

                    // Use Equipment-related fields
                    string invoiceAbbrv = company.EquipSummaryAbbrv ?? "";
                    int invoiceYearDigits = company02.NoOfDigitsToEqpsupplierSumm ?? 0;
                    int NoOfDigitsInInvoiceNo = company02.NoOfDigitsToEqpinvSumm ?? 5;
                    bool isResetInvoiceInYear = company.IsResetInvoiceInYear ?? false;

                    DateTime invoiceDate = DateTime.Now;

                    // Generate new Equipment Invoice No
                    string newInvoiceNo = GetInvoiceNoAPI(
                        invoiceAbbrv,
                        invoiceYearDigits,
                        invoiceDate,
                        isResetInvoiceInYear,
                        NoOfDigitsInInvoiceNo
                    );

                    return Ok(newInvoiceNo);
                }
                else
                {
                    return BadRequest("Tenant or DB Context not found.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        public string GetInvoiceNoAPI(string invoiceAbbr, int yearInDigit, DateTime invoiceDate, bool isResetByYear, int NoOfDigitsInInvoiceNo)
        {
            string strYear = "";
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    int maxNumber = 0;

                    var query = dbContext.Tbl40118PropertyInvoiceMasters.AsQueryable();

                    if (isResetByYear)
                    {
                        query = query.Where(d => d.InvoiceDate.HasValue && d.InvoiceDate.Value.Year == invoiceDate.Year);
                    }

                    maxNumber = query
                        .Select(d => d.InvoiceNo)
                        .Where(no => !string.IsNullOrEmpty(no) && no.Length >= NoOfDigitsInInvoiceNo)
                        .AsEnumerable()
                        .Select(no => int.TryParse(no.Substring(no.Length - NoOfDigitsInInvoiceNo), out int number) ? number : 0)
                        .DefaultIfEmpty(0)
                        .Max();

                    maxNumber += 1;

                    // Prepare Year Part
                    strYear = invoiceDate.Year.ToString();
                    if (yearInDigit > 0 && yearInDigit <= 4)
                    {
                        strYear = strYear.Substring(strYear.Length - yearInDigit, yearInDigit);
                    }
                    else if (yearInDigit <= 0)
                    {
                        strYear = "";
                    }

                    // Final Invoice No
                    string newInvoiceNumber = $"{(string.IsNullOrWhiteSpace(invoiceAbbr) ? "" : invoiceAbbr)}{strYear}-{maxNumber.ToString().PadLeft(NoOfDigitsInInvoiceNo, '0')}";

                    return newInvoiceNumber;
                }
                else
                {
                    throw new Exception("Tenant or DbContext not found");
                }
            }
            catch (Exception ex)
            {
                // Fallback Invoice No
                strYear = invoiceDate.Year.ToString();
                if (yearInDigit > 0 && yearInDigit <= 4)
                {
                    strYear = strYear.Substring(strYear.Length - yearInDigit, yearInDigit);
                }
                else
                {
                    strYear = "";
                }

                return $"{(string.IsNullOrWhiteSpace(invoiceAbbr) ? "" : invoiceAbbr)}-{strYear}-000001";
            }
        }
        [HttpGet]
        public IActionResult GetInvoiceSummary(string invoiceNo)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var master = dbContext.Tbl40118PropertyInvoiceMasters
                        .FirstOrDefault(x => x.InvoiceNo == invoiceNo);

                    if (master == null)
                    {
                        return NotFound("Invoice not found.");
                    }

                    var details = dbContext.Tbl40118PropertyInvoiceMasters
                        .Where(x => x.InvoiceNo == invoiceNo)
                        .ToList();

                    return Ok(new
                    {
                        Master = master,
                        Details = details
                    });
                }
                else
                {
                    return BadRequest("Tenant or DB Context not found.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error fetching invoice: " + ex.Message);
            }
        }


        [HttpGet]
        public IActionResult GetAllPropertyDescriptions()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var propertyTypes = dbContext.Tbl40101PropertyMasters
                        .Select(s => new
                        {
                            s.PropertyNo,
                            s.PropertyDescription
                        })
                        .ToList();

                    return Ok(propertyTypes);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error loading Property Types: {ex.Message}");
                    return StatusCode(500, new { message = "Failed to load Property Types.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant." });
        }
        [HttpGet]
        public async Task<IActionResult> GetInvoiceDetails(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return BadRequest(new { message = "Unable to resolve tenant or DbContext." });

                // Default date range: first day → last day of current month
                fromDate ??= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                toDate ??= new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));

                var query = dbContext.Qry40115PropertyInvoiceDetails.AsQueryable();

                // Apply optional date filter if InvoiceDate exists
                if (fromDate.HasValue && toDate.HasValue)
                {
                    query = query.Where(i => i.InvoiceDate >= fromDate && i.InvoiceDate <= toDate);
                }

                var data = await query.Select(i => new
                {
                    i.InvoiceNo,
                    i.InvoiceDate,
                    i.InvoicePeriod,
                    i.ClientCode,
                    i.ClientName,
                    i.PropertyNo,
                    i.PropertyDescription,
                    i.QuantityInvoiced,
                    i.UnitRate,
                    i.UoM,
                    i.TotalAmount,
                    i.TotalAdditions,
                    i.TotalDeductions,
                    i.NetAmount,
                    i.TotalVatamount,
                    i.TotalWithVatamount,
                    i.VatinvoiceNo,
                    i.TaxRateInWord,
                    i.Remarks,
                    i.DeliveryNoteNos
                }).ToListAsync();

                _logger.LogInformation($"GetInvoiceDetails returned {data.Count} rows for tenant {tenant.Name}.");

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in VATModule/GetInvoiceDetails");
                return StatusCode(500, new { message = "Error while fetching invoice details.", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateSummary(string invoiceNo, DateTime invoiceDueDate)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }

            try
            {
                // Call your stored procedure
                await dbContext.Database.ExecuteSqlRawAsync(
                    "EXEC stpro401_08InsertToEquipInvoiceFromBillSummary @p0",
                    invoiceNo
                );

                // Update InvoiceDueDate
                await dbContext.Database.ExecuteSqlRawAsync(
                    "UPDATE tbl20161VATInvoiceMaster SET InvoiceDueDate = {0} WHERE InvoiceNo = {1}",
                    invoiceDueDate, invoiceNo
                );

                return Json(new { success = true, invoiceNo });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> CreateVatInvoiceFromSummary([FromBody] string billSummaryNo)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return BadRequest("Tenant not found.");

            try
            {
                // 🔹 Call your stored procedure
                var result = await dbContext.Database
                    .SqlQueryRaw<string>("EXEC stpro401_08InsertToEquipInvoiceFromBillSummary @BillSummaryNo = {0}", billSummaryNo)
                    .ToListAsync();

                // Assume SP returns new InvoiceNo
                string newInvoiceNo = result.FirstOrDefault();

                // ✅ Optional: update Due Date logic
                await dbContext.Database.ExecuteSqlRawAsync(
                    "UPDATE tbl20161VATInvoiceMaster SET InvoiceDueDate = DATEADD(DAY, 30, InvoiceDate) WHERE InvoiceNo = {0}",
                    newInvoiceNo
                );

                return Ok(new { success = true, invoiceNo = newInvoiceNo });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
      [HttpPost("CreateOrUpdateDetailedInvoice")]
public async Task<IActionResult> CreateOrUpdateDetailedInvoice([FromBody] List<BillingInvoiceRequest> requests)
{
    if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
        return Json(new { success = false, message = "Tenant or DB context not found." });

    var resultList = new List<object>();

    foreach (var request in requests)
    {
        try
        {
            // 1️⃣ Check if invoice already exists
            var existingInvoice = dbContext.Tbl20161VatinvoiceMasters
                .FirstOrDefault(x => x.InvoiceSummNo == request.BillingSummaryNo);

            if (existingInvoice != null)
            {
                // 2️⃣ Update the Due Date using raw SQL
                var updateSql = $"UPDATE tbl20161VATInvoiceMaster SET InvoiceDueDate = '{request.InvoiceDueDate:yyyy-MM-dd}' WHERE InvoiceNo = '{existingInvoice.InvoiceNo}'";
                await dbContext.Database.ExecuteSqlRawAsync(updateSql);

                resultList.Add(new
                {
                    BillingSummaryNo = request.BillingSummaryNo,
                    status = "Updated",
                    invoiceNo = existingInvoice.InvoiceNo
                });

                continue;
            }

            // 3️⃣ Execute stored procedure to create invoice
            await dbContext.Database.ExecuteSqlRawAsync(
                "EXEC stpro401_08InsertToEquipDetailedInvoiceFromBillSummary @BillingSummaryNo = {0}",
                request.BillingSummaryNo
            );

            // 4️⃣ Retrieve the newly created invoice
            var newInvoice = dbContext.Tbl20161VatinvoiceMasters
                .OrderByDescending(x => x.AddedOn)
                .FirstOrDefault(x => x.InvoiceSummNo == request.BillingSummaryNo);

            if (newInvoice != null)
            {
                resultList.Add(new
                {
                    BillingSummaryNo = request.BillingSummaryNo,
                    status = "Created",
                    invoiceNo = newInvoice.InvoiceNo
                });
            }
            else
            {
                resultList.Add(new
                {
                    BillingSummaryNo = request.BillingSummaryNo,
                    status = "Failed",
                    invoiceNo = ""
                });
            }
        }
        catch (Exception ex)
        {
            resultList.Add(new
            {
                BillingSummaryNo = request.BillingSummaryNo,
                status = "Error",
                message = ex.Message
            });
        }
    }

    return Json(new { success = true, results = resultList });
}

public class BillingInvoiceRequest
{
    public string BillingSummaryNo { get; set; }
    public DateTime InvoiceDueDate { get; set; }
}


        [HttpGet]
        public async Task<ActionResult> GetInvoiceChildren(string InvoiceNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var resultWithDetails = new List<ExpandoObject>();

                    // Query the Tbl40137PropertyRequestChildren table for the given EquipmentRequestNo
                    var result = dbContext.Qry40119PropertyInvoiceChildWithCalcs
                        .Where(x => x.InvoiceNo == InvoiceNo)
                        .ToList();

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

                        var PropertyDescription = await dbContext.Tbl40101PropertyMasters
                            .Where(x => x.PropertyNo == gridDetails.DetailedDescription)
                            .Select(x => x.PropertyDescription)
                            .FirstOrDefaultAsync();

                        // Retrieve GroupName based on QuoteGroupItemSlNo

                        //var currencyRate = await dbContext.Tbl40136PropertyRequestMasters
                        // .Where(x => x.EqiupmentRequestNo == EquipmentRequestNo)
                        // .Select(x => x.CurrencyRate)
                        // .FirstOrDefaultAsync();

                        //dict["LineTotal"] = gridDetails.LineTotal / currencyRate;
                        //dict["ExpectedUnitRate"] = gridDetails.ExpectedUnitRate / currencyRate;

                        resultWithDetails.Add(item);
                    }

                    return Json(resultWithDetails);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetProject: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
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
					var result = dbContext.Tbl60502materialReceiptChildren
						.Where(x => x.ReceiptNo == ReceiptNo)
						.ToList();

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
        public async Task<IActionResult> SaveOrUpdateInvoice([FromBody] Tbl40118PropertyInvoiceMaster VM)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { success = false, message = "Invalid tenant context." });

            if (VM == null || string.IsNullOrEmpty(VM.InvoiceNo))
                return BadRequest(new { success = false, message = "Invoice No is required." });

            try
            {
                // ===== MASTER =====
                var existingInvoice = await dbContext.Tbl40118PropertyInvoiceMasters
                    .FirstOrDefaultAsync(x => x.InvoiceNo == VM.InvoiceNo);

                if (existingInvoice != null)
                {
                    existingInvoice.InvoiceDate = VM.InvoiceDate;
                    existingInvoice.ClientCode = VM.ClientCode;
                    existingInvoice.QuotationNo = VM.QuotationNo;
                    existingInvoice.ClientReferenceName = VM.ClientReferenceName;
                    existingInvoice.ClientEmail = VM.ClientEmail;
                    existingInvoice.ClientContactNo = VM.ClientContactNo;
                    existingInvoice.TypeOfInvoice = VM.TypeOfInvoice;
                    existingInvoice.Podates = VM.Podates;
                    existingInvoice.VatinvoiceNo = VM.VatinvoiceNo;
                    existingInvoice.DeliveryNoteNos = VM.DeliveryNoteNos;
                    existingInvoice.Pono = VM.Pono;
                    existingInvoice.InvoiceStartDate = VM.InvoiceStartDate;
                    existingInvoice.InvoiceEndDate = VM.InvoiceEndDate;
                    existingInvoice.Remarks = VM.Remarks;
                    existingInvoice.ModifiedBy = "System";
                    existingInvoice.ModifiedOn = DateTime.UtcNow;
                }
                else
                {
                    var newInvoice = new Tbl40118PropertyInvoiceMaster
                    {
                        InvoiceNo = VM.InvoiceNo,
                        InvoiceDate = VM.InvoiceDate,
                        ClientCode = VM.ClientCode,
                        QuotationNo = VM.QuotationNo,
                        ClientReferenceName = VM.ClientReferenceName,
                        ClientEmail = VM.ClientEmail,
                        ClientContactNo = VM.ClientContactNo,
                        TypeOfInvoice = VM.TypeOfInvoice,
                        Podates = VM.Podates,
                        VatinvoiceNo = VM.VatinvoiceNo,
                        DeliveryNoteNos = VM.DeliveryNoteNos,
                        Pono = VM.Pono,
                        InvoiceStartDate = VM.InvoiceStartDate,
                        InvoiceEndDate = VM.InvoiceEndDate,
                        Remarks = VM.Remarks,
                        AddedBy = "System",
                        AddedOn = DateTime.UtcNow
                    };

                    await dbContext.Tbl40118PropertyInvoiceMasters.AddAsync(newInvoice);
                }

                // ===== CHILDREN =====
                if (VM.Items != null && VM.Items.Any())
                {
                    var existingItems = await dbContext.Tbl40119PropertyInvoiceChildren
                        .Where(i => i.InvoiceNo == VM.InvoiceNo)
                        .ToListAsync();

                    var postedIds = VM.Items.Where(x => x.InvoiceChildSlNo > 0).Select(x => x.InvoiceChildSlNo).ToList();
                    var toDelete = existingItems.Where(x => !postedIds.Contains(x.InvoiceChildSlNo)).ToList();
                    dbContext.Tbl40119PropertyInvoiceChildren.RemoveRange(toDelete);

                    foreach (var item in VM.Items)
                    {
                        var existingItem = existingItems.FirstOrDefault(x => x.InvoiceChildSlNo == item.InvoiceChildSlNo);

                        if (existingItem == null)
                        {
                            await dbContext.Tbl40119PropertyInvoiceChildren.AddAsync(item);
                        }
                        else
                        {
                            dbContext.Entry(existingItem).CurrentValues.SetValues(item);
                        }
                    }
                }

                await dbContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Invoice saved/updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in SaveOrUpdateInvoice: {ex.Message}", ex);
                return StatusCode(500, new { success = false, message = "Internal server error. Please try again later." });
            }
        }


        //[HttpPost]
        //public async Task<IActionResult> SaveOrUpdateMaterialReceipt([FromBody] MaterialReceiptViewModel VM)
        //{
        //	if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
        //	{
        //		return Unauthorized(new { success = false, message = "Invalid tenant context." });
        //	}

        //	if (VM == null || string.IsNullOrEmpty(VM.ReceiptNo))
        //	{
        //		return BadRequest(new { success = false, message = "Receipt No is required." });
        //	}

        //	try//
        //	{
        //		// Ensure child list is initialized
        //		//VM.RFQDetailses = VM.RFQDetailses ?? new List<Tbl60702rfqchild>();

        //		// Check if the master record exists
        //		var existingMaster = await dbContext.Tbl60501materialReceiptMasters
        //			.FirstOrDefaultAsync(x => x.ReceiptNo == VM.ReceiptNo);

        //		if (existingMaster != null)
        //		{
        //			//Update existing master with manual property mapping
        //			existingMaster.ReceiptDate = VM.ReceiptDate;
        //			existingMaster.SupplierDeliveryNoteNo = VM.SupplierDeliveryNoteNo;
        //			existingMaster.SupplierCode = VM.SupplierCode;
        //			existingMaster.Mprno = VM.Mprno;
        //			existingMaster.SupplierQuotationNo = VM.SupplierQuotationNo;
        //			existingMaster.JobCode = VM.JobCode;
        //			existingMaster.ClientCode = VM.ClientCode;
        //			existingMaster.Rfqno = VM.Rfqno;
        //			existingMaster.OurPurchaseOrderNo = VM.OurPurchaseOrderNo;
        //			existingMaster.SalesPersonCode = VM.SalesPersonCode;
        //			existingMaster.StoreReceivedIn = VM.StoreReceivedIn;
        //			existingMaster.ProjectMasterCode = VM.ProjectMasterCode;
        //		    existingMaster.ReceiptSignatory = VM.ReceiptSignatory.HasValue ? (byte?)VM.ReceiptSignatory.Value : null;
        //			existingMaster.IssueRemarks = VM.IssueRemarks;
        //		    existingMaster.CompanyBranch = VM.CompanyBranch.HasValue ? (byte?)VM.CompanyBranch.Value : null;
        //			existingMaster.InventoryMasterGroupId = VM.InventoryMasterGroupId.HasValue ? (byte?)VM.InventoryMasterGroupId.Value : null;
        //			existingMaster.ModeOfReceiptId = VM.ModeOfReceiptId.HasValue ? (byte?)VM.ModeOfReceiptId.Value : null;


        //		}
        //		else
        //		{
        //			// Insert new master
        //			var newMaster = new Tbl60501materialReceiptMaster
        //			{

        //		ReceiptNo=VM.ReceiptNo,
        //		ReceiptDate=VM.ReceiptDate,
        //		SupplierDeliveryNoteNo=VM.SupplierDeliveryNoteNo,
        //		SupplierCode=VM.SupplierCode,
        //		Mprno=VM.Mprno,
        //		SupplierQuotationNo=VM.SupplierQuotationNo,
        //		JobCode=VM.JobCode,
        //		ClientCode=VM.ClientCode,
        //		Rfqno = VM.Rfqno,
        //		OurPurchaseOrderNo=VM.OurPurchaseOrderNo,
        //		SalesPersonCode=VM.SalesPersonCode,
        //		StoreReceivedIn=VM.StoreReceivedIn,
        //		ProjectMasterCode=VM.ProjectMasterCode,
        //		ReceiptSignatory=Convert.ToByte(VM.ReceiptSignatory),
        //		IssueRemarks=VM.IssueRemarks,
        //		CompanyBranch=Convert.ToByte(VM.CompanyBranch),
        //		InventoryMasterGroupId =Convert.ToByte(VM.InventoryMasterGroupId),
        //		ModeOfReceiptId=Convert.ToByte(VM.ModeOfReceiptId)


        //			};

        //			await dbContext.Tbl60501materialReceiptMasters.AddAsync(newMaster);
        //		}

        //		// Handle child entries
        //		var existingChildren = await dbContext.Tbl60502materialReceiptChildren
        //			.Where(x => x.ReceiptNo == VM.ReceiptNo)
        //			.ToListAsync();

        //		foreach (var child in VM.MaterialReceiptDetailses)
        //		{
        //			if (child.ReceiptChildSlNo == 0)
        //			{
        //				// New child entry
        //				child.ReceiptNo = VM.ReceiptNo; // Ensure foreign key is set
        //				await dbContext.Tbl60502materialReceiptChildren.AddAsync(child);
        //			}
        //			else
        //			{
        //				// Existing child entry
        //				var existingChild = existingChildren
        //					.FirstOrDefault(x => x.ReceiptChildSlNo == child.ReceiptChildSlNo);

        //				if (existingChild != null)
        //				{
        //					dbContext.Entry(existingChild).CurrentValues.SetValues(child);
        //				}
        //			}
        //		}

        //		await dbContext.SaveChangesAsync();

        //		return Ok(new { success = true, message = "Material Receipt Details saved/updated successfully." });
        //	}
        //	catch (Exception ex)
        //	{
        //		_logger.LogError($"An error occurred while fetching the data : {ex.Message}");
        //		return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
        //	}
        //}
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

				return Ok(new { success = true, message = "Material Receipt details deleted successfully." });
			}
			catch (Exception ex)
			{
				_logger.LogError($"An error occurred while fetching the data : {ex.Message}");
				return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
			}
		}

		


		

	}
}
