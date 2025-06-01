using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EF;

public class AppDbContext : IdentityDbContext<UserEntity>
{
    public AppDbContext(DbContextOptions options) : base(options)
    {
        
    }

    protected AppDbContext()
    {
    }
    
    public DbSet<CompanyEntity> Companies { get; set; }
    public DbSet<ReviewEntity> Reviews { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<UserEntity>()
            .OwnsOne(u => u.Details);

        // Konfiguracja klucza CompanyEntity
        builder.Entity<CompanyEntity>()
            .HasKey(c => c.Rank);

        // Konfiguracja klucza ReviewEntity
        builder.Entity<ReviewEntity>()
            .HasKey(r => r.Id);

        // Relacja Review -> User
        builder.Entity<ReviewEntity>()
            .HasOne(r => r.User)
            .WithMany() // jeśli UserEntity nie ma kolekcji Reviews
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relacja Review -> Company
        builder.Entity<ReviewEntity>()
            .HasOne(r => r.Company)
            .WithMany() // jeśli CompanyEntity nie ma kolekcji Reviews
            .HasForeignKey(r => r.CompanyRank)
            .OnDelete(DeleteBehavior.Cascade);
    }
    
}