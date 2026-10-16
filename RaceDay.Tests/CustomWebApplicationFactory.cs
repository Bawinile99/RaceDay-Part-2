using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.API.Data;

namespace RaceDay.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection;

    public CustomWebApplicationFactory()
    {
        // Shared in-memory SQLite connection — lives for the lifetime of this factory
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Tell Program.cs to skip migrations
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove the app's real DbContext registration
            var descriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<RaceDayDbContext>)
                         || d.ServiceType == typeof(RaceDayDbContext))
                .ToList();
            foreach (var d in descriptors) services.Remove(d);

            // Re-register against the shared in-memory connection
            services.AddDbContext<RaceDayDbContext>(options =>
                options.UseSqlite(_connection));

            // Build the schema once, on this connection
            using var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();
            db.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}