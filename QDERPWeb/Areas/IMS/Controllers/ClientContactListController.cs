using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using SkiaSharp;

namespace QD.ERP.Web.Areas.IMS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ClientContactListController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<ClientContactListController> _logger;
        public ClientContactListController(ILogger<ClientContactListController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpPost]
       
        public IActionResult SaveClientCategory([FromBody] Tbl3010102clientContactList item)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
            {
                dbContext.Tbl3010102clientContactLists.Add(item);
                dbContext.SaveChanges();
                return Ok(new { message = "Client Category saved successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error saving data: " + ex.Message });
            }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpPost]
        public IActionResult DeleteClientCategory([FromBody] string ClientCode)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
            {
                var entity = dbContext.Tbl3010102clientContactLists
                    .FirstOrDefault(x => x.ClientCode == ClientCode);

                if (entity == null)
                {
                    return NotFound(new { success = false, message = "Record not found." });
                }

                dbContext.Tbl3010102clientContactLists.Remove(entity);
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
