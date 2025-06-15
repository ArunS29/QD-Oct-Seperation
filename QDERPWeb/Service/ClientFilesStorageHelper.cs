using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;

namespace QD.ERP.Web.Service
{
    public class ClientFilesStorageHelper
    {
        private readonly string _connectionString;
        private readonly string _containerName;
        private readonly string _containerUri;
        private readonly ILogger<ClientFilesStorageHelper> _logger;

        public ClientFilesStorageHelper(string containerUri, string connectionString, string containerName, ILogger<ClientFilesStorageHelper> logger)
        {
            _containerUri = containerUri;
            _connectionString = connectionString;
            _containerName = containerName;
            _logger = logger;
        }

        private BlobContainerClient CreateBlobContainerClientWithConnectionString()
        {
            return new BlobContainerClient(_connectionString, _containerName);
        }

        private BlobContainerClient CreateBlobContainerClientWithManagedIdentity()
        {
            if (string.IsNullOrWhiteSpace(_containerUri))
            {
                _logger?.LogError("Container URI is null or empty while using Managed Identity.");
                throw new ArgumentNullException(nameof(_containerUri), "Container URI must be provided when using Managed Identity.");
            }

            var credential = new ChainedTokenCredential(
                new DefaultAzureCredential(),
                new VisualStudioCredential(),
                new ManagedIdentityCredential()
            );

            var blobServiceClient = new BlobServiceClient(new Uri(_containerUri), credential);
            return blobServiceClient.GetBlobContainerClient(_containerName);
        }


        private BlobContainerClient CreateBlobContainerClient()
        {
            if (!string.IsNullOrWhiteSpace(_connectionString))
            {
                return CreateBlobContainerClientWithConnectionString();
            }

            return CreateBlobContainerClientWithManagedIdentity();
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string folderName, string fileName)
        {
            var blobClient = CreateBlobContainerClient();
            await blobClient.CreateIfNotExistsAsync();

            string blobName = $"{folderName}/{fileName}";
            var blob = blobClient.GetBlobClient(blobName);
            await blob.UploadAsync(fileStream, overwrite: true);

            return blob.Uri.ToString();
        }

        public async Task<Stream> DownloadFileAsync(string blobPath)
        {
            var blobClient = CreateBlobContainerClient();
            var blob = blobClient.GetBlobClient(blobPath);

            if (await blob.ExistsAsync())
            {
                return await blob.OpenReadAsync();
            }

            throw new FileNotFoundException("File not found in Blob Storage: " + blobPath);
        }
    }
}
