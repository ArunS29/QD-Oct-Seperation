using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using DevExtreme.AspNet.Mvc;
using DevExtreme.AspNet.Data;
namespace QD.ERP.Web.Areas.Finance.Controllers
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
        //    string voucherString = $"{currentYear[^2..]}-{currentMonth}-"; // Optimized substring
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
                        i.ClientOtherIdtype

                    });


                    return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
                }

            }
            catch (Exception ex) { throw ex; }
            return Unauthorized(new { message = "Invalid tenant.", success = false });

        }


        [HttpGet]
        public IActionResult GetGoodsAndServices()
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
            catch(Exception ex)
            {
                throw ex;
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });

        }



    }
}