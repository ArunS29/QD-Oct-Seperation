using DevExpress.CodeParser;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;

namespace QD.ERP.Web.Areas.ERM.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SupplierContactListController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<SupplierContactListController> _logger;
        public SupplierContactListController(ILogger<SupplierContactListController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }
        [HttpGet]
        public IActionResult GetContactBySlNo(int supplierContactSlNo)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var data = dbContext.Tbl3019902SupplierContactLists
                        .FirstOrDefault(x => x.SupplierContactSlNo == supplierContactSlNo);

                    if (data != null)
                    {
                        return Ok(data);
                    }

                    return NotFound(new { message = "Supplier contact not found." });
                }

                return Unauthorized(new { message = "Invalid tenant." });
            }
            catch (Exception ex)
            {
                  _logger.LogError($"Error in GetProject: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult SaveOrUpdateSupplierContact([FromBody] Tbl3019902SupplierContactList item)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    if (item.SupplierContactSlNo > 0)
                    {
                        // Update
                        var existing = dbContext.Tbl3019902SupplierContactLists
                            .FirstOrDefault(s => s.SupplierContactSlNo == item.SupplierContactSlNo);

                        if (existing != null)
                        {
                            existing.SupplierCode = item.SupplierCode;
                            //existing.SupplierName = item.SupplierName;
                            existing.ContactPerson = item.ContactPerson;
                            existing.ContactPersonTitle = item.ContactPersonTitle;
                            existing.ContactMobile1 = item.ContactMobile1;
                            existing.ContactPhone1 = item.ContactPhone1;
                            existing.ContactMobile2 = item.ContactMobile2;
                            existing.ContactPhone2 = item.ContactPhone2;
                            existing.ContactFaxNo = item.ContactFaxNo;
                            existing.ContactEmail = item.ContactEmail;

                            dbContext.SaveChanges();

                            return Ok(new
                            {
                                success = true,
                                message = "Supplier contact updated successfully",
                                lastSupplierContactSlNo = existing.SupplierContactSlNo
                            });
                        }

                    }
                    else
                    {
                        // Insert
                        dbContext.Tbl3019902SupplierContactLists.Add(item);
                        dbContext.SaveChanges();

                        return Ok(new
                        {
                            success = true,
                            message = "Supplier contact saved successfully",
                            lastSupplierContactSlNo = item.SupplierContactSlNo
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetProject: {ex.Message}");
                    return StatusCode(500, new { success = false, message = "Error saving data: " + ex.Message });
                }
            }

            return Unauthorized(new { success = false, message = "Invalid tenant." });
        }

        [HttpPost]
        public IActionResult DeleteSupplierContactList([FromBody] long SupplierContactSlNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var entity = dbContext.Tbl3019902SupplierContactLists
                        .FirstOrDefault(x => x.SupplierContactSlNo == SupplierContactSlNo);

                    if (entity == null)
                    {
                        return NotFound(new { success = false, message = "Record not found." });
                    }

                    dbContext.Tbl3019902SupplierContactLists.Remove(entity);
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
    }
}
