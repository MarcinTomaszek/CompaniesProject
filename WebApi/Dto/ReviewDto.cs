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

/// <summary>
/// Response containing paginated review list for companies and HATEOAS links.
/// </summary>

public class ReviewsResposne
{
    /// <summary>Current page number.</summary>
    public int Page { get; set; }

    /// <summary>Number of reviews per page.</summary>
    public int PageSize { get; set; }

    /// <summary>Total number of reviews for this company.</summary>
    public int TotalCount { get; set; }

    /// <summary>List of reviews.</summary>
    public List<ReviewDto> Reviews { get; set; } = [];
    
    /// <summary>Hypermedia links for navigation (HATEOAS).</summary>
    public List<LinkDto> Links { get; set; } = [];
}