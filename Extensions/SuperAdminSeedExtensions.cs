using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Models;
using Microsoft.AspNetCore.Identity;

namespace CampusPlacementManagement.Extensions;

public static class SuperAdminSeedExtensions
{
    public static async Task SeedSuperAdminAsync(
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        using var scope = serviceProvider.CreateScope();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        string email = configuration["InitialSuperAdmin:Email"]
            ?? throw new InvalidOperationException(
                "Initial SuperAdmin email is not configured.");

        string password = configuration["InitialSuperAdmin:Password"]
            ?? throw new InvalidOperationException(
                "Initial SuperAdmin password is not configured.");

        string fullName = configuration["InitialSuperAdmin:FullName"]
            ?? "System Administrator";

        var user = await userManager.FindByEmailAsync(email);

        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                EmailConfirmed = true,
                AccountStatus = AccountStatus.Approved,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(" ", result.Errors.Select(e => e.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(user, "SuperAdmin"))
        {
            var roleResult = await userManager.AddToRoleAsync(
                user, "SuperAdmin");

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(" ", roleResult.Errors.Select(e => e.Description)));
            }
        }
    }
}