using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Infrastructure.EF;
using Microsoft.Extensions.DependencyInjection;
using WebApi;
using WebApi.Dto.ReviewDTOs;
using Xunit;

namespace Test;

public class ReviewsControllerTests : IClassFixture<AppTestFactory<Program>>
{
    private readonly AppTestFactory<Program> _factory;
    private readonly HttpClient _client;

    public ReviewsControllerTests(AppTestFactory<Program> factory)
    {
        _factory = factory;
        _client = CreateAuthorizedClient(_factory);
    }

    private async Task<HttpClient> CreateAuthorizedClientAsync(AppTestFactory<Program> factory)
    {
        var client = factory.CreateClient();

        var guid = Guid.NewGuid().ToString("N").Substring(0, 8);
        var uniqueLogin = $"reviewuser_{guid}";
        var password = "Test123!";
        var email = $"user_{guid}@example.com";

        var registerDto = new
        {
            Login = uniqueLogin,
            Email = email,
            Password = password,
            RepPassword = password
        };

        var registerResponse = await client.PostAsJsonAsync("/api/users/register", registerDto);
        registerResponse.EnsureSuccessStatusCode();

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

        return client;
    }

    private HttpClient CreateAuthorizedClient(AppTestFactory<Program> factory)
    {
        return CreateAuthorizedClientAsync(factory).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task PostReview_CreatesReviewForCompany()
    {
        var review = new { content = "Świetna firma!" };

        var response = await _client.PostAsJsonAsync("/api/companies/1/reviews", review);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Świetna firma", content);
    }

    [Fact]
    public async Task GetReviews_ReturnsCreatedReview()
    {
        var review = new { content = "Firma godna polecenia." };

        var postResponse = await _client.PostAsJsonAsync("/api/companies/1/reviews", review);
        postResponse.EnsureSuccessStatusCode();

        var getResponse = await _client.GetAsync("/api/companies/1/reviews");
        getResponse.EnsureSuccessStatusCode();

        var content = await getResponse.Content.ReadAsStringAsync();
        Assert.Contains("Firma godna polecenia", content);
    }

    [Fact]
    public async Task PostReview_NonExistingCompany_ReturnsNotFound()
    {
        var review = new { content = "Recenzja testowa" };
        var response = await _client.PostAsJsonAsync("/api/companies/9999/reviews", review);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetReviews_EmptyDb_ReturnsEmptyArray()
    {
        var factory = new AppTestFactory<Program> { SeedData = false };
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/companies/1/reviews");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("content", content, StringComparison.OrdinalIgnoreCase);
    }
    
    [Fact]
    public async Task DeleteReview_RemovesReview()
    {
        var review = new { content = "Do usunięcia" };
        var postResponse = await _client.PostAsJsonAsync("/api/companies/1/reviews", review);
        postResponse.EnsureSuccessStatusCode();

        var created = await postResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var reviewId = created?["reviewId"]?.ToString();

        Assert.False(string.IsNullOrEmpty(reviewId), "Nie udało się uzyskać ID recenzji");

        var deleteResponse = await _client.DeleteAsync($"/api/companies/1/reviews/{reviewId}");
        deleteResponse.EnsureSuccessStatusCode();

        var getResponse = await _client.GetAsync("/api/companies/1/reviews");
        getResponse.EnsureSuccessStatusCode();

        var content = await getResponse.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Do usunięcia", content);
    }
        
    [Fact]
    public async Task PutReview_UpdatesReviewContent()
    {
        // 1. Utwórz recenzję
        var originalContent = "Treść do edycji";
        var updatedContent = "Zmieniona treść recenzji";

        var postResponse = await _client.PostAsJsonAsync("/api/companies/1/reviews", new { content = originalContent });
        postResponse.EnsureSuccessStatusCode();

        var created = await postResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var reviewId = created?["reviewId"]?.ToString();
        Assert.False(string.IsNullOrEmpty(reviewId), "Nie udało się uzyskać ID recenzji");

        // 2. Zaktualizuj recenzję
        var putResponse = await _client.PutAsJsonAsync($"/api/companies/1/reviews/{reviewId}", updatedContent);
        putResponse.EnsureSuccessStatusCode();

        var putContent = await putResponse.Content.ReadAsStringAsync();

        // 3. Sprawdź, czy treść została zaktualizowana
        Assert.Contains(updatedContent, putContent);
        Assert.DoesNotContain(originalContent, putContent);
    }

}
