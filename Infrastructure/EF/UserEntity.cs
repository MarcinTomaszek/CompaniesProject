using ApplicationCore.Models;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.EF;

public class UserEntity:IdentityUser
{
    public UserDetails Details { get; set; }
}