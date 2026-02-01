using System.Globalization;
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
using Academy.Infrastructure.Identity;
using Academy.Shared.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
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
    private readonly string _databaseName;
    private readonly string _connectionString;

    public ApiIntegrationTests()
    {
        _databaseName = $"AcademyTest_{Guid.NewGuid():N}";
        _connectionString = BuildConnectionString(_databaseName);

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Default"] = _connectionString,
                    ["RateLimiting:AuthPerMinute"] = "1000",
                    ["RateLimiting:GeneralPerMinute"] = "10000",
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
                ConfigureSqlServerForTests(services, _connectionString);
                services.RemoveAll<IGoogleIdTokenValidator>();
                services.AddSingleton<FakeGoogleIdTokenValidator>();
                services.AddSingleton<IGoogleIdTokenValidator>(sp => sp.GetRequiredService<FakeGoogleIdTokenValidator>());
            });
        });
    }

    public async Task InitializeAsync()
    {
        await DropDatabaseAsync(_databaseName);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await DbSeeder.SeedAsync(scope.ServiceProvider);
    }

    public Task DisposeAsync()
    {
        _factory.Dispose();

        return DropDatabaseAsync(_databaseName);
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
    public async Task AuthRateLimit_ReturnsTooManyRequests()
    {
        var rateDatabaseName = $"AcademyTestRate_{Guid.NewGuid():N}";
        using var limitedFactory = CreateFactory(rateDatabaseName, authLimit: 3, generalLimit: 10000);
        using (var scope = limitedFactory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbSeeder.SeedAsync(scope.ServiceProvider);
        }

        var client = limitedFactory.CreateClient();
        HttpResponseMessage? lastResponse = null;

        for (var i = 0; i < 12; i++)
        {
            lastResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new
            {
                email = "invalid@local.test",
                password = "badpass1"
            });
        }

        Assert.NotNull(lastResponse);
        Assert.Equal(HttpStatusCode.TooManyRequests, lastResponse!.StatusCode);
        Assert.Equal("application/problem+json", lastResponse.Content.Headers.ContentType?.MediaType);

        using var document = JsonDocument.Parse(await lastResponse.Content.ReadAsStringAsync());
        Assert.Equal("Too many requests", document.RootElement.GetProperty("title").GetString());

        await DropDatabaseAsync(rateDatabaseName);
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

    [Fact]
    public async Task Students_AdminCanCreate()
    {
        var client = _factory.CreateClient();
        var (accessToken, _, _) = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.PostAsJsonAsync("/api/v1/students", new
        {
            fullName = "Student One",
            notes = "Notes"
        });

        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Student One", document.RootElement.GetProperty("fullName").GetString());
    }

    [Fact]
    public async Task Students_ParentCannotList()
    {
        var client = _factory.CreateClient();
        var parentEmail = $"parent_{Guid.NewGuid():N}@local.test";
        var parentToken = await RegisterAsync(client, parentEmail, "Parent User");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parentToken);

        var response = await client.GetAsync("/api/v1/students?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ParentPortal_Returns_Linked_Student()
    {
        var client = _factory.CreateClient();
        var (adminToken, _, _) = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var studentId = await CreateStudentAsync(client, "Child One");
        var guardianId = await CreateGuardianAsync(client, "Parent One");

        var (parentToken, parentUserId) = await RegisterWithUserAsync(client, $"parent_{Guid.NewGuid():N}@local.test", "Parent One");

        var linkUserResponse = await client.PostAsJsonAsync($"/api/v1/guardians/{guardianId}/link-user", new
        {
            userId = parentUserId
        });
        linkUserResponse.EnsureSuccessStatusCode();

        var linkStudentResponse = await client.PostAsJsonAsync($"/api/v1/students/{studentId}/guardians/{guardianId}", new
        {
            relation = "Parent"
        });
        linkStudentResponse.EnsureSuccessStatusCode();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parentToken);
        var response = await client.GetAsync("/api/v1/parent/me/children");
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var items = document.RootElement.EnumerateArray().ToArray();
        Assert.Single(items);
        Assert.Equal(studentId, items[0].GetProperty("id").GetGuid());
    }

    [Fact]
    public async Task ParentPortal_OtherParent_Sees_Empty_List()
    {
        var client = _factory.CreateClient();
        var (adminToken, _, _) = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var studentId = await CreateStudentAsync(client, "Child Two");
        var guardianId = await CreateGuardianAsync(client, "Parent Two");

        var (_, parentUserId) = await RegisterWithUserAsync(client, $"parent_{Guid.NewGuid():N}@local.test", "Parent Two");

        var linkUserResponse = await client.PostAsJsonAsync($"/api/v1/guardians/{guardianId}/link-user", new
        {
            userId = parentUserId
        });
        linkUserResponse.EnsureSuccessStatusCode();

        var linkStudentResponse = await client.PostAsJsonAsync($"/api/v1/students/{studentId}/guardians/{guardianId}", new
        {
            relation = "Parent"
        });
        linkStudentResponse.EnsureSuccessStatusCode();

        var (otherParentToken, _) = await RegisterWithUserAsync(client, $"parent_{Guid.NewGuid():N}@local.test", "Other Parent");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherParentToken);

        var response = await client.GetAsync("/api/v1/parent/me/children");
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Empty(document.RootElement.EnumerateArray());
    }

    [Fact]
    public async Task Programs_Courses_Levels_AdminCanCreate()
    {
        var client = _factory.CreateClient();
        var (accessToken, _, _) = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var programResponse = await client.PostAsJsonAsync("/api/v1/programs", new
        {
            name = "Program A",
            description = "Program description"
        });
        programResponse.EnsureSuccessStatusCode();

        using var programDocument = JsonDocument.Parse(await programResponse.Content.ReadAsStringAsync());
        var programId = programDocument.RootElement.GetProperty("id").GetGuid();

        var courseResponse = await client.PostAsJsonAsync("/api/v1/courses", new
        {
            programId,
            name = "Course A",
            description = "Course description"
        });
        courseResponse.EnsureSuccessStatusCode();

        using var courseDocument = JsonDocument.Parse(await courseResponse.Content.ReadAsStringAsync());
        var courseId = courseDocument.RootElement.GetProperty("id").GetGuid();

        var levelResponse = await client.PostAsJsonAsync("/api/v1/levels", new
        {
            courseId,
            name = "Level 1",
            sortOrder = 0
        });
        levelResponse.EnsureSuccessStatusCode();

        using var levelDocument = JsonDocument.Parse(await levelResponse.Content.ReadAsStringAsync());
        Assert.Equal(courseId, levelDocument.RootElement.GetProperty("courseId").GetGuid());
    }

    [Fact]
    public async Task Programs_TenantFilter_Hides_OtherAcademy()
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
        var createResponse = await client.PostAsJsonAsync("/api/v1/programs", new
        {
            name = "Other Program",
            description = "Other description"
        });
        createResponse.EnsureSuccessStatusCode();

        using var createDocument = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        var otherProgramId = createDocument.RootElement.GetProperty("id").GetGuid();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var forbiddenResponse = await client.GetAsync($"/api/v1/programs/{otherProgramId}");

        Assert.Equal(HttpStatusCode.NotFound, forbiddenResponse.StatusCode);
    }

    [Fact]
    public async Task Groups_InstructorCannotCreate()
    {
        var client = _factory.CreateClient();
        var (adminToken, _, _) = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var programId = await CreateProgramAsync(client, "Program G");
        var courseId = await CreateCourseAsync(client, programId, "Course G");
        var levelId = await CreateLevelAsync(client, courseId, "Level G", 0);

        var (instructorId, instructorToken) = await CreateInstructorAsync(client, "instructor1@local.test");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", instructorToken);

        var response = await client.PostAsJsonAsync("/api/v1/groups", new
        {
            programId,
            courseId,
            levelId,
            name = "Group G",
            instructorUserId = instructorId
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Groups_Mine_Returns_Only_Assigned()
    {
        var client = _factory.CreateClient();
        var (adminToken, _, _) = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var programId = await CreateProgramAsync(client, "Program H");
        var courseId = await CreateCourseAsync(client, programId, "Course H");
        var levelId = await CreateLevelAsync(client, courseId, "Level H", 0);

        var (instructorOneId, instructorOneToken) = await CreateInstructorAsync(client, "instructor2@local.test");
        var (instructorTwoId, _) = await CreateInstructorAsync(client, "instructor3@local.test");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        await CreateGroupAsync(client, programId, courseId, levelId, "Group One", instructorOneId);
        await CreateGroupAsync(client, programId, courseId, levelId, "Group Two", instructorTwoId);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", instructorOneToken);
        var response = await client.GetAsync("/api/v1/groups/mine?page=1&pageSize=10");
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var items = document.RootElement.GetProperty("items").EnumerateArray().ToArray();
        Assert.Single(items);
        Assert.Equal("Group One", items[0].GetProperty("name").GetString());
    }

    [Fact]
    public async Task RoutineSlots_InstructorSeesOnlyAssigned()
    {
        var client = _factory.CreateClient();
        var (adminToken, _, _) = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var programId = await CreateProgramAsync(client, "Program R1");
        var courseId = await CreateCourseAsync(client, programId, "Course R1");
        var levelId = await CreateLevelAsync(client, courseId, "Level R1", 0);

        var (instructorOneId, instructorOneToken) = await CreateInstructorAsync(client, "routine_instructor1@local.test");
        var (instructorTwoId, _) = await CreateInstructorAsync(client, "routine_instructor2@local.test");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var groupOneId = await CreateGroupAsync(client, programId, courseId, levelId, "Group R1", instructorOneId);
        var groupTwoId = await CreateGroupAsync(client, programId, courseId, levelId, "Group R2", instructorTwoId);

        await CreateRoutineSlotAsync(client, groupOneId, instructorOneId, DayOfWeek.Monday, new TimeOnly(8, 0), 60);
        await CreateRoutineSlotAsync(client, groupTwoId, instructorTwoId, DayOfWeek.Tuesday, new TimeOnly(9, 0), 60);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", instructorOneToken);
        var response = await client.GetAsync("/api/v1/routine-slots/mine?page=1&pageSize=10");
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, body);

        using var document = JsonDocument.Parse(body);
        var items = document.RootElement.GetProperty("items").EnumerateArray().ToArray();
        Assert.Single(items);
        Assert.Equal(instructorOneId, items[0].GetProperty("instructorUserId").GetGuid());
    }

    [Fact]
    public async Task RoutineSlots_GenerateSessions_CreatesWithoutDuplicates()
    {
        var client = _factory.CreateClient();
        var (adminToken, _, _) = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var programId = await CreateProgramAsync(client, "Program R2");
        var courseId = await CreateCourseAsync(client, programId, "Course R2");
        var levelId = await CreateLevelAsync(client, courseId, "Level R2", 0);

        var (instructorId, _) = await CreateInstructorAsync(client, "routine_instructor3@local.test");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var groupId = await CreateGroupAsync(client, programId, courseId, levelId, "Group R3", instructorId);

        await CreateRoutineSlotAsync(client, groupId, instructorId, DayOfWeek.Monday, new TimeOnly(10, 0), 60);

        var from = NextDate(DateOnly.FromDateTime(DateTime.UtcNow), DayOfWeek.Monday);
        var to = from.AddDays(6);

        var firstResponse = await client.PostAsync(
            $"/api/v1/routine-slots/generate-sessions?from={from.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}&to={to.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}",
            null);
        firstResponse.EnsureSuccessStatusCode();

        using var firstDocument = JsonDocument.Parse(await firstResponse.Content.ReadAsStringAsync());
        Assert.Equal(1, firstDocument.RootElement.GetProperty("created").GetInt32());

        var secondResponse = await client.PostAsync(
            $"/api/v1/routine-slots/generate-sessions?from={from.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}&to={to.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}",
            null);
        secondResponse.EnsureSuccessStatusCode();

        using var secondDocument = JsonDocument.Parse(await secondResponse.Content.ReadAsStringAsync());
        Assert.Equal(0, secondDocument.RootElement.GetProperty("created").GetInt32());
    }

    [Fact]
    public async Task Sessions_Create_Respects_TenantScope()
    {
        var client = _factory.CreateClient();
        var (adminToken, _, adminUser) = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var programId = await CreateProgramAsync(client, "Program S");
        var courseId = await CreateCourseAsync(client, programId, "Course S");
        var levelId = await CreateLevelAsync(client, courseId, "Level S", 0);
        var groupId = await CreateGroupAsync(client, programId, courseId, levelId, "Group S", adminUser.Id);

        var createSessionResponse = await client.PostAsJsonAsync("/api/v1/sessions", new
        {
            groupId,
            instructorUserId = adminUser.Id,
            startsAtUtc = DateTime.UtcNow.AddDays(1),
            durationMinutes = 60,
            notes = "Session notes"
        });
        createSessionResponse.EnsureSuccessStatusCode();

        var seedResponse = await client.PostAsync("/api/v1/tenant-debug/seed-second-academy", null);
        if (!seedResponse.IsSuccessStatusCode)
        {
            var body = await seedResponse.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Seed endpoint failed: {(int)seedResponse.StatusCode} {seedResponse.ReasonPhrase}. Body: {body}");
        }

        client.DefaultRequestHeaders.Authorization = null;
        var otherLogin = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "otheradmin@local.test",
            password = "Admin123$"
        });
        otherLogin.EnsureSuccessStatusCode();

        using var otherLoginDocument = JsonDocument.Parse(await otherLogin.Content.ReadAsStringAsync());
        var otherToken = otherLoginDocument.RootElement.GetProperty("accessToken").GetString();
        var otherUserId = otherLoginDocument.RootElement.GetProperty("user").GetProperty("id").GetGuid();
        Assert.False(string.IsNullOrWhiteSpace(otherToken));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherToken);
        var otherProgramId = await CreateProgramAsync(client, "Program Other");
        var otherCourseId = await CreateCourseAsync(client, otherProgramId, "Course Other");
        var otherLevelId = await CreateLevelAsync(client, otherCourseId, "Level Other", 0);
        var otherGroupId = await CreateGroupAsync(client, otherProgramId, otherCourseId, otherLevelId, "Group Other", otherUserId);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var forbiddenResponse = await client.PostAsJsonAsync("/api/v1/sessions", new
        {
            groupId = otherGroupId,
            instructorUserId = adminUser.Id,
            startsAtUtc = DateTime.UtcNow.AddDays(2),
            durationMinutes = 60
        });

        Assert.Equal(HttpStatusCode.NotFound, forbiddenResponse.StatusCode);
    }

    [Fact]
    public async Task Enrollments_AdminCanEnrollStudent()
    {
        var client = _factory.CreateClient();
        var (adminToken, _, adminUser) = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var programId = await CreateProgramAsync(client, "Program E1");
        var courseId = await CreateCourseAsync(client, programId, "Course E1");
        var levelId = await CreateLevelAsync(client, courseId, "Level E1", 0);
        var groupId = await CreateGroupAsync(client, programId, courseId, levelId, "Group E1", adminUser.Id);
        var studentId = await CreateStudentAsync(client, "Student E1");

        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var response = await client.PostAsJsonAsync("/api/v1/enrollments", new
        {
            studentId,
            groupId,
            startDate
        });

        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(studentId, document.RootElement.GetProperty("studentId").GetGuid());
        Assert.Equal(groupId, document.RootElement.GetProperty("groupId").GetGuid());
    }

    [Fact]
    public async Task Enrollments_EndingEnrollment_Works()
    {
        var client = _factory.CreateClient();
        var (adminToken, _, adminUser) = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var programId = await CreateProgramAsync(client, "Program E2");
        var courseId = await CreateCourseAsync(client, programId, "Course E2");
        var levelId = await CreateLevelAsync(client, courseId, "Level E2", 0);
        var groupId = await CreateGroupAsync(client, programId, courseId, levelId, "Group E2", adminUser.Id);
        var studentId = await CreateStudentAsync(client, "Student E2");

        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var enrollmentId = await CreateEnrollmentAsync(client, studentId, groupId, startDate);

        var endDate = startDate.AddDays(7);
        var endResponse = await client.PostAsJsonAsync($"/api/v1/enrollments/{enrollmentId}/end", new
        {
            endDate
        });

        endResponse.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await endResponse.Content.ReadAsStringAsync());
        var endDateText = document.RootElement.GetProperty("endDate").GetString();
        Assert.Equal(endDate, DateOnly.Parse(endDateText ?? string.Empty));
    }

    [Fact]
    public async Task Enrollments_TenantFilter_Hides_OtherAcademy()
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

        client.DefaultRequestHeaders.Authorization = null;
        var otherLogin = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "otheradmin@local.test",
            password = "Admin123$"
        });
        otherLogin.EnsureSuccessStatusCode();

        using var otherLoginDocument = JsonDocument.Parse(await otherLogin.Content.ReadAsStringAsync());
        var otherToken = otherLoginDocument.RootElement.GetProperty("accessToken").GetString();
        var otherUserId = otherLoginDocument.RootElement.GetProperty("user").GetProperty("id").GetGuid();
        Assert.False(string.IsNullOrWhiteSpace(otherToken));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherToken);
        var programId = await CreateProgramAsync(client, "Program E3");
        var courseId = await CreateCourseAsync(client, programId, "Course E3");
        var levelId = await CreateLevelAsync(client, courseId, "Level E3", 0);
        var groupId = await CreateGroupAsync(client, programId, courseId, levelId, "Group E3", otherUserId);
        var studentId = await CreateStudentAsync(client, "Student E3");

        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        await CreateEnrollmentAsync(client, studentId, groupId, startDate);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var forbiddenResponse = await client.GetAsync($"/api/v1/students/{studentId}/enrollments");

        Assert.Equal(HttpStatusCode.NotFound, forbiddenResponse.StatusCode);
    }

    [Fact]
    public async Task Attendance_InstructorCanSubmitForOwnSession()
    {
        var client = _factory.CreateClient();
        var (adminToken, _, adminUser) = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var programId = await CreateProgramAsync(client, "Program A1");
        var courseId = await CreateCourseAsync(client, programId, "Course A1");
        var levelId = await CreateLevelAsync(client, courseId, "Level A1", 0);

        var (instructorId, instructorToken) = await CreateInstructorAsync(client, "attendance_instructor1@local.test");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var groupId = await CreateGroupAsync(client, programId, courseId, levelId, "Group A1", instructorId);
        var sessionId = await CreateSessionAsync(client, groupId, instructorId, DateTime.UtcNow.AddDays(1), 60);
        var studentId = await CreateStudentAsync(client, "Student A1");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", instructorToken);
        var response = await client.PostAsJsonAsync($"/api/v1/sessions/{sessionId}/attendance", new
        {
            items = new[]
            {
                new
                {
                    studentId,
                    status = 1,
                    note = "Present"
                }
            }
        });

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Attendance_InstructorForOtherSession_IsForbidden()
    {
        var client = _factory.CreateClient();
        var (adminToken, _, adminUser) = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var programId = await CreateProgramAsync(client, "Program A2");
        var courseId = await CreateCourseAsync(client, programId, "Course A2");
        var levelId = await CreateLevelAsync(client, courseId, "Level A2", 0);

        var (instructorOneId, instructorOneToken) = await CreateInstructorAsync(client, "attendance_instructor2@local.test");
        var (instructorTwoId, _) = await CreateInstructorAsync(client, "attendance_instructor3@local.test");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var groupId = await CreateGroupAsync(client, programId, courseId, levelId, "Group A2", instructorTwoId);
        var sessionId = await CreateSessionAsync(client, groupId, instructorTwoId, DateTime.UtcNow.AddDays(2), 60);
        var studentId = await CreateStudentAsync(client, "Student A2");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", instructorOneToken);
        var response = await client.PostAsJsonAsync($"/api/v1/sessions/{sessionId}/attendance", new
        {
            items = new[]
            {
                new
                {
                    studentId,
                    status = 2,
                    note = "Absent"
                }
            }
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Attendance_AdminCanSubmit()
    {
        var client = _factory.CreateClient();
        var (adminToken, _, adminUser) = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var programId = await CreateProgramAsync(client, "Program A3");
        var courseId = await CreateCourseAsync(client, programId, "Course A3");
        var levelId = await CreateLevelAsync(client, courseId, "Level A3", 0);

        var groupId = await CreateGroupAsync(client, programId, courseId, levelId, "Group A3", adminUser.Id);
        var sessionId = await CreateSessionAsync(client, groupId, adminUser.Id, DateTime.UtcNow.AddDays(3), 60);
        var studentId = await CreateStudentAsync(client, "Student A3");

        var response = await client.PostAsJsonAsync($"/api/v1/sessions/{sessionId}/attendance", new
        {
            items = new[]
            {
                new
                {
                    studentId,
                    status = 4,
                    note = "Excused"
                }
            }
        });

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AttendanceQuery_StaffList_ReturnsPaged()
    {
        var client = _factory.CreateClient();
        var (adminToken, _, adminUser) = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var programId = await CreateProgramAsync(client, "Program AQ1");
        var courseId = await CreateCourseAsync(client, programId, "Course AQ1");
        var levelId = await CreateLevelAsync(client, courseId, "Level AQ1", 0);
        var groupId = await CreateGroupAsync(client, programId, courseId, levelId, "Group AQ1", adminUser.Id);
        var sessionId = await CreateSessionAsync(client, groupId, adminUser.Id, DateTime.UtcNow.AddDays(4), 60);
        var studentId = await CreateStudentAsync(client, "Student AQ1");

        var submitResponse = await client.PostAsJsonAsync($"/api/v1/sessions/{sessionId}/attendance", new
        {
            items = new[]
            {
                new
                {
                    studentId,
                    status = 1
                }
            }
        });
        submitResponse.EnsureSuccessStatusCode();

        var response = await client.GetAsync($"/api/v1/attendance?groupId={groupId}&page=1&pageSize=10");
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, body);

        using var document = JsonDocument.Parse(body);
        Assert.True(document.RootElement.TryGetProperty("items", out var items));
        Assert.Equal(1, items.GetArrayLength());
        Assert.Equal(1, document.RootElement.GetProperty("page").GetInt32());
        Assert.Equal(10, document.RootElement.GetProperty("pageSize").GetInt32());
        Assert.Equal(1, document.RootElement.GetProperty("total").GetInt64());
    }

    [Fact]
    public async Task ParentAttendance_ReturnsOnlyLinkedChildRecords()
    {
        var client = _factory.CreateClient();
        var (adminToken, _, adminUser) = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var programId = await CreateProgramAsync(client, "Program AQ2");
        var courseId = await CreateCourseAsync(client, programId, "Course AQ2");
        var levelId = await CreateLevelAsync(client, courseId, "Level AQ2", 0);
        var groupId = await CreateGroupAsync(client, programId, courseId, levelId, "Group AQ2", adminUser.Id);
        var sessionId = await CreateSessionAsync(client, groupId, adminUser.Id, DateTime.UtcNow.AddDays(5), 60);

        var childStudentId = await CreateStudentAsync(client, "Child AQ2");
        var otherStudentId = await CreateStudentAsync(client, "Other AQ2");

        var guardianId = await CreateGuardianAsync(client, "Parent AQ2");
        var (parentToken, parentUserId) = await RegisterWithUserAsync(client, $"parent_{Guid.NewGuid():N}@local.test", "Parent AQ2");

        var linkUserResponse = await client.PostAsJsonAsync($"/api/v1/guardians/{guardianId}/link-user", new
        {
            userId = parentUserId
        });
        linkUserResponse.EnsureSuccessStatusCode();

        var linkStudentResponse = await client.PostAsJsonAsync($"/api/v1/students/{childStudentId}/guardians/{guardianId}", new
        {
            relation = "Parent"
        });
        linkStudentResponse.EnsureSuccessStatusCode();

        var submitResponse = await client.PostAsJsonAsync($"/api/v1/sessions/{sessionId}/attendance", new
        {
            items = new[]
            {
                new { studentId = childStudentId, status = 1 },
                new { studentId = otherStudentId, status = 2 }
            }
        });
        submitResponse.EnsureSuccessStatusCode();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parentToken);
        var response = await client.GetAsync("/api/v1/parent/me/attendance?page=1&pageSize=10");
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, body);

        using var document = JsonDocument.Parse(body);
        var items = document.RootElement.GetProperty("items").EnumerateArray().ToArray();
        Assert.Single(items);
        Assert.Equal(childStudentId, items[0].GetProperty("studentId").GetGuid());
    }

    [Fact]
    public async Task Assignments_ParentSees_Assignments_For_Child_Group()
    {
        var client = _factory.CreateClient();
        var (adminToken, _, adminUser) = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var programId = await CreateProgramAsync(client, "Program AS1");
        var courseId = await CreateCourseAsync(client, programId, "Course AS1");
        var levelId = await CreateLevelAsync(client, courseId, "Level AS1", 0);
        var groupId = await CreateGroupAsync(client, programId, courseId, levelId, "Group AS1", adminUser.Id);
        var studentId = await CreateStudentAsync(client, "Student AS1");

        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        await CreateEnrollmentAsync(client, studentId, groupId, startDate);

        var guardianId = await CreateGuardianAsync(client, "Parent AS1");
        var (parentToken, parentUserId) = await RegisterWithUserAsync(client, $"parent_{Guid.NewGuid():N}@local.test", "Parent AS1");

        var linkUserResponse = await client.PostAsJsonAsync($"/api/v1/guardians/{guardianId}/link-user", new
        {
            userId = parentUserId
        });
        linkUserResponse.EnsureSuccessStatusCode();

        var linkStudentResponse = await client.PostAsJsonAsync($"/api/v1/students/{studentId}/guardians/{guardianId}", new
        {
            relation = "Parent"
        });
        linkStudentResponse.EnsureSuccessStatusCode();

        var createResponse = await client.PostAsJsonAsync("/api/v1/assignments", new
        {
            groupId,
            title = "Assignment One",
            description = "Read chapter 1"
        });
        createResponse.EnsureSuccessStatusCode();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parentToken);
        var response = await client.GetAsync("/api/v1/parent/me/assignments?page=1&pageSize=10");
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var items = document.RootElement.GetProperty("items").EnumerateArray().ToArray();
        Assert.Single(items);
        Assert.Equal("Assignment One", items[0].GetProperty("title").GetString());
    }

    [Fact]
    public async Task Assignments_OtherParent_Sees_Empty_List()
    {
        var client = _factory.CreateClient();
        var (adminToken, _, adminUser) = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var programId = await CreateProgramAsync(client, "Program AS2");
        var courseId = await CreateCourseAsync(client, programId, "Course AS2");
        var levelId = await CreateLevelAsync(client, courseId, "Level AS2", 0);
        var groupId = await CreateGroupAsync(client, programId, courseId, levelId, "Group AS2", adminUser.Id);
        var studentId = await CreateStudentAsync(client, "Student AS2");

        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        await CreateEnrollmentAsync(client, studentId, groupId, startDate);

        var guardianId = await CreateGuardianAsync(client, "Parent AS2");
        var (_, parentUserId) = await RegisterWithUserAsync(client, $"parent_{Guid.NewGuid():N}@local.test", "Parent AS2");

        var linkUserResponse = await client.PostAsJsonAsync($"/api/v1/guardians/{guardianId}/link-user", new
        {
            userId = parentUserId
        });
        linkUserResponse.EnsureSuccessStatusCode();

        var linkStudentResponse = await client.PostAsJsonAsync($"/api/v1/students/{studentId}/guardians/{guardianId}", new
        {
            relation = "Parent"
        });
        linkStudentResponse.EnsureSuccessStatusCode();

        var createResponse = await client.PostAsJsonAsync("/api/v1/assignments", new
        {
            groupId,
            title = "Assignment Two",
            description = "Read chapter 2"
        });
        createResponse.EnsureSuccessStatusCode();

        var (otherParentToken, _) = await RegisterWithUserAsync(client, $"parent_{Guid.NewGuid():N}@local.test", "Other Parent");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherParentToken);

        var response = await client.GetAsync("/api/v1/parent/me/assignments?page=1&pageSize=10");
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Empty(document.RootElement.GetProperty("items").EnumerateArray());
    }

    [Fact]
    public async Task Announcements_Publish_CreatesNotifications_For_GroupParents()
    {
        var client = _factory.CreateClient();
        var (adminToken, _, adminUser) = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var programId = await CreateProgramAsync(client, "Program AN1");
        var courseId = await CreateCourseAsync(client, programId, "Course AN1");
        var levelId = await CreateLevelAsync(client, courseId, "Level AN1", 0);
        var groupId = await CreateGroupAsync(client, programId, courseId, levelId, "Group AN1", adminUser.Id);
        var studentId = await CreateStudentAsync(client, "Student AN1");

        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        await CreateEnrollmentAsync(client, studentId, groupId, startDate);

        var guardianId = await CreateGuardianAsync(client, "Parent AN1");
        var (parentToken, parentUserId) = await RegisterWithUserAsync(client, $"parent_{Guid.NewGuid():N}@local.test", "Parent AN1");

        var linkUserResponse = await client.PostAsJsonAsync($"/api/v1/guardians/{guardianId}/link-user", new
        {
            userId = parentUserId
        });
        linkUserResponse.EnsureSuccessStatusCode();

        var linkStudentResponse = await client.PostAsJsonAsync($"/api/v1/students/{studentId}/guardians/{guardianId}", new
        {
            relation = "Parent"
        });
        linkStudentResponse.EnsureSuccessStatusCode();

        var createResponse = await client.PostAsJsonAsync("/api/v1/announcements", new
        {
            title = "Announcement One",
            body = "Hello parents",
            audience = 3,
            groupId
        });
        createResponse.EnsureSuccessStatusCode();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parentToken);
        var notificationsResponse = await client.GetAsync("/api/v1/notifications?page=1&pageSize=10");
        var body = await notificationsResponse.Content.ReadAsStringAsync();
        Assert.True(notificationsResponse.IsSuccessStatusCode, body);

        using var document = JsonDocument.Parse(body);
        var items = document.RootElement.GetProperty("items").EnumerateArray().ToArray();
        Assert.Contains(items, item => item.GetProperty("title").GetString() == "Announcement One");
    }

    [Fact]
    public async Task ParentAnnouncements_OnlyParentTargets()
    {
        var client = _factory.CreateClient();
        var (adminToken, _, adminUser) = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var programId = await CreateProgramAsync(client, "Program AN2");
        var courseId = await CreateCourseAsync(client, programId, "Course AN2");
        var levelId = await CreateLevelAsync(client, courseId, "Level AN2", 0);
        var groupId = await CreateGroupAsync(client, programId, courseId, levelId, "Group AN2", adminUser.Id);
        var studentId = await CreateStudentAsync(client, "Student AN2");

        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        await CreateEnrollmentAsync(client, studentId, groupId, startDate);

        var guardianId = await CreateGuardianAsync(client, "Parent AN2");
        var (parentToken, parentUserId) = await RegisterWithUserAsync(client, $"parent_{Guid.NewGuid():N}@local.test", "Parent AN2");

        var linkUserResponse = await client.PostAsJsonAsync($"/api/v1/guardians/{guardianId}/link-user", new
        {
            userId = parentUserId
        });
        linkUserResponse.EnsureSuccessStatusCode();

        var linkStudentResponse = await client.PostAsJsonAsync($"/api/v1/students/{studentId}/guardians/{guardianId}", new
        {
            relation = "Parent"
        });
        linkStudentResponse.EnsureSuccessStatusCode();

        var allParentsResponse = await client.PostAsJsonAsync("/api/v1/announcements", new
        {
            title = "All Parents",
            body = "Parents only",
            audience = 1
        });
        allParentsResponse.EnsureSuccessStatusCode();

        var allStaffResponse = await client.PostAsJsonAsync("/api/v1/announcements", new
        {
            title = "All Staff",
            body = "Staff only",
            audience = 2
        });
        allStaffResponse.EnsureSuccessStatusCode();

        var groupParentsResponse = await client.PostAsJsonAsync("/api/v1/announcements", new
        {
            title = "Group Parents",
            body = "Group only",
            audience = 3,
            groupId
        });
        groupParentsResponse.EnsureSuccessStatusCode();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parentToken);
        var response = await client.GetAsync("/api/v1/parent/me/announcements?page=1&pageSize=10");
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var titles = document.RootElement.GetProperty("items")
            .EnumerateArray()
            .Select(item => item.GetProperty("title").GetString())
            .Where(title => !string.IsNullOrWhiteSpace(title))
            .ToArray();

        Assert.Contains("All Parents", titles);
        Assert.Contains("Group Parents", titles);
        Assert.DoesNotContain("All Staff", titles);
    }

    [Fact]
    public async Task EvaluationTemplates_AdminCanCreateTemplateAndCriterion()
    {
        var client = _factory.CreateClient();
        var (adminToken, _, _) = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var templateId = await CreateEvaluationTemplateAsync(client, "Template A");
        var criterionId = await CreateRubricCriterionAsync(client, templateId, "Participation", 10, 1.5m, 0);

        var response = await client.GetAsync($"/api/v1/evaluation-templates/{templateId}/criteria?page=1&pageSize=10");
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var items = document.RootElement.GetProperty("items").EnumerateArray().ToArray();
        Assert.Single(items);
        Assert.Equal(criterionId, items[0].GetProperty("id").GetGuid());
    }

    [Fact]
    public async Task EvaluationTemplates_ParentForbidden()
    {
        var client = _factory.CreateClient();
        var parentToken = await RegisterAsync(client, $"parent_{Guid.NewGuid():N}@local.test", "Parent Eval");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parentToken);

        var response = await client.GetAsync("/api/v1/evaluation-templates?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task EvaluationTemplates_TenantIsolation_Hides_OtherAcademy()
    {
        var client = _factory.CreateClient();
        var (adminToken, _, _) = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var seedResponse = await client.PostAsync("/api/v1/tenant-debug/seed-second-academy", null);
        seedResponse.EnsureSuccessStatusCode();

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
        var otherTemplateId = await CreateEvaluationTemplateAsync(client, "Template Other");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var response = await client.GetAsync($"/api/v1/evaluation-templates/{otherTemplateId}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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

    private static async Task<(string AccessToken, Guid UserId)> RegisterWithUserAsync(
        HttpClient client,
        string email,
        string displayName)
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password = "Parent123$",
            displayName
        });

        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var accessToken = document.RootElement.GetProperty("accessToken").GetString() ?? string.Empty;
        var userId = document.RootElement.GetProperty("user").GetProperty("id").GetGuid();
        return (accessToken, userId);
    }

    private static WebApplicationFactory<Program> CreateFactory(string databaseName, int authLimit, int generalLimit)
        => new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Default"] = BuildConnectionString(databaseName),
                    ["RateLimiting:AuthPerMinute"] = authLimit.ToString(),
                    ["RateLimiting:GeneralPerMinute"] = generalLimit.ToString(),
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
                ConfigureSqlServerForTests(services, BuildConnectionString(databaseName));
                services.RemoveAll<IGoogleIdTokenValidator>();
                services.AddSingleton<FakeGoogleIdTokenValidator>();
                services.AddSingleton<IGoogleIdTokenValidator>(sp => sp.GetRequiredService<FakeGoogleIdTokenValidator>());
            });
        });

    private async Task<(Guid UserId, string Token)> CreateInstructorAsync(HttpClient client, string email)
    {
        var password = "Instructor123$";
        client.DefaultRequestHeaders.Authorization = null;

        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password,
            displayName = "Instructor"
        });
        registerResponse.EnsureSuccessStatusCode();

        using var registerDoc = JsonDocument.Parse(await registerResponse.Content.ReadAsStringAsync());
        var userId = registerDoc.RootElement.GetProperty("user").GetProperty("id").GetGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user is null)
            {
                throw new InvalidOperationException("Instructor user not found.");
            }

            if (!await userManager.IsInRoleAsync(user, Roles.Instructor))
            {
                var result = await userManager.AddToRoleAsync(user, Roles.Instructor);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to assign instructor role: {errors}");
                }
            }
        }

        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email,
            password
        });
        loginResponse.EnsureSuccessStatusCode();

        using var loginDoc = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync());
        var token = loginDoc.RootElement.GetProperty("accessToken").GetString() ?? string.Empty;

        return (userId, token);
    }

    private static async Task<Guid> CreateProgramAsync(HttpClient client, string name)
    {
        var response = await client.PostAsJsonAsync("/api/v1/programs", new
        {
            name,
            description = "Auto"
        });
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("id").GetGuid();
    }

    private static async Task<Guid> CreateCourseAsync(HttpClient client, Guid programId, string name)
    {
        var response = await client.PostAsJsonAsync("/api/v1/courses", new
        {
            programId,
            name,
            description = "Auto"
        });
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("id").GetGuid();
    }

    private static async Task<Guid> CreateLevelAsync(HttpClient client, Guid courseId, string name, int sortOrder)
    {
        var response = await client.PostAsJsonAsync("/api/v1/levels", new
        {
            courseId,
            name,
            sortOrder
        });
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("id").GetGuid();
    }

    private static async Task<Guid> CreateGroupAsync(
        HttpClient client,
        Guid programId,
        Guid courseId,
        Guid levelId,
        string name,
        Guid? instructorUserId)
    {
        var response = await client.PostAsJsonAsync("/api/v1/groups", new
        {
            programId,
            courseId,
            levelId,
            name,
            instructorUserId
        });
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("id").GetGuid();
    }

    private static async Task<Guid> CreateRoutineSlotAsync(
        HttpClient client,
        Guid groupId,
        Guid instructorUserId,
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        int durationMinutes)
    {
        var response = await client.PostAsJsonAsync("/api/v1/routine-slots", new
        {
            groupId,
            dayOfWeek = (int)dayOfWeek,
            startTime = startTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
            durationMinutes,
            instructorUserId
        });
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("id").GetGuid();
    }

    private static async Task<Guid> CreateSessionAsync(
        HttpClient client,
        Guid groupId,
        Guid instructorUserId,
        DateTime startsAtUtc,
        int durationMinutes)
    {
        var response = await client.PostAsJsonAsync("/api/v1/sessions", new
        {
            groupId,
            instructorUserId,
            startsAtUtc,
            durationMinutes,
            notes = "Session"
        });
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("id").GetGuid();
    }

    private static async Task<Guid> CreateStudentAsync(HttpClient client, string fullName)
    {
        var response = await client.PostAsJsonAsync("/api/v1/students", new
        {
            fullName
        });
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("id").GetGuid();
    }

    private static async Task<Guid> CreateGuardianAsync(HttpClient client, string fullName)
    {
        var response = await client.PostAsJsonAsync("/api/v1/guardians", new
        {
            fullName
        });
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("id").GetGuid();
    }

    private static async Task<Guid> CreateEnrollmentAsync(
        HttpClient client,
        Guid studentId,
        Guid groupId,
        DateOnly startDate)
    {
        var response = await client.PostAsJsonAsync("/api/v1/enrollments", new
        {
            studentId,
            groupId,
            startDate
        });
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("id").GetGuid();
    }

    private static async Task<Guid> CreateEvaluationTemplateAsync(HttpClient client, string name)
    {
        var response = await client.PostAsJsonAsync("/api/v1/evaluation-templates", new
        {
            name,
            description = "Template"
        });
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("id").GetGuid();
    }

    private static async Task<Guid> CreateRubricCriterionAsync(
        HttpClient client,
        Guid templateId,
        string name,
        int maxScore,
        decimal weight,
        int sortOrder)
    {
        var response = await client.PostAsJsonAsync($"/api/v1/evaluation-templates/{templateId}/criteria", new
        {
            name,
            maxScore,
            weight,
            sortOrder
        });
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("id").GetGuid();
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

    private static DateOnly NextDate(DateOnly start, DayOfWeek dayOfWeek)
    {
        var date = start;
        while (date.DayOfWeek != dayOfWeek)
        {
            date = date.AddDays(1);
        }

        return date;
    }

    private static void ConfigureSqlServerForTests(IServiceCollection services, string connectionString)
    {
        services.RemoveAll<DbContextOptions<AppDbContext>>();
        services.RemoveAll<AppDbContext>();
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
        });
    }

    private static string BuildConnectionString(string databaseName)
        => $"Server=.;Database={databaseName};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Pooling=False";

    private static async Task DropDatabaseAsync(string databaseName)
    {
        var masterConnection = "Server=.;Database=master;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;";

        await using var connection = new SqlConnection(masterConnection);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText =
            $"IF DB_ID(N'{databaseName}') IS NOT NULL BEGIN ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{databaseName}]; END";
        await command.ExecuteNonQueryAsync();
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
