using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.Areas.Finance.Controllers;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;

namespace QD.ERP.Web.Areas.ERM.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DocumentTypesController : ControllerBase
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<DocumentTypesController> _logger;

        public DocumentTypesController(ILogger<DocumentTypesController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetDocumentTypes()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var documentTypes = await dbContext.Tbl101DocumentTypes.ToListAsync();
                    return Ok(documentTypes);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetDocumentTypes: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public async Task<IActionResult> AddDocumentType([FromBody] Tbl101DocumentType documentType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var exists = await dbContext.Tbl101DocumentTypes
                        .AnyAsync(x => x.DocumentType == documentType.DocumentType);
                    if (exists)
                    {
                        return BadRequest(new { success = false, message = "Document Type already exists." });
                    }

                    short lastId = dbContext.Tbl101DocumentTypes
                        .OrderByDescending(x => x.DocumentTypeId)
                        .Select(x => x.DocumentTypeId)
                        .FirstOrDefault();

                    documentType.DocumentTypeId = (short)(lastId + 1);

                    dbContext.Tbl101DocumentTypes.Add(documentType);
                    await dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Document Type added successfully." });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in AddDocumentType: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateDocumentType([FromBody] Tbl101DocumentType documentType)
        {
            if (documentType == null || documentType.DocumentTypeId == 0)
            {
                return BadRequest("Invalid DocumentType data.");
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var existing = await dbContext.Tbl101DocumentTypes
                        .FirstOrDefaultAsync(x => x.DocumentTypeId == documentType.DocumentTypeId);

                    if (existing == null)
                    {
                        return NotFound(new { success = false, message = $"Document Type with ID {documentType.DocumentTypeId} not found." });
                    }

                    var exists = await dbContext.Tbl101DocumentTypes
                        .AnyAsync(x => x.DocumentType == documentType.DocumentType && x.DocumentTypeId != documentType.DocumentTypeId);
                    if (exists)
                    {
                        return BadRequest(new { success = false, message = "Document Type already exists." });
                    }

                    existing.DocumentType = documentType.DocumentType;
                    existing.ReminderDays = documentType.ReminderDays;
                    existing.IsEmployeeDocument = documentType.IsEmployeeDocument;

                    dbContext.Entry(existing).State = EntityState.Modified;
                    await dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Document Type updated successfully." });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in UpdateDocumentType: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDocumentType([FromBody] Tbl101DocumentType documentType)
        { try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var entity = await dbContext.Tbl101DocumentTypes.FindAsync(documentType.DocumentTypeId);
                    if (entity == null)
                    {
                        return NotFound(new { success = false, message = "Document Type not found." });
                    }

                    dbContext.Tbl101DocumentTypes.Remove(entity);
                    await dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Document Type deleted successfully." });
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in DeleteDocumentType: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}

