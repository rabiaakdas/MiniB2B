namespace MiniB2B.Application.DTOs.Auth;

public class AuthResult
{
    public bool Succeeded { get; set; }
    public int StatusCode { get; set; }
    public string[] Errors { get; set; } = [];
    public AuthResponse? Data { get; set; }

    public static AuthResult Success(AuthResponse response, int statusCode = 200)
    {
        return new AuthResult
        {
            Succeeded = true,
            StatusCode = statusCode,
            Data = response
        };
    }

    public static AuthResult Failure(int statusCode, params string[] errors)
    {
        return new AuthResult
        {
            Succeeded = false,
            StatusCode = statusCode,
            Errors = errors
        };
    }
}
