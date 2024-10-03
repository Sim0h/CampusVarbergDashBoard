using CampusVarbergDashBoard.Repository;
using Microsoft.AspNetCore.Mvc;

namespace CampusVarbergDashBoard.Components
{
    public class EducationDistributionViewComponent : ViewComponent
    {
        private readonly IApplicantRepository _applicantRepo;
        public EducationDistributionViewComponent(IApplicantRepository applicantRepo)
        {
            _applicantRepo = applicantRepo;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var educationDistribution = await _applicantRepo.GetAllEducationsAsync();
            return View(educationDistribution);
        }

    }
}
