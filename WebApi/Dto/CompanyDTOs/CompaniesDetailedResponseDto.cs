using Infrastructure.EF;

namespace WebApi.Dto.CompanyDTOs;

/// <summary>
/// Response containing paginated companies list and HATEOAS links.
/// </summary>
public class CompaniesDetailedResponseDto
{
    /// <summary>Current page number.</summary>
    public int Page { get; set; }

    /// <summary>Number of companies per page.</summary>
    public int PageSize { get; set; }

    /// <summary>Total number of companies.</summary>
    public int TotalCount { get; set; }

    /// <summary>List of companies.</summary>
    public List<CompanyEntity> Companies { get; set; } = [];

    /// <summary>Hypermedia links for navigation (HATEOAS).</summary>
    public List<LinkDto> Links { get; set; } = [];
}