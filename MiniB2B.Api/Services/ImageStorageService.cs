namespace MiniB2B.Api.Services;

public class ImageStorageService : IImageStorageService
{
    private const long MaxImageSizeInBytes = 5 * 1024 * 1024;

    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    private static readonly HashSet<string> AllowedImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    private readonly IWebHostEnvironment _environment;

    public ImageStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<ImageStorageResult> SaveImageAsync(IFormFile file, string folderName)
    {
        var validationErrors = ValidateImage(file);

        if (validationErrors.Count > 0)
        {
            return ImageStorageResult.Failure(validationErrors.ToArray());
        }

        var uploadsRoot = GetUploadsRoot(folderName);
        Directory.CreateDirectory(uploadsRoot);

        var extension = Path.GetExtension(file.FileName);
        var generatedFileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var absoluteFilePath = Path.GetFullPath(Path.Combine(uploadsRoot, generatedFileName));

        if (!absoluteFilePath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase))
        {
            return ImageStorageResult.Failure("Geçersiz dosya yolu.");
        }

        await using (var stream = File.Create(absoluteFilePath))
        {
            await file.CopyToAsync(stream);
        }

        return ImageStorageResult.Success($"/uploads/{folderName}/{generatedFileName}");
    }

    public void DeleteIfSafe(string? relativePath, string folderName)
    {
        if (string.IsNullOrWhiteSpace(relativePath)
            || !relativePath.StartsWith($"/uploads/{folderName}/", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var fileName = Path.GetFileName(relativePath);

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return;
        }

        var uploadsRoot = GetUploadsRoot(folderName);
        var absolutePath = Path.GetFullPath(Path.Combine(uploadsRoot, fileName));

        if (!absolutePath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase)
            || !File.Exists(absolutePath))
        {
            return;
        }

        File.Delete(absolutePath);
    }

    private static List<string> ValidateImage(IFormFile file)
    {
        var errors = new List<string>();
        var extension = Path.GetExtension(file.FileName);

        if (file.Length <= 0)
        {
            errors.Add("Dosya boş olamaz.");
        }

        if (file.Length > MaxImageSizeInBytes)
        {
            errors.Add("Dosya boyutu en fazla 5 MB olabilir.");
        }

        if (string.IsNullOrWhiteSpace(extension) || !AllowedImageExtensions.Contains(extension))
        {
            errors.Add("Sadece jpg, jpeg, png veya webp dosyaları yüklenebilir.");
        }

        if (!AllowedImageContentTypes.Contains(file.ContentType))
        {
            errors.Add("Geçersiz image content type.");
        }

        return errors;
    }

    private string GetUploadsRoot(string folderName)
    {
        var webRootPath = _environment.WebRootPath;

        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
        }

        return Path.GetFullPath(Path.Combine(webRootPath, "uploads", folderName));
    }
}
