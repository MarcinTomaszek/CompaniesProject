using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Xml.Serialization;
using Infrastructure.EF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Dto;
using WebApi.Dto.CompanyDTOs;

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
                .Select(c => new CompanyDto()
                {
                    Rank = c.Rank,
                    Name = c.Name,
                    Url = c.Url,
                    State = c.State,
                    City = c.City
                })
                .ToList();

            var totalCompanies = _dbContext.Companies.Count();

            var totalPages = (int)Math.Ceiling((double)totalCompanies / pageSize);

            var links = new List<LinkDto>
            {
                new LinkDto(){ Rel = "self", Href = Url.Action("GetCompanies", new { page, pageSize }) }
            };

            if (page > 1)
            {
                links.Add(new LinkDto(){ Rel = "prev", Href = Url.Action("GetCompanies", new { page = page - 1, pageSize }) });
            }

            if (page < totalPages)
            {
                links.Add(new LinkDto(){ Rel = "next", Href = Url.Action("GetCompanies", new { page = page + 1, pageSize }) });
            }

            links.Add(new LinkDto(){ Rel = "first", Href = Url.Action("GetCompanies", new { page = 1, pageSize }) });
            links.Add(new LinkDto(){ Rel = "last", Href = Url.Action("GetCompanies", new { page = totalPages, pageSize }) });
            
            return Ok(new CompaniesResponse()
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

        [HttpPost]
        [Authorize(Policy = "Bearer")]
        public async Task<IActionResult> CreateCompany([FromBody] CompanyCreateDto dto)
        {
            int nextRank = (_dbContext.Companies.Any())
                ? _dbContext.Companies.Max(c => c.Rank) + 1
                : 1;

            var company = new CompanyEntity
            {
                Rank = nextRank,
                Name = dto.Name,
                Url = dto.Url,
                State = dto.State,
                City = dto.City,
                GrowthPercent = dto.GrowthPercent,
                Workers = dto.Workers,
                Founded = dto.Founded,
                YrsOnList = dto.YrsOnList,
                PreviousWorkers = dto.PreviousWorkers,
                Metro = dto.Metro,
                Revenue = dto.Revenue,
                Industry = dto.Industry,
                Profile = dto.Profile
            };

            _dbContext.Companies.Add(company);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCompanyById), new { rank = nextRank }, company);
        }
        
        
        [HttpGet("{rank}")]
        public async Task<IActionResult> GetCompanyById(int rank)
        {
            var company = await _dbContext.Companies.FindAsync(rank);
            if (company == null) return NotFound();

            return Ok(company);
        }
        
        [HttpDelete("{rank}")]
        [Authorize(Policy = "Bearer")]
        public async Task<IActionResult> DeleteCompany(int rank)
        {
            var company = await _dbContext.Companies.FindAsync(rank);
            if (company == null) return NotFound();

            _dbContext.Companies.Remove(company);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
        
        [HttpPut("{rank}")]
        [Authorize(Policy = "Bearer")]
        public async Task<IActionResult> UpdateCompany(int rank, [FromBody] CompanyCreateDto dto)
        {
            var company = await _dbContext.Companies.FirstOrDefaultAsync(c => c.Rank == rank);
            if (company == null)
            {
                return NotFound(new { message = $"Company with rank {rank} not found." });
            }

            company.Name = dto.Name;
            company.Url = dto.Url;
            company.State = dto.State;
            company.City = dto.City;
            company.GrowthPercent = dto.GrowthPercent;
            company.Workers = dto.Workers;
            company.Founded = dto.Founded;
            company.YrsOnList = dto.YrsOnList;
            company.PreviousWorkers = dto.PreviousWorkers;
            company.Metro = dto.Metro;
            company.Revenue = dto.Revenue;
            company.Industry = dto.Industry;
            company.Profile = dto.Profile;

            await _dbContext.SaveChangesAsync();

            return Ok(company);
        }
    }
}