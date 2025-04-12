using DevExpress.Xpo;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ClientCategoryController : Controller
    {
        private ERPMasterWtDataContext _context;

        public ClientCategoryController(ERPMasterWtDataContext context) {
      
            _context = context;
        }


        //[HttpPost]
        //public async Task<ActionResult> AddClientEntry(DataSourceLoadOptions loadOptions, ClientCategoryDisplayDTO VE)
        //{
        //    if (VE == null)
        //    {
        //        return BadRequest(new { success = false, message = "Invalid data received." });
        //    }

        //    try
        //    {
        //        short lastCode = await _context.Tbl30102ClientCategories
        //         .Where(e => e.CategoryCode == VE.CategoryCode)
        //       .Select(e => (dynamic)e.ClientCategoryCode)
        //       .OrderByDescending(e => e)
        //       .FirstOrDefaultAsync() ?? 0;

        //        VE.ClientCategoryCode = (short)(lastCode + 1);


        //        // Map DTO to Entity
        //        var newEntity = new Tbl30102ClientCategory
        //        {
        //            CategoryCode = VE.CategoryCode,
        //            ClientCategory = VE.ClientCategory,
        //            ClientCategoryCode = (short)(lastCode + 1),
        //           // AddedOn = DateTime.Now, // Optional: if you have audit fields
        //           //AddedBy = "CurrentUser" // Replace with actual user context
        //        };

        //        _context.Tbl30102ClientCategories.Add(newEntity);
        //        await _context.SaveChangesAsync();

        //        // Query updated list for return
        //        var query = _context.Tbl30102ClientCategories
        //            .Where(p => p.CategoryCode == VE.CategoryCode)
        //            .OrderBy(e => e.ClientCategoryCode)
        //            .Select(e => new ClientCategoryDisplayDTO
        //            {
        //                CategoryCode = e.CategoryCode,
        //                ClientCategory = e.ClientCategory,
        //                ClientCategoryCode = e.ClientCategoryCode
        //            });

        //        var result = await DataSourceLoader.LoadAsync<ClientCategoryDisplayDTO>(query, loadOptions);
        //        return Json(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
        //    }
        //}

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
                return Ok(new { message = "Updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Update failed: {ex.Message}");
            }
        }



    }
}
