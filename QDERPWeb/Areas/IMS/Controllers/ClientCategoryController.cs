using DevExpress.Xpo;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using QD.ERP.Web.Services.Logging;

namespace QD.ERP.Web.Areas.IMS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ClientCategoryController : Controller
    {
        private ERPMasterWtDataContext _context;
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<SalesOrdersController> _logger;
        private readonly IUserActionLogger _userActionLogger;


        public ClientCategoryController(ERPMasterWtDataContext context, IUserActionLogger userActionLogger) 
        {

            _userActionLogger = userActionLogger;

            _context = context;
        }


      

        [HttpGet]
        public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions)
        {
            try
            {
               
                    var qry = _context.Tbl30102ClientCategories
                        .Select(i => new
                        {
                           i.ClientCategoryCode,
                           i.ClientCategory,
                           i.CategoryCode

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
        public IActionResult CreateClientCategory([FromBody] ClientCategoryDisplayDTO vm)
        {
            try
            {
                // Step 1: Get the last ClientCategoryCode
                short lastCode = _context.Tbl30102ClientCategories
                                         .OrderByDescending(c => c.ClientCategoryCode)
                                         .Select(c => c.ClientCategoryCode)
                                         .FirstOrDefault();

                short newCode = (short)(lastCode + 1);

                // Step 2: Create and save new record
                var newCategory = new Tbl30102ClientCategory
                {
                    ClientCategory = vm.ClientCategory,
                    CategoryCode = vm.CategoryCode,
                    ClientCategoryCode = newCode
                };

                _context.Tbl30102ClientCategories.Add(newCategory);
                _context.SaveChanges();
                _userActionLogger.LogAsync(module: "IMS > Create Client Category",
                         actionDetail: $":Created Client Category {vm.ClientCategoryCode}",
                       documentNo: $"{vm.ClientCategoryCode}"
                    );


                var allData = _context.Tbl30102ClientCategories
             .OrderBy(e => e.ClientCategoryCode)
             .Select(e => new ClientCategoryDisplayDTO
             {
                 CategoryCode = e.CategoryCode,
                 ClientCategory = e.ClientCategory,
                 ClientCategoryCode = e.ClientCategoryCode
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
        public IActionResult UpdateClientCategories([FromBody] List<ClientCategoryDisplayDTO> updatedList)
        {
            try
            {
                foreach (var item in updatedList)
                {
                    var entity = _context.Tbl30102ClientCategories
                        .FirstOrDefault(x => x.ClientCategoryCode == item.ClientCategoryCode);

                    if (entity != null)
                    {
                        entity.ClientCategory = item.ClientCategory;
                        entity.CategoryCode = item.CategoryCode;
                        // Update other fields if necessary
                    }
                }

                _context.SaveChanges();
                _userActionLogger.LogAsync(module: "IMS > Update Client Categories",
                   actionDetail: $":Updated Client Categories {updatedList[0].ClientCategoryCode}",
                   documentNo: $"{updatedList[0].ClientCategoryCode}"
                );
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
