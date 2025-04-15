using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;

namespace QD.ERP.Web.Areas.IMS.Controllers
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

        [HttpPost]

        public IActionResult SaveSupplierContactList([FromBody] Tbl3019902SupplierContactList item)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    dbContext.Tbl3019902SupplierContactLists.Add(item);
                    dbContext.SaveChanges();
                   return Ok(new 
            { 
                success = true, 
                message = "Client Category saved successfully",
                lastSupplierContactSlNo = item.SupplierContactSlNo 
            });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "Error saving data: " + ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
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
                    return StatusCode(500, new { success = false, message = $"Delete failed: {ex.Message}" });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
    }
}
