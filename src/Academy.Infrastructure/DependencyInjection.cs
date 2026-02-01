using Academy.Application.Abstractions.Academy;
using Academy.Application.Abstractions.Announcements;
using Academy.Application.Abstractions.Attendance;
using Academy.Application.Abstractions.Auth;
using Academy.Application.Abstractions.Assignments;
using Academy.Application.Abstractions.Behavior;
using Academy.Application.Abstractions.Catalog;
using Academy.Application.Abstractions.Exams;
using Academy.Application.Abstractions.Evaluations;
using Academy.Application.Abstractions.Enrollments;
using Academy.Application.Abstractions.Guardians;
using Academy.Application.Abstractions.Notifications;
using Academy.Application.Abstractions.Parents;
using Academy.Application.Abstractions.Questions;
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

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(connectionString, b =>
                b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
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
        services.AddScoped<IStudentRiskService, StudentRiskService>();
        services.AddScoped<IStudentPhotoService, StudentPhotoService>();
        services.AddScoped<IGuardianService, GuardianService>();
        services.AddScoped<IParentPortalService, ParentPortalService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IAttendanceQueryService, AttendanceQueryService>();
        services.AddScoped<IBehaviorService, BehaviorService>();
        services.AddScoped<IAssignmentService, AssignmentService>();
        services.AddScoped<IAssignmentAttachmentService, AssignmentAttachmentService>();
        services.AddScoped<IAnnouncementService, AnnouncementService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IEvaluationTemplateService, EvaluationTemplateService>();
        services.AddScoped<IEvaluationService, EvaluationService>();
        services.AddScoped<IQuestionBankService, QuestionBankService>();
        services.AddScoped<IExamService, ExamService>();
        services.AddScoped<IExamAssignmentService, ExamAssignmentService>();
        services.AddScoped<IExamAttemptService, ExamAttemptService>();
        services.AddScoped<IExamGradingService, ExamGradingService>();
        services.AddScoped<IExamManualGradingService, ExamManualGradingService>();
        services.AddScoped<IExamAnalyticsService, ExamAnalyticsService>();
        services.AddSingleton<IGoogleIdTokenValidator, GoogleIdTokenValidator>();

        return services;
    }
}
