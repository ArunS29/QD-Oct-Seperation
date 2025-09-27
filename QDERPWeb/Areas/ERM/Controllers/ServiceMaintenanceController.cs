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

        [HttpPost]
                public async Task<IActionResult> SaveOrUpdateServiceRequest([FromBody] Tbl40132PropertyServiceMaster VM)
                {
                    if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    {
                        return Unauthorized(new { success = false, message = "Invalid tenant context." });
                    }

                    if (VM == null || string.IsNullOrEmpty(VM.ServiceSheetNo))
                    {
                        return BadRequest(new { success = false, message = "Service Sheet No is required." });
                    }

                    try
                    {
                        // ===== MASTER TABLE =====
                        var existingMaster = await dbContext.Tbl40132PropertyServiceMasters
                            .FirstOrDefaultAsync(x => x.ServiceSheetNo == VM.ServiceSheetNo);

                        if (existingMaster != null)
                        {
                            // Update existing
                            existingMaster.ServiceSheetNo = VM.ServiceSheetNo;
                            existingMaster.ServiceDate = VM.ServiceDate;
                            existingMaster.PropertyNo = VM.PropertyNo;
                            existingMaster.ServicedBy = VM.ServicedBy;
                            existingMaster.PropertyBroughtBy = VM.PropertyBroughtBy;
                            existingMaster.Location = VM.Location;
                            existingMaster.OperatorName = VM.OperatorName;
                            existingMaster.Complaint = VM.Complaint;
                            existingMaster.WorkDetails = VM.WorkDetails;
                            existingMaster.ServiceStatus = VM.ServiceStatus;
                            existingMaster.AddedBy = VM.AddedBy;
                            existingMaster.AddedOn = VM.AddedOn;
                            existingMaster.ModifiedBy = "System";
                            existingMaster.ModifiedOn = DateTime.UtcNow;
                            existingMaster.OperatorContactNo = VM.OperatorContactNo;
                            existingMaster.Project = VM.Project;
                            existingMaster.ServiceOrderType = VM.ServiceOrderType;
                            existingMaster.StoreIssuedFrom = VM.StoreIssuedFrom;
                        }
                        else
                        {
                            // Insert new
                            var newMaster = new Tbl40132PropertyServiceMaster
                            {
                                ServiceSheetNo = VM.ServiceSheetNo,
                                ServiceDate = VM.ServiceDate,
                                PropertyNo = VM.PropertyNo,
                                ServicedBy = VM.ServicedBy,
                                PropertyBroughtBy = VM.PropertyBroughtBy,
                                Location = VM.Location,
                                OperatorName = VM.OperatorName,
                                Complaint = VM.Complaint,
                                WorkDetails = VM.WorkDetails,
                                ServiceStatus = VM.ServiceStatus,
                                AddedBy = VM.AddedBy,
                                AddedOn = VM.AddedOn,
                                ModifiedBy = "System",
                                ModifiedOn = DateTime.UtcNow,
                                OperatorContactNo = VM.OperatorContactNo,
                                Project = VM.Project,
                                ServiceOrderType = VM.ServiceOrderType,
                                StoreIssuedFrom = VM.StoreIssuedFrom
                            };

                            await dbContext.Tbl40132PropertyServiceMasters.AddAsync(newMaster);
                        }

                        await dbContext.SaveChangesAsync();

                        await _userActionLogger.LogAsync(
                            module: "ERM > Save Enquiry",
                            actionDetail: $"Saved Enquiry Request {VM.ServiceSheetNo}",
                            documentNo: $"{VM.ServiceSheetNo}"
                        );

                        return Ok(new { success = true, message = "Property saved/updated successfully." });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error in SaveOrUpdatePropertyRequest: {ex.Message}", ex);
                        return StatusCode(500, new { success = false, message = "Internal server error. Please try again later." });
                    }
                }


    }
}
