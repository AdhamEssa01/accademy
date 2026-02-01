using Academy.Application.Abstractions.Academy;
using Academy.Application.Abstractions.Attendance;
using Academy.Application.Abstractions.Auth;
using Academy.Application.Abstractions.Catalog;
using Academy.Application.Abstractions.Enrollments;
using Academy.Application.Abstractions.Guardians;
using Academy.Application.Abstractions.Parents;
using Academy.Application.Abstractions.Scheduling;
using Academy.Application.Abstractions.Students;
using Academy.Infrastructure.Auth;
using Academy.Infrastructure.Services;
using Academy.Infrastructure.Data;
using Academy.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Academy.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'Default' not found.");
        }

        var provider = config["Database:Provider"];
        if (string.IsNullOrWhiteSpace(provider))
        {
            var environment = config["ASPNETCORE_ENVIRONMENT"];
            provider = string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase)
                ? "SqlServer"
                : "Sqlite";
        }

        services.AddDbContext<AppDbContext>(options =>
        {
            if (string.Equals(provider, "SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlServer(connectionString, b =>
                    b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
                return;
            }

            if (string.Equals(provider, "Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlite(connectionString, b =>
                    b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
                return;
            }

            throw new InvalidOperationException($"Unsupported database provider '{provider}'.");
        });

        services.AddIdentityCore<AppUser>()
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAcademyService, AcademyService>();
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IProgramCatalogService, ProgramCatalogService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IRoutineSlotService, RoutineSlotService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<IStudentPhotoService, StudentPhotoService>();
        services.AddScoped<IGuardianService, GuardianService>();
        services.AddScoped<IParentPortalService, ParentPortalService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IAttendanceQueryService, AttendanceQueryService>();
        services.AddSingleton<IGoogleIdTokenValidator, GoogleIdTokenValidator>();

        return services;
    }
}
