using DevExpress.Pdf;
using DevExpress.Printing.Utils.DocumentStoring;
using DevExpress.XtraRichEdit.Import.Html;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using QD.ERP.Web.Areas.Finance.Models;
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
        public class PropertyOperatorModel
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


        [HttpPost]
        public async Task<IActionResult> SaveOrUpdate([FromBody] PropertyOperatorModel model)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant context." });
            }

            if (model == null || string.IsNullOrEmpty(model.EquipmentNo))
            {
                return BadRequest(new { success = false, message = "MPR No. is required." });
            }

            try
            {
                // Check if record already exists (update case)
                var existing = await dbContext.Tbl40107PropertyOperators
                    .FirstOrDefaultAsync(x => x.EquipmentNo == model.EquipmentNo);

                if (existing != null)
                {
                    // 🔹 Update existing record
                    existing.OperatorName = model.OperatorName;
                    existing.OperatorRegHourlyRate = model.OperatorRegHourlyRate;
                    existing.OperatorOthourlyRate = model.OperatorOthourlyRate;
                    existing.WorkStartDate = model.WorkStartDate;
                    existing.WorkEndDate = model.WorkEndDate;
                    existing.OperatorRemarks = model.OperatorRemarks;
                    existing.WorkingShift = model.WorkingShift;

                    dbContext.Tbl40107PropertyOperators.Update(existing);
                    await dbContext.SaveChangesAsync();

                    return Ok(new { message = "Updated successfully!", success = true });
                }
                else
                {
                    var entity = new Tbl40107PropertyOperator
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

                    return Ok(new { message = "Saved successfully!", success = true });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error: " + ex.Message });
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
        [HttpGet("GetByOffHire")]
        public async Task<IActionResult> GetByOffHire(string code)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                if (string.IsNullOrEmpty(code))
                return BadRequest(new { success = false, message = "Invalid property code" });

            var property = await dbContext.Tbl40117PropertyIssuesChilds
                .FirstOrDefaultAsync(p => p.PropertyNo == code);

            if (property == null)
                return NotFound(new { success = false, message = "Property not found" });

            return Ok(new
            {
                property.PropertyIssueNo,
                property.PropertyNo,
                property.QuantityIssued,
                property.Operator,
                property.MeterReading,
                property.MeterReadingHours,
                property.IssueNotes,
                property.PropertyDetailedDescription,
                property.UnitRateMethod,
                property.UnitRate,
                property.UoM,
                property.ClientRatePerHour,
                property.ClientOvertimeRatePerHour,
                property.OffHireNoteRefNo,
                property.ReasonOffHire,
                property.IsReturnOfHire,
                property.ReasonofReturnOfHire,
                property.DemobilizationCharges,
                property.DemobilizationRemarks,
                property.ReplacementPropertyNo,
                property.ReplacementDeliveryNoteNo,
                property.ReplacementCharges,
                property.ReplacementRemarks,
                property.EquipmentDamageCharges,
                property.EquipmentDamageRemarks,
                property.TransportingBackVehicleNo,
                property.TransportingBackVehicleDriverName,
                property.TransportingBackVehicleDriverId,
                property.OffhireMeterReading,
                property.OffhireMeterReadingHours,
                property.ReasonOfReplacement,
                property.OffHireNoteNo,
                property.TransportationBackBy,
                property.HiringRateBasis,
                property.DeliveryNoteMobilizationRate,
                property.DeliveryNoteDemobilizationRate
            });
        }
            [HttpPost]
            public async Task<IActionResult> SaveOrUpdate([FromBody] Tbl40117PropertyIssuesChild property)
            {

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))

                if (property == null)
                    return BadRequest(new { success = false, message = "Invalid data" });

                try
                {

                Tbl40117PropertyIssuesChild entity;

                if (!string.IsNullOrEmpty(property.PropertyNo))
                {
                        entity = await dbContext.Tbl40117PropertyIssuesChilds
                                               .FirstOrDefaultAsync(x => x.PropertyNo == property.PropertyNo);

                        if (entity == null)
                            return NotFound(new { success = false, message = "Record not found" });
                    }
                    else // Insert
                    {
                        entity = new Tbl40117PropertyIssuesChild();
                    dbContext.Tbl40117PropertyIssuesChilds.Add(entity);
                    }

                    // 🔗 Map fields
                    entity.PropertyIssueNo = property.PropertyIssueNo;
                    entity.PropertyNo = property.PropertyNo;
                    entity.QuantityIssued = property.QuantityIssued;
                    entity.Operator = property.Operator;
                    entity.MeterReading = property.MeterReading;
                    entity.MeterReadingHours = property.MeterReadingHours;
                    entity.IssueNotes = property.IssueNotes;
                    entity.PropertyDetailedDescription = property.PropertyDetailedDescription;
                    entity.UnitRateMethod = property.UnitRateMethod;
                    entity.UnitRate = property.UnitRate;
                    entity.UoM = property.UoM;
                    entity.ClientRatePerHour = property.ClientRatePerHour;
                    entity.ClientOvertimeRatePerHour = property.ClientOvertimeRatePerHour;
                    entity.OffHireNoteRefNo = property.OffHireNoteRefNo;
                    entity.ReasonOffHire = property.ReasonOffHire;
                    entity.IsReturnOfHire = property.IsReturnOfHire;
                    entity.ReasonofReturnOfHire = property.ReasonofReturnOfHire;
                    entity.DemobilizationCharges = property.DemobilizationCharges;
                    entity.DemobilizationRemarks = property.DemobilizationRemarks;
                    entity.ReplacementPropertyNo = property.ReplacementPropertyNo;
                    entity.ReplacementDeliveryNoteNo = property.ReplacementDeliveryNoteNo;
                    entity.ReplacementCharges = property.ReplacementCharges;
                    entity.ReplacementRemarks = property.ReplacementRemarks;
                    entity.EquipmentDamageCharges = property.EquipmentDamageCharges;
                    entity.EquipmentDamageRemarks = property.EquipmentDamageRemarks;
                    entity.TransportingBackVehicleNo = property.TransportingBackVehicleNo;
                    entity.TransportingBackVehicleDriverName = property.TransportingBackVehicleDriverName;
                    entity.TransportingBackVehicleDriverId = property.TransportingBackVehicleDriverId;
                    entity.OffhireMeterReading = property.OffhireMeterReading;
                    entity.OffhireMeterReadingHours = property.OffhireMeterReadingHours;
                    entity.ReasonOfReplacement = property.ReasonOfReplacement;
                    entity.OffHireNoteNo = property.OffHireNoteNo;
                    entity.TransportationBackBy = property.TransportationBackBy;
                    entity.HiringRateBasis = property.HiringRateBasis;
                    entity.DeliveryNoteMobilizationRate = property.DeliveryNoteMobilizationRate;
                    entity.DeliveryNoteDemobilizationRate = property.DeliveryNoteDemobilizationRate;

                    await dbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "Saved successfully", data = entity });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { success = false, message = ex.Message });
                }
            }
        }

    }

    
