using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using QD.ERP.Web.Services.Logging;
using System.Dynamic;

namespace QD.ERP.Web.Areas.ERM.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AddNewQuotation1Controller : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<AddNewQuotation1Controller> _logger;
        private readonly IUserActionLogger _userActionLogger;


        public AddNewQuotation1Controller(ILogger<AddNewQuotation1Controller> logger, TenantDbContextHelper tenantDbContextHelper, IUserActionLogger userActionLogger)
        {
            _userActionLogger = userActionLogger;
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        //[HttpGet]
        //public ActionResult<string> GetNewDebitNoteNoApi()
        //{
        //	try
        //	{
        //		if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
        //		{


        //			var company = dbContext.Tbl901CompanyDetails
        //								   .FirstOrDefault(c => c.CompanyNameShort == "Pulse Infotech");


        //			if (company == null)
        //			{
        //				return NotFound("Company not found.");
        //			}

        //			string invoiceAbbrv = company.InvoiceAbbrv;
        //			int invoiceYearDigits = company.InvoiceYearDigits ?? 0;

        //			bool isResetInvoiceInYear = company.IsResetInvoiceInYear ?? false;

        //			DateTime invoiceDate = DateTime.Now;



        //			// Step 4: Generate New Debit Note No
        //			string newDebitNoteNo = GetNewDebitNoteNo(invoiceAbbrv, invoiceYearDigits, invoiceDate, isResetInvoiceInYear, dbContext);

        //			return Ok(newDebitNoteNo);
        //		}
        //		else
        //		{
        //			return BadRequest("Tenant or DB Context not found.");
        //		}
        //	}
        //	catch (Exception ex)
        //	{
        //		return StatusCode(500, "Internal server error: " + ex.Message);
        //	}
        //}

        [HttpGet]
        public ActionResult<string> GetNewDebitNoteNoApi()
        {
            try
            {
                // Step 1: Get tenant name from session
                var tenantName = HttpContext.Session.GetString("TenantName");
                if (string.IsNullOrWhiteSpace(tenantName))
                {
                    return Unauthorized(new { message = "Tenant name not found in session.", success = false });
                }

                // Step 2: Try to get DbContext for tenant
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    // Step 3: Get company from Tbl901CompanyDetails using tenantName
                    var company = dbContext.Tbl901CompanyDetails
                                           .FirstOrDefault(c => c.CompanyNameShort == tenantName);

                    if (company == null)
                    {
                        return NotFound("Company not found in Tbl901CompanyDetails.");
                    }

                    // Step 4: Get NoOfDigitsToInventoryQuotation using CompanyId from Tbl901CompanyDetails02
                    int noOfDigits = dbContext.Tbl901CompanyDetails02s
                    .Where(c => c.CompanyId == company.CompanyId)
                    .Select(c => c.NoOfDigitsToEquipmentQuotation ?? 4)
                    .FirstOrDefault(); // Default to 4 if not found

                    // Step 5: Extract values for quotation number
                    string EquipQuoteAbbrv = company.EquipQuoteAbbrv;
                    int invoiceYearDigits = company.InvoiceYearDigits ?? 0;
                    bool isResetInvoiceInYear = company.IsResetInvoiceInYear ?? false;
                    DateTime invoiceDate = DateTime.Now;
                    // Generate new debit note QuotationAbbrv
                    string newDebitNoteNo = GetNewDebitNoteNo(EquipQuoteAbbrv, invoiceYearDigits, invoiceDate, isResetInvoiceInYear, noOfDigits, dbContext);



                    return Ok(newDebitNoteNo);
                }
                else
                {
                    return BadRequest("Tenant or DB Context not found.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetNewDebitNoteNoApi: {ex.Message}");
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }



        private string GetNewDebitNoteNo(string EquipQuoteAbbrv, int yearInDigit, DateTime invoiceDate, bool isResetByYear, int noOfDigits, ERPMasterWtDataContext dbContext)
        {
            try
            {
                var mprNumbers = dbContext.Tbl40103PropertyQuoteMasters
                    .Where(d => d.QuoteNo != null &&
                                d.QuoteNo.Length >= noOfDigits &&
                                (!isResetByYear || (d.QuoteDate.HasValue && d.QuoteDate.Value.Year == invoiceDate.Year)))
                    .Select(d => d.QuoteNo)
                    .ToList();

                int maxRunningNumber = mprNumbers
                    .Select(no => int.TryParse(no.Substring(no.Length - noOfDigits), out int num) ? num : 0)
                    .DefaultIfEmpty(0)
                    .Max();

                maxRunningNumber += 1;

                string strNewDebitNoteNo = maxRunningNumber.ToString().PadLeft(noOfDigits, '0');

                string strYear = invoiceDate.Year.ToString();
                if (yearInDigit > 0)
                    strYear = strYear.Substring(strYear.Length - yearInDigit, yearInDigit);
                else
                    strYear = "";

                return $"{EquipQuoteAbbrv}{strYear}-{strNewDebitNoteNo}";
            }
            catch (Exception)
            {
                string strYear = invoiceDate.Year.ToString();
                if (yearInDigit > 0)
                    strYear = strYear.Substring(strYear.Length - yearInDigit, yearInDigit);
                else
                    strYear = "";

                return $"{EquipQuoteAbbrv}{strYear}-{"1".PadLeft(noOfDigits, '0')}";
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetQuotationdataByCode([FromQuery] string QuoteNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                if (string.IsNullOrEmpty(QuoteNo))
                    return BadRequest("Quote No is required.");

                try
                {
                    //Tbl40103PropertyQuoteMasters
                    var client = await dbContext.Tbl40103PropertyQuoteMasters
                        .Where(c => c.QuoteNo == QuoteNo)
                        .FirstOrDefaultAsync();

                    if (client == null)
                        return NotFound("Quotation not found.");

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
        public async Task<ActionResult> GetQuotationChildren(string QuoteNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var resultWithDetails = new List<ExpandoObject>();

                    // Query the Tbl60602purchaseRequestChildren table for the given Mprno
                    var result = dbContext.Tbl60102quotationChildren
                        .Where(x => x.QuoteNo == QuoteNo)
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














        public class QuotationViewModel
        {
            public string QuoteNo { get; set; }
            public byte? RevisionNo { get; set; }
            public DateTime? QuoteDate { get; set; }
            public string ClientCode { get; set; }
            public string SalesPersonCode { get; set; }
            public string ClientRefNo { get; set; }
            public string Attention { get; set; }
            public string ClientContactEmail { get; set; }
            public string ClientContactNo { get; set; }
            public byte? ModeOfRequest { get; set; }
            public byte? TypeOfRequest { get; set; }
            public string Project { get; set; }
            public string JobSite { get; set; }
            public string TransportationScope { get; set; }
            public string SubjectTitle { get; set; }
            public string QuotationSummary { get; set; }
            public string QuoteIntro { get; set; }
            public string QuoteThanksNote { get; set; }
            public string AddedBy { get; set; }
            public DateTime? AddedOn { get; set; }
            public DateTime? QuoteDueDate { get; set; }
            public string QuoteType { get; set; }
            public string AdditionsText { get; set; }
            public decimal? QuoteTransport { get; set; }
            public string DiscountsText { get; set; }
            public decimal? QuoteDiscount { get; set; }
            public byte? CompanyBranch { get; set; }
            public string ReferenceNo { get; set; }
            public byte? VATApplicableRate { get; set; }
            public string PreparedBy { get; set; }
            public byte? VerifiedSignatory { get; set; }
            public byte? ApprovedSignatory { get; set; }

            public decimal? CurrencyRate { get; set; }
            public int? BaseCurrencyId { get; set; }
            public int? CurrencyId { get; set; }




















            //public byte? TypeOfQuote { get; set; }





            //public byte? PaymentTerms { get; set; }

            //public byte? DeliveryPeriod { get; set; }

            //public byte? DeliveryTerms { get; set; }

            //public string QuoteValidity { get; set; }


            //public DateTime? PreparedOn { get; set; }

            //public string ApprovedBy { get; set; }

            //public DateTime? ApprovedOn { get; set; }






            //public string ModifiedBy { get; set; }

            //public DateTime? ModifiedOn { get; set; }

            //public string Rfqcode { get; set; }





            //public byte? QuoteSignatory { get; set; }









            //public bool? IsVerified { get; set; }

            //public bool? IsApproved { get; set; }



            //public string Mprno { get; set; }



            //public string QuoteColumn2 { get; set; }


            //public string QuoteLabel1 { get; set; }


            //public string QuoteLabel3 { get; set; }

            //public DateTime? QuoteSubmittedOn { get; set; }

            //public string QuoteSubmittedBy { get; set; }

            //public byte? QuoteStatus { get; set; }

            //public byte? InventoryMasterGroupId { get; set; }



            //public bool? IsSubmitted { get; set; }

            //public string SubmittedBy { get; set; }

            //public DateTime? SubmittedOn { get; set; }

            //public string VerifiedBy { get; set; }

            //public DateTime? VerifiedOn { get; set; }

            //public string ProjectMasterCode { get; set; }

            //public DateTime? BidClosingDate { get; set; }




            //public decimal? CurrencyRate { get; set; }
            //public int? BaseCurrencyId { get; set; }
            //public int? CurrencyId { get; set; }
            public List<Tbl40104PropertyQuoteChild> QuotationDetailses { get; set; }


        }






        [HttpPost]
        public async Task<IActionResult> SaveOrUpdateQuotation([FromBody] QuotationViewModel VM)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant context." });
            }

            if (VM == null || string.IsNullOrEmpty(VM.QuoteNo))
            {
                return BadRequest(new { success = false, message = "Quote No. is required." });
            }

            try
            {
                // Check if the master record exists
                var existingMaster = await dbContext.Tbl40103PropertyQuoteMasters
                    .FirstOrDefaultAsync(x => x.QuoteNo == VM.QuoteNo);

                if (existingMaster != null)
                {
                    //Update existing master with manual property mapping

                    existingMaster.RevisionNo = VM.RevisionNo;
                    existingMaster.QuoteDate = VM.QuoteDate;
                    existingMaster.ClientCode = VM.ClientCode;
                    existingMaster.SalesPersonCode = VM.SalesPersonCode;
                    existingMaster.ClientRefNo = VM.ClientRefNo;
                    existingMaster.Attention = VM.Attention;
                    existingMaster.ClientContactEmail = VM.ClientContactEmail;
                    existingMaster.ClientContactNo = VM.ClientContactNo;
                    existingMaster.ModeOfRequest = VM.ModeOfRequest.HasValue ? (byte?)VM.ModeOfRequest.Value : null;
                    existingMaster.TypeOfRequest = VM.TypeOfRequest.HasValue ? (byte?)VM.TypeOfRequest.Value : null;
                    existingMaster.Project = VM.Project;
                    existingMaster.JobSite = VM.JobSite;
                    existingMaster.TransportationScope = VM.TransportationScope;
                    existingMaster.SubjectTitle = VM.SubjectTitle;
                    existingMaster.QuotationSummary = VM.QuotationSummary;
                    existingMaster.QuoteIntro = VM.QuoteIntro;
                    existingMaster.QuoteThanksNote = VM.QuoteThanksNote;
                    existingMaster.AddedBy = VM.AddedBy;
                    existingMaster.AddedOn = VM.AddedOn;
                    existingMaster.QuoteDate = VM.QuoteDueDate;
                    existingMaster.QuoteType = VM.QuoteType;
                    existingMaster.AdditionsText = VM.AdditionsText;
                    existingMaster.QuoteTransport = VM.QuoteTransport;
                    existingMaster.DiscountsText = VM.DiscountsText;
                    existingMaster.QuoteDiscount = VM.QuoteDiscount;
                    existingMaster.CompanyBranch = VM.CompanyBranch.HasValue ? (byte?)VM.CompanyBranch.Value : null;
                    existingMaster.ReferenceNo = VM.ReferenceNo;
                    existingMaster.VatapplicableRate = VM.VATApplicableRate;
                    existingMaster.PreparedBy = VM.PreparedBy;
                    existingMaster.VerifiedSignatory = VM.VerifiedSignatory;
                    existingMaster.ApprovedSignatory = VM.ApprovedSignatory;
                    existingMaster.CurrencyId = VM.CurrencyId ?? 1;
                    existingMaster.CurrencyRate = VM.CurrencyRate ?? 1;
                    existingMaster.BaseCurrencyId = VM.BaseCurrencyId ?? 1;


                }
                else
                {
                    // Insert new master
                    var newMaster = new Tbl40103PropertyQuoteMaster
                    {

                        QuoteNo = VM.QuoteNo,
                        RevisionNo = VM.RevisionNo,
                        QuoteDate = VM.QuoteDate,
                        ClientCode = VM.ClientCode,
                        SalesPersonCode = VM.SalesPersonCode,
                        ClientRefNo = VM.ClientRefNo,
                        Attention = VM.Attention,
                        ClientContactEmail = VM.ClientContactEmail,
                        ClientContactNo = VM.ClientContactNo,
                        ModeOfRequest = Convert.ToByte(VM.ModeOfRequest),
                        TypeOfRequest = Convert.ToByte(VM.TypeOfRequest),
                        Project = VM.Project,
                        JobSite = VM.JobSite,
                        TransportationScope = VM.TransportationScope,
                        SubjectTitle = VM.SubjectTitle,
                        QuotationSummary = VM.QuotationSummary,
                        QuoteIntro = VM.QuoteIntro,
                        QuoteThanksNote = VM.QuoteThanksNote,
                        AddedBy = VM.AddedBy,
                        AddedOn = VM.AddedOn,
                        QuoteDueDate = VM.QuoteDueDate,
                        QuoteType = VM.QuoteType,
                        AdditionsText = VM.AdditionsText,
                        QuoteTransport = VM.QuoteTransport,
                        DiscountsText = VM.DiscountsText,
                        QuoteDiscount = VM.QuoteDiscount,
                        CompanyBranch = Convert.ToByte(VM.CompanyBranch),
                        ReferenceNo = VM.ReferenceNo,
                        VatapplicableRate = VM.VATApplicableRate,
                        PreparedBy = VM.PreparedBy,
                        VerifiedSignatory = VM.VerifiedSignatory,
                        ApprovedSignatory = VM.ApprovedSignatory,
                        CurrencyId = VM.CurrencyId ?? 1,
                        CurrencyRate = VM.CurrencyRate ?? 1,
                        BaseCurrencyId = VM.BaseCurrencyId ?? 1,


                    };

                    await dbContext.Tbl40103PropertyQuoteMasters.AddAsync(newMaster);
                }

                // Handle child entries
                var existingChildren = await dbContext.Tbl40104PropertyQuoteChildren
                    .Where(x => x.QuoteNo == VM.QuoteNo)
                    .ToListAsync();



                //var currencyRate = await dbContext.Tbl60101quotationMasters
                //            .Where(x => x.QuoteNo == VM.QuoteNo)
                //            .Select(x => x.CurrencyRate)
                //            .FirstOrDefaultAsync();



                // Track QuoteChildId from client
                var incomingIds = VM.QuotationDetailses
                    .Where(x => x.QuoteChildId > 0)
                    .Select(x => x.QuoteChildId)
                    .ToList();

                // Delete missing children
                var toDelete = existingChildren
                    .Where(x => !incomingIds.Contains(x.QuoteChildId))
                    .ToList();

                if (toDelete.Any())
                {
                    dbContext.Tbl40104PropertyQuoteChildren.RemoveRange(toDelete);
                }


                foreach (var child in VM.QuotationDetailses)
                {
                    if (child.QuoteChildId == 0)
                    {
                        // New child entry
                        child.QuoteNo = VM.QuoteNo;
                        child.LineOrderNo = child.LineOrderNo;
                        child.AddlNotes = child.AddlNotes;
                        child.PropertyAddlDescription = child.PropertyAddlDescription;
                        child.QuotedQuantity = child.QuotedQuantity;
                        child.QuoteMethod = child.QuoteMethod;
                        child.UnitRate1 = child.UnitRate1 * VM.CurrencyRate;
                        child.MobRate = child.MobRate * VM.CurrencyRate;
                        child.DemobRate = child.DemobRate * VM.CurrencyRate;

                        await dbContext.Tbl40104PropertyQuoteChildren.AddAsync(child);
                    }
                    else
                    {
                        // Existing child entry
                        var existingChild = existingChildren
                            .FirstOrDefault(x => x.QuoteChildId == child.QuoteChildId);

                        if (existingChild != null)
                        {
                            existingChild.LineOrderNo = child.LineOrderNo;
                            existingChild.AddlNotes = child.AddlNotes;
                            existingChild.PropertyAddlDescription = child.PropertyAddlDescription;
                            existingChild.QuotedQuantity = child.QuotedQuantity;
                            existingChild.UnitRateMethod1 = child.UnitRateMethod1;
                            existingChild.UnitRate1 = (child.UnitRate1 ?? 0) * (VM.CurrencyRate ?? 1);
                            existingChild.MobRate = (child.MobRate ?? 0) * (VM.CurrencyRate ?? 1);
                            existingChild.DemobRate = (child.DemobRate ?? 0) * (VM.CurrencyRate ?? 1);
                        }
                    }
                }

                // await dbContext.SaveChangesAsync();

                var rows = await dbContext.SaveChangesAsync();

                await _userActionLogger.LogAsync(
                    module: "ERM > Save Quotation",
                   actionDetail: $"Saved Quotation: {VM.QuoteNo}",
                    documentNo: $"{VM.QuoteNo}"
                );

                return Ok(new { success = true, message = "Quotation Details saved/updated successfully.", quoteno = VM.QuoteNo });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

















        public class QuotationDetailsRequest
        {
            public string QuoteNo { get; set; }
            public string DetailedDescription { get; set; }
            public string Certification { get; set; }
            public string Capacity { get; set; }
            public string Operator { get; set; }
            public string Attachment { get; set; }
            public byte? UnitRateMethod2 { get; set; }
            public decimal? UnitRate2 { get; set; }
            public byte? UnitRateMethod3 { get; set; }
            public decimal? UnitRate3 { get; set; }
            public string Notes { get; set; }
            public string AdditionalNotes { get; set; }
            public decimal? MobilizationRate { get; set; }
            public decimal? DemobRate { get; set; }
            public string DeliveryDetails { get; set; }
            public decimal? LineOrderNo { get; set; }
        }







        [HttpPost]
        public async Task<IActionResult> SaveOrUpdateQuotationChild([FromBody] QuotationDetailsRequest request)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant context." });
            }

            if (request == null || string.IsNullOrEmpty(request.QuoteNo))
            {
                return BadRequest(new { success = false, message = "Quote No is required." });
            }

            try
            {
                // ✅ Check if child already exists for given QuoteNo + LineOrderNo
                var existing = await dbContext.Tbl40104PropertyQuoteChildren
                    .FirstOrDefaultAsync(x => x.QuoteNo == request.QuoteNo && x.LineOrderNo == request.LineOrderNo);

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
                    decimal nextLineOrderNo = (await dbContext.Tbl40104PropertyQuoteChildren
                        .Where(x => x.QuoteNo == request.QuoteNo)
                        .MaxAsync(x => (decimal?)x.LineOrderNo)) ?? 0;

                    nextLineOrderNo++; // increment

                    // ➕ Insert new record
                    var newChild = new Tbl40104PropertyQuoteChild
                    {
                        QuoteNo = request.QuoteNo,
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

                    await dbContext.Tbl40104PropertyQuoteChildren.AddAsync(newChild);
                }

                await dbContext.SaveChangesAsync();

                await _userActionLogger.LogAsync(
                    module: "ERM > Save Quotation Child",
                    actionDetail: $"Saved Quotation Child: {request.QuoteNo} - {request.LineOrderNo}",
                    documentNo: $"{request.QuoteNo}"
                );

                return Ok(new { success = true, message = "Quotation Child saved successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }





























        [HttpGet]
        public async Task<ActionResult> GetQuoteGridData(string QuoteNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var currencyRate = await dbContext.Tbl40103PropertyQuoteMasters
                        .Where(x => x.QuoteNo == QuoteNo)
                        .Select(x => x.CurrencyRate)
                        .FirstOrDefaultAsync();

                    var result = dbContext.Tbl40104PropertyQuoteChildren
                        .Where(x => x.QuoteNo == QuoteNo)
                        .Select(x => new
                        {
                            LineOrderNo = x.LineOrderNo,
                            Description = x.AddlNotes,                  // adjust if needed
                            DetailedDescription = x.PropertyAddlDescription,
                            Qty = x.QuotedQuantity,
                            UnitMethod = x.QuoteMethod,
                            Rate1 = x.UnitRate1 / currencyRate,
                            MobRate = x.MobRate / currencyRate,
                            DemobRate = x.DemobRate / currencyRate
                        })
                        .ToList();

                    return Json(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetProject: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }



        public class QuoteChildInsertRequest
        {
            public string QuoteNo { get; set; }          // Parent Quote Number (required)

            // Core fields

            public string PropertyAddlDescription { get; set; }


            public string DetailedDescription { get; set; }
            public decimal? QuotedQuantity { get; set; }
            public string QuoteMethod { get; set; }
            public byte? UnitRateMethod { get; set; }

            // Extra fields (map to your child table)
            public string Certification { get; set; }
            public string Capacity { get; set; }
            public string Operator { get; set; }
            public string Attachment { get; set; }
            public byte? UnitRateMethod2 { get; set; }
            public decimal? UnitRate2 { get; set; }
            public byte? UnitRateMethod3 { get; set; }
            public decimal? UnitRate3 { get; set; }
            public string Notes { get; set; }
            public string AdditionalNotes { get; set; }

            // Mobilization/Demobilization
            public decimal? MobilizationRate { get; set; }
            public decimal? DemobRate { get; set; }

            // Delivery details
            public string DeliveryDetails { get; set; }

            public string QuotedUom { get; set; }
            public string EquipmentQuotedFor { get; set; }


        }


        [HttpPost]
        public async Task<IActionResult> AddQuoteChild([FromBody] QuoteChildInsertRequest request)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { message = "Invalid tenant.", success = false });

            if (request == null || string.IsNullOrEmpty(request.QuoteNo))
                return BadRequest(new { success = false, message = "QuoteNo is required." });

            try
            {
                // 1️⃣ Get max existing LineOrderNo for this quote
                decimal nextLineOrderNo = 1;
                var existingMax = await dbContext.Tbl40104PropertyQuoteChildren
                    .Where(x => x.QuoteNo == request.QuoteNo && x.LineOrderNo > 0)
                    .MaxAsync(x => (decimal?)x.LineOrderNo);

                if (existingMax.HasValue)
                    nextLineOrderNo = existingMax.Value + 1;

                // 2️⃣ Get last row (highest LineOrderNo)
                var lastRow = await dbContext.Tbl40104PropertyQuoteChildren
                    .Where(x => x.QuoteNo == request.QuoteNo)
                    .OrderByDescending(x => x.LineOrderNo)
                    .FirstOrDefaultAsync();

                Tbl40104PropertyQuoteChild targetRow;
                bool didInsert;

                // 3️⃣ Decide Insert or Update based on HasEquipmentDetails
                if (lastRow != null && lastRow.HasEquipmentDetails == "Yes")
                {
                    // ✏️ Update last row if Equipment Details exist
                    targetRow = lastRow;
                    targetRow.EquipmentQuotedFor = request.PropertyAddlDescription;
                    targetRow.QuotedQuantity = request.QuotedQuantity;
                    targetRow.QuoteMethod = request.QuoteMethod;
                    targetRow.UnitRateMethod = request.UnitRateMethod;
                    targetRow.QuotedUom = request.QuotedUom;

                    // mark that Equipment Details were updated
                    targetRow.HasEquipmentDetails = "Updated";
                    didInsert = false;
                }
                else
                {
                    // ➕ Insert a new row if no last row OR last row does not have Equipment Details
                    targetRow = new Tbl40104PropertyQuoteChild
                    {
                        QuoteNo = request.QuoteNo,
                        LineOrderNo = nextLineOrderNo,
                        EquipmentQuotedFor = request.PropertyAddlDescription,
                        QuotedQuantity = request.QuotedQuantity,
                        QuoteMethod = request.QuoteMethod,
                        UnitRateMethod = request.UnitRateMethod,
                        QuotedUom = request.QuotedUom,
                        HasEquipmentDetails = "No" // default for newly added without details
                    };

                    await dbContext.Tbl40104PropertyQuoteChildren.AddAsync(targetRow);
                    didInsert = true;
                }

                await dbContext.SaveChangesAsync();

                // 4️⃣ Prepare response
                dynamic rowData = new ExpandoObject();
                var dict = (IDictionary<string, object>)rowData;
                dict["LineOrderNo"] = targetRow.LineOrderNo;
                dict["DetailedDescription"] = targetRow.PropertyAddlDescription;
                dict["MobRate"] = targetRow.MobRate;
                dict["DemobRate"] = targetRow.DemobRate;

                return Ok(new
                {
                    success = true,
                    message = didInsert ? "Child row inserted successfully." : "Child row updated successfully.",
                    rowData = rowData
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }





















        [HttpGet]
        public async Task<ActionResult> GetQuoteChildDetails(string quoteNo, int lineOrderNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var gridDetails = await dbContext.Tbl40104PropertyQuoteChildren
                        .Where(x => x.QuoteNo == quoteNo && x.LineOrderNo == lineOrderNo)
                        .FirstOrDefaultAsync();

                    if (gridDetails == null)
                        return Ok(null); // return null for easier frontend handling

                    dynamic item = new ExpandoObject();
                    var dict = (IDictionary<string, object>)item;

                    // Map only required fields
                    dict["DetailedDescription"] = gridDetails.PropertyAddlDescription;
                    dict["MobRate"] = gridDetails.MobRate;
                    dict["DemobRate"] = gridDetails.DemobRate;

                    return Json(item);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }





        [HttpDelete]
        public async Task<IActionResult> DeleteQuotation([FromQuery] string QuoteNo)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant context." });
            }

            if (string.IsNullOrEmpty(QuoteNo))
            {
                return BadRequest(new { success = false, message = "QuoteNo. is required." });
            }

            try
            {
                // Retrieve the master record
                var masterRecord = await dbContext.Tbl40103PropertyQuoteMasters
                    .FirstOrDefaultAsync(x => x.QuoteNo == QuoteNo);

                if (masterRecord == null)
                {
                    return NotFound(new { success = false, message = "Quotation not found." });
                }

                // Retrieve and remove child records
                var childRecords = dbContext.Tbl40104PropertyQuoteChildren
                    .Where(x => x.QuoteNo == QuoteNo);

                dbContext.Tbl40104PropertyQuoteChildren.RemoveRange(childRecords);

                // Remove the master record
                dbContext.Tbl40103PropertyQuoteMasters.Remove(masterRecord);

                await dbContext.SaveChangesAsync();

                await _userActionLogger.LogAsync(
                    module: "ERM > Delete Quotation",
                   actionDetail: $"Deleted Quotation: {QuoteNo}",
                    documentNo: $"{QuoteNo}"
                );

                return Ok(new { success = true, message = "Quotation details deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }



        [HttpPost]
        public async Task<IActionResult> SubmitQuotation(string QuoteNo)
        {
            // Validate tenant context
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant context." });
            }

            // Validate MPR number
            if (string.IsNullOrEmpty(QuoteNo))
            {
                return BadRequest(new { success = false, message = "Quote No. is required." });
            }

            // Retrieve MPR master record
            var master = await dbContext.Tbl40103PropertyQuoteMasters.FirstOrDefaultAsync(x => x.QuoteNo == QuoteNo);
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

            await _userActionLogger.LogAsync(
                    module: "ERM >  Quotation Submit",
                   actionDetail: $" Quotation Submitted: {QuoteNo}",
                    documentNo: $"{QuoteNo}"
                );

            return Ok(new { success = true, message = "Quotation submitted successfully." });
        }
        [HttpPost]
        public async Task<IActionResult> VerifyQuotation(string QuoteNo)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { message = "Invalid tenant context." });
            }

            if (string.IsNullOrEmpty(QuoteNo))
            {
                return BadRequest(new { message = "Quote is required." });
            }

            var voucher = await dbContext.Tbl40103PropertyQuoteMasters
                .FirstOrDefaultAsync(v => v.QuoteNo == QuoteNo);

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

            await _userActionLogger.LogAsync(
                    module: "ERM > Verify Quotation",
                   actionDetail: $"Verified Quotation: {QuoteNo}",
                    documentNo: $"{QuoteNo}"
                );

            return Ok(new
            {
                message = "Quotation has been Verified and processed for Approval."
            });
        }
        [HttpPost]
        public async Task<ActionResult> ApproveQuotation(string QuoteNo)
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


                    if (string.IsNullOrEmpty(QuoteNo))
                    {
                        return BadRequest(new { Message = "Quote number is required." });
                    }

                    var voucher = dbContext.Tbl40103PropertyQuoteMasters
                                           .FirstOrDefault(v => v.QuoteNo == QuoteNo);

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
                    await _userActionLogger.LogAsync(
                    module: "ERM > Approve Quotation",
                   actionDetail: $"Approved Quotation: {QuoteNo}",
                    documentNo: $"{QuoteNo}"
                );

                    return Ok(new
                    {
                        Message = "Quotation has been Approved.",
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
        [HttpGet]
        public async Task<IActionResult> GetQuotationStatus(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var clients = dbContext.Tbl60107quotationStatuses.Select(i => new
                    {
                        i.QuoteStatusId,
                        i.QuoteStatus
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
        public async Task<IActionResult> GetClientDetailz(string clientCode)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var client = await dbContext.Tbl30101ClientMasters
                        .Where(c => c.ClientCode == clientCode)
                        .Select(c => new
                        {
                            contactPerson = c.ContactPerson,
                            contactPersonTitle = c.ContactPersonTitle,
                            contactEmail = c.ContactEmail,
                            contactMobile1 = c.ContactMobile1
                        })
                        .FirstOrDefaultAsync();

                    if (client == null)
                        return NotFound(new { message = "Client not found" });

                    return Ok(client);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetClientDetailsById: {ex.Message}");
                    return StatusCode(500, new { message = "Error fetching client details", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant", success = false });
        }



        [HttpGet]
        public async Task<IActionResult> CheckQuoteNo(string quoteNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var exists = await dbContext.Tbl40103PropertyQuoteMasters
                        .AnyAsync(q => q.QuoteNo == quoteNo);

                    if (!exists)
                        return NotFound(new { message = "Quotation No not found", exists = false });

                    return Ok(new { message = "Quotation No already exists", exists = true });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in CheckQuoteNo: {ex.Message}");
                    return StatusCode(500, new { message = "Error checking quotation no", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant", success = false });
        }










    }
}
