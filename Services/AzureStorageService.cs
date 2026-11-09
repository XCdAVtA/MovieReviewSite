using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace MoviesAdmin.Services;

public class AzureStorageService : IAzureStorageService
{
    private readonly BlobContainerClient _container;

    public AzureStorageService(IConfiguration configuration)
    {
        var connectionString = configuration["AzureStorage:ConnectionString"] 
                               ?? throw new InvalidOperationException("Missing configuration: AzureStorage:ConnectionString");
        var containerName = configuration["AzureStorage:ContainerName"]
                               ?? throw new InvalidOperationException("Missing configuration: AzureStorage:ContainerName");

        _container = new BlobServiceClient(connectionString).GetBlobContainerClient(containerName);
    }

    // return the URL of the uploaded file
    public async Task<string> UploadFileAsync(IFormFile file, string filename)
    {
        // public read access so <img src> works in the browser
        await _container.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var blob = _container.GetBlobClient(filename);
        var options = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType }
        };

        using var stream = file.OpenReadStream();
        await blob.UploadAsync(stream, options);
        return blob.Uri.ToString();
    }

    public async Task DeleteFileAsync(string fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl)) return;
        var filename = Path.GetFileName(new Uri(fileUrl).AbsolutePath);
        await _container.DeleteBlobIfExistsAsync(filename);
    }
}