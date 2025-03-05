using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QDERPWeb.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SalesPersonMasterController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<SalesPersonMasterController> _logger;

        public SalesPersonMasterController(ILogger<SalesPersonMasterController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var tbl20101salespersonmasters = dbContext.Tbl20101SalesPersonMasters.Select(i => new
                {
                    i.SalesPersonCode,
                    i.SalesPersonName,
                    i.UserCode,
                    i.EmailAddress,
                    i.SalesPersonContactNo,
                    i.SalespersonOldCode
                });

                return Json(await DataSourceLoader.LoadAsync(tbl20101salespersonmasters, loadOptions));
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public async Task<IActionResult> Post(string values)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var model = new Tbl20101SalesPersonMaster();
                var valuesDict = JsonConvert.DeserializeObject<IDictionary>(values);
                PopulateModel(model, valuesDict);

                if (!TryValidateModel(model))
                    return BadRequest(GetFullErrorMessage(ModelState));

                var result = dbContext.Tbl20101SalesPersonMasters.Add(model);
                await dbContext.SaveChangesAsync();

                return Json(new { result.Entity.SalesPersonCode });
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPut]
        public async Task<IActionResult> Put(string key, string values)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var model = await dbContext.Tbl20101SalesPersonMasters.FirstOrDefaultAsync(item => item.SalesPersonCode == key);
                if (model == null)
                    return StatusCode(409, "Object not found");

                var valuesDict = JsonConvert.DeserializeObject<IDictionary>(values);
                PopulateModel(model, valuesDict);

                if (!TryValidateModel(model))
                    return BadRequest(GetFullErrorMessage(ModelState));

                await dbContext.SaveChangesAsync();
                return Ok();
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string key)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var model = await dbContext.Tbl20101SalesPersonMasters.FirstOrDefaultAsync(item => item.SalesPersonCode == key);

                if (model == null)
                    return StatusCode(409, "Object not found");

                dbContext.Tbl20101SalesPersonMasters.Remove(model);
                await dbContext.SaveChangesAsync();

                return Ok();
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        private void PopulateModel(Tbl20101SalesPersonMaster model, IDictionary values)
        {
            string SALES_PERSON_CODE = nameof(Tbl20101SalesPersonMaster.SalesPersonCode);
            string SALES_PERSON_NAME = nameof(Tbl20101SalesPersonMaster.SalesPersonName);
            string USER_CODE = nameof(Tbl20101SalesPersonMaster.UserCode);
            string EMAIL_ADDRESS = nameof(Tbl20101SalesPersonMaster.EmailAddress);
            string SALES_PERSON_CONTACT_NO = nameof(Tbl20101SalesPersonMaster.SalesPersonContactNo);
            string SALESPERSON_OLD_CODE = nameof(Tbl20101SalesPersonMaster.SalespersonOldCode);

            if (values.Contains(SALES_PERSON_CODE))
            {
                model.SalesPersonCode = Convert.ToString(values[SALES_PERSON_CODE]);
            }

            if (values.Contains(SALES_PERSON_NAME))
            {
                model.SalesPersonName = Convert.ToString(values[SALES_PERSON_NAME]);
            }

            if (values.Contains(USER_CODE))
            {
                model.UserCode = values[USER_CODE] != null ? Convert.ToByte(values[USER_CODE]) : null;
            }

            if (values.Contains(EMAIL_ADDRESS))
            {
                model.EmailAddress = Convert.ToString(values[EMAIL_ADDRESS]);
            }

            if (values.Contains(SALES_PERSON_CONTACT_NO))
            {
                model.SalesPersonContactNo = Convert.ToString(values[SALES_PERSON_CONTACT_NO]);
            }

            if (values.Contains(SALESPERSON_OLD_CODE))
            {
                model.SalespersonOldCode = Convert.ToString(values[SALESPERSON_OLD_CODE]);
            }
        }

        private string GetFullErrorMessage(ModelStateDictionary modelState)
        {
            var messages = new List<string>();

            foreach (var entry in modelState)
            {
                foreach (var error in entry.Value.Errors)
                    messages.Add(error.ErrorMessage);
            }

            return string.Join(" ", messages);
        }
    }
}










