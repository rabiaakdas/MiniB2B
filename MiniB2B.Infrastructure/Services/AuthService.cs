using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MiniB2B.Application.Common;
using MiniB2B.Application.DTOs.Auth;
using MiniB2B.Application.Interfaces;
using MiniB2B.Application.Validation;
using MiniB2B.Infrastructure.Identity;
using MiniB2B.Infrastructure.Options;

namespace MiniB2B.Infrastructure.Services;

public class AuthService : IAuthService
{
    private const string InvalidCredentialsMessage = "Email veya şifre hatalı.";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        IOptions<JwtOptions> jwtOptions)
    {
        _userManager = userManager;
        _configuration = configuration;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request)
    {
        var validationErrors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.FirstName))
        {
            validationErrors.Add("Ad zorunludur.");
        }
        else if (request.FirstName.Trim().Length > 100)
        {
            validationErrors.Add("Ad en fazla 100 karakter olabilir.");
        }

        if (string.IsNullOrWhiteSpace(request.LastName))
        {
            validationErrors.Add("Soyad zorunludur.");
        }
        else if (request.LastName.Trim().Length > 100)
        {
            validationErrors.Add("Soyad en fazla 100 karakter olabilir.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            validationErrors.Add("Email zorunludur.");
        }
        else if (!EmailValidator.IsValidEmail(request.Email))
        {
            validationErrors.Add(EmailValidator.ErrorMessage);
        }

        if (!PhoneNumberValidator.IsValidOptionalPhoneNumber(request.PhoneNumber))
        {
            validationErrors.Add(PhoneNumberValidator.ErrorMessage);
        }

        if (validationErrors.Count > 0)
        {
            return AuthResult.Failure(400, validationErrors.ToArray());
        }

        var email = request.Email.Trim();
        var existingUser = await _userManager.FindByEmailAsync(email);

        if (existingUser is not null)
        {
            return AuthResult.Failure(400, "Bu email adresi zaten kullanılıyor.");
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim()
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);

        if (!createResult.Succeeded)
        {
            return AuthResult.Failure(400, createResult.Errors.Select(error => error.Description).ToArray());
        }

        var roleResult = await _userManager.AddToRoleAsync(user, Roles.User);

        if (!roleResult.Succeeded)
        {
            return AuthResult.Failure(400, roleResult.Errors.Select(error => error.Description).ToArray());
        }

        return AuthResult.Success(await CreateAuthResponseAsync(user), 201);
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim();
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return AuthResult.Failure(401, InvalidCredentialsMessage);
        }

        if (!user.IsActive)
        {
            return AuthResult.Failure(403, "Kullanıcı hesabı aktif değil.");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);

        if (!isPasswordValid)
        {
            return AuthResult.Failure(401, InvalidCredentialsMessage);
        }

        return AuthResult.Success(await CreateAuthResponseAsync(user));
    }

    private async Task<AuthResponse> CreateAuthResponseAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? Roles.User;
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes);
        var token = CreateJwtToken(user, role, expiresAt);

        return new AuthResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            Role = role
        };
    }

    private string CreateJwtToken(ApplicationUser user, string role, DateTime expiresAt)
    {
        var jwtKey = _configuration["Jwt:Key"];

        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            throw new InvalidOperationException("JWT signing key 'Jwt:Key' was not found.");
        }

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.GivenName, user.FirstName),
            new(ClaimTypes.Surname, user.LastName),
            new(ClaimTypes.Role, role)
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
