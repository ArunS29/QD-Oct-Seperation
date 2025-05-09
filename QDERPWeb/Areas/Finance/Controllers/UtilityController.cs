using DevExpress.Internal;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Bcpg;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System.Data;
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
        [HttpPost]
        public IActionResult GrantUserAccessPermissions([FromBody] int userId)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    // Execute the stored procedure with the userId parameter
                    dbContext.Database.ExecuteSqlRaw("EXEC stPro901_03InsertUserAccessPermissionsforweb @ToUser = {0}", userId);

                    return Ok(new { success = true, message = "User access permissions granted successfully." });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GrantUserAccessPermissions: {ex.Message}");
                    return StatusCode(500, new { success = false, message = "An error occurred.", error = ex.Message });
                }
            }

            return Unauthorized(new { success = false, message = "Invalid tenant." });
        }



        [HttpGet]
        public async Task<IActionResult> GetUserAccessWebs(DataSourceLoadOptions loadOptions, int userId)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    // Filter the data based on the userId
                    var accessData = dbContext.TblUserAccessWebs.Where(x => x.UserId == userId);
                    return Json(await DataSourceLoader.LoadAsync(accessData, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetUserAccessWebs: {ex.Message}");
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "An error occurred while fetching the user access records.",
                        error = ex.Message
                    });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }


        //[HttpPost]
        //public async Task<IActionResult> UpdateUserAccessWeb(int key, [FromBody] Dictionary<string, object> values)
        //{
        //    if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
        //    {
        //        try
        //        {
        //            var accessRecord = await dbContext.TblUserAccessWebs.FindAsync(key);
        //            if (accessRecord == null)
        //            {
        //                return NotFound();
        //            }

        //            // Log incoming values for debugging
        //            _logger.LogInformation("UpdateUserAccessWeb received values: " + System.Text.Json.JsonSerializer.Serialize(values));

        //            // Apply changes using reflection
        //            foreach (var item in values)
        //            {
        //                var property = typeof(TblUserAccessWeb).GetProperty(item.Key);
        //                if (property != null && property.CanWrite)
        //                {
        //                    try
        //                    {
        //                        var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

        //                        object safeValue;

        //                        if (targetType == typeof(bool))
        //                        {
        //                            // Handle different types of boolean representations
        //                            if (item.Value is string strVal)
        //                            {
        //                                safeValue = bool.Parse(strVal);
        //                            }
        //                            else
        //                            {
        //                                safeValue = Convert.ToBoolean(item.Value);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            safeValue = Convert.ChangeType(item.Value, targetType);
        //                        }

        //                        property.SetValue(accessRecord, safeValue);
        //                    }
        //                    catch (Exception e)
        //                    {
        //                        _logger.LogWarning($"Failed to update property {item.Key}: {e.Message}");
        //                    }
        //                }
        //            }

        //            await dbContext.SaveChangesAsync();
        //            return Ok();
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError($"Error updating user access: {ex.Message}");
        //            return StatusCode(500, new { message = "Error updating record.", error = ex.Message });
        //        }
        //    }

        //    return Unauthorized(new { message = "Invalid tenant." });
        //}
        [HttpPost]
        public async Task<IActionResult> UpdateUserAccessWeb([FromBody] List<ItemVisibilityUpdate> items)
        {
            _logger.LogInformation($"Received request to update visibility for items: {string.Join(", ", items.Select(i => $"SlNo={i.SlNo}, ItemVisible={i.ItemVisible}"))}");

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    foreach (var item in items)
                    {
                        var userAccess = await dbContext.TblUserAccessWebs.FirstOrDefaultAsync(u => u.SlNo == item.SlNo);

                        if (userAccess == null)
                        {
                            _logger.LogWarning($"User access with SlNo {item.SlNo} not found.");
                            return NotFound(new { success = false, message = $"User access with SlNo {item.SlNo} not found." });
                        }

                        _logger.LogInformation($"Current ItemVisible for SlNo {item.SlNo}: {userAccess.ItemVisible}");
                        userAccess.ItemVisible = item.ItemVisible;
                        _logger.LogInformation($"Updated ItemVisible for SlNo {item.SlNo} to: {item.ItemVisible}");

                        dbContext.TblUserAccessWebs.Update(userAccess);
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

        //user info
        [HttpGet]
        public async Task<IActionResult> GetUsers(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var usersQuery = dbContext.TblUserMasters
                        .Select(u => new
                        {
                            u.UserId,
                            u.UserName,
                            u.Password,
                            u.EmailAddress,
                            u.MobileNo,
                            u.LastLogOnTime,
                            u.LastLogOffTime,
                            u.DeptCode,
                            u.CompanyId,
                            u.BranchCode,
                            u.UserLevel,
                            u.HrlevelCode,
                            u.InventoryAccess,
                            u.PettyCashAccount,
                            u.EqptQuotationAccess,
                            u.InventoryMpraccess,
                            u.HrtimeSheetProjectGroup
                        });

                    var result = await DataSourceLoader.LoadAsync(usersQuery, loadOptions);
                    return Json(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetUsers: {ex.Message}");
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "An error occurred while fetching users.",
                        error = ex.Message
                    });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        public async Task<IActionResult> GetUserID(byte userId)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var user = await (from u in dbContext.TblUserMasters
                                      where u.UserId == userId

                                      join c in dbContext.Tbl901CompanyDetails
                                      on u.CompanyId equals c.CompanyId into userCompany
                                      from uc in userCompany.DefaultIfEmpty()

                                      join b in dbContext.Tbl20115CompanyBranches
                                      on u.BranchCode.ToString() equals b.BranchCode into userBranch
                                      from ub in userBranch.DefaultIfEmpty()

                                      join ul in dbContext.Tbl901UserLevelMasters
                                      on u.UserLevel equals ul.UserLevelId into UserLevelDesc
                                      from ulm in UserLevelDesc.DefaultIfEmpty()

                                      join hr in dbContext.Tbl901HruserLevelMasters
                                      on u.HrlevelCode equals hr.HruserLevelId into HruserLevelDesc
                                      from hrm in HruserLevelDesc.DefaultIfEmpty()

                                      join a in dbContext.Qry201ListOfAccounts
                                      on u.PettyCashAccount equals a.AccountId into AccountHead
                                      from aj in AccountHead.DefaultIfEmpty()

                                      join sp in dbContext.Tbl901SalesPersonAccessLevelMasters
                                      on u.InventoryMpraccess equals sp.SalesPersonAccessLevelId into salesPersonJoin
                                      from spm in salesPersonJoin.DefaultIfEmpty()

                                      select new
                                      {
                                          u.UserId,
                                          u.UserName,
                                          u.Password,
                                          u.EmailAddress,
                                          u.MobileNo,
                                          u.LastLogOnTime,
                                          u.LastLogOffTime,
                                          u.DeptCode,
                                          u.CompanyId,
                                          CompanyName = uc.CompanyName,
                                          u.BranchCode,
                                          BranchName = ub.BranchName,
                                          u.UserLevel,
                                          UserLevelDesc = ulm.UserLevelDesc,
                                          u.HrlevelCode,
                                          HruserLevelDesc = hrm.HruserLevelDesc,
                                          u.InventoryAccess,
                                          u.PettyCashAccount,
                                          AccountHead = aj.AccountHead,
                                          u.EqptQuotationAccess,
                                          u.InventoryMpraccess,
                                          InventoryMprAccessDesc = spm.SalesPersonAccessDesc,
                                          u.HrtimeSheetProjectGroup
                                      }).FirstOrDefaultAsync();

                    if (user == null)
                    {
                        return NotFound(new { success = false, message = "User not found." });
                    }

                    return Json(new { success = true, data = user });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetUserID: {ex.Message}");
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "An error occurred while fetching the user.",
                        error = ex.Message
                    });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }




        //krish

        [HttpGet]
        public async Task<IActionResult> GetUserID_Test(byte userId)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var user = await dbContext.TblUserMasters
                        .Where(u => u.UserId == userId)
                        .Select(u => new
                        {
                            u.UserId,
                            u.UserName,
                            u.Password,
                            u.EmailAddress,
                            u.MobileNo,
                            u.LastLogOnTime,
                            u.LastLogOffTime,
                            u.DeptCode,
                            u.CompanyId,
                            u.BranchCode,
                            u.UserLevel,
                            u.HrlevelCode,
                            u.InventoryAccess,
                            u.PettyCashAccount,
                            u.EqptQuotationAccess,
                            u.InventoryMpraccess,
                            u.HrtimeSheetProjectGroup
                        })
                        .FirstOrDefaultAsync();

                    if (user == null)
                    {
                        return NotFound(new { success = false, message = "User not found." });
                    }

                    return Json(new { success = true, data = user });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetUserID: {ex.Message}");
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "An error occurred while fetching the user.",
                        error = ex.Message
                    });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }


        //krish

        [HttpGet]
        public IActionResult GetDepartments(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var departments = dbContext.Tbl101Departments
                        .Select(d => new {
                            d.DepartmentCode,
                            d.Department
                        })
                        .ToList();

                    departments.Add(new
                    {
                        DepartmentCode = (short)99,
                        Department = "<All Departments>"
                    });
                    return Json(DataSourceLoader.Load(departments, loadOptions));
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetCompanies(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    // Get company list from DB
                    var companyList = await dbContext.Tbl901CompanyDetails
                        .Select(c => new
                        {
                            CompanyID = c.CompanyId.ToString(), // Ensure all are strings for union
                            CompanyName = c.CompanyName
                        }).ToListAsync();

                    // Add "All Companies" option at the beginning
                    companyList.Insert(0, new
                    {
                        CompanyID = "99",
                        CompanyName = "<All Companies>"
                    });

                    return Json(DataSourceLoader.Load(companyList, loadOptions));
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetBranches(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    // Get actual branches from the database
                    var branchList = await dbContext.Tbl20115CompanyBranches
                        .Select(b => new
                        {
                            BranchCode = b.BranchCode.ToString(), // Ensure consistent string type
                            BranchName = b.BranchName,
                            BranchNameAr = b.BranchNameAr
                        })
                        .ToListAsync();

                    // Insert <All Divisions> option
                    branchList.Insert(0, new
                    {
                        BranchCode = "99",
                        BranchName = "<All Divisions>",
                        BranchNameAr = ""
                    });

                    return Json(DataSourceLoader.Load(branchList, loadOptions));
                }
                catch (Exception ex)
                {
                    // Consider logging the exception
                    return StatusCode(500, new { message = "Error loading branches.", success = false });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetUserLevels(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    // Fetch user levels from the database
                    var userLevelList = await dbContext.Tbl901UserLevelMasters
                        .Select(ul => new
                        {
                            UserLevelID = ul.UserLevelId.ToString(),
                            UserLevelDesc = ul.UserLevelDesc
                        })
                        .ToListAsync();

                    // Insert <All User Levels> option
                    userLevelList.Insert(0, new
                    {
                        UserLevelID = "99",
                        UserLevelDesc = "<All User Levels>"
                    });

                    return Json(DataSourceLoader.Load(userLevelList, loadOptions));
                }
                catch (Exception ex)
                {
                    // Log the exception (consider using a logging framework)
                    // _logger.LogError(ex, "Error loading user levels.");

                    return StatusCode(500, new { message = "Error loading user levels.", success = false });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployeeGroups(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    // Fetch employee groups from the database
                    var employeeGroupList = await dbContext.Tbl901HruserLevelMasters
                        .Select(eg => new
                        {
                            EmpGroupID = eg.HruserLevelId.ToString(),
                            EmployeeGroup = eg.HruserLevelDesc
                        })
                        .ToListAsync();

                    // Optionally insert a <All Groups> option
                    employeeGroupList.Insert(0, new
                    {
                        EmpGroupID = "99",
                        EmployeeGroup = "<All Employee Groups>"
                    });

                    return Json(DataSourceLoader.Load(employeeGroupList, loadOptions));
                }
                catch (Exception ex)
                {
                    // _logger.LogError(ex, "Error loading employee groups.");
                    return StatusCode(500, new { message = "Error loading employee groups.", success = false });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetInventoryGroups(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var inventoryGroupList = await dbContext.Tbl60008inventoryMasterGroups
                        .Select(ig => new InventoryGroupDto
                        {
                            InventoryMasterGroupID = ig.InventoryMasterGroupId,
                            InventoryMasterGroup = ig.InventoryMasterGroup
                        })
                        .ToListAsync();

                    // Add the "<Full Inventory Access>" option
                    inventoryGroupList.Add(new InventoryGroupDto
                    {
                        InventoryMasterGroupID = 99,
                        InventoryMasterGroup = "<Full Inventory Access>"
                    });

                    return Json(DataSourceLoader.Load(inventoryGroupList, loadOptions));
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "Error loading inventory groups.", success = false });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetFilteredAccountHead(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var filteredAccounts = dbContext.Qry201ListOfAccounts
                        .Where(i => i.AccountGroupId == "A012")
                        .Select(i => new
                        {
                            i.AccountId,
                            i.AccountHead,
                            i.AccountGroup,
                            i.AccountGroupId,
                            i.AccountHeadArabic,
                            i.ReferenceNo,
                            i.IsRestricted,
                            i.MasterGroup,
                            i.MasterGroupId,
                            i.IsLedgerObselete
                        });

                    return Json(await DataSourceLoader.LoadAsync(filteredAccounts, loadOptions));
                }
                catch (Exception ex)
                {
                    // Log error if logging is implemented
                    throw ex;
                }
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetHRUserLevels(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var query = dbContext.Tbl901HruserLevelMasters
                        .Select(x => new
                        {
                            x.HruserLevelId,
                            x.HruserLevelDesc
                        });

                    return Json(await DataSourceLoader.LoadAsync(query, loadOptions));
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetSalesPersonAccessLevels(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var query = dbContext.Tbl901SalesPersonAccessLevelMasters.Select(i => new
                    {
                        i.SalesPersonAccessLevelId,
                        i.SalesPersonAccessDesc
                    });

                    return Json(await DataSourceLoader.LoadAsync(query, loadOptions));
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public IActionResult GetProjectGroups(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var data = dbContext.Tbl101ProjectsGroups
                        .Select(pg => new ProjectGroupDto
                        {
                            ProjectGroupID = pg.ProjectGroupId,
                            ProjectGroupName = pg.ProjectGroupName
                        }).ToList();

                    // Add the custom "All Project Groups" option
                    data.Add(new ProjectGroupDto
                    {
                        ProjectGroupID = 99,
                        ProjectGroupName = "<All Project Groups>"
                    });

                    // Use Load (not LoadAsync) because data is in-memory (List)
                    return Json(DataSourceLoader.Load(data, loadOptions));
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }



        //Svae funcanality

        [HttpPost]
        public async Task<IActionResult> SaveUserDetails([FromBody] TblUserMaster user)
        {
            if (user == null)
                return BadRequest(new { success = false, message = "Invalid user data." });

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    // Check if user already exists within the current tenant
                    var existingUser = await dbContext.TblUserMasters
                        .FirstOrDefaultAsync(u => u.UserId == user.UserId);

                    if (existingUser != null)
                    {
                        // Update existing user
                        existingUser.UserName = user.UserName;
                        existingUser.Password = user.Password;
                        existingUser.DeptCode = user.DeptCode;
                        existingUser.CompanyId = user.CompanyId;
                        existingUser.BranchCode = user.BranchCode;
                        existingUser.UserLevel = user.UserLevel;
                        existingUser.HrlevelCode = user.HrlevelCode;
                        existingUser.InventoryAccess = user.InventoryAccess;
                        existingUser.PettyCashAccount = user.PettyCashAccount;
                        existingUser.EqptQuotationAccess = user.EqptQuotationAccess;
                        existingUser.InventoryMpraccess = user.InventoryMpraccess;
                        existingUser.HrtimeSheetProjectGroup = user.HrtimeSheetProjectGroup;
                        existingUser.MobileNo = user.MobileNo;
                        existingUser.EmailAddress = user.EmailAddress;
                        existingUser.LastLogOnTime = user.LastLogOnTime;
                        existingUser.LastLogOffTime = user.LastLogOffTime;
                    }
                    else
                    {
                        // Insert new user
                        var newUser = new TblUserMaster
                        {
                            //  TenantId = tenant.TenantId,
                            UserId = user.UserId,
                            UserName = user.UserName,
                            Password = user.Password,
                            //  Department = user.Department,
                            CompanyId = user.CompanyId,
                            BranchCode = user.BranchCode,
                            //   Division = user.Division,
                            UserLevel = user.UserLevel,
                            //   AccessGroup = user.AccessGroup,
                            InventoryAccess = user.InventoryAccess,
                            PettyCashAccount = user.PettyCashAccount,
                            //  EquipmentSalesPerson = user.EquipmentSalesPerson,
                            // InventorySalesPerson = user.InventorySalesPerson,
                            // HRTimesheetGroup = user.HRTimesheetGroup,
                            MobileNo = user.MobileNo,
                            EmailAddress = user.EmailAddress,
                            LastLogOnTime = user.LastLogOnTime,
                            LastLogOffTime = user.LastLogOffTime
                        };

                        await dbContext.TblUserMasters.AddAsync(newUser);
                    }

                    await dbContext.SaveChangesAsync();
                    return Ok(new { success = true, message = "User saved successfully." });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { success = false, message = "Error: " + ex.Message });
                }
            }

            return Unauthorized(new { success = false, message = "Invalid tenant context." });
        }




        public class ProjectGroupDto
        {
            public int ProjectGroupID { get; set; }
            public string ProjectGroupName { get; set; }
        }
        public class InventoryGroupDto
        {
            public int InventoryMasterGroupID { get; set; }
            public string InventoryMasterGroup { get; set; }
        }

        public class ItemVisibilityUpdate
        {
            public int SlNo { get; set; }
            public bool ItemVisible { get; set; }

        }

    }
}
