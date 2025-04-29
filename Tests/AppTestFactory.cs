using System.Data.Common;
using Infrastructure.EF;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Test;

public class AppTestFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                     typeof(DbContextOptions<AppDbContext>));
            services.Remove(dbContextDescriptor);
            var dbConnectionDescriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                     typeof(DbConnection));

            services.Remove(dbConnectionDescriptor);

            // Create open SqliteConnection so EF won't automatically close it.
            // services.AddSingleton<DbConnection>(container =>
            // {
            //     var connection = new SqliteConnection("Filename=:memory:");
            //     connection.Open();
            //     return connection;
            // });

            services
                .AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<AppDbContext>((container, options) =>
                {
                    options.UseInMemoryDatabase("AppTest").UseInternalServiceProvider(container);
                });
        });
        builder.UseEnvironment("Development");
    }
}