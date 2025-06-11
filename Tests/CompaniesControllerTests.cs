using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Infrastructure.EF;
using WebApi;

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
    
    [Fact]
    public async Task PutCompany_UpdatesExistingCompany()
    {
        // Utwórz firmę
        var companyDto = new
        {
            profile = "Profil testowy",
            name = "PutCo",
            url = "http://putco.example.com",
            state = "CA",
            revenue = "1M",
            growthPercent = "10%",
            industry = "Fintech",
            workers = "50",
            previousWorkers = "40",
            founded = 2020,
            yrsOnList = 1,
            metro = "San Francisco",
            city = "SF"
        };

        var postResponse = await _client.PostAsJsonAsync("/api/companies", companyDto);
        postResponse.EnsureSuccessStatusCode();

        // Pobierz rank utworzonej firmy
        var createdCompany = await postResponse.Content.ReadFromJsonAsync<CompanyEntity>();
        var rank = createdCompany?.Rank ?? throw new Exception("Nie udało się pobrać rank.");

        // Zmodyfikuj dane firmy
        var updatedCompanyDto = new
        {
            profile = "Zaktualizowany profil",
            name = "UpdatedPutCo",
            url = "http://putco-updated.example.com",
            state = "TX",
            revenue = "2M",
            growthPercent = "15%",
            industry = "AI",
            workers = "60",
            previousWorkers = "50",
            founded = 2019,
            yrsOnList = 2,
            metro = "Austin",
            city = "Austin"
        };

        var putResponse = await _client.PutAsJsonAsync($"/api/companies/{rank}", updatedCompanyDto);
        putResponse.EnsureSuccessStatusCode();

        var content = await putResponse.Content.ReadAsStringAsync();
        Assert.Contains("UpdatedPutCo", content);
        Assert.Contains("Austin", content);
    }
    
    [Fact]
    public async Task DeleteCompany_RemovesExistingCompany()
    {
        // Dodaj firmę do usunięcia
        var companyDto = new
        {
            profile = "Do usunięcia",
            name = "DeleteCo",
            url = "http://deleteco.example.com",
            state = "NY",
            revenue = "3M",
            growthPercent = "30%",
            industry = "HR",
            workers = "80",
            previousWorkers = "70",
            founded = 2018,
            yrsOnList = 3,
            metro = "NYC",
            city = "New York"
        };

        var postResponse = await _client.PostAsJsonAsync("/api/companies", companyDto);
        postResponse.EnsureSuccessStatusCode();

        var createdCompany = await postResponse.Content.ReadFromJsonAsync<CompanyEntity>();
        var rank = createdCompany?.Rank ?? throw new Exception("Nie udało się pobrać rank.");

        // Usuń firmę
        var deleteResponse = await _client.DeleteAsync($"/api/companies/{rank}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Sprawdź, czy została faktycznie usunięta
        var getResponse = await _client.GetAsync($"/api/companies/{rank}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
