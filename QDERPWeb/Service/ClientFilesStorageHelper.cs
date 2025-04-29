using Azure.Identity;
using Azure.Storage.Blobs;

namespace qd.utilities
{
    public class ClientFilesStorageHelper
    {
        private readonly string _connectionString;
        private readonly string _containerName;
        private readonly string _containerUri;

        public ClientFilesStorageHelper(string containerUri, string connectionString, string containerName)
        {
            _containerUri = containerUri;
            _connectionString = connectionString;
            _containerName = containerName;
        }

        private BlobContainerClient CreateBlobContainerClientWithConnectionString()
        {
            return new BlobContainerClient(_connectionString, _containerName);
        }

        private BlobContainerClient CreateBlobContainerClientWithManagedIdentity()
        {
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
            if(!string.IsNullOrEmpty(_connectionString))
            {
                return CreateBlobContainerClientWithConnectionString();
            }
            return CreateBlobContainerClientWithManagedIdentity();
        }

        /// <summary>
        /// Uploads a file to Azure Blob Storage.
        /// </summary>
        /// <param name="fileStream">File stream of the file to be uploaded.</param>
        /// <param name="fileName">Name of the file to be uploaded.</param>
        /// <returns>URL of the uploaded blob.</returns>
        public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
        {
            var blobClient = CreateBlobContainerClient();
            await blobClient.CreateIfNotExistsAsync();

            var blob = blobClient.GetBlobClient(fileName);
            await blob.UploadAsync(fileStream, true);

            return blob.Uri.ToString(); // Returns the blob URL.
        }

        /// <summary>
        /// Uploads a file to Azure Blob Storage.
        /// </summary>
        /// <param name="fileStream">File stream of the file to be uploaded.</param>
        /// <param name="fileName">Name of the file to be uploaded.</param>
        /// <returns>URL of the uploaded blob.</returns>
        public async Task<string> UploadFileAsync(Stream fileStream, string folderName, string fileName)
        {
            var blobClient = CreateBlobContainerClient();
            await blobClient.CreateIfNotExistsAsync();

            string blobName = $"{folderName}/{fileName}";

            var blob = blobClient.GetBlobClient(blobName);
            await blob.UploadAsync(fileStream, true);

            return blob.Uri.ToString(); // Returns the blob URL.
        }

        /// <summary>
        /// Downloads a file from Azure Blob Storage.
        /// </summary>
        /// <param name="fileName">Name of the file to download.</param>
        /// <returns>Stream of the downloaded file.</returns>
        public async Task<Stream> DownloadFileAsync(string fileName)
        {
            var blobClient = CreateBlobContainerClientWithConnectionString();
            var blob = blobClient.GetBlobClient(fileName);

            if (await blob.ExistsAsync())
            {
                return await blob.OpenReadAsync();
            }

            throw new FileNotFoundException("File not found in Blob Storage.");
        }
    }

}
