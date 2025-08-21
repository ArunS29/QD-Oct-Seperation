using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using DevExpress.Printing.Utils.DocumentStoring;
using DevExpress.Pdf;
using System.IO;
using PdfSharpCore.Pdf.IO;
using PdfSharpCore.Pdf;
using System.Collections.Generic;
using System.IO;

namespace QD.ERP.Web.Areas.ERM.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PropertyOperatorController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<PropertyOperatorController> _logger;
        private readonly IConfiguration _configuration;

        public PropertyOperatorController(ILogger<PropertyOperatorController> logger, TenantDbContextHelper tenantDbContextHelper, IConfiguration configuration)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
            _configuration = configuration;
        }
        [HttpGet("GetAllEmployee")]
        public IActionResult GetAllEmployee()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var Employee = dbContext.Qry101EmployeeViews
                        .Select(s => new
                        {
                           s. EmployeeGroup,
                          s. EmployeeId,
                          s.  EmployeeName,
                          s.  NationalityEn,
                          s.NationalId,
                           s. PassportNo,
                           s. IsDiscontinued,
                        })
                        .ToList();

                    return Ok(Employee);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error loading Property Types: {ex.Message}");
                    return StatusCode(500, new { message = "Failed to load Property Types.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant." });
        }
    }
}
