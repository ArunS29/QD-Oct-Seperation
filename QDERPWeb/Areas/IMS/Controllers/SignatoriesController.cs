using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.Areas.VAT.Controllers;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using QD.ERP.Web.Services.Logging;

namespace QD.ERP.Web.Areas.IMS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SignatoriesController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<SignatoriesController> _logger;
        private readonly IUserActionLogger _userActionLogger;


        public SignatoriesController(ILogger<SignatoriesController> logger, TenantDbContextHelper tenantDbContextHelper, IUserActionLogger userActionLogger)
        {
            _userActionLogger = userActionLogger;
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllSignData()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var dbSignatories = await dbContext.Tbl90104DocumentSignatories
                       .Select(s => new
                       {
                           SignatoryID = s.SignatoryId.ToString(),
                           s.SignatoryName,
                           s.SignatoryPosition,
                           s.SignatoryContact,
                           s.SignatoryEmail,
                           s.SignatureDescription,
                           s.SignatureCode,
                           s.SignatoryMobile1,
                           s.SignatoryMobile2,
                           s.SignatoryNameAr,
                           s.SignatoryPositionAr,
                           s.IsFinanceManager,
                           s.UserId,
                           DecodedBusinessCard1 = s.SignatureImage != null ? $"data:image/png;base64,{Convert.ToBase64String(s.SignatureImage)}" : null

                       })
                        .ToListAsync();

                    return Json(dbSignatories); // return raw data, paging/sorting done on client-side
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> SaveOrUpdateSignatory([FromBody] Tbl90104DocumentSignatory model)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var existingRecord = await dbContext.Tbl90104DocumentSignatories
                        .FirstOrDefaultAsync(x => x.SignatoryId == model.SignatoryId);

                    if (existingRecord != null)
                    {
                        // Update existing record
                        existingRecord.SignatoryName = model.SignatoryName;
                        existingRecord.SignatoryPosition = model.SignatoryPosition;
                        existingRecord.SignatoryContact = model.SignatoryContact;
                        existingRecord.SignatoryEmail = model.SignatoryEmail;

                        existingRecord.SignatureImage = model.SignatureImage;


                        await dbContext.SaveChangesAsync();
                        await _userActionLogger.LogAsync(
                          module: "IMS > Save Or Update Signatory",
                          actionDetail: $":Saved Signatory {model.SignatoryId}",
                          documentNo: $"{model.SignatoryId}"
                        );

                        return Ok(new { success = true, message = "Updated successfully", id = existingRecord.SignatoryId });
                    }
                    else
                    {
                        // Insert new record
                        var lastId = await dbContext.Tbl90104DocumentSignatories
                            .OrderByDescending(x => x.SignatoryId)
                            .Select(x => x.SignatoryId)
                            .FirstOrDefaultAsync();

                        model.SignatoryId = lastId == 0 ? (byte)1 : (byte)(lastId + 1);


                        dbContext.Tbl90104DocumentSignatories.Add(model);
                        await dbContext.SaveChangesAsync();
                        await _userActionLogger.LogAsync(
                          module: "IMS > Save Or Update Signatory",
                          actionDetail: $":Saved Signatory {model.SignatoryId}",
                          documentNo: $"{model.SignatoryId}"
                        );

                        return Ok(new { success = true, message = "Saved successfully", id = model.SignatoryId });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in SaveOrUpdateSignatory: {ex}");
                    return StatusCode(500, new { success = false, message = ex.Message });
                }
            }

            return Unauthorized(new { success = false, message = "Invalid tenant" });
        }
        [HttpDelete]
        public IActionResult Delete(int key)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var record = dbContext.Tbl90104DocumentSignatories.FirstOrDefault(x => x.SignatoryId == key);
                    if (record == null)
                        return NotFound();

                    dbContext.Tbl90104DocumentSignatories.Remove(record);
                    dbContext.SaveChanges();
                    _userActionLogger.LogAsync(module: "IMS > Delete ",
                        actionDetail: $":Deleted  {key}",
                        documentNo: $"{key}"
                       );
                    return Ok();
                }

                return Unauthorized(new { success = false, message = "Invalid tenant" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });

            }
        }
        [HttpPost]
        public async Task<IActionResult> DeleteMultiple([FromBody] List<int> signatoryIds)
        {
            if (signatoryIds == null || !signatoryIds.Any())
                return BadRequest(new { success = false, message = "No IDs provided" });

            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var toDelete = dbContext.Tbl90104DocumentSignatories
                                            .Where(s => signatoryIds.Contains(s.SignatoryId)) // ✅ Make sure 'SignatoryId' is correct
                                            .ToList();

                    if (toDelete.Count == 0)
                        return NotFound(new { success = false, message = "No matching records found" });

                    dbContext.Tbl90104DocumentSignatories.RemoveRange(toDelete);
                    await dbContext.SaveChangesAsync();
                    await _userActionLogger.LogAsync(
                          module: "IMS > Delete Multiple",
                          actionDetail: $":Deleted Multiple {signatoryIds}",
                          documentNo: $"{signatoryIds}"
                    );

                    return Ok(new { success = true });
                }

                return Unauthorized(new { success = false, message = "Invalid tenant" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting signatories");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }




    }
}
