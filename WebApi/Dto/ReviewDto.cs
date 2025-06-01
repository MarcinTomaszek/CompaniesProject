using Infrastructure.EF;

namespace WebApi.Dto;

public class ReviewDto
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public int CompanyRank { get; set; }
    public string Content { get; set; }
    public UserEntity User { get; set; }
    public CompanyEntity Company { get; set; }
}