using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;

namespace QD.ERP.Web.Areas.IMS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ClientStatusCodeController : Controller
    {
        private ERPMasterWtDataContext _context;
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<ClientLeadsController> _logger;

       

        public ClientStatusCodeController(ILogger<ClientLeadsController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
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
        public IActionResult CreateClientCategory([FromBody] ClientStatusDisplayDTO vm)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
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
                _logger.LogError($"Error in CreateClientCategory: {ex.Message}");
                return StatusCode(500, $"Error: {ex.Message}");
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
                return Ok(new { message = "Updated successfully" });
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UpdateClientCategories: {ex.Message}");
                return StatusCode(500, $"Update failed: {ex.Message}");
            }
        }



    }
}
