using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.DAL.Entities;

namespace QD.ERP.Web.Areas.IMS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ClientStatusCodeController : Controller
    {
        private ERPMasterWtDataContext _context;
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<SalesOrdersController> _logger;

        public ClientStatusCodeController(ERPMasterWtDataContext context)
        {

            _context = context;
        }




        [HttpGet]
        public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions)
        {
            try
            {

                var qry = _context.Tbl30103ClientStatusCodes
                    .Select(i => new
                    {
                        i.StatusCode,
                        i.Status,
                      

                    });

                return Json(await DataSourceLoader.LoadAsync(qry, loadOptions));



            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }


        [HttpPost]
        public IActionResult CreateClientCategory([FromBody] ClientStatusDisplayDTO vm)
        {
            try
            {
                // Step 1: Get the last ClientCategoryCode
                short lastCode = _context.Tbl30103ClientStatusCodes
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

                _context.Tbl30103ClientStatusCodes.Add(newCategory);
                _context.SaveChanges();

                var allData = _context.Tbl30103ClientStatusCodes
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
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpPost]
        public IActionResult UpdateClientCategories([FromBody] List<ClientStatusDisplayDTO> updatedList)
        {
            try
            {
                foreach (var item in updatedList)
                {
                    var entity = _context.Tbl30103ClientStatusCodes
                        .FirstOrDefault(x => x.StatusCode == item.StatusCode);

                    if (entity != null)
                    {
                        entity.Status = item.Status;
                        entity.StatusCode = item.StatusCode;
                        // Update other fields if necessary
                    }
                }

                _context.SaveChanges();
                return Ok(new { message = "Updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, $"Update failed: {ex.Message}");
            }
        }



    }
}
