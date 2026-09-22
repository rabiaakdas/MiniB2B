namespace MiniB2B.Api.Services;

public class ImageStorageResult
{
    public bool Succeeded { get; set; }
    public string? RelativePath { get; set; }
    public string[] Errors { get; set; } = [];

    public static ImageStorageResult Success(string relativePath)
    {
        return new ImageStorageResult
        {
            Succeeded = true,
            RelativePath = relativePath
        };
    }

    public static ImageStorageResult Failure(params string[] errors)
    {
        return new ImageStorageResult
        {
            Succeeded = false,
            Errors = errors
        };
    }
}
