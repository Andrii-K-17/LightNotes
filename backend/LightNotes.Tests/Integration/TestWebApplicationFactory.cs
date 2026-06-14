using LightNotes.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MySql;

namespace LightNotes.Tests.Integration;

/// <summary>
/// Configures a test version of the application with a real containerized MySQL database.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MySqlContainer _mySqlContainer = new MySqlBuilder()
        .WithDatabase("lightnotes_test")
        .WithUsername("testuser")
        .WithPassword("testpassword")
        .Build();

    public async ValueTask InitializeAsync()
    {
        await _mySqlContainer.StartAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        await _mySqlContainer.StopAsync();
        await base.DisposeAsync();

        GC.SuppressFinalize(this);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services
                .SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            var mysqlServices = services
                .Where(s => s.ServiceType.FullName?.Contains("Pomelo") == true
                         || s.ImplementationType?.FullName?.Contains("Pomelo") == true
                         || s.ServiceType.FullName?.Contains("EntityFrameworkCore") == true)
                .ToList();

            foreach (var serviceDescriptor in mysqlServices)
            {
                services.Remove(serviceDescriptor);
            }

            var connectionString = _mySqlContainer.GetConnectionString();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.EnsureCreated();
        });
    }
}
