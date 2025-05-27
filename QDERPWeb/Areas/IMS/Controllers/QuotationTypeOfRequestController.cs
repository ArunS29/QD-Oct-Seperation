using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;

namespace QD.ERP.Web.Areas.IMS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class QuotationTypeOfRequestController : Controller
    {

        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<QuotationTypeOfRequestController> _logger;

        public QuotationTypeOfRequestController(ILogger<QuotationTypeOfRequestController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetTypeofRequest()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var dbSignatories = await dbContext.Tbl30104TypeOfRequestMasters
                       .Select(s => new
                       {
                          s.TypeOfRequest,
                          s.TermsCategory,
                          s.TypeOfRequestId

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
        public async Task<IActionResult> SaveOrUpdateSignatory([FromBody] Tbl30104TypeOfRequestMaster model)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var now = DateTime.Now;

                    var existing = await dbContext.Tbl30104TypeOfRequestMasters
                        .FirstOrDefaultAsync(x => x.TypeOfRequestId == model.TypeOfRequestId
);

                    if (existing != null)
                    {
                        existing.TypeOfRequest = model.TypeOfRequest;
                        existing.TermsCategory = model.TermsCategory;
                  
                    }
                    else
                    {
                        // Assign new SignatoryId
                        var lastId = await dbContext.Tbl30104TypeOfRequestMasters
                            .OrderByDescending(x => x.TypeOfRequestId)
                            .Select(x => x.TypeOfRequestId)
                            .FirstOrDefaultAsync();

                        model.TypeOfRequestId = (byte)(lastId + 1); // Assuming short type

                        dbContext.Tbl30104TypeOfRequestMasters.Add(model);
                    }

                    await dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Saved successfully", id = model.TypeOfRequestId });
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
