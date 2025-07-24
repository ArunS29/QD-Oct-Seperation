using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class AzureBlobHelper
{
    private readonly string _connectionString;
    private readonly string _containerName;
    
    public AzureBlobHelper(string connectionString, string containerName)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentNullException(nameof(connectionString), "Azure Blob connection string cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(containerName))
            throw new ArgumentNullException(nameof(containerName), "Blob container name cannot be null or empty.");

        _connectionString = connectionString;
        _containerName = containerName;
    }

    // 🔽 Sanitize tenant name (no slashes, spaces, or special chars)
    private string SanitizeTenantName(string tenantName)
    {
        return Regex.Replace(tenantName.Trim(), @"[^a-zA-Z0-9\-]", "_");
    }

    public async Task<string> UploadFileAsync(IFormFile file, string fileName, string tenantName)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Invalid file.");

        const long maxFileSizeInBytes = 5 * 1024 * 1024; // 5 MB

        if (file.Length > maxFileSizeInBytes)
            throw new InvalidOperationException("File size exceeds 5 MB. Please upload a file smaller than 5 MB.");

        var blobServiceClient = new BlobServiceClient(_connectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

        var sanitizedTenant = SanitizeTenantName(tenantName);
        //var sanitizedUser = SanitizeTenantName(userName); // reuse same sanitizer
        var blobPath = $"{sanitizedTenant}/{fileName}";

        var blobClient = containerClient.GetBlobClient(blobPath);

        using (var stream = file.OpenReadStream())
        {
            await blobClient.UploadAsync(stream, overwrite: true);
        }

        return blobPath; // Store only relative path in DB
    }



    public string GetBlobSasUrl(string blobPath, int validMinutes = 30)
    {
        if (string.IsNullOrWhiteSpace(blobPath))
            throw new ArgumentNullException(nameof(blobPath), "Blob path cannot be null or empty.");

        var blobServiceClient = new BlobServiceClient(_connectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(blobPath);

        if (!blobClient.CanGenerateSasUri)
        {
            throw new InvalidOperationException("SAS URI generation not supported. Ensure you're using a connection string with account key.");
        }

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _containerName,
            BlobName = blobPath,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(validMinutes)
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        return blobClient.GenerateSasUri(sasBuilder).ToString();
    }

    public async Task<bool> DeleteFileFromAzureAsync(string blobPath)
    {
        try
        {
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobPath);

            var result = await blobClient.DeleteIfExistsAsync();
            return result.Value;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Azure deletion failed: " + ex.Message);
            return false;
        }
    }

    // Get Blobs under folder 
    public async Task<List<string>> ListBlobsAsync(string prefix)
    {
        var result = new List<string>();
        var containerClient = new BlobContainerClient(_connectionString, _containerName);

        await foreach (var blob in containerClient.GetBlobsAsync(prefix: prefix))
        {
            result.Add(blob.Name); // only relative path
        }

        return result;
    }



}
