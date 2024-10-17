using CampusVarbergDashBoard.FilterData;

namespace CampusVarbergDashBoard.ViewModels
{
    public class DashboardViewModel
    {
        public TotalApplicants TotalApplicants { get; set; }
        public IEnumerable<EducationDistribution> EducationDistribution { get; set; }
        public CompetenceDistribution CompetenceDistribution { get; set; }
        public IEnumerable<AgeDistribution> AgeDistribution { get; set; }
        public GenderDistribution GenderDistribution { get; set; }
    }
}
