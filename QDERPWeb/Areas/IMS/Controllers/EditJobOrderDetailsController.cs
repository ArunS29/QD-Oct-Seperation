using DevExpress.CodeParser;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;

namespace QD.ERP.Web.Areas.IMS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class EditJobOrderDetailsController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<EditJobOrderDetailsController> _logger;

        public EditJobOrderDetailsController(ILogger<EditJobOrderDetailsController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetClientDetails(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var clients = dbContext.Tbl30101ClientMasters.Select(i => new
                    {
                        i.ClientCode,
                        i.ClientName,
                        i.ContactPerson,
                        i.ContactMobile1,
                        i.ContactEmail,
                        i.ClientAddress
                    });

                    return Json(await DataSourceLoader.LoadAsync(clients, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetClientDetails: {ex.Message}");
                    return StatusCode(500, new { message = "Error fetching client details", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant", success = false });
        }
        [HttpGet]
        public async Task<IActionResult> GetJobOrderStatus(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var clients = dbContext.Tbl60806jobOrderStatusMasters.Select(i => new
                    {
                        i.JobOrderStatusId,
                        i.JobOrderStatus
                    });

                    return Json(await DataSourceLoader.LoadAsync(clients, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetJobOrderStatus: {ex.Message}");
                    return StatusCode(500, new { message = "Error fetching client details", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant", success = false });
        }
        [HttpGet]
        public async Task<IActionResult> GetCompany(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var clients = dbContext.Tbl901CompanyDetails.Select(i => new
                    {
                        i.CompanyId,
                        i.CompanyName
                    });

                    return Json(await DataSourceLoader.LoadAsync(clients, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetJobOrderStatus: {ex.Message}");
                    return StatusCode(500, new { message = "Error fetching client details", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant", success = false });
        }

        [HttpPost]
        public IActionResult InsertOrUpdate([FromBody] Tbl60801jobOrderMaster jobOrderMaster)
        {
            if (jobOrderMaster == null)
            {
                return BadRequest("Invalid jobOrder data.");
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var existingJobOrder = dbContext.Tbl60801jobOrderMasters
                        .FirstOrDefault(c => c.JobOrderNo == jobOrderMaster.JobOrderNo);

                    if (existingJobOrder != null)
                    {
                        // Update existing record
                        existingJobOrder.JobOrderType = jobOrderMaster.JobOrderType;
                        existingJobOrder.Operator = jobOrderMaster.Operator;
                        existingJobOrder.JobOrderDate = jobOrderMaster.JobOrderDate;
                        existingJobOrder.ClientCode = jobOrderMaster.ClientCode;
                        existingJobOrder.WorkOrderNo = jobOrderMaster.WorkOrderNo;
                        existingJobOrder.JobOrderStatus = jobOrderMaster.JobOrderStatus;
                        existingJobOrder.JobOrderDescription = jobOrderMaster.JobOrderDescription;
                        existingJobOrder.CompanyBranch = jobOrderMaster.CompanyBranch;
                        existingJobOrder.JobOrderType = jobOrderMaster.JobOrderType;
                        existingJobOrder.Operator = jobOrderMaster.Operator;
                        existingJobOrder.ModelNo = jobOrderMaster.ModelNo;
                        existingJobOrder.ClientWorkOrderNo = jobOrderMaster.ClientWorkOrderNo;


                        existingJobOrder.Size = jobOrderMaster.Size;
                        existingJobOrder.Materials = jobOrderMaster.Materials;
                        existingJobOrder.TagNo = jobOrderMaster.TagNo;
                        existingJobOrder.ClientProject = jobOrderMaster.ClientProject;
                        existingJobOrder.Class = jobOrderMaster.Class;
                        existingJobOrder.Make = jobOrderMaster.Make;
                        existingJobOrder.ItemSlNo = jobOrderMaster.ItemSlNo;
                        existingJobOrder.ValveType = jobOrderMaster.ValveType;
                        existingJobOrder.JobDetailedDescription = jobOrderMaster.JobDetailedDescription;


                        existingJobOrder.InitialObsrvDate = jobOrderMaster.InitialObsrvDate;
                        existingJobOrder.InitialObservationNotes = jobOrderMaster.InitialObservationNotes;
                        existingJobOrder.InitialObsrvPreparedBy = jobOrderMaster.InitialObsrvPreparedBy;
                        existingJobOrder.InitialObsrvPreparedOn = jobOrderMaster.InitialObsrvPreparedOn;
                        existingJobOrder.InitialObsrvApprovedBy = jobOrderMaster.InitialObsrvApprovedBy;
                        existingJobOrder.InitialObsrvApprovedOn = jobOrderMaster.InitialObsrvApprovedOn;


                        existingJobOrder.InspectionDate = jobOrderMaster.InspectionDate;
                        existingJobOrder.InspectionPreTested = jobOrderMaster.InspectionPreTested;
                        existingJobOrder.InspectionScope = jobOrderMaster.InspectionScope;
                        existingJobOrder.InspectionNotes = jobOrderMaster.InspectionNotes;
                        existingJobOrder.InspectionPreparedBy = jobOrderMaster.InspectionPreparedBy;
                        existingJobOrder.InspectionPreparedOn = jobOrderMaster.InspectionPreparedOn;
                        existingJobOrder.InspectionApprovedBy = jobOrderMaster.InspectionApprovedBy;
                        existingJobOrder.InspectionApprovedOn = jobOrderMaster.InspectionApprovedOn;


                        existingJobOrder.RepairReportdate = jobOrderMaster.RepairReportdate;
                        existingJobOrder.ResultStandard = jobOrderMaster.ResultStandard;
                        existingJobOrder.ResultShell = jobOrderMaster.ResultShell;
                        existingJobOrder.ResultBackSeat = jobOrderMaster.ResultBackSeat;
                        existingJobOrder.ResultMedia = jobOrderMaster.ResultMedia;
                        existingJobOrder.ResultSeatHp = jobOrderMaster.ResultSeatHp;
                        existingJobOrder.ResultSeatLp = jobOrderMaster.ResultSeatLp;
                        existingJobOrder.ResultShellTime = jobOrderMaster.ResultShellTime;
                        existingJobOrder.ResultSeatLeakage = jobOrderMaster.ResultSeatLeakage;
                        existingJobOrder.ResultSeatTime2 = jobOrderMaster.ResultSeatTime2;


                        existingJobOrder.RepairPreparedBy = jobOrderMaster.RepairPreparedBy;
                        existingJobOrder.RepairPreparedByPosition = jobOrderMaster.RepairPreparedByPosition;
                        existingJobOrder.RepairWitnessedBy = jobOrderMaster.RepairWitnessedBy;
                        existingJobOrder.RepairWitnessedByPosition = jobOrderMaster.RepairWitnessedByPosition;
                        existingJobOrder.RepairPreparedOn = jobOrderMaster.RepairPreparedOn;
                        existingJobOrder.RepairWitnessedOn = jobOrderMaster.RepairWitnessedOn;



                        existingJobOrder.DispatchReportDate = jobOrderMaster.DispatchReportDate;
                        existingJobOrder.DispatchNotes = jobOrderMaster.DispatchNotes;
                        existingJobOrder.DispatchPreparedBy = jobOrderMaster.DispatchPreparedBy;
                        existingJobOrder.DispatchPreparedOn = jobOrderMaster.DispatchPreparedOn;
                        existingJobOrder.DispatchApprovedBy = jobOrderMaster.DispatchApprovedBy;
                        existingJobOrder.DispatchApprovedOn = jobOrderMaster.DispatchApprovedOn;






                        dbContext.Tbl60801jobOrderMasters.Update(existingJobOrder);
                        dbContext.SaveChanges();

                        return Ok(new { success = true, message = "Job Order Data updated successfully." });
                    }
                    else
                    {
                        // Insert new record
                        var newClient = new Tbl60801jobOrderMaster
                        {
                            JobOrderNo = jobOrderMaster.JobOrderNo,

                            JobOrderDate = jobOrderMaster.JobOrderDate,
                            ClientCode = jobOrderMaster.ClientCode,
                            WorkOrderNo = jobOrderMaster.WorkOrderNo,
                            JobOrderStatus = jobOrderMaster.JobOrderStatus,
                            JobOrderDescription = jobOrderMaster.JobOrderDescription,
                            CompanyBranch = jobOrderMaster.CompanyBranch,
                            JobOrderType = jobOrderMaster.JobOrderType,
                            Operator = jobOrderMaster.Operator,
                            ModelNo = jobOrderMaster.ModelNo,
                            ClientWorkOrderNo = jobOrderMaster.ClientWorkOrderNo,
                            Size = jobOrderMaster.Size,
                            Materials = jobOrderMaster.Materials,
                            TagNo = jobOrderMaster.TagNo,
                            ClientProject = jobOrderMaster.ClientProject,
                            Class = jobOrderMaster.Class,
                            Make = jobOrderMaster.Make,
                            ItemSlNo = jobOrderMaster.ItemSlNo,
                            ValveType = jobOrderMaster.ValveType,
                            JobDetailedDescription = jobOrderMaster.JobDetailedDescription,

                            InitialObsrvDate = jobOrderMaster.InitialObsrvDate,
                            InitialObservationNotes = jobOrderMaster.InitialObservationNotes,
                            InitialObsrvPreparedBy = jobOrderMaster.InitialObsrvPreparedBy,
                            InitialObsrvPreparedOn = jobOrderMaster.InitialObsrvPreparedOn,
                            InitialObsrvApprovedBy = jobOrderMaster.InitialObsrvApprovedBy,
                            InitialObsrvApprovedOn = jobOrderMaster.InitialObsrvApprovedOn,

                            InspectionDate = jobOrderMaster.InspectionDate,
                            InspectionPreTested = jobOrderMaster.InspectionPreTested,
                            InspectionScope = jobOrderMaster.InspectionScope,
                            InspectionNotes = jobOrderMaster.InspectionNotes,
                            InspectionPreparedBy = jobOrderMaster.InspectionPreparedBy,
                            InspectionPreparedOn = jobOrderMaster.InspectionPreparedOn,
                            InspectionApprovedBy = jobOrderMaster.InspectionApprovedBy,
                            InspectionApprovedOn = jobOrderMaster.InspectionApprovedOn,

                            RepairReportdate = jobOrderMaster.RepairReportdate,
                            ResultStandard = jobOrderMaster.ResultStandard,
                            ResultShell = jobOrderMaster.ResultShell,
                            ResultBackSeat = jobOrderMaster.ResultBackSeat,
                            ResultMedia = jobOrderMaster.ResultMedia,
                            ResultSeatHp = jobOrderMaster.ResultSeatHp,
                            ResultSeatLp = jobOrderMaster.ResultSeatLp,
                            ResultShellTime = jobOrderMaster.ResultShellTime,
                            ResultSeatLeakage = jobOrderMaster.ResultSeatLeakage,
                            ResultSeatTime2 = jobOrderMaster.ResultSeatTime2,

                            RepairPreparedBy = jobOrderMaster.RepairPreparedBy,
                            RepairPreparedByPosition = jobOrderMaster.RepairPreparedByPosition,
                            RepairWitnessedBy = jobOrderMaster.RepairWitnessedBy,
                            RepairWitnessedByPosition = jobOrderMaster.RepairWitnessedByPosition,
                            RepairPreparedOn = jobOrderMaster.RepairPreparedOn,
                            RepairWitnessedOn = jobOrderMaster.RepairWitnessedOn,



                            DispatchReportDate = jobOrderMaster.DispatchReportDate,
                            DispatchNotes = jobOrderMaster.DispatchNotes,
                            DispatchPreparedBy = jobOrderMaster.DispatchPreparedBy,
                            DispatchPreparedOn = jobOrderMaster.DispatchPreparedOn,
                            DispatchApprovedBy = jobOrderMaster.DispatchApprovedBy,
                            DispatchApprovedOn = jobOrderMaster.DispatchApprovedOn


                        };

                        dbContext.Tbl60801jobOrderMasters.Add(newClient);
                        dbContext.SaveChanges();

                        return Ok(new { success = true, message = "Job Order data saved successfully." });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error saving Job Order: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet]
        public async Task<IActionResult> GetByJobOrderNo(string JobOrderNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                if (string.IsNullOrEmpty(JobOrderNo))
                    return BadRequest("RFQ No is required.");

                try
                {

                    var JobOrder = await dbContext.Tbl60801jobOrderMasters
                        .Where(c => c.JobOrderNo == JobOrderNo)
                        .FirstOrDefaultAsync();

                    if (JobOrder == null)
                        return NotFound("Job Order not found.");

                    return Ok(JobOrder);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
    }
}
