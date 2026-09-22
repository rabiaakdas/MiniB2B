namespace MiniB2B.Application.DTOs.Common;

public class ServiceResult<T>
{
    public bool Succeeded { get; set; }
    public int StatusCode { get; set; }
    public string[] Errors { get; set; } = [];
    public T? Data { get; set; }

    public static ServiceResult<T> Success(T data, int statusCode = 200)
    {
        return new ServiceResult<T>
        {
            Succeeded = true,
            StatusCode = statusCode,
            Data = data
        };
    }

    public static ServiceResult<T> Failure(int statusCode, params string[] errors)
    {
        return new ServiceResult<T>
        {
            Succeeded = false,
            StatusCode = statusCode,
            Errors = errors
        };
    }
}
