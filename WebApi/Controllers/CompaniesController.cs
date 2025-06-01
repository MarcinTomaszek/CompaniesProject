using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Infrastructure.EF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "Bearer")]
    public class CompaniesController : ControllerBase
    {
        
        private readonly AppDbContext _dbContext;
        private readonly UserManager<UserEntity> _userManager;
        public CompaniesController(AppDbContext dbContext, UserManager<UserEntity> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }
        
        
        
        [HttpGet]
        [AllowAnonymous]
        [EndpointDescription("Get list of companies paginated 20 companies per page")]
        public IActionResult GetCompanies(
            [FromQuery, Description("Page number of the companies list (default is 1).")] int page = 1,
            [FromQuery, Description("Number of companies per page (default is 20).")] int pageSize = 20)

        {
            
            var companies = _dbContext.Companies
                .OrderBy(c => c.Rank) 
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new
                {
                    c.Rank,
                    c.Name,
                    c.Url,
                    c.State,
                    c.City
                })
                .ToList();
            
            var totalCompanies = _dbContext.Companies.Count();

            return Ok(new
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCompanies,
                Companies = companies
            });
        }
         
        [HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("User")]
        public UserEntity? GetCurrentUser()
        {
            var user = HttpContext.User.Identity as ClaimsIdentity;

            if (user != null)
            {
                string username = user.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Name)?.Value;

                return _userManager.FindByNameAsync(username).Result;
            }

            return null;
        }
    }
}