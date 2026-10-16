# write-tests.ps1
# Run from solution root: C:\Users\Student\source\repos\RaceDay

$ErrorActionPreference = "Stop"

# ============================================================
# CustomWebApplicationFactory — spins up API against in-memory SQLite
# ============================================================

@'
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.API.Data;

namespace RaceDay.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();

        builder.ConfigureServices(services =>
        {
            var descriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<RaceDayDbContext>)
                         || d.ServiceType == typeof(RaceDayDbContext))
                .ToList();
            foreach (var d in descriptors) services.Remove(d);

            services.AddDbContext<RaceDayDbContext>(options =>
                options.UseSqlite(_connection));

            // Build schema
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();
            db.Database.EnsureCreated();
        });

        builder.UseEnvironment("Testing");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection.Dispose();
    }
}
'@ | Set-Content -Path "RaceDay.Tests\CustomWebApplicationFactory.cs" -Encoding UTF8

# ============================================================
# TestHelpers — creates a logged-in HttpClient
# ============================================================

@'
using System.Net;
using System.Net.Http.Json;

namespace RaceDay.Tests;

public static class TestHelpers
{
    public static async Task<HttpClient> CreateLoggedInClientAsync(
        HttpClient client, string email, string password, string role)
    {
        var reg = await client.PostAsJsonAsync("/api/auth/register", new
        {
            FirstName = "Test",
            LastName = "User",
            Email = email,
            Password = password,
            Role = role,
            PhoneNumber = (string?)null,
            DateOfBirth = (DateTime?)null
        });

        if (reg.StatusCode != HttpStatusCode.Created && reg.StatusCode != HttpStatusCode.Conflict)
            throw new Exception($"Register failed: {reg.StatusCode} - {await reg.Content.ReadAsStringAsync()}");

        var login = await client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = password });
        login.EnsureSuccessStatusCode();

        return client;
    }
}
'@ | Set-Content -Path "RaceDay.Tests\TestHelpers.cs" -Encoding UTF8

# ============================================================
# AuthTests
# ============================================================

@'
using System.Net;
using System.Net.Http.Json;

namespace RaceDay.Tests;

public class AuthTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    public AuthTests(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Register_NewParticipant_Returns201()
    {
        var client = _factory.CreateClient();

        var resp = await client.PostAsJsonAsync("/api/auth/register", new
        {
            FirstName = "Sipho",
            LastName = "Ndlovu",
            Email = "sipho@example.com",
            Password = "Secret123!",
            Role = "Participant",
            PhoneNumber = "0821234567",
            DateOfBirth = (DateTime?)null
        });

        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        var client = _factory.CreateClient();
        var payload = new
        {
            FirstName = "Ann", LastName = "Botha", Email = "ann@example.com",
            Password = "Passw0rd!", Role = "Participant",
            PhoneNumber = (string?)null, DateOfBirth = (DateTime?)null
        };

        await client.PostAsJsonAsync("/api/auth/register", payload);
        var second = await client.PostAsJsonAsync("/api/auth/register", payload);

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_Returns200()
    {
        var client = _factory.CreateClient();

        await client.PostAsJsonAsync("/api/auth/register", new
        {
            FirstName = "Zanele", LastName = "Mokoena", Email = "zanele@example.com",
            Password = "Passw0rd!", Role = "Participant",
            PhoneNumber = (string?)null, DateOfBirth = (DateTime?)null
        });

        var login = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "zanele@example.com", Password = "Passw0rd!"
        });

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    [Fact]
    public async Task Login_InvalidPassword_Returns401()
    {
        var client = _factory.CreateClient();

        await client.PostAsJsonAsync("/api/auth/register", new
        {
            FirstName = "Thabo", LastName = "Dlamini", Email = "thabo@example.com",
            Password = "Passw0rd!", Role = "Participant",
            PhoneNumber = (string?)null, DateOfBirth = (DateTime?)null
        });

        var login = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "thabo@example.com", Password = "WrongPass!"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
    }
}
'@ | Set-Content -Path "RaceDay.Tests\AuthTests.cs" -Encoding UTF8

