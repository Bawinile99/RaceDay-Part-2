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
