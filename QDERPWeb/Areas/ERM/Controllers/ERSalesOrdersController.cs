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
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static QD.ERP.Web.Areas.ERM.Controllers.AddNewQuotation1Controller;

namespace QD.ERP.Web.Areas.ERM.Controllers
{
    [Area("ERM")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ERSalesOrdersController : Controller
    {     
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<ERSalesOrdersController> _logger;
        public ERSalesOrdersController(ILogger<ERSalesOrdersController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetSalesOrders(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var query = dbContext.Qry60204salesOrderViewMasters.AsQueryable();


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
                    query = query.Where(i => i.SalesOrderDate >= fromDate && i.SalesOrderDate <= toDate);

                    // Fetching the data
                    var data = await query.Select(i => new
                    {
                        i.SalesOrderNo,
                        i.SalesOrderDate,
                        i.ClientName,
                        i.ClientPono,
                        i.Mprno,
                        i.QuoteNo,
                        i.InvoiceStatus,
                        i.OrderStatus,
                        i.NoOfItems,
                        i.TotalBeforeDiscount,
                        i.Discount,
                        i.TotalAfterDiscount,
                        i.TotalTaxAmount,
                        i.TotalWithTax,
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


        [HttpGet]
        public async Task<IActionResult> GetSalesOrdersDetails(DateTime? fromDate, DateTime? toDate)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var query = dbContext.Qry60212salesOrderItemsWithDetails.AsQueryable();
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
                query = query.Where(i => i.SalesOrderDate >= fromDate && i.SalesOrderDate <= toDate);

                // Fetching the data
                var data = await query.Select(i => new
                {
                    i.SalesOrderNo,
                    i.SalesOrderDate,
                    i.ClientName,
                    i.ClientPono,
                    i.SalesPersonName,
                    i.Gscode,
                    i.Gsdescrpition,
                    i.UnitType,
                    i.UnitDesc,
                    i.QuotedQuantity,
                    i.DeliveryTerms,
                    i.QuoteRemarks,
                    i.QuotedUnitPrice,
                    i.LineTotalBeforeDiscount,
                    i.QuotedDiscount,
                    i.LineTotalAfterDiscount,
                    i.LineTaxAmount,
                    i.LineTotalWithTax,
                    i.BalanceToDeliver,
                    i.TotalOrderedQty,
                    i.TotalIssdQty,
                    i.DeliveryPeriod,
                    i.CostPrice,
                }).ToListAsync();

                return Json(data);
            }

            return Unauthorized(new { message = "Invalid tenant." });
        }


        [HttpGet]
        public IActionResult SalesOrderNoIncrease()
        {
            try
            {
                // Get tenant name from session
                string tenantName = HttpContext.Session.GetString("TenantName");
                if (string.IsNullOrWhiteSpace(tenantName))
                {
                    _logger.LogWarning("Tenant name not found in session when generating SalesOrderNo.");
                    return Unauthorized(new { message = "Tenant name not found in session." });
                }

                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    _logger.LogWarning("Invalid tenant context when generating SalesOrderNo.");
                    return Unauthorized(new { message = "Invalid tenant." });
                }

                // Build prefix from tenant name
                string[] words = tenantName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string prefix;
                if (words.Length == 1)
                {
                    var word = words[0];
                    prefix = (word.Length >= 2)
                        ? $"{char.ToUpper(word[0])}{char.ToUpper(word[^1])}"
                        : word.ToUpper();
                }
                else
                {
                    prefix = string.Concat(words.Select(w => char.ToUpper(w[0])));
                }

                string year = DateTime.Now.Year.ToString();

                // 🔄 Updated format: {Prefix}-SO-{Year}-
                string basePrefix = $"{prefix}-SO-{year}-";

                // Fetch existing Sales Order Nos matching this format
                var orderNos = dbContext.Tbl40129PropertySalesOrderMasters
                    .Where(x => x.SalesOrderNo.StartsWith(basePrefix))
                    .Select(x => x.SalesOrderNo)
                    .ToList();

                // Extract number part using Regex
                int maxNumber = 0;
                var regex = new Regex($@"^{Regex.Escape(basePrefix)}(\d+)$");
                foreach (var orderNo in orderNos)
                {
                    var match = regex.Match(orderNo ?? "");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int num))
                    {
                        if (num > maxNumber)
                            maxNumber = num;
                    }
                }

                // Generate next number
                int nextNumber = maxNumber + 1;
                string nextOrderNo = $"{basePrefix}{nextNumber:D5}";

                return Ok(new { salesOrderNo = nextOrderNo });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in SalesOrderNoIncrease: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
            }
        }




        private string GenerateNextOrderNo(string lastOrderNo)
        {
            if (string.IsNullOrWhiteSpace(lastOrderNo))
                return "SO-18-00001"; // Fallback default if nothing found

            var match = Regex.Match(lastOrderNo, @"^(.*-)(\d+)$");
            if (!match.Success)
                return lastOrderNo + "-00001"; // Unexpected format fallback

            string prefix = match.Groups[1].Value;
            int number = int.Parse(match.Groups[2].Value);

            return $"{prefix}{(number + 1):D5}";
        }


        [HttpGet]
        public async Task<IActionResult> GetClientNames()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var data = await dbContext.Tbl30101ClientMasters
                        .Select(i => new
                        {
                            i.ClientCode,
                            i.ClientName,
                            i.ContactMobile1,
                            i.ContactEmail,
                            i.ContactPerson


                        })
                        .ToListAsync();

                    return Json(data); // return raw data, paging/sorting done on client-side
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetProject()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var data = await dbContext.Qry70002projectsViewMasters
                        .Select(i => new
                        {
                            i.ProjectId,
                            i.ProjectDescription

                        })
                        .ToListAsync();

                    return Json(data); // return raw data, paging/sorting done on client-side
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTypeOfSalesOrder()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var data = await dbContext.Tbl30104TypeOfRequestMasters
                        .Select(i => new
                        {
                            i.TypeOfRequestId,
                            i.TypeOfRequest

                        })
                        .ToListAsync();

                    return Json(data); // return raw data, paging/sorting done on client-side
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }



