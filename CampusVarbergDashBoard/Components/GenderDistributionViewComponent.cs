using CampusVarbergDashBoard.Repository;
using Microsoft.AspNetCore.Mvc;

namespace CampusVarbergDashBoard.Components
{
    public class GenderDistributionViewComponent : ViewComponent
    {
        private readonly IApplicantRepository _applicantRepo;

        public GenderDistributionViewComponent(IApplicantRepository applicantRepo)
        {
            _applicantRepo = applicantRepo;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var genderDistribution = await _applicantRepo.GetGenderDistributionAsync();
            return View(genderDistribution);
        }

        
       
    }
}
