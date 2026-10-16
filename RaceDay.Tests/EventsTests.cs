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
