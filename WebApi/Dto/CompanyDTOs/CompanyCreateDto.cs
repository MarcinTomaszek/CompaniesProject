using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Dto.CompanyDTOs;

public class CompanyCreateDto
{
    [Required]
    [MinLength(3, ErrorMessage = "Name must be at least 3 characters long.")]
    [Description("Name of the company.")]
    public string Name { get; set; }

    [Required]
    [Url(ErrorMessage = "Url must be a valid URL.")]
    [Description("Official website URL of the company.")]
    public string Url { get; set; }

    [StringLength(2, MinimumLength = 2, ErrorMessage = "State must be a 2-letter code.")]
    [Description("US state where the company is located.")]
    public string State { get; set; }

    [StringLength(100)]
    [Description("City where the company is located.")]
    public string City { get; set; }

    [Description("Percentage growth of the company.")]
    public string GrowthPercent { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Workers must be a positive number.")]
    [Description("Number of employees currently working at the company.")]
    public string Workers { get; set; }

    [Description("Year the company was founded.")]
    public int Founded { get; set; }

    [Description("Number of years the company has been on the list.")]
    public int YrsOnList { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Previous workers must be a positive number.")]
    [Description("Number of employees in the previous year.")]
    public string PreviousWorkers { get; set; }

    [Description("Metropolitan area in which the company operates.")]
    public string Metro { get; set; }

    [Description("Annual revenue of the company.")]
    public string Revenue { get; set; }

    [Description("Industry sector the company belongs to.")]
    public string Industry { get; set; }

    [Required]
    [MinLength(3, ErrorMessage = "Profile must be at least 3 characters long.")]
    [Description("Short profile or description of the company.")]
    public string Profile { get; set; }
}