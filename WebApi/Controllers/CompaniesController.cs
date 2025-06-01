using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Xml.Serialization;
using Infrastructure.EF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApi.Dto;

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
        [EndpointDescription("Get list of companies")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CompaniesResponse), StatusCodes.Status200OK)]
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

            var totalPages = (int)Math.Ceiling((double)totalCompanies / pageSize);

            var links = new List<object>
            {
                new { rel = "self", href = Url.Action("GetCompanies", new { page, pageSize }) }
            };

            if (page > 1)
            {
                links.Add(new { rel = "prev", href = Url.Action("GetCompanies", new { page = page - 1, pageSize }) });
            }

            if (page < totalPages)
            {
                links.Add(new { rel = "next", href = Url.Action("GetCompanies", new { page = page + 1, pageSize }) });
            }

            links.Add(new { rel = "first", href = Url.Action("GetCompanies", new { page = 1, pageSize }) });
            links.Add(new { rel = "last", href = Url.Action("GetCompanies", new { page = totalPages, pageSize }) });

            return Ok(new
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCompanies,
                Companies = companies,
                Links = links
            });
        }

         
        
        [HttpGet("detailed")]
        [Authorize (Policy = "Bearer")]
        [EndpointDescription("Get detailed list of companies for logged-in users.")]
        [ProducesResponseType(typeof(CompaniesResponse), StatusCodes.Status200OK)]
        public IActionResult GetCompaniesDetailed(
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
                    c.City,
                    c.GrowthPercent, 
                    c.Workers,
                    c.Founded,
                    c.YrsOnList,
                    c.PreviousWorkers,
                    c.Metro,
                    c.Revenue,
                    c.Industry,
                    c.Profile
                })
                .ToList();

            var totalCompanies = _dbContext.Companies.Count();

            var totalPages = (int)Math.Ceiling((double)totalCompanies / pageSize);

            var links = new List<object>
            {
                new { rel = "self", href = Url.Action("GetCompaniesDetailed", new { page, pageSize }) }
            };

            if (page > 1)
            {
                links.Add(new { rel = "prev", href = Url.Action("GetCompaniesDetailed", new { page = page - 1, pageSize }) });
            }

            if (page < totalPages)
            {
                links.Add(new { rel = "next", href = Url.Action("GetCompaniesDetailed", new { page = page + 1, pageSize }) });
            }

            links.Add(new { rel = "first", href = Url.Action("GetCompaniesDetailed", new { page = 1, pageSize }) });
            links.Add(new { rel = "last", href = Url.Action("GetCompaniesDetailed", new { page = totalPages, pageSize }) });

            return Ok(new
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCompanies,
                Companies = companies,
                Links = links
            });
        }

        
        [HttpGet]
        [Route("User")]
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