using DevExpress.PivotGrid.PivotTable;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using QD.ERP.Web.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Area("Finance")] // Specify the area if required
    [Route("api/[controller]")] // Base route will be api/Master
    [ApiController] // Enable API-specific behavior
    public class MasterController : Controller
    {
        private readonly ERPMasterWtDataContext _context;

        public MasterController(ERPMasterWtDataContext context)
        {
            _context = context;
        }

        // GET: api/Master/GetBranches
        [HttpGet("GetBranches")]
        public async Task<IActionResult> GetBranches()
        {
            try
            {
                // Retrieve branch data
                var branches = await _context.Tbl20115CompanyBranches.ToListAsync();

                // Return the data as JSON
                return Ok(branches);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/Master/AddBranch
        [HttpPost("AddBranch")]
        public async Task<IActionResult> AddBranch([FromBody] Tbl20115CompanyBranch branch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Get the max BranchCode and increment
                var maxBranchCode = _context.Tbl20115CompanyBranches
                    .OrderByDescending(b => b.BranchCode)
                    .Select(b => b.BranchCode)
                    .FirstOrDefault();

                int newBranchCode = string.IsNullOrEmpty(maxBranchCode) ? 1 : int.Parse(maxBranchCode) + 1;
                branch.BranchCode = newBranchCode.ToString(); // Format as 5-digit number (e.g., "00001")

                // Add the new branch
                _context.Tbl20115CompanyBranches.Add(branch);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Branch added successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/Master/UpdateBranch
        [HttpPost("UpdateBranch")]
        public async Task<IActionResult> UpdateBranch([FromBody] Tbl20115CompanyBranch branch)
        {
            if (branch == null || string.IsNullOrEmpty(branch.BranchCode))
            {
                return BadRequest("Invalid branch data.");
            }

            try
            {
                // Retrieve the existing branch using the BranchCode
                var existingBranch = await _context.Tbl20115CompanyBranches
                    .FirstOrDefaultAsync(b => b.BranchCode == branch.BranchCode);

                if (existingBranch == null)
                {
                    return NotFound(new { success = false, message = $"Branch with code {branch.BranchCode} not found." });
                }

                // Update only the fields that are provided
                if (!string.IsNullOrEmpty(branch.BranchName))
                {
                    existingBranch.BranchName = branch.BranchName;
                }

                if (!string.IsNullOrEmpty(branch.BranchNameAr))
                {
                    existingBranch.BranchNameAr = branch.BranchNameAr;
                }

                // Save the changes to the database
                _context.Entry(existingBranch).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Branch updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        // GET: api/Master/GetAssetMaintanencetype
        [HttpGet("GetAssetMaintanencetype")]
        public async Task<IActionResult> GetAssetMaintanencetype()
        {
            try
            {
                // Retrieve all asset maintenance types
                var assetMaintenanceTypes = await _context.Tbl20112AssetMaintenanceTypes.ToListAsync();

                // Return data as JSON
                return Ok(assetMaintenanceTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/Master/AddAssetMaintanencetype
        [HttpPost("AddAssetMaintanencetype")]
        public async Task<IActionResult> AddAssetMaintanencetype([FromBody] Tbl20112AssetMaintenanceType AssetmaintenanceData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Retrieve the maximum AssetMaintenanceTypeId from the database
                var maxId = await _context.Tbl20112AssetMaintenanceTypes
                    .OrderByDescending(a => a.AssetMaintenanceTypeId)
                    .Select(a => a.AssetMaintenanceTypeId)
                    .FirstOrDefaultAsync();

                // If maxId is 0 (or no data), start with 1
                // Explicitly cast the integer to byte, but ensure the range is within byte limits (0 to 255)
                AssetmaintenanceData.AssetMaintenanceTypeId = (byte)(maxId == 0 ? 1 : maxId + 1);

                // Add the new asset maintenance type to the database
                _context.Tbl20112AssetMaintenanceTypes.Add(AssetmaintenanceData);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Asset maintenance type added successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        // POST: api/Master/UpdateAssetMaintanencetype
        [HttpPost("UpdateAssetMaintanencetype")]
        public async Task<IActionResult> UpdateAssetMaintanencetype([FromBody] Tbl20112AssetMaintenanceType assetMaintenanceType)
        {
            if (assetMaintenanceType == null || string.IsNullOrEmpty(assetMaintenanceType.AssetMaintenanceTypeId.ToString()))
            {
                return BadRequest("Invalid asset maintenance type data.");
            }

            try
            {
                // Retrieve the existing asset maintenance type by ID
                var existingAssetMaintenanceType = await _context.Tbl20112AssetMaintenanceTypes
                    .FirstOrDefaultAsync(a => a.AssetMaintenanceTypeId == assetMaintenanceType.AssetMaintenanceTypeId);

                if (existingAssetMaintenanceType == null)
                {
                    return NotFound(new { success = false, message = "Asset maintenance type not found." });
                }

                // Update only the provided fields
                existingAssetMaintenanceType.AssetMaintenanceType = assetMaintenanceType.AssetMaintenanceType;

                // Save the changes to the database
                _context.Entry(existingAssetMaintenanceType).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Asset maintenance type updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        // GET: api/Master/GetAssetCategory
        [HttpGet("GetAssetCategory")]
        public async Task<IActionResult> GetAssetCategory()
        {
            var assetCategories = await _context.Tbl20106AssetCategories
                .Select(x => new
                {
                    x.AssetCategoryCode,
                    x.AssetCategory
                })
                .ToListAsync();
            return Ok(assetCategories);
        }

        // POST: api/Master/AddAssetCategory
        [HttpPost("AddAssetCategory")]
        public async Task<IActionResult> AddAssetCategory([FromBody] Tbl20106AssetCategory assetCategory)
        {
            if (assetCategory == null)
            {
                return BadRequest("Invalid data.");
            }

            try
            {

                // Retrieve the maximum AssetMaintenanceTypeId from the database
                var maxId = await _context.Tbl20106AssetCategories
                    .OrderByDescending(a => a.AssetCategoryCode)
                    .Select(a => a.AssetCategoryCode)
                    .FirstOrDefaultAsync();

                // If maxId is 0 (or no data), start with 1
                // Explicitly cast the integer to byte, but ensure the range is within byte limits (0 to 255)
                assetCategory.AssetCategoryCode = (byte)(maxId == 0 ? 1 : maxId + 1);

                _context.Tbl20106AssetCategories.Add(assetCategory);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Asset category added successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/Master/UpdateAssetCategory
        [HttpPost("UpdateAssetCategory")]
        public async Task<IActionResult> UpdateAssetCategory([FromBody] Tbl20106AssetCategory assetCategory)
        {
            if (assetCategory == null || assetCategory.AssetCategoryCode == 0)
            {
                return BadRequest("Invalid asset category data.");
            }

            try
            {
                var existingAssetCategory = await _context.Tbl20106AssetCategories
                    .FirstOrDefaultAsync(a => a.AssetCategoryCode == assetCategory.AssetCategoryCode);

                if (existingAssetCategory == null)
                {
                    return NotFound(new { success = false, message = "Asset category not found." });
                }

                existingAssetCategory.AssetCategory = assetCategory.AssetCategory;
                existingAssetCategory.DepreciationLedgerNo = assetCategory.DepreciationLedgerNo;
                existingAssetCategory.AccumDepLedgerNo = assetCategory.AccumDepLedgerNo;

                _context.Entry(existingAssetCategory).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Asset category updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        // GET: api/Master/Getassetlocation
        [HttpGet("Getassetlocation")]
        public IActionResult Getassetlocation()
        {
            var assetLocations = _context.Tbl20107AssetLocations.ToList();
            return Ok(assetLocations);
        }

        // POST: api/AssetLocation/Addassetlocation
        [HttpPost("Addassetlocation")]
        public IActionResult Add([FromBody] Tbl20107AssetLocation assetLocation)
        {
            if (assetLocation == null || string.IsNullOrWhiteSpace(assetLocation.AssetLocation))
            {
                return BadRequest("Asset Location is required.");
            }

            // Get the maximum AssetLocationCode from the database
            var maxAssetLocationCode = _context.Tbl20107AssetLocations
                .OrderByDescending(x => x.AssetLocationCode)
                .FirstOrDefault()?.AssetLocationCode ?? 0; // Default to 0 if no records are found.

            // Set the new AssetLocationCode to be max + 1
            assetLocation.AssetLocationCode = (short)(maxAssetLocationCode + 1);

            // Add the new Asset Location to the database
            _context.Tbl20107AssetLocations.Add(assetLocation);
            _context.SaveChanges();

            return Ok(new { message = "Asset Location added successfully." });
        }


        // POST: api/AssetLocation/Updateassetlocation
        [HttpPost("Updateassetlocation")]
        public IActionResult Update([FromBody] Tbl20107AssetLocation assetLocation)
        {
            if (assetLocation == null || string.IsNullOrWhiteSpace(assetLocation.AssetLocation))
            {
                return BadRequest("Asset Location is required.");
            }

            var existingAssetLocation = _context.Tbl20107AssetLocations
                .FirstOrDefault(x => x.AssetLocationCode == assetLocation.AssetLocationCode);

            if (existingAssetLocation == null)
            {
                return NotFound("Asset Location not found.");
            }

            existingAssetLocation.AssetLocation = assetLocation.AssetLocation;
            _context.SaveChanges();

            return Ok(new { message = "Asset Location updated successfully." });
        }
        // GET: api/Master/GetSalesPersons
        [HttpGet("GetSalesPersons")]
        public async Task<IActionResult> GetSalesPersons()
        {
            try
            {
                // Retrieve all salesperson records
                var salesPersons = await _context.Tbl20101SalesPersonMasters.ToListAsync();

                // Return the data as JSON
                return Ok(salesPersons);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("AddSalesPerson")]
        public async Task<IActionResult> AddSalesPerson([FromBody] Tbl20101SalesPersonMaster salesPerson)
        {
            if (!ModelState.IsValid)
            {
                // Log the validation errors
                var validationErrors = string.Join(", ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                return BadRequest($"Validation failed: {validationErrors}");
            }

            try
            {
                // Log the received data (optional)
                Console.WriteLine($"Received SalesPerson Data: {JsonConvert.SerializeObject(salesPerson)}");

                // If userCode is null, ensure proper handling in database
                if (salesPerson.UserCode == null)
                {
                    salesPerson.UserCode = null; // Ensure it's properly handled as nullable
                }

                // Add the new salesperson to the database
                _context.Tbl20101SalesPersonMasters.Add(salesPerson);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Salesperson added successfully." });
            }
            catch (Exception ex)
            {
                // Log the error and return the exception message
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/Master/UpdateSalesPerson
        [HttpPost("UpdateSalesPerson")]
        public async Task<IActionResult> UpdateSalesPerson([FromBody] Tbl20101SalesPersonMaster salesPerson)
        {
            if (salesPerson == null || string.IsNullOrEmpty(salesPerson.SalesPersonCode))
            {
                return BadRequest("Invalid salesperson data.");
            }

            try
            {
                // Retrieve the existing salesperson by SalesPersonCode
                var existingSalesPerson = await _context.Tbl20101SalesPersonMasters
                    .FirstOrDefaultAsync(s => s.SalesPersonCode == salesPerson.SalesPersonCode);

                if (existingSalesPerson == null)
                {
                    return NotFound(new { success = false, message = $"Salesperson with code {salesPerson.SalesPersonCode} not found." });
                }

                // Update only the fields that are provided
                existingSalesPerson.SalesPersonName = salesPerson.SalesPersonName;
                existingSalesPerson.UserCode = salesPerson.UserCode;
                existingSalesPerson.EmailAddress = salesPerson.EmailAddress;
                existingSalesPerson.SalesPersonContactNo = salesPerson.SalesPersonContactNo;

                // Mark the entity as modified
                _context.Entry(existingSalesPerson).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Salesperson updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }




        [HttpGet("GetLedgerSubGroups")]
        public IActionResult GetLedgerSubGroups()
        {
            var subGroups = _context.Tbl20123LedgerSubGroups.ToList();
            return Ok(subGroups);
        }

        // POST: api/Master/AddLedgerSubGroup
        [HttpPost("AddLedgerSubGroup")]
        public IActionResult AddLedgerSubGroup([FromBody] Tbl20123LedgerSubGroup subGroup)
        {
            if (subGroup == null)
            {
                return BadRequest("Invalid data. The subGroup is null.");
            }

            try
            {
                // Debug log: Check if subGroup is properly received.
                Console.WriteLine($"Received subGroup: {subGroup.SubGroupName}");

                // Get the max LedgerSubGroupCode and increment it
                var maxLedgerSubGroupCode = _context.Tbl20123LedgerSubGroups
                    .OrderByDescending(sg => sg.LedgerSubGroupCode)
                    .Select(sg => sg.LedgerSubGroupCode)
                    .FirstOrDefault();

                // Base prefix that will remain constant in the code
                string codePrefix = "A011-";

                // Generate new LedgerSubGroupCode
                string newLedgerSubGroupCode;

                if (string.IsNullOrEmpty(maxLedgerSubGroupCode))
                {
                    // If no existing code found, start from "A011-001"
                    newLedgerSubGroupCode = codePrefix + "001";
                }
                else
                {
                    // Split the existing LedgerSubGroupCode into prefix and numeric part
                    var parts = maxLedgerSubGroupCode.Split('-');

                    if (parts.Length == 2 && int.TryParse(parts[1], out int currentNumber))
                    {
                        // Increment the numeric part
                        currentNumber++;

                        // Format the new number part to be 3 digits long (e.g., "005")
                        newLedgerSubGroupCode = codePrefix + currentNumber.ToString("D3");
                    }
                    else
                    {
                        // Handle cases where the format is unexpected
                        return BadRequest("Invalid format for LedgerSubGroupCode.");
                    }
                }

                // Set the newly generated LedgerSubGroupCode
                subGroup.LedgerSubGroupCode = newLedgerSubGroupCode;

                // Debug log: Check the new generated LedgerSubGroupCode.
                Console.WriteLine($"Generated new LedgerSubGroupCode: {subGroup.LedgerSubGroupCode}");

                // Add the new Ledger SubGroup to the database
                _context.Tbl20123LedgerSubGroups.Add(subGroup);
                _context.SaveChanges();

                return Ok(new { success = true, message = "Ledger SubGroup added successfully." });
            }
            catch (Exception ex)
            {
                // Log the exception for debugging.
                Console.WriteLine($"Error occurred: {ex.Message}");

                // Return a detailed error message.
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/Master/UpdateLedgerSubGroup
        [HttpPost("UpdateLedgerSubGroup")]
        public async Task<IActionResult> UpdateLedgerSubGroup([FromBody] Tbl20123LedgerSubGroup subGroup)
        {
            // Check for null data or invalid LedgerSubGroupCode
            if (subGroup == null || string.IsNullOrEmpty(subGroup.LedgerSubGroupCode))
            {
                return BadRequest("Invalid sub-group data.");
            }

            try
            {
                // Retrieve the existing LedgerSubGroup using the LedgerSubGroupCode
                var existingSubGroup = await _context.Tbl20123LedgerSubGroups
                    .FirstOrDefaultAsync(sg => sg.LedgerSubGroupCode == subGroup.LedgerSubGroupCode);

                if (existingSubGroup == null)
                {
                    return NotFound(new { success = false, message = $"Sub-group with code {subGroup.LedgerSubGroupCode} not found." });
                }

                // Update only the fields that are provided (checking for null or empty)
                if (!string.IsNullOrEmpty(subGroup.SubGroupName))
                {
                    existingSubGroup.SubGroupName = subGroup.SubGroupName;
                }

                if (!string.IsNullOrEmpty(subGroup.SubGroupNameAr))
                {
                    existingSubGroup.SubGroupNameAr = subGroup.SubGroupNameAr;
                }

                // Save the changes to the database
                _context.Entry(existingSubGroup).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Sub-group updated successfully." });
            }
            catch (Exception ex)
            {
                // If there's any error, return internal server error with message
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpPost("DeleteLedgerSubGroup")]
        public IActionResult DeleteLedgerSubGroup([FromBody] string ledgerSubGroupCode)
        {
            var subGroup = _context.Tbl20123LedgerSubGroups.FirstOrDefault(x => x.LedgerSubGroupCode == ledgerSubGroupCode);
            if (subGroup != null)
            {
                _context.Tbl20123LedgerSubGroups.Remove(subGroup);
                _context.SaveChanges();
            }
            return Ok();
        }

        // GET: api/AssetTypeMaster/GetAssetTypes
        [HttpGet("GetAssetTypes")]
        public async Task<IActionResult> GetAssetTypes()
        {
            try
            {
                // Retrieve asset type data
                var assetTypes = await _context.Tbl20109AssetsDocTypes.ToListAsync();

                // Return the data as JSON
                return Ok(assetTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/AssetTypeMaster/AddAssetType
        [HttpPost("AddAssetType")]
        public async Task<IActionResult> AddAssetType([FromBody] Tbl20109AssetsDocType assetType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Get the max DocumentTypeId and increment
                var maxDocumentTypeId = _context.Tbl20109AssetsDocTypes
                    .OrderByDescending(a => a.DocumentTypeId)
                    .Select(a => a.DocumentTypeId)
                    .FirstOrDefault();

                short newDocumentTypeId = (short)(maxDocumentTypeId + 1); // Cast to short
                assetType.DocumentTypeId = newDocumentTypeId; // Assign the new ID to the asset type


                // Add the new asset type
                _context.Tbl20109AssetsDocTypes.Add(assetType);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Asset Type added successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/AssetTypeMaster/UpdateAssetType
        [HttpPost("UpdateAssetType")]
        public async Task<IActionResult> UpdateAssetType([FromBody] Tbl20109AssetsDocType assetType)
        {
            if (assetType == null || assetType.DocumentTypeId <= 0)
            {
                return BadRequest("Invalid asset type data.");
            }

            try
            {
                // Retrieve the existing asset type using DocumentTypeId
                var existingAssetType = await _context.Tbl20109AssetsDocTypes
                    .FirstOrDefaultAsync(a => a.DocumentTypeId == assetType.DocumentTypeId);

                if (existingAssetType == null)
                {
                    return NotFound(new { success = false, message = $"Asset Type with ID {assetType.DocumentTypeId} not found." });
                }

                // Update only the fields that are provided
                if (!string.IsNullOrEmpty(assetType.DocumentType))
                {
                    existingAssetType.DocumentType = assetType.DocumentType;
                }

                if (assetType.ReminderDays.HasValue)
                {
                    existingAssetType.ReminderDays = assetType.ReminderDays;
                }

                // Save the changes to the database
                _context.Entry(existingAssetType).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Asset Type updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }

}

