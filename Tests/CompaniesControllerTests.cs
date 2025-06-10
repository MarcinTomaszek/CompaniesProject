using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Infrastructure.EF;
using Microsoft.Extensions.DependencyInjection;
using WebApi;
using Xunit;

namespace Test;

public class CompaniesControllerTests : IClassFixture<AppTestFactory<Program>>
{
    private readonly AppTestFactory<Program> _factory;
    private readonly HttpClient _client;

    public CompaniesControllerTests(AppTestFactory<Program> factory)
    {
        _factory = factory;
        _client = CreateAuthorizedClient(_factory);
    }

    private async Task<HttpClient> CreateAuthorizedClientAsync(AppTestFactory<Program> factory)
    {
        var client = factory.CreateClient();

        // Unikalna nazwa użytkownika (np. na podstawie timestampu)
        var uniqueLogin = $"testuser_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
        var password = "Test123!";
        var email = $"{uniqueLogin}@example.com";

        // Rejestracja nowego użytkownika
        var registerDto = new
        {
            Login = uniqueLogin,
            Email = email,
            Password = password,
            RepPassword = password
        };

        var registerResponse = await client.PostAsJsonAsync("/api/users/register", registerDto);
        registerResponse.EnsureSuccessStatusCode(); // upewnia się, że rejestracja się powiodła

        // Logowanie na tego samego użytkownika
        var loginDto = new
        {
            Login = uniqueLogin,
            Password = password
        };

        var loginResponse = await client.PostAsJsonAsync("/api/users/login", loginDto);
        loginResponse.EnsureSuccessStatusCode();

        var content = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, string>>();

        if (content == null || !content.TryGetValue("token", out var token))
            throw new InvalidOperationException("Login did not return a valid token.");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        Console.WriteLine(client.DefaultRequestHeaders.Authorization);
        
        return client;
    }

    private HttpClient CreateAuthorizedClient(AppTestFactory<Program> factory)
    {
        return CreateAuthorizedClientAsync(factory).GetAwaiter().GetResult();
    }
    
    [Fact]
    public async Task GetCompanies_ReturnsTestCompany()
    {
        var response = await _client.GetAsync("/api/companies");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Test Company", content);
    }

    [Fact]
    public async Task PostCompany_CreatesNewCompany()
    {
        var companyJson = """
                          {
                              "rank": 999,
                              "profile": "Dynamiczna firma technologiczna",
                              "name": "NewCo",
                              "url": "http://newco.example.com",
                              "state": "MA",
                              "revenue": "2M",
                              "growthPercent": "25%",
                              "industry": "IT",
                              "workers": "200",
                              "previousWorkers": "150",
                              "founded": 2015,
                              "yrsOnList": 2,
                              "metro": "Boston",
                              "city": "Cambridge"
                          }
                          """;
        
        var response = await _client.PostAsync("/api/companies", new StringContent(companyJson, System.Text.Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("NewCo", content);
    }

    [Fact]
    public async Task GetCompanyById_ReturnsCompany()
    {
        var factory = new AppTestFactory<Program> { SeedData = true };
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/companies/1");

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Test Company", content);
    }

    [Fact]
    public async Task PostCompany_MissingRequiredFields_ReturnsBadRequest()
    {
        var invalidJson = """{ "rank": 123 }""";
        var response = await _client.PostAsync("/api/companies", new StringContent(invalidJson, System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }


    [Fact]
    public async Task GetCompanies_EmptyDb_ReturnsEmptyArray()
    {
        var factory = new AppTestFactory<Program> { SeedData = false };
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/companies");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Test Company", content);
    }

}
