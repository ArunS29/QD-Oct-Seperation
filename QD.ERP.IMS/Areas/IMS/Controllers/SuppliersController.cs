using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.Areas.Finance.Controllers;
using QD.ERP.Shared.DAL.Entities;
using QD.ERP.Web.Service;

namespace QD.ERP.Web.Areas.IMS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SuppliersController : Controller
    {

        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<ClientLeadsController> _logger;

        public SuppliersController(ILogger<ClientLeadsController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var qry = dbContext.Qry30199QrySupplierLists
                        .Select(i => new
                        {
                            i.SupplierCode,
                            i.SupplierName,
                            i.ContactPerson,
                            i.ContactMobile1,
                            i.ContactPhone1,
                            i.ContactPersonTitle,
                            i.SupplierAccountLedgerNo,
                            i.SupplierCategory,
                            
                            DecodedBusinessCard1 = i.BusinessCard1 != null ? $"data:image/png;base64,{Convert.ToBase64String(i.BusinessCard1)}" : null,
                            DecodedBusinessCard2 = i.BusinessCard2 != null ? $"data:image/png;base64,{Convert.ToBase64String(i.BusinessCard2)}" : null,

                        });

                    return Json(await DataSourceLoader.LoadAsync(qry, loadOptions));
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }

    }
}
