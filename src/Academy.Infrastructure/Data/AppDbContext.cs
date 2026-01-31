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

        builder.ApplyAcademyScopedQueryFilters(() => _currentAcademyId);
    }
}
