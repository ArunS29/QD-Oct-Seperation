using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.Areas.IMS.Controllers;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;

namespace QD.ERP.Web.Areas.ERM.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class EquipmentAssetsController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<EquipmentAssetsController> _logger;

        public EquipmentAssetsController(ILogger<EquipmentAssetsController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }
        [HttpGet]
        //public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions)
        //{
        //    try
        //    {
        //        if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
        //        {
        //            var qry = dbContext.Qry40102PropertyMasterView2s
        //                .Select(i => new
        //                {
        //                    i.SupplierCode,
        //                    i.SupplierName,
        //                    i.PlateNo,
        //                    i.Model,
        //                    i.Capacity,
        //                    i.PropertyType,
        //                    i.MobilizedTo,
        //                    i.MobilizedOn,
        //                    i.ClientRatePerHour,
        //                    i.SupplierPono,
        //                    i.SupplierRefNo,
        //                    i.HiredOn,
        //                    i.CurrentStatus,
        //                    i.PropertyNo,
        //                    i.EquipmentOperatorId,
        //                    i.EquipmentOperatorName,
        //                    i.OperatorWorkStartDate,
        //                    i.PropertyDescription,
        //                    i.ChassisNo,
        //                    i.PropertyGroup,
        //                    i.PropertyCategoryName,
        //                    i.BuyingRatePerHour,
        //                    i.BuyingRatePerMonth







        //                });

        //            return Json(await DataSourceLoader.LoadAsync(qry, loadOptions));
        //        }

        //        return Unauthorized(new { message = "Invalid tenant.", success = false });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Error in GetProject: {ex.Message}");
        //        return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
        //    }
        //}


        public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var qry = dbContext.Qry40102PropertyMasterView2s
                        .Select(i => new
                        {
                            i.SupplierCode,
                            i.SupplierName,
                            i.PlateNo,
                            i.Model,
                            i.Capacity,
                            i.PropertyType,
                            i.MobilizedTo,
                            i.MobilizedOn,
                            i.ClientRatePerHour,
                            i.SupplierPono,
                            i.SupplierRefNo,
                            i.HiredOn,
                            i.CurrentStatus,
                            i.PropertyNo,
                            i.EquipmentOperatorId,
                            i.EquipmentOperatorName,
                            i.OperatorWorkStartDate,
                            i.PropertyDescription,
                            i.ChassisNo,
                            i.PropertyGroup,
                            i.PropertyCategoryName,
                            i.BuyingRatePerHour,
                            i.BuyingRatePerMonth

                        });

                    return Json(await DataSourceLoader.LoadAsync(qry, loadOptions));
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Get: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }

    }
}
