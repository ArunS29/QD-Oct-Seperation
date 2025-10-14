using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.Areas.VAT.Controllers;
using QD.ERP.Shared.DAL.Entities;
using QD.ERP.Shared.Service;
using Microsoft.Extensions.Logging;

namespace QD.ERP.ERM.Areas.ERM.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ERSignatoriesController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<ERSignatoriesController> _logger;

        public ERSignatoriesController(ILogger<ERSignatoriesController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
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



    }
}
