using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
namespace QD.ERP.Web.Areas.ERM.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]

    public class PropertyCategoryController : Controller
    {

        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<PropertyCategoryController> _logger;

        public PropertyCategoryController(ILogger<PropertyCategoryController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }
        [HttpGet("GetAllProperty")]
        public IActionResult GetAllProperty()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var PropertyCategory = dbContext.Tbl40102PropertyCategories
                        .Select(s => new
                        {
                            s.PropertyCategoryId,
                            s.PropertyCategoryName
                        })
                        .ToList();

                    return Ok(PropertyCategory);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error loading Propertys: {ex.Message}");
                    return StatusCode(500, new { message = "Failed to load Propertys.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant." });
        }
        public class PropertyCategoryInputModel
        {
            public string PropertyCategoryName { get; set; }
        }
        [HttpPost]
        public IActionResult AddPropertyCategory([FromBody] PropertyCategoryInputModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.PropertyCategoryName))
                    return BadRequest(new { message = "Property is required." });

                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized(new { message = "Invalid tenant." });

                // Get the max existing ID (assuming StockClassificationId is a byte, short, or int)
                short maxId = (short)dbContext.Tbl40102PropertyCategories
                         .Select(x => (short)x.PropertyCategoryId)
                         .AsEnumerable()
                         .DefaultIfEmpty((short)0)
                         .Max();

                // Check if maxId has reached its limit
                if (maxId == short.MaxValue)
                    return BadRequest(new { message = "Maximum Property ID limit reached." });

                var newPropertycategory = new Tbl40102PropertyCategory
                {
                    PropertyCategoryId = (byte)(maxId + 1),  // Assign the new ID
                    PropertyCategoryName = model.PropertyCategoryName
                };

                dbContext.Tbl40102PropertyCategories.Add(newPropertycategory);
                dbContext.SaveChanges();

                return Ok(new { message = "Property saved successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddProperty failed");
                return StatusCode(500, new { message = "An unexpected error occurred.", detailed = ex.Message });
            }
        }




        public class PropertyCategoryUpdateDto
        {
            public int Key { get; set; }
            public string Values { get; set; }  // JSON string of updated fields
        }

        [HttpPut("UpdatePropertCategory")]
        public IActionResult UpdatePropertCategory([FromForm] PropertyCategoryUpdateDto updateDto)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized(new { message = "Invalid tenant." });

                var existing = dbContext.Tbl40102PropertyCategories.FirstOrDefault(x => x.PropertyCategoryId == updateDto.Key);
                if (existing == null)
                    return NotFound(new { message = "Record not found." });

                // Deserialize the JSON string of values into dictionary
                var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(updateDto.Values);

                // Serialize back to JSON and populate existing entity only on provided fields
                var jsonValues = JsonConvert.SerializeObject(values);
                JsonConvert.PopulateObject(jsonValues, existing);

                dbContext.SaveChanges();

                return Ok(new { message = "Property Category updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateProperty failed");
                return StatusCode(500, new { message = "An error occurred while updating.", detailed = ex.Message });
            }
        }
        [HttpDelete]
        public IActionResult DeletePropertyCategory(string key)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized(new { message = "Invalid tenant." });

                if (!short.TryParse(key, out short id))
                    return BadRequest(new { message = "Invalid Property ID." });

                var PropertyCategory = dbContext.Tbl40102PropertyCategories.FirstOrDefault(x => x.PropertyCategoryId == id);
                if (PropertyCategory == null)
                    return NotFound(new { message = "Property Category not found." });

                dbContext.Tbl40102PropertyCategories.Remove(PropertyCategory);
                dbContext.SaveChanges();

                return Ok(new { message = "Deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error.", error = ex.Message });
            }
        }

    }
}
