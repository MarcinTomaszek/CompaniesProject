using System.Globalization;
using ApplicationCore.Models;
using CsvHelper;
using Infrastructure.EF;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using WebApi.Configuration;
using WebApi.Mappers;

namespace WebApi;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 🔧 Rejestracja wszystkich usług
        builder.Services.AddAuthorization();
        builder.Services.AddControllers();

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.MigrationsAssembly("Infrastructure")));

        builder.Services.AddIdentity<UserEntity, IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        builder.Services.AddSingleton<JwtSettings>();
        builder.Services.ConfigureJWT(new JwtSettings(builder.Configuration));
        builder.Services.ConfigureCors();
        builder.Services.AddOpenApi();

        var app = builder.Build();

        // 💡 Tworzenie bazy danych (z migracjami!)
        CreateDatabaseIfNotExists(app);

        if (app.Environment.IsDevelopment())
        {
            app.MapScalarApiReference();
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }

    private static void CreateDatabaseIfNotExists(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        context.Database.Migrate();
        Console.WriteLine("Baza danych została utworzona lub zaktualizowana.");
        
        if (context.Companies.Any())
        {
            Console.WriteLine("Baza danych już istnieje i zawiera firmy. Nic nie robię.");
            return;
        }
        
        var companiesCsvPath = "./Data/Inc 5000 Companies 2019.csv";
        using var reader = new StreamReader(companiesCsvPath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        csv.Context.RegisterClassMap<CompanyCsvMap>();

        var records = csv.GetRecords<CompanyEntity>()
            .Where(r => r != null && r.Rank != 0)
            .ToList();

        var distinctCompanies = records
            .GroupBy(c => c.Rank)
            .Select(g => g.First())
            .ToList();

        context.Companies.AddRange(distinctCompanies);
        context.SaveChanges();

        Console.WriteLine("Firmy zostały dodane.");
    }
}
