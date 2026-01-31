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

    public DbSet<Student> Students => Set<Student>();

    public DbSet<Guardian> Guardians => Set<Guardian>();

    public DbSet<StudentGuardian> StudentGuardians => Set<StudentGuardian>();

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

        builder.ApplyAcademyScopedQueryFilters(() => _currentAcademyId);
    }
}
