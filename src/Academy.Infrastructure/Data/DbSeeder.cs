using Academy.Domain;
using Academy.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Academy.Infrastructure.Data;

public static class DbSeeder
{
    private static readonly string[] Roles =
    {
        Academy.Shared.Security.Roles.Admin,
        Academy.Shared.Security.Roles.Instructor,
        Academy.Shared.Security.Roles.Parent,
        Academy.Shared.Security.Roles.Student
    };

    public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken ct = default)
    {
        var dbContext = serviceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync(ct);

        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to create role '{role}': {errors}");
                }
            }
        }

        var academy = await dbContext.Academies.FirstOrDefaultAsync(ct);
        if (academy is null)
        {
            academy = new Academy.Domain.Academy
            {
                Id = Guid.NewGuid(),
                Name = "Default Academy",
                CreatedAtUtc = DateTime.UtcNow
            };

            dbContext.Academies.Add(academy);
            await dbContext.SaveChangesAsync(ct);
        }

        var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
        const string adminEmail = "admin@local.test";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(adminUser, "Admin123$");
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create admin user: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(adminUser, Academy.Shared.Security.Roles.Admin))
        {
            var roleResult = await userManager.AddToRoleAsync(adminUser, Academy.Shared.Security.Roles.Admin);
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to assign admin role: {errors}");
            }
        }

        var profileExists = await dbContext.UserProfiles
            .AnyAsync(p => p.UserId == adminUser.Id, ct);

        if (!profileExists)
        {
            var profile = new UserProfile
            {
                Id = Guid.NewGuid(),
                UserId = adminUser.Id,
                AcademyId = academy.Id,
                DisplayName = "Admin",
                CreatedAtUtc = DateTime.UtcNow
            };

            dbContext.UserProfiles.Add(profile);
            await dbContext.SaveChangesAsync(ct);
        }
    }
}
