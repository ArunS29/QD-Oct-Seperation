using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using QD.ERP.Web.Services.Logging;

namespace QD.ERP.Web.Areas.ERM.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ServiceMaintenanceController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<ServiceMaintenanceController> _logger;
        private readonly IUserActionLogger _userActionLogger;


        public ServiceMaintenanceController(ILogger<ServiceMaintenanceController> logger, TenantDbContextHelper tenantDbContextHelper, IUserActionLogger userActionLogger)
        {
            _userActionLogger = userActionLogger;
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }
        [HttpGet]
        public ActionResult<string> GetNewRequestNoApi()
        {
            try
            {
                // Retrieve tenant name from session
                var tenantName = HttpContext.Session.GetString("TenantName");
                if (string.IsNullOrWhiteSpace(tenantName))
                {
                    return Unauthorized(new { message = "Tenant name not found in session.", success = false });
                }

                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    // Use tenantName to find the company
                    var company = dbContext.Tbl901CompanyDetails
                        .FirstOrDefault(c => c.CompanyNameShort == tenantName);
                    if (company == null)
                    {
                        return NotFound("Company not found.");
                    }

                    string EquipServiceOrderAbbrv = company.EquipServiceOrderAbbrv;
                    int invoiceYearDigits = company.InvoiceYearDigits ?? 0;
                    bool isResetInvoiceInYear = company.IsResetInvoiceInYear ?? false;
                    DateTime invoiceDate = DateTime.Now;

                    // Generate new debit note number
                    string newDebitNoteNo = GetNewDebitNoteNo(EquipServiceOrderAbbrv, invoiceYearDigits, invoiceDate, isResetInvoiceInYear, dbContext);

                    return Ok(newDebitNoteNo);
                }
                else
                {
                    return BadRequest("Tenant or DB Context not found.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetNewRequestNoApi: {ex.Message}");
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }


        private string GetNewDebitNoteNo(string invoiceAbbrv, int yearInDigit, DateTime invoiceDate, bool isResetByYear, ERPMasterWtDataContext dbContext)
        {
            try
            {
                // Retrieve MPR numbers into memory
                var mprNumbers = dbContext.Tbl40132PropertyServiceMasters
                    .Where(d => d.ServiceSheetNo != null && d.ServiceSheetNo.Length >= 5 &&
                                (!isResetByYear || (d.ServiceDate.HasValue && d.ServiceDate.Value.Year == invoiceDate.Year)))
                    .Select(d => d.ServiceSheetNo)
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

                return $"{invoiceAbbrv}{strYear}-{strNewDebitNoteNo}";
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

                return $"{invoiceAbbrv}{strYear}-00001";
            }
        }
        [HttpGet]
        public async Task<IActionResult> Getservicemaintance(DateTime? fromDate, DateTime? toDate)

        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var query = dbContext.Qry40501propertyServiceViews.AsQueryable();


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
                    query = query.Where(i => i.ServiceDate >= fromDate && i.ServiceDate <= toDate);

                    // Fetching the data
                    var data = await query.Select(i => new
                    {
                        i.ServiceSheetNo,
                        i.ServiceDate,
                        i.ServiceStatus,
                        i.PropertyDescription,
                        i.ServicedBy,
                        i.OperatorName,
                        i.Complaint,
                        i.TotalCost

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
        public async Task<IActionResult> GetpropertyVehicle()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var dbSignatories = await dbContext.Qry40102PropertyMasterView2s
                       .Select(s => new
                       {
                           s.PropertyNo,
                           s.PropertyDescription,
                           s.Brand,
                           s.PlateNo,
                           s.DoorNo,
                           s.Model,
                           s.ModelType
                       })
                        .ToListAsync();

                    return Json(dbSignatories); // return raw data, paging/sorting done on client-side
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetStoreItem()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var dbSignatories = await dbContext.Tbl60001storeMasters
                       .Select(s => new
                       {
                           s.StoreId,
                           s.StoreName
                       })
                        .ToListAsync();

                    return Json(dbSignatories); // return raw data, paging/sorting done on client-side
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetServiceTemplate()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var dbSignatories = await dbContext.Tbl40133PropertyServiceTemplates
                       .Select(s => new
                       {
                           s.ServiceTemplateCode,
                           s.TemplateType
                       })
                        .ToListAsync();

                    return Json(dbSignatories); // return raw data, paging/sorting done on client-side
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }

   
        public List<Qry40503propertyServiceSpareUsed> Childrens { get; set; }

            [HttpGet]
            public async Task<IActionResult> GetServiceProperty(string ServiceSheetNo)
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    if (string.IsNullOrEmpty(ServiceSheetNo))
                        return BadRequest("Service Sheet No is required.");

                    try
                    {
                        // Get master record
                        var requestNo = await dbContext.Tbl40132PropertyServiceMasters
                            .Where(c => c.ServiceSheetNo == ServiceSheetNo)
                            .FirstOrDefaultAsync();

                        if (requestNo == null)
                            return NotFound("Service Sheet No not found.");

                        // Get currency rate safely
                        var currencyRate = requestNo.CurrencyRate;
                        if (currencyRate == 0) currencyRate = 1m; // avoid divide by zero

                        // Get children records
                        var children = await dbContext.Qry40503propertyServiceSpareUseds
                            .Where(c => c.ServiceSheetNo == ServiceSheetNo)
                            .OrderBy(c => c.SpareSlNo)
                            .ToListAsync();

                        // Map children and adjust prices based on currencyRate
                        var childrenDto = children.Select((c, index) => new
                        {
                            SNo = index + 1,
                            c.SpareSlNo,
                            c.ServiceSheetNo,
                            c.Gscode,
                            c.UnitDesc,
                            c.QtyUsed,
                            CostPrice = c.CostPrice / currencyRate, // adjusted
                            TotalCost = c.TotalCost / currencyRate, // adjusted
                            c.Gsdescrpition
                        }).ToList();

                        // ✅ Return both master and children
                        return Ok(new
                        {
                            Master = requestNo,
                            Children = childrenDto
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error in GetServiceProperty: {ex.Message}");
                        return StatusCode(500, $"Internal server error: {ex.Message}");
                    }
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }

        public class ServiceRequestDto
                {
                    public decimal? CurrencyRate { get; set; }

                    public decimal? CostPrice { get; set; }
                    public List<Tbl40132PropertyServiceMaster> Master { get; set; }
                    public List<Tbl40134PropertyServiceSpareUsed> Children { get; set; }
                }

                [HttpPost]
                public async Task<IActionResult> SaveOrUpdateServiceRequest([FromBody] ServiceRequestDto VM)
                {
                    if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    {
                        return Unauthorized(new { success = false, message = "Invalid tenant context." });
                    }

                    if (VM == null || VM.Master == null || !VM.Master.Any())
                    {
                        return BadRequest(new { success = false, message = "At least one master record is required." });
                    }

                    try
                    {
                        foreach (var master in VM.Master)
                        {
                            if (string.IsNullOrEmpty(master.ServiceSheetNo))
                                return BadRequest(new { success = false, message = "ServiceSheetNo is required for all master records." });

                            // ===== MASTER =====
                            var existingMaster = await dbContext.Tbl40132PropertyServiceMasters
                                .FirstOrDefaultAsync(x => x.ServiceSheetNo == master.ServiceSheetNo);

                            if (existingMaster != null)
                            {
                                // Update existing
                                existingMaster.ServiceDate = master.ServiceDate;
                                existingMaster.PropertyNo = master.PropertyNo;
                                existingMaster.ServicedBy = master.ServicedBy;
                                existingMaster.PropertyBroughtBy = master.PropertyBroughtBy;
                                existingMaster.Location = master.Location;
                                existingMaster.OperatorName = master.OperatorName;
                                existingMaster.Complaint = master.Complaint;
                                existingMaster.WorkDetails = master.WorkDetails;
                                existingMaster.ServiceStatus = master.ServiceStatus;
                                existingMaster.ModifiedBy = "System";
                                existingMaster.ModifiedOn = DateTime.UtcNow;
                                existingMaster.OperatorContactNo = master.OperatorContactNo;
                                existingMaster.Project = master.Project;
                                existingMaster.StoreIssuedFrom = master.StoreIssuedFrom;
                                existingMaster.ServiceOrderType = master.ServiceOrderType;
                                existingMaster.CurrencyRate = master.CurrencyRate ?? 1;
                                existingMaster.BaseCurrencyId = master.BaseCurrencyId ?? 1;
                                existingMaster.CurrencyId = master.CurrencyId ?? 1;
                            }
                            else
                            {
                                // Insert new
                                var newMaster = new Tbl40132PropertyServiceMaster
                                {
                                    ServiceSheetNo = master.ServiceSheetNo,
                                    ServiceDate = master.ServiceDate,
                                    PropertyNo = master.PropertyNo,
                                    ServicedBy = master.ServicedBy,
                                    PropertyBroughtBy = master.PropertyBroughtBy,
                                    Location = master.Location,
                                    OperatorName = master.OperatorName,
                                    Complaint = master.Complaint,
                                    WorkDetails = master.WorkDetails,
                                    ServiceStatus = master.ServiceStatus,
                                    StoreIssuedFrom = master.StoreIssuedFrom,
                                    AddedBy = master.AddedBy,
                                    AddedOn = master.AddedOn ?? DateTime.UtcNow,
                                    ModifiedBy = "System",
                                    ModifiedOn = DateTime.UtcNow,
                                    OperatorContactNo = master.OperatorContactNo,
                                    Project = master.Project,
                                    ServiceOrderType = master.ServiceOrderType,
                                    CurrencyRate = master.CurrencyRate ?? 1,
                                    BaseCurrencyId = master.BaseCurrencyId ?? 1 ,
                                    CurrencyId = master.CurrencyId ?? 1

                                };

                                await dbContext.Tbl40132PropertyServiceMasters.AddAsync(newMaster);
                            }

                    // ===== CHILDREN =====
                    var childrenForMaster = VM.Children?.ToList();
                    if (childrenForMaster != null && childrenForMaster.Any())
                    {
                        var existingChildren = await dbContext.Tbl40134PropertyServiceSpareUseds
                            .Where(c => c.ServiceSheetNo == master.ServiceSheetNo)
                            .ToListAsync();

                        var currencyRate = master.CurrencyRate ?? 1; // use master-level rate

                        foreach (var child in childrenForMaster)
                        {
                            // 🔑 ensure ServiceSheetNo is set here
                            child.ServiceSheetNo = master.ServiceSheetNo;

                            var existingChild = existingChildren.FirstOrDefault(c => c.SpareSlNo == child.SpareSlNo);
                            if (existingChild != null)
                            {
                                existingChild.Gscode = child.Gscode;
                                existingChild.QtyUsed = child.QtyUsed;
                                existingChild.CostPrice = child.CostPrice * currencyRate;
                                existingChild.GsuoM = child.GsuoM;
                                existingChild.ModifiedBy = "System";
                                existingChild.ModifiedOn = DateTime.UtcNow;
                            }
                            else
                            {
                                var newChild = new Tbl40134PropertyServiceSpareUsed
                                {
                                    SpareSlNo = child.SpareSlNo,
                                    ServiceSheetNo = master.ServiceSheetNo,
                                    Gscode = child.Gscode,
                                    QtyUsed = child.QtyUsed,
                                    GsuoM = child.GsuoM,
                                    CostPrice = child.CostPrice * currencyRate,
                                    AddedBy = child.AddedBy,
                                    AddedOn = child.AddedOn ?? DateTime.UtcNow,
                                    ModifiedBy = "System",
                                    ModifiedOn = DateTime.UtcNow
                                };

                                await dbContext.Tbl40134PropertyServiceSpareUseds.AddAsync(newChild);
                            }
                        }
                    }
                }

                        // ===== SAVE ALL CHANGES =====
                        await dbContext.SaveChangesAsync();

                        // ===== LOGGING =====
                        await _userActionLogger.LogAsync(
                            module: "ERM > Service Request",
                            actionDetail: $"Saved/Updated {VM.Master.Count} Service Request(s)",
                            documentNo: string.Join(",", VM.Master.Select(m => m.ServiceSheetNo))
                        );

                        return Ok(new { success = true, message = "Service request(s) saved/updated successfully." });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error in SaveOrUpdateServiceRequest: {ex.Message}", ex);
                        return StatusCode(500, new { success = false, message = "Internal server error. Please try again later." });
                    }
                }
       [HttpDelete("{SpareSlNo}")]
            public async Task<IActionResult> DeleteServiceChild(int SpareSlNo)
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized("Invalid tenant");

                try
                {
                    // ✅ Call the stored procedure directly
                    var rowsAffected = await dbContext.Database.ExecuteSqlInterpolatedAsync(
                        $"EXEC sp40106DeleteSpareItem @SpareSlNo = {SpareSlNo}"
                    );

                    if (rowsAffected == 0)
                        return NotFound(new { error = "Child record not found or already deleted." });

                    // ✅ Log action
                    await _userActionLogger.LogAsync(
                        module: "ERM > Delete Request",
                        actionDetail: $"Deleted Spare Item with SpareSlNo {SpareSlNo}",
                        documentNo: SpareSlNo.ToString()
                    );

                    return Ok(new { message = "Child record deleted successfully." });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { error = ex.Message });
                }
            }
        [HttpDelete("{serviceSheetNo}")]
        public async Task<IActionResult> DeleteServiceMaster(string serviceSheetNo)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized("Invalid tenant");

            try
            {
                var masterRecord = await dbContext.Tbl40132PropertyServiceMasters
                    .FirstOrDefaultAsync(x => x.ServiceSheetNo == serviceSheetNo);

                if (masterRecord == null)
                    return NotFound(new { error = "Master record not found." });

                dbContext.Tbl40132PropertyServiceMasters.Remove(masterRecord);
                await dbContext.SaveChangesAsync();

                // ✅ Log action
                await _userActionLogger.LogAsync(
                    module: "ERM > Delete Request",
                    actionDetail: $"Deleted Service Master with ServiceSheetNo {serviceSheetNo}",
                    documentNo: serviceSheetNo.ToString()
                );

                return Ok(new { message = "Service master deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

    }
}
    