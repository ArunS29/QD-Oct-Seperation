using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using SkiaSharp;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AssetViewController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<AssetViewController> _logger;

        public AssetViewController(ILogger<AssetViewController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }
        [HttpGet]
        public async Task<ActionResult> GetAssetView(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var ledgerData = dbContext.AssetRegisterViews
                        .FromSqlRaw("EXEC sp20157AssetRegisterView")
                        .AsQueryable(); // ✅ Keep it as IQueryable

                    return Json(await DataSourceLoader.LoadAsync(ledgerData, loadOptions)); // ✅ No ToListAsync() here
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetAssetView: {ex.Message}");
                    return BadRequest(new { message = "An error occurred while fetching data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }


        [HttpGet]
        public async Task<ActionResult> GetAssetViewedit(string assetLedgerNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    if (!string.IsNullOrEmpty(assetLedgerNo))
                    {
                        assetLedgerNo = assetLedgerNo.Trim('"');
                    }

                    var ledgerData = await dbContext.Tbl20105AssetMasters
                        .Where(x => string.IsNullOrEmpty(assetLedgerNo) || x.AssetLedgerNo == assetLedgerNo)
                        .Select(x => new
                        {
                            x.AssetLedgerNo,
                            x.AssetDescription,
                            x.Specifications,
                            x.Brand,
                            x.PlateNo,
                            x.Model,
                            x.Year,
                            x.Ownership,
                            PurchaseDate = x.PurchaseDate.HasValue ? x.PurchaseDate.Value.ToString("dd-MMM-yyyy") : string.Empty,
                            x.ValueOfProperty,
                            x.PurchasedAs,
                            x.IsFinanced,
                            x.FinancedFrom,
                            x.NoOfInstallments,
                            x.InitialDownPayment,
                            x.InitialDocCharges,
                            x.MonthlyInstallment,
                            x.FinalInstallment,
                            InstallmentStartDate = x.InstallmentStartDate.HasValue ? x.InstallmentStartDate.Value.ToString("dd-MMM-yyyy") : string.Empty,
                            InstallmentEndDate = x.InstallmentEndDate.HasValue ? x.InstallmentEndDate.Value.ToString("dd-MMM-yyyy") : string.Empty,
                            x.PurchasedFrom,
                            x.CurrentCondition,
                            x.CurrentReading,
                            x.DepreciationMethod,
                            x.LifeSpanOfProperty,
                            x.ScrapValueOfProperty,
                            x.AssetCategory,
                            x.AssetLocation,
                            x.AssetType,
                            x.Fmv,
                            x.Bmv,
                            x.FinancedBy,
                            x.DepPercentage,
                            x.PropertyNo,
                            x.AddedBy,
                            x.AddedOn,
                            x.ModifiedBy,
                            x.ModifiedOn
                        })
                        .ToListAsync();

                    return Json(ledgerData);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetAssetViewedit: {ex.Message}");
                    return BadRequest(new { message = "An error occurred while fetching data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public async Task<ActionResult> UpdateAsset([FromBody] Tbl20105AssetMaster updatedAsset)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    if (updatedAsset == null)
                    {
                        return BadRequest(new { message = "Invalid asset data." });
                    }

                    var existingAsset = await dbContext.Tbl20105AssetMasters
                        .FirstOrDefaultAsync(x => x.AssetLedgerNo == updatedAsset.AssetLedgerNo);

                    if (existingAsset == null)
                    {
                        return NotFound(new { message = "Asset not found." });
                    }

                    existingAsset.AssetDescription = updatedAsset.AssetDescription;
                    existingAsset.Specifications = updatedAsset.Specifications;
                    existingAsset.Brand = updatedAsset.Brand;
                    existingAsset.PlateNo = updatedAsset.PlateNo;
                    existingAsset.Model = updatedAsset.Model;
                    existingAsset.Year = updatedAsset.Year;
                    existingAsset.Ownership = updatedAsset.Ownership;
                    existingAsset.ValueOfProperty = updatedAsset.ValueOfProperty;
                    existingAsset.PurchasedAs = updatedAsset.PurchasedAs;
                    existingAsset.PurchaseDate = updatedAsset.PurchaseDate;
                    existingAsset.IsFinanced = updatedAsset.IsFinanced;
                    existingAsset.FinancedFrom = updatedAsset.FinancedFrom;
                    existingAsset.NoOfInstallments = updatedAsset.NoOfInstallments;
                    existingAsset.InitialDownPayment = updatedAsset.InitialDownPayment;
                    existingAsset.InitialDocCharges = updatedAsset.InitialDocCharges;
                    existingAsset.MonthlyInstallment = updatedAsset.MonthlyInstallment;
                    existingAsset.FinalInstallment = updatedAsset.FinalInstallment;
                    existingAsset.InstallmentStartDate = updatedAsset.InstallmentStartDate;
                    existingAsset.InstallmentEndDate = updatedAsset.InstallmentEndDate;
                    existingAsset.PurchasedFrom = updatedAsset.PurchasedFrom;
                    existingAsset.CurrentCondition = updatedAsset.CurrentCondition;
                    existingAsset.CurrentReading = updatedAsset.CurrentReading;
                    existingAsset.DepreciationMethod = updatedAsset.DepreciationMethod;
                    existingAsset.LifeSpanOfProperty = updatedAsset.LifeSpanOfProperty;
                    existingAsset.ScrapValueOfProperty = updatedAsset.ScrapValueOfProperty;
                    existingAsset.AssetCategory = updatedAsset.AssetCategory;
                    existingAsset.AssetLocation = updatedAsset.AssetLocation;
                    existingAsset.AssetType = updatedAsset.AssetType;
                    existingAsset.Fmv = updatedAsset.Fmv;
                    existingAsset.Bmv = updatedAsset.Bmv;
                    existingAsset.FinancedBy = updatedAsset.FinancedBy;
                    existingAsset.DepPercentage = updatedAsset.DepPercentage;
                    existingAsset.PropertyNo = updatedAsset.PropertyNo;

                    existingAsset.ModifiedBy = "Admin"; // Replace with actual current user
                    existingAsset.ModifiedOn = DateTime.Now;

                    await dbContext.SaveChangesAsync();

                    return Ok(new { message = "Asset updated successfully." });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in UpdateAsset: {ex.Message}");
                    return BadRequest(new { message = "An error occurred while updating the asset.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public IActionResult Delete(string assetLedgerNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var result = dbContext.Database.ExecuteSqlRaw("EXEC sp20121DeleteAssetRegister @AssetLedgerNo = {0}", assetLedgerNo);

                    if (result == 0)
                        return NotFound(new { message = "Asset not found or could not be deleted." });

                    return Ok(new { message = "Asset deleted successfully." });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in Delete: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while deleting the asset.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public IActionResult DeleteAsset(string assetLedgerNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                if (string.IsNullOrEmpty(assetLedgerNo))
                {
                    return BadRequest("AssetLedgerNo is required.");
                }

                var asset = dbContext.Tbl20105AssetMasters.FirstOrDefault(a => a.AssetLedgerNo == assetLedgerNo);
                if (asset == null)
                {
                    return NotFound("Asset not found.");
                }

                dbContext.Tbl20105AssetMasters.Remove(asset);
                dbContext.SaveChanges();

                return Ok("Asset deleted successfully.");
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<ActionResult> GetAssetCategoriesByCode(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var assetcategory = dbContext.Tbl20106AssetCategories.Select(i => new
                {
                    i.AssetCategoryCode,
                    i.AssetCategory
                });

                return Json(await DataSourceLoader.LoadAsync(assetcategory, loadOptions));
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<ActionResult> GetddlAssetlocation(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var assetlocation = dbContext.Tbl20107AssetLocations.Select(i => new
                {
                    i.AssetLocationCode,
                    i.AssetLocation
                });

                return Json(await DataSourceLoader.LoadAsync(assetlocation, loadOptions));
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<ActionResult> GetPropertDetails(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var qryListOfAccountlists = dbContext.Tbl40101PropertyMasters.Select(i => new
                {
                    i.PropertyNo,
                    i.PropertyDescription,
                    i.Specifications,
                    i.Brand,
                    i.PlateNo,
                    i.DoorNo,
                    i.ChassisNo,
                    i.Color,
                    i.Capacity,
                    i.Model,
                    i.Year,
                    i.Ownership,
                    i.PropertyGroupId,
                    i.PropertyType
                });

                return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<ActionResult> GetddlAssetMaintanence(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var assetlocation = dbContext.Tbl20112AssetMaintenanceTypes.Select(i => new
                {
                    i.AssetMaintenanceTypeId,
                    i.AssetMaintenanceType
                });

                return Json(await DataSourceLoader.LoadAsync(assetlocation, loadOptions));
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpPost]
        public IActionResult DeleteMaintenanceSchedule(long MaintenanceRefNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var schedule = dbContext.Tbl20111AssetMaintenanceMasters
                                            .FirstOrDefault(m => m.MaintenanceRefNo == MaintenanceRefNo);

                    if (schedule == null)
                    {
                        return Json(new { success = false, message = "Maintenance Schedule not found." });
                    }

                    dbContext.Tbl20111AssetMaintenanceMasters.Remove(schedule);
                    dbContext.SaveChanges();

                    return Json(new { success = true, message = "Master Records has been successfully removed from the database." });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error deleting Maintenance Schedule: {ex.Message}");
                    return Json(new { success = false, message = "An error occurred while deleting the record." });
                }
            }

            return Json(new { success = false, message = "Invalid tenant." });
        }

        [HttpGet]
        public async Task<ActionResult> GetMaintenance(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var qryListOfAccountlists = dbContext.Tbl20111AssetMaintenanceMasters.Select(i => new
                {


    i.AssetLedgerNo,
    i.MaintenanceRefNo,
       i.MaintenanceTypeId,
         i.MaintenanceReading,

        i.MaintenanceDate,

         i.ReminderDate,

        i.IsMaintenanceDone, 

       i.ActualMaintenanceDoneOn,

        i.MaintenanceRemarks 
    });

                return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

    }
}
