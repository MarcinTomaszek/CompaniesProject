using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize("Bearer")]
    public class BooksController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetBook()
        {
            return Ok(new
            {
                Title = "C#",
                Autor = "Bloch"
            });
        }
    }
}
