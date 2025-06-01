using Microsoft.AspNetCore.Mvc;

namespace WebApi.Dto;

public class LinkDto
{
    /// <summary>Link relation type (e.g. self, next, prev).</summary>
    public string Rel { get; set; } = null!;

    /// <summary>URL of the link.</summary>
    public string Href { get; set; } = null!;
}