using QD.ERP.Web.Models.DALCommon;
using QD.ERP.Web.Models.DAL;
using QD.ERP.Web.Models.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.Service;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace QD.ERP.Web.Areas.Utility.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PermissionsController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        private readonly DbContextFactory _dbContextFactory;
        public PermissionsController (IMemoryCache cache, DbContextFactory dbContextFactory)
        {
            _cache = cache; 
            _dbContextFactory = dbContextFactory;
        }
        [HttpGet]
        public IActionResult GetAccessDetails(string formId)
        {
            if (string.IsNullOrWhiteSpace(formId))
            {
                return BadRequest(new { message = "All fields are required.", success = false });
            }
            var tenantName = User.Claims.FirstOrDefault(c => c.Type == "TenantName")?.Value;
            var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (_cache.TryGetValue("tenant_", out Dictionary<string, Tenant> tenantCache) &&
                tenantCache.TryGetValue(tenantName.ToLower(), out Tenant tenant))
            {
                try
                {
                    using (var dbContext = _dbContextFactory.CreateDbContext(tenant.ConnectionString))
                    {

                        var permissions = dbContext.TblUserAccesses
                            .Where(p => p.UserId == byte.Parse(userId) && p.ItemName.ToLower().StartsWith(formId.ToLower() + "_"))
                            .Select(p => new Permission
                            {
                                UserId = p.UserId,
                                ItemForm = p.ItemForm,
                                ItemName = p.ItemName,
                                ItemEnabled = p.ItemEnabled,
                                ItemVisible = p.ItemVisible
                            })
                            .ToList();

                        
                        return Ok(new
                        {
                            message = "successful",
                            success = true,
                            permissions
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred during login.", success = false });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

    }
}
