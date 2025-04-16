using DevExpress.Internal;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UtilityController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<UtilityController> _logger;

        public UtilityController(ILogger<UtilityController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult> SaveLayout(string layout, string form)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    ERPMasterWtDataContextProcedures _procedures = new ERPMasterWtDataContextProcedures(dbContext);
                    var ledgerData = await _procedures.sp901_01UpdateLayoutAsync(layout, form, "101", true);
                    return Json(ledgerData);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in SaveLayout: {ex.Message}");
                    return StatusCode(500, new { success = false, message = "An error occurred while saving the layout.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public ActionResult LoadLayout(string form)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var layout = dbContext.Tbl90111LayoutMasters
                        .Where(p => p.UserId == 101 && p.FormId == form)
                        .Select(i => new
                        {
                            i.LayoutJson
                        }).FirstOrDefault();

                    if (layout != null && layout.LayoutJson != null)
                    {
                        return Json(layout.LayoutJson);
                    }
                    else
                    {
                        return Json(null);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in LoadLayout: {ex.Message}");
                    return StatusCode(500, new { success = false, message = "An error occurred while loading the layout.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetCurrencyList(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var currencyList = dbContext.CurrencyMasters.Select(c => new
                    {
                        c.CurrencyID,
                        c.CurrencyName,
                        c.CurrencySymbol,
                        c.CurrencyUnicode,
                        c.IsDefault
                    });

                    return Json(await DataSourceLoader.LoadAsync(currencyList, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetCurrencyList: {ex.Message}");
                    return StatusCode(500, new { success = false, message = "An error occurred while fetching the currency list.", error = ex.Message });
                }
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet]
        public async Task<IActionResult> GetUserDetailsprofile(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    //var userId = HttpContext.Session.GetInt32("UserId");

                    //if (userId == null)
                    //{
                    //    return Unauthorized(new { message = "User is not logged in.", success = false });
                    //}
                    var userId = 106;

                    var userDetails = dbContext.TblUserMasters
                        .Where(u => u.UserId == userId)
                        .Select(u => new
                        {
                            u.UserPicture,
                            u.UserId,
                            u.UserName,
                            u.EmailAddress,
                            u.MobileNo,
                            u.CreatedBy,
                            u.CreatedOn,
                            u.ModifiedBy,
                            u.ModifiedOn
                        });

                    return Json(await DataSourceLoader.LoadAsync(userDetails, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetUserDetails: {ex.Message}");
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "An error occurred while fetching the user details.",
                        error = ex.Message
                    });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }


        [HttpGet]
        public async Task<IActionResult> GetDistinctModules(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var modules = dbContext.TblUserAccesses
                .Select(x => x.Module)
                .Distinct()
                .Select(x => new { Module = x });

                return Json(await DataSourceLoader.LoadAsync(modules, loadOptions));
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet]
        public async Task<IActionResult> GetDistinctAccessPrefixes(DataSourceLoadOptions loadOptions, string module)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                if (string.IsNullOrEmpty(module))
                    return Json(DataSourceLoader.Load(Enumerable.Empty<object>().AsQueryable(), loadOptions));

                var rawData = await dbContext.TblUserAccesses
                    .Where(u => u.ItemName.Contains("_") && u.Module == module)
                    .Select(u => new { u.ItemName, u.Module })
                    .ToListAsync();

                var grouped = rawData
                    .Where(x => x.ItemName.Contains("_"))
                    .GroupBy(x => x.ItemName.Substring(0, x.ItemName.IndexOf("_")))
                    .Select(g => new
                    {
                        Prefix = g.Key,
                        Module = g.First().Module
                    });

                return Json(DataSourceLoader.Load(grouped.AsQueryable(), loadOptions));
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet]
        public async Task<IActionResult> Getuserdetails(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var user = dbContext.TblUserMasters
                        .Select(u => new
                        {
                            u.UserId,
                            u.UserName
                        });

                    return Json(await DataSourceLoader.LoadAsync(user, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in Getuserdetails: {ex.Message}");
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "An error occurred while fetching the user details.",
                        error = ex.Message
                    });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetUserAccessDetails(DataSourceLoadOptions loadOptions, int userId, string module, string prefix)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var userAccessDetails = dbContext.TblUserAccesses
                        .Where(u => u.UserId == userId && u.Module == module && (string.IsNullOrEmpty(prefix) || u.ItemName.StartsWith(prefix + "_")))
                        .Select(u => new
                        {
                            u.ItemName,
                            u.ItemEnabled,
                            u.ItemVisible,
                            u.SlNo,
                            Prefix = u.ItemName.Contains("_") ? u.ItemName.Substring(0, u.ItemName.IndexOf("_")) : u.ItemName,
                            ActionName = u.ItemName.Contains("_") ? u.ItemName.Substring(u.ItemName.IndexOf("_") + 1) : null
                        });

                    return Json(await DataSourceLoader.LoadAsync(userAccessDetails, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetUserAccessDetails: {ex.Message}");
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "An error occurred while fetching the user access details.",
                        error = ex.Message
                    });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateItemVisible([FromBody] List<ItemVisibilityUpdate> items)
        {
            _logger.LogInformation($"Received request to update visibility for items: {string.Join(", ", items.Select(i => $"SlNo={i.SlNo}, ItemVisible={i.ItemVisible}"))}");

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    foreach (var item in items)
                    {
                        var userAccess = await dbContext.TblUserAccesses.FirstOrDefaultAsync(u => u.SlNo == item.SlNo);

                        if (userAccess == null)
                        {
                            _logger.LogWarning($"User access with SlNo {item.SlNo} not found.");
                            return NotFound(new { success = false, message = $"User access with SlNo {item.SlNo} not found." });
                        }

                        _logger.LogInformation($"Current ItemVisible for SlNo {item.SlNo}: {userAccess.ItemVisible}");
                        userAccess.ItemVisible = item.ItemVisible;
                        _logger.LogInformation($"Updated ItemVisible for SlNo {item.SlNo} to: {item.ItemVisible}");

                        dbContext.TblUserAccesses.Update(userAccess);
                    }

                    await dbContext.SaveChangesAsync();
                    _logger.LogInformation("Item visibility updated successfully for all items.");
                    return Ok(new { success = true, message = "Item visibility updated successfully for all items." });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in UpdateItemVisible: {ex.Message}");
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "An error occurred while updating the item visibility.",
                        error = ex.Message
                    });
                }
            }

            _logger.LogWarning("Invalid tenant.");
            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        public class ItemVisibilityUpdate
        {
            public int SlNo { get; set; }
            public bool ItemVisible { get; set; }
        }

    }
}
