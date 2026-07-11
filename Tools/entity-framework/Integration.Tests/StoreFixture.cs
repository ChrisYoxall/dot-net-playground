using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Web.Store.Backend;
using Web.Store.Backend.Data;

namespace Integration.Tests;

// ReSharper disable once ClassNeverInstantiated.Global
public class StoreFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder("postgres:18-alpine").Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:DB", _db.GetConnectionString());

        // Fake only what you don't own (payment gateways, email providers, etc.)
    }

    public async ValueTask InitializeAsync()
    {
        await _db.StartAsync();

        // Create the schema once the container is up. If your app already
        // migrates on startup, drop this and let the host do it.
        using var scope = Services.CreateScope();
        await scope.ServiceProvider
            .GetRequiredService<AppDbContext>()
            .Database.MigrateAsync();
    }

    public new Task DisposeAsync() => _db.DisposeAsync().AsTask();
}
