namespace MoviesAdmin.Services;

public interface IAzureStorageService
{
    Task<string> UploadFileAsync(IFormFile file, string filename);
    Task DeleteFileAsync(string fileUrl);
}