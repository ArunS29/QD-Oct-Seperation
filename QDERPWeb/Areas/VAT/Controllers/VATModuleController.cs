using DevExpress.DataProcessing.InMemoryDataProcessor;
using DevExpress.XtraPrinting.BarCode;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.Areas.VAT.Models;
using QD.ERP.Web.Areas.VAT.Reports.VATCreditNote;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using QD.ERP.Web.Services.Logging;
using QDERPWeb.Models;
using QRCoder;
using SkiaSharp;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static QD.ERP.Web.Service.UserAccessService;


namespace QD.ERP.Web.Areas.VAT.Controllers
{

    [Route("api/[controller]/[action]")]
    public class VATModuleController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<VATModuleController> _logger;
        private readonly IUserActionLogger _userActionLogger;

        private readonly FcmService _fcmService;

        public VATModuleController(ILogger<VATModuleController> logger, TenantDbContextHelper tenantDbContextHelper, IUserActionLogger userActionLogger, FcmService fcmService)
        {
            _userActionLogger = userActionLogger;
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
            _fcmService = fcmService;
        }

        //[HttpGet]
        //public async Task<ActionResult> GetVatInvoices(string frmDate, string toDate, bool useEffectiveDate)
        //{
        //	if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
        //	{
        //		try
        //		{
        //			string[] acceptedFormats = { "MM/dd/yyyy", "yyyy-MM-dd" };

        //			if (!DateTime.TryParseExact(frmDate, acceptedFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime from))
        //				return BadRequest("Invalid from date format. Use MM/dd/yyyy or yyyy-MM-dd.");

        //			if (!DateTime.TryParseExact(toDate, acceptedFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime to))
        //				return BadRequest("Invalid to date format. Use MM/dd/yyyy or yyyy-MM-dd.");


        //			// Ensure end date includes the full day (up to 23:59:59)
        //			DateTime toWithTime = to.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

        //			List<Qry201607vatinvoiceRegisterMainView> vatInvoices;

        //			if (useEffectiveDate)
        //			{
        //				// If checkbox is enabled, use InvoiceEffectiveDate
        //				vatInvoices = await dbContext.Qry201607vatinvoiceRegisterMainViews
        //					.FromSqlRaw("SELECT * FROM Qry201_607vatinvoiceRegisterMainView WHERE InvoiceEffectiveDate BETWEEN @p0 AND @p1", from, toWithTime)
        //					.ToListAsync();
        //			}
        //			else
        //			{
        //				// Default: use InvoiceDate (original logic)
        //				vatInvoices = await dbContext.Qry201607vatinvoiceRegisterMainViews
        //					.FromSqlRaw("SELECT * FROM Qry201_607vatinvoiceRegisterMainView WHERE InvoiceDate BETWEEN @p0 AND @p1", from, toWithTime)
        //					.ToListAsync();
        //			}

        //			return Json(vatInvoices);
        //		}
        //		catch (Exception ex)
        //		{
        //			return StatusCode(500, $"Internal server error: {ex.Message}");
        //		}
        //	}

        //	return Unauthorized(new { message = "Invalid tenant.", success = false });
        //}
        //[Area("VAT")]
        //[Route("api/[controller]/[action]")]
        //[ApiController]
        //public class GoodsCombinedController : Controller
        //{
        //    private readonly TenantDbContextHelper _tenantDbContextHelper;
        //    private readonly ILogger<GoodsCombinedController> _logger;

        //    public GoodsCombinedController(ILogger<GoodsCombinedController> logger, TenantDbContextHelper tenantDbContextHelper)
        //    {
        //        _tenantDbContextHelper = tenantDbContextHelper;
        //        _logger = logger;
        //    }

        //    [HttpGet]
        //    public async Task<IActionResult> GetCombinedGoodsData(DateTime? fromDate, DateTime? toDate)
        //    {
        //        try
        //        {
        //            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
        //            {
        //                // Default dates: current month if not passed
        //                fromDate ??= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        //                toDate ??= new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));

        //                _logger.LogInformation($"Running SP and fetching combined goods data from {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}");

        //                // 1. Execute stored procedure
        //                await dbContext.Database.ExecuteSqlRawAsync("EXEC sp20131InsertGoodsCombinedMaster @p0, @p1", fromDate, toDate);

        //                // 2. Fetch filtered result from tbl20124GoodsCombinedMaster
        //                var data = await dbContext.Tbl20124GoodsCombinedMasters
        //                    .Where(x => x.DocumentDate >= fromDate && x.DocumentDate <= toDate)
        //                    .OrderByDescending(x => x.DocumentDate)
        //                    .Select(x => new
        //                    {
        //                        x.DocumentNo,
        //                        x.DocumentDate,
        //                        x.Gscode,
        //                        x.UnitRateMethod,
        //                        x.DocumentQty,
        //                        x.DocumentLineUnitPrice,
        //                        x.Discount,
        //                        x.DocumentLineTotalAmount,
        //                        x.TransactionType
        //                    })
        //                    .ToListAsync();

        //                return Ok(new
        //                {
        //                    Message = "Data retrieved successfully.",
        //                    FromDate = fromDate.Value.ToString("yyyy-MM-dd"),
        //                    ToDate = toDate.Value.ToString("yyyy-MM-dd"),
        //                    Data = data
        //                });
        //            }

        //            return Unauthorized(new { Message = "Invalid tenant context." });
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError($"Error in GetCombinedGoodsData: {ex.Message}");
        //            return StatusCode(500, new
        //            {
        //                Message = "An error occurred while processing the request.",
        //                Error = ex.Message
        //            });
        //        }
        //    }
        //}
       
        public async Task<ActionResult> GetVatInvoices(
    string frmDate,
    string toDate,
    bool useEffectiveDate,
    int pageNumber = 1,
    int pageSize = 10)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    string[] acceptedFormats = { "MM/dd/yyyy", "yyyy-MM-dd" };

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

                    if (!DateTime.TryParseExact(frmDate, acceptedFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime from))
                        return BadRequest("Invalid from date format. Use MM/dd/yyyy or yyyy-MM-dd.");

                    if (!DateTime.TryParseExact(toDate, acceptedFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime to))
                        return BadRequest("Invalid to date format. Use MM/dd/yyyy or yyyy-MM-dd.");

                    DateTime toWithTime = to.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

                    IQueryable<Qry201607vatinvoiceRegisterMainView> query;

                    if (useEffectiveDate)
                    {
                        query = dbContext.Qry201607vatinvoiceRegisterMainViews
                            .FromSqlRaw("SELECT * FROM Qry201_607vatinvoiceRegisterMainView WHERE InvoiceEffectiveDate BETWEEN @p0 AND @p1", from, toWithTime);
                    }
                    else
                    {
                        query = dbContext.Qry201607vatinvoiceRegisterMainViews
                            .FromSqlRaw("SELECT * FROM Qry201_607vatinvoiceRegisterMainView WHERE InvoiceDate BETWEEN @p0 AND @p1", from, toWithTime);
                    }

                    // Get total count before pagination
                    int totalCount = await query.CountAsync();

                    // Apply pagination
                    List<Qry201607vatinvoiceRegisterMainView> pagedResults = await query
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();


                    foreach (var item in pagedResults)
                    {
                        //decimal total = item.TotalInvoiceAmount ?? 0;
                        //decimal rate = item.ExchangeRate ?? 1;
                        //decimal? currencyRate = total * rate;

                        item.CurrencyImage = company.CurrencyImage; // If you are overwriting with converted amount
                        item.CurrencySymbole = company.CurrencySymbol; 

                    }


                    return Json(new
                    {
                        data = pagedResults,
                        totalCount,
                        currentPage = pageNumber,
                        pageSize
                    });
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
                    var resultWithVAT = new List<ExpandoObject>();
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

                    if (!DateTime.TryParseExact(frmDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime from))
                        return BadRequest("Invalid from date format. Use MM/dd/yyyy.");

                    if (!DateTime.TryParseExact(toDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime to))
                        return BadRequest("Invalid to date format. Use MM/dd/yyyy.");

                    // Fetch records based on the date range
                    var vatInvoices = await dbContext.Qry201707vatpurchaseRegisterMainViews
                        .FromSqlRaw("SELECT * FROM qry201_707VATPurchaseRegisterMainView WHERE PurchaseVoucherDate BETWEEN @p0 AND @p1", from, to)
                        .ToListAsync();

