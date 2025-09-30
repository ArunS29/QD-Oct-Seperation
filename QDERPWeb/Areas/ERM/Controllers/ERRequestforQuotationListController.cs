using System;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.AspNet.Mvc;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.Areas.Finance.Reports.Payable_Statements;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using static Org.BouncyCastle.Math.EC.ECCurve;


namespace QD.ERP.Web.Areas.ERM.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ERRequestforQuotationListController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<ERRequestforQuotationListController> _logger;
        private readonly IDbConnection _db;

        public ERRequestforQuotationListController(ILogger<ERRequestforQuotationListController> logger, TenantDbContextHelper tenantDbContextHelper, IConfiguration config)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
            _db = new SqlConnection(config.GetConnectionString("DefaultConnection"));
        }
        [HttpGet]
        public async Task<IActionResult> GetRFQRequest(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var query = dbContext.Qry60704rfqviewMasters.AsQueryable();


                    // Default dates if not provided
                    if (!fromDate.HasValue)
                    {
                        fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1); // Start of the current month
                    }

                    if (!toDate.HasValue)
                    {
                        toDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)); // End of the current month
                    }

                    // Filtering by date range
                    query = query.Where(i => i.Rfqdate >= fromDate && i.Rfqdate <= toDate);

                    // Fetching the data
                    var data = await query.Select(i => new
                    {
                        i.Rfqno,
                        i.Rfqdate,
                        i.Mprno,
                        i.SupplierName,
                        i.SupplierQuotationNo,
                        i.QuoteHasItemsToPo,
                        i.Pono,
                        i.NoOfItems,
                        i.TotalBeforeTax,
                        i.TotalDiscount,
                        i.TotalAfterDiscount,
                    }).ToListAsync();

                    return Json(data);
                }

                return Unauthorized(new { message = "Invalid tenant." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
            }
        }
        //RFQ Item Details form
        [HttpGet]
        public async Task<IActionResult> GetGenarateTimeGrid(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var query = dbContext.Qry40305equipmentTsmasterViews.AsQueryable();

                    // Default dates if not provided
                    if (!fromDate.HasValue)
                        fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

                    if (!toDate.HasValue)
                        toDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month,
                                  DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));

                    // Filter by date
                    query = query.Where(i => i.TimeSheetMonth >= fromDate && i.TimeSheetMonth <= toDate);

                    // Group by Year + Month
                    var rawData = await query
                    .Where(i => i.TimeSheetMonth >= fromDate && i.TimeSheetMonth <= toDate)
                    .ToListAsync(); // materialize first

                    var data = rawData
                        .GroupBy(i => new { Year = i.TimeSheetMonth.Value.Year, Month = i.TimeSheetMonth.Value.Month })
                        .Select(g => new
                        {
                            g.Key.Year,
                            g.Key.Month,
                            Records = g.Select(i => new
                            {
                                i.PropertyGroup,
                                i.ClientName,
                                i.ClientPono,
                                i.PropertyNo,
                                i.PropertyDescription,
                                i.SupplierTotalHours,
                                i.ClientRatePerHour2,
                                i.ClientTotalAmount,
                                i.SupplierRatePerHour,
                                i.SupplierTotalAmount,
                                i.PropertySummInvoiceNo,
                                i.SupplierTssummaryNo,
                                i.Deductions,
                                i.TimeSheetMasterId,
                                i.TimeSheetMonthId,
                                i.TimeSheetMonth,
                                i.PropertyType,
                                i.PropertySuppliedBy,
                                i.OperatorName,
                                i.OperatorRate,
                                i.HiredOn,
                                i.PropertyCategoryName,
                                i.Brand,
                                i.PlateNo,
                                i.TotalRentAmount,
                                i.TotalHoursWorked,
                                i.ClientTotalHours,
                                i.ClientOtratePerHour,
                                i.Project,
                                i.ProjectDescription,
                                i.ClientAccountLedgerNo,
                                i.SupplierAccountLedgerNo,
                                i.GatePassNo,
                                i.ClientSite

                            }).ToList()
                        })
                        .OrderBy(x => x.Year)
                        .ThenBy(x => x.Month)
                        .ToList();

                    return Json(data);
                }

                return Unauthorized(new { message = "Invalid tenant." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetTimeSheetReq: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTimesheetGrid(DataSourceLoadOptions loadOptions, string propertyNo, int? timeSheetMaster)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var query = dbContext.Qry40125PropertyTimeSheetDatesEdits
                        .Where(x => x.PropertyNo == propertyNo);

                    if (timeSheetMaster.HasValue)
                        query = query.Where(x => x.TimeSheetMasterId == timeSheetMaster.Value);

                    // 🔹 Project all fields
                    var projectedQuery = query.Select(x => new
                    {
                        x.TimeSheetId,
                        x.PropertyNo,
                        x.TimeSheetMasterId,
                        x.TimeSheetDate,
                        x.StartTimeShift1,
                        x.EndTimeShift1,
                        x.StartTimeShift2,
                        x.EndTimeShift2,
                        x.BasicWorkHours,
                        x.WorkStatus,
                        x.BasicWorkMinutes,
                        x.ProjectId,
                        x.BonusMinutes,
                        x.TotalHoursM,
                        x.ClientCode,
                        x.ClientName,
                        x.ClientRatePerHour,
                        x.PropertyIssueNo,
                        x.ClientOtratePerHour,
                        x.ClientRegHours,
                        x.ClientOthours,
                        x.SupplierCode,
                        x.SuppRatePerHour,
                        x.SuppOtratePerHour,
                        x.SuppRegHoursInMin,
                        x.SuppOthoursInMin,
                        x.OperatorCode,
                        x.OprtRatePerHour,
                        x.OprtOtratePerHour,
                        x.OprtRegHoursInMin,
                        x.OprtOthoursInMin,
                        x.SupplierName,
                        x.EmployeeName,
                        x.SupplierTotalHours,
                        x.OprtTotalHours,
                        x.NightOprtCode,
                        x.NightOprtRatePerHour,
                        x.NightOprtOtratePerHour,
                        x.NightOprtRegHours,
                        x.NightOprtOthours,
                        x.NightOprtTotalHours,
                        x.NightOperatorName
                    });

                    return Json(await DataSourceLoader.LoadAsync(projectedQuery, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetTimesheetGrid: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }


        [HttpGet]
        public IActionResult GenerateTimeGrid(string frmDate, string toDate)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { message = "Invalid tenant.", success = false });

            try
            {
                // Parse input dates
                if (!DateTime.TryParseExact(frmDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime from))
                    return BadRequest("Select the date");

                if (!DateTime.TryParseExact(toDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime to))
                    return BadRequest("Invalid to date format. Use MM/dd/yyyy.");

                // Fetch data directly from the view and filter by PropertyIssuedDate
                var filteredData = dbContext.Qry40102PropertyMasterView2s
                    .Select(c => new

                    {

                        c.PropertyNo,
                        c.PlateNo,
                        c.Location,
                        c.ClientRatePerHour,
                        c.SupplierName,
                        c.PropertyDescription,
                        c.PropertyCategory,
                        c.Specifications,
                        c.Brand,
                        c.DoorNo,
                        c.ChassisNo,
                        c.Color,
                        c.Capacity,
                        c.Model,
                        c.Year,
                        c.Ownership,
                        c.PurchaseDate,
                        c.PurchasedAs,
                        c.BuyingRatePerHour,
                        c.BuyingRatePerDay,
                        c.BuyingRatePerMonth,
                        c.SellingRatePerHour,
                        c.SellingRatePerDay,
                        c.SellingRatePerMonth,
                        c.PropertyCondition,
                        c.IsFinanced,
                        c.FinancedFrom,
                        c.ValueOfProperty,
                        c.InitialDownPayment,
                        c.InitialDocCharges,
                        c.MonthlyInstallment,
                        c.NoOfInstallments,
                        c.InstallmentStartDate,
                        c.InstallmentEndDate,
                        c.FinalInstallment,
                        c.DepreciationMethod,
                        c.LifeSpanOfProperty,
                        c.ScrapValueOfProperty,
                        c.CreatedBy,
                        c.CreatedOn,
                        c.ModifiedBy,
                        c.ModifiedOn,
                        c.IsDiscontinued,
                        c.PropertyCategoryName,
                        c.DiscontinuedOn,
                        c.DiscontinuedRemarks,
                        c.ModelType,
                        c.Weight,
                        c.LxWxH,
                        c.PlatformHeight,
                        c.OperatingCapacity,
                        c.OperatingWeight,
                        c.AddlSpec1,
                        c.AddlSpec2,
                        c.AddlSpec3,
                        c.AddlField1,
                        c.AddlField2,
                        c.AddlField3,
                        c.PurchasedFrom,
                        c.PurchasedAs2,
                        c.PropertyCondition2,
                        c.HiringMode,
                        c.HiredOn,
                        c.HiredFrom,
                        c.IsOperatorIncluded,
                        c.OperatorCode,
                        c.OperatorRate,
                        c.IsReturned,
                        c.ReturnedOn,
                        c.ReturnedRemarks,
                        c.FinancingCompanyName,
                        c.OperatorName,
                        c.PropertySuppliedBy,
                        c.PropertyGroupId,
                        c.Operator,
                        c.PropertyGroup,
                        c.PropertyGroupCode,
                        c.FinancedBy2,
                        c.PropertyType,

                        c.OperatorContactMobile,
                        c.OperatorContactMobile2,

                        c.EngineNo,
                        c.AlternatorNo,

                        c.CurrentStatus,
                        c.MobilizedTo,
                        c.IsBreakDown,
                        c.OwnershipText,
                        c.IsNeededTimesheet,
                        c.PropertyRemarks,
                        c.PropertyCertification,
                        c.PropertyAttachment,
                        c.EquipmentOperatorName,
                        c.OperatorWorkStartDate,

                        c.BuyingRatePerWeek,

                        c.SupplierRefNo,
                        c.ProjectMasterCode,
                        c.ProjectDescription,
                        c.SupplierAccountLedgerNo,
                        c.MobilizedOn,
                        c.ClientOvertimeRatePerHour,
                        c.UnitRate,
                        c.DeliveryNoteMobilizationRate,
                        c.DeliveryNoteDemobilizationRate,
                        c.ClientUnitMethod,
                        c.SupplierMobRate,
                        c.SupplierDemobRate,
                        c.AgreementHours,
                        c.CordinatorName


                    }).ToList();


                return Json(filteredData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        public class TimesheetRequestbutton
        {
            public string PropertyNo { get; set; }
            public int MonthId { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public decimal? ClientRate { get; set; }
        }
        [HttpPost]
        public async Task<IActionResult> GenerateTimesheetbutton(
    [FromBody] TimesheetRequestbutton request)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { success = false, message = "Invalid tenant." });

            await using var connection = dbContext.Database.GetDbConnection();
            if (connection.State == ConnectionState.Closed)
                await connection.OpenAsync();

            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // 1️⃣ Create/Reset Master Timesheet
                long masterId;
                await using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandText = "stpro401_01UpdatePropertyTimesheet";
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@PropertyNo", request.PropertyNo));
                    command.Parameters.Add(new SqlParameter("@MonthID", request.MonthId));
                    command.Parameters.Add(new SqlParameter("@StartDate", request.StartDate));
                    command.Parameters.Add(new SqlParameter("@EndDate", request.EndDate));
                    command.Parameters.Add(new SqlParameter("@JustInsertedTimeSheetMasterID", 0));
                    command.Parameters.Add(new SqlParameter("@RatePerHour", request.ClientRate));

                    var result = await command.ExecuteScalarAsync();
                    masterId = Convert.ToInt64(result);
                }

                // 2️⃣ Update Child Timesheet for Billing Codes
                for (int billingCode = 3; billingCode <= 8; billingCode++)
                {
                    await using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandText = "stpro401_03UpdateTbl40122PropertyTimeSheetChild";
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add(new SqlParameter("@PropertyNo", request.PropertyNo));
                        command.Parameters.Add(new SqlParameter("@BillingCode", billingCode));
                        command.Parameters.Add(new SqlParameter("@StartDate", request.StartDate));
                        command.Parameters.Add(new SqlParameter("@EndDate", request.EndDate));

                        await command.ExecuteNonQueryAsync();
                    }
                }

                // 3️⃣ Update Day Shift Operators
                await using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandText = "stpro401_15UpdateDayShiftOperatorToTimesheet";
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@PropertyNo", request.PropertyNo));
                    command.Parameters.Add(new SqlParameter("@StartDate", request.StartDate));
                    command.Parameters.Add(new SqlParameter("@EndDate", request.EndDate));

                    await command.ExecuteNonQueryAsync();
                }

                // 4️⃣ Update Night Shift Operators
                await using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandText = "stpro401_16UpdateNightShiftOperatorToTimesheet";
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@PropertyNo", request.PropertyNo));
                    command.Parameters.Add(new SqlParameter("@StartDate", request.StartDate));
                    command.Parameters.Add(new SqlParameter("@EndDate", request.EndDate));

                    await command.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();

                return Ok(new { success = true, message = "Timesheet generated successfully." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
        //   [HttpGet]
        //   public async Task<IActionResult> GetBillingData(
        //string propertyNo,
        //string frmDate,
        //string toDate,
        //int billingCode)
        //   {
        //       if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
        //           return Unauthorized(new { success = false, message = "Invalid tenant." });

        //       try
        //       {
        //           // Parse dates in MM/dd/yyyy format
        //           if (!DateTime.TryParseExact(frmDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDate))
        //               return BadRequest("Invalid from date format. Use MM/dd/yyyy.");

        //           if (!DateTime.TryParseExact(toDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endDate))
        //               return BadRequest("Invalid to date format. Use MM/dd/yyyy.");

        //           // Execute stored procedure and map results to TimesheetBillingData
        //           var billingData = await dbContext.Set<TimesheetBillingData>()
        //               .FromSqlRaw("EXEC stpro401_02GetDataForUpdatingTimeSheet @PropertyNo={0}, @StartDate={1}, @EndDate={2}, @BillingCode={3}",
        //                           propertyNo, startDate, endDate, billingCode)
        //               .ToListAsync();

        //           return Ok(new { success = true, data = billingData });
        //       }
        //       catch (Exception ex)
        //       {
        //           return StatusCode(500, new { success = false, message = ex.Message });
        //       }
        //   }



        [HttpGet]
        public async Task<IActionResult> GetWorkStatus(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var WorkDay = dbContext.Tbl40123TimeSheetDayStatuses.Select(i => new
                    {
                        i.DayCode,
                        i.DayDescription

                    });

                    return Json(await DataSourceLoader.LoadAsync(WorkDay, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetProject: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet]
        public async Task<IActionResult> GetCompany(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var ClientCategory = dbContext.Tbl901CompanyDetails.Select(i => new
                    {
                        i.CompanyId,
                        i.CompanyName

                    });

                    return Json(await DataSourceLoader.LoadAsync(ClientCategory, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetProject: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet]
        public async Task<IActionResult> GetInventoryGroup(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var ClientCategory = dbContext.Tbl60008inventoryMasterGroups.Select(i => new
                    {
                        i.InventoryMasterGroupId,
                        i.InventoryMasterGroup

                    });

                    return Json(await DataSourceLoader.LoadAsync(ClientCategory, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetProject: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetRFQdataByCode(string RFQno)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                if (string.IsNullOrEmpty(RFQno))
                    return BadRequest("RFQ No is required.");

                try
                {

                    var client = await dbContext.Tbl60701rfqmasters
                        .Where(c => c.Rfqno == RFQno)
                        .FirstOrDefaultAsync();

                    if (client == null)
                        return NotFound("RFQ not found.");

                    return Ok(client);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet]
        public async Task<ActionResult> GetRFQChildren(string RFQno)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var resultWithDetails = new List<ExpandoObject>();

                    // Query the Tbl60602purchaseRequestChildren table for the given Mprno
                    var result = dbContext.Tbl60702rfqchildren
                        .Where(x => x.Rfqno == RFQno)
                        .ToList();

                    foreach (var gridDetails in result)
                    {
                        dynamic item = new ExpandoObject();
                        var dict = (IDictionary<string, object>)item;

                        // Copy all existing fields from gridDetails into dynamic object
                        var properties = gridDetails.GetType().GetProperties();
                        foreach (var prop in properties)
                        {
                            dict[prop.Name] = prop.GetValue(gridDetails);
                        }

                        // Retrieve UnitDesc based on UnitCode
                        var unitDesc = await dbContext.Tbl40111PropertyUnitCodes
                            .Where(x => x.UnitCode == gridDetails.UnitRateMethod)
                            .Select(x => x.UnitDesc)
                            .FirstOrDefaultAsync();



                        // Retrieve Gsdescription based on Gscode
                        var gsDescription = await dbContext.Tbl20164GoodsAndServicesMasters
                            .Where(x => x.Gscode == gridDetails.Gscode)
                            .Select(x => x.Gsdescrpition)
                            .FirstOrDefaultAsync();

                        // Add the retrieved values to the dynamic object
                        dict["UnitDesc"] = unitDesc;

                        dict["GsDescription"] = gsDescription;

                        resultWithDetails.Add(item);
                    }

                    return Json(resultWithDetails);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public async Task<IActionResult> SaveOrUpdateRFQ([FromBody] RFQViewModel VM)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant context." });
            }

            if (VM == null || string.IsNullOrEmpty(VM.Rfqno))
            {
                return BadRequest(new { success = false, message = "RFQ No. is required." });
            }

            try//
            {
                // Ensure child list is initialized
                //VM.RFQDetailses = VM.RFQDetailses ?? new List<Tbl60702rfqchild>();

                // Check if the master record exists
                var existingMaster = await dbContext.Tbl60701rfqmasters
                    .FirstOrDefaultAsync(x => x.Rfqno == VM.Rfqno);

                if (existingMaster != null)
                {
                    //Update existing master with manual property mapping

                    //existingMaster.Rfqno = VM.Rfqno;
                    existingMaster.Rfqdate = VM.Rfqdate;
                    existingMaster.SupplierCode = VM.SupplierCode;
                    existingMaster.Mprno = VM.Mprno;
                    existingMaster.Project = VM.Project;
                    existingMaster.Attention = VM.Attention;
                    existingMaster.SupplierContactEmail = VM.SupplierContactEmail;
                    existingMaster.SupplierContactNo = VM.SupplierContactNo;
                    existingMaster.SupplierQuotationNo = VM.SupplierQuotationNo;
                    existingMaster.SupplierQuotationDt = VM.SupplierQuotationDt;
                    existingMaster.ProjectMasterCode = VM.ProjectMasterCode;
                    existingMaster.Rfqsubject = VM.Rfqsubject;
                    existingMaster.Rfqintro = VM.Rfqintro;
                    existingMaster.Rfqsummary = VM.Rfqsummary;


                    existingMaster.Rfqsignatory = VM.Rfqsignatory.HasValue ? (byte?)VM.Rfqsignatory.Value : null;
                    existingMaster.CompanyBranch = VM.CompanyBranch.HasValue ? (byte?)VM.CompanyBranch.Value : null;
                    existingMaster.InventoryMasterGroupId = VM.InventoryMasterGroupId.HasValue ? (byte?)VM.InventoryMasterGroupId.Value : null;

                }
                else
                {
                    // Insert new master
                    var newMaster = new Tbl60701rfqmaster
                    {
                        Rfqno = VM.Rfqno,
                        Rfqdate = VM.Rfqdate,
                        SupplierCode = VM.SupplierCode,

                        Mprno = VM.Mprno,
                        Project = VM.Project,
                        Attention = VM.Attention,
                        SupplierContactEmail = VM.SupplierContactEmail,
                        SupplierContactNo = VM.SupplierContactNo,

                        SupplierQuotationNo = VM.SupplierQuotationNo,
                        SupplierQuotationDt = VM.SupplierQuotationDt,
                        ProjectMasterCode = VM.ProjectMasterCode,
                        Rfqsubject = VM.Rfqsubject,
                        Rfqintro = VM.Rfqintro,
                        Rfqsummary = VM.Rfqsummary,
                        Rfqsignatory = Convert.ToByte(VM.Rfqsignatory),
                        CompanyBranch = Convert.ToByte(VM.CompanyBranch),
                        InventoryMasterGroupId = Convert.ToByte(VM.InventoryMasterGroupId)
,

                    };

                    await dbContext.Tbl60701rfqmasters.AddAsync(newMaster);
                }

                // Handle child entries
                var existingChildren = await dbContext.Tbl60702rfqchildren
                    .Where(x => x.Rfqno == VM.Rfqno)
                    .ToListAsync();

                foreach (var child in VM.RFQDetailses)
                {
                    if (child.RfqchildSlNo == 0)
                    {
                        // New child entry
                        child.Rfqno = VM.Rfqno; // Ensure foreign key is set
                        await dbContext.Tbl60702rfqchildren.AddAsync(child);
                    }
                    else
                    {
                        // Existing child entry
                        var existingChild = existingChildren
                            .FirstOrDefault(x => x.RfqchildSlNo == child.RfqchildSlNo);

                        if (existingChild != null)
                        {
                            dbContext.Entry(existingChild).CurrentValues.SetValues(child);
                        }
                    }
                }

                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "RFQ Details saved/updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
        private async Task<int?> GetSignatoryIDfromUserID(int? userId)
        {
            if (userId == null)
                return null;

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return await dbContext.Tbl90104DocumentSignatories
                    .Where(x => x.UserId == userId)
                    .Select(x => x.SignatoryId)
                    .FirstOrDefaultAsync();
            }

            // Tenant context is invalid; return null
            return null;
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteRfq([FromQuery] string Rfqno)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant context." });
            }

            if (string.IsNullOrEmpty(Rfqno))
            {
                return BadRequest(new { success = false, message = "Rfqno. is required." });
            }

            try
            {
                // Retrieve the master record
                var masterRecord = await dbContext.Tbl60701rfqmasters
                    .FirstOrDefaultAsync(x => x.Rfqno == Rfqno);

                if (masterRecord == null)
                {
                    return NotFound(new { success = false, message = "RFQ not found." });
                }

                // Retrieve and remove child records
                var childRecords = dbContext.Tbl60702rfqchildren
                    .Where(x => x.Rfqno == Rfqno);

                dbContext.Tbl60702rfqchildren.RemoveRange(childRecords);

                // Remove the master record
                dbContext.Tbl60701rfqmasters.Remove(masterRecord);

                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "RFQ details deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SubmitRFQ(string Rfqno)
        {
            // Validate tenant context
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant context." });
            }

            // Validate MPR number
            if (string.IsNullOrEmpty(Rfqno))
            {
                return BadRequest(new { success = false, message = "RFQ No. is required." });
            }

            // Retrieve MPR master record
            var master = await dbContext.Tbl60701rfqmasters.FirstOrDefaultAsync(x => x.Rfqno == Rfqno);
            if (master == null)
            {
                return NotFound(new { success = false, message = "MPR not found." });
            }

            // Retrieve session values
            var userName = HttpContext.Session.GetString("UserName");
            var userIdString = HttpContext.Session.GetString("UserId");

            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized(new { success = false, message = "Invalid or missing UserId in session." });
            }

            // Update MPR master record
            master.IsSubmitted = true;
            master.SubmittedBy = userName;
            master.SubmittedOn = DateTime.Now;
            master.ModifiedBy = userName;
            master.ModifiedOn = DateTime.Now;

            // Retrieve signatory ID
            //var signatoryId = await GetSignatoryIDfromUserID(userId);
            //if (signatoryId.HasValue)
            //{
            //	master.RequestSignatory = (byte)signatoryId.Value;
            //}
            //else
            //{
            //	master.RequestSignatory = null;
            //}


            //master.PurchaseRequestStatusId = 31; // Enquiry/Request Submitted

            // Save changes to the database
            await dbContext.SaveChangesAsync();

            return Ok(new { success = true, message = "RFQ submitted successfully." });
        }
        [HttpPost]
        public async Task<IActionResult> VerifyRFQ(string Rfqno)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { message = "Invalid tenant context." });
            }

            if (string.IsNullOrEmpty(Rfqno))
            {
                return BadRequest(new { message = "Rfqno is required." });
            }

            var voucher = await dbContext.Tbl60701rfqmasters
                .FirstOrDefaultAsync(v => v.Rfqno == Rfqno);

            if (voucher == null)
            {
                return NotFound(new { message = "Credit note not found." });
            }

            var userName = HttpContext.Session.GetString("UserName");
            var userIdString = HttpContext.Session.GetString("UserId");

            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized(new { message = "Invalid or missing UserId in session." });
            }

            // Update voucher fields
            voucher.IsVerified = true;
            voucher.VerifiedOn = DateTime.Now;
            voucher.VerifiedBy = userName;
            //voucher.PurchaseRequestStatusId = 32; // Enquiry/Request Verified

            //var signatoryId = await GetSignatoryIDfromUserID(userId);
            //if (signatoryId.HasValue)
            //{
            //	voucher.MprverifiedSign = (byte)signatoryId.Value;
            //}

            await dbContext.SaveChangesAsync();

            return Ok(new
            {
                message = "RFQ has been Verified and processed for Approval."
            });
        }
        [HttpPost]
        public async Task<ActionResult> ApproveRFQ(string Rfqno)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var userName = HttpContext.Session.GetString("UserName");
                    var userIdString = HttpContext.Session.GetString("UserId");
                    if (!int.TryParse(userIdString, out int userId))
                    {
                        return Unauthorized(new { message = "Invalid or missing UserId in session." });
                    }


                    if (string.IsNullOrEmpty(Rfqno))
                    {
                        return BadRequest(new { Message = "RFQ No number is required." });
                    }

                    var voucher = dbContext.Tbl60701rfqmasters
                                           .FirstOrDefault(v => v.Rfqno == Rfqno);

                    if (voucher == null)
                    {
                        return NotFound(new { Message = "CreditNoteNo not found." });
                    }

                    // Update approval details
                    voucher.IsApproved = true;
                    voucher.ApprovedOn = DateTime.Now;
                    voucher.ApprovedBy = userName;
                    //voucher.PurchaseRequestStatusId = 33; // Status: Enquiry/Request Approved
                    //var signatoryId = await GetSignatoryIDfromUserID(userId);
                    //if (signatoryId.HasValue)
                    //{
                    //	voucher.MprapprovedSign = (byte)signatoryId.Value;
                    //}



                    dbContext.SaveChanges();

                    return Ok(new
                    {
                        Message = "RFQ has been Approved.",
                        VoucherApprovedBy = userName
                    });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { Message = ex.Message });
                }
            }

            return Unauthorized(new { Message = "Invalid tenant.", Success = false });
        }
    }
}
