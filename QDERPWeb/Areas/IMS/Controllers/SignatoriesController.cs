using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.Areas.VAT.Controllers;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;

namespace QD.ERP.Web.Areas.IMS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SignatoriesController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<SignatoriesController> _logger;

        public SignatoriesController(ILogger<SignatoriesController> logger, TenantDbContextHelper tenantDbContextHelper)
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
                    var now = DateTime.Now;

                    var existing = await dbContext.Tbl90104DocumentSignatories
                        .FirstOrDefaultAsync(x => x.SignatoryId == model.SignatoryId);

                    if (existing != null)
                    {
                        existing.SignatoryName = model.SignatoryName;
                        existing.SignatoryPosition = model.SignatoryPosition;
                        existing.SignatoryContact = model.SignatoryContact;
                        existing.SignatoryEmail = model.SignatoryEmail;
                        existing.SignatureImage = model.SignatureImage;
                        existing.SignatureDescription = model.SignatureDescription;
                        existing.SignatureCode = model.SignatureCode;
                        existing.SignatoryMobile1 = model.SignatoryMobile1;
                        existing.SignatoryMobile2 = model.SignatoryMobile2;
                        existing.SignatoryNameAr = model.SignatoryNameAr;
                        existing.SignatoryPositionAr = model.SignatoryPositionAr;
                        existing.IsFinanceManager = model.IsFinanceManager;
                        existing.UserId = model.UserId;
                        // No Created/Modified dates in entity? Add if needed
                    }
                    else
                    {
                        // Assign new SignatoryId
                        var lastId = await dbContext.Tbl90104DocumentSignatories
                            .OrderByDescending(x => x.SignatoryId)
                            .Select(x => x.SignatoryId)
                            .FirstOrDefaultAsync();

                        model.SignatoryId = (byte)(lastId + 1); // Assuming short type

                        dbContext.Tbl90104DocumentSignatories.Add(model);
                    }

                    await dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Saved successfully", id = model.SignatoryId });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in SaveSignatory: {ex}");
                    return StatusCode(500, new { success = false, message = ex.Message });
                }
            }

            return Unauthorized(new { success = false, message = "Invalid tenant" });
        }

    }
}
