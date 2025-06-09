namespace WebApi.Dto.CompanyDTOs;

public class CompanyCreateDto
{

    public string Name { get; set; }
    public string Url { get; set; }
    public string State { get; set; }
    public string City { get; set; }
    public string GrowthPercent { get; set; }
    public string Workers { get; set; }
    public int Founded { get; set; }
    public int YrsOnList { get; set; }
    public string PreviousWorkers { get; set; }
    public string Metro { get; set; }
    public string Revenue { get; set; }
    public string Industry { get; set; }
    public string Profile { get; set; }
}