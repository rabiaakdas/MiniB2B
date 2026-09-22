namespace MiniB2B.Api.Services;

public interface IImageStorageService
{
    Task<ImageStorageResult> SaveImageAsync(IFormFile file, string folderName);
    void DeleteIfSafe(string? relativePath, string folderName);
}
