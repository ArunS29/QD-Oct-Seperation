using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Shared.DAL.Entities;
using QD.ERP.Web.Service;

namespace QD.ERP.Web.Areas.ERM.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ERSupplierMasterController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<ERSupplierMasterController> _logger;

        public ERSupplierMasterController(ILogger<ERSupplierMasterController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetSupplierCategory(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var SupplierCategory = dbContext.Tbl3019901SupplierCategories.Select(i => new
                    {
                        i.SupplierCategoryCode,
                        i.SupplierCategory,
                        i.CategoryCode
                       
                    });

                    return Json(await DataSourceLoader.LoadAsync(SupplierCategory, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetProject: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet]
        public IActionResult GetLatestSupplierCode(string categoryCode)//SW
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var latestSupplierCode = dbContext.Tbl30199SupplierMasters
                        .Where(c => c.SupplierCode.StartsWith(categoryCode + "-"))
                        .OrderByDescending(c => c.SupplierCode)
                        .Select(c => c.SupplierCode)
                        .FirstOrDefault();

                    return Ok(latestSupplierCode); // returns e.g., "SW-4"  get LastSw no
                }

                return Unauthorized(new { message = "Invalid tenant." });
            }
            catch (Exception ex)
            {
                  _logger.LogError($"Error in GetProject: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });

            }
        }
        [HttpGet]
        public async Task<IActionResult> GetSupplierLedgerNo(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var SupplierLedgerNo = dbContext.Qry201710vatsundryCreditorsAndCashAccs.Select(i => new
                    {
                        i.AccountId,
                        i.AccountHead
                    });

                    return Json(await DataSourceLoader.LoadAsync(SupplierLedgerNo, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetProject: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public IActionResult InsertOrUpdate([FromBody] Tbl30199SupplierMaster supplierMaster)
        {
            if (supplierMaster == null)
            {
                return BadRequest("Invalid client data.");
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var existingClient = dbContext.Tbl30199SupplierMasters
                        .FirstOrDefault(c => c.SupplierCode == supplierMaster.SupplierCode);

                    if (existingClient != null)
                    {
                        // Update existing record
                        existingClient.SupplierName = supplierMaster.SupplierName;
                        existingClient.SupplierCategory = supplierMaster.SupplierCategory;
                       
                        existingClient.SupplierAccountLedgerNo = supplierMaster.SupplierAccountLedgerNo;
                        existingClient.SupplierAddress = supplierMaster.SupplierAddress;
                        existingClient.ContactPerson = supplierMaster.ContactPerson;
                        existingClient.ContactPersonTitle = supplierMaster.ContactPersonTitle;
                        existingClient.ContactMobile1 = supplierMaster.ContactMobile1;
                        existingClient.ContactMobile2 = supplierMaster.ContactMobile2;
                        existingClient.ContactPhone1 = supplierMaster.ContactPhone1;
                        existingClient.ContactPhone2 = supplierMaster.ContactPhone2;
                        existingClient.ContactFaxNo = supplierMaster.ContactFaxNo;
                        existingClient.ContactEmail = supplierMaster.ContactEmail;
         
                        existingClient.ContactRemarks = supplierMaster.ContactRemarks;


                        // Handle image fields (update them as byte[] or Base64)
                        if (supplierMaster.BusinessCard1 != null)
                            existingClient.BusinessCard1 = supplierMaster.BusinessCard1;

                        if (supplierMaster.BusinessCard2 != null)
                            existingClient.BusinessCard2 = supplierMaster.BusinessCard2;



                        dbContext.Tbl30199SupplierMasters.Update(existingClient);
                        dbContext.SaveChanges();

                        return Ok(new { success = true, message = "Supplier updated successfully." });
                    }
                    else
                    {
                        // Insert new record
                        var newSupplier = new Tbl30199SupplierMaster
                        {
                            SupplierCode = supplierMaster.SupplierCode,
                            SupplierName = supplierMaster.SupplierName,
                            SupplierCategory = supplierMaster.SupplierCategory,

                            SupplierAccountLedgerNo = supplierMaster.SupplierAccountLedgerNo,
                            SupplierAddress = supplierMaster.SupplierAddress,
                            ContactPerson = supplierMaster.ContactPerson,
                            ContactPersonTitle = supplierMaster.ContactPersonTitle,
                            ContactMobile1 = supplierMaster.ContactMobile1,
                            ContactMobile2 = supplierMaster.ContactMobile2,
                            ContactPhone1 = supplierMaster.ContactPhone1,
                            ContactPhone2 = supplierMaster.ContactPhone2,
                            ContactFaxNo = supplierMaster.ContactFaxNo,
                            ContactEmail = supplierMaster.ContactEmail,
                           
                            ContactRemarks = supplierMaster.ContactRemarks,
                            // Handle image fields (insert them as byte[] or Base64)
                            BusinessCard1 = supplierMaster.BusinessCard1,
                            BusinessCard2 = supplierMaster.BusinessCard2

                        };

                        dbContext.Tbl30199SupplierMasters.Add(newSupplier);
                        dbContext.SaveChanges();

                        return Ok(new { success = true, message = "Supplier saved successfully." });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error saving SupplierMaster: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet]
        public async Task<IActionResult> GetSupplierByCode(string supplierCode)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                if (string.IsNullOrEmpty(supplierCode))
                    return BadRequest("Supplier Code is required.");

                try
                {

                    var supplier = await dbContext.Tbl30199SupplierMasters
                        .Where(c => c.SupplierCode == supplierCode)
                        .FirstOrDefaultAsync();

                    if (supplier == null)
                        return NotFound("Supplier not found.");

                    return Ok(supplier);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetProject: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpPost]
        public IActionResult DeleteSupplierLeadMaster([FromBody] string SupplierCode)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var entity = dbContext.Tbl30199SupplierMasters
                        .FirstOrDefault(x => x.SupplierCode == SupplierCode);

                    if (entity == null)
                    {
                        return NotFound(new { success = false, message = "Record not found." });
                    }

                    dbContext.Tbl30199SupplierMasters.Remove(entity);
                    dbContext.SaveChanges();

                    return Ok(new { success = true, message = "Deleted successfully." });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetProject: {ex.Message}");
                    return StatusCode(500, new { success = false, message = $"Delete failed: {ex.Message}" });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet]
        public async Task<IActionResult> GetContactList(string SupplierCode)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var result = await dbContext.Tbl3019902SupplierContactLists
                     .Where(i => i.SupplierCode == SupplierCode)
                     .ToListAsync();

                    return Json(result);

                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetProject: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpDelete]
        public IActionResult DeleteContact(long SupplierContactSlNo)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var contact = dbContext.Tbl3019902SupplierContactLists
                        .FirstOrDefault(c => c.SupplierContactSlNo == SupplierContactSlNo);

                    if (contact != null)
                    {
                        dbContext.Tbl3019902SupplierContactLists.Remove(contact);
                        dbContext.SaveChanges();
                        return Ok(new { success = true, message = "Contact deleted successfully." });
                    }

                    return NotFound(new { success = false, message = "Contact not found." });
                }

                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }
            catch (Exception ex)
            {
                  _logger.LogError($"Error in GetProject: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
            }
        }
    }
}
