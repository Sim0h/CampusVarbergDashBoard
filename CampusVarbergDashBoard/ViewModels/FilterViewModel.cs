using CampusVarbergDashBoard.FilterData;

namespace CampusVarbergDashBoard.ViewModels
{
    public class FilterViewModel
    {
        public IEnumerable<YearDistribution> YearDistributions { get; set; }
        public GenderDistribution GenderDistribution { get; set; }
        public CompetenceDistribution CompetenceDistribution { get; set; }
    }
}
