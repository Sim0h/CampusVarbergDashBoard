using CampusVarbergDashBoard.Repository;
using Microsoft.AspNetCore.Mvc;

namespace CampusVarbergDashBoard.Components
{
    public class AgeDistributionViewComponent : ViewComponent
    {
        private readonly IApplicantRepository _applicantRepo;
        public AgeDistributionViewComponent(IApplicantRepository applicantRepo)
        {
            _applicantRepo = applicantRepo;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var ageDistribution = await _applicantRepo.GetAgeDistributionAsync();
            return View(ageDistribution);
        }
    }
}
