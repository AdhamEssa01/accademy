using Academy.Application.Abstractions.Security;
using Academy.Domain;
using Academy.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Academy.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
{
    private readonly Guid? _currentAcademyId;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserContext? currentUserContext = null)
        : base(options)
    {
        _currentAcademyId = currentUserContext?.IsAuthenticated == true
            ? currentUserContext.AcademyId
            : null;
    }

    public DbSet<Academy.Domain.Academy> Academies => Set<Academy.Domain.Academy>();

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Branch> Branches => Set<Branch>();

    public DbSet<Academy.Domain.Program> Programs => Set<Academy.Domain.Program>();

    public DbSet<Course> Courses => Set<Course>();

    public DbSet<Level> Levels => Set<Level>();

    public DbSet<Group> Groups => Set<Group>();

    public DbSet<Session> Sessions => Set<Session>();

    public DbSet<RoutineSlot> RoutineSlots => Set<RoutineSlot>();

    public DbSet<Student> Students => Set<Student>();

    public DbSet<Guardian> Guardians => Set<Guardian>();

    public DbSet<StudentGuardian> StudentGuardians => Set<StudentGuardian>();

    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();

    public DbSet<Assignment> Assignments => Set<Assignment>();

    public DbSet<AssignmentAttachment> AssignmentAttachments => Set<AssignmentAttachment>();

    public DbSet<AssignmentTarget> AssignmentTargets => Set<AssignmentTarget>();

    public DbSet<Announcement> Announcements => Set<Announcement>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<EvaluationTemplate> EvaluationTemplates => Set<EvaluationTemplate>();

    public DbSet<RubricCriterion> RubricCriteria => Set<RubricCriterion>();

    public DbSet<Evaluation> Evaluations => Set<Evaluation>();

    public DbSet<EvaluationItem> EvaluationItems => Set<EvaluationItem>();

    public DbSet<BehaviorEvent> BehaviorEvents => Set<BehaviorEvent>();

    public DbSet<Question> Questions => Set<Question>();

    public DbSet<QuestionOption> QuestionOptions => Set<QuestionOption>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Academy.Domain.Academy>(entity =>
        {
            entity.Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(200);
            entity.Property(a => a.CreatedAtUtc)
                .IsRequired();
        });

        builder.Entity<UserProfile>(entity =>
        {
            entity.Property(p => p.DisplayName)
                .IsRequired()
                .HasMaxLength(150);
            entity.Property(p => p.CreatedAtUtc)
                .IsRequired();

            entity.HasIndex(p => p.UserId)
                .IsUnique();

            entity.HasOne<Academy.Domain.Academy>()
                .WithMany()
                .HasForeignKey(p => p.AcademyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<RefreshToken>(entity =>
        {
            entity.Property(r => r.TokenHash)
                .IsRequired()
                .HasMaxLength(128);

            entity.Property(r => r.ReplacedByTokenHash)
                .HasMaxLength(128);

            entity.Property(r => r.CreatedByIp)
                .HasMaxLength(64);

            entity.Property(r => r.RevokedByIp)
                .HasMaxLength(64);

            entity.HasIndex(r => r.TokenHash)
                .IsUnique();

            entity.HasIndex(r => r.UserId);

            entity.HasOne<AppUser>()
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Branch>(entity =>
        {
            entity.Property(b => b.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(b => b.Address)
                .HasMaxLength(400);

            entity.Property(b => b.CreatedAtUtc)
                .IsRequired();

            entity.HasIndex(b => new { b.AcademyId, b.Name })
                .IsUnique();
        });

        builder.Entity<Academy.Domain.Program>(entity =>
        {
            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(p => p.Description)
                .HasMaxLength(800);

            entity.Property(p => p.CreatedAtUtc)
                .IsRequired();

            entity.HasIndex(p => new { p.AcademyId, p.Name })
                .IsUnique();
        });

        builder.Entity<Course>(entity =>
        {
            entity.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(c => c.Description)
                .HasMaxLength(800);

            entity.Property(c => c.CreatedAtUtc)
                .IsRequired();

            entity.HasIndex(c => new { c.AcademyId, c.ProgramId, c.Name })
                .IsUnique();

            entity.HasOne<Academy.Domain.Program>()
                .WithMany()
                .HasForeignKey(c => c.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Level>(entity =>
        {
            entity.Property(l => l.Name)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(l => l.CreatedAtUtc)
                .IsRequired();

            entity.HasIndex(l => new { l.AcademyId, l.CourseId, l.Name })
                .IsUnique();

            entity.HasOne<Course>()
                .WithMany()
                .HasForeignKey(l => l.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Group>(entity =>
        {
            entity.Property(g => g.Name)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(g => g.CreatedAtUtc)
                .IsRequired();

            entity.HasOne<Academy.Domain.Program>()
                .WithMany()
                .HasForeignKey(g => g.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Course>()
                .WithMany()
                .HasForeignKey(g => g.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Level>()
                .WithMany()
                .HasForeignKey(g => g.LevelId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<AppUser>()
                .WithMany()
                .HasForeignKey(g => g.InstructorUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Session>(entity =>
        {
            entity.Property(s => s.StartsAtUtc)
                .IsRequired();

            entity.Property(s => s.DurationMinutes)
                .IsRequired();

            entity.Property(s => s.Notes)
                .HasMaxLength(800);

            entity.Property(s => s.CreatedAtUtc)
                .IsRequired();

            entity.HasOne<Group>()
                .WithMany()
                .HasForeignKey(s => s.GroupId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<AppUser>()
                .WithMany()
                .HasForeignKey(s => s.InstructorUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<RoutineSlot>(entity =>
        {
            entity.Property(r => r.DayOfWeek)
                .IsRequired();

            entity.Property(r => r.StartTime)
                .IsRequired();

            entity.Property(r => r.DurationMinutes)
                .IsRequired();

            entity.Property(r => r.CreatedAtUtc)
                .IsRequired();

            entity.HasIndex(r => new { r.AcademyId, r.GroupId, r.DayOfWeek, r.StartTime })
                .IsUnique();

            entity.HasOne<Group>()
                .WithMany()
                .HasForeignKey(r => r.GroupId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<AppUser>()
                .WithMany()
                .HasForeignKey(r => r.InstructorUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Student>(entity =>
        {
            entity.Property(s => s.FullName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(s => s.PhotoUrl)
                .HasMaxLength(500);

            entity.Property(s => s.Notes)
                .HasMaxLength(800);

            entity.Property(s => s.CreatedAtUtc)
                .IsRequired();
        });

        builder.Entity<Guardian>(entity =>
        {
            entity.Property(g => g.FullName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(g => g.Phone)
                .HasMaxLength(30);

            entity.Property(g => g.Email)
                .HasMaxLength(254);

            entity.Property(g => g.CreatedAtUtc)
                .IsRequired();
        });

        builder.Entity<StudentGuardian>(entity =>
        {
            entity.Property(sg => sg.Relation)
                .HasMaxLength(50);

            entity.Property(sg => sg.CreatedAtUtc)
                .IsRequired();

            entity.HasIndex(sg => new { sg.AcademyId, sg.StudentId, sg.GuardianId })
                .IsUnique();

            entity.HasOne<Student>()
                .WithMany()
                .HasForeignKey(sg => sg.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Guardian>()
                .WithMany()
                .HasForeignKey(sg => sg.GuardianId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Enrollment>(entity =>
        {
            entity.Property(e => e.StartDate)
                .IsRequired();

            entity.Property(e => e.CreatedAtUtc)
                .IsRequired();

            entity.HasIndex(e => new { e.AcademyId, e.StudentId, e.GroupId, e.StartDate });

            entity.HasIndex(e => new { e.AcademyId, e.StudentId });
            entity.HasIndex(e => new { e.AcademyId, e.GroupId });

            entity.HasOne<Student>()
                .WithMany()
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Group>()
                .WithMany()
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<AttendanceRecord>(entity =>
        {
            entity.Property(a => a.Status)
                .IsRequired();

            entity.Property(a => a.Reason)
                .HasMaxLength(200);

            entity.Property(a => a.Note)
                .HasMaxLength(500);

            entity.Property(a => a.MarkedAtUtc)
                .IsRequired();

            entity.HasIndex(a => new { a.AcademyId, a.SessionId, a.StudentId })
                .IsUnique();

            entity.HasOne<Session>()
                .WithMany()
                .HasForeignKey(a => a.SessionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Student>()
                .WithMany()
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<AppUser>()
                .WithMany()
                .HasForeignKey(a => a.MarkedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<BehaviorEvent>(entity =>
        {
            entity.Property(b => b.Type)
                .IsRequired();

            entity.Property(b => b.Points)
                .IsRequired();

            entity.Property(b => b.Reason)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(b => b.Note)
                .HasMaxLength(500);

            entity.Property(b => b.CreatedAtUtc)
                .IsRequired();

            entity.HasIndex(b => new { b.AcademyId, b.StudentId });

            entity.HasOne<Student>()
                .WithMany()
                .HasForeignKey(b => b.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Session>()
                .WithMany()
                .HasForeignKey(b => b.SessionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<AppUser>()
                .WithMany()
                .HasForeignKey(b => b.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Question>(entity =>
        {
            entity.Property(q => q.Text)
                .IsRequired()
                .HasMaxLength(4000);

            entity.Property(q => q.Tags)
                .HasMaxLength(500);

            entity.Property(q => q.CreatedAtUtc)
                .IsRequired();

            entity.Property(q => q.Type)
                .IsRequired();

            entity.Property(q => q.Difficulty)
                .IsRequired();

            entity.HasOne<Academy.Domain.Program>()
                .WithMany()
                .HasForeignKey(q => q.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Course>()
                .WithMany()
                .HasForeignKey(q => q.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Level>()
                .WithMany()
                .HasForeignKey(q => q.LevelId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<AppUser>()
                .WithMany()
                .HasForeignKey(q => q.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<QuestionOption>(entity =>
        {
            entity.Property(o => o.Text)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(o => o.SortOrder)
                .IsRequired();

            entity.HasIndex(o => new { o.AcademyId, o.QuestionId });

            entity.HasOne<Question>()
                .WithMany()
                .HasForeignKey(o => o.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Assignment>(entity =>
        {
            entity.Property(a => a.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(a => a.Description)
                .HasMaxLength(2000);

            entity.Property(a => a.CreatedAtUtc)
                .IsRequired();

            entity.HasOne<Group>()
                .WithMany()
                .HasForeignKey(a => a.GroupId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<AppUser>()
                .WithMany()
                .HasForeignKey(a => a.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<AssignmentAttachment>(entity =>
        {
            entity.Property(a => a.FileUrl)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(a => a.FileName)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(a => a.ContentType)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(a => a.CreatedAtUtc)
                .IsRequired();

            entity.HasOne<Assignment>()
                .WithMany()
                .HasForeignKey(a => a.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AssignmentTarget>(entity =>
        {
            entity.Property(a => a.CreatedAtUtc)
                .IsRequired();

            entity.HasIndex(a => new { a.AcademyId, a.AssignmentId, a.StudentId })
                .IsUnique();

            entity.HasOne<Assignment>()
                .WithMany()
                .HasForeignKey(a => a.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Student>()
                .WithMany()
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Announcement>(entity =>
        {
            entity.Property(a => a.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(a => a.Body)
                .IsRequired()
                .HasMaxLength(5000);

            entity.Property(a => a.PublishedAtUtc)
                .IsRequired();

            entity.Property(a => a.CreatedAtUtc)
                .IsRequired();

            entity.HasOne<Group>()
                .WithMany()
                .HasForeignKey(a => a.GroupId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<AppUser>()
                .WithMany()
                .HasForeignKey(a => a.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Notification>(entity =>
        {
            entity.Property(n => n.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(n => n.Body)
                .IsRequired()
                .HasMaxLength(5000);

            entity.Property(n => n.CreatedAtUtc)
                .IsRequired();

            entity.HasIndex(n => new { n.AcademyId, n.UserId });

            entity.HasOne<AppUser>()
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Announcement>()
                .WithMany()
                .HasForeignKey(n => n.AnnouncementId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<EvaluationTemplate>(entity =>
        {
            entity.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(t => t.Description)
                .HasMaxLength(800);

            entity.Property(t => t.CreatedAtUtc)
                .IsRequired();

            entity.HasOne<Academy.Domain.Program>()
                .WithMany()
                .HasForeignKey(t => t.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Course>()
                .WithMany()
                .HasForeignKey(t => t.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Level>()
                .WithMany()
                .HasForeignKey(t => t.LevelId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<RubricCriterion>(entity =>
        {
            entity.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(c => c.MaxScore)
                .IsRequired();

            entity.Property(c => c.Weight)
                .HasColumnType("decimal(5,2)");

            entity.Property(c => c.SortOrder)
                .IsRequired();

            entity.Property(c => c.CreatedAtUtc)
                .IsRequired();

            entity.HasIndex(c => new { c.AcademyId, c.TemplateId, c.Name })
                .IsUnique();

            entity.HasOne<EvaluationTemplate>()
                .WithMany()
                .HasForeignKey(c => c.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Evaluation>(entity =>
        {
            entity.Property(e => e.Notes)
                .HasMaxLength(1000);

            entity.Property(e => e.TotalScore)
                .HasColumnType("decimal(7,2)");

            entity.Property(e => e.CreatedAtUtc)
                .IsRequired();

            entity.HasOne<Student>()
                .WithMany()
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<EvaluationTemplate>()
                .WithMany()
                .HasForeignKey(e => e.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Session>()
                .WithMany()
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<AppUser>()
                .WithMany()
                .HasForeignKey(e => e.EvaluatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<EvaluationItem>(entity =>
        {
            entity.Property(i => i.Score)
                .HasColumnType("decimal(5,2)");

            entity.Property(i => i.Comment)
                .HasMaxLength(500);

            entity.HasIndex(i => new { i.AcademyId, i.EvaluationId, i.CriterionId })
                .IsUnique();

            entity.HasOne<Evaluation>()
                .WithMany()
                .HasForeignKey(i => i.EvaluationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<RubricCriterion>()
                .WithMany()
                .HasForeignKey(i => i.CriterionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.ApplyAcademyScopedQueryFilters(() => _currentAcademyId);
    }
}
