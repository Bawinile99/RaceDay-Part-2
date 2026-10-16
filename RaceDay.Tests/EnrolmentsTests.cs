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