                        foreach (var result in vatInvoices)
                        {

                            dynamic item = new ExpandoObject();
                            var dict = (IDictionary<string, object>)item;

                            // Copy all existing fields from gridDetails into dynamic object
                            var properties = result.GetType().GetProperties();
                            foreach (var prop in properties)
                            {
                                dict[prop.Name] = prop.GetValue(result);
                            }

                            dict["CurrencyImage"] = company.CurrencyImage;
                            dict["CurrencySymbole"] = company.CurrencySymbol;


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
        public async Task<ActionResult> InsertPurchaseCloneEInvoice(string InvoiceNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    // Fetch the invoice master details using the given InvoiceNo
                    var invoice = await dbContext.Tbl20166VatpurchaseMasters
                                                 .FirstOrDefaultAsync(i => i.PurchaseVoucherNo == InvoiceNo);

                    if (invoice == null)
                    {
                        return NotFound(new { Message = "Invoice not found." });
                    }

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

                    // Extract values from the fetched invoice
                    string ToInvoiceNo = newPurchaseVoucherNo; // You can generate or assign this as needed
                    DateTime InvoiceDate = invoice.PurchaseVoucherDate ?? DateTime.Now;
                    string AddedBy = invoice.AddedBy ?? "System"; // Fallback if null
                    DateTime AddedOn = invoice.AddedOn ?? DateTime.Now;


                    // Execute the stored procedure
                    var result = dbContext.Database.ExecuteSqlRaw(
                        "EXEC sp201_73InsertDuplicatePurchaseBill @p0,@p1,@p2,@p3,@p4",
                        InvoiceNo, ToInvoiceNo, InvoiceDate, AddedBy, AddedOn);

                    dbContext.SaveChanges();
                    await _userActionLogger.LogAsync(
             module: "VAT > Purchase Clone EInvoice",
             actionDetail: $"Saved voucher: {newPurchaseVoucherNo}",
             documentNo: newPurchaseVoucherNo
         );
                    return Ok(new
                    {
                        Message = "Purchase Invoice cloned successfully.",
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
        
        [HttpGet]
        public async Task<ActionResult> GetProfitandloss(string frmDate, string toDate)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }

            try
            {
                string[] acceptedFormats = { "MM/dd/yyyy", "dd-MM-yyyy", "yyyy-MM-dd" };

                if (!DateTime.TryParseExact(frmDate, acceptedFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime from))
                {
                    return BadRequest($"Invalid 'from' date format: '{frmDate}'. Supported formats: MM/dd/yyyy, dd-MM-yyyy, yyyy-MM-dd.");
                }

                if (!DateTime.TryParseExact(toDate, acceptedFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime to))
                {
                    return BadRequest($"Invalid 'to' date format: '{toDate}'. Supported formats: MM/dd/yyyy, dd-MM-yyyy, yyyy-MM-dd.");
                }

                // Optional: Ensure from date is not greater than to date
                if (from > to)
                {
                    return BadRequest("The 'from' date must be earlier than or equal to the 'to' date.");
                }

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
                        i.ClientNeighborhood,
                        i.ClientNeighborhoodAr,
                        i.ClientCountryCode,
                        i.AccountHeadArabic,
                        i.BillingName

                    });


                    return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
                }

            }
            catch (Exception ex) { throw ex; }
            return Unauthorized(new { message = "Invalid tenant.", success = false });

        }



        //[HttpGet]
        //public async Task<IActionResult> GetGoodsAndServices(int pageNumber = 1,int pageSize = 10)
        //{
        //    try
        //    {
        //        if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
        //        {
        //            var resultWithVAT = new List<ExpandoObject>();
        //            var result = await dbContext.Tbl20164GoodsAndServicesMasters
        //            .Select(g => new
        //            {
        //                g.Gscode,
        //                g.Gsdescrpition,
        //                g.GsgroupId,
        //                g.GsdescriptionAr,
        //                g.ItemPartNo,
        //                g.CostPrice,
        //                g.GssellingRate,
        //                g.ReorderQty,
        //                g.StoreCode,
        //                g.MaxQty,
        //                g.MinQty,
        //                g.GsuoM,
        //                g.GspackingUnit


        //            })
        //            .ToListAsync();



        //            foreach (var gridDetails in result)
        //            {
        //                dynamic item = new ExpandoObject();
        //                var dict = (IDictionary<string, object>)item;

        //                // Copy all existing fields from gridDetails into dynamic object
        //                var properties = gridDetails.GetType().GetProperties();
        //                foreach (var prop in properties)
        //                {
        //                    dict[prop.Name] = prop.GetValue(gridDetails);
        //                }

        //                var UnitRateMethodDesc = dbContext.Tbl40111PropertyUnitCodes
        //           .Where(x => x.UnitCode == gridDetails.GsgroupId)
        //           .Select(x => x.UnitDesc)
        //           .FirstOrDefault();


        //                // Add new dynamic column
        //                dict["UnitRateMethod"] = UnitRateMethodDesc;


        //                //dict["VAT"] = vatValue;
        //                //dict["TotalVAT"] = totalValue;

        //                resultWithVAT.Add(item);
        //            }

        //            return Ok(resultWithVAT);
        //        }
        //    }
        //    catch (Exception ex) { throw ex; }
        //    return Unauthorized(new { message = "Invalid tenant.", success = false });

        //}

        [HttpGet]
        public async Task<IActionResult> GetGoodsAndServices(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var query = from g in dbContext.Tbl20164GoodsAndServicesMasters
                                join u in dbContext.Tbl40111PropertyUnitCodes
                                    on g.GsgroupId equals u.UnitCode into gj
                                from unit in gj.DefaultIfEmpty()
                                orderby g.Gscode
                                select new
                                {
                                    GSCode = g.Gscode,
                                    GSDescrpition = g.Gsdescrpition,
                                    g.GsgroupId,
                                    GsdescriptionAr = g.GsdescriptionAr,
                                    g.ItemPartNo,
                                    g.CostPrice,
                                    GSSellingRate = g.GssellingRate,
                                    g.ReorderQty,
                                    g.StoreCode,
                                    g.MaxQty,
                                    g.MinQty,
                                    g.GsuoM,
                                    g.GspackingUnit,
                                    UnitDescription = unit.UnitDesc
                                };

                    // Get total count for pager
                    var totalCount = await query.CountAsync();

                    // Apply pagination
                    var pagedData = await query
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

                    return Ok(new
                    {
                        data = pagedData,
                        totalCount = totalCount
                    });
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", details = ex.Message });
            }
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
        public ActionResult<string> GetVATInvoiceNo()
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

                    string invoiceAbbrv = "";
                    if (company.IsUseEinvoiceAbbrv == true)
                    {
                        invoiceAbbrv = company.EinvoiceAbbrv;
                    }
                    else
                    {
                        invoiceAbbrv = "";
                    }
                    int invoiceYearDigits = company.InvoiceYearDigits ?? 0;
                    bool isResetInvoiceInYear = company.IsResetInvoiceInYear ?? false;
                    DateTime invoiceDate = DateTime.Now;
                    int NoOfDigitsInEInvoiceNo = company.NoOfDigitsInEinvoiceNo ?? 5; // 6 is the default if null



                    // Step 4: Generate New Debit Note No
                    string newInvoiceNo = GetVATInvoiceNoAPI(invoiceAbbrv, invoiceYearDigits, invoiceDate, isResetInvoiceInYear,NoOfDigitsInEInvoiceNo);

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

        public string GetVATInvoiceNoAPI(string invoiceAbbr, int yearInDigit, DateTime invoiceDate, bool isResetByYear,int NoOfDigitsInEInvoiceNo)
        {
            string strYear = "";
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    int maxNumber = 0;

                    var query = dbContext.Tbl20161VatinvoiceMasters.AsQueryable();

                    if (isResetByYear)
                    {
                        query = query.Where(d => d.InvoiceDate.HasValue && d.InvoiceDate.Value.Year == invoiceDate.Year);
                    }

                    maxNumber = query
     .Select(d => d.InvoiceNo)
     .Where(no => !string.IsNullOrEmpty(no) && no.Length >= NoOfDigitsInEInvoiceNo)
     .AsEnumerable() // Important to move to memory
     .Select(no => int.TryParse(no.Substring(no.Length - NoOfDigitsInEInvoiceNo), out int number) ? number : 0)
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
                    string newInvoiceNumber = $"{(string.IsNullOrWhiteSpace(invoiceAbbr) ? "" : invoiceAbbr)}{strYear}-{maxNumber.ToString().PadLeft(NoOfDigitsInEInvoiceNo, '0')}";

                    return newInvoiceNumber;
                }
                else
                {
                    throw new Exception("Tenant or DbContext not found");
                }
            }
            catch (Exception ex)
            {
                // Fallback Invoice No in case of any error
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

        //[HttpGet]
        //public async Task<ActionResult> GetVATInvoiceNoAPI(DataSourceLoadOptions loadOptions)

        //{
        //    try
        //    {
        //        if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
        //        {
        //            string yearSuffix = DateTime.Now.ToString("yyyy"); // Get last two digits of the year

        //            // Get invoice abbreviation
        //            //var invoiceAbbrv = await dbContext.Tbl901CompanyDetails
        //            //	.Select(c => c.EinvoiceAbbrv)
        //            //	.FirstOrDefaultAsync();

        //            //if (string.IsNullOrEmpty(invoiceAbbrv))
        //            //	return BadRequest("Invoice abbreviation not found.");

        //            // Get last invoice number
        //            //var lastInvoiceNumber = await dbContext.Tbl20161VatinvoiceMasters
        //            //	.Where(i => i.InvoiceNo.StartsWith($"{invoiceAbbrv}{yearSuffix}-"))
        //            //	.OrderByDescending(i => i.InvoiceNo)
        //            //	.Select(i => i.InvoiceNo)
        //            //	.FirstOrDefaultAsync();

        //            var lastInvoiceNumber = await dbContext.Tbl20161VatinvoiceMasters
        //                .Where(i => i.InvoiceNo.StartsWith($"{yearSuffix}-"))
        //                .OrderByDescending(i => i.InvoiceNo)
        //                .Select(i => i.InvoiceNo)
        //                .FirstOrDefaultAsync();

        //            int newNumber = 1; // Default if no previous invoices exist
        //            if (!string.IsNullOrEmpty(lastInvoiceNumber))
        //            {
        //                var match = Regex.Match(lastInvoiceNumber, @"-(\d+)$");
        //                if (match.Success)
        //                {
        //                    newNumber = int.Parse(match.Groups[1].Value) + 1;
        //                }
        //            }

        //            // Generate new invoice number
        //            //string newInvoiceNumber = $"{invoiceAbbrv}{yearSuffix}-{newNumber:D5}";
        //            string newInvoiceNumber = $"{yearSuffix}-{newNumber:D5}";

        //            //  return Ok(new { InvoiceNumber = newInvoiceNumber });
        //            return Json(newInvoiceNumber);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Internal server error: {ex.Message}");
        //    }


        //    return BadRequest("Failed to retrieve tenant and database context.");
        //}

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
                    await _userActionLogger.LogAsync(
            module: "VAT > Cost Allocation",
            actionDetail: $"Saved Cost Allocation: {CM.Gscode}",
            documentNo: CM.Gscode
                );
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
                    await _userActionLogger.LogAsync(
module: "VAT > Goods and Services",
actionDetail: $"Saved Goods and Services: {model.Gscode}",
documentNo: model.Gscode
);
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
        public IActionResult AddUom(string unitType, string unitDesc, string unitDescAr)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                // Check if the UnitType and UnitDesc already exists
                bool exists = dbContext.Tbl40111PropertyUnitCodes.Any(u =>
                    u.UnitType.Trim().ToLower() == unitType.Trim().ToLower() &&
                    u.UnitDesc.Trim().ToLower() == unitDesc.Trim().ToLower());
                  //  u.UnitDescAr.Trim().ToLower() == unitDescAr.Trim().ToLower());

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
                 _userActionLogger.LogAsync(
module: "VAT> UOM ",
actionDetail: $"Saved voucher: {unitType}",
documentNo: unitType
).Wait();
                return Json(new { success = true, unitCode = newUom.UnitCode });
            }

            return Json(new { success = false, message = "Unable to get tenant context" });
        }
        //Unitofmeasure
        [HttpPost]
        public IActionResult UpdateUom(byte unitCode, string unitType, string unitDesc, string unitDescAr)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(unitType) || string.IsNullOrWhiteSpace(unitDesc))
                {
                    return Json(new { success = false, message = "All fields are required." });
                }
                unitType = unitType.Trim();
                unitDesc = unitDesc.Trim();
                unitDescAr = unitDescAr?.Trim() ?? string.Empty;

                // Find existing record
                var existing = dbContext.Tbl40111PropertyUnitCodes.FirstOrDefault(u => u.UnitCode == unitCode);
                if (existing == null)
                {
                    return Json(new { success = false, message = "Unit not found." });
                }

                // Check for duplicate values (in other records)
                bool isDuplicate = dbContext.Tbl40111PropertyUnitCodes.Any(u =>
                    u.UnitCode != unitCode &&
                    u.UnitType.ToLower() == unitType.ToLower());
                    //u.UnitDesc.ToLower() == unitDesc.ToLower() &&
                    //u.UnitDescAr.ToLower() == unitDescAr.ToLower());

                if (isDuplicate)
                {
                    return Json(new { success = false, message = "Another unit with the same values already exists." });
                }

                // Apply update
                existing.UnitType = unitType;
                existing.UnitDesc = unitDesc;
                existing.UnitDescAr = unitDescAr;

                dbContext.SaveChanges();
                 _userActionLogger.LogAsync(
module: "VAT Update UOM> ",
actionDetail: $"Saved voucher: {unitType}",
documentNo: unitType
);
                return Json(new { success = true, message = "Unit updated successfully." });
            }

            return Json(new { success = false, message = "Unable to get tenant context." });
        }
        [HttpDelete]
        public IActionResult DeleteUOM(byte key)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var record = dbContext.Tbl40111PropertyUnitCodes.FirstOrDefault(x => x.UnitCode == key);
                    if (record == null)
                        return NotFound();

                    dbContext.Tbl40111PropertyUnitCodes.Remove(record);
                    dbContext.SaveChanges();
                    _userActionLogger.LogAsync(
                        module: "VAT UOM> ",
                        actionDetail: $"Deleted UOM: {record.UnitType}",
                        documentNo: record.UnitType
                    ).Wait();
                    return Ok();
                }

                return Unauthorized(new { success = false, message = "Invalid tenant" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Delete: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while Deleting the data.", error = ex.Message });

            }
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
                    string SignatoryId = Convert.ToString(model.SignatoryId);
                    await _userActionLogger.LogAsync(
    module: "VAT UOM> ",
    actionDetail: $"Save SignatoryID: {model.SignatoryId}",
    documentNo: SignatoryId
);
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
                    var UserId = HttpContext.Session.GetString("UserId");
                    var TenantName = HttpContext.Session.GetString("TenantName");

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
                    
                    var notifyRequest = new NotificationRequest
                        {
                             UserId = UserId, // or fetch from session/DB
                             VoucherName = InvoiceNo,
                             ActionType = "You have one Sales Invoice to approve",
                            TenantName = TenantName 
                    };

                await _fcmService.SendNotificationAsync(notifyRequest);

                    await _userActionLogger.LogAsync(
    module: "VAT> Sales Verify Voucher",
    actionDetail: $"Verify VoucherNo: {InvoiceNo}",
    documentNo: InvoiceNo
);
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
        public async Task<ActionResult> ApproveVoucher(string InvoiceNo, bool IsDirectApproval)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var UserName = HttpContext.Session.GetString("UserName");
                    var UserId = HttpContext.Session.GetString("UserId");
                    var TenantName = HttpContext.Session.GetString("TenantName");

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

                    if (IsDirectApproval == false)
                    {
                        voucher.IsVerified = true;
                        voucher.VerifiedOn = DateTime.Now;
                        voucher.VerifiedBy = UserName;

                    }

                    dbContext.SaveChanges();
                    await _userActionLogger.LogAsync(
  module: "VAT> Sales Approve Voucher",
  actionDetail: $"Approve VoucherNo: {InvoiceNo}",
  documentNo: InvoiceNo
);
                     var notifyRequest = new NotificationRequest
             {
                 UserId = UserId, // or fetch from session/DB
                 VoucherName = InvoiceNo,
                 ActionType = "You have one Sales Invoice to post",
                TenantName = TenantName 
        };

        await _fcmService.SendNotificationAsync(notifyRequest);


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
        public async Task<ActionResult> PostInvoice(string InvoiceNo, bool IsDirectApproval)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    bool IsDirect = false;
                    bool IsPosted = false;

                    var UserName = HttpContext.Session.GetString("UserName");
                    var UserId = HttpContext.Session.GetString("UserId");
                    var TenantName = HttpContext.Session.GetString("TenantName");

                    if (string.IsNullOrEmpty(InvoiceNo))
                    {
                        return BadRequest(new { Message = "Invoice number is required." });
                    }

                    var voucher = dbContext.Tbl20161VatinvoiceMasters.FirstOrDefault(v => v.InvoiceNo == InvoiceNo);

                    if (voucher == null)
                    {
                        return NotFound(new { Message = "Invoice not found." });
                    }



                    // 🔁 Call the stored procedure sp201_62InsertVATtoVoucher
                    //var result = dbContext.Database.ExecuteSqlRaw("EXEC sp201_62InsertVATtoVoucher @p0,@p1,@p2,@p3", InvoiceNo, JustAddedVoucherEntryNoSubLedger, JustAddedVoucherEntryNoCostAlloc, IsCashOrBankAccount);
                    if (voucher.IsPosted != true)
                    {
                        // Update the fields
                        voucher.IsPosted = true;
                        voucher.PostedOn = DateTime.Now;
                        voucher.PostedBy = UserName;

                        int JustAddedVoucherEntryNoSubLedger = 0;
                        int JustAddedVoucherEntryNoCostAlloc = 0;
                        bool IsCashOrBankAccount = false;

                        var result1 = dbContext.Database.ExecuteSqlRaw("EXEC sp201_62InsertVATtoVoucher_BHD @p0,@p1,@p2,@p3", InvoiceNo, JustAddedVoucherEntryNoSubLedger, JustAddedVoucherEntryNoCostAlloc, IsCashOrBankAccount);

                    }

                    dbContext.SaveChanges();
                    await _userActionLogger.LogAsync(
module: "VAT> Sales Post Invoice",
actionDetail: $"Post InvoiceNo: {InvoiceNo}",
documentNo: InvoiceNo
);
                    IsDirect = true;

                    if (IsDirectApproval == true)
                    {
                        IsDirect = false;
                    }


                    return Ok(new
                    {
                        Message = "Invoice posted successfully.",
                        VoucherVerifiedBy = UserName,
                        IsDirect = IsDirect,
                        IsPosted = voucher.IsPosted
                        // Example, replace with actual data if needed
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
                    var resultWithVAT = new List<ExpandoObject>();
                    // Get default company ID from session
                    string defaultCompanyString = HttpContext.Session.GetString("DefaultcompanyID") ?? "";
                    if (!byte.TryParse(defaultCompanyString, out byte companyId))
                        return BadRequest("Invalid company ID.");

                    // Parse dates
                    if (!DateTime.TryParseExact(frmDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime from))
                        return BadRequest("Invalid from date format. Use MM/dd/yyyy.");

                    if (!DateTime.TryParseExact(toDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime to))
                        return BadRequest("Invalid to date format. Use MM/dd/yyyy.");

                    // Get company details
                    var company = await dbContext.Tbl901CompanyDetails
                        .FirstOrDefaultAsync(c => c.CompanyId == companyId);

                    if (company == null)
                        return NotFound($"Company with ID {companyId} not found.");

                    // Fetch VAT credit note details
                    var vatInvoices = await dbContext.Qry201807vatcreditNoteRegisterMainViews
                        .FromSqlRaw("SELECT * FROM qry201_807VATCreditNoteRegisterMainView WHERE CreditNoteDate BETWEEN @p0 AND @p1", from, to)
                        .ToListAsync();

                    foreach (var result in vatInvoices)
                    {

                        dynamic item = new ExpandoObject();
                        var dict = (IDictionary<string, object>)item;

                        dict["CurrencyImage"] = company.CurrencyImage;
                        dict["CurrencySymbole"] = company.CurrencySymbol;


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
        public async Task<ActionResult> GetInvoiceDetails(string InvoiceNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var resultWithVAT = new List<ExpandoObject>();

                    var result = dbContext.Tbl20161VatinvoiceMasters
                      .Where(x => x.InvoiceNo == InvoiceNo)
                      .ToList();
                    foreach (var headerDetails in result)
                    {
                        dynamic item = new ExpandoObject();
                        var dict = (IDictionary<string, object>)item;

                        // Copy all existing fields from gridDetails into dynamic object
                        var properties = headerDetails.GetType().GetProperties();
                        foreach (var prop in properties)
                        {
                            dict[prop.Name] = prop.GetValue(headerDetails);
                        }

                      

                        var CurrencyName = dbContext.Tbl20169CurrencyExchanges
                   .Where(x => x.CurrencyExchangeId == Convert.ToInt16(headerDetails.InvoiceCurrencyCode))
                   .Select(x => x.CurrencyName)
                   .FirstOrDefault();


                        // Add new dynamic column
                        dict["CurrencyName"] = CurrencyName;
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


                        // Add new dynamic column
                        dict["UnitRateMethod"] = UnitRateMethodDesc;
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
                    var resultWithVAT = new List<ExpandoObject>();
                    // Get default company ID from session
                    string defaultCompanyString = HttpContext.Session.GetString("DefaultcompanyID") ?? "";
                    if (!byte.TryParse(defaultCompanyString, out byte companyId))
                        return BadRequest("Invalid company ID.");

                    // Parse dates
                    if (!DateTime.TryParseExact(frmDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime from))
                        return BadRequest("Invalid from date format. Use MM/dd/yyyy.");

                    if (!DateTime.TryParseExact(toDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime to))
                        return BadRequest("Invalid to date format. Use MM/dd/yyyy.");

                    // Get company details
                    var company = await dbContext.Tbl901CompanyDetails
                        .FirstOrDefaultAsync(c => c.CompanyId == companyId);

                    if (company == null)
                        return NotFound($"Company with ID {companyId} not found.");

                    // Fetch VAT credit note details
                    var vatInvoices = await dbContext.Qry201807vatcreditNoteRegisterMainViews
                        .FromSqlRaw("SELECT * FROM qry201_807VATCreditNoteRegisterMainView WHERE CreditNoteDate BETWEEN @p0 AND @p1", from, to)
                        .ToListAsync();

                    foreach (var result in vatInvoices)
                    {

                        dynamic item = new ExpandoObject();
                        var dict = (IDictionary<string, object>)item;

                        // Copy all existing fields from gridDetails into dynamic object
                        var properties = result.GetType().GetProperties();
                        foreach (var prop in properties)
                        {
                            dict[prop.Name] = prop.GetValue(result);
                        }

                        dict["CurrencyImage"] = company.CurrencyImage; 
                        dict["CurrencySymbole"] = company.CurrencySymbol; 


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
                      var UserId = HttpContext.Session.GetString("UserId");
            var TenantName = HttpContext.Session.GetString("TenantName");

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
                    await _userActionLogger.LogAsync(
module: "VAT> Credit Note Verify",
actionDetail: $"Credit Note Verify Number: {CreditNoteNo}",
documentNo: CreditNoteNo
);

                         var notifyRequest = new NotificationRequest
             {
                 UserId = UserId, // or fetch from session/DB
                 VoucherName = CreditNoteNo,
                 ActionType = "You have one Credit Note to approve",
                TenantName = TenantName 
            };

        await _fcmService.SendNotificationAsync(notifyRequest);

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
                      var UserId = HttpContext.Session.GetString("UserId");
            var TenantName = HttpContext.Session.GetString("TenantName");

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
                    await _userActionLogger.LogAsync(
module: "VAT> Credit Note Approve",
actionDetail: $"Credit Note Approve Number: {CreditNoteNo}",
documentNo: CreditNoteNo
);


             var notifyRequest = new NotificationRequest
             {
                 UserId = UserId, // or fetch from session/DB
                 VoucherName = CreditNoteNo,
                 ActionType = "You have one Credit Note to post",
                TenantName = TenantName 
            };

        await _fcmService.SendNotificationAsync(notifyRequest);


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
        public async Task<ActionResult> DebitApproveVoucher(string DebitNoteNo, bool IsDirectApproval)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var UserName = HttpContext.Session.GetString("UserName");
                    var UserId = HttpContext.Session.GetString("UserId");
                    var TenantName = HttpContext.Session.GetString("TenantName");

                    if (string.IsNullOrEmpty(DebitNoteNo))
                    {
                        return BadRequest(new { Message = "DebitNote number is required." });
                    }

                    var voucher = dbContext.Tbl20172VatdebitNoteMasters.FirstOrDefault(v => v.DebitNoteNo == DebitNoteNo);

                    if (voucher == null)
                    {
                        return NotFound(new { Message = "DebitNoteNo not found." });
                    }

                    // Update the fields
                    voucher.IsApproved = true;
                    voucher.ApprovedOn = DateTime.Now;
                    voucher.ApprovedBy = UserName;

                    if (IsDirectApproval == false)
                    {
                        voucher.IsVerified = true;
                        voucher.VerifiedOn = DateTime.Now;
                        voucher.VerifiedBy = UserName;

                    }

                    dbContext.SaveChanges();
                    await _userActionLogger.LogAsync(
module: "VAT> Debit Note Verify",
actionDetail: $"Debit Note Verify Number: {DebitNoteNo}",
documentNo: DebitNoteNo
);

                                 var notifyRequest = new NotificationRequest
             {
                 UserId = UserId, // or fetch from session/DB
                 VoucherName = DebitNoteNo,
                 ActionType = "You have one Debit Note to post",
                TenantName = TenantName 
            };

        await _fcmService.SendNotificationAsync(notifyRequest);

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
        public async Task<ActionResult> CreditPostInvoice(string CreditNoteNo, bool IsDirectApproval)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    bool IsDirect = false;

                    var UserName = HttpContext.Session.GetString("UserName");
                      var UserId = HttpContext.Session.GetString("UserId");
            var TenantName = HttpContext.Session.GetString("TenantName");

                    if (string.IsNullOrEmpty(CreditNoteNo))
                    {
                        return BadRequest(new { Message = "CreditNoteNo is required." });
                    }

                    var voucher = dbContext.Tbl20170VatcreditNoteMasters.FirstOrDefault(v => v.CreditNoteNo == CreditNoteNo);

                    if (voucher == null)
                    {
                        return NotFound(new { Message = "CreditNoteNo not found." });
                    }

                    if (voucher.IsPosted != true)
                    {
                        // Update the fields
                        voucher.IsPosted = true;
                        voucher.PostedOn = DateTime.Now;
                        voucher.PostedBy = UserName;

                        int JustAddedVoucherEntryNoSubLedger = 0;
                        int JustAddedVoucherEntryNoCostAlloc = 0;

                        // 🔁 Call the stored procedure sp201_62InsertVATtoVoucher
                        //var result = dbContext.Database.ExecuteSqlRaw("EXEC sp201_62InsertVATtoVoucher @p0,@p1,@p2,@p3", InvoiceNo, JustAddedVoucherEntryNoSubLedger, JustAddedVoucherEntryNoCostAlloc, IsCashOrBankAccount);

                        var result1 = dbContext.Database.ExecuteSqlRaw("EXEC sp201_82InsertVATCreditNotetoVoucher_BHD @p0,@p1,@p2", CreditNoteNo, JustAddedVoucherEntryNoSubLedger, JustAddedVoucherEntryNoCostAlloc);

                    }
                    dbContext.SaveChanges();
                    await _userActionLogger.LogAsync(
module: "VAT> Credit Note Post",
actionDetail: $"Debit Note Post Number: {CreditNoteNo}",
documentNo: CreditNoteNo
);
                    IsDirect = true;

                    if (IsDirectApproval == true)
                    {
                        IsDirect = false;
                    }


                    return Ok(new
                    {
                        Message = "Invoice posted successfully.",
                        VoucherVerifiedBy = UserName,
                        IsDirect = IsDirect,
                        IsPosted = voucher.IsPosted
                        // Example, replace with actual data if needed
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
                      //.Where(x => x.IsLedgerObselete == null || x.IsLedgerObselete == false)
                         //x => x.RecordStatus == "Record Complete" &&
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
                             i.SupplierCountryCode,
                             i.IsLedgerObselete
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
        public async Task<IActionResult> GetPurchaseCompanyBranch(DataSourceLoadOptions loadOptions)
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
        //[HttpPost]
        //public async Task<ActionResult> UpdateInvoiceChildDetails(List<InvoiceItem1> InvoiceChildren)
        //{
        //	if (InvoiceChildren == null || InvoiceChildren.Count == 0)
        //	{
        //		return BadRequest(new { success = false, message = "Invalid or empty invoice data received." });
        //	}

        //	try
        //	{
        //		if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
        //		{
        //			foreach (var child in InvoiceChildren)
        //			{
        //				if (child == null) continue;

        //				if (child.PurchaseChildSlNo == 0)
        //				{
        //					// Create new child
        //					var newChild = new Tbl20167VatpurchaseChild
        //					{
        //						PurchaseVoucherNo = child.PurchaseVoucherNo,
        //						UoM = child.UoM ?? "Each",
        //						QuantityInvoiced = child.QuantityInvoiced ?? 0m,
        //						UnitsToBill = child.UnitsToBill ?? 1m,
        //						UnitRateMethod = child.UnitRateMethod ?? (byte)49,
        //						UnitRate = child.UnitRate ?? 0m,
        //						LineAmount = child.LineAmount ?? 0m,
        //						DetailedDescription = child.DetailedDescription ?? string.Empty,
        //						ItemRemarks = child.ItemRemarks ?? string.Empty,
        //						DatesBilledFor = child.DatesBilledFor ?? string.Empty,
        //						DeliveryNoteNo = child.DeliveryNoteNo ?? string.Empty,
        //						DeliveryDate = child.DeliveryDate,
        //						TaxSlabCode = child.TaxSlabCode ?? (byte)8,
        //						TaxAmount = child.TaxAmount ?? 0m,
        //						ItemCode = child.ItemCode ?? string.Empty,
        //						IsExpenses = child.IsExpenses ?? false,
        //						ExpensesLedgerNo = child.ExpensesLedgerNo ?? string.Empty,
        //						Discount = child.Discount ?? 0m,
        //						ItemPurchaseType = child.ItemPurchaseType ?? string.Empty,
        //						IsGoodsInTransitAccount = child.IsGoodsInTransitAccount ?? false,
        //						GoodsInTransitAccountNo = child.GoodsInTransitAccountNo ?? string.Empty,
        //						GoodsInTransitPurchaseVoucherNo = child.GoodsInTransitPurchaseVoucherNo ?? string.Empty,
        //						LineOrderNo = child.LineOrderNo ?? 0m,
        //						EmployeeNo = child.EmployeeNo ?? string.Empty,
        //						PropertyNo = child.PropertyNo ?? string.Empty
        //					};

        //					await dbContext.Tbl20167VatpurchaseChildren.AddAsync(newChild);
        //				}
        //				else
        //				{
        //					// Update existing child
        //					var existingChild = await dbContext.Tbl20167VatpurchaseChildren
        //						.FirstOrDefaultAsync(x => x.PurchaseChildSlNo == child.PurchaseChildSlNo);

        //					if (existingChild != null)
        //					{
        //						existingChild.PurchaseVoucherNo = child.PurchaseVoucherNo;
        //						existingChild.UoM = child.UoM ?? "Each";
        //						existingChild.QuantityInvoiced = child.QuantityInvoiced ?? 0m;
        //						existingChild.UnitsToBill = child.UnitsToBill ?? 1m;
        //						existingChild.UnitRateMethod = child.UnitRateMethod ?? (byte)49;
        //						existingChild.UnitRate = child.UnitRate ?? 0m;
        //						existingChild.LineAmount = child.LineAmount ?? 0m;
        //						existingChild.DetailedDescription = child.DetailedDescription ?? string.Empty;
        //						existingChild.ItemRemarks = child.ItemRemarks ?? string.Empty;
        //						existingChild.DatesBilledFor = child.DatesBilledFor ?? string.Empty;
        //						existingChild.DeliveryNoteNo = child.DeliveryNoteNo ?? string.Empty;
        //						existingChild.DeliveryDate = child.DeliveryDate;
        //						existingChild.TaxSlabCode = child.TaxSlabCode ?? (byte)8;
        //						existingChild.TaxAmount = child.TaxAmount ?? 0m;
        //						existingChild.ItemCode = child.ItemCode ?? string.Empty;
        //						existingChild.IsExpenses = child.IsExpenses ?? false;
        //						existingChild.ExpensesLedgerNo = child.ExpensesLedgerNo ?? string.Empty;
        //						existingChild.Discount = child.Discount ?? 0m;
        //						existingChild.ItemPurchaseType = child.ItemPurchaseType ?? string.Empty;
        //						existingChild.IsGoodsInTransitAccount = child.IsGoodsInTransitAccount ?? false;
        //						existingChild.GoodsInTransitAccountNo = child.GoodsInTransitAccountNo ?? string.Empty;
        //						existingChild.GoodsInTransitPurchaseVoucherNo = child.GoodsInTransitPurchaseVoucherNo ?? string.Empty;
        //						existingChild.LineOrderNo = child.LineOrderNo ?? 0m;
        //						existingChild.EmployeeNo = child.EmployeeNo ?? string.Empty;
        //						existingChild.PropertyNo = child.PropertyNo ?? string.Empty;

        //						dbContext.Tbl20167VatpurchaseChildren.Update(existingChild);
        //					}
        //				}
        //			}

        //			await dbContext.SaveChangesAsync();

        //			return Ok(new { success = true, message = "Invoice child records updated successfully!" });
        //		}
        //	}
        //	catch (Exception ex)
        //	{
        //		return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
        //	}

        //	return BadRequest("Failed to retrieve tenant and database context.");
        //}

        //[HttpPost]
        //public async Task<ActionResult> UpdatePurchaseMasterDetails(Tbl20166VatpurchaseMaster InvoiceMaster)
        //{
        //	if (InvoiceMaster == null)
        //	{
        //		return BadRequest(new { success = false, message = "Invalid invoice data received." });
        //	}

        //	try
        //	{
        //		if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
        //		{
        //			var existingInvoice = await dbContext.Tbl20166VatpurchaseMasters
        //														 .FirstOrDefaultAsync(v => v.PurchaseVoucherNo == InvoiceMaster.PurchaseVoucherNo);

        //			if (existingInvoice != null)
        //			{
        //				// Update existing master record
        //				dbContext.Entry(existingInvoice).CurrentValues.SetValues(InvoiceMaster);
        //			}
        //			else
        //			{
        //				// Insert new invoice master record
        //				await dbContext.Tbl20166VatpurchaseMasters.AddAsync(InvoiceMaster);
        //			}


        //			await dbContext.SaveChangesAsync();
        //			// await transaction.CommitAsync();

        //			return Ok(new { success = true, message = existingInvoice != null ? "Invoice and child records updated successfully!" : "New invoice and child records added successfully!" });
        //		}
        //	}
        //	catch (Exception ex)
        //	{
        //		// await transaction.RollbackAsync();
        //		return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
        //	}


        //	return BadRequest("Failed to retrieve tenant and database context.");
        //}
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

                    string invoiceAbbrv = "";
                    if (company.IsUseEinvoiceAbbrv == true)
                    {
                        invoiceAbbrv = company.EinvoiceAbbrv;
                    }
                    else
                    {
                        invoiceAbbrv = "";
                    }
                    int invoiceYearDigits = company.InvoiceYearDigits ?? 0;
                    bool isResetInvoiceInYear = company.IsResetInvoiceInYear ?? false;
                    DateTime invoiceDate = DateTime.Now;
                    int NoOfDigitsInEInvoiceNo = company.NoOfDigitsInEinvoiceNo ?? 5; // 6 is the default if null



                    // Step 4: Generate New Debit Note No
                    string newInvoiceNo = GetVATInvoiceNoAPI(invoiceAbbrv, invoiceYearDigits, invoiceDate, isResetInvoiceInYear, NoOfDigitsInEInvoiceNo);

                    // Extract values from the fetched invoice
                    string ToInvoiceNo = newInvoiceNo; // You can generate or assign this as needed
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
                    await _userActionLogger.LogAsync(
module: "VAT> Sales Clone EInvoice",
actionDetail: $"Clone EInvoiceNo: {InvoiceNo}",
documentNo: InvoiceNo
);
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

                    if (invoice == null)
                    {
                        return NotFound(new { Message = "Invoice not found." });
                    }

                    // Extract values from the fetched invoice
                    string CreditNoteNo = newCreditNoteNumber;
                    DateTime InvoiceDate = invoice.InvoiceDate ?? DateTime.Now;
                    string AddedBy = invoice.AddedBy ?? "System";
                    DateTime AddedOn = invoice.AddedOn ?? DateTime.Now;
                    string CreditNoteUUID = invoice.InvoiceUuid ?? Guid.NewGuid().ToString();
                    long? InvoiceCounterValue = invoice.InvoiceCounterValue ?? 0;
                    var IsPosted = invoice.IsPosted;
                    var CreditInvoiceNo = "";
                    // Fetch the invoice master details using the given InvoiceNo
                    var creditNoteMaster = await dbContext.Tbl20170VatcreditNoteMasters
    .Where(i => i.InvoiceNo == InvoiceNo)
    .FirstOrDefaultAsync();

                    if (creditNoteMaster == null && IsPosted == true)
                    {
                        CreditInvoiceNo = null;
                        // Call the stored procedure (6 parameters only)
                        var result = dbContext.Database.ExecuteSqlRaw(
                            "EXEC sp201_66InsertCreditNoteFromInvoice @p0, @p1, @p2, @p3, @p4, @p5",
                            InvoiceNo, CreditNoteNo, AddedBy, AddedOn, CreditNoteUUID, InvoiceCounterValue
                        );
                    }
                    else
                    {
                        CreditInvoiceNo = creditNoteMaster.InvoiceNo;
                    }

                    dbContext.SaveChanges();
                    await _userActionLogger.LogAsync(
module: "VAT> Sales Amend EInvoice",
actionDetail: $"Sales Amend EInvoiceNo: {InvoiceNo}",
documentNo: InvoiceNo
);
                    return Ok(new
                    {
                        Message = "Invoice Amended successfully.",
                        VoucherVerifiedBy = User.Identity?.Name ?? "System",
                        IsPosted = IsPosted,
                        CreditInvoiceNo = CreditInvoiceNo,
                        CreditNoteNo = CreditNoteNo
                    });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { Message = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }


        //eInvoice DebitNote - InsertAmend EInvoice 
        public async Task<ActionResult> InsertAmendEInvoiceDebit(string InvoiceNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    string yearSuffix = DateTime.Now.ToString("yy"); // e.g., "25"
                    string debitNoteAbbrv = "DBN"; // Hardcoded abbreviation

                    // Get last credit note number
                    var lastDebitNoteNumber = await dbContext.Tbl20172VatdebitNoteMasters
                        .Where(cn => cn.DebitNoteNo.StartsWith($"{debitNoteAbbrv}-{yearSuffix}-"))
                        .OrderByDescending(cn => cn.DebitNoteNo)
                        .Select(cn => cn.DebitNoteNo)
                        .FirstOrDefaultAsync();

                    int newNumber = 1; // Default if no previous credit notes exist
                    if (!string.IsNullOrEmpty(lastDebitNoteNumber))
                    {
                        var match = Regex.Match(lastDebitNoteNumber, @"-(\d+)$");
                        if (match.Success)
                        {
                            newNumber = int.Parse(match.Groups[1].Value) + 1;
                        }
                    }

                    // Generate new Credit Note number
                    string newDebitNoteNumber = $"{debitNoteAbbrv}-{yearSuffix}-{newNumber:D5}";

                    // Fetch the invoice master details using the given InvoiceNo
                    var invoice = await dbContext.Tbl20166VatpurchaseMasters
                                                 .FirstOrDefaultAsync(i => i.PurchaseVoucherNo == InvoiceNo);

                    if (invoice == null)
                    {
                        return NotFound(new { Message = "Invoice not found." });
                    }

                    // Extract values from the fetched invoice
                    string DebitNoteNo = newDebitNoteNumber;
                    DateTime InvoiceDate = invoice.PurchaseVoucherDate ?? DateTime.Now;
                    string AddedBy = invoice.AddedBy ?? "System";
                    DateTime AddedOn = invoice.AddedOn ?? DateTime.Now;
                    //string DebitNoteUUID = invoice.InvoiceUuid ?? Guid.NewGuid().ToString();
                    //long? InvoiceCounterValue = invoice.InvoiceCounterValue ?? 0;
                    var IsPosted = invoice.IsPosted;
                    var DebitInvoiceNo = "";
                    // Fetch the invoice master details using the given InvoiceNo
                    var debitNoteMaster = await dbContext.Tbl20172VatdebitNoteMasters
    .Where(i => i.PurchaseVoucherNo == InvoiceNo)
    .FirstOrDefaultAsync();

                    if (debitNoteMaster == null && IsPosted == true)
                    {
                        DebitInvoiceNo = null;
                        // Call the stored procedure (6 parameters only)
                        var result = dbContext.Database.ExecuteSqlRaw(
                            "EXEC sp201_85InsertDebiteNoteFromPurchase @p0, @p1, @p2, @p3",
                            InvoiceNo, DebitNoteNo, AddedBy, AddedOn
                        );
                    }
                    else
                    {
                        DebitInvoiceNo = debitNoteMaster.PurchaseVoucherNo;
                    }

                    dbContext.SaveChanges();
                    await _userActionLogger.LogAsync(
module: "VAT> Purchase Amend EInvoice",
actionDetail: $"Purchase Amend EInvoiceNo: {InvoiceNo}",
documentNo: InvoiceNo
);

                    return Ok(new
                    {
                        Message = "Invoice Amended successfully.",
                        VoucherVerifiedBy = User.Identity?.Name ?? "System",
                        IsPosted = IsPosted,
                        DebitInvoiceNo = DebitInvoiceNo,
                        DebitNoteNo = DebitNoteNo
                    });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { Message = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }



        //[HttpPost]
        //public async Task<ActionResult> InsertAmendEInvoice(string InvoiceNo)
        //{
        //    if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
        //    {
        //        try
        //        {
        //            // Step 1: Validate if invoice exists
        //            var invoice = await dbContext.Tbl20161VatinvoiceMasters
        //                                         .FirstOrDefaultAsync(i => i.InvoiceNo == InvoiceNo);

        //            if (invoice == null)
        //                return NotFound(new { Message = "Invoice not found." });

        //            // Step 2: Check if invoice is posted
        //            if (!(invoice.IsPosted ?? false))
        //            {
        //                return BadRequest(new
        //                {
        //                    Message = "VAT Invoice is not posted. You must post the invoice before cancelling or amending it."
        //                });
        //            }

        //            // Step 3: Get credit note totals
        //            var creditNoteData = await dbContext.Qry201807vatcreditNoteRegisterMainViews
        //                .Where(cn => cn.InvoiceNo == InvoiceNo)
        //                .GroupBy(cn => cn.InvoiceNo)
        //                .Select(g => new
        //                {
        //                    CreditNoteAmount = g.Sum(x => x.TotalCreditNoteAmount ?? 0)
        //                }).FirstOrDefaultAsync();

        //            decimal creditNoteTotal = creditNoteData?.CreditNoteAmount ?? 0;
        //            decimal invoiceTotal = invoice.TotalInvoiceAmount ?? 0;

        //            // Step 4: Validation rules
        //            if (creditNoteTotal < 0)
        //                return BadRequest(new { Message = "Credit Note cannot be issued for this invoice. Please review the invoice again." });

        //            if (invoiceTotal == 0)
        //                return BadRequest(new { Message = "VAT Invoice already has no amount to pass a credit note. Please review the invoice total value." });

        //            if (invoiceTotal == creditNoteTotal || invoiceTotal < creditNoteTotal)
        //                return BadRequest(new { Message = "VAT Invoice has already been fully or over-issued with a credit note. Please review the invoice & credit note details." });

        //            // Step 5: Generate new credit note number
        //            string yearSuffix = DateTime.Now.ToString("yy"); // e.g., "25"
        //            string creditNoteAbbrv = "CRN";

        //            var lastCreditNoteNumber = await dbContext.Tbl20170VatcreditNoteMasters
        //                .Where(cn => cn.CreditNoteNo.StartsWith($"{creditNoteAbbrv}-{yearSuffix}-"))
        //                .OrderByDescending(cn => cn.CreditNoteNo)
        //                .Select(cn => cn.CreditNoteNo)
        //                .FirstOrDefaultAsync();

        //            int newNumber = 1;
        //            if (!string.IsNullOrEmpty(lastCreditNoteNumber))
        //            {
        //                var match = Regex.Match(lastCreditNoteNumber, @"-(\d+)$");
        //                if (match.Success)
        //                {
        //                    newNumber = int.Parse(match.Groups[1].Value) + 1;
        //                }
        //            }

        //            string newCreditNoteNumber = $"{creditNoteAbbrv}-{yearSuffix}-{newNumber:D5}";

        //            // Extract values from the fetched invoice
        //            string CreditNoteNo = newCreditNoteNumber;
        //            DateTime InvoiceDate = invoice.InvoiceDate ?? DateTime.Now;
        //            string AddedBy = invoice.AddedBy ?? "System";
        //            DateTime AddedOn = invoice.AddedOn ?? DateTime.Now;
        //            string CreditNoteUUID = invoice.InvoiceUuid ?? Guid.NewGuid().ToString();
        //            long? InvoiceCounterValue = invoice.InvoiceCounterValue;

        //            // Step 6: Execute stored procedure
        //            var result = dbContext.Database.ExecuteSqlRaw(
        //                "EXEC sp201_66InsertCreditNoteFromInvoice @p0,@p1,@p2,@p3,@p4,@p5,@p6",
        //                InvoiceNo, CreditNoteNo, AddedBy, AddedOn, CreditNoteUUID, InvoiceCounterValue);

        //            await dbContext.SaveChangesAsync();

        //            return Ok(new
        //            {
        //                Message = "Credit Note has been added to the database.",
        //                CreditNoteNo,
        //                VoucherVerifiedBy = User.Identity?.Name ?? "System"
        //            });
        //        }
        //        catch (Exception ex)
        //        {
        //            return BadRequest(new { Message = ex.Message });
        //        }
        //    }

        //    return Unauthorized(new { Message = "Invalid tenant.", success = false });
        //}


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

        [HttpGet]
        public async Task<ActionResult> GetVatDebitNoteDetails(string frmDate, string toDate)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var resultWithVAT = new List<ExpandoObject>();
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

                    if (!DateTime.TryParseExact(frmDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime from))
                        return BadRequest("Invalid from date format. Use MM/dd/yyyy.");

                    if (!DateTime.TryParseExact(toDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime to))
                        return BadRequest("Invalid to date format. Use MM/dd/yyyy.");

                    // Fetch records based on the date range
                    var vatInvoices = await dbContext.Qry201907vatdebitNoteRegisterMainViews
                        .FromSqlRaw("SELECT * FROM qry201_907VATDebitNoteRegisterMainView WHERE DebitNoteDate BETWEEN @p0 AND @p1", from, to)
                        .ToListAsync();

                    foreach (var result in vatInvoices)
                    {

                        dynamic item = new ExpandoObject();
                        var dict = (IDictionary<string, object>)item;

                        // Copy all existing fields from gridDetails into dynamic object
                        var properties = result.GetType().GetProperties();
                        foreach (var prop in properties)
                        {
                            dict[prop.Name] = prop.GetValue(result);
                        }

                        dict["CurrencyImage"] = company.CurrencyImage;
                        dict["CurrencySymbole"] = company.CurrencySymbol;


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
        public async Task<ActionResult> GetVatProformaDetails(string frmDate, string toDate)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var resultWithVAT = new List<ExpandoObject>();
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

                    if (!DateTime.TryParseExact(frmDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime from))
                        return BadRequest("Invalid from date format. Use MM/dd/yyyy.");

                    if (!DateTime.TryParseExact(toDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime to))
                        return BadRequest("Invalid to date format. Use MM/dd/yyyy.");

                    // Fetch records based on the date range
                    var vatInvoices = await dbContext.Qry201657proformaInvoiceRegisterMainViews
                        .FromSqlRaw("SELECT * FROM qry201_657ProformaInvoiceRegisterMainView WHERE ProformaInvoiceDate BETWEEN @p0 AND @p1", from, to)
                        .ToListAsync();

                    foreach (var result in vatInvoices)
                    {

                        dynamic item = new ExpandoObject();
                        var dict = (IDictionary<string, object>)item;

                        // Copy all existing fields from gridDetails into dynamic object
                        var properties = result.GetType().GetProperties();
                        foreach (var prop in properties)
                        {
                            dict[prop.Name] = prop.GetValue(result);
                        }

                        dict["CurrencyImage"] = company.CurrencyImage;
                        dict["CurrencySymbole"] = company.CurrencySymbol;


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
        public async Task<IActionResult> GetVATInvoiceReporting(DateTime? startDate, DateTime? endDate)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var query = dbContext.Qry201620salesReportings.AsQueryable();

                    if (startDate.HasValue && endDate.HasValue)
                    {
                        query = query.Where(x => x.InvoiceDate >= startDate.Value && x.InvoiceDate <= endDate.Value);
                    }
                    var result = await query.Select(item => new
                    {
                        item.InvoiceNo,
                        item.InvoiceDate,
                        item.InvoiceEffectiveDate,
                        item.AccountHead,
                        item.SalesPersonName,
                        item.CostAllocationUnit,
                        item.CostAllocationGroup,
                        item.CostAllocationMasterGroup,
                        item.CompanyBranch,
                        item.TotalInvoiceAmountBeforeDiscount,
                        item.TotalDiscount,
                        item.TotalInvoiceAmount,
                        item.CalcTaxAmount,
                        item.InvoiceStatus,
                        item.AdvanceAmount,
                        item.RetentionAmount,
                        item.InvMonth,
                        item.InvEffectiveMonth,
                        item.TotalRevenue,
                        item.MonthNumber,
                        item.InvoiceMonth,
                        item.InvoiceEffectiveMonth,
                        item.CreditNoteNo,
                        item.CreditNoteDate,
                        item.CreditNoteAmount,
                        item.CreditNoteTaxAmount,
                        item.TotalCreditNoteAmount,
                        item.TotalAmountCredited,
                        item.InvoiceBalance,
                        item.ReceivedMonth,
                        item.RevenueLedgerNo,
                        item.RevenueLedgerName,
                        item.Pono,
                        item.Podate,
                        item.PaymentTerms,
                        item.BranchName,
                        item.BranchCode,
                        item.AccountId,
                        item.InvoicePeriod,
                        item.CostCenterCode,
                        item.InvoiceStartDate,
                        item.InvoiceEndDate,
                        item.InvoiceDueDate,
                        item.SalesOrderNo,
                        item.TypeOfInvoice,
                        item.BuyerNeighborhood,
                        item.CostAllocationUnitId
                    }).ToListAsync();

                    return Ok(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetTrialBalances: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet]
        public async Task<IActionResult> GetVATInvoiceReportingInDetails(DateTime? startDate, DateTime? endDate)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var query = dbContext.Qry201620salesReportingInDetails.AsQueryable();

                    // Removed accountGroup check

                    if (startDate.HasValue)
                        query = query.Where(x => x.InvoiceDate >= startDate.Value);

                    if (endDate.HasValue)
                        query = query.Where(x => x.InvoiceDate <= endDate.Value);

                    var result = await query.Select(item => new
                    {
                        ClientName = item.ClientName,
                        InvoiceMonth = item.InvoiceMonth,
                        AmountAfterDiscount = item.AmountAfterDiscount,
                        VatAmount = item.VatAmount,
                        Total = item.Total
                    }).ToListAsync();

                    return Ok(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetVATInvoiceReportingInDetails: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet]
        public async Task<IActionResult> GetDebtorsCollection(DateTime? startDate, DateTime? endDate)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var query = dbContext.Qry201621debtorsWithCollectionReports.AsQueryable();

                    if (startDate.HasValue)
                        query = query.Where(x => x.VoucherDate >= startDate.Value);

                    if (endDate.HasValue)
                        query = query.Where(x => x.VoucherDate <= endDate.Value);

                    var result = await query.Select(item => new
                    {
                        AccountHead = item.AccountHead,
                        DrAmount = item.DrAmount,
                        CrAmount = item.CrAmount,
                        VoucherMonth = item.VoucherMonth,
                        ReferenceNo = item.ReferenceNo,
                        AccountNo = item.AccountNo,
                        Amount = item.Amount,
                        DrCr = item.DrCr,
                        VoucherAmountFormatted = item.VoucherAmountFormatted,
                        AccountGroup = item.AccountGroup,
                        MasterGroup = item.MasterGroup,
                        AccountHeadNo = item.AccountHeadNo,
                        EntryNarration = item.EntryNarration,
                        SysRemarks = item.SysRemarks,
                        VoucherDate = item.VoucherDate,
                        VoucherEffectiveDate = item.VoucherEffectiveDate,
                        EffectiveMonth = item.EffectiveMonth,
                        InvoiceSalesPersonCode = item.InvoiceSalesPersonCode,
                        InvoiceSalesPersonName = item.InvoiceSalesPersonName,
                        VoucherNo = item.VoucherNo
                    }).ToListAsync();

                    return Ok(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetDebtorsCollection: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetSupplierName()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var result = await dbContext.Qry201710vatsundryCreditorsAndCashAccs
                        .Select(g => new
                        {
                            g.AccountId,           // Value member for GridLookUpEdit
                            g.AccountHead,        // Display member
                            g.RecordStatus

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
        public async Task<IActionResult> GetPurchaseVoucher(DataSourceLoadOptions loadOptions, string supplierid)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var result = dbContext.Qry20179PurchaseBillsWithBalances
     .Where(p => p.SupplierCode == supplierid)
     .Select(p => new
     {
         p.PurchaseVoucherNo,
         p.PurchaseVoucherDate,
         p.SupplierCode,
         p.PurchaseBillNo,
         p.PurchaseBillDate,
         p.TotalPurchaseAmount,
         p.PayableAmount,
         p.Paid,
         p.Balance
     })
     .ToList();


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
        public async Task<IActionResult> GetItemsReturnedFromStore()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var result = await dbContext.Tbl60001storeMasters
                        .Select(g => new
                        {
                            g.StoreId,
                            g.StoreName
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
        public async Task<IActionResult> GetItemsPurchaseAccount()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var result = await dbContext.Qry201709vatexpensesLedgers
                        .Select(g => new
                        {
                            g.AccountId,
                            g.IncomeLedger
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
        public async Task<IActionResult> GetDebitNoteDivision()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var result = await dbContext.Tbl901CompanyDetails
                    .Select(g => new
                    {
                        g.CompanyId,
                        g.CompanyName

                    })
                    .ToListAsync();

                    return Ok(result);
                }
            }
            catch (Exception ex) { throw ex; }
            return Unauthorized(new { message = "Invalid tenant.", success = false });

        }


        //     public string GetNewDebitNoteNo(string invoiceAbbr, int yearInDigit, DateTime invoiceDate, bool isResetByYear)
        //     {
        //try
        //{
        //	if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
        //	{
        //		string query;
        //		if (isResetByYear)
        //		{
        //			query = $"SELECT MAX(CAST(RIGHT(DebitNoteNo, 6) AS INT)) FROM tbl20172VATDebitNoteMaster WHERE YEAR(DebitNoteDate) = '{invoiceDate.Year}'";
        //		}
        //		else
        //		{
        //			query = "SELECT MAX(CAST(RIGHT(DebitNoteNo, 6) AS INT)) FROM tbl20172VATDebitNoteMaster";
        //		}

        //		int int1 = 0;
        //		using (SqlCommand sqlCommand = new SqlCommand(query, dbContext)) // yourSqlConnection must be your open SqlConnection
        //		{
        //			object result = sqlCommand.ExecuteScalar();
        //			if (result != DBNull.Value && result != null)
        //			{
        //				int1 = Convert.ToInt32(result);
        //			}
        //		}

        //		int1 += 1;
        //		string strNewQuotationNo = "000000" + int1.ToString();
        //		strNewQuotationNo = strNewQuotationNo.Substring(strNewQuotationNo.Length - 6); // ensure only last 6 digits

        //		if (yearInDigit <= 0)
        //		{
        //			yearInDigit = 0;
        //		}

        //		string strYear = invoiceDate.Year.ToString();
        //		if (yearInDigit > 0)
        //		{
        //			strYear = strYear.Substring(strYear.Length - yearInDigit, yearInDigit);
        //		}
        //		else
        //		{
        //			strYear = ""; // no year part if YearInDigit is 0
        //		}

        //		strNewQuotationNo = $"DBN-{strYear}-{strNewQuotationNo}";

        //		return strNewQuotationNo;
        //	}
        //}
        //catch (Exception ex)
        //{
        //	string strYear = invoiceDate.Year.ToString();
        //	if (yearInDigit > 0)
        //	{
        //		strYear = strYear.Substring(strYear.Length - yearInDigit, yearInDigit);
        //	}
        //	else
        //	{
        //		strYear = "";
        //	}

        //	string strNewQuotationNo = $"DBN-{strYear}-000001";
        //	return strNewQuotationNo;
        //}
        //     }

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

                    string invoiceAbbrv = company.InvoiceAbbrv;
                    int invoiceYearDigits = company.InvoiceYearDigits ?? 0;

                    bool isResetInvoiceInYear = company.IsResetInvoiceInYear ?? false;

                    DateTime invoiceDate = DateTime.Now;



                    // Step 4: Generate New Debit Note No
                    string newDebitNoteNo = GetNewDebitNoteNo(invoiceAbbrv, invoiceYearDigits, invoiceDate, isResetInvoiceInYear, dbContext);

                    return Ok(newDebitNoteNo);
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


        private string GetNewDebitNoteNo(string invoiceAbbrv, int yearInDigit, DateTime invoiceDate, bool isResetByYear, ERPMasterWtDataContext dbContext)
        {
            try
            {
                int maxRunningNumber = 0;

                IEnumerable<string> debitNoteNumbers;

                if (isResetByYear)
                {
                    debitNoteNumbers = dbContext.Tbl20172VatdebitNoteMasters
                        .Where(d => d.DebitNoteDate.HasValue && d.DebitNoteDate.Value.Year == invoiceDate.Year)
                        .Select(d => d.DebitNoteNo)
                        .AsEnumerable(); // Force client-side evaluation
                }
                else
                {
                    debitNoteNumbers = dbContext.Tbl20172VatdebitNoteMasters
                        .Select(d => d.DebitNoteNo)
                        .AsEnumerable(); // Force client-side evaluation
                }

                maxRunningNumber = debitNoteNumbers
                    .Where(no => !string.IsNullOrEmpty(no) && no.Length >= 6)
                    .Select(no =>
                    {
                        bool parsed = int.TryParse(no.Substring(no.Length - 6), out int number);
                        return parsed ? number : 0;
                    })
                    .DefaultIfEmpty(0)
                    .Max();

                maxRunningNumber += 1;

                string strNewDebitNoteNo = "000000" + maxRunningNumber;
                strNewDebitNoteNo = strNewDebitNoteNo.Substring(strNewDebitNoteNo.Length - 6);

                string strYear = invoiceDate.Year.ToString();
                if (yearInDigit > 0)
                {
                    strYear = strYear.Substring(strYear.Length - yearInDigit, yearInDigit);
                }
                else
                {
                    strYear = "";
                }

                return $"DBN-{strYear}-{strNewDebitNoteNo}";
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

                return $"DBN-{strYear}-000001";
            }
        }

        [HttpGet]
        public ActionResult<string> GetVATProformaInvoiceNo()
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

                    string invoiceAbbrv = company.InvoiceAbbrv;
                    int invoiceYearDigits = company.InvoiceYearDigits ?? 0;

                    bool isResetInvoiceInYear = company.IsResetInvoiceInYear ?? false;

                    DateTime invoiceDate = DateTime.Now;



                    // Step 4: Generate New Debit Note No
                    string newDebitNoteNo = GetNewProformaInvoiceNo(invoiceAbbrv, invoiceYearDigits, invoiceDate, isResetInvoiceInYear);

                    return Ok(newDebitNoteNo);
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

        public string GetNewProformaInvoiceNo(string invoiceAbbr, int yearInDigit, DateTime invoiceDate, bool isResetByYear)
        {
            string strYear = "";
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    int maxNumber = 0;

                    var query = dbContext.Tbl20181ProformaInvoiceMasters.AsQueryable();

                    if (isResetByYear)
                    {
                        query = query.Where(d => d.ProformaInvoiceDate.HasValue && d.ProformaInvoiceDate.Value.Year == invoiceDate.Year);
                    }

                    maxNumber = query
     .Select(d => d.ProformaInvoiceNo)
     .Where(no => !string.IsNullOrEmpty(no) && no.Length >= 6)
     .AsEnumerable() // Important to move to memory
     .Select(no => int.TryParse(no.Substring(no.Length - 6), out int number) ? number : 0)
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
                    string newProformaInvoiceNo = $"{(string.IsNullOrWhiteSpace(invoiceAbbr) ? "PRO" : invoiceAbbr)}-{strYear}-{maxNumber.ToString().PadLeft(6, '0')}";
                    return newProformaInvoiceNo;
                }
                else
                {
                    throw new Exception("Tenant or DbContext not found");
                }
            }
            catch (Exception ex)
            {
                // Fallback Invoice No in case of any error
                strYear = invoiceDate.Year.ToString();
                if (yearInDigit > 0 && yearInDigit <= 4)
                {
                    strYear = strYear.Substring(strYear.Length - yearInDigit, yearInDigit);
                }
                else
                {
                    strYear = "";
                }

                return $"{(string.IsNullOrWhiteSpace(invoiceAbbr) ? "PRO" : invoiceAbbr)}-{strYear}-000001";
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProformaClientName()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var result = await dbContext.Qry10143SundryDebtors
                        .Select(g => new
                        {
                            g.AccountId,           // Value member for GridLookUpEdit
                            g.AccountHead,     // Display member
                            g.AccountHeadArabic


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
        public async Task<IActionResult> GetTaxCategory()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var result = await dbContext.Tbl20163VatTaxSlabs
                        .Select(g => new
                        {
                            g.TaxCategoryId,           // Value member for GridLookUpEdit
                            g.TaxCategory,
                            g.TaxCategoryAr

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
        public async Task<IActionResult> GetVATInvoiceSubType()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var result = await dbContext.Tbl00108InvoiceSubTypeCodes
                        .Select(g => new
                        {
                            g.InvoiceSubType,           // Value member for GridLookUpEdit
                            g.InvoiceSubTypeCode     // Display member

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
        [HttpGet("{invoiceNo}")]
        public async Task<ActionResult> GetInvoiceApprovalStatus(string invoiceNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {

                    // Query the invoice approval status from tbl20161VATInvoiceMaster
                    var invoice = await dbContext.Tbl20161VatinvoiceMasters
                        .Where(i => i.InvoiceNo == invoiceNo)
                        .FirstOrDefaultAsync();

                    if (invoice == null)
                    {
                        return Ok(new { isApproved = invoice.IsApproved ?? false });
                    }

                    // Return the approval status
                    return Ok(new { isApproved = invoice.IsApproved,isPosted = invoice.IsPosted });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error fetching approval status for invoice {invoiceNo}: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized("Unable to fetch tenant information.");
        }
        [HttpGet("{CreditNoteNo}")]
        public async Task<ActionResult> GetCreditNoteApprovalStatus(string CreditNoteNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var invoice = await dbContext.Tbl20170VatcreditNoteMasters
                        .Where(i => i.CreditNoteNo == CreditNoteNo)
                        .FirstOrDefaultAsync();

                    if (invoice == null)
                    {
                        return Ok(new { isApproved = false }); // Safe fallback
                    }

                    return Ok(new { isApproved = invoice.IsApproved ?? false });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error fetching approval status for CreditNoteNo {CreditNoteNo}: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized("Unable to fetch tenant information.");
        }
        [HttpGet("{DebitNoteNo}")]
        public async Task<ActionResult> GetDebitNoteApprovalStatus(string DebitNoteNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var invoice = await dbContext.Tbl20172VatdebitNoteMasters
                        .Where(i => i.DebitNoteNo == DebitNoteNo)
                        .FirstOrDefaultAsync();

                    if (invoice == null)
                    {
                        return Ok(new { isApproved = false }); // Safe fallback
                    }

                    return Ok(new { isApproved = invoice.IsApproved ?? false });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error fetching approval status for CreditNoteNo {DebitNoteNo}: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized("Unable to fetch tenant information.");
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tbl00109ReasonsForCreditNote>>> GetCreditNoteReasons()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var reasons = await dbContext.Tbl00109ReasonsForCreditNotes
                .Select(r => new Tbl00109ReasonsForCreditNote
                {
                    ReasonCode = r.ReasonCode,
                    ReasonForCreditNote = r.ReasonForCreditNote
                })
                .ToListAsync();

                    return Ok(reasons);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }
            return Unauthorized("Unable to fetch tenant information.");
        }

        [HttpGet]
        public IActionResult GetVATInvoiceTypes()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {

                    var invoiceTypes = dbContext.Tbl00101InvoiceTypeCodes
                        .Select(i => new
                        {
                            i.InvoiceTypeCode,
                            i.InvoiceTypeName
                        })
                        .ToList();

                    return Ok(invoiceTypes);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized("Unable to fetch tenant information.");
        }
        [HttpGet]
        public IActionResult IsOldVATInvoice(string invoiceNo)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var isOldInvoice = dbContext.Tbl20161VatinvoiceMasters
            .Any(x => x.InvoiceNo == invoiceNo && x.InvoiceUuid == "0");
                    // Adjusted condition

                    return new JsonResult(isOldInvoice); // Return true/false
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }

            return new JsonResult(new { success = false, message = "Invalid tenant." });
        }

        [HttpGet]
        public async Task<IActionResult> Getcountrycode()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var result = await dbContext.Tbl00107CountryCodes
                    .Select(g => new
                    {
                        g.CountryCodeNumeric,
                        g.CountryName,
                        g.CountryCodeAlpha2
                    })
                    .ToListAsync();

                    return Ok(result);
                }
            }
            catch (Exception ex) { throw ex; }
            return Unauthorized(new { message = "Invalid tenant.", success = false });

        }
        [HttpGet]
        public async Task<IActionResult> GetInvoiceNumbers()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var result = await dbContext.Tbl20170VatcreditNoteMasters
                    .Select(g => new
                    {
                        g.InvoiceNo,


                    })
                    .ToListAsync();

                    return Ok(result);
                }
            }
            catch (Exception ex) { throw ex; }
            return Unauthorized(new { message = "Invalid tenant.", success = false });

        }


        [HttpGet]
        public async Task<IActionResult> GetInvoiceSubTypes()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var result = await dbContext.Tbl00108InvoiceSubTypeCodes
                .Select(x => new
                {
                    x.InvoiceSubTypeCode,
                    x.InvoiceSubType
                })
                .ToListAsync();

                    return Ok(result);
                }
            }
            catch (Exception ex) { throw ex; }
            return Unauthorized(new { message = "Invalid tenant.", success = false });

        }

        [HttpPost]
        public async Task<ActionResult> PurchaseApproveVoucher(string InvoiceNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var UserName = HttpContext.Session.GetString("UserName");
                     var UserId = HttpContext.Session.GetString("UserId");
            var TenantName = HttpContext.Session.GetString("TenantName");

                    if (string.IsNullOrEmpty(InvoiceNo))
                    {
                        return BadRequest(new { Message = "InvoiceNo number is required." });
                    }

                    var voucher = dbContext.Tbl20166VatpurchaseMasters.FirstOrDefault(v => v.PurchaseVoucherNo == InvoiceNo);

                    if (voucher == null)
                    {
                        return NotFound(new { Message = "InvoiceNo not found." });
                    }

                    // Update the fields
                    voucher.IsApproved = true;
                    voucher.ApprovedOn = DateTime.Now;
                    voucher.ApprovedBy = UserName;

                    //if (IsDirectApproval == false)
                    //{
                    //    voucher.IsVerified = true;
                    //    voucher.VerifiedOn = DateTime.Now;
                    //    voucher.VerifiedBy = UserName;

                    //}

                    dbContext.SaveChanges();
                    await _userActionLogger.LogAsync(
module: "VAT> Purchase Approve Voucher",
actionDetail: $"Purchase Approve VoucherNo: {InvoiceNo}",
documentNo: InvoiceNo
);

                             var notifyRequest = new NotificationRequest
             {
                 UserId = UserId, // or fetch from session/DB
                 VoucherName = InvoiceNo,
                 ActionType = "You have one Purchase Invoice to post",
                TenantName = TenantName 
            };

        await _fcmService.SendNotificationAsync(notifyRequest);

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
        public async Task<ActionResult> PurchasePostInvoice(string InvoiceNo, bool IsDirectApproval)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    bool IsDirect = false;

                    var UserName = HttpContext.Session.GetString("UserName");
                     var UserId = HttpContext.Session.GetString("UserId");
            var TenantName = HttpContext.Session.GetString("TenantName");

                    if (string.IsNullOrEmpty(InvoiceNo))
                    {
                        return BadRequest(new { Message = "Invoice number is required." });
                    }

                    var voucher = dbContext.Tbl20166VatpurchaseMasters.FirstOrDefault(v => v.PurchaseVoucherNo == InvoiceNo);

                    if (voucher == null)
                    {
                        return NotFound(new { Message = "Invoice not found." });
                    }

                    if (voucher.IsPosted != true)
                    {

                        // Update the fields
                        voucher.IsPosted = true;
                        voucher.PostedOn = DateTime.Now;
                        voucher.PostedBy = UserName;

                        int JustAddedVoucherEntryNoSubLedger = 0;
                        int JustAddedVoucherEntryNoCostAlloc = 0;
                        bool IsCashOrBankAccount = false;
                        bool IsExpensesAccount = false;

                        // 🔁 Call the stored procedure sp201_62InsertVATtoVoucher
                        var result = dbContext.Database.ExecuteSqlRaw("EXEC sp201_72InsertVATPurchasetoVoucher_BHD @p0,@p1,@p2,@p3,@p4", InvoiceNo, JustAddedVoucherEntryNoSubLedger, JustAddedVoucherEntryNoCostAlloc, IsCashOrBankAccount, IsExpensesAccount);

                        //   var result1 = dbContext.Database.ExecuteSqlRaw("EXEC sp201_62InsertVATtoVoucher_BHD @p0,@p1,@p2,@p3", InvoiceNo, JustAddedVoucherEntryNoSubLedger, JustAddedVoucherEntryNoCostAlloc, IsCashOrBankAccount);
                    }

                    dbContext.SaveChanges();
                    await _userActionLogger.LogAsync(
module: "VAT> Purchase Post Invoice",
actionDetail: $"Purchase Post InvoiceNo: {InvoiceNo}",
documentNo: InvoiceNo
);
                    IsDirect = true;

                    if (IsDirectApproval == true)
                    {
                        IsDirect = false;
                    }

                    return Ok(new
                    {
                        Message = "Invoice posted successfully.",
                        VoucherVerifiedBy = UserName,
                        IsDirect = IsDirect,
                        IsPosted = voucher.IsPosted
                        // Example, replace with actual data if needed
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
        public async Task<ActionResult> DebitPostInvoice(string DebitNoteNo, bool IsDirectApproval)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    bool IsDirect = false;

                    var UserName = HttpContext.Session.GetString("UserName");
                    var UserId = HttpContext.Session.GetString("UserId");
            var TenantName = HttpContext.Session.GetString("TenantName");

                    if (string.IsNullOrEmpty(DebitNoteNo))
                    {
                        return BadRequest(new { Message = "Debit number is required." });
                    }

                    var voucher = dbContext.Tbl20172VatdebitNoteMasters.FirstOrDefault(v => v.DebitNoteNo == DebitNoteNo);

                    if (voucher == null)
                    {
                        return NotFound(new { Message = "DebitNoteNo not found." });
                    }

                    if (voucher.IsPosted != true)
                    {

                        // Update the fields
                        voucher.IsPosted = true;
                        voucher.PostedOn = DateTime.Now;
                        voucher.PostedBy = UserName;

                        int JustAddedVoucherEntryNoSubLedger = 0;
                        int JustAddedVoucherEntryNoCostAlloc = 0;
                        bool IsCashOrBankAccount = false;


                        // 🔁 Call the stored procedure sp201_62InsertVATtoVoucher
                        var result = dbContext.Database.ExecuteSqlRaw("EXEC sp201_92InsertVATDebitNotetoVoucher_BHD @p0,@p1,@p2,@p3", DebitNoteNo, JustAddedVoucherEntryNoSubLedger, JustAddedVoucherEntryNoCostAlloc, IsCashOrBankAccount);

                        //   var result1 = dbContext.Database.ExecuteSqlRaw("EXEC sp201_62InsertVATtoVoucher_BHD @p0,@p1,@p2,@p3", InvoiceNo, JustAddedVoucherEntryNoSubLedger, JustAddedVoucherEntryNoCostAlloc, IsCashOrBankAccount);
                    }

                    dbContext.SaveChanges();
                    await _userActionLogger.LogAsync(
module: "VAT> Debit Post Invoice",
actionDetail: $"Debit Post InvoiceNo: {DebitNoteNo}",
documentNo: DebitNoteNo
);
                    IsDirect = true;

                    if (IsDirectApproval == true)
                    {
                        IsDirect = false;
                    }


                    return Ok(new
                    {
                        Message = "Invoice posted successfully.",
                        VoucherVerifiedBy = UserName,
                        IsDirect = IsDirect,
                        IsPosted = voucher.IsPosted
                        // Example, replace with actual data if needed
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
        public async Task<ActionResult> GetPurchaseMasterInvoiceDetails(string InvoiceNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {

                    var result = dbContext.Tbl20166VatpurchaseMasters
                      .Where(x => x.PurchaseVoucherNo == InvoiceNo)
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
        public async Task<ActionResult> GetPurchaseChildInvoiceDetails(string InvoiceNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var resultWithVAT = new List<ExpandoObject>();

                    var result = dbContext.Qry201701vatpurchaseChildren
                      .Where(x => x.PurchaseVoucherNo == InvoiceNo)
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

                        // Get the TaxRateInWord from the TaxSlab table
                        var taxRateInWord = dbContext.Tbl20163VatTaxSlabs
                            .Where(x => x.TaxSlabCode == gridDetails.TaxSlabCode)
                            .Select(x => x.TaxRateInWord)
                            .FirstOrDefault();

                        var UnitRateMethodDesc = dbContext.Tbl40111PropertyUnitCodes
                   .Where(x => x.UnitCode == gridDetails.UnitRateMethod)
                   .Select(x => x.UnitDesc)
                   .FirstOrDefault();


                        // Add new dynamic column
                        dict["UnitRateMethod"] = UnitRateMethodDesc;
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
        public async Task<IActionResult> DeleteInvoiceAllLineItem(string InvoiceNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(InvoiceNo))
                    {
                        return BadRequest(new { Message = "Invoice number is required." });
                    }

                    // Check if invoice exists (optional)
                    var invoiceExists = await dbContext.Tbl20162VatinvoiceChildren
                                            .AnyAsync(v => v.InvoiceNo == InvoiceNo);

                    if (!invoiceExists)
                    {
                        return NotFound(new { Message = "Invoice not found." });
                    }

                    return Ok(new { Message = "Invoice child records deleted successfully." });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { Message = "Error while deleting invoice child records.", Error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpPost]
        public async Task<IActionResult> DeleteInvoiceChild([FromBody] int InvoiceChildSlNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {


                    // Call the stored procedure with SerialNumber
                    var result = await dbContext.Database.ExecuteSqlRawAsync(
                        "EXEC sp201_61DeleteVATInvoiceChild @p0", InvoiceChildSlNo);
                    string InvChildSlNo = InvoiceChildSlNo.ToString();
                    await _userActionLogger.LogAsync(
module: "VAT> Delete sales invoice child",
actionDetail: $"Sales invoice child number: {InvChildSlNo}",
documentNo: InvChildSlNo
);
                    return Ok(new { success = true, message = "Line item deleted successfully." });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { success = false, message = "Server error occurred.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }




        [HttpPost]
        public async Task<ActionResult> UpdateToPurchaseUnlocktheBil(string invoiceNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {

                    var records = dbContext.Tbl20166VatpurchaseMasters
                  .Where(x => x.PurchaseVoucherNo == invoiceNo)
                  .ToList();

                    foreach (var record in records)
                    {
                        record.IsApproved = false;
                        record.IsVerified = false;
                    }

                    dbContext.SaveChanges();
                    await _userActionLogger.LogAsync(
module: "VAT> Purchase To Unlock the Bill",
actionDetail: $"Purchase Bill Number: {invoiceNo}",
documentNo: invoiceNo
);

                    return Ok(new
                    {
                        Message = "Unlocak the bill successfully.",

                    });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public IActionResult CreditDetailDescription(string Description)
        {

            return PartialView("~/Areas/VAT/Pages/CreditDetailDescription.cshtml", Description); // Ensure this is inside /Views/VoucherEntryReceipts/
        }
        [HttpGet]
        public IActionResult VATSalesTaxExemption()
        {

            return PartialView("~/Areas/VAT/Pages/VATSalesTaxExemption.cshtml"); // Ensure this is inside /Views/VoucherEntryReceipts/
        }


        [HttpGet]
        public IActionResult VATPercentageCal(decimal amount)
        {
            // Use the amount value in your logic/view
            ViewBag.Amount = amount;
            return PartialView("~/Areas/VAT/Pages/VATPercentageCal.cshtml");
        }

        [HttpGet]
        public async Task<IActionResult> GetCurrencyImage()
        {
            // 1️⃣  Resolve the tenant‑scoped DbContext
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(
                    out Tenant _,
                    out ERPMasterWtDataContext dbContext))
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                                  "Tenant context is unavailable.");
            }
            try
            {

                // 2️⃣  Company short‑name comes from the session
                var companyNameShort = HttpContext.Session.GetString("TenantName");
                if (string.IsNullOrWhiteSpace(companyNameShort))
                    return BadRequest("Company name not found in session.");

                // 3️⃣  Fetch the SVG (single round‑trip, async)
                string? svg = await dbContext.Tbl901CompanyDetails
     .Where(c => c.CompanyNameShort.ToLower().Contains(companyNameShort.ToLower()))
     .Select(c => c.CurrencyImage)
     .FirstOrDefaultAsync();


                if (string.IsNullOrWhiteSpace(svg))
                    return NotFound();

                // 4️⃣  Serve it *as* SVG so <img src="…"> works
                return Content(svg, "image/svg+xml; charset=utf-8");
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        [HttpPost]
        public async Task<ActionResult> UpdateToDebitNoteUnlocktheBil(string invoiceNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {

                    var records = dbContext.Tbl20172VatdebitNoteMasters
                  .Where(x => x.DebitNoteNo == invoiceNo)
                  .ToList();

                    foreach (var record in records)
                    {
                        record.IsApproved = false;
                        record.IsVerified = false;
                    }

                    dbContext.SaveChanges();
                    await _userActionLogger.LogAsync(
module: "VAT> Debit Note To Unlock the Bill",
actionDetail: $"Debit Bill No: {invoiceNo}",
documentNo: invoiceNo
);

                    return Ok(new
                    {
                        Message = "Unlocak the bill successfully.",

                    });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public async Task<ActionResult> DeleteDebitNoteEInvoice(string InvoiceNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    // Fetch the invoice master details using the given InvoiceNo
                    var invoice = await dbContext.Tbl20172VatdebitNoteMasters
                                                 .FirstOrDefaultAsync(i => i.DebitNoteNo == InvoiceNo);

                    if (invoice == null)
                    {
                        return NotFound(new { Message = "Invoice not found." });
                    }

                    string invoiceAbbr = "PUR"; // Fixed abbreviation
                    DateTime invoiceDate = DateTime.Now;
                    string yearDigits = invoiceDate.ToString("yy");

                    // Format: PUR-YY-
                    string invoicePrefix = $"{invoiceAbbr}-{yearDigits}-";

                    // Get last voucher number matching current year
                    var lastInvoiceNumber = await dbContext.Tbl20172VatdebitNoteMasters
                        .Where(i => i.DebitNoteNo.StartsWith(invoicePrefix))
                        .OrderByDescending(i => i.DebitNoteNo)
                        .Select(i => i.DebitNoteNo)
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

                    // Extract values from the fetched invoice
                    string ToInvoiceNo = newPurchaseVoucherNo; // You can generate or assign this as needed
                    DateTime InvoiceDate = invoice.DebitNoteDate ?? DateTime.Now;
                    string AddedBy = invoice.AddedBy ?? "System"; // Fallback if null
                    DateTime AddedOn = invoice.AddedOn ?? DateTime.Now;


                    // Execute the stored procedure
                    var result = dbContext.Database.ExecuteSqlRaw(
                        "EXEC sp201_73InsertDuplicatePurchaseBill @p0,@p1,@p2,@p3,@p4",
                        InvoiceNo, ToInvoiceNo, InvoiceDate, AddedBy, AddedOn);

                    dbContext.SaveChanges();
                    await _userActionLogger.LogAsync(
module: "VAT> Delete Debit Note Einvoice",
actionDetail: $"Debit EInvoice No: {InvoiceNo}",
documentNo: InvoiceNo
);
                    return Ok(new
                    {
                        Message = "Purchase Invoice cloned successfully.",
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
        public async Task<IActionResult> GetCompanyBranchAndDivision()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {

                    var result = await dbContext.Tbl901CompanyDetails
                        .Select(g => new
                        {
                            g.CompanyName,           // Value member for GridLookUpEdit
                            g.CompanyId     // Display member

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
        public async Task<IActionResult> GetSalesPersonMaster()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var result = await dbContext.Tbl20101SalesPersonMasters
                        .Select(g => new
                        {
                            g.SalesPersonName,        // Value member for GridLookUpEdit
                            g.SalesPersonCode     // Display member

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
        public async Task<ActionResult> GetProformaHeaderInvoiceDetails(string InvoiceNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {

                    var result = dbContext.Tbl20181ProformaInvoiceMasters
                      .Where(x => x.ProformaInvoiceNo == InvoiceNo)
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
        public async Task<ActionResult> GetGridProformaInvoiceDetails(string InvoiceNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
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

                    var resultWithVAT = new List<ExpandoObject>();

                    var result1 = dbContext.Qry201651proformaInvoiceChildren
                        .Where(x => x.ProformaInvoiceNo == InvoiceNo)
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


                        // Add new dynamic column
                        dict["UnitRateMethod"] = UnitRateMethodDesc;
                        dict["VATPercentage"] = taxRateInWord;

                        item.CurrencyImage = company.CurrencyImage; // If you are overwriting with converted amount
                        item.CurrencySymbole = company.CurrencySymbol;

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
        public async Task<IActionResult> DeleteProformaInvoiceChild([FromBody] int InvoiceChildSlNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {


                    // Call the stored procedure with SerialNumber
                    var result = await dbContext.Database.ExecuteSqlRawAsync(
                        "EXEC sp201_61DeleteProformaInvoiceChild @p0", InvoiceChildSlNo);

                    return Ok(new { success = true, message = "Line item deleted successfully." });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { success = false, message = "Server error occurred.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        //[HttpPost]
        //public async Task<IActionResult> CreateVATInvoice(Tbl20181ProformaInvoiceMaster aTbl20181ProformaInvoiceMasters)
        //{
        //    if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
        //    {
        //        try
        //        {

        //            string yearSuffix = DateTime.Now.ToString("yyyy"); // Get last two digits of the year

        //            var lastInvoiceNumber = await dbContext.Tbl20161VatinvoiceMasters
        //                .Where(i => i.InvoiceNo.StartsWith($"{yearSuffix}-"))
        //                .OrderByDescending(i => i.InvoiceNo)
        //                .Select(i => i.InvoiceNo)
        //                .FirstOrDefaultAsync();

        //            int newNumber = 1; // Default if no previous invoices exist
        //            if (!string.IsNullOrEmpty(lastInvoiceNumber))
        //            {
        //                var match = Regex.Match(lastInvoiceNumber, @"-(\d+)$");
        //                if (match.Success)
        //                {
        //                    newNumber = int.Parse(match.Groups[1].Value) + 1;
        //                }
        //            }

        //            // Generate new invoice number
        //            //string newInvoiceNumber = $"{invoiceAbbrv}{yearSuffix}-{newNumber:D5}";
        //            string ToInvoiceNo = $"{yearSuffix}-{newNumber:D5}";
        //            var AddedBy = HttpContext.Session.GetString("UserName");
        //            string InvoiceUUID = Guid.NewGuid().ToString();
        //            long? InvoiceCounterValue = 0;
        //            string InvoiceTransactionCode = "0100000";
        //            DateTime AddedOn = DateTime.Now;
        //            // Call the stored procedure with SerialNumber
        //            var result = await dbContext.Database.ExecuteSqlRawAsync(
        //                "EXEC sp201_65InsertInvoiceFromProforma @p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7", aTbl20181ProformaInvoiceMasters.ProformaInvoiceNo, ToInvoiceNo, aTbl20181ProformaInvoiceMasters.ProformaInvoiceDate, AddedBy, AddedOn, InvoiceUUID, InvoiceCounterValue, InvoiceTransactionCode);

        //            return Ok(new { success = true, message = "Line item deleted successfully." });
        //        }
        //        catch (Exception ex)
        //        {
        //            return StatusCode(500, new { success = false, message = "Server error occurred.", error = ex.Message });
        //        }
        //    }

        //    return Unauthorized(new { message = "Invalid tenant.", success = false });
        //}


        [HttpPost]
        public async Task<IActionResult> CreateVATInvoice([FromBody] JsonElement json)
        {
            string proformaInvoiceNo = null;
            DateTime? proformaInvoiceDate = null;

            try
            {
                if (json.TryGetProperty("ProformaInvoiceNo", out var pino))
                    proformaInvoiceNo = pino.GetString();

                if (json.TryGetProperty("ProformaInvoiceDate", out var pidate))
                    proformaInvoiceDate = pidate.ValueKind == JsonValueKind.String
                        ? DateTime.Parse(pidate.GetString())
                        : pidate.GetDateTime();
            }
            catch
            {
                return BadRequest(new { success = false, message = "Invalid input format." });
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    //string yearSuffix = DateTime.Now.ToString("yyyy");

                    //var lastInvoiceNumber = await dbContext.Tbl20161VatinvoiceMasters
                    //    .Where(i => i.InvoiceNo.StartsWith($"{yearSuffix}-"))
                    //    .OrderByDescending(i => i.InvoiceNo)
                    //    .Select(i => i.InvoiceNo)
                    //    .FirstOrDefaultAsync();

                    //int newNumber = 1;
                    //if (!string.IsNullOrEmpty(lastInvoiceNumber))
                    //{
                    //    var match = Regex.Match(lastInvoiceNumber, @"-(\d+)$");
                    //    if (match.Success)
                    //        newNumber = int.Parse(match.Groups[1].Value) + 1;
                    //}

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
                    string invoiceAbbrv = "";
                    if (company.IsUseEinvoiceAbbrv==true)
                    {
                        invoiceAbbrv = company.EinvoiceAbbrv;
                    }
                    else
                    {
                        invoiceAbbrv = "";
                    }
                    int invoiceYearDigits = company.InvoiceYearDigits ?? 0;
                    bool isResetInvoiceInYear = company.IsResetInvoiceInYear ?? false;
                    DateTime invoiceDate = DateTime.Now;
                    int NoOfDigitsInEInvoiceNo = company.NoOfDigitsInEinvoiceNo ?? 5; // 6 is the default if null



                    // Step 4: Generate New Debit Note No
                    string newInvoiceNo = GetVATInvoiceNoAPI(invoiceAbbrv, invoiceYearDigits, invoiceDate, isResetInvoiceInYear, NoOfDigitsInEInvoiceNo);

                    string toInvoiceNo = newInvoiceNo;
                    string addedBy = HttpContext.Session.GetString("UserName");
                    string invoiceUUID = Guid.NewGuid().ToString();
                    DateTime addedOn = DateTime.Now;
                    long? invoiceCounterValue = 0;
                    string invoiceTransactionCode = "0100000";

                    await dbContext.Database.ExecuteSqlRawAsync(
                        "EXEC sp201_65InsertInvoiceFromProforma @p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7",
                        proformaInvoiceNo, toInvoiceNo, proformaInvoiceDate, addedBy, addedOn, invoiceUUID, invoiceCounterValue, invoiceTransactionCode
                    );

                    return Ok(new { success = true, toInvoiceNo = toInvoiceNo });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { success = false, message = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }



        public static string GetTLVBase64(string sellerName, string vatNumber, DateTime? timeStamp)
        {
            using (var ms = new MemoryStream())
            {
                void AddTLV(int tag, string value)
                {
                    byte[] valueBytes = Encoding.UTF8.GetBytes(value);
                    ms.WriteByte((byte)tag);
                    ms.WriteByte((byte)valueBytes.Length);
                    ms.Write(valueBytes, 0, valueBytes.Length);
                }

                AddTLV(1, sellerName ?? "");
                AddTLV(2, vatNumber ?? "");
                AddTLV(3, timeStamp.HasValue
                    ? timeStamp.Value.ToString("yyyy-MM-ddTHH:mm:ssZ")
                    : "");

                return Convert.ToBase64String(ms.ToArray());
            }
        }


        public static string GenerateInvoiceXml(Tbl20161VatinvoiceMaster InvoiceMaster, List<InvoiceItem> children)
        {
            var xml = new XElement("Invoice",
                new XElement("InvoiceNo", InvoiceMaster.InvoiceNo),
                new XElement("InvoiceDate", InvoiceMaster.InvoiceDate,
                new XElement("SellerName", InvoiceMaster.SellerName),
                new XElement("SellerVAT", InvoiceMaster.SellerVatnumber),
                new XElement("BuyerName", InvoiceMaster.BuyerName),
                new XElement("BuyerVAT", InvoiceMaster.BuyerVatnumber)
            //new XElement("Total", children.Total.ToString("F2")),
            //new XElement("VATAmount", InvoiceMaster.VATAmount.ToString("F2")),
            //new XElement("InvoiceLines",
            //    children.Select(c =>
            //        new XElement("Item",
            //            new XElement("Name", c.ItemName),
            //            new XElement("LineTotal", c.LineTotal.ToString("F2")),
            //            new XElement("TaxAmount", c.TaxAmount.ToString("F2"))
            //        )
            //    )
            //)
            ));

            return xml.ToString();
        }

        public static string GenerateQrCodeBase64(string tlvBase64)
        {
            var qrGenerator = new QRCoder.QRCodeGenerator(); // No using here

            using QRCodeData qrCodeData = qrGenerator.CreateQrCode(tlvBase64, QRCoder.QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrBytes = qrCode.GetGraphic(20);
            return Convert.ToBase64String(qrBytes);
        }

        [HttpPost]
        public IActionResult GenerateZatcaInvoiceXml(Tbl20161VatinvoiceMaster InvoiceMaster)
        {
            // Sample mock child data (replace with actual)
            var children = new List<InvoiceItem>
        {
            new InvoiceItem { InvoiceNo = InvoiceMaster.InvoiceNo, Description = "", Total = 100, TaxAmount = 15 }
        };

            // 1. Generate XML string
            string xml = GenerateInvoiceXml(InvoiceMaster, children);

            // 2. Create TLV QR base64
            string tlvBase64 = GetTLVBase64(
                InvoiceMaster.SellerName,
                InvoiceMaster.SellerVatnumber,
                InvoiceMaster.InvoiceDate
            );

            // 3. Generate QR image
            string qrCodeImageBase64 = GenerateQrCodeBase64(tlvBase64);

            return Ok(new
            {
                InvoiceXml = xml,
                TLVBase64 = tlvBase64,
                QRBase64 = qrCodeImageBase64
            });
        }


        [HttpGet]
        public async Task<IActionResult> GetInvoiceSubType(DataSourceLoadOptions loadOptions)
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
        public async Task<ActionResult> GetDebitNoteDetails(string DebitNoteNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {

                    var result = dbContext.Tbl20172VatdebitNoteMasters
                      .Where(x => x.DebitNoteNo == DebitNoteNo)
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
        public async Task<ActionResult> GetGridDebitNoteDetails(string DebitNoteNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
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

                    var resultWithVAT = new List<ExpandoObject>();

                    var result1 = dbContext.Qry201901vatdebitNoteChildren
                        .Where(x => x.DebitNoteNo == DebitNoteNo)
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

                        gridDetails.CurrencyImage = company.CurrencyImage; // If you are overwriting with converted amount
                        gridDetails.CurrencySymbole = company.CurrencySymbol;

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
        public async Task<IActionResult> GetPettyCashAccount(string voucherGroupId = "A012")
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    string userIdString = HttpContext.Session.GetString("UserId");
                    var pettyCashAccount = "";
                    if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out int userId))
                    {
                        // Step 1: Try to get Petty Cash Account from user
                         pettyCashAccount = await dbContext.TblUserMasters
                            .Where(u => u.UserId == userId)
                            .Select(u => u.PettyCashAccount)
                            .FirstOrDefaultAsync();

                        // Use pettyCashAccount as needed...
                    }

                    if (string.IsNullOrEmpty(pettyCashAccount))
                    {
                        // Step 2: Get default cash account
                        var defaultAccount = await dbContext.Tbl201ChartOfAccounts
                            .Where(c => c.AccountGroupId == voucherGroupId && (c.IsDefaultForCash ?? false))
                            .MaxAsync(c => c.AccountId);

                        return Ok(new
                        {
                            Status = "Default",
                            AccountID = defaultAccount
                        });
                    }
                    else
                    {
                        return Ok(new
                        {
                            Status = "UserSpecific",
                            AccountID = pettyCashAccount
                        });
                    }
                }
                else
                {
                    return BadRequest(new { Message = "Invalid tenant or DB context not found." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while processing your request.", Details = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetRevenueDefaultEntry(string voucherGroupId)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                voucherGroupId = "A015";
                var accountId = dbContext.Tbl201ChartOfAccounts
                    .Where(a => a.AccountGroupId == voucherGroupId && (a.IsDefaultForCash ?? false))
                    .Max(a => a.AccountId);

             
                return Ok(new
                {
                    Status = "Default",
                    AccountID = accountId
                });
            }

            return BadRequest("Invalid tenant context");
        }


        [HttpGet]
        public async Task<IActionResult> GetDefalutCompanyBranch()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    string defaultCompanyString = HttpContext.Session.GetString("DefaultcompanyID");
                    byte defaultCompanyByte = 0;

                    if (!string.IsNullOrWhiteSpace(defaultCompanyString))
                    {
                        // Safely try to parse the value
                        byte.TryParse(defaultCompanyString, out defaultCompanyByte);
                    }

                    byte companyId = defaultCompanyByte;

                    var result = await dbContext.Tbl901CompanyDetails
                        .Where(g => g.CompanyId == companyId)
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

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                // Logging the error is better than rethrowing directly
                _logger.LogError(ex, "Error occurred in GetCompanyBranch");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving company branch.", success = false });
            }
        }


        [HttpGet]
        public IActionResult GetPurchaseVatTaxSlabs()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var vatTaxSlabs = dbContext.Tbl20168VatpurchaseTaxSlabs
          .Where(x => x.PurchaseTaxInWords != "10%")
          .Select(x => new
          {
              x.PurchaseTaxSlabCode,
              x.PurchaseTaxSlab,
              x.PurchaseTaxRate,
              x.PurchaseTaxInWords
          })
          .ToList();
                    return Ok(vatTaxSlabs);
                }
                else
                {
                    return Unauthorized(new { message = "Invalid tenant.", success = false });
                }
            }
            catch (Exception ex)
            {
                // Log the exception if possible
                return StatusCode(500, new { message = ex.Message, success = false });
            }
        }

        [HttpPost]
        public IActionResult CheckIfPosted(string voucherId)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var voucher = dbContext.Tbl20166VatpurchaseMasters.FirstOrDefault(v => v.PurchaseVoucherNo == voucherId);

                    if (voucher == null)
                    {
                        return Json(new { success = false, message = "Voucher not found." });
                    }

                    if (voucher.IsPosted != true)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "This VAT Purchase Bill is not yet posted to your ledgers. Please post the voucher and try printing the voucher."
                        });
                    }

                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Tenant context not available." });
                }
            }
            catch (Exception ex)
            {
                // Log the exception if you have logging set up
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        public async Task<ActionResult> UpdateToProformaUnlocktheBil(string invoiceNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {

                    var records = dbContext.Tbl20181ProformaInvoiceMasters
                  .Where(x => x.ProformaInvoiceNo == invoiceNo)
                  .ToList();

                    foreach (var record in records)
                    {
                        record.IsApproved = false;
                        record.IsVerified = false;
                    }

                    dbContext.SaveChanges();
                    await _userActionLogger.LogAsync(
module: "VAT> Proforma To Unlock the Bill",
actionDetail: $"Proforma Bill No: {invoiceNo}",
documentNo: invoiceNo
);

                    return Ok(new
                    {
                        Message = "Unlocak the bill successfully.",

                    });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public async Task<ActionResult> InsertProformaCloneEInvoice(string InvoiceNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    // Fetch the invoice master details using the given InvoiceNo
                    var invoice = await dbContext.Tbl20181ProformaInvoiceMasters
                                                 .FirstOrDefaultAsync(i => i.ProformaInvoiceNo == InvoiceNo);

                    if (invoice == null)
                    {
                        return NotFound(new { Message = "Invoice not found." });
                    }

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

                    string invoiceAbbrv = company.InvoiceAbbrv;
                    int invoiceYearDigits = company.InvoiceYearDigits ?? 0;

                    bool isResetInvoiceInYear = company.IsResetInvoiceInYear ?? false;

                    DateTime invoiceDate = DateTime.Now;



                    // Step 4: Generate New Debit Note No
                    string newProformaInvoiceNo = GetNewProformaInvoiceNo(invoiceAbbrv, invoiceYearDigits, invoiceDate, isResetInvoiceInYear);

                    // Extract values from the fetched invoice
                    string ToInvoiceNo = newProformaInvoiceNo; // You can generate or assign this as needed
                    DateTime InvoiceDate = invoice.ProformaInvoiceDate ?? DateTime.Now;
                    string AddedBy = invoice.AddedBy ?? "System"; // Fallback if null
                    DateTime AddedOn = invoice.AddedOn ?? DateTime.Now;


                    // Execute the stored procedure
                    var result = dbContext.Database.ExecuteSqlRaw(
                        "EXEC sp201_63InsertDuplicateProformaInvoice @p0,@p1,@p2,@p3,@p4",
                        InvoiceNo, ToInvoiceNo, InvoiceDate, AddedBy, AddedOn);

                    dbContext.SaveChanges();
                    await _userActionLogger.LogAsync(
module: "VAT> Proforma Clone EInvoice",
actionDetail: $"Proforma Clone EInvoiceNo: {InvoiceNo}",
documentNo: InvoiceNo
);
                    return Ok(new
                    {
                        Message = "Purchase Invoice cloned successfully.",
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
        public IActionResult DeleteProformaInvoiceMasterView(string proformaInvoiceNo)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return StatusCode(500, "Unable to retrieve database context for the current tenant.");
            }

            if (string.IsNullOrWhiteSpace(proformaInvoiceNo))
            {
                return BadRequest("Invoice number is required.");
            }

            try
            {
                var masterRecord = dbContext.Tbl20181ProformaInvoiceMasters
                    .FirstOrDefault(x => x.ProformaInvoiceNo == proformaInvoiceNo);

                if (masterRecord == null)
                {
                    return NotFound("Proforma invoice not found.");
                }

                // Get related child records
                var childRecords = dbContext.Tbl20182ProformaInvoiceChildren
                    .Where(x => x.ProformaInvoiceNo == proformaInvoiceNo)
                    .ToList();

                // Remove child records
                if (childRecords.Any())
                {
                    dbContext.Tbl20182ProformaInvoiceChildren.RemoveRange(childRecords);
                }

                // Remove master record
                dbContext.Tbl20181ProformaInvoiceMasters.Remove(masterRecord);

                dbContext.SaveChanges();
                 _userActionLogger.LogAsync(
module: "VAT> Delete ProformaInvoice Master View",
actionDetail: $"Proforma Invoice No: {proformaInvoiceNo}",
documentNo: proformaInvoiceNo
).Wait();

                return Ok(new { Message = "Proforma invoice deleted successfully." });
            }
            catch (Exception ex)
            {
                // Log the exception if logging is implemented
                return StatusCode(500, $"An error occurred while deleting the proforma invoice. Details: {ex.Message}");
            }
        }


        [HttpPost]
        public async Task<ActionResult> DebitVerifyVoucher(string DebitNoteNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var UserName = HttpContext.Session.GetString("UserName");
                     var UserId = HttpContext.Session.GetString("UserId");
            var TenantName = HttpContext.Session.GetString("TenantName");

                    if (string.IsNullOrEmpty(DebitNoteNo))
                    {
                        return BadRequest(new { Message = "Debit number is required." });
                    }

                    var voucher = dbContext.Tbl20172VatdebitNoteMasters.FirstOrDefault(v => v.DebitNoteNo == DebitNoteNo);

                    if (voucher == null)
                    {
                        return NotFound(new { Message = "Debit not found." });
                    }

                    // Update the fields
                    voucher.IsVerified = true;
                    voucher.VerifiedOn = DateTime.Now;
                    voucher.VerifiedBy = UserName;

                    dbContext.SaveChanges();
                    await _userActionLogger.LogAsync(
module: "VAT>Debit Verify Voucher",
actionDetail: $"Debit Verify Voucher Number: {DebitNoteNo}",
documentNo: DebitNoteNo
);

                             var notifyRequest = new NotificationRequest
             {
                 UserId = UserId, // or fetch from session/DB
                 VoucherName = DebitNoteNo,
                 ActionType = "You have one Debit Note to approve",
                TenantName = TenantName 
            };

        await _fcmService.SendNotificationAsync(notifyRequest);

                    return Ok(new
                    {
                        Message = "DebitNoteNo verified successfully.",
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
        public async Task<ActionResult> PurchaseVerifyVoucher(string InvoiceNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var UserName = HttpContext.Session.GetString("UserName");
                     var UserId = HttpContext.Session.GetString("UserId");
            var TenantName = HttpContext.Session.GetString("TenantName");


                    if (string.IsNullOrEmpty(InvoiceNo))
                    {
                        return BadRequest(new { Message = "Voucher number is required." });
                    }

                    var voucher = dbContext.Tbl20166VatpurchaseMasters.FirstOrDefault(v => v.PurchaseVoucherNo == InvoiceNo);

                    if (voucher == null)
                    {
                        return NotFound(new { Message = "Voucher not found." });
                    }

                    // Update the fields
                    voucher.IsVerified = true;
                    voucher.VerifiedOn = DateTime.Now;
                    voucher.VerifiedBy = UserName;

                    dbContext.SaveChanges();
                    await _userActionLogger.LogAsync(
module: "VAT>Purchase Verify Voucher",
actionDetail: $"Purchase Verify Voucher Number: {InvoiceNo}",
documentNo: InvoiceNo
);

                 var notifyRequest = new NotificationRequest
             {
                 UserId = UserId, // or fetch from session/DB
                 VoucherName = InvoiceNo,
                 ActionType = "You have one Purchase Invoice to Approve",
                TenantName = TenantName 
            };

        await _fcmService.SendNotificationAsync(notifyRequest);

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

        [HttpGet]
        public IActionResult GetGoodsAndServicesDetails(string gsCode)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var result = dbContext.Tbl20164GoodsAndServicesMasters
                                          .Where(x => x.Gscode == gsCode)
                                          .ToList();
                    var resultWithVAT = new List<ExpandoObject>();
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

                        var UnitRateMethodDesc = dbContext.Tbl40111PropertyUnitCodes
                   .Where(x => x.UnitCode == gridDetails.GsgroupId)
                   .Select(x => x.UnitDesc)
                   .FirstOrDefault();


                        // Add new dynamic column
                        dict["UnitRateMethod"] = UnitRateMethodDesc;


                        //dict["VAT"] = vatValue;
                        //dict["TotalVAT"] = totalValue;

                        resultWithVAT.Add(item);
                    }

                    return Ok(resultWithVAT); // ✅ Move this inside the try block
                }
                catch (Exception ex)
                {
                    return BadRequest(new { Message = ex.Message });
                }
            }

            return BadRequest(new { Message = "Unable to access tenant database context." }); // ✅ Handle TryGet failure
        }

        [HttpPost]
        public async Task<ActionResult> ProformaVerifyVoucher(string InvoiceNo)
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

                    var voucher = dbContext.Tbl20181ProformaInvoiceMasters.FirstOrDefault(v => v.ProformaInvoiceNo == InvoiceNo);

                    if (voucher == null)
                    {
                        return NotFound(new { Message = "Voucher not found." });
                    }

                    // Update the fields
                    voucher.IsVerified = true;
                    voucher.VerifiedOn = DateTime.Now;
                    voucher.VerifiedBy = UserName;

                    dbContext.SaveChanges();
                    await _userActionLogger.LogAsync(
module: "VAT>Proforma Verify Voucher",
actionDetail: $"Proforma Verify Voucher Number: {InvoiceNo}",
documentNo: InvoiceNo
);
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
        public async Task<ActionResult> ProformaApproveVoucher(string InvoiceNo, bool IsDirectApproval)
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

                    var voucher = dbContext.Tbl20181ProformaInvoiceMasters.FirstOrDefault(v => v.ProformaInvoiceNo == InvoiceNo);

                    if (voucher == null)
                    {
                        return NotFound(new { Message = "InvoiceNo not found." });
                    }

                    // Update the fields
                    voucher.IsApproved = true;
                    voucher.ApprovedOn = DateTime.Now;
                    voucher.ApprovedBy = UserName;

                    if (IsDirectApproval == false)
                    {
                        voucher.IsVerified = true;
                        voucher.VerifiedOn = DateTime.Now;
                        voucher.VerifiedBy = UserName;

                    }

                    dbContext.SaveChanges();
                    await _userActionLogger.LogAsync(
  module: "VAT> Proforma Approve Voucher",
  actionDetail: $"Approve VoucherNo: {InvoiceNo}",
  documentNo: InvoiceNo
);
                    return Ok(new
                    {
                        Message = "InvoiceNo Approved successfully.",
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

        [HttpGet]
        public async Task<IActionResult> GetDefalutCompanyDetails()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    string defaultCompanyString = HttpContext.Session.GetString("DefaultcompanyID");
                    byte defaultCompanyByte = 0;

                    if (!string.IsNullOrWhiteSpace(defaultCompanyString))
                    {
                        // Safely try to parse the value
                        byte.TryParse(defaultCompanyString, out defaultCompanyByte);
                    }

                    byte companyId = defaultCompanyByte;

                    var result = await dbContext.Tbl901CompanyDetails
                        .Where(g => g.CompanyId == companyId)
                        .Select(g => new
                        {
                            g.CompanyId,
                           g.CurrencyType
                        })
                        .ToListAsync();

                    return Ok(result);
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                // Logging the error is better than rethrowing directly
                _logger.LogError(ex, "Error occurred in GetDefalutCompanyDetails");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving company branch.", success = false });
            }
        }

        [HttpGet("{invoiceNo}")]
        public async Task<ActionResult> GetPurchaseApprovalStatus(string invoiceNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {

                    // Query the invoice approval status from tbl20161VATInvoiceMaster
                    var invoice = await dbContext.Tbl20166VatpurchaseMasters
                        .Where(i => i.PurchaseVoucherNo == invoiceNo)
                        .FirstOrDefaultAsync();

                    if (invoice == null)
                    {
                        return Ok(new { isApproved = invoice.IsApproved ?? false });
                    }

                    // Return the approval status
                    return Ok(new { isApproved = invoice.IsApproved, isPosted = invoice.IsPosted });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error fetching approval status for invoice {invoiceNo}: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized("Unable to fetch tenant information.");
        }

        [HttpGet]
        public IActionResult GetDropdownItemByCode(string code)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(code))
                        return BadRequest("Code is required.");

                    var item = dbContext.Tbl20164GoodsAndServicesMasters
                        .Where(x => x.Gscode == code)
                        .Select(x => new
                        {
                            x.Gscode,
                            x.Gsdescrpition,
                            x.GsdescriptionAr,
                            x.ItemPartNo,
                            x.CostPrice,
                            x.GssellingRate,
                            x.ReorderQty
                            //,x.Uni
                           
                        })
                        .FirstOrDefault();

                    if (item == null)
                        return NotFound($"Item with code '{code}' not found.");

                    return Json(item);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error fetching item details for code: {code}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return StatusCode(500, "Tenant context could not be established.");
        }



    }
}