# ============================================================
# EventsTests
# ============================================================

@'
using System.Net;
using System.Net.Http.Json;

namespace RaceDay.Tests;

public class EventsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    public EventsTests(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Organiser_CanCreateEvent()
    {
        var client = _factory.CreateClient();
        await TestHelpers.CreateLoggedInClientAsync(client, "org1@example.com", "Passw0rd!", "Organiser");

        var resp = await client.PostAsJsonAsync("/api/events", new
        {
            Name = "Cape Town Marathon",
            Description = "Annual marathon",
            EventDate = DateTime.UtcNow.AddMonths(2),
            Location = "Cape Town",
            Distance = 42.2,
            EventType = "Run",
            BannerImageUrl = (string?)null
        });

        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotCreateEvent()
    {
        var client = _factory.CreateClient();
        await TestHelpers.CreateLoggedInClientAsync(client, "part1@example.com", "Passw0rd!", "Participant");

        var resp = await client.PostAsJsonAsync("/api/events", new
        {
            Name = "Soweto Run",
            Description = "Nope",
            EventDate = DateTime.UtcNow.AddMonths(1),
            Location = "Soweto",
            Distance = 10.0,
            EventType = "Run",
            BannerImageUrl = (string?)null
        });

        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }

    [Fact]
    public async Task Anyone_CanListEvents()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/api/events");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }
}
'@ | Set-Content -Path "RaceDay.Tests\EventsTests.cs" -Encoding UTF8

# ============================================================
# EnrolmentsTests
# ============================================================

@'
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.Tests;

public class EnrolmentsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    public EnrolmentsTests(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Participant_CanEnrolInEvent()
    {
        var orgClient = _factory.CreateClient();
        await TestHelpers.CreateLoggedInClientAsync(orgClient, "org-enrol@example.com", "Passw0rd!", "Organiser");

        var createResp = await orgClient.PostAsJsonAsync("/api/events", new
        {
            Name = "Two Oceans", Description = "Beautiful",
            EventDate = DateTime.UtcNow.AddMonths(3), Location = "Cape Town",
            Distance = 21.1, EventType = "Run", BannerImageUrl = (string?)null
        });
        var createJson = await createResp.Content.ReadFromJsonAsync<JsonElement>();
        var eventId = createJson.GetProperty("eventId").GetInt32();

        var catResp = await orgClient.PostAsJsonAsync($"/api/events/{eventId}/categories", new
        {
            Name = "Senior", MinAge = 20, MaxAge = 39
        });
        var catJson = await catResp.Content.ReadFromJsonAsync<JsonElement>();
        var categoryId = catJson.GetProperty("categoryId").GetInt32();

        var partClient = _factory.CreateClient();
        await TestHelpers.CreateLoggedInClientAsync(partClient, "part-enrol@example.com", "Passw0rd!", "Participant");

        var enrolResp = await partClient.PostAsJsonAsync("/api/enrolments", new
        {
            EventId = eventId, CategoryId = categoryId
        });
        Assert.Equal(HttpStatusCode.Created, enrolResp.StatusCode);
    }
}
'@ | Set-Content -Path "RaceDay.Tests\EnrolmentsTests.cs" -Encoding UTF8

# ============================================================
# RoleAccessTests
# ============================================================

@'
using System.Net;

namespace RaceDay.Tests;

public class RoleAccessTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    public RoleAccessTests(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Unauthenticated_ProfileRequest_Returns401()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/api/profile");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Unauthenticated_EnrolmentsRequest_Returns401()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/api/enrolments");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }
}
'@ | Set-Content -Path "RaceDay.Tests\RoleAccessTests.cs" -Encoding UTF8

Write-Host ""
Write-Host "==========================================" -ForegroundColor Green
Write-Host " Test files written successfully!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Next: dotnet test" -ForegroundColor Yellow