using DevExpress.PivotGrid.PivotTable;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Shared.DAL.Entities;
using QD.ERP.Web.Service;
using QD.ERP.Web.Services.Logging;

namespace QD.ERP.Web.Areas.IMS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ClientStatusCodeController : Controller
    {
        private ERPMasterWtDataContext _context;
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<ClientLeadsController> _logger;
        private readonly IUserActionLogger _userActionLogger;




        public ClientStatusCodeController(ILogger<ClientLeadsController> logger, TenantDbContextHelper tenantDbContextHelper, IUserActionLogger userActionLogger)
        {
            _userActionLogger = userActionLogger;
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }


        [HttpGet]
        public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var qry = dbContext.Tbl30103ClientStatusCodes
                    .Select(i => new
                    {
                        i.StatusCode,
                        i.Status,
                      

                    });

                return Json(await DataSourceLoader.LoadAsync(qry, loadOptions));

                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetStatusCode: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> BatchUpdate([FromBody] List<BatchUpdateModel> updates)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized(new { message = "Invalid tenant", success = false });

                foreach (var item in updates)
                {
                    var entity = await dbContext.Tbl30103ClientStatusCodes.FindAsync(item.key);
                    if (entity == null) continue;

                    // Only update the fields passed from client
                    foreach (var kv in item.values)
                    {
                        if (kv.Key == "Status")
                        {
                            entity.Status = kv.Value?.ToString();
                        }
                    }
                }

                await dbContext.SaveChangesAsync();
                await _userActionLogger.LogAsync(
                  module: "IMS > Batch Update",
                  actionDetail: $":Batch Updated {updates[0].key}",
                   documentNo: $"{updates[0].key}"
                );

                return Json(new { success = true, message = "Updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"🔥 Error in BatchUpdate: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Failed to save changes.", details = ex.Message });
            }
        }

        public class BatchUpdateModel
        {
            public int key { get; set; }
            public Dictionary<string, object> values { get; set; }
        }

        [HttpPost]
        public IActionResult CreateClientCategory([FromBody] ClientStatusDisplayDTO vm)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    bool isDuplicate = dbContext.Tbl30103ClientStatusCodes
                .Any(c => c.Status.ToLower() == vm.Status.ToLower());

                    if (isDuplicate)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "This client status is already in the database. Please check again."
                        });
                    }

                    // Step 1: Get the last ClientCategoryCode
                    short lastCode = dbContext.Tbl30103ClientStatusCodes
                                         .OrderByDescending(c => c.StatusCode)
                                         .Select(c => c.StatusCode)
                                         .FirstOrDefault();

                short newCode = (short)(lastCode + 1);

                // Step 2: Create and save new record
                var newCategory = new Tbl30103ClientStatusCode
                {
                   
                    Status = vm.Status,
                    StatusCode =(byte)newCode
                };

                dbContext.Tbl30103ClientStatusCodes.Add(newCategory);
                dbContext.SaveChanges();
                    _userActionLogger.LogAsync(module: "IMS > Create Client Category",
                     actionDetail: $":Created Client Category {vm.StatusCode}",
                     documentNo: $"{vm.StatusCode}"
                   );


                    var allData = dbContext.Tbl30103ClientStatusCodes
             .OrderBy(e => e.StatusCode)
             .Select(e => new ClientStatusDisplayDTO
             {
                 StatusCode = e.StatusCode,
                 Status = e.Status,
                
             })
             .ToList();

                // ✅ Return all data in the same structure
                return Ok(new { data = allData });
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                
                _logger.LogError($"Error in SaveOrUpdateMode: {ex}");
                return StatusCode(500, new { success = false, message = "An error occurred while saving Client Status." });


            }
        }

        [HttpPost]
        public IActionResult UpdateClientCategories([FromBody] List<ClientStatusDisplayDTO> updatedList)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    foreach (var item in updatedList)
                {
                    var entity = dbContext.Tbl30103ClientStatusCodes
                        .FirstOrDefault(x => x.StatusCode == item.StatusCode);

                    if (entity != null)
                    {
                        entity.Status = item.Status;
                        entity.StatusCode = item.StatusCode;
                        // Update other fields if necessary
                    }
                }

                dbContext.SaveChanges();
                    _userActionLogger.LogAsync(module: "IMS > Update Client Categories",
                      actionDetail: $":Updated Client Categories {updatedList[0].StatusCode}",
                      documentNo: $"{updatedList[0].StatusCode}"
                    );
                    return Ok(new { message = "Updated successfully" });
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UpdateClient: {ex.Message}");
                return StatusCode(500, $"Update failed: {ex.Message}");
            }
        }

        [HttpDelete]
        public IActionResult Delete(byte key)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var record = dbContext.Tbl30103ClientStatusCodes.FirstOrDefault(x => x.StatusCode == key);
                    if (record == null)
                        return NotFound();

                    dbContext.Tbl30103ClientStatusCodes.Remove(record);
                    dbContext.SaveChanges();
                    _userActionLogger.LogAsync(module: "IMS > Delete ",
                        actionDetail: $"Deleted  {key}",
                        documentNo: $"{key}"
                       );

                    return Ok();
                }

                return Unauthorized(new { success = false, message = "Invalid tenant" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Delete: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });

            }
        }

    }
}
