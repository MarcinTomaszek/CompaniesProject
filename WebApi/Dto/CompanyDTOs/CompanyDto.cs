namespace WebApi.Dto.CompanyDTOs;

public class CompanyDto
{
    /// <summary>Rank of the company.</summary>
    public int Rank { get; set; }

    /// <summary>Name of the company.</summary>
    public string Name { get; set; } = null!;

    /// <summary>Website URL of the company.</summary>
    public string? Url { get; set; }

    /// <summary>State of the company.</summary>
    public string? State { get; set; }

    /// <summary>City of the company.</summary>
    public string? City { get; set; }
}


