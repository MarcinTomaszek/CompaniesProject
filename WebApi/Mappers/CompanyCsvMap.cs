using CsvHelper.Configuration;
using Infrastructure.EF;

namespace WebApi.Mappers;

public sealed class CompanyCsvMap : ClassMap<CompanyEntity>
{
    public CompanyCsvMap()
    {
        Map(m => m.Rank).Name("rank");
        Map(m => m.Profile).Name("profile");
        Map(m => m.Name).Name("name");
        Map(m => m.Url).Name("url");
        Map(m => m.State).Name("state");
        Map(m => m.Revenue).Name("revenue");
        Map(m => m.GrowthPercent).Name("growth_%");
        Map(m => m.Industry).Name("industry");
        Map(m => m.Workers).Name("workers");
        Map(m => m.PreviousWorkers).Name("previous_workers");
        Map(m => m.Founded).Name("founded");
        Map(m => m.YrsOnList).Name("yrs_on_list");
        Map(m => m.Metro).Name("metro");
        Map(m => m.City).Name("city");
    }
}