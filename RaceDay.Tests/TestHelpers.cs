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
