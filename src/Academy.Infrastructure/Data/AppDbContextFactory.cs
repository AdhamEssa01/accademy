using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Academy.Infrastructure.Data;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();
        var apiPath = Path.Combine(basePath, "src", "Academy.Api");
        if (File.Exists(Path.Combine(apiPath, "appsettings.Development.json")))
        {
            basePath = apiPath;
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var provider = configuration["Database:Provider"];
        if (string.IsNullOrWhiteSpace(provider))
        {
            provider = "SqlServer";
        }

        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = string.Equals(provider, "Sqlite", StringComparison.OrdinalIgnoreCase)
                ? "Data Source=academy_dev.db"
                : "Server=(localdb)\\MSSQLLocalDB;Database=AcademyDev;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
        }

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        if (string.Equals(provider, "SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            optionsBuilder.UseSqlServer(connectionString, b =>
                b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
        }
        else if (string.Equals(provider, "Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            optionsBuilder.UseSqlite(connectionString, b =>
                b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
        }
        else
        {
            throw new InvalidOperationException($"Unsupported database provider '{provider}'.");
        }

        var options = optionsBuilder.Options;

        return new AppDbContext(options, null);
    }
}
