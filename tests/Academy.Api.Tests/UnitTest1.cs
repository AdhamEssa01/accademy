using System.IO;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Academy.Application.Abstractions.Auth;
using Academy.Application.Contracts.Auth;
using Academy.Application.Exceptions;
using Academy.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Academy.Api.Tests;

public sealed class ApiIntegrationTests : IAsyncLifetime
{
    private const string TestIssuer = "academy-test";
    private const string TestAudience = "academy-test";
    private const string TestKey = "dev-test-key-please-change-1234567890";

    private readonly WebApplicationFactory<Program> _factory;
    private readonly string _dbPath;

    public ApiIntegrationTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"academy_test_{Guid.NewGuid():N}.db");

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Default"] = $"Data Source={_dbPath}",
                    ["Jwt:Issuer"] = TestIssuer,
                    ["Jwt:Audience"] = TestAudience,
                    ["Jwt:Key"] = TestKey,
                    ["Jwt:AccessTokenMinutes"] = "15",
                    ["Jwt:RefreshTokenDays"] = "7",
                    ["Seeding:Enabled"] = "false",
                    ["DebugEndpoints:Enabled"] = "true"
                });
            });
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IGoogleIdTokenValidator>();
                services.AddSingleton<FakeGoogleIdTokenValidator>();
                services.AddSingleton<IGoogleIdTokenValidator>(sp => sp.GetRequiredService<FakeGoogleIdTokenValidator>());
            });
        });
    }

    public async Task InitializeAsync()
    {
        if (File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }

        var wal = _dbPath + "-wal";
        if (File.Exists(wal))
        {
            File.Delete(wal);
        }

        var shm = _dbPath + "-shm";
        if (File.Exists(shm))
        {
            File.Delete(shm);
        }

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        await DbSeeder.SeedAsync(scope.ServiceProvider);
    }

    public Task DisposeAsync()
    {
        _factory.Dispose();

        if (File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }

        var wal = _dbPath + "-wal";
        if (File.Exists(wal))
        {
            File.Delete(wal);
        }

        var shm = _dbPath + "-shm";
        if (File.Exists(shm))
        {
            File.Delete(shm);
        }

        return Task.CompletedTask;
    }

    [Fact]
    public async Task Echo_Invalid_ReturnsValidationProblem()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/demo/echo", new { name = "A", age = 3 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(document.RootElement.TryGetProperty("errors", out var errors));
        Assert.NotEmpty(errors.EnumerateObject());
        Assert.True(document.RootElement.TryGetProperty("traceId", out _));
    }

    [Fact]
    public async Task Paged_ReturnsPagedResponse()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/demo/paged?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(document.RootElement.TryGetProperty("items", out var items));
        Assert.Equal(10, items.GetArrayLength());
        Assert.Equal(1, document.RootElement.GetProperty("page").GetInt32());
        Assert.Equal(10, document.RootElement.GetProperty("pageSize").GetInt32());
        Assert.Equal(50, document.RootElement.GetProperty("total").GetInt64());
    }

    [Fact]
    public async Task Throw_ReturnsProblemDetails()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/demo/throw");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(document.RootElement.TryGetProperty("traceId", out _));
    }

    [Fact]
    public async Task Health_Live_ReturnsStatus()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(document.RootElement.TryGetProperty("status", out _));
    }

    [Fact]
    public async Task Health_Ready_ReturnsStatus()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(document.RootElement.TryGetProperty("status", out _));
    }

    [Fact]
    public async Task Login_WithSeededAdmin_Succeeds()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "admin@local.test",
            password = "Admin123$"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(document.RootElement.TryGetProperty("accessToken", out var accessToken));
        Assert.False(string.IsNullOrWhiteSpace(accessToken.GetString()));
        Assert.True(document.RootElement.TryGetProperty("refreshToken", out var refreshToken));
        Assert.False(string.IsNullOrWhiteSpace(refreshToken.GetString()));

        var roles = document.RootElement.GetProperty("user").GetProperty("roles");
        Assert.Contains("Admin", roles.EnumerateArray().Select(r => r.GetString()));
    }

    [Fact]
    public async Task Me_RequiresAuth()
    {
        var client = _factory.CreateClient();

        var anonymous = await client.GetAsync("/api/v1/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);

        var (accessToken, _, user) = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var meResponse = await client.GetAsync("/api/v1/auth/me");
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        using var document = JsonDocument.Parse(await meResponse.Content.ReadAsStringAsync());
        Assert.Equal(user.Id, document.RootElement.GetProperty("id").GetGuid());
        Assert.Equal(user.Email, document.RootElement.GetProperty("email").GetString());
    }

    [Fact]
    public async Task Refresh_Rotates_Token()
    {
        var client = _factory.CreateClient();
        var (_, refreshToken, _) = await LoginAsync(client);

        var refreshResponse = await client.PostAsJsonAsync("/api/v1/auth/refresh", new
        {
            refreshToken
        });

        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);

        using var document = JsonDocument.Parse(await refreshResponse.Content.ReadAsStringAsync());
        var newRefreshToken = document.RootElement.GetProperty("refreshToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(newRefreshToken));
        Assert.NotEqual(refreshToken, newRefreshToken);

        var oldRefreshResponse = await client.PostAsJsonAsync("/api/v1/auth/refresh", new
        {
            refreshToken
        });

        Assert.Equal(HttpStatusCode.BadRequest, oldRefreshResponse.StatusCode);
    }

    [Fact]
    public async Task Logout_Revokes_RefreshToken()
    {
        var client = _factory.CreateClient();
        var (_, refreshToken, _) = await LoginAsync(client);

        var logoutResponse = await client.PostAsJsonAsync("/api/v1/auth/logout", new
        {
            refreshToken
        });

        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);

        var refreshResponse = await client.PostAsJsonAsync("/api/v1/auth/refresh", new
        {
            refreshToken
        });

        Assert.Equal(HttpStatusCode.BadRequest, refreshResponse.StatusCode);
    }

    [Fact]
    public async Task GoogleLogin_CreatesUser()
    {
        var client = _factory.CreateClient();
        SetGooglePayload(new GoogleIdTokenPayload
        {
            Subject = "google-sub-123",
            Email = "parent1@test.local",
            EmailVerified = true,
            Name = "Parent One"
        });

        var response = await client.PostAsJsonAsync("/api/v1/auth/google", new
        {
            idToken = "fake-token"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("parent1@test.local", document.RootElement.GetProperty("user").GetProperty("email").GetString());
        var roles = document.RootElement.GetProperty("user").GetProperty("roles");
        Assert.Contains("Parent", roles.EnumerateArray().Select(r => r.GetString()));
    }

    [Fact]
    public async Task GoogleLogin_ExistingUser_DoesNotDuplicateProfile()
    {
        var client = _factory.CreateClient();
        SetGooglePayload(new GoogleIdTokenPayload
        {
            Subject = "google-sub-999",
            Email = "parent2@test.local",
            EmailVerified = true,
            Name = "Parent Two"
        });

        var first = await client.PostAsJsonAsync("/api/v1/auth/google", new { idToken = "fake-token" });
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        var second = await client.PostAsJsonAsync("/api/v1/auth/google", new { idToken = "fake-token" });
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await dbContext.Users.FirstAsync(u => u.Email == "parent2@test.local");
        var profileCount = await dbContext.UserProfiles.CountAsync(p => p.UserId == user.Id);
        Assert.Equal(1, profileCount);
    }

    [Fact]
    public async Task GoogleLogin_UnverifiedEmail_Fails()
    {
        var client = _factory.CreateClient();
        SetGooglePayload(new GoogleIdTokenPayload
        {
            Subject = "google-sub-555",
            Email = "parent3@test.local",
            EmailVerified = false,
            Name = "Parent Three"
        }, throwUnverified: true);

        var response = await client.PostAsJsonAsync("/api/v1/auth/google", new
        {
            idToken = "fake-token"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Unverified Google email", document.RootElement.GetProperty("title").GetString());
    }

    [Fact]
    public async Task TenantDebug_Me_RequiresAuth()
    {
        var client = _factory.CreateClient();

        var anonymous = await client.GetAsync("/api/v1/tenant-debug/me");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);

        var (accessToken, _, _) = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.GetAsync("/api/v1/tenant-debug/me");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(document.RootElement.TryGetProperty("academyId", out _));
    }

    [Fact]
    public async Task TenantDebug_SeedSecondAcademy_RequiresAdmin()
    {
        var client = _factory.CreateClient();

        var parentEmail = $"parent_{Guid.NewGuid():N}@local.test";
        var parentToken = await RegisterAsync(client, parentEmail, "Parent User");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parentToken);

        var forbidden = await client.PostAsync("/api/v1/tenant-debug/seed-second-academy", null);
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);

        var (adminToken, _, _) = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await client.PostAsync("/api/v1/tenant-debug/seed-second-academy", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task TenantDebug_QueryFilter_Hides_OtherAcademy()
    {
        var client = _factory.CreateClient();

        var (adminToken, _, _) = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var seedResponse = await client.PostAsync("/api/v1/tenant-debug/seed-second-academy", null);
        if (!seedResponse.IsSuccessStatusCode)
        {
            var body = await seedResponse.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Seed endpoint failed: {(int)seedResponse.StatusCode} {seedResponse.ReasonPhrase}. Body: {body}");
        }

        using var seedDocument = JsonDocument.Parse(await seedResponse.Content.ReadAsStringAsync());
        var secondAcademyId = seedDocument.RootElement.GetProperty("academyId").GetGuid();
        var otherAdminUserId = seedDocument.RootElement.GetProperty("otherAdminUserId").GetGuid();

        var hiddenResponse = await client.GetAsync($"/api/v1/tenant-debug/user-profiles/{otherAdminUserId}");
        Assert.Equal(HttpStatusCode.NotFound, hiddenResponse.StatusCode);

        client.DefaultRequestHeaders.Authorization = null;
        var otherLogin = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "otheradmin@local.test",
            password = "Admin123$"
        });
        otherLogin.EnsureSuccessStatusCode();

        using var otherLoginDocument = JsonDocument.Parse(await otherLogin.Content.ReadAsStringAsync());
        var otherToken = otherLoginDocument.RootElement.GetProperty("accessToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(otherToken));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherToken);
        var visibleResponse = await client.GetAsync($"/api/v1/tenant-debug/user-profiles/{otherAdminUserId}");
        Assert.Equal(HttpStatusCode.OK, visibleResponse.StatusCode);

        using var visibleDocument = JsonDocument.Parse(await visibleResponse.Content.ReadAsStringAsync());
        Assert.Equal(secondAcademyId, visibleDocument.RootElement.GetProperty("academyId").GetGuid());
    }

    [Fact]
    public async Task TenantDebug_MissingAcademyClaim_ReturnsForbidden()
    {
        var client = _factory.CreateClient();
        var token = CreateTokenWithoutAcademyClaim();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/v1/tenant-debug/me");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Missing academy scope", document.RootElement.GetProperty("title").GetString());
    }

    [Fact]
    public async Task Branches_AdminCanCreateAndList()
    {
        var client = _factory.CreateClient();
        var (accessToken, _, _) = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var createResponse = await client.PostAsJsonAsync("/api/v1/branches", new
        {
            name = "Main Branch",
            address = "123 Main St"
        });

        createResponse.EnsureSuccessStatusCode();

        var listResponse = await client.GetAsync("/api/v1/branches?page=1&pageSize=10");
        listResponse.EnsureSuccessStatusCode();

        using var listDocument = JsonDocument.Parse(await listResponse.Content.ReadAsStringAsync());
        var items = listDocument.RootElement.GetProperty("items").EnumerateArray().ToArray();
        Assert.Contains(items, item => item.GetProperty("name").GetString() == "Main Branch");
    }

    [Fact]
    public async Task Branches_ParentCannotCreate()
    {
        var client = _factory.CreateClient();
        var parentEmail = $"parent_{Guid.NewGuid():N}@local.test";
        var parentToken = await RegisterAsync(client, parentEmail, "Parent User");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parentToken);

        var response = await client.PostAsJsonAsync("/api/v1/branches", new
        {
            name = "Parent Branch",
            address = "123 Main St"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<(string AccessToken, string RefreshToken, UserSnapshot User)> LoginAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "admin@local.test",
            password = "Admin123$"
        });

        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var accessToken = document.RootElement.GetProperty("accessToken").GetString() ?? string.Empty;
        var refreshToken = document.RootElement.GetProperty("refreshToken").GetString() ?? string.Empty;

        var userElement = document.RootElement.GetProperty("user");
        var user = new UserSnapshot(
            userElement.GetProperty("id").GetGuid(),
            userElement.GetProperty("email").GetString() ?? string.Empty);

        return (accessToken, refreshToken, user);
    }

    private async Task<string> RegisterAsync(HttpClient client, string email, string displayName)
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password = "Parent123$",
            displayName
        });

        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("accessToken").GetString() ?? string.Empty;
    }

    private static string CreateTokenWithoutAcademyClaim()
    {
        var userId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, "missing@local.test")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: TestIssuer,
            audience: TestAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private readonly record struct UserSnapshot(Guid Id, string Email);

    private void SetGooglePayload(GoogleIdTokenPayload payload, bool throwUnverified = false)
    {
        var validator = _factory.Services.GetRequiredService<FakeGoogleIdTokenValidator>();
        validator.Payload = payload;
        validator.ThrowUnverified = throwUnverified;
    }

    private sealed class FakeGoogleIdTokenValidator : IGoogleIdTokenValidator
    {
        public GoogleIdTokenPayload? Payload { get; set; }
        public bool ThrowUnverified { get; set; }

        public Task<GoogleIdTokenPayload> ValidateAsync(string idToken, CancellationToken ct)
        {
            if (Payload is null)
            {
                throw new InvalidGoogleTokenException();
            }

            if (ThrowUnverified)
            {
                throw new UnverifiedGoogleEmailException();
            }

            return Task.FromResult(Payload);
        }
    }
}
