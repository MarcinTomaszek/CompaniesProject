using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.EF;

public class EfDesignTimeFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        var connectionString =
            "Server=.\\SQLEXPRESS;Database=companies;Trusted_Connection=True;TrustServerCertificate=True;";
        optionsBuilder.UseSqlServer(connectionString,
            b => b.MigrationsAssembly("Infrastructure"));

        return new AppDbContext(optionsBuilder.Options);
    }
}