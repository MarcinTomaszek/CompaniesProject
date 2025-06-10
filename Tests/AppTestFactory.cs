using Infrastructure.EF;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Test;

public class AppTestFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    public bool SeedData { get; set; } = true; // domyślnie seeding włączony

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Usuń stare konfiguracje DbContext
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Użyj losowej bazy InMemory (dla izolacji testów)
            var dbName = Guid.NewGuid().ToString();
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(dbName));
            var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
            
            if (SeedData)
            {
                db.Companies.Add(new CompanyEntity
                {
                    Rank = 1,
                    Name = "Test Company",
                    Profile = "Test",
                    Url = "http://example.com",
                    State = "NY",
                    Revenue = "10M",
                    GrowthPercent = "100%",
                    Industry = "Tech",
                    Workers = "100",
                    PreviousWorkers = "50",
                    Founded = 2000,
                    YrsOnList = 1,
                    Metro = "NY Metro",
                    City = "New York"
                });
                db.SaveChanges();
            }
        });

        return base.CreateHost(builder);
    }
}
