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

        builder.ApplyAcademyScopedQueryFilters(() => _currentAcademyId);
    }
}
