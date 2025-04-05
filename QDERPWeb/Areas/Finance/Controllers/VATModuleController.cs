using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using DevExtreme.AspNet.Mvc;
using DevExtreme.AspNet.Data;

using QD.ERP.Web.Areas.Finance.Models;
using System.Text.RegularExpressions;
using System.Data.SqlClient;


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
                        i.ClientOtherIdtype

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
            catch(Exception ex)
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

    }
}