using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using QDERPWeb.DAL.Entities;

namespace QDERPWeb.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    public class SalesPersonMasterController : Controller
    {
        private ERPMasterWtDataContext _context;

        public SalesPersonMasterController(ERPMasterWtDataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions)
        {
            var tbl20101salespersonmasters = _context.Tbl20101SalesPersonMasters.Select(i => new
            {
                i.SalesPersonCode,
                i.SalesPersonName,
                i.UserCode,
                i.EmailAddress,
                i.SalesPersonContactNo,
                i.SalespersonOldCode
            });

            // If underlying data is a large SQL table, specify PrimaryKey and PaginateViaPrimaryKey.
            // This can make SQL execution plans more efficient.
            // For more detailed information, please refer to this discussion: https://github.com/DevExpress/DevExtreme.AspNet.Data/issues/336.
            // loadOptions.PrimaryKey = new[] { "SalesPersonCode" };
            // loadOptions.PaginateViaPrimaryKey = true;

            return Json(await DataSourceLoader.LoadAsync(tbl20101salespersonmasters, loadOptions));
        }

        [HttpPost]
        public async Task<IActionResult> Post(string values)
        {
            var model = new Tbl20101SalesPersonMaster();
            var valuesDict = JsonConvert.DeserializeObject<IDictionary>(values);
            PopulateModel(model, valuesDict);

            if (!TryValidateModel(model))
                return BadRequest(GetFullErrorMessage(ModelState));

            var result = _context.Tbl20101SalesPersonMasters.Add(model);
            await _context.SaveChangesAsync();

            return Json(new { result.Entity.SalesPersonCode });
        }

        [HttpPut]
        public async Task<IActionResult> Put(string key, string values)
        {
            var model = await _context.Tbl20101SalesPersonMasters.FirstOrDefaultAsync(item => item.SalesPersonCode == key);
            if (model == null)
                return StatusCode(409, "Object not found");

            var valuesDict = JsonConvert.DeserializeObject<IDictionary>(values);
            PopulateModel(model, valuesDict);

            if (!TryValidateModel(model))
                return BadRequest(GetFullErrorMessage(ModelState));

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete]
        public async Task Delete(string key)
        {
            var model = await _context.Tbl20101SalesPersonMasters.FirstOrDefaultAsync(item => item.SalesPersonCode == key);

            _context.Tbl20101SalesPersonMasters.Remove(model);
            await _context.SaveChangesAsync();
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