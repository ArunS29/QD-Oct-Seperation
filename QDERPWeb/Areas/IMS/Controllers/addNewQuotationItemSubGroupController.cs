using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.Service;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.DAL.Entities;

namespace QD.ERP.Web.Areas.IMS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class addNewQuotationItemSubGroupController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<addNewQuotationItemSubGroupController> _logger;

        public addNewQuotationItemSubGroupController(ILogger<addNewQuotationItemSubGroupController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetSubGroup()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var SubGroup = await dbContext.Tbl60107quotationChildItemGroups.ToListAsync();
                    return Ok(SubGroup);
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
        public async Task<IActionResult> AddSubGroup([FromBody] Tbl60107quotationChildItemGroup documentType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var exists = await dbContext.Tbl60107quotationChildItemGroups
                        .AnyAsync(x => x.GroupName == documentType.GroupName);
                    if (exists)
                    {
                        return BadRequest(new { success = false, message = "Document Type already exists." });
                    }

                 

                    dbContext.Tbl60107quotationChildItemGroups.Add(documentType);
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
        public async Task<IActionResult> UpdateSubGroup([FromBody] Tbl60107quotationChildItemGroup documentType)
        {
            if (documentType == null || documentType.QuoteGroupItemSlNo == 0)
            {
                return BadRequest("Invalid DocumentType data.");
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var existing = await dbContext.Tbl60107quotationChildItemGroups
                        .FirstOrDefaultAsync(x => x.QuoteGroupItemSlNo == documentType.QuoteGroupItemSlNo);

                    if (existing == null)
                    {
                        return NotFound(new { success = false, message = $"Document Type with ID {documentType.QuoteGroupItemSlNo} not found." });
                    }

                    var exists = await dbContext.Tbl60107quotationChildItemGroups
                        .AnyAsync(x => x.GroupName == documentType.GroupName && x.QuoteGroupItemSlNo != documentType.QuoteGroupItemSlNo);
                    if (exists)
                    {
                        return BadRequest(new { success = false, message = "Sub Group already exists." });
                    }

                    // Update only if the new value is not null
                    if (documentType.GroupCode != null)
                        existing.GroupCode = documentType.GroupCode;

                    if (documentType.GroupName != null)
                        existing.GroupName = documentType.GroupName;

                    if (documentType.GroupRemarks != null)
                        existing.GroupRemarks = documentType.GroupRemarks;

                    if (documentType.IsShowLineItemsTotal.HasValue)
                        existing.IsShowLineItemsTotal = documentType.IsShowLineItemsTotal.Value;

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
        public async Task<IActionResult> DeleteSubGroup([FromBody] Tbl60107quotationChildItemGroup documentType)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var entity = await dbContext.Tbl60107quotationChildItemGroups.FindAsync(documentType.QuoteGroupItemSlNo);
                if (entity == null)
                {
                    return NotFound(new { success = false, message = "Sub Group not found." });
                }

                dbContext.Tbl60107quotationChildItemGroups.Remove(entity);
                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Sub Group deleted successfully." });
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
    }
}
