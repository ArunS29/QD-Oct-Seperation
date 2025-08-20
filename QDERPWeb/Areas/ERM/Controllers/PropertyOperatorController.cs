using DevExpress.Pdf;
using DevExpress.Printing.Utils.DocumentStoring;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using SkiaSharp;
using System.Collections.Generic;
using System.IO;
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
                            s.EmployeeGroup,
                            s.EmployeeId,
                            s.EmployeeName,
                            s.NationalityEn,
                            s.NationalId,
                            s.PassportNo,
                            s.IsDiscontinued,
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
        [HttpGet("GetByNo")]
        public async Task<IActionResult> GetByNo([FromQuery] string code)
        {
            if (string.IsNullOrEmpty(code))
                return BadRequest(new { message = "Code is required" });

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { message = "Invalid tenant" });

            var entity = await dbContext.Tbl40107PropertyOperators
                .FirstOrDefaultAsync(x => x.EquipmentNo == code);

            if (entity == null)
                return NotFound(new { message = "Record not found" });

            return Ok(new
            {
                entity.EquipmentNo,
                entity.OperatorName,
                entity.OperatorRegHourlyRate,
                entity.OperatorOthourlyRate,
                entity.WorkStartDate,
                entity.WorkEndDate,
                entity.OperatorRemarks,
                entity.WorkingShift
            });
        }
        public class PropertyOperatorDto
        {
            public string EquipmentNo { get; set; }
            public string OperatorName { get; set; }
            public decimal OperatorRegHourlyRate { get; set; }
            public decimal OperatorOthourlyRate { get; set; }
            public DateTime? WorkStartDate { get; set; }
            public DateTime? WorkEndDate { get; set; }
            public string OperatorRemarks { get; set; }
            public string WorkingShift { get; set; }
        }

        [HttpPost("SaveOrUpdate")]
        public async Task<IActionResult> SaveOrUpdate([FromBody] PropertyOperatorDto model)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { message = "Invalid tenant" });

            if (string.IsNullOrEmpty(model.EquipmentNo))
                return BadRequest(new { message = "EquipmentNo is required" });

            var entity = await dbContext.Tbl40107PropertyOperators
                .FirstOrDefaultAsync(x => x.EquipmentNo == model.EquipmentNo);

            if (entity == null)
            {
                // Insert new record
                entity = new Tbl40107PropertyOperator
                {
                    EquipmentNo = model.EquipmentNo,
                    OperatorName = model.OperatorName,
                    OperatorRegHourlyRate = model.OperatorRegHourlyRate,
                    OperatorOthourlyRate = model.OperatorOthourlyRate,
                    WorkStartDate = model.WorkStartDate,
                    WorkEndDate = model.WorkEndDate,
                    OperatorRemarks = model.OperatorRemarks,
                    WorkingShift = model.WorkingShift
                };
                dbContext.Tbl40107PropertyOperators.Add(entity);
                await dbContext.SaveChangesAsync();
                return Ok(new { message = "Saved successfully" });
            }
            else
            {
                // Update existing record
                entity.OperatorName = model.OperatorName;
                entity.OperatorRegHourlyRate = model.OperatorRegHourlyRate;
                entity.OperatorOthourlyRate = model.OperatorOthourlyRate;
                entity.WorkStartDate = model.WorkStartDate;
                entity.WorkEndDate = model.WorkEndDate;
                entity.OperatorRemarks = model.OperatorRemarks;
                entity.WorkingShift = model.WorkingShift;

                await dbContext.SaveChangesAsync();
                return Ok(new { message = "Updated successfully" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetByEquipmentNo(string equipmentNo)
        {
            //if (string.IsNullOrEmpty(equipmentNo))
            //    return BadRequest("EquipmentNo is required");

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized("Invalid tenant");

            var data = await dbContext.Tbl40107PropertyOperators
                .Where(x => x.EquipmentNo == equipmentNo)
                .Select(x => new {
                    x.EmployeeId,
                    x.OperatorName,
                    x.WorkStartDate,
                    x.WorkEndDate,
                    x.OperatorRegHourlyRate,
                    x.OperatorOthourlyRate,
                    x.WorkingShift
                })
                .ToListAsync();
            data = data != null ? data : [];
            return Ok(data);
        }

        [HttpGet("GetAllOperatorTypes")]
        public IActionResult GetAllOperatorTypes()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var OperatorType = dbContext.Tbl40138PropertyOperatorTypes
                        .Select(s => new
                        {
                            s.PropertyOperatorTypeId,
                            s.OperatorType
                        })
                        .ToList();

                    return Ok(OperatorType);
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

