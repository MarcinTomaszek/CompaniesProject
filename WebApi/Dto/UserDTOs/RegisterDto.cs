using System.ComponentModel;

namespace WebApi.Dto.UserDTOs;

public class RegisterDto
{
    [Description("User Login, must be uniqe")]
    public string Login { get; set; }

    [Description("User Password")]
    public string Password { get; set; }

    [Description("Repeated User password, passwords must match")]
    public string RepPassword { get; set; }
    
    [Description("User E-mail")]
    public string Email { get; set; }
}