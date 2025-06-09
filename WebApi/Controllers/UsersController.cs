using System.Security.Claims;
using System.Text;
using ApplicationCore.Models;
using Infrastructure.EF;
using JWT.Algorithms;
using JWT.Builder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client.Instance.Discovery;
using Microsoft.IdentityModel.JsonWebTokens;
using WebApi.Configuration;
using WebApi.Dto;
using WebApi.Dto.UserDTOs;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(SignInManager<UserEntity> signInManager, UserManager<UserEntity> userManager, JwtSettings _jwtSettings) : ControllerBase
    {
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await userManager.FindByNameAsync(dto.Login);
            if (user is null)
                return BadRequest(new {error ="Invalid user or Password"});

            var result = await signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (result.Succeeded)
            {
                return Ok(new {token=CreateToken(user)});
            }
            
            return BadRequest(new {error ="Invalid user or Password"});
            
        }
        
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            // Sprawdzenie, czy hasła się zgadzają
            if (dto.Password != dto.RepPassword)
                return BadRequest(new { error = "Passwords do not match" });

            // Sprawdzenie, czy użytkownik już istnieje
            var existingUser = await userManager.FindByNameAsync(dto.Login);
            if (existingUser != null)
                return BadRequest(new { error = "User already exists" });

            // Utworzenie nowego użytkownika
            var user = new UserEntity()
            {
                UserName = dto.Login,
                Email = dto.Email,
                Details = new UserDetails
                {
                    CreatedAt = DateTime.UtcNow 
                }
            };

            
            var result = await userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return BadRequest(new { error = string.Join("; ", result.Errors.Select(e => e.Description)) });
            
            return Ok();
        }
        
        private string CreateToken(UserEntity user)
        {
            return new JwtBuilder()
                .WithAlgorithm(new HMACSHA256Algorithm())
                .WithSecret(Encoding.UTF8.GetBytes(_jwtSettings.Secret))
                .AddClaim(JwtRegisteredClaimNames.Name, user.UserName)
                .AddClaim(JwtRegisteredClaimNames.Gender, "male")
                .AddClaim(JwtRegisteredClaimNames.Email, user.Email)
                .AddClaim(JwtRegisteredClaimNames.Exp, DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds())
                .AddClaim(JwtRegisteredClaimNames.Jti, Guid.NewGuid())
                .Audience(_jwtSettings.Audience)
                .Issuer(_jwtSettings.Issuer)
                .Encode();
        }
        
    }
}
