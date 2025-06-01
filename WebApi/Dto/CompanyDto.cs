namespace WebApi.Dto;

public class CompanyDto
{
    public int Rank { get; set; }
    public string Profile { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    public string State { get; set; }
    public string Revenue { get; set; }
    public string GrowthPercent { get; set; }
    public string Industry { get; set; }
    public string Workers { get; set; }
    public string PreviousWorkers { get; set; }
    public int? Founded { get; set; }
    public int? YrsOnList { get; set; }
    public string Metro { get; set; }
    public string City { get; set; }
}

/// <summary>
/// Response containing paginated companies list and HATEOAS links.
/// </summary>
public class CompaniesResponse
{
    /// <summary>Current page number.</summary>
    public int Page { get; set; }

    /// <summary>Number of companies per page.</summary>
    public int PageSize { get; set; }

    /// <summary>Total number of companies.</summary>
    public int TotalCount { get; set; }

    /// <summary>List of companies.</summary>
    public List<CompanyDto> Companies { get; set; } = [];

    /// <summary>Hypermedia links for navigation (HATEOAS).</summary>
    public List<LinkDto> Links { get; set; } = [];
}

public class LinkDto
{
    /// <summary>Link relation type (e.g. self, next, prev).</summary>
    public string Rel { get; set; } = null!;

    /// <summary>URL of the link.</summary>
    public string Href { get; set; } = null!;
}