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
//using QD.ERP.Web.Areas.Finance.Models;
//using QD.ERP.Web.Areas.Finance.Reports.Payable_Statements;
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
        private readonly SqlConnection _db;


        public ERRequestforQuotationListController(ILogger<ERRequestforQuotationListController> logger, TenantDbContextHelper tenantDbContextHelper, IConfiguration config)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
            _db = new SqlConnection(config.GetConnectionString("DefaultConnection"));
        }
      
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
        public class TimesheetCopyDto
        {
            public string PropertyNo { get; set; }
            public int TimeSheetMasterID { get; set; }
        }

        [HttpPost]

public IActionResult CopyClientToSuppliers([FromBody] List<TimesheetCopyDto> timesheets)
{
     if (timesheets == null || timesheets.Count == 0)
         return BadRequest(new { message = "No data provided", success = false });
 
     // ✅ Try to get the correct tenant context
     if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
         return Unauthorized(new { message = "Invalid tenant", success = false });
 
     try
     {
         // ✅ Use connection from current tenant context
         using (var connection = (SqlConnection)dbContext.Database.GetDbConnection())
         {
             connection.Open();
             foreach (var ts in timesheets)
             {
                 using (var cmd = new SqlCommand("stpro401_17CopyClientTStoSupplierTS", connection))
                 {
                     cmd.CommandType = CommandType.StoredProcedure;
                     cmd.Parameters.AddWithValue("@PropertyNo", ts.PropertyNo);
                     cmd.Parameters.AddWithValue("@TimeSheetMasterID", ts.TimeSheetMasterID);
                     cmd.ExecuteNonQuery();
                 }
             }
             connection.Close();
         }
 
         return Ok(new { message = "Timesheets copied successfully!", success = true });
     }
     catch (Exception ex)
     {
         return StatusCode(500, new { message = "An error occurred while copying timesheets.", details = ex.Message, success = false });
     }
}
        //[HttpPost]
        //public IActionResult CopyClientToSuppliers([FromBody] List<TimesheetCopyDto> timesheets)
        //{
        //    if (timesheets == null || timesheets.Count == 0)
        //        return BadRequest("No data provided");

        //    _db.Open();
        //    foreach (var ts in timesheets)
        //    {
        //        using (var cmd = new SqlCommand("stpro401_17CopyClientTStoSupplierTS", _db))
        //        {
        //            cmd.CommandType = CommandType.StoredProcedure;
        //            cmd.Parameters.AddWithValue("@PropertyNo", ts.PropertyNo);
        //            cmd.Parameters.AddWithValue("@TimeSheetMasterID", ts.TimeSheetMasterID);
        //            cmd.ExecuteNonQuery();
        //        }
        //    }
        //    _db.Close();

        //    return Ok(new { message = "Timesheets copied successfully!" });
        //}
        [HttpGet]
        public async Task<IActionResult> GetTimeSlots()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var timeSlots = await dbContext.Tbl101TimeSlots
                        .Select(i => new
                        {
                            i.TimeSlot

                        }).ToListAsync();

                    return Json(timeSlots);  // ✅ returns array
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetTimeSlots: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching timeslots.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
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

                    var projectedList = await projectedQuery.ToListAsync();

                    var statusLookup = dbContext.Tbl40123TimeSheetDayStatuses
                        .ToDictionary(x => x.DaySlNo, x => x.DayDescription);

                    var resultWithVAT = new List<ExpandoObject>();

                    foreach (var gridDetails in projectedList)
                    {
                        dynamic item = new ExpandoObject();
                        var dict = (IDictionary<string, object>)item;

                        foreach (var prop in gridDetails.GetType().GetProperties())
                        {
                            dict[prop.Name] = prop.GetValue(gridDetails);
                        }

                        dict["WorkStatus"] = statusLookup.TryGetValue(gridDetails.WorkStatus ?? 0, out var desc)
                            ? desc
                            : null;

                        resultWithVAT.Add(item);
                    }

                    // ✅ Return through DataSourceLoader for paging/filtering
                    return Json(DataSourceLoader.Load(resultWithVAT, loadOptions));
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

        [HttpGet("GetWorkStatus")]
        public async Task<ActionResult> GetWorkStatus()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var workStatuses = dbContext.Tbl40123TimeSheetDayStatuses
                        .Select(x => new
                        {
                            x.DaySlNo,
                            x.DayCode,
                            x.DayDescription
                        })
                        .OrderBy(x => x.DaySlNo)
                        .ToList();

                    return Json(workStatuses);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        //[HttpGet]
        //public async Task<IActionResult> GetWorkStatus()
        //{
        //    if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
        //    {
        //        try
        //        {
        //            var workDay = await dbContext.Tbl40123TimeSheetDayStatuses
        //                .Select(i => new {
        //                    i.DayCode,
        //                    i.DayDescription
        //                }).ToListAsync();

        //            return Json(workDay);   // 👈 just return array
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError($"Error in GetWorkStatus: {ex.Message}");
        //            return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
        //        }
        //    }

        //    return Unauthorized(new { message = "Invalid tenant.", success = false });
        //}




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