        [HttpGet]
        public async Task<IActionResult> GetSignatory()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var data = await dbContext.Tbl90104DocumentSignatories
                        .Select(i => new
                        {
                            i.SignatoryId,
                            i.SignatoryName

                        })
                        .ToListAsync();

                    return Json(data); // return raw data, paging/sorting done on client-side
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCompanyBranch()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var data = await dbContext.Tbl901CompanyDetails
                        .Select(i => new
                        {
                            i.CompanyId,
                            i.CompanyName

                        })
                        .ToListAsync();

                    return Json(data); // return raw data, paging/sorting done on client-side
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetSalesPersonName()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var data = await dbContext.Tbl20101SalesPersonMasters
                        .Select(i => new
                        {
                            i.SalesPersonName,
                            i.SalesPersonCode

                        })
                        .ToListAsync();

                    return Json(data); // return raw data, paging/sorting done on client-side
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetGroupMaster()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var data = await dbContext.Tbl60008inventoryMasterGroups
                        .Select(i => new
                        {
                            i.InventoryMasterGroupId,
                            i.InventoryMasterGroup

                        })
                        .ToListAsync();

                    return Json(data); // return raw data, paging/sorting done on client-side
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCostCenterMaster()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var data = await dbContext.Tbl201CostAllocationUnits
                        .Select(i => new
                        {
                            i.CostAllocationMasterGroup,
                            i.CostAllocationUnitId


                        })
                        .ToListAsync();

