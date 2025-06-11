using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Infrastructure.EF;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using WebApi;
using WebApi.Dto;
using WebApi.Dto.UserDTOs;

namespace Test;

public class AppUsersControllerTests: IClassFixture<AppTestFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly AppTestFactory<Program> _app;
    private readonly AppDbContext _context;

    public AppUsersControllerTests(AppTestFactory<Program> app)
    {
        _app = app;
        _client = app.CreateClient();
        var hasher = new PasswordHasher<UserEntity>();
        
        using (var scope = app.Services.CreateScope())
        {
            _context = scope.ServiceProvider.GetService<AppDbContext>();
            _context.Users.Add(new UserEntity()
            {
                Id = "eac0adc0-c5fc-4765-9f9d-b1ff1f8794c8",
                Email = "admin@wsei.edu.pl",
                NormalizedEmail = "ADMIN@WSEI.EDU.PL",
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                ConcurrencyStamp = "eac0adc0-c5fc-4765-9f9d-b1ff1f8794c8",
                SecurityStamp = "eac0adc0-c5fc-4765-9f9d-b1ff1f8794c8",
                EmailConfirmed = true,
                PasswordHash = "AQAAAAIAAYagAAAAEOrArrSG1swr5b94IyFxxXI9wv/pMOWdiSK3LvAtL3VoMmk6sTFHTvhuRqAesmP/Ag=="
            });
            _context.SaveChanges();
        }
    }

    [Fact]
    public async void TestValidLogin()
    {
        var loginBody = new LoginDto
        {
            Login = "admin",
            Password = "1234!"
        };
        var result = await _client.PostAsJsonAsync("/api/users/login", loginBody);
        
        Assert.NotNull(result);
        Assert.Equal(result.StatusCode, HttpStatusCode.OK);
        JsonNode node = JsonNode.Parse(await result.Content.ReadAsStringAsync());
        var token = node["token"].AsValue().ToString();
        Assert.NotNull(token);
    }
}