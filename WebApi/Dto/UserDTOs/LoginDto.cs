using System.ComponentModel;

namespace WebApi.Dto.UserDTOs;

public class LoginDto
{
   
    [Description("User Login")]
    public string Login { get; set; }
    
    [Description("User Password")]
    public string Password { get; set; }
}