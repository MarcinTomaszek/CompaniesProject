using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Infrastructure.EF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Dto;
using WebApi.Dto.ReviewDTOs;

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
        [FromRoute] int companyRank,
        [FromQuery, Description("Page number (default is 1).")] int page = 1,
        [FromQuery, Description("Reviews per page (default is 10).")] int pageSize = 10,
        [FromQuery, Description("Text to search in usernames.")] string? search = null,
        [FromQuery, Description("Field to sort by: id, username, content.")] string? sortBy = "id",
        [FromQuery, Description("Sort descending (true/false).")] bool descending = true)
    {
        var query = dbContext.Reviews
            .Include(r => r.User)
            .Include(r => r.Company)
            .Where(r => r.CompanyRank == companyRank);
        
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(r =>
                r.User.UserName.ToLower().Contains(search));
        }
        
        query = sortBy?.ToLower() switch
        {
            "username" => descending ? query.OrderByDescending(r => r.User.UserName) : query.OrderBy(r => r.User.UserName),
            "content" => descending ? query.OrderByDescending(r => r.Content) : query.OrderBy(r => r.Content),
            _ => descending ? query.OrderByDescending(r => r.Id) : query.OrderBy(r => r.Id)
        };

        var totalCount = query.Count();

        var reviews = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReviewDisplayDto
            {
                ReviewId = r.Id.ToString(),
                CompanyName = r.Company.Name,
                UserName = r.User.UserName,
                Content = r.Content
            })
            .ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var links = new List<LinkDto>
        {
            new() { Rel = "self", Href = Url.Action("GetReviews", new { companyRank, page, pageSize, search, sortBy, descending }) }
        };

        if (page > 1)
            links.Add(new() { Rel = "prev", Href = Url.Action("GetReviews", new { companyRank, page = page - 1, pageSize, search, sortBy, descending }) });

        if (page < totalPages)
            links.Add(new() { Rel = "next", Href = Url.Action("GetReviews", new { companyRank, page = page + 1, pageSize, search, sortBy, descending }) });

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
    [Authorize]
    public async Task<IActionResult> CreateReview([FromRoute] int companyRank, [FromBody] ReviewCreateDto dto)
    {
        var user =  GetCurrentUser();
        if (user == null)
            return Unauthorized();

        var company = await dbContext.Companies.FirstOrDefaultAsync(c => c.Rank == companyRank);
        if (company == null)
            return NotFound();

        var review = new ReviewEntity
        {
            CompanyRank = companyRank,
            UserId = user.Id,
            Content = dto.Content
        };

        dbContext.Reviews.Add(review);
        await dbContext.SaveChangesAsync();

        var responseDto = new ReviewDisplayDto
        {
            ReviewId = review.Id.ToString(),
            CompanyName = company.Name,
            UserName = user.UserName,
            Content = review.Content
        };

        return Ok(responseDto);
    }
    
    [HttpPut("{reviewId}")]
    [Authorize]
    public async Task<IActionResult> UpdateReview(
        [FromRoute] int companyRank,
        [FromRoute] int reviewId,
        [FromBody] string updatedContent)
    {
        var user = GetCurrentUser();
        if (user == null)
            return Unauthorized();

        var review = await dbContext.Reviews
            .Include(r => r.User)
            .Include(r => r.Company)
            .FirstOrDefaultAsync(r => r.Id == reviewId && r.CompanyRank == companyRank);

        if (review == null)
            return NotFound();

        if (review.UserId != user.Id)
            return Forbid();

        review.Content = updatedContent;
        await dbContext.SaveChangesAsync();

        var responseDto = new ReviewDisplayDto
        {
            ReviewId = review.Id.ToString(),
            CompanyName = review.Company.Name,
            UserName = review.User.UserName,
            Content = review.Content
        };

        return Ok(responseDto);
    }


    [HttpDelete("{reviewId}")]
    [Authorize]
    public async Task<IActionResult> DeleteReview([FromRoute] int companyRank, [FromRoute] int reviewId)
    {
        var user =  GetCurrentUser();
        if (user == null)
            return Unauthorized();
        
        var review = await dbContext.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId && r.CompanyRank == companyRank);

        if (review == null)
            return NotFound();

        dbContext.Reviews.Remove(review);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }
    
    private UserEntity? GetCurrentUser()
    {
        var user = HttpContext.User.Identity as ClaimsIdentity;

        if (user != null)
        {
            string username = user.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Name)?.Value;

            return userManager.FindByNameAsync(username).Result;
        }

        return null;
    }
}
