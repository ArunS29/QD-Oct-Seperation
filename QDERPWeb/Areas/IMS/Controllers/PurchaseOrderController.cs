using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System.Globalization;
using QD.ERP.Web.Services.Logging;

namespace QD.ERP.Web.Areas.IMS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PurchaseOrderController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<PurchaseOrderController> _logger;
        private readonly IUserActionLogger _userActionLogger;


        public PurchaseOrderController(ILogger<PurchaseOrderController> logger, TenantDbContextHelper tenantDbContextHelper, IUserActionLogger userActionLogger)
        {
            _userActionLogger = userActionLogger;
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetPurchaseOrderCategories()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var PurchaseOrderCategories = await dbContext.Tbl60404pocategories
                       .Select(s => new
                       {
                           s.PocategoryId,
                           s.PocategoryName

                       })
                        .ToListAsync();

                    return Json(PurchaseOrderCategories); // return raw data, paging/sorting done on client-side
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPurchaseOrderCategories");
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveOrUpdatePurchaseOrderCategories([FromBody] Tbl60404pocategory model) // ✅ Use correct entity class
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var existing = await dbContext.Tbl60404pocategories
                        .FirstOrDefaultAsync(x => x.PocategoryId == model.PocategoryId);

                    if (existing != null)
                    {
                        existing.PocategoryName = model.PocategoryName;
                    }
                    else
                    {
                        var lastId = await dbContext.Tbl60404pocategories
                            .OrderByDescending(x => x.PocategoryId)
                            .Select(x => (int?)x.PocategoryId)
                            .FirstOrDefaultAsync();

                        model.PocategoryId = (byte)((lastId ?? 0) + 1);
                        dbContext.Tbl60404pocategories.Add(model);
                    }

                    await dbContext.SaveChangesAsync();
                    await _userActionLogger.LogAsync(
                      module: "IMS > Save Or Update Purchase Order Categories",
                      actionDetail: $"Saved Purchase Order Categories  {model.PocategoryId}",
                      documentNo: $"{model.PocategoryId}"
                    );
                    return Ok(new { success = true, message = "Saved successfully", id = model.PocategoryId });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in SaveOrUpdateStatus: {ex}");
                    return StatusCode(500, new { success = false, message = ex.Message });
                }
            }

            return Unauthorized(new { success = false, message = "Invalid tenant" });
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePurchaseOrderCategories(byte id)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var existing = await dbContext.Tbl60404pocategories
                        .FirstOrDefaultAsync(x => x.PocategoryId == id);

                    if (existing == null)
                    {
                        return NotFound(new { success = false, message = "Status not found" });
                    }

                    dbContext.Tbl60404pocategories.Remove(existing);
                    await dbContext.SaveChangesAsync();
                    await _userActionLogger.LogAsync(
                     module: "IMS > Delete Purchase Order Categories",
                     actionDetail: $":Deleted Purchase Order Categories  {id}",
                     documentNo: $"{id}"
                   );

                    return Ok(new { success = true, message = "Deleted successfully" });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in DeleteStatus: {ex}");
                    return StatusCode(500, new { success = false, message = ex.Message });
                }
            }

            return Unauthorized(new { success = false, message = "Invalid tenant" });
        }
        [HttpGet]
        public async Task<IActionResult> GetPurchaseItemDetailsByPoDate(string frmDate, string toDate){
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext)){
                try{
                    if (!DateTime.TryParseExact(frmDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime from))
                        return BadRequest("Invalid fromDate format. Use MM/dd/yyyy.");
                     

                    if (!DateTime.TryParseExact(toDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime to))
                        return BadRequest("Invalid toDate format. Use MM/dd/yyyy.");

                    var data = await dbContext.Qry60410purchaseItemDetails
                        .Where(x => x.Podate >= from && x.Podate <= to)
                        .ToListAsync();

                    return Ok(data);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"An error occurred while fetching the data : {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant." });
        }
        [HttpGet]
        public IActionResult Get(DataSourceLoadOptions loadOptions, string Mprno){
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
            try
                {
                    var data = dbContext.Qry60404purchaseOrderViewMasters.AsQueryable();

                    if (!string.IsNullOrEmpty(Mprno))
                    {
                        data = data.Where(item => item.Pono == Mprno);
                    }

                    var result = DataSourceLoader.Load(data, loadOptions);
                    return Json(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"An error occurred while fetching the data : {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant." });
        }
    }
}
