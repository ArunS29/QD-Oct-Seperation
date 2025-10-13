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
//using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Shared.DAL.Entities;
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
            public string EmployeeId { get; set; }
            public byte? PropertyOperatorTypeId { get; set; }
            public long PropertyOperatorCode { get; set; }
        }


        //[HttpPost]
        //public async Task<IActionResult> SaveOrUpdate([FromBody] PropertyOperatorModel model)
        //{
        //    if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
        //    {
        //        return Unauthorized(new { success = false, message = "Invalid tenant context." });
        //    }

        //    if (model == null || string.IsNullOrEmpty(model.EquipmentNo))
        //    {
        //        return BadRequest(new { success = false, message = "Equipment No. is required." });
        //    }

        //    try
        //    {
        //        // Check if record already exists (update case)
        //        var existing = await dbContext.Tbl40107PropertyOperators
        //            .FirstOrDefaultAsync(x => x.EquipmentNo == model.EquipmentNo);

        //        if (existing != null)
        //        {
        //            existing.OperatorName = model.OperatorName;
        //            existing.OperatorRegHourlyRate = model.OperatorRegHourlyRate;
        //            existing.OperatorOthourlyRate = model.OperatorOthourlyRate;
        //            existing.WorkStartDate = model.WorkStartDate;
        //            existing.WorkEndDate = model.WorkEndDate;
        //            existing.OperatorRemarks = model.OperatorRemarks;
        //            existing.WorkingShift = model.WorkingShift;

        //            dbContext.Tbl40107PropertyOperators.Update(existing);
        //            await dbContext.SaveChangesAsync();

        //            return Ok(new { message = "Updated successfully!", success = true });
        //        }
        //        else
        //        {
        //            var entity = new Tbl40107PropertyOperator
        //            {
        //                EquipmentNo = model.EquipmentNo,
        //                OperatorName = model.OperatorName,
        //                OperatorRegHourlyRate = model.OperatorRegHourlyRate,
        //                OperatorOthourlyRate = model.OperatorOthourlyRate,
        //                WorkStartDate = model.WorkStartDate,
        //                WorkEndDate = model.WorkEndDate,
        //                OperatorRemarks = model.OperatorRemarks,
        //                WorkingShift = model.WorkingShift
        //            };
        //            dbContext.Tbl40107PropertyOperators.Add(entity);
        //            await dbContext.SaveChangesAsync();

        //            return Ok(new { message = "Saved successfully!", success = true });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { success = false, message = "Error: " + ex.Message });
        //    }
        //}

        [HttpPost]
        public IActionResult SaveOrUpdate([FromBody] PropertyOperatorModel model)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant context." });
            }

            if (model == null || string.IsNullOrEmpty(model.EquipmentNo))
            {
                return BadRequest(new { success = false, message = "Equipment No. is required." });
            }

            try
            {
                if (model.PropertyOperatorCode > 0)
                {
                    // ✅ UPDATE LOGIC

                    // Find existing record
                    var existing = dbContext.Tbl40107PropertyOperators
                        .FirstOrDefault(o => o.PropertyOperatorCode == model.PropertyOperatorCode);

                    if (existing == null)
                    {
                        return NotFound(new { success = false, message = "Record not found." });
                    }

                    // Check for duplicates against other records
                    bool duplicateExists = dbContext.Tbl40107PropertyOperators
                        .Any(o => o.EquipmentNo == model.EquipmentNo
                               && o.EmployeeId == model.EmployeeId
                               && o.PropertyOperatorCode != model.PropertyOperatorCode);

                    if (duplicateExists)
                    {
                        return Ok(new
                        {
                            success = false,
                            message = "You cannot assign. This Equipment/Property already has the same operator assigned."
                        });
                    }

                    // Update fields
                    existing.EquipmentNo = model.EquipmentNo;
                    existing.PropertyOperatorTypeId = model.PropertyOperatorTypeId;
                    existing.OperatorName = model.OperatorName;
                    existing.OperatorRegHourlyRate = model.OperatorRegHourlyRate;
                    existing.OperatorOthourlyRate = model.OperatorOthourlyRate;
                    existing.WorkStartDate = model.WorkStartDate;
                    existing.WorkEndDate = model.WorkEndDate;
                    existing.OperatorRemarks = model.OperatorRemarks;
                    existing.WorkingShift = model.WorkingShift;
                    existing.EmployeeId = model.EmployeeId;

                    dbContext.SaveChanges();

                    return Ok(new
                    {
                        success = true,
                        message = "Property Operator updated successfully",
                        propertyOperatorTypeId = existing.PropertyOperatorTypeId
                    });
                }
                else
                {
                    // ✅ INSERT LOGIC

                    // Check if Employee is already assigned to the same Equipment
                    bool duplicateExists = dbContext.Tbl40107PropertyOperators
                        .Any(o => o.EquipmentNo == model.EquipmentNo && o.EmployeeId == model.EmployeeId);

                    if (duplicateExists)
                    {
                        return Ok(new
                        {
                            success = false,
                            message = "You cannot assign. This Equipment/Property already has the same operator assigned."
                        });
                    }

                    // Create new record
                    var entity = new Tbl40107PropertyOperator
                    {
                        EquipmentNo = model.EquipmentNo,
                        PropertyOperatorTypeId = model.PropertyOperatorTypeId,
                        OperatorName = model.OperatorName,
                        OperatorRegHourlyRate = model.OperatorRegHourlyRate,
                        OperatorOthourlyRate = model.OperatorOthourlyRate,
                        WorkStartDate = model.WorkStartDate,
                        WorkEndDate = model.WorkEndDate,
                        OperatorRemarks = model.OperatorRemarks,
                        WorkingShift = model.WorkingShift,
                        EmployeeId = model.EmployeeId
                    };

                    dbContext.Tbl40107PropertyOperators.Add(entity);
                    dbContext.SaveChanges();

                    return Ok(new
                    {
                        success = true,
                        message = "Property Operator saved successfully",
                        propertyOperatorTypeId = entity.PropertyOperatorTypeId
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in SaveOrUpdate PropertyOperator: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Error saving data: " + ex.Message });
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
                    x.WorkingShift,
                    x.PropertyOperatorTypeId,
                    x.EquipmentNo,
                    x.PropertyOperatorCode
                })
                .ToListAsync();
            data = data != null ? data : [];
            return Ok(data);
        }

     
        [HttpGet("{code}")]
        public async Task<IActionResult> GetByOffHire(string code)
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    return BadRequest(new { success = false, message = "Failed to establish database connection." });
                }

                if (string.IsNullOrEmpty(code))
                {
                    return BadRequest(new { success = false, message = "Invalid property code." });
                }

                var property = await dbContext.Tbl40117PropertyIssuesChilds
                    .FirstOrDefaultAsync(p => p.PropertyNo == code);

                if (property == null)
                {
                    return NotFound(new { success = false, message = "Property not found." });
                }

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

        //[HttpGet("{equipmentNo}")]
        //public IActionResult GetOperatorsByEquipment(string equipmentNo)
        //{
        //    if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
        //    {
        //        return BadRequest(new { success = false, message = "Failed to establish database connection." });
        //    }

        //    if (string.IsNullOrEmpty(equipmentNo))
        //    {
        //        return BadRequest(new { success = false, message = "Equipment number is required." });
        //    }
        //    var operators = dbContext.Tbl40107PropertyOperators
        //    .Where(o => o.EquipmentNo == equipmentNo)  
        //    .Select(o => new
        //    {
        //        o.PropertyOperatorCode,
        //        o.EmployeeId,
        //        o.OperatorName,
        //        o.WorkStartDate,
        //        o.WorkEndDate,
        //        o.OperatorRegHourlyRate,
        //        o.OperatorOthourlyRate,
        //        o.WorkingShift,
        //        o.PropertyOperatorTypeId,
        //        o.EquipmentNo
        //    })
        //    .ToList();
        //    //var operators = dbContext.Tbl40107PropertyOperators
        //    //    .Where(o => o.EquipmentNo == equipmentNo)
        //    //    .ToList();
        //    return Ok(operators);
        //}
        [HttpGet]
        public async Task<IActionResult> GetOperators([FromQuery] string equipmentNo, [FromQuery] long propertyOperatorCode)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return BadRequest(new { success = false, message = "Failed to establish database connection." });
            }

            if (string.IsNullOrEmpty(equipmentNo))
            {
                return BadRequest(new { success = false, message = "Equipment number is required." });
            }

            var operators = dbContext.Tbl40107PropertyOperators
                .Where(o => o.EquipmentNo == equipmentNo && o.PropertyOperatorCode == propertyOperatorCode)
                .Select(o => new
                {
                    o.EmployeeId,
                    o.OperatorName,
                    o.WorkStartDate,
                    o.WorkEndDate,
                    o.OperatorRegHourlyRate,
                    o.OperatorOthourlyRate,
                    o.WorkingShift,
                    o.PropertyOperatorTypeId,
                    o.EquipmentNo,
                    o.PropertyOperatorCode,
                    o.OperatorRemarks
                })
        .ToList();


            return Ok(operators);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrSave([FromBody] Tbl40117PropertyIssuesChild model)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(
                out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return BadRequest(new { success = false, message = "Failed to establish database connection." });
            }

            if (model == null)
                return BadRequest(new { success = false, message = "Invalid data." });

            // Insert (new record if PK = 0)
            if (model.PropertyIssueChildSlNo == 0)
            {
                var entity = new Tbl40117PropertyIssuesChild
                {
                    PropertyIssueNo = model.PropertyIssueNo,
                    PropertyNo = model.PropertyNo,
                    QuantityIssued = model.QuantityIssued,
                    Operator = model.Operator,
                    MeterReading = model.MeterReading,
                    MeterReadingHours = model.MeterReadingHours,
                    IssueNotes = model.IssueNotes,
                    PropertyDetailedDescription = model.PropertyDetailedDescription,
                    UnitRateMethod = model.UnitRateMethod,
                    UnitRate = model.UnitRate,
                    UoM = model.UoM,
                    ClientRatePerHour = model.ClientRatePerHour,
                    ClientOvertimeRatePerHour = model.ClientOvertimeRatePerHour,
                    OffHireNoteRefNo = model.OffHireNoteRefNo,
                    ReasonOffHire = model.ReasonOffHire,
                    IsReturnOfHire = model.IsReturnOfHire,
                    ReasonofReturnOfHire = model.ReasonofReturnOfHire,
                    DemobilizationCharges = model.DemobilizationCharges,
                    DemobilizationRemarks = model.DemobilizationRemarks,
                    ReplacementPropertyNo = model.ReplacementPropertyNo,
                    ReplacementDeliveryNoteNo = model.ReplacementDeliveryNoteNo,
                    ReplacementCharges = model.ReplacementCharges,
                    ReplacementRemarks = model.ReplacementRemarks,
                    EquipmentDamageCharges = model.EquipmentDamageCharges,
                    EquipmentDamageRemarks = model.EquipmentDamageRemarks,
                    TransportingBackVehicleNo = model.TransportingBackVehicleNo,
                    TransportingBackVehicleDriverName = model.TransportingBackVehicleDriverName,
                    TransportingBackVehicleDriverId = model.TransportingBackVehicleDriverId,
                    OffhireMeterReading = model.OffhireMeterReading,
                    OffhireMeterReadingHours = model.OffhireMeterReadingHours,
                    ReasonOfReplacement = model.ReasonOfReplacement,
                    OffHireNoteNo = model.OffHireNoteNo,
                    TransportationBackBy = model.TransportationBackBy,
                    OffhireDetailsAddedOn = model.OffhireDetailsAddedOn,
                    HiringRateBasis = model.HiringRateBasis,
                    DeliveryNoteMobilizationRate = model.DeliveryNoteMobilizationRate,
                    DeliveryNoteDemobilizationRate = model.DeliveryNoteDemobilizationRate,
                    AddedBy = "system", // 🔹 replace with user from context
                    AddedOn = DateTime.Now
                };

                dbContext.Tbl40117PropertyIssuesChilds.Add(entity);
                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Record created successfully.", id = entity.PropertyIssueChildSlNo });
            }
            else
            {
                // Update
                var entity = await dbContext.Tbl40117PropertyIssuesChilds
                    .FirstOrDefaultAsync(p => p.PropertyIssueChildSlNo == model.PropertyIssueChildSlNo);

                if (entity == null)
                    return NotFound(new { success = false, message = "Record not found." });

                // Update fields
                entity.PropertyIssueNo = model.PropertyIssueNo;
                entity.PropertyNo = model.PropertyNo;
                entity.QuantityIssued = model.QuantityIssued;
                entity.Operator = model.Operator;
                entity.MeterReading = model.MeterReading;
                entity.MeterReadingHours = model.MeterReadingHours;
                entity.IssueNotes = model.IssueNotes;
                entity.PropertyDetailedDescription = model.PropertyDetailedDescription;
                entity.UnitRateMethod = model.UnitRateMethod;
                entity.UnitRate = model.UnitRate;
                entity.UoM = model.UoM;
                entity.ClientRatePerHour = model.ClientRatePerHour;
                entity.ClientOvertimeRatePerHour = model.ClientOvertimeRatePerHour;
                entity.OffHireNoteRefNo = model.OffHireNoteRefNo;
                entity.ReasonOffHire = model.ReasonOffHire;
                entity.IsReturnOfHire = model.IsReturnOfHire;
                entity.ReasonofReturnOfHire = model.ReasonofReturnOfHire;
                entity.DemobilizationCharges = model.DemobilizationCharges;
                entity.DemobilizationRemarks = model.DemobilizationRemarks;
                entity.ReplacementPropertyNo = model.ReplacementPropertyNo;
                entity.ReplacementDeliveryNoteNo = model.ReplacementDeliveryNoteNo;
                entity.ReplacementCharges = model.ReplacementCharges;
                entity.ReplacementRemarks = model.ReplacementRemarks;
                entity.EquipmentDamageCharges = model.EquipmentDamageCharges;
                entity.EquipmentDamageRemarks = model.EquipmentDamageRemarks;
                entity.TransportingBackVehicleNo = model.TransportingBackVehicleNo;
                entity.TransportingBackVehicleDriverName = model.TransportingBackVehicleDriverName;
                entity.TransportingBackVehicleDriverId = model.TransportingBackVehicleDriverId;
                entity.OffhireMeterReading = model.OffhireMeterReading;
                entity.OffhireMeterReadingHours = model.OffhireMeterReadingHours;
                entity.ReasonOfReplacement = model.ReasonOfReplacement;
                entity.OffHireNoteNo = model.OffHireNoteNo;
                entity.TransportationBackBy = model.TransportationBackBy;
                entity.HiringRateBasis = model.HiringRateBasis;
                entity.DeliveryNoteMobilizationRate = model.DeliveryNoteMobilizationRate;
                entity.DeliveryNoteDemobilizationRate = model.DeliveryNoteDemobilizationRate;
                entity.ModifiedBy = "system"; // 🔹 replace with user from context
                entity.ModifiedOn = DateTime.Now;

                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Record updated successfully.", id = entity.PropertyIssueChildSlNo });
            }
        }
        [HttpGet("GetAllPropertyTypes")]
        public IActionResult GetAllPropertyTypes()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var OperatorTypes = dbContext.Tbl40138PropertyOperatorTypes
                        .Select(s => new
                        {
                            s.PropertyOperatorTypeId,
                            s.OperatorType
                        })
                        .ToList();

                    return Ok(OperatorTypes);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error loading Property Types: {ex.Message}");
                    return StatusCode(500, new { message = "Failed to load Property Types.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant." });
        }

        [HttpDelete]
        public IActionResult Delete(string EmployeeId, string EquipmentNo)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Json(new { success = false, message = "Invalid tenant context" });
            }

            if (string.IsNullOrEmpty(EmployeeId) || string.IsNullOrEmpty(EquipmentNo))
            {
                return Json(new { success = false, message = "EmployeeId and EquipmentNo are required" });
            }

            // Find the specific record with BOTH EquipmentNo and EmployeeId
            var entity = dbContext.Tbl40107PropertyOperators
                .FirstOrDefault(x => x.EquipmentNo == EquipmentNo && x.EmployeeId == EmployeeId);

            if (entity == null)
                return Json(new { success = false, message = "Record not found" });

            dbContext.Tbl40107PropertyOperators.Remove(entity);
            dbContext.SaveChanges();

            return Json(new { success = true, message = "Record deleted successfully" });
        }
    }
}
