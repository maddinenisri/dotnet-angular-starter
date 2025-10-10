using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PersonApi.Infrastructure.Data;
using Testcontainers.MsSql;

namespace PersonApi.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory that uses Testcontainers for SQL Server
/// Implements IAsyncLifetime for proper container lifecycle management
/// </summary>
public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _msSqlContainer;

    public IntegrationTestWebAppFactory()
    {
        // Create SQL Server container using Testcontainers
        // Uses SQL Server 2022 image with Rosetta emulation on Mac M-series
        _msSqlContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("YourStrong@Password123")
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add DbContext using Testcontainers SQL Server connection string
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(_msSqlContainer.GetConnectionString());
            });

            // Build the service provider and run migrations
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<AppDbContext>();

            // Apply migrations to create the database schema
            db.Database.Migrate();
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        // Start the SQL Server container before tests run
        await _msSqlContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        // Stop and cleanup the SQL Server container after tests complete
        await _msSqlContainer.DisposeAsync();
    }
}
