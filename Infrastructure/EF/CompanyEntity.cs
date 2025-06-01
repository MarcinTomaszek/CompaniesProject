using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.EF;

public class CompanyEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)] 
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