                    return Json(data); // return raw data, paging/sorting done on client-side
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }




        public class SalesorderRequest
        {
            public string SalesOrderNo { get; set; }

            public DateTime? SalesOrderDate { get; set; }

            public string ClientPono { get; set; }

            public DateTime? ClientPodate { get; set; }

            public string QuoteNo { get; set; }

            public DateTime? QuoteDate { get; set; }

            public string ClientRefNo { get; set; }

            public string Attention { get; set; }

            public string SubjectTitle { get; set; }

            public byte? TypeOfQuote { get; set; }

            public string QuoteType { get; set; }

            public decimal? QuoteTransport { get; set; }
            public decimal? CurrencyRate { get; set; }
            public int? BaseCurrencyId { get; set; }
            public int? CurrencyId { get; set; }

            public decimal? QuoteDiscount { get; set; }

            public byte? PaymentTerms { get; set; }

            public byte? DeliveryPeriod { get; set; }

            public byte? DeliveryTerms { get; set; }

            public string QuoteValidity { get; set; }

            public string PreparedBy { get; set; }

            public DateTime? PreparedOn { get; set; }

            public string ApprovedBy { get; set; }

            public DateTime? ApprovedOn { get; set; }

            public string AddedBy { get; set; }

            public DateTime? AddedOn { get; set; }

            public string ModifiedBy { get; set; }

            public DateTime? ModifiedOn { get; set; }

            public string Rfqcode { get; set; }

            public string ClientContactNo { get; set; }

            public string ClientContactEmail { get; set; }

            public string ClientCode { get; set; }

            public string QuotationSummary { get; set; }

            public byte? QuoteSignatory { get; set; }

            public string QuoteIntro { get; set; }

            public byte? TypeOfRequest { get; set; }

            public byte? ModeOfRequest { get; set; }

            public string DeliveryContactPerson { get; set; }

            public string DiscountsText { get; set; }

            public DateTime? QuoteDueDate { get; set; }

            public string Project { get; set; }

            public string SalesPersonCode { get; set; }

            public bool? IsVerified { get; set; }

            public bool? IsApproved { get; set; }

            public byte? RevisionNo { get; set; }

            public byte? CompanyBranch { get; set; }

            public string ProjectMasterCode { get; set; }

            public DateTime? OrderExpiryDate { get; set; }

            public byte? InventoryMasterGroupId { get; set; }

            public string SalesOrderRemarks { get; set; }

            public DateTime? ExpectedDeliveryDate { get; set; }

            public bool? IsSubmitted { get; set; }

            public string SubmittedBy { get; set; }

            public DateTime? SubmittedOn { get; set; }

            public string VerifiedBy { get; set; }

            public DateTime? VerifiedOn { get; set; }

            public string DeliveryContactEmail { get; set; }

            public string DeliveryContactMobile { get; set; }

            public string DeliveryLocation { get; set; }

            public string DeliveryRegion { get; set; }

            public string OrderDuration { get; set; }

            public string PaymentMode { get; set; }

            public string MobChargesBy { get; set; }


            public decimal? AdvanceAmt { get; set; }

            public string CustomsChargesBy { get; set; }


            public string CreditPeriod { get; set; }


            public decimal? CreditLimit { get; set; }


            public string RefNo { get; set; }


            public string ClientName { get; set; }







            public List<Tbl40130PropertySalesOrderChild> SalesOrderChildren { get; set; }

        }

























        [HttpPost]
        public async Task<IActionResult> SaveSalesOrder([FromBody] SalesorderRequest model)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized(new { success = false, message = "Invalid tenant." });

                if (model == null)
                    return BadRequest(new { success = false, message = "Invalid data." });
            

                bool isUpdate = false;
                var salesOrderNo = model.SalesOrderNo?.Trim();

                var existingEntity = await dbContext.Tbl40129PropertySalesOrderMasters
                    .FirstOrDefaultAsync(x => x.SalesOrderNo == salesOrderNo);

                if (existingEntity != null)
                {
                    isUpdate = true;
                    existingEntity.SalesOrderDate = model.SalesOrderDate;
                    existingEntity.ClientName = model.ClientName;
                    existingEntity.QuoteNo = model.QuoteNo;
                    existingEntity.QuoteDate = model.QuoteDate;
                    existingEntity.ClientRefNo = model.ClientRefNo;
                    existingEntity.ClientContactNo = model.ClientContactNo;
                    existingEntity.ClientPono = model.ClientPono;
                    existingEntity.ClientPodate = model.ClientPodate;
                    existingEntity.ClientContactEmail = model.ClientContactEmail;
                    existingEntity.Project = model.Project;
                    existingEntity.DeliveryContactPerson = model.DeliveryContactPerson;
                    existingEntity.DeliveryContactMobile = model.DeliveryContactMobile;
                    existingEntity.DeliveryContactEmail = model.DeliveryContactEmail;
                    existingEntity.DeliveryLocation = model.DeliveryLocation;
                    existingEntity.DeliveryRegion = model.DeliveryRegion;
                    existingEntity.OrderDuration = model.OrderDuration;
                    existingEntity.PaymentMode = model.PaymentMode;
                    existingEntity.MobChargesBy = model.MobChargesBy;
                    existingEntity.AdvanceAmount = model.AdvanceAmt;
                    existingEntity.CustomsChargesBy = model.CustomsChargesBy;
                    existingEntity.CreditPeriod = model.CreditPeriod;
                    existingEntity.CreditLimit = model.CreditLimit;
                    existingEntity.CompanyBranch = model.CompanyBranch;
                    existingEntity.SalesPersonCode = model.SalesPersonCode;
                    existingEntity.RefNo = model.RefNo;
                    existingEntity.OrderExpiryDate = model.OrderExpiryDate;
                    existingEntity.PreparedBy = model.PreparedBy;
                    existingEntity.VerifiedBy = model.VerifiedBy;
                    existingEntity.ApprovedBy = model.ApprovedBy;
                    existingEntity.AddedOn = DateTime.Now;
                   // existingEntity.IsVerified = true;
                    existingEntity.AddedBy = model.AddedBy ?? User.Identity?.Name;

                  

                    dbContext.Tbl40129PropertySalesOrderMasters.Update(existingEntity);
                }
                else
                {
                    var entity = new Tbl40129PropertySalesOrderMaster
                    {
                        SalesOrderNo = salesOrderNo,
                        ClientName=model.ClientName,
                        SalesOrderDate = model.SalesOrderDate,
                        QuoteNo = model.QuoteNo,
                        QuoteDate = model.QuoteDate,
                        ClientRefNo = model.ClientRefNo,
                        ClientContactNo = model.ClientContactNo,
                        ClientPono = model.ClientPono,
                        ClientPodate = model.ClientPodate,
                        ClientContactEmail = model.ClientContactEmail,
                        Project = model.Project,
                        DeliveryContactPerson =model.DeliveryContactPerson,
                        DeliveryContactMobile = model.DeliveryContactMobile,
                        DeliveryContactEmail =model.DeliveryContactEmail,
                        DeliveryLocation=model.DeliveryLocation,
                        DeliveryRegion = model.DeliveryRegion,
                        OrderDuration = model.OrderDuration,
                        PaymentMode = model.PaymentMode,
                        MobChargesBy = model.MobChargesBy,
                        AdvanceAmount = model.AdvanceAmt,
                        CustomsChargesBy = model.CustomsChargesBy,
                        CreditPeriod = model.CreditPeriod,
                        CreditLimit = model.CreditLimit,
                        CompanyBranch = model.CompanyBranch,
                        SalesPersonCode = model.SalesPersonCode,
                        RefNo = model.RefNo,
                        OrderExpiryDate = model.OrderExpiryDate,
                        PreparedBy = model.PreparedBy,
                        VerifiedBy = model.VerifiedBy,
                        ApprovedBy = model.ApprovedBy,
                        IsApproved = false,
                        IsVerified = false,
                        IsSubmitted = false,






                        //Attention = model.Attention,
                        ClientCode = model.ClientCode,                       
                        //QuoteTransport = model.QuoteTransport,
                        //QuoteDiscount = model.QuoteDiscount,                        
                        //TypeOfRequest = model.TypeOfRequest,
                        //QuoteSignatory = model.QuoteSignatory,
                        //ProjectMasterCode = model.ProjectMasterCode,
                        //InventoryMasterGroupId = model.InventoryMasterGroupId,
                        //AdditionsText = model.AdditionsText,
                       // SalesOrderRemarks = model.SalesOrderRemarks,
                        //ExpectedDeliveryDate = model.ExpectedDeliveryDate,
                        //DeliveryPeriod = model.DeliveryPeriod,
                        //DeliveryTerms = model.DeliveryTerms,
                        //DiscountsText = model.DiscountsText,
                       // CostAllocationMasterGroup = model.CostAllocationMasterGroup,
                        AddedBy = model.AddedBy ?? User.Identity?.Name,
                        AddedOn = DateTime.Now,
                        
                        
                    };

                    await dbContext.Tbl40129PropertySalesOrderMasters.AddAsync(entity);
                }

                await dbContext.SaveChangesAsync();





                // 🔹 Handle Child Rows with HasEquipmentDetails & LineOrderNo logic
                var existingChildren = await dbContext.Tbl40130PropertySalesOrderChildren
                    .Where(x => x.SalesOrderNo == model.SalesOrderNo)
                    .OrderBy(x => x.LineOrderNo)
                    .ToListAsync();

                decimal nextLineOrderNo = 1;
                if (existingChildren.Any())
                    nextLineOrderNo = existingChildren.Max(x => x.LineOrderNo ?? 0) + 1;

                foreach (var child in model.SalesOrderChildren ?? new List<Tbl40130PropertySalesOrderChild>())
                {
                    // 1️⃣ Check last child row
                    var lastChild = existingChildren.LastOrDefault();

                    if (lastChild != null && lastChild.HasEquipmentDetails == "Yes")
                    {
                        // Update last row

                        lastChild.PropertyOrdered = child.PropertyOrdered;
                        lastChild.UnitRateMethod = child.UnitRateMethod;
                        lastChild.QuotedQuantity = child.QuotedQuantity;
                        lastChild.UnitRate = child.UnitRate;
                        lastChild.MobRate = child.MobRate;
                        lastChild.DemobRate = child.DemobRate;

                        lastChild.HasEquipmentDetails = "Updated";

                    }
                    else
                    {
                        // Insert new child row
                        var newChild = new Tbl40130PropertySalesOrderChild
                        {
                            SalesOrderNo = model.SalesOrderNo,
                            LineOrderNo = nextLineOrderNo++,
                            PropertyOrdered = child.PropertyOrdered,
                            UnitRateMethod = child.UnitRateMethod,
                            QuotedQuantity = child.QuotedQuantity,
                            UnitRate = child.UnitRate,
                            MobRate = child.MobRate,
                            DemobRate = child.DemobRate,
                            HasEquipmentDetails = "No"
                        };

                        await dbContext.Tbl40130PropertySalesOrderChildren.AddAsync(newChild);
                        existingChildren.Add(newChild); // so next iteration considers this row
                    }
                }

                await dbContext.SaveChangesAsync();






















                return Ok(new
                {
                    success = true,
                    message = isUpdate ? "Sales order updated successfully." : "Sales order created successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SaveSalesOrder");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while saving the sales order.",
                    details = ex.Message
                });
            }
        }

        public class SalesOrderDetailsRequest
        {
            public string SalesOrderNo { get; set; }
            public string DetailedDescription { get; set; }
            public string Certification { get; set; }
            public string Capacity { get; set; }
            public string Operator { get; set; }
            public string Attachment { get; set; }
            public string UnitRateMethod2 { get; set; }
            public decimal? UnitRate2 { get; set; }
            public string UnitRateMethod3 { get; set; }
            public decimal? UnitRate3 { get; set; }
            public string Notes { get; set; }
            public string AdditionalNotes { get; set; }
            public decimal? MobilizationRate { get; set; }
            public decimal? DemobRate { get; set; }
            public string DeliveryDetails { get; set; }
            public decimal? LineOrderNo { get; set; }
        }





        [HttpPost]
        public async Task<IActionResult> SaveOrUpdateQuotationChild([FromBody] SalesOrderDetailsRequest request)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant context." });
            }

            if (request == null || string.IsNullOrEmpty(request.SalesOrderNo))
            {
                return BadRequest(new { success = false, message = "Quote No is required." });
            }

            try
            {
                // ✅ Check if child already exists for given QuoteNo + LineOrderNo
                var existing = await dbContext.Tbl40130PropertySalesOrderChildren
                    .FirstOrDefaultAsync(x => x.SalesOrderNo == request.SalesOrderNo && x.LineOrderNo == request.LineOrderNo);

                if (existing != null)
                {
                    // 🔄 Update existing record
                    existing.PropertyAddlDescription = request.DetailedDescription;
                    existing.Certification = request.Certification;
                    existing.Capacity = request.Capacity;
                    existing.Operator = request.Operator;
                    existing.Attachment = request.Attachment;
                    existing.UnitRateMethod2 = request.UnitRateMethod2;
                    existing.UnitRate2 = request.UnitRate2;
                    existing.UnitRateMethod3 = request.UnitRateMethod3;
                    existing.UnitRate3 = request.UnitRate3;
                    existing.Notes = request.Notes;
                    existing.AddlNotes = request.AdditionalNotes;
                    existing.MobRate = request.MobilizationRate;
                    existing.DemobRate = request.DemobRate;
                    existing.DeliveryDetails = request.DeliveryDetails;
                }
                else
                {
                    // ✅ Generate next LineOrderNo for this QuoteNo
                    decimal nextLineOrderNo = (await dbContext.Tbl40130PropertySalesOrderChildren
                        .Where(x => x.SalesOrderNo == request.SalesOrderNo)
                        .MaxAsync(x => (decimal?)x.LineOrderNo)) ?? 0;

                    nextLineOrderNo++; // increment

                    // ➕ Insert new record
                    var newChild = new Tbl40130PropertySalesOrderChild
                    {
                        SalesOrderNo = request.SalesOrderNo,
                        LineOrderNo = nextLineOrderNo,   // ✅ sequential number
                        PropertyAddlDescription = request.DetailedDescription,
                        Certification = request.Certification,
                        Capacity = request.Capacity,
                        Operator = request.Operator,
                        Attachment = request.Attachment,
                        UnitRateMethod2 = request.UnitRateMethod2,
                        UnitRate2 = request.UnitRate2,
                        UnitRateMethod3 = request.UnitRateMethod3,
                        UnitRate3 = request.UnitRate3,
                        Notes = request.Notes,
                        AddlNotes = request.AdditionalNotes,
                        MobRate = request.MobilizationRate,
                        DemobRate = request.DemobRate,
                        DeliveryDetails = request.DeliveryDetails,
                        HasEquipmentDetails = "Yes"

                    };

                    await dbContext.Tbl40130PropertySalesOrderChildren.AddAsync(newChild);
                }

                await dbContext.SaveChangesAsync();

               

                return Ok(new { success = true, message = "Equipment Details saved successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }











        [HttpPost]
public async Task<IActionResult> GenerateJobOrders([FromBody] SalesorderViewModel model)
{
    try
    {
        if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            return Unauthorized(new { success = false, message = "Invalid tenant." });

        if (string.IsNullOrWhiteSpace(model.SalesOrderNo))
            return BadRequest(new { success = false, message = "Missing Sales Order No." });

        if (model.SalesOrderChildren == null || !model.SalesOrderChildren.Any())
            return BadRequest(new { success = false, message = "No line items selected." });

        foreach (var child in model.SalesOrderChildren)
        {
            if (child.SalesOrderChildId == 0)
                return BadRequest(new { success = false, message = "Invalid SalesOrderChildId in line items." });

            var parameters = new[]
            {
                new SqlParameter("@JobOrderNo", model.SalesOrderNo ?? (object)DBNull.Value),
                new SqlParameter("@AddedBy", model.AddedBy ?? User.Identity?.Name ?? (object)DBNull.Value),
                new SqlParameter("@SalesOrderChildID", child.SalesOrderChildId),
                //new SqlParameter("@ValveType", model.ValveType ?? (object)DBNull.Value)
            };

            await dbContext.Database.ExecuteSqlRawAsync(
                "EXEC [dbo].[sp608_01InsertToJobOrderFromSalesOrderChild] " +
                "@JobOrderNo, @AddedBy, @SalesOrderChildID, @ValveType", parameters);
        }

        return Ok(new { success = true, message = "Job orders generated successfully." });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error generating job orders");
        return StatusCode(500, new { success = false, message = ex.Message });
    }
}







        [HttpGet]
        public async Task<IActionResult> GetSalesOrderByNo(string salesOrderNo)
        {
            if (string.IsNullOrWhiteSpace(salesOrderNo))
                return BadRequest();

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized();

            var order = await dbContext.Tbl40129PropertySalesOrderMasters
                .FirstOrDefaultAsync(x => x.SalesOrderNo == salesOrderNo);

            if (order == null)
                return NotFound();

                        var children = await (
                from c in dbContext.Tbl40130PropertySalesOrderChildren
                where c.SalesOrderNo == salesOrderNo
                select new
                {
                    c.SalesOrderNo,
                    c.PropertyOrdered,
                    c.UnitRateMethod,
                    c.QuotedQuantity,
                    UnitRate = c.UnitRate,
                    c.MobRate,
                    c.DemobRate
                }
            ).ToListAsync();


            // Return both master and children
            return Ok(new
            {
                order.SalesOrderNo,
                order.ClientName,
                order.SalesOrderDate,
                order.ClientPono,
                order.ClientPodate,
                order.QuoteNo,
                order.QuoteDate,
                order.ClientRefNo,
                order.ClientCode,
                order.Project,
                order.SalesPersonCode,
                order.ClientContactEmail,
                order.ClientContactNo,
                order.TypeOfRequest,
                order.QuoteSignatory,
                order.CompanyBranch,
                order.AdditionsText,
                order.OrderExpiryDate,
                order.DeliveryPeriod,
                order.DeliveryTerms,
                order.DiscountsText,
                order.PreparedBy,
                order.VerifiedBy,
                order.ApprovedBy,
                order.IsApproved,
                order.IsVerified,
                order.IsSubmitted,
                SalesOrderChildren = children
            });
        }



        [HttpDelete]
        public async Task<IActionResult> DeleteSalesOrder(string salesOrderNo)
        {
            if (string.IsNullOrWhiteSpace(salesOrderNo))
                return BadRequest(new { message = "SalesOrderNo is required." });

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { message = "Invalid tenant." });

            // Fetch the master record
            var entity = await dbContext.Tbl60201salesOrderMasters
                .FirstOrDefaultAsync(x => x.SalesOrderNo == salesOrderNo);

            if (entity == null)
                return NotFound(new { message = "Sales order not found." });

            // Fetch and delete all child records
            var children = await dbContext.Tbl60202salesOrderChildren
                .Where(x => x.SalesOrderNo == salesOrderNo)
                .ToListAsync();

            if (children.Any())
                dbContext.Tbl60202salesOrderChildren.RemoveRange(children);

            // Delete master record
            dbContext.Tbl60201salesOrderMasters.Remove(entity);

            await dbContext.SaveChangesAsync();

            return Ok(new { success = true, message = "Sales order and its child items deleted successfully." });
        }


        //[HttpGet]
        //public IActionResult GetGoodsAndServices()
        //{
        //    try
        //    {
        //        if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
        //        {
        //            var items = dbContext.Tbl20164GoodsAndServicesMasters
        //                .Where(x => x.IsDiscontinued == false) // Optional filter
        //                .Select(x => new
        //                {
        //                    x.Gscode,
        //                    x.Gsdescrpition,
        //                    x.GsdescriptionAr,
        //                    x.ItemPartNo,
        //                    x.GspackingUnit
        //                })
        //                .ToList();

        //            return Json(items);
        //        }
        //        else
        //        {
        //            return BadRequest("Unable to resolve tenant context.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Optional: Log the exception (example: using ILogger)
        //        Console.WriteLine("Error loading Goods and Services: " + ex.Message);
        //        return StatusCode(500, "An error occurred while retrieving the data.");
        //    }
        //}
        [HttpGet]
        public async Task<IActionResult> GetGoodsAndServices()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var data = await dbContext.Tbl20164GoodsAndServicesMasters
                        .Select(i => new
                        {
                            i.Gscode,
                            i.Gsdescrpition,
                            i.GsdescriptionAr,
                            i.ItemPartNo,
                            i.GspackingUnit,
                            i.IsDiscontinued,
                            i.CostPrice,
                            i.GsuoM

                        })
                        .ToListAsync();

                    return Json(data); // return raw data, paging/sorting done on client-side
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUnitCodes()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var data = await dbContext.Tbl40111PropertyUnitCodes
                        .Select(i => new
                        {
                            i.UnitDescAr,
                            i.UnitDesc,
                            i.UnitType,
                            i.UnitCode,

                        })
                        .ToListAsync();

                    return Json(data); // return raw data, paging/sorting done on client-side
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }


		[HttpGet]
		public async Task<IActionResult> GetStoreToIssueItem()
		{
			try
			{
				if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				{
					var data = await dbContext.Tbl60001storeMasters
						.Select(i => new
						{
							i.StoreId,
							i.StoreName,
							i.LedgerNo,
							i.CostAllocationUnitId,

						})
						.ToListAsync();

					return Json(data); // return raw data, paging/sorting done on client-side
				}

				return Unauthorized(new { message = "Invalid tenant.", success = false });
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error in GetProject: {ex.Message}");
				return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
			}
		}
        [HttpGet]
        public IActionResult GetTaxSlabs()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var vatTaxSlabs = dbContext.Tbl20163VatTaxSlabs
         .Select(x => new
         {
             x.TaxSlabCode,
             x.TaxSlab,
             x.TaxRate,
             x.TaxCodeToDisplay,
             x.TaxRateInWord,
            x.TaxRateIn100
         })
         .ToList();

                    return Ok(vatTaxSlabs); 
                }
            }
            catch (Exception ex)
            {
                // Ideally log the exception, don't just throw
                return StatusCode(500, new { message = ex.Message, success = false });
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        //[HttpGet]
        //public async Task<IActionResult> GetStoreStockAvailabilityGrid()
        //{
        //	try
        //	{
        //		if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
        //		{
        //			var data = await dbContext.Qry65320storeStockAvaliabilityForSalesOrders
        //				.Select(i => new
        //				{
        //					i.StoreId,
        //					i.StoreName,
        //					i.LedgerNo,
        //					i.CostAllocationUnitId,

        //				})
        //				.ToListAsync();

        //			return Json(data); // return raw data, paging/sorting done on client-side
        //		}

        //		return Unauthorized(new { message = "Invalid tenant.", success = false });
        //	}
        //	catch (Exception ex)
        //	{
        //		_logger.LogError($"Error in GetProject: {ex.Message}");
        //		return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
        //	}
        //}

        [HttpPost]
        public async Task<IActionResult> SubmitSalesOrder([FromBody] string salesOrderNo)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized(new { success = false, message = "Invalid tenant." });

                if (string.IsNullOrWhiteSpace(salesOrderNo))
                    return BadRequest(new { success = false, message = "Invalid Sales Order No." });

                var existingEntity = await dbContext.Tbl60201salesOrderMasters
                    .FirstOrDefaultAsync(x => x.SalesOrderNo == salesOrderNo);

                if (existingEntity == null)
                    return NotFound(new { success = false, message = "Sales Order not found. Please save it first." });

                existingEntity.IsSubmitted = true;
                dbContext.Tbl60201salesOrderMasters.Update(existingEntity);
                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Sales Order submitted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while submitting Sales Order.");
                return StatusCode(500, new { success = false, message = "Internal server error", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> VerifySalesOrder([FromBody] string salesOrderNo)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized(new { success = false, message = "Invalid tenant." });

                var order = await dbContext.Tbl60201salesOrderMasters
                    .FirstOrDefaultAsync(x => x.SalesOrderNo == salesOrderNo);

                if (order == null)
                    return NotFound(new { success = false, message = "Sales order not found." });

                if (order.IsSubmitted != true)
                    return BadRequest(new { success = false, message = "Please submit before verifying." });

                order.IsVerified = true;

                // ✅ Add this line to mark entity as modified
                dbContext.Tbl60201salesOrderMasters.Update(order);

                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Sales order verified successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "VerifySalesOrder error");
                return StatusCode(500, new { success = false, message = "Internal error." });
            }
        }


        [HttpPost]
        public async Task<IActionResult> ApproveSalesOrder([FromBody] string salesOrderNo)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized(new { success = false, message = "Invalid tenant." });

                if (string.IsNullOrWhiteSpace(salesOrderNo))
                    return BadRequest(new { success = false, message = "Sales Order No is required." });

                var order = await dbContext.Tbl60201salesOrderMasters
                    .FirstOrDefaultAsync(x => x.SalesOrderNo == salesOrderNo);

                if (order == null)
                    return NotFound(new { success = false, message = "Sales Order not found." });

                if (order.IsVerified != true)
                    return BadRequest(new { success = false, message = "Sales Order must be verified before approval." });

                order.IsApproved = true;
                dbContext.Tbl60201salesOrderMasters.Update(order);
                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Sales Order approved successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving Sales Order");
                return StatusCode(500, new { success = false, message = "Internal server error." });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetSalesOrderStatus(string salesOrderNo)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized(new { success = false, message = "Invalid tenant." });

                var entity = await dbContext.Tbl40129PropertySalesOrderMasters
                    .Where(x => x.SalesOrderNo == salesOrderNo)
                    .Select(x => new
                    {
                        x.IsSubmitted,
                        x.IsVerified
                    })
                    .FirstOrDefaultAsync();

                if (entity == null)
                    return NotFound(new { success = false, message = "Sales order not found." });

                return Ok(new { success = true, data = entity });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sales order status.");
                return StatusCode(500, new { success = false, message = "Internal server error." });
            }
        }

		[HttpPost]
		public async Task<IActionResult> UnlockSalesOrder([FromBody] SalesorderViewModel request)
		{
			try
			{
				if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
					return Unauthorized(new { success = false, message = "Invalid tenant." });

				if (string.IsNullOrWhiteSpace(request?.SalesOrderNo))
					return BadRequest(new { success = false, message = "Sales Order No is required." });

				var existingEntity = await dbContext.Tbl60201salesOrderMasters
					.FirstOrDefaultAsync(x => x.SalesOrderNo == request.SalesOrderNo);

				if (existingEntity == null)
					return NotFound(new { success = false, message = "Sales Order not found." });

				if (existingEntity.IsApproved != true)
					return Ok(new { success = false, message = "Sales Order is already unlocked." });

				existingEntity.IsApproved = false;
				dbContext.Tbl60201salesOrderMasters.Update(existingEntity);
				await dbContext.SaveChangesAsync();

				return Ok(new { success = true, message = "Sales Order has been unlocked successfully." });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while unlocking Sales Order.");
				return StatusCode(500, new { success = false, message = "Internal server error", details = ex.Message });
			}
		}


		[HttpGet]
		public async Task<IActionResult> CheckIfApproved(string salesOrderNo)
		{
			if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				return Unauthorized(new { success = false, message = "Invalid tenant." });

			if (string.IsNullOrWhiteSpace(salesOrderNo))
				return BadRequest(new { success = false, message = "Sales Order No is required." });

			var existingEntity = await dbContext.Tbl60201salesOrderMasters
				.Where(x => x.SalesOrderNo == salesOrderNo)
				.Select(x => new { x.IsApproved })
				.FirstOrDefaultAsync();

			if (existingEntity == null)
				return NotFound(new { success = false, message = "Sales Order not found." });

			return Ok(new { success = true, isApproved = existingEntity.IsApproved == true });
		}

		[HttpGet]
		public IActionResult CheckDeliveryExists(string salesOrderNo)
		{
			if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
				return Unauthorized(new { message = "Invalid tenant." });

			// Use the correct DbSet name here
			bool exists = dbContext.Tbl60301deliveryNoteMasters.Any(d => d.SalesOrderNo == salesOrderNo);
			return Ok(new { exists });
		}




        public class ClientPOViewModel
        {
            public long SlNo { get; set; }
            public string SalesOrderNo { get; set; }
            public string ClientPONo { get; set; }
            public DateTime? ClientPODate { get; set; }
            public string ClientPOType { get; set; }
            public DateTime? ClientPOEndDate { get; set; }
            public decimal ClientPOValue { get; set; }
        }





        [HttpPost]
        public async Task<IActionResult> SaveClientPO([FromBody] ClientPOViewModel model)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized(new { success = false, message = "Invalid tenant." });

                if (model == null)
                    return BadRequest(new { success = false, message = "Invalid data." });

                bool isUpdate = false;

                // ✅ Check if record exists (using SlNo as PK)
                var existingEntity = await dbContext.Tbl40145EqpSalesOrderPodetails
                    .FirstOrDefaultAsync(x => x.SlNo == model.SlNo);

                if (existingEntity != null)
                {
                    isUpdate = true;

                    existingEntity.SalesOrderNo = model.SalesOrderNo?.Trim();
                    existingEntity.ClientPono = model.ClientPONo;
                    existingEntity.ClientPodate = model.ClientPODate;
                    existingEntity.ClientPotype = model.ClientPOType;
                    existingEntity.ClientPoendDate = model.ClientPOEndDate;
                    existingEntity.ClientPovalue = model.ClientPOValue;

                    dbContext.Tbl40145EqpSalesOrderPodetails.Update(existingEntity);
                }
                else
                {
                    var entity = new Tbl40145EqpSalesOrderPodetail
                    {
                        SalesOrderNo = model.SalesOrderNo?.Trim(),
                        ClientPono = model.ClientPONo,
                        ClientPodate = model.ClientPODate,
                        ClientPotype = model.ClientPOType,
                        ClientPoendDate = model.ClientPOEndDate,
                        ClientPovalue = model.ClientPOValue
                    };

                    await dbContext.Tbl40145EqpSalesOrderPodetails.AddAsync(entity);
                }

                await dbContext.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = isUpdate ? "Client PO updated successfully." : "Client PO created successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SaveClientPO");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while saving Client PO.",
                    details = ex.Message
                });
            }
        }




        [HttpGet]
        public async Task<IActionResult> GetClientPONos()
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized(new { success = false, message = "Invalid tenant." });

                // Fetch only ClientPONo values
                var clientPONos = await dbContext.Tbl40145EqpSalesOrderPodetails
                    .Select(x => new
                    {
                        x.SlNo,
                        x.ClientPono
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = clientPONos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetClientPONos");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while fetching Client PO Nos.",
                    details = ex.Message
                });
            }
        }







    }








}




