using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Infrastructure.EF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "Bearer")]
    public class BooksController(UserManager<UserEntity> userManager) : ControllerBase
    {
        [HttpGet]
        public IActionResult GetBook()
        {
            Console.WriteLine(GetCurrentUser().UserName);
            
            return Ok(new
            {
                Title = "C#",
                Autor = "Bloch"
            });
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
}