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
