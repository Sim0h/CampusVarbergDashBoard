using CampusVarbergDashBoard.FilterData;
using CampusVarbergDashBoard.Models;

namespace CampusVarbergDashBoard.ViewModels
{
    public class FilterViewModel
    {
        public IEnumerable<YearDistribution> YearDistributions { get; set; }
        public GenderDistribution GenderDistribution { get; set; }
        public CompetenceDistribution CompetenceDistribution { get; set; }
        public IEnumerable<AgeDistribution> AgeDistribution { get; set; }
        public List<Applicant> Applicants { get; set; }
        public OfferedSpotDistribution OfferedSpotDistribution { get; set; }
        public string SelectedUtbildning { get; set; }
        public string SelectedKön { get; set; }
        public string SelectedÅr { get; set; }
        public string SelectedTermin { get; set; }
		public LateApplicationDistribution LateApplicationDistribution { get; set; }
        public AcceptedOfferDistribution AcceptedOfferDistribution { get; set; }
    }
}
