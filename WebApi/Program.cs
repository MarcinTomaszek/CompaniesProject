using Infrastructure.EF;
using Microsoft.AspNetCore.Identity;
using Scalar.AspNetCore;
using WebApi.Configuration;

namespace WebApi;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Services.AddAuthorization();
        builder.Services.AddControllers();
        builder.Services.AddDbContext<AppDbContext>();
        builder.Services.AddIdentity<UserEntity, IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();
        builder.Services.AddSingleton<JwtSettings>();
        builder.Services.ConfigureJWT(new JwtSettings(builder.Configuration));
        builder.Services.ConfigureCors();
        builder.Services.AddOpenApi();

        var app = builder.Build();
        
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
}