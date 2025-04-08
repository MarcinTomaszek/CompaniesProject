using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EF;

public class AppDbContext : IdentityDbContext<UserEntity>
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        var adminId = "eac0adc0-c5fc-4765-9f9d-b1ff1f8794c8";
        var adminCreatedAt = new DateTime(2025, 4, 8);
        var hash = "AQAAAAIAAYagAAAAEH/Uosuypol2GlcgtIDtJclI3knRka6lxJEGB+D2azWAG09Y5wXUZuCm42ur+JqrYg==";
        var adminUser = new UserEntity()
        {
            Id = adminId,
            Email = "admin@wsei.edu.pl",
            NormalizedEmail = "ADMIN@WSEI.EDU.PL",
            UserName = "admin",
            NormalizedUserName = "ADMIN",
            ConcurrencyStamp = adminId,
            SecurityStamp = adminId,
            PasswordHash = hash
        };
       

        builder.Entity<UserEntity>()
            .HasData(adminUser);

        builder.Entity<UserEntity>()
            .OwnsOne(u => u.Details)
            .HasData(
                new
                {
                    UserEntityId = adminId,
                    CreatedAt = adminCreatedAt
                }
            );
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=d:\\Data\\app.db");
    }
}