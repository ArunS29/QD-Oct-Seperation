using DevExpress.XtraRichEdit.Fields;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using QD.ERP.Shared.DAL.Entities;
using QD.ERP.Shared.Service;


namespace QD.ERP.Finance.Areas.Finance.Pages
{
    public class VoucherMasterModel : PageModel
    {
     
			private readonly ILogger<VoucherMasterModel> _logger;
			private readonly ClientFilesStorageHelper _blobStorageHelper;
			[BindProperty]
			public IFormFile UploadedFile { get; set; }
			[BindProperty(SupportsGet = true)]
			public string fileName { get; set; }

			string tenantId = "MyTestTenant";

			public VoucherMasterModel(ILogger<IndexModel> logger, ClientFilesStorageHelper blobStorageHelper)
			{
				_logger = (ILogger<VoucherMasterModel>)logger;
				_blobStorageHelper = blobStorageHelper;
			}

			public async Task<IActionResult> OnPostUploadAsync()
			{

				string uploadReference = string.Empty;
				if (UploadedFile != null)
				{
					// Upload the file to Azure Blob Storage
					using (var stream = UploadedFile.OpenReadStream())
					{
						uploadReference = await _blobStorageHelper.UploadFileAsync(stream, tenantId, UploadedFile.FileName);
						// Database logic here: save the uploadReference in your preferred database
					}

					TempData["Message"] = "File uploaded successfully!. Reference = " + uploadReference;
				}

				return Page();
			}

			public async Task<IActionResult> OnGetDownloadFileAsync()
			{
				// Download the file from Azure Blob Storage
				var stream = await _blobStorageHelper.DownloadFileAsync($"{tenantId}/{fileName}");
				return File(stream, "application/octet-stream", fileName);
			}

		
	}
}

