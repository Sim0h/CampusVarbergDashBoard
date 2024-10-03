using CampusVarbergDashBoard.Repository;
using Microsoft.AspNetCore.Mvc;

namespace CampusVarbergDashBoard.Components
{
    public class CompetenceViewComponent : ViewComponent
    {
        private readonly IApplicantRepository _applicantRepo;
        public CompetenceViewComponent(IApplicantRepository applicantRepo)
        {
            _applicantRepo = applicantRepo;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var competenceDistribution = await _applicantRepo.GetCompetenceDistributionAsync();
            return View(competenceDistribution);
        }
    }
}
