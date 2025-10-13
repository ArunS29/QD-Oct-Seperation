using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using QD.ERP.Shared.Service;

namespace QD.ERP.Finance.Areas.Finance.Pages
{
    public class AzureAttachmentModel : PageModel
    {
        private readonly ILogger<AzureAttachmentModel> _logger;
        private readonly ClientFilesStorageHelper _blobStorageHelper;

        [BindProperty]
        public IFormFile UploadedFile { get; set; }

        [BindProperty(SupportsGet = true)]
        public string fileName { get; set; }

        private readonly string tenantId = "MyTestTenant";

        public AzureAttachmentModel(ILogger<AzureAttachmentModel> logger, ClientFilesStorageHelper blobStorageHelper)
        {
            _logger = logger;
            _blobStorageHelper = blobStorageHelper;
        }

        public async Task<IActionResult> OnPostUploadAsync([FromForm] IFormFile uploadedFile)
        {
            if (uploadedFile == null)
            {
                _logger.LogWarning("UploadedFile is null.");
                TempData["Error"] = "Please select a file to upload.";
                return Page();
            }

            string uploadReference;
            using (var stream = uploadedFile.OpenReadStream())
            {
                uploadReference = await _blobStorageHelper.UploadFileAsync(stream, tenantId, uploadedFile.FileName);
                // Save the uploadReference in database if needed
            }

            TempData["Message"] = "File uploaded successfully! Reference = " + uploadReference;
            return Page();
        }

        public async Task<IActionResult> OnGetDownloadFileAsync()
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                TempData["Error"] = "File name is required.";
                return Page();
            }

            try
            {
                var stream = await _blobStorageHelper.DownloadFileAsync($"{tenantId}/{fileName}");
                return File(stream, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file");
                TempData["Error"] = "File not found or error occurred.";
                return Page();
            }
        }
    }
}
