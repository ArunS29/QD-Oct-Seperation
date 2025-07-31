using Azure.Storage.Blobs;
using DevExpress.CodeParser;
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

        [HttpGet]
        public async Task<IActionResult> GetNewDocumentNos(int count)
        {
            if (count <= 0 || count > 10)
                return BadRequest(new { message = "Invalid count requested." });

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var documents = await dbContext.Tbl20116LedgerDocuments
                        .Where(x => !string.IsNullOrEmpty(x.DocumentNo))
                        .Select(x => x.DocumentNo)
                        .ToListAsync();

                    var numericNos = documents
                        .Select(docNo => int.TryParse(docNo, out var num) ? num : (int?)null)
                        .Where(num => num.HasValue)
                        .Select(num => num.Value)
                        .ToList();

                    int startNo = numericNos.Any() ? numericNos.Max() + 1 : 1;

                    var newDocNos = Enumerable.Range(startNo, count)
                                              .Select(n => n.ToString())
                                              .ToList();

                    return Ok(newDocNos);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetNewDocumentNos: {ex.Message}");
                    return StatusCode(500, new
                    {
                        message = "An error occurred while generating document numbers.",
                        error = ex.Message
                    });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }



        [HttpPost]
        public async Task<IActionResult> AddDocumentsEntry(string folderId, string moduleType, string isMaster, string menuType)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                    return Unauthorized("Invalid tenant context.");
               
                var tenantName = HttpContext.Session.GetString("TenantName")?.Trim();
                if (string.IsNullOrWhiteSpace(tenantName))
                    return Unauthorized("Tenant name not found in session.");

                var form = await Request.ReadFormAsync();
                var files = form.Files;
                if (files == null || files.Count == 0)
                    return BadRequest(new { success = false, message = "No files uploaded." });

                // Read and split DocumentNos
                var docNosRaw = form["DocumentNo"].ToString(); // e.g., "101,102"
                var docNos = docNosRaw.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
                if (docNos.Count != files.Count)
                    return BadRequest("Document number count does not match file count.");

                // Parse area from referer
                var referer = Request.Headers["Referer"].ToString();
                Uri? refererUri = !string.IsNullOrWhiteSpace(referer) ? new Uri(referer) : null;
                string area = refererUri?.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries).ElementAtOrDefault(1) ?? "UnknownArea";

                string Clean(string val) => string.IsNullOrWhiteSpace(val) ? "unknown" : val.Trim().Replace(" ", "_").ToLower();
                string CleanFolderId(string val) => string.IsNullOrWhiteSpace(val) ? "unknown" : val.Trim().Replace(" ", "_");

                area = Clean(area);
                moduleType = Clean(moduleType);
                folderId = CleanFolderId(folderId);
                tenantName = Clean(tenantName);

                string isMasterNormalized = isMaster?.Trim().ToLower();
                string documentType = (isMasterNormalized == "true" || isMasterNormalized == "master documents")
                    ? "Master Documents"
                    : "Transaction Documents";
                menuType = Clean(menuType);
                // Get voucher date if transaction document
                DateTime? voucherDate = null;
                if (documentType == "Transaction Documents") {

                    if (menuType == "finance") {


                        DateTime? task1 = await dbContext.Tbl201VoucherEntries
                            .Where(v => v.VoucherNo == folderId)
                            .Select(v => (DateTime?)v.AddedOn)
                            .FirstOrDefaultAsync();

                        DateTime? task2 = await dbContext.Tbl20102ExpenseClaimMasters
                            .Where(c => c.ClaimRefNo == folderId)
                            .Select(c => (DateTime?)c.ClaimCreatedOn)
                            .FirstOrDefaultAsync();

                        DateTime? task3 = await dbContext.Tbl20126JournalRegisterMasters
                            .Where(c => c.JournalRefNo == folderId)
                            .Select(c => (DateTime?)c.JournalEntryDate)
                            .FirstOrDefaultAsync();

                        DateTime? task4 = await dbContext.Tbl201ChartOfAccounts
                           .Where(c => c.AccountId == folderId)
                           .Select(c => (DateTime?)c.RecordCreatedOn)
                           .FirstOrDefaultAsync();

                        DateTime? task5 = await dbContext.Tbl20105AssetMasters
                           .Where(c => c.AssetLedgerNo == folderId)
                           .Select(c => (DateTime?)c.AddedOn)
                           .FirstOrDefaultAsync();



                        voucherDate = task1 ?? task2 ?? task3 ?? task4 ?? task5;

                    

                    } else if (menuType == "vat") {


                        DateTime? task1 = await dbContext.Tbl20170VatcreditNoteMasters
                            .Where(v => v.CreditNoteNo == folderId)
                            .Select(v => (DateTime?)v.CreditNoteDate)
                            .FirstOrDefaultAsync();

                        DateTime? task2 = await dbContext.Tbl20161VatinvoiceMasters
                            .Where(c => c.InvoiceNo == folderId)
                            .Select(c => (DateTime?)c.InvoiceDate)
                            .FirstOrDefaultAsync();

                        DateTime? task3 = await dbContext.Tbl20166VatpurchaseMasters
                            .Where(c => c.PurchaseVoucherNo == folderId)
                            .Select(c => (DateTime?)c.PurchaseVoucherDate)
                            .FirstOrDefaultAsync();

                        DateTime? task4 = await dbContext.Tbl20172VatdebitNoteMasters
                            .Where(c => c.DebitNoteNo == folderId)
                            .Select(c => (DateTime?)c.DebitNoteDate)
                            .FirstOrDefaultAsync();

                        DateTime? task5 = await dbContext.Tbl20181ProformaInvoiceMasters
                            .Where(c => c.ProformaInvoiceNo == folderId)
                            .Select(c => (DateTime?)c.ProformaInvoiceDate)
                            .FirstOrDefaultAsync();

                        voucherDate = task1 ?? task2 ?? task3 ?? task4 ?? task5;

                    }
                }



                


                int year = voucherDate?.Year ?? DateTime.Now.Year;
                string month = (voucherDate?.Month ?? DateTime.Now.Month).ToString("00");

                var uploadedDocs = new List<Tbl20116LedgerDocument>();
                var blobHelper = new AzureBlobHelper(_configuration.GetConnectionString("AzureBlobStorage"), "client-files");

                for (int i = 0; i < files.Count; i++)
                {
                    var file = files[i];
                    var docNo = docNos[i];

                    var fileName = $"{docNo}_{Path.GetFileName(file.FileName)}";
                    string blobPath = (documentType == "Transaction Documents")
                        ? $"transaction_documents/{menuType}/year{year}/{month}/{moduleType}/{folderId}/{fileName}"
                        : $"master_documents/{menuType}/{moduleType}/{folderId}/{fileName}";

                    var azurePath = await blobHelper.UploadFileAsync(file, blobPath, tenantName);

                    DateTime today = DateTime.Today;

                    // Try parse DocumentExpDate, or set default (1 month later)
                    DateTime? expDate = DateTime.TryParse(form["DocumentExpDate"], out var d)
                        ? d
                        : today.AddMonths(1);

                    // Notification date: Try parse or fallback to 7 days before expiry
                    DateTime? notificationDate = DateTime.TryParse(form["NotificationDate"], out var nd)
                        ? nd
                        : expDate?.AddDays(-7);

                    // Hijri date (converted from expDate)
                    string? hijriDate = null;
                    if (expDate.HasValue)
                    {
                        var hijri = new HijriCalendar();
                        hijriDate = $"{hijri.GetYear(expDate.Value)}/{hijri.GetMonth(expDate.Value):D2}/{hijri.GetDayOfMonth(expDate.Value):D2}";
                    }
                    var addedBy = HttpContext.Session.GetString("UserName") ?? "System";
                    // Create document object
                    var document = new Tbl20116LedgerDocument
                    {

                        DocumentNo = docNo,
                        LedgerNo = form["LedgerNo"],
                        DocumentType = short.TryParse(form["DocumentType"], out var docType) ? docType : (short?)null,
                        DocumentRefNo = form["DocumentRefNo"],
                        DocumentRemarks = form["DocumentRemarks"],
                        DocumentExpDate = expDate,
                        DocumentNotificationDate = notificationDate,
                        DocumentExpDateAr = hijriDate,
                        AzurePath = azurePath,
                        DocumentStatus = 1,
                        DocumentStatusRemarks = "Active",
                        AddedBy = addedBy,
                        AddedOn = DateTime.UtcNow
                    };

                    dbContext.Tbl20116LedgerDocuments.Add(document);
                    uploadedDocs.Add(document);
                }

                await dbContext.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = $"{uploadedDocs.Count} document(s) uploaded successfully.",
                    uploaded = uploadedDocs.Select(d => new
                    {
                        d.DocumentNo,
                        d.DocumentRefNo,
                        d.DocumentType,
                        d.DocumentExpDate,
                        d.AzurePath
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddDocumentsEntry");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
            public async Task<IActionResult> GetDocuments(string folderId, string module, string menuType)
            {
                try
                {
                    if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                        return Unauthorized("Invalid tenant context.");

                    var tenantName = HttpContext.Session.GetString("TenantName")?.Trim();
                    if (string.IsNullOrWhiteSpace(tenantName))
                        return Unauthorized("Tenant name not found in session.");

                    if (string.IsNullOrWhiteSpace(module) || string.IsNullOrWhiteSpace(folderId))
                        return BadRequest("Both module and folderId are required.");

                    // Azure Blob configuration
                    var connectionString = _configuration.GetConnectionString("AzureBlobStorage");
                    var containerName = "client-files";
                    if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(containerName))
                        return StatusCode(500, "Azure Blob configuration is missing.");

                    var blobHelper = new AzureBlobHelper(connectionString, containerName);

                    // Normalize paths
                    string Clean(string val) => string.IsNullOrWhiteSpace(val) ? "" : val.Replace(" ", "_").Trim().ToLower();
                    var normalizedModule = Clean(module);
                    var normalizedFolderId = Clean(folderId);
                    var normalizedTenant = Clean(tenantName);
                    var normalizedMenu = Clean(menuType);

                    // Build partial Azure path prefix
                    var azurePathPrefix = $"{normalizedTenant}/"; // Tenant root

                    // Optional: further narrow down to expected subfolder
                    // e.g., transaction_documents/module/folderId
                    // You can append further filtering here if your structure is consistent

                    var allBlobsInTenant = await blobHelper.ListBlobsAsync(azurePathPrefix);

                    var dbDocs = await dbContext.Tbl20116LedgerDocuments
                        .Where(d => !string.IsNullOrEmpty(d.AzurePath) &&
                                    d.AzurePath.Contains(normalizedMenu) &&
                                    d.AzurePath.Contains(normalizedModule) &&
                                    d.AzurePath.Contains(normalizedFolderId))
                        .ToListAsync();

                    var matchingDocs = dbDocs
                        .Where(d => allBlobsInTenant.Contains(d.AzurePath))
                        .Select((d, index) => new
                        {
                            SerialNo = index + 1,
                            d.DocumentNo,
                            d.DocumentType,
                            d.DocumentRefNo,
                            d.DocumentRemarks,
                            d.DocumentExpDate,
                            d.DocumentExpDateAr,
                            d.DocumentNotificationDate,
                            d.DocumentStatus,
                            d.DocumentStatusRemarks,
                            FileUrl = blobHelper.GetBlobSasUrl(d.AzurePath)
                        });

                    return Ok(matchingDocs);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetDocuments: {ex}");
                    return StatusCode(500, $"An error occurred: {ex.Message}");
                }
            }

        [HttpPut]
        public async Task<IActionResult> UpdateDocumentEntry([FromBody] Tbl20116LedgerDocument doc)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                    return Unauthorized("Invalid tenant context.");
               
                var existing = await dbContext.Tbl20116LedgerDocuments
                    .FirstOrDefaultAsync(d => d.DocumentNo == doc.DocumentNo);
                var addedBy = HttpContext.Session.GetString("UserName") ?? "System";
                if (existing == null)
                    return NotFound($"Document with DocumentNo {doc.DocumentNo} not found.");

                // Update
                existing.DocumentType = doc.DocumentType;
                existing.DocumentRefNo = doc.DocumentRefNo;
                existing.DocumentRemarks = doc.DocumentRemarks;
                existing.DocumentExpDate = doc.DocumentExpDate;
                existing.DocumentExpDateAr = doc.DocumentExpDateAr;
                existing.DocumentNotificationDate = doc.DocumentNotificationDate;
                existing.ModifiedBy = addedBy;
                existing.ModifiedOn = DateTime.UtcNow;

                await dbContext.SaveChangesAsync();

                // ✅ Optional: Return file URL from AzurePath
                var blobHelper = new AzureBlobHelper(
                    _configuration.GetConnectionString("AzureBlobStorage"),
                    "client-files"
                );
                string fileUrl = blobHelper.GetBlobSasUrl(existing.AzurePath);

                return Ok(new
                {
                    success = true,
                    message = "Document updated successfully.",
                    fileUrl
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateDocumentEntry");
                return StatusCode(500, new { success = false, message = "Error updating document", ex.Message });
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

       
        [HttpDelete]
        public async Task<IActionResult> DeleteDocumentEntry([FromBody] DocumentDeleteRequest request)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                return Unauthorized("Invalid tenant context.");

            var doc = await dbContext.Tbl20116LedgerDocuments
                        .FirstOrDefaultAsync(d => d.DocumentNo == request.key);

            if (doc == null)
                return NotFound("Document not found.");

            // Delete file from Azure Blob if it exists
            if (!string.IsNullOrEmpty(doc.AzurePath))
            {
                var blobHelper = new AzureBlobHelper(_configuration.GetConnectionString("AzureBlobStorage"), "client-files");
                await blobHelper.DeleteFileFromAzureAsync(doc.AzurePath); // <-- AzurePath is blobPath
            }



            dbContext.Tbl20116LedgerDocuments.Remove(doc);
            await dbContext.SaveChangesAsync();

            return Ok();
        }

        public class DocumentDeleteRequest
        {
            public string key { get; set; }
        }

        // Attachment Count 
        [HttpGet]
        public async Task<IActionResult> GetDocumentCount(string folderId, string module)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                    return Unauthorized("Invalid tenant context.");

                var tenantName = HttpContext.Session.GetString("TenantName")?.Trim();
                if (string.IsNullOrWhiteSpace(tenantName))
                    return Unauthorized("Tenant name not found in session.");

                if (string.IsNullOrWhiteSpace(module) || string.IsNullOrWhiteSpace(folderId))
                    return BadRequest("Both module and folderId are required.");

                var normalizedModule = module.Replace(" ", "_").Trim();
                var normalizedFolderId = folderId.Trim();

                var count = await dbContext.Tbl20116LedgerDocuments
                    .Where(d => !string.IsNullOrEmpty(d.AzurePath) &&
                                d.AzurePath.Contains(normalizedModule) &&
                                d.AzurePath.Contains(normalizedFolderId))
                    .CountAsync();

                return Ok(count); // <-- Just return the count
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetDocumentCount: {ex}");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [HttpGet]
        public ActionResult LedgerDocuments(string ReferenceNo, string ModuleType, string isMaster)
        {
            ViewBag.ReferenceNo = ReferenceNo;
            ViewBag.ModuleType = ModuleType;
            ViewBag.IsMaster = isMaster;

            return PartialView("~/Areas/Finance/Pages/LedgerDocuments.cshtml");
        }

    }
}

