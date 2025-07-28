using LightNotes;
using LightNotes.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace LightNotes.Tests.Integration;

/// <summary>
/// Налаштовує тестову версію застосунку з базою даних у памʼяті.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Видаляємо всі сервіси, пов'язані з DbContext (щоб уникнути конфліктів із основною бд MySQL)
            var dbContextDescriptor = services
                .SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            // Видаляємо всі сервіси, пов'язані з MySQL (Pomelo)
            var mysqlServices = services
                .Where(s => s.ServiceType.FullName?.Contains("Pomelo") == true
                         || s.ImplementationType?.FullName?.Contains("Pomelo") == true
                         || s.ServiceType.FullName?.Contains("EntityFrameworkCore") == true)
                .ToList();

            foreach (var serviceDescriptor in mysqlServices)
            {
                services.Remove(serviceDescriptor);
            }

            // Реєструємо новий контекст з InMemory бд
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("LightNotesTestDb"));

            // Ініціалізуємо базу даних (щоб кожен тест починався з чистого стану)
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
        });
    }
}
