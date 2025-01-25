using AutoMapper.Execution;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.DAL.Entities;
using System.Drawing;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    //[Area("Finance")]
    [Route("api/[controller]/[action]")]
    // [Route("Finapi/[controller]/[action]")]
    [ApiController]
    public class AssetViewController : Controller
    {
        private ERPMasterWtDataContext _context;

        public AssetViewController(ERPMasterWtDataContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult> GetAssetView()
        {
            try
            {
                ERPMasterWtDataContextProcedures _procedures = new ERPMasterWtDataContextProcedures(_context);


                var ledgerData = await _procedures.sp20157AssetRegisterViewAsync();


                var result = ledgerData.Select(x => new
                {
                    x.AccountGroup,
                    x.AssetLedgerNo,
                    x.AccountHead,
                    x.AssetDescription,
                    x.Specifications,
                    x.PropertyNo,
                    x.DepreciationMethod,
                    x.ScrapValueOfProperty,
                    x.LifeSpanOfProperty,
                    x.DepreciationPercentage,
                    x.OpeningTotal,
                    x.TotalDebit,
                    x.TotalCredit,
                    //TotalCredit = x.TotalCredit.HasValue ? (x.TotalCredit < 0 ? $"{Math.Abs(x.TotalCredit.Value):N2}Cr" 
                    //: $"{x.TotalCredit.Value:N2}") 
                    //: "0.00",
                    x.ClosingBalance,
                    x.TotalDepreciatedAmount,
                    x.NetBookValue,
                    x.AssetCategory,
                    x.AssetLocation,
                    x.Brand,
                    x.PlateNo,
                    x.Model,
                    x.Year,
                    x.AssetType,
                    x.FMV,
                    x.BMV,
                    x.FinancedBy,
                    x.Ownership,
                    PurchaseDate = x.PurchaseDate.HasValue
                    ? x.PurchaseDate.Value.ToString("dd-MMM-yyyy")
                    : string.Empty,
                    x.ValueOfProperty,
                    x.PurchasedAs,
                    x.IsFinanced,
                    x.FinancedFrom,
                    x.NoOfInstallments,
                    x.InitialDownPayment,
                    x.InitialDocCharges,
                    x.MonthlyInstallment,
                    x.FinalInstallment,
                    InstallmentStartDate = x.InstallmentStartDate.HasValue
                    ? x.InstallmentStartDate.Value.ToString("dd-MMM-yyyy")
                    : string.Empty,
                    InstallmentEndDate = x.InstallmentEndDate.HasValue
                    ? x.InstallmentEndDate.Value.ToString("dd-MMM-yyyy")
                    : string.Empty,
                    x.PurchasedFrom,
                    x.CurrentCondition


                }).ToList();



                return Json(result);
            }
            catch (Exception ex)

            {

                return BadRequest(new { message = "An error occurred while fetching data.", error = ex.Message });
            }
        }
        [HttpGet]
        public async Task<ActionResult> GetAssetViewedit(string assetLedgerNo)
        {
            try
            {
                if (!string.IsNullOrEmpty(assetLedgerNo))
                {
                    assetLedgerNo = assetLedgerNo.Trim('"');
                }

                // Assuming you are using Entity Framework to query the database
                var ledgerData = await _context.Tbl20105AssetMasters
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
                    })
                    .ToListAsync();

                return Json(ledgerData);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred while fetching data.", error = ex.Message });
            }
        }
        [HttpPost]
        public async Task<ActionResult> updateAsset([FromBody] Tbl20105AssetMaster updatedAsset)
        {
            try
            {
                if (updatedAsset == null)
                {
                    return BadRequest(new { message = "Invalid asset data." });
                }

                // Fetch the existing asset record
                var existingAsset = await _context.Tbl20105AssetMasters
                    .FirstOrDefaultAsync(x => x.AssetLedgerNo == updatedAsset.AssetLedgerNo);

                if (existingAsset == null)
                {
                    return NotFound(new { message = "Asset not found." });
                }

                //// Validate and convert PurchaseDate if necessary
                //if (!string.IsNullOrEmpty(updatedAsset.PurchaseDate?.ToString()))
                //{
                //    if (DateTime.TryParse(updatedAsset.PurchaseDate.ToString(), out var parsedDate))
                //    {
                //        existingAsset.PurchaseDate = parsedDate; // Set the parsed date
                //    }
                //    else
                //    {
                //        return BadRequest(new { message = "Invalid date format for PurchaseDate." });
                //    }
                //}

                // Update fields
                existingAsset.AssetDescription = updatedAsset.AssetDescription;
                existingAsset.Specifications = updatedAsset.Specifications;
                existingAsset.Brand = updatedAsset.Brand;
                existingAsset.PlateNo = updatedAsset.PlateNo;
                existingAsset.Model = updatedAsset.Model;
                existingAsset.Year = updatedAsset.Year;
                existingAsset.Ownership = updatedAsset.Ownership;
                existingAsset.ValueOfProperty = updatedAsset.ValueOfProperty;
                existingAsset.PurchasedAs = updatedAsset.PurchasedAs;
                //existingAsset.PurchaseDate = updatedAsset.PurchaseDate;
                existingAsset.IsFinanced = updatedAsset.IsFinanced;
                existingAsset.FinancedFrom = updatedAsset.FinancedFrom;
                existingAsset.NoOfInstallments = updatedAsset.NoOfInstallments;
                existingAsset.InitialDownPayment = updatedAsset.InitialDownPayment;
                existingAsset.InitialDocCharges = updatedAsset.InitialDocCharges;
                existingAsset.MonthlyInstallment = updatedAsset.MonthlyInstallment;
                existingAsset.FinalInstallment = updatedAsset.FinalInstallment;
                //existingAsset.InstallmentStartDate = updatedAsset.InstallmentStartDate;
                //existingAsset.InstallmentEndDate = updatedAsset.InstallmentEndDate;
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

                // Set modification details
                existingAsset.ModifiedBy = "Admin"; // Replace with actual current user
                existingAsset.ModifiedOn = DateTime.Now;

                // Save changes
                await _context.SaveChangesAsync();

                return Ok(new { message = "Asset updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred while updating the asset.", error = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Delete(string assetLedgerNo)
        {
            try
            {

                var result = _context.Database.ExecuteSqlRaw("EXEC sp20121DeleteAssetRegister @AssetLedgerNo = {0}", assetLedgerNo);

                if (result == 0)
                    return NotFound(new { message = "Asset not found or could not be deleted." });

                return Ok(new { message = "Asset deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the asset.", error = ex.Message });
            }
        }


        [HttpPost]
        public IActionResult DeleteAsset(string assetLedgerNo)
        {
            if (string.IsNullOrEmpty(assetLedgerNo))
            {
                return BadRequest("AssetLedgerNo is required.");
            }

            // Assuming DbContext is named _context
            var asset = _context.Tbl20105AssetMasters.FirstOrDefault(a => a.AssetLedgerNo == assetLedgerNo);
            if (asset == null)
            {
                return NotFound("Asset not found.");
            }

            _context.Tbl20105AssetMasters.Remove(asset);
            _context.SaveChanges();

            return Ok("Asset deleted successfully.");
        }


        [HttpGet]
        public async Task<ActionResult> GetAssetCategoriesByCode(DataSourceLoadOptions loadOptions)
        {
            var assetcategory = _context.Tbl20106AssetCategories.Select(i => new
            {
                i.AssetCategoryCode,
                i.AssetCategory


            });

            return Json(await DataSourceLoader.LoadAsync(assetcategory, loadOptions));
        }
        [HttpGet]
        public async Task<ActionResult> GetddlAssetlocation(DataSourceLoadOptions loadOptions)
        {
            var assetlocation = _context.Tbl20107AssetLocations.Select(i => new
            {
                i.AssetLocationCode,
                i.AssetLocation


            });

            return Json(await DataSourceLoader.LoadAsync(assetlocation, loadOptions));
        }
        [HttpGet]
        public async Task<ActionResult> GetPropertDetails(DataSourceLoadOptions loadOptions)
        {
            var qryListOfAccountlists = _context.Tbl40101PropertyMasters.Select(i => new
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

        [HttpGet]
        public async Task<ActionResult> GetddlAssetMaintanence(DataSourceLoadOptions loadOptions)
        {
            var assetlocation = _context.Tbl20112AssetMaintenanceTypes.Select(i => new
            {
                i.AssetMaintenanceTypeId,
                i.AssetMaintenanceType


            });

            return Json(await DataSourceLoader.LoadAsync(assetlocation, loadOptions));
        }
    }
}
