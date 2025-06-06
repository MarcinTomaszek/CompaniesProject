using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Infrastructure.EF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Dto;

namespace WebApi.Controllers;

[Route("api/companies/{companyRank}/reviews")]
[ApiController]
[Authorize(Policy = "Bearer")]
public class ReviewsController(AppDbContext dbContext, UserManager<UserEntity> userManager) : ControllerBase
{
    
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ReviewsResposne), StatusCodes.Status200OK)]
    public IActionResult GetReviews(
        int companyRank,
        [FromQuery, Description("Page number (default is 1).")] int page = 1,
        [FromQuery, Description("Reviews per page (default is 10).")] int pageSize = 10)
    {
        var query = dbContext.Reviews
            .Include(r => r.User)
            .Include(r => r.Company)
            .Where(r => r.CompanyRank == companyRank);

        var totalCount = query.Count();

        var reviews = query
            .OrderByDescending(r => r.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReviewDto
            {
                Id = r.Id,
                UserId = r.UserId,
                CompanyRank = r.CompanyRank,
                Content = r.Content,
                User = r.User,
                Company = r.Company
            })
            .ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var links = new List<LinkDto>
        {
            new() { Rel = "self", Href = Url.Action("GetReviews", new { companyRank, page, pageSize }) }
        };

        if (page > 1)
            links.Add(new() { Rel = "prev", Href = Url.Action("GetReviews", new { companyRank, page = page - 1, pageSize }) });
        if (page < totalPages)
            links.Add(new() { Rel = "next", Href = Url.Action("GetReviews", new { companyRank, page = page + 1, pageSize }) });

        return Ok(new ReviewsResposne
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Reviews = reviews,
            Links = links
        });
    }


    [HttpPost]
    public async Task<IActionResult> CreateReview(int rank, [FromBody] ReviewCreateDto dto)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var company = await dbContext.Companies.FirstOrDefaultAsync(c => c.Rank == rank);
        if (company == null)
            return NotFound();

        var review = new ReviewEntity
        {
            CompanyRank = rank,
            UserId = user.Id,
            Content = dto.Content
        };

        dbContext.Reviews.Add(review);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetReviewById), new { rank }, review);
    }


    [HttpGet("{rank}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetReviewById(int rank)
    {
        var review = await dbContext.Reviews
            .Include(r => r.User)
            .Include(r => r.Company)
            .FirstOrDefaultAsync(r => r.CompanyRank == rank);

        if (review is null)
            return NotFound();

        return Ok(new ReviewDto
        {
            Id = review.Id,
            UserId = review.UserId,
            CompanyRank = review.CompanyRank,
            Content = review.Content,
            User = review.User,
            Company = review.Company
        });
    }

    [HttpPut("{rank}")]
    public async Task<IActionResult> UpdateReview(int rank, [FromBody] string updatedContent)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var review = await dbContext.Reviews.FirstOrDefaultAsync(r => r.CompanyRank == rank);

        if (review is null)
            return NotFound();
        if (review.UserId != userId)
            return Forbid();

        review.Content = updatedContent;
        await dbContext.SaveChangesAsync();

        return Ok(review);
    }

    [HttpDelete("{rank}")]
    public async Task<IActionResult> DeleteReview(int rank)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var review = await dbContext.Reviews.FirstOrDefaultAsync(r =>r.CompanyRank == rank);

        if (review is null)
            return NotFound();
        if (review.UserId != userId)
            return Forbid();

        dbContext.Reviews.Remove(review);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }
}
