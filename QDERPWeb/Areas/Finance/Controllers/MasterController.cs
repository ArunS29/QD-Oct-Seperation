using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Area("Finance")]
    [Route("api/[controller]")]
    [ApiController]
    public class MasterController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<MasterController> _logger;

        public MasterController(ILogger<MasterController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpGet("GetUserddl")]
        public async Task<ActionResult> GetUserddl(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var users = dbContext.TblUserMasters.Select(u => new { u.UserId, u.UserName });
                    return Json(await DataSourceLoader.LoadAsync(users, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetUserddl: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet("GetBranches")]
        public async Task<IActionResult> GetBranches()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var branches = await dbContext.Tbl20115CompanyBranches.ToListAsync();
                    return Ok(branches);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetBranches: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost("AddBranch")]
        public async Task<IActionResult> AddBranch([FromBody] Tbl20115CompanyBranch branch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var exists = await dbContext.Tbl20115CompanyBranches
                        .AnyAsync(x => x.BranchName == branch.BranchName);
                    if (exists)
                    {
                        return BadRequest(new { success = false, message = "Branch Name English already exists." });
                    }
                    var maxBranchCode = dbContext.Tbl20115CompanyBranches
                        .AsEnumerable() // Switch to in-memory processing
                        .Select(b => int.TryParse(b.BranchCode, out int code) ? code : 0) // Convert BranchCode to integer
                        .OrderByDescending(code => code) // Order by the integer value
                        .FirstOrDefault(); // Get the maximum value

                    int newBranchCode = maxBranchCode + 1;
                    branch.BranchCode = newBranchCode.ToString();

                    dbContext.Tbl20115CompanyBranches.Add(branch);
                    await dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Branch added successfully." });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in AddBranch: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost("UpdateBranch")]
        public async Task<IActionResult> UpdateBranch([FromBody] Tbl20115CompanyBranch branch)
        {
            if (branch == null || string.IsNullOrEmpty(branch.BranchCode))
            {
                return BadRequest("Invalid branch data.");
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var existingBranch = await dbContext.Tbl20115CompanyBranches
                        .FirstOrDefaultAsync(b => b.BranchCode == branch.BranchCode);

                    if (existingBranch == null)
                    {
                        return NotFound(new { success = false, message = $"Branch with code {branch.BranchCode} not found." });
                    }

                    var exists = await dbContext.Tbl20115CompanyBranches
                        .AnyAsync(b => b.BranchName == branch.BranchName && b.BranchCode != branch.BranchCode);
                    if (exists)
                    {
                        return BadRequest(new { success = false, message = "Branch Name already exists." });
                    }

                    if (!string.IsNullOrEmpty(branch.BranchName))
                    {
                        existingBranch.BranchName = branch.BranchName;
                    }

                    if (!string.IsNullOrEmpty(branch.BranchNameAr))
                    {
                        existingBranch.BranchNameAr = branch.BranchNameAr;
                    }

                    dbContext.Entry(existingBranch).State = EntityState.Modified;
                    await dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Branch updated successfully." });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in UpdateBranch: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost("DeleteBranchMaster")]
        public async Task<IActionResult> DeleteBranchMaster([FromBody] Tbl20115CompanyBranch branch)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var BranchToDelete = await dbContext.Tbl20115CompanyBranches.FindAsync(branch.BranchCode);
                if (BranchToDelete == null)
                {
                    return NotFound();
                }

                dbContext.Tbl20115CompanyBranches.Remove(BranchToDelete);
                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Branch deleted successfully." });
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet("GetAssetMaintanencetype")]
        public async Task<IActionResult> GetAssetMaintanencetype()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var assetMaintenanceTypes = await dbContext.Tbl20112AssetMaintenanceTypes.ToListAsync();
                    return Ok(assetMaintenanceTypes);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetAssetMaintanencetype: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost("AddAssetMaintanencetype")]
        public async Task<IActionResult> AddAssetMaintanencetype([FromBody] Tbl20112AssetMaintenanceType AssetmaintenanceData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var exists = await dbContext.Tbl20112AssetMaintenanceTypes
                        .AnyAsync(x => x.AssetMaintenanceType == AssetmaintenanceData.AssetMaintenanceType);
                    if (exists)
                    {
                        return BadRequest(new { success = false, message = "Asset maintenance type already exists." });
                    }

                    var maxId = await dbContext.Tbl20112AssetMaintenanceTypes
                        .OrderByDescending(x => x.AssetMaintenanceTypeId)
                        .Select(x => x.AssetMaintenanceTypeId)
                        .FirstOrDefaultAsync();

                    int newAssetMaintenanceTypeId = maxId + 1;
                    AssetmaintenanceData.AssetMaintenanceTypeId = (byte)newAssetMaintenanceTypeId;

                    dbContext.Tbl20112AssetMaintenanceTypes.Add(AssetmaintenanceData);
                    await dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Asset maintenance type added successfully." });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in AddAssetMaintanencetype: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost("UpdateAssetMaintanencetype")]
        public async Task<IActionResult> UpdateAssetMaintanencetype([FromBody] Tbl20112AssetMaintenanceType AssetmaintenanceData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var exists = await dbContext.Tbl20112AssetMaintenanceTypes
                        .AnyAsync(x => x.AssetMaintenanceType == AssetmaintenanceData.AssetMaintenanceType
                                       && x.AssetMaintenanceTypeId != AssetmaintenanceData.AssetMaintenanceTypeId);
                    if (exists)
                    {
                        return BadRequest(new { success = false, message = "Asset maintenance type already exists." });
                    }

                    dbContext.Tbl20112AssetMaintenanceTypes.Update(AssetmaintenanceData);
                    await dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Asset maintenance type updated successfully." });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in UpdateAssetMaintanencetype: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost("DeleteAssetMaintanencetype")]
        public async Task<IActionResult> DeleteAssetMaintanencetype([FromBody] Tbl20112AssetMaintenanceType AssetmaintenanceData)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var assetToDelete = await dbContext.Tbl20112AssetMaintenanceTypes.FindAsync(AssetmaintenanceData.AssetMaintenanceTypeId);
                if (assetToDelete == null)
                {
                    return NotFound();
                }

                dbContext.Tbl20112AssetMaintenanceTypes.Remove(assetToDelete);
                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Asset maintenance type deleted successfully." });
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet("GetAssetCategory")]
        public async Task<IActionResult> GetAssetCategory()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var assetCategories = await dbContext.Tbl20106AssetCategories
                    .Select(x => new
                    {
                        x.AssetCategoryCode,
                        x.AssetCategory
                    })
                    .ToListAsync();
                return Ok(assetCategories);
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost("AddAssetCategory")]
        public async Task<IActionResult> AddAssetCategory([FromBody] Tbl20106AssetCategory assetCategory)
        {
            if (assetCategory == null)
            {
                return BadRequest("Invalid data.");
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var normalizedAssetCategory = assetCategory.AssetCategory.Trim().ToLower();

                    var exists = await dbContext.Tbl20106AssetCategories
                        .AnyAsync(x => x.AssetCategory.Trim().ToLower() == normalizedAssetCategory);

                    if (exists)
                    {
                        return BadRequest(new { success = false, message = "Asset Category already exists." });
                    }

                    var maxId = await dbContext.Tbl20106AssetCategories
                        .OrderByDescending(a => a.AssetCategoryCode)
                        .Select(a => a.AssetCategoryCode)
                        .FirstOrDefaultAsync();

                    assetCategory.AssetCategoryCode = (byte)(maxId == 0 ? 1 : maxId + 1);

                    dbContext.Tbl20106AssetCategories.Add(assetCategory);
                    await dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Asset category added successfully." });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in AddAssetCategory: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost("UpdateAssetCategory")]
        public async Task<IActionResult> UpdateAssetCategory([FromBody] Tbl20106AssetCategory assetCategory)
        {
            if (assetCategory == null || assetCategory.AssetCategoryCode == 0)
            {
                return BadRequest("Invalid asset category data.");
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var normalizedAssetCategory = assetCategory.AssetCategory.Trim().ToLower();

                    var exists = await dbContext.Tbl20106AssetCategories
                        .AnyAsync(x => x.AssetCategory.Trim().ToLower() == normalizedAssetCategory && x.AssetCategoryCode != assetCategory.AssetCategoryCode);

                    if (exists)
                    {
                        return BadRequest(new { success = false, message = "Asset Category already exists." });
                    }

                    var existingAssetCategory = await dbContext.Tbl20106AssetCategories
                        .FirstOrDefaultAsync(a => a.AssetCategoryCode == assetCategory.AssetCategoryCode);

                    if (existingAssetCategory == null)
                    {
                        return NotFound(new { success = false, message = "Asset category not found." });
                    }

                    existingAssetCategory.AssetCategory = assetCategory.AssetCategory;
                    existingAssetCategory.DepreciationLedgerNo = assetCategory.DepreciationLedgerNo;
                    existingAssetCategory.AccumDepLedgerNo = assetCategory.AccumDepLedgerNo;

                    dbContext.Entry(existingAssetCategory).State = EntityState.Modified;
                    await dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Asset category updated successfully." });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in UpdateAssetCategory: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet("Getassetlocation")]
        public IActionResult Getassetlocation()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var assetLocations = dbContext.Tbl20107AssetLocations.ToList();
                return Ok(assetLocations);
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost("Addassetlocation")]
        public IActionResult Add([FromBody] Tbl20107AssetLocation assetLocation)
        {
            if (assetLocation == null || string.IsNullOrWhiteSpace(assetLocation.AssetLocation))
            {
                return BadRequest("Asset Location is required.");
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var existingAssetLocation = dbContext.Tbl20107AssetLocations
                        .FirstOrDefault(x => x.AssetLocation == assetLocation.AssetLocation);

                    if (existingAssetLocation != null)
                    {
                        return Conflict("Asset Location already exists.");
                    }

                    var maxAssetLocationCode = dbContext.Tbl20107AssetLocations
                        .OrderByDescending(x => x.AssetLocationCode)
                        .FirstOrDefault()?.AssetLocationCode ?? 0;

                    assetLocation.AssetLocationCode = (short)(maxAssetLocationCode + 1);

                    dbContext.Tbl20107AssetLocations.Add(assetLocation);
                    dbContext.SaveChanges();

                    return Ok(new { message = "Asset Location added successfully." });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in Addassetlocation: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost("UpdateAssetLocation")]
        public IActionResult Update([FromBody] Tbl20107AssetLocation assetLocation)
        {
            if (assetLocation == null || string.IsNullOrWhiteSpace(assetLocation.AssetLocation))
            {
                return BadRequest("Asset Location is required.");
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var existingAssetLocation = dbContext.Tbl20107AssetLocations
                    .FirstOrDefault(x => x.AssetLocation == assetLocation.AssetLocation && x.AssetLocationCode != assetLocation.AssetLocationCode);

                if (existingAssetLocation != null)
                {
                    return BadRequest("Asset Location already exists.");
                }

                var assetLocationToUpdate = dbContext.Tbl20107AssetLocations
                    .FirstOrDefault(x => x.AssetLocationCode == assetLocation.AssetLocationCode);

                if (assetLocationToUpdate == null)
                {
                    return NotFound("Asset Location not found.");
                }

                assetLocationToUpdate.AssetLocation = assetLocation.AssetLocation;
                dbContext.SaveChanges();

                return Ok(new { message = "Asset Location updated successfully." });
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpPost("DeleteAssetLocation")]
        public async Task<IActionResult> DeleteAssetLocation([FromBody] Tbl20107AssetLocation branch)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var BranchToDelete = await dbContext.Tbl20107AssetLocations.FindAsync(branch.AssetLocationCode);
                if (BranchToDelete == null)
                {
                    return NotFound();
                }

                dbContext.Tbl20107AssetLocations.Remove(BranchToDelete);
                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "AssetLocation deleted successfully." });
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet("GetSalesPersons")]
        public async Task<IActionResult> GetSalesPersons()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var salesPersons = await dbContext.Tbl20101SalesPersonMasters.ToListAsync();
                    return Ok(salesPersons);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetSalesPersons: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost("AddSalesPerson")]
        public async Task<IActionResult> AddSalesPerson([FromBody] Tbl20101SalesPersonMaster salesPerson)
        {
            if (!ModelState.IsValid)
            {
                var validationErrors = string.Join(", ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                return BadRequest($"Validation failed: {validationErrors}");
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var existingSalesPerson = dbContext.Tbl20101SalesPersonMasters
                        .FirstOrDefault(x => x.SalesPersonCode == salesPerson.SalesPersonCode);

                    if (existingSalesPerson != null)
                    {
                        return Conflict("SalesPerson/ Project Manager Code already exists.");
                    }

                    dbContext.Tbl20101SalesPersonMasters.Add(salesPerson);
                    await dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Salesperson added successfully." });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in AddSalesPerson: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost("UpdateSalesPerson")]
        public async Task<IActionResult> UpdateSalesPerson([FromBody] Tbl20101SalesPersonMaster salesPerson)
        {
            if (salesPerson == null || string.IsNullOrEmpty(salesPerson.SalesPersonCode))
            {
                return BadRequest("Invalid salesperson data.");
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var existingSalesPerson = await dbContext.Tbl20101SalesPersonMasters
                        .FirstOrDefaultAsync(s => s.SalesPersonCode == salesPerson.SalesPersonCode);

                    if (existingSalesPerson == null)
                    {
                        return NotFound(new { success = false, message = $"Salesperson with code {salesPerson.SalesPersonCode} not found." });
                    }

                    if (salesPerson.UserCode.HasValue)
                    {
                        var userCodeExists = await dbContext.Tbl20101SalesPersonMasters
                            .AnyAsync(s => s.UserCode == salesPerson.UserCode && s.SalesPersonCode != salesPerson.SalesPersonCode);

                        if (!userCodeExists)
                        {
                            return BadRequest(new { success = false, message = $"The provided UserCode {salesPerson.UserCode} does not exist for any other salesperson." });
                        }

                        if (salesPerson.UserCode < 0 || salesPerson.UserCode > 255)
                        {
                            return BadRequest(new { success = false, message = "UserCode must be between 0 and 255." });
                        }
                    }

                    if (!string.IsNullOrEmpty(salesPerson.SalesPersonName))
                        existingSalesPerson.SalesPersonName = salesPerson.SalesPersonName;
                    if (salesPerson.UserCode.HasValue)
                        existingSalesPerson.UserCode = salesPerson.UserCode.Value;
                    if (!string.IsNullOrEmpty(salesPerson.EmailAddress))
                        existingSalesPerson.EmailAddress = salesPerson.EmailAddress;
                    if (!string.IsNullOrEmpty(salesPerson.SalesPersonContactNo))
                        existingSalesPerson.SalesPersonContactNo = salesPerson.SalesPersonContactNo;

                    dbContext.Entry(existingSalesPerson).State = EntityState.Modified;
                    await dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Salesperson updated successfully." });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in UpdateSalesPerson: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpDelete("DeleteSalesPerson/{salesPersonCode}")]
        public async Task<IActionResult> DeleteSalesPerson(string salesPersonCode)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var salesPersonToDelete = await dbContext.Tbl20101SalesPersonMasters.FindAsync(salesPersonCode);
                if (salesPersonToDelete == null)
                {
                    return NotFound(new { success = false, message = "Salesperson not found." });
                }

                dbContext.Tbl20101SalesPersonMasters.Remove(salesPersonToDelete);
                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Salesperson deleted successfully." });
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }


        [HttpGet("GetLedgerSubGroups")]
        public IActionResult GetLedgerSubGroups()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var subGroups = dbContext.Tbl20123LedgerSubGroups.ToList();
                return Ok(subGroups);
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost("AddLedgerSubGroup")]
        public IActionResult AddLedgerSubGroup([FromBody] Tbl20123LedgerSubGroup subGroup)
        {
            if (subGroup == null)
            {
                return BadRequest("Invalid data. The subGroup is null.");
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var maxLedgerSubGroupCode = dbContext.Tbl20123LedgerSubGroups
                        .OrderByDescending(sg => sg.LedgerSubGroupCode)
                        .Select(sg => sg.LedgerSubGroupCode)
                        .FirstOrDefault();

                    string codePrefix = "A011-";
                    string newLedgerSubGroupCode;

                    if (string.IsNullOrEmpty(maxLedgerSubGroupCode))
                    {
                        newLedgerSubGroupCode = codePrefix + "001";
                    }
                    else
                    {
                        var parts = maxLedgerSubGroupCode.Split('-');

                        if (parts.Length == 2 && int.TryParse(parts[1], out int currentNumber))
                        {
                            currentNumber++;
                            newLedgerSubGroupCode = codePrefix + currentNumber.ToString("D3");
                        }
                        else
                        {
                            return BadRequest("Invalid format for LedgerSubGroupCode.");
                        }
                    }

                    subGroup.LedgerSubGroupCode = newLedgerSubGroupCode;

                    dbContext.Tbl20123LedgerSubGroups.Add(subGroup);
                    dbContext.SaveChanges();

                    return Ok(new { success = true, message = "Ledger SubGroup added successfully." });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in AddLedgerSubGroup: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost("UpdateLedgerSubGroup")]
        public async Task<IActionResult> UpdateLedgerSubGroup([FromBody] Tbl20123LedgerSubGroup subGroup)
        {
            if (subGroup == null || string.IsNullOrEmpty(subGroup.LedgerSubGroupCode))
            {
                return BadRequest("Invalid sub-group data.");
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var existingSubGroup = await dbContext.Tbl20123LedgerSubGroups
                        .FirstOrDefaultAsync(sg => sg.LedgerSubGroupCode == subGroup.LedgerSubGroupCode);

                    if (existingSubGroup == null)
                    {
                        return NotFound(new { success = false, message = $"Sub-group with code {subGroup.LedgerSubGroupCode} not found." });
                    }

                    if (!string.IsNullOrEmpty(subGroup.SubGroupName))
                    {
                        existingSubGroup.SubGroupName = subGroup.SubGroupName;
                    }

                    if (!string.IsNullOrEmpty(subGroup.SubGroupNameAr))
                    {
                        existingSubGroup.SubGroupNameAr = subGroup.SubGroupNameAr;
                    }

                    dbContext.Entry(existingSubGroup).State = EntityState.Modified;
                    await dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Sub-group updated successfully." });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in UpdateLedgerSubGroup: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost("DeleteLedgerSubGroup")]
        public IActionResult DeleteLedgerSubGroup([FromQuery] string ledgerSubGroupCode)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var subGroup = dbContext.Tbl20123LedgerSubGroups.FirstOrDefault(x => x.LedgerSubGroupCode == ledgerSubGroupCode);
                if (subGroup != null)
                {
                    dbContext.Tbl20123LedgerSubGroups.Remove(subGroup);
                    dbContext.SaveChanges();
                }
                return Ok();
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet("GetAssetTypes")]
        public async Task<IActionResult> GetAssetTypes()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var assetTypes = await dbContext.Tbl20109AssetsDocTypes.ToListAsync();
                    return Ok(assetTypes);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetAssetTypes: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost("AddAssetType")]
        public async Task<IActionResult> AddAssetType([FromBody] Tbl20109AssetsDocType assetType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var existingAssetType = dbContext.Tbl20109AssetsDocTypes
                        .FirstOrDefault(x => x.DocumentType == assetType.DocumentType);

                    if (existingAssetType != null)
                    {
                        return Conflict("Asset DocumentType already exists.");
                    }

                    var maxDocumentTypeId = dbContext.Tbl20109AssetsDocTypes
                        .OrderByDescending(a => a.DocumentTypeId)
                        .Select(a => a.DocumentTypeId)
                        .FirstOrDefault();

                    short newDocumentTypeId = (short)(maxDocumentTypeId + 1);
                    assetType.DocumentTypeId = newDocumentTypeId;

                    dbContext.Tbl20109AssetsDocTypes.Add(assetType);
                    await dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Asset Type added successfully." });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in AddAssetType: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost("UpdateAssetType")]
        public async Task<IActionResult> UpdateAssetType([FromBody] Tbl20109AssetsDocType assetType)
        {
            if (assetType == null || assetType.DocumentTypeId <= 0)
            {
                return BadRequest("Invalid asset type data.");
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var existingAssetType = await dbContext.Tbl20109AssetsDocTypes
                        .FirstOrDefaultAsync(a => a.DocumentTypeId == assetType.DocumentTypeId);

                    if (existingAssetType == null)
                    {
                        return NotFound(new { success = false, message = $"Asset Type with ID {assetType.DocumentTypeId} not found." });
                    }

                    if (!string.IsNullOrEmpty(assetType.DocumentType))
                    {
                        existingAssetType.DocumentType = assetType.DocumentType;
                    }

                    if (assetType.ReminderDays.HasValue)
                    {
                        existingAssetType.ReminderDays = assetType.ReminderDays;
                    }

                    dbContext.Entry(existingAssetType).State = EntityState.Modified;
                    await dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Asset Type updated successfully." });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in UpdateAssetType: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
    }
}