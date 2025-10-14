using DevExpress.XtraRichEdit.Import.Html;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using QD.ERP.Shared.DAL.Entities;
using QD.ERP.Shared.Service;
using QD.ERP.Shared.Services.Logging;
using SkiaSharp;
using System.Globalization;

namespace QD.ERP.IMS.Areas.IMS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class IMSLedgerDocumentController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<IMSLedgerDocumentController> _logger;
        private readonly IUserActionLogger _userActionLogger;


        public IMSLedgerDocumentController(ILogger<IMSLedgerDocumentController> logger, TenantDbContextHelper tenantDbContextHelper, IUserActionLogger userActionLogger)
        {
            _userActionLogger = userActionLogger;
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }



        [HttpGet]
        public async Task<IActionResult> GetDocumentTypes(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var DocumentTypes = dbContext.Tbl101DocumentTypes.Select(i => new
                    {
                        i.DocumentTypeId,
                        i.DocumentType,
                       
                    });

                    return Json(await DataSourceLoader.LoadAsync(DocumentTypes, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetProject: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        public async Task<IActionResult> GetNewDocumentNo()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    // First, retrieve all documents from the database
                    var documents = await dbContext.Tbl20116LedgerDocuments
                        .Where(x => !string.IsNullOrEmpty(x.DocumentNo))
                        .Select(x => x.DocumentNo)
                        .ToListAsync();

                    // Now, filter and find the highest numeric DocumentNo in memory
                    var highestNo = documents
                        .Where(docNo => int.TryParse(docNo, out _)) // Filter only numeric DocumentNos
                        .Select(docNo => int.Parse(docNo))
                        .Max(); // Get the highest numeric DocumentNo

                    int nextNumber = highestNo + 1;

                    var nextDocNo = $"{nextNumber}"; // Example: 24
                    return Ok(nextDocNo);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetNewDocumentNo: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while generating the document number.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }


        [HttpPost]
    
        public async Task<IActionResult> AddDocumentsEntry([FromBody] Tbl20116LedgerDocument documentDetails, string DocumentType)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {

                if (documentDetails == null)
            {
                return BadRequest(new { success = false, message = "Invalid data received." });
            }

            try
            {
                    //// ✅ Convert DocumentExpDate to Hijri (Arabic) format
                    //if (documentDetails.DocumentExpDate.HasValue)
                    //{
                    //    HijriCalendar hijriCalendar = new HijriCalendar();
                    //    DateTime expDate = documentDetails.DocumentExpDate.Value;

                    //    string hijriDate = $"{hijriCalendar.GetYear(expDate)}/{hijriCalendar.GetMonth(expDate):D2}/{hijriCalendar.GetDayOfMonth(expDate):D2}";

                    //    // ✅ Set Arabic date field
                    //    documentDetails.DocumentExpDateAr = hijriDate;
                    //}

                    // Add to DB
                    dbContext.Tbl20116LedgerDocuments.Add(documentDetails);
                await dbContext.SaveChangesAsync();
                    await _userActionLogger.LogAsync(
                      module: "IMS > Add Documents Entry",
                      actionDetail: $":Added Documents Entry  {documentDetails.DocumentNo}",
                      documentNo: $"{documentDetails.DocumentNo}"
                    );

                    // Filter: Only records from current month for the same DocumentRefNo
                    var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;

                var qryListOfAccountlists = dbContext.Tbl20116LedgerDocuments
                    .Where(p => p.DocumentExpDate.HasValue &&
                                p.DocumentExpDate.Value.Month == currentMonth &&
                                p.DocumentExpDate.Value.Year == currentYear ) 
                              //  p.DocumentRefNo == documentDetails.DocumentRefNo
                             
                    .Select(i => new
                    {
                        i.DocumentNo,
                        i.DocumentType,
                        i.DocumentRefNo,
                        i.DocumentRemarks,
                        i.DocumentExpDate,
                        i.DocumentExpDateAr,
                      i.DocumentNotificationDate
                    });

                var result = await qryListOfAccountlists.ToListAsync();

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }
         }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateDocumentEntries([FromBody] List<Tbl20116LedgerDocument> documents)
        {
            try
            {



                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    foreach (var doc in documents)
                    {
                        var existing = await dbContext.Tbl20116LedgerDocuments
                            .FirstOrDefaultAsync(d => d.DocumentNo == doc.DocumentNo);

                        if (existing != null)
                        {
                            existing.DocumentType = doc.DocumentType;
                            existing.DocumentRefNo = doc.DocumentRefNo;
                            existing.DocumentRemarks = doc.DocumentRemarks;
                            existing.DocumentExpDate = doc.DocumentExpDate;
                            existing.DocumentExpDateAr = doc.DocumentExpDateAr;
                            existing.DocumentNotificationDate = doc.DocumentNotificationDate;
                        }
                    }

                    await dbContext.SaveChangesAsync();
                    await _userActionLogger.LogAsync(
                      module: "IMS > Update Document Entries",
                      actionDetail: $":Updated Document Entries  {documents[0].DocumentNo}",
                      documentNo: $"{documents[0].DocumentNo}"
                    );
                    return Ok(new { success = true });
                }

                return Unauthorized(new { success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while fetching the data.", ex });
            }
        }

    }
}

