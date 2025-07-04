using DevExpress.Office.Drawing;
using DevExpress.XtraRichEdit.Import.Html;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using SkiaSharp;
using System.Globalization;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class LedgerDocumentsFController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<LedgerDocumentsFController> _logger;
        private readonly IConfiguration _configuration; // ✅ Add this

        public LedgerDocumentsFController(ILogger<LedgerDocumentsFController> logger, TenantDbContextHelper tenantDbContextHelper, IConfiguration configuration)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
            _configuration = configuration;
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
        public async Task<IActionResult> AddDocumentsEntry(string module)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                    return Unauthorized("Invalid tenant context.");

                var tenantName = HttpContext.Session.GetString("TenantName")?.Trim();

                if (string.IsNullOrWhiteSpace(tenantName))
                    return Unauthorized("Tenant name not found in session.");

                var form = await Request.ReadFormAsync();
                var file = form.Files.FirstOrDefault();

                if (file == null || file.Length == 0)
                    return BadRequest(new { success = false, message = "No file uploaded." });

                var referer = Request.Headers["Referer"].ToString();
                Uri? refererUri = !string.IsNullOrWhiteSpace(referer) ? new Uri(referer) : null;

                string area = "UnknownArea";
               // module = "UnknownModule";

                if (refererUri != null)
                {
                    var pathSegments = refererUri.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
                    if (pathSegments.Length >= 3)
                    {
                        // pathSegments[0] = tenant, [1] = area, [2] = module
                        area = pathSegments[1];
                       // module = pathSegments[2];
                    }
                }

                area = area.Replace(" ", "_");
                module = module.Replace(" ", "_");
                tenantName = tenantName?.Replace(" ", "_");

                // Fallback for Razor Pages/controller-based naming
                var routeData = HttpContext.GetRouteData();
                var formName = routeData.Values["page"]?.ToString() ??
                               routeData.Values["controller"]?.ToString() ??
                               "UnknownForm";
                formName = formName.Replace(" ", "_");

                // Construct full file name and blob path
                var fileName = $"{form["DocumentNo"]}_{Path.GetFileName(file.FileName)}";
                var filePathInBlob = $"{area}/{module}/{fileName}";

                var blobHelper = new AzureBlobHelper(
                    _configuration.GetConnectionString("AzureBlobStorage"),
                    "client-files"
                );
                var blobPath = await blobHelper.UploadFileAsync(file, filePathInBlob, tenantName);

                // 🔽 Optional date and Hijri conversion
                DateTime? expDate = DateTime.TryParse(form["DocumentExpDate"], out var d) ? d : null;
                string? hijriDate = null;

                if (expDate.HasValue)
                {
                    HijriCalendar hijri = new HijriCalendar();
                    hijriDate = $"{hijri.GetYear(expDate.Value)}/{hijri.GetMonth(expDate.Value):D2}/{hijri.GetDayOfMonth(expDate.Value):D2}";
                }

                // 🔽 Save to database
                var documentDetails = new Tbl20116LedgerDocument
                {
                    DocumentNo = form["DocumentNo"],
                    DocumentType = short.TryParse(form["DocumentType"], out var docType) ? docType : (short?)null,
                    DocumentRefNo = form["DocumentRefNo"],
                    DocumentRemarks = form["DocumentRemarks"],
                    DocumentExpDate = expDate,
                    DocumentNotificationDate = DateTime.TryParse(form["NotificationDate"], out var nd) ? nd : null,
                    DocumentExpDateAr = hijriDate,
                    AzurePath = blobPath,
                    DocumentStatus = 1,
                    DocumentStatusRemarks = "Active",
                };

                dbContext.Tbl20116LedgerDocuments.Add(documentDetails);
                await dbContext.SaveChangesAsync();

                // 🔽 Filter documents expiring in current month
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;

                var filteredList = await dbContext.Tbl20116LedgerDocuments
                    .Where(p => p.DocumentExpDate.HasValue &&
                                p.DocumentExpDate.Value.Month == currentMonth &&
                                p.DocumentExpDate.Value.Year == currentYear)
                    .Select(i => new
                    {
                        i.DocumentNo,
                        i.DocumentType,
                        i.DocumentRefNo,
                        i.DocumentRemarks,
                        i.DocumentExpDate,
                        i.DocumentExpDateAr,
                        i.DocumentNotificationDate
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Document saved and uploaded successfully.",
                    data = filteredList
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error.",
                    error = ex.Message
                });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetDocuments(string module)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                    return Unauthorized("Invalid tenant context.");

                var tenantName = HttpContext.Session.GetString("TenantName")?.Trim();
                if (string.IsNullOrWhiteSpace(tenantName))
                    return Unauthorized("Tenant name not found in session.");

                if (string.IsNullOrWhiteSpace(module))
                    return BadRequest("Module parameter is required.");

                string area = "UnknownArea";

                var referer = Request.Headers["Referer"].ToString();
                if (!string.IsNullOrWhiteSpace(referer) && Uri.IsWellFormedUriString(referer, UriKind.Absolute))
                {
                    var refererUri = new Uri(referer);
                    var pathSegments = refererUri.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);

                    if (pathSegments.Length >= 3)
                    {
                        area = pathSegments[1];
                    }
                }

                area = area.Replace(" ", "_");
                module = module.Replace(" ", "_");
                tenantName = tenantName.Replace(" ", "_");

                var documents = await dbContext.Tbl20116LedgerDocuments
                    .Where(d => !string.IsNullOrEmpty(d.AzurePath) &&
                                EF.Functions.Like(d.AzurePath, $"%{area}%{module}%"))
                    .Select(d => new
                    {
                        d.DocumentNo,
                        d.DocumentType,
                        d.DocumentRefNo,
                        d.DocumentRemarks,
                        d.DocumentExpDate,
                        d.DocumentExpDateAr,
                        d.DocumentNotificationDate
                    })
                    .ToListAsync();

                return Json(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetDocuments: {ex}");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
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
        [HttpGet]
        public IActionResult GetAllDocuments()
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                return StatusCode(500, "Tenant not found.");

            var connectionString = _configuration.GetConnectionString("AzureBlobStorage");
            var containerName = "client-files"; // or from config if preferred

            if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(containerName))
                return StatusCode(500, "Azure Blob configuration is missing.");

            var blobHelper = new AzureBlobHelper(connectionString, containerName);

            var documents = dbContext.Tbl20116LedgerDocuments
                .Select(d => new
                {
                    d.DocumentNo,
                    d.DocumentType,
                    d.DocumentRefNo,
                    d.DocumentRemarks,
                    d.AzurePath // ✅ Already includes tenant folder
                })
                .ToList();

            var result = documents
                .Where(doc => !string.IsNullOrWhiteSpace(doc.AzurePath))
                .Select(doc => new
                {
                    doc.DocumentNo,
                    doc.DocumentType,
                    doc.DocumentRefNo,
                    doc.DocumentRemarks,
                    FileUrl = blobHelper.GetBlobSasUrl(doc.AzurePath) // ✅ No extraction needed
                });

            return Ok(result);
        }

    }
}

