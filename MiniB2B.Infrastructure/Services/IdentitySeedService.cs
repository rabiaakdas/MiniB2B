using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MiniB2B.Application.Common;
using MiniB2B.Infrastructure.Identity;
using MiniB2B.Infrastructure.Options;

namespace MiniB2B.Infrastructure.Services;

public class IdentitySeedService
{
    private readonly RoleManager<IdentityRole<int>> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly DevelopmentAdminOptions _developmentAdminOptions;

    public IdentitySeedService(
        RoleManager<IdentityRole<int>> roleManager,
        UserManager<ApplicationUser> userManager,
        IOptions<DevelopmentAdminOptions> developmentAdminOptions)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _developmentAdminOptions = developmentAdminOptions.Value;
    }

    public async Task SeedAsync(bool seedDevelopmentAdmin)
    {
        await EnsureRoleAsync(Roles.Admin);
        await EnsureRoleAsync(Roles.User);

        if (seedDevelopmentAdmin)
        {
            await EnsureDevelopmentAdminAsync();
        }
    }

    private async Task EnsureRoleAsync(string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            await _roleManager.CreateAsync(new IdentityRole<int>(roleName));
        }
    }

    private async Task EnsureDevelopmentAdminAsync()
    {
        if (string.IsNullOrWhiteSpace(_developmentAdminOptions.Email)
            || string.IsNullOrWhiteSpace(_developmentAdminOptions.Password))
        {
            return;
        }

        var email = _developmentAdminOptions.Email.Trim();
        var admin = await _userManager.FindByEmailAsync(email);

        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = string.IsNullOrWhiteSpace(_developmentAdminOptions.FirstName)
                    ? "Development"
                    : _developmentAdminOptions.FirstName.Trim(),
                LastName = string.IsNullOrWhiteSpace(_developmentAdminOptions.LastName)
                    ? "Admin"
                    : _developmentAdminOptions.LastName.Trim(),
                IsActive = true
            };

            var createResult = await _userManager.CreateAsync(admin, _developmentAdminOptions.Password);

            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException("Development admin user could not be created.");
            }
        }

        if (!await _userManager.IsInRoleAsync(admin, Roles.Admin))
        {
            await _userManager.AddToRoleAsync(admin, Roles.Admin);
        }
    }
}
