using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Dto.ReviewDTOs;

public class ReviewCreateDto
{
    [Required]
    [MinLength(3, ErrorMessage = "Content must be at least 3 characters long.")]
    [Description("Content of the Review")]
    public string Content { get; set; }
